using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextView : MonoBehaviour
{
    [Header("Template"), SerializeField] private TextViewTemplate _template = null;
    [Header("Template Parant"), SerializeField] private Transform _trTemplateParant = null;

    private List<TextViewTemplate> _pool = new List<TextViewTemplate>();

    public void Initialize()
    {
        _template.Initialize();
    }

    public void UpdateText(string content)
    {
        var obj = Instantiate(_template, _trTemplateParant);
        var com = obj.GetComponent<TextViewTemplate>();

        com.SetTemplate(content);
    }
}
