using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using static Shop;

public class Bonfire : MonoBehaviour
{
    [Serializable]
    public class BonfireTemplate
    {
        public Button button = null;
        public Image buttonImage = null;
        public TMP_Text _textButtonLabel = null;

        [HideInInspector] public DataManager.Skill_Data skill = null;

        [HideInInspector] public int index = 0;
        [HideInInspector] public bool isBuy = false;

        public void AddListener(int i, DataManager.Skill_Data data, Action<BonfireTemplate> onCallback)
        {
            index = i;
            skill = data;

            _textButtonLabel.text = skill.name;

            button?.onClick.AddListener(() =>
            {
                GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

                onCallback(this);
            });
        }

        public void Buy()
        {
            buttonImage.color = Color.gray;
            isBuy = true;
        }

        public void Clear()
        {
            button.gameObject.SetActive(true);
            _textButtonLabel.text = null;

            skill = null;

            isBuy = false;
        }
    }
    [SerializeField] private Button _buttonClose = null;
    [SerializeField] private Button _buttonRest = null;
    [SerializeField] private Button _buttonSelectSkill = null;

    [Header("Select Skill")]
    [SerializeField] private Button _buttonCloseSelect = null;
    [SerializeField] private Button _buttonSelect = null;
    [SerializeField] private GameObject _objSelectSkill = null;

    [Header("Information")]
    [SerializeField] private GameObject _objInformation = null;
    [SerializeField] private TMP_Text _textName = null;
    [SerializeField] private TMP_Text _textDescription = null;
    [SerializeField] private TMP_Text _textCooldown = null;

    [Header("Goods"), SerializeField] private List<BonfireTemplate> _template = null;

    private Action<string> _onUpdateTextCallback = null;
    private Action<string> _onUpdatePopupCallback = null;
    private Action<int, int, int> _onResultCallback = null;
    private Action<DataManager.User_Data, Action<int>> _onOpenUserSkillCallback = null;

    private DataManager.Npc_Data _npc = null;
    private DataManager.User_Data _user = null;

    private int _selectTemplateIndex = 0;
    private int removeId = 0;
    private bool _isSelect = false;

    public void Initialize(Action<string> onUpdateTextCallback, Action<string> onUpdatePopupCallback, Action<int, int, int> onResultCallback, Action<DataManager.User_Data, Action<int>> onOpenUserSkillCallback)
    {
        if (onUpdateTextCallback != null)
        {
            _onUpdateTextCallback = onUpdateTextCallback;
        }

        if (onUpdatePopupCallback != null)
        {
            _onUpdatePopupCallback = onUpdatePopupCallback;
        }

        if (onResultCallback != null)
        {
            _onResultCallback = onResultCallback;
        }

        if(onOpenUserSkillCallback != null)
        {
            _onOpenUserSkillCallback = onOpenUserSkillCallback;
        }

        _buttonClose.onClick.AddListener(OnClose);
        _buttonRest.onClick.AddListener(OnRest);
        _buttonSelectSkill.onClick.AddListener(OnOpenSelectSkill);

        _buttonCloseSelect.onClick.AddListener(OnCloseSelectSkill);
        _buttonSelect.onClick.AddListener(OnSelect);

        _isSelect = false;

        _buttonSelect.gameObject.SetActive(true);
        this.gameObject.SetActive(false);
    }

    public void Open(DataManager.Npc_Data npc, DataManager.User_Data user)
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        _npc = npc;
        _user = user;

        this.gameObject.SetActive(true);    

        GameManager.instance.tools.Move_Local_XY(eDir.Y, this.GetComponent<RectTransform>(), -1338f, 0.5f, 0, Ease.OutBack, null);
    }

    private void OnClose()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        GameManager.instance.tools.Move_Local_XY(eDir.Y, this.GetComponent<RectTransform>(), -2671f, 0.5f, 0, Ease.InBack, () =>
        {
            this.gameObject.SetActive(false);
        });
    }

    private void OnRest()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        OnClose();

        _onResultCallback?.Invoke(_npc.currentNodeIndex, 0, 0);
    }

    private void OnOpenSelectSkill()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        if (_isSelect == true)
        {
            _onUpdatePopupCallback?.Invoke("이미 선택했습니다.");

            return;
        }

        for (int i = 0; i < _template.Count; i++)
        {
            if (i > _npc.SkillIndexs.Count - 1)
            {
                _template[i].button.gameObject.SetActive(false);

                continue;
            }

            _template[i].AddListener(i, GameManager.instance.dataManager.GetskillData(_npc.SkillIndexs[i]), OnOpenInformation);
        }

        _objSelectSkill.SetActive(true);

        GameManager.instance.tools.Move_Local_XY(eDir.Y, _objSelectSkill.GetComponent<RectTransform>(), -1338f, 0.5f, 0, Ease.OutBack, null);
    }

    private void OnCloseSelectSkill() 
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        CloseInformation();

        GameManager.instance.tools.Move_Local_XY(eDir.Y, _objSelectSkill.GetComponent<RectTransform>(), -2671f, 0.5f, 0, Ease.InBack, () =>
        {
            _objSelectSkill.gameObject.SetActive(false);

            _selectTemplateIndex = -1;
            removeId = 0;

            for (int i = 0; i < _template.Count; i++)
            {
                _template[i].Clear();
            }
        });
    }

    private void OnSelect()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        CloseInformation();

        if (_user.skillDataIndexs.Count == 3)
        {
            _onUpdateTextCallback?.Invoke("--- 남은 공간이 부족합니다.");
            OpenUserSkill(() => 
            {
                var template = _template[_selectTemplateIndex];

                if (_isSelect == true)
                {
                    return;
                }

                _isSelect = true;

                _template[_selectTemplateIndex].Buy();

                OnClose();

                _onResultCallback?.Invoke(_npc.currentNodeIndex, template.skill.id, removeId);
            });
        }
        else
        {
            var template = _template[_selectTemplateIndex];

            if (_isSelect == true)
            {
                return;
            }

            _isSelect = true;

            _template[_selectTemplateIndex].Buy();

            OnClose();

            _onResultCallback?.Invoke(_npc.currentNodeIndex, template.skill.id, removeId);
        }
    }

    private void OpenInformation()
    {
        var template = _template[_selectTemplateIndex];

        _textName.text = template.skill.name;
        _textDescription.text = template.skill.description;
        _textCooldown.text = template.skill.coolDown.ToString();

        _objInformation.SetActive(true);

        GameManager.instance.tools.Move_Local_XY(eDir.X, _objInformation.GetComponent<RectTransform>(), 283f, 0.5f, 0, Ease.OutBack, null);
    }

    private void CloseInformation()
    {
        GameManager.instance.tools.Move_Local_XY(eDir.X, _objInformation.GetComponent<RectTransform>(), 1713f, 0.5f, 0, Ease.InBack, () =>
        {
            _objInformation.SetActive(false);

            _textName.text = string.Empty;
            _textDescription.text = string.Empty;
        });
    }

    private void OpenUserSkill(Action onResultCallback)
    {
        _onOpenUserSkillCallback?.Invoke(_user, (result) => 
        {
            removeId = result;

            onResultCallback?.Invoke();
        });
    }

    private void OnOpenInformation(BonfireTemplate data)
    {
        _selectTemplateIndex = data.index;

        OpenInformation();
    }
}
