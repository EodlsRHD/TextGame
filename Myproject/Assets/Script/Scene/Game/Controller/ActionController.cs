using System.Threading;
using UnityEngine;

public class ActionController : MonoBehaviour
{
    private eDir _dir = eDir.Non;
    private int _nodeIndex = -1;

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
                IngameManager.instance.UpdateText("--- 모닥불의 불꽃이 사그라들었습니다.");
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
                IngameManager.instance.UpdateText("@상인 : 물건이 다 떨어졌어요 다음에 들러주세요!");

                return;
            }
            else
            {
                IngameManager.instance.UpdateText("@상인 : 안녕하세요! 좋은 물건 구경해보세요!");

                IngameManager.instance.ShopOpen(npc);
            }
        }
    }

    public void Buy(int currentIndex, int index, short price)
    {
        if (index < 0)
        {
            return;
        }

        IngameManager.instance.saveData.mapData.npcDatas.Find(x => x.currentNodeIndex == currentIndex).itemIndexs.Remove(IngameManager.instance.saveData.mapData.npcDatas[0].itemIndexs.Find(x => x == index));

        GameManager.instance.soundManager.PlaySfx(eSfx.Coin);

        IngameManager.instance.UpdateText("@상인 : 구매해주셔서 고마워요");
        IngameManager.instance.UpdateText("가방에 보관되었습니다.");

        IngameManager.instance.saveData.userData.data.coin -= price;
        IngameManager.instance.saveData.userData.data.itemIndexs.Add((short)index);

        GameManager.instance.dataManager.AddEncyclopedia_Item(index);
    }

    public void SelectSkill(int currentIndex, int getIndex, int removeIndex)
    {
        IngameManager.instance.saveData.mapData.npcDatas.Find(x => x.currentNodeIndex == currentIndex).isUseBonfire = true;
        IngameManager.instance.saveData.mapData.nodeDatas[currentIndex].isUseBonfire = true;
        IngameManager.instance.UpdateData();

        IngameManager.instance.UpdateText("모닥불의 불꽃이 사그라들고 있다.");

        if (getIndex == 0 && removeIndex == 0)
        {
            IngameManager.instance.UpdateText("체력과 마나가 회복되었습니다.");

            short hp = (short)(IngameManager.instance.saveData.userData.maximumHP * 0.35f);
            short mp = (short)(IngameManager.instance.saveData.userData.maximumMP * 0.35f);

            if (IngameManager.instance.saveData.userData.data.currentHP + hp > IngameManager.instance.saveData.userData.maximumHP)
            {
                IngameManager.instance.saveData.userData.data.currentHP = IngameManager.instance.saveData.userData.maximumHP;
            }
            else
            {
                IngameManager.instance.saveData.userData.data.currentHP += hp;
            }

            if (IngameManager.instance.saveData.userData.data.currentHP + hp > IngameManager.instance.saveData.userData.maximumMP)
            {
                IngameManager.instance.saveData.userData.data.currentHP = IngameManager.instance.saveData.userData.maximumMP;
            }
            else
            {
                IngameManager.instance.saveData.userData.data.currentHP += mp;
            }

            IngameManager.instance.UpdateData();

            return;
        }

        if (removeIndex != 0)
        {
            for (int i = 0; i < IngameManager.instance.saveData.userData.data.skillIndexs.Count; i++)
            {
                if (IngameManager.instance.saveData.userData.data.skillIndexs[i] != removeIndex)
                {
                    continue;
                }

                IngameManager.instance.saveData.userData.data.skillIndexs[i] = (short)getIndex;

                break;
            }
        }
    }

    private int GetPlayerDamage(int battleDamage, DataManager.User_Data data)
    {
        return data.data.currentATTACK + battleDamage + (int)(data.data.currentATTACK * (0.1f * data.data.Attack_Effect_Per)) + data.data.Attack_Effect;
    }

    private int GetPlayerDefence(DataManager.User_Data data)
    {
        return data.data.currentDEFENCE + (int)(data.data.currentDEFENCE * (0.1f * data.data.Defence_Effect_Per)) + data.data.Defence_Effect; ;
    }

    private int GetMonsterDamage(int battleDamage, DataManager.Creature_Data data)
    {
        return (int)(battleDamage + ((battleDamage * 0.1f) * data.attack));
    }

    private int GetMonsterDefence(DataManager.Creature_Data data)
    {
        return data.defence;
    }
    
    private void HitSfx(int damage, int value)
    {
        if (value < (short)Mathf.Abs(damage))
        {
            GameManager.instance.soundManager.PlaySfx(eSfx.Hit_hard);
        }
        else
        {
            GameManager.instance.soundManager.PlaySfx(eSfx.Hit_light);
        }
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
                IngameManager.instance.UpdateText("--- " + monster.name + " (이)가 승리했습니다.");

                int monsterAttack = GetMonsterDamage(damage, monster);
                int playerDef = GetPlayerDefence(IngameManager.instance.saveData.userData);
                int resultDamage = (playerDef - monsterAttack);

                if (resultDamage >= 0)
                {
                    GameManager.instance.soundManager.PlaySfx(eSfx.Blocked);
                    IngameManager.instance.UpdateText("--- " + monster.name + " 의 공격이 방어도에 막혔습니다.");

                    return;
                }

                IngameManager.instance.saveData.userData.data.currentHP -= (short)Mathf.Abs(resultDamage);

                HitSfx(resultDamage, IngameManager.instance.saveData.userData.data.currentHP / 3);

                IngameManager.instance.UpdateText("--- " + Mathf.Abs(resultDamage) + " 의 피해를 입었습니다.");

                if (IngameManager.instance.saveData.userData.data.currentHP <= 0)
                {
                    IngameManager.instance.saveData.userData.data.currentHP = 0;

                    GameManager.instance.soundManager.PlaySfx(eSfx.RoundFail);
                    IngameManager.instance.UpdateData(Mathf.Abs(resultDamage) + " 의 피해를 입어 패배하였습니다.");

                    return;
                }

                IngameManager.instance.UpdateData();

                return;
            }

            if (result == eWinorLose.Draw)
            {
                IngameManager.instance.UpdateText("--- 무승부입니다.");

                IngameManager.instance.UpdateData();

                return;
            }

            IngameManager.instance.UpdateText("--- " + IngameManager.instance.saveData.userData.data.name + " (이)가 승리했습니다.");

            int playerDamage = GetPlayerDamage(damage, IngameManager.instance.saveData.userData);
            int _damage = GetMonsterDefence(monster) - playerDamage;

            if (_damage >= 0)
            {
                IngameManager.instance.UpdateText("--- " + IngameManager.instance.saveData.userData.data.name + " 의 공격이 방어도에 막혔습니다.");
                GameManager.instance.soundManager.PlaySfx(eSfx.Blocked);

                return;
            }

            GameManager.instance.soundManager.PlaySfx(eSfx.Attack);

            IngameManager.instance.UpdateText("--- " + IngameManager.instance.saveData.userData.data.name + " (이)가 " + playerDamage + " 의 공격력으로 공격합니다.");
            IngameManager.instance.UpdateText("방어도를 제외한 " + Mathf.Abs(_damage) + " 의 데미지를 가했습니다.");

            monster.hp -= (short)Mathf.Abs(_damage);

            if (monster.hp == 0)
            {
                IngameManager.instance.saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == nodeMonsterIndex).hp = 0;

                IngameManager.instance.UpdateText(monster.name + " (을)를 처치하였습니다");
                IngameManager.instance.UpdateText("--- 경험치 " + monster.exp + " , 코인 " + monster.coin + "을 획득했습니다");

                if (monster.itemIndexs != null)
                {
                    for (int i = 0; i < monster.itemIndexs.Count; i++)
                    {
                        IngameManager.instance.GetMonsterItem(monster.itemIndexs[i]);
                    }
                }

                IngameManager.instance.GetGold(monster.coin);
                IngameManager.instance.GetExp(monster.exp);

                IngameManager.instance.MonsterDead(monster);
            }
            else
            {
                IngameManager.instance.UpdateText(monster.name + "의 체력이 " + monster.hp + " 만큼 남았습니다.");

                IngameManager.instance.saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == nodeMonsterIndex).hp = monster.hp;
            }

            if (IngameManager.instance.saveData.mapData.monsterDatas.Count == 0)
            {
                GameManager.instance.soundManager.PlaySfx(eSfx.ExitOpen);

                IngameManager.instance.isAllMonsterDead = true;
            }

            IngameManager.instance.UpdateData();
        });
    }

    public void Defence()
    {
        if (IngameManager.instance.saveData.userData.data.currentAP == 0)
        {
            IngameManager.instance.UpdateText("--- 남아있는 행동력이 없습니다.");

            return;
        }

        short ap = IngameManager.instance.saveData.userData.data.currentAP;
        IngameManager.instance.saveData.userData.data.currentDEFENCE += ap;
        IngameManager.instance.saveData.userData.data.currentAP = 0;

        IngameManager.instance.UpdateText("행동력을 모드 소진하였습니다.");
        IngameManager.instance.UpdateText("방어도가 " + ap + "만큼 증가했습니다.");

        IngameManager.instance.UpdatePlayerInfo(eStats.AP);
        IngameManager.instance.UpdatePlayerInfo(eStats.Defence);
    }
}
