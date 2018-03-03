using System;
using System.Collections.Generic;
using UnityEngine;

//#if !UNITY_EDITOR
public class LogManager : MonoBehaviour
{
    private static Queue<object> _textQueue = new Queue<object>();

    void OnGUI()
    {
        int i = 0;
        foreach (object str in _textQueue)
        {
            GUI.Label(new Rect(10, 10 + (i * 25), 1000, 22), str.ToString());
            i++;
        }
    }

    private static void Add(object text)
    {
        _textQueue.Enqueue(text);

        TimerMgr.instance.Invoke(3.0f, () =>
        {
            _textQueue.Dequeue();
        });
    }


    public static void Log(object text, UnityEngine.Object obj = null, bool onScreen = true)
    {
        if (onScreen)
            Add(text);
        UnityEngine.Debug.Log(text, obj);
    }

    public static void LogWarning(object text, UnityEngine.Object obj = null)
    {
        Add(text);
        UnityEngine.Debug.LogWarning(text, obj);
    }

    public static void LogException(object text, UnityEngine.Object obj = null)
    {
        Add(text);

        //UnityEngine.Debug.LogException(text, obj);
    }

    public static void LogError(object text, UnityEngine.Object obj = null)
    {
        Add(text);
        UnityEngine.Debug.LogError(text, obj);
    }

    public static void LogFormat(string format, params object[] args)
    {
        Add(format);
    }


    public static void LogWarningFormat(string format, params object[] args)
    {
        Add(format);
    }

    public static void LogErrorFormat(string format, params object[] args)
    {
        Add(format);
    }

}
//#endif