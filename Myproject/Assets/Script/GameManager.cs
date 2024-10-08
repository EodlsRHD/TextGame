using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance = null;
    public static GameManager instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = new GameManager();
            }

            return _instance;
        }
    }

    [Header("Tool Proxy"), SerializeField] private ToolProxy _toolProxy = null;
    [Header("Data Manager"), SerializeField] private DataManager _dataManager = null;
    [Header("Sound Manager"), SerializeField] private SoundManager _soundManager = null;
    [Header("Google Ads"), SerializeField] private GoogleAds _googleAds = null;

    private bool _isMapBackgroundUpdate = false;

    #region GetSet

    public ToolProxy tools
    {
        get { return _toolProxy; }
    }

    public DataManager dataManager
    {
        get { return _dataManager; }
    }

    public SoundManager soundManager
    {
        get { return _soundManager; }
    }

    public GoogleAds googleAds
    {
        get { return _googleAds; }
    }

    public bool isMapBackgroundUpdate { get; set; }

    #endregion

    private void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(this.gameObject);

        _toolProxy.Initialize(_soundManager.VolumeDown);
        _dataManager.Initialize();
    }

    public void GameError()
    {
        _dataManager.SaveDataToCloud();
        Application.Quit();
    }
}
