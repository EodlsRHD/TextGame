using System;
using System.Collections;
using System.Collections.Generic;
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
            IngameManager.instance.UpdateText("--- 살아있는 몬스터가 없습니다. 순서를 건너뜁니다.");
            IngameManager.instance.PlayerTurn();

            yield break;
        }

        IngameManager.instance.UpdateText("--- 몬스터의 순서입니다.");

        int attackMonsterIndex = 0;
        bool isAttack = false;

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
            IngameManager.instance.Attack(IngameManager.instance.saveData.mapData.monsterDatas[attackMonsterIndex].currentNodeIndex, () =>
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

        int range = IngameManager.instance.saveData.mapData.monsterDatas[m].attackRange;
        for (int i = -range; i <= range; i++)
        {
            dx.Add(i);
            dy.Add(i);
        }

        bool isAttack = false;
        List<int> NearbyIndexs = IngameManager.instance.GetNearbyBlocks(IngameManager.instance.saveData.mapData.monsterDatas[m].currentNodeIndex);

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

        bool isFindPlayer = false;
        List<int> NearbyIndexs = IngameManager.instance.Vision(IngameManager.instance.saveData.mapData.monsterDatas[m].vision, IngameManager.instance.saveData.mapData.monsterDatas[m].currentNodeIndex);

        for (int i = 0; i < NearbyIndexs.Count; i++)
        {
            if (IngameManager.instance.saveData.mapData.nodeDatas[NearbyIndexs[i]].isWalkable == false)
            {
                continue;
            }

            if (IngameManager.instance.saveData.mapData.nodeDatas[NearbyIndexs[i]].isMonster == true)
            {
                continue;
            }

            if (IngameManager.instance.saveData.mapData.nodeDatas[NearbyIndexs[i]].isShop == true)
            {
                continue;
            }

            if (IngameManager.instance.saveData.userData.data.currentNodeIndex == NearbyIndexs[i])
            {
                isFindPlayer = true;

                break;
            }
        }

        if (isFindPlayer == true)
        {
            MonsterTargetPlayer(m);
            return;
        }

        List<int> nearbyBlocks = IngameManager.instance.GetNearbyBlocks(IngameManager.instance.saveData.mapData.monsterDatas[m].currentNodeIndex);

        if (nearbyBlocks.Count == 0)
        {
            return;
        }

        MonsterSelectMoveBlock(m, ap, ref nearbyBlocks);
    }

    public void MonsterDead(DataManager.Creature_Data monster)
    {
        IngameManager.instance.saveData.mapData.nodeDatas[monster.currentNodeIndex].isMonster = false;
        IngameManager.instance.saveData.mapData.monsterDatas.Remove(IngameManager.instance.saveData.mapData.monsterDatas.Find(x => x.id == monster.id));
        IngameManager.instance.UpdateData();
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
        IngameManager.instance.saveData.mapData.nodeDatas[IngameManager.instance.saveData.mapData.monsterDatas[m].currentNodeIndex].isMonster = false;
        IngameManager.instance.saveData.mapData.nodeDatas[nearbyBlocks[randomIndex]].isMonster = true;

        IngameManager.instance.saveData.mapData.monsterDatas[m].currentNodeIndex = nearbyBlocks[randomIndex];
        ap -= 1;

        MonsterSelectMoveBlock(m, ap, ref nearbyBlocks);
    }

    private void MonsterTargetPlayer(int m)
    {
        DataManager.Creature_Data player = IngameManager.instance.saveData.userData.data;
        int result = IngameManager.instance.PathFinding(ref IngameManager.instance.saveData.mapData, IngameManager.instance.saveData.mapData.monsterDatas[m].currentNodeIndex, player.currentNodeIndex);
        IngameManager.instance.saveData.mapData.nodeDatas[IngameManager.instance.saveData.mapData.monsterDatas[m].currentNodeIndex].isMonster = false;
        IngameManager.instance.saveData.mapData.nodeDatas[result].isMonster = true;

        IngameManager.instance.saveData.mapData.monsterDatas[m].currentNodeIndex = result;
    }
}
