using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.IO;

public class DataManager : MonoBehaviour
{
    #region Creatures

    [Serializable]
    public class Creature_Data
    {
        public ushort index = 0;

        public string name = string.Empty;
        public string description = string.Empty;
        public ushort coin = 0;

        public ushort hp = 0;
        public ushort mp = 0;
        public ushort ap = 0; // Active Point 
        public ushort exp = 0;
        public ushort attack = 0;
        public ushort defence = 0;

        public bool useSkill = false;
        public ushort[] skillIndexs = null;
        public Skill_Data[] skillDatas = null;
    }

    [Serializable]
    public class User_Data
    {
        public Creature_Data data = null;

        public ushort level = 0; // 1레벨 경험치 * 레벨수
    }

    [Serializable]
    public class Skill_Data
    {
        public ushort index = 0;

        public string name = string.Empty;
        public string description = string.Empty;

        public short hp = 0;
        public short mp = 0;
        public short ap = 0;
        public short exp = 0;
        public short expPercentIncreased = 0;
        public short coin = 0;
        public short coinPercentIncreased = 0;

        public short attack = 0;
        public short attackPercentIncreased = 0;
        public short defence = 0;
        public short defencePercentIncreased = 0;
        public ushort duration = 0;
    }

    [Serializable]
    public class Item_Data
    {
        public ushort index = 0;

        public string name = string.Empty;
        public string description = string.Empty;
        public ushort price = 0;

        public short hp = 0;
        public short mp = 0;
        public short ap = 0;
        public short exp = 0;
        public short expPercentIncreased = 0;
        public short coin = 0;
        public short coinPercentIncreased = 0;

        public short attack = 0;
        public short attackPercentIncreased = 0;
        public short defence = 0;
        public short defencePercentIncreased = 0;
        public ushort duration = 0;
    }

    #endregion

    #region Map

    [Serializable]
    public class Map_Data
    {
        public Block_Data[] blockDatas = null;

        public Creature_Data userData = null;
        public Creature_Data[] monsterDatas = null;
    }

    [Serializable]
    public class Block_Data
    {
        public ushort x = 0;
        public ushort y = 0;

        public bool isWalkable = false;
        public bool isCreatureExist = false;
    }

    #endregion

    [Serializable]
    public class Save_Data
    {
        public ushort round = 0;

        public User_Data userData = null;
        public Map_Data mapData = null;
    }

    [Header("Data Path")]
    [SerializeField] private string _saveDataPath = string.Empty;
    [SerializeField] private string _creatureDataPath = string.Empty;
    [SerializeField] private string _itemDataPath = string.Empty;
    [SerializeField] private string _skillDataPath = string.Empty;

    private Save_Data _saveData = null;

    private List<Creature_Data> _creatureDatas = null;
    private List<Item_Data> _itemDatas = null;
    private List<Skill_Data> _skillDatas = null;

    public void Initialize()
    {
        this.gameObject.SetActive(true);
    }

    public void ReadGameData()
    {
        ReadCreaturesData();
        ReadItemsData();
        ReadSkillsData();
    }

    #region SaveData

    public void CreateSaveData()
    {
        _saveData = new Save_Data();
    }

    public bool CheckSaveData()
    {
        return CheckData(_saveDataPath);
    }

    public void WriteSaveData()
    {

    }

    public void ReadSaveData()
    {
        
    }

    public void ChangePlayerData(string name)
    {
        _saveData.userData.data.name = name;

        Debug.Log(_saveData.userData.data.name);
    }

    public void ChangePlayerData(eCreatureData dataType, ushort value)
    {

    }

    public void ChangePlayerData(Save_Data newData)
    {
        _saveData = newData;
    }

    #endregion

    #region Creature

    private void ReadCreaturesData()
    {
        if(_creatureDatas != null)
        {
            _creatureDatas.Clear();
        }

        _creatureDatas = new List<Creature_Data>();

        GetJsonFile<Creature_Data>(_creatureDataPath, (datas) =>
        {
            _creatureDatas = datas;
        });
    }

    #endregion

    #region Item

    private void ReadItemsData()
    {

    }

    #endregion

    #region Skill

    private void ReadSkillsData()
    {

    }

    #endregion

    private void GetJsonFile<T>(string path, Action<List<T>> callback)
    {
        path = Application.dataPath + "/Resources/" + path;
        if (CheckData(path) == false)
        {
            Debug.LogWarning("Data Read False");
            GameManager.instance.GameError();

            return;
        }

        string json = File.ReadAllText(path + ".json");

        var respons = new
        {
            datas = new List<T>()
        };

        var result = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(json, respons);

        callback?.Invoke(respons.datas);
    }

    private bool CheckData(string path)
    {
        return File.Exists(path + ".json");
    }
}
