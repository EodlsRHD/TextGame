using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Threading.Tasks;

public class IngameUI : MonoBehaviour
{
    [SerializeField] private int _maxLevelPoint = 5;
    [SerializeField] private GameObject _objBlocker = null;

    [Header("Top")]
    [SerializeField] private Button _buttonViewMap = null;
    [SerializeField] private Button _buttonGameMenu = null;
    [SerializeField] private TMP_Text _textRound = null;

    [Header("Next Round")]
    [SerializeField] private GameObject _objNextRound = null;
    [SerializeField] private TMP_Text _textLabel = null;
    [SerializeField] private Button _buttonNextRound = null;
    [SerializeField] private TMP_Text _textButtonLabel = null;

    [Header("Player Infomation")]
    [SerializeField] private TMP_Text _textLevel = null;
    [SerializeField] private TMP_Text _textHP = null;
    [SerializeField] private TMP_Text _textMP = null;
    [SerializeField] private TMP_Text _textAP = null;
    [SerializeField] private TMP_Text _textEXP = null;

    [Header("Attack")]
    [SerializeField] private Attacker _Attacker = null;

    [Header("Level Point")]
    [SerializeField] private LevelPoint _levelPoint = null;

    private Action<Action> _onViewMapCallback = null;
    private Action<eRoundClear> _onNextRoundCallback = null;
    private Action<string> _onUpdateTextCallback = null;

    private eRoundClear _type = eRoundClear.Non;

    public void Initialize(Action<Action> onViewMapCallback, Action<eRoundClear> onNextRoundCallback, Action<string> onUpdateTextCallback)
    {
        if(onViewMapCallback != null)
        {
            _onViewMapCallback = onViewMapCallback;
        }

        if(onNextRoundCallback != null)
        {
            _onNextRoundCallback = onNextRoundCallback;
        }

        if(onUpdateTextCallback != null)
        {
            _onUpdateTextCallback = onUpdateTextCallback;
        }

        _levelPoint.Initialize(CloseLevelPoint);
        _Attacker.Initialize(CloseAttacker, _onUpdateTextCallback);

        _buttonViewMap.onClick.AddListener(OnMap);
        _buttonGameMenu.onClick.AddListener(OnOpenGameMenu);
        _buttonNextRound.onClick.AddListener(OnNextRound);

        _buttonViewMap.gameObject.SetActive(false);

        _objNextRound.SetActive(false);

        _objBlocker.SetActive(false);

        this.gameObject.SetActive(true);
    }

    public void StartGame()
    {
        _buttonViewMap.gameObject.SetActive(true);
    }

    private void OnMap()
    {
        _buttonViewMap.gameObject.SetActive(false);

        _onViewMapCallback?.Invoke(() => { _buttonViewMap.gameObject.SetActive(true); });
    }

    private void OnOpenGameMenu()
    {
        UiManager.instance.OpenGameMenu();
    }

    private void OnNextRound()
    {
        _onNextRoundCallback?.Invoke(_type);
        _objNextRound.SetActive(false);

        _type = eRoundClear.Non;
    }

    public void SetRoundText(int round)
    {
        _textRound.text = round.ToString();
    }

    public void OpenNextRoundWindow(eRoundClear type)
    {
        _type = type;

        switch (type)
        {
            case eRoundClear.First:
                _textLabel.text = string.Empty;
                _textButtonLabel.text = "시작";
                break;

            case eRoundClear.Success:
                _textLabel.text = "라운드를 돌파하셨습니다.";
                _textButtonLabel.text = "다음 라운드로";
                break;

            case eRoundClear.Fail:
                _textLabel.text = "라운드를 실패하셨습니다.";
                _textButtonLabel.text = "메인 메뉴로";
                break;
        }

        _objNextRound.SetActive(true);
    }

    public void UpdatePlayerInfo(DataManager.User_Data UserData)
    {
        _textLevel.text = UserData.level.ToString();
        _textHP.text = UserData.currentHP + " / " + UserData.maximumHP;
        _textMP.text = UserData.currentMP + " / " + UserData.maximumMP;
        _textAP.text = UserData.currentAP + " / " + UserData.maximumAP;
        _textEXP.text = UserData.currentEXP + " / " + UserData.maximumEXP;
    }

    public void UpdatePlayerInfo(eStats type, DataManager.User_Data userData)
    {
        switch(type)
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

    public void CallAttacker(DataManager.User_Data userData, DataManager.Creature_Data monster, Action onLastCallback, Action<eWinorLose, int> onResultCallback)
    {
        _Attacker.CallAttacker(userData, monster, onLastCallback, onResultCallback);
    }

    private void CloseAttacker()
    {
        _Attacker.Close();
    }

    public void OpneLevelPoint(DataManager.User_Data userData, Action<DataManager.User_Data> onResultCallback)
    {
        _objBlocker.SetActive(true);

        _levelPoint.Open(_maxLevelPoint, userData, onResultCallback);
    }

    private void CloseLevelPoint()
    {
        _objBlocker.SetActive(false);

        _levelPoint.Close();
    }
}
