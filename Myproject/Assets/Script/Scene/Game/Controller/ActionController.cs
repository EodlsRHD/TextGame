using System.Threading;
using UnityEngine;

public class ActionController : MonoBehaviour
{
    public void Initialize()
    {
        this.gameObject.SetActive(true);
    }

    public void Npc(int nodeIndex)
    {
        DataManager.Npc_Data npc = IngameManager.instance.saveData.mapData.npcDatas.Find(x => x.currentNodeIndex == nodeIndex);

        if (npc.isBonfire == true)
        {
            if (npc.isUseBonfire == true)
            {
                IngameManager.instance.UpdateText("--- 모닥불의 불꽃이 사그라들었습니다.");
            }
            else
            {
                IngameManager.instance.BonfireOpen(npc);
            }
        }
        else if (npc.isShop == true)
        {
            if (npc.itemIndexs.Count == 0)
            {
                IngameManager.instance.UpdateText("@상인 : 물건이 다 떨어졌어요 다음에 들러주세요!");

                return;
            }
            else
            {
                IngameManager.instance.UpdateText("@상인 : 안녕하세요! 좋은 물건 구경해보세요!");

                IngameManager.instance.ShopOpen(npc);
            }
        }
    }

    public void Buy(int currentIndex, int index, short price)
    {
        if (index < 0)
        {
            return;
        }

        IngameManager.instance.saveData.mapData.npcDatas.Find(x => x.currentNodeIndex == currentIndex).itemIndexs.Remove(IngameManager.instance.saveData.mapData.npcDatas[0].itemIndexs.Find(x => x == index));

        GameManager.instance.soundManager.PlaySfx(eSfx.Coin);

        IngameManager.instance.UpdateText("@상인 : 구매해주셔서 고마워요");
        IngameManager.instance.UpdateText("가방에 보관되었습니다.");

        IngameManager.instance.saveData.userData.data.coin -= price;
        IngameManager.instance.saveData.userData.itemDataIndexs.Add((short)index);

        GameManager.instance.dataManager.AddEncyclopedia_Item(index);
    }

    public void SelectSkill(int currentIndex, int getIndex, int removeIndex)
    {
        IngameManager.instance.saveData.mapData.npcDatas.Find(x => x.currentNodeIndex == currentIndex).isUseBonfire = true;
        IngameManager.instance.saveData.mapData.nodeDatas[currentIndex].isUseBonfire = true;
        IngameManager.instance.UpdateData();

        IngameManager.instance.UpdateText("모닥불의 불꽃이 사그라들고 있다.");

        if (getIndex == 0 && removeIndex == 0)
        {
            IngameManager.instance.UpdateText("체력과 마나가 회복되었습니다.");

            short hp = (short)(IngameManager.instance.saveData.userData.maximumHP * 0.35f);
            short mp = (short)(IngameManager.instance.saveData.userData.maximumMP * 0.35f);

            if (IngameManager.instance.saveData.userData.currentHP + hp > IngameManager.instance.saveData.userData.maximumHP)
            {
                IngameManager.instance.saveData.userData.currentHP = IngameManager.instance.saveData.userData.maximumHP;
            }
            else
            {
                IngameManager.instance.saveData.userData.currentHP += hp;
            }

            if (IngameManager.instance.saveData.userData.currentHP + hp > IngameManager.instance.saveData.userData.maximumMP)
            {
                IngameManager.instance.saveData.userData.currentHP = IngameManager.instance.saveData.userData.maximumMP;
            }
            else
            {
                IngameManager.instance.saveData.userData.currentHP += mp;
            }

            IngameManager.instance.UpdateData();

            return;
        }

        if (removeIndex != 0)
        {
            for (int i = 0; i < IngameManager.instance.saveData.userData.skillDataIndexs.Count; i++)
            {
                if (IngameManager.instance.saveData.userData.skillDataIndexs[i] != removeIndex)
                {
                    continue;
                }

                IngameManager.instance.saveData.userData.skillDataIndexs[i] = (short)getIndex;

                break;
            }
        }
    }

    private int GetPlayerDamage(int battleDamage, DataManager.User_Data data)
    {
        return data.currentATTACK + battleDamage + (int)(data.currentATTACK * (0.1f * data.Attack_Effect_Per)) + data.Attack_Effect;
    }

