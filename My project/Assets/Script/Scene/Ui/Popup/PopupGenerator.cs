using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class PopupGenerator : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text _textTitle = null;
    [SerializeField] private TMP_Text _textContent = null;

    [Header("Button")]
    [SerializeField] private Button _buttonMiddle = null;
    [SerializeField] private Button _buttonLeft = null;
    [SerializeField] private Button _buttonRight = null;

    private Action _onClosePopupCallback = null;
    private Action _onMiddleCallback = null;
    private Action _onLeftCallback = null;
    private Action _onRightCallback = null;

    public void Initialize(Action onClosePopupCallback)
    {
        if(onClosePopupCallback != null)
        {
            _onClosePopupCallback = onClosePopupCallback;
        }

        _buttonMiddle.onClick.AddListener(OnMiddle);
        _buttonLeft.onClick.AddListener(OnRight);
        _buttonRight.onClick.AddListener(OnLeft);

        _buttonMiddle.gameObject.SetActive(false);
        _buttonLeft.gameObject.SetActive(false);
        _buttonRight.gameObject.SetActive(false);
    }

    public void Open_OneButton(string title, string content, string buttonText = "confirm", Action onButtonCallback = null)
    {
        if(onButtonCallback != null)
        {
            _onMiddleCallback = onButtonCallback;
        }

        _textTitle.text = title;
        _textContent.text = content;

        _buttonMiddle.GetComponentInChildren<TMP_Text>().text = buttonText;

        _buttonMiddle.gameObject.SetActive(true);
    }

    public void Open_TwoButton(string title, string content, string leftButtonText = "confirm", string rightButtonText = "cancel", Action onLeftButtonCallback = null, Action onRightButtonCallback = null)
    {
        if (onLeftButtonCallback != null)
        {
            _onLeftCallback = onLeftButtonCallback;
        }

        if (onRightButtonCallback != null)
        {
            _onRightCallback = onRightButtonCallback;
        }

        _textTitle.text = title;
        _textContent.text = content;

        _buttonLeft.GetComponentInChildren<TMP_Text>().text = leftButtonText;
        _buttonRight.GetComponentInChildren<TMP_Text>().text = rightButtonText;

        _buttonLeft.gameObject.SetActive(true);
        _buttonRight.gameObject.SetActive(true);
    }

    public void ClosePopup()
    {
        _textTitle.text = string.Empty;
        _textContent.text = string.Empty;

        _buttonMiddle.gameObject.SetActive(false);
        _buttonLeft.gameObject.SetActive(false);
        _buttonRight.gameObject.SetActive(false);
    }

    private void onClosePopup()
    {
        _onClosePopupCallback?.Invoke();
    }

    private void OnMiddle()
    {
        _onMiddleCallback?.Invoke();
        onClosePopup();
    }

    private void OnLeft()
    {
        _onLeftCallback?.Invoke();
        onClosePopup();
    }

    private void OnRight()
    {
        _onRightCallback?.Invoke();
        onClosePopup();
    }
}