using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class ControlPad : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private Button _buttomUp = null;
    [SerializeField] private Button _buttomLeft = null;
    [SerializeField] private Button _buttomRight = null;
    [SerializeField] private Button _buttomDown = null;

    [Header("Defence")]
    [SerializeField] private Button _buttomDefence = null;

    [Header("Info")]
    [SerializeField] private Button _buttomItem = null;

    [Header("Skill")]
    [SerializeField] private Button _buttomSkill = null;

    [Header("Rest")]
    [SerializeField] private Button _buttomRest = null;

    [Header("Search nearby")]
    [SerializeField] private Button _buttomSearchNearby = null;

    [Space(20)]

    [SerializeField] private GameObject _objSkillAndBagPad = null;
    [SerializeField] private TMP_Text _textSkillAndBagPadTitle = null;
    [SerializeField] private Button _buttonCloseSkillAndBagPad = null;
    [SerializeField] private Button _buttonUseSkillAndBagPad = null;

    [Space(10)]

    [SerializeField] private SkillPad _skillPad = null;
    [SerializeField] private BagPad _BagPad = null;
    [SerializeField] private Targeting _targeting = null;

    [Header("Information")]
    [SerializeField] private GameObject _objInformation = null;
    [SerializeField] private TMP_Text _textName = null;
    [SerializeField] private TMP_Text _textDescription = null;

    private Action<eControl> _onMoveCallback = null;
    private Action<eControl> _onActionCallback = null;

    private Action<eTool, int> _onResultCallback = null;

    private UserData _data;

    private eControl _eOpenPad = eControl.Non;
    private bool _isOpen = false;

    public void Initialize(Action<eControl> onMoveCallback, Action<eControl> onActionCallback)
    {
        if(onMoveCallback != null)
        {
            _onMoveCallback = onMoveCallback;
        }

        if(onActionCallback != null)
        {
            _onActionCallback = onActionCallback;
        }

        _buttomUp.onClick.AddListener(() => { OnMove(eControl.Up); });
        _buttomLeft.onClick.AddListener(() => { OnMove(eControl.Left); });
        _buttomRight.onClick.AddListener(() => { OnMove(eControl.Right); });
        _buttomDown.onClick.AddListener(() => { OnMove(eControl.Down); });

        _buttomDefence.onClick.AddListener(() => { OnAction(eControl.Defence); });
        _buttomItem.onClick.AddListener(() => { OnAction(eControl.Bag); });
        _buttomSkill.onClick.AddListener(() => { OnAction(eControl.Skill); });
        _buttomRest.onClick.AddListener(() => { OnAction(eControl.Rest); });
        _buttomSearchNearby.onClick.AddListener(() => OnAction(eControl.SearchNearby));

        _buttonCloseSkillAndBagPad.onClick.AddListener(OnCloseSkillAndBagPad);
        _buttonUseSkillAndBagPad.onClick.AddListener(OnUseSkillAndBagPad);

        _objInformation.SetActive(false);
        _textName.text = string.Empty;
        _textDescription.text = string.Empty;

        _objSkillAndBagPad.SetActive(false);

        _skillPad.Initialize(OpenInformation);
        _BagPad.Initialize(OpenInformation);
        _targeting.Initialize();
    }

    private void OnMove(eControl type)
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        _onMoveCallback?.Invoke(type);
    }

    private void OnAction(eControl type)
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        _onActionCallback?.Invoke(type);
    }

    public void UpdateData(UserData data)
    {
        _data = data;

        OnOpenSkillAndBagPad(_eOpenPad);
    }

    public void Skill(UserData data, Action<eTool, int> onSkillResultCallback)
    {
        _data = data;

        if(onSkillResultCallback != null)
        {
            _onResultCallback = onSkillResultCallback;
        }

        OnOpenSkillAndBagPad(eControl.Skill);
    }

    public void Bag(UserData data, Action<eTool, int> onBagResultCallback)
    {
        _data = data;

        if (onBagResultCallback != null)
        {
            _onResultCallback = onBagResultCallback;
        }

        OnOpenSkillAndBagPad(eControl.Bag);
    }

    private void OnOpenSkillAndBagPad(eControl type)
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.MenuOpen);

        _textSkillAndBagPadTitle.text = type.ToString();
        _eOpenPad = type;

        switch (type)
        {
            case eControl.Skill:
                _skillPad.Open(_data);
                break;

            case eControl.Bag:
                _BagPad.Open(_data);
                break;
        } 

        _objSkillAndBagPad.SetActive(true);

        _isOpen = true;

        GameManager.instance.tools.Move_Anchor_XY(eUiDir.Y, _objSkillAndBagPad.GetComponent<RectTransform>(), 350f, 0.5f, 0, Ease.OutBack, null);
    }

    private void OnUseSkillAndBagPad()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        int value = -1;

        switch (_eOpenPad)
        {
            case eControl.Skill:
                {
                    value = _skillPad.Use();

                    _targeting.Open(_eOpenPad, value, (coordResult, dirResult) =>
                    {
                        if (coordResult == -1 && dirResult == eDir.Non)
                        {
                            _onResultCallback?.Invoke(eTool.Skill, value);
                            _onResultCallback = null;

                            return;
                        }

                        IngameManager.instance.SetDirCoord(coordResult, dirResult);

                        _onResultCallback?.Invoke(eTool.Skill, value);
                        _onResultCallback = null;
                    });
                }
                break;

            case eControl.Bag:
                {
                    value = _BagPad.Use();

                    _targeting.Open(_eOpenPad, value, (coordResult, dirResult) =>
                    {
                        if (coordResult == -1 && dirResult == eDir.Non)
                        {
                            _onResultCallback?.Invoke(eTool.Item, value);
                            _onResultCallback = null;

                            return;
                        }

                        IngameManager.instance.SetDirCoord(coordResult, dirResult);

                        _onResultCallback?.Invoke(eTool.Item, value);
                        _onResultCallback = null;
                    });
                }
                break;
        }

        CloseInformation();
    }

    private void OnCloseSkillAndBagPad()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);
        GameManager.instance.soundManager.PlaySfx(eSfx.MenuClose);

        if (_isOpen == false)
        {
            return;
        }

        _isOpen = false;

        CloseInformation();

        GameManager.instance.tools.Move_Anchor_XY(eUiDir.Y, _objSkillAndBagPad.GetComponent<RectTransform>(), -360f, 0.5f, 0, Ease.InBack, () =>
        {
            _objSkillAndBagPad.SetActive(false);

            switch (_eOpenPad)
            {
                case eControl.Skill:
                    _skillPad.Close();
                    break;

                case eControl.Bag:
                    _BagPad.Close();
                    break;
            }

            _textSkillAndBagPadTitle.text = string.Empty;
            _eOpenPad = eControl.Non;
        });
    }

    private void OpenInformation(string name, string description)
    {
        if (_isOpen == false)
        {
            return;
        }

        _textName.text = name;
        _textDescription.text = description;

        _objInformation.SetActive(true);

        GameManager.instance.tools.Move_Anchor_XY(eUiDir.X, _objInformation.GetComponent<RectTransform>(), -423f, 0.5f, 0, Ease.OutBack, null);
    }

    private void CloseInformation()
    {
        GameManager.instance.tools.Move_Anchor_XY(eUiDir.X, _objInformation.GetComponent<RectTransform>(), 447f, 0.5f, 0, Ease.InBack, () => 
        {
            _objInformation.SetActive(false);

            _textName.text = string.Empty;
            _textDescription.text = string.Empty;
        });
    }
}
