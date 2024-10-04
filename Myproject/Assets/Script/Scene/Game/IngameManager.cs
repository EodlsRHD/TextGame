using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class IngameManager : MonoBehaviour
{
    [Header("Tutorial"), SerializeField] private Tutorial _tutorial = null;
    [Header("Ingame UI"), SerializeField] private IngameUI _ingameUI = null;
    [Header("TextView"), SerializeField] private TextView _textView = null;
    [Header("Control"), SerializeField] private ControlPad _controlPad = null;
    [Header("MapController"), SerializeField] private MapController _mapController = null;
    [Header("Ingame Popup"), SerializeField] private IngamePopup _ingamePopup = null;

    private DataManager.Save_Data _saveData = null;
    private MapGenerator _mapGenerator = null;

    private bool _isPlayerTurn = false;
    private bool _isAllMonsterDead = true;

    void Start()
    {
        _saveData = GameManager.instance.dataManager.CopySaveData();

        _tutorial.Initialize();
        _ingameUI.Initialize(OpenMap, OpenNextRound, _textView.UpdateText);
        _textView.Initialize();
        _controlPad.Initialize(PlayerMove, PlayerAction);
        _mapController.Initialize(_saveData.mapData.mapSize);
        _ingamePopup.Initialize();

        FirstSet();

        this.gameObject.SetActive(true);
    }

    private void FirstSet()
    {
        if(_saveData.round > 1)
        {
            _ingameUI.OpenNextRoundWindow(eRoundClear.Load);

            return;
        }

        _ingameUI.OpenNextRoundWindow(eRoundClear.First);
        //_tutorial.Open();
    }

    private void OpenMap(System.Action onCallback)
    {
        _mapController.OpenMap(onCallback);
    }

    private void OpenNextRound(eRoundClear type)
    {
        _textView.DeleteTemplate();

        if (type == eRoundClear.Load)
        {
            _ingameUI.UpdatePlayerInfo(_saveData.userData);
            return;
        }

        if (type == eRoundClear.Fail)
        {
            GameManager.instance.tools.SceneChange(eScene.Lobby, () => 
            {
                GameManager.instance.dataManager.FailGame(_saveData);
            });

            return;
        }

        GameManager.instance.dataManager.SaveDataToCloud(_saveData, () => 
        {
            _saveData.userData.Reset();

            _mapGenerator = new MapGenerator(GenerateMap, _saveData);

            _ingameUI.UpdatePlayerInfo(_saveData.userData);
        }); 
    }

    private void GenerateMap(DataManager.Map_Data mapData)
    {
        _ingameUI.StartGame();
        _saveData.mapData = mapData;
        
        RoundSet();
    }
     
    private void RoundSet()
    {
        _textView.UpdateText(_saveData.mapData.nodeDatas[_saveData.userData.data.currentNodeIndex]);
        _ingameUI.SetRoundText(_saveData.round);

        PlayerTurn();

        if (_saveData.mapData.monsterDatas.Count > 0)
        {
            _isAllMonsterDead = false;
        }
    }

    private void RoundClear()
    {
        _mapController.RemoveTemplate();
        _saveData.round++;

        OpenNextRound(eRoundClear.Success);
    }

    #region Player

    private void PlayerMove(eControl type)
    {
        if(_isPlayerTurn == false)
        {
            _ingamePopup.UpdateText(_saveData.userData.data.name + "의 순서가 아닙니다.");

            return;
        }

        if (_saveData.userData.currentAP <= 0)
        {
            _ingamePopup.UpdateText("ap가 부족합니다.");

            return;
        }

        int nearbyBlockIndex = PlayerMoveType(type, _saveData.userData.data.currentNodeIndex);

        if (nearbyBlockIndex == -1)
        {
            return;
        }

        _textView.UpdateText(_saveData.mapData.nodeDatas[nearbyBlockIndex]);

        _saveData.mapData.nodeDatas[_saveData.userData.data.currentNodeIndex].isUser = false;
        _saveData.mapData.nodeDatas[nearbyBlockIndex].isUser = true;

        _saveData.userData.data.currentNodeIndex = nearbyBlockIndex;
        _saveData.userData.currentAP -= 1;

        _ingameUI.UpdatePlayerInfo(eStats.AP, _saveData.userData);

        UpdateData();
    }

    private int PlayerMoveType(eControl type, int currentIndex)
    {
        int x = 0;
        int y = 0;

        switch (type)
        {
            case eControl.Up:
                x = 0;
                y = 1;
                break;

            case eControl.Left:
                x = -1;
                y = 0;
                break;

            case eControl.Right:
                x = 1;
                y = 0;
                break;

            case eControl.Down:
                x = 0;
                y = -1;
                break;
        }

        int result = GetNearbyBlocks(x, y, currentIndex);

        if (result == -1)
        {
            _ingamePopup.UpdateText("이동할 수 없습니다.");

            return result;
        }

        if (_saveData.mapData.nodeDatas[result].isWalkable == false)
        {
            _ingamePopup.UpdateText("이동할 수 없습니다.");

            return -1;
        }

        if (_saveData.mapData.nodeDatas[result].isMonster == true)
        {
            Attack(result);

            return -1;
        }

        if (_saveData.mapData.exitNodeIndex == result)
        {
            if(_isAllMonsterDead == false)
            {
                _textView.UpdateText("--- 남아있는 몬스터가 있습니다.");

                return -1;
            }

            UiManager.instance.OpenPopup(string.Empty, "다음 라운드로 넘어가시겠습니까?", string.Empty, string.Empty, () =>
            {
                RoundClear();
            }, null);

            return -1;
        }

        return result;
    }

    private void PlayerAction(eControl type)
    {
        if(_isPlayerTurn == false)
        {
            _ingamePopup.UpdateText(_saveData.userData.data.name + "의 순서가 아닙니다.");

            return;
        }

        switch(type)
        {
            case eControl.Defence:
                {
                    UiManager.instance.OpenPopup(string.Empty, "방어력을 높히시겠습니까? 남은 행동력을 모두 소진합니다.", "확인", "취소", () =>
                    {
                        Defence();
                    }, null);
                }
                break;

            case eControl.Skill:
                {
                    _controlPad.Skill(_saveData.userData, Skill);
                }
                break;

            case eControl.Bag:
                {
                    _controlPad.Bag(_saveData.userData, Bag);
                }
                break;

            case eControl.Rest:
                {
                    UiManager.instance.OpenPopup(string.Empty, "휴식하시겠습니까?", "확인", "취소", () =>
                    {
                        PlayerTurnOut();
                        StartCoroutine(MonsterTurn());
                    }, null);
                }
                break;

            case eControl.SearchNearby:
                {
                    PlayerSearchNearby();
                }
                break;
        }
    }

    private void PlayerTurn()
    {
        _isPlayerTurn = true;

        _saveData.userData.currentAP = _saveData.userData.maximumAP;
        _ingameUI.UpdatePlayerInfo(eStats.AP, _saveData.userData);

        _textView.UpdateText("행동력이 " + _saveData.userData.currentAP + "만큼 남았습니다.");

        List<int> NearbyIndexs = PlayerSearchNearby();

        _mapController.SetMap(_saveData, NearbyIndexs);
    }

    private List<int> PlayerVision()
    {
        List<int> dx = new List<int>();
        List<int> dy = new List<int>();

        for (int i = -_saveData.userData.currentVISION; i <= _saveData.userData.currentVISION; i++)
        {
            dx.Add(i);
            dy.Add(i);
        }

        return GetNearbyBlocks_Diagonal(dx, dy, _saveData.userData.data.currentNodeIndex);
    }

    private List<int> PlayerSearchNearby()
    {
        List<System.Action> actions = new List<System.Action>();
        List<int> NearbyIndexs = PlayerVision();

        bool Non = false;

        for (int i = 0; i < NearbyIndexs.Count; i++)
        {
            int index = NearbyIndexs[i];

            if (_saveData.mapData.exitNodeIndex == index)
            {
                actions.Add(() => 
                {
                    _textView.UpdateText(eFind.Exit, _saveData.mapData.nodeDatas[index], _saveData.mapData.nodeDatas[_saveData.userData.data.currentNodeIndex]);
                });

                Non = true;

                continue;
            }

            if (_saveData.mapData.nodeDatas[index].isItem == true)
            {
                actions.Add(() =>
                {
                    _textView.UpdateText(eFind.Item, _saveData.mapData.nodeDatas[index], _saveData.mapData.nodeDatas[_saveData.userData.data.currentNodeIndex]);
                });

                Non = true;

                continue;
            }

            if (_saveData.mapData.nodeDatas[index].isMonster == true)
            {
                actions.Add(() =>
                {
                    _textView.UpdateText(eFind.Monster, _saveData.mapData.nodeDatas[index], _saveData.mapData.nodeDatas[_saveData.userData.data.currentNodeIndex]);
                });

                Non = true;

                continue;
            }
        }

        if (Non == false)
        {
            _textView.UpdateText("--- 주변에 발견된게 없습니다.");

            return NearbyIndexs;
        }

        _textView.UpdateText("--- 주변 검색을 시작합니다.");

        foreach (var action in actions)
        {
            action?.Invoke();
        }

        _textView.UpdateText("--- 주변 검색이 끝났습니다.");

        return NearbyIndexs;
    }

    private void PlayerTurnOut()
    {
        _isPlayerTurn = false;

        SkillRemindDuration();

        _textView.UpdateText("--- " + _saveData.userData.data.name + "의 순서가 종료됩니다.");
    }

    #endregion

    #region Monster

    IEnumerator MonsterTurn()
    {
        if (_isAllMonsterDead == true)
        {
            _textView.UpdateText("--- 살아있는 몬스터가 없습니다. 순서를 건너뜁니다.");
            PlayerTurn();

            yield break;
        }

        _textView.UpdateText("--- 몬스터의 순서입니다.");

        int attackMonsterIndex = 0;
        bool isAttack = false;

        for (int m = 0; m < _saveData.mapData.monsterDatas.Count; m++)
        {
            yield return new WaitForSeconds(0.5f);

            if(isAttack == false)
            {
                if (MonsterAttack(m) == true)
                {
                    attackMonsterIndex = m;

                    isAttack = true;
                }
            }

            MonsterMove(m, 1);
        }

        if(isAttack == true)
        {
            Attack(_saveData.mapData.monsterDatas[attackMonsterIndex].currentNodeIndex, () =>
            {
                MonsterTurnOut();
            });

            yield break;
        }

        MonsterTurnOut();
    }

    private void MonsterTurnOut()
    {
        _textView.UpdateText("--- 몬스터의 순서가 종료되었습니다.");

        UpdateData();
        PlayerTurn();
    }

    private bool MonsterAttack(int m)
    {
        List<int> dx = new List<int>();
        List<int> dy = new List<int>();

        for (int i = -_saveData.mapData.monsterDatas[m].attackRange; i <= _saveData.mapData.monsterDatas[m].attackRange; i++)
        {
            dx.Add(i);
            dy.Add(i);
        }

        bool isAttack = false;
        List<int> NearbyIndexs = GetNearbyBlocks(_saveData.mapData.monsterDatas[m].currentNodeIndex);

        for (int i = 0; i < NearbyIndexs.Count; i++)
        {
            if (_saveData.userData.data.currentNodeIndex == NearbyIndexs[i])
            {
                isAttack = true;

                break;
            }
        }

        if (isAttack == true)
        {
            return isAttack;
        }

        return isAttack;
    }

    private void MonsterMove(int m, int ap)
    {
        List<int> dx = new List<int>();
        List<int> dy = new List<int>();

        for (int i = -_saveData.mapData.monsterDatas[m].vision; i <= _saveData.mapData.monsterDatas[m].vision; i++)
        {
            dx.Add(i);
            dy.Add(i);
        }

        bool isFindPlayer = false;
        List<int> NearbyIndexs = GetNearbyBlocks_Diagonal(dx, dy, _saveData.mapData.monsterDatas[m].currentNodeIndex);

        for (int i = 0; i < NearbyIndexs.Count; i++)
        {
            if (_saveData.mapData.nodeDatas[NearbyIndexs[i]].isWalkable == false)
            {
                continue;
            }

            if (_saveData.mapData.nodeDatas[NearbyIndexs[i]].isMonster == true)
            {
                continue;
            }

            if (_saveData.userData.data.currentNodeIndex == NearbyIndexs[i])
            {
                isFindPlayer = true;

                break;
            }
        }

        if (isFindPlayer == true)
        {
            MonsterTargetPlayer(m);

            return;
        }

        List<int> nearbyBlocks = GetNearbyBlocks(_saveData.mapData.monsterDatas[m].currentNodeIndex);

        if (nearbyBlocks.Count == 0)
        {
            return;
        }

        MonsterSelectMoveBlock(m, ap, ref nearbyBlocks);
    }

    private void MonsterDead(DataManager.Creature_Data monster)
    {
        _saveData.mapData.nodeDatas[monster.currentNodeIndex].isMonster = false;

        _saveData.mapData.monsterDatas.Remove(_saveData.mapData.monsterDatas.Find(x => x.id == monster.id));

        UpdateData();
    }

    private void MonsterSelectMoveBlock(int m, int ap, ref List<int> nearbyBlocks)
    {
        if (ap <= 0)
        {
            return;
        }

        int randomIndex = Random.Range(0, nearbyBlocks.Count);

        if (_saveData.mapData.nodeDatas[nearbyBlocks[randomIndex]].isUser == true)
        {
            ap -= 1;
            MonsterTargetPlayer(m);

            return;
        }

        if (_saveData.mapData.nodeDatas[nearbyBlocks[randomIndex]].isWalkable == false)
        {
            MonsterSelectMoveBlock(m, ap, ref nearbyBlocks);

            return;
        }

        if (_saveData.mapData.nodeDatas[nearbyBlocks[randomIndex]].isMonster == true)
        {
            MonsterSelectMoveBlock(m, ap, ref nearbyBlocks);

            return;
        }

        _saveData.mapData.nodeDatas[_saveData.mapData.monsterDatas[m].currentNodeIndex].isMonster = false;
        _saveData.mapData.nodeDatas[nearbyBlocks[randomIndex]].isMonster = true;

        _saveData.mapData.monsterDatas[m].currentNodeIndex = nearbyBlocks[randomIndex];
        ap -= 1;

        MonsterSelectMoveBlock(m, ap, ref nearbyBlocks);
    }

    private void MonsterTargetPlayer(int m)
    {
        DataManager.Creature_Data player = _saveData.userData.data;

        int result = _mapGenerator.PathFinding(ref _saveData.mapData, _saveData.mapData.monsterDatas[m].currentNodeIndex, player.currentNodeIndex);

        _saveData.mapData.nodeDatas[_saveData.mapData.monsterDatas[m].currentNodeIndex].isMonster = false;
        _saveData.mapData.nodeDatas[result].isMonster = true;

        _saveData.mapData.monsterDatas[m].currentNodeIndex = result;
    }

    #endregion

    #region Action

    private void Attack(int nodeMonsterIndex, System.Action onLastCallback = null)
    {
        DataManager.Creature_Data monster = _saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == nodeMonsterIndex);

        _saveData.userData.currentAP -= 1;
        _ingameUI.UpdatePlayerInfo(eStats.AP, _saveData.userData);

        _ingameUI.CallAttacker(_saveData.userData, monster, onLastCallback, (result, damage) => 
        {
            if(result == eWinorLose.Lose)
            {
                _textView.UpdateText("--- " + monster.name + " (이)가 승리했습니다.");

                int monsterAttack = damage + monster.attack;

                _textView.UpdateText("--- " + monster.name + " (이)가 " + monsterAttack + " 의 공격력으로 공격합니다.");

                int playerDef = _saveData.userData.currentDEFENCE + (int)(_saveData.userData.currentDEFENCE * (0.01f * _saveData.userData.Defence_Effect_Per)) + _saveData.userData.Defence_Effect;
                ushort resultDamage = (ushort)Mathf.Abs((playerDef - monsterAttack));
                _saveData.userData.currentHP -= resultDamage;

                _textView.UpdateText("--- " + resultDamage + " 의 피해를 입었습니다.");

                if(_saveData.userData.currentHP > 60000)
                {
                    _saveData.userData.currentHP = 0;
                }

                UpdateData();

                return;
            }

            if(result == eWinorLose.Draw)
            {
                _textView.UpdateText("--- 무승부입니다.");

                UpdateData();

                return;
            }

            _textView.UpdateText("--- " + _saveData.userData.data.name + " (이)가 승리했습니다.");

            int playerDamage = damage + (int)(damage * (0.01f * _saveData.userData.Attack_Effect_Per)) + _saveData.userData.Attack_Effect;
            int _damage = monster.defence - playerDamage;

            if(_damage >= 0)
            {
                _textView.UpdateText(monster.name + " 의 방어도에 막혔습니다.");

                return;
            }
            else
            {
                _textView.UpdateText("--- " + _saveData.userData.data.name + " (이)가 " + playerDamage + " 의 공격력으로 공격합니다.");
                _textView.UpdateText("방어도를 제외한 " + Mathf.Abs(_damage) + " 의 데미지를 가했습니다.");

                monster.hp -= (ushort)Mathf.Abs(_damage);

                if (monster.hp >= 60000 || monster.hp == 0)
                {
                    _saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == nodeMonsterIndex).hp = 0;

                    _textView.UpdateText(monster.name + " (을)를 처치하였습니다");
                    _textView.UpdateText("--- 경험치 " + monster.exp + " , 코인 " + monster.coin + "을 획득했습니다");
                    
                    if(monster.itemIndexs != null)
                    {
                        if(monster.itemIndexs.Count > 0)
                        {
                            _textView.UpdateText("--- 아이템 " + monster.itemIndexs.Count + " 개를 획득했습니다.");
                        }
                    }
                     
                    _saveData.userData.data.coin += (ushort)(monster.coin + (monster.coin * 0.01f * _saveData.userData.Coin_Effect_Per));
                    _saveData.userData.currentEXP += (ushort)(monster.exp + (monster.exp * 0.01f * _saveData.userData.EXP_Effect_Per));

                    if (_saveData.userData.maximumEXP <= _saveData.userData.currentEXP)
                    {
                        _textView.UpdateText("레벨이 증가했습니다 !");

                        _saveData.userData.level += 1;
                        _saveData.userData.currentEXP = (ushort)Mathf.Abs(_saveData.userData.maximumEXP - _saveData.userData.currentEXP);

                        _ingameUI.UpdatePlayerInfo(eStats.EXP, _saveData.userData);
                        _ingameUI.UpdatePlayerInfo(eStats.Level, _saveData.userData);

                        _ingameUI.OpneLevelPoint(_saveData.userData, (newData) => 
                        {
                            _saveData.userData.data.hp = newData.data.hp;
                            _saveData.userData.data.mp = newData.data.mp;
                            _saveData.userData.data.ap = newData.data.ap;
                            _saveData.userData.data.attack = newData.data.attack;
                            _saveData.userData.data.defence = newData.data.defence;
                            _saveData.userData.data.vision = newData.data.vision;
                            _saveData.userData.data.attackRange = newData.data.attackRange;

                            _saveData.userData.currentHP = _saveData.userData.maximumHP;
                            _saveData.userData.currentMP = _saveData.userData.maximumMP;
                            _saveData.userData.currentAP = _saveData.userData.maximumAP;

                            _ingameUI.UpdatePlayerInfo(_saveData.userData);
                        });
                    }

                    MonsterDead(monster);
                }
                else
                {
                    _textView.UpdateText(monster.name + "의 체력이 " + monster.hp + " 만큼 남았습니다.");

                    _saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == nodeMonsterIndex).hp = monster.hp;
                }

                if(_saveData.mapData.monsterDatas.Count == 0)
                {
                    _isAllMonsterDead = true;
                }

                UpdateData();
            }
        });
    }

    private void Defence()
    {
        if (_saveData.userData.currentAP == 0)
        {
            _textView.UpdateText("--- 남아있는 행동력이 없습니다.");

            return;
        }

        ushort ap = _saveData.userData.currentAP;
        _saveData.userData.currentDEFENCE += ap;

        _textView.UpdateText("행동력을 모두 소진하여 방어도가 " + ap + "만큼 증가했습니다.");

        _ingameUI.UpdatePlayerInfo(eStats.AP, _saveData.userData);
    }

    private void Skill(int id)
    {
        int reCooldown = 0;
        if(SkillCheckCoolDown(id, ref reCooldown) == false)
        {
            _textView.UpdateText("대기순서가 " + reCooldown  + "만큼 남았습니다.");

            return;
        }

        DataManager.Skill_Data data = GameManager.instance.dataManager.GetskillData(id);

        DataManager.Skill_CoolDown cooldown = new DataManager.Skill_CoolDown();
        cooldown.id = data.id;
        cooldown.name = data.name;
        cooldown.coolDown = data.coolDown;
        _saveData.userData.coolDownSkill.Add(cooldown);

        int useMP = data.usemp;
        int reMP = _saveData.userData.currentMP - useMP;
        _saveData.userData.currentMP = (ushort)(reMP < 0 ? 0 : reMP);

        SkillConsumptionCheck(data, eStats.HP, data.hp);
        SkillConsumptionCheck(data, eStats.MP, data.mp);
        SkillConsumptionCheck(data, eStats.AP, data.ap);

        SkillConsumptionCheck(data, eStats.EXP, data.exp);
        SkillConsumptionCheck(data, eStats.EXP, data.expPercentIncreased, true);

        SkillConsumptionCheck(data, eStats.Coin, data.coin);
        SkillConsumptionCheck(data, eStats.Coin, data.coinPercentIncreased, true);

        SkillConsumptionCheck(data, eStats.Attack, data.attack);
        SkillConsumptionCheck(data, eStats.Attack, data.attackPercentIncreased, true);

        SkillConsumptionCheck(data, eStats.Defence, data.defence);
        SkillConsumptionCheck(data, eStats.Defence, data.defencePercentIncreased, true);

        _textView.UpdateText("스킬 " + data.name + " (을)를 발동하셨습니다.");
    }

    private bool SkillCheckCoolDown(int id, ref int cooldown)
    {
        for (int i = 0; i < _saveData.userData.coolDownSkill.Count; i++)
        {
            if (_saveData.userData.coolDownSkill[i].id == id)
            {
                cooldown = _saveData.userData.coolDownSkill[i].coolDown;
                return false;
            }
        }

        return true;
    }

    private void SkillConsumptionCheck(DataManager.Skill_Data data, eStats type, int useValue, bool isPercent = false)
    {
        eSkill_IncreaseDecrease result = eSkill_IncreaseDecrease.Non;

        if (useValue == 0)
        {
            return;
        }

        if (useValue < 0)
        {
            result = eSkill_IncreaseDecrease.Decrease;
        }

        if (useValue == -9999)
        {
            result = eSkill_IncreaseDecrease.ALLDecrease;
        }

        if (useValue > 0)
        {
            result = eSkill_IncreaseDecrease.Increase;
        }

        DataManager.Skill_Duration temp = new DataManager.Skill_Duration();
        temp.skill_ID = data.id;
        temp.name = data.name;
        temp.stats = type;
        temp.inDe = result;
        temp.isPercent = isPercent;
        temp.value = useValue;

        _saveData.userData.useSkill.Add(temp);
    }

    private void SkillRemindDuration()
    {
        for (int i = _saveData.userData.useSkill.Count - 1; i >= 0; i--)
        {
            if(_saveData.userData.useSkill[i].remaindDuration == 0)
            {
                _textView.UpdateText(_saveData.userData.useSkill[i].name + " 의 효과가 끝났습니다.");
                SkillRemoveEffect(_saveData.userData.useSkill[i]);

                _saveData.userData.useSkill.Remove(_saveData.userData.useSkill[i]);

                continue;
            }

            SkillApplyEffect(_saveData.userData.useSkill[i]);
            _saveData.userData.useSkill[i].remaindDuration -= 1;
        }

        for (int i = _saveData.userData.coolDownSkill.Count - 1; i >= 0; i--)
        {
            if(_saveData.userData.coolDownSkill[i].coolDown == 0)
            {
                _textView.UpdateText(_saveData.userData.coolDownSkill[i].name + " (을)를 다시 사용 할 수 있습니다.");

                _saveData.userData.coolDownSkill.Remove(_saveData.userData.coolDownSkill[i]);

                continue;
            }

            _saveData.userData.coolDownSkill[i].coolDown -= 1;
        }

        _ingameUI.UpdatePlayerInfo(_saveData.userData);
    }

    private void SkillRemoveEffect(DataManager.Skill_Duration useSkill)
    {
        switch (useSkill.stats)
        {
            case eStats.HP:
                {
                    _saveData.userData.HP_Effect -= (short)(useSkill.inDe == eSkill_IncreaseDecrease.Increase ? useSkill.value : (useSkill.value * -1));
                }
                break;

            case eStats.MP:
                {
                    _saveData.userData.MP_Effect -= (short)(useSkill.inDe == eSkill_IncreaseDecrease.Increase ? useSkill.value : (useSkill.value * -1));
                }
                break;

            case eStats.AP:
                {
                    _saveData.userData.AP_Effect -= (short)(useSkill.inDe == eSkill_IncreaseDecrease.Increase ? useSkill.value : (useSkill.value * -1));
                }
                break;

            case eStats.EXP:
                {
                    if (useSkill.isPercent == true)
                    {
                        _saveData.userData.EXP_Effect_Per -= (short)(useSkill.inDe == eSkill_IncreaseDecrease.Increase ? useSkill.value : (useSkill.value * -1));

                        return;
                    }

                    _saveData.userData.EXP_Effect -= (short)(useSkill.inDe == eSkill_IncreaseDecrease.Increase ? useSkill.value : (useSkill.value * -1));
                }
                break;

            case eStats.Coin:
                {
                    if (useSkill.isPercent == true)
                    {
                        _saveData.userData.Coin_Effect_Per -= (short)(useSkill.inDe == eSkill_IncreaseDecrease.Increase ? useSkill.value : (useSkill.value * -1));

                        return;
                    }

                    _saveData.userData.Coin_Effect -= (short)(useSkill.inDe == eSkill_IncreaseDecrease.Increase ? useSkill.value : (useSkill.value * -1));
                }
                break;

            case eStats.Attack:
                {
                    if (useSkill.isPercent == true)
                    {
                        _saveData.userData.Attack_Effect_Per -= (short)(useSkill.inDe == eSkill_IncreaseDecrease.Increase ? useSkill.value : (useSkill.value * -1));

                        return;
                    }

                    _saveData.userData.Attack_Effect -= (short)(useSkill.inDe == eSkill_IncreaseDecrease.Increase ? useSkill.value : (useSkill.value * -1));
                }
                break;

            case eStats.Defence:
                {
                    if (useSkill.isPercent == true)
                    {
                        _saveData.userData.Defence_Effect_Per -= (short)(useSkill.inDe == eSkill_IncreaseDecrease.Increase ? useSkill.value : (useSkill.value * -1));

                        return;
                    }

                    _saveData.userData.Defence_Effect -= (short)(useSkill.inDe == eSkill_IncreaseDecrease.Increase ? useSkill.value : (useSkill.value * -1));
                }
                break;
        }
    }

    private void SkillApplyEffect(DataManager.Skill_Duration useSkill)
    {
        switch(useSkill.stats)
        {
            case eStats.HP:
                {
                    _saveData.userData.HP_Effect += (short)(useSkill.inDe == eSkill_IncreaseDecrease.Increase ? useSkill.value : (useSkill.value * -1));
                }
                break;

            case eStats.MP:
                {
                    _saveData.userData.MP_Effect += (short)(useSkill.inDe == eSkill_IncreaseDecrease.Increase ? useSkill.value : (useSkill.value * -1));
                }
                break;

            case eStats.AP:
                {
                    _saveData.userData.AP_Effect += (short)(useSkill.inDe == eSkill_IncreaseDecrease.Increase ? useSkill.value : (useSkill.value * -1));
                }
                break;

            case eStats.EXP:
                {
                    if(useSkill.isPercent == true)
                    {
                        _saveData.userData.EXP_Effect_Per += (short)(useSkill.inDe == eSkill_IncreaseDecrease.Increase ? useSkill.value : (useSkill.value * -1));

                        return;
                    }

                    _saveData.userData.EXP_Effect += (short)(useSkill.inDe == eSkill_IncreaseDecrease.Increase ? useSkill.value : (useSkill.value * -1));
                }
                break;

            case eStats.Coin:
                {
                    if (useSkill.isPercent == true)
                    {
                        _saveData.userData.Coin_Effect_Per += (short)(useSkill.inDe == eSkill_IncreaseDecrease.Increase ? useSkill.value : (useSkill.value * -1));

                        return;
                    }

                    _saveData.userData.Coin_Effect += (short)(useSkill.inDe == eSkill_IncreaseDecrease.Increase ? useSkill.value : (useSkill.value * -1));
                }
                break;

            case eStats.Attack:
                {
                    if (useSkill.isPercent == true)
                    {
                        _saveData.userData.Attack_Effect_Per += (short)(useSkill.inDe == eSkill_IncreaseDecrease.Increase ? useSkill.value : (useSkill.value * -1));

                        return;
                    }

                    _saveData.userData.Attack_Effect += (short)(useSkill.inDe == eSkill_IncreaseDecrease.Increase ? useSkill.value : (useSkill.value * -1));
                }
                break;

            case eStats.Defence:
                {
                    if (useSkill.isPercent == true)
                    {
                        _saveData.userData.Defence_Effect_Per += (short)(useSkill.inDe == eSkill_IncreaseDecrease.Increase ? useSkill.value : (useSkill.value * -1));

                        return;
                    }

                    _saveData.userData.Defence_Effect += (short)(useSkill.inDe == eSkill_IncreaseDecrease.Increase ? useSkill.value : (useSkill.value * -1));
                }
                break;
        }
    }

    private void Bag(int id)
    {
        eStats type = eStats.Non;
        int value = 0;
        int maxValue = 0;



        _ingameUI.UpdatePlayerInfo(type, _saveData.userData);
    }

    #endregion

    #region Function

    private void UpdateData()
    {
        _ingameUI.UpdatePlayerInfo(_saveData.userData);

        if(_saveData.userData.currentHP == 0)
        {
            OpenNextRound(eRoundClear.Fail);

            return;
        }

        _mapController.UpdateMapData(_saveData, PlayerVision());
    }

    private int GetNearbyBlocks(int x, int y, int index)
    {
        int result = 0;

        if (((y == -1 || y == 1) && x == 0) || (y == 0 && (x == 1 || x == -1)))
        {
            int nearbyX = (index % _saveData.mapData.mapSize) + x;

            if (nearbyX < 0 || _saveData.mapData.mapSize <= nearbyX)
            {
                return -1;
            }

            int nearbyY = (index / _saveData.mapData.mapSize) + y;

            if (nearbyY < 0 || _saveData.mapData.mapSize <= nearbyY)
            {
                return -1;
            }

            int resultIndex = nearbyX + (nearbyY * _saveData.mapData.mapSize);

            if (index == resultIndex)
            {
                return -1;
            }

            if (0 <= resultIndex && resultIndex < (_saveData.mapData.mapSize * _saveData.mapData.mapSize))
            {
                result = resultIndex;
            }
        }

        return result;
    }

    int[] dx = new int[] { -1, 0, 1 };
    int[] dy = new int[] { -1, 0, 1 };
    private List<int> GetNearbyBlocks(int index)
    {
        List<int> result = new List<int>();

        for (int y = 0; y < dy.Length; y++)
        {
            for (int x = 0; x < dx.Length; x++)
            {
                if (Mathf.Abs(dx[x]) + Mathf.Abs(dy[y]) <= Mathf.Abs(dx[dx.Length - 1]))
                {
                    int nearbyX = (index % _saveData.mapData.mapSize) + dx[x];

                    if (nearbyX < 0 || _saveData.mapData.mapSize <= nearbyX)
                    {
                        continue;
                    }

                    int nearbyY = (index / _saveData.mapData.mapSize) + dy[y];

                    if (nearbyY < 0 || _saveData.mapData.mapSize <= nearbyY)
                    {
                        continue;
                    }

                    int resultIndex = nearbyX + (nearbyY * _saveData.mapData.mapSize);

                    if (index == resultIndex)
                    {
                        continue;
                    }

                    if (resultIndex == _saveData.mapData.exitNodeIndex)
                    {
                        continue;
                    }

                    if (0 <= resultIndex && resultIndex < (_saveData.mapData.mapSize * _saveData.mapData.mapSize))
                    {
                        result.Add(resultIndex);
                    }
                }
            }
        }

        return result;
    }

    private List<int> GetNearbyBlocks_Diagonal(List<int> dx, List<int> dy, int index)
    {
        List<int> result = new List<int>();

        for (int y = 0; y < dy.Count; y++)
        {
            for (int x = 0; x < dx.Count; x++)
            {
                if(Mathf.Abs(dx[x]) + Mathf.Abs(dy[y]) <= Mathf.Abs(dx[dx.Count - 1]) || 
                    Mathf.Abs(dx[x]) <= 1 && Mathf.Abs(dy[y]) <= 1)
                {
                    int nearbyX = (index % _saveData.mapData.mapSize) + dx[x];

                    if (nearbyX < 0 || _saveData.mapData.mapSize <= nearbyX)
                    {
                        continue;
                    }

                    int nearbyY = (index / _saveData.mapData.mapSize) + dy[y];

                    if (nearbyY < 0 || _saveData.mapData.mapSize <= nearbyY)
                    {
                        continue;
                    }

                    int resultIndex = nearbyX + (nearbyY * _saveData.mapData.mapSize);

                    if (index == resultIndex)
                    {
                        continue;
                    }

                    if (0 <= resultIndex && resultIndex < (_saveData.mapData.mapSize * _saveData.mapData.mapSize))
                    {
                        result.Add(resultIndex);
                    }
                }
            }
        }

        return result;
    }

    #endregion
}

