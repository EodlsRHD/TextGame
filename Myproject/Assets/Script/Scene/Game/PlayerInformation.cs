using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;

public class PlayerInformation : MonoBehaviour
{
    [Header("Horizontal")]
    [SerializeField] private TMP_Text _textLevel = null;
    [SerializeField] private TMP_Text _textHP = null;
    [SerializeField] private TMP_Text _textMP = null;
    [SerializeField] private TMP_Text _textAP = null;
    [SerializeField] private TMP_Text _textEXP = null;

    [Header("Vertical")]
    [SerializeField] private GameObject _objMaker = null;
    [SerializeField] private GameObject _objInformation = null;
    [SerializeField] private TMP_Text _textInformation = null;
    [SerializeField] private TMP_Text _textAbnormalStatus = null;

    public void Initialize()
    {
        _textAbnormalStatus.text = string.Empty;

        _objInformation.SetActive(false);
    }

    public void UpdatePlayerInfo(UserData userData)
    {
        _textLevel.text = userData.level.ToString();
        _textHP.text = userData.stats.hp.current + " / " + userData.stats.hp.maximum;
        _textMP.text = userData.stats.mp.current + " / " + userData.stats.mp.maximum;
        _textAP.text = userData.stats.ap.current + " / " + userData.stats.ap.maximum;
        _textEXP.text = userData.stats.exp.current + " / " + userData.maximumEXP;

        Information(userData);
        Abnormalstatus(userData);
    }

    public void UpdatePlayerInfo(eStats type, UserData userData)
    {
        switch (type)
        {
            case eStats.Level:
                _textLevel.text = userData.level.ToString();
                break;

            case eStats.HP:
                _textHP.text = userData.stats.hp.current + " / " + userData.stats.hp.maximum;
                break;

            case eStats.MP:
                _textMP.text = userData.stats.mp.current + " / " + userData.stats.mp.maximum;
                break;

            case eStats.AP:
                _textAP.text = userData.stats.ap.current + " / " + userData.stats.ap.maximum;
                break;

            case eStats.EXP:
                _textEXP.text = userData.stats.exp.current + " / " + userData.maximumEXP;
                break;
        }

        Information(userData);
        Abnormalstatus(userData);
    }

    public void Open()
    {
        _objMaker.SetActive(false);
        _objInformation.SetActive(true);

        GameManager.instance.tools.Move_Anchor_XY(eUiDir.X, _objInformation.GetComponent<RectTransform>(), 0f, 0.5f, 0, Ease.OutBack, null);
    }

    public void Close(System.Action onResultCallback = null)
    {
        GameManager.instance.tools.Move_Anchor_XY(eUiDir.X, _objInformation.GetComponent<RectTransform>(), 1000f, 0.5f, 0, Ease.InBack, () => 
        {
            _objMaker.SetActive(true);
            _objInformation.SetActive(false);

            _textAbnormalStatus.text = string.Empty;

           onResultCallback?.Invoke();
        });
    }

    private void Information(UserData userData)
    {
        _textInformation.text = "Gold : " + userData.stats.coin.current + "\n";
        _textInformation.text += "Attack : " + (userData.stats.attack.current + userData.stats.attack.plus) + "\n";
        _textInformation.text += "Defence : " + (userData.stats.defence.current + userData.stats.defence.plus) + "\n";
        _textInformation.text += "Vision : " + (userData.stats.vision.current + userData.stats.vision.plus);
    }

    private void Abnormalstatus(UserData userData)
    {
        for(int i = 0; i < userData.data.abnormalStatuses.Count; i++)
        {
            switch(userData.data.abnormalStatuses[i].currentStatus)
            {
                case eStrengtheningTool.UnableAct:
                    _textAbnormalStatus.text += "행동 불능 : " + userData.data.abnormalStatuses[i].duration + "턴 남았습니다." + "\n";
                    break;

                case eStrengtheningTool.ContinuousDamage:
                    _textAbnormalStatus.text += "지속 피해 : " + userData.data.abnormalStatuses[i].duration + "턴/ " + userData.data.abnormalStatuses[i].value + "\n";
                    break;

                case eStrengtheningTool.Recovery:
                    _textAbnormalStatus.text += "회복 : " + userData.data.abnormalStatuses[i].duration + "턴 / " + userData.data.abnormalStatuses[i].value + "\n";
                    break;

                case eStrengtheningTool.Slowdown:
                    _textAbnormalStatus.text += "둔화 : " + userData.data.abnormalStatuses[i].duration + "턴 남았습니다." + "\n";
                    break;

                case eStrengtheningTool.BloodSucking:
                    _textAbnormalStatus.text += "흡혈 : " + userData.data.abnormalStatuses[i].duration + "턴 남았습니다." + "\n";
                    break;

                case eStrengtheningTool.Hardness:
                    _textAbnormalStatus.text += "강성 : " + userData.data.abnormalStatuses[i].duration + "턴 남았습니다." + "\n";
                    break;

                case eStrengtheningTool.Stealth:
                    _textAbnormalStatus.text += "은신 : " + userData.data.abnormalStatuses[i].duration + "턴 남았습니다." + "\n";
                    break;

                case eStrengtheningTool.AttackBlocking:
                    _textAbnormalStatus.text += "공격 방어 : " + userData.data.abnormalStatuses[i].value + "회 남았습니다." + "\n";
                    break;

                case eStrengtheningTool.SkillBlocking:
                    _textAbnormalStatus.text += "스킬 방어 : " + userData.data.abnormalStatuses[i].value + "회 남았습니다." + "\n";
                    break;

                case eStrengtheningTool.SkillReflect:
                    _textAbnormalStatus.text += "스킬 반사 : " + userData.data.abnormalStatuses[i].value + "회 남았습니다." + "\n";
                    break;

                case eStrengtheningTool.Invincibility:
                    _textAbnormalStatus.text += "무적 : " + userData.data.abnormalStatuses[i].value + "회 남았습니다." + "\n";
                    break;

            }
        }
    }
}
