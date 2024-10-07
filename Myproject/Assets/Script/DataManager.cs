using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;

public class DataManager : MonoBehaviour
{
    #region Creatures

    [Serializable]
    public class Creature_Data
    {
        public ushort id = 0;

        public string name = string.Empty;
        public string description = string.Empty;
        public ushort coin = 0;

        public ushort hp = 0;
        public ushort mp = 0;
        public ushort ap = 0; // Active Point 
        public ushort exp = 0;
        public ushort attack = 0;
        public ushort defence = 0;
        public ushort vision = 0;
        public ushort attackRange = 0;

        public bool useSkill = false;
        public List<ushort> skillIndexs = null;
        public List<ushort> itemIndexs = null;

        public int currentNodeIndex = 0;
    }

    [Serializable]
    public class Npc_Data
    {
        public ushort id = 0;

        public string name = string.Empty;
        public string description = string.Empty;

        public List<ushort> itemIndexs = null;
        public int currentNodeIndex = 0;
    }

    [Serializable]
    public class User_Data
    {
        public Creature_Data data = null;

        public ushort level = 1; // 1레벨 (경험치 * 0.4) * 레벨수

        private ushort _defultHP = 10;
        private ushort _defultMP = 10;
        private ushort _defultAP = 10;
        private ushort _defultEXP = 15;
        private ushort _defultATTACK = 10;
        private ushort _defultDEFENCE = 4;
        private ushort _defultVISION = 1;
        private ushort _defultATTACKRANGE = 1;

        public ushort currentHP = 10;
        public ushort currentMP = 10;
        public ushort currentAP = 10;
        public ushort currentEXP = 0;
        public ushort currentATTACK = 10;
        public ushort currentDEFENCE = 10;
        public ushort currentVISION = 3;
        public ushort currentATTACKRANGE = 10;

        public short Attack_Effect = 0;
        public short Defence_Effect = 0;

        public short Attack_Effect_Per = 0;
        public short Defence_Effect_Per = 0;

        public short EXP_Effect_Per = 0;
        public short Coin_Effect_Per = 0;

        public List<ushort> itemDataIndexs = null;
        public List<ushort> skillDataIndexs = null;

        public List<Duration> useSkill = null;
        public List<Duration> useItem = null;

        public List<Skill_CoolDown> coolDownSkill = null;

        #region GetSet

        public ushort maximumHP
        {
            get { return (ushort)(_defultHP * data.hp); }
            set { data.hp = (ushort)value; }
        }

        public ushort maximumMP
        {
            get { return (ushort)(_defultMP * data.mp); }
            set { data.mp = (ushort)value; }
        }

        public ushort maximumAP
        {
            get { return (ushort)(_defultAP * data.ap); }
            set { data.ap = (ushort)value; }
        }

        public ushort maximumEXP
        {
            get { return (ushort)(level * _defultEXP); }
        }

        public ushort maximumATTACK
        {
            get { return (ushort)((_defultATTACK * data.attack) + ((_defultATTACK * data.attack) * 0.01f * Attack_Effect_Per)); }
            set { data.attack = (ushort)value; }
        }

        public ushort maximumDEFENCE
        {
            get { return (ushort)(_defultDEFENCE * data.defence + ((_defultDEFENCE * data.defence) * 0.01f * Defence_Effect_Per)); }
            set { data.defence = (ushort)value; }
        }

        public ushort maximumVISION
        {
            get { return (ushort)(_defultVISION * data.vision); }
            set 
            { 
                if(value > 5)
                {
                    return;
                }

                data.vision = (ushort)value; 
            }
        }

        public ushort maximumATTACKRANGE
        {
            get { return (ushort)(_defultATTACKRANGE * data.attackRange); }
            set
            {
                if (value > 5)
                {
                    return;
                }

                data.attackRange = (ushort)value;
            }
        }

        #endregion

        public void Reset()
        {
            currentHP = maximumHP;
            currentMP = maximumMP;
            currentAP = maximumAP;
            currentATTACK = maximumATTACK;
            currentDEFENCE = maximumDEFENCE;
            currentVISION = maximumVISION;
            currentATTACKRANGE = maximumATTACKRANGE;
        }
    }

