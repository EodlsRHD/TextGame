using UnityEngine;
using System;
using System.Collections.Generic;
using System.Threading;

public class Monster : MonoBehaviour
{
    private CreatureData _data = null;

    private Action<int> _onAttackCallback = null;
    private Action<int, int> _onMoveCallback = null;
    private Action<int, int> _onSkillCallback = null;

    public void Initialize(CreatureData data, Action<int> onAttackCallback, Action<int, int> onMoveCallback, Action<int, int> onSkillCallback)
    {
        if(onAttackCallback != null)
        {
            _onAttackCallback = onAttackCallback;
        }

        if(onMoveCallback != null)
        {
            _onMoveCallback = onMoveCallback;
        }

        if(onSkillCallback != null)
        {
            _onSkillCallback = onSkillCallback;
        }

        _data = data;
    }

    public void UpdateData(CreatureData data)
    {
        _data = data;
    }

    public void Action()
    {
        if(_data.isDead == true)
        {
            return;
        }

        List<int> nearbyIndexs = IngameManager.instance.Vision(_data.stats.vision.current, _data.currentNodeIndex);

        for(int i = 0; i < nearbyIndexs.Count; i++)
        {
            int index = nearbyIndexs[i];

            if(IngameManager.instance.saveData.mapData.nodeDatas[index].isUser == true)
            {
                TargetPlayer();
                return;
            }
        }

        Move();
    }

    private void TargetPlayer()
    {
        if(_data.haveSkill == true)
        {
            Skill();

            return;
        }

        List<int> list =  IngameManager.instance.GetNearbyNodes_NonDiagonal(_data.currentNodeIndex);

        for (int i = 0;i < list.Count;i++) 
        {
            if(IngameManager.instance.saveData.mapData.nodeDatas[list[i]].isUser == true)
            {
                _onAttackCallback?.Invoke(_data.id);

                return;
            }
        }

        int result = IngameManager.instance.PathFinding(_data.currentNodeIndex, IngameManager.instance.saveData.userData.data.currentNodeIndex);
        _onMoveCallback?.Invoke(_data.id, result);
    }

    private void Skill()
    {
        for(int i = 0; i < _data.skillIndexs.Count; i++)
        {
            bool isCool = false;

            for(int j = 0; j < _data.coolDownSkill.Count; j++)
            {
                if(_data.skillIndexs[0] == _data.coolDownSkill[j].id)
                {
                    isCool = true;

                    break;
                }
            }

            if(isCool == true)
            {
                continue;
            }

            _onSkillCallback?.Invoke(_data.id, _data.skillIndexs[i]);

            break;
        }
    }

    private void Move()
    {
        List<int> list = IngameManager.instance.GetNearbyNodes_NonDiagonal(_data.currentNodeIndex);

        for(int i = 0; i < list.Count; i++)
        {
            int index = list[i];

            if(IngameManager.instance.saveData.mapData.nodeDatas[index].isWalkable == false)
            {
                continue;
            }

            if(IngameManager.instance.saveData.mapData.nodeDatas[index].isMonster == true)
            {
                continue;
            }

            if(IngameManager.instance.saveData.mapData.nodeDatas[index].isExit == true)
            {
                continue;
            }

            _onMoveCallback?.Invoke(_data.id, index);

            break;
        }
    }
}
