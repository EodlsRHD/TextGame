using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    private void Awake()
    {
        _instance = this;

        _popupGenerator.Initialize(ClosePopup);
        _gameMenu.Initialize(CloseMenu);

        ActiveBlocker(false);
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

    public void OpenSettings()
    {
        //ActiveBlocker(true);

        OpenPopup("시스템", "아직 준비중입니다.", string.Empty, null);
    }

    private void ClsoeSettings()
    {
        //ActiveBlocker(false);
    }

    #endregion

    #region Encyclopedia

    public void OpenEncyclopedia()
    {
        //ActiveBlocker(true);

        OpenPopup("시스템", "아직 준비중입니다.", string.Empty, null);
    }

    private void CloseEncyclopedia()
    {
        //ActiveBlocker(false);
    }

    #endregion
}
