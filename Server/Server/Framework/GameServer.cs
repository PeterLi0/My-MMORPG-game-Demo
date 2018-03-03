using System;
using System.Collections.Generic;
using System.Threading;
using log4net;
using log4net.Config;
using System.Reflection;
using System.IO;

public class GameServer
{  
    // 游戏循环线程
    private Thread _gameloopThread;

    private bool _gameloopRunning = false;

    private const int World_Sleep_Const = 50;

    private Time _timerHelper;

    public GameServer()
    {
        _timerHelper = new Time();
        Time.Init();

        XmlConfigurator.Configure();
        Type type = MethodBase.GetCurrentMethod().DeclaringType;
        ILog m_log = LogManager.GetLogger(type);

        m_log.Debug("这是一个Debug日志");
        m_log.Info("这是一个Info日志");
        m_log.Warn("这是一个Warn日志");
        m_log.Error("这是一个Error日志");
        m_log.Fatal("这是一个Fatal日志");


        // 载入配置文件
        ConfigManager.instance.Initialize(new ConfigParser());
        
        // 连接到数据库
        MysqlManager.instance.Connect();

        // 服务器初始化
        NetworkManager ss = new NetworkManager(9000, new HandlerCenter());
        ss.Start(6650);


        // 初始化所有世界场景
        Dictionary<int, SceneCfg> sceneCfgs = ConfigManager.instance.GetAllScenes();
        SceneManager.instance.Initialize(sceneCfgs);


        _gameloopRunning = true;
        _gameloopThread = new Thread(Run);
        _gameloopThread.Start();
    }


    private void Tick(float dt)
    {
        SceneManager.instance.Update(dt);
        //ArenaManager.instance.Update(dt);
        //BattleGroundManager.instance.Update(dt);
        //DungeonManager.instance.Update(dt);
    }


    private void Run()
    {
        // 当前时间
        uint realCurrTime = 0;

        // 上一次更新开始的时间
        uint realPrevTime = Time.time;

        // 上一次更新Sleep的时间
        uint prevSleepTime = 0;

        while (_gameloopRunning)
        {
            // 获取当前时间
            realCurrTime = Time.time;

            // 更新增量时间
            uint diff = Time.GetMSTimeDiff(realPrevTime, realCurrTime);
            Time.deltaTime = diff;

            float framedt = diff / 1000f;

            Tick(framedt);


            realPrevTime = realCurrTime;

            if (diff <= World_Sleep_Const + prevSleepTime)
            {
                prevSleepTime = World_Sleep_Const + prevSleepTime - diff;
                Thread.Sleep((int)prevSleepTime);
            }
            else
            {
                prevSleepTime = 0;
            }
        }
    }
}
