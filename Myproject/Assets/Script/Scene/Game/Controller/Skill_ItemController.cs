using JetBrains.Annotations;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
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

    private void MonsterDuration(ref DataManager.MapData mapData)
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

    public void UseSkill(bool isMonster, int id, ref CreatureData creature)
    {
        if(id == -1)
        {
            if(isMonster == false)
            {
                IngameManager.instance.UpdatePopup("선택된 스킬이 없습니다.");
            }

            return;
        }

        if (SkillCheckCoolDown(id, ref creature) == true)
        {
            return;
        }

        SkillData skill = GameManager.instance.dataManager.GetskillData(id);
        StrengtheningTool(ref creature, skill.tool);

        creature.stats.mp.MinusCurrnet(skill.useMp);

        if (isMonster == false)
        {
            IngameManager.instance.UpdatePlayerInfo(eStats.MP);
        }

        Skill_CoolDown cooldown = new Skill_CoolDown();
        cooldown.id = skill.id;
        cooldown.name = skill.name;
        cooldown.coolDown = skill.coolDown + 1;
        IngameManager.instance.saveData.userData.data.coolDownSkill.Add(cooldown);

        if ((_dir == eDir.Non && _nodeIndex == -1) == false)
        {
            UseGround(isMonster, ref skill, creature.currentNodeIndex, _dir, _nodeIndex);
        }

        SkillConsumptionCheck(ref creature, skill, eStats.HP, skill.tool.hp.value);
        SkillConsumptionCheck(ref creature, skill, eStats.HP, skill.tool.hp.percent, true);

        SkillConsumptionCheck(ref creature, skill, eStats.MP, skill.tool.mp.value);
        SkillConsumptionCheck(ref creature, skill, eStats.MP, skill.tool.mp.percent, true);

        SkillConsumptionCheck(ref creature, skill, eStats.AP, skill.tool.ap.value);
        SkillConsumptionCheck(ref creature, skill, eStats.AP, skill.tool.ap.percent, true);

        SkillConsumptionCheck(ref creature, skill, eStats.EXP, skill.tool.exp.value);
        SkillConsumptionCheck(ref creature, skill, eStats.EXP, skill.tool.exp.percent, true);

        SkillConsumptionCheck(ref creature, skill, eStats.Coin, skill.tool.coin.value);
        SkillConsumptionCheck(ref creature, skill, eStats.Coin, skill.tool.coin.percent, true);

        SkillConsumptionCheck(ref creature, skill, eStats.Attack, skill.tool.attack.value);
        SkillConsumptionCheck(ref creature, skill, eStats.Attack, skill.tool.attack.percent, true);

        SkillConsumptionCheck(ref creature, skill, eStats.Defence, skill.tool.defence.value);
        SkillConsumptionCheck(ref creature, skill, eStats.Defence, skill.tool.defence.percent, true);

        IngameManager.instance.ControlPadUpdateData();
        IngameManager.instance.UpdateData();

        _dir = eDir.Non;
        _nodeIndex = -1;
    }

    private bool SkillCheckCoolDown(int id, ref CreatureData creature)
    {
        if(creature.coolDownSkill.Find(x => x.id == id) != null)
        {
            return true;
        }

        return false;
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
        duration.remaindCooldown = data.coolDown;
        duration.remaindDuration = data.tool.duration;

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
            if (data.skill_Duration[i].remaindDuration == 1)
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
            if (data.coolDownSkill[i].coolDown == 1)
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
        if (id == -1)
        {
            IngameManager.instance.UpdatePopup("선택된 아이템이 없습니다.");

            return;
        }

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
        duration.remaindDuration = data.tool.duration;

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
            if (data.item_Duration[i].remaindDuration == 1)
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

    #region Effect

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

    #endregion

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
                data.abnormalStatuses[i].duration = tool.duration;

                return;
            }
        }

        AbnormalStatus newData = new AbnormalStatus();
        newData.currentStatus = tool.grantStatus;
        newData.duration = tool.duration;
        newData.value = tool.value;

        data.abnormalStatuses.Add(newData);
    }

    private void AbnormalStatusesDuration(ref CreatureData data)
    {
        for (int i = data.abnormalStatuses.Count - 1; i >= 0; i--)
        {
            if (data.abnormalStatuses[i].duration <= 0)
            {
                data.abnormalStatuses.Remove(data.abnormalStatuses[i]);

                continue;
            }

            data.abnormalStatuses[i].duration -= 1;
        }
    }

    private void UseGround(bool isMonster, ref SkillData skill, int currentNodeIndex, eDir dir, int nodeIndex)
    {
        if(dir != eDir.Non)
        {
            int range = skill.tool.range;

            bool isWalkable = true;
            int blockBeforeNode = 0;
            List<int> nodeIndexs = IngameManager.instance.GetRangeNodes(currentNodeIndex, dir, range);

            for (int i = 0; i < nodeIndexs.Count; i++)
            {
                if (isWalkable == false)
                {
                    blockBeforeNode = i - 1;

                    break;
                }

                var node = IngameManager.instance.saveData.mapData.nodeDatas[nodeIndexs[i]];

                if (i == (nodeIndexs.Count - 1))
                {
                    if (node.isGuide == true)
                    {
                        isWalkable = false;
                    }

                    if (node.isBonfire == true)
                    {
                        isWalkable = false;
                    }

                    if (node.isShop == true)
                    {
                        isWalkable = false;
                    }

                    if (node.isWalkable == false)
                    {
                        isWalkable = false;
                    }

                    if (node.isExit == false)
                    {
                        isWalkable = false;
                    }
                }

                if (skill.tool.grantStatus == eStrengtheningTool.Non_Teleportation)
                {
                    if(isMonster  == false)
                    {
                        if (node.isMonster == true)
                        {
                            isWalkable = false;
                        }
                    }
                    else if(isMonster == true)
                    {
                        if(node.isUser == true)
                        {
                            isWalkable = false;
                        }
                    }
                }

                blockBeforeNode = i;
            }

            if(blockBeforeNode == -1)
            {
                return;
            }

            if(range > 0)
            {
                switch(skill.tool.grantStatus)
                {
                    case eStrengtheningTool.Non:
                        RangeAttack(isMonster, skill, nodeIndexs);
                        break;

                    case eStrengtheningTool.Teleportation:
                        RangeMove(isMonster, skill, nodeIndexs, currentNodeIndex);
                        break;

                    case eStrengtheningTool.Non_Teleportation:
                        RangeMove(isMonster, skill, nodeIndexs, currentNodeIndex, blockBeforeNode);
                        break;
                }
            }
        }

        if(nodeIndex != -1)
        {
            switch (skill.tool.grantStatus)
            {
                case eStrengtheningTool.Non:

                    break;

                case eStrengtheningTool.Teleportation:

                    break;

                case eStrengtheningTool.Non_Teleportation:

                    break;
            }
        }
    }

    private void RangeAttack(bool isMonster, SkillData skill, List<int> indexs)
    {
        AbnormalStatus abnormalStatus = new AbnormalStatus();
        abnormalStatus.id = skill.id;
        abnormalStatus.currentStatus = skill.tool.grantStatus;
        abnormalStatus.duration = skill.tool.duration;
        abnormalStatus.value = skill.tool.value;

        for (int i = 0; i < indexs.Count; i++)
        {
            int index = indexs[i];

            if(isMonster == false)
            {
                if (IngameManager.instance.saveData.mapData.nodeDatas[index].isMonster == true)
                {
                    if(skill.tool.duration == 0)
                    {
                        IngameManager.instance.MonsterHit(index, skill.tool.value);
                    }
                    else
                    {
                        IngameManager.instance.saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == index).abnormalStatuses.Add(abnormalStatus);
                    }
                }
            }
            else if(isMonster == true)
            {
                if (IngameManager.instance.saveData.mapData.nodeDatas[index].isUser == true)
                {
                    if (skill.tool.duration == 0)
                    {
                        IngameManager.instance.PlayerHit(skill.tool.value);
                    }
                    else
                    {
                        IngameManager.instance.saveData.userData.data.abnormalStatuses.Add(abnormalStatus);
                    }
                }
            }
        }
    }

    private void RangeMove(bool isMonster, SkillData skill, List<int> indexs, int currentNodeIndex, int blockBeforeNodeIndex = -1)
    {
        int destinationIndex = indexs[blockBeforeNodeIndex == -1 ? indexs.Count - 1 : blockBeforeNodeIndex];

        if(isMonster == false)
        {
            int before = IngameManager.instance.saveData.userData.data.currentNodeIndex;
            IngameManager.instance.saveData.mapData.nodeDatas[before].isUser = false;

            IngameManager.instance.saveData.userData.data.currentNodeIndex = destinationIndex;
            IngameManager.instance.saveData.mapData.nodeDatas[destinationIndex].isUser = true;
        }
        else if(isMonster == true)
        {
            IngameManager.instance.saveData.mapData.nodeDatas[currentNodeIndex].isMonster = false;
            IngameManager.instance.saveData.mapData.nodeDatas[destinationIndex].isMonster = true;
            IngameManager.instance.saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == currentNodeIndex).currentNodeIndex = destinationIndex;
        }

        IngameManager.instance.UpdateData();
    }

    private void NodeAttack()
    {

    }

    private void NodeMove()
    {

    }
}
