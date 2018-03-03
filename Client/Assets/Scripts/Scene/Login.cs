using System;
using System.Collections.Generic;


public class Login : Singleton<Login>, IScene
{
    public void Initialize()
    {
        WindowManager.instance.Open<LoginWnd>().Initialize();
    }
    public void Finalise()
    {
        WindowManager.instance.Close<LoginWnd>();
    }
}