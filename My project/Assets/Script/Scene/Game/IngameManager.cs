using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameManager : MonoBehaviour
{
    [Header("Ingame UI"), SerializeField] private IngameUI _ingameUI = null;
    [Header("TextView"), SerializeField] private TextView _textView = null;
    [Header("Control"), SerializeField] private ControlPad _controlPad = null;
    [Header("MapController"), SerializeField] private MapController _mapController = null;
    [Header("Ingame Popup"), SerializeField] private IngamePopup _ingamePopup = null;

    private DataManager.Save_Data _saveData = null;
    private MapGenerator _mapGenerator = null;

    private bool _isPlayerTurn = false;
    private bool _isMonsterDead = true;

    void Start()
    {
        _saveData = GameManager.instance.dataManager.CopySaveData();

        _ingameUI.Initialize(OpenMap, OpenNextRound);
        _textView.Initialize();
        _controlPad.Initialize(Move, Action);
        _mapController.Initialize();
        _ingamePopup.Initialize();

        _ingameUI.OpenNextRoundWindow(eRoundClear.First);

        this.gameObject.SetActive(true);
    }

    private void OpenMap(System.Action onCallback)
    {
        _mapController.OpenMap(onCallback);
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

        GameManager.instance.dataManager.SaveDataToCloud(_saveData, () => 
        {
            _saveData.round++;
            _ingameUI.SetRoundText(_saveData.round);

            _mapGenerator = new MapGenerator((mapData) => 
            {
                _saveData.mapData = mapData;
                _mapController.SetMap(_saveData);
                
                if(_saveData.mapData.monsterDatas.Count > 0)
                {
                    _isMonsterDead = false;
                }
            });

            _mapGenerator.Start(_saveData.round);
        }); 
    }

    private void Move(eControl type)
    {
        if(_isPlayerTurn == false)
        {
            _ingamePopup.UpdateText(_saveData.userData.data.name + "의 순서가 아닙니다.");

            return;
        }

        if (_saveData.userData.data.ap >= 0)
        {
            _ingamePopup.UpdateText("ap가 부족합니다.");

            return;
        }

        int nearbyBlockIndex = SearchNearbyBlock(type, _saveData.userData.data.currentBlockIndex);
        
        if(_saveData.mapData.blockDatas[nearbyBlockIndex].isMonster == true)
        {
            _textView.UpdateText("몬스터를 만났습니다.");

            PlayerTurn(false);

            return;
        }

        _saveData.userData.data.currentBlockIndex = nearbyBlockIndex;
        _saveData.userData.data.ap -= 1;

        UpdateData();
    }

    private int SearchNearbyBlock(eControl type, int currentBlockIndex)
    {
        DataManager.Block_Data currentBlockData = _saveData.mapData.blockDatas[currentBlockIndex];

        int n = 8;

        int X = currentBlockData.x;
        int Y = currentBlockData.y;

        int result = -1;

        switch (type)
        {
            case eControl.Up:
                int up = X + ((Y + 1) * n);
                CheckBlock(up, Y + 1, ref _saveData.mapData.blockDatas, ref result);
                break;

            case eControl.Left:
                int left = X + X + (Y * n) - 1;
                CheckBlock(left, Y + 1, ref _saveData.mapData.blockDatas, ref result);
                break;

            case eControl.Right:
                int right = X + (Y * n) + 1;
                CheckBlock(right, Y + 1, ref _saveData.mapData.blockDatas, ref result);
                break;

            case eControl.Down:
                int down = X + ((Y - 1) * n);
                CheckBlock(down, Y + 1, ref _saveData.mapData.blockDatas, ref result);
                break;
        }

        return result;
    }

    private void CheckBlock(int index, int Y, ref List<DataManager.Block_Data> blockDAtas, ref int result)
    {
        if (0 < index && index < blockDAtas.Count)
        {
            if(Y == blockDAtas.Count)
            {
                if(blockDAtas[index].isWalkable == true)
                {
                    result = index;
                }
            }
        }
    }

    private void Action(eControl type)
    {

    }

    private void UpdateData()
    {
        _mapController.UpdateData(_saveData);
    }

    private void PlayerTurn(bool isTurn)
    {
        _isPlayerTurn = isTurn;
    }

    private void MonsterTurn()
    {
        if(_isMonsterDead == true)
        {
            return;
        }

        List<DataManager.Creature_Data> monsters = _saveData.mapData.monsterDatas;

        PlayerTurn(true);
    }

    // 만난 적, 획득한 아이템 저장 하는 기능 필요
}

public class MapGenerator
{
    public class BlockData
    {
        public ushort x = 0;
        public ushort y = 0;

        public bool isWalkable = true;
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
                _blockData[coord].x = (ushort)(x - 1);
                _blockData[coord].y = (ushort)(y - 1);
            }
        }

        SpawnDoorway();

        int blocker = Random.Range(0, 3);
        for (int i = 0; i <= blocker; i++)
        {
            SpawnBlocker();
        }

        ShortestDistance();
    }

    private void SpawnDoorway()
    {
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

        if (_blockData[index].isWalkable == false)
        {
            SpawnBlocker();
            return;
        }

        _blockData[index].isWalkable = false;

        SpawnBlocker_SearchAround(index, 2);
    }

    private List<int> GetNearbyBlocks(int centerIndex, List<Node> nodes = null)
    {
        if(0 > centerIndex && centerIndex > _blockData.Length)
        {
            return null;
        }

        List<int> result = new List<int>();
        BlockData centerBlock = _blockData[centerIndex];

        int n = 8; 

        int X = centerBlock.x;
        int Y = centerBlock.y;

        int coordUp = X + ((Y + 1) * n); // 위
        if (coordUp < _blockData.Length && coordUp > 0)
        {
            if(Y + 1 == _blockData[coordUp].y)
            {
                if (nodes != null)
                {
                    if (nodes[coordUp].isWalkable == true)
                    {
                        if(nodes[coordUp].isPass == false)
                        {
                            result.Add(coordUp);
                        }
                    }
                }
                else
                {
                    result.Add(coordUp);
                }
            }
        }

        int coordLeft = X + (Y * n) - 1; // 좌
        if (coordLeft < _blockData.Length && coordLeft > 0)
        {
            if(Y == _blockData[coordLeft].y)
            {
                if (nodes != null)
                {
                    if (nodes[coordLeft].isWalkable == true)
                    {
                        if (nodes[coordLeft].isPass == false)
                        {
                            result.Add(coordLeft);
                        }
                    }
                }
                else
                {
                    result.Add(coordLeft);
                }
            }
        }


        int coordRight = X + (Y * n) + 1; // 우
        if (coordRight < _blockData.Length && coordRight > 0)
        {
            if(Y == _blockData[coordRight].y)
            {
                if (nodes != null)
                {
                    if (nodes[coordRight].isWalkable == true)
                    {
                        if (nodes[coordRight].isPass == false)
                        {
                            result.Add(coordRight);
                        }
                    }
                }
                else
                {
                    result.Add(coordRight);
                }
            }
        }



        int coordDown = X + ((Y - 1) * n); // 아래
        if (coordDown > 0)
        {
            if (Y - 1 == _blockData[coordDown].y)
            {
                if (nodes != null)
                {
                    if (nodes[coordDown].isWalkable == true)
                    {
                        if (nodes[coordDown].isPass == false)
                        {
                            result.Add(coordDown);
                        }
                    }
                }
                else
                {
                    result.Add(coordDown);
                }
            }
        }

        return result;
    }

    private void SpawnBlocker_SearchAround(int centerIndex, int a)
    {
        if (a == 0)
        {
            return;
        }

        List<int> indexs = GetNearbyBlocks(centerIndex, null);

        if (indexs == null)
        {
            return;
        }

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

            if (_blockData[indexs[i]].isWalkable == false)
            {
                continue;
            }

            if (_blockData[indexs[i]].isEnter == true || _blockData[indexs[i]].isExit == true)
            {
                _blockData[centerIndex].isWalkable = true;

                continue;
            }

            _blockData[indexs[i]].isWalkable = Random.Range(0, 4) > 2 ? true : false;

            SpawnBlocker_SearchAround(indexs[i], a - 1);
        }
    }

    public class Node
    {
        public bool isPass = false;
        public bool isWalkable = false;

        public int passIndex = 0;
        public int _x = 0;
        public int _y = 0;

        public int _costG = 0;
        public int _costH = 0;

        public int costF
        {
            get { return _costG + _costH; }
        }

        public Node(BlockData data, BlockData startData, BlockData endData)
        {
            isWalkable = data.isWalkable;

            _x = data.x;
            _y = data.y;

            _costG = (int)(Mathf.Abs(Mathf.Sqrt(Mathf.Pow(_x - startData.x, 2) + Mathf.Pow(_y - startData.y, 2))) * 10);
            _costH = (int)(Mathf.Abs(Mathf.Sqrt(Mathf.Pow(_x - endData.x, 2) + Mathf.Pow(_y - endData.y, 2))) * 10); 
        }
    }

    private void ShortestDistance()
    {
        bool isDone = false;
        int count = 0;

        List<Node> _nodes = new List<Node>(_blockData.Length);
        List<int> _passNodes = new List<int>();
        _passNodes.Add(_mapData.enterBlockIndex);

        for (int i = 0; i < _blockData.Length; i++)
        {
            _nodes.Add(new Node(_blockData[i], _blockData[_mapData.enterBlockIndex], _blockData[_mapData.exitBlockIndex]));
        }

        PathFinding_aStar(_mapData.enterBlockIndex, ref _nodes, ref _passNodes, ref isDone, ref count);

        CheckStuckNode();
    }

    private void PathFinding_aStar(int centerIndex, ref List<Node> nodes, ref List<int> passNodes, ref bool isDone, ref int count)
    {
        if (isDone == true)
        {
            return;
        }

        if(count > 50)
        {
            isDone = false;

            return;
        }
        
        count++;

        List<int> indexs = GetNearbyBlocks(centerIndex, nodes);

        if (indexs == null)
        {
            return;
        }

        if (indexs.Count == 0)
        {
            return;
        }

        int minCostIndex = indexs[0];

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

            if (indexs[i] == _mapData.enterBlockIndex)
            {
                continue;
            }

            if (indexs[i] == _mapData.exitBlockIndex)
            {
                isDone = true;
                passNodes.Add(_mapData.exitBlockIndex);

                break;
            }

            if (nodes[indexs[i]]._costH < nodes[minCostIndex]._costH)
            {
                if (indexs[i] == _mapData.exitBlockIndex)
                {
                    isDone = true;
                    passNodes.Add(_mapData.exitBlockIndex);

                    break;
                }

                minCostIndex = indexs[i];
            }
        }

        nodes[minCostIndex].isPass = true;
        nodes[minCostIndex].passIndex = minCostIndex;

        foreach (var item in passNodes)
        {
            if(item == minCostIndex)
            {
                break;
            }
            if(minCostIndex == _mapData.enterBlockIndex)
            {
                passNodes = new List<int>();
            }

            passNodes.Add(minCostIndex);
            break;
        }

        if(isDone == true)
        {
            return;
        }

        PathFinding_aStar(minCostIndex, ref nodes, ref passNodes, ref isDone, ref count);
    }

    private void CheckStuckNode()
    {
        for (int i = 0; i < _blockData.Length; i++)
        {
            if(_blockData[i].isWalkable == false)
            {
                continue;
            }

            List<int> indexs = GetNearbyBlocks(i);

            if(indexs == null)
            {
                return;
            }

            bool isNotStuck = false;

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

                if(_blockData[i].isWalkable == false)
                {
                    continue;
                }

                isNotStuck = true;
            }

            if (isNotStuck == false)
            {
                _blockData[i].isWalkable = false;
            }
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
            _mapData.blockDatas[i].isWalkable = _blockData[i].isWalkable;
            _mapData.blockDatas[i].isMonster = _blockData[i].isCreatureExist;
        }

        _onResultCallback(_mapData);
    }
}