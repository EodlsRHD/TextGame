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

        _objSfxMute.SetActive(GameManager.instance.soundManager.isMuteBGM);
        _objBgmMute.SetActive(GameManager.instance.soundManager.isMuteSFX);

        this.gameObject.SetActive(false);
    }

    public void Open()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.MenuOpen);

        this.gameObject.SetActive(true);

        GameManager.instance.tools.Move_Anchor_XY(eUiDir.Y, this.GetComponent<RectTransform>(), -200f, 00.5f, 0, Ease.OutBack, () => 
        {
            GameManager.instance.googleAds.ShowGameMenuAd();
        });
    }

    public void Close()
    {
        this.gameObject.SetActive(false);
    }

    private void OnClose()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);
        GameManager.instance.soundManager.PlaySfx(eSfx.MenuClose);

        GameManager.instance.googleAds.HideGameMenuAd();

        GameManager.instance.tools.Move_Anchor_XY(eUiDir.Y, this.GetComponent<RectTransform>(), 1800f, 0.5f, 0, Ease.InBack, () =>
        {
            _onCloseCallback?.Invoke();
        });
    }

    private void OnRankings()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);
        GameManager.instance.googleAds.HideGameMenuAd();

        UiManager.instance.OpenRankings(() =>
        {
            GameManager.instance.googleAds.ShowGameMenuAd();
        });
    }

    private void OnEncyclopedia()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);
        GameManager.instance.googleAds.HideGameMenuAd();

        UiManager.instance.OpenEncyclopedia(() => 
        {
            GameManager.instance.googleAds.ShowGameMenuAd();
        });
    }

    private void OnGotoMainMenu()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        GameManager.instance.dataManager.SaveDataToCloud(null, (result) =>
        {
            if (result == false)
            {
                UiManager.instance.OpenPopup(string.Empty, "저장에 실패하였습니다. 잠시후 다시 시도해주세요.", string.Empty, null);

                return;
            }

            GameManager.instance.googleAds.HideGameMenuAd();

            OnClose();

            GameManager.instance.tools.Fade(false, () =>
            {
                GameManager.instance.soundManager.PlaySfx(eSfx.GotoLobby);

                GameManager.instance.tools.SceneChange(eScene.Lobby);
            });
        });
    }

    private void OnLanguage()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        UiManager.instance.OpenPopup("시스템", "아직 준비중입니다.", string.Empty, null);
    }

    private void OnBgm()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        GameManager.instance.soundManager.MuteBgm((isMute) =>
        {
            _objBgmMute.SetActive(isMute);
        });
    }

    private void OnSfx()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        GameManager.instance.soundManager.MuteSfx((isMute) => 
        {
            _objSfxMute.SetActive(isMute);
        });
    }

    private void OnViewMap()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        PlayerPrefs.SetInt("MAP_BACKGROUND", !GameManager.instance.isMapBackgroundUpdate == true ? 1 : 0);
        GameManager.instance.isMapBackgroundUpdate = PlayerPrefs.GetInt("MAP_BACKGROUND") == 1 ? true : false;

        IngameManager.instance.ViewMap();
    }

    private void OnCredit()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        GameManager.instance.googleAds.HideGameMenuAd();

        UiManager.instance.OpenCradit(() =>
        {
            GameManager.instance.googleAds.ShowGameMenuAd();
        });
    }
}
