using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class IngameManager : MonoBehaviour
{
    private static IngameManager _instance = null;
    public static IngameManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new IngameManager();
            }

            return _instance;
        }
    }

    [Header("Controllers")]
    [SerializeField] private MonsterController _monsterController = null;
    [SerializeField] private PlayerCntroller _playerController = null;
    [SerializeField] private ActionController _actionController = null;

    [Header("Ingame UI"), SerializeField] private IngameUI _ingameUI = null;
    [Header("TextView"), SerializeField] private TextView _textView = null;
    [Header("Control"), SerializeField] private ControlPad _controlPad = null;
    [Header("MapController"), SerializeField] private MapController _mapController = null;
    [Header("Ingame Popup"), SerializeField] private IngamePopup _ingamePopup = null;
    [Header("Shop"), SerializeField] private Shop _shop = null;
    [Header("Bonfire"), SerializeField] private Bonfire _bonfire = null;

    private DataManager.Save_Data _saveData = null;
    private MapGenerator _mapGenerator = null;

    private bool _isPlayerTurn = false;
    private bool _isHuntMonster = false;

    public DataManager.Save_Data saveData
    {
        get { return _saveData; }
    }

    public bool isAllMonsterDead
    {
        get { return _monsterController.isAllMonsterDead; }
        set { _monsterController.isAllMonsterDead = value; }
    }

    public bool isPlayerTurn
    {
        get { return _isPlayerTurn; }
        set { _isPlayerTurn = value; }
    }

    public bool isHuntMonster 
    {
        get { return _isHuntMonster; }
        set { _isHuntMonster = value; }
    }

    void Start()
    {
        _instance = this;

        GameManager.instance.tools.Fade(true, null);

        _saveData = GameManager.instance.dataManager.CopySaveData();

        _monsterController.Initialize();
        _playerController.Initialize();
        _actionController.Initialize();

        _ingameUI.Initialize(OpenMap, OpenNextRound, _textView.UpdateText, _ingamePopup.UpdateText);
        _textView.Initialize();
        _controlPad.Initialize(_playerController.PlayerMove, _playerController.PlayerAction);
        _mapController.Initialize(GameManager.instance.dataManager.MapSize);
        _ingamePopup.Initialize();
        _shop.Initialize(_textView.UpdateText, _ingamePopup.UpdateText, _actionController.Buy);
        _bonfire.Initialize(_textView.UpdateText, _ingamePopup.UpdateText, _actionController.SelectSkill, _controlPad.Skill);

        FirstSet();

        this.gameObject.SetActive(true);
    }

    private void FirstSet()
    {
        if (_saveData.round % 5 == 0)
        {
            PlayBgm(eBgm.Shop);
        }
        else
        {
            PlayBgm(eBgm.Ingame);
        }

        if (_saveData.round > 1)
        {
            _ingameUI.OpenNextRoundWindow(eRoundClear.Load);

            return;
        }

        _ingameUI.OpenNextRoundWindow(eRoundClear.First);
    }

    private void OpenMap(System.Action onCallback)
    {
        _mapController.OpenMap(onCallback);
    }

    private void OpenNextRound(eRoundClear type)
    {
        if(type == eRoundClear.Restart)
        {
            UpdateText("--- 부활하였습니다.");
            
            GameManager.instance.dataManager.ResetPlayerData();
            _ingameUI.UpdatePlayerInfo(_saveData.userData);

            return;
        }

        _textView.DeleteTemplate();

        if (type == eRoundClear.Load)
        {
            RoundSet();

            return;
        }

        if (type == eRoundClear.Fail)
        {
            GameManager.instance.tools.Fade(false, () => 
            {
                GameManager.instance.soundManager.PlaySfx(eSfx.GotoLobby);

                GameManager.instance.tools.SceneChange(eScene.Lobby, () =>
                {
                    GameManager.instance.dataManager.FailGame(_saveData);
                });
            });

            return;
        }

        if(type == eRoundClear.Success)
        {
            UpdateText("--- " + _saveData.round + " 라운드 입니다.");
            GameManager.instance.soundManager.PlaySfx(eSfx.TurnPage);
        }

        GameManager.instance.dataManager.SaveDataToCloud(_saveData, (result) => 
        {
            _mapGenerator = new MapGenerator(GenerateMap, _saveData);

            GameManager.instance.dataManager.ResetPlayerData();
            UpdateData();
        }); 
    }

    private void GenerateMap(DataManager.Map_Data mapData)
    {
        _saveData.mapData = mapData;
        
        RoundSet();
    }
     
    private void RoundSet()
    {
        UpdatePlayerCoord();
        _ingameUI.SetRoundText(_saveData.round);

        UpdateData();

        PlayerTurn();

        if (_saveData.mapData.monsterDatas.Count > 0)
        {
            _monsterController.isAllMonsterDead = false;
        }
    }

    public void RoundClear()
    {
        GameManager.instance.dataManager.UpdatePlayerData(_saveData);

        _mapController.Close();
        _saveData.round++;

        _ingameUI.OpenNextRoundWindow(eRoundClear.Success);
    }

    public void ControlPad_Skill()
    {
        _controlPad.Skill(_saveData.userData, _actionController.Skill);
    }

    public void ControlPad_Bag()
    {
        _controlPad.Bag(_saveData.userData, _actionController.Bag);
    }

    public void SetMap(List<int> nearbyIndexs)
    {
        _mapController.SetMap(_saveData, nearbyIndexs);
    }

    public void PlayBgm(eBgm type)
    {
        GameManager.instance.soundManager.PlayBgm(type);
    }

    #region ActionController

    public void BonfireOpen(DataManager.Npc_Data npc)
    {
        _bonfire.Open(npc, _saveData.userData);
    }

    public void ShopOpen(DataManager.Npc_Data npc)
    {
        _shop.Open(npc, _saveData.userData.data.coin);
    }

    public void CallAttacker(DataManager.Creature_Data monster, Action onLastCallback, Action<eWinorLose, int> onResultCallback)
    {
        _ingameUI.CallAttacker(_saveData.userData, monster, onLastCallback, onResultCallback);
    }

    public void OpneLevelPoint()
    {
        _ingameUI.OpneLevelPoint(_saveData.userData, (newData) =>
        {
            _saveData.userData.data.hp = newData.data.hp;
            _saveData.userData.data.mp = newData.data.mp;
            _saveData.userData.data.ap = newData.data.ap;
            _saveData.userData.data.attack = newData.data.attack;
            _saveData.userData.data.defence = newData.data.defence;
            _saveData.userData.data.vision = newData.data.vision;
            _saveData.userData.data.attackRange = newData.data.attackRange;

            _saveData.userData.currentHP = _saveData.userData.maximumHP;
            _saveData.userData.currentMP = _saveData.userData.maximumMP;
            _saveData.userData.currentAP = _saveData.userData.maximumAP;
            _saveData.userData.currentVISION = _saveData.userData.maximumVISION;

            UpdateData();
        });
    }

    public void ControlPadUpdateData()
    {
        _controlPad.UpdateData(_saveData.userData);
    }

    public void Attack(int index, Action onLastCallback = null)
    {
        _actionController.Attack(index, onLastCallback);
    }

    public void Defence()
    {
        _actionController.Defence();
    }

    public void Npc(int index)
    {
        _actionController.Npc(index);
    }

    #endregion

    #region Creature

    public void PlayerTurn()
    {
        _isPlayerTurn = true;

        _saveData.userData.currentAP = _saveData.userData.maximumAP;
        UpdatePlayerInfo(eStats.AP);

        UpdateText("행동력이 " + IngameManager.instance.saveData.userData.currentAP + "만큼 남았습니다.");

        List<int> NearbyIndexs = _playerController.PlayerSearchNearby();

        SetMap(NearbyIndexs);
    }

    public void PlayerTurnOut()
    {
        _isPlayerTurn = false;

        _actionController.SkillRemindDuration();
        _actionController.ItemRemindDuration();

        UpdateText("--- " + _saveData.userData.data.name + "의 순서가 종료됩니다.");
    }

    public void MonsterTurn()
    {
        StartCoroutine(_monsterController.MonsterTurn());
    }

    public void MonsterDead(DataManager.Creature_Data monster)
    {
        _monsterController.MonsterDead(monster);
    }

    public void MonsterTurnOut()
    {
        UpdateText("--- 몬스터의 순서가 종료되었습니다.");

        UpdateData();
        PlayerTurn();
    }

    #endregion

    #region Update

    public void UpdatePlayerCoord()
    {
        _textView.UpdateText(_saveData.mapData.nodeDatas[_saveData.userData.data.currentNodeIndex]);
    }

    public void UpdateText(string message)
    {
        _textView.UpdateText(message);
    }

    public void UpdateText(int nearbyBlockIndex)
    {
        _textView.UpdateText(_saveData.mapData.nodeDatas[nearbyBlockIndex]);
    }

    public void UpdateText(eCreature type, int monsterNodeIndex)
    {
        _textView.UpdateText(type, _saveData.mapData.nodeDatas[monsterNodeIndex], _saveData.mapData.nodeDatas[_saveData.userData.data.currentNodeIndex]);
    }

    public void UpdatePopup(string message)
    {
        _ingamePopup.UpdateText(message);
    }

    public void UpdatePlayerInfo(eStats type)
    {
        _ingameUI.UpdatePlayerInfo(type, _saveData.userData);
    }

    public void UpdateMap()
    {
        if (GameManager.instance.isMapBackgroundUpdate == true)
        {
            _mapController.Close(false, () =>
            {
                _ingameUI.HideMapButton();
                _mapController.UpdateMapData(_saveData, Vision(_saveData.userData.currentVISION, _saveData.userData.data.currentNodeIndex));
            });

            return;
        }

        _ingameUI.HideMapButton();
        _mapController.UpdateMapData(_saveData, Vision(_saveData.userData.currentVISION, _saveData.userData.data.currentNodeIndex));
    }

    public void UpdateData(string contnet = null)
    {
        _ingameUI.UpdatePlayerInfo(_saveData.userData);

        if (_saveData.userData.currentHP == 0)
        {
            _ingameUI.OpenNextRoundWindow(eRoundClear.Fail, contnet);
        }

        _mapController.UpdateMapData(_saveData, Vision(_saveData.userData.currentVISION, _saveData.userData.data.currentNodeIndex));
    }

    #endregion

    #region Function

    public int PathFinding(ref DataManager.Map_Data mapData, int startNodeIndex, int endNodeIndex)
    {
        return _mapGenerator.PathFinding(ref mapData, startNodeIndex, endNodeIndex);
    }

    public int GetNearbyBlocks(int x, int y, int index)
    {
        int result = 0;

        if (((y == -1 || y == 1) && x == 0) || (y == 0 && (x == 1 || x == -1)))
        {
            int nearbyX = (index % _saveData.mapData.mapSize) + x;

            if (nearbyX < 0 || _saveData.mapData.mapSize <= nearbyX)
            {
                return -1;
            }

            int nearbyY = (index / _saveData.mapData.mapSize) + y;

            if (nearbyY < 0 || _saveData.mapData.mapSize <= nearbyY)
            {
                return -1;
            }

            int resultIndex = nearbyX + (nearbyY * _saveData.mapData.mapSize);

            if (index == resultIndex)
            {
                return -1;
            }

            if (0 <= resultIndex && resultIndex < (_saveData.mapData.mapSize * _saveData.mapData.mapSize))
            {
                result = resultIndex;
            }
        }

        return result;
    }

    int[] dx = new int[] { -1, 0, 1 };
    int[] dy = new int[] { -1, 0, 1 };
    public List<int> GetNearbyBlocks(int index)
    {
        List<int> result = new List<int>();

        for (int y = 0; y < dy.Length; y++)
        {
            for (int x = 0; x < dx.Length; x++)
            {
                if (Mathf.Abs(dx[x]) + Mathf.Abs(dy[y]) <= Mathf.Abs(dx[dx.Length - 1]))
                {
                    int nearbyX = (index % _saveData.mapData.mapSize) + dx[x];

                    if (nearbyX < 0 || _saveData.mapData.mapSize <= nearbyX)
                    {
                        continue;
                    }

                    int nearbyY = (index / _saveData.mapData.mapSize) + dy[y];

                    if (nearbyY < 0 || _saveData.mapData.mapSize <= nearbyY)
                    {
                        continue;
                    }

                    int resultIndex = nearbyX + (nearbyY * _saveData.mapData.mapSize);

                    if (index == resultIndex)
                    {
                        continue;
                    }

                    if (resultIndex == _saveData.mapData.exitNodeIndex)
                    {
                        continue;
                    }

                    if (0 <= resultIndex && resultIndex < (_saveData.mapData.mapSize * _saveData.mapData.mapSize))
                    {
                        result.Add(resultIndex);
                    }
                }
            }
        }

        return result;
    }

    public List<int> GetNearbyBlocks_Diagonal(List<int> dx, List<int> dy, int index)
    {
        List<int> result = new List<int>();

        for (int y = 0; y < dy.Count; y++)
        {
            for (int x = 0; x < dx.Count; x++)
            {
                if (Mathf.Abs(dx[x]) + Mathf.Abs(dy[y]) <= Mathf.Abs(dx[dx.Count - 1]) ||
                    Mathf.Abs(dx[x]) <= 1 && Mathf.Abs(dy[y]) <= 1)
                {
                    int nearbyX = (index % _saveData.mapData.mapSize) + dx[x];

                    if (nearbyX < 0 || _saveData.mapData.mapSize <= nearbyX)
                    {
                        continue;
                    }

                    int nearbyY = (index / _saveData.mapData.mapSize) + dy[y];

                    if (nearbyY < 0 || _saveData.mapData.mapSize <= nearbyY)
                    {
                        continue;
                    }

                    int resultIndex = nearbyX + (nearbyY * _saveData.mapData.mapSize);

                    if (index == resultIndex)
                    {
                        continue;
                    }

                    if (0 <= resultIndex && resultIndex < (_saveData.mapData.mapSize * _saveData.mapData.mapSize))
                    {
                        result.Add(resultIndex);
                    }
                }
            }
        }

        return result;
    }

    public List<int> Vision(int vision, int currentIndex)
    {
        List<int> dx = new List<int>();
        List<int> dy = new List<int>();

        for (int i = -vision; i <= vision; i++)
        {
            dx.Add(i);
            dy.Add(i);
        }

        return GetNearbyBlocks_Diagonal(dx, dy, currentIndex);
    }

    #endregion
}