    [Serializable]
    public class Skill_Data
    {
        public ushort id = 0;

        public string name = string.Empty;
        public string description = string.Empty;

        public short coolDown = 0;
        public short usemp = 0;

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
        public short duration = 0;
    }

    [Serializable]
    public class Item_Data
    {
        public ushort id = 0;

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

    public class Duration
    {
        public int ID = 0;
        public string name = string.Empty;

        public int remaindDuration = 0;
        public int remaindCooldown = 0;

        public eStats stats = eStats.Non;
        public eEffect_IncreaseDecrease inDe = eEffect_IncreaseDecrease.Non;
        public bool isPercent = false;
        public int value;
    }

    public class Skill_CoolDown
    {
        public int id = 0;
        public string name = string.Empty;

        public int coolDown = 0;
    }

    #endregion

    #region Map

    [Serializable]
    public class Map_Data
    {
        public int mapSize = 0;

        public int enterNodeIndex = 0;
        public int exitNodeIndex = 0;

        public List<Node_Data> nodeDatas = null;
        public List<Creature_Data> monsterDatas = null;
        public List<Item_Data> itemDatas = null;
        public Npc_Data npcData = null;
    }

    [Serializable]
    public class Node_Data
    {
        public int index = 0;
        public ushort x = 0;
        public ushort y = 0;

        public bool isWalkable = false;
        public bool isMonster = false;
        public bool isUser = false;
        public bool isShop = false;
        public bool isItem = false;
    }

    #endregion

    [Serializable]
    public class Encyclopedia_Data
    {
        public string maxRoundDate = string.Empty;
        public int maxRound = 0;

        public string maxLevelDate = string.Empty;
        public int maxLevel = 0;

        public List<Creature_Data> creatureDatas = null;
        public List<Item_Data> itemDatas = null;
        public List<Skill_Data> skillData = null;
    }

    [Serializable]
    public class Save_Data
    {
        public ushort round = 0;

        public User_Data userData = null;
        public Map_Data mapData = null;
        public Encyclopedia_Data encyclopediaData = null;
    }

    [Header("Data Path")]
    [SerializeField] private string _creatureDataPath = string.Empty;
    [SerializeField] private string _npcDataPath = string.Empty;
    [SerializeField] private string _itemDataPath = string.Empty;
    [SerializeField] private string _skillDataPath = string.Empty;
    [Header("Map Size"), Tooltip("3의 배수이되 홀수여야함"), SerializeField] private int _mapSize = 9;

    private Save_Data _saveData = null;

    private List<Creature_Data> _creatureDatas = null;
    private List<Npc_Data> _npcDatas = null;
    private List<Item_Data> _itemDatas = null;
    private List<Skill_Data> _skillDatas = null;

    private Action _onSaveOrLoadCallback = null;

    public int MapSize
    {
        get { return _mapSize; }
    }

    public void Initialize()
    {
        this.gameObject.SetActive(true);
    }

    public void ReadGameData()
    {
        ReadCreaturesData();
        ReadNpcData();
        ReadItemsData();
        ReadSkillsData();
    }

    #region SaveData

    public void CreateNewSaveData()
    {
        Debug.Log("TEST Stat Set");

        _saveData = new Save_Data();
        _saveData.round = 1;

        _saveData.userData = new User_Data();
        _saveData.userData.itemDataIndexs = new List<ushort>();
        _saveData.userData.itemDataIndexs.Add(501);

        _saveData.userData.skillDataIndexs = new List<ushort>();
        _saveData.userData.skillDataIndexs.Add(301);

        _saveData.userData.useSkill = new List<Duration>();
        _saveData.userData.useItem = new List<Duration>();
        _saveData.userData.coolDownSkill = new List<Skill_CoolDown>();

        _saveData.userData.data = new Creature_Data();
        _saveData.userData.data.hp = 10;
        _saveData.userData.data.mp = 10;
        _saveData.userData.data.ap = 10;
        _saveData.userData.data.attack = 100;
        _saveData.userData.data.defence = 100;
        _saveData.userData.data.vision = 3;
        _saveData.userData.data.attackRange = 1;
        _saveData.userData.data.coin = 1000;

        _saveData.mapData = new Map_Data();
        _saveData.mapData.mapSize = _mapSize;
        _saveData.mapData.nodeDatas = new List<Node_Data>();
        _saveData.mapData.monsterDatas = new List<Creature_Data>();
        _saveData.mapData.itemDatas = new List<Item_Data>();
        _saveData.mapData.npcData = new Npc_Data();

        _saveData.encyclopediaData = new Encyclopedia_Data();
        _saveData.encyclopediaData.creatureDatas = new List<Creature_Data>();
        _saveData.encyclopediaData.itemDatas = new List<Item_Data>();
        _saveData.encyclopediaData.skillData = new List<Skill_Data>();
    }

