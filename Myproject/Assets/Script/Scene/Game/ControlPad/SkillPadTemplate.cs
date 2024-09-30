using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SkillPadTemplate : MonoBehaviour
{
    [SerializeField] private Button buttonSelect = null;
    [SerializeField] private Image _imageSkill = null;

    private Action<int> _onViewInfoCallback;

    private int _id = -1;

    public void Initialize(int id, Action<int> onViewInfoCallback)
    {
        if (onViewInfoCallback != null)
        {
            _onViewInfoCallback = onViewInfoCallback;
        }

        buttonSelect.onClick.AddListener(OnClick);

        _id = id;

        GetImage();

        this.gameObject.SetActive(false);
    }

    private void GetImage()
    {

    }

    private void OnClick()
    {
        _onViewInfoCallback?.Invoke(_id);
    }
}
