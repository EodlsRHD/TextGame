using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PopupCanvas : MonoBehaviour
{
    [SerializeField] private PopupGenerator _popupGenerator = null;

    private Action _onClosePopupCallback = null;

    public void Initialize(Action onClosePopupCallback)
    {
        if(onClosePopupCallback != null)
        {
            _onClosePopupCallback = null;
        }

        _popupGenerator.Initialize(ClosePopup);

        this.gameObject.SetActive(false);
    }

    public void OpenPopup(string title, string content, string buttonText, Action onButtonCallback)
    {
        _popupGenerator.Open_OneButton(title, content, buttonText, onButtonCallback);

        this.gameObject.SetActive(true);
    }

    public void OpenPopup(string title, string content, string leftButtonText, string rightButtonText, Action onLeftButtonCallback, Action onRightButtonCallback)
    {
        _popupGenerator.Open_TwoButton(title, content, leftButtonText, rightButtonText, onLeftButtonCallback, onRightButtonCallback);

        this.gameObject.SetActive(true);
    }
    
    public void ClosePopup()
    {
        _popupGenerator.ClosePopup();
        this.gameObject.SetActive(false);

        _onClosePopupCallback?.Invoke();
    }
}
