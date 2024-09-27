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

    [Header("Button Text")]
    [SerializeField] private TMP_Text _textMiddle = null;
    [SerializeField] private TMP_Text _textLeft = null;
    [SerializeField] private TMP_Text _textRight = null;

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
        _buttonLeft.onClick.AddListener(OnLeft);
        _buttonRight.onClick.AddListener(OnRight);

        _buttonMiddle.gameObject.SetActive(false);
        _buttonLeft.gameObject.SetActive(false);
        _buttonRight.gameObject.SetActive(false);

        this.gameObject.SetActive(false);
    }

    public void Open_OneButton(string title, string content, string buttonText, Action onButtonCallback = null)
    {
        if(onButtonCallback != null)
        {
            _onMiddleCallback = onButtonCallback;
        }

        _textTitle.text = title;
        _textContent.text = content;

        _textMiddle.text = (buttonText.Length == 0) ? "확인" : buttonText;

        _buttonMiddle.gameObject.SetActive(true);

        this.gameObject.SetActive(true);
    }

    public void Open_TwoButton(string title, string content, string leftButtonText, string rightButtonText, Action onLeftButtonCallback = null, Action onRightButtonCallback = null)
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

        _textLeft.text = (leftButtonText.Length == 0) ? "확인" : leftButtonText;
        _textRight.text = (rightButtonText.Length == 0) ? "취소" : rightButtonText;

        _buttonLeft.gameObject.SetActive(true);
        _buttonRight.gameObject.SetActive(true);

        this.gameObject.SetActive(true);
    }

    public void ClosePopup()
    {
        _textTitle.text = string.Empty;
        _textContent.text = string.Empty;

        _buttonMiddle.gameObject.SetActive(false);
        _buttonLeft.gameObject.SetActive(false);
        _buttonRight.gameObject.SetActive(false);

        this.gameObject.SetActive(false);
    }

    private void onClosePopup()
    {
        _onClosePopupCallback?.Invoke();
    }

    private void OnMiddle()
    {
        onClosePopup();

        _onMiddleCallback?.Invoke();
    }

    private void OnLeft()
    {
        onClosePopup();

        _onLeftCallback?.Invoke();
    }

    private void OnRight()
    {
        onClosePopup();

        _onRightCallback?.Invoke();
    }
}
