using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using System.Text;
using System;

public class GooglePlayGameServeice : MonoBehaviour
{
    private Action<bool> _onSaveResultCallback = null;
    private Action<bool, string> _onLoadResultCallback = null;
    private Action<bool> _onCheckResultCallback = null;

    private const string SAVEFILE = "SAVEFILE";

    private string _saveJson = string.Empty;

    public void Initialize()
    {
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
    }

    public void SignIn()
    {
        PlayGamesPlatform.Instance.Authenticate((status) => 
        {
            if (status != SignInStatus.Success)
            {
                Debug.LogError("sign in Failed !");

                return;
            }

            string name = PlayGamesPlatform.Instance.GetUserDisplayName();
            string id = PlayGamesPlatform.Instance.GetUserId();
            string imgUrl = PlayGamesPlatform.Instance.GetUserImageUrl();

            Debug.LogError("sign in Success !     " + name + "     " + id + "     " + imgUrl);
        });
    }

    public void SignOut()
    {

    }

    public void SaveData(string json, Action<bool> onSaveResultCallback)
    {
        if(onSaveResultCallback != null)
        {
            _onSaveResultCallback = onSaveResultCallback;
        }

        _saveJson = json;

        OpenSaveGame();
    }

    public void LoadData(Action<bool, string> onLoadResultCallback)
    {
        if(onLoadResultCallback !=null)
        {
            _onLoadResultCallback = onLoadResultCallback;
        }

        OpenLoadGame();
    }

    private void OpenSaveGame()
    {
        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;

        // 데이터 접근
        saveGameClient.OpenWithAutomaticConflictResolution(SAVEFILE,
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLastKnownGood,
            onsavedGameOpend);
    }


    private void onsavedGameOpend(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;

        if (status != SavedGameRequestStatus.Success)
        {
            Debug.LogError("onsavedGameOpend Failed");

            _onSaveResultCallback?.Invoke(false);

            return;

        }
        var update = new SavedGameMetadataUpdate.Builder().Build();

        byte[] data = Encoding.UTF8.GetBytes(_saveJson);

        saveGameClient.CommitUpdate(game, update, data, OnSavedGameWritten);
    }
    
    private void OnSavedGameWritten(SavedGameRequestStatus status, ISavedGameMetadata data)
    {
        if (status != SavedGameRequestStatus.Success)
        {
            Debug.LogError("OnSavedGameWritten Failed");

            _onSaveResultCallback?.Invoke(false);

            return;
        }

        _onSaveResultCallback?.Invoke(true);
    }

    private void OpenLoadGame()
    {
        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;

        saveGameClient.OpenWithAutomaticConflictResolution(SAVEFILE,
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLastKnownGood,
            LoadGameData);
    }

    private void LoadGameData(SavedGameRequestStatus status, ISavedGameMetadata data)
    {
        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;

        if (status != SavedGameRequestStatus.Success)
        {
            Debug.LogError("LoadGameData Failed");

            _onLoadResultCallback?.Invoke(false, string.Empty);

            return;
        }
        Debug.Log("!! GoodLee");

        saveGameClient.ReadBinaryData(data, onSavedGameDataRead);
    }

    
    private void onSavedGameDataRead(SavedGameRequestStatus status, byte[] loadedData)
    {
        string data = Encoding.UTF8.GetString(loadedData);

        if(data.Length == 0)
        {
            _onLoadResultCallback?.Invoke(false, string.Empty);

            return;
        }

        _onLoadResultCallback?.Invoke(true, data);
    }
}
