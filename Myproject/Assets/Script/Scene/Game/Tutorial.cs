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

    public void Initialize()
    {
        this.gameObject.SetActive(false);
    }

    public void Open()
    {
        for (int i = 0; i < _sprites.Count; i++)
        {

        }

        this.gameObject.SetActive(true);
    }
}