public class MapGenerator
{
    private class Node
    {
        public bool isEnter = false;
        public bool isExit = false;

        public bool isVisit = false;
        public bool isWalkable = true;

        public int _index = 0;
        public int _x = 0;
        public int _y = 0;

        public int _g = 0; // cost to start
        public int _h = 0; // cost to end
        public int f 
        { 
            get { return _g + _h; }
        }

        public Node(int index, int x, int y)
        {
            _index = index;
            _x = x;
            _y = y;
        }

        public void SetStartEnd(Node startNode, Node endNode)
        {
            _g = (int)Mathf.Sqrt(Mathf.Pow(Mathf.Abs(_x - startNode._x), 2) + Mathf.Pow(Mathf.Abs(_y - startNode._y), 2));
            _h = (int)Mathf.Sqrt(Mathf.Pow(Mathf.Abs(_x - endNode._x), 2) + Mathf.Pow(Mathf.Abs(_y - endNode._y), 2));
        }
    }

    private System.Action<DataManager.Map_Data> _onResultCallback = null;
    private DataManager.Save_Data _saveData = null;

    private List<Node> _nodes = null;

    private int _mapSize = 0;

    public MapGenerator(System.Action<DataManager.Map_Data> onResultCallback, DataManager.Save_Data saveData)
    {
        if(onResultCallback != null)
        {
            _onResultCallback = onResultCallback;
        }

        _mapSize = saveData.mapData.mapSize;
        _saveData = saveData;

        Start();
    }

