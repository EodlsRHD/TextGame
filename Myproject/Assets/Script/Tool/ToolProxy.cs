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

    [Header("Ui Control"), SerializeField]private UiControl _uiControl = null;

    private Action<Action> _onVolumeDown = null;

    private const int defultScreenWidth = 1080;
    private const int defultScreenHeight = 1920;

    public void Initialize(Action<Action> onVolumeDown)
    {
        if(onVolumeDown != null)
        {
            _onVolumeDown = onVolumeDown;
        }

        _uiControl.Initialize();

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

        _onVolumeDown?.Invoke(null);
        com.FadeOut(() =>
        {
            _onResultCallback?.Invoke();
        });
    }

    public void SceneChange(eScene _scene, Action _onResultCallback = null)
    { 
        var obj = Instantiate(_prefabSceneChanger, _parentSceneChanger);
        var com = obj.GetComponent<SceneChanger>();

        com.Initialize();
        com.ChangeScene(_scene, _onResultCallback);
    }

    public void Move(RectTransform tr, Vector3 targetPosition, float duration, float delay, Ease ease, Action onResultCallback)
    {
        _uiControl.Move(tr, targetPosition, duration, delay, ease, onResultCallback);
    }

    public void Move_XY(eUiDir type, RectTransform tr, float targetPosition, float duration, float delay, Ease ease, Action onResultCallback)
    {
        switch (type)
        {
            case eUiDir.X:
                _uiControl.Move_X(tr, targetPosition, duration, delay, ease, onResultCallback);
                break;

            case eUiDir.Y:
                _uiControl.Move_Y(tr, targetPosition, duration, delay, ease, onResultCallback);
                break;
        }
    }

    public void Move_Local(RectTransform tr, Vector3 targetPosition, float duration, float delay, Ease ease, Action onResultCallback)
    {
        _uiControl.Move_Local(tr, targetPosition, duration, delay, ease, onResultCallback);
    }

    public void Move_Local_XY(eUiDir type, RectTransform tr, float targetPosition, float duration, float delay, Ease ease, Action onResultCallback)
    {
        switch (type)
        {
            case eUiDir.X:
                {
                    _uiControl.Move_Local_X(tr, targetPosition, duration, delay, ease, onResultCallback);
                }
                break;

            case eUiDir.Y:
                {
                    _uiControl.Move_Local_Y(tr, targetPosition, duration, delay, ease, onResultCallback);
                }
                break;
        }
    }

    public void Move_Anchor_XY(eUiDir type, RectTransform tr, float targetPosition, float duration, float delay, Ease ease, Action onResultCallback)
    {
        switch (type)
        {
            case eUiDir.X:
                {
                    _uiControl.Move_Anchor_X(tr, targetPosition, duration, delay, ease, onResultCallback);
                }
                break;

            case eUiDir.Y:
                {
                    _uiControl.Move_Anchor_Y(tr, targetPosition, duration, delay, ease, onResultCallback);
                }
                break;
        }
    }
}
