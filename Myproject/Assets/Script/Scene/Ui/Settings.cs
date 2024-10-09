using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Settings : MonoBehaviour
{
    [SerializeField] private Button _buttonClose = null;
    [SerializeField] private Button _buttonTutorial = null;
    [SerializeField] private Button _buttonRankings = null;
    [SerializeField] private Button _buttonLanguage = null;
    [SerializeField] private Button _buttonCradit = null;

    [Header("Sound"), SerializeField] private Button _buttonBgm = null;
    [SerializeField] private Button _buttonSfx = null;
    [SerializeField] private GameObject _objSfxMute = null;
    [SerializeField] private GameObject _objBgmMute = null;

    private Action _onCloseCallback = null;

    public void Initialize(Action onCloseCallback)
    {
        if(onCloseCallback != null)
        {
            _onCloseCallback = onCloseCallback;
        }

        _buttonClose.onClick.AddListener(OnClose);
        _buttonTutorial.onClick.AddListener(OnTutorial);
        _buttonRankings.onClick.AddListener(OnRankings);
        _buttonLanguage.onClick.AddListener(OnLanguage);
        _buttonCradit.onClick.AddListener(OnCradit);
        _buttonBgm.onClick.AddListener(OnBgm);
        _buttonSfx.onClick.AddListener(OnSfx);

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
    }

    public void Close()
    {
        this.gameObject.SetActive(false);
    }

    private void OnClose()
    {
        _onCloseCallback?.Invoke();
    }

    private void OnTutorial()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        UiManager.instance.OpenRankings();
    }

    private void OnRankings()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        UiManager.instance.OpenRankings();
    }

    private void OnLanguage()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        UiManager.instance.OpenRankings();
    }

    private void OnCradit()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        UiManager.instance.OpenRankings();
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
}
