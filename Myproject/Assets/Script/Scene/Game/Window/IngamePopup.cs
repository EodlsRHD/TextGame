using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class IngamePopup : MonoBehaviour
{
    [SerializeField] private TextViewTemplate _template = null;
    [SerializeField] private Transform _trTemplateParant = null;

    private TextViewTemplate _currentTemplate = null;
    private Coroutine _coTimer = null;
    private float timer = 0;

    public void Initialize()
    {
        _template.Initialize();

        var obj = Instantiate(_template, _trTemplateParant);
        _currentTemplate = obj.GetComponent<TextViewTemplate>();
    }
    
    public void UpdateText(string content)
    {
        if(timer != 0)
        {
            _currentTemplate.RemoveLabel();
            StopCoroutine(_coTimer);
        }

        _currentTemplate.SetTemplate_NonTimeStemp(content);
        timer = 2f;
        _coTimer = StartCoroutine(Co_Timer());
    }

    IEnumerator Co_Timer()
    {
        while(timer > 0)
        {
            timer -= 0.1f;

            yield return new WaitForSeconds(0.1f);
        }

        _currentTemplate.RemoveLabel();
    }
}
