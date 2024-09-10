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

        _ingameUI.Initialize();
        _textView.Initialize();
        _control.Initialize(Move, Action);

        SetMap();
    }

    private void Move(eControl type)
    {

    }

    private void Action(eControl type)
    {

    }

    private void SetMap()
    {
        _mapGenerator = new MapGenerator(ASD);

        _mapGenerator.Start(_saveData.round);
    }

    private void ASD(DataManager.Map_Data mapData)
    {

    }
}

public class BlockData
{
    public ushort x = 0;
    public ushort y = 0;

    public bool isWalkAble = true;
    public bool isCreatureExist = false;

    public bool isEnter = false;
    public bool isExit = false;

    public ushort CreatureID = 0;
}

public class MapGenerator
{
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
        _blockData = new BlockData[64];
        _round = round;

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

        EnterExit();
    }

    private void EnterExit()
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

        for (int i = 0; i <= Random.Range(0, 2); i++)
        {
            Walkable();
        }

        ConnectToEnterAndExit();

        View3D();
    }

    private void Walkable()
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
            Walkable();
            return;
        }

        _blockData[index].isWalkAble = false;

        SearchAround(index, 2);
    }

    private void SearchAround(int centerIndex, int a)
    {
        if(a == 0)
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

            if(_blockData[indexs[i]].isEnter == true || _blockData[indexs[i]].isExit == true)
            {
                _blockData[centerIndex].isWalkAble = true;
                Walkable();

                return;
            }

            if(_blockData[i].isWalkAble == false)
            {
                continue;
            }

            _blockData[indexs[i]].isWalkAble = Random.Range(0, 100) > 80 ? true : false;

            SearchAround(indexs[i], a - 1);
        }
    }

    private void ConnectToEnterAndExit()
    {
        bool isConnect = true;

        for (int i = 0; i < _blockData.Length; i++)
        {
            //if()
        }
    }

    private void View3D()
    {
        // Wall
        for (int y = 0; y < 10; y++)
        {
            for (int x = 0; x < 10; x++)
            {
                if (y == 0 || y == 9)
                {
                    GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    obj.transform.position = new Vector3(x, 0, y);
                }

                if (x == 0 || x == 9)
                {
                    GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    obj.transform.position = new Vector3(x, 0, y);
                }
            }
        }

        //Game Station
        for (int i = 0; i < _blockData.Length; i++)
        {
            if (_blockData[i].isEnter == true)
            {
                continue;
            }

            if (_blockData[i].isExit == true)
            {
                continue;
            }

            if (_blockData[i].isWalkAble == false)
            {
                GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obj.transform.position = new Vector3(_blockData[i].x, 0, _blockData[i].y);
            }
            else
            {
                GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                obj.transform.position = new Vector3(_blockData[i].x, 0, _blockData[i].y);
            }
        }
    }
}