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

        GameManager.instance.isMapBackgroundUpdate = PlayerPrefs.GetInt("MAP_BACKGROUND", 0) == 1 ? true : false;

        bool isSFX = PlayerPrefs.GetInt("SFX") == 0 ? false : true;
        bool isBGM = PlayerPrefs.GetInt("BGM") == 0 ? false : true;

        GameManager.instance.soundManager.MuteSfx(!isSFX);
        GameManager.instance.soundManager.MuteBgm(!isBGM);

        _objSfxMute.SetActive(!isSFX);
        _objBgmMute.SetActive(!isBGM);

        this.gameObject.SetActive(false);
    }

    public void Open()
    {
        this.gameObject.SetActive(true);

        GameManager.instance.tools.Move_Local_XY(eDir.Y, this.GetComponent<RectTransform>(), 0, 00.5f, 0, Ease.OutBack, () => 
        {
            GameManager.instance.googleAds.ShowGameMenuAd();
        });
    }

    public void Close()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);
        GameManager.instance.googleAds.HideGameMenuAd();

        this.gameObject.SetActive(false);
    }

    private void OnClose()
    {
        GameManager.instance.tools.Move_Local_XY(eDir.Y, this.GetComponent<RectTransform>(), 4444, 0.5f, 0, Ease.InBack, () =>
        {
            _onCloseCallback?.Invoke();
        });
    }

    private void OnRankings()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        UiManager.instance.OpenRankings();
    }

    private void OnEncyclopedia()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);
        GameManager.instance.googleAds.HideGameMenuAd();

        UiManager.instance.OpenEncyclopedia();
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

        if (IngameManager.instance != null)
        {
            IngameManager.instance.UpdateMap();
        }
    }

    private void OnCredit()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        UiManager.instance.OpenCradit();
    }
}
