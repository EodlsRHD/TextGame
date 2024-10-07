using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GameMenu : MonoBehaviour
{
    [SerializeField] private Button _buttonClose = null;
    [SerializeField] private Button _buttonSettings = null;
    [SerializeField] private Button _buttonRankings = null;
    [SerializeField] private Button _buttonEncyclopedia = null;
    [SerializeField] private Button _buttonGotoMainMenu = null;
    [SerializeField] private Button _buttonLanguage = null;

    [Header("Sound"), SerializeField] private Button _buttonSound = null;
    [SerializeField] private Button _buttonEffect = null;
    [SerializeField] private GameObject _objEffectMute = null;
    [SerializeField] private GameObject _objSoundMute = null;

    [Space(10)]

    [SerializeField] private Button _buttonViewMap = null;
    [SerializeField] private Button _buttonDeveloper = null;

    private Action _onCloseCallback = null;

    public void Initialize(Action onCloseCallback)
    {
        if(onCloseCallback != null)
        {
            _onCloseCallback = onCloseCallback;
        }

        _buttonClose.onClick.AddListener(OnClose);
        _buttonSettings.onClick.AddListener(OnSettings);
        _buttonRankings.onClick.AddListener(OnRankings);
        _buttonEncyclopedia.onClick.AddListener(OnEncyclopedia);
        _buttonGotoMainMenu.onClick.AddListener(OnGotoMainMenu);
        _buttonLanguage.onClick.AddListener(OnLanguage);

        _buttonSound.onClick.AddListener(OnSound);
        _buttonEffect.onClick.AddListener(OnEffect);

        _buttonViewMap.onClick.AddListener(OnViewMap);
        _buttonDeveloper.onClick.AddListener(OnDeveloper);

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

    private void OnRankings()
    {
        UiManager.instance.OpenRankings();
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
                GameManager.instance.tools.SceneChange(eScene.Lobby);
            });
        }, () => 
        {
            GameManager.instance.tools.SceneChange(eScene.Game);
        });
    }

    private void OnLanguage()
    {

    }

    private void OnSound()
    {
        GameManager.instance.soundManager.MuteBgm((isMute) =>
        {
            _objSoundMute.SetActive(isMute);
        });
    }

    private void OnEffect()
    {
        GameManager.instance.soundManager.MuteSfx((isMute) => 
        {
            _objEffectMute.SetActive(isMute);
        });
    }

    private void OnViewMap()
    {

    }

    private void OnDeveloper()
    {

    }
}
