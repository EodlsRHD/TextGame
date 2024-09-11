using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameManager : MonoBehaviour
{
    [Header("Ingame UI"), SerializeField] private IngameUI _ingameUI = null;
    [Header("TextView"), SerializeField] private TextView _textView = null;
    [Header("Control"), SerializeField] private Control _control = null;

    private DataManager.Save_Data _saveData = null;
    private MapGenerator _mapGenerator = null;

    void Start()
    {
        _saveData = GameManager.instance.dataManager.CopySaveData();

        _ingameUI.Initialize(GetMap, OpenNextRound);
        _textView.Initialize();
        _control.Initialize(Move, Action);

        _ingameUI.OpenNextRoundWindow(eRoundClear.First);

        this.gameObject.SetActive(true);
    }

    private void GetMap(System.Action<DataManager.Map_Data> onResultCallback)
    {
        onResultCallback?.Invoke(_saveData.mapData);
    }

    private void OpenNextRound(eRoundClear type)
    {
        if(type == eRoundClear.Fail)
        {
            GameManager.instance.tools.SceneChange(eScene.Lobby, () => 
            {
                GameManager.instance.dataManager.FailGame(_saveData);
            });

            return;
        }

        _saveData.round++;
        _ingameUI.SetRoundText(_saveData.round);

        GameManager.instance.dataManager.SaveDataToCloud(_saveData, () => 
        {
            GenarateMap();
        });
    }

    private void GenarateMap()
    {
        _mapGenerator = new MapGenerator((mapData) => { _saveData.mapData = mapData; });
        _mapGenerator.Start(_saveData.round);
    }

    private void Move(eControl type)
    {

    }

    private void Action(eControl type)
    {

    }

    // 만난 적, 획득한 아이템 저장 하는 기능 필요
}

public class MapGenerator
{
    public class BlockData
    {
        public ushort x = 0;
        public ushort y = 0;

        public bool isWalkAble = true;
        public bool isCreatureExist = false;

        public bool isEnter = false;
        public bool isExit = false;

        public DataManager.Creature_Data creature = null;
    }

    private DataManager.Map_Data _mapData = null;
    private BlockData[] _blockData = null;
    private int _round = 0;

    public delegate void OnResultCallback(DataManager.Map_Data mapData);
    private OnResultCallback _onResultCallback = null;

    public MapGenerator(OnResultCallback onResultCallback)
    {
        if (onResultCallback != null)
        {
            _onResultCallback = onResultCallback;
        }
    }

    public void Start(ushort round)
    {
        _mapData = new DataManager.Map_Data();
        _mapData.blockDatas = new List<DataManager.Block_Data>();
        _mapData.monsterDatas = new List<DataManager.Creature_Data>();
        _mapData.itemDatas = new List<DataManager.Item_Data>();

        _blockData = new BlockData[64];
        _round = round;

        GenarateMap();
        GenerateCreature();
        SetMapData();
    }

    private void GenarateMap()
    {
        for (int y = 1; y < 9; y++)
        {
            for (int x = 1; x < 9; x++)
            {
                int coord = (x + (y - 1) * 8) - 1;

                _blockData[coord] = new BlockData();
                _blockData[coord].x = (ushort)x;
                _blockData[coord].y = (ushort)y;
            }
        }

        bool isX = Random.Range(0, 2) == 0 ? true : false;

        int enterX = 0;
        int enterY = 0;
        int index = 0;

        if (isX == true)
        {
            enterX = Random.Range(1, 9);
            enterY = Random.Range(0, 2) == 0 ? 1 : 8;

            index = (enterX + (enterY - 1) * 8) - 1;
        }
        else
        {
            enterX = Random.Range(0, 2) == 0 ? 1 : 8;
            enterY = Random.Range(1, 9);

            index = (enterY + (enterX - 1) * 8) - 1;
        }

        _blockData[index].isEnter = true;
        _blockData[63 - index].isExit = true;

        _mapData.enterBlockIndex = index;
        _mapData.exitBlockIndex = 63 - index;

        int blocker = Random.Range(0, 2);
        for (int i = 0; i <= blocker; i++)
        {
            SpawnBlocker();
        }

        ShortestDistance();
    }

    private void SpawnBlocker()
    {
        bool isX = Random.Range(0, 2) == 0 ? true : false;

        int enterX = 0;
        int enterY = 0;
        int index = 0;

        if (isX == true)
        {
            enterX = Random.Range(2, 8);
            enterY = Random.Range(0, 2) == 0 ? 2 : 7;

            index = (enterX + (enterY - 1) * 8) - 1;
        }
        else
        {
            enterX = Random.Range(0, 2) == 0 ? 2 : 7;
            enterY = Random.Range(2, 8);

            index = (enterY + (enterX - 1) * 8) - 1;
        }

        if (_blockData[index].isWalkAble == false)
        {
            SpawnBlocker();
            return;
        }

        _blockData[index].isWalkAble = false;

        SpawnBlocker_SearchAround(index, 2);
    }

