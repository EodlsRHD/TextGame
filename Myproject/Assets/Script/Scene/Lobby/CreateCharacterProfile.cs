using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using System.Text.RegularExpressions;

public class CreateCharacterProfile : MonoBehaviour
{
    [SerializeField] private TMP_InputField _inputfieldName = null;

    [Header("Buttons")]
    [SerializeField] private Button _buttonSave = null;
    [SerializeField] private Button _buttonBack = null;

    private Action _onNewGameCallback = null;

    public void Initialize(Action onNewGameCallback)
    {
        if(onNewGameCallback != null)
        {
            _onNewGameCallback = onNewGameCallback;
        }

        _buttonSave.onClick.AddListener(OnSave);
        _buttonBack.onClick.AddListener(OnBack);

        this.gameObject.SetActive(false);
    }

    public void Open()
    {
        this.gameObject.SetActive(true);
    }

    private void OnSave()
    {
        if(_inputfieldName.text.Length == 0)
        {
            UiManager.instance.OpenPopup("캐릭터 생성", "닉네임은 비워둘 수 없습니다.", string.Empty, null);

            return;
        }

        string str_new = _inputfieldName.text.Replace(" ", "");

        UiManager.instance.OpenPopup("캐릭터 생성", str_new + "\n" + "이대로 시작하시겠습니까?", string.Empty, string.Empty, () =>
        {
            GameManager.instance.dataManager.CreateNewSaveData();
            GameManager.instance.dataManager.ChangePlayerData(str_new);

            GameManager.instance.tools.SceneChange(eScene.Game);
        }, null);
    }

    private void OnBack()
    {
        this.gameObject.SetActive(false);

        _inputfieldName.text = string.Empty;
    }
}
