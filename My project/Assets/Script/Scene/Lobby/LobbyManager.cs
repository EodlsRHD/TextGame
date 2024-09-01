using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    private void Start()
    {
        GameManager.instance.tools.Fade(true, Result);
    }

    private void Result()
    {
        Debug.Log("Manager is HERE!");
    }
}
