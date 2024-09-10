using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IngameUI : MonoBehaviour
{
    [Header("Top")]
    [SerializeField] private Button _buttonGameMenu = null;
    [SerializeField] private TMP_Text _textRound = null;

    public void Initialize()
    {
        _buttonGameMenu.onClick.AddListener(OnOpenGameMenu);
    }

    private void OnOpenGameMenu()
    {
        UiManager.instance.OpenGameMenu();
    }
}