#region Generator

public class MapGenerator
{
    private class Node
    {
        public bool isEnter = false;
        public bool isExit = false;

        public bool isVisit = false;
        public bool isWalkable = true;

        public int _index = 0;
        public int _x = 0;
        public int _y = 0;

        public int _g = 0; // cost to start
        public int _h = 0; // cost to end
        public int f 
        { 
            get { return _g + _h; }
        }

        public Node(int index, int x, int y)
        {
            _index = index;
            _x = x;
            _y = y;
        }

        public void SetStartEnd(Node startNode, Node endNode)
        {
            _g = (int)Mathf.Sqrt(Mathf.Pow(Mathf.Abs(_x - startNode._x), 2) + Mathf.Pow(Mathf.Abs(_y - startNode._y), 2));
            _h = (int)Mathf.Sqrt(Mathf.Pow(Mathf.Abs(_x - endNode._x), 2) + Mathf.Pow(Mathf.Abs(_y - endNode._y), 2));
        }
    }

    private System.Action<DataManager.Map_Data> _onResultCallback = null;
    private DataManager.Save_Data _saveData = null;

    private List<Node> _nodes = null;

    private int _mapSize = 0;

    public MapGenerator(System.Action<DataManager.Map_Data> onResultCallback, DataManager.Save_Data saveData)
    {
        if(onResultCallback != null)
        {
            _onResultCallback = onResultCallback;
        }

        _mapSize = saveData.mapData.mapSize;
        _saveData = saveData;

        Start();
    }