    private int GetPlayerDefence(DataManager.User_Data data)
    {
        return data.currentDEFENCE + (int)(data.currentDEFENCE * (0.1f * data.Defence_Effect_Per)) + data.Defence_Effect; ;
    }

    private int GetMonsterDamage(int battleDamage, DataManager.Creature_Data data)
    {
        return (int)(battleDamage + ((battleDamage * 0.1f) * data.attack));
    }

    private int GetMonsterDefence(DataManager.Creature_Data data)
    {
        return data.defence;
    }
    
    private void HitSfx(int damage, int value)
    {
        if (value < (short)Mathf.Abs(damage))
        {
            GameManager.instance.soundManager.PlaySfx(eSfx.Hit_hard);
        }
        else
        {
            GameManager.instance.soundManager.PlaySfx(eSfx.Hit_light);
        }
    }

    public void Attack(int nodeMonsterIndex, System.Action onLastCallback = null)
    {
        DataManager.Creature_Data monster = IngameManager.instance.saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == nodeMonsterIndex);
        GameManager.instance.dataManager.AddEncyclopedia_Creature(monster.id);

        IngameManager.instance.isHuntMonster = true;

        IngameManager.instance.CallAttacker(monster, onLastCallback, (result, damage) =>
        {
            IngameManager.instance.PlayBgm(eBgm.Ingame);

            if (result == eWinorLose.Lose)
            {
                IngameManager.instance.UpdateText("--- " + monster.name + " (이)가 승리했습니다.");

                int monsterAttack = GetMonsterDamage(damage, monster);
                int playerDef = GetPlayerDefence(IngameManager.instance.saveData.userData);
                int resultDamage = (playerDef - monsterAttack);

                if (resultDamage >= 0)
                {
                    GameManager.instance.soundManager.PlaySfx(eSfx.Blocked);
                    IngameManager.instance.UpdateText("--- " + monster.name + " 의 공격이 방어도에 막혔습니다.");

                    return;
                }

                IngameManager.instance.saveData.userData.currentHP -= (short)Mathf.Abs(resultDamage);

                HitSfx(resultDamage, IngameManager.instance.saveData.userData.currentHP / 3);

                IngameManager.instance.UpdateText("--- " + Mathf.Abs(resultDamage) + " 의 피해를 입었습니다.");

                if (IngameManager.instance.saveData.userData.currentHP <= 0)
                {
                    IngameManager.instance.saveData.userData.currentHP = 0;

                    GameManager.instance.soundManager.PlaySfx(eSfx.RoundFail);
                    IngameManager.instance.UpdateData(Mathf.Abs(resultDamage) + " 의 피해를 입어 패배하였습니다.");

                    return;
                }

                IngameManager.instance.UpdateData();

                return;
            }

            if (result == eWinorLose.Draw)
            {
                IngameManager.instance.UpdateText("--- 무승부입니다.");

                IngameManager.instance.UpdateData();

                return;
            }

            IngameManager.instance.UpdateText("--- " + IngameManager.instance.saveData.userData.data.name + " (이)가 승리했습니다.");

            int playerDamage = GetPlayerDamage(damage, IngameManager.instance.saveData.userData);
            int _damage = GetMonsterDefence(monster) - playerDamage;

            if (_damage >= 0)
            {
                IngameManager.instance.UpdateText("--- " + IngameManager.instance.saveData.userData.data.name + " 의 공격이 방어도에 막혔습니다.");
                GameManager.instance.soundManager.PlaySfx(eSfx.Blocked);

                return;
            }

            GameManager.instance.soundManager.PlaySfx(eSfx.Attack);

            IngameManager.instance.UpdateText("--- " + IngameManager.instance.saveData.userData.data.name + " (이)가 " + playerDamage + " 의 공격력으로 공격합니다.");
            IngameManager.instance.UpdateText("방어도를 제외한 " + Mathf.Abs(_damage) + " 의 데미지를 가했습니다.");

            monster.hp -= (short)Mathf.Abs(_damage);

            if (monster.hp == 0)
            {
                IngameManager.instance.saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == nodeMonsterIndex).hp = 0;

                IngameManager.instance.UpdateText(monster.name + " (을)를 처치하였습니다");
                IngameManager.instance.UpdateText("--- 경험치 " + monster.exp + " , 코인 " + monster.coin + "을 획득했습니다");

                if (monster.itemIndexs != null)
                {
                    for (int i = 0; i < monster.itemIndexs.Count; i++)
                    {
                        IngameManager.instance.GetMonsterItem(monster.itemIndexs[i]);
                    }
                }

                IngameManager.instance.GetGold(monster.coin);
                IngameManager.instance.GetExp(monster.exp);

                IngameManager.instance.MonsterDead(monster);
            }
            else
            {
                IngameManager.instance.UpdateText(monster.name + "의 체력이 " + monster.hp + " 만큼 남았습니다.");

                IngameManager.instance.saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == nodeMonsterIndex).hp = monster.hp;
            }

