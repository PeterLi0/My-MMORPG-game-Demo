using UnityEngine;
using System.Collections.Generic;
using System;

public class Timer
{
    // 延迟时间
    private float _delay;

    // 已经过去的时间
    private float _elapsedTime = 0f;

    // 延迟事件
    private Action _action;

    // 计时器标记
    private bool _end = false;

    // 是否重复
    private bool _repeat = false;

    public bool end { get { return _end; } }

    public Timer(float delay, Action act, bool repeat = false)
    {
        _delay = delay;
        _action = act;
        _repeat = repeat;
    }
    public void Update(float dt)
    {
        if(_elapsedTime >= _delay)
        {
            if(_repeat)
            {
                _action();
                _elapsedTime = _elapsedTime - _delay;
            }
            else
            {
                _end = true;
                _action();
            }
        }
        _elapsedTime += dt;
    }
}

public class TimerMgr : Singleton<TimerMgr>
{
    private List<Timer> _timers = new List<Timer>();

    public void Invoke(float delay, Action act, bool repeat = false)
    {
        _timers.Add(new Timer(delay, act, repeat));
    }

    public void Update(float dt)
    {
        for(int i = 0; i < _timers.Count; i++)
        {
            Timer timer = _timers[i];
            if (timer.end)
                _timers.Remove(timer);
            else
                timer.Update(dt);
        }
    }

    public void Clear()
    {
        _timers.Clear();
    }
}