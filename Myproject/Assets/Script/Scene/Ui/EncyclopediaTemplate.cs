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

    private DataManager.Creature_Data _creature = null;
    private DataManager.Item_Data _item = null;

    public void Initialize()
    {
        _imageThumbnail.gameObject.SetActive(false); // Update List
        this.gameObject.SetActive(false);
    }

    public void Set(DataManager.Creature_Data data)
    {
        _creature = data;

        _textName.text = _creature.name;
        _textDescription.text = _creature.description;
    }

    public void Set(DataManager.Item_Data data)
    {
        _item = data;

        _textName.text = _item.name;
        _textDescription.text = _item.description;
    }

    public void Set() // Achievements
    {

    }

    public void Active(bool isActive)
    {
        this.gameObject.SetActive(isActive);
    }

    public void Remove()
    {

    }
}
