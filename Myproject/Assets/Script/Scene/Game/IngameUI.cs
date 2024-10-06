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

    [Header("Attack"), SerializeField] private Attacker _Attacker = null;
    [Header("Level Point"), SerializeField] private LevelPoint _levelPoint = null;
    [Header("Player Information"), SerializeField] private PlayerInformation _playerInformation = null;

    [SerializeField] private Button _buttonOpenPlayerInformation = null;

    private Action<Action> _onViewMapCallback = null;
    private Action<eRoundClear> _onNextRoundCallback = null;

    private eRoundClear _type = eRoundClear.Non;

    public void Initialize(Action<Action> onViewMapCallback, Action<eRoundClear> onNextRoundCallback, Action<string> onUpdateTextCallback, Action<string> onUpdatePopupCallback)
    {
        if(onViewMapCallback != null)
        {
            _onViewMapCallback = onViewMapCallback;
        }

        if(onNextRoundCallback != null)
        {
            _onNextRoundCallback = onNextRoundCallback;
        }

        _levelPoint.Initialize(CloseLevelPoint);
        _Attacker.Initialize(CloseAttacker, onUpdateTextCallback, onUpdatePopupCallback);
        _playerInformation.Initialize();

        _buttonViewMap.onClick.AddListener(OnMap);
        _buttonGameMenu.onClick.AddListener(OnOpenGameMenu);
        _buttonNextRound.onClick.AddListener(OnNextRound);
        _buttonOpenPlayerInformation.onClick.AddListener(OnOpenPlayerInformation);

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

        if(_type != eRoundClear.Fail)
        {
            _objNextRound.SetActive(false);
        }

        _type = eRoundClear.Non;
    }

    public void SetRoundText(int round)
    {
        _textRound.text = round.ToString();
    }

    public void OpenNextRoundWindow(eRoundClear type, string content = null)
    {
        _type = type;

        switch (type)
        {
            case eRoundClear.First:
                _textLabel.text = "������ �����մϴ�.";
                _textButtonLabel.text = "����";
                break;

            case eRoundClear.Load:
                _textLabel.text = "�̾��ϱ�";
                _textButtonLabel.text = "����";
                break;

            case eRoundClear.Success:
                _textLabel.text = "���带 �����ϼ̽��ϴ�.";
                _textButtonLabel.text = "���� �����";
                break;

            case eRoundClear.Fail:
                _textLabel.text = "���带 �����ϼ̽��ϴ�." + "\n" + content;
                _textButtonLabel.text = "���� �޴���";
                break;
        }

        _objNextRound.SetActive(true);
    }

    public void UpdatePlayerInfo(DataManager.User_Data userData)
    {
        _playerInformation.UpdatePlayerInfo(userData);
    }

    public void UpdatePlayerInfo(eStats type, DataManager.User_Data userData)
    {
        _playerInformation.UpdatePlayerInfo(type, userData);
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

    private void OnOpenPlayerInformation()
    {
        _playerInformation.Open();
    }
}