    private void Start()
    {
        _nodes = new List<Node>();

        for (int y = 0; y < _mapSize; y++)
        {
            for (int x = 0; x < _mapSize; x++)
            {
                Node node = new Node((y * _mapSize) + x, x, y);

                _nodes.Add(node);
            }
        }

        GenerateBlocker();
        SelectExitBlock();
        Done();
    }

    private void GenerateBlocker()
    {
        List<int> middlePoints = new List<int>();

        int middleMapSize = _mapSize / 3;

        for (int y = 0; y < middleMapSize; y++)
        {
            for (int x = 0; x < middleMapSize; x++)
            {
                int middleX = 1 + (3 * x);
                int middleY = 1 + (3 * y);
                int middleCoord = middleX + (middleY * _mapSize);

                if (middleCoord == (Mathf.Pow(_mapSize, 2) - 1) * 0.5f)
                {
                    continue;
                }

                middlePoints.Add(middleCoord);
            }
        }

        List<int> randomBlockerIndexs = RandomIndex(middlePoints.Count, 3);

        for (int r = 0; r < randomBlockerIndexs.Count; r++)
        {
            List<int> nearbyBlocks = GetNearbyBlocks_Diagonal(middlePoints[randomBlockerIndexs[r]]);
            _nodes[middlePoints[randomBlockerIndexs[r]]].isWalkable = false;

            for (int n = 0; n < nearbyBlocks.Count; n++)
            {
                _nodes[nearbyBlocks[n]].isWalkable = false;
            }
        }

        CheckStuckBlocker(ref middlePoints);
        SelectEnterBlock(ref middlePoints);
    }

