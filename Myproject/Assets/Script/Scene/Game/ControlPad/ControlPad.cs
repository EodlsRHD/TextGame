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

    [SerializeField] private GameObject _objControlPad = null;
    [SerializeField] private TMP_Text _textTitle = null;
    [SerializeField] private Button _buttonCloseSecControlPad = null;
    [SerializeField] private Button _buttonUseSecControlPad = null;

    [Space(10)]

    [SerializeField] private SkillPad _skillPad = null;
    [SerializeField] private BagPad _BagPad = null;

    private Action<eControl> _onMoveCallback = null;
    private Action<eControl> _onActionCallback = null;

    private Action<int> _onResultCallback = null;

    private DataManager.User_Data _data;

    private eControl _eOpenPad = eControl.Non;

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

        _buttonCloseSecControlPad.onClick.AddListener(OnCloseSecControlPad);
        _buttonUseSecControlPad.onClick.AddListener(OnUseSecControlPad);

        _objControlPad.SetActive(false);
        _skillPad.Initialize();
        _BagPad.Initialize();
    }

    private void OnMove(eControl type)
    {
        _onMoveCallback?.Invoke(type);
    }

    private void OnAction(eControl type)
    {
        _onActionCallback?.Invoke(type);
    }
    
    public void Skill(DataManager.User_Data data, Action<int> onSkillResultCallback)
    {
        _data = data;

        if(onSkillResultCallback != null)
        {
            _onResultCallback = onSkillResultCallback;
        }

        OnOpenSecContolPad(eControl.Skill);
    }

    public void Bag(DataManager.User_Data data, Action<int> onBagResultCallback)
    {
        _data = data;

        if (onBagResultCallback != null)
        {
            _onResultCallback = onBagResultCallback;
        }

        OnOpenSecContolPad(eControl.Bag);
    }

    private void OnOpenSecContolPad(eControl type)
    {
        _textTitle.text = type.ToString();
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

        _objControlPad.SetActive(true);
    }

    private void OnUseSecControlPad()
    {
        int value = -1;

        switch (_eOpenPad)
        {
            case eControl.Skill:
                value = _skillPad.Use();
                break;

            case eControl.Bag:
                value = _BagPad.Use();
                break;
        }

        _onResultCallback?.Invoke(value);
        _onResultCallback = null;
    }

    private void OnCloseSecControlPad()
    {
        _objControlPad.SetActive(false);

        switch (_eOpenPad)
        {
            case eControl.Skill:
                _skillPad.Close();
                break;

            case eControl.Bag:
                _BagPad.Close();
                break;
        }

        _textTitle.text = string.Empty;
        _eOpenPad = eControl.Non;
    }
}