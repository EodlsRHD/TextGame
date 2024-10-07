using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TextViewTemplate : MonoBehaviour
{
    [SerializeField] private TMP_Text _textTime = null;
    [SerializeField] private TMP_Text _textLabel = null;

    public void Initialize()
    {
        if(_textTime != null)
        {
            _textTime.text = string.Empty;
        }

        _textLabel.text = string.Empty;

        this.gameObject.SetActive(false);
    }

    public void SetTemplate(string label)
    {
        //_textTime.text = "[" + DateTime.Now.ToString("HH:mm:ss") + "]";
        _textLabel.text = label;

        this.gameObject.SetActive(true);
    }

    public void SetTemplate_NonTimeStemp(string label)
    {
        _textLabel.text = label;

        this.gameObject.SetActive(true);
    }

    public void RemoveLabel()
    {
        _textLabel.text = string.Empty;

        this.gameObject.SetActive(false);
    }
}
