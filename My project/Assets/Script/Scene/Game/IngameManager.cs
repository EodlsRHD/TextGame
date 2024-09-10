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

        if(isX == true)
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

        Walkable();
    }

    private void Walkable()
    {
        for (int y = 1; y < 9; y++)
        {
            for (int x = 1; x < 9; x++)
            {
                int coord = (x + (y - 1) * 8) - 1;

                GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                obj.transform.position = new Vector3(x, 0, y);

                if (_blockData[coord].isEnter == false && _blockData[coord].isExit == false)
                {
                    if (y == 1 || y == 8)
                    {
                        obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        obj.transform.position = new Vector3(x, 0, y);

                        _blockData[coord].isWalkAble = false;
                    }

                    if (x == 1 || x == 8)
                    {
                        obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                        obj.transform.position = new Vector3(x, 0, y);

                        _blockData[coord].isWalkAble = false;
                    }
                }
            }
        }
    }
}