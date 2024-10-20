using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.VisualScripting;
using TMPro;

public class MapController : MonoBehaviour
{
    [SerializeField] private Button _buttonCloseViewMap = null;
    [SerializeField] private GameObject _map = null;

    [Header("Template"), SerializeField] private BlockTemplate _tempalte = null;

    [Header("Menu")]
    [SerializeField] private Transform _trTemplateParant_Fog = null;
    [SerializeField] private Transform _trTemplateParant_Creauture = null;
    [SerializeField] private Transform _trTemplateParant_Tile = null;

    [Header("Back"), SerializeField] private Transform _trBack = null;

    private List<BlockTemplate> _poolFog = new List<BlockTemplate>();
    private List<BlockTemplate> _poolCreature = new List<BlockTemplate>();
    private List<BlockTemplate> _poolTile = new List<BlockTemplate>();

    private Action _onCloseCallback = null;

    private bool _isOpen = false;

    private eDir _revealMapDir = eDir.Non;
    private List<int> _revealMapIndexs = new List<int>();

    public void Initialize(int mapSize)
    {
        _buttonCloseViewMap?.onClick.AddListener(CloseMap);

        _tempalte.Initialize();

        for (int i = 0; i < mapSize * mapSize; i++)
        {
            var obj = Instantiate(_tempalte, _trTemplateParant_Fog);
            var com = obj.GetComponent<BlockTemplate>();

            com.Initialize();
            _poolFog.Add(com);
        }

        for (int i = 0; i < mapSize * mapSize; i++)
        {
            var obj = Instantiate(_tempalte, _trTemplateParant_Creauture);
            var com = obj.GetComponent<BlockTemplate>();

            com.Initialize();
            _poolCreature.Add(com);
        }

        for (int i = 0; i < mapSize * mapSize; i++)
        {
            var obj = Instantiate(_tempalte, _trTemplateParant_Tile);
            var com = obj.GetComponent<BlockTemplate>();

            com.Initialize();
            _poolTile.Add(com);
        }

        _trBack.gameObject.SetActive(GameManager.instance.isMapBackgroundUpdate);

        _buttonCloseViewMap.gameObject.SetActive(false);
        this.gameObject.SetActive(false);
    }

    public void SetMap(DataManager.SaveData saveData)
    {
        NextTurn();

        _trBack.gameObject.SetActive(GameManager.instance.isMapBackgroundUpdate);

        var blockData = saveData.mapData.nodeDatas;

        for (int i = 0; i < blockData.Count; i++)
        {
            var templateFog = _poolFog[i];
            var templateCreature = _poolCreature[i];
            var templateTile = _poolTile[i];

            bool isExit = false;

            if (i == saveData.mapData.exitNodeIndex)
            {
                isExit = true;
            }

            templateFog.gameObject.transform.SetParent(_trTemplateParant_Fog);
            templateFog.SetFog(false, blockData[i]);

            templateCreature.gameObject.transform.SetParent(_trTemplateParant_Creauture);
            templateCreature.SetObject(isExit, blockData[i]);

            templateTile.gameObject.transform.SetParent(_trTemplateParant_Tile);
            templateTile.SetTile(blockData[i]);
        }
    }

    public void UpdateMapData(DataManager.SaveData saveData, List<int> nearbyIndexs)
    {
        _trBack.gameObject.SetActive(GameManager.instance.isMapBackgroundUpdate);

        if(GameManager.instance.isMapBackgroundUpdate == true)
        {
            _map.transform.SetParent(_trBack);
            _map.GetComponent<RectTransform>().localPosition = new Vector2(0, 0);
        }
        else
        {
            _map.transform.SetParent(this.transform);
            _map.GetComponent<RectTransform>().localPosition = new Vector2(0, 0);
        }

        var blockData = saveData.mapData.nodeDatas;

        for (int i = 0; i < blockData.Count; i++)
        {
            var templateFog = _poolFog[i];
            var templateCreature = _poolCreature[i];

            bool isNearby = false;
            bool isExit = false;

            if (i == saveData.mapData.exitNodeIndex)
            {
                isExit = true;
            }

            for (int j = 0; j < nearbyIndexs.Count; j++)
            {
                if (i == nearbyIndexs[j])
                {
                    isNearby = true;

                    break;
                }
            }

            if(_revealMapDir == eDir.All)
            {
                isNearby = true;
            }
            else if(_revealMapDir == eDir.Non && _revealMapDir != eDir.All)
            {
                for(int j = 0; j < _revealMapIndexs.Count; j++)
                {
                    if(i == _revealMapIndexs[j])
                    {
                        isNearby = true;

                        break;
                    }
                }
            }

            templateFog.gameObject.transform.SetParent(_trTemplateParant_Fog);
            templateFog.SetFog(isNearby, blockData[i]);

            templateCreature.gameObject.transform.SetParent(_trTemplateParant_Creauture);
            templateCreature.SetObject(isExit, blockData[i]);
        }
    }

    public void RevealMap(eDir dir, List<int> indexs)
    {
        _revealMapDir = dir;
        _revealMapIndexs = indexs;
    }

    public void NextTurn()
    {
        _revealMapDir = eDir.Non;

        if(_revealMapIndexs.Count != 0)
        {
            _revealMapIndexs.Clear();
        }
    }

    public void OpenMap(Action onCallback)
    {
        if (onCallback != null)
        {
            _onCloseCallback = onCallback;
        }

        GameManager.instance.soundManager.PlaySfx(eSfx.Map);

        this.gameObject.SetActive(true);
        _buttonCloseViewMap.gameObject.SetActive(true);

        _isOpen = true;

        GameManager.instance.tools.Move_Anchor_XY(eUiDir.Y, this.GetComponent<RectTransform>(), 276f, 0.5f, 0, Ease.OutBack, null);
    }

    private void CloseMap()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        if(_isOpen == false)
        {
            return;
        }

        _isOpen = false;

        Close(true);
    }

    public void Close(bool isCloseButton = false, Action onResultCallbacl = null)
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.Map);

        GameManager.instance.tools.Move_Anchor_XY(eUiDir.Y, this.GetComponent<RectTransform>(), 1800f, 0.5f, 0, Ease.InBack, () =>
        {
            _buttonCloseViewMap.gameObject.SetActive(false);
            this.gameObject.SetActive(false);

            onResultCallbacl?.Invoke();

            if (GameManager.instance.isMapBackgroundUpdate == false)
            {
                if (isCloseButton == false)
                {
                    RemoveTemplate();
                }
            }

            _onCloseCallback?.Invoke();
        });
    }

    private void RemoveTemplate()
    {
        for (int i = 0; i < _poolFog.Count; i++)
        {
            _poolFog[i].RemoveTemplate();
        }

        for (int i = 0; i < _poolCreature.Count; i++)
        {
            _poolCreature[i].RemoveTemplate();
        }

        for (int i = 0; i < _poolTile.Count; i++)
        {
            _poolTile[i].RemoveTemplate();
        }
    }
}
