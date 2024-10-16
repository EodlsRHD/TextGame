using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

public class DataManager : MonoBehaviour
{
    #region Creatures

    [Serializable]
    private class Creature_Data
    {
        public short id = 0;

        public string name = string.Empty;
        public string description = string.Empty;
        public short spriteIndex = 0;

        public short coin = 0;
        public short hp = 0;
        public short mp = 0;
        public short ap = 0;
        public short exp = 0;
        public short attack = 0;
        public short defence = 0;
        public short vision = 0;
        public short attackRange = 0;
        public short evasion = 0;

        public eStrengtheningTool defultStatus = eStrengtheningTool.Non;

        public bool useSkill = false;
        public List<short> skillIndexs = null;
        public List<short> itemIndexs = null;
    }

    [Serializable]
    public class Npc_Data
    {
        public short id = 0;

        public string name = string.Empty;
        public string description = string.Empty;

        public bool isShop = false;
        public bool isBonfire = false;
        public bool isGuide = false;

        public bool isUseBonfire = false;

        public List<short> itemIndexs = null;
        public List<short> SkillIndexs = null;

        public int currentNodeIndex = 0;
    }

    [Serializable]
    public class Skill_Data
    {
        public short id = 0;

        public string name = string.Empty;
        public string description = string.Empty;

        public short coolDown = 0;
        public short useMp = 0;

        public short duration = 0;

        public short hp = 0;
        public short hpPercentIncreased = 0;
        public short mp = 0;
        public short mpPercentIncreased = 0;
        public short ap = 0;
        public short apPercentIncreased = 0;
        public short exp = 0;
        public short expPercentIncreased = 0;
        public short coin = 0;
        public short coinPercentIncreased = 0;
        public short attack = 0;
        public short attackPercentIncreased = 0;
        public short defence = 0;
        public short defencePercentIncreased = 0;

        public eDir dir = eDir.Non;

        public eStrengtheningTool needStatus = eStrengtheningTool.Non;
        public eStrengtheningTool grantStatus = eStrengtheningTool.Non;

        public short range = 0;
        public float value = 0;

        public bool revealMap = false;
    }

    [Serializable]
    public class Item_Data
    {
        public short id = 0;

        public string name = string.Empty;
        public string description = string.Empty;

        public short price = 0;

        public short duration = 0;

        public short hp = 0;
        public short hpPercentIncreased = 0;
        public short mp = 0;
        public short mpPercentIncreased = 0;
        public short ap = 0;
        public short apPercentIncreased = 0;
        public short exp = 0;
        public short expPercentIncreased = 0;
        public short coin = 0;
        public short coinPercentIncreased = 0;
        public short attack = 0;
        public short attackPercentIncreased = 0;
        public short defence = 0;
        public short defencePercentIncreased = 0;

        public eDir dir = eDir.Non;

        public eStrengtheningTool needStatus = eStrengtheningTool.Non;
        public eStrengtheningTool grantStatus = eStrengtheningTool.Non;

        public short range = 0;
        public float value = 0;

        public bool revealMap = false;

        public int currentNodeIndex = 0;
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
        public List<CreatureData> monsterDatas = null;
        public List<ItemData> itemDatas = null;
        public List<Npc_Data> npcDatas = null;
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
        public bool isBonfire = false;
        public bool isItem = false;
        public bool isGuide = false;

        public bool isUseBonfire = false;
    }

    #endregion

    [Serializable]
    public class Encyclopedia_Data
    {
        public string maxRoundDate = string.Empty;
        public int maxRound = 0;

        public string maxLevelDate = string.Empty;
        public int maxLevel = 0;

        public List<CreatureData> creatureDatas = null;
        public List<ItemData> itemDatas = null;
        public List<Achievements_Data> _achievementsDatas = null;
    }

    [Serializable]
    public class Save_Data
    {
        public short round = 0;

        public UserData userData = null;
        public Map_Data mapData = null;
        public Encyclopedia_Data encyclopediaData = null;
    }

    [Serializable]
    public class Achievements_Data
    {
        public bool isSuccess = false;
        public string name = string.Empty;
        public string description = string.Empty;
    }

    [Serializable]
    public class Tutorial_Data
    {
        public int id = 0;

        public string content = string.Empty;
        public List<Tutorial_answer> answers = null;

        public int isQuest = -1;
    }

    [Header("Data Path")]
    [SerializeField] private string _creatureDataPath = string.Empty;
    [SerializeField] private string _npcDataPath = string.Empty;
    [SerializeField] private string _itemDataPath = string.Empty;
    [SerializeField] private string _skillDataPath = string.Empty;

