using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class IngameUI : MonoBehaviour
{
    [Header("Top")]
    [SerializeField] private Button _buttonViewMap = null;
    [SerializeField] private Button _buttonGameMenu = null;
    [SerializeField] private TMP_Text _textRound = null;

    [Header("Next Round")]
    [SerializeField] private GameObject _objNextRound = null;
    [SerializeField] private TMP_Text _textLabel = null;
    [SerializeField] private Button _buttonNextRound = null;
    [SerializeField] private TMP_Text _textButtonLabel = null;

    private Action<Action> _onViewMapCallback = null;
    private Action<eRoundClear> _OnNextRoundCallback = null;

    private eRoundClear _type = eRoundClear.Non;

    public void Initialize(Action<Action> onViewMapCallback, Action<eRoundClear> OnNextRoundCallback)
    {
        if(onViewMapCallback != null)
        {
            _onViewMapCallback = onViewMapCallback;
        }

        if(OnNextRoundCallback != null)
        {
            _OnNextRoundCallback = OnNextRoundCallback;
        }

        _buttonViewMap.onClick.AddListener(OnMap);
        _buttonGameMenu.onClick.AddListener(OnOpenGameMenu);
        _buttonNextRound.onClick.AddListener(OnNextRound);

        _objNextRound.SetActive(false);
        this.gameObject.SetActive(true);
    }
    private void OnMap()
    {
        _buttonViewMap.gameObject.SetActive(false);

        _onViewMapCallback?.Invoke(() => { _buttonViewMap.gameObject.SetActive(true); });
    }

    private void OnOpenGameMenu()
    {
        UiManager.instance.OpenGameMenu();
    }

    private void OnNextRound()
    {
        _OnNextRoundCallback?.Invoke(_type);
        _objNextRound.SetActive(false);

        _type = eRoundClear.Non;
    }

    public void SetRoundText(int round)
    {
        _textRound.text = round.ToString();
    }

    public void OpenNextRoundWindow(eRoundClear type)
    {
        _type = type;

        switch (type)
        {
            case eRoundClear.First:
                _textLabel.text = string.Empty;
                _textButtonLabel.text = "시작";
                break;

            case eRoundClear.Success:
                _textLabel.text = "라운드를 돌파하셨습니다.";
                _textButtonLabel.text = "다음 라운드로";
                break;

            case eRoundClear.Fail:
                _textLabel.text = "라운드를 실패하셨습니다.";
                _textButtonLabel.text = "메인 메뉴로";
                break;
        }

        _objNextRound.SetActive(true);
    }
}
