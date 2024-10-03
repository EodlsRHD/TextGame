using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SkillPad : MonoBehaviour
{
    [SerializeField] private Transform _trTemplateParant = null;
    [SerializeField] private PadTemplate _template = null;

    [SerializeField] private TMP_Text _textName = null;
    [SerializeField] private TMP_Text _textDescription = null;

    [SerializeField] private GameObject _objEmptyLabel = null;

    private DataManager.User_Data _data = null;

    private int _id = -1;

    public void Initialize()
    {
        _template.Initialize(ViewInfo);

        _textName.text = string.Empty;
        _textDescription.text = string.Empty;

        _objEmptyLabel.SetActive(false);
        this.gameObject.SetActive(false);
    }

    public void Open(DataManager.User_Data data)
    {
        _data = data;

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
        if(_data.skillDataIndexs.Count == 0)
        {
            _objEmptyLabel.SetActive(true);
        }

        foreach (var id in _data.skillDataIndexs)
        {
            var obj = Instantiate(_template, _trTemplateParant);
            var com = obj.GetComponent<PadTemplate>();

            com.Set(GameManager.instance.dataManager.GetskillData(id));
        }
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