    public Save_Data CopySaveData()
    {
        return _saveData;
    }

    public bool CheckSaveData()
    {
        if(_saveData == null)
        {
            return false;
        }

        return !(_saveData.userData.data.name.Length == 0);
    }

    public void SaveDataToCloud(Save_Data saveData = null, Action onSaveOrLoadCallback = null)
    {
        var request = new
        {
            data = saveData
        };

        string json = JsonConvert.SerializeObject(request);
        PlayerPrefs.SetString("SAVE", json);

        onSaveOrLoadCallback?.Invoke();
    }

    public void LoadDataToCloud(Action onSaveOrLoadCallback = null)
    {
        string json = PlayerPrefs.GetString("SAVE");

        var respons = new
        {
            data = new Save_Data()
        };

        var result = JsonConvert.DeserializeAnonymousType(json, respons);
        _saveData = result.data;

        onSaveOrLoadCallback?.Invoke();
    }

    public void FailGame(Save_Data saveData)
    {
        OrganizeEncyclopedia(saveData);

        _saveData = new Save_Data();
        _saveData.round = 1;

        _saveData.userData = new User_Data();
        _saveData.userData.itemDataIndexs = new List<ushort>();
        _saveData.userData.itemDataIndexs.Add(501);

        _saveData.userData.skillDataIndexs = new List<ushort>();
        _saveData.userData.skillDataIndexs.Add(301);

        _saveData.userData.useSkill = new List<Duration>();
        _saveData.userData.useItem = new List<Duration>();
        _saveData.userData.coolDownSkill = new List<Skill_CoolDown>();

        _saveData.userData.data = new Creature_Data();
        _saveData.userData.data.hp = 1;
        _saveData.userData.data.mp = 1;
        _saveData.userData.data.ap = 10;
        _saveData.userData.data.attack = 1;
        _saveData.userData.data.defence = 0;
        _saveData.userData.data.vision = 1;
        _saveData.userData.data.attackRange = 1;

        _saveData.mapData = new Map_Data();
        _saveData.mapData.mapSize = _mapSize;
        _saveData.mapData.nodeDatas = new List<Node_Data>();
        _saveData.mapData.monsterDatas = new List<Creature_Data>();

        SaveDataToCloud(_saveData);
    }

    public void ChangePlayerData(string name)
    {
        _saveData.userData.data.name = name;
    }

    public void ChangePlayerData(Save_Data newData)
    {
        _saveData = newData;
    }

