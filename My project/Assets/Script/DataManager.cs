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
        public List<Skill_Data> skillDatas = null;

        public int blockIndex = 0;
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
        public string duration = string.Empty;
    }

    [Serializable]
    public class Item_Data
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

    #endregion

    #region Map

    [Serializable]
    public class Map_Data
    {
        public int enterBlockIndex = 0;
        public int exitBlockIndex = 0;

        public List<Block_Data> blockDatas = null;
        public List<Creature_Data> monsterDatas = null;
        public List<Item_Data> itemDatas = null;
    }

    [Serializable]
    public class Block_Data
    {
        public ushort x = 0;
        public ushort y = 0;

        public bool isWalkable = false;
        public bool isMonster = false;
        public bool isUser = false;
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

    //https://ljhyunstory.tistory.com/354

    private void GooglePlayGamesLogin()
    {
    #if UNITY_EDITOR
        return;
    #endif

        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
        PlayGamesPlatform.Instance.Authenticate((status) => { Debug.Log(status.ToString()); });

        GooglePlayGamesRead(true, null);
    }

    public void GooglePlayGamesRead(bool isSave, Action onSaveOrLoadCallback = null)
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
        _saveData = new Save_Data();
        _saveData.round = 0;

        _saveData.userData = new User_Data();
        _saveData.userData.data = new Creature_Data();
        _saveData.userData.data.skillDatas = new List<Skill_Data>();

        _saveData.mapData = new Map_Data();
        _saveData.mapData.blockDatas = new List<Block_Data>();
        _saveData.mapData.monsterDatas = new List<Creature_Data>();

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
    #if UNITY_EDITOR
        return false;
    #endif
        return (_saveData != null);
    }

    public void SaveDataToCloud(Save_Data saveData = null, Action onSaveOrLoadCallback = null)
    {
    #if UNITY_EDITOR
        if(saveData != null)
        {
            _saveData = saveData;
        }

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
        _saveData.mapData.blockDatas = new List<Block_Data>();
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
                if(saveCreature.index == lastCreature.index)
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
                if (saveItem.index == lastItem.index)
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
                if (saveSkill.index == lastSkill.index)
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
        return _creatureDatas.Find(x => x.index == (index + 101));
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
        return _itemDatas.Find(x => x.index == (index + 501));
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
        return _skillDatas.Find(x => x.index == (index + 301));
    }

    #endregion
}