    private void CheckStuckBlocker(ref List<int> middlePoints)
    {
        for (int m = 0; m < middlePoints.Count; m++)
        {
            if (_nodes[middlePoints[m]].isWalkable == false)
            {
                continue;
            }

            List<int> NearbyMiddlePoints = NearbyMiddlePoint(middlePoints[m]);
            bool isStuck = true;

            for (int n = 0; n < NearbyMiddlePoints.Count; n++)
            {
                if (_nodes[NearbyMiddlePoints[n]].isWalkable == true)
                {
                    isStuck = false;
                    break;
                }
            }

            if(isStuck == true)
            {
                List<int> nearbyBlocks = GetNearbyBlocks_Diagonal(middlePoints[m]);
                _nodes[middlePoints[m]].isWalkable = false;

                for (int n = 0; n < nearbyBlocks.Count; n++)
                {
                    _nodes[nearbyBlocks[n]].isWalkable = false;
                }
            }
        }
    }

    private void SelectEnterBlock(ref List<int> middlePoints)
    {
        List<int> openMiddlePoints = new List<int>();

        for (int i = 0; i < middlePoints.Count; i++)
        {
            if(_nodes[middlePoints[i]].isWalkable == false)
            {
                continue;
            }

            openMiddlePoints.Add(middlePoints[i]);
        }

        _nodes[openMiddlePoints[Random.Range(0, openMiddlePoints.Count)]].isEnter = true;
    }

