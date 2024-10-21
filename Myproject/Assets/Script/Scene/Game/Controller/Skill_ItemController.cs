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
    }

    public void MonsterDuration(ref DataManager.MapData mapData)
    {
        for(int i = 0; i < mapData.monsterDatas.Count; i++)
        {
            CreatureData creatrue = mapData.monsterDatas[i];

            for(int j = creatrue.skill_Duration.Count - 1; j >= 0; j--)
            {
                creatrue.skill_Duration[i].remaindDuration -= 1;

                if(creatrue.skill_Duration[j].remaindDuration <= 0)
                {
                    RemoveEffect(ref creatrue, creatrue.skill_Duration[j]);

                    creatrue.skill_Duration.Remove(creatrue.skill_Duration[j]);

                    continue;
                }

                ApplyEffect(ref creatrue, creatrue.skill_Duration[i]);
            }

            for(int j = creatrue.coolDownSkill.Count - 1; j >= 0; j--)
            {
                creatrue.coolDownSkill[j].coolDown -= 1;

                if(creatrue.coolDownSkill[j].coolDown <= 0)
                {
                    creatrue.coolDownSkill.Remove(creatrue.coolDownSkill[j]);

                    continue;
                }
            }
        }

        IngameManager.instance.UpdateData();
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
            Skill_UseGround(isMonster, ref skill, creature.currentNodeIndex, _dir, _nodeIndex);
        }
        else
        {
            if(isMonster == false)
            {
                IngameManager.instance.UpdatePlayerInfo(eStats.MP);
            }
        }

        SkillConsumptionCheck(ref creature, skill, eStats.HP, skill.GetStat_Value(eStats.HP));
        SkillConsumptionCheck(ref creature, skill, eStats.HP, skill.GetStat_Percent(eStats.HP), true);

        SkillConsumptionCheck(ref creature, skill, eStats.MP, skill.GetStat_Value(eStats.MP));
        SkillConsumptionCheck(ref creature, skill, eStats.MP, skill.GetStat_Percent(eStats.MP), true);

        SkillConsumptionCheck(ref creature, skill, eStats.AP, skill.GetStat_Value(eStats.AP));
        SkillConsumptionCheck(ref creature, skill, eStats.AP, skill.GetStat_Percent(eStats.AP), true);

        SkillConsumptionCheck(ref creature, skill, eStats.EXP, skill.GetStat_Value(eStats.EXP));
        SkillConsumptionCheck(ref creature, skill, eStats.EXP, skill.GetStat_Percent(eStats.EXP), true);

        SkillConsumptionCheck(ref creature, skill, eStats.Coin, skill.GetStat_Value(eStats.Coin));
        SkillConsumptionCheck(ref creature, skill, eStats.Coin, skill.GetStat_Percent(eStats.Coin), true);

        SkillConsumptionCheck(ref creature, skill, eStats.Attack, skill.GetStat_Value(eStats.Attack));
        SkillConsumptionCheck(ref creature, skill, eStats.Attack, skill.GetStat_Percent(eStats.Attack), true);

        SkillConsumptionCheck(ref creature, skill, eStats.Defence, skill.GetStat_Value(eStats.Defence));
        SkillConsumptionCheck(ref creature, skill, eStats.Defence, skill.GetStat_Percent(eStats.Defence), true);

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
        duration.maintain = data.tool.maintain;

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
            data.skill_Duration[i].remaindDuration -= 1;

            if (data.skill_Duration[i].remaindDuration <= 0)
            {
                RemoveEffect(ref data, data.skill_Duration[i]);

                data.skill_Duration.Remove(data.skill_Duration[i]);

                continue;
            }

            ApplyEffect(ref data, data.skill_Duration[i]);
        }

        for (int i = data.coolDownSkill.Count - 1; i >= 0; i--)
        {
            data.coolDownSkill[i].coolDown -= 1;

            if (data.coolDownSkill[i].coolDown <= 0)
            {
                data.coolDownSkill.Remove(data.coolDownSkill[i]);

                continue;
            }
        }
    }

    private void Skill_UseGround(bool isMonster, ref SkillData skill, int currentNodeIndex, eDir dir, int nodeIndex)
    {
        IngameManager.instance.UpdateText(skill.name + " (을)를 사용했습니다.");

        if(skill.tool.revealMap == true)
        {
            Skill_RevealMap(dir, nodeIndex, skill.tool.range);

            return;
        }

        if(dir != eDir.Non)
        {
            int range = skill.tool.range;

            bool isWalkable = true;
            int blockBeforeNode = 0;
            List<int> nodeIndexs = IngameManager.instance.GetDirectionRangeNodes(currentNodeIndex, dir, range);

            for(int i = 0; i < nodeIndexs.Count; i++)
            {
                if(isWalkable == false)
                {
                    blockBeforeNode = i - 1;

                    break;
                }

                var node = IngameManager.instance.saveData.mapData.nodeDatas[nodeIndexs[i]];

                if(i == (nodeIndexs.Count - 1))
                {
                    if(node.isGuide == true)
                    {
                        isWalkable = false;
                    }

                    if(node.isBonfire == true)
                    {
                        isWalkable = false;
                    }

                    if(node.isShop == true)
                    {
                        isWalkable = false;
                    }

                    if(node.isWalkable == false)
                    {
                        isWalkable = false;
                    }

                    if(node.isExit == false)
                    {
                        isWalkable = false;
                    }
                }

                if(skill.tool.grantStatus == eStrengtheningTool.Non_Teleportation)
                {
                    if(isMonster == false)
                    {
                        if(node.isMonster == true)
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

            switch(skill.tool.grantStatus)
            {
                case eStrengtheningTool.Non:
                    Skill_RangeAttack(isMonster, skill, nodeIndexs);
                    break;

                case eStrengtheningTool.Teleportation:
                    Skill_RangeMove(isMonster, skill, nodeIndexs, currentNodeIndex);
                    break;

                case eStrengtheningTool.Non_Teleportation:
                    Skill_RangeMove(isMonster, skill, nodeIndexs, currentNodeIndex, blockBeforeNode);
                    break;
            }
        }

        if(nodeIndex != -1)
        {
            switch(skill.tool.grantStatus)
            {
                case eStrengtheningTool.Non:
                    Skill_NodeAttack(isMonster, skill, nodeIndex);
                    break;

                case eStrengtheningTool.Teleportation:
                    Skill_NodeMove(isMonster, skill, nodeIndex, currentNodeIndex);
                    break;
            }
        }

        IngameManager.instance.UpdateData();
    }

    private void Skill_RevealMap(eDir dir, int nodeIndex, int range)
    {
        List<int> dx = new List<int>();
        List<int> dy = new List<int>();

        if(dir != eDir.All)
        {
            for(int i = -range; i <= range; i++)
            {
                dx.Add(i);
                dy.Add(i);
            }
        }

        List<int> list = IngameManager.instance.GetRangeNodes_Diagonal(dx, dy, nodeIndex);
        IngameManager.instance.RevealMap(dir, list);
    }

    private void Skill_RangeAttack(bool isMonster, SkillData skill, List<int> indexs)
    {
        AbnormalStatus abnormalStatus = new AbnormalStatus();
        abnormalStatus.id = skill.id;
        abnormalStatus.currentStatus = skill.tool.grantStatus;
        abnormalStatus.duration = skill.tool.duration;
        abnormalStatus.value = skill.Get_Value();

        for(int i = 0; i < indexs.Count; i++)
        {
            int index = indexs[i];

            if(isMonster == false)
            {
                if(IngameManager.instance.saveData.mapData.nodeDatas[index].isMonster == true)
                {
                    if(skill.tool.duration == 0)
                    {
                        IngameManager.instance.MonsterHit(index, skill.Get_Value());
                    }
                    else
                    {
                        IngameManager.instance.saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == index).abnormalStatuses.Add(abnormalStatus);
                    }
                }
            }
            else if(isMonster == true)
            {
                if(IngameManager.instance.saveData.mapData.nodeDatas[index].isUser == true)
                {
                    if(skill.tool.duration == 0)
                    {
                        IngameManager.instance.PlayerHit(skill.Get_Value());
                    }
                    else
                    {
                        IngameManager.instance.saveData.userData.data.abnormalStatuses.Add(abnormalStatus);
                    }
                }
            }
        }
    }

    private void Skill_RangeMove(bool isMonster, SkillData skill, List<int> indexs, int currentNodeIndex, int blockBeforeNodeIndex = -1)
    {
        int destinationIndex = indexs[blockBeforeNodeIndex == -1 ? indexs.Count - 1 : blockBeforeNodeIndex];

        AbnormalStatus abnormalStatus = new AbnormalStatus();
        abnormalStatus.id = skill.id;
        abnormalStatus.currentStatus = skill.tool.grantStatus;
        abnormalStatus.duration = skill.tool.duration;
        abnormalStatus.value = skill.tool.value;

        if(isMonster == false)
        {
            IngameManager.instance.saveData.userData.stats.mp.MinusCurrnet(skill.useMp);

            if(skill.tool.duration > 0)
            {
                IngameManager.instance.saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == currentNodeIndex).abnormalStatuses.Add(abnormalStatus);
            }

            int before = IngameManager.instance.saveData.userData.data.currentNodeIndex;
            IngameManager.instance.saveData.mapData.nodeDatas[before].isUser = false;

            IngameManager.instance.saveData.userData.data.currentNodeIndex = destinationIndex;
            IngameManager.instance.saveData.mapData.nodeDatas[destinationIndex].isUser = true;
        }
        else if(isMonster == true)
        {
            if(skill.tool.duration > 0)
            {
                IngameManager.instance.saveData.userData.data.abnormalStatuses.Add(abnormalStatus);
            }

            IngameManager.instance.saveData.mapData.nodeDatas[currentNodeIndex].isMonster = false;
            IngameManager.instance.saveData.mapData.nodeDatas[destinationIndex].isMonster = true;
            IngameManager.instance.saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == currentNodeIndex).currentNodeIndex = destinationIndex;
        }
    }

    private void Skill_NodeAttack(bool isMonster, SkillData skill, int index)
    {
        AbnormalStatus abnormalStatus = new AbnormalStatus();
        abnormalStatus.id = skill.id;
        abnormalStatus.currentStatus = skill.tool.grantStatus;
        abnormalStatus.duration = skill.tool.duration;
        abnormalStatus.value = skill.Get_Value();

        if(skill.tool.range > 0)
        {
            List<int> dx = new List<int>();
            List<int> dy = new List<int>();

            for(int i = -skill.tool.range; i <= skill.tool.range; i++)
            {
                dx.Add(i);
                dy.Add(i);
            }

            List<int> nearbyIndexs = IngameManager.instance.GetRangeNodes_Diagonal(dx, dy, index);

            for(int i = 0; i < nearbyIndexs.Count; i++)
            {
                int node = nearbyIndexs[i];

                if(isMonster == false)
                {
                    IngameManager.instance.saveData.userData.stats.mp.MinusCurrnet(skill.useMp);

                    if(IngameManager.instance.saveData.mapData.nodeDatas[node].isMonster == true)
                    {
                        if(skill.tool.duration == 0)
                        {
                            IngameManager.instance.MonsterHit(node, skill.Get_Value());
                        }
                        else
                        {
                            IngameManager.instance.saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == node).abnormalStatuses.Add(abnormalStatus);
                        }
                    }
                }
                else if(isMonster == false)
                {
                    if(IngameManager.instance.saveData.mapData.nodeDatas[node].isUser == true)
                    {
                        if(skill.tool.duration == 0)
                        {
                            IngameManager.instance.PlayerHit(skill.Get_Value());
                        }
                        else
                        {
                            IngameManager.instance.saveData.userData.data.abnormalStatuses.Add(abnormalStatus);
                        }
                    }
                }
            }

            return;
        }

        if(isMonster == false)
        {
            IngameManager.instance.saveData.userData.stats.mp.MinusCurrnet(skill.useMp);

            if(IngameManager.instance.saveData.mapData.nodeDatas[index].isMonster == true)
            {
                if(skill.tool.duration == 0)
                {
                    IngameManager.instance.MonsterHit(index, skill.Get_Value());
                }
                else
                {
                    IngameManager.instance.saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == index).abnormalStatuses.Add(abnormalStatus);
                }
            }
        }
        else if(isMonster == true)
        {
            if(IngameManager.instance.saveData.mapData.nodeDatas[index].isUser == true)
            {
                if(skill.tool.duration == 0)
                {
                    IngameManager.instance.PlayerHit(skill.Get_Value());
                }
                else
                {
                    IngameManager.instance.saveData.userData.data.abnormalStatuses.Add(abnormalStatus);
                }
            }
        }
    }

    private void Skill_NodeMove(bool isMonster, SkillData skill, int destinationIndex, int currentNodeIndex)
    {
        AbnormalStatus abnormalStatus = new AbnormalStatus();
        abnormalStatus.id = skill.id;
        abnormalStatus.currentStatus = skill.tool.grantStatus;
        abnormalStatus.duration = skill.tool.duration;
        abnormalStatus.value = skill.tool.value;

        if(isMonster == false)
        {
            if(IngameManager.instance.CheckWalkableNode(destinationIndex) == false)
            {
                IngameManager.instance.UpdatePopup("이동이 불가능 합니다.");

                return;
            }

            int before = IngameManager.instance.saveData.userData.data.currentNodeIndex;
            IngameManager.instance.saveData.mapData.nodeDatas[before].isUser = false;

            IngameManager.instance.saveData.userData.data.currentNodeIndex = destinationIndex;
            IngameManager.instance.saveData.mapData.nodeDatas[destinationIndex].isUser = true;

            IngameManager.instance.saveData.userData.stats.mp.MinusCurrnet(skill.useMp);
        }
        else if(isMonster == true)
        {
            if(IngameManager.instance.CheckWalkableNode(destinationIndex) == false)
            {
                return;
            }

            IngameManager.instance.saveData.mapData.nodeDatas[currentNodeIndex].isMonster = false;
            IngameManager.instance.saveData.mapData.nodeDatas[destinationIndex].isMonster = true;
            IngameManager.instance.saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == currentNodeIndex).currentNodeIndex = destinationIndex;
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

        ItemData item = GameManager.instance.dataManager.GetItemData(id);
        StrengtheningTool(ref creature, item.tool);

        if((_dir == eDir.Non && _nodeIndex == -1) == false)
        {
            Item_UseGround(false, ref item, creature.currentNodeIndex, _dir, _nodeIndex);
        }

        ItemConsumptionCheck(ref creature, item, eStats.HP, item.tool.hp.value);
        ItemConsumptionCheck(ref creature, item, eStats.HP, item.tool.hp.percent, true);

        ItemConsumptionCheck(ref creature, item, eStats.MP, item.tool.mp.value);
        ItemConsumptionCheck(ref creature, item, eStats.MP, item.tool.mp.percent, true);

        ItemConsumptionCheck(ref creature, item, eStats.AP, item.tool.ap.value);
        ItemConsumptionCheck(ref creature, item, eStats.AP, item.tool.ap.percent, true);

        ItemConsumptionCheck(ref creature, item, eStats.EXP, item.tool.exp.value);
        ItemConsumptionCheck(ref creature, item, eStats.EXP, item.tool.exp.percent, true);

        ItemConsumptionCheck(ref creature, item, eStats.Coin, item.tool.coin.value);
        ItemConsumptionCheck(ref creature, item, eStats.Coin, item.tool.coin.percent, true);

        ItemConsumptionCheck(ref creature, item, eStats.Attack, item.tool.attack.value);
        ItemConsumptionCheck(ref creature, item, eStats.Attack, item.tool.attack.percent, true);

        ItemConsumptionCheck(ref creature, item, eStats.Defence, item.tool.defence.value);
        ItemConsumptionCheck(ref creature, item, eStats.Defence, item.tool.defence.percent, true);

        IngameManager.instance.ControlPadUpdateData();
        IngameManager.instance.UpdateData();

        _dir = eDir.Non;
        _nodeIndex = -1;

        IngameManager.instance.UpdateText("아이템 " + item.name + " (을)를 사용했습니다.");
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
        duration.maintain = data.tool.maintain;

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
            data.item_Duration[i].remaindDuration -= 1;

            if (data.item_Duration[i].remaindDuration <= 0)
            {
                RemoveEffect(ref data, data.item_Duration[i]);

                data.item_Duration.Remove(data.item_Duration[i]);

                continue;
            }

            ApplyEffect(ref data, data.item_Duration[i]);
        }
    }


    private void Item_UseGround(bool isMonster, ref ItemData item, int currentNodeIndex, eDir dir, int nodeIndex)
    {
        IngameManager.instance.UpdateText(item.name + " (을)를 사용했습니다.");

        if(item.tool.revealMap == true)
        {
            Item_RevealMap(dir, nodeIndex, item.tool.range);

            return;
        }

        if(dir != eDir.Non)
        {
            int range = item.tool.range;

            bool isWalkable = true;
            int blockBeforeNode = 0;
            List<int> nodeIndexs = IngameManager.instance.GetDirectionRangeNodes(currentNodeIndex, dir, range);

            for(int i = 0; i < nodeIndexs.Count; i++)
            {
                if(isWalkable == false)
                {
                    blockBeforeNode = i - 1;

                    break;
                }

                var node = IngameManager.instance.saveData.mapData.nodeDatas[nodeIndexs[i]];

                if(i == (nodeIndexs.Count - 1))
                {
                    if(node.isGuide == true)
                    {
                        isWalkable = false;
                    }

                    if(node.isBonfire == true)
                    {
                        isWalkable = false;
                    }

                    if(node.isShop == true)
                    {
                        isWalkable = false;
                    }

                    if(node.isWalkable == false)
                    {
                        isWalkable = false;
                    }

                    if(node.isExit == false)
                    {
                        isWalkable = false;
                    }
                }

                if(item.tool.grantStatus == eStrengtheningTool.Non_Teleportation)
                {
                    if(isMonster == false)
                    {
                        if(node.isMonster == true)
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

            switch(item.tool.grantStatus)
            {
                case eStrengtheningTool.Non:
                    Item_RangeAttack(isMonster, item, nodeIndexs);
                    break;

                case eStrengtheningTool.Teleportation:
                    Item_RangeMove(isMonster, item, nodeIndexs, currentNodeIndex);
                    break;

                case eStrengtheningTool.Non_Teleportation:
                    Item_RangeMove(isMonster, item, nodeIndexs, currentNodeIndex, blockBeforeNode);
                    break;
            }
        }

        if(nodeIndex != -1)
        {
            switch(item.tool.grantStatus)
            {
                case eStrengtheningTool.Non:
                    Item_NodeAttack(isMonster, item, nodeIndex);
                    break;

                case eStrengtheningTool.Teleportation:
                    Item_NodeMove(isMonster, item, nodeIndex, currentNodeIndex);
                    break;
            }
        }

        IngameManager.instance.UpdateData();
    }

    private void Item_RevealMap(eDir dir, int nodeIndex, int range)
    {
        List<int> dx = new List<int>();
        List<int> dy = new List<int>();

        if(dir != eDir.All)
        {
            for(int i = -range; i <= range; i++)
            {
                dx.Add(i);
                dy.Add(i);
            }
        }

        List<int> list = IngameManager.instance.GetRangeNodes_Diagonal(dx, dy, nodeIndex);
        list.Add(nodeIndex);
        
        IngameManager.instance.RevealMap(dir, list);
    }

    private void Item_RangeAttack(bool isMonster, ItemData item, List<int> indexs)
    {
        AbnormalStatus abnormalStatus = new AbnormalStatus();
        abnormalStatus.id = item.id;
        abnormalStatus.currentStatus = item.tool.grantStatus;
        abnormalStatus.duration = item.tool.duration;
        abnormalStatus.value = item.tool.value;

        for(int i = 0; i < indexs.Count; i++)
        {
            int index = indexs[i];

            if(isMonster == false)
            {
                if(IngameManager.instance.saveData.mapData.nodeDatas[index].isMonster == true)
                {
                    if(item.tool.duration == 0)
                    {
                        IngameManager.instance.MonsterHit(index, item.tool.value);
                    }
                    else
                    {
                        IngameManager.instance.saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == index).abnormalStatuses.Add(abnormalStatus);
                    }
                }
            }
            else if(isMonster == true)
            {
                if(IngameManager.instance.saveData.mapData.nodeDatas[index].isUser == true)
                {
                    if(item.tool.duration == 0)
                    {
                        IngameManager.instance.PlayerHit(item.tool.value);
                    }
                    else
                    {
                        IngameManager.instance.saveData.userData.data.abnormalStatuses.Add(abnormalStatus);
                    }
                }
            }
        }
    }

    private void Item_RangeMove(bool isMonster, ItemData item, List<int> indexs, int currentNodeIndex, int blockBeforeNodeIndex = -1)
    {
        int destinationIndex = indexs[blockBeforeNodeIndex == -1 ? indexs.Count - 1 : blockBeforeNodeIndex];

        AbnormalStatus abnormalStatus = new AbnormalStatus();
        abnormalStatus.id = item.id;
        abnormalStatus.currentStatus = item.tool.grantStatus;
        abnormalStatus.duration = item.tool.duration;
        abnormalStatus.value = item.tool.value;

        if(isMonster == false)
        {
            if(item.tool.duration > 0)
            {
                IngameManager.instance.saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == currentNodeIndex).abnormalStatuses.Add(abnormalStatus);
            }

            int before = IngameManager.instance.saveData.userData.data.currentNodeIndex;
            IngameManager.instance.saveData.mapData.nodeDatas[before].isUser = false;

            IngameManager.instance.saveData.userData.data.currentNodeIndex = destinationIndex;
            IngameManager.instance.saveData.mapData.nodeDatas[destinationIndex].isUser = true;
        }
        else if(isMonster == true)
        {
            if(item.tool.duration > 0)
            {
                IngameManager.instance.saveData.userData.data.abnormalStatuses.Add(abnormalStatus);
            }

            IngameManager.instance.saveData.mapData.nodeDatas[currentNodeIndex].isMonster = false;
            IngameManager.instance.saveData.mapData.nodeDatas[destinationIndex].isMonster = true;
            IngameManager.instance.saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == currentNodeIndex).currentNodeIndex = destinationIndex;
        }
    }

    private void Item_NodeAttack(bool isMonster, ItemData item, int index)
    {
        AbnormalStatus abnormalStatus = new AbnormalStatus();
        abnormalStatus.id = item.id;
        abnormalStatus.currentStatus = item.tool.grantStatus;
        abnormalStatus.duration = item.tool.duration;
        abnormalStatus.value = item.tool.value;

        if(item.tool.range > 0)
        {
            List<int> dx = new List<int>();
            List<int> dy = new List<int>();

            for(int i = -item.tool.range; i <= item.tool.range; i++)
            {
                dx.Add(i);
                dy.Add(i);
            }

            List<int> nearbyIndexs = IngameManager.instance.GetRangeNodes_Diagonal(dx, dy, index);

            for(int i = 0; i < nearbyIndexs.Count; i++)
            {
                int node = nearbyIndexs[i];

                if(isMonster == false)
                {
                    if(IngameManager.instance.saveData.mapData.nodeDatas[node].isMonster == true)
                    {
                        if(item.tool.duration == 0)
                        {
                            IngameManager.instance.MonsterHit(node, item.tool.value);
                        }
                        else
                        {
                            IngameManager.instance.saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == node).abnormalStatuses.Add(abnormalStatus);
                        }
                    }
                }
                else if(isMonster == false)
                {
                    if(IngameManager.instance.saveData.mapData.nodeDatas[node].isUser == true)
                    {
                        if(item.tool.duration == 0)
                        {
                            IngameManager.instance.PlayerHit(item.tool.value);
                        }
                        else
                        {
                            IngameManager.instance.saveData.userData.data.abnormalStatuses.Add(abnormalStatus);
                        }
                    }
                }
            }

            return;
        }

        if(isMonster == false)
        {
            if(IngameManager.instance.saveData.mapData.nodeDatas[index].isMonster == true)
            {
                if(item.tool.duration == 0)
                {
                    IngameManager.instance.MonsterHit(index, item.tool.value);
                }
                else
                {
                    IngameManager.instance.saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == index).abnormalStatuses.Add(abnormalStatus);
                }
            }
        }
        else if(isMonster == true)
        {
            if(IngameManager.instance.saveData.mapData.nodeDatas[index].isUser == true)
            {
                if(item.tool.duration == 0)
                {
                    IngameManager.instance.PlayerHit(item.tool.value);
                }
                else
                {
                    IngameManager.instance.saveData.userData.data.abnormalStatuses.Add(abnormalStatus);
                }
            }
        }
    }

    private void Item_NodeMove(bool isMonster, ItemData Item, int destinationIndex, int currentNodeIndex)
    {
        AbnormalStatus abnormalStatus = new AbnormalStatus();
        abnormalStatus.id = Item.id;
        abnormalStatus.currentStatus = Item.tool.grantStatus;
        abnormalStatus.duration = Item.tool.duration;
        abnormalStatus.value = Item.tool.value;

        if(isMonster == false)
        {
            if(IngameManager.instance.CheckWalkableNode(destinationIndex) == false)
            {
                IngameManager.instance.UpdatePopup("이동이 불가능 합니다.");

                return;
            }

            int before = IngameManager.instance.saveData.userData.data.currentNodeIndex;
            IngameManager.instance.saveData.mapData.nodeDatas[before].isUser = false;

            IngameManager.instance.saveData.userData.data.currentNodeIndex = destinationIndex;
            IngameManager.instance.saveData.mapData.nodeDatas[destinationIndex].isUser = true;
        }
        else if(isMonster == true)
        {
            if(IngameManager.instance.CheckWalkableNode(destinationIndex) == false)
            {
                return;
            }

            IngameManager.instance.saveData.mapData.nodeDatas[currentNodeIndex].isMonster = false;
            IngameManager.instance.saveData.mapData.nodeDatas[destinationIndex].isMonster = true;
            IngameManager.instance.saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == currentNodeIndex).currentNodeIndex = destinationIndex;
        }
    }

    #endregion

    #region Effect

    private void RemoveEffect(ref CreatureData data, Duration duration)
    {
        switch (duration.stats)
        {
            case eStats.HP:
                Remove(duration, ref data.stats.hp.current, ref data.stats.hp.percent);
                break;

            case eStats.MP:
                Remove(duration, ref data.stats.mp.current, ref data.stats.mp.percent);
                break;

            case eStats.AP:
                Remove(duration, ref data.stats.ap.current, ref data.stats.ap.percent);
                break;

            case eStats.EXP:
                Remove(duration, ref data.stats.exp.current, ref data.stats.exp.percent);
                IngameManager.instance.GetExp(0);
                break;

            case eStats.Coin:
                Remove(duration, ref data.stats.coin.current, ref data.stats.coin.percent);
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
                Apply(duration, ref data.stats.hp.current, ref data.stats.hp.percent);
                break;

            case eStats.MP:
                Apply(duration, ref data.stats.mp.current, ref data.stats.mp.percent);
                break;

            case eStats.AP:
                Apply(duration, ref data.stats.ap.current, ref data.stats.ap.percent);
                break;

            case eStats.EXP:
                Apply(duration, ref data.stats.exp.current, ref data.stats.exp.percent);
                IngameManager.instance.GetExp(0);
                break;

            case eStats.Coin:
                Apply(duration, ref data.stats.coin.current, ref data.stats.coin.percent);
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
        newData.maintain = tool.maintain;
        newData.duration = tool.duration;
        newData.value = tool.value;

        data.abnormalStatuses.Add(newData);
    }

    private void AbnormalStatusesDuration(ref CreatureData data)
    {
        for (int i = data.abnormalStatuses.Count - 1; i >= 0; i--)
        {
            data.abnormalStatuses[i].duration -= 1;

            if (data.abnormalStatuses[i].duration <= 0)
            {
                data.abnormalStatuses.Remove(data.abnormalStatuses[i]);

                continue;
            }
        }
    }
}
