using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
    private List<Monster> _monsters = new List<Monster>();

    private bool _isAttack = false;
    private bool _isAllMonsterDead = false;

    public bool isAllMonsterDead
    {
        get { return _isAllMonsterDead; }
        set { _isAllMonsterDead = value; }
    }

    public void Initialize()
    {
        this.gameObject.SetActive(true);
    }

    public void CreateMonster(List<CreatureData> monsters)
    {
        _monsters.Clear();

        if(monsters.Count == 0)
        {
            _isAllMonsterDead = true;

            return;
        }

        for(int i = 0; i < monsters.Count; i++)
        {
            Monster monster = new Monster();
            monster.Initialize(monsters[i], Attack, Move, Skill);

            _monsters.Add(monster);
        }
    }

    public IEnumerator MonsterTurn()
    {
        if(_isAllMonsterDead == true)
        {
            IngameManager.instance.UpdateText("--- 몬스터의 순서를 건너뜁니다.");
            IngameManager.instance.PlayerTurn();

            yield break;
        }

        IngameManager.instance.UpdateText("--- 몬스터의 순서입니다.");

        for(int i = 0; i < _monsters.Count; i++)
        {
            yield return new WaitForSeconds(0.5f);

            _monsters[i].Action();
        }
    }

    private void Attack(int id)
    {
        if(_isAttack == true)
        {
            return;
        }

        _isAttack = true;
        IngameManager.instance.Attack(true, IngameManager.instance.saveData.mapData.monsterDatas[id].currentNodeIndex, () =>
        {
            _isAttack = false;
            MonsterTurnOut();
        });
    }

    private void Move(int id, int nodeIndex)
    {
        IngameManager.instance.saveData.mapData.nodeDatas[IngameManager.instance.saveData.mapData.monsterDatas[id].currentNodeIndex].isMonster = false;
        IngameManager.instance.saveData.mapData.nodeDatas[nodeIndex].isMonster = true;
        IngameManager.instance.saveData.mapData.monsterDatas[id].currentNodeIndex = nodeIndex;

        if(id == (_monsters.Count - 1))
        {
            if(_isAttack == true)
            {
                return;
            }

            MonsterTurnOut();
        }
    }

    private void Skill(int id, int skillId)
    {
        IngameManager.instance.MonsterSkill(id, skillId);

        if(id == (_monsters.Count - 1))
        {
            if(_isAttack == true)
            {
                return;
            }

            MonsterTurnOut();
        }
    }

    private void MonsterTurnOut()
    {
        IngameManager.instance.MonsterTurnOut();
    }

    public void UpdateData(List<CreatureData> monsters)
    {
        if(_isAllMonsterDead == true)
        {
            return;
        }

        if(monsters.Count == 0)
        {
            _isAllMonsterDead = true;
        }

        bool check = true;
        for(int i = 0; i < _monsters.Count; i++)
        {
            _monsters[i].UpdateData(monsters[i]);

            if(check == false)
            {
                continue;
            }

            check = monsters[i].isDead;
        }

        _isAllMonsterDead = check;
    }

    public void SplitMonster()
    {
        for(int i = IngameManager.instance.saveData.mapData.monsterDatas.Count - 1; i >= 0; i--)
        {
            if(IngameManager.instance.saveData.mapData.monsterDatas[i].defultStatus != eStrengtheningTool.split)
            {
                continue;
            }

            if(IngameManager.instance.saveData.mapData.monsterDatas[i].stats.hp.maximum * 0.5f < IngameManager.instance.saveData.mapData.monsterDatas[i].stats.hp.current)
            {
                continue;
            }

            IngameManager.instance.saveData.mapData.monsterDatas[i].defultStatus = eStrengtheningTool.Non;

            CreatureData newCreature = IngameManager.instance.saveData.mapData.monsterDatas[i].DeepCopy();
            newCreature.id = (short)IngameManager.instance.saveData.mapData.monsterDatas.Count;
            newCreature.stats = new CreatureStats();
            newCreature.stats.coin = new CreatureStat((short)(IngameManager.instance.saveData.mapData.monsterDatas[i].stats.coin.defult * 0.5f), 1, 0, 0);
            newCreature.stats.hp = new CreatureStat((short)(IngameManager.instance.saveData.mapData.monsterDatas[i].stats.hp.current), 1, 0, 0);
            newCreature.stats.mp = new CreatureStat((short)(IngameManager.instance.saveData.mapData.monsterDatas[i].stats.mp.defult), 1, 0, 0);
            newCreature.stats.ap = new CreatureStat((short)(IngameManager.instance.saveData.mapData.monsterDatas[i].stats.ap.defult * 0.5f), 1, 0, 0);
            newCreature.stats.exp = new CreatureStat((short)(IngameManager.instance.saveData.mapData.monsterDatas[i].stats.exp.defult * 0.5f), 1, 0, 0);
            newCreature.stats.attack = new CreatureStat((short)(IngameManager.instance.saveData.mapData.monsterDatas[i].stats.attack.defult * 0.5f), 1, 0, 0);
            newCreature.stats.defence = new CreatureStat((short)(IngameManager.instance.saveData.mapData.monsterDatas[i].stats.defence.defult * 0.5f), 1, 0, 0);
            newCreature.stats.vision = new CreatureStat((short)(IngameManager.instance.saveData.mapData.monsterDatas[i].stats.vision.defult * 0.5f), 1, 0, 0);
            newCreature.stats.attackRange = new CreatureStat((short)(IngameManager.instance.saveData.mapData.monsterDatas[i].stats.attackRange.defult * 0.5f), 1, 0, 0);
            List<int> indexs = IngameManager.instance.GetNearbyNodes_NonDiagonal(IngameManager.instance.saveData.mapData.monsterDatas[i].currentNodeIndex);

            for(int j = 0; j < indexs.Count; j++)
            {
                if(IngameManager.instance.CheckWalkableNode(indexs[j]) == true)
                {
                    newCreature.currentNodeIndex = indexs[j];
                    break;
                }
            }

            IngameManager.instance.saveData.mapData.nodeDatas[newCreature.currentNodeIndex].isMonster = true;
            IngameManager.instance.saveData.mapData.monsterDatas.Add(newCreature);

            IngameManager.instance.UpdateText(newCreature.name + "(이)가 분열했습니다.");
        }

        IngameManager.instance.UpdateData();
    }

    public void HardnessMonster()
    {
        for(int i = IngameManager.instance.saveData.mapData.monsterDatas.Count - 1; i >= 0; i--)
        {
            if(IngameManager.instance.saveData.mapData.monsterDatas[i].defultStatus != eStrengtheningTool.Hardness)
            {
                continue;
            }

            if(IngameManager.instance.saveData.mapData.monsterDatas[i].stats.hp.maximum == IngameManager.instance.saveData.mapData.monsterDatas[i].stats.hp.current)
            {
                continue;
            }

            int minusHp = IngameManager.instance.saveData.mapData.monsterDatas[i].stats.hp.maximum - IngameManager.instance.saveData.mapData.monsterDatas[i].stats.hp.current;
            IngameManager.instance.saveData.mapData.monsterDatas[i].stats.attack.plus += (short)(minusHp * 0.3f);

            IngameManager.instance.UpdateText(IngameManager.instance.saveData.mapData.monsterDatas[i].name + "(이)가 강성으로 인해 강해졌습니다.");
        }
    }
}
