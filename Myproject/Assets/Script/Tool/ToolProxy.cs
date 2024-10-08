using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ToolProxy : MonoBehaviour
{
    [Header("Fade")]
    [SerializeField] private GameObject _prefabFadeScreen = null;
    [SerializeField] private Transform _parentFadeScreen = null;

    [Header("SceneChanger")]
    [SerializeField] private GameObject _prefabSceneChanger = null;
    [SerializeField] private Transform _parentSceneChanger = null;

    private Action<Action> _onVolumeDown = null;

    public void Initialize(Action<Action> onVolumeDown)
    {
        if(onVolumeDown != null)
        {
            _onVolumeDown = onVolumeDown;
        }

        this.gameObject.SetActive(true);
    }

    public void Fade(bool isIn, Action _onResultCallback)
    {
        var obj = Instantiate(_prefabFadeScreen, _parentFadeScreen);
        var com = obj.GetComponent<FadeScreen>();

        com.Initialize();

        if (isIn == true)
        {
            com.FadeIn(() => 
            { 
                _onResultCallback?.Invoke();  
                Destroy(obj); 
            });

            return;
        }

        _onVolumeDown?.Invoke(() => 
        {
            com.FadeOut(_onResultCallback);
        });
    }

    public void SceneChange(eScene _scene, Action _onResultCallback = null)
    {
        var obj = Instantiate(_prefabSceneChanger, _parentSceneChanger);
        var com = obj.GetComponent<SceneChanger>();

        com.Initialize();
        com.ChangeScene(_scene, _onResultCallback);
    }
}
