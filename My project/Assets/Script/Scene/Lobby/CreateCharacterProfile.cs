using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class CreateCharacterProfile : MonoBehaviour
{
    [SerializeField] private TMP_InputField _inputfieldName = null;

    [Header("Buttons")]
    [SerializeField] private Button _buttonSave = null;

    private Action _onNewGameCallback = null;

    public void Initialize(Action onNewGameCallback)
    {
        if(onNewGameCallback != null)
        {
            _onNewGameCallback = onNewGameCallback;
        }

        _buttonSave.onClick.AddListener(OnSave);

        this.gameObject.SetActive(false);
    }

    public void Open()
    {
        this.gameObject.SetActive(true);
    }

    private void OnSave()
    {
        UiManager.instance.OpenPopup("Create Character", _inputfieldName.text + "\n" + "Do you want to start like this?", string.Empty, string.Empty, () =>
        {
            GameManager.instance.dataManager.ChangePlayerData(_inputfieldName.text);
        }, null);
    }
}
