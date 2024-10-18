using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MonsterController : MonoBehaviour
{

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

    public IEnumerator MonsterTurn()
    {
        if (_isAllMonsterDead == true)
        {
            IngameManager.instance.UpdateText("--- 몬스터의 순서를 건너뜁니다.");
            IngameManager.instance.PlayerTurn();

            yield break;
        }

        IngameManager.instance.UpdateText("--- 몬스터의 순서입니다.");

        int attackMonsterIndex = 0;
        bool isAttack = false;
        bool isSkill = false;

        for (int m = 0; m < IngameManager.instance.saveData.mapData.monsterDatas.Count; m++)
        {
            yield return new WaitForSeconds(0.5f);

            if (isAttack == false)
            {
                if (MonsterAttack(m) == true)
                {
                    attackMonsterIndex = m;

                    isAttack = true;
                }
            }

            MonsterMove(m, 1);
        }

        if (isAttack == true)
        {
            IngameManager.instance.Attack(true, IngameManager.instance.saveData.mapData.monsterDatas[attackMonsterIndex].currentNodeIndex, () =>
            {
                MonsterTurnOut();
            });

            yield break;
        }

        MonsterTurnOut();
    }

    private void MonsterTurnOut()
    {
        IngameManager.instance.MonsterTurnOut();
    }

    private bool MonsterAttack(int m)
    {
        List<int> dx = new List<int>();
        List<int> dy = new List<int>();

        int range = IngameManager.instance.saveData.mapData.monsterDatas[m].stats.attackRange.current;
        for (int i = -range; i <= range; i++)
        {
            dx.Add(i);
            dy.Add(i);
        }

        bool isAttack = false;
        List<int> NearbyIndexs = IngameManager.instance.GetNearbyNodes_NonDiagonal(IngameManager.instance.saveData.mapData.monsterDatas[m].currentNodeIndex);

        for (int i = 0; i < NearbyIndexs.Count; i++)
        {
            if (IngameManager.instance.saveData.userData.data.currentNodeIndex == NearbyIndexs[i])
            {
                isAttack = true;

                break;
            }
        }

        if (isAttack == true)
        {
            return isAttack;
        }

        return isAttack;
    }

    private void MonsterMove(int m, int ap)
    {
        List<int> visionIndexs = IngameManager.instance.Vision(IngameManager.instance.saveData.mapData.monsterDatas[m].stats.vision.current, IngameManager.instance.saveData.mapData.monsterDatas[m].currentNodeIndex);

        FindPlayer(m, (result) =>
        {
            if(result == true)
            {
                MonsterSkill(m);

                return;
            }

            List<int> nearbyIndexs = IngameManager.instance.GetNearbyNodes_NonDiagonal(IngameManager.instance.saveData.mapData.monsterDatas[m].currentNodeIndex);

            if (nearbyIndexs.Count == 0)
            {
                return;
            }

            MonsterSelectMoveBlock(m, ap, ref nearbyIndexs);
        });
    }

    private void MonsterSkill(int m)
    {
        CreatureData monster = IngameManager.instance.saveData.mapData.monsterDatas[m];

        Debug.LogError("TEST");
        return;

        if (monster.useSkill == false)
        {
            MonsterTargetPlayer(m);

            return;
        }

        FindPlayer(m, (result) =>
        {
            if(result == false)
            {
                MonsterTargetPlayer(m);

                return;
            }

            SkillData skill = GameManager.instance.dataManager.GetskillData(m);

            if(monster.coolDownSkill.Find(x => x.id == skill.id) != null)
            {
                return;
            }

            IngameManager.instance.MonsterSkill(m, skill.id);
        });
    }

    private void MonsterSelectMoveBlock(int m, int ap, ref List<int> nearbyBlocks)
    {
        if (ap <= 0)
        {
            return;
        }

        int randomIndex = UnityEngine.Random.Range(0, nearbyBlocks.Count);

        if (IngameManager.instance.saveData.mapData.nodeDatas[nearbyBlocks[randomIndex]].isUser == true)
        {
            ap -= 1;
            MonsterTargetPlayer(m);

            return;
        }

        if (IngameManager.instance.saveData.mapData.nodeDatas[nearbyBlocks[randomIndex]].isWalkable == false)
        {
            MonsterSelectMoveBlock(m, ap, ref nearbyBlocks);

            return;
        }

        if (IngameManager.instance.saveData.mapData.nodeDatas[nearbyBlocks[randomIndex]].isMonster == true)
        {
            MonsterSelectMoveBlock(m, ap, ref nearbyBlocks);

            return;
        }

        if(IngameManager.instance.saveData.mapData.nodeDatas[nearbyBlocks[randomIndex]].isExit == true)
        {
            MonsterSelectMoveBlock(m, ap, ref nearbyBlocks);

            return;
        }

        IngameManager.instance.saveData.mapData.nodeDatas[IngameManager.instance.saveData.mapData.monsterDatas[m].currentNodeIndex].isMonster = false;
        IngameManager.instance.saveData.mapData.nodeDatas[nearbyBlocks[randomIndex]].isMonster = true;

        IngameManager.instance.saveData.mapData.monsterDatas[m].currentNodeIndex = nearbyBlocks[randomIndex];
        ap -= 1;

        MonsterSelectMoveBlock(m, ap, ref nearbyBlocks);
    }

    private void MonsterTargetPlayer(int m)
    {
        CreatureData player = IngameManager.instance.saveData.userData.data;
        int result = IngameManager.instance.PathFinding(ref IngameManager.instance.saveData.mapData, IngameManager.instance.saveData.mapData.monsterDatas[m].currentNodeIndex, player.currentNodeIndex);

        IngameManager.instance.saveData.mapData.nodeDatas[IngameManager.instance.saveData.mapData.monsterDatas[m].currentNodeIndex].isMonster = false;
        IngameManager.instance.saveData.mapData.nodeDatas[result].isMonster = true;
        IngameManager.instance.saveData.mapData.monsterDatas[m].currentNodeIndex = result;
    }

    private void FindPlayer(int m,Action<bool> onFindPlayerActionCallback)
    {
        CreatureData monster = IngameManager.instance.saveData.mapData.monsterDatas[m];

        bool isFindPlayer = false;
        List<int> visionIndexs = IngameManager.instance.Vision(monster.stats.vision.current, monster.currentNodeIndex);

        for (int i = 0; i < visionIndexs.Count; i++)
        {
            if (IngameManager.instance.saveData.mapData.nodeDatas[visionIndexs[i]].isWalkable == false)
            {
                continue;
            }

            if (IngameManager.instance.saveData.mapData.nodeDatas[visionIndexs[i]].isMonster == true)
            {
                continue;
            }

            if (IngameManager.instance.saveData.mapData.nodeDatas[visionIndexs[i]].isShop == true)
            {
                continue;
            }

            if (IngameManager.instance.saveData.mapData.nodeDatas[visionIndexs[i]].isBonfire == true)
            {
                continue;
            }

            if (IngameManager.instance.saveData.userData.data.currentNodeIndex == visionIndexs[i])
            {
                isFindPlayer = true;

                break;
            }
        }

        onFindPlayerActionCallback?.Invoke(isFindPlayer);
    }
}
