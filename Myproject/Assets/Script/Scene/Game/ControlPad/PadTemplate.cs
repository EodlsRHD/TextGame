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
    [SerializeField] private TMP_Text _textCoolDown = null;

    private Action<int, string, string, bool> _onViewInfoCallback;

    private DataManager.Skill_Data _skillData = null;
    private DataManager.Item_Data _itemData = null;

    private bool _isItem = false;

    public void Initialize(Action<int, string, string, bool> onViewInfoCallback)
    {
        if (onViewInfoCallback != null)
        {
            _onViewInfoCallback = onViewInfoCallback;
        }

        _isItem = false;

        _buttonSelect.onClick.AddListener(OnClick);
        this.gameObject.SetActive(false);
    }

    public void Set(DataManager.Skill_Data skillData, int coolDown, bool isUse)
    {
        _buttonSelect.interactable = !isUse;
        _textCoolDown.gameObject.SetActive(isUse);

        if (isUse == true)
        {
            _textCoolDown.text = coolDown.ToString();
        }

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
            _onViewInfoCallback?.Invoke(_itemData.id, _itemData.name, _itemData.description, true);

            return;
        }

        _onViewInfoCallback?.Invoke(_skillData.id, _skillData.name, _skillData.description, !_buttonSelect.interactable);

    }
}
