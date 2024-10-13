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
            IngameManager.instance.UpdatePopup(IngameManager.instance.saveData.userData.data.name + "의 순서가 아닙니다.");

            return;
        }

        if (IngameManager.instance.saveData.userData.currentAP == 0)
        {
            IngameManager.instance.UpdatePopup("ap가 부족합니다.");

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
        IngameManager.instance.saveData.userData.currentAP -= 1;

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

        int result = IngameManager.instance.GetNearbyBlocks(x, y, currentIndex);

        if (result == -1)
        {
            IngameManager.instance.UpdateText("이동할 수 없습니다.");

            return result;
        }

        if (IngameManager.instance.saveData.mapData.nodeDatas[result].isWalkable == false)
        {
            IngameManager.instance.UpdateText("이동할 수 없습니다.");

            return -1;
        }

        if (IngameManager.instance.saveData.mapData.nodeDatas[result].isMonster == true)
        {
            if(IngameManager.instance.saveData.userData.currentAP < 2)
            {
                return -1;
            }

            IngameManager.instance.saveData.userData.currentAP -= 2;
            IngameManager.instance.UpdatePlayerInfo(eStats.AP);

            IngameManager.instance.Attack(result);

            return -1;
        }

        if (IngameManager.instance.saveData.mapData.nodeDatas[result].isShop == true || IngameManager.instance.saveData.mapData.nodeDatas[result].isBonfire == true)
        {
            if (IngameManager.instance.saveData.userData.currentAP < 1)
            {
                return -1;
            }

            IngameManager.instance.saveData.userData.currentAP -= 1;
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
                IngameManager.instance.UpdateText("--- 남아있는 몬스터가 있습니다.");

                return -1;
            }

            UiManager.instance.OpenPopup(string.Empty, "다음 라운드로 넘어가시겠습니까?", string.Empty, string.Empty, () =>
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
            IngameManager.instance.UpdateText(IngameManager.instance.saveData.userData.data.name + "의 순서가 아닙니다.");

            return;
        }

        switch (type)
        {
            case eControl.Defence:
                {
                    UiManager.instance.OpenPopup(string.Empty, "방어력을 높히시겠습니까? 남은 행동력을 모두 소진합니다.", "확인", "취소", () =>
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
                    UiManager.instance.OpenPopup(string.Empty, "휴식하시겠습니까?", "확인", "취소", () =>
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
        List<System.Action> actions = new List<System.Action>();
        List<int> NearbyIndexs = IngameManager.instance.Vision(IngameManager.instance.saveData.userData.currentVISION, IngameManager.instance.saveData.userData.data.currentNodeIndex);

        bool Non = false;

        for (int i = 0; i < NearbyIndexs.Count; i++)
        {
            int index = NearbyIndexs[i];

            if (IngameManager.instance.saveData.mapData.exitNodeIndex == index)
            {
                actions.Add(() =>
                {
                    IngameManager.instance.UpdateText(eCreature.Exit, index);
                });

                Non = true;

                continue;
            }

            if (IngameManager.instance.saveData.mapData.nodeDatas[index].isItem == true)
            {
                actions.Add(() =>
                {
                    IngameManager.instance.UpdateText(eCreature.Item, index);
                });

                Non = true;

                continue;
            }

            if (IngameManager.instance.saveData.mapData.nodeDatas[index].isShop == true)
            {
                actions.Add(() =>
                {
                    IngameManager.instance.UpdateText(eCreature.Shop, index);
                });

                Non = true;

                continue;
            }

            if (IngameManager.instance.saveData.mapData.nodeDatas[index].isBonfire == true)
            {
                actions.Add(() =>
                {
                    IngameManager.instance.UpdateText(eCreature.Bonfire, index);
                });

                Non = true;

                continue;
            }

            if (IngameManager.instance.saveData.mapData.nodeDatas[index].isUseBonfire == true)
            {
                actions.Add(() =>
                {
                    IngameManager.instance.UpdateText(eCreature.UseBonfire, index);
                });

                Non = true;

                continue;
            }

            if (IngameManager.instance.saveData.mapData.nodeDatas[index].isMonster == true)
            {
                actions.Add(() =>
                {
                    IngameManager.instance.UpdateText(eCreature.Monster, index);
                });

                Non = true;

                continue;
            }

            if (IngameManager.instance.saveData.mapData.nodeDatas[index].isGuide == true)
            {
                actions.Add(() =>
                {
                    IngameManager.instance.UpdateText(eCreature.Guide, index);
                });

                Non = true;

                continue;
            }
        }

        if (Non == false)
        {
            IngameManager.instance.UpdateText("--- 주변에 발견된게 없습니다.");

            return NearbyIndexs;
        }

        IngameManager.instance.UpdateText("--- 주변 검색을 시작합니다.");

        foreach (var action in actions)
        {
            action?.Invoke();
        }

        IngameManager.instance.UpdateText("--- 주변 검색이 끝났습니다.");

        return NearbyIndexs;
    }

    private void PlayerTurnOut()
    {
        IngameManager.instance.PlayerTurnOut();
    }
}
