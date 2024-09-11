using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GameMenu : MonoBehaviour
{
    [SerializeField] private Button _buttonClose = null;
    [SerializeField] private Button _buttonSettings = null;
    [SerializeField] private Button _buttonEncyclopedia = null;
    [SerializeField] private Button _buttonGotoMainMenu = null;

    private Action _onCloseCallback = null;

    public void Initialize(Action onCloseCallback)
    {
        if(onCloseCallback != null)
        {
            _onCloseCallback = onCloseCallback;
        }

        _buttonClose.onClick.AddListener(OnClose);
        _buttonSettings.onClick.AddListener(OnSettings);
        _buttonEncyclopedia.onClick.AddListener(OnEncyclopedia);
        _buttonGotoMainMenu.onClick.AddListener(OnGotoMainMenu);

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

    private void OnSettings()
    {
        UiManager.instance.OpenSettings();
    }

    private void OnEncyclopedia()
    {
        UiManager.instance.OpenEncyclopedia();
    }

    private void OnGotoMainMenu()
    {
        UiManager.instance.OpenPopup("시스템", "저장하시겠습니까?", string.Empty, string.Empty, () =>
        {
            GameManager.instance.dataManager.SaveDataToCloud(null, () => 
            {
                GameManager.instance.tools.SceneChange(eScene.Game);
            });
        }, () => 
        {
            GameManager.instance.tools.SceneChange(eScene.Game);
        });
    }
}