    private void Start()
    {
        _nodes = new List<Node>();

        for (int y = 0; y < _mapSize; y++)
        {
            for (int x = 0; x < _mapSize; x++)
            {
                Node node = new Node((y * _mapSize) + x, x, y);

                _nodes.Add(node);
            }
        }

        if (_saveData.round % 5 != 0)
        {
            GenerateBlocker();
        }
        else
        {
            _nodes[14].isEnter = true;
        }

        SelectExitBlock();
        Done();
    }
    private void GenerateBlocker()
    {
        List<int> middlePoints = new List<int>();

        int middleMapSize = _mapSize / 3;

        for (int y = 0; y < middleMapSize; y++)
        {
            for (int x = 0; x < middleMapSize; x++)
            {
                int middleX = 1 + (3 * x);
                int middleY = 1 + (3 * y);
                int middleCoord = middleX + (middleY * _mapSize);

                if (middleCoord == (Mathf.Pow(_mapSize, 2) - 1) * 0.5f)
                {
                    continue;
                }

                middlePoints.Add(middleCoord);
            }
        }

        List<int> randomBlockerIndexs = RandomIndex(middlePoints.Count, 3);

        for (int r = 0; r < randomBlockerIndexs.Count; r++)
        {
            List<int> nearbyBlocks = GetNearbyBlocks_Diagonal(middlePoints[randomBlockerIndexs[r]]);
            _nodes[middlePoints[randomBlockerIndexs[r]]].isWalkable = false;

            for (int n = 0; n < nearbyBlocks.Count; n++)
            {
                _nodes[nearbyBlocks[n]].isWalkable = false;
            }
        }

        CheckStuckBlocker(ref middlePoints);
        SelectEnterBlock(ref middlePoints);
    }

