using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class MapController : MonoBehaviour
{
    [SerializeField] private Button _buttonCloseViewMap = null;

    [Header("Template"), SerializeField] private BlockTemplate _tempalte = null;
    [SerializeField] private Transform _trTemplateParant = null;
    [SerializeField] private Transform _trTemplateParant_Back = null;

    private List<BlockTemplate> _pool = new List<BlockTemplate>();

    private Action _onCloseCallback = null;

    public void Initialize(int mapSize)
    {
        _buttonCloseViewMap?.onClick.AddListener(CloseMap);

        _tempalte.Initialize();

        for (int i = 0; i < mapSize * mapSize; i++)
        {
            var obj = Instantiate(_tempalte, _trTemplateParant);
            var com = obj.GetComponent<BlockTemplate>();

            com.Initialize();
            _pool.Add(com);
        }

        _trTemplateParant_Back.gameObject.SetActive(GameManager.instance.isMapBackgroundUpdate);

        this.gameObject.SetActive(false);
    }

    public void SetMap(DataManager.Save_Data saveData, List<int> nearbyIndexs)
    {
        _trTemplateParant_Back.gameObject.SetActive(GameManager.instance.isMapBackgroundUpdate);

        var blockData = saveData.mapData.nodeDatas;

        for (int i = 0; i < blockData.Count; i++)
        {
            var template = _pool[i];
            bool same = false;

            for (int j = 0; j < nearbyIndexs.Count; j++)
            {
                if(i == nearbyIndexs[j])
                {
                    same = true;
                    
                    break;
                }
            }

            template.gameObject.transform.SetParent(GameManager.instance.isMapBackgroundUpdate == false ? _trTemplateParant : _trTemplateParant_Back);
            template.SetTemplate(same, i == saveData.mapData.exitNodeIndex, blockData[i]);
        }
    }

    public void UpdateMapData(DataManager.Save_Data saveData, List<int> nearbyIndexs)
    {
        SetMap(saveData, nearbyIndexs);
    }

    public void OpenMap(Action onCallback)
    {
        if (onCallback != null)
        {
            _onCloseCallback = onCallback;
        }

        this.gameObject.SetActive(true);
    }

    private void CloseMap()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        this.gameObject.SetActive(false);
        _onCloseCallback?.Invoke();
    }

    public void Close()
    {
        this.gameObject.SetActive(false);
        _onCloseCallback?.Invoke();
    }

    public void RemoveTemplate()
    {
        for (int i = 0; i < _pool.Count; i++)
        {
            _pool[i].RemoveTemplate();
        }
    }
}
