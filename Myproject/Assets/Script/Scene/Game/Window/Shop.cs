using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class Shop : MonoBehaviour
{
    [Header("Information")]
    [SerializeField] private GameObject _objInformation = null;
    [SerializeField] private TMP_Text _textName = null;
    [SerializeField] private TMP_Text _textDescription = null;

    [Space(10)]

    [SerializeField] private Button _buttonClose = null;

    private Action<string> _onUpdateTextCallback = null;
    private Action<string> _onUpdatePopupCallback = null;

    private DataManager.Npc_Data _npc = null;

    public void Initialize(Action<string> onUpdateTextCallback, Action<string> onUpdatePopupCallback)
    {
        if(onUpdateTextCallback != null)
        {
            _onUpdateTextCallback = onUpdateTextCallback;
        }

        if(onUpdatePopupCallback != null)
        {
            _onUpdatePopupCallback = onUpdatePopupCallback;
        }

        this.gameObject.SetActive(false);
    }

    public void Open(DataManager.Npc_Data npc)
    {
        _npc = npc;

        _textName.text = string.Empty;
        _textDescription.text = string.Empty;

        this.gameObject.SetActive(true);
    }

    private void OnClose()
    {
        this.gameObject.SetActive(false);
    }

    private void OpenInformation(string name, string description)
    {
        _textName.text = name;
        _textDescription.text = description;
    }
}
