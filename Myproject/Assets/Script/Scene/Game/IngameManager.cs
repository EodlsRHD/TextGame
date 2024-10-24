using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class IngameManager : MonoBehaviour
{
    private static IngameManager _instance = null;
    public static IngameManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new IngameManager();
            }

            return _instance;
        }
    }

    [Header("Controllers")]
    [SerializeField] private MonsterController _monsterController = null;
    [SerializeField] private PlayerCntroller _playerController = null;
    [SerializeField] private ActionController _actionController = null;
    [SerializeField] private Skill_ItemController _skillitemCountroller = null;

    [Header("Ingame UI"), SerializeField] private IngameUI _ingameUI = null;
    [Header("TextView"), SerializeField] private TextView _textView = null;
    [Header("Control"), SerializeField] private ControlPad _controlPad = null;
    [Header("MapController"), SerializeField] private MapController _mapController = null;
    [Header("Ingame Popup"), SerializeField] private IngamePopup _ingamePopup = null;
    [Header("Shop"), SerializeField] private Shop _shop = null;
    [Header("Bonfire"), SerializeField] private Bonfire _bonfire = null;

    [SerializeField] private DataManager.SaveData _saveData = null;
    private MapGenerator _mapGenerator = null;

    private bool _isPlayerTurn = false;
    private bool _isHuntMonster = false;

    private int _turnCount = 1;
    
    #region

    public DataManager.SaveData saveData
    {
        get { return _saveData; }
    }

    public bool isAllMonsterDead
    {
        get { return _monsterController.isAllMonsterDead; }
        set { _monsterController.isAllMonsterDead = value; }
    }

    public bool isPlayerTurn
    {
        get { return _isPlayerTurn; }
        set { _isPlayerTurn = value; }
    }

    public bool isHuntMonster 
    {
        get { return _isHuntMonster; }
        set { _isHuntMonster = value; }
    }

    #endregion

    void Start()
    {
        _instance = this;

        GameManager.instance.tools.Fade(true, null);

        _saveData = GameManager.instance.dataManager.CopySaveData();

        _monsterController.Initialize();
        _playerController.Initialize();
        _actionController.Initialize();
        _skillitemCountroller.Initialize();

        _ingameUI.Initialize(OpenMap, OpenNextRound);
        _textView.Initialize();
        _controlPad.Initialize(_playerController.PlayerMove, _playerController.PlayerAction);
        _mapController.Initialize(GameManager.instance.dataManager.MapSize);
        _ingamePopup.Initialize();
        _shop.Initialize(_actionController.Buy);
        _bonfire.Initialize(_actionController.SelectSkill, _controlPad.Skill);

        FirstSet();

        this.gameObject.SetActive(true);
    }

    private void FirstSet()
    {
        if (_saveData.round % 5 == 0)
        {
            PlayBgm(eBgm.Shop);
        }
        else
        {
            PlayBgm(eBgm.Ingame);
        }

        if (_saveData.round > 1)
        {
            OpenNextRoundWindow(eRoundClear.Load);

            return;
        }

        OpenNextRoundWindow(eRoundClear.First);
    }

    private void OpenMap(System.Action onCallback)
    {
        _mapController.OpenMap(onCallback);
    }

    private void OpenNextRound(eRoundClear type)
    {
        if(type == eRoundClear.Restart)
        {
            UpdateText("--- 부활하였습니다.");
            
            GameManager.instance.dataManager.ResetPlayerData();
            _ingameUI.UpdatePlayerInfo(_saveData.userData);

            return;
        }

        _textView.DeleteTemplate();

        if (type == eRoundClear.Load)
        {
            RoundSet();

            return;
        }

        if (type == eRoundClear.Fail)
        {
            GameManager.instance.tools.Fade(false, () => 
            {
                GameManager.instance.soundManager.PlaySfx(eSfx.GotoLobby);

                GameManager.instance.tools.SceneChange(eScene.Lobby, () =>
                {
                    GameManager.instance.dataManager.FailGame(_saveData);
                });
            });

            return;
        }

        if(type == eRoundClear.Success)
        {
            UpdateText("--- " + _saveData.round + " 라운드 입니다.");
            GameManager.instance.soundManager.PlaySfx(eSfx.TurnPage);
        }

        GameManager.instance.dataManager.SaveDataToCloud(_saveData, (result) => 
        {
            _mapGenerator = new MapGenerator(GenerateMap, _saveData);

            GameManager.instance.dataManager.ResetPlayerData();
            UpdateData();
        }); 
    }

    private void GenerateMap(DataManager.MapData mapData)
    {
        _saveData.mapData = mapData;
        _turnCount = 1;

        _monsterController.CreateMonster(_saveData.mapData.monsterDatas);
        _mapController.SetMap(_saveData);

        RoundSet();
    }
     
    private void RoundSet()
    {
        UpdatePlayerCoord();
        UpdateRoundText();

        UpdateData();

        PlayerTurn();

        if (_saveData.mapData.monsterDatas.Count > 0)
        {
            _monsterController.isAllMonsterDead = false;
        }
    }

    public void RoundClear()
    {
        GameManager.instance.dataManager.UpdatePlayerData(_saveData);

        _mapController.NextTurn();
        _mapController.Close();
        _saveData.round++;

        OpenNextRoundWindow(eRoundClear.Success);
    }

    #region connection Method

    public void ControlPad_Skill()
    {
        _controlPad.Skill(_saveData.userData, PlayerItem_Skill);
    }

    public void ControlPad_Bag()
    {
        _controlPad.Bag(_saveData.userData, PlayerItem_Skill);
    }

    public void PlayBgm(eBgm type)
    {
        GameManager.instance.soundManager.PlayBgm(type);
    }

    public bool isItemListEmpty()
    {
        return _saveData.userData.data.itemIndexs.Count <= 10; 
    }

    public void SetDirCoord(int nodeIndex, eDir type)
    {
        _skillitemCountroller.SetDirCoord(nodeIndex, type);
    }

    public void GetMonsterItem(short id)
    {
        if(isItemListEmpty() == false)
        {
            UpdatePopup("가방이 비어있지 않습니다.");

            List<int> list = Vision(_saveData.userData.stats.vision.current, _saveData.userData.data.currentNodeIndex);
            int ranIndex = Random.Range(0, list.Count);
            ItemData item = GameManager.instance.dataManager.GetItemData(id);
            item.currentNodeIndex = list[ranIndex];

            _saveData.mapData.nodeDatas[list[ranIndex]].isItem = true;
            _saveData.mapData.itemDatas.Add(item);

            return;
        }

        _saveData.userData.data.itemIndexs.Add(id);
        GameManager.instance.dataManager.AddEncyclopedia_Item(id);
    }

    public void GetFieldItem(int nodeIndex)
    {
        if (isItemListEmpty() == false)
        {
            UpdatePopup("가방이 비어있지 않습니다.");
            
            return;
        }

        UpdateText("아이템을 가방에 보관하였습니다.");

        short id = _saveData.mapData.itemDatas.Find(x => x.currentNodeIndex == nodeIndex).id;

        _saveData.userData.data.itemIndexs.Add(id);
        GameManager.instance.dataManager.AddEncyclopedia_Item(id);
    }

    public void GetExp(short value)
    {
        _saveData.userData.PlusExp(value, (result) => 
        { 
            if(result == false)
            {
                return;
            }

            UpdateText("레벨이 증가했습니다 !");

            UpdatePlayerInfo(eStats.EXP);
            UpdatePlayerInfo(eStats.Level);

            OpneLevelPoint();
        });
    }

    public bool CheckAbnormalStatusEffect(eStrengtheningTool type, CreatureData creature)
    {
        AbnormalStatus status = creature.abnormalStatuses.Find(x => x.currentStatus == type);

        if (status != null)
        {
            if(status.maintain == eMaintain.Temporary)
            {
                if(status.value <= 0)
                {
                    return false;
                }

                status.value -= 1;

                return true;
            }
        }

        return (status != null);
    }

    public short GetValueAbnormalStatusEffect(eStrengtheningTool type, CreatureData creature)
    {
        return (short)creature.abnormalStatuses.Find(x => x.currentStatus == type).value;
    }

    public void RemoveAbnormalStatusEffect(eStrengtheningTool type, ref CreatureData creature)
    {
        AbnormalStatus status = creature.abnormalStatuses.Find(x => x.currentStatus == type);

        if(status.maintain == eMaintain.Temporary)
        {
            creature.abnormalStatuses.Remove(status);
        }

        UpdateData();
    }

    public void PlayerDefence(Duration duration)
    {
        _skillitemCountroller.PlayerDefence(ref _saveData.userData.data, duration);
        _saveData.userData.data.item_Duration.Add(duration);

        _saveData.userData.stats.ap.current = 0;
        UpdatePlayerInfo(eStats.AP);
    }

    public bool PlayerHit(float damage)
    {
        if ((_saveData.userData.stats.hp.maximum / 3) < (short)Mathf.Abs(damage))
        {
            GameManager.instance.soundManager.PlaySfx(eSfx.Hit_hard);
        }
        else
        {
            GameManager.instance.soundManager.PlaySfx(eSfx.Hit_light);
        }

        _saveData.userData.stats.hp.MinusCurrnet((short)damage);

        if(_saveData.userData.stats.hp.current == 0)
        {
            UpdateData(damage + " 의 피해를 입었습니다.");

            return true;
        }

        UpdatePlayerInfo(eStats.HP);
        return false;
    }

    public void MonsterHit(int nodeMonsterNodeIndex, float damage)
    {
        CreatureData monster = _saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == nodeMonsterNodeIndex);

        GameManager.instance.soundManager.PlaySfx(eSfx.Attack);

        monster.stats.hp.MinusCurrnet((short)damage);

        if (monster.stats.hp.current == 0)
        {
            UpdateText(monster.name + " (을)를 처치하였습니다");
            UpdateText("--- 경험치 " + monster.stats.exp.current + " , 코인 " + monster.stats.coin.current + "을 획득했습니다");

            if (monster.itemIndexs != null)
            {
                for (int i = 0; i < monster.itemIndexs.Count; i++)
                {
                    GetMonsterItem(monster.itemIndexs[i]);
                }
            }

            _saveData.userData.stats.PlusCoin(monster.stats.coin.current);
            GetExp(monster.stats.exp.current);

            MonsterDead(monster);
        }
        else
        {
            UpdateText(monster.name + "의 체력이 " + monster.stats.hp.current + " 만큼 남았습니다.");
            _saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == nodeMonsterNodeIndex).stats.hp.current = monster.stats.hp.current;
        }
    }

    private void PlayerItem_Skill(eTool type, int id)
    {
        switch (type)
        {
            case eTool.Skill:
                GameManager.instance.soundManager.PlaySfx(eSfx.UseSkill);

                _skillitemCountroller.UseSkill(false, id, ref _saveData.userData.data);

                break;

            case eTool.Item:
                GameManager.instance.soundManager.PlaySfx(eSfx.UseItem);

                _skillitemCountroller.UseConsumptionItem(id, ref _saveData.userData.data);

                break;
        }
    }

    public void MonsterSkill(int monsterIdnex, int skill_Index)
    {
        CreatureData data = _saveData.mapData.monsterDatas[monsterIdnex];
        SkillData skill = GameManager.instance.dataManager.GetskillData(skill_Index);

        UpdateText(data.name + " (이)가 " + skill.name + " (을)를 시전했습니다.");

        int playerIndex = -1;
        eDir playerDir = eDir.Non;

        if(skill.tool.dir == eDir.DesignateDirection)
        {
            playerDir = GetTargetDirection(monsterIdnex, playerIndex);
        }
        else if(skill.tool.dir == eDir.DesignateCoordination)
        {
            playerIndex = _saveData.userData.data.currentNodeIndex;
        }

        SetDirCoord(playerIndex, playerDir);

        _skillitemCountroller.UseSkill(true, skill_Index, ref data);
    }

    #endregion

    #region ActionController

    public void BonfireOpen(DataManager.Npc_Data npc)
    {
        _bonfire.Open(npc, _saveData.userData);
    }

    public void ShopOpen(DataManager.Npc_Data npc)
    {
        _shop.Open(npc, _saveData.userData.stats.coin.current);
    }

    public void CallAttacker(CreatureData monster, Action onLastCallback, Action<eWinorLose, int> onResultCallback)
    {
        _ingameUI.CallAttacker(_saveData.userData, monster, onLastCallback, onResultCallback);
    }

    public void OpneLevelPoint()
    {
        _ingameUI.OpneLevelPoint(_saveData.userData, (newData) =>
        {
            _saveData.userData.stats.hp.point = newData.stats.hp.point;
            _saveData.userData.stats.mp.point = newData.stats.mp.point;
            _saveData.userData.stats.ap.point = newData.stats.ap.point;
            _saveData.userData.stats.attack.point = newData.stats.attack.point;
            _saveData.userData.stats.defence.point = newData.stats.defence.point;
            _saveData.userData.stats.vision.current = newData.stats.vision.current;
            _saveData.userData.stats.attackRange.point = newData.stats.attackRange.point;

            _saveData.userData.stats.Maximum();

            UpdateData();
        });
    }

    public void ControlPadUpdateData()
    {
        _controlPad.UpdateData(_saveData.userData);
    }

    public void Attack(bool isMonster, int index, Action onLastCallback = null)
    {
        _actionController.Attack(isMonster, index, onLastCallback);
    }

    public void Defence()
    {
        _actionController.Defence();
    }

    public void Npc(int index)
    {
        _actionController.Npc(index);
    }

    public void RevealMap(eDir dir, List<int> indexs)
    {
        _mapController.RevealMap(dir, indexs);

        UpdateData();
    }

    public void MonsterEffect()
    {
        _monsterController.SplitMonster();
        _monsterController.HardnessMonster();
    }

    private void DoneTurn()
    {
        _turnCount++;

        UpdateRoundText();
        _mapController.NextTurn();
        _skillitemCountroller.UserDuration(ref _saveData.userData.data);

        if(_saveData.round == 1)
        {
            return;
        }

        if((_turnCount % 5) > 0)
        {
            return;
        }

        UpdateText("--- 몬스터가 강화되었습니다.");

        for(int i = 0; i < _saveData.mapData.monsterDatas.Count; i++)
        {
            _saveData.mapData.monsterDatas[i].stats.Enforce();
        }
    }

    #endregion

    #region Creature

    public void PlayerTurn()
    {
        _isPlayerTurn = true;

        short maxAp = _saveData.userData.stats.ap.maximum;

        if(CheckAbnormalStatusEffect(eStrengtheningTool.Slowdown, _saveData.userData.data) == true)
        {
            maxAp -= GetValueAbnormalStatusEffect(eStrengtheningTool.Slowdown, _saveData.userData.data);
        }

        short recoveryMp = (short)(_saveData.userData.stats.mp.maximum * 0.05f);

        if(recoveryMp < 1)
        {
            recoveryMp = 1;
        }

        short recoveryHp = (short)(_saveData.userData.stats.hp.maximum * 0.05f);

        if(recoveryHp < 1)
        {
            recoveryHp = 1;
        }

        _saveData.userData.stats.ap.PlusCurrent(maxAp);
        _saveData.userData.stats.mp.PlusCurrent(recoveryMp);
        _saveData.userData.stats.hp.PlusCurrent(recoveryHp);

        UpdateText("행동력이 " + saveData.userData.stats.ap.current + "만큼 남았습니다.");

        _playerController.PlayerSearchNearby();
        UpdateData();
    }

    public void PlayerTurnOut()
    {
        _isPlayerTurn = false;

        _skillitemCountroller.MonsterDuration(ref _saveData.mapData);

        UpdateText("--- " + _saveData.userData.data.name + "의 순서가 종료됩니다.");
    }

    public void MonsterTurn()
    {
        StartCoroutine(_monsterController.MonsterTurn());
    }

    public void MonsterDead(CreatureData monster)
    {
        _saveData.mapData.nodeDatas[monster.currentNodeIndex].isMonster = false;
        _saveData.mapData.monsterDatas.Find(x => x.id == monster.id).stats.Reset();
        _saveData.mapData.monsterDatas.Remove(_saveData.mapData.monsterDatas.Find(x => x.id == monster.id));
        UpdateData();

        if (_saveData.mapData.monsterDatas.Count == 0)
        {
            UpdateText("--- 잠겨있던 계단이 열렸습니다.");
            GameManager.instance.soundManager.PlaySfx(eSfx.ExitOpen);

            isAllMonsterDead = true;
        }
    }

    public void MonsterTurnOut()
    {
        UpdateText("--- 몬스터의 순서가 종료되었습니다.");

        DoneTurn();

        UpdateData();
        PlayerTurn();
    }

    #endregion

    #region Update

    public void UpdatePlayerCoord()
    {
        _textView.UpdateText(_saveData.mapData.nodeDatas[_saveData.userData.data.currentNodeIndex]);
    }

    public void UpdateText(string message)
    {
        _textView.UpdateText(message);
    }

    public void UpdateText(int nearbyBlockIndex)
    {
        _textView.UpdateText(_saveData.mapData.nodeDatas[nearbyBlockIndex]);
    }

    public void UpdateText(eMapObject type, int monsterNodeIndex)
    {
        _textView.UpdateText(type, _saveData.mapData.nodeDatas[monsterNodeIndex], _saveData.mapData.nodeDatas[_saveData.userData.data.currentNodeIndex]);
    }

    public void UpdatePopup(string message)
    {
        _ingamePopup.UpdateText(message);
    }

    public void UpdatePlayerInfo(eStats type)
    {
        _ingameUI.UpdatePlayerInfo(type, _saveData.userData);
    }

    public void UpdateRoundText()
    {
        _ingameUI.SetRoundText(_saveData.round, _turnCount);
    }

    public void OpenNextRoundWindow(eRoundClear type)
    {
        _ingameUI.OpenNextRoundWindow(type);
    }

    public void ViewMap()
    {
        if (GameManager.instance.isMapBackgroundUpdate == true)
        {
            _mapController.Close(false, () =>
            {
                _ingameUI.HideMapButton();
                _mapController.UpdateMapData(_saveData, Vision(_saveData.userData.stats.vision.current, _saveData.userData.data.currentNodeIndex));
                _textView.UpdateTextViewHeight();
            });

            return;
        }

        _ingameUI.HideMapButton();
        _mapController.UpdateMapData(_saveData, Vision(_saveData.userData.stats.vision.current, _saveData.userData.data.currentNodeIndex));
        _textView.UpdateTextViewHeight();
    }

    public void UpdateData(string contnet = null)
    {
        _ingameUI.UpdatePlayerInfo(_saveData.userData);

        if (_saveData.userData.stats.hp.current == 0)
        {
            _ingameUI.OpenNextRoundWindow(eRoundClear.Fail, contnet);
        }

        _mapController.UpdateMapData(_saveData, Vision(_saveData.userData.stats.vision.current, _saveData.userData.data.currentNodeIndex));
        _monsterController.UpdateData(_saveData.mapData.monsterDatas);
    }

    public void UpdatePlayerData()
    {
        _ingameUI.UpdatePlayerInfo(_saveData.userData);
    }

    #endregion

    #region Function

    public int PathFinding(int startNodeIndex, int endNodeIndex)
    {
        return _mapGenerator.PathFinding(ref _saveData.mapData, startNodeIndex, endNodeIndex);
    }

    public void CheckWalkableNode(int x, int y, Action<int, bool> onRasultCallback)
    {
        int index = (x + 1) + (_saveData.mapData.mapSize * y);

        if(index < 0 || index > _saveData.mapData.nodeDatas.Count)
        {
            onRasultCallback?.Invoke(index, false);

            return;
        }

        var node = _saveData.mapData.nodeDatas[index];

        if(node.isGuide == false)
        {
            onRasultCallback?.Invoke(index, false);

            return;
        }

        if (node.isUser == false)
        {
            onRasultCallback?.Invoke(index, false);

            return;
        }

        if (node.isWalkable == false)
        {
            onRasultCallback?.Invoke(index, false);

            return;
        }

        if (node.isMonster == false)
        {
            onRasultCallback?.Invoke(index, false);

            return;
        }

        if (node.isBonfire == false)
        {
            onRasultCallback?.Invoke(index, false);

            return;
        }

        if (node.isShop == false)
        {
            onRasultCallback?.Invoke(index, false);

            return;
        }

        onRasultCallback?.Invoke(index, true);
    }

    public bool CheckWalkableNode(int index)
    {
        if(index < 0 || index >= _saveData.mapData.nodeDatas.Count)
        {
            return false;
        }

        var node = _saveData.mapData.nodeDatas[index];

        if(node.isGuide == true)
        {
            return false;
        }

        if(node.isUser == true)
        {
            return false;
        }

        if(node.isWalkable == false)
        {
            return false;
        }

        if(node.isMonster == true)
        {
            return false;
        }

        if(node.isBonfire == true)
        {
            return false;
        }

        if(node.isShop == true)
        {
            return false;
        }

        if(node.isExit == true)
        {
            return false;
        }

        return true;
    }

    public List<int> GetDirectionRangeNodes(int currentNodeIndex, eDir dir, int range)
    {
        List<int> dx = new List<int>();
        List<int> dy = new List<int>();

        switch (dir)
        {
            case eDir.Up:
                {
                    dx.Add(0);

                    for (int i = 1; i <= range; i++)
                    {
                        dy.Add(i);
                    }
                }
                break;

            case eDir.Left:
                {
                    dy.Add(0);

                    for (int i = -1; i >= -range; i--)
                    {
                        dx.Add(i);
                    }
                }
                break;

            case eDir.Right:
                {
                    dy.Add(0);

                    for (int i = 1; i <= range; i++)
                    {
                        dx.Add(i);
                    }
                }
                break;

            case eDir.Down: 
                {
                    dx.Add(0);

                    for (int i = -1; i >= -range; i--)
                    {
                        dy.Add(i);
                    }
                }
                break;
        }

        return GetRangeNodes_NonDiagonal(dx, dy, currentNodeIndex);
    }

    public int GetNearbyNodes(int x, int y, int index)
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
    public List<int> GetNearbyNodes_NonDiagonal(int index)
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

                    if (0 <= resultIndex && resultIndex < (_saveData.mapData.mapSize * _saveData.mapData.mapSize))
                    {
                        result.Add(resultIndex);
                    }
                }
            }
        }

        return result;
    }

    public List<int> GetNearbyNodes_NonDiagonal(List<int> dx, List<int> dy, int index)
    {
        List<int> result = new List<int>();

        for(int y = 0; y < dy.Count; y++)
        {
            for(int x = 0; x < dx.Count; x++)
            {
                if(Mathf.Abs(dx[x]) + Mathf.Abs(dy[y]) <= Mathf.Abs(dx[dx.Count - 1]))
                {
                    int nearbyX = (index % _saveData.mapData.mapSize) + dx[x];

                    if(nearbyX < 0 || _saveData.mapData.mapSize <= nearbyX)
                    {
                        continue;
                    }

                    int nearbyY = (index / _saveData.mapData.mapSize) + dy[y];

                    if(nearbyY < 0 || _saveData.mapData.mapSize <= nearbyY)
                    {
                        continue;
                    }

                    int resultIndex = nearbyX + (nearbyY * _saveData.mapData.mapSize);

                    if(index == resultIndex)
                    {
                        continue;
                    }

                    if(0 <= resultIndex && resultIndex < (_saveData.mapData.mapSize * _saveData.mapData.mapSize))
                    {
                        result.Add(resultIndex);
                    }
                }
            }
        }

        return result;
    }

    public List<int> GetRangeNodes_NonDiagonal(List<int> dx, List<int> dy, int index)
    {
        List<int> result = new List<int>();

        for (int y = 0; y < dy.Count; y++)
        {
            for (int x = 0; x < dx.Count; x++)
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

        return result;
    }

    public List<int> GetRangeNodes_Diagonal(List<int> dx, List<int> dy, int index)
    {
        List<int> result = new List<int>();

        for (int y = 0; y < dy.Count; y++)
        {
            for (int x = 0; x < dx.Count; x++)
            {
                if (Mathf.Abs(dx[x]) + Mathf.Abs(dy[y]) <= Mathf.Abs(dx[dx.Count - 1]) ||
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

    public eDir GetTargetDirection(int currentNodeIndex, int targetNodeIndex)
    {
        int value = targetNodeIndex - currentNodeIndex;

        if((value % _saveData.mapData.mapSize) == 0)
        {
            if(value > 0)
            {
                return eDir.Up;
            }
            else if(value < 0)
            {
                return eDir.Down;
            }
        }

        if(value > 0)
        {
            return eDir.Right;
        }
        else if(value < 0)
        {
            return eDir.Left;
        }

        return eDir.Non;
    }

    public List<int> Vision(int vision, int currentIndex)
    {
        List<int> dx = new List<int>();
        List<int> dy = new List<int>();

        for (int i = -vision; i <= vision; i++)
        {
            dx.Add(i);
            dy.Add(i);
        }

        return GetRangeNodes_Diagonal(dx, dy, currentIndex);
    }

    #endregion
}

#region Generator

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

    private System.Action<DataManager.MapData> _onResultCallback = null;
    private DataManager.SaveData _saveData = null;
    private CreatureGenerator _creatureGenerator = null;

    private List<Node> _nodes = null;

    private int _mapSize = 0;

    public MapGenerator(System.Action<DataManager.MapData> onResultCallback, DataManager.SaveData saveData)
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

        if (_saveData.round % 5 != 0)
        {
            GenerateBlocker();
        }
        else
        {
            _nodes[14].isEnter = true;
        }

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

        _nodes[openMiddlePoints[UnityEngine.Random.Range(0, openMiddlePoints.Count)]].isEnter = true;
    }

    private void SelectExitBlock()
    {
        int centerIndex = (int)((Mathf.Pow(_mapSize, 2) - 1) * 0.5f);

        _nodes[centerIndex].isExit = true;
    }

    private void Done()
    {
        _saveData.mapData = new DataManager.MapData();
        _saveData.mapData.mapSize = _mapSize;
        _saveData.mapData.nodeDatas = new List<DataManager.NodeData>();

        for (int n = 0; n < _nodes.Count; n++)
        {
            DataManager.NodeData node = new DataManager.NodeData();
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
                node.isExit = true;
            }

            _saveData.mapData.nodeDatas.Add(node);
        }

        _creatureGenerator = new CreatureGenerator(GenerateCreature, _saveData);
    }

    private void GenerateCreature(CreatureData playerData, List<CreatureData> monsterData)
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

    public int PathFinding(ref DataManager.MapData mapData, int startNodeIndex, int endNodeIndex)
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
    private System.Action<CreatureData, List<CreatureData>> _onResultCallback = null;
    private DataManager.SaveData _saveData = null;

    public CreatureGenerator(System.Action<CreatureData, List<CreatureData>> onResultCallback, DataManager.SaveData saveData)
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
        _saveData.mapData.monsterDatas = new List<CreatureData>();

        GeneratePlayer();

         if (_saveData.round % 5 == 0)
        {
            GenerateShop();
            GeneratorBonfire();
        }
        else
        {
            if (_saveData.round == 1)
            {
                GenerateGuide();
            }

            GenerateMonster();
        }

        Done();
    }

    private void GenerateGuide()
    {
        _saveData.mapData.npcDatas = new List<DataManager.Npc_Data>();

        int currentIndex = _saveData.mapData.enterNodeIndex + 1;

        DataManager.Npc_Data npc = GameManager.instance.dataManager.GetNpcData(203);
        npc.id = 203;
        npc.currentNodeIndex = currentIndex;
        npc.isGuide = true;

        _saveData.mapData.nodeDatas[currentIndex].isGuide = true;
        _saveData.mapData.npcDatas.Add(npc);
    }

    private void GeneratePlayer()
    {
        _saveData.userData.data.currentNodeIndex = _saveData.mapData.enterNodeIndex;
        _saveData.mapData.nodeDatas[_saveData.userData.data.currentNodeIndex].isUser = true;
    }

    private void GenerateShop()
    {
        _saveData.mapData.npcDatas = new List<DataManager.Npc_Data>();

        int currentIndex = (int)(_saveData.mapData.mapSize * 6) +  2;

        DataManager.Npc_Data npc = GameManager.instance.dataManager.GetNpcData(201);
        npc.id = 201;
        npc.currentNodeIndex = currentIndex;
        npc.itemIndexs = new List<short>(3);
        npc.isShop = true;

        for (int i = 0; i < 3; i++)
        {
            int ran = Random.Range(501, 500 + GameManager.instance.dataManager.GetItemDataCount());

            npc.itemIndexs.Add((short)ran);
        }

        _saveData.mapData.nodeDatas[currentIndex].isShop = true;
        _saveData.mapData.npcDatas.Add(npc);
    }

    private void GeneratorBonfire()
    {
        int currentIndex = (int)(_saveData.mapData.mapSize * 6) + 6;

        DataManager.Npc_Data npc = GameManager.instance.dataManager.GetNpcData(202);
        npc.id = 202;
        npc.currentNodeIndex = currentIndex;
        npc.SkillIndexs = new List<short>(3);
        npc.isBonfire = true;

        for (int i = 0; i < 3; i++)
        {
            int ran = Random.Range(301, 300 + GameManager.instance.dataManager.GetSkillDataCount());

            npc.SkillIndexs.Add((short)ran);
        }

        _saveData.mapData.nodeDatas[currentIndex].isBonfire = true;
        _saveData.mapData.npcDatas.Add(npc);
    }

    private void GenerateMonster()
    {
        _saveData.mapData.monsterDatas = new List<CreatureData>();

        int openNodeCount = 0;

        for (int i = 0; i < _saveData.mapData.nodeDatas.Count; i++)
        {
            if(_saveData.mapData.nodeDatas[i].isWalkable == false)
            {
                continue;
            }

            openNodeCount++;
        }

        int monsterSpawnCount = 0;
        if (_saveData.round < 3)
        {
            monsterSpawnCount = _saveData.round;
        }
        else
        {
            monsterSpawnCount = Random.Range(3, openNodeCount / _saveData.mapData.mapSize);
        }

        for (int i = 0; i < monsterSpawnCount; i++)
        {
            DataManager.NodeData node = new DataManager.NodeData();
            SpawnMonsterNodeSelect(Random.Range(0, _saveData.mapData.nodeDatas.Count), ref node);

            int id = Random.Range(0, GameManager.instance.dataManager.GetCreaturDataCount());
            CreatureData creature = GameManager.instance.dataManager.GetCreatureData(id);

            if (creature != null)
            {
                creature.id = (short)i;
                creature.currentNodeIndex = node.index;
                _saveData.mapData.nodeDatas[node.index].isMonster = true;

                MonsterStats(ref creature);

                _saveData.mapData.monsterDatas.Add(creature);
            }
        }
    }

    private void MonsterStats(ref CreatureData creature)
    {
        creature.stats.Reset();

        float increasePoint = ((_saveData.round - 1) * 0.1f);

        creature.stats.coin.current += (short)(creature.stats.coin.current * increasePoint);
        creature.stats.hp.current += (short)(creature.stats.hp.current * increasePoint);
        creature.stats.exp.current += (short)(creature.stats.exp.current * increasePoint);
        creature.stats.attack.current += (short)(creature.stats.attack.current * increasePoint);
        creature.stats.defence.current += (short)(creature.stats.defence.current * (increasePoint * 0.4f));
    }

    private void Done()
    {
        _onResultCallback?.Invoke(_saveData.userData.data, _saveData.mapData.monsterDatas);
    }

    private void SpawnMonsterNodeSelect(int index, ref DataManager.NodeData node)
    {
        if (_saveData.mapData.nodeDatas[index].isWalkable == true 
            && _saveData.mapData.nodeDatas[index].isMonster == false 
            && _saveData.mapData.nodeDatas[index].isUser == false
            && _saveData.mapData.nodeDatas[index].isExit == false
            && _saveData.mapData.nodeDatas[index].isGuide == false)
        {
            node = _saveData.mapData.nodeDatas[index];

            return;
        }

        int newIndex = index;

        if (_saveData.mapData.nodeDatas[index].isWalkable == false)
        {
            newIndex += 1;
        }

        if (newIndex >= _saveData.mapData.nodeDatas.Count)
        {
            newIndex -= _saveData.mapData.nodeDatas.Count;
        }

        if (_saveData.mapData.nodeDatas[newIndex].isMonster == true)
        {
            newIndex += 5;
        }

        if (newIndex >= _saveData.mapData.nodeDatas.Count)
        {
            newIndex -= _saveData.mapData.nodeDatas.Count;
        }

        if (_saveData.mapData.nodeDatas[newIndex].isUser == true)
        {
            newIndex += 5;
        }

        if (newIndex >= _saveData.mapData.nodeDatas.Count)
        {
            newIndex -= _saveData.mapData.nodeDatas.Count;
        }

        if (_saveData.mapData.nodeDatas[newIndex].isGuide == true)
        {
            newIndex += 5;
        }

        if (newIndex >= _saveData.mapData.nodeDatas.Count)
        {
            newIndex -= _saveData.mapData.nodeDatas.Count;
        }

        if (newIndex == _saveData.mapData.enterNodeIndex)
        {
            newIndex += 4;
        }

        if (newIndex >= _saveData.mapData.nodeDatas.Count)
        {
            newIndex -= _saveData.mapData.nodeDatas.Count;
        }

        if (newIndex == _saveData.mapData.exitNodeIndex)
        {
            newIndex += 4;
        }

        if (newIndex >= _saveData.mapData.nodeDatas.Count)
        {
            newIndex -= _saveData.mapData.nodeDatas.Count;
        }

        SpawnMonsterNodeSelect(newIndex, ref node);
    }
}

#endregion