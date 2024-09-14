using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockTemplate : MonoBehaviour
{
    [SerializeField] private Image _imageBlock = null;
    [SerializeField] private TMPro.TMP_Text _text = null;

    public void Initialize()
    {
        this.gameObject.SetActive(false);
    }

    public void SetTemplate(int index, DataManager.Block_Data blockData)
    {
        _text.text = index.ToString();

        SetColor(blockData.isWalkable == true ? Color.white : Color.black);

        if (blockData.isMonster == true)
        {
            SetColor(Color.red);
        }

        if (blockData.isUser == true)
        {
            SetColor(Color.green);
        }

        this.gameObject.SetActive(true);
    }

    public void EnterOrExit(int index, eDoorway doorway)
    {
        _text.text = index.ToString();

        if (doorway == eDoorway.Enter)
        {
            SetColor(Color.blue);
        }

        if (doorway == eDoorway.Exit)
        {
            SetColor(Color.magenta);
        }

        this.gameObject.SetActive(true);
    }

    private void SetColor(Color color)
    {
        _imageBlock.color = color;
    }
}
