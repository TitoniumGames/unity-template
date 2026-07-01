using System.Collections;
using System.Collections.Generic;
using GameTemplate.Runtime.WGUI;
using GameTemplate.Runtime.WGUI.Demo;
using UnityEngine;

public class GameController : MonoBehaviour
{
    void Start()
    {
        UIManager.Instance.Show<LoadingScreen>();
    }
}
