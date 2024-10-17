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

        IngameManager.instance.saveData.userData.stats.coin.MinusCurrnet(price);
        IngameManager.instance.saveData.userData.data.itemIndexs.Add((short)index);

        GameManager.instance.dataManager.AddEncyclopedia_Item(index);
    }

    public void SelectSkill(int currentIndex, int getIndex, int removeIndex)
    {
        IngameManager.instance.saveData.mapData.npcDatas.Find(x => x.currentNodeIndex == currentIndex).isUseBonfire = true;
        IngameManager.instance.UpdateData();

        IngameManager.instance.UpdateText("모닥불의 불꽃이 사그라들고 있다.");

        if (getIndex == 0 && removeIndex == 0)
        {
            IngameManager.instance.UpdateText("체력과 마나가 회복되었습니다.");

            short hp = (short)(IngameManager.instance.saveData.userData.stats.hp.maximum * 0.35f);
            short mp = (short)(IngameManager.instance.saveData.userData.stats.mp.maximum * 0.35f);

            IngameManager.instance.saveData.userData.stats.hp.PlusCurrent(hp);
            IngameManager.instance.saveData.userData.stats.mp.PlusCurrent(hp);

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

    private int GetPlayerDamage(int battleDamage, UserData data)
    {
        return data.stats.attack.currnet + battleDamage + (int)(data.stats.attack.currnet * (0.1f * data.stats.attack.percent)) + data.stats.attack.plus;
    }

    private int GetPlayerDefence(UserData data)
    {
        return data.stats.defence.currnet + (int)(data.stats.defence.currnet * (0.1f * data.stats.defence.percent)) + data.stats.defence.plus;
    }

    private int GetMonsterDamage(int battleDamage, CreatureData data)
    {
        return (int)(battleDamage + ((battleDamage * 0.1f) * data.stats.attack.currnet));
    }

    private int GetMonsterDefence(CreatureData data)
    {
        return data.stats.defence.currnet;
    }
    
    public void Attack(int nodeMonsterIndex, System.Action onLastCallback = null)
    {
        CreatureData monster = IngameManager.instance.saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == nodeMonsterIndex);
        GameManager.instance.dataManager.AddEncyclopedia_Creature(monster.id);

        IngameManager.instance.isHuntMonster = true;

        IngameManager.instance.CallAttacker(monster, onLastCallback, (result, damage) =>
        {
            IngameManager.instance.PlayBgm(eBgm.Ingame);

            if (result == eWinorLose.Lose)
            {
                IngameManager.instance.UpdateText("--- " + monster.name + " (이)가 승리했습니다.");

                int monsterDamage = GetMonsterDamage(damage, monster);
                int playerDef = GetPlayerDefence(IngameManager.instance.saveData.userData);

                if (IngameManager.instance.CheckAbnormalStatusEffect(eStrengtheningTool.Hardness, monster) == true)
                {
                    short value = IngameManager.instance.GetValueAbnormalStatusEffect(eStrengtheningTool.Hardness, monster);
                    short hardnessValue = (short)(monsterDamage * (0.01f * value));
                    monsterDamage += hardnessValue;

                    IngameManager.instance.UpdateText("강성으로 인해 " + hardnessValue + "만큼 피해가 추가로 더해졌습니다.");
                }

                if (IngameManager.instance.CheckAbnormalStatusEffect(eStrengtheningTool.ReductionHalf, IngameManager.instance.saveData.userData.data) == true)
                {
                    short reductionHalfValue = (short)(monsterDamage * 0.5f);
                    monsterDamage = reductionHalfValue;

                    IngameManager.instance.UpdateText("반감으로 인해 피해가 반감되었습니다.");
                }

                int resultDamage = (playerDef - monsterDamage);

                if (resultDamage >= 0)
                {
                    GameManager.instance.soundManager.PlaySfx(eSfx.Blocked);
                    IngameManager.instance.UpdateText("--- " + monster.name + " 의 공격이 방어도에 막혔습니다.");

                    return;
                }

                resultDamage = Mathf.Abs(resultDamage);

                if(IngameManager.instance.PlayerHit(resultDamage) == true)
                {
                    return;
                }

                if (IngameManager.instance.CheckAbnormalStatusEffect(eStrengtheningTool.BloodSucking, monster) == true)
                {
                    short value = IngameManager.instance.GetValueAbnormalStatusEffect(eStrengtheningTool.BloodSucking, monster);
                    short BloodSuckingValue = (short)(resultDamage * (0.01f * value));

                    monster.stats.hp.PlusCurrent(BloodSuckingValue);
                    IngameManager.instance.UpdateText("흡혈로 인해 " + BloodSuckingValue + "만큼 회복했습니다.");
                }

                IngameManager.instance.UpdateText("--- " + resultDamage + " 의 피해를 입었습니다.");
                IngameManager.instance.UpdateData();

                return;
            }

            if (result == eWinorLose.Draw)
            {
                IngameManager.instance.UpdateText("--- 무승부입니다.");

                IngameManager.instance.UpdateData();

                return;
            }

            #region Win

            IngameManager.instance.UpdateText("--- " + IngameManager.instance.saveData.userData.data.name + " (이)가 승리했습니다.");

            int playerDamage = GetPlayerDamage(damage, IngameManager.instance.saveData.userData);

            IngameManager.instance.UpdateText("--- " + IngameManager.instance.saveData.userData.data.name + " (이)가 " + playerDamage + " 의 공격력으로 공격합니다.");

            if (IngameManager.instance.CheckAbnormalStatusEffect(eStrengtheningTool.Hardness, IngameManager.instance.saveData.userData.data) == true)
            {
                short value = IngameManager.instance.GetValueAbnormalStatusEffect(eStrengtheningTool.Hardness, IngameManager.instance.saveData.userData.data);
                short hardnessValue = (short)(playerDamage * (0.01f * value));
                playerDamage += hardnessValue;

                IngameManager.instance.UpdateText("강성으로 인해 " + hardnessValue + "만큼 피해가 추가로 더해졌습니다.");
            }

            if (IngameManager.instance.CheckAbnormalStatusEffect(eStrengtheningTool.ReductionHalf, monster) == true)
            {
                short reductionHalfValue = (short)(playerDamage * 0.5f);
                playerDamage = reductionHalfValue;

                IngameManager.instance.UpdateText("반감으로 인해 피해가 반감되었습니다.");
            }

            int Damage = GetMonsterDefence(monster) - playerDamage;

            if (Damage >= 0)
            {
                IngameManager.instance.UpdateText("--- " + IngameManager.instance.saveData.userData.data.name + " 의 공격이 방어도에 막혔습니다.");
                GameManager.instance.soundManager.PlaySfx(eSfx.Blocked);

                return;
            }

            Damage = Mathf.Abs(Damage);

            IngameManager.instance.UpdateText("방어도를 제외한 " + damage + " 의 데미지를 가했습니다.");
            IngameManager.instance.MonsterHit(nodeMonsterIndex, Damage);

            if (IngameManager.instance.CheckAbnormalStatusEffect(eStrengtheningTool.BloodSucking, IngameManager.instance.saveData.userData.data) == true)
            {
                short value = IngameManager.instance.GetValueAbnormalStatusEffect(eStrengtheningTool.BloodSucking, IngameManager.instance.saveData.userData.data);
                short BloodSuckingValue = (short)(damage * (0.01f * value));

                IngameManager.instance.saveData.userData.stats.hp.PlusCurrent(BloodSuckingValue);
                IngameManager.instance.UpdateText("흡혈로 인해 " + BloodSuckingValue + "만큼 회복했습니다.");
            }

            #endregion

            IngameManager.instance.UpdateData();
        });
    }

    public void Defence()
    {
        short ap = IngameManager.instance.saveData.userData.stats.ap.currnet;

        Duration duration = new Duration();
        duration.id = 0;
        duration.name = "일시적 방어도 상승";
        duration.remaindDuration = 0;
        duration.stats = eStats.Defence;
        duration.value = ap;

        IngameManager.instance.saveData.userData.stats.ap.currnet = 0;
        IngameManager.instance.PlayerDefence(duration);

        IngameManager.instance.UpdateText("행동력을 모드 소진하였습니다.");
        IngameManager.instance.UpdateText("방어도가 " + ap + "만큼 증가했습니다.");
    }
}