    private void CheckStuckBlocker(ref List<int> middlePoints)
    {
        for (int m = 0; m < middlePoints.Count; m++)
        {
            if (_nodes[middlePoints[m]].isWalkable == false)
            {
                continue;
            }

            List<int> NearbyMiddlePoints = NearbyMiddlePoint(middlePoints[m]);
            bool isStuck = true;

            for (int n = 0; n < NearbyMiddlePoints.Count; n++)
            {
                if (_nodes[NearbyMiddlePoints[n]].isWalkable == true)
                {
                    isStuck = false;
                    break;
                }
            }

            if(isStuck == true)
            {
                List<int> nearbyBlocks = GetNearbyBlocks_Diagonal(middlePoints[m]);
                _nodes[middlePoints[m]].isWalkable = false;

                for (int n = 0; n < nearbyBlocks.Count; n++)
                {
                    _nodes[nearbyBlocks[n]].isWalkable = false;
                }
            }
        }
    }

    private void SelectEnterBlock(ref List<int> middlePoints)
    {
        List<int> openMiddlePoints = new List<int>();

        for (int i = 0; i < middlePoints.Count; i++)
        {
            if(_nodes[middlePoints[i]].isWalkable == false)
            {
                continue;
            }

            openMiddlePoints.Add(middlePoints[i]);
        }

        _nodes[openMiddlePoints[UnityEngine.Random.Range(0, openMiddlePoints.Count)]].isEnter = true;
    }