    private void SelectExitBlock()
    {
        int centerIndex = (int)((Mathf.Pow(_mapSize, 2) - 1) * 0.5f);

        _nodes[centerIndex].isExit = true;
    }

    private void Done()
    {
        _saveData.mapData = new DataManager.Map_Data();
        _saveData.mapData.mapSize = _mapSize;
        _saveData.mapData.nodeDatas = new List<DataManager.Node_Data>();

        for (int n = 0; n < _nodes.Count; n++)
        {
            DataManager.Node_Data node = new DataManager.Node_Data();
            node.index = _nodes[n]._index;
            node.x = (ushort)_nodes[n]._x;
            node.y = (ushort)_nodes[n]._y;
            node.isWalkable = _nodes[n].isWalkable;

            if(_nodes[n].isEnter == true)
            {
                _saveData.mapData.enterNodeIndex = n;
            }

            if (_nodes[n].isExit == true)
            {
                _saveData.mapData.exitNodeIndex = n;
            }

            _saveData.mapData.nodeDatas.Add(node);
        }

        CreatureGenerator _creatureGenerator = new CreatureGenerator(GenerateCreature, _saveData);
    }

    private void GenerateCreature(DataManager.Creature_Data playerData, List<DataManager.Creature_Data> monsterData)
    {
        _saveData.userData.data = playerData;
        _saveData.mapData.monsterDatas = monsterData;

        _onResultCallback?.Invoke(_saveData.mapData);
    }

