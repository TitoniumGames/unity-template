using System.Collections;
using System.Collections.Generic;
using GameTemplate.Runtime.WGUI;
using GameTemplate.Runtime.WGUI.Demo;
using UnityEngine;
using UnityEngine.UI;

public class PlayScreen : MonoBehaviour
{
    public Button setting;

    void Start()
    {
        setting.onClick.AddListener(() =>
        {
            UIManager.Instance.Show<SettingScreen>();
        });
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.N))
        {
            NotficationHelper.Show("This is a notification message!");
        }
    }
}
