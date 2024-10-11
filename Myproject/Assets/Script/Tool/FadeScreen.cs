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
        _cgSplashScreen.gameObject.SetActive(false);
    }

    public void FadeIn(Action _onResultCallback)
    {
        _cgSplashScreen.gameObject.SetActive(false);
        StartCoroutine(Co_FadeIn(_onResultCallback));
    }
    public void FadeOut(Action _onResultCallback)
    {
        _cgSplashScreen.gameObject.SetActive(false);
        StartCoroutine(Co_FadeOut(_onResultCallback));
    }

    IEnumerator Co_FadeIn(Action _onResultCallback)
    {
        _cgSplashScreen.alpha = 1.0f;

        _cgSplashScreen.gameObject.SetActive(true);

        yield return null;

        while (true)
        {
            if(_cgSplashScreen.alpha <= 0)
            {
                break;
            }

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

        _cgSplashScreen.gameObject.SetActive(true);

        yield return null;

        while (true)
        {
            if (_cgSplashScreen.alpha >= 1.0f)
            {
                break;
            }

            _cgSplashScreen.alpha += _minusValue;

            yield return null;
        }

        _cgSplashScreen.alpha = 1.0f;

        _onResultCallback?.Invoke();
        yield break;
    }
}
