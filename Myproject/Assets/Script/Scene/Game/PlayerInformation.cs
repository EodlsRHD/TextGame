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

    private UserData _userData = null;

    public void Initialize()
    {
        _objInformation.SetActive(false);
    }

    public void UpdatePlayerInfo(UserData userData)
    {
        _userData = userData;

        _textLevel.text = userData.level.ToString();
        _textHP.text = userData.stats.hp.currnet + " / " + userData.stats.hp.maximum;
        _textMP.text = userData.stats.mp.currnet + " / " + userData.stats.mp.maximum;
        _textAP.text = userData.stats.ap.currnet + " / " + userData.stats.ap.maximum;
        _textEXP.text = userData.stats.exp.currnet + " / " + userData.maximumEXP;
    }

    public void UpdatePlayerInfo(eStats type, UserData userData)
    {
        _userData = userData;

        switch (type)
        {
            case eStats.Level:
                _textLevel.text = userData.level.ToString();
                break;

            case eStats.HP:
                _textHP.text = userData.stats.hp.currnet + " / " + userData.stats.hp.maximum;
                break;

            case eStats.MP:
                _textMP.text = userData.stats.mp.currnet + " / " + userData.stats.mp.maximum;
                break;

            case eStats.AP:
                _textAP.text = userData.stats.ap.currnet + " / " + userData.stats.ap.maximum;
                break;

            case eStats.EXP:
                _textEXP.text = userData.stats.exp.currnet + " / " + userData.maximumEXP;
                break;
        }
    }

    public void Open()
    {
        _textCoin.text = _userData.stats.coin.currnet.ToString();
        _textAttack.text = (_userData.stats.attack.currnet + _userData.stats.attack.plus).ToString();
        _textDefence.text = (_userData.stats.defence.currnet + _userData.stats.defence.plus).ToString();
        _textVision.text = (_userData.stats.vision.currnet + _userData.stats.vision.plus).ToString();
        _textAttackRange.text = (_userData.stats.attackRange.currnet + _userData.stats.attackRange.plus).ToString();

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
