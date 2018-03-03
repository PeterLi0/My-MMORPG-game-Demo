using System;
using System.Collections.Generic;
using System.Timers;


public class Time
{
    public static uint AppStartTime = 0;

    public static uint deltaTime;

    public static void Init()
    {
        AppStartTime = (uint)Environment.TickCount;
    }

    /// <summary>
    /// 获取当前时间
    /// </summary>
    /// <returns></returns>
    public static uint time
    {
        get
        {
            return (uint)Environment.TickCount - AppStartTime;
        }
    }

    /// <summary>
    /// 获取时间增量
    /// </summary>
    /// <param name="oldTime"></param>
    /// <param name="newTime"></param>
    /// <returns></returns>
    public static uint GetMSTimeDiff(uint oldTime, uint newTime)
    {
        return newTime - oldTime;
    }
}
