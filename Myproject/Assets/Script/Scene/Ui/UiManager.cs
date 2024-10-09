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

        _rankings.SetActive(false);
        _cradit.SetActive(false);
        ActiveBlocker(false);

        this.gameObject.SetActive(true);
    }

    private void ActiveBlocker(bool isActive)
    {
        _objBlocker.SetActive(isActive);
    }

    #region Popup

    /// <summary>
    ///  If the button text is empty => "confirm"
    /// </summary>
    public void OpenPopup(string title, string content, string buttonText, Action onButtonCallback)
    {
        ActiveBlocker(true);

        _popupGenerator.Open_OneButton(title, content, buttonText, onButtonCallback);
    }

    /// <summary>
    ///  If the button text is empty => Left "confirm", Right "cancel"
    /// </summary>
    public void OpenPopup(string title, string content, string leftButtonText, string rightButtonText, Action onLeftButtonCallback, Action onRightButtonCallback)
    {
        ActiveBlocker(true);

        _popupGenerator.Open_TwoButton(title, content, leftButtonText, rightButtonText, onLeftButtonCallback, onRightButtonCallback);
    }

    private void ClosePopup()
    {
        ActiveBlocker(false);

        _popupGenerator.ClosePopup();
    }

    #endregion

    #region Game Menu

    public void OpenGameMenu()
    {
        ActiveBlocker(true);

        _gameMenu.Open();
    }

    private void CloseMenu()
    {
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
    }

    private void ClsoeSettings()
    {
        ActiveBlocker(false);

        _settings.Close();
        _onSettingsCloseResultCallback?.Invoke();
    }

    #endregion

    #region Rankings

    public void OpenRankings()
    {
        ActiveBlocker(true);
        _rankings.SetActive(true);
    }

    private void CloseRankings()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        ActiveBlocker(false);
        _rankings.SetActive(false);
    }

    #endregion

    #region Encyclopedia

    private Action _onEncyclopediaCloseResultCallback = null;

    public void OpenEncyclopedia(Action onCloseResultCallback = null)
    {
        ActiveBlocker(true);

        if(onCloseResultCallback != null)
        {
            _onEncyclopediaCloseResultCallback = onCloseResultCallback;
        }

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

    public void OpenTutorial()
    {
        //ActiveBlocker(true);

        OpenPopup("시스템", "아직 준비중입니다.", string.Empty, null);
    }

    private void CloseTutorial()
    {
        //ActiveBlocker(false);
    }

    #endregion

    #region Cradit

    public void OpenCradit()
    {
        ActiveBlocker(true);

        _cradit.SetActive(true);
    }

    public void CloseCradit()
    {
        ActiveBlocker(false);

        _cradit.SetActive(false);
    }

    #endregion
}
