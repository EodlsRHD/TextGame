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
                IngameManager.instance.UpdateText("--- ��ں��� �Ҳ��� ��׶������ϴ�.");
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
                IngameManager.instance.UpdateText("@���� : ������ �� ��������� ������ �鷯�ּ���!");

                return;
            }
            else
            {
                IngameManager.instance.UpdateText("@���� : �ȳ��ϼ���! ���� ���� �����غ�����!");

                IngameManager.instance.ShopOpen(npc);
            }
        }
    }

    public void Buy(int currentIndex, int index, ushort price)
    {
        if (index < 0)
        {
            return;
        }

        IngameManager.instance.saveData.mapData.npcDatas.Find(x => x.currentNodeIndex == currentIndex).itemIndexs.Remove(IngameManager.instance.saveData.mapData.npcDatas[0].itemIndexs.Find(x => x == index));

        GameManager.instance.soundManager.PlaySfx(eSfx.Coin);

        IngameManager.instance.UpdateText("@���� : �������ּż� ��������");
        IngameManager.instance.UpdateText("���濡 �����Ǿ����ϴ�.");

        IngameManager.instance.saveData.userData.data.coin -= price;
        IngameManager.instance.saveData.userData.itemDataIndexs.Add((ushort)index);

        GameManager.instance.dataManager.AddEncyclopedia_Item(index);
    }

    public void SelectSkill(int currentIndex, int getIndex, int removeIndex)
    {
        IngameManager.instance.saveData.mapData.npcDatas.Find(x => x.currentNodeIndex == currentIndex).isUseBonfire = true;
        IngameManager.instance.saveData.mapData.nodeDatas[currentIndex].isUseBonfire = true;
        IngameManager.instance.UpdateData();

        IngameManager.instance.UpdateText("��ں��� �Ҳ��� ��׶��� �ִ�.");

        if (getIndex == 0 && removeIndex == 0)
        {
            IngameManager.instance.UpdateText("ü�°� ������ ȸ���Ǿ����ϴ�.");

            ushort hp = (ushort)(IngameManager.instance.saveData.userData.maximumHP * 0.35f);
            ushort mp = (ushort)(IngameManager.instance.saveData.userData.maximumMP * 0.35f);

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

                IngameManager.instance.saveData.userData.skillDataIndexs[i] = (ushort)getIndex;
                break;
            }
        }

        IngameManager.instance.saveData.userData.skillDataIndexs.Add((ushort)getIndex);
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
                IngameManager.instance.UpdateText("--- " + monster.name + " (��)�� �¸��߽��ϴ�.");

                int monsterAttack = (int)(damage + ((damage * 0.1f) * monster.attack));
                int playerDef = IngameManager.instance.saveData.userData.currentDEFENCE + (int)(IngameManager.instance.saveData.userData.currentDEFENCE * (0.1f * IngameManager.instance.saveData.userData.Defence_Effect_Per)) + IngameManager.instance.saveData.userData.Defence_Effect;
                int resultDamage = (playerDef - monsterAttack);

                if (resultDamage >= 0)
                {
                    GameManager.instance.soundManager.PlaySfx(eSfx.Blocked);
                    IngameManager.instance.UpdateText("--- " + monster.name + " �� ������ ���� �������ϴ�.");

                    return;
                }

                IngameManager.instance.saveData.userData.currentHP -= (ushort)Mathf.Abs(resultDamage);

                int soundHP = IngameManager.instance.saveData.userData.currentHP / 3;

                if (soundHP > (ushort)Mathf.Abs(resultDamage))
                {
                    GameManager.instance.soundManager.PlaySfx(eSfx.Hit_hard);
                }
                else
                {
                    GameManager.instance.soundManager.PlaySfx(eSfx.Hit_light);
                }

                IngameManager.instance.UpdateText("--- " + Mathf.Abs(resultDamage) + " �� ���ظ� �Ծ����ϴ�.");

                if (IngameManager.instance.saveData.userData.currentHP > 60000 || IngameManager.instance.saveData.userData.currentHP == 0)
                {
                    IngameManager.instance.saveData.userData.currentHP = 0;

                    GameManager.instance.soundManager.PlaySfx(eSfx.RoundFail);
                    IngameManager.instance.UpdateData(Mathf.Abs(resultDamage) + " �� ���ظ� �Ծ� �й��Ͽ����ϴ�.");

                    return;
                }

                IngameManager.instance.UpdateData();

                return;
            }

            if (result == eWinorLose.Draw)
            {
                IngameManager.instance.UpdateText("--- ���º��Դϴ�.");

                IngameManager.instance.UpdateData();

                return;
            }

            IngameManager.instance.UpdateText("--- " + IngameManager.instance.saveData.userData.data.name + " (��)�� �¸��߽��ϴ�.");

            int playerDamage = IngameManager.instance.saveData.userData.currentATTACK + damage + (int)(IngameManager.instance.saveData.userData.currentATTACK * (0.1f * IngameManager.instance.saveData.userData.Attack_Effect_Per)) + IngameManager.instance.saveData.userData.Attack_Effect;
            int _damage = monster.defence - playerDamage;

            if (_damage >= 0)
            {
                IngameManager.instance.UpdateText("--- " + IngameManager.instance.saveData.userData.data.name + " �� ������ ���� �������ϴ�.");
                GameManager.instance.soundManager.PlaySfx(eSfx.Blocked);

                return;
            }
            else
            {
                GameManager.instance.soundManager.PlaySfx(eSfx.Attack);
                IngameManager.instance.UpdateText("--- " + IngameManager.instance.saveData.userData.data.name + " (��)�� " + playerDamage + " �� ���ݷ����� �����մϴ�.");
                IngameManager.instance.UpdateText("���� ������ " + Mathf.Abs(_damage) + " �� �������� ���߽��ϴ�.");

                monster.hp -= (ushort)Mathf.Abs(_damage);

                if (monster.hp >= 60000 || monster.hp == 0)
                {
                    IngameManager.instance.saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == nodeMonsterIndex).hp = 0;

                    IngameManager.instance.UpdateText(monster.name + " (��)�� óġ�Ͽ����ϴ�");
                    IngameManager.instance.UpdateText("--- ����ġ " + monster.exp + " , ���� " + monster.coin + "�� ȹ���߽��ϴ�");

                    if (monster.itemIndexs != null)
                    {
                        if (monster.itemIndexs.Count > 0)
                        {
                            IngameManager.instance.UpdateText("--- ������ " + monster.itemIndexs.Count + " ���� ȹ���߽��ϴ�.");

                            for (int i = 0; i < monster.itemIndexs.Count; i++)
                            {
                                IngameManager.instance.saveData.userData.itemDataIndexs.Add(monster.itemIndexs[i]);
                                GameManager.instance.dataManager.AddEncyclopedia_Item(monster.itemIndexs[i]);
                            }
                        }
                    }

                    IngameManager.instance.saveData.userData.data.coin += (ushort)(monster.coin + (monster.coin * 0.0f * IngameManager.instance.saveData.userData.Coin_Effect_Per));
                    IngameManager.instance.saveData.userData.currentEXP += (ushort)(monster.exp + (monster.exp * 0.1f * IngameManager.instance.saveData.userData.EXP_Effect_Per));

                    LevelUp();

                    IngameManager.instance.MonsterDead(monster);
                }
                else
                {
                    IngameManager.instance.UpdateText(monster.name + "�� ü���� " + monster.hp + " ��ŭ ���ҽ��ϴ�.");

                    IngameManager.instance.saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == nodeMonsterIndex).hp = monster.hp;
                }

                if (IngameManager.instance.saveData.mapData.monsterDatas.Count == 0)
                {
                    GameManager.instance.soundManager.PlaySfx(eSfx.ExitOpen);
                    IngameManager.instance.isAllMonsterDead = true;
                }

                IngameManager.instance.UpdateData();
            }

            IngameManager.instance.UpdateData();
        });
    }

    public void Defence()
    {
        if (IngameManager.instance.saveData.userData.currentAP == 0)
        {
            IngameManager.instance.UpdateText("--- �����ִ� �ൿ���� �����ϴ�.");

            return;
        }

        ushort ap = IngameManager.instance.saveData.userData.currentAP;
        IngameManager.instance.saveData.userData.currentDEFENCE += ap;
        IngameManager.instance.saveData.userData.currentAP = 0;

        IngameManager.instance.UpdateText("�ൿ���� ��� �����Ͽ����ϴ�.");
        IngameManager.instance.UpdateText("���� " + ap + "��ŭ �����߽��ϴ�.");

        IngameManager.instance.UpdatePlayerInfo(eStats.AP);
        IngameManager.instance.UpdatePlayerInfo(eStats.Defence);
    }

    private void LevelUp()
    {
        if (IngameManager.instance.saveData.userData.maximumEXP <= IngameManager.instance.saveData.userData.currentEXP)
        {
            IngameManager.instance.UpdateText("������ �����߽��ϴ� !");

            IngameManager.instance.saveData.userData.level += 1;
            IngameManager.instance.saveData.userData.currentEXP = (ushort)Mathf.Abs(IngameManager.instance.saveData.userData.maximumEXP - IngameManager.instance.saveData.userData.currentEXP);

            IngameManager.instance.UpdatePlayerInfo(eStats.EXP);
            IngameManager.instance.UpdatePlayerInfo(eStats.Level);

            IngameManager.instance.OpneLevelPoint();
        }
    }

    public void Skill(int id)
    {
        if (id == -1)
        {
            IngameManager.instance.UpdateText("���õ� ��ų�� �����ϴ�.");
            return;
        }

        int reCooldown = 0;
        if (SkillCheckCoolDown(id, ref reCooldown) == false)
        {
            IngameManager.instance  .UpdateText("�������� " + reCooldown + "��ŭ ���ҽ��ϴ�.");

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
        IngameManager.instance.saveData.userData.currentMP = (ushort)(reMP < 0 ? 0 : reMP);

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

        IngameManager.instance.UpdateText("��ų " + data.name + " (��)�� �ߵ��ϼ̽��ϴ�.");
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
                IngameManager.instance.UpdateText(IngameManager.instance.saveData.userData.coolDownSkill[i].name + " (��)�� �ٽ� ��� �� �� �ֽ��ϴ�.");

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
            IngameManager.instance.UpdateText("���õ� �������� �����ϴ�.");

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
        IngameManager.instance.UpdateText(use.name + " �� ȿ���� �������ϴ�.");

        switch (use.stats)
        {
            case eStats.HP:
                {
                    ushort value = (ushort)(use.inDe == eEffect_IncreaseDecrease.Increase ? use.value : (use.value * -1));

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
                    ushort value = (ushort)(use.inDe == eEffect_IncreaseDecrease.Increase ? use.value : (use.value * -1));

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
                    ushort value = (ushort)(use.inDe == eEffect_IncreaseDecrease.Increase ? use.value : (use.value * -1));

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
                    ushort value = (ushort)(use.inDe == eEffect_IncreaseDecrease.Increase ? use.value : (use.value * -1));

                    if (use.isPercent == true)
                    {
                        if (IngameManager.instance.saveData.userData.EXP_Effect_Per - (short)value < 0)
                        {
                            IngameManager.instance.saveData.userData.EXP_Effect_Per = 0;

                            return;
                        }

                        IngameManager.instance.saveData.userData.EXP_Effect_Per -= (short)value;

                        return;
                    }

                    if (IngameManager.instance.saveData.userData.currentEXP - value < 0)
                    {
                        IngameManager.instance.saveData.userData.currentEXP = 0;

                        return;
                    }

                    IngameManager.instance.saveData.userData.currentEXP -= (ushort)value;
                }
                break;

            case eStats.Coin:
                {
                    ushort value = (ushort)(use.inDe == eEffect_IncreaseDecrease.Increase ? use.value : (use.value * -1));

                    if (use.isPercent == true)
                    {
                        if (IngameManager.instance.saveData.userData.Coin_Effect_Per - (short)value < 0)
                        {
                            IngameManager.instance.saveData.userData.Coin_Effect_Per = 0;

                            return;
                        }

                        IngameManager.instance.saveData.userData.Coin_Effect_Per -= (short)value;

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
                    ushort value = (ushort)(use.inDe == eEffect_IncreaseDecrease.Increase ? use.value : (use.value * -1));

                    if (use.isPercent == true)
                    {
                        if (IngameManager.instance.saveData.userData.Attack_Effect_Per - (short)value < 0)
                        {
                            IngameManager.instance.saveData.userData.Attack_Effect_Per = 0;

                            return;
                        }

                        IngameManager.instance.saveData.userData.Attack_Effect_Per -= (short)value;

                        return;
                    }

                    if (IngameManager.instance.saveData.userData.Attack_Effect - value < 0)
                    {
                        IngameManager.instance.saveData.userData.Attack_Effect = 0;

                        return;
                    }

                    IngameManager.instance.saveData.userData.Attack_Effect -= (short)value;
                }
                break;

            case eStats.Defence:
                {
                    ushort value = (ushort)(use.inDe == eEffect_IncreaseDecrease.Increase ? use.value : (use.value * -1));

                    if (use.isPercent == true)
                    {
                        if (IngameManager.instance.saveData.userData.Defence_Effect_Per - (short)value < 0)
                        {
                            IngameManager.instance.saveData.userData.Defence_Effect_Per = 0;

                            return;
                        }

                        IngameManager.instance.saveData.userData.Defence_Effect_Per -= (short)value;

                        return;
                    }

                    if (IngameManager.instance.saveData.userData.Defence_Effect - value < 0)
                    {
                        IngameManager.instance.saveData.userData.Defence_Effect = 0;

                        return;
                    }

                    IngameManager.instance.saveData.userData.Defence_Effect -= (short)value;
                }
                break;
        }
    }

    private void ApplyEffect(DataManager.Duration use)
    {
        IngameManager.instance.UpdateText(use.name + " �� ȿ���� ����Ǿ����ϴ�.");

        switch (use.stats)
        {
            case eStats.HP:
                {
                    ushort value = (ushort)(use.inDe == eEffect_IncreaseDecrease.Increase ? use.value : (use.value * -1));

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
                    ushort value = (ushort)(use.inDe == eEffect_IncreaseDecrease.Increase ? use.value : (use.value * -1));

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
                    ushort value = (ushort)(use.inDe == eEffect_IncreaseDecrease.Increase ? use.value : (use.value * -1));

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
                    ushort value = (ushort)(use.inDe == eEffect_IncreaseDecrease.Increase ? use.value : (use.value * -1));

                    if (use.isPercent == true)
                    {
                        IngameManager.instance.saveData.userData.EXP_Effect_Per += (short)value;

                        return;
                    }

                    IngameManager.instance.saveData.userData.currentEXP += (ushort)value;

                    LevelUp();
                }
                break;

            case eStats.Coin:
                {
                    ushort value = (ushort)(use.inDe == eEffect_IncreaseDecrease.Increase ? use.value : (use.value * -1));

                    if (use.isPercent == true)
                    {
                        IngameManager.instance.saveData.userData.Coin_Effect_Per += (short)value;

                        return;
                    }

                    IngameManager.instance.saveData.userData.data.coin += value;
                }
                break;

            case eStats.Attack:
                {
                    ushort value = (ushort)(use.inDe == eEffect_IncreaseDecrease.Increase ? use.value : (use.value * -1));

                    if (use.isPercent == true)
                    {
                        IngameManager.instance.saveData.userData.Attack_Effect_Per += (short)value;

                        return;
                    }

                    IngameManager.instance.saveData.userData.Attack_Effect += (short)value;
                }
                break;

            case eStats.Defence:
                {
                    ushort value = (ushort)(use.inDe == eEffect_IncreaseDecrease.Increase ? use.value : (use.value * -1));

                    if (use.isPercent == true)
                    {
                        IngameManager.instance.saveData.userData.Defence_Effect_Per += (short)value;

                        return;
                    }

                    IngameManager.instance.saveData.userData.Defence_Effect += (short)value;
                }
                break;
        }
    }
}