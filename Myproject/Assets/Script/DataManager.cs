using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.IO;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
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
        public List<Skill_Data> skillDatas = null;

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

        public List<Item_Data> itemDatas = null;


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
            get { return (ushort)(_defultATTACK * data.attack); }
            set { data.attack = (ushort)value; }
        }

        public ushort maximumDEFENCE
        {
            get { return (ushort)(_defultDEFENCE * data.defence); }
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
        public string duration = string.Empty;
    }

    [Serializable]
    public class Item_Data
    {
        public ushort id = 0;

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
    [SerializeField] private string _itemDataPath = string.Empty;
    [SerializeField] private string _skillDataPath = string.Empty;
    [Header("Map Size"), Tooltip("3의 배수이되 홀수여야함"), SerializeField] private int _mapSize = 9;

    private Save_Data _saveData = null;

    private List<Creature_Data> _creatureDatas = null;
    private List<Item_Data> _itemDatas = null;
    private List<Skill_Data> _skillDatas = null;

    private Action _onSaveOrLoadCallback = null;

    public void Initialize()
    {
        GooglePlayGamesLogin();

        this.gameObject.SetActive(true);
    }

    public void ReadGameData()
    {
        ReadCreaturesData();
        ReadItemsData();
        ReadSkillsData();
    }

    #region Google Play Game Services

    //https://toytvstory.tistory.com/2497

    private void GooglePlayGamesLogin()
    {
#if UNITY_EDITOR
        return;
#endif

        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        PlayGamesPlatform.Instance.Authenticate((status) => 
        {
            Debug.Log(status.ToString());

            if (status == SignInStatus.Success)
            {
                GooglePlayGamesRead(false, null);
                return;
            }
        });
    }

    private void GooglePlayGamesRead(bool isSave, Action onSaveOrLoadCallback = null)
    {
        if (onSaveOrLoadCallback != null)
        {
            _onSaveOrLoadCallback = onSaveOrLoadCallback;
        }

        if (CheckLogin() == false)
        {
            GooglePlayGamesLogin();
        }

        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;

        if (isSave  == true)
        {
            savedGameClient.OpenWithAutomaticConflictResolution("SaveData_TextGame", DataSource.ReadCacheOrNetwork, 
                ConflictResolutionStrategy.UseLongestPlaytime, GooglePlayGamesSave);

            return;
        }

        savedGameClient.OpenWithAutomaticConflictResolution("SaveData_TextGame", DataSource.ReadCacheOrNetwork, 
            ConflictResolutionStrategy.UseLongestPlaytime, GooglePlayGamesLoad);
    }

    private void GooglePlayGamesSave(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        if(status != SavedGameRequestStatus.Success)
        {
            UiManager.instance.OpenPopup("System", "Save failed. \n Would you like to retry?", string.Empty, string.Empty, () =>
            {
                GooglePlayGamesRead(true);
            }, null);

            return;
        }

        GooglePlayGamesSaveData(game);
    }

    private void GooglePlayGamesSaveData(ISavedGameMetadata gameMetadata)
    {
        var json = Newtonsoft.Json.JsonConvert.SerializeObject(_saveData);
        byte[] savedData = Encoding.UTF8.GetBytes(json);

        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        SavedGameMetadataUpdate.Builder builder = new SavedGameMetadataUpdate.Builder();
        builder = builder.WithUpdatedDescription(DateTime.Now.ToString());

        SavedGameMetadataUpdate updateMetadata = builder.Build();
        savedGameClient.CommitUpdate(gameMetadata, updateMetadata, savedData, (status, gameMetadata) => 
        {
            if (status != SavedGameRequestStatus.Success)
            {
                UiManager.instance.OpenPopup("System", "Save failed. \n Would you like to retry?", string.Empty, string.Empty, () =>
                {
                    GooglePlayGamesRead(true);
                }, null);

                return;
            }

            UiManager.instance.OpenPopup("System", "Save was successful.", string.Empty, () =>
            {
                _onSaveOrLoadCallback?.Invoke();
                _onSaveOrLoadCallback = null;
            });
        });
    }

    private void GooglePlayGamesLoad(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        if (status != SavedGameRequestStatus.Success)
        {
            UiManager.instance.OpenPopup("System", "Load failed. \n Would you like to retry?", string.Empty, string.Empty, () =>
            {
                GooglePlayGamesRead(false);
            }, null);

            return;
        }

        GooglePlayGamesLoadData(game);
    }

    private void GooglePlayGamesLoadData(ISavedGameMetadata game)
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.ReadBinaryData(game, (status, saveDatas) =>
        {
            if (status != SavedGameRequestStatus.Success)
            {
                UiManager.instance.OpenPopup("System", "Load failed. \n Would you like to retry?", string.Empty, string.Empty, () =>
                {
                    GooglePlayGamesRead(false);
                }, null);

                return;
            }

            var respons = new
            {
                data = new Save_Data()
            };

            var json = Encoding.UTF8.GetString(saveDatas);
            var result = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(json, respons);

            _saveData = result.data;

            UiManager.instance.OpenPopup("System", "Load was successful.", string.Empty, () => 
            {
                _onSaveOrLoadCallback?.Invoke();
                _onSaveOrLoadCallback = null;
            });
        });
    }

    private bool CheckLogin()
    {
        return Social.localUser.authenticated;
    }

    #endregion

    #region SaveData

    public void CreateNewSaveData()
    {
        Debug.LogWarning("TEST Stat Set");

        _saveData = new Save_Data();
        _saveData.round = 0;

        _saveData.userData = new User_Data();
        _saveData.userData.itemDatas = new List<Item_Data>();
        _saveData.userData.data = new Creature_Data();
        _saveData.userData.data.hp = 1;
        _saveData.userData.data.mp = 1;
        _saveData.userData.data.ap = 10;
        _saveData.userData.data.attack = 1;
        _saveData.userData.data.defence = 0;
        _saveData.userData.data.vision = 1;
        _saveData.userData.data.attackRange = 1;
        _saveData.userData.data.skillDatas = new List<Skill_Data>();

        _saveData.mapData = new Map_Data();
        _saveData.mapData.mapSize = _mapSize;
        _saveData.mapData.nodeDatas = new List<Node_Data>();
        _saveData.mapData.monsterDatas = new List<Creature_Data>();

        _saveData.encyclopediaData = new Encyclopedia_Data();
        _saveData.encyclopediaData.creatureDatas = new List<Creature_Data>();
        _saveData.encyclopediaData.itemDatas = new List<Item_Data>();
        _saveData.encyclopediaData.skillData = new List<Skill_Data>();

    }

    public Save_Data CopySaveData()
    {
        return _saveData.DeepCopy();
    }

    public bool CheckSaveData()
    {
        return (_saveData != null);
    }

    public void SaveDataToCloud(Save_Data saveData = null, Action onSaveOrLoadCallback = null)
    {
#if UNITY_EDITOR
        _saveData = new Save_Data();
        onSaveOrLoadCallback?.Invoke();

        return;
#endif

        if (saveData != null)
        {
            _saveData = saveData;
        }

        GooglePlayGamesRead(true, onSaveOrLoadCallback);
    }

    public void LoadDataToCloud(Action onSaveOrLoadCallback = null)
    {
#if UNITY_EDITOR
        onSaveOrLoadCallback?.Invoke();

        return;
#endif

        GooglePlayGamesRead(false, onSaveOrLoadCallback);
    }

    public void FailGame(Save_Data saveData)
    {
        OrganizeEncyclopedia(saveData);

        _saveData.round = 0;

        _saveData.userData = new User_Data();
        _saveData.userData.data = new Creature_Data();
        _saveData.userData.data.skillDatas = new List<Skill_Data>();

        _saveData.mapData = new Map_Data();
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

    public Item_Data GetItemData(int index)
    {
        return _itemDatas.Find(x => x.id == (index + 501)).DeepCopy();
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
        return _skillDatas.Find(x => x.id == (index + 301)).DeepCopy();
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
