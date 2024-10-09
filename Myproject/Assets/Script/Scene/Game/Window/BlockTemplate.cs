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
        _imageBlock.enabled = true;

        this.gameObject.SetActive(false);
    }

    public void SetTemplate(bool isNearby, bool isExit, DataManager.Node_Data blockData)
    {
        _imageBlock.enabled = true;

        if (isNearby == true)
        {
            _isRresrarch = true;
        }

        Color color = new Color();
        color = Color.white;

        if (isNearby == false && _isRresrarch == false)
        {
            _imageBlock.enabled = true;
            _imageBlock.sprite = GameManager.instance.dataManager.GetCreatureSprite(eCreature.Fog);
            color = Color.gray;
            color.a = 0.7f;

        }
        else if(isNearby == false && _isRresrarch == true)
        {
            if (blockData.isWalkable == true)
            {
                _imageBlock.sprite = GameManager.instance.dataManager.GetCreatureSprite(eCreature.Fog);
                color = Color.white;
                color.a = 0.2f;
            }
            else if (blockData.isWalkable == false)
            {
                _imageBlock.enabled = true;
                _imageBlock.sprite = GameManager.instance.dataManager.GetCreatureSprite(eCreature.Blocker);
                color = Color.white;
                color.a = 0.5f;
            }

            if (isExit == true)
            {
                _imageBlock.enabled = true;
                _imageBlock.sprite = GameManager.instance.dataManager.GetCreatureSprite(eCreature.Exit);
                color = Color.red;
                color.a = 0.5f;
            }
        }
        else if(isNearby == true && isNearby == true)
        {
            if (blockData.isWalkable == true)
            {
                _imageBlock.enabled = false;
                color.a = 1f;
            }
            else if (blockData.isWalkable == false)
            {
                _imageBlock.enabled = true;
                _imageBlock.sprite = GameManager.instance.dataManager.GetCreatureSprite(eCreature.Blocker);
                color = Color.white;
                color.a = 1f;
            }

            if (blockData.isMonster == true)
            {
                _imageBlock.enabled = true;
                _imageBlock.sprite = GameManager.instance.dataManager.GetCreatureSprite(eCreature.Monster);
                color = Color.white;
                color.a = 1f;
            }

            if (blockData.isShop == true)
            {
                _imageBlock.enabled = true;
                _imageBlock.sprite = GameManager.instance.dataManager.GetCreatureSprite(eCreature.Shop);
                color = Color.white;
                color.a = 1f;
            }

            if (isExit == true)
            {
                _imageBlock.enabled = true;
                _imageBlock.sprite = GameManager.instance.dataManager.GetCreatureSprite(eCreature.Exit);
                color = Color.red;
                color.a = 1f;
            }
        }

        if (blockData.isUser == true)
        {
            _imageBlock.enabled = true;
            _imageBlock.sprite = GameManager.instance.dataManager.GetCreatureSprite(eCreature.Player);
            color = Color.white;
            color.a = 1f;
        }

        SetColor(color);
        this.gameObject.SetActive(true);
    }

    public void RemoveTemplate()
    {
        _isRresrarch = false;
        _imageBlock.enabled = true;

        this.gameObject.SetActive(false);
    }

    private void SetColor(Color color)
    {
        if(GameManager.instance.isMapBackgroundUpdate == true)
        {
            color.a = 0.7f;
        }

        _imageBlock.color = color;
    }
}