    private List<int> RandomIndex(int listCount, int value)
    {
        List<int> result = new List<int>();

        int count = 0;

        while(count < value)
        {
            int num = UnityEngine.Random.Range(0, listCount);
            bool isSelect = false;

            for (int i = 0; i < result.Count; i++)
            {
                if(result[i] == num)
                {
                    isSelect = true;
                    break;
                }
            }

            if(isSelect == true)
            {
                continue;
            }

            result.Add(num);
            count++;
        }

        return result;
    }

    int[] dx = new int[] { -1, 0, 1 };
    int[] dy = new int[] { -1, 0, 1 };

    private List<int> GetNearbyBlocks_Diagonal(int index)
    {
        List<int> result = new List<int>();

        for (int y = 0; y < dy.Length; y++)
        {
            for (int x = 0; x < dx.Length; x++)
            {
                int nearbyX = (index % _mapSize) + dx[x];
                int nearbyY = (index / _mapSize) + dy[y];
                int resultIndex = nearbyX + (nearbyY * _mapSize);

                if (index == resultIndex)
                {
                    continue;
                }

                if (0 <= resultIndex && resultIndex < (_mapSize * _mapSize))
                {
                    result.Add(resultIndex);
                }
            }
        }

        return result;
    }

    private List<int> GetNearbyBlocks(int index)
    {
        List<int> result = new List<int>();

        for (int y = 0; y < dy.Length; y++)
        {
            for (int x = 0; x < dx.Length; x++)
            {
                if(((dy[y] == -1 || dy[y] == 1) && dx[x] == 0) || (dy[y] == 0 && (dx[x] == 1 || dx[x] == -1)))
                {
                    int nearbyX = (index % _mapSize) + dx[x];

                    if (nearbyX < 0 || _mapSize <= nearbyX)
                    {
                        continue;
                    }

                    int nearbyY = (index / _mapSize) + dy[y];

                    if (nearbyY < 0 || _mapSize <= nearbyY)
                    {
                        continue;
                    }

                    int resultIndex = nearbyX + (nearbyY * _mapSize);

                    if (index == resultIndex)
                    {
                        continue;
                    }

                    if (0 <= resultIndex && resultIndex < (_mapSize * _mapSize))
                    {
                        result.Add(resultIndex);
                    }
                }
            }
        }

        return result;
    }