    private void SelectExitBlock()
    {
        int centerIndex = (int)((Mathf.Pow(_mapSize, 2) - 1) * 0.5f);

        _nodes[centerIndex].isExit = true;
    }

    private void Done()
    {
        _saveData.mapData = new DataManager.Map_Data();
        _saveData.mapData.mapSize = _mapSize;
        _saveData.mapData.nodeDatas = new List<DataManager.Node_Data>();

        for (int n = 0; n < _nodes.Count; n++)
        {
            DataManager.Node_Data node = new DataManager.Node_Data();
            node.index = _nodes[n]._index;
            node.x = (ushort)_nodes[n]._x;
            node.y = (ushort)_nodes[n]._y;
            node.isWalkable = _nodes[n].isWalkable;

            if(_nodes[n].isEnter == true)
            {
                _saveData.mapData.enterNodeIndex = n;
            }

            if (_nodes[n].isExit == true)
            {
                _saveData.mapData.exitNodeIndex = n;
            }

            _saveData.mapData.nodeDatas.Add(node);
        }

        CreatureGenerator _creatureGenerator = new CreatureGenerator(GenerateCreature, _saveData);
    }

    private void GenerateCreature(DataManager.Creature_Data playerData, List<DataManager.Creature_Data> monsterData)
    {
        _saveData.userData.data = playerData;
        _saveData.mapData.monsterDatas = monsterData;

        _onResultCallback?.Invoke(_saveData.mapData);
    }

    private List<int> RandomIndex(int listCount, int value)
    {
        List<int> result = new List<int>();

        int count = 0;

        while(count < value)
        {
            int num = UnityEngine.Random.Range(0, listCount);
            bool isSelect = false;

            for (int i = 0; i < result.Count; i++)
            {
                if(result[i] == num)
                {
                    isSelect = true;
                    break;
                }
            }

            if(isSelect == true)
            {
                continue;
            }

            result.Add(num);
            count++;
        }

        return result;
    }

    int[] dx = new int[] { -1, 0, 1 };
    int[] dy = new int[] { -1, 0, 1 };

    private List<int> GetNearbyBlocks_Diagonal(int index)
    {
        List<int> result = new List<int>();

        for (int y = 0; y < dy.Length; y++)
        {
            for (int x = 0; x < dx.Length; x++)
            {
                int nearbyX = (index % _mapSize) + dx[x];
                int nearbyY = (index / _mapSize) + dy[y];
                int resultIndex = nearbyX + (nearbyY * _mapSize);

                if (index == resultIndex)
                {
                    continue;
                }

                if (0 <= resultIndex && resultIndex < (_mapSize * _mapSize))
                {
                    result.Add(resultIndex);
                }
            }
        }

        return result;
    }

    private List<int> GetNearbyBlocks(int index)
    {
        List<int> result = new List<int>();

        for (int y = 0; y < dy.Length; y++)
        {
            for (int x = 0; x < dx.Length; x++)
            {
                if(((dy[y] == -1 || dy[y] == 1) && dx[x] == 0) || (dy[y] == 0 && (dx[x] == 1 || dx[x] == -1)))
                {
                    int nearbyX = (index % _mapSize) + dx[x];

                    if (nearbyX < 0 || _mapSize <= nearbyX)
                    {
                        continue;
                    }

                    int nearbyY = (index / _mapSize) + dy[y];

                    if (nearbyY < 0 || _mapSize <= nearbyY)
                    {
                        continue;
                    }

                    int resultIndex = nearbyX + (nearbyY * _mapSize);

                    if (index == resultIndex)
                    {
                        continue;
                    }

                    if (0 <= resultIndex && resultIndex < (_mapSize * _mapSize))
                    {
                        result.Add(resultIndex);
                    }
                }
            }
        }

        return result;
    }

    int[] mdx = new int[] { -3, 0, 3 };
    int[] mdy = new int[] { -3, 0, 3 };

    private List<int> NearbyMiddlePoint(int index)
    {
        List<int> result = new List<int>();

        for (int y = 0; y < mdy.Length; y++)
        {
            for (int x = 0; x < mdx.Length; x++)
            {
                if(((mdy[y] == -3 || mdy[y] == 3) && mdx[x] == 0) || (mdy[y] == 0 && (mdx[x] == 3 || mdx[x] == -3)))
                {
                    int nearbyX = (index % _mapSize) + mdx[x];

                    if (nearbyX < 0 || _mapSize <= nearbyX)
                    {
                        continue;
                    }

                    int nearbyY = (index / _mapSize) + mdy[y];

                    if (nearbyY < 0 || _mapSize <= nearbyY)
                    {
                        continue;
                    }

                    int resultIndex = nearbyX + (nearbyY * _mapSize);

                    if (index == resultIndex)
                    {
                        continue;
                    }

                    if (0 <= resultIndex && resultIndex < (_mapSize * _mapSize))
                    {
                        result.Add(resultIndex);
                    }
                }
            }
        }

        return result;
    }

