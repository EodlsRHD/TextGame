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
    private CreatureGenerator _creatureGenerator = null;

    private bool _isPlayerTurn = false;
    private bool _isAllMonsterDead = true;

    void Start()
    {
        _saveData = GameManager.instance.dataManager.CopySaveData();

        _ingameUI.Initialize(OpenMap, OpenNextRound);
        _textView.Initialize();
        _controlPad.Initialize(Move, Action);
        _mapController.Initialize(_saveData.mapData.mapSize);
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
            
            _mapGenerator = new MapGenerator(GenerateMap, _saveData);
        }); 
    }

    private void GenerateMap(DataManager.Map_Data mapData)
    {
        _saveData.mapData = mapData;
        
        RoundSet();
    }
     
    private void RoundSet()
    {
        _ingameUI.SetRoundText(_saveData.round);
        _mapController.SetMap(_saveData);

        _isPlayerTurn = true;

        if (_saveData.mapData.monsterDatas.Count > 0)
        {
            _isAllMonsterDead = false;
        }
    }

    private void Move(eControl type)
    {
        if(_isPlayerTurn == false)
        {
            _ingamePopup.UpdateText(_saveData.userData.data.name + "의 순서가 아닙니다.");

            return;
        }

        if (_saveData.userData.data.ap <= 0)
        {
            _ingamePopup.UpdateText("ap가 부족합니다.");

            return;
        }

        int nearbyBlockIndex = MoveType(type, _saveData.userData.data.currentNodeIndex);
        Debug.LogError(nearbyBlockIndex);
        if (nearbyBlockIndex == -1) // -1 : out of map
        {
            return;
        }

        if (nearbyBlockIndex == -2) // -2 : blocker
        {
            _ingamePopup.UpdateText("이동할 수 없습니다.");

            return;
        }

        if(nearbyBlockIndex == -3) // -3 : monster
        {
            _textView.UpdateText("몬스터를 만났습니다.");

            return;
        }

        if (nearbyBlockIndex == -3) // -4 : exit
        {
            _textView.UpdateText("출구입니다.");

            return;
        }

        _saveData.mapData.nodeDatas[_saveData.userData.data.currentNodeIndex].isUser = false;
        _saveData.mapData.nodeDatas[nearbyBlockIndex].isUser = true;

        _saveData.userData.data.currentNodeIndex = nearbyBlockIndex;
        _saveData.userData.data.ap -= 1;

        UpdateData();
    }

    private void Action(eControl type)
    {

    }

    private void UpdateData()
    {
        _mapController.UpdateData(_saveData);
    }

    private void MonsterTurn()
    {
        if(_isAllMonsterDead == true)
        {
            return;
        }

        List<DataManager.Creature_Data> monsters = _saveData.mapData.monsterDatas;
    }

    // 만난 적, 획득한 아이템 저장 하는 기능 필요

    private int MoveType(eControl type, int currentIndex)
    {
        int x = 0;
        int y = 0;

        switch (type)
        {
            case eControl.Up:
                x = 0;
                y = 1;
                break;

            case eControl.Left:
                x = -1;
                y = 0;
                break;

            case eControl.Right:
                x = 1;
                y = 0;
                break;

            case eControl.Down:
                x = 0;
                y = -1;
                break;
        }

        int result = GetNearbyBlocks(x, y, currentIndex);

        if (result == -1)
        {
            return -1;
        }

        if (_saveData.mapData.nodeDatas[result].isWalkable == false)
        {
            return -2;
        }

        if (_saveData.mapData.nodeDatas[result].isMonster == false)
        {
            return -3;
        }

        if (_saveData.mapData.exitNodeIndex == result)
        {
            return -4;
        }

        return result;
    }

    private int GetNearbyBlocks(int x, int y, int index)
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
}

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

        public Node(int index, int x, int y, DataManager.Node_Data enterBlock, DataManager.Node_Data exitBlock)
        {
            _index = index;
            _x = x;
            _y = y;

            //_g = Mathf.Abs(Mathf.)
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
                Node node = new Node((y * _mapSize) + x, x, y, null, null);

                _nodes.Add(node);
            }
        }

        GenerateBlocker();
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

        CheckStuckBlocker(ref middlePoints, ref randomBlockerIndexs);
        SelectEnterBlock(ref middlePoints);
    }

    private void CheckStuckBlocker(ref List<int> middlePoints, ref List<int> randomBlockerIndex)
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

        _nodes[openMiddlePoints[Random.Range(0, openMiddlePoints.Count)]].isEnter = true;
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

                if(0 <= resultIndex && resultIndex < (_mapSize * _mapSize))
                {
                    result.Add(resultIndex);
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
        GenerateMonster();
        GeneratePlayer();
        Done();
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

        int monsterSpawnCount = Random.Range(2, openNodeCount / _saveData.mapData.mapSize);

        for (int i = 0; i < monsterSpawnCount; i++)
        {
            DataManager.Node_Data node = new DataManager.Node_Data();
            SpawnMonsterNodeSelect(Random.Range(0, _saveData.mapData.nodeDatas.Count), ref node);

            DataManager.Creature_Data creature = GameManager.instance.dataManager.GetCreatureData(_saveData.round + i);
            creature.currentNodeIndex = node.index;
            _saveData.mapData.nodeDatas[node.index].isMonster = true; 

            MonsterStats(ref creature);

            _saveData.mapData.monsterDatas.Add(creature);
        }
    }

    private void MonsterStats(ref DataManager.Creature_Data creature)
    {
        float increasePoint = (_saveData.round * 0.1f);

        creature.coin *= (ushort)increasePoint;
        creature.hp *= (ushort)increasePoint;
        creature.exp *= (ushort)increasePoint;
        creature.attack *= (ushort)increasePoint;
        creature.defence *= (ushort)increasePoint;
    }

    private void GeneratePlayer()
    {
        _saveData.userData.data.currentNodeIndex = _saveData.mapData.enterNodeIndex;
        _saveData.mapData.nodeDatas[_saveData.userData.data.currentNodeIndex].isUser = true;
    }

    private void Done()
    {
        _onResultCallback?.Invoke(_saveData.userData.data, _saveData.mapData.monsterDatas);
    }

    private void SpawnMonsterNodeSelect(int index, ref DataManager.Node_Data node)
    {
        if (_saveData.mapData.nodeDatas[index].isWalkable == true && _saveData.mapData.nodeDatas[index].isMonster == false)
        {
            node = _saveData.mapData.nodeDatas[index];

            return;
        }

        int newIndex = 0;

        if(_saveData.mapData.nodeDatas[index].isWalkable == false)
        {
            newIndex = index + 1;
        }
        else if(_saveData.mapData.nodeDatas[index].isMonster == true)
        {
            newIndex = index + 5;
        }

        if(newIndex >= _saveData.mapData.nodeDatas.Count)
        {
            newIndex -= _saveData.mapData.nodeDatas.Count;
        }

        SpawnMonsterNodeSelect(newIndex, ref node);
    }
}