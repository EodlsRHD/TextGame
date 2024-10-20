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
    [SerializeField] private GameObject _objInformation = null;
    [SerializeField] private TMP_Text _textCoin = null;
    [SerializeField] private TMP_Text _textAttack = null;
    [SerializeField] private TMP_Text _textDefence = null;
    [SerializeField] private TMP_Text _textVision = null;
    [SerializeField] private TMP_Text _textAttackRange = null;

    public void Initialize()
    {
        _objInformation.SetActive(false);
    }

    public void UpdatePlayerInfo(UserData userData)
    {
        _textLevel.text = userData.level.ToString();
        _textHP.text = userData.stats.hp.current + " / " + userData.stats.hp.maximum;
        _textMP.text = userData.stats.mp.current + " / " + userData.stats.mp.maximum;
        _textAP.text = userData.stats.ap.current + " / " + userData.stats.ap.maximum;
        _textEXP.text = userData.stats.exp.current + " / " + userData.maximumEXP;

        _textCoin.text = userData.stats.coin.current.ToString();
        _textAttack.text = (userData.stats.attack.current + userData.stats.attack.plus).ToString();
        _textDefence.text = (userData.stats.defence.current + userData.stats.defence.plus).ToString();
        _textVision.text = (userData.stats.vision.current + userData.stats.vision.plus).ToString();
        _textAttackRange.text = (userData.stats.attackRange.current + userData.stats.attackRange.plus).ToString();
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

        _textCoin.text = userData.stats.coin.current.ToString();
        _textAttack.text = (userData.stats.attack.current + userData.stats.attack.plus).ToString();
        _textDefence.text = (userData.stats.defence.current + userData.stats.defence.plus).ToString();
        _textVision.text = (userData.stats.vision.current + userData.stats.vision.plus).ToString();
        _textAttackRange.text = (userData.stats.attackRange.current + userData.stats.attackRange.plus).ToString();
    }

    public void Open()
    {
        _objInformation.SetActive(true);

        GameManager.instance.tools.Move_Anchor_XY(eUiDir.X, _objInformation.GetComponent<RectTransform>(), 0f, 0.5f, 0, Ease.OutBack, null);
    }

    public void Close(System.Action onResultCallback = null)
    {
        GameManager.instance.tools.Move_Anchor_XY(eUiDir.X, _objInformation.GetComponent<RectTransform>(), 447f, 0.5f, 0, Ease.InBack, () => 
        {
            _objInformation.SetActive(false);

            onResultCallback?.Invoke();
        });
    }
}
