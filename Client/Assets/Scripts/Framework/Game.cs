using System;
using System.Collections.Generic;
using UnityEngine;

public enum SceneType
{
    Loading = 1,
    Login = 2,
    SelectRole = 3,
    Battle = 5
}

public class Game : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(this);
        ConfigManager.instance.Initialize(new ConfigParser());


        WindowManager.instance.Initialize();
        Net.instance.Initialize();

        Loading.instance.LoadScene("Login");
    }

    void OnLevelWasLoaded(int level)
    {
        if(level == (int)SceneType.Loading)
        {
            Loading.instance.Initialize();
        }
        else if(level == (int)SceneType.Login)
        {
            Login.instance.Initialize();
        }
        else if(level == (int)SceneType.SelectRole)
        {
            SelectRole.instance.Initialize();
        }
        else if(level >= (int)SceneType.Battle)
        {
            Battle.instance.Initialize();
        }
    }

    void Update()
    {
        float dt = Time.deltaTime;
        TimerMgr.instance.Update(dt);
        WindowManager.instance.Update(dt);

        //SkillManager.instance.Update(dt);
        Net.instance.Update();

        Battle.instance.Update(dt);
    }

    void OnApplicationQuit()
    {
        // 断开连接
        Net.instance.Disconnect();
    }
}