using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class Shop : MonoBehaviour
{
    [Serializable]
    public class ShopTemplate
    {
        public Button button = null;
        public Image buttonImage = null;
        public TMP_Text _textButtonLabel = null;

        [HideInInspector] public DataManager.Item_Data item = null;

        [HideInInspector] public int index = 0;
        [HideInInspector] public bool isBuy = false;

        public void AddListener(int i, DataManager.Item_Data data, Action<ShopTemplate> onCallback)
        {
            index = i;
            item = data;

            _textButtonLabel.text = item.name;

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

            item = null;

            isBuy = false;
        }
    }

    [Header("Information")]
    [SerializeField] private GameObject _objInformation = null;
    [SerializeField] private TMP_Text _textName = null;
    [SerializeField] private TMP_Text _textDescription = null;
    [SerializeField] private TMP_Text _textPrice = null;

    [Header("UI")]
    [SerializeField] private Button _buttonClose = null;
    [SerializeField] private Button _buttonBuy = null;
    [SerializeField] private TMP_Text _textCoin = null;

    [Header("Goods"), SerializeField] private List<ShopTemplate> _template = null;
    
    private Action<int, int, short> _onBuyCallback = null;

    private DataManager.Npc_Data _npc = null;
    private int _userCoin = 0;
    private int _selectTemplateIndex = 0;

    public void Initialize(Action<int, int, short> onBuyCallback)
    {
        if (onBuyCallback != null)
        {
            _onBuyCallback = onBuyCallback;
        }

        _buttonClose.onClick.AddListener(OnClose);
        _buttonBuy.onClick.AddListener(OnBuy);

        _objInformation.SetActive(false);
        this.gameObject.SetActive(false);
    }

    public void Open(DataManager.Npc_Data npc, int userCoin)
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ShopEnter);
        GameManager.instance.soundManager.PlaySfx(eSfx.MenuOpen);

        _npc = npc;
        _userCoin = userCoin;

        _textCoin.text = _userCoin.ToString();

        for (int i = 0; i < _template.Count; i++)
        {
            if(i > _npc.itemIndexs.Count - 1)
            {
                _template[i].button.gameObject.SetActive(false);

                continue;
            }

            _template[i].AddListener(i, GameManager.instance.dataManager.GetItemData(_npc.itemIndexs[i]), OnGoodsButton);
        }

        this.gameObject.SetActive(true);

        GameManager.instance.tools.Move_Anchor_XY(eDir.Y, this.GetComponent<RectTransform>(), 350f, 0.5f, 0, Ease.OutBack, null);
    }

    private void OnClose()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);
        GameManager.instance.soundManager.PlaySfx(eSfx.MenuClose);

        CloseInformation();

        GameManager.instance.tools.Move_Anchor_XY(eDir.Y, this.GetComponent<RectTransform>(), -360f, 0.5f, 0, Ease.InBack, () =>
        {
            this.gameObject.SetActive(false);

            _textCoin.text = string.Empty;

            _selectTemplateIndex = -1;

            for (int i = 0; i < _template.Count; i++)
            {
                _template[i].Clear();
            }
        });
    }

    private void OnBuy()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

        CloseInformation();

        var template = _template[_selectTemplateIndex];

        if (template.isBuy == true)
        {
            IngameManager.instance.UpdatePopup("이미 구매하였습니다.");

            return;
        }

        if(_userCoin - template.item.price < 0)
        {
            IngameManager.instance.UpdatePopup("구매할 수 없습니다.");

            return;
        }

        _template[_selectTemplateIndex].Buy();

        _userCoin -= template.item.price;
        _textCoin.text = _userCoin.ToString();

        _onBuyCallback?.Invoke(_npc.currentNodeIndex, template.item.id, template.item.price);
    }

    private void OpenInformation()
    {
        var template = _template[_selectTemplateIndex];

        _textName.text = template.item.name;
        _textDescription.text = template.item.description;
        _textPrice.text = template.item.price.ToString();

        _objInformation.SetActive(true);

        GameManager.instance.tools.Move_Anchor_XY(eDir.X, _objInformation.GetComponent<RectTransform>(), -423f, 0.5f, 0, Ease.OutBack, null);
    }

    private void CloseInformation()
    {
        GameManager.instance.tools.Move_Anchor_XY(eDir.X, _objInformation.GetComponent<RectTransform>(), 447f, 0.5f, 0, Ease.InBack, () =>
        {
            _objInformation.SetActive(false);

            _textName.text = string.Empty;
            _textDescription.text = string.Empty;
        });
    }

    private void OnGoodsButton(ShopTemplate data)
    {
        _selectTemplateIndex = data.index;

        OpenInformation();
    }
}
