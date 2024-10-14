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

    private const int currentScreenWidth = 1920;
    private const int currentScreenHeight = 3414;

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

    public void Move_XY(eDir type, RectTransform tr, float targetPosition, float duration, float delay, Ease ease, Action onResultCallback)
    {
        switch (type)
        {
            case eDir.X:
                _uiControl.Move_X(tr, targetPosition, duration, delay, ease, onResultCallback);
                break;

            case eDir.Y:
                _uiControl.Move_Y(tr, targetPosition, duration, delay, ease, onResultCallback);
                break;
        }
    }

    public void Move_Local(RectTransform tr, Vector3 targetPosition, float duration, float delay, Ease ease, Action onResultCallback)
    {
        _uiControl.Move_Local(tr, targetPosition, duration, delay, ease, onResultCallback);
    }

    // h : 1920, w : 1080

    public void Move_Local_XY(eDir type, RectTransform tr, float targetPosition, float duration, float delay, Ease ease, Action onResultCallback)
    {
        switch (type)
        {
            case eDir.X:
                {
                    float screenWidth = Screen.width;
                    float x = targetPosition;
                    float ratio = 0;

                    if (screenWidth != currentScreenWidth)
                    {
                        ratio = screenWidth / currentScreenWidth;
                        x *= ratio;
                    }

                    _uiControl.Move_Local_X(tr, x, duration, delay, ease, onResultCallback);
                }
                break;

            case eDir.Y:
                {
                    float screenHeight = Screen.height;
                    float x = targetPosition;
                    float ratio = 0;

                    if (screenHeight != currentScreenHeight)
                    {
                        ratio = screenHeight / currentScreenHeight;
                        x *= ratio;
                    }

                    _uiControl.Move_Local_Y(tr, x, duration, delay, ease, onResultCallback);
                }
                break;
        }
    }
}
