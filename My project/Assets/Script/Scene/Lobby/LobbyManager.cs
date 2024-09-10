using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private CreateCharacterProfile _createCharacterProfile = null;

    [Header("UI")]
    [SerializeField] private Button _buttonStart = null;
    [SerializeField] private Button _buttonSettings = null;
    [SerializeField] private Button _buttonDictionary = null;

    private const string SAVE_DATA = "SAVE_DATA";

    private void Start()
    {
        GameManager.instance.tools.Fade(true, null);

        _buttonStart.onClick.AddListener(OnStart);
        _buttonSettings.onClick.AddListener(OnSettings);
        _buttonDictionary.onClick.AddListener(OnDictionary);

        _createCharacterProfile.Initialize(NewGame);

        GameManager.instance.dataManager.ReadGameData();
    }

    private void OnStart()
    {
        if(GameManager.instance.dataManager.CheckSaveData() == false)
        {
            _createCharacterProfile.Open();

            return;
        }

        UiManager.instance.OpenPopup(string.Empty, "Saved data exists." + "\n" + " Would you like to continue?", string.Empty, string.Empty, () =>
        {
            GameManager.instance.dataManager.LoadData(() => 
            {
                GameManager.instance.tools.SceneChange(eScene.Game);
            });

        }, () =>
        {
            _createCharacterProfile.Open();
        });
    }

    private void OnSettings()
    {
        UiManager.instance.OpenSettings();
    }

    private void OnDictionary()
    {
        UiManager.instance.OpenEncyclopedia();
    }

    private void NewGame()
    {
        GameManager.instance.tools.SceneChange(eScene.Game);
    }
}
