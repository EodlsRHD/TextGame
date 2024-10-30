using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

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
    [SerializeField] private TMP_Text _textMaxRound = null;
    [SerializeField] private TMP_Text _textMaxRoundDate = null;
    [SerializeField] private TMP_Text _textMexLevel = null;
    [SerializeField] private TMP_Text _textMaxLevelDate = null;

    [Header("Information")]
    [SerializeField] private TMP_Text _textName = null;
    [SerializeField] private TMP_Text _textDescription = null;

    private Action _onCloseCallback = null;

    private DataManager.EncyclopediaData _data = null;

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

            GameManager.instance.soundManager.PlaySfx(eSfx.TurnPage);

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

            GameManager.instance.soundManager.PlaySfx(eSfx.TurnPage);

            Toggle(1);
        });

        _toggleAchievements.onValueChanged.AddListener((isOn) =>
        {
            if (isOn == false)
            {
                return;
            }

            _toggleCreature.isOn = false;
            _toggleItem.isOn = false;

            GameManager.instance.soundManager.PlaySfx(eSfx.TurnPage);

            Toggle(2);
        });

        _templateAchievements.Initialize(SetInformation);

        _textName.text = string.Empty;
        _textDescription.text = string.Empty;

        _objEmptyLabel.SetActive(false);
        _objPlayerInformation.SetActive(false);
        this.gameObject.SetActive(false);
    }

    public void Open()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.TurnPage);

        GameManager.instance.dataManager.LoadEncyclopediaToCloud((result) =>
        {
            _data = GameManager.instance.dataManager.CopyEncyclopediaData();
            _templates = new Dictionary<int, List<EncyclopediaTemplate>>();

            

            _templates.Add(0, new List<EncyclopediaTemplate>());
            for (int i = 0; i < _data.creatureDatas.Count; i++)
            {
                var obj = Instantiate(_templateCreature_Item, _trCreature_Item);
                var com = obj.GetComponent<EncyclopediaTemplate>();

                com.Initialize(SetInformation);
                com.Set(_data.creatureDatas[i]);

                _templates[0].Add(com);
            }

            _templates.Add(1, new List<EncyclopediaTemplate>());
            for (int i = 0; i < _data.itemDatas.Count; i++)
            {
                var obj = Instantiate(_templateCreature_Item, _trCreature_Item);
                var com = obj.GetComponent<EncyclopediaTemplate>();

                com.Initialize(SetInformation);
                com.Set(_data.itemDatas[i]);

                _templates[1].Add(com);
            }

            _templates.Add(2, new List<EncyclopediaTemplate>());
            for (int i = 0; i < _data._achievementsDatas.Count; i++)
            {
                var obj = Instantiate(_templateAchievements, _trAchievements);
                var com = obj.GetComponent<EncyclopediaTemplate>();

                com.Initialize(SetInformation);
                com.Set(_data._achievementsDatas[i]);

                _templates[2].Add(com);
            }

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

        DeleteTemplate();
    }

    private void OnClose()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.TurnPage);

        _onCloseCallback?.Invoke();
    }

    private void OpenPlayerInformation()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        _textMaxRound.text = _data.maxRound.ToString();
        _textMaxRoundDate.text = _data.maxRoundDate.ToString();

        _textMexLevel.text = _data.maxLevel.ToString();
        _textMaxLevelDate.text = _data.maxLevelDate.ToString();

        _objPlayerInformation.SetActive(true);

        GameManager.instance.tools.Move_Anchor_XY(eUiDir.X, _objPlayerInformation.GetComponent<RectTransform>(), 0f, 0.5f, 0, Ease.OutBack, null);
    }

    private void ClosePlayerInformation()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        GameManager.instance.tools.Move_Anchor_XY(eUiDir.X, _objPlayerInformation.GetComponent<RectTransform>(), 1025f, 0.5f, 0, Ease.InBack, () =>
        {
            _objPlayerInformation.SetActive(false);

            _textMaxRound.text = string.Empty;
            _textMaxRoundDate.text = string.Empty;

            _textMexLevel.text = string.Empty;
            _textMaxLevelDate.text = string.Empty;
        });
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

        _objEmptyLabel.SetActive(_templates[value].Count == 0);

        if (value <= 1)
        {
            _objCreature_Item.SetActive(true);
            _objAchievements.SetActive(false);
        }

        if (value == 2)
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

    private void SetInformation(string name, string description)
    {
        _textName.text = name;
        _textDescription.text = description;
    }

    public void DeleteTemplate()
    {
        var child = _trCreature_Item.transform.GetComponentsInChildren<Transform>();

        foreach (var iter in child)
        {
            if (iter != _trCreature_Item.transform)
            {
                Destroy(iter.gameObject);
            }
        }

        var child2 = _trAchievements.transform.GetComponentsInChildren<Transform>();

        foreach (var iter in child2)
        {
            if (iter != _trAchievements.transform)
            {
                Destroy(iter.gameObject);
            }
        }
    }
}
