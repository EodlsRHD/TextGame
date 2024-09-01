using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitializeGame : MonoBehaviour
{
    [SerializeField] private int _targetFrameRate = 60;

    void Start()
    {
        Application.targetFrameRate = _targetFrameRate;

        SceneManager.LoadScene(SceneIndex.Lobby.ToString());
    }
}
