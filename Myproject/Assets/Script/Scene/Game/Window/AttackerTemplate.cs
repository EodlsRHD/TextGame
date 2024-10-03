using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AttackerTemplate : MonoBehaviour
{
    [SerializeField] private TMP_Text _textShape = null;
    [SerializeField] private TMP_Text _textNum = null;

    [SerializeField] private Image _imageBack = null;

    private eCardShape _shapeType = eCardShape.Non;
    private int _num = 0;

    public eCardShape Shape
    {
        get { return _shapeType; }
    }

    public int Num
    {
        get { return _num; }
    }

    public string Name
    {
        get { return _textShape.text + _textNum.text; }
    }

    public void Initialize()
    {
        _imageBack.gameObject.SetActive(false);
        this.gameObject.SetActive(false);
    }

    public void Set(eCardShape shapeType, string shape, int num)
    {
        _shapeType = shapeType;
        _num = num;

        _textShape.text = shape;

        if(num == 1)
        {
            _num = 14;
            _textNum.text = "A";
        }
        else if(num == 11)
        {
            _textNum.text = "J";
        }
        else if(num == 12)
        {
            _textNum.text = "Q";
        }
        else if(num == 13)
        {
            _textNum.text = "K";
        }
        else
        {
            _textNum.text = num.ToString();
        }

        if(shapeType == eCardShape.Spade || shapeType == eCardShape. Clob)
        {
            _textShape.color = Color.black;
            _textNum.color = Color.black;
        }
        else
        {
            _textShape.color = Color.red;
            _textNum.color = Color.red;
        } 
    }

    public void ChangePositionAndActive(Transform tr, bool isActive)
    {
        this.gameObject.transform.SetParent(tr);
        this.gameObject.SetActive(isActive);
    }

    public void HideAndSeek(bool isActive)
    {
        _imageBack.gameObject.SetActive(isActive);
    }
}
