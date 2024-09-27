using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FadeScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroup _cgSplashScreen = null;
    [Space(10)]
    [SerializeField] private float _minusValue = 0.01f;

    public void Initialize()
    {
         
    }

    public void FadeIn(Action _onResultCallback)
    {
        StartCoroutine(Co_FadeIn(_onResultCallback));
    }
    public void FadeOut(Action _onResultCallback)
    {
        StartCoroutine(Co_FadeOut(_onResultCallback));
    }

    IEnumerator Co_FadeIn(Action _onResultCallback)
    {
        _cgSplashScreen.alpha = 1.0f;

        yield return null;

        while (_cgSplashScreen.alpha != 0)
        {
            _cgSplashScreen.alpha -= _minusValue;

            yield return null;
        }

        _cgSplashScreen.alpha = 0f;

        _onResultCallback?.Invoke();
        yield break;
    }

    IEnumerator Co_FadeOut(Action _onResultCallback)
    {
        _cgSplashScreen.alpha = 0f;

        yield return null;

        while (_cgSplashScreen.alpha != 0)
        {
            _cgSplashScreen.alpha -= _minusValue;

            yield return null;
        }

        _cgSplashScreen.alpha = 1.0f;

        _onResultCallback?.Invoke();
        yield break;
    }
}
