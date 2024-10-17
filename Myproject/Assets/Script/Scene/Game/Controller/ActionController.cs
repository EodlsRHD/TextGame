using GoogleMobileAds.Api;
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

        IngameManager.instance.saveData.userData.stats.coin.MinusCurrnet(price);
        IngameManager.instance.saveData.userData.data.itemIndexs.Add((short)index);

        GameManager.instance.dataManager.AddEncyclopedia_Item(index);
    }

    public void SelectSkill(int currentIndex, int getIndex, int removeIndex)
    {
        IngameManager.instance.saveData.mapData.npcDatas.Find(x => x.currentNodeIndex == currentIndex).isUseBonfire = true;
        IngameManager.instance.UpdateData();

        IngameManager.instance.UpdateText("��ں��� �Ҳ��� ��׶��� �ִ�.");

        if (getIndex == 0 && removeIndex == 0)
        {
            IngameManager.instance.UpdateText("ü�°� ������ ȸ���Ǿ����ϴ�.");

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
                IngameManager.instance.UpdateText("--- " + monster.name + " (��)�� �¸��߽��ϴ�.");

                int monsterDamage = GetMonsterDamage(damage, monster);
                int playerDef = GetPlayerDefence(IngameManager.instance.saveData.userData);

                if (IngameManager.instance.CheckAbnormalStatusEffect(eStrengtheningTool.Hardness, monster) == true)
                {
                    short value = IngameManager.instance.GetValueAbnormalStatusEffect(eStrengtheningTool.Hardness, monster);
                    short hardnessValue = (short)(monsterDamage * (0.01f * value));
                    monsterDamage += hardnessValue;

                    IngameManager.instance.UpdateText("�������� ���� " + hardnessValue + "��ŭ ���ذ� �߰��� ���������ϴ�.");
                }

                if (IngameManager.instance.CheckAbnormalStatusEffect(eStrengtheningTool.ReductionHalf, IngameManager.instance.saveData.userData.data) == true)
                {
                    short reductionHalfValue = (short)(monsterDamage * 0.5f);
                    monsterDamage = reductionHalfValue;

                    IngameManager.instance.UpdateText("�ݰ����� ���� ���ذ� �ݰ��Ǿ����ϴ�.");
                }

                int resultDamage = (playerDef - monsterDamage);

                if (resultDamage >= 0)
                {
                    GameManager.instance.soundManager.PlaySfx(eSfx.Blocked);
                    IngameManager.instance.UpdateText("--- " + monster.name + " �� ������ ���� �������ϴ�.");

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
                    IngameManager.instance.UpdateText("������ ���� " + BloodSuckingValue + "��ŭ ȸ���߽��ϴ�.");
                }

                IngameManager.instance.UpdateText("--- " + resultDamage + " �� ���ظ� �Ծ����ϴ�.");
                IngameManager.instance.UpdateData();

                return;
            }

            if (result == eWinorLose.Draw)
            {
                IngameManager.instance.UpdateText("--- ���º��Դϴ�.");

                IngameManager.instance.UpdateData();

                return;
            }

            #region Win

            IngameManager.instance.UpdateText("--- " + IngameManager.instance.saveData.userData.data.name + " (��)�� �¸��߽��ϴ�.");

            int playerDamage = GetPlayerDamage(damage, IngameManager.instance.saveData.userData);

            IngameManager.instance.UpdateText("--- " + IngameManager.instance.saveData.userData.data.name + " (��)�� " + playerDamage + " �� ���ݷ����� �����մϴ�.");

            if (IngameManager.instance.CheckAbnormalStatusEffect(eStrengtheningTool.Hardness, IngameManager.instance.saveData.userData.data) == true)
            {
                short value = IngameManager.instance.GetValueAbnormalStatusEffect(eStrengtheningTool.Hardness, IngameManager.instance.saveData.userData.data);
                short hardnessValue = (short)(playerDamage * (0.01f * value));
                playerDamage += hardnessValue;

                IngameManager.instance.UpdateText("�������� ���� " + hardnessValue + "��ŭ ���ذ� �߰��� ���������ϴ�.");
            }

            if (IngameManager.instance.CheckAbnormalStatusEffect(eStrengtheningTool.ReductionHalf, monster) == true)
            {
                short reductionHalfValue = (short)(playerDamage * 0.5f);
                playerDamage = reductionHalfValue;

                IngameManager.instance.UpdateText("�ݰ����� ���� ���ذ� �ݰ��Ǿ����ϴ�.");
            }

            int Damage = GetMonsterDefence(monster) - playerDamage;

            if (Damage >= 0)
            {
                IngameManager.instance.UpdateText("--- " + IngameManager.instance.saveData.userData.data.name + " �� ������ ���� �������ϴ�.");
                GameManager.instance.soundManager.PlaySfx(eSfx.Blocked);

                return;
            }

            Damage = Mathf.Abs(Damage);

            IngameManager.instance.UpdateText("���� ������ " + Damage + " �� �������� ���߽��ϴ�.");
            IngameManager.instance.MonsterHit(nodeMonsterIndex, Damage);

            bool isBloodSucking = false;
            int bloodSuckingValue = 0;

            if (IngameManager.instance.CheckAbnormalStatusEffect(eStrengtheningTool.BloodSucking, IngameManager.instance.saveData.userData.data) == true)
            {
                isBloodSucking = true;

                short value = IngameManager.instance.GetValueAbnormalStatusEffect(eStrengtheningTool.BloodSucking, IngameManager.instance.saveData.userData.data);
                short BloodSuckingValue = (short)(damage * value);
                bloodSuckingValue = value;

                IngameManager.instance.saveData.userData.stats.hp.PlusCurrent(BloodSuckingValue);
                IngameManager.instance.UpdateText("������ ���� " + BloodSuckingValue + "��ŭ ȸ���߽��ϴ�.");
            }

            if (IngameManager.instance.CheckAbnormalStatusEffect(eStrengtheningTool.QuickAttack, IngameManager.instance.saveData.userData.data) == true)
            {
                short plusValue = IngameManager.instance.GetValueAbnormalStatusEffect(eStrengtheningTool.QuickAttack, IngameManager.instance.saveData.userData.data);
                short plusDamage = (short)(damage * plusValue);

                IngameManager.instance.saveData.userData.stats.hp.PlusCurrent(plusDamage);
                IngameManager.instance.UpdateText("����Ÿ������ ���� " + plusDamage + "��ŭ �ѹ��� �����մϴ�.");

                int addDamage = GetMonsterDefence(monster) - plusDamage;

                if (addDamage >= 0)
                {
                    IngameManager.instance.UpdateText("--- " + IngameManager.instance.saveData.userData.data.name + " �� ������ ���� �������ϴ�.");
                    GameManager.instance.soundManager.PlaySfx(eSfx.Blocked);

                    return;
                }

                Damage = Mathf.Abs(addDamage);

                IngameManager.instance.UpdateText("���� ������ " + Damage + " �� �������� ���߽��ϴ�.");
                IngameManager.instance.MonsterHit(nodeMonsterIndex, Damage);

                if(isBloodSucking == true)
                {
                    short BloodSuckingValue = (short)(Damage * bloodSuckingValue);

                    IngameManager.instance.saveData.userData.stats.hp.PlusCurrent(BloodSuckingValue);
                    IngameManager.instance.UpdateText("������ ���� " + BloodSuckingValue + "��ŭ ȸ���߽��ϴ�.");
                }
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
        duration.name = "�Ͻ��� �� ���";
        duration.remaindDuration = 0;
        duration.stats = eStats.Defence;
        duration.value = ap;

        IngameManager.instance.saveData.userData.stats.ap.currnet = 0;
        IngameManager.instance.PlayerDefence(duration);

        IngameManager.instance.UpdateText("�ൿ���� ��� �����Ͽ����ϴ�.");
        IngameManager.instance.UpdateText("���� " + ap + "��ŭ �����߽��ϴ�.");
    }

    private void AttackLogic(CreatureData attacker, CreatureData defencer, int damage, bool attackerIsMonster, int monsterNodeIndex)
    {
        IngameManager.instance.UpdateText("--- " + attacker.name + " (��)�� �¸��߽��ϴ�.");

        int attackerDamage = attacker.stats.attack.maximum;
        int defencerDefence = defencer.stats.defence.maximum;

        IngameManager.instance.UpdateText("--- " + attacker.name + " (��)�� " + attackerDamage + " �� ���ݷ����� �����մϴ�.");

        if (IngameManager.instance.CheckAbnormalStatusEffect(eStrengtheningTool.Hardness, attacker) == true)
        {
            short value = IngameManager.instance.GetValueAbnormalStatusEffect(eStrengtheningTool.Hardness, attacker);
            short hardnessValue = (short)(attackerDamage * (0.01f * value));
            attackerDamage += hardnessValue;

            IngameManager.instance.UpdateText("�������� ���� " + hardnessValue + "��ŭ ���ذ� �߰��� ���������ϴ�.");
        }

        if (IngameManager.instance.CheckAbnormalStatusEffect(eStrengtheningTool.ReductionHalf, defencer) == true)
        {
            short reductionHalfValue = (short)(attackerDamage * 0.5f);
            attackerDamage = reductionHalfValue;

            IngameManager.instance.UpdateText("�ݰ����� ���� ���ذ� �ݰ��Ǿ����ϴ�.");
        }

        int Damage = defencerDefence - attackerDamage;

        if (Damage >= 0)
        {
            IngameManager.instance.UpdateText("--- " + attacker.name + " �� ������ ���� �������ϴ�.");
            GameManager.instance.soundManager.PlaySfx(eSfx.Blocked);

            return;
        }

        Damage = Mathf.Abs(Damage);

        IngameManager.instance.UpdateText("���� ������ " + Damage + " �� �������� ���߽��ϴ�.");
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

            IngameManager.instance.UpdateText("������ ���� " + BloodSuckingValue + "��ŭ ȸ���߽��ϴ�.");
        }

        if (IngameManager.instance.CheckAbnormalStatusEffect(eStrengtheningTool.QuickAttack, attacker) == true)
        {
            short plusValue = IngameManager.instance.GetValueAbnormalStatusEffect(eStrengtheningTool.QuickAttack, attacker);
            short plusDamage = (short)(damage * plusValue);

            IngameManager.instance.UpdateText("����Ÿ������ ���� " + plusDamage + "��ŭ �ѹ��� �����մϴ�.");

            int addDamage = defencerDefence - plusDamage;

            if (addDamage >= 0)
            {
                IngameManager.instance.UpdateText("--- " + attacker.name + " �� ������ ���� �������ϴ�.");
                GameManager.instance.soundManager.PlaySfx(eSfx.Blocked);

                return;
            }

            Damage = Mathf.Abs(addDamage);

            IngameManager.instance.UpdateText("���� ������ " + Damage + " �� �������� ���߽��ϴ�.");

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

                IngameManager.instance.UpdateText("������ ���� " + BloodSuckingValue + "��ŭ ȸ���߽��ϴ�.");
            }
        }
    }
}
