using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "SO_Tutorial", menuName = "Scriptable Objects/SO_Tutorial")]
public class SO_TutorialData : ScriptableObject
{
    [SerializeField] private string _dataPath = string.Empty;
    [SerializeField] private List<Sprite> _images = new List<Sprite>(); 

    private List<TutorialData> _list = new List<TutorialData>();

    public void Initialize()
    {
        ReadData();
    }

    private void ReadData()
    {
        if (_list != null)
        {
            _list.Clear();
        }

        _list = new List<TutorialData>();

        string json = Resources.Load<TextAsset>(_dataPath).text;

        var respons = new
        {
            datas = new List<DataManager.Tutorial_Data>()
        };

        var result = JsonConvert.DeserializeAnonymousType(json, respons);

        for(int i = 0; i < result.datas.Count; i++)
        {
            TutorialData temp = new TutorialData(result.datas[i]);

            _list.Add(temp);
        }
    }

    public int GetSpriteCount()
    {
        return _images.Count; 
    }

    public Sprite GetSprite(int id)
    {
        return _images[id];
    }

    public TutorialData GetData(int id)
    {
        return _list.Find(x => x.id == id);
    }
}
