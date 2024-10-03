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

    [SerializeField] private TMP_Text _textPlayerNameLabel = null;
    [SerializeField] private TMP_Text _textPlayerCard = null;
    [SerializeField] private TMP_Text _textCommunityCard = null;

    [Space(10)]

    [SerializeField] private GameObject _objButtons = null;
    [SerializeField] private Button _buttonBat = null;
    [SerializeField] private Button _buttonRaise = null;
    [SerializeField] private Button _buttonAllin = null;
    [SerializeField] private Button _buttonFold = null;

    private Action _onCloseCallback = null;
    private Action _onLastCallback = null;
    private Action<eWinorLose, int> _onResultCallback = null;
    private Action<string> _onUpdateTextCallback = null;

    private List<AttackerTemplate> _cards = null;
    private List<int> _fieldCardIndex = new List<int>();
    private DataManager.User_Data _userData = null;
    private DataManager.Creature_Data _monster = null;

    private int _playerCoinCount = 0;

    private int _monsterCoinCount = 0;
    private int _batCount = 0;
    private int _totalCount = 0;

    private int _turnCount = 0;

    private eWinorLose _isPlayerWin = eWinorLose.Non;
    private int _resultDamage = 0;

    private eBattleAction _playerBattleAction = eBattleAction.Non;
    private eBattleAction _monsterBattleAction = eBattleAction.Non;

    public void Initialize(Action onCloseCallback, Action<string> onUpdateTextCallback)
    {
        if(onCloseCallback != null)
        {
            _onCloseCallback = onCloseCallback;
        }

        if(onUpdateTextCallback != null)
        {
            _onUpdateTextCallback = onUpdateTextCallback;
        }

        _popup.Initialize();
        _template.Initialize();

        _textPlayerNameLabel.text = string.Empty;
        _textPlayerCard.text = string.Empty;
        _textCommunityCard.text = string.Empty;

        _buttonBat.onClick.AddListener(() => { OnBat(true, ref _playerCoinCount, ref _batCount); });
        _buttonRaise.onClick.AddListener(() => { OnRaise(true, ref _playerCoinCount, ref _batCount); });
        _buttonAllin.onClick.AddListener(() => { OnAllin(true, ref _playerCoinCount, ref _batCount); });
        _buttonFold.onClick.AddListener(() => { OnFold(true, ref _playerCoinCount, ref _batCount); });

        MakeCard();

        this.gameObject.SetActive(false);
    }

    public void CallAttacker(DataManager.User_Data userData, DataManager.Creature_Data monster, Action onLastCallback, Action<eWinorLose, int> onResultCallback)
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
        _monsterCoinCount = monster.hp;

        _batCount = 1;

        _textPlayerNameLabel.text = userData.data.name;

        _textCoin.text = _playerCoinCount.ToString();
        _textBat.text = _batCount.ToString();

        _onUpdateTextCallback?.Invoke(_monster.name + " 과 전투를 시작합니다!");
        _onUpdateTextCallback?.Invoke("--- " + (_turnCount + 1) + "번째 턴");

        CardDistribution();

        _objButtons.SetActive(true);
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

        string str_name = isPlayer == true ? _userData.data.name : _monster.name;
        _onUpdateTextCallback?.Invoke(str_name + " (이)가 " + bat + " 만큼 Call 했습니다.");

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

        _textBat.text = bat.ToString();
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

        string str_name = isPlayer == true ? _userData.data.name : _monster.name;
        _onUpdateTextCallback?.Invoke(str_name + " (이)가 Raise를 했습니다.");

        bat *= 2;
        coin -= bat;
        _totalCount += bat;

        if(isPlayer == true)
        {
            _playerBattleAction = eBattleAction.Raise;

            _textCoin.text = coin.ToString();
        }
        else
        {
            _monsterBattleAction = eBattleAction.Raise;
        }

        _textBat.text = bat.ToString();
        _textTotal.text = _totalCount.ToString();

        Check(isPlayer);
    }

    private void OnAllin(bool isPlayer, ref int coin, ref int bat)
    {
        string str_name = isPlayer == true ? _userData.data.name : _monster.name;
        _onUpdateTextCallback?.Invoke(str_name + "   ALL IN");
        
        _totalCount += coin;
        bat = coin;
        coin = 0;

        if (isPlayer == true)
        {
            _playerBattleAction = eBattleAction.Allin;

            _textCoin.text = coin.ToString();
        }
        else
        {
            _monsterBattleAction = eBattleAction.Allin;
        }

        _textBat.text = bat.ToString();
        _textTotal.text = _totalCount.ToString();

        Check(isPlayer);
    }

    private void OnFold(bool isPlayer, ref int coin, ref int bat)
    {
        string str_name = isPlayer == true ? _userData.data.name : _monster.name;
        _onUpdateTextCallback?.Invoke(str_name + " (이)가 Fold 했습니다.");

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
                _isPlayerWin = eWinorLose.Lose;

                SettleUp(false);

                return;
            }

            StartCoroutine(Co_EnemyTurn());

            return;
        }

        if(_turnCount < 4 && _playerCoinCount == 0)
        {
            _isPlayerWin = eWinorLose.Lose;

            SettleUp(false);

            return;
        }

        if(_monsterBattleAction == eBattleAction.Fold)
        {
            _isPlayerWin = eWinorLose.Win;

            SettleUp(false);

            return;
        }

        ButtonControl();
        CardOpen();
    }

    IEnumerator Co_EnemyTurn()
    {
        // logic

        yield return new WaitForSecondsRealtime(0.7f);

        OnBat(false, ref _monsterCoinCount, ref _batCount);
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

        if (_playerCoinCount < _batCount)
        {
            _buttonBat.gameObject.SetActive(false);
            _buttonRaise.gameObject.SetActive(false);
        }

        if (_playerCoinCount < (_batCount * 2))
        {
            _buttonRaise.gameObject.SetActive(false);
        }

        _objButtons.SetActive(true);
    }

    private void CardOpen()
    {
        _turnCount++;
        _onUpdateTextCallback?.Invoke("--- " + (_turnCount + 1) + "번째 턴");

        string str = _textCommunityCard.text;

        if (_turnCount == 1)
        {
            string s1 = _cards[_fieldCardIndex[2]].Name;
            s1 += _cards[_fieldCardIndex[3]].Name + " ";
            s1 += _cards[_fieldCardIndex[4]].Name + " ";

            str = s1 + "** **";
        }

        if (_turnCount == 2)
        {
            string s1 = _cards[_fieldCardIndex[2]].Name;
            s1 += _cards[_fieldCardIndex[3]].Name + " ";
            s1 += _cards[_fieldCardIndex[4]].Name + " ";
            s1 += _cards[_fieldCardIndex[5]].Name + " ";

            str = s1 + "**";
        }

        if (_turnCount == 3)
        {
            string s1 = _cards[_fieldCardIndex[2]].Name;
            s1 += _cards[_fieldCardIndex[3]].Name + " ";
            s1 += _cards[_fieldCardIndex[4]].Name + " ";
            s1 += _cards[_fieldCardIndex[5]].Name + " ";
            s1 += _cards[_fieldCardIndex[6]].Name;

            str = s1;
        }

        _textCommunityCard.text = str;

        if (_turnCount == 4)
        {
            SettleUp(true);
        }
    }

    private void SettleUp(bool isDone)
    {
        _objButtons.SetActive(false);

        if (isDone == false)
        {
            Done(_isPlayerWin);

            return;
        }

        StartCoroutine(Co_SettleUp());
    }

    IEnumerator Co_SettleUp()
    {
        yield return new WaitForSecondsRealtime(1f);

        List<int> nums = new List<int>(6);

        for (int i = 0; i <= 6; i++)
        {
            nums.Add(_fieldCardIndex[i]);
        }

        ePedigree playerPedigree = Pedigree(Sort(nums));

        nums.Clear();

        for (int i = 2; i < _fieldCardIndex.Count; i++)
        {
            nums.Add(_fieldCardIndex[i]);
        }

        ePedigree monsterPedigree = Pedigree(Sort(nums));

        _onUpdateTextCallback?.Invoke(_userData.data.name + " 의 결과 : " + playerPedigree);
        _onUpdateTextCallback?.Invoke(_monster.name + " 의 결과 : " + monsterPedigree);

        if (playerPedigree > monsterPedigree)
        {
            _isPlayerWin = eWinorLose.Win;
        }
        else if(playerPedigree < monsterPedigree)
        {
            _isPlayerWin = eWinorLose.Lose;
        }
        else if(playerPedigree == monsterPedigree)
        {
            _isPlayerWin = eWinorLose.Draw;
        }

        OnClose();
    }

    private void Done(eWinorLose isPlayerWin)
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
                com.Set(eCardShape.Clob, "♣", (i + 1) - 39);
            }

            _cards.Add(com);
        }
    }

    private void CardDistribution()
    {
        int voitedCount = 9;
        _fieldCardIndex = new List<int>();

        while (_fieldCardIndex.Count < 9)
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
                //_cards[index].ChangePositionAndActive(_trMyParant, true);
                _textPlayerCard.text += _cards[index].Name + " ";

                continue;
            }

            if(6 < i)
            {
                //_cards[index].ChangePositionAndActive(_trEnemyParant, true);
                //_cards[index].HideAndSeek(true);

                continue;
            }

            //_cards[index].ChangePositionAndActive(_trCommunityParant, true);
            //_cards[index].HideAndSeek(true);

            _textCommunityCard.text += "** ";
        }
    }

    private List<AttackerTemplate> Sort(List<int> indexs)
    {
        List<AttackerTemplate> card = new List<AttackerTemplate>();

        AttackerTemplate temp = new AttackerTemplate();

        for (int i = 0; i < indexs.Count; i++)
        {
            card.Add(_cards[indexs[i]]);
        }

        for (int i = card.Count - 1; i > 0; i--)
        {
            for (int j = 0; j < i; j++)
            {
                if (card[j].Num < card[j + 1].Num)
                {
                    temp = card[j];
                    card[j] = card[j + 1];
                    card[j + 1] = temp;
                }
            }
        }

        return card;
    }

    private ePedigree Pedigree(List<AttackerTemplate> card)
    {
        ePedigree result = ePedigree.HighCard;

        List<ePedigree> results = new List<ePedigree>();

        bool royal = false;
        bool twoPair = false;
        bool threeofaKind = false;
        bool foreofaKind = false;
        bool flush = false;
        bool straight = false;
        bool fullHouse = false;

        bool isChange = false;

        int pairCount = 0;
        int straightCount = 0;
        int numSameCount = 0;

        int spadeCount = 0;
        int heartCount = 0;
        int diamondCount = 0;
        int clobCount = 0;

        for (int i = 0; i < (card.Count - 1); i++)
        {
            if (card[i].Num != card[i + 1].Num)
            {
                isChange = true;
                numSameCount = 0;

                if ((card[i].Num - card[i + 1].Num) == 1)
                {
                    straightCount++;

                    if (straightCount >= 5)
                    {
                        straight = true;
                    }

                    if (card[i].Num == 14)
                    {
                        royal = true;
                    }
                }
                else
                {
                    royal = false;
                }
            }

            if (card[i].Num == card[i + 1].Num)
            {
                numSameCount++;

                if(numSameCount == 1)
                {
                    pairCount++;
                }

                if(isChange == true && pairCount == 2)
                {
                    twoPair = true;
                }

                if(numSameCount == 2)
                {
                    threeofaKind = true;
                }

                if(numSameCount == 3)
                {
                    foreofaKind = true;
                }

                if (threeofaKind == true && isChange == true && pairCount >= 1)
                {
                    fullHouse = true;
                }

                isChange = false;
            }
        }

        for (int i = 0; i < card.Count; i++)
        {
            if (card[i].Shape == eCardShape.Spade)
                spadeCount++;

            if (card[i].Shape == eCardShape.Heart)
                heartCount++;

            if (card[i].Shape == eCardShape.Diamond)
                diamondCount++;

            if (card[i].Shape == eCardShape.Clob)
                clobCount++;
        }

        if(spadeCount >= 5 || heartCount >= 5 || diamondCount >= 5 || clobCount >= 5)
        {
            flush = true;
        }

        if (royal == true) // RoyalStraightFlush
        {
            result = ePedigree.RoyalStraightFlush;
        }
        else if (straight == true && flush == true) // StraightFlush
        {
            result = ePedigree.StraightFlush;
        }
        else if (foreofaKind == true) // FourofaKind
        {
            result = ePedigree.FourofaKind;
        }
        else if (fullHouse == true) // FullHouse
        {
            result = ePedigree.FullHouse;
        }
        else if (flush == true) // Flush
        {
            result = ePedigree.Flush;
        }
        else if (straight == true) // Straight
        {
            result = ePedigree.Straight;
        }
        else if (threeofaKind == true) // ThreeofaKind
        {
            result = ePedigree.ThreeofaKind;
        }
        else if (twoPair == true) // TwoPair
        {
            result = ePedigree.TwoPair;
        }
        else if (pairCount == 1) // OnePair
        {
            result = ePedigree.OnePair;
        }
        else // HighCard
        {
            result = ePedigree.HighCard;
        }

        return result;
    }
}
