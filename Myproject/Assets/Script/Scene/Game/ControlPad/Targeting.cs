using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;
using static UnityEditor.Progress;

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

    [SerializeField] private Action<int, eAttackDirection> _onResultCallback = null;

    private eAttackDirection _dir = eAttackDirection.Non;
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

    public void Open(eControl type, int id, Action<int, eAttackDirection> onResultCallback)
    {
        if(onResultCallback != null)
        {
            _onResultCallback = onResultCallback;
        }

        if(type == eControl.Bag)
        {
            var item = GameManager.instance.dataManager.GetItemData(id);
            
            if(item.dir != eAttackDirection.DesignateDirection || item.dir != eAttackDirection.DesignateCoordination)
            {
                _onResultCallback?.Invoke(-1, eAttackDirection.Non);

                return;
            }

            if (item.dir == eAttackDirection.DesignateDirection)
            {
                _objDirection.SetActive(true);
            }
            else if (item.dir == eAttackDirection.DesignateCoordination)
            {
                _objCoordination.SetActive(true);
            }
        }
        else if(type == eControl.Skill)
        {
            var skill = GameManager.instance.dataManager.GetskillData(id);

            if (skill.dir != eAttackDirection.DesignateDirection || skill.dir != eAttackDirection.DesignateCoordination)
            {
                _onResultCallback?.Invoke(-1, eAttackDirection.Non);

                return;
            }

            if (skill.dir == eAttackDirection.DesignateDirection)
            {
                _objDirection.SetActive(true);
            }
            else if (skill.dir == eAttackDirection.DesignateCoordination)
            {
                _objCoordination.SetActive(true);
            }
        }

        this.gameObject.SetActive(true);
        GameManager.instance.tools.Move_Anchor_XY(eDir.Y, this.GetComponent<RectTransform>(), 350f, 0.5f, 0, Ease.OutBack, null);
    }

    private void OnClose()
    {
        GameManager.instance.tools.Move_Anchor_XY(eDir.Y, this.GetComponent<RectTransform>(), -360f, 0.5f, 0, Ease.InBack, () =>
        {
            this.gameObject.SetActive(false);

            _objCoordination.SetActive(false);
            _objDirection.SetActive(false);

            _nodeIndex = -1;
            _dir = eAttackDirection.Non;
        });
    }

    private void OnSelect()
    {
        if (_dir != eAttackDirection.Non)
        {
            _onResultCallback?.Invoke(_nodeIndex, _dir);

            return;
        }

        int X = int.Parse(_inputXcoord.text);
        int Y = int.Parse(_inputYcoord.text);

        IngameManager.instance.CheckNode(X, Y, (coord, result) =>
        {
            if (result == false)
            {
                IngameManager.instance.UpdatePopup("이동할 수 없는 좌표입니다.");

                return;
            }

            _onResultCallback?.Invoke(coord, _dir);
        });
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
        _dir = eAttackDirection.Up;

        SetText("위");
    }

    private void OnLeft()
    {
        _dir = eAttackDirection.Left;

        SetText("왼");
    }

    private void OnRight()
    {
        _dir = eAttackDirection.Right;

        SetText("우");
    }

    private void OnDown()
    {
        _dir = eAttackDirection.Down;

        SetText("아래");
    }

    private void SetText(string content)
    {
        _textDirection.text = content;
    }
}