    private void OrganizeEncyclopedia(Save_Data lastData)
    {
        if(_saveData.encyclopediaData == null)
        {
            _saveData.encyclopediaData = new Encyclopedia_Data();
            _saveData.encyclopediaData.creatureDatas = new List<Creature_Data>();
            _saveData.encyclopediaData.itemDatas = new List<Item_Data>();
            _saveData.encyclopediaData.skillData = new List<Skill_Data>();
        }

        if (_saveData.encyclopediaData.maxLevel < lastData.userData.level)
        {
            _saveData.encyclopediaData.maxLevel = lastData.userData.level;
            _saveData.encyclopediaData.maxLevelDate = DateTime.Now.ToString("yyyy-MM-d HH:m:ss:fff");
        }

        if (_saveData.encyclopediaData.maxRound < lastData.round)
        {
            _saveData.encyclopediaData.maxRound = lastData.round;
            _saveData.encyclopediaData.maxRoundDate = DateTime.Now.ToString("yyyy-MM-d HH:m:ss:fff");
        }

        foreach (var saveCreature in _saveData.encyclopediaData.creatureDatas)
        {
            foreach (var lastCreature in lastData.encyclopediaData.creatureDatas)
            {
                if(saveCreature.id == lastCreature.id)
                {
                    continue;
                }

                _saveData.encyclopediaData.creatureDatas.Add(lastCreature);
            }
        }

        foreach (var saveItem in _saveData.encyclopediaData.itemDatas)
        {
            foreach (var lastItem in lastData.encyclopediaData.itemDatas)
            {
                if (saveItem.id == lastItem.id)
                {
                    continue;
                }

                _saveData.encyclopediaData.itemDatas.Add(lastItem);
            }
        }

        foreach (var saveSkill in _saveData.encyclopediaData.skillData)
        {
            foreach (var lastSkill in lastData.encyclopediaData.skillData)
            {
                if (saveSkill.id == lastSkill.id)
                {
                    continue;
                }

                _saveData.encyclopediaData.skillData.Add(lastSkill);
            }
        }
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

        string json = Resources.Load<TextAsset>(_creatureDataPath).text;

        var respons = new
        {
            datas = new List<Creature_Data>()
        };

        var result = JsonConvert.DeserializeAnonymousType(json, respons);

        _creatureDatas = result.datas;
    }

    public Creature_Data GetCreatureData(int index)
    {
        if (index >= _creatureDatas.Count)
        {
            index %= _creatureDatas.Count;
        }

        return _creatureDatas.Find(x => x.id == (index + 101)).DeepCopy();
    }

    #endregion


    #region Npc

    private void ReadNpcData()
    {
        if (_npcDatas != null)
        {
            _npcDatas.Clear();
        }

        _npcDatas = new List<Npc_Data>();

        string json = Resources.Load<TextAsset>(_npcDataPath).text;

        var respons = new
        {
            datas = new List<Npc_Data>()
        };

        var result = JsonConvert.DeserializeAnonymousType(json, respons);

        _npcDatas = result.datas;
    }

    public Npc_Data GetNpcData(int index)
    {
        if (index >= _npcDatas.Count)
        {
            index %= _npcDatas.Count;
        }

        return _npcDatas.Find(x => x.id == index).DeepCopy();
    }

    #endregion

    #region Item

    private void ReadItemsData()
    {
        if (_itemDatas != null)
        {
            _itemDatas.Clear();
        }

        _itemDatas = new List<Item_Data>();

        string json = Resources.Load<TextAsset>(_itemDataPath).text;

        var respons = new
        {
            datas = new List<Item_Data>()
        };

        var result = JsonConvert.DeserializeAnonymousType(json, respons);

        _itemDatas = result.datas;
    }

    public Item_Data GetItemData(int index, bool isID= true)
    {
        return _itemDatas.Find(x => x.id == (index + (isID == true ? 0 : 501)));
    }

    public int GetItemDataCount()
    {
        return _itemDatas.Count;
    }

    #endregion

    #region Skill

    private void ReadSkillsData()
    {
        if(_skillDatas != null)
        {
            _skillDatas.Clear();
        }

        _skillDatas = new List<Skill_Data>();

        string json = Resources.Load<TextAsset>(_skillDataPath).text;

        var respons = new
        {
            datas = new List<Skill_Data>()
        };

        var result = JsonConvert.DeserializeAnonymousType(json, respons);

        _skillDatas = result.datas;
    }

    public Skill_Data GetskillData(int index)
    {
        return _skillDatas.Find(x => x.id == (index));
    }

    #endregion
}

public static class Extensions
{
    public static T DeepCopy<T>(this T source) where T : new()
    {
        if (!typeof(T).IsSerializable)
        {
            // fail
            return source;
        }

        try
        {
            object result = null;
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, source);
                ms.Position = 0;
                result = (T)formatter.Deserialize(ms);
                ms.Close();
            }

            return (T)result;
        }
        catch (Exception)
        {
            // fail
            return new T();
        }
    }
}
