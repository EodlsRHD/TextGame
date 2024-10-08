using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class Tutorial : MonoBehaviour
{
    [SerializeField] private Button _buttonSkip = null;
    [SerializeField] private Button _buttonNext = null;
    [SerializeField] private Button _buttonPrevious = null;
    [SerializeField] private Image _imageTemplate = null;

    [Space(10)]

    [SerializeField] private List<Sprite> _sprites = new List<Sprite>();

    private Action _onCloseCallback = null;

    public void Initialize(Action onCloseCallback)
    {
        if(onCloseCallback != null)
        {
            _onCloseCallback = onCloseCallback;
        }

        this.gameObject.SetActive(false);
    }

    public void Open()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        for (int i = 0; i < _sprites.Count; i++)
        {

        }

        this.gameObject.SetActive(true);
    }
}