    public int PathFinding(ref DataManager.Map_Data mapData, int startNodeIndex, int endNodeIndex)
    {
        int result = startNodeIndex;

        foreach (var node in _nodes)
        {
            node.SetStartEnd(_nodes[startNodeIndex], _nodes[endNodeIndex]);
        }

        List<int> nearbyNodeIndexs = GetNearbyBlocks(startNodeIndex);

        int f = 9999;

        for (int i = 0; i < nearbyNodeIndexs.Count; i++)
        {
            int index = nearbyNodeIndexs[i];

            if (_nodes[index].f < f)
            {
                if(mapData.nodeDatas[index].isMonster == true)
                {
                    continue;
                }

                if (mapData.nodeDatas[index].isWalkable == false)
                {
                    continue;
                }

                if (mapData.nodeDatas[index].isUser == true)
                {
                    continue;
                }

                if (index == mapData.enterNodeIndex)
                {
                    continue;
                }

                if (index == mapData.exitNodeIndex)
                {
                    continue;
                }

                result = index;
                f = _nodes[index].f;
            }
        }

        return result;
    }
}

public class CreatureGenerator
{
    private System.Action<DataManager.Creature_Data, List<DataManager.Creature_Data>> _onResultCallback = null;
    private DataManager.Save_Data _saveData = null;

    public CreatureGenerator(System.Action<DataManager.Creature_Data, List<DataManager.Creature_Data>> onResultCallback, DataManager.Save_Data saveData)
    {
        if(onResultCallback != null)
        {
            _onResultCallback = onResultCallback;
        }

        _saveData = saveData;

        Start();
    }

    private void Start()
    {
        _saveData.mapData.monsterDatas = new List<DataManager.Creature_Data>();

        GeneratePlayer();

         if (_saveData.round % 5 == 0)
        {
            GenerateShop();
            GeneratorBonfire();
        }
        else
        {
            if (_saveData.round == 1)
            {
                GenerateGuide();
            }

            GenerateMonster();
        }

        Done();
    }

    private void GenerateGuide()
    {
        _saveData.mapData.npcDatas = new List<DataManager.Npc_Data>();

        int currentIndex = _saveData.mapData.enterNodeIndex + 1;

        DataManager.Npc_Data npc = GameManager.instance.dataManager.GetNpcData(203);
        npc.id = 203;
        npc.currentNodeIndex = currentIndex;
        npc.isGuide = true;

        _saveData.mapData.nodeDatas[currentIndex].isGuide = true;
        _saveData.mapData.npcDatas.Add(npc);
    }

    private void GeneratePlayer()
    {
        _saveData.userData.data.currentNodeIndex = _saveData.mapData.enterNodeIndex;
        _saveData.mapData.nodeDatas[_saveData.userData.data.currentNodeIndex].isUser = true;
    }

    private void GenerateShop()
    {
        _saveData.mapData.npcDatas = new List<DataManager.Npc_Data>();

        int currentIndex = (int)(_saveData.mapData.mapSize * 6) +  2;

        DataManager.Npc_Data npc = GameManager.instance.dataManager.GetNpcData(201);
        npc.id = 201;
        npc.currentNodeIndex = currentIndex;
        npc.itemIndexs = new List<short>(3);
        npc.isShop = true;

        for (int i = 0; i < 3; i++)
        {
            int ran = Random.Range(501, 500 + GameManager.instance.dataManager.GetItemDataCount());

            npc.itemIndexs.Add((short)ran);
        }

        _saveData.mapData.nodeDatas[currentIndex].isShop = true;
        _saveData.mapData.npcDatas.Add(npc);
    }

    private void GeneratorBonfire()
    {
        int currentIndex = (int)(_saveData.mapData.mapSize * 6) + 6;

        DataManager.Npc_Data npc = GameManager.instance.dataManager.GetNpcData(202);
        npc.id = 202;
        npc.currentNodeIndex = currentIndex;
        npc.SkillIndexs = new List<short>(3);
        npc.isBonfire = true;

        for (int i = 0; i < 3; i++)
        {
            int ran = Random.Range(301, 300 + GameManager.instance.dataManager.GetSkillDataCount());

            npc.SkillIndexs.Add((short)ran);
        }

        _saveData.mapData.nodeDatas[currentIndex].isBonfire = true;
        _saveData.mapData.npcDatas.Add(npc);
    }

