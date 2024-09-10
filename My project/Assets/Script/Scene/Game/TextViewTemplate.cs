using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextViewTemplate : MonoBehaviour
{
    [SerializeField] private TMP_Text _textLabel = null;

    public void Initialize()
    {
        _textLabel.text = string.Empty;

        this.gameObject.SetActive(false);
    }

    public void SetTemplate(string label)
    {
        _textLabel.text = label;

        this.gameObject.SetActive(true);
    }
}
