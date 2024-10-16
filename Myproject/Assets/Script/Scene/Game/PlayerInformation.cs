using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    private DataManager.User_Data _userData = null;

    public void Initialize()
    {
        _objInformation.SetActive(false);
    }

    public void UpdatePlayerInfo(DataManager.User_Data userData)
    {
        _userData = userData;

        _textLevel.text = userData.level.ToString();
        _textHP.text = userData.data.currentHP + " / " + userData.maximumHP;
        _textMP.text = userData.data.currentMP + " / " + userData.maximumMP;
        _textAP.text = userData.data.currentAP + " / " + userData.maximumAP;
        _textEXP.text = userData.data.currentEXP + " / " + userData.maximumEXP;
    }

    public void UpdatePlayerInfo(eStats type, DataManager.User_Data userData)
    {
        _userData = userData;

        switch (type)
        {
            case eStats.Level:
                _textLevel.text = userData.level.ToString();
                break;

            case eStats.HP:
                _textHP.text = userData.data.currentHP + " / " + userData.maximumHP;
                break;

            case eStats.MP:
                _textMP.text = userData.data.currentMP + " / " + userData.maximumMP;
                break;

            case eStats.AP:
                _textAP.text = userData.data.currentAP + " / " + userData.maximumAP;
                break;

            case eStats.EXP:
                _textEXP.text = userData.data.currentHP + " / " + userData.maximumEXP;
                break;
        }
    }

    public void Open()
    {
        _textCoin.text = _userData.data.coin.ToString();
        _textAttack.text = _userData.data.currentATTACK.ToString();
        _textDefence.text = _userData.data.currentDEFENCE.ToString();
        _textVision.text = _userData.data.currentVISION.ToString();
        _textAttackRange.text = _userData.data.currentATTACKRANGE.ToString();

        _objInformation.SetActive(true);

        GameManager.instance.tools.Move_Anchor_XY(eUiDir.X, _objInformation.GetComponent<RectTransform>(), 423f, 0.5f, 0, Ease.OutBack, null);
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
