using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class GameMenu : MonoBehaviour
{
    [SerializeField] private Button _buttonClose = null;
    [SerializeField] private Button _buttonRankings = null;
    [SerializeField] private Button _buttonEncyclopedia = null;
    [SerializeField] private Button _buttonGotoMainMenu = null;
    [SerializeField] private Button _buttonLanguage = null;

    [Header("Sound"), SerializeField] private Button _buttonBgm = null;
    [SerializeField] private Button _buttonSfx = null;
    [SerializeField] private GameObject _objSfxMute = null;
    [SerializeField] private GameObject _objBgmMute = null;

    [Space(10)]

    [SerializeField] private Button _buttonViewMap = null;
    [SerializeField] private Button _buttonCredit = null;

    private Action _onCloseCallback = null;

    public void Initialize(Action onCloseCallback)
    {
        if(onCloseCallback != null)
        {
            _onCloseCallback = onCloseCallback;
        }

        _buttonClose.onClick.AddListener(OnClose);
        _buttonRankings.onClick.AddListener(OnRankings);
        _buttonEncyclopedia.onClick.AddListener(OnEncyclopedia);
        _buttonGotoMainMenu.onClick.AddListener(OnGotoMainMenu);
        _buttonLanguage.onClick.AddListener(OnLanguage);

        _buttonBgm.onClick.AddListener(OnBgm);
        _buttonSfx.onClick.AddListener(OnSfx);

        _buttonViewMap.onClick.AddListener(OnViewMap);
        _buttonCredit.onClick.AddListener(OnCredit);

        bool isSFX = PlayerPrefs.GetInt("SFX", 1) == 0 ? false : true;
        bool isBGM = PlayerPrefs.GetInt("BGM", 1) == 0 ? false : true;

        GameManager.instance.soundManager.MuteSfx(!isSFX);
        GameManager.instance.soundManager.MuteBgm(!isBGM);

        _objSfxMute.SetActive(!isSFX);
        _objBgmMute.SetActive(!isBGM);

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
                GameManager.instance.tools.Fade(false, () =>
                {
                    GameManager.instance.tools.SceneChange(eScene.Lobby);
                });
            });
        }, null);
    }

    private void OnLanguage()
    {

    }

    private void OnBgm()
    {
        GameManager.instance.soundManager.MuteBgm((isMute) =>
        {
            _objBgmMute.SetActive(isMute);
        });
    }

    private void OnSfx()
    {
        GameManager.instance.soundManager.MuteSfx((isMute) => 
        {
            _objSfxMute.SetActive(isMute);
        });
    }

    private void OnViewMap()
    {

    }

    private void OnCredit()
    {
        // bgm by https://makotohiramatsu.itch.io/
        // sfx by https://zombieham.itch.io/ (book sound)
    }
}
