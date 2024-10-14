using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UiManager : MonoBehaviour
{
    private static UiManager _instance = null;
    public static UiManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new UiManager();
            }

            return _instance;
        }
    }

    [SerializeField] private GameObject _objBlocker = null;
    [SerializeField] private GameObject _objPopupBlocker = null;

    [Header("Popup"), SerializeField] private PopupGenerator _popupGenerator = null;
    [Header("Game Menu"), SerializeField] private GameMenu _gameMenu = null;
    [Header("Tutorial"), SerializeField] private Tutorial _tutorial = null;
    [Header("Settings"), SerializeField] private Settings _settings = null;
    [Header("Encyclopedia"), SerializeField] private Encyclopedia _encyclopedias = null;

    [Space(10)]

    [Header("Rankings"), SerializeField] private GameObject _rankings = null;
    [SerializeField] private Button _buttonCloseRankings = null;

    [Header("Cradit"), SerializeField] private GameObject _cradit = null;
    [SerializeField] private Button _buttonCloseCradit = null;
    [SerializeField] private Button _buttonSupport = null;

    private bool _openGameMenu = false;
    private bool _openSettings = false;

    private void Awake()
    {
        _instance = this;

        _popupGenerator.Initialize(ClosePopup);
        _gameMenu.Initialize(CloseMenu);
        _tutorial.Initialize(CloseTutorial);
        _settings.Initialize(ClsoeSettings);
        _encyclopedias.Initialize(CloseEncyclopedia);

        _buttonCloseRankings.onClick.AddListener(CloseRankings);
        _buttonCloseCradit.onClick.AddListener(CloseCradit);
        _buttonSupport.onClick.AddListener(OnSupportDeveloper);

        _rankings.SetActive(false);
        _cradit.SetActive(false);
        ActiveBlocker(false);

        this.gameObject.SetActive(true);
    }

    private void ActiveBlocker(bool isActive)
    {
        if(_openGameMenu == true)
        {
            return;
        }

        if(_openSettings == true)
        {
            return;
        }

        _objBlocker.SetActive(isActive);
    }

    #region Popup

    /// <summary>
    ///  If the button text is empty => "confirm"
    /// </summary>
    public void OpenPopup(string title, string content, string buttonText, Action onButtonCallback)
    {
        _objPopupBlocker.SetActive(true);

        _popupGenerator.Open_OneButton(title, content, buttonText, onButtonCallback);
    }

    /// <summary>
    ///  If the button text is empty => Left "confirm", Right "cancel"
    /// </summary>
    public void OpenPopup(string title, string content, string leftButtonText, string rightButtonText, Action onLeftButtonCallback, Action onRightButtonCallback)
    {
        _objPopupBlocker.SetActive(true);

        _popupGenerator.Open_TwoButton(title, content, leftButtonText, rightButtonText, onLeftButtonCallback, onRightButtonCallback);
    }

    private void ClosePopup()
    {
        _objPopupBlocker.SetActive(false);

        _popupGenerator.ClosePopup();
    }

    #endregion

    #region Game Menu

    public void OpenGameMenu()
    {
        ActiveBlocker(true);

        _gameMenu.Open();
        _openGameMenu = true;
    }

    private void CloseMenu()
    {
        _openGameMenu = false;

        ActiveBlocker(false);

        _gameMenu.Close();
    }

    #endregion

    #region Settings

    private Action _onSettingsCloseResultCallback = null;

    public void OpenSettings(Action onCloseResultCallback = null)
    {
        ActiveBlocker(true);

        if(onCloseResultCallback != null)
        {
            _onSettingsCloseResultCallback = onCloseResultCallback;
        }

        _settings.Open();
        _openSettings = true;
    }

    private void ClsoeSettings()
    {
        _openSettings = false;

        ActiveBlocker(false);

        _settings.Close();
        _onSettingsCloseResultCallback?.Invoke();
    }

    #endregion

    #region Rankings

    private Action _onRankingsCallback = null;

    public void OpenRankings(Action onResultCallback = null)
    {
        if(onResultCallback != null)
        {
            _onRankingsCallback = onResultCallback;
        }

        GameManager.instance.soundManager.PlaySfx(eSfx.MenuOpen);

        ActiveBlocker(true);

        _rankings.SetActive(true);

        GameManager.instance.tools.Move_Anchor_XY(eDir.Y, _rankings.GetComponent<RectTransform>(), 0, 0.5f, 0, Ease.OutBack, null);
    }

    private void CloseRankings()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);
        GameManager.instance.soundManager.PlaySfx(eSfx.MenuClose);

        GameManager.instance.tools.Move_Anchor_XY(eDir.Y, _rankings.GetComponent<RectTransform>(), 3000f, 0.5f, 0, Ease.InBack, () =>
        {
            ActiveBlocker(false);

            _rankings.SetActive(false);
            _onRankingsCallback?.Invoke();
        });
    }

    #endregion

    #region Encyclopedia

    private Action _onEncyclopediaCloseResultCallback = null;

    public void OpenEncyclopedia(Action onResultCallback = null)
    {
        ActiveBlocker(true);

        if(onResultCallback != null)
        {
            _onEncyclopediaCloseResultCallback = onResultCallback;
        }

        GameManager.instance.soundManager.PlaySfx(eSfx.TurnPage);

        _encyclopedias.Open();
    }

    public void CloseEncyclopedia()
    {
        ActiveBlocker(false);

        _encyclopedias.Close();
        _onEncyclopediaCloseResultCallback?.Invoke();
    }

    #endregion

    #region Turorial

    public void OpenTutorial(bool isMenu = false)
    {
        ActiveBlocker(true);

        _tutorial.Open(isMenu);
    }

    private void CloseTutorial()
    {
        ActiveBlocker(false);

        _tutorial.Close();
    }

    #endregion

    #region Cradit

    private Action _onCraditCallback = null;

    public void OpenCradit(Action onResultCallback = null)
    {
        if(onResultCallback != null)
        {
            _onCraditCallback = onResultCallback;
        }

        GameManager.instance.soundManager.PlaySfx(eSfx.MenuOpen);

        ActiveBlocker(true);

        _cradit.SetActive(true);

        GameManager.instance.tools.Move_Anchor_XY(eDir.Y, _cradit.GetComponent<RectTransform>(), 0, 0.5f, 0, Ease.OutBack, null);
    }

    public void CloseCradit()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);
        GameManager.instance.soundManager.PlaySfx(eSfx.MenuClose);

        GameManager.instance.tools.Move_Anchor_XY(eDir.Y, _cradit.GetComponent<RectTransform>(), 3000f, 0.5f, 0, Ease.InBack, () =>
        {
            ActiveBlocker(false);

            _cradit.SetActive(false);
            _onCraditCallback?.Invoke();
        });
    }

    private void OnSupportDeveloper()
    {
        Application.OpenURL(@"https://buymeacoffee.com/eodls0810de");
    }

    #endregion
}
