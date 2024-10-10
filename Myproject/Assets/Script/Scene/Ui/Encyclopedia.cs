using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Encyclopedia : MonoBehaviour
{
    [SerializeField] private Button _buttonClose = null;

    [Header("Toggle")]
    [SerializeField] private Toggle _toggleCreature = null;
    [SerializeField] private Toggle _toggleItem = null;
    [SerializeField] private Toggle _toggleAchievements = null;

    [Header("Tap Menu")]
    [SerializeField] private GameObject _objCreature_Item = null;
    [SerializeField] private Transform _trCreature_Item = null;
    [SerializeField] private EncyclopediaTemplate _templateCreature_Item = null;
    [SerializeField] private GameObject _objEmptyLabel = null;

    [Space(10)]

    [SerializeField] private GameObject _objAchievements = null;
    [SerializeField] private Transform _trAchievements = null;
    [SerializeField] private EncyclopediaTemplate _templateAchievements = null;

    [Space(10), Header("PlayerInformation")]
    [SerializeField] private Button _buttonOpenPlayerInformation = null;
    [SerializeField] private Button _buttonClosePlayerInformation = null;
    [SerializeField] private GameObject _objPlayerInformation = null;
    [SerializeField] private GameObject _objPlayerInformationMaker = null;

    private Action _onCloseCallback = null;

    private DataManager.Encyclopedia_Data _data = null;

    private Dictionary<int, List<EncyclopediaTemplate>> _templates = null;

    public void Initialize(Action onCloseCallback)
    {
        if (onCloseCallback != null)
        {
            _onCloseCallback = onCloseCallback;
        }

        _buttonClose.onClick.AddListener(OnClose);
        _buttonOpenPlayerInformation.onClick.AddListener(OpenPlayerInformation);
        _buttonClosePlayerInformation.onClick.AddListener(ClosePlayerInformation);

        _toggleCreature.onValueChanged.AddListener((isOn) =>
        {
            if (isOn == false)
            {
                return;
            }

            _toggleItem.isOn = false;
            _toggleAchievements.isOn = false;

            GameManager.instance.soundManager.PlaySfx(eSfx.SceneChange);

            Toggle(0);
        });

        _toggleItem.onValueChanged.AddListener((isOn) =>
        {
            if (isOn == false)
            {
                return;
            }

            _toggleCreature.isOn = false;
            _toggleAchievements.isOn = false;

            GameManager.instance.soundManager.PlaySfx(eSfx.SceneChange);

            Toggle(1);
        });

        _toggleAchievements.onValueChanged.AddListener((isOn) =>
        {
            if(isOn == false)
            {
                return;
            }

            _toggleCreature.isOn = false;
            _toggleItem.isOn = false;

            GameManager.instance.soundManager.PlaySfx(eSfx.SceneChange);

            Toggle(2);
        });

        _objEmptyLabel.SetActive(false);
        _objPlayerInformation.SetActive(false);
        this.gameObject.SetActive(false);
    }

    public void Open()
    {
        GameManager.instance.dataManager.LoadEncyclopediaToCloud((result) =>
        {
            if(result  == false)
            {
                return;
            }

            _data = GameManager.instance.dataManager.CopyEncyclopediaData();
            _templates = new Dictionary<int, List<EncyclopediaTemplate>>();

            _templates.Add(0, new List<EncyclopediaTemplate>());
            for (int i = 0; i < _data.creatureDatas.Count; i++)
            {
                var obj = Instantiate(_templateCreature_Item, _trCreature_Item);
                var com = obj.GetComponent<EncyclopediaTemplate>();

                com.Initialize();
                com.Set(_data.creatureDatas[i]);

                _templates[0].Add(com);
            }

            _templates.Add(1, new List<EncyclopediaTemplate>());
            for (int i = 0; i < _data.itemDatas.Count; i++)
            {
                var obj = Instantiate(_templateCreature_Item, _trCreature_Item);
                var com = obj.GetComponent<EncyclopediaTemplate>();

                com.Initialize();
                com.Set(_data.itemDatas[i]);

                _templates[1].Add(com);
            }

            _templates.Add(2, new List<EncyclopediaTemplate>());

            _toggleCreature.isOn = true;
            _toggleItem.isOn = false;
            _toggleAchievements.isOn = false;

            Toggle(0);

            this.gameObject.SetActive(true);
        });
    }

    public void Close()
    {
        this.gameObject.SetActive(false);
    }

    private void OnClose()
    {
        _onCloseCallback?.Invoke();
    }

    private void OpenPlayerInformation()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        _objPlayerInformation.SetActive(true);
    }

    private void ClosePlayerInformation()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        _objPlayerInformation.SetActive(false);
    }

    private void Toggle(int value)
    {
        for (int i = 0; i < _templates.Count; i++)
        {
            for (int j = 0; j < _templates[i].Count; j++)
            {
                _templates[i][j].Active(false);
            }
        }

        if(value <= 1)
        {
            _objCreature_Item.SetActive(true);
            _objAchievements.SetActive(false);

            if (_templates[value].Count == 0)
            {
                _objEmptyLabel.SetActive(true);

                return;
            }
        }
        if(value == 2)
        {
            _objEmptyLabel.SetActive(false);
            _objCreature_Item.SetActive(false);
            _objAchievements.SetActive(true);
        }

        for (int i = 0; i < _templates[value].Count; i++)
        {
            _templates[value][i].Active(true);
        }
    }
}
