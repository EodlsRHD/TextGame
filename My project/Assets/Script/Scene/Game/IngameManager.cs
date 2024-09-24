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
    private class Node
    {
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

        public Node(int index, int x, int y, DataManager.Block_Data enterBlock, DataManager.Block_Data exitBlock)
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
        SelectEnterBlock();
        SelectExitBlock();
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

        List<int> ranbomBlockerIndexs = RandomIndex(middlePoints.Count, 3);

        for (int r = 0; r < ranbomBlockerIndexs.Count; r++)
        {
            Debug.LogWarning(middlePoints[ranbomBlockerIndexs[r]]);

            List<int> nearbyBlocks = GetNearbyBlocks_Diagonal(middlePoints[ranbomBlockerIndexs[r]]);

            for (int n = 0; n < nearbyBlocks.Count; n++)
            {
                _nodes[nearbyBlocks[n]].isWalkable = false;
            }
        }
    }

    private void SelectEnterBlock()
    {

    }

    private void SelectExitBlock()
    {
        float centerIndex = (Mathf.Pow(_mapSize, 2) - 1) * 0.5f;
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

                 result.Add(resultIndex);
            }
        }

        return result;
    }
}