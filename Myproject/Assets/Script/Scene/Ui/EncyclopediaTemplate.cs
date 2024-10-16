using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class EncyclopediaTemplate : MonoBehaviour
{
    [SerializeField] private Button _button = null;

    [SerializeField] private Image _imageThumbnail = null;
    [SerializeField] private TMP_Text _textName = null;
    [SerializeField] private TMP_Text _textDescription = null;

    private Action<string, string> _onSetInformationCallback = null;

    private CreatureData _creature = null;
    private ItemData _item = null;
    private DataManager.Achievements_Data _acievement = null;

    public void Initialize(Action<string, string> onSetInformationCallback)
    {
        if(onSetInformationCallback != null)
        {
            _onSetInformationCallback = onSetInformationCallback;
        }

        _button.onClick.AddListener(OnInformation);

        _imageThumbnail.gameObject.SetActive(false); // Update List
        this.gameObject.SetActive(false);
    }

    public void Set(CreatureData data)
    {
        _creature = data;

        _textName.text = _creature.name;
        _textDescription.text = _creature.description;
    }

    public void Set(ItemData data)
    {
        _item = data;

        _textName.text = _item.name;
        _textDescription.text = _item.description;
    }

    public void Set(DataManager.Achievements_Data data)
    {
        _acievement = data;

        Debug.LogError(_acievement.isSuccess + "       " + _acievement.name);
        _imageThumbnail.gameObject.SetActive(_acievement.isSuccess);

        _textName.text = _acievement.name;
    }

    public void Active(bool isActive)
    {
        this.gameObject.SetActive(isActive);
    }

    private void OnInformation()
    {
        if(_creature != null)
        {
            _onSetInformationCallback?.Invoke(_creature.name, _creature.description);

            return;
        }

        if (_item != null)
        {
            _onSetInformationCallback?.Invoke(_item.name, _item.description);

            return;
        }

        if (_acievement != null)
        {
            _onSetInformationCallback?.Invoke(_acievement.name, _acievement.description);

            return;
        }
    }
}
