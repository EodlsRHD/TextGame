using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class PadTemplate : MonoBehaviour
{
    [SerializeField] private Button _buttonSelect = null;
    [SerializeField] private TMP_Text _textButtonLabel = null;

    private Action<string, string> _onViewInfoCallback;

    private DataManager.Skill_Data _skillData = null;
    private DataManager.Item_Data _itemData = null;

    private bool _isItem = false;
    private int _id = -1;

    public void Initialize(Action<string, string> onViewInfoCallback)
    {
        if (onViewInfoCallback != null)
        {
            _onViewInfoCallback = onViewInfoCallback;
        }

        _isItem = false;

        _buttonSelect.onClick.AddListener(OnClick);
        this.gameObject.SetActive(false);
    }

    public void Set(DataManager.Skill_Data skillData)
    {
        _isItem = false;
        _skillData = skillData;

        _textButtonLabel.text = skillData.name;

        this.gameObject.SetActive(true);
    }

    public void Set(DataManager.Item_Data itemData)
    {
        _isItem = true;
        _itemData = itemData;

        _textButtonLabel.text = itemData.name;

        this.gameObject.SetActive(true);
    }

    private void OnClick()
    {
        if(_isItem == true)
        {
            _onViewInfoCallback?.Invoke(_itemData.name, _itemData.description);

            return;
        }

        _onViewInfoCallback?.Invoke(_skillData.name, _skillData.description);

    }
}
