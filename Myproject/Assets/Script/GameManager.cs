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
    [Header("Google Play Game Serveices"), SerializeField] private GooglePlayGameServeice _googlePlayGameServeice = null;

    [Header("Loading Progress")]
    [SerializeField] private GameObject _objLoading = null;
    [SerializeField] private LoadSpinner _loadSpinner = null;

    private bool _loginFaild = false;

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

    public GooglePlayGameServeice googlePlayGameServeice
    {
        get { return _googlePlayGameServeice; }
    }

    public bool isMapBackgroundUpdate 
    {
        get 
        { 
            return PlayerPrefs.GetInt("MAP_BACKGROUND", 0) == 1 ? true : false;
        }
        set 
        {
            PlayerPrefs.SetInt("MAP_BACKGROUND", value == true ? 1 : 0);
        }
    }

    public bool isViewRanking
    {
        get 
        { 
            return PlayerPrefs.GetInt("VIEW_RANKINGS", 0) == 1 ? true : false; 
        }
        set
        {
            PlayerPrefs.SetInt("VIEW_RANKINGS", value == true ? 1 : 0);
        }
    }

    public bool GpgsloginFaild
    {
        get { return _loginFaild; }
    }

    #endregion

    private void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(this.gameObject);

#if UNITY_EDITOR
        PlayerPrefs.DeleteAll();
#endif

        _toolProxy.Initialize(_soundManager.VolumeDown);
        _dataManager.Initialize();
        _soundManager.Initialize();
        _googleAds.Initialize();
        _googlePlayGameServeice.Initialize(InstallGpgs);
        _loadSpinner.Initialize();

        _objLoading.SetActive(false);
    }

    private void InstallGpgs()
    {
        _loginFaild = true;
    }

    private void OnApplicationQuit()
    {
        Debug.LogError("OnApplicationQuit");
        _dataManager.saveAllData();
    }

    #region Loading Progress

    public void StartLoad()
    {
        _objLoading.SetActive(true);
        _loadSpinner.StartLoading();
    }

    public void StopLoad()
    {
        _loadSpinner.StopLoading();
        _objLoading.SetActive(false);
    }

    #endregion
}
