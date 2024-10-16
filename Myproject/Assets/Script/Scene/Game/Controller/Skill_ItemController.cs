using System.Collections.Generic;
using UnityEngine; 

public class Skill_ItemController : MonoBehaviour
{
    private eDir _dir = eDir.Non;
    private int _nodeIndex = -1;

    public void Initialize()
    {
        this.gameObject.SetActive(true);
    }

    public void UserDuration(ref CreatureData userData)
    {
        SkillRemindDuration(ref userData);
        ItemRemindDuration(ref userData);
        AbnormalStatusesDuration(ref userData);

        IngameManager.instance.UpdateData();

        MonsterDuration(ref IngameManager.instance.saveData.mapData);
    }

    private void MonsterDuration(ref DataManager.Map_Data mapData)
    {
        //for (int i = 0; i < mapData.; i++)
        //{

        //}
    }

    public void SetDirCoord(int nodeIndex, eDir type)
    {
        _nodeIndex = nodeIndex;
        _dir = type;
    }

    #region Skill

    public void UseSkill(int id, ref CreatureData creature)
    {
        if (SkillCheckCoolDown(id, ref creature) == false)
        {
            return;
        }

        SkillData data = GameManager.instance.dataManager.GetskillData(id);
        StrengtheningTool(ref creature, data.tool);

        Skill_CoolDown cooldown = new Skill_CoolDown();
        cooldown.id = data.id;
        cooldown.name = data.name;
        cooldown.coolDown = data.coolDown + 1;
        IngameManager.instance.saveData.userData.data.coolDownSkill.Add(cooldown);

        creature.stats.mp.MinusCurrnet(data.useMp);

        SkillConsumptionCheck(ref creature, data, eStats.HP, data.tool.hp.value);
        SkillConsumptionCheck(ref creature, data, eStats.HP, data.tool.hp.percent, true);

        SkillConsumptionCheck(ref creature, data, eStats.MP, data.tool.mp.value);
        SkillConsumptionCheck(ref creature, data, eStats.MP, data.tool.mp.percent, true);

        SkillConsumptionCheck(ref creature, data, eStats.AP, data.tool.ap.value);
        SkillConsumptionCheck(ref creature, data, eStats.AP, data.tool.ap.percent, true);

        SkillConsumptionCheck(ref creature, data, eStats.EXP, data.tool.exp.value);
        SkillConsumptionCheck(ref creature, data, eStats.EXP, data.tool.exp.percent, true);

        SkillConsumptionCheck(ref creature, data, eStats.Coin, data.tool.coin.value);
        SkillConsumptionCheck(ref creature, data, eStats.Coin, data.tool.coin.percent, true);

        SkillConsumptionCheck(ref creature, data, eStats.Attack, data.tool.attack.value);
        SkillConsumptionCheck(ref creature, data, eStats.Attack, data.tool.attack.percent, true);

        SkillConsumptionCheck(ref creature, data, eStats.Defence, data.tool.defence.value);
        SkillConsumptionCheck(ref creature, data, eStats.Defence, data.tool.defence.percent, true);

        IngameManager.instance.ControlPadUpdateData();
        IngameManager.instance.UpdateData();

        _dir = eDir.Non;
        _nodeIndex = -1;
    }

    private bool SkillCheckCoolDown(int id, ref CreatureData creature)
    {
        if(creature.coolDownSkill.Find(x => x.id == id) == null)
        {
            return false;
        }

        return true;
    }

    private void SkillConsumptionCheck(ref CreatureData creature, SkillData data, eStats type, int useValue, bool isPercent = false)
    {
        if (useValue == 0)
        {
            return;
        }

        Duration duration = new Duration();
        duration.id = data.id;
        duration.name = data.name;
        duration.stats = type;
        duration.isPercent = isPercent;
        duration.value = (short)useValue;
        duration.remaindCooldown = data.coolDown + 1;
        duration.remaindDuration = data.tool.duration + 1;

        if (data.tool.duration == 0)
        {
            ApplyEffect(ref creature, duration);

            return;
        }

        creature.skill_Duration.Add(duration);
    }

    public void SkillRemindDuration(ref CreatureData data)
    {
        for (int i = data.skill_Duration.Count - 1; i >= 0; i--)
        {
            if (data.skill_Duration[i].remaindDuration == 0)
            {
                RemoveEffect(ref data, data.skill_Duration[i]);

                data.skill_Duration.Remove(data.skill_Duration[i]);

                continue;
            }

            ApplyEffect(ref data, data.skill_Duration[i]);
            data.skill_Duration[i].remaindDuration -= 1;
        }

        for (int i = data.coolDownSkill.Count - 1; i >= 0; i--)
        {
            if (data.coolDownSkill[i].coolDown == 0)
            {
                data.coolDownSkill.Remove(data.coolDownSkill[i]);

                continue;
            }

            data.coolDownSkill[i].coolDown -= 1;
        }
    }

    #endregion

    #region Consumption Item

    public void PlayerDefence(ref CreatureData creature, Duration duration)
    {
        ApplyEffect(ref creature, duration);

        IngameManager.instance.UpdatePlayerData();
    }

    public void UseConsumptionItem(int id, ref CreatureData creature)
    {
        creature.itemIndexs.Remove(creature.itemIndexs.Find(x => x == id));

        ItemData data = GameManager.instance.dataManager.GetItemData(id);
        StrengtheningTool(ref creature, data.tool);

        ItemConsumptionCheck(ref creature, data, eStats.HP, data.tool.hp.value);
        ItemConsumptionCheck(ref creature, data, eStats.HP, data.tool.hp.percent, true);

        ItemConsumptionCheck(ref creature, data, eStats.MP, data.tool.mp.value);
        ItemConsumptionCheck(ref creature, data, eStats.MP, data.tool.mp.percent, true);

        ItemConsumptionCheck(ref creature, data, eStats.AP, data.tool.ap.value);
        ItemConsumptionCheck(ref creature, data, eStats.AP, data.tool.ap.percent, true);

        ItemConsumptionCheck(ref creature, data, eStats.EXP, data.tool.exp.value);
        ItemConsumptionCheck(ref creature, data, eStats.EXP, data.tool.exp.percent, true);

        ItemConsumptionCheck(ref creature, data, eStats.Coin, data.tool.coin.value);
        ItemConsumptionCheck(ref creature, data, eStats.Coin, data.tool.coin.percent, true);

        ItemConsumptionCheck(ref creature, data, eStats.Attack, data.tool.attack.value);
        ItemConsumptionCheck(ref creature, data, eStats.Attack, data.tool.attack.percent, true);

        ItemConsumptionCheck(ref creature, data, eStats.Defence, data.tool.defence.value);
        ItemConsumptionCheck(ref creature, data, eStats.Defence, data.tool.defence.percent, true);

        IngameManager.instance.ControlPadUpdateData();
        IngameManager.instance.UpdateData();

        _dir = eDir.Non;
        _nodeIndex = -1;

        IngameManager.instance.UpdateText("아이템 " + data.name + " (을)를 사용했습니다.");
    }

    private void ItemConsumptionCheck(ref CreatureData creature, ItemData data, eStats type, int useValue, bool isPercent = false)
    {
        if (useValue == 0)
        {
            return;
        }
        Duration duration = new Duration();
        duration.id = data.id;
        duration.name = data.name;
        duration.stats = type;
        duration.isPercent = isPercent;
        duration.value = (short)useValue;
        duration.remaindDuration = data.tool.duration + 1;

        if (data.tool.duration == 0)
        {
            ApplyEffect(ref creature, duration);

            return;
        }

        IngameManager.instance.saveData.userData.data.item_Duration.Add(duration);
    }

    public void ItemRemindDuration(ref CreatureData data)
    {
        for (int i = data.item_Duration.Count - 1; i >= 0; i--)
        {
            if (data.item_Duration[i].remaindDuration == 0)
            {
                RemoveEffect(ref data, data.item_Duration[i]);

                data.item_Duration.Remove(data.item_Duration[i]);

                continue;
            }

            ApplyEffect(ref data, data.item_Duration[i]);
            data.item_Duration[i].remaindDuration -= 1;
        }
    }

