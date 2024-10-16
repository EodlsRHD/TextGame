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

    public void UserDuration(ref DataManager.Creature_Data userData)
    {
        SkillRemindDuration(ref userData);
        ItemRemindDuration(ref userData);

        IngameManager.instance.CheckOverValue();

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

    public void UseSkill(int id, ref DataManager.Creature_Data creature)
    {
        if (SkillCheckCoolDown(id, ref creature) == false)
        {
            return;
        }

        SkillData data = GameManager.instance.dataManager.GetskillData(id);

        Skill_CoolDown cooldown = new Skill_CoolDown();
        cooldown.id = data.id;
        cooldown.name = data.name;
        cooldown.coolDown = data.coolDown + 1;
        IngameManager.instance.saveData.userData.data.coolDownSkill.Add(cooldown);

        int useMP = data.useMp;
        int reMP = creature.currentMP - useMP;
        creature.currentMP = (short)(reMP < 0 ? 0 : reMP);

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

    private bool SkillCheckCoolDown(int id, ref DataManager.Creature_Data creature)
    {
        if(creature.coolDownSkill.Find(x => x.id == id) == null)
        {
            return false;
        }

        return true;
    }

    private void SkillConsumptionCheck(ref DataManager.Creature_Data creature, SkillData data, eStats type, int useValue, bool isPercent = false)
    {
        if (useValue == 0)
        {
            return;
        }

        Duration temp = new Duration();
        temp.ID = data.id;
        temp.name = data.name;
        temp.stats = type;
        temp.isPercent = isPercent;
        temp.value = (short)useValue;
        temp.remaindCooldown = data.coolDown + 1;
        temp.remaindDuration = data.tool.duration + 1;

        if (data.tool.duration == 0)
        {
            ApplyEffect(ref creature, temp);

            return;
        }

        creature.Skill_Duration.Add(temp);
    }

    public void SkillRemindDuration(ref DataManager.Creature_Data data)
    {
        for (int i = data.Skill_Duration.Count - 1; i >= 0; i--)
        {
            if (data.Skill_Duration[i].remaindDuration == 0)
            {
                RemoveEffect(ref data, data.Skill_Duration[i]);

                data.Skill_Duration.Remove(data.Skill_Duration[i]);

                continue;
            }

            ApplyEffect(ref data, data.Skill_Duration[i]);
            data.Skill_Duration[i].remaindDuration -= 1;
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

        IngameManager.instance.UpdateData();
    }

    #endregion

    #region Consumption Item

    public void UseConsumptionItem(int id, ref DataManager.Creature_Data creature)
    {
        creature.itemIndexs.Remove(creature.itemIndexs.Find(x => x == id));

        ItemData data = GameManager.instance.dataManager.GetItemData(id);

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

    private void ItemConsumptionCheck(ref DataManager.Creature_Data creature, ItemData data, eStats type, int useValue, bool isPercent = false)
    {
        if (useValue == 0)
        {
            return;
        }
        Duration temp = new Duration();
        temp.ID = data.id;
        temp.name = data.name;
        temp.stats = type;
        temp.isPercent = isPercent;
        temp.value = (short)useValue;
        temp.remaindDuration = data.tool.duration + 1;

        if (data.tool.duration == 0)
        {
            ApplyEffect(ref creature, temp);

            return;
        }

        IngameManager.instance.saveData.userData.data.item_Duration.Add(temp);
    }

    public void ItemRemindDuration(ref DataManager.Creature_Data data)
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

        IngameManager.instance.UpdateData();
    }

    #endregion

    private void RemoveEffect(ref DataManager.Creature_Data data, Duration duration)
    {
        switch (duration.stats)
        {
            case eStats.HP:
                Remove(duration, ref data.currentHP, ref data.HP_Effect_Per);
                break;

            case eStats.MP:
                Remove(duration, ref data.currentMP, ref data.MP_Effect_Per);
                break;

            case eStats.AP:
                Remove(duration, ref data.currentAP, ref data.AP_Effect_Per);
                break;

            case eStats.EXP:
                Remove(duration, ref data.currentEXP, ref data.EXP_Effect_Per);
                IngameManager.instance.CheckExp();
                break;

            case eStats.Coin:
                Remove(duration, ref data.coin, ref data.Coin_Effect_Per);
                break;

            case eStats.Attack:
                Remove(duration, ref data.Attack_Effect, ref data.Attack_Effect_Per);
                break;

            case eStats.Defence:
                Remove(duration, ref data.Defence_Effect, ref data.Defence_Effect_Per);
                break;
        }
    }

    private void Remove(Duration duration, ref short value, ref short percent)
    {
        if (duration.isPercent == true)
        {
            if (percent - duration.value < 0)
            {
                percent = 0;

                return;
            }

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

    private void ApplyEffect(ref DataManager.Creature_Data data, Duration duration)
    {
        switch (duration.stats)
        {
            case eStats.HP:
                Apply(duration, ref data.currentHP, ref data.HP_Effect_Per);
                break;

            case eStats.MP:
                Apply(duration, ref data.currentMP, ref data.MP_Effect_Per);
                break;

            case eStats.AP:
                Apply(duration, ref data.currentAP, ref data.AP_Effect_Per);
                break;

            case eStats.EXP:
                Apply(duration, ref data.currentEXP, ref data.EXP_Effect_Per);
                IngameManager.instance.CheckExp();
                break;

            case eStats.Coin:
                Apply(duration, ref data.coin, ref data.Coin_Effect_Per);
                break;

            case eStats.Attack:
                Apply(duration, ref data.Attack_Effect, ref data.Attack_Effect_Per);
                break;

            case eStats.Defence:
                Apply(duration, ref data.Defence_Effect, ref data.Defence_Effect_Per);
                break;
        }
    }

    private void Apply(Duration duration, ref short value, ref short percent)
    {
        if (duration.isPercent == true)
        {
            percent += value;

            return;
        }

        value += value;
    }
}
