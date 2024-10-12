using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TutorialAnswerTemplate : MonoBehaviour
{
    [SerializeField] private Button _button = null;
    [SerializeField]private TMP_Text _textLabel = null;

    private Action<int, eTutorialQuest> _onClickCallback = null;

    private eTutorialQuest _type = eTutorialQuest.Non;
    private int _next = -1;

    public void Initialize(Action<int, eTutorialQuest> onClickCallback)
    {
        if(onClickCallback != null)
        {
            _onClickCallback = onClickCallback;
        }

        _button.onClick.AddListener(OnClick);

        this.gameObject.SetActive(false);
    }

    public void Set(DataManager.Tutorial_answer answer, eTutorialQuest type)
    {
        _type = type;

        _next = answer.next;
        _textLabel.text = answer.answer;   

        this.gameObject.SetActive(true);
    }

    public void Remove()
    {
        this.gameObject.SetActive(false);
        _textLabel.text = string.Empty;
    }

    private void OnClick()
    {
        _onClickCallback?.Invoke(_next, _type);
    }
}