    #endregion

    private void RemoveEffect(ref CreatureData data, Duration duration)
    {
        switch (duration.stats)
        {
            case eStats.HP:
                Remove(duration, ref data.stats.hp.currnet, ref data.stats.hp.percent);
                break;

            case eStats.MP:
                Remove(duration, ref data.stats.mp.currnet, ref data.stats.mp.percent);
                break;

            case eStats.AP:
                Remove(duration, ref data.stats.ap.currnet, ref data.stats.ap.percent);
                break;

            case eStats.EXP:
                Remove(duration, ref data.stats.exp.currnet, ref data.stats.exp.percent);
                IngameManager.instance.GetExp(0);
                break;

            case eStats.Coin:
                Remove(duration, ref data.stats.coin.currnet, ref data.stats.coin.percent);
                break;

            case eStats.Attack:
                Remove(duration, ref data.stats.attack.plus, ref data.stats.attack.percent);
                break;

            case eStats.Defence:
                Remove(duration, ref data.stats.defence.plus, ref data.stats.defence.percent);
                break;
        }

        IngameManager.instance.UpdatePlayerData();
    }

    private void Remove(Duration duration, ref short value, ref short percent)
    {
        if (duration.isPercent == true)
        {
            percent -= duration.value;
            
            return;
        }

        if (value - duration.value < 0)
        {
            value = 0;

            return;
        }

        value -= duration.value;
    }

    private void ApplyEffect(ref CreatureData data, Duration duration)
    {
        switch (duration.stats)
        {
            case eStats.HP:
                Apply(duration, ref data.stats.hp.currnet, ref data.stats.hp.percent);
                break;

            case eStats.MP:
                Apply(duration, ref data.stats.mp.currnet, ref data.stats.mp.percent);
                break;

            case eStats.AP:
                Apply(duration, ref data.stats.ap.currnet, ref data.stats.ap.percent);
                break;

            case eStats.EXP:
                Apply(duration, ref data.stats.exp.currnet, ref data.stats.exp.percent);
                IngameManager.instance.GetExp(0);
                break;

            case eStats.Coin:
                Apply(duration, ref data.stats.coin.currnet, ref data.stats.coin.percent);
                break;

            case eStats.Attack:
                Apply(duration, ref data.stats.attack.plus, ref data.stats.attack.percent);
                break;

            case eStats.Defence:
                Apply(duration, ref data.stats.defence.plus, ref data.stats.defence.percent);
                break;
        }

        IngameManager.instance.UpdatePlayerData();
    }

    private void Apply(Duration duration, ref short value, ref short percent)
    {
        if (duration.isPercent == true)
        {
            percent += value;

            return;
        }

        value += duration.value;
    }

    private void StrengtheningTool(ref CreatureData data, StrengtheningTool tool)
    {
        if(tool.grantStatus == eStrengtheningTool.Non)
        {
            return;
        }

        if(tool.needStatus != eStrengtheningTool.Non)
        { 
            if(tool.needStatus != data.defultStatus)
            {
                return;
            }
        }

        for (int i = 0; i < data.abnormalStatuses.Count; i++)
        {
            if (data.abnormalStatuses[i].currentStatus == tool.grantStatus)
            {
                data.abnormalStatuses[i].statusCount = tool.duration;

                return;
            }
        }

        AbnormalStatus newData = new AbnormalStatus();
        newData.currentStatus = tool.grantStatus;
        newData.statusCount = tool.duration;
        newData.value = tool.value;

        data.abnormalStatuses.Add(newData);
    }

    private void AbnormalStatusesDuration(ref CreatureData data)
    {
        for (int i = data.abnormalStatuses.Count - 1; i >= 0; i--)
        {
            if (data.abnormalStatuses[i].statusCount == 0)
            {
                data.abnormalStatuses.Remove(data.abnormalStatuses[i]);

                continue;
            }

            data.abnormalStatuses[i].statusCount -= 1;
        }
    }
}
