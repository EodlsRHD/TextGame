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
    [SerializeField] private Button _buttomInfo = null;

    [Header("Skill")]
    [SerializeField] private Button _buttomSkill = null;

    [Header("Rest")]
    [SerializeField] private Button _buttomRest = null;

    [Space(20)]

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

        _buttomInfo.onClick.AddListener(() => { OnAction(eControl.Info); });

        _buttomSkill.onClick.AddListener(() => { OnAction(eControl.Skill); });

        _buttomRest.onClick.AddListener(() => { OnAction(eControl.Rest); });

    }

    private void OnMove(eControl type)
    {
        _onMoveCallback?.Invoke(type);
    }

    private void OnAction(eControl type)
    {
        switch (type)
        {
            case eControl.Attack:

                break;

            case eControl.Defence:

                break;

            case eControl.Info:

                break;

            case eControl.Skill:

                break;

            case eControl.Rest:

                break;
        }
    }
}
