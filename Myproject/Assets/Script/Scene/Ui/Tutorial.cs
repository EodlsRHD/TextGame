using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

public class Tutorial : MonoBehaviour
{
    [Header("Guide")]
    [SerializeField] private GameObject _objGuide = null;
    [SerializeField] private TutorialAnswerTemplate _template = null;
    [SerializeField] private GameObject _objAnswerParant = null;

    [Space(10)]

    [SerializeField]private Button _buttonSkipGuide = null;

    [Space(10)]

    [SerializeField] private GameObject _objAnswer = null;
    [SerializeField] private GameObject _objSpeechBubble = null;
    [SerializeField] private TMP_Text _textSpeechBubble;

    [Header("Menu")]
    [SerializeField] private GameObject _objMenu = null;
    [SerializeField] private Image _menuImage = null;
    [SerializeField] private Button _buttonClose = null;
    [SerializeField] private Button _buttonNext = null;
    [SerializeField] private Button _buttonPrevious = null;

    private Action _onCloseCallback = null;

    private WaitForSeconds _waitBubbleDuration = new WaitForSeconds(0.05f);
    private Coroutine _coContentOutput = null;

    private eTutorialQuest _quest = eTutorialQuest.Non;
    private int _speechIndex = -1;

    private int _pageCount = 0;

    private bool _start = false;
    private bool _isDone = false;
    private bool _isMenu = false;

    public void Initialize(Action onCloseCallback)
    {
        if(onCloseCallback != null)
        {
            _onCloseCallback = onCloseCallback;
        }

        _buttonSkipGuide.onClick.AddListener(OnSkipGuide);

        _buttonClose.onClick.AddListener(OnClose);
        _buttonNext.onClick.AddListener(OnNext);
        _buttonPrevious.onClick.AddListener(OnPrevious);

        _template.Initialize(OnClickAnswer);

        _textSpeechBubble.text = string.Empty;
        _speechIndex = 0;

        _objGuide.SetActive(false);
        _objMenu.SetActive(false);
        this.gameObject.SetActive(false);
    }

    public void Open(bool isMenu)
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        _isMenu = isMenu;

        if (isMenu == true)
        {
            GameManager.instance.soundManager.PlaySfx(eSfx.TurnPage);

            _pageCount = 0;

            SetImage();

            _objMenu.SetActive(isMenu);
            this.gameObject.SetActive(true);

            return;
        }

        _objGuide.SetActive(true);
        this.gameObject.SetActive(true);

        GameManager.instance.soundManager.PlaySfx(eSfx.MenuOpen);

        GameManager.instance.tools.Move_Anchor_XY(eUiDir.X, _objSpeechBubble.GetComponent<RectTransform>(), 0f, 0.5f, 0, Ease.OutBack, null);
        GameManager.instance.tools.Move_Anchor_XY(eUiDir.Y, _objAnswer.GetComponent<RectTransform>(), 0f, 0.5f, 0, Ease.OutBack, () => 
        {
            if (_isDone == true)
            {
                ContentOutput("맵 중앙에 다음층으로 내려가는 계단이 있어!");
                InstantiateAnswer(null, eTutorialQuest.Non);

                return;
            }

            DataManager.TutorialData data = GameManager.instance.dataManager.GetTutorialData(_speechIndex);

            if (_start == true)
            {
                if (IngameManager.instance.isHuntMonster == false)
                {
                    ContentOutput("사냥하고 말해줘!");
                    InstantiateAnswer(null, eTutorialQuest.Non);

                    return;
                }
            }

            ContentOutput(data.content);
            InstantiateAnswer(data.answers, (eTutorialQuest)data.isQuest);

            _start = true;
        });
    }

    public void Close()
    {
        if (_isMenu == true)
        {
            GameManager.instance.soundManager.PlaySfx(eSfx.GotoLobby);
        }

        _isMenu = false;

        this.gameObject.SetActive(false);
        _objGuide.SetActive(false);
        _objMenu.SetActive(false);
    }

    private void OnClose()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.MenuClose);

        GameManager.instance.tools.Move_Anchor_XY(eUiDir.X, _objSpeechBubble.GetComponent<RectTransform>(), 554f, 0.5f, 0, Ease.InBack, null);
        GameManager.instance.tools.Move_Anchor_XY(eUiDir.Y, _objAnswer.GetComponent<RectTransform>(), -700f, 0.5f, 0, Ease.InBack, () => 
        {
            _onCloseCallback?.Invoke();
        });
    }

    #region Menu

    private void OnNext()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        if (GameManager.instance.dataManager.GetSpriteCount() == (_pageCount + 1))
        {
            return;
        }

        _pageCount += 1;

        SetImage();
    }

    private void OnPrevious()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        if (_pageCount == 0)
        {
            return;
        }

        _pageCount -= 1;

        SetImage();
    }

    private void SetImage()
    {
        Sprite sprite = GameManager.instance.dataManager.GetTutorialSprite(_pageCount);
        _menuImage.sprite = sprite;
    }

    #endregion

    #region Guide

    private void OnSkipGuide()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        UiManager.instance.OpenPopup("초급 길잡이", "정말로 넘길거야?", "응", "아니", () => 
        {
            _buttonSkipGuide.gameObject.SetActive(false);
            _isDone = true;

            OnClose();
        }, null);
    }

    private void OnClickAnswer(int next, eTutorialQuest type)
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        _quest = type;

        if(next == -1)
        {
            _isDone = true;
            OnClose();

            return;
        }

        switch (type)
        {
            case eTutorialQuest.Non:
                {
                    DataManager.TutorialData data = GameManager.instance.dataManager.GetTutorialData(next);
                    ContentOutput(data.content);
                    InstantiateAnswer(data.answers, (eTutorialQuest)data.isQuest);
                }
                break;

            case eTutorialQuest.Attack:
                {
                    _speechIndex = next;
                    OnClose();
                }
                break;
        }
    }

    private void ContentOutput(string content)
    {
        if(_coContentOutput != null)
        {
            StopCoroutine(_coContentOutput);
        }

        _coContentOutput = StartCoroutine(Co_ContentOutput(content));
    }

    IEnumerator Co_ContentOutput(string content)
    {
        _textSpeechBubble.text = string.Empty;

        for (int i = 0; i < content.Length; i++)
        {
            _textSpeechBubble.text += content[i].ToString();

            yield return _waitBubbleDuration;
        }
    }

    private void InstantiateAnswer(List<Tutorial_answer> answers, eTutorialQuest type)
    {
        _objAnswerParant.SetActive(false);

        if(answers == null)
        {
            Invoke(nameof(OnClose), 2f);

            return;
        }

        DeleteTemplate();

        for (int i = 0; i < answers.Count; i++)
        {
            var obj = Instantiate(_template, _objAnswerParant.transform);
            var com = obj.GetComponent<TutorialAnswerTemplate>();

            com.Initialize(OnClickAnswer);
            com.Set(answers[i], type);
        }

        _objAnswerParant.SetActive(true);
    }

    private void DeleteTemplate()
    {
        var child = _objAnswerParant.transform.transform.GetComponentsInChildren<Transform>();

        foreach (var iter in child)
        {
            if (iter != _objAnswerParant.transform.transform)
            {
                Destroy(iter.gameObject);
            }
        }

        _objAnswerParant.transform.transform.DetachChildren();
    }

    #endregion
}
