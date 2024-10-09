using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Encyclopedia : MonoBehaviour
{
    [SerializeField] private Button _buttonClose = null;

    private Action _onCloseCallback = null;

    public void Initialize(Action onCloseCallback)
    {
        if (onCloseCallback != null)
        {
            _onCloseCallback = onCloseCallback;
        }

        _buttonClose.onClick.AddListener(OnClose);

        //GameManager.instance.dataManager.LoadEncyclopediaToCloud()
        //GameManager.instance.dataManager.CopyEncyclopediaData()

        this.gameObject.SetActive(false);
    }

    public void Open()
    {
        this.gameObject.SetActive(true);
    }

    public void Close()
    {
        this.gameObject.SetActive(false);
    }

    private void OnClose()
    {
        _onCloseCallback?.Invoke();
    }
}
