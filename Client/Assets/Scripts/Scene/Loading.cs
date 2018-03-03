using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;


public class Loading : Singleton<Loading>, IScene
{
    private string _targetScene;

    public void Initialize()
    {
        WindowManager.instance.Open<LoadingWnd>().Initialize();
        TimerMgr.instance.Invoke(0.5f, () => {
            Finalise();
            SceneManager.LoadScene(_targetScene);
        });
    }

    public void Finalise()
    {
        WindowManager.instance.Close<LoadingWnd>();
    }

    public void LoadScene(string targetScene)
    {
        _targetScene = targetScene;

        SceneManager.LoadScene("Loading");
    }
}