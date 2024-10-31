using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class Attacker : MonoBehaviour
{
    public class Card
    {
        public int num = 0;
        public eCardShape shape = eCardShape.Non;

        public bool isHide = true;
        public string name = string.Empty;

        public void Set(eCardShape cardShape, int cardNum)
        {
            num = cardNum;
            shape = cardShape;

            if(shape == eCardShape.Clob)
            {
                name = "♣️";
            }
            else if(shape == eCardShape.Spade)
            {
                name = "♠";
            }
            else if(shape == eCardShape.Heart)
            {
                name = "♥";
            }
            else if(shape == eCardShape.Diamond)
            {
                name = "♦️";
            }

            if(num == 1)
            {
                num = 14;
                name += "A";
            }
            else if(num == 11)
            {
                name += "J";
            }
            else if(num == 12)
            {
                name += "Q";
            }
            else if(num == 13)
            {
                name += "K";
            }
            else
            {
                name += num;
            }
        }

        public void IsHide(bool hide)
        {
            isHide = hide;
        }
    }

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

    [Space(10)]
    [SerializeField] private GameObject _objViewRanking = null;
    [SerializeField] private TMP_Text _textViewRanking = null;

    private Action _onCloseCallback = null;
    private Action _onLastCallback = null;
    private Action<eWinorLose, int> _onResultCallback = null;

    private List<Card> _cards = null;
    private List<int> _fieldCardIndex = new List<int>();
    private UserData _userData = null;
    private CreatureData _monster = null;

    private int _playerCoinCount = 0;

    private int _monsterCoinCount = 0;
    private int _batCount = 0;
    private int _totalCount = 0;

    private int _playerBat = 0;

    private int _turnCount = 0;

    private eWinorLose _isPlayerWin = eWinorLose.Non;
    private int _resultDamage = 0;

    private eBattleAction _playerBattleAction = eBattleAction.Non;
    private eBattleAction _monsterBattleAction = eBattleAction.Non;

    public void Initialize(Action onCloseCallback)
    {
        if(onCloseCallback != null)
        {
            _onCloseCallback = onCloseCallback;
        }

        _textPlayerNameLabel.text = string.Empty;
        _textPlayerCard.text = string.Empty;
        _textCommunityCard.text = string.Empty;

        _buttonBat.onClick.AddListener(() => { OnBat(true, ref _playerCoinCount, ref _batCount); });
        _buttonRaise.onClick.AddListener(() => { OnRaise(true, ref _playerCoinCount, ref _batCount); });
        _buttonAllin.onClick.AddListener(() => { OnAllin(true, ref _playerCoinCount, ref _batCount); });
        _buttonFold.onClick.AddListener(() => { OnFold(true, ref _playerCoinCount, ref _batCount); });

        MakeCard();

        _objViewRanking.SetActive(false);
        this.gameObject.SetActive(false);
    }

    public void CallAttacker(UserData userData, CreatureData monster, Action onLastCallback, Action<eWinorLose, int> onResultCallback)
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.MenuOpen);
        PlayBgm(eBgm.Battle);

        ViewRanking();

        if (onLastCallback != null)
        {
            _onLastCallback = onLastCallback;
        }

        if(onResultCallback != null)
        {
            _onResultCallback = onResultCallback;
        }

        _userData = userData;
        _monster = monster;

        _playerCoinCount = userData.stats.hp.current;
        _monsterCoinCount = monster.stats.hp.current;

        _batCount = userData.level;
        _totalCount += _batCount * 2;
        _playerBat += _batCount;

        _textPlayerNameLabel.text = userData.data.name;

        _textCoin.text = _playerCoinCount.ToString();
        _textBat.text = _batCount.ToString();
        _textTotal.text = _totalCount.ToString();

        IngameManager.instance.UpdateText(_monster.name + " (와)과 전투를 시작합니다!");
        IngameManager.instance.UpdateText((_turnCount + 1) + "번째 턴");

        CardDistribution();

        _objButtons.SetActive(true);
        this.gameObject.SetActive(true);

        GameManager.instance.tools.Move_Anchor_XY(eUiDir.Y, this.GetComponent<RectTransform>(), 350f, 0.5f, 0, Ease.OutBack, null);

        if(IngameManager.instance.CheckAbnormalStatusEffect(eStrengtheningTool.UnableAct, monster) == true)
        {
            IngameManager.instance.RemoveAbnormalStatusEffect(eStrengtheningTool.UnableAct, ref monster);
            IngameManager.instance.UpdateText("상대가 행동불능 상태로 인해 전투를 포기했습니다.");

            OnFold(false, ref _playerCoinCount, ref _batCount);

            return;
        }

        if(IngameManager.instance.CheckAbnormalStatusEffect(eStrengtheningTool.UnableAct, IngameManager.instance.saveData.userData.data) == true)
        {
            IngameManager.instance.RemoveAbnormalStatusEffect(eStrengtheningTool.UnableAct, ref IngameManager.instance.saveData.userData.data);
            IngameManager.instance.UpdateText("행동불능 상태로 인해 전투를 포기합니다.");

            OnFold(true, ref _playerCoinCount, ref _batCount);
        }
    }

    public void Close()
    {
        this.gameObject.SetActive(false);

        _textPlayerNameLabel.text = string.Empty;
        _textPlayerCard.text = string.Empty;
        _textCommunityCard.text = string.Empty;
        _textTotal.text = string.Empty;
        _textViewRanking.text = string.Empty;

        _playerBat = 0;
        _turnCount = 0;
        _totalCount = 0;
        _playerBattleAction = eBattleAction.Non;
        _monsterBattleAction = eBattleAction.Non;

        _fieldCardIndex = new List<int>();

        _onResultCallback?.Invoke(_isPlayerWin, _resultDamage);
        _onLastCallback?.Invoke();

        _buttonBat.gameObject.SetActive(true);
        _buttonRaise.gameObject.SetActive(true);
        _buttonAllin.gameObject.SetActive(true);
        _buttonFold.gameObject.SetActive(true);
    }

    public void ViewRanking()
    {
        _objViewRanking.SetActive(GameManager.instance.isViewRanking);

        if(GameManager.instance.isViewRanking == false)
        {
            return;
        }

        PredictionViewRanking();
    }

    private void OnClose()
    {
        GameManager.instance.soundManager.PlaySfx(eSfx.MenuClose);

        GameManager.instance.tools.Move_Anchor_XY(eUiDir.Y, this.GetComponent<RectTransform>(), -600f, 0.5f, 0, Ease.InBack, () => 
        {
            _onCloseCallback?.Invoke();
        });
    }

    private void OnBat(bool isPlayer, ref int coin, ref int bat)
    {
        if (isPlayer == true)
        {
            GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

            if (coin < bat)
            {
                IngameManager.instance.UpdatePopup("남은 체력이 부족합니다.");

                return;
            }
        }

        string str_name = isPlayer == true ? _userData.data.name : _monster.name;
        IngameManager.instance.UpdateText(str_name + " (이)가 " + bat + " 만큼 Call 했습니다.");

        coin -= bat;
        _totalCount += bat;

        if(isPlayer == true)
        {
            _playerBat += bat;
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
            GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);

            if (coin < (bat * 2))
            {
                IngameManager.instance.UpdatePopup("남은 체력이 부족합니다.");

                return;
            }
        }

        string str_name = isPlayer == true ? _userData.data.name : _monster.name;
        IngameManager.instance.UpdateText(str_name + " (이)가 Raise를 했습니다.");

        bat *= 2;
        coin -= bat;
        _totalCount += bat;

        if(isPlayer == true)
        {
            _playerBat += bat;
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
        if(isPlayer == true)
        {
            GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);
        }

        string str_name = isPlayer == true ? _userData.data.name : _monster.name;
        IngameManager.instance.UpdateText(str_name + "   ALL IN");
        
        _totalCount += coin;
        bat = coin;
        coin = 0;

        if (isPlayer == true)
        {
            _playerBat += bat;
            _playerBattleAction = eBattleAction.ALLin;

            _textCoin.text = coin.ToString();
        }
        else
        {
            _monsterBattleAction = eBattleAction.ALLin;
        }

        _textBat.text = bat.ToString();
        _textTotal.text = _totalCount.ToString();

        Check(isPlayer);
    }

    private void OnFold(bool isPlayer, ref int coin, ref int bat)
    {
        if (isPlayer == true)
        {
            GameManager.instance.soundManager.PlaySfx(eSfx.ButtonPress);
        }

        string str_name = isPlayer == true ? _userData.data.name : _monster.name;
        IngameManager.instance.UpdateText(str_name + " (이)가 Fold 했습니다.");

        if (isPlayer == true)
        {
            _playerBattleAction = eBattleAction.Fold;
            _isPlayerWin = eWinorLose.Lose;
        }
        else
        {
            _monsterBattleAction = eBattleAction.Fold;
            _isPlayerWin = eWinorLose.Win;
        }

        _turnCount = 4;
        _resultDamage = _totalCount - _playerBat;

        ButtonControl();
        Done(_isPlayerWin);
    }

    private void Check(bool isPlayer)
    {
        if (isPlayer == true)
        {
            _objButtons.SetActive(false);

            if (_playerBattleAction == eBattleAction.Fold)
            {
                _isPlayerWin = eWinorLose.Lose;

                SettleUp();

                return;
            }

            StartCoroutine(Co_EnemyTurn());

            return;
        }

        if (_monsterBattleAction == eBattleAction.Fold)
        {
            _isPlayerWin = eWinorLose.Win;

            SettleUp();

            return;
        }

        _turnCount++;

        if (_turnCount < 4)
        {
            IngameManager.instance.UpdateText((_turnCount + 1) + "번째 턴");
        }

        CardOpen();
        ButtonControl();

        if (_turnCount < 4 && _playerCoinCount == 0)
        {
            IngameManager.instance.UpdateText("남은 Hp가 없어 Call할 수 없습니다.");

            StartCoroutine(Co_EnemyTurn());
        }
    }

    IEnumerator Co_EnemyTurn()
    {
        yield return new WaitForSecondsRealtime(0.7f);

        List<int> nums = new List<int>(6);

        for (int i = 2; i < _fieldCardIndex.Count; i++)
        {
            if(_cards[_fieldCardIndex[i]].isHide == true)
            {
                continue;
            }

            nums.Add(_fieldCardIndex[i]);
        }

        int monsterHighCardNum = 0;
        eRankings monsterPedigree = Rankings(Sort(nums), ref monsterHighCardNum);

        if(monsterPedigree == eRankings.Non)
        {
            OnBat(false, ref _monsterCoinCount, ref _batCount);

            yield break;
        }

        if ((int)monsterPedigree < 2)
        {
            OnBat(false, ref _monsterCoinCount, ref _batCount);
        }

        if(2 <= (int)monsterPedigree && (int)monsterPedigree < 4)
        {
            bool result = Probability(65);

            if (result == true)
            {
                OnBat(false, ref _monsterCoinCount, ref _batCount);
            }
            else
            {
                OnRaise(false, ref _monsterCoinCount, ref _batCount);
            }
        }

        if (4 <= (int)monsterPedigree && (int)monsterPedigree < 8)
        {
            bool result = Probability(70);

            if (result == true)
            {
                OnBat(false, ref _monsterCoinCount, ref _batCount);
            }
            else
            {
                OnRaise(false, ref _monsterCoinCount, ref _batCount);
            }
        }

        if ((int)monsterPedigree == 8)
        {
            bool result = Probability(80);

            if (result == true)
            {
                OnBat(false, ref _monsterCoinCount, ref _batCount);
            }
            else
            {
                OnRaise(false, ref _monsterCoinCount, ref _batCount);
            }
        }

        if ((int)monsterPedigree == 9)
        {
            bool result = Probability(90);

            if (result == true)
            {
                OnRaise(false, ref _monsterCoinCount, ref _batCount);
            }
            else
            {
                OnBat(false, ref _monsterCoinCount, ref _batCount);
            }
        }

        yield return new WaitForSecondsRealtime(0.3f);
    }

    private void ButtonControl()
    {
        if (_turnCount == 4)
        {
            _objButtons.SetActive(false);

            return;
        }

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
        string str = _textCommunityCard.text;

        if (_turnCount == 1)
        {
            _cards[_fieldCardIndex[2]].IsHide(false);
            _cards[_fieldCardIndex[3]].IsHide(false);
            _cards[_fieldCardIndex[4]].IsHide(false);

            string s1 = _cards[_fieldCardIndex[2]].name + " ";
            s1 += _cards[_fieldCardIndex[3]].name + " ";
            s1 += _cards[_fieldCardIndex[4]].name + " ";

            str = s1 + "** **";
        }

        if (_turnCount == 2)
        {
            _cards[_fieldCardIndex[5]].IsHide(false);

            string s1 = _cards[_fieldCardIndex[2]].name + " ";
            s1 += _cards[_fieldCardIndex[3]].name + " ";
            s1 += _cards[_fieldCardIndex[4]].name + " ";
            s1 += _cards[_fieldCardIndex[5]].name + " ";

            str = s1 + "**";
        }

        if (_turnCount == 3)
        {
            _cards[_fieldCardIndex[6]].IsHide(false);

            string s1 = _cards[_fieldCardIndex[2]].name + " ";
            s1 += _cards[_fieldCardIndex[3]].name + " ";
            s1 += _cards[_fieldCardIndex[4]].name + " ";
            s1 += _cards[_fieldCardIndex[5]].name + " ";
            s1 += _cards[_fieldCardIndex[6]].name;

            str = s1;
        }

        _textCommunityCard.text = str;

        if (_turnCount == 4)
        {
            SettleUp();
        }

        PredictionViewRanking();
    }

    private void SettleUp()
    {
        _objButtons.SetActive(false);
        StartCoroutine(Co_SettleUp());
    }

    IEnumerator Co_SettleUp()
    {
        yield return new WaitForSecondsRealtime(1f);

        IngameManager.instance.UpdateText("전투가 종료되었습니다.");

        List<int> nums = new List<int>(6);

        for (int i = 0; i <= 6; i++)
        {
            nums.Add(_fieldCardIndex[i]);
        }

        int playerHighCardNum = 0;
        eRankings playerRankings = Rankings(Sort(nums), ref playerHighCardNum);

        nums.Clear();

        for (int i = 2; i < _fieldCardIndex.Count; i++)
        {
            nums.Add(_fieldCardIndex[i]);
        }

        int monsterHighCardNum = 0;
        eRankings monsterRankings = Rankings(Sort(nums), ref monsterHighCardNum);

        IngameManager.instance.UpdateText(_userData.data.name + " 의 결과 : " + playerRankings);
        IngameManager.instance.UpdateText(_monster.name + " 의 결과 : " + monsterRankings);

        _resultDamage = _totalCount;

        if (playerRankings > monsterRankings)
        {
            _isPlayerWin = eWinorLose.Win;
            _resultDamage = _playerBat;
        }
        else if(playerRankings < monsterRankings)
        {
            _isPlayerWin = eWinorLose.Lose;
            _resultDamage -= _playerBat;
        }
        else if(playerRankings == monsterRankings)
        {
            _isPlayerWin = eWinorLose.Draw;

            IngameManager.instance.UpdateText(_userData.data.name + " 의 높은 카드 : " + ChangeCardNum(playerHighCardNum));
            IngameManager.instance.UpdateText(_monster.name + " 의 높은 카드 : " + ChangeCardNum(monsterHighCardNum));

            if(playerHighCardNum == monsterHighCardNum)
            {
                _isPlayerWin = eWinorLose.Draw;
                _resultDamage = 0;
            }
            else if(playerHighCardNum > monsterHighCardNum)
            {
                _isPlayerWin = eWinorLose.Win;
                _resultDamage = _playerBat;
            }
            else if(playerHighCardNum < monsterHighCardNum)
            {
                _isPlayerWin = eWinorLose.Lose;
                _resultDamage -= _playerBat;
            }

            _resultDamage = 0;
        }

        yield return new WaitForSecondsRealtime(1f);

        Done(_isPlayerWin);
    }

    private void Done(eWinorLose isPlayerWin)
    {
        _isPlayerWin = isPlayerWin;

        OnClose();
    }

    private void MakeCard()
    {
        _cards = new List<Card>();

        for (int i = 0; i < 52; i++)
        {
            Card card = new Card();

            if(i < 13)
            {
                card.Set(eCardShape.Spade, (i + 1));
            }

            if(13 <= i && i < 26)
            {
                card.Set(eCardShape.Diamond, (i + 1) - 13);
            }

            if (26 <= i && i < 39)
            {
                card.Set(eCardShape.Heart, (i + 1) - 26);
            }

            if (39 <= i)
            {
                card.Set(eCardShape.Clob, (i + 1) - 39);
            }

            _cards.Add(card);
        }
    }

    private void CardDistribution()
    {
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
        }

        for (int i = 0; i < _fieldCardIndex.Count; i++)
        {
            int index = _fieldCardIndex[i];

            if(i < 2)
            {
                _cards[index].IsHide(false);
                _textPlayerCard.text += _cards[index].name + " ";

                continue;
            }

            if(6 < i)
            {
                _cards[index].IsHide(false);
                continue;
            }

            _textCommunityCard.text += "** ";
        }
    }

    private List<Card> Sort(List<int> indexs)
    {
        List<Card> card = new List<Card>();

        for(int i = 0; i < indexs.Count; i++)
        {
            card.Add(_cards[indexs[i]]);
        }

        for (int i = card.Count - 1; i > 0; i--)
        {
            for (int j = 0; j < i; j++)
            {
                if (card[j].num < card[j + 1].num)
                {
                    Card temp = card[j];
                    card[j] = card[j + 1];
                    card[j + 1] = temp;
                }
            }
        }

        return card;
    }

    private eRankings Rankings(List<Card> card, ref int highCardNum)
    {
        if(card.Count == 0)
        {
            return eRankings.Non;
        }

        eRankings result = eRankings.HighCard;

        List<eRankings> results = new List<eRankings>();

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
            if (card[i].num != card[i + 1].num)
            {
                isChange = true;
                numSameCount = 0;

                if ((card[i].num - card[i + 1].num) == 1)
                {
                    straightCount++;

                    if (straightCount >= 5)
                    {
                        straight = true;

                        continue;
                    }

                    if (card[i].num == 14)
                    {
                        royal = true;

                        continue;
                    }
                }
                else
                {
                    royal = false;
                }
            }

            if (card[i].num == card[i + 1].num)
            {
                numSameCount++;
                isChange = false;

                if(numSameCount == 1)
                {
                    pairCount++;

                    continue;
                }

                if(isChange == true && pairCount == 2)
                {
                    twoPair = true;

                    continue;
                }

                if(numSameCount == 2)
                {
                    threeofaKind = true;

                    continue;
                }

                if(numSameCount == 3)
                {
                    foreofaKind = true;

                    continue;
                }

                if (threeofaKind == true && isChange == true && pairCount >= 1)
                {
                    fullHouse = true;

                    continue;
                }
            }

            if(highCardNum < card[i].num)
            {
                highCardNum = card[i].num;
            }
        }

        for (int i = 0; i < card.Count; i++)
        {
            if (card[i].shape == eCardShape.Spade)
            {
                spadeCount++;
            }
            else if(card[i].shape == eCardShape.Heart)
            {
                heartCount++;
            }
            else if(card[i].shape == eCardShape.Diamond)
            {
                diamondCount++;
            }
            else if(card[i].shape == eCardShape.Clob)
            {
                clobCount++;
            }
        }

        if(spadeCount >= 5 || heartCount >= 5 || diamondCount >= 5 || clobCount >= 5)
        {
            flush = true;
        }

        if (royal == true) // RoyalStraightFlush
        {
            result = eRankings.RoyalStraightFlush;
        }
        else if (straight == true && flush == true) // StraightFlush
        {
            result = eRankings.StraightFlush;
        }
        else if (foreofaKind == true) // FourofaKind
        {
            result = eRankings.FourofaKind;
        }
        else if (fullHouse == true) // FullHouse
        {
            result = eRankings.FullHouse;
        }
        else if (flush == true) // Flush
        {
            result = eRankings.Flush;
        }
        else if (straight == true) // Straight
        {
            result = eRankings.Straight;
        }
        else if (threeofaKind == true) // ThreeofaKind
        {
            result = eRankings.ThreeofaKind;
        }
        else if (twoPair == true) // TwoPair
        {
            result = eRankings.TwoPair;
        }
        else if (pairCount == 1) // OnePair
        {
            result = eRankings.OnePair;
        }
        else // HighCard
        {
            result = eRankings.HighCard;
            highCardNum = card[0].num;
        }

        return result;
    }

    private bool Probability(int value)
    {
        return (UnityEngine.Random.Range(0, 100) < value);
    }

    private string ChangeCardNum(int value)
    {
        string num = value.ToString();

        if (value == 11)
        {
            num = "J";
        }
        else if(value == 12)
        {
            num = "Q";
        }
        else if(value == 13)
        {
            num = "K";
        }
        else if(value == 14)
        {
            num = "A";
        }

        return num;
    }

    private void PlayBgm(eBgm type)
    {
        GameManager.instance.soundManager.PlayBgm(type);
    }

    private void PredictionViewRanking()
    {
        if(_fieldCardIndex.Count == 0)
        {
            return;
        }

        List<int> nums = new List<int>(6);

        for(int i = 0; i < _fieldCardIndex.Count - 2; i++)
        {
            if(_cards[_fieldCardIndex[i]].isHide == true)
            {
                continue;
            }

            nums.Add(_fieldCardIndex[i]);
        }

        int highCardNum = 0;
        eRankings pedigree = Rankings(Sort(nums), ref highCardNum);

        if(pedigree == eRankings.Non)
        {
            return;
        }

        _textViewRanking.text = "예상 족보는 " + pedigree + "입니다.";
    }
}
