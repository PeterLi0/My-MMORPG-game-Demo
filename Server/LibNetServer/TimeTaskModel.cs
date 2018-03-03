using System;
using System.Collections.Generic;

public class TimeTaskModel
{
    // 任务逻辑
    private TimeEvent execut;

    // 任务执行的时间
    public long time;

    // 任务ID
    public int id;

    public TimeTaskModel(int id, TimeEvent execut, long time)
    {
        this.id = id;
        this.execut = execut;
        this.time = time;
    }
    public void run()
    {
        execut();
    }
}