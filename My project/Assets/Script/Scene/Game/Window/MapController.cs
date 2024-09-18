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

    private List<BlockTemplate> _pool = new List<BlockTemplate>();

    private Action _onCloseCallback = null;

    public void Initialize()
    {
        _buttonCloseViewMap?.onClick.AddListener(CloseMap);

        _tempalte.Initialize();

        for (int i = 0; i < 64; i++)
        {
            var obj = Instantiate(_tempalte, _trTemplateParant);
            var com = obj.GetComponent<BlockTemplate>();

            _pool.Add(com);
        }

        this.gameObject.SetActive(false);
    }

    public void SetMap(DataManager.Save_Data saveData)
    {
        var blockData = saveData.mapData.blockDatas;

        for (int i = 0; i < blockData.Count; i++)
        {
            var template = _pool[i];

            if (i == saveData.mapData.enterBlockIndex)
            {
                template.EnterOrExit(i, eDoorway.Enter);

                continue;
            }

            if (i == saveData.mapData.exitBlockIndex)
            {
                template.EnterOrExit(i, eDoorway.Exit);

                continue;
            }
            
            template.SetTemplate(i, blockData[i]);
        }
    }

    public void UpdateData(DataManager.Save_Data saveData)
    {

    }

    public void OpenMap(Action onCallback)
    {
        if(onCallback != null)
        {
            _onCloseCallback = onCallback;
        }

        this.gameObject.SetActive(true);
    }

    private void CloseMap()
    {
        this.gameObject.SetActive(false);
        _onCloseCallback?.Invoke();
    }
}
