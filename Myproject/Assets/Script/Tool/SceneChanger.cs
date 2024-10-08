using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class SceneChanger : MonoBehaviour
{
    private Action _onResultCallback = null;

    public void Initialize()
    {
        SceneManager.sceneLoaded += LoadedsceneEvent;
    }

    public void ChangeScene(eScene _scene, Action onResultCallback)
    {
        if (onResultCallback != null)
        {
            _onResultCallback = onResultCallback;
        }

        SceneManager.LoadSceneAsync(_scene.ToString());
        SceneManager.LoadScene(eScene.Ui.ToString(), LoadSceneMode.Additive);
    }

    private void LoadedsceneEvent(Scene scene, LoadSceneMode mode)
    {
        _onResultCallback?.Invoke();
}
}
