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
                IngameManager.instance.UpdateText("--- ��ں��� �Ҳ��� ��׶������ϴ�.");
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
                IngameManager.instance.UpdateText("@���� : ������ �� ��������� ������ �鷯�ּ���!");

                return;
            }
            else
            {
                IngameManager.instance.UpdateText("@���� : �ȳ��ϼ���! ���� ���� �����غ�����!");

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

        IngameManager.instance.UpdateText("@���� : �������ּż� ������");
        IngameManager.instance.UpdateText("���濡 �����Ǿ����ϴ�.");

        IngameManager.instance.saveData.userData.data.coin -= price;
        IngameManager.instance.saveData.userData.data.itemIndexs.Add((short)index);

        GameManager.instance.dataManager.AddEncyclopedia_Item(index);
    }

    public void SelectSkill(int currentIndex, int getIndex, int removeIndex)
    {
        IngameManager.instance.saveData.mapData.npcDatas.Find(x => x.currentNodeIndex == currentIndex).isUseBonfire = true;
        IngameManager.instance.saveData.mapData.nodeDatas[currentIndex].isUseBonfire = true;
        IngameManager.instance.UpdateData();

        IngameManager.instance.UpdateText("��ں��� �Ҳ��� ��׶��� �ִ�.");

        if (getIndex == 0 && removeIndex == 0)
        {
            IngameManager.instance.UpdateText("ü�°� ������ ȸ���Ǿ����ϴ�.");

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
                IngameManager.instance.UpdateText("--- " + monster.name + " (��)�� �¸��߽��ϴ�.");

                int monsterAttack = GetMonsterDamage(damage, monster);
                int playerDef = GetPlayerDefence(IngameManager.instance.saveData.userData);
                int resultDamage = (playerDef - monsterAttack);

                if (resultDamage >= 0)
                {
                    GameManager.instance.soundManager.PlaySfx(eSfx.Blocked);
                    IngameManager.instance.UpdateText("--- " + monster.name + " �� ������ ���� �������ϴ�.");

                    return;
                }

                IngameManager.instance.saveData.userData.data.currentHP -= (short)Mathf.Abs(resultDamage);

                HitSfx(resultDamage, IngameManager.instance.saveData.userData.data.currentHP / 3);

                IngameManager.instance.UpdateText("--- " + Mathf.Abs(resultDamage) + " �� ���ظ� �Ծ����ϴ�.");

                if (IngameManager.instance.saveData.userData.data.currentHP <= 0)
                {
                    IngameManager.instance.saveData.userData.data.currentHP = 0;

                    GameManager.instance.soundManager.PlaySfx(eSfx.RoundFail);
                    IngameManager.instance.UpdateData(Mathf.Abs(resultDamage) + " �� ���ظ� �Ծ� �й��Ͽ����ϴ�.");

                    return;
                }

                IngameManager.instance.UpdateData();

                return;
            }

            if (result == eWinorLose.Draw)
            {
                IngameManager.instance.UpdateText("--- ���º��Դϴ�.");

                IngameManager.instance.UpdateData();

                return;
            }

            IngameManager.instance.UpdateText("--- " + IngameManager.instance.saveData.userData.data.name + " (��)�� �¸��߽��ϴ�.");

            int playerDamage = GetPlayerDamage(damage, IngameManager.instance.saveData.userData);
            int _damage = GetMonsterDefence(monster) - playerDamage;

            if (_damage >= 0)
            {
                IngameManager.instance.UpdateText("--- " + IngameManager.instance.saveData.userData.data.name + " �� ������ ���� �������ϴ�.");
                GameManager.instance.soundManager.PlaySfx(eSfx.Blocked);

                return;
            }

            GameManager.instance.soundManager.PlaySfx(eSfx.Attack);

            IngameManager.instance.UpdateText("--- " + IngameManager.instance.saveData.userData.data.name + " (��)�� " + playerDamage + " �� ���ݷ����� �����մϴ�.");
            IngameManager.instance.UpdateText("���� ������ " + Mathf.Abs(_damage) + " �� �������� ���߽��ϴ�.");

            monster.hp -= (short)Mathf.Abs(_damage);

            if (monster.hp == 0)
            {
                IngameManager.instance.saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == nodeMonsterIndex).hp = 0;

                IngameManager.instance.UpdateText(monster.name + " (��)�� óġ�Ͽ����ϴ�");
                IngameManager.instance.UpdateText("--- ����ġ " + monster.exp + " , ���� " + monster.coin + "�� ȹ���߽��ϴ�");

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
                IngameManager.instance.UpdateText(monster.name + "�� ü���� " + monster.hp + " ��ŭ ���ҽ��ϴ�.");

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
            IngameManager.instance.UpdateText("--- �����ִ� �ൿ���� �����ϴ�.");

            return;
        }

        short ap = IngameManager.instance.saveData.userData.data.currentAP;
        IngameManager.instance.saveData.userData.data.currentDEFENCE += ap;
        IngameManager.instance.saveData.userData.data.currentAP = 0;

        IngameManager.instance.UpdateText("�ൿ���� ��� �����Ͽ����ϴ�.");
        IngameManager.instance.UpdateText("���� " + ap + "��ŭ �����߽��ϴ�.");

        IngameManager.instance.UpdatePlayerInfo(eStats.AP);
        IngameManager.instance.UpdatePlayerInfo(eStats.Defence);
    }
}