    [Header("Scriptable Object")]
    [SerializeField] private SO_TutorialData _soTutorial = null;

    [Header("Map Size"), Tooltip("3의 배수이되 홀수여야함"), SerializeField] private int _mapSize = 9;
    [Header("Creature Sprite Template"), SerializeField] private List<BlockImageTemplate> _creatureSpriteTemplate = null;

    private Save_Data _saveData = null;
    private Encyclopedia_Data _encyclopediaData = null;

    private List<CreatureData> _creatureDatas = null;
    private List<Npc_Data> _npcDatas = null;
    private List<ItemData> _itemDatas = null;
    private List<SkillData> _skillDatas = null;

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

        _soTutorial.Initialize();
    }

    public void saveAllData()
    {
        SaveDataToCloud(_saveData);
        OrganizeEncyclopedia(_saveData);
        SaveEncyclopediaToCloud(_encyclopediaData);
    }

    #region SaveData

    public void CreateNewSaveData(Action<bool> onResultCallback)
    {
        UnityEngine.Debug.LogError("TEST Stat Set");

        _saveData = new Save_Data();
        _saveData.round = 1;

        _saveData.userData = new UserData();
        _saveData.userData.data = new CreatureData();
        _saveData.userData.data.stats = new CreatureStats();
        _saveData.userData.data.stats.coin = new CreatureStat(0, 1, 0, 0);
        _saveData.userData.data.stats.hp = new CreatureStat(10, 1, 0, 0);
        _saveData.userData.data.stats.mp = new CreatureStat(10, 1, 0, 0);
        _saveData.userData.data.stats.ap = new CreatureStat(3, 1, 0, 0);
        _saveData.userData.data.stats.exp = new CreatureStat(0, 1, 0, 0);
        _saveData.userData.data.stats.attack = new CreatureStat(5, 1, 0, 0);
        _saveData.userData.data.stats.defence = new CreatureStat(0, 1, 0, 0);
        _saveData.userData.data.stats.vision = new CreatureStat(1, 1, 0, 0);
        _saveData.userData.data.stats.attackRange = new CreatureStat(1, 1, 0, 0);
        _saveData.userData.data.skillIndexs = new List<short>();
        _saveData.userData.data.itemIndexs = new List<short>();

        _saveData.userData.data.skill_Duration = new List<Duration>();
        _saveData.userData.data.item_Duration = new List<Duration>();
        _saveData.userData.data.coolDownSkill = new List<Skill_CoolDown>();
        _saveData.userData.data.abnormalStatuses = new List<AbnormalStatus>();

        _saveData.mapData = new Map_Data();
        _saveData.mapData.mapSize = _mapSize;
        _saveData.mapData.nodeDatas = new List<Node_Data>();
        _saveData.mapData.monsterDatas = new List<CreatureData>();
        _saveData.mapData.itemDatas = new List<ItemData>();
        _saveData.mapData.npcDatas = new List<Npc_Data>();

        _saveData.encyclopediaData = new Encyclopedia_Data();
        _saveData.encyclopediaData.creatureDatas = new List<CreatureData>();
        _saveData.encyclopediaData.itemDatas = new List<ItemData>();
        _saveData.encyclopediaData._achievementsDatas = new List<Achievements_Data>();



        _encyclopediaData = new Encyclopedia_Data();
        _encyclopediaData.creatureDatas = new List<CreatureData>();
        _encyclopediaData.itemDatas = new List<ItemData>();
        _encyclopediaData._achievementsDatas = new List<Achievements_Data>();

        ResetPlayerData();

        SaveDataToCloud(_saveData, (result) => 
        {
            onResultCallback?.Invoke(result);
        });
    }

    public Save_Data CopySaveData()
    {
        return _saveData;
    }

    public Encyclopedia_Data CopyEncyclopediaData()
    {
        if (_encyclopediaData == null)
        {
            _encyclopediaData = new Encyclopedia_Data();
            _encyclopediaData.creatureDatas = new List<CreatureData>();
            _encyclopediaData.itemDatas = new List<ItemData>();
            _encyclopediaData._achievementsDatas = new List<Achievements_Data>();
        }

        return _encyclopediaData;
    }

    public void SaveDataToCloud(Save_Data saveData = null, Action<bool> onSaveOrLoadCallback = null)
    {
        GameManager.instance.StartLoad();

        OrganizeEncyclopedia(saveData);

        if (saveData != null)
        {
            _saveData = saveData;
        }

        var saveRequest = new
        {
            data = _saveData
        };

        string savejson = JsonConvert.SerializeObject(saveRequest);

#if UNITY_EDITOR

        PlayerPrefs.SetString("SAVE", savejson);

        GameManager.instance.StopLoad();

        onSaveOrLoadCallback?.Invoke(true);

        return;
#endif

        GameManager.instance.googlePlayGameServeice.SaveData(savejson, (result) =>
        {
            GameManager.instance.StopLoad();

            onSaveOrLoadCallback?.Invoke(result);
        });
    }

    public void SaveEncyclopediaToCloud(Encyclopedia_Data data, Action<bool> onSaveOrLoadCallback = null)
    {
        GameManager.instance.StartLoad();

        var enRequest = new
        {
            data = _encyclopediaData
        };

        string enjson = JsonConvert.SerializeObject(enRequest);

#if UNITY_EDITOR

        PlayerPrefs.SetString("SAVE_EncylopediaData", enjson);

        GameManager.instance.StopLoad();

        onSaveOrLoadCallback?.Invoke(true);

        return;
#endif

        GameManager.instance.googlePlayGameServeice.SaveEncylopediaData(enjson, (result) =>
        {
            GameManager.instance.StopLoad();

            onSaveOrLoadCallback?.Invoke(result);
        });
    }

    public void LoadDataToCloud(Action<bool, Save_Data> onSaveOrLoadCallback = null)
    {
        GameManager.instance.StartLoad();

#if UNITY_EDITOR
        string sapjson = PlayerPrefs.GetString("SAVE");

        var prespons = new
        {
            data = new Save_Data()
        };

        var presult = JsonConvert.DeserializeAnonymousType(sapjson, prespons);

        if (presult == null)
        {
            GameManager.instance.StopLoad();

            onSaveOrLoadCallback?.Invoke(false, null);

            return;
        }

        _saveData = presult.data;

        GameManager.instance.StopLoad();

        onSaveOrLoadCallback?.Invoke(true, presult.data);

        return;
#endif

        GameManager.instance.googlePlayGameServeice.LoadData((result, json) =>
        {
            GameManager.instance.StopLoad();

            if (result == false)
            {
                onSaveOrLoadCallback?.Invoke(false, null);

                return;
            }

            var respons = new
            {
                data = new Save_Data()
            };

            var data = JsonConvert.DeserializeAnonymousType(json, respons);
            _saveData = data.data;

            onSaveOrLoadCallback?.Invoke(true, data.data);
        });
    }

    public void LoadEncyclopediaToCloud(Action<bool> onSaveOrLoadCallback = null)
    {
        GameManager.instance.StartLoad();

#if UNITY_EDITOR
        string enpjson = PlayerPrefs.GetString("SAVE_EncylopediaData");

        if (enpjson.Length == 0)
        {
            GameManager.instance.StopLoad();

            onSaveOrLoadCallback?.Invoke(false);

            return;
        }

        var prespons = new
        {
            data = new Encyclopedia_Data()
        };

        var presult = JsonConvert.DeserializeAnonymousType(enpjson, prespons);
        _encyclopediaData = presult.data;

        GameManager.instance.StopLoad();

        onSaveOrLoadCallback?.Invoke(true);

        return;
#endif

        GameManager.instance.googlePlayGameServeice.LoadEncylopediaData((result, json) =>
        {
            GameManager.instance.StopLoad();

            if (result == false)
            {
                onSaveOrLoadCallback?.Invoke(false);

                return;
            }

            var respons = new
            {
                data = new Encyclopedia_Data()
            };

            var data = JsonConvert.DeserializeAnonymousType(json, respons);
            _encyclopediaData = data.data;

            onSaveOrLoadCallback?.Invoke(true);
        });
    }

    private void DeleteData()
    {
#if UNITY_EDITOR

        UnityEngine.Debug.LogError("DeleteData");
        PlayerPrefs.SetString("SAVE", string.Empty);
        _saveData = null;

        return;
#endif
        GameManager.instance.googlePlayGameServeice.DeleteData((result) =>
        {
            if(result == false)
            {
                return;
            }

            _saveData = null;
        });
    }

    public void FailGame(Save_Data saveData = null)
    {
        if (saveData != null)
        {
            OrganizeEncyclopedia(saveData);
        }

        DeleteData();
    }

    public void UpdatePlayerData(string name)
    {
        _saveData.userData.data.name = name;
    }

    public void UpdatePlayerData(Save_Data newData)
    {
        _saveData = newData;
    }

    public void AddEncyclopedia_Creature(int id)
    {
        if (_encyclopediaData == null)
        {
            _encyclopediaData = new Encyclopedia_Data();
            _encyclopediaData.creatureDatas = new List<CreatureData>();
            _encyclopediaData.itemDatas = new List<ItemData>();
        }

        if(_encyclopediaData.creatureDatas.Count  == 0)
        {
            _encyclopediaData.creatureDatas.Add(GetCreatureData(id));
        }
        else
        {
            foreach (var saveCreature in _encyclopediaData.creatureDatas)
            {
                if (saveCreature.id == id)
                {
                    continue;
                }

                _encyclopediaData.creatureDatas.Add(GetCreatureData(id));

                break;
            }
        }

        SaveEncyclopediaToCloud(_encyclopediaData);
    }

    public void AddEncyclopedia_Item(int id)
    {
        if (_encyclopediaData == null)
        {
            _encyclopediaData = new Encyclopedia_Data();
            _encyclopediaData.creatureDatas = new List<CreatureData>();
            _encyclopediaData.itemDatas = new List<ItemData>();
        }

        foreach (var saveItem in _encyclopediaData.itemDatas)
        {
            if (saveItem.id == id)
            {
                continue;
            }

            _encyclopediaData.itemDatas.Add(GetItemData(id));

            break;
        }

        SaveEncyclopediaToCloud(_encyclopediaData);
    }

    private void OrganizeEncyclopedia(Save_Data lastData)
    {
        if(lastData == null)
        {
            SaveEncyclopediaToCloud(_encyclopediaData);
            return;
        }

        if(_encyclopediaData == null)
        {
            _encyclopediaData = new Encyclopedia_Data();
            _encyclopediaData.creatureDatas = new List<CreatureData>();
            _encyclopediaData.itemDatas = new List<ItemData>();
        }

        if (_encyclopediaData.maxLevel < lastData.userData.level)
        {
            _encyclopediaData.maxLevel = lastData.userData.level;
            _encyclopediaData.maxLevelDate = DateTime.Now.ToString("yyyy-MM-d HH:m:ss:fff");
        }

        if (_encyclopediaData.maxRound < lastData.round)
        {
            _encyclopediaData.maxRound = lastData.round;
            _encyclopediaData.maxRoundDate = DateTime.Now.ToString("yyyy-MM-d HH:m:ss:fff");
        }

        foreach (var saveCreature in _encyclopediaData.creatureDatas)
        {
            foreach (var lastCreature in lastData.encyclopediaData.creatureDatas)
            {
                if(saveCreature.id == lastCreature.id)
                {
                    continue;
                }

                _encyclopediaData.creatureDatas.Add(lastCreature);
            }
        }

        foreach (var saveItem in _encyclopediaData.itemDatas)
        {
            foreach (var lastItem in lastData.encyclopediaData.itemDatas)
            {
                if (saveItem.id == lastItem.id)
                {
                    continue;
                }

                _encyclopediaData.itemDatas.Add(lastItem);
            }
        }

        SaveEncyclopediaToCloud(_encyclopediaData);
    }

    public void ResetPlayerData()
    {
        _saveData.userData.stats.Maximum();
    }

    #endregion

    #region Creature

    private void ReadCreaturesData()
    {
        if(_creatureDatas != null)
        {
            _creatureDatas.Clear();
        }

        _creatureDatas = new List<CreatureData>();

        string json = Resources.Load<TextAsset>(_creatureDataPath).text;

        var respons = new
        {
            datas = new List<Creature_Data>()
        };

        var result = JsonConvert.DeserializeAnonymousType(json, respons);

        List<Creature_Data> datas = new List<Creature_Data>();
        datas = result.datas;

        for (int i = 0; i < datas.Count; i++)
        {
            Creature_Data data = datas[i];

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

            temp.defultStatus = data.defultStatus;

            temp.useSkill = data.useSkill;

            temp.skillIndexs = new List<short>();
            if(data.skillIndexs != null)
            {
                temp.skillIndexs.AddRange(data.skillIndexs);
            }

            temp.itemIndexs = new List<short>();
            if(data.itemIndexs != null)
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

    public CreatureData GetCreatureData(int index)
    {
        if (index >= _creatureDatas.Count)
        {
            index %= _creatureDatas.Count;
        }

        return _creatureDatas.Find(x => x.id == (index + 101)).DeepCopy();
    }

    public int GetCreaturDataCount()
    {
        return _creatureDatas.Count;
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

        _itemDatas = new List<ItemData>();

        string json = Resources.Load<TextAsset>(_itemDataPath).text;

        var respons = new
        {
            datas = new List<Item_Data>()
        };

        var result = JsonConvert.DeserializeAnonymousType(json, respons);

        List<Item_Data> datas = new List<Item_Data>();
        datas = result.datas;

        for (int i = 0; i < datas.Count; i++)
        {
            Item_Data data = datas[i];

            ItemData newData = new ItemData();
            newData.id = data.id;
            newData.name = data.name;  
            newData.description = data.description;
            newData.price = data.price;

            newData.tool = new StrengtheningTool();
            newData.tool.duration = data.duration;
            newData.tool.hp = new Stat_In_De(data.hp, data.hpPercentIncreased);
            newData.tool.mp = new Stat_In_De(data.mp, data.mpPercentIncreased);
            newData.tool.ap = new Stat_In_De(data.ap, data.apPercentIncreased);
            newData.tool.exp = new Stat_In_De(data.exp, data.expPercentIncreased);
            newData.tool.coin = new Stat_In_De(data.coin, data.coinPercentIncreased);
            newData.tool.attack = new Stat_In_De(data.attack, data.attackPercentIncreased);
            newData.tool.defence = new Stat_In_De(data.defence, data.defencePercentIncreased);
            newData.tool.dir = data.dir;
            newData.tool.needStatus = data.needStatus;
            newData.tool.grantStatus = data.grantStatus;
            newData.tool.range = data.range;
            newData.tool.value = data.value;
            newData.tool.revealMap = data.revealMap;

            _itemDatas.Add(newData);
        }
    }

    public ItemData GetItemData(int index, bool isID= true)
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

        _skillDatas = new List<SkillData>();

        string json = Resources.Load<TextAsset>(_skillDataPath).text;

        var respons = new
        {
            datas = new List<Skill_Data>()
        };

        var result = JsonConvert.DeserializeAnonymousType(json, respons);

        List<Skill_Data> datas = new List<Skill_Data>();
        datas = result.datas;

        for (int i = 0; i < datas.Count; i++)
        {
            Skill_Data data = datas[i];

            SkillData newData = new SkillData();
            newData.id = data.id;
            newData.name = data.name;
            newData.description = data.description;
            newData.coolDown = data.coolDown;

            newData.tool = new StrengtheningTool();
            newData.tool.duration = data.duration;
            newData.tool.hp = new Stat_In_De(data.hp, data.hpPercentIncreased);
            newData.tool.mp = new Stat_In_De(data.mp, data.mpPercentIncreased);
            newData.tool.ap = new Stat_In_De(data.ap, data.apPercentIncreased);
            newData.tool.exp = new Stat_In_De(data.exp, data.expPercentIncreased);
            newData.tool.coin = new Stat_In_De(data.coin, data.coinPercentIncreased);
            newData.tool.attack = new Stat_In_De(data.attack, data.attackPercentIncreased);
            newData.tool.defence = new Stat_In_De(data.defence, data.defencePercentIncreased);
            newData.tool.dir = data.dir;
            newData.tool.needStatus = data.needStatus;
            newData.tool.grantStatus = data.grantStatus;
            newData.tool.range = data.range;
            newData.tool.value = data.value;
            newData.tool.revealMap = data.revealMap;

            _skillDatas.Add(newData);
        }
    }

    public SkillData GetskillData(int index)
    {
        return _skillDatas.Find(x => x.id == (index));
    }

    public int GetSkillDataCount()
    {
        return _skillDatas.Count;
    }

    #endregion

    #region Tutorial

    public int GetSpriteCount()
    {
        return _soTutorial.GetSpriteCount();
    }

    public Sprite GetTutorialSprite(int id)
    {
        return _soTutorial.GetSprite(id);
    }

    public Tutorial_Data GetTutorialData(int id)
    {
        return _soTutorial.GetData(id);
    }

    #endregion

    public Sprite GetCreatureSprite(eMapObject type)
    {
        BlockImageTemplate blockImageTemplate = _creatureSpriteTemplate.Find(x => x.type == type);

        Debug.Log(type);

        if(blockImageTemplate.sprites == null)
        {
            return blockImageTemplate.sprite;
        }

        if (blockImageTemplate.sprites.Count == 0)
        {
            return blockImageTemplate.sprite;
        }

        int index = -1;

        if(type == eMapObject.Ground)
        {
            int ran = UnityEngine.Random.Range(0, 10);

            if(ran >= 8)
            {
                index = UnityEngine.Random.Range(1, blockImageTemplate.sprites.Count);
            }
        }

        if(index == -1)
        {
            return blockImageTemplate.sprite;
        }

        return blockImageTemplate.sprites[index];
    }
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
