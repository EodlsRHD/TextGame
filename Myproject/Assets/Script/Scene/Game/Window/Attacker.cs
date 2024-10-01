using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class Attacker : MonoBehaviour
{
    [SerializeField] private IngamePopup _popup = null;

    [Space(10)]

    [SerializeField]private AttackerTemplate _template = null;
    [SerializeField] private Transform _trTemplateParant = null;

    [SerializeField] private Transform _trMyParant = null;
    [SerializeField] private Transform _trEnemyParant = null;
    [SerializeField] private Transform _trCommunityParant = null;

    [Space(10)]

    [SerializeField] private TMP_Text _textCoin = null;
    [SerializeField] private TMP_Text _textBat = null;
    [SerializeField] private TMP_Text _textTotal = null;

    [Space(10)]

    [SerializeField] private GameObject _objButtons = null;
    [SerializeField] private Button _buttonBat = null;
    [SerializeField] private Button _buttonRaise = null;
    [SerializeField] private Button _buttonAllin = null;
    [SerializeField] private Button _buttonFold = null;

    private Action _onCloseCallback = null;
    private Action _onLastCallback = null;
    private Action<bool, int> _onResultCallback = null;

    private List<AttackerTemplate> _cards = null;
    private List<int> _fieldCardIndex = new List<int>();
    private DataManager.User_Data _userData = null;
    private DataManager.Creature_Data _monster = null;

    private int _playerCoinCount = 0;
    private int _playerBatCount = 0;

    private int _monsterCoinCount = 0;
    private int _monsterBatCount = 0;

    private int _totalCount = 0;

    private int _turnCount = 0;

    private bool _isPlayerWin = false;
    private int _resultDamage = 0;

    private eBattleAction _playerBattleAction = eBattleAction.Non;
    private eBattleAction _monsterBattleAction = eBattleAction.Non;

    public void Initialize(Action onCloseCallback)
    {
        if(onCloseCallback != null)
        {
            _onCloseCallback = onCloseCallback;
        }

        _popup.Initialize();
        _template.Initialize();

        _buttonBat.onClick.AddListener(() => { OnBat(true, ref _playerCoinCount, ref _playerBatCount); });
        _buttonRaise.onClick.AddListener(() => { OnRaise(true, ref _playerCoinCount, ref _playerBatCount); });
        _buttonAllin.onClick.AddListener(() => { OnAllin(true, ref _playerCoinCount, ref _playerBatCount); });
        _buttonFold.onClick.AddListener(() => { OnFold(true, ref _playerCoinCount, ref _playerBatCount); });

        MakeCard();

        this.gameObject.SetActive(false);
    }

    public void CallAttacker(DataManager.User_Data userData, DataManager.Creature_Data monster, Action onLastCallback, Action<bool, int> onResultCallback)
    {
        if(onLastCallback != null)
        {
            _onLastCallback = onLastCallback;
        }

        if(onResultCallback != null)
        {
            _onResultCallback = onResultCallback;
        }

        _userData = userData;
        _monster = monster;

        _playerCoinCount = userData.currentHP;
        _playerBatCount = 1;

        _monsterCoinCount = monster.hp;
        _monsterBatCount = 1;

        _textCoin.text = _playerCoinCount.ToString();
        _textBat.text = _playerBatCount.ToString();

        CardDistribution();

        this.gameObject.SetActive(true);
    }

    public void Close()
    {
        this.gameObject.SetActive(false);

        _turnCount = 0;
        _playerBattleAction = eBattleAction.Non;

        _fieldCardIndex = new List<int>();

        _onResultCallback?.Invoke(_isPlayerWin, _resultDamage);
        _onLastCallback?.Invoke();

        _buttonBat.gameObject.SetActive(true);
        _buttonRaise.gameObject.SetActive(true);
        _buttonAllin.gameObject.SetActive(true);
        _buttonFold.gameObject.SetActive(true);

        ReturnTemplate();
    }

    private void OnClose()
    {
        _onCloseCallback?.Invoke();
    }

    private void OnBat(bool isPlayer, ref int coin, ref int bat)
    {
        if(isPlayer == true)
        {
            if (coin < bat)
            {
                _popup.UpdateText("남은 체력이 부족합니다.");

                return;
            }
        }

        _popup.UpdateText(isPlayer == true ? _userData.data.name : _monster.name + " (이)가 " + bat + " 만큼 배팅 하셨습니다.");

        coin -= bat;
        _totalCount += bat;

        if(isPlayer == true)
        {
            _playerBattleAction = eBattleAction.Bat;

            _textCoin.text = coin.ToString();
        }
        else
        {
            _monsterBattleAction = eBattleAction.Bat;
        }

        _textTotal.text = _totalCount.ToString();

        Check(isPlayer);
    }

    private void OnRaise(bool isPlayer, ref int coin, ref int bat)
    {
        if (isPlayer == true)
        {
            if (coin < (bat * 2))
            {
                _popup.UpdateText("남은 체력이 부족합니다.");

                return;
            }
        }

        _popup.UpdateText(isPlayer == true ? _userData.data.name : _monster.name + " (이)가 베팅액을 2배로 올려 " + (coin * 2) + " 만큼 배팅하셨습니다.");

        bat *= 2;
        coin -= bat;
        _totalCount += bat;

        if(isPlayer == true)
        {
            _playerBattleAction = eBattleAction.Raise;

            _textCoin.text = coin.ToString();
            _textBat.text = bat.ToString();
        }
        else
        {
            _monsterBattleAction = eBattleAction.Raise;
        }

        _textTotal.text = _totalCount.ToString();

        Check(isPlayer);
    }

    private void OnAllin(bool isPlayer, ref int coin, ref int bat)
    {
        _popup.UpdateText(isPlayer == true ? _userData.data.name : _monster.name + "   ALL IN");

        coin = 0;
        _totalCount += coin;

        if(isPlayer == true)
        {
            _playerBattleAction = eBattleAction.Allin;

            _textCoin.text = coin.ToString();
        }
        else
        {
            _monsterBattleAction = eBattleAction.Allin;
        }

        _textTotal.text = _totalCount.ToString();

        Check(isPlayer);
    }

    private void OnFold(bool isPlayer, ref int coin, ref int bat)
    {
        _popup.UpdateText(isPlayer == true ? _userData.data.name : _monster.name + " (이)가 Fold 했습니다.");

        if (isPlayer == true)
        {
            _playerBattleAction = eBattleAction.Fold;
        }
        else
        {
            _monsterBattleAction = eBattleAction.Fold;
        }

        Check(isPlayer);
    }

    private void Check(bool isPlayer)
    {
        if(isPlayer == true)
        {
            _objButtons.SetActive(false);

            if (_playerBattleAction == eBattleAction.Fold)
            {
                _isPlayerWin = false;

                SettleUp(false);
            }

            EnemyTurn();

            return;
        }

        if(_monsterBattleAction == eBattleAction.Fold)
        {
            _isPlayerWin = true;

            SettleUp(false);
        }

        ButtonControl();
        CardOpen();
    }

    private void EnemyTurn()
    {
        // logic

        OnBat(false, ref _monsterCoinCount, ref _monsterBatCount);
    }

    private void ButtonControl()
    {
        _buttonBat.gameObject.SetActive(true);
        _buttonRaise.gameObject.SetActive(true);
        _buttonAllin.gameObject.SetActive(true);
        _buttonFold.gameObject.SetActive(true);

        if (_playerCoinCount == 0)
        {
            _buttonBat.gameObject.SetActive(false);
            _buttonRaise.gameObject.SetActive(false);
            _buttonAllin.gameObject.SetActive(false);

            return;
        }

        if (_playerCoinCount < _playerBatCount)
        {
            _buttonBat.gameObject.SetActive(false);
            _buttonRaise.gameObject.SetActive(false);
        }

        if (_playerCoinCount < (_playerBatCount * 2))
        {
            _buttonRaise.gameObject.SetActive(false);
        }

        _objButtons.SetActive(true);
    }

    private void CardOpen()
    {
        _turnCount++;

        if (_turnCount == 1)
        {
            _cards[_fieldCardIndex[2]].HideAndSeek(false);
            _cards[_fieldCardIndex[3]].HideAndSeek(false);
            _cards[_fieldCardIndex[4]].HideAndSeek(false);
        }

        if (_turnCount == 2)
        {
            _cards[_fieldCardIndex[5]].HideAndSeek(false);
        }

        if (_turnCount == 3)
        {
            _cards[_fieldCardIndex[6]].HideAndSeek(false);
        }

        if (_turnCount == 4)
        {
            SettleUp(true);
        }
    }

    private void SettleUp(bool isDone)
    {
        if(isDone == false)
        {
            Done(_isPlayerWin);

            return;
        }

        // 정산

        // 로열 스트레이트 플러시 - 무늬가 같고 A, K, Q, J, 10 로 구성된 카드
        // 스트레이트 플러시 - 무늬가 같고, 숫자가 연결된 카드 (순서대로)
        // 포카드 - 4장의 같은 카드
        // 풀하우스 - 3장의 같은 카드, 2장의 같은 카드
        // 플러시 - 5장이 무늬만 같은 카드
        // 스트레이트 - 5장의 숫자가 연결된 카드 (순서대로)
        // 트리플 - 3장의 숫자가 연결된 카드 (순서대로)
        // 투페어 - 2장의 수가 같고, 총 2세트 있는 카드
        // 원페어 - 2장의 수가 같고, 총 1세트 있는 카드
        // 하이카드(노페어) - 5장이 문양, 숫자가 다 다른 카드 / 둘다 하이카드일 경우 가장 높은 숫자를 가지고 있는 플레이어가 승리

        //https://blog.naver.com/josoblue/220817060229

        Done(_isPlayerWin);
    }

    private void Done(bool isPlayerWin)
    {
        _isPlayerWin = isPlayerWin;
        _resultDamage = _totalCount;

        OnClose();
    }

    public void ReturnTemplate()
    {
        foreach (var index in _fieldCardIndex)
        {
            _cards[index].ChangePositionAndActive(_trTemplateParant, false);
        }
    }

    private void MakeCard()
    {
        _cards = new List<AttackerTemplate>();

        for (int i = 0; i < 52; i++)
        {
            var obj = Instantiate(_template, _trTemplateParant);
            var com = obj.GetComponent<AttackerTemplate>();

            if(i < 13)
            {
                com.Set(eCardShape.Spade, "♠", (i + 1));
            }

            if(13 <= i && i < 26)
            {
                com.Set(eCardShape.Diamond, "◆", (i + 1) - 13);
            }

            if (26 <= i && i < 39)
            {
                com.Set(eCardShape.Heart, "♥", (i + 1) - 26);
            }

            if (39 <= i)
            {
                com.Set(eCardShape.Clover, "♣", (i + 1) - 39);
            }

            _cards.Add(com);
        }
    }

    private void CardDistribution()
    {
        int voitedCount = 9;
        _fieldCardIndex = new List<int>();

        while(_fieldCardIndex.Count < 9)
        {
            int ranNum = UnityEngine.Random.Range(0, _cards.Count);
            bool isVoited = false;

            foreach (var index in _fieldCardIndex)
            {
                if (index == ranNum)
                {
                    isVoited = true;

                    break;
                }
            }

            if (isVoited == true)
            {
                continue;
            }

            _fieldCardIndex.Add(ranNum);
            voitedCount--;
        }

        for (int i = 0; i < _fieldCardIndex.Count; i++)
        {
            int index = _fieldCardIndex[i];

            if(i < 2)
            {
                _cards[index].ChangePositionAndActive(_trMyParant, true);

                continue;
            }

            if(6 < i)
            {
                _cards[index].ChangePositionAndActive(_trEnemyParant, true);
                _cards[index].HideAndSeek(true);

                continue;
            }

            _cards[index].ChangePositionAndActive(_trCommunityParant, true);
            _cards[index].HideAndSeek(true);
        }
    }
}
