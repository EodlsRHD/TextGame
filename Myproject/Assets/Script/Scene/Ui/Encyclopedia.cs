using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Encyclopedia : MonoBehaviour
{
    [SerializeField] private Button _buttonClose = null;

    [Header("Toggle")]
    [SerializeField] private Toggle _toggleCreature = null;

    private Action _onCloseCallback = null;

    private DataManager.Encyclopedia_Data _data = null;

    public void Initialize(Action onCloseCallback)
    {
        if (onCloseCallback != null)
        {
            _onCloseCallback = onCloseCallback;
        }

        _buttonClose.onClick.AddListener(OnClose);

        this.gameObject.SetActive(false);
    }

    public void Open()
    {
        GameManager.instance.dataManager.LoadEncyclopediaToCloud((result) =>
        {
            if(result  == false)
            {
                return;
            }

            _data = GameManager.instance.dataManager.CopyEncyclopediaData();

            this.gameObject.SetActive(true);
        });
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
