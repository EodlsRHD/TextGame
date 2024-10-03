using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PadTemplate : MonoBehaviour
{
    [SerializeField] private Button buttonSelect = null;
    [SerializeField] private Image _image = null;

    private Action<int> _onViewInfoCallback;

    private DataManager.Skill_Data _skillData = null;
    private DataManager.Item_Data _itemData = null;

    private int _id = -1;

    public void Initialize(Action<int> onViewInfoCallback)
    {
        if (onViewInfoCallback != null)
        {
            _onViewInfoCallback = onViewInfoCallback;
        }

        buttonSelect.onClick.AddListener(OnClick);
        this.gameObject.SetActive(false);
    }

    public void Set(DataManager.Skill_Data skillData)
    {
        _skillData = skillData;

        GetImage(eControl.Skill, _skillData.id);

        this.gameObject.SetActive(true);
    }

    public void Set(DataManager.Item_Data itemData)
    {
        _itemData = itemData;

        GetImage(eControl.Skill, _itemData.id);

        this.gameObject.SetActive(true);
    }

    private void GetImage(eControl type, int id)
    {
        if(type == eControl.Skill)
        {


            return;
        }

        // item
    }

    private void OnClick()
    {
        _onViewInfoCallback?.Invoke(_id);
    }
}
