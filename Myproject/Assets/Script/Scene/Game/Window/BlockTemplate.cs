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

    public void SetFog(bool isNearby, DataManager.NodeData blockData)
    {
        if (isNearby == true)
        {
            _isRresrarch = true;
        }

        Color color = new Color();

        if (isNearby == false && _isRresrarch == false)
        {
            if (blockData.isUser == true)
            {
                _imageBlock.enabled = false;
                color.a = 0f;
            }
            else
            {
                _imageBlock.enabled = true;
                _imageBlock.sprite = GameManager.instance.dataManager.GetCreatureSprite(eMapObject.Fog);
                color = Color.gray;
                color.a = 1f;
            }
        }
        else if (isNearby == false && _isRresrarch == true)
        {
            _imageBlock.enabled = true;
            _imageBlock.sprite = GameManager.instance.dataManager.GetCreatureSprite(eMapObject.Fog);
            color = Color.black;
            color.a = 0.2f;
        }
        else if(isNearby == true)
        {
            _imageBlock.enabled = false;
            color.a = 0f;
        }

        SetColor(color);
        this.gameObject.SetActive(true);
    }

    public void SetObject(bool isExit, DataManager.NodeData blockData)
    {
        Color color = new Color();

        if (blockData.isMonster == true)
        {
            _imageBlock.enabled = true;
            _imageBlock.sprite = GameManager.instance.dataManager.GetCreatureSprite(eMapObject.Monster);
            color = Color.white;
            color.a = 1f;
        }

        if (blockData.isGuide == true)
        {
            _imageBlock.enabled = true;
            _imageBlock.sprite = GameManager.instance.dataManager.GetCreatureSprite(eMapObject.Guide);
            color = Color.white;
            color.a = 1f;
        }

        if (blockData.isItem == true)
        {
            _imageBlock.enabled = true;
            _imageBlock.sprite = GameManager.instance.dataManager.GetCreatureSprite(eMapObject.Item);
            color = Color.white;
            color.a = 1f;
        }

        if (blockData.isShop == true)
        {
            _imageBlock.enabled = true;
            _imageBlock.sprite = GameManager.instance.dataManager.GetCreatureSprite(eMapObject.Shop);
            color = Color.white;
            color.a = 1f;
        }

        if (blockData.isBonfire == true)
        {
            _imageBlock.enabled = true;

            if (IngameManager.instance.saveData.mapData.npcDatas.Find(x => x.currentNodeIndex == blockData.index).isUseBonfire == true)
            {
                _imageBlock.sprite = GameManager.instance.dataManager.GetCreatureSprite(eMapObject.UseBonfire);
            }
            else
            {
                _imageBlock.sprite = GameManager.instance.dataManager.GetCreatureSprite(eMapObject.Bonfire);
            }

            color = Color.white;
            color.a = 1f;
        }

        if (isExit == true)
        {
            _imageBlock.enabled = true;
            _imageBlock.sprite = GameManager.instance.dataManager.GetCreatureSprite(eMapObject.Exit);
            color = Color.white;
            color.a = 1f;
        }

        if (blockData.isUser == true)
        {
            _imageBlock.enabled = true;
            _imageBlock.sprite = GameManager.instance.dataManager.GetCreatureSprite(eMapObject.Player);
            color = Color.white;
            color.a = 1f;
        }

        SetColor(color);
        this.gameObject.SetActive(true);
    }

    public void SetTile(DataManager.NodeData blockData)
    {
        Color color = new Color();

        if(blockData.isWalkable == false)
        {
            _imageBlock.enabled = true;
            _imageBlock.sprite = GameManager.instance.dataManager.GetCreatureSprite(eMapObject.Blocker);
            color = Color.white;
            color.a = 1f;
        }
        else if(blockData.isWalkable == true)
        {
            _imageBlock.enabled = true;
            _imageBlock.sprite = GameManager.instance.dataManager.GetCreatureSprite(eMapObject.Ground);
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
            color.a *= 0.8f;
        }

        _imageBlock.color = color;
    }
}
