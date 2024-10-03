using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BagPad : MonoBehaviour
{
    [SerializeField] private Transform _trTemplateParant = null;
    [SerializeField] private PadTemplate _template = null;

    [SerializeField] private TMP_Text _textName = null;
    [SerializeField] private TMP_Text _textDescription = null;

    private int _id = -1;

    public void Initialize()
    {
        //_template.Initialize();

        this.gameObject.SetActive(false);
    }

    public void Open(DataManager.User_Data data)
    {
        this.gameObject.SetActive(true);
    }

    public int Use()
    {
        return _id;
    }

    public void Close()
    {
        this.gameObject.SetActive(false);
    }
}
