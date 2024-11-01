using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class LevelPoint : MonoBehaviour
{
    [SerializeField] private Button _buttonClose = null;
    [SerializeField] private Button _buttonApply = null;
    [SerializeField] private Button _buttonReset = null;
    [SerializeField] private TMP_Text _textMaxPoint = null;

    [Space(10)]

    [SerializeField] private LevelPointTemplate _template = null;
    [SerializeField] private Transform _trTemplateParant = null;

    private Action _onCloseCallback = null;
    private Action<UserData> _onResultCallback = null;

    private List<LevelPointTemplate> _templates = new List<LevelPointTemplate>();
    private UserData _userData = null;

    private int _maxPoint = 0;
    private int _point = 0;

    public void Initialize(Action onCloseCallback)
    {
        if(onCloseCallback != null)
        {
            _onCloseCallback = onCloseCallback;
        }

        _buttonClose.onClick.AddListener(OnClose);
        _buttonReset.onClick.AddListener(ResetPoint);

        for (int i = 0; i < 6; i++)
        {
            var obj = Instantiate(_template.gameObject, _trTemplateParant);
            var com = obj.GetComponent<LevelPointTemplate>();

            _templates.Add(com);
        }

        _templates[0].Initialize(eStats.HP, Plus, Minus, Result);
        _templates[1].Initialize(eStats.MP, Plus, Minus, Result);
        _templates[2].Initialize(eStats.AP, Plus, Minus, Result);
        _templates[3].Initialize(eStats.Attack, Plus, Minus, Result);
        _templates[4].Initialize(eStats.Defence, Plus, Minus, Result);
        _templates[5].Initialize(eStats.Vision, Plus, Minus, Result);

        _buttonReset.gameObject.SetActive(true);
        _template.gameObject.SetActive(false);

        this.gameObject.SetActive(false);
    }

    public void Open(int maxPoint, UserData userData, Action<UserData> onResultCallback)
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.MenuOpen);

        if (onResultCallback != null)
        {
            _onResultCallback = onResultCallback;
        }

        _templates[0].SetPoint(userData.stats.hp.point);
        _templates[1].SetPoint(userData.stats.mp.point);
        _templates[2].SetPoint(userData.stats.ap.point);
        _templates[3].SetPoint(userData.stats.attack.point);
        _templates[4].SetPoint(userData.stats.defence.point);
        _templates[5].SetPoint(userData.stats.vision.current);

        _textMaxPoint.text = maxPoint.ToString();

        _userData = userData;
        _maxPoint = maxPoint;
        _point = maxPoint;

        this.gameObject.SetActive(true);

        GameManager.instance.tools.Move_Anchor_XY(eUiDir.Y, this.GetComponent<RectTransform>(), 0f, 0.5f, 0, Ease.OutBack, null);
    }

    public void Close()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);
        GameManager.instance.soundManager.PlaySfx(eSfx.MenuClose);

        if (_point > 0)
        {
            UiManager.instance.OpenPopup(string.Empty, "아직 사용 가능한 포인트가 남았습니다", string.Empty, null);

            return;
        }

        UiManager.instance.OpenPopup(string.Empty, "적용하시겠습니까?", string.Empty, string.Empty, () =>
        {
            _onResultCallback?.Invoke(_userData);

            GameManager.instance.tools.Move_Anchor_XY(eUiDir.Y, this.GetComponent<RectTransform>(), 1800f, 0.5f, 0, Ease.InBack, () =>
            {
                this.gameObject.SetActive(false);

                foreach (var item in _templates)
                {
                    item.Result();
                }

                _onResultCallback?.Invoke(_userData);
            });
        }, null);
    }

    private void OnClose()
    {
        _onCloseCallback?.Invoke();
    }

    private void ResetPoint()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        foreach (var item in _templates)
        {
            item.ResetPoint();
        }

        _point = _maxPoint;
        _textMaxPoint.text = _point.ToString();
    }

    private void Plus(Action<bool> onResultCallback)
    {
        if(_point == 0)
        {
            onResultCallback?.Invoke(false);

            return;
        }

        _point -= 1;
        _textMaxPoint.text = _point.ToString();

        onResultCallback?.Invoke(true);
    }

    private void Minus(bool isOri, Action<bool> onResultCallback)
    {
        if (_point == _maxPoint)
        {
            onResultCallback?.Invoke(false);

            return;
        }

        if(isOri == true)
        {
            return;
        }

        _point += 1;
        _textMaxPoint.text = _point.ToString();

        onResultCallback?.Invoke(true);
    }

    private void Result(eStats type, int point)
    {
        switch(type)
        {
            case eStats.HP:
                _userData.stats.hp.point = (short)point;
                break;

            case eStats.MP:
                _userData.stats.mp.point = (short)point;
                break;

            case eStats.AP:
                _userData.stats.ap.point = (short)point;
                break;

            case eStats.Attack:
                _userData.stats.attack.point = (short)point;
                break;

            case eStats.Defence:
                _userData.stats.defence.point = (short)point;
                break;

            case eStats.Vision:
                _userData.stats.vision.current = (short)point;
                break;

            case eStats.AttackRange:
                _userData.stats.attackRange.point = (short)point;
                break;
        }
    }
}
