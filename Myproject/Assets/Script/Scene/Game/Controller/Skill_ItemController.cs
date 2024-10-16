using UnityEngine;

public class Skill_ItemController : MonoBehaviour
{
    private eAttackDirection _dir = eAttackDirection.Non;
    private int _nodeIndex = -1;

    public void Initialize()
    {
        this.gameObject.SetActive(true);
    }

    public void Duration()
    {
        SkillRemindDuration();
        ItemRemindDuration();
    }

    public void SetDirCoord(int nodeIndex, eAttackDirection type)
    {
        _nodeIndex = nodeIndex;
        _dir = type;
    }

    #region Skill

    public void UseSkill(int id)
    {
        if (id == -1)
        {
            IngameManager.instance.UpdateText("선택된 스킬이 없습니다.");
            return;
        }

        int reCooldown = 0;
        if (SkillCheckCoolDown(id, ref reCooldown) == false)
        {
            IngameManager.instance.UpdateText("대기순서가 " + reCooldown + "만큼 남았습니다.");

            return;
        }

        DataManager.Skill_Data data = GameManager.instance.dataManager.GetskillData(id);

        DataManager.Skill_CoolDown cooldown = new DataManager.Skill_CoolDown();
        cooldown.id = data.id;
        cooldown.name = data.name;
        cooldown.coolDown = data.coolDown + 1;
        IngameManager.instance.saveData.userData.coolDownSkill.Add(cooldown);

        int useMP = data.useMp;
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

        _dir = eAttackDirection.Non;
        _nodeIndex = -1;

        IngameManager.instance.UpdateText("스킬 " + data.name + " (을)를 발동했습니다.");
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

    #endregion

    #region ConsumptionItem

    public void UseIConsumptiontem(int id)
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

        _dir = eAttackDirection.Non;
        _nodeIndex = -1;

        IngameManager.instance.UpdateText("아이템 " + data.name + " (을)를 사용했습니다.");
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

    #endregion

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