            if (IngameManager.instance.saveData.mapData.monsterDatas.Count == 0)
            {
                GameManager.instance.soundManager.PlaySfx(eSfx.ExitOpen);

                IngameManager.instance.isAllMonsterDead = true;
            }

            IngameManager.instance.UpdateData();
        });
    }

    public void Defence()
    {
        if (IngameManager.instance.saveData.userData.currentAP == 0)
        {
            IngameManager.instance.UpdateText("--- 남아있는 행동력이 없습니다.");

            return;
        }

        short ap = IngameManager.instance.saveData.userData.currentAP;
        IngameManager.instance.saveData.userData.currentDEFENCE += ap;
        IngameManager.instance.saveData.userData.currentAP = 0;

        IngameManager.instance.UpdateText("행동력을 모드 소진하였습니다.");
        IngameManager.instance.UpdateText("방어도가 " + ap + "만큼 증가했습니다.");

        IngameManager.instance.UpdatePlayerInfo(eStats.AP);
        IngameManager.instance.UpdatePlayerInfo(eStats.Defence);
    }

    public void Skill(int id)
    {
        if (id == -1)
        {
            IngameManager.instance.UpdateText("선택된 스킬이 없습니다.");
            return;
        }

        int reCooldown = 0;
        if (SkillCheckCoolDown(id, ref reCooldown) == false)
        {
            IngameManager.instance  .UpdateText("대기순서가 " + reCooldown + "만큼 남았습니다.");

            return;
        }

        DataManager.Skill_Data data = GameManager.instance.dataManager.GetskillData(id);

        DataManager.Skill_CoolDown cooldown = new DataManager.Skill_CoolDown();
        cooldown.id = data.id;
        cooldown.name = data.name;
        cooldown.coolDown = data.coolDown + 1;
        IngameManager.instance.saveData.userData.coolDownSkill.Add(cooldown);

        int useMP = data.usemp;
        int reMP = IngameManager.instance.saveData.userData.currentMP - useMP;
        IngameManager.instance.saveData.userData.currentMP = (short)(reMP < 0 ? 0 : reMP);

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

        IngameManager.instance.ControlPadUpdateData();
        IngameManager.instance.UpdateData();

        IngameManager.instance.UpdateText("스킬 " + data.name + " (을)를 발동하셨습니다.");
    }

    private bool SkillCheckCoolDown(int id, ref int cooldown)
    {
        for (int i = 0; i < IngameManager.instance.saveData.userData.coolDownSkill.Count; i++)
        {
            if (IngameManager.instance.saveData.userData.coolDownSkill[i].id == id)
            {
                cooldown = IngameManager.instance.saveData.userData.coolDownSkill[i].coolDown;
                return false;
            }
        }

        return true;
    }

    private void SkillConsumptionCheck(DataManager.Skill_Data data, eStats type, int useValue, bool isPercent = false)
    {
        eEffect_IncreaseDecrease result = eEffect_IncreaseDecrease.Non;

        if (useValue == 0)
        {
            return;
        }

        if (useValue < 0)
        {
            result = eEffect_IncreaseDecrease.Decrease;
        }

        if (useValue == -9999)
        {
            result = eEffect_IncreaseDecrease.ALLDecrease;
        }

        if (useValue > 0)
        {
            result = eEffect_IncreaseDecrease.Increase;
        }

        DataManager.Duration temp = new DataManager.Duration();
        temp.ID = data.id;
        temp.name = data.name;
        temp.stats = type;
        temp.inDe = result;
        temp.isPercent = isPercent;
        temp.value = useValue;
        temp.remaindCooldown = data.coolDown + 1;
        temp.remaindDuration = data.duration + 1;

        if (data.duration == 0)
        {
            ApplyEffect(temp);

            return;
        }

        IngameManager.instance.saveData.userData.useSkill.Add(temp);
    }

    public void SkillRemindDuration()
    {
        for (int i = IngameManager.instance.saveData.userData.useSkill.Count - 1; i >= 0; i--)
        {
            if (IngameManager.instance.saveData.userData.useSkill[i].remaindDuration == 0)
            {
                RemoveEffect(IngameManager.instance.saveData.userData.useSkill[i]);

                IngameManager.instance.saveData.userData.useSkill.Remove(IngameManager.instance.saveData.userData.useSkill[i]);

                continue;
            }

            ApplyEffect(IngameManager.instance.saveData.userData.useSkill[i]);
            IngameManager.instance.saveData.userData.useSkill[i].remaindDuration -= 1;
        }

        for (int i = IngameManager.instance.saveData.userData.coolDownSkill.Count - 1; i >= 0; i--)
        {
            if (IngameManager.instance.saveData.userData.coolDownSkill[i].coolDown == 0)
            {
                IngameManager.instance.UpdateText(IngameManager.instance.saveData.userData.coolDownSkill[i].name + " (을)를 다시 사용 할 수 있습니다.");

                IngameManager.instance.saveData.userData.coolDownSkill.Remove(IngameManager.instance.saveData.userData.coolDownSkill[i]);

                continue;
            }

            IngameManager.instance.saveData.userData.coolDownSkill[i].coolDown -= 1;
        }

        IngameManager.instance.UpdateData();
    }

    public void Bag(int id)
    {
        if (id == -1)
        {
            IngameManager.instance.UpdateText("선택된 아이템이 없습니다.");

            return;
        }

        for (int i = IngameManager.instance.saveData.userData.itemDataIndexs.Count - 1; i >= 0; i--)
        {
            if (IngameManager.instance.saveData.userData.itemDataIndexs[i] == id)
            {
                IngameManager.instance.saveData.userData.itemDataIndexs.Remove(IngameManager.instance.saveData.userData.itemDataIndexs[i]);

                break;
            }
        }

        DataManager.Item_Data data = GameManager.instance.dataManager.GetItemData(id);

        ItemConsumptionCheck(data, eStats.HP, data.hp);
        ItemConsumptionCheck(data, eStats.MP, data.mp);
        ItemConsumptionCheck(data, eStats.AP, data.ap);

        ItemConsumptionCheck(data, eStats.EXP, data.exp);
        ItemConsumptionCheck(data, eStats.EXP, data.expPercentIncreased, true);

        ItemConsumptionCheck(data, eStats.Coin, data.coin);
        ItemConsumptionCheck(data, eStats.Coin, data.coinPercentIncreased, true);

        ItemConsumptionCheck(data, eStats.Attack, data.attack);
        ItemConsumptionCheck(data, eStats.Attack, data.attackPercentIncreased, true);

        ItemConsumptionCheck(data, eStats.Defence, data.defence);
        ItemConsumptionCheck(data, eStats.Defence, data.defencePercentIncreased, true);

        IngameManager.instance.ControlPadUpdateData();
        IngameManager.instance.UpdateData();
    }

    private void ItemConsumptionCheck(DataManager.Item_Data data, eStats type, int useValue, bool isPercent = false)
    {
        eEffect_IncreaseDecrease result = eEffect_IncreaseDecrease.Non;

        if (useValue == 0)
        {
            return;
        }

        if (useValue < 0)
        {
            result = eEffect_IncreaseDecrease.Decrease;
        }

        if (useValue == -9999)
        {
            result = eEffect_IncreaseDecrease.ALLDecrease;
        }

        if (useValue > 0)
        {
            result = eEffect_IncreaseDecrease.Increase;
        }

        DataManager.Duration temp = new DataManager.Duration();
        temp.ID = data.id;
        temp.name = data.name;
        temp.stats = type;
        temp.inDe = result;
        temp.isPercent = isPercent;
        temp.value = useValue;
        temp.remaindDuration = data.duration + 1;

        if (data.duration == 0)
        {
            ApplyEffect(temp);

            return;
        }

        IngameManager.instance.saveData.userData.useItem.Add(temp);
    }

    public void ItemRemindDuration()
    {
        for (int i = IngameManager.instance.saveData.userData.useItem.Count - 1; i >= 0; i--)
        {
            if (IngameManager.instance.saveData.userData.useItem[i].remaindDuration == 0)
            {
                RemoveEffect(IngameManager.instance.saveData.userData.useItem[i]);

                IngameManager.instance.saveData.userData.useItem.Remove(IngameManager.instance.saveData.userData.useItem[i]);

                continue;
            }

            ApplyEffect(IngameManager.instance.saveData.userData.useItem[i]);
            IngameManager.instance.saveData.userData.useItem[i].remaindDuration -= 1;
        }

        IngameManager.instance.UpdateData();
    }

    private void RemoveEffect(DataManager.Duration use)
    {
        IngameManager.instance.UpdateText(use.name + " 의 효과가 끝났습니다.");

        switch (use.stats)
        {
            case eStats.HP:
                {
                    short value = (short)(use.inDe == eEffect_IncreaseDecrease.Increase ? use.value : (use.value * -1));

                    if (IngameManager.instance.saveData.userData.currentHP - value < 0)
                    {
                        IngameManager.instance.saveData.userData.currentHP = 0;
                    }
                    else
                    {
                        IngameManager.instance.saveData.userData.currentHP -= value;
                    }
                }
                break;

            case eStats.MP:
                {
                    short value = (short)(use.inDe == eEffect_IncreaseDecrease.Increase ? use.value : (use.value * -1));

                    if (IngameManager.instance.saveData.userData.currentMP - value < 0)
                    {
                        IngameManager.instance.saveData.userData.currentMP = 0;
                    }
                    else
                    {
                        IngameManager.instance.saveData.userData.currentMP -= value;
                    }
                }
                break;

            case eStats.AP:
                {
                    short value = (short)(use.inDe == eEffect_IncreaseDecrease.Increase ? use.value : (use.value * -1));

                    if (IngameManager.instance.saveData.userData.currentAP - value < 0)
                    {
                        IngameManager.instance.saveData.userData.currentAP = 0;

                        return;
                    }

                    IngameManager.instance.saveData.userData.currentAP -= value;
                }
                break;

            case eStats.EXP:
                {
                    short value = (short)(use.inDe == eEffect_IncreaseDecrease.Increase ? use.value : (use.value * -1));

                    if (use.isPercent == true)
                    {
                        if (IngameManager.instance.saveData.userData.EXP_Effect_Per - value < 0)
                        {
                            IngameManager.instance.saveData.userData.EXP_Effect_Per = 0;

                            return;
                        }

                        IngameManager.instance.saveData.userData.EXP_Effect_Per -= value;

                        return;
                    }

                    if (IngameManager.instance.saveData.userData.currentEXP - value < 0)
                    {
                        IngameManager.instance.saveData.userData.currentEXP = 0;

                        return;
                    }

                    IngameManager.instance.saveData.userData.currentEXP -= value;
                }
                break;

            case eStats.Coin:
                {
                    short value = (short)(use.inDe == eEffect_IncreaseDecrease.Increase ? use.value : (use.value * -1));

                    if (use.isPercent == true)
                    {
                        if (IngameManager.instance.saveData.userData.Coin_Effect_Per - value < 0)
                        {
                            IngameManager.instance.saveData.userData.Coin_Effect_Per = 0;

                            return;
                        }

                        IngameManager.instance.saveData.userData.Coin_Effect_Per -= value;

                        return;
                    }

                    if (IngameManager.instance.saveData.userData.data.coin - value < 0)
                    {
                        IngameManager.instance.saveData.userData.data.coin = 0;

                        return;
                    }

                    IngameManager.instance.saveData.userData.data.coin -= value;
                }
                break;

            case eStats.Attack:
                {
                    short value = (short)(use.inDe == eEffect_IncreaseDecrease.Increase ? use.value : (use.value * -1));

                    if (use.isPercent == true)
                    {
                        if (IngameManager.instance.saveData.userData.Attack_Effect_Per - value < 0)
                        {
                            IngameManager.instance.saveData.userData.Attack_Effect_Per = 0;

                            return;
                        }

                        IngameManager.instance.saveData.userData.Attack_Effect_Per -= value;

                        return;
                    }

                    if (IngameManager.instance.saveData.userData.Attack_Effect - value < 0)
                    {
                        IngameManager.instance.saveData.userData.Attack_Effect = 0;

                        return;
                    }

                    IngameManager.instance.saveData.userData.Attack_Effect -= value;
                }
                break;

            case eStats.Defence:
                {
                    short value = (short)(use.inDe == eEffect_IncreaseDecrease.Increase ? use.value : (use.value * -1));

                    if (use.isPercent == true)
                    {
                        if (IngameManager.instance.saveData.userData.Defence_Effect_Per - value < 0)
                        {
                            IngameManager.instance.saveData.userData.Defence_Effect_Per = 0;

                            return;
                        }

                        IngameManager.instance.saveData.userData.Defence_Effect_Per -= value;

                        return;
                    }

                    if (IngameManager.instance.saveData.userData.Defence_Effect - value < 0)
                    {
                        IngameManager.instance.saveData.userData.Defence_Effect = 0;

                        return;
                    }

                    IngameManager.instance.saveData.userData.Defence_Effect -= value;
                }
                break;
        }
    }

    private void ApplyEffect(DataManager.Duration use)
    {
        IngameManager.instance.UpdateText(use.name + " 의 효과가 적용되었습니다.");

        switch (use.stats)
        {
            case eStats.HP:
                {
                    short value = (short)(use.inDe == eEffect_IncreaseDecrease.Increase ? use.value : (use.value * -1));

                    if (IngameManager.instance.saveData.userData.currentHP + value > IngameManager.instance.saveData.userData.maximumHP)
                    {
                        IngameManager.instance.saveData.userData.currentHP = IngameManager.instance.saveData.userData.maximumHP;
                    }
                    else
                    {
                        IngameManager.instance.saveData.userData.currentHP += value;
                    }
                }
                break;

            case eStats.MP:
                {
                    short value = (short)(use.inDe == eEffect_IncreaseDecrease.Increase ? use.value : (use.value * -1));

                    if (IngameManager.instance.saveData.userData.currentMP + value > IngameManager.instance.saveData.userData.maximumMP)
                    {
                        IngameManager.instance.saveData.userData.currentMP = IngameManager.instance.saveData.userData.maximumMP;
                    }
                    else
                    {
                        IngameManager.instance.saveData.userData.currentMP += value;
                    }
                }
                break;

            case eStats.AP:
                {
                    short value = (short)(use.inDe == eEffect_IncreaseDecrease.Increase ? use.value : (use.value * -1));

                    if (IngameManager.instance.saveData.userData.currentAP + value > IngameManager.instance.saveData.userData.maximumAP)
                    {
                        IngameManager.instance.saveData.userData.currentAP = IngameManager.instance.saveData.userData.maximumAP;
                    }
                    else
                    {
                        IngameManager.instance.saveData.userData.currentAP += value;
                    }
                }
                break;

            case eStats.EXP:
                {
                    short value = (short)(use.inDe == eEffect_IncreaseDecrease.Increase ? use.value : (use.value * -1));

                    if (use.isPercent == true)
                    {
                        IngameManager.instance.saveData.userData.EXP_Effect_Per += value;

                        return;
                    }

                    IngameManager.instance.GetExp(value);
                }
                break;

            case eStats.Coin:
                {
                    short value = (short)(use.inDe == eEffect_IncreaseDecrease.Increase ? use.value : (use.value * -1));

                    if (use.isPercent == true)
                    {
                        IngameManager.instance.saveData.userData.Coin_Effect_Per += value;

                        return;
                    }

                    IngameManager.instance.saveData.userData.data.coin += value;
                }
                break;

            case eStats.Attack:
                {
                    short value = (short)(use.inDe == eEffect_IncreaseDecrease.Increase ? use.value : (use.value * -1));

                    if (use.isPercent == true)
                    {
                        IngameManager.instance.saveData.userData.Attack_Effect_Per += value;

                        return;
                    }

                    IngameManager.instance.saveData.userData.Attack_Effect += value;
                }
                break;

            case eStats.Defence:
                {
                    short value = (short)(use.inDe == eEffect_IncreaseDecrease.Increase ? use.value : (use.value * -1));

                    if (use.isPercent == true)
                    {
                        IngameManager.instance.saveData.userData.Defence_Effect_Per += value;

                        return;
                    }

                    IngameManager.instance.saveData.userData.Defence_Effect += value;
                }
                break;
        }
    }
}
