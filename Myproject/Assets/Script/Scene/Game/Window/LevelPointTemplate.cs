using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class LevelPointTemplate : MonoBehaviour
{
    [SerializeField] private Button _buttonPlus = null;
    [SerializeField] private Button _buttonMinus = null;

    [SerializeField] private TMP_Text _textLabel = null;
    [SerializeField] private TMP_Text _textPoint = null;

    private Action<eStats, int> _onResultCallback = null;
    private Action<Action<bool>> _onPlusCallback = null;
    private Action<bool, Action<bool>> _onMinusCallback = null;

    private eStats _type = eStats.Non;
    private int _oriPoint = 0;
    private int _point = 0;

    public void Initialize(eStats type, Action<Action<bool>> onPlusCallback, Action<bool, Action<bool>> onMinusCallback, Action<eStats, int> onResultCallback)
    {
        if(onResultCallback != null)
        {
            _onResultCallback = onResultCallback;
        }

        if(onPlusCallback != null)
        {
            _onPlusCallback = onPlusCallback;
        }

        if(onMinusCallback!= null)
        {
            _onMinusCallback = onMinusCallback;
        }

        _buttonPlus.onClick.AddListener(OnPlus);
        _buttonMinus.onClick.AddListener(OnMinus);

        _textLabel.text = type.ToString();

        _type = type;
    }

    public void SetPoint(int point)
    {
        _oriPoint = point;
        _point = point;

        _textPoint.text = point.ToString();

        this.gameObject.SetActive(true);
    }

    public void ResetPoint()
    {
        _point = _oriPoint;
        _textPoint.text = _point.ToString();
    }

    public void Result()
    {
        _onResultCallback?.Invoke(_type, _point);
    }

    private void OnPlus()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        if (_type == eStats.Vision)
        {
            if(_point == 4 || _oriPoint == 4)
            {
                UiManager.instance.OpenPopup(string.Empty, "최대치입니다.", string.Empty, null);

                return;
            }
        }

        _onPlusCallback?.Invoke((result) =>
        {
            if(result == false)
            {
                return;
            }

            _point += 1;
            _textPoint.text = _point.ToString();
        });
    }

    private void OnMinus()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        _onMinusCallback?.Invoke((_point == _oriPoint), (result) =>
        {
            if (result == false)
            {
                return;
            }

            if(_point - 1 <= 0)
            {
                return;
            }

            if(_point == _oriPoint)
            {
                return;
            }

            _buttonPlus.gameObject.SetActive(true);

            _point -= 1;
            _textPoint.text = _point.ToString();
        });
    }
}
