using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SkillPad : MonoBehaviour
{
    [SerializeField] private Transform _trTemplateParant = null;
    [SerializeField] private SkillPadTemplate _template = null;

    [SerializeField] private TMP_Text _textName = null;
    [SerializeField] private TMP_Text _textDescription = null;

    private int _id = -1;

    public void Initialize()
    {
        _template.Initialize(-1, ViewInfo);

        _textName.text = string.Empty;
        _textDescription.text = string.Empty;

        this.gameObject.SetActive(false);
    }

    public void Open(DataManager.User_Data data)
    {
        MakeList();

        this.gameObject.SetActive(true);
    }

    public int Use()
    {
        return _id;
    }

    public void Close()
    {
        this.gameObject.SetActive(false);

        DeleteTemplate();

        _textName.text = string.Empty;
        _textDescription.text = string.Empty;
    }

    private void MakeList()
    {

    }

    public void DeleteTemplate()
    {
        var child = _trTemplateParant.transform.GetComponentsInChildren<Transform>();

        foreach (var iter in child)
        {
            if (iter != _trTemplateParant.transform)
            {
                Destroy(iter.gameObject);
            }
        }

        _trTemplateParant.transform.DetachChildren();
    }

    private void ViewInfo(int id)
    {
        _id = id;
    }
}
