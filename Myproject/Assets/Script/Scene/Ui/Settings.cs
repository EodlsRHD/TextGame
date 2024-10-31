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

    [Header("Sound")]
    [SerializeField] private Toggle _toggleBgm = null;
    [SerializeField] private Toggle _toggleSfx = null;

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

        _toggleBgm.isOn = GameManager.instance.soundManager.isMuteBGM;
        _toggleSfx.isOn = GameManager.instance.soundManager.isMuteSFX;

        _toggleBgm.onValueChanged.AddListener(OnBgm);
        _toggleSfx.onValueChanged.AddListener(OnSfx);

        this.gameObject.SetActive(false);
    }

    public void Open()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.Map);

        this.gameObject.SetActive(true);

        GameManager.instance.tools.Move_Anchor_XY(eUiDir.Y, this.GetComponent<RectTransform>(), 0f, 0.5f, 0, Ease.OutBack, null);
    }

    public void Close()
    {
        this.gameObject.SetActive(false);
    }

    private void OnClose()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);
        GameManager.instance.soundManager.PlaySfx(eSfx.Map);

        GameManager.instance.tools.Move_Anchor_XY(eUiDir.Y, this.GetComponent<RectTransform>(), 1800f, 0.5f, 0, Ease.InBack, () =>
        {
            _onCloseCallback?.Invoke();
        });
    }

    private void OnTutorial()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        UiManager.instance.OpenTutorial(true);
    }

    private void OnRankings()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        UiManager.instance.OpenRankings();
    }

    private void OnLanguage()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        UiManager.instance.OpenPopup("시스템", "아직 준비중입니다.", string.Empty, null);

        //UiManager.instance.OpenLanguage();
    }

    private void OnCradit()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        UiManager.instance.OpenCradit();
    }

    private void OnBgm(bool isTrue)
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        GameManager.instance.soundManager.isMuteBGM = isTrue;
    }

    private void OnSfx(bool isTrue)
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        GameManager.instance.soundManager.isMuteSFX = isTrue;
    }
}
