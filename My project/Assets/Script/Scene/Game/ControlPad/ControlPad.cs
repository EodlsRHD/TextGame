using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class ControlPad : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private Button _buttomUp = null;
    [SerializeField] private Button _buttomLeft = null;
    [SerializeField] private Button _buttomRight = null;
    [SerializeField] private Button _buttomDown = null;

    [Header("Attack")]
    [SerializeField] private Button _buttomAttack = null;

    [Header("Defence")]
    [SerializeField] private Button _buttomDefence = null;

    [Header("Info")]
    [SerializeField] private Button _buttomItem = null;

    [Header("Skill")]
    [SerializeField] private Button _buttomSkill = null;

    [Header("Rest")]
    [SerializeField] private Button _buttomRest = null;

    [Space(20)]

    [SerializeField] private SkillPad _skillPad = null;
    [SerializeField] private ItemPad _itemPad = null;

    private Action<eControl> _onMoveCallback = null;
    private Action<eControl> _onActionCallback = null;

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

        _buttomAttack.onClick.AddListener(() => { OnAction(eControl.Attack); });
        _buttomDefence.onClick.AddListener(() => { OnAction(eControl.Defence); });
        _buttomItem.onClick.AddListener(() => { OnAction(eControl.Item); });
        _buttomSkill.onClick.AddListener(() => { OnAction(eControl.Skill); });
        _buttomRest.onClick.AddListener(() => { OnAction(eControl.Rest); });

        _skillPad.Initialize();
        _itemPad.Initialize();
    }

    private void OnMove(eControl type)
    {
        _onMoveCallback?.Invoke(type);
    }

    private void OnAction(eControl type)
    {
        _onActionCallback?.Invoke(type);
    }

    public void Attack()
    {

    }

    public void Defence()
    {

    }

    public void Skill(Action<int> _onResultCallback)
    {
        int result = 0;

        _onResultCallback?.Invoke(result);
    }

    public void Item(Action<int> _onResultCallback)
    {
        int result = 0;

        _onResultCallback?.Invoke(result);
    }
}