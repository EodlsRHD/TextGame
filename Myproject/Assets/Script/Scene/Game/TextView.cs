using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextView : MonoBehaviour
{
    [Header("Template"), SerializeField] private TextViewTemplate _template = null;
    [Header("Template Parant"), SerializeField] private Transform _trTemplateParant = null;

    private RectTransform _rectTransform;

    public void Initialize()
    {
        _template.Initialize();

        _rectTransform = GetComponent<RectTransform>();
    }

    /// <summary>
    ///  Defult message
    /// </summary>
    public void UpdateText(string content)
    {
        InstantiateTemplate(content);
    }

    /// <summary>
    ///  Player Position
    /// </summary>
    public void UpdateText(DataManager.NodeData node)
    {
        string content = "플레이어 좌표 : " + node.x + " , " + node.y; 

        InstantiateTemplate(content);
    }

    public void UpdateTextViewHeight()
    {
        this.gameObject.SetActive(false);

        var offset = _rectTransform.offsetMax;

        if(GameManager.instance.isMapBackgroundUpdate == true)
        {
            offset = new Vector2(offset.x, -1020f);
        }
        else
        {
            offset = new Vector2(offset.x, -180f);
        }

        _rectTransform.offsetMax = offset;

        this.gameObject.SetActive(true);
    }

    /// <summary>
    ///  Find Monster
    /// </summary>
    public void UpdateText(eMapObject type, DataManager.NodeData monsterNode, DataManager.NodeData playerNode)
    {
        string dirStr = string.Empty;
        string findType = string.Empty;
        int x = monsterNode.x - playerNode.x;
        int y = monsterNode.y - playerNode.y;
        int distance = (int)Mathf.Sqrt(Mathf.Pow(x, 2) + Mathf.Pow(y, 2));

        if (x > 0 && y > 0) // right up
        {
            dirStr = "오른쪽 위";
        }

        if (x > 0 && y == 0) // right 
        {
            dirStr = "오른쪽";
        }

        if (x > 0 && y < 0) // right down
        {
            dirStr = "오른쪽 아래";
        }

        if (x == 0 && y < 0) // down
        {
            dirStr = "아래";
        }

        if (x < 0 && y < 0) // left down
        {
            dirStr = "왼쪽 아래";
        }

        if (x < 0 && y == 0) // left
        {
            dirStr = "왼쪽";
        }

        if (x < 0 && y > 0) // left up
        {
            dirStr = "왼쪽 위";
        }

        if (x == 0 && y > 0) // up
        {
            dirStr = "위";
        }

        switch(type)
        {
            case eMapObject.Monster:
                findType = "몬스터";
                break;

            case eMapObject.Item:
                findType = "아이템";
                break;

            case eMapObject.Shop:
                findType = "상인";
                break;

            case eMapObject.Bonfire:
                findType = "모닥불";
                break;

            case eMapObject.UseBonfire:
                findType = "꺼진 모닥불";
                break;

            case eMapObject.Exit:
                findType = "아래로 가는 계단";
                break;

            case eMapObject.Guide:
                findType = "길잡이";
                break;
        }

        string content = dirStr + " 방향, " + distance + " 거리에 " + findType + "(이)가 있습니다.";

        InstantiateTemplate(content);
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

    private void InstantiateTemplate(string content)
    {
        var obj = Instantiate(_template, _trTemplateParant);
        var com = obj.GetComponent<TextViewTemplate>();

        com.SetTemplate(content);
    }
}
