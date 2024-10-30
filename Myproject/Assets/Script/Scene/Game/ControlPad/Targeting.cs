using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class Targeting : MonoBehaviour
{
    [SerializeField] private Button _buttonClose = null;
    [SerializeField] private Button _buttonSelect = null;

    [Space(10)]

    [Header("Coordination ")]
    [SerializeField] private GameObject _objCoordination = null;
    [SerializeField] private TMP_InputField _inputXcoord = null;
    [SerializeField] private TMP_InputField _inputYcoord = null;

    [Header("Direction")]
    [SerializeField] private GameObject _objDirection = null;
    [SerializeField] private TMP_Text _textDirection = null;
    [SerializeField] private Button _buttonUp = null;
    [SerializeField] private Button _buttonLeft = null;
    [SerializeField] private Button _buttonRight = null;
    [SerializeField] private Button _buttonDown = null;

    [SerializeField] private Action<int, eDir> _onResultCallback = null;

    private eDir _dir = eDir.Non;
    private int _nodeIndex = -1;

    public void Initialize()
    {
        _buttonClose.onClick.AddListener(OnClose);
        _buttonSelect.onClick.AddListener(OnSelect);

        _inputXcoord.onValueChanged.AddListener((text) => { OnCheckCoord(text, ref _inputXcoord); });
        _inputYcoord.onValueChanged.AddListener((text) => { OnCheckCoord(text, ref _inputYcoord); });

        _buttonUp.onClick.AddListener(OnUp);
        _buttonLeft.onClick.AddListener(OnLeft);
        _buttonRight.onClick.AddListener(OnRight);
        _buttonDown.onClick.AddListener(OnDown);

        _textDirection.text = string.Empty;

        _objCoordination.SetActive(false);
        _objDirection.SetActive(false);
        this.gameObject.SetActive(false);
    }

    public void Open(eControl type, int id, Action<int, eDir> onResultCallback)
    {
        if (onResultCallback != null)
        {
            _onResultCallback = onResultCallback;
        }

        if(type == eControl.Bag)
        {
            var item = GameManager.instance.dataManager.GetItemData(id);

            if (item == null)
            {
                _onResultCallback?.Invoke(-1, eDir.Non);

                return;
            }

            if(item.tool.dir == eDir.All)
            {
                _onResultCallback?.Invoke(-1, eDir.All);

                return;
            }

            if (item.tool.dir == eDir.DesignateDirection)
            {
                _objDirection.SetActive(true);
            }
            else if (item.tool.dir == eDir.DesignateCoordination)
            {
                _objCoordination.SetActive(true);
            }
            else
            {
                _onResultCallback?.Invoke(-1, eDir.Non);

                return;
            }
        }
        else if(type == eControl.Skill)
        {
            var skill = GameManager.instance.dataManager.GetskillData(id);

            if(skill == null)
            {
                _onResultCallback?.Invoke(-1, eDir.Non);

                return;
            }

            if(IngameManager.instance.saveData.userData.stats.mp.current < skill.useMp)
            {
                IngameManager.instance.UpdateText("마나가 부족합니다.");

                return;
            }

            if (skill.tool.dir == eDir.DesignateDirection)
            {
                _objDirection.SetActive(true);
            }
            else if (skill.tool.dir == eDir.DesignateCoordination)
            {
                _objCoordination.SetActive(true);
            }
            else
            {
                _onResultCallback?.Invoke(-1, eDir.Non);

                return;
            }
        }

        GameManager.instance.soundManager.PlaySfx(eSfx.MenuOpen);

        this.gameObject.SetActive(true);
        GameManager.instance.tools.Move_Anchor_XY(eUiDir.Y, this.GetComponent<RectTransform>(), 350f, 0.5f, 0, Ease.OutBack, null);
    }

    private void OnClose()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.MenuClose);

        GameManager.instance.tools.Move_Anchor_XY(eUiDir.Y, this.GetComponent<RectTransform>(), -360f, 0.5f, 0, Ease.InBack, () =>
        {
            this.gameObject.SetActive(false);

            _objCoordination.SetActive(false);
            _objDirection.SetActive(false);

            _inputXcoord.text = string.Empty;
            _inputYcoord.text = string.Empty;

            _nodeIndex = -1;
            _dir = eDir.Non;
            SetText(string.Empty);
        });
    }

    private void OnSelect()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        if (_dir != eDir.Non)
        {
            _onResultCallback?.Invoke(_nodeIndex, _dir);
            OnClose();

            return;
        }

        int X = int.Parse(_inputXcoord.text);
        int Y = int.Parse(_inputYcoord.text);

        int index = X + (IngameManager.instance.saveData.mapData.mapSize * Y);
        _onResultCallback?.Invoke(index, _dir);

        OnClose();
    }

    private void OnCheckCoord(string coord, ref TMP_InputField input)
    {
        int Coord = int.Parse(coord);

        if(Coord > 8)
        {
            input.text = "8";
        }

        if(Coord < 0)
        {
            input.text = "0";
        }
    }

    private void OnUp()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        _dir = eDir.Up;

        SetText("위");
    }

    private void OnLeft()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        _dir = eDir.Left;

        SetText("왼");
    }

    private void OnRight()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        _dir = eDir.Right;

        SetText("우");
    }

    private void OnDown()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        _dir = eDir.Down;

        SetText("아래");
    }

    private void SetText(string content)
    {
        _textDirection.text = content;
    }
}
