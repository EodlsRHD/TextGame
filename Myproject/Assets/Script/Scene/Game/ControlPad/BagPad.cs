using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BagPad : MonoBehaviour
{
    [SerializeField] private Transform _trTemplateParant = null;
    [SerializeField] private PadTemplate _template = null;

    [SerializeField] private GameObject _objEmptyLabel = null;

    private UserData _data = null;

    private Action<string, string> _onOpenInformationCallback = null;

    private int _id = -1;

    public void Initialize(Action<string, string> onOpenInformationCallback)
    {
        if (onOpenInformationCallback != null)
        {
            _onOpenInformationCallback = onOpenInformationCallback;
        }

        _template.Initialize(ViewInfo);

        _objEmptyLabel.SetActive(false);
        this.gameObject.SetActive(false);
    }

    public void Open(UserData data)
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
    }

    private void MakeList()
    {
        if (_data.data.itemIndexs.Count == 0)
        {
            _objEmptyLabel.SetActive(true);
        }

        DeleteTemplate();

        foreach (var id in _data.data.itemIndexs)
        {
            var obj = Instantiate(_template, _trTemplateParant);
            var com = obj.GetComponent<PadTemplate>();

            com.Initialize(ViewInfo);
            com.Set(GameManager.instance.dataManager.GetItemData(id));
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

    private void ViewInfo(int id, string name, string des, bool isUse)
    {
        _id = id;

        _onOpenInformationCallback?.Invoke(name, des);
    }
}
