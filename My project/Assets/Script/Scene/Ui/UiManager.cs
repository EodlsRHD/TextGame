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

    [Header("Popup"), SerializeField] private PopupCanvas _popupCanvas = null;

    private void Awake()
    {
        _instance = this;

        _popupCanvas.Initialize(ClosePopup);
    }

    public void OpenPopup(string title, string content, string buttonText, Action onButtonCallback)
    {
        _popupCanvas.OpenPopup(title, content, buttonText, onButtonCallback);

        _popupCanvas.gameObject.SetActive(true);
    }

    public void OpenPopup(string title, string content, string leftButtonText, string rightButtonText, Action onLeftButtonCallback, Action onRightButtonCallback)
    {
        _popupCanvas.OpenPopup(title, content, leftButtonText, rightButtonText, onLeftButtonCallback, onRightButtonCallback);
    }

    private void ClosePopup()
    {  
        _popupCanvas.ClosePopup();
    }
}
