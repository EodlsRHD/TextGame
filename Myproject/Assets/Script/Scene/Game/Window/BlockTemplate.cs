using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockTemplate : MonoBehaviour
{
    [SerializeField] private Image _imageBlock = null;

    private bool _isRresrarch = false;

    public void Initialize()
    {
        _isRresrarch = false;

        this.gameObject.SetActive(false);
    }

    public void SetTemplate(bool isNearby, bool isExit, DataManager.Node_Data blockData)
    {
        if(isNearby == true)
        {
            _isRresrarch = true;
        }

        Color color = new Color();

        if (isNearby == false && _isRresrarch == false)
        {
            color = Color.gray;
            color.a = 1f;
        }
        else if(isNearby == false && _isRresrarch == true)
        {
            if (blockData.isWalkable == true)
            {
                color = Color.white;
                color.a = 0.5f;
            }
            else if (blockData.isWalkable == false)
            {
                color = Color.black;
                color.a = 0.5f;
            }

            if (isExit == true)
            {
                color = Color.magenta;
                color.a = 0.5f;
            }
        }
        else if(isNearby == true && isNearby == true)
        {
            if (blockData.isWalkable == true)
            {
                color = Color.white;
                color.a = 1f;
            }
            else if (blockData.isWalkable == false)
            {
                color = Color.black;
                color.a = 1f;
            }

            if (blockData.isMonster == true)
            {
                color = Color.red;
                color.a = 1f;
            }

            if (blockData.isShop == true)
            {
                color = Color.yellow;
                color.a = 1f;
            }

            if (isExit == true)
            {
                color = Color.magenta;
                color.a = 1f;
            }
        }

        if (blockData.isUser == true)
        {
            color = Color.green;
            color.a = 255f;
        }

        SetColor(color);
        this.gameObject.SetActive(true);
    }

    public void RemoveTemplate()
    {
        _isRresrarch = false;
        this.gameObject.SetActive(false);
    }

    private void SetColor(Color color)
    {
        _imageBlock.color = color;
    }
}
