using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;

[CreateAssetMenu(fileName = "SO_CreatureDataSprite", menuName = "Scriptable Objects/SO_CreatureDataSprite")]
public class SO_CreatureDataSprite : ScriptableObject
{
    [Serializable]
    private class ImageTemplate
    {
        public int id = 0;
        public Sprite sprite = null;
    }

    [SerializeField] private string _dataPath = string.Empty;

    [Space(10)]

    [SerializeField] private Sprite _spriteNull;
    [SerializeField] private List<ImageTemplate> _tempalte = null;

    private List<CreatureData> _creatureDatas = null;

    public void Initialize()
    {
        ReadData();
    }

    private void ReadData()
    {
        if (_creatureDatas != null)
        {
            _creatureDatas.Clear();
        }

        _creatureDatas = new List<CreatureData>();

        string json = Resources.Load<TextAsset>(_dataPath).text;

        var respons = new
        {
            datas = new List<DataManager.Creature_Data>()
        };

        var result = JsonConvert.DeserializeAnonymousType(json, respons);

        List<DataManager.Creature_Data> datas = new List<DataManager.Creature_Data>();
        datas = result.datas;

        for (int i = 0; i < datas.Count; i++)
        {
            DataManager.Creature_Data data = datas[i];

            CreatureData temp = new CreatureData();
            temp.id = data.id;
            temp.name = data.name;
            temp.description = data.description;
            temp.spriteIndex = data.spriteIndex;

            temp.stats = new CreatureStats();
            temp.stats.coin = new CreatureStat(data.coin, 1, 0, 0);
            temp.stats.hp = new CreatureStat(data.hp, 1, 0, 0);
            temp.stats.mp = new CreatureStat(data.mp, 1, 0, 0);
            temp.stats.ap = new CreatureStat(data.ap, 1, 0, 0);
            temp.stats.exp = new CreatureStat(data.exp, 1, 0, 0);
            temp.stats.attack = new CreatureStat(data.attack, 1, 0, 0);
            temp.stats.defence = new CreatureStat(data.defence, 1, 0, 0);
            temp.stats.vision = new CreatureStat(data.vision, 1, 0, 0);
            temp.stats.attackRange = new CreatureStat(data.attackRange, 1, 0, 0);
            temp.stats.evesion = new CreatureStat(data.evasion, 1, 0, 0);

            temp.defultStatus = data.defultStatus;
            temp.defultStatusValue = data.defultStatusValue;

            temp.haveSkill = data.useSkill.Equals("TRUE") ? true : false;

            temp.skillIndexs = new List<short>();
            if (data.skillIndexs != null)
            {
                temp.skillIndexs.AddRange(data.skillIndexs);
            }

            temp.itemIndexs = new List<short>();
            if (data.itemIndexs != null)
            {
                temp.itemIndexs.AddRange(data.itemIndexs);
            }

            temp.skill_Duration = new List<Duration>();
            temp.item_Duration = new List<Duration>();
            temp.coolDownSkill = new List<Skill_CoolDown>();
            temp.abnormalStatuses = new List<AbnormalStatus>();

            _creatureDatas.Add(temp);
        }
    }

    public CreatureData GetData(int index)
    {
        if (index >= _creatureDatas.Count)
        {
            index %= _creatureDatas.Count;
        }

        return _creatureDatas.Find(x => x.id == (index + 101)).DeepCopy();
    }

    public int GetDataCount()
    {
        return _creatureDatas.Count;
    }
    
    public Sprite GetSprite(int id)
    {
        if (_tempalte.Find(x => x.id == id) == null)
        {
            Debug.LogError("_tempalte.Find(x => x.id == id) = null     " + id);
            return _spriteNull;
        }

        if(_tempalte.Find(x => x.id == id).sprite == null)
        {
            return _spriteNull;
        }

        return _tempalte.Find(x => x.id == id).sprite;
    }
}