    int[] mdx = new int[] { -3, 0, 3 };
    int[] mdy = new int[] { -3, 0, 3 };

    private List<int> NearbyMiddlePoint(int index)
    {
        List<int> result = new List<int>();

        for (int y = 0; y < mdy.Length; y++)
        {
            for (int x = 0; x < mdx.Length; x++)
            {
                if(((mdy[y] == -3 || mdy[y] == 3) && mdx[x] == 0) || (mdy[y] == 0 && (mdx[x] == 3 || mdx[x] == -3)))
                {
                    int nearbyX = (index % _mapSize) + mdx[x];

                    if (nearbyX < 0 || _mapSize <= nearbyX)
                    {
                        continue;
                    }

                    int nearbyY = (index / _mapSize) + mdy[y];

                    if (nearbyY < 0 || _mapSize <= nearbyY)
                    {
                        continue;
                    }

                    int resultIndex = nearbyX + (nearbyY * _mapSize);

                    if (index == resultIndex)
                    {
                        continue;
                    }

                    if (0 <= resultIndex && resultIndex < (_mapSize * _mapSize))
                    {
                        result.Add(resultIndex);
                    }
                }
            }
        }

        return result;
    }

    public int PathFinding(ref DataManager.Map_Data mapData, int startNodeIndex, int endNodeIndex)
    {
        int result = startNodeIndex;

        foreach (var node in _nodes)
        {
            node.SetStartEnd(_nodes[startNodeIndex], _nodes[endNodeIndex]);
        }

        List<int> nearbyNodeIndexs = GetNearbyBlocks(startNodeIndex);

        int f = 9999;

        for (int i = 0; i < nearbyNodeIndexs.Count; i++)
        {
            int index = nearbyNodeIndexs[i];

            if (_nodes[index].f < f)
            {
                if(mapData.nodeDatas[index].isMonster == true)
                {
                    continue;
                }

                if (mapData.nodeDatas[index].isWalkable == false)
                {
                    continue;
                }

                if (mapData.nodeDatas[index].isUser == true)
                {
                    continue;
                }

                if (index == mapData.enterNodeIndex)
                {
                    continue;
                }

                if (index == mapData.exitNodeIndex)
                {
                    continue;
                }

                result = index;
                f = _nodes[index].f;
            }
        }

        return result;
    }
}

