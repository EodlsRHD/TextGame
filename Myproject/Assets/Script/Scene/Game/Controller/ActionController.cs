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

        if(getIndex == 0 && removeIndex == 0)
        {
            IngameManager.instance.UpdateText("체력과 마나가 회복되었습니다.");

            short hp = (short)(IngameManager.instance.saveData.userData.stats.hp.maximum * 0.5f);
            short mp = (short)(IngameManager.instance.saveData.userData.stats.mp.maximum * 0.5f);

            IngameManager.instance.saveData.userData.stats.hp.PlusCurrent(hp);
            IngameManager.instance.saveData.userData.stats.mp.PlusCurrent(hp);

            IngameManager.instance.UpdateData();

            return;
        }
        else if(removeIndex > 0)
        {
            for(int i = 0; i < IngameManager.instance.saveData.userData.data.skillIndexs.Count; i++)
            {
                if(IngameManager.instance.saveData.userData.data.skillIndexs[i] != removeIndex)
                {
                    continue;
                }

                IngameManager.instance.saveData.userData.data.skillIndexs[i] = (short)getIndex;

                break;
            }
        }
        else if(removeIndex == -1)
        {
            IngameManager.instance.UpdateText("스킬이 강화되었습니다.");
        }
    }
    
    public void Attack(bool isInitiateMonster, int nodeMonsterIndex, System.Action onLastCallback = null)
    {
        CreatureData monster = IngameManager.instance.saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == nodeMonsterIndex);
        GameManager.instance.dataManager.AddEncyclopedia_Creature(monster.id);

        if(isInitiateMonster == true)
        {
            if (IngameManager.instance.CheckAbnormalStatusEffect(eStrengtheningTool.AttackBlocking, IngameManager.instance.saveData.userData.data) == true)
            {
                IngameManager.instance.RemoveAbnormalStatusEffect(eStrengtheningTool.AttackBlocking, ref IngameManager.instance.saveData.userData.data);
                IngameManager.instance.UpdateText("상대의 공격 시도를 막았습니다.");

                return;
            }
        }
        else if(isInitiateMonster == false)
        {
            if (IngameManager.instance.CheckAbnormalStatusEffect(eStrengtheningTool.AttackBlocking, monster) == true)
            {
                IngameManager.instance.RemoveAbnormalStatusEffect(eStrengtheningTool.AttackBlocking, ref monster);
                IngameManager.instance.UpdateText(monster.name + " 의 효과에 공격 시도가 막혔습니다.");

                return;
            }
        }

        IngameManager.instance.isHuntMonster = true;

        IngameManager.instance.CallAttacker(monster, onLastCallback, (result, damage) =>
        {
            IngameManager.instance.PlayBgm(eBgm.Ingame);

            if (result == eWinorLose.Lose)
            {
                AttackLogic(monster, IngameManager.instance.saveData.userData.data, damage, true, nodeMonsterIndex);
            }
            else if (result == eWinorLose.Draw)
            {
                IngameManager.instance.UpdateText("--- 무승부입니다.");
            }
            else
            {
                AttackLogic(IngameManager.instance.saveData.userData.data, monster, damage, false, nodeMonsterIndex);
            }

            IngameManager.instance.UpdateData();
        });
    }

    public void Defence()
    {
        short ap = IngameManager.instance.saveData.userData.stats.ap.current;

        Duration duration = new Duration();
        duration.id = 0;
        duration.name = "일시적 방어도 상승";
        duration.remaindDuration = 0;
        duration.stats = eStats.Defence;
        duration.value = ap;

        IngameManager.instance.saveData.userData.stats.ap.current = 0;
        IngameManager.instance.PlayerDefence(duration);

        IngameManager.instance.UpdateText("행동력을 모드 소진하였습니다.");
        IngameManager.instance.UpdateText("방어도가 " + ap + "만큼 증가했습니다.");
    }

    private void AttackLogic(CreatureData attacker, CreatureData defencer, int damage, bool attackerIsMonster, int monsterNodeIndex)
    {
        IngameManager.instance.UpdateText("--- " + attacker.name + " (이)가 승리했습니다.");

        int attackerDamage = attacker.stats.attack.maximum;
        int defencerDefence = defencer.stats.defence.maximum;

        IngameManager.instance.UpdateText("--- " + attacker.name + " (이)가 " + attackerDamage + " 의 공격력으로 공격합니다.");

        if (IngameManager.instance.CheckAbnormalStatusEffect(eStrengtheningTool.Hardness, attacker) == true)
        {
            short value = IngameManager.instance.GetValueAbnormalStatusEffect(eStrengtheningTool.Hardness, attacker);
            short hardnessValue = (short)(attackerDamage * (0.01f * value));
            attackerDamage += hardnessValue;

            IngameManager.instance.UpdateText("강성으로 인해 " + hardnessValue + "만큼 피해가 추가로 더해졌습니다.");
        }

        if (IngameManager.instance.CheckAbnormalStatusEffect(eStrengtheningTool.ReductionHalf, defencer) == true)
        {
            short reductionHalfValue = (short)(attackerDamage * 0.5f);
            attackerDamage = reductionHalfValue;

            IngameManager.instance.UpdateText("반감으로 인해 피해가 반감되었습니다.");
        }

        int Damage = defencerDefence - attackerDamage;

        if (Damage >= 0)
        {
            IngameManager.instance.UpdateText("--- " + attacker.name + " 의 공격이 방어도에 막혔습니다.");
            GameManager.instance.soundManager.PlaySfx(eSfx.Blocked);

            return;
        }

        Damage = Mathf.Abs(Damage);

        IngameManager.instance.UpdateText("방어도를 제외한 " + Damage + " 의 데미지를 가했습니다.");
        if(attackerIsMonster == true)
        {
            IngameManager.instance.PlayerHit(Damage);
        }
        else
        {
            IngameManager.instance.MonsterHit(monsterNodeIndex, Damage);
        }

        bool isBloodSucking = false;
        int bloodSuckingValue = 0;

        if (IngameManager.instance.CheckAbnormalStatusEffect(eStrengtheningTool.BloodSucking, attacker) == true)
        {
            isBloodSucking = true;

            short value = IngameManager.instance.GetValueAbnormalStatusEffect(eStrengtheningTool.BloodSucking, attacker);
            short BloodSuckingValue = (short)(damage * value);
            bloodSuckingValue = value;

            if(attackerIsMonster == true)
            {
                IngameManager.instance.saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == monsterNodeIndex).stats.hp.PlusCurrent((short)bloodSuckingValue);
            }
            else
            {
                IngameManager.instance.saveData.userData.stats.hp.PlusCurrent(BloodSuckingValue);
            }

            IngameManager.instance.UpdateText("흡혈로 인해 " + BloodSuckingValue + "만큼 회복했습니다.");
        }

        if (IngameManager.instance.CheckAbnormalStatusEffect(eStrengtheningTool.QuickAttack, attacker) == true)
        {
            short plusValue = IngameManager.instance.GetValueAbnormalStatusEffect(eStrengtheningTool.QuickAttack, attacker);
            short plusDamage = (short)(damage * plusValue);

            IngameManager.instance.UpdateText("연속타격으로 인해 " + plusDamage + "만큼 한번더 공격합니다.");

            int addDamage = defencerDefence - plusDamage;

            if (addDamage >= 0)
            {
                IngameManager.instance.UpdateText("--- " + attacker.name + " 의 공격이 방어도에 막혔습니다.");
                GameManager.instance.soundManager.PlaySfx(eSfx.Blocked);

                return;
            }

            Damage = Mathf.Abs(addDamage);

            IngameManager.instance.UpdateText("방어도를 제외한 " + Damage + " 의 데미지를 가했습니다.");

            if(attackerIsMonster == true)
            {
                IngameManager.instance.PlayerHit(Damage);
            }
            else
            {
                IngameManager.instance.MonsterHit(monsterNodeIndex, Damage);
            }

            if (isBloodSucking == true)
            {
                short BloodSuckingValue = (short)(Damage * bloodSuckingValue);

                if (attackerIsMonster == true)
                {
                    IngameManager.instance.saveData.mapData.monsterDatas.Find(x => x.currentNodeIndex == monsterNodeIndex).stats.hp.PlusCurrent((short)bloodSuckingValue);
                }
                else
                {
                    IngameManager.instance.saveData.userData.stats.hp.PlusCurrent(BloodSuckingValue);
                }

                IngameManager.instance.UpdateText("흡혈로 인해 " + BloodSuckingValue + "만큼 회복했습니다.");
            }
        }
    }
}
