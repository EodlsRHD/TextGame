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
        _objInformation.SetActive(true);
    }

    private void OnClose()
    {
        _objInformation.SetActive(false);

        _userData = null;
    }
}
