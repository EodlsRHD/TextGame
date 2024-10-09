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
    private Action<bool> _onDeleteCallback = null;

    private const string GPGS_SAVEFILE = "SAVEFILE";
    private const string GPGS_ENCYCLOPEDIA = "ENCYCLOPEDIA";

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

    #region Save

    public void SaveData(string json, Action<bool> onSaveResultCallback)
    {
        if(onSaveResultCallback != null)
        {
            _onSaveResultCallback = onSaveResultCallback;
        }

        _saveJson = json;

        OpenSaveGame(GPGS_SAVEFILE);
    }

    public void SaveEncylopediaData(string json, Action<bool> onSaveResultCallback)
    {
        if (onSaveResultCallback != null)
        {
            _onSaveResultCallback = onSaveResultCallback;
        }

        _saveJson = json;

        OpenSaveGame(GPGS_ENCYCLOPEDIA);
    }

    private void OpenSaveGame(string Filename)
    {
        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;

        saveGameClient.OpenWithAutomaticConflictResolution(Filename,
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

        _saveJson = string.Empty;
        _onSaveResultCallback?.Invoke(true);
    }

    #endregion

    #region Load

    public void LoadData(Action<bool, string> onLoadResultCallback)
    {
        if (onLoadResultCallback != null)
        {
            _onLoadResultCallback = onLoadResultCallback;
        }

        OpenLoadGame(GPGS_SAVEFILE);
    }

    public void LoadEncylopediaData(Action<bool, string> onLoadResultCallback)
    {
        if (onLoadResultCallback != null)
        {
            _onLoadResultCallback = onLoadResultCallback;
        }

        OpenLoadGame(GPGS_ENCYCLOPEDIA);
    }

    private void OpenLoadGame(string filename)
    {
        ISavedGameClient saveGameClient = PlayGamesPlatform.Instance.SavedGame;

        saveGameClient.OpenWithAutomaticConflictResolution(filename,
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

        saveGameClient.ReadBinaryData(data, onSavedGameDataRead);
    }

    
    private void onSavedGameDataRead(SavedGameRequestStatus status, byte[] loadedData)
    {
        string data = Encoding.UTF8.GetString(loadedData);

        if(data.Length == 0)
        {
            Debug.LogError("onSavedGameDataRead Failed");

            _onLoadResultCallback?.Invoke(false, string.Empty);

            return;
        }

        _onLoadResultCallback?.Invoke(true, data);
    }

    #endregion

    #region Delete

    public void DeleteData(Action<bool> onDeleteCallback)
    {
        if(onDeleteCallback != null)
        {
            _onDeleteCallback = onDeleteCallback;
        }

        DeleteGameData(GPGS_SAVEFILE);
    }

    void DeleteGameData(string filename)
    {
        // Open the file to get the metadata.
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.OpenWithAutomaticConflictResolution(filename, DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime, DeleteSavedGame);
    }

    public void DeleteSavedGame(SavedGameRequestStatus status, ISavedGameMetadata game)
    {
        if (status != SavedGameRequestStatus.Success)
        {
            Debug.LogError("DeleteSavedGame Failed");

            _onDeleteCallback?.Invoke(false);

            return;
        }

        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.Delete(game);

        _onDeleteCallback?.Invoke(true);
    }

    #endregion
}