    private void GenerateMonster()
    {
        _saveData.mapData.monsterDatas = new List<DataManager.Creature_Data>();

        int openNodeCount = 0;

        for (int i = 0; i < _saveData.mapData.nodeDatas.Count; i++)
        {
            if(_saveData.mapData.nodeDatas[i].isWalkable == false)
            {
                continue;
            }

            openNodeCount++;
        }

        int monsterSpawnCount = 0;
        if (_saveData.round < 3)
        {
            monsterSpawnCount = _saveData.round;
        }
        else
        {
            monsterSpawnCount = Random.Range(3, openNodeCount / _saveData.mapData.mapSize);
        }

        for (int i = 0; i < monsterSpawnCount; i++)
        {
            DataManager.Node_Data node = new DataManager.Node_Data();
            SpawnMonsterNodeSelect(Random.Range(0, _saveData.mapData.nodeDatas.Count), ref node);

            int id = Random.Range(0, GameManager.instance.dataManager.GetCreaturDataCount());
            DataManager.Creature_Data creature = GameManager.instance.dataManager.GetCreatureData(id);
            
            if (creature != null)
            {
                creature.id = (short)i;
                creature.currentNodeIndex = node.index;
                _saveData.mapData.nodeDatas[node.index].isMonster = true;

                MonsterStats(ref creature);

                _saveData.mapData.monsterDatas.Add(creature);
            }
        }
    }

    private void MonsterStats(ref DataManager.Creature_Data creature)
    {
        float increasePoint = ((_saveData.round - 1) * 0.1f);

        creature.coin += (short)(creature.coin * increasePoint);
        creature.hp += (short)(creature.hp * increasePoint);
        creature.exp += (short)(creature.exp * increasePoint);
        creature.attack += (short)(creature.attack * increasePoint);
        creature.defence += (short)(creature.defence * (increasePoint * 0.5f));
    }

    private void Done()
    {
        _onResultCallback?.Invoke(_saveData.userData.data, _saveData.mapData.monsterDatas);
    }

    private void SpawnMonsterNodeSelect(int index, ref DataManager.Node_Data node)
    {
        if (_saveData.mapData.nodeDatas[index].isWalkable == true 
            && _saveData.mapData.nodeDatas[index].isMonster == false 
            && _saveData.mapData.nodeDatas[index].isUser == false)
        {
            node = _saveData.mapData.nodeDatas[index];

            return;
        }

        int newIndex = index;

        if (_saveData.mapData.nodeDatas[index].isWalkable == false)
        {
            newIndex += 1;
        }

        if (newIndex >= _saveData.mapData.nodeDatas.Count)
        {
            newIndex -= _saveData.mapData.nodeDatas.Count;
        }

        if (_saveData.mapData.nodeDatas[newIndex].isMonster == true)
        {
            newIndex += 5;
        }

        if (newIndex >= _saveData.mapData.nodeDatas.Count)
        {
            newIndex -= _saveData.mapData.nodeDatas.Count;
        }

        if (_saveData.mapData.nodeDatas[newIndex].isUser == true)
        {
            newIndex += 5;
        }

        if (newIndex >= _saveData.mapData.nodeDatas.Count)
        {
            newIndex -= _saveData.mapData.nodeDatas.Count;
        }

        if (_saveData.mapData.nodeDatas[newIndex].isGuide == true)
        {
            newIndex += 5;
        }

        if (newIndex >= _saveData.mapData.nodeDatas.Count)
        {
            newIndex -= _saveData.mapData.nodeDatas.Count;
        }

        if (newIndex == _saveData.mapData.enterNodeIndex)
        {
            newIndex += 4;
        }

        if (newIndex >= _saveData.mapData.nodeDatas.Count)
        {
            newIndex -= _saveData.mapData.nodeDatas.Count;
        }

        if (newIndex == _saveData.mapData.exitNodeIndex)
        {
            newIndex += 4;
        }

        if (newIndex >= _saveData.mapData.nodeDatas.Count)
        {
            newIndex -= _saveData.mapData.nodeDatas.Count;
        }

        SpawnMonsterNodeSelect(newIndex, ref node);
    }
}

#endregion