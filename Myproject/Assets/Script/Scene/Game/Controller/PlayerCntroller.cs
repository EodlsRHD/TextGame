using DG.Tweening;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCntroller : MonoBehaviour
{
    public void Initialize()
    {
        this.gameObject.SetActive(true);
    }

    public void PlayerMove(eControl type)
    {
        if (IngameManager.instance.isPlayerTurn == false)
        {
            IngameManager.instance.UpdatePopup("������ �ƴմϴ�.");

            return;
        }

        if (IngameManager.instance.saveData.userData.stats.ap.current == 0)
        {
            IngameManager.instance.UpdatePopup("�ൿ���� �����մϴ�");

            return;
        }

        if(IngameManager.instance.CheckAbnormalStatusEffect(eStrengtheningTool.UnableAct, IngameManager.instance.saveData.userData.data) == true)
        {
            IngameManager.instance.UpdateText("�ൿ �Ҵ� �����Դϴ�.");

            return;
        }

        int nearbyBlockIndex = PlayerMoveType(type, IngameManager.instance.saveData.userData.data.currentNodeIndex);

        if (nearbyBlockIndex == -1)
        {
            return;
        }

        IngameManager.instance.UpdateText(nearbyBlockIndex);

        IngameManager.instance.saveData.mapData.nodeDatas[IngameManager.instance.saveData.userData.data.currentNodeIndex].isUser = false;
        IngameManager.instance.saveData.mapData.nodeDatas[nearbyBlockIndex].isUser = true;

        IngameManager.instance.saveData.userData.data.currentNodeIndex = nearbyBlockIndex;

        IngameManager.instance.saveData.userData.stats.ap.MinusCurrnet(1);
        IngameManager.instance.UpdatePlayerInfo(eStats.AP);

        IngameManager.instance.UpdateData();
    }

    private int PlayerMoveType(eControl type, int currentIndex)
    {
        int x = 0;
        int y = 0;

        switch (type)
        {
            case eControl.Up:
                x = 0;
                y = 1;
                break;

            case eControl.Left:
                x = -1;
                y = 0;
                break;

            case eControl.Right:
                x = 1;
                y = 0;
                break;

            case eControl.Down:
                x = 0;
                y = -1;
                break;
        }

        int result = IngameManager.instance.GetNearbyNodes(x, y, currentIndex);

        if (result == -1)
        {
            IngameManager.instance.UpdateText("�̵��� �� �����ϴ�.");

            return result;
        }

        if (IngameManager.instance.saveData.mapData.nodeDatas[result].isWalkable == false)
        {
            IngameManager.instance.UpdateText("�̵��� �� �����ϴ�.");

            return -1;
        }

        if (IngameManager.instance.saveData.mapData.nodeDatas[result].isItem == true)
        {
            IngameManager.instance.GetFieldItem(result);

            return -1;
        }

        if (IngameManager.instance.saveData.mapData.nodeDatas[result].isMonster == true)
        {
            if(IngameManager.instance.saveData.userData.stats.ap.current < 2)
            {
                IngameManager.instance.UpdatePopup("�ൿ���� �����մϴ�");
                return -1;
            }

            IngameManager.instance.saveData.userData.stats.ap.MinusCurrnet(2);
            IngameManager.instance.UpdatePlayerInfo(eStats.AP);

            IngameManager.instance.Attack(false, result);

            return -1;
        }

        if (IngameManager.instance.saveData.mapData.nodeDatas[result].isShop == true || IngameManager.instance.saveData.mapData.nodeDatas[result].isBonfire == true)
        {
            if (IngameManager.instance.saveData.userData.stats.ap.current < 1)
            {
                IngameManager.instance.UpdatePopup("�ൿ���� �����մϴ�");

                return -1;
            }

            IngameManager.instance.saveData.userData.stats.ap.MinusCurrnet(1);
            IngameManager.instance.UpdatePlayerInfo(eStats.AP);

            IngameManager.instance.Npc(result);

            return -1;
        }

        if (IngameManager.instance.saveData.mapData.nodeDatas[result].isGuide == true)
        {
            UiManager.instance.OpenTutorial(false);

            return -1;
        }

        if (IngameManager.instance.saveData.mapData.exitNodeIndex == result)
        {
            if (IngameManager.instance.isAllMonsterDead == false)
            {
                IngameManager.instance.UpdatePopup("���� �����ֽ��ϴ�.");

                return -1;
            }

            UiManager.instance.OpenPopup(string.Empty, "���� ����� �Ѿ�ðڽ��ϱ�?", string.Empty, string.Empty, () =>
            {
                GameManager.instance.soundManager.PlaySfx(eSfx.RoundSuccess);
                IngameManager.instance.RoundClear();
            }, null);

            return -1;
        }

        return result;
    }

    public void PlayerAction(eControl type)
    {
        if (IngameManager.instance.isPlayerTurn == false)
        {
            IngameManager.instance.UpdatePopup("������ �ƴմϴ�.");

            return;
        }

        switch (type)
        {
            case eControl.Defence:
                {
                    if (IngameManager.instance.saveData.userData.stats.ap.current == 0)
                    {
                        IngameManager.instance.UpdateText("�����ִ� �ൿ���� �����ϴ�.");

                        break;
                    }

                    UiManager.instance.OpenPopup(string.Empty, "������ �����ðڽ��ϱ�? ���� �ൿ���� ��� �����մϴ�.", "Ȯ��", "���", () =>
                    {
                        IngameManager.instance.Defence();
                    }, null);
                }
                break;

            case eControl.Skill:
                {
                    IngameManager.instance.ControlPad_Skill();
                }
                break;

            case eControl.Bag:
                {
                    IngameManager.instance.ControlPad_Bag();
                }
                break;

            case eControl.Rest:
                {
                    UiManager.instance.OpenPopup(string.Empty, "�޽��Ͻðڽ��ϱ�?", "Ȯ��", "���", () =>
                    {
                        PlayerTurnOut();
                        IngameManager.instance.MonsterTurn();
                    }, null);
                }
                break;

            case eControl.SearchNearby:
                {
                    PlayerSearchNearby();
                }
                break;
        }
    }

    public List<int> PlayerSearchNearby()
    {
        List<Action> actions = new List<Action>();
        List<int> NearbyIndexs = IngameManager.instance.Vision(IngameManager.instance.saveData.userData.stats.vision.current, IngameManager.instance.saveData.userData.data.currentNodeIndex);

        bool Non = false;

        for (int i = 0; i < NearbyIndexs.Count; i++)
        {
            int index = NearbyIndexs[i];

            if (IngameManager.instance.saveData.mapData.exitNodeIndex == index)
            {
                actions.Add(() =>
                {
                    IngameManager.instance.UpdateText(eMapObject.Exit, index);
                });

                Non = true;

                continue;
            }

            if (IngameManager.instance.saveData.mapData.nodeDatas[index].isItem == true)
            {
                actions.Add(() =>
                {
                    IngameManager.instance.UpdateText(eMapObject.Item, index);
                });

                Non = true;

                continue;
            }

            if (IngameManager.instance.saveData.mapData.nodeDatas[index].isShop == true)
            {
                actions.Add(() =>
                {
                    IngameManager.instance.UpdateText(eMapObject.Shop, index);
                });

                Non = true;

                continue;
            }

            if (IngameManager.instance.saveData.mapData.nodeDatas[index].isBonfire == true)
            {
                if (IngameManager.instance.saveData.mapData.npcDatas.Find(x => x.currentNodeIndex == index).isUseBonfire == true)
                {
                    actions.Add(() =>
                    {
                        IngameManager.instance.UpdateText(eMapObject.UseBonfire, index);
                    });

                    Non = true;

                    continue;
                }

                actions.Add(() =>
                {
                    IngameManager.instance.UpdateText(eMapObject.Bonfire, index);
                });

                Non = true;

                continue;
            }

            if (IngameManager.instance.saveData.mapData.nodeDatas[index].isMonster == true)
            {
                actions.Add(() =>
                {
                    CreatureData monster = IngameManager.instance.saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == index);

                    if(monster.defultStatus == eStrengtheningTool.Incubation)
                    {
                        return;
                    }
                    else if(monster.defultStatus == eStrengtheningTool.Camouflage)
                    {
                        IngameManager.instance.UpdateText(eMapObject.Player, index);
                    }
                    else if(monster.defultStatus == eStrengtheningTool.Stealth)
                    {
                        IngameManager.instance.UpdateText(eMapObject.Stealth, index);
                    }
                    else
                    {
                        IngameManager.instance.UpdateText(eMapObject.Monster, index);
                    }
                });

                Non = true;

                continue;
            }

            if (IngameManager.instance.saveData.mapData.nodeDatas[index].isGuide == true)
            {
                actions.Add(() =>
                {
                    IngameManager.instance.UpdateText(eMapObject.Guide, index);
                });

                Non = true;

                continue;
            }
        }

        if (Non == false)
        {
            IngameManager.instance.UpdateText("�ֺ��� �߰ߵȰ� �����ϴ�.");

            return NearbyIndexs;
        }

        IngameManager.instance.UpdateText("�ֺ� �˻��� �����մϴ�.");

        foreach (var action in actions)
        {
            action?.Invoke();
        }

        IngameManager.instance.UpdateText("�ֺ� �˻��� �������ϴ�.");

        return NearbyIndexs;
    }

    private void PlayerTurnOut()
    {
        IngameManager.instance.PlayerTurnOut();
    }
}