public class CreatureGenerator
{
    private System.Action<DataManager.Creature_Data, List<DataManager.Creature_Data>> _onResultCallback = null;
    private DataManager.Save_Data _saveData = null;

    public CreatureGenerator(System.Action<DataManager.Creature_Data, List<DataManager.Creature_Data>> onResultCallback, DataManager.Save_Data saveData)
    {
        if(onResultCallback != null)
        {
            _onResultCallback = onResultCallback;
        }

        _saveData = saveData;

        Start();
    }

    private void Start()
    {
        GeneratePlayer();
        GenerateMonster();
        Done();
    }

    private void GeneratePlayer()
    {
        _saveData.userData.data.currentNodeIndex = _saveData.mapData.enterNodeIndex;
        _saveData.mapData.nodeDatas[_saveData.userData.data.currentNodeIndex].isUser = true;
    }

    private void GenerateMonster()
    {
        _saveData.mapData.monsterDatas = new List<DataManager.Creature_Data>();

        int openNodeCount = 0;

        for (int i = 0; i < _saveData.mapData.nodeDatas.Count; i++)
        {
            if(_saveData.mapData.nodeDatas[i].isWalkable == false)
            {
                continue;
            }

            openNodeCount++;
        }

        int monsterSpawnCount = Random.Range(2, openNodeCount / _saveData.mapData.mapSize);

        for (int i = 0; i < monsterSpawnCount; i++)
        {
            DataManager.Node_Data node = new DataManager.Node_Data();
            SpawnMonsterNodeSelect(Random.Range(0, _saveData.mapData.nodeDatas.Count), ref node);

            DataManager.Creature_Data creature = GameManager.instance.dataManager.GetCreatureData(_saveData.round + i);
            
            if (creature != null)
            {
                creature.id = (ushort)i;
                creature.currentNodeIndex = node.index;
                _saveData.mapData.nodeDatas[node.index].isMonster = true;

                MonsterStats(ref creature);

                _saveData.mapData.monsterDatas.Add(creature);
            }
        }
    }

    private void MonsterStats(ref DataManager.Creature_Data creature)
    {
        float increasePoint = (_saveData.round * 0.1f);

        creature.coin += (ushort)(creature.coin * increasePoint);
        creature.hp += (ushort)(creature.hp * increasePoint);
        creature.exp += (ushort)(creature.exp * increasePoint);
        creature.attack += (ushort)(creature.attack * increasePoint);
        creature.defence += (ushort)(creature.defence * (increasePoint * 0.5f));
    }

    private void Done()
    {
        _onResultCallback?.Invoke(_saveData.userData.data, _saveData.mapData.monsterDatas);
    }

    private void SpawnMonsterNodeSelect(int index, ref DataManager.Node_Data node)
    {
        if (_saveData.mapData.nodeDatas[index].isWalkable == true 
            && _saveData.mapData.nodeDatas[index].isMonster == false 
            && _saveData.mapData.nodeDatas[index].isUser == false)
        {
            node = _saveData.mapData.nodeDatas[index];

            return;
        }

        int newIndex = index;

        if(_saveData.mapData.nodeDatas[index].isWalkable == false)
        {
            newIndex += 1;
        }
        
        if(_saveData.mapData.nodeDatas[index].isMonster == true)
        {
            newIndex += 5;
        }

        if(_saveData.mapData.nodeDatas[index].isUser == true)
        {
            newIndex += 5;
        }
        
        if(index == _saveData.mapData.enterNodeIndex)
        {
            newIndex += 4;
        }

        if(index == _saveData.mapData.exitNodeIndex)
        {
            newIndex += 4;
        }

        if(newIndex >= _saveData.mapData.nodeDatas.Count)
        {
            newIndex -= _saveData.mapData.nodeDatas.Count;
        }

        SpawnMonsterNodeSelect(newIndex, ref node);
    }
}