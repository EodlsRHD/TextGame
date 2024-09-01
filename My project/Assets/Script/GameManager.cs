using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance = null;
    public static GameManager instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = new GameManager();
            }

            return _instance;
        }
    }

    [Header("Tool Proxy"), SerializeField] private ToolProxy _toolProxy = null;

    #region GetSet

    public ToolProxy tools
    {
        get { return _toolProxy; }
    }

    #endregion

    private void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(this.gameObject);

        _toolProxy.Initialize();
    }
}
