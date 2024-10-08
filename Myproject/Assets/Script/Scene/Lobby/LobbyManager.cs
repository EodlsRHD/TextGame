using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyManager : MonoBehaviour
{
    [SerializeField] private CreateCharacterProfile _createCharacterProfile = null;

    [Header("UI")]
    [SerializeField] private Button _buttonStart = null;
    [SerializeField] private Button _buttonSettings = null;
    [SerializeField] private Button _buttonDictionary = null;
    [SerializeField] private TMP_Text _textVersion = null;

    private void Start()
    {
        GameManager.instance.tools.Fade(true, null);

        _buttonStart.onClick.AddListener(OnStart);
        _buttonSettings.onClick.AddListener(OnSettings);
        _buttonDictionary.onClick.AddListener(OnDictionary);

        _createCharacterProfile.Initialize(NewGame);

        _textVersion.text = Application.version;

        GameManager.instance.soundManager.PlayBgm(eBgm.Lobby);
        GameManager.instance.dataManager.ReadGameData();
    }

    private void OnStart()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        if(GameManager.instance.dataManager.CheckSaveData() == false)
        {
            _createCharacterProfile.Open();

            return;
        }

        UiManager.instance.OpenPopup(string.Empty, "����� �����Ͱ� �����մϴ�." + "\n" + " �̾ �Ͻðڽ��ϱ�?", string.Empty, string.Empty, () =>
        {
            GameManager.instance.dataManager.LoadDataToCloud(() => 
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
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        UiManager.instance.OpenSettings();
    }

    private void OnDictionary()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        UiManager.instance.OpenEncyclopedia();
    }

    private void NewGame()
    {
        GameManager.instance.tools.Fade(false, () =>
        {
            GameManager.instance.tools.SceneChange(eScene.Game);
        });
    }
}
