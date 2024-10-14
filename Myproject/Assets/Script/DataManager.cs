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
    public class Creature_Data
    {
        public short id = 0;

        public string name = string.Empty;
        public string description = string.Empty;
        public short coin = 0;

        public short hp = 0;
        public short mp = 0;
        public short ap = 0; // Active Point 
        public short exp = 0;
        public short attack = 0;
        public short defence = 0;
        public short vision = 0;
        public short attackRange = 0;
        public short evasion = 0;

        public eStats defultStatus = eStats.Non;

        public eStatus _status = eStatus.Non;
        public byte _statusCount = 0;

        public bool recovery = false;
        public byte recoveryCount = 0;

        public bool useSkill = false;
        public List<short> skillIndexs = null;
        public List<short> itemIndexs = null;

        public int currentNodeIndex = 0;
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
    public class User_Data
    {
        public Creature_Data data = null;

        public ushort level = 1; // 1레벨 (경험치 * 0.4) * 레벨수

        private short _defultHP = 10;
        private short _defultMP = 10;
        private short _defultAP = 3;
        private short _defultEXP = 15;
        private short _defultATTACK = 5;
        private short _defultDEFENCE = 0;
        private short _defultVISION = 1;
        private short _defultATTACKRANGE = 1;

        public short currentHP = 10;
        public short currentMP = 10;
        public short currentAP = 3;
        public short currentEXP = 0;
        public short currentATTACK = 5;
        public short currentDEFENCE = 0;
        public short currentVISION = 3;
        public short currentATTACKRANGE = 10;

        public short Attack_Effect = 0;
        public short Defence_Effect = 0;

        public short Attack_Effect_Per = 0;
        public short Defence_Effect_Per = 0;

        public short EXP_Effect_Per = 0;
        public short Coin_Effect_Per = 0;

        public List<short> itemDataIndexs = null;
        public List<short> skillDataIndexs = null;

        public List<Duration> useSkill = null;
        public List<Duration> useItem = null;

        public List<Skill_CoolDown> coolDownSkill = null;

        #region GetSet

        public short maximumHP
        {
            get { return (short)(_defultHP * data.hp); }
        }

        public short maximumMP
        {
            get { return (short)(_defultMP * data.mp); }
        }

        public short maximumAP
        {
            get { return (short)(_defultAP * data.ap); }
        }

        public short maximumEXP
        {
            get { return (short)(level * _defultEXP); }
        }

        public short maximumATTACK
        {
            get { return (short)((_defultATTACK * data.attack) + ((_defultATTACK * data.attack) * 0.01f * Attack_Effect_Per)); }
        }

        public short maximumDEFENCE
        {
            get { return (short)(_defultDEFENCE * data.defence + ((_defultDEFENCE * data.defence) * 0.01f * Defence_Effect_Per)); }
        }

        public short maximumVISION
        {
            get { return (short)(_defultVISION * data.vision); }
        }

        public short maximumATTACKRANGE
        {
            get { return (short)(_defultATTACKRANGE * data.attackRange); }
        }

        #endregion
    }

    [Serializable]
    public class Skill_Data
    {
        public short id = 0;

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
        public short id = 0;

        public string name = string.Empty;
        public string description = string.Empty;
        public short price = 0;

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

        public int currentNodeIndex = 0;
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

        public List<Creature_Data> creatureDatas = null;
        public List<Item_Data> itemDatas = null;
        public List<Achievements_Data> _achievementsDatas = null;
    }

    [Serializable]
    public class Save_Data
    {
        public short round = 0;

        public User_Data userData = null;
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

    [Serializable]
    public class Tutorial_answer
    {
        public string answer = string.Empty;
        public int next = 0;
    }

    [Serializable]
    public class BlockImageTemplate
    {
        public eMapObject type = eMapObject.Non;
        public Sprite sprite = null;

        public List<Sprite> sprites = null;
    }

    [Serializable]
    public class SoundTemplate
    {
        public eSfx type = eSfx.Non;
        public AudioClip clip = null;
        public List<AudioClip> clips = null;
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

        _saveData.userData = new User_Data();
        _saveData.userData.itemDataIndexs = new List<short>() { 501 };
        _saveData.userData.skillDataIndexs = new List<short>() { 301 };

        _saveData.userData.useSkill = new List<Duration>();
        _saveData.userData.useItem = new List<Duration>();
        _saveData.userData.coolDownSkill = new List<Skill_CoolDown>();

        _saveData.userData.data = new Creature_Data();
        _saveData.userData.data.hp = 1;
        _saveData.userData.data.mp = 1;
        _saveData.userData.data.ap = 1;
        _saveData.userData.data.attack = 0;
        _saveData.userData.data.defence = 0;
        _saveData.userData.data.vision = 1;
        _saveData.userData.data.attackRange = 1;
        _saveData.userData.data.coin = 0;

        _saveData.mapData = new Map_Data();
        _saveData.mapData.mapSize = _mapSize;
        _saveData.mapData.nodeDatas = new List<Node_Data>();
        _saveData.mapData.monsterDatas = new List<Creature_Data>();
        _saveData.mapData.itemDatas = new List<Item_Data>();
        _saveData.mapData.npcDatas = new List<Npc_Data>();

        _saveData.encyclopediaData = new Encyclopedia_Data();
        _saveData.encyclopediaData.creatureDatas = new List<Creature_Data>();
        _saveData.encyclopediaData.itemDatas = new List<Item_Data>();
        _saveData.encyclopediaData._achievementsDatas = new List<Achievements_Data>();



        _encyclopediaData = new Encyclopedia_Data();
        _encyclopediaData.creatureDatas = new List<Creature_Data>();
        _encyclopediaData.itemDatas = new List<Item_Data>();
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
            _encyclopediaData.creatureDatas = new List<Creature_Data>();
            _encyclopediaData.itemDatas = new List<Item_Data>();
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
            _encyclopediaData.creatureDatas = new List<Creature_Data>();
            _encyclopediaData.itemDatas = new List<Item_Data>();
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
            _encyclopediaData.creatureDatas = new List<Creature_Data>();
            _encyclopediaData.itemDatas = new List<Item_Data>();
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
            _encyclopediaData.creatureDatas = new List<Creature_Data>();
            _encyclopediaData.itemDatas = new List<Item_Data>();
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
        _saveData.userData.currentHP = _saveData.userData.maximumHP;
        _saveData.userData.currentMP = _saveData.userData.maximumMP;
        _saveData.userData.currentAP = _saveData.userData.maximumAP;
        _saveData.userData.currentATTACK = _saveData.userData.maximumATTACK;
        _saveData.userData.currentDEFENCE = _saveData.userData.maximumDEFENCE;
        _saveData.userData.currentVISION = _saveData.userData.maximumVISION;
        _saveData.userData.currentATTACKRANGE = _saveData.userData.maximumATTACKRANGE;
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
