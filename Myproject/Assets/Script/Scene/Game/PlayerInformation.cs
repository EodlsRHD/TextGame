using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerInformation : MonoBehaviour
{
    [SerializeField] private GameObject _objInformation = null;

    [Header("Horizontal")]
    [SerializeField] private TMP_Text _textLevel = null;
    [SerializeField] private TMP_Text _textHP = null;
    [SerializeField] private TMP_Text _textMP = null;
    [SerializeField] private TMP_Text _textAP = null;
    [SerializeField] private TMP_Text _textEXP = null;

    [Header("Vertical")]
    [SerializeField] private TMP_Text _textCoin = null;
    [SerializeField] private TMP_Text _textAttack = null;
    [SerializeField] private TMP_Text _textDefence = null;
    [SerializeField] private TMP_Text _textVision = null;
    [SerializeField] private TMP_Text _textAttackRange = null;

    [Space(10)]

    [SerializeField] private Button _buttonClose = null;

    private DataManager.User_Data _userData = null;

    public void Initialize()
    {
        _buttonClose.onClick.AddListener(OnClose);

        _objInformation.SetActive(false);
    }

    public void UpdatePlayerInfo(DataManager.User_Data userData)
    {
        _userData = userData;

        _textLevel.text = userData.level.ToString();
        _textHP.text = userData.currentHP + " / " + userData.maximumHP;
        _textMP.text = userData.currentMP + " / " + userData.maximumMP;
        _textAP.text = userData.currentAP + " / " + userData.maximumAP;
        _textEXP.text = userData.currentEXP + " / " + userData.maximumEXP;
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
                _textHP.text = userData.currentHP + " / " + userData.maximumHP;
                break;

            case eStats.MP:
                _textMP.text = userData.currentMP + " / " + userData.maximumMP;
                break;

            case eStats.AP:
                _textAP.text = userData.currentAP + " / " + userData.maximumAP;
                break;

            case eStats.EXP:
                _textEXP.text = userData.currentHP + " / " + userData.maximumEXP;
                break;
        }
    }

    public void Open()
    {
        _textCoin.text = _userData.data.coin.ToString();
        _textAttack.text = _userData.currentATTACK.ToString();
        _textDefence.text = _userData.currentDEFENCE.ToString();
        _textVision.text = _userData.currentVISION.ToString();
        _textAttackRange.text = _userData.currentATTACKRANGE.ToString();

        _objInformation.SetActive(true);
    }

    private void OnClose()
    {
        _objInformation.SetActive(false);

        _userData = null;
    }
}
