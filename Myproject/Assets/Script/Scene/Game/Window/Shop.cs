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

            button?.onClick.AddListener(() => { onCallback(this); });
        }

        public void Buy()
        {
            buttonImage.color = Color.black;
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
    
    private Action<string> _onUpdateTextCallback = null;
    private Action<string> _onUpdatePopupCallback = null;
    private Action<int, ushort> _onBuyCallback = null;

    private DataManager.Npc_Data _npc = null;
    private int _userCoin = 0;
    private int _selectTemplateIndex = 0;

    public void Initialize(Action<string> onUpdateTextCallback, Action<string> onUpdatePopupCallback, Action<int, ushort> onBuyCallback)
    {
        if(onUpdateTextCallback != null)
        {
            _onUpdateTextCallback = onUpdateTextCallback;
        }

        if(onUpdatePopupCallback != null)
        {
            _onUpdatePopupCallback = onUpdatePopupCallback;
        }

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
    }

    private void OnClose()
    {
        this.gameObject.SetActive(false);
        _objInformation.SetActive(false);

        _textName.text = string.Empty;
        _textDescription.text = string.Empty;

        _textCoin.text = string.Empty;

        _selectTemplateIndex = -1;

        for (int i = 0; i < _template.Count; i++)
        {
            _template[i].Clear();
        }
    }

    private void OnBuy()
    {
        _objInformation.SetActive(false);

        var template = _template[_selectTemplateIndex];

        if (template.isBuy == true)
        {
            _onUpdatePopupCallback?.Invoke("이미 구매하였습니다.");

            return;
        }

        if(_userCoin - template.item.price < 0)
        {
            _onUpdatePopupCallback?.Invoke("구매할 수 없습니다.");

            return;
        }

        _template[_selectTemplateIndex].Buy();

        _userCoin -= template.item.price;
        _textCoin.text = _userCoin.ToString();

        _onBuyCallback?.Invoke(template.item.id, template.item.price);
    }

    private void OpenInformation()
    {
        var template = _template[_selectTemplateIndex];

        _textName.text = template.item.name;
        _textDescription.text = template.item.description;
        _textPrice.text = template.item.price.ToString();

        _objInformation.SetActive(true);
    }

    private void OnGoodsButton(ShopTemplate data)
    {
        _selectTemplateIndex = data.index;

        OpenInformation();
    }
}
