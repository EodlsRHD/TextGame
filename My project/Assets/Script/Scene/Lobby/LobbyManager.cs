using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button _buttonStart = null;
    [SerializeField] private Button _buttonSettings = null;
    [SerializeField] private Button _buttonDictionary = null;

    private const string SAVE_DATA = "SAVE_DATA";

    [Header("TEST"), SerializeField] private bool isTEST = false;

    private void Start()
    {
        GameManager.instance.tools.Fade(true, null);

        _buttonStart.onClick.AddListener(OnStart);
        _buttonSettings.onClick.AddListener(OnSettings);
        _buttonDictionary.onClick.AddListener(OnDictionary);
    }

    private void OnStart()
    {
        Debug.LogWarning("TEST MODE ¿‘¥œ¥Ÿ.");
        int value = isTEST ? PlayerPrefs.GetInt(SAVE_DATA) : 0;

        if(value == 0) // no data
        {
            GameManager.instance.dataManager.CreateSaveData();
        }

        GameManager.instance.dataManager.ReadSaveData();
    }

    private void OnSettings()
    {
        UiManager.instance.OpenPopup("UI", "Settings is still in preparation.", string.Empty, null);
    }

    private void OnDictionary()
    {
        UiManager.instance.OpenPopup("UI", "Dictionary is Still in preparation.", string.Empty, null);
    }

    private void NewGame()
    {

    }
}