    private void SpawnBlocker_SearchAround(int centerIndex, int a)
    {
        if (a == 0)
        {
            return;
        }

        List<int> indexs = new List<int>(4);
        indexs.Add(centerIndex - 8);
        indexs.Add(centerIndex + 8);
        indexs.Add(centerIndex - 1);
        indexs.Add(centerIndex + 1);

        for (int i = 0; i < indexs.Count; i++)
        {
            if(0 > indexs[i])
            {
                continue;
            }

            if(_blockData.Length - 1 < indexs[i])
            {
                continue;
            }

            if (_blockData[indexs[i]].isWalkAble == false)
            {
                continue;
            }

            if (_blockData[indexs[i]].isEnter == true || _blockData[indexs[i]].isExit == true)
            {
                _blockData[centerIndex].isWalkAble = true;

                continue;
            }

            _blockData[indexs[i]].isWalkAble = false;

            SpawnBlocker_SearchAround(indexs[i], a - 1);
        }
    }

    public class Node
    {
        public bool isPass = false;

        public int passIndex = 0;
        public int _x = 0;
        public int _y = 0;

        private int _costG = 0;
        private int _costH = 0;

        public int costF
        {
            get { return _costG + _costH; }
        }

        public Node(BlockData data, BlockData endData)
        {
            _x = data.x;
            _y = data.y;

            _costH = (int)(Mathf.Pow(_x - endData.x, 2) + Mathf.Pow(_y - endData.y, 2));
        }

        public void SetCost(int currentG)
        {
            _costG = currentG + 1;
        }
    }

    private void ShortestDistance()
    {
        bool isDone = false;

        List<Node> _nodes = new List<Node>(_blockData.Length);
        List<Node> _passNodes = new List<Node>();

        for (int i = 0; i < _blockData.Length; i++)
        {
            _nodes.Add(new Node(_blockData[i], _blockData[_mapData.exitBlockIndex]));
        }

        _nodes[_mapData.enterBlockIndex].SetCost(0);

        SearchAroundNonBlocker(_mapData.enterBlockIndex, ref _nodes, ref _passNodes, ref isDone);

        if(isDone == false)
        {
            CheckCloseNode(ref _passNodes);
        }
    }

    private void SearchAroundNonBlocker(int centerIndex, ref List<Node> nodes, ref List<Node> passNodes, ref bool isDone)
    {
        if (isDone == true)
        {
            return;
        }

        List<int> indexs = new List<int>(4);
        indexs.Add(centerIndex - 8);
        indexs.Add(centerIndex + 8);
        indexs.Add(centerIndex - 1);
        indexs.Add(centerIndex + 1);

        int minCostIndex = centerIndex;

        for (int i = 0; i < indexs.Count; i++)
        {
            if (0 > indexs[i])
            {
                continue;
            }

            if (nodes.Count - 1 < indexs[i])
            {
                continue;
            }

            nodes[indexs[i]].SetCost(0);

            if (nodes[minCostIndex].costF <= nodes[indexs[i]].costF)
            {
                continue;
            }

            minCostIndex = indexs[i];

            if (nodes[minCostIndex].isPass == false)
            {
                if (indexs[i] == _mapData.exitBlockIndex)
                {
                    isDone = true;

                    return;
                }

                nodes[minCostIndex].passIndex = minCostIndex;
                nodes[minCostIndex].isPass = true;
                passNodes.Add(nodes[minCostIndex]);
            }

            SearchAroundNonBlocker(minCostIndex, ref nodes, ref passNodes, ref isDone);
        }
    }

    private void CheckCloseNode(ref List<Node> passNodes)
    {
        for (int i = 0; i < passNodes.Count; i++)
        {
            List<int> indexs = new List<int>(4);
            indexs.Add(passNodes[i].passIndex - 8);
            indexs.Add(passNodes[i].passIndex + 8);
            indexs.Add(passNodes[i].passIndex - 1);
            indexs.Add(passNodes[i].passIndex + 1);

            for (int j = 0; j < indexs.Count; j++)
            {
                if (0 > indexs[j])
                {
                    continue;
                }

                if (_blockData.Length - 1 < indexs[j])
                {
                    continue;
                }

                if(_blockData[indexs[j]].isWalkAble == true)
                {
                    continue;
                }

                _blockData[indexs[j]].isWalkAble = Random.Range(0, 100) > 80 ? true : false;
            }

            _blockData[passNodes[i].passIndex].isWalkAble = true;
        }
    }

    private void GenerateCreature()
    {
        int maxMonsterCount = 3;

        if(_round > 10)
        {
            maxMonsterCount = 2;
        }

        Debug.Log("Select Monster /// TEST MODE");
        int minValue = _round - 3 < 0 ? 0 : _round - 3;
        int maxValue = _round + 1 > 10 ? 10 : _round + 1;

        int monsterCount = Random.Range(1, maxMonsterCount + 1);
        List<DataManager.Creature_Data> monsters = new List<DataManager.Creature_Data>(monsterCount);

        for (int i = 0; i < monsterCount; i++)
        {
            monsters.Add(GameManager.instance.dataManager.GetCreatureData(Random.Range(minValue, maxValue + 1)));
        }

        SelectMonsterPosition(ref monsters);
    }

    private void SelectMonsterPosition(ref List<DataManager.Creature_Data> monsters)
    {
        //_blockData
    }

    private void SetMapData()
    {
        for (int i = 0; i < _blockData.Length; i++)
        {
            _mapData.blockDatas.Add(new DataManager.Block_Data());

            _mapData.blockDatas[i].x = _blockData[i].x;
            _mapData.blockDatas[i].y = _blockData[i].y;
            _mapData.blockDatas[i].isWalkable = _blockData[i].isWalkAble;
            _mapData.blockDatas[i].isCreatureExist = _blockData[i].isCreatureExist;
        }

        _onResultCallback(_mapData);
    }
}