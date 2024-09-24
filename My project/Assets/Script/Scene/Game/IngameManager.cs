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
            }, 9, _saveData);

            _mapGenerator.Start();
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

        int nearbyBlockIndex = SearchNearbyBlock(type, _saveData.userData.data.currentNodeIndex);
        
        if(_saveData.mapData.nodeDatas[nearbyBlockIndex].isMonster == true)
        {
            _textView.UpdateText("몬스터를 만났습니다.");

            PlayerTurn(false);

            return;
        }

        _saveData.userData.data.currentNodeIndex = nearbyBlockIndex;
        _saveData.userData.data.ap -= 1;

        UpdateData();
    }

    private int SearchNearbyBlock(eControl type, int currentBlockIndex)
    {
        DataManager.Node_Data currentBlockData = _saveData.mapData.nodeDatas[currentBlockIndex];

        int n = 8;

        int X = currentBlockData.x;
        int Y = currentBlockData.y;

        int result = -1;

        switch (type)
        {
            case eControl.Up:
                int up = X + ((Y + 1) * n);
                CheckBlock(up, Y + 1, ref _saveData.mapData.nodeDatas, ref result);
                break;

            case eControl.Left:
                int left = X + X + (Y * n) - 1;
                CheckBlock(left, Y + 1, ref _saveData.mapData.nodeDatas, ref result);
                break;

            case eControl.Right:
                int right = X + (Y * n) + 1;
                CheckBlock(right, Y + 1, ref _saveData.mapData.nodeDatas, ref result);
                break;

            case eControl.Down:
                int down = X + ((Y - 1) * n);
                CheckBlock(down, Y + 1, ref _saveData.mapData.nodeDatas, ref result);
                break;
        }

        return result;
    }

    private void CheckBlock(int index, int Y, ref List<DataManager.Node_Data> blockDAtas, ref int result)
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
    private class Node
    {
        public bool isEnter = false;
        public bool isExit = false;

        public bool isVisit = false;
        public bool isWalkable = false;

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

    public MapGenerator(System.Action<DataManager.Map_Data> onResultCallback, int mapSize, DataManager.Save_Data saveData)
    {
        if(onResultCallback != null)
        {
            _onResultCallback = onResultCallback;
        }

        _mapSize = mapSize;
        _saveData = saveData;
    }

    public void Start()
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
        for (int i = 0; i < randomBlockerIndex.Count; i++)
        {
            List<int> NearbyMiddlePoints = NearbyMiddlePoint(middlePoints[randomBlockerIndex[i]]);

            if (_nodes[middlePoints[randomBlockerIndex[i]]].isWalkable == false)
            {
                continue;
            }

            bool isStuck = false;

            for (int n = 0; n < NearbyMiddlePoints.Count; n++)
            {
                if (_nodes[n].isWalkable == true)
                {
                    isStuck = false;
                    break;
                }
            }

            if (isStuck == true)
            {
                List<int> nearbyBlocks = GetNearbyBlocks_Diagonal(middlePoints[randomBlockerIndex[i]]);

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
        _saveData.mapData.nodeDatas = new List<DataManager.Node_Data>();

        for (int n = 0; n < _nodes.Count; n++)
        {
            DataManager.Node_Data node = new DataManager.Node_Data();
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