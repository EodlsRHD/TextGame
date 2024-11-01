using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Threading.Tasks;

public class IngameUI : MonoBehaviour
{
    [SerializeField] private GameObject _objBlocker = null;

    [Header("Top")]
    [SerializeField] private Button _buttonViewMap = null;
    [SerializeField] private Button _buttonGameMenu = null;
    [SerializeField] private TMP_Text _textRound = null;

    [Header("Next Round")]
    [SerializeField] private GameObject _objNextRound = null;
    [SerializeField] private TMP_Text _textLabel = null;
    [SerializeField] private Button _buttonNextRound = null;
    [SerializeField] private Button _buttonGiveUp = null;
    [SerializeField] private Button _buttonResurrection = null;
    [SerializeField] private TMP_Text _textButtonLabel = null;

    [Header("Attack"), SerializeField] private Attacker _Attacker = null;
    [Header("Level Point"), SerializeField] private LevelPoint _levelPoint = null;
    [Header("Player Information"), SerializeField] private PlayerInformation _playerInformation = null;

    [SerializeField] private Button _buttonOpenPlayerInformation = null;
    [SerializeField] private Button _buttonClosePlayerInformation = null;

    private Action<Action> _onViewMapCallback = null;
    private Action<eRoundClear> _onNextRoundCallback = null;

    private eRoundClear _type = eRoundClear.Non;

    private int _resurrectionCount = 2;
    private int _maxLevelPoint = 3;

    public void Initialize(Action<Action> onViewMapCallback, Action<eRoundClear> onNextRoundCallback)
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
        _Attacker.Initialize(CloseAttacker);
        _playerInformation.Initialize();

        _buttonViewMap.onClick.AddListener(OnMap);
        _buttonGameMenu.onClick.AddListener(OnOpenGameMenu);
        _buttonNextRound.onClick.AddListener(OnNextRound);
        _buttonGiveUp.onClick.AddListener(OnGiveUp);
        _buttonResurrection.onClick.AddListener(OnResurrection);
        _buttonOpenPlayerInformation.onClick.AddListener(OnOpenPlayerInformation);
        _buttonClosePlayerInformation.onClick.AddListener(OnClosePlayerInformation);

        _objNextRound.SetActive(false);

        _objBlocker.SetActive(false);

        _buttonViewMap.gameObject.SetActive(!GameManager.instance.isMapBackgroundUpdate);

        this.gameObject.SetActive(true);
    }

    private void OnMap()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        _buttonViewMap.gameObject.SetActive(!GameManager.instance.isMapBackgroundUpdate);

        _onViewMapCallback?.Invoke(() => { _buttonViewMap.gameObject.SetActive(true); });
    }

    private void OnOpenGameMenu()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        UiManager.instance.OpenGameMenu();
    }

    private void OnNextRound()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);
        GameManager.instance.soundManager.PlaySfx(eSfx.MenuOpen);

        _onNextRoundCallback?.Invoke(_type);

        CloseNextRound();
        _type = eRoundClear.Non;
    }

    private void OnGiveUp()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);
        GameManager.instance.soundManager.PlaySfx(eSfx.MenuOpen);

        _onNextRoundCallback?.Invoke(eRoundClear.Fail);

        CloseNextRound();
        _type = eRoundClear.Non;
    }

    private void OnResurrection()
    {
        if(_resurrectionCount == 0)
        {
            return;
        }

        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);
        GameManager.instance.soundManager.PlaySfx(eSfx.MenuOpen);

        GameManager.instance.googleAds.ShowRewardedAd_Resurrection(() =>
        {
            _onNextRoundCallback?.Invoke(eRoundClear.Restart);

            CloseNextRound();

            _resurrectionCount--;
            _type = eRoundClear.Non;
        });
    }

    private void CloseNextRound()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.MenuClose);

        GameManager.instance.tools.Move_Anchor_XY(eUiDir.Y, _objNextRound.GetComponent<RectTransform>(), -360f, 0.5f, 0, Ease.InBack, () => 
        {
            _objNextRound.SetActive(false);
        });
    }

    public void SetRoundText(int round, int turnCount)
    {
        _textRound.text = round + " - " + turnCount;
    }

    public void OpenNextRoundWindow(eRoundClear type, string content = null)
    {
        _type = type;

        _buttonNextRound.gameObject.SetActive(false);
        _buttonGiveUp.gameObject.SetActive(false);
        _buttonResurrection.gameObject.SetActive(false);

        switch (type)
        {
            case eRoundClear.First:
                _textLabel.text = "게임을 시작합니다.";
                _textButtonLabel.text = "시작";
                _buttonNextRound.gameObject.SetActive(true);
                break;

            case eRoundClear.Load:
                _textLabel.text = "이어하기";
                _textButtonLabel.text = "시작";
                _buttonNextRound.gameObject.SetActive(true);
                break;

            case eRoundClear.Success:
                _textLabel.text = "라운드를 돌파하셨습니다.";
                _textButtonLabel.text = "다음 라운드로";
                _buttonNextRound.gameObject.SetActive(true);
                break;

            case eRoundClear.Fail:
                _textLabel.text = "라운드를 실패하셨습니다." + "\n" + content;
                _textButtonLabel.text = "메인 메뉴로";

                if(_resurrectionCount == 0)
                {
                    _buttonNextRound.gameObject.SetActive(true);
                }
                else if(_resurrectionCount > 0)
                {
                    _buttonGiveUp.gameObject.SetActive(true);
                    _buttonResurrection.gameObject.SetActive(true);
                }
                break;
        }

        _objNextRound.SetActive(true);

        GameManager.instance.tools.Move_Anchor_XY(eUiDir.Y, _objNextRound.GetComponent<RectTransform>(), 350f, 0.5f, 0, Ease.OutBack, null);
    }

    public void UpdatePlayerInfo(UserData userData)
    {
        _playerInformation.UpdatePlayerInfo(userData);
    }

    public void UpdatePlayerInfo(eStats type, UserData userData)
    {
        _playerInformation.UpdatePlayerInfo(type, userData);
    }

    public void CallAttacker(UserData userData, CreatureData monster, Action onLastCallback, Action<eWinorLose, int> onResultCallback)
    {
        _Attacker.CallAttacker(userData, monster, onLastCallback, onResultCallback);
    }

    public void ViewRanking()
    {
        _Attacker.ViewRanking();
    }

    private void CloseAttacker()
    {
        _Attacker.Close();
    }

    public void OpneLevelPoint(UserData userData, Action<UserData> onResultCallback)
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
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);
        GameManager.instance.soundManager.PlaySfx(eSfx.Map);

        _playerInformation.Open();
    }

    private void OnClosePlayerInformation()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);
        GameManager.instance.soundManager.PlaySfx(eSfx.Map);

        _playerInformation.Close();
    }

    public void HideMapButton()
    {
        _buttonViewMap.gameObject.SetActive(!GameManager.instance.isMapBackgroundUpdate);
    }
}
