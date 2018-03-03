using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 所有界面类的基类
/// </summary>
public abstract class BaseWnd
{
    protected Transform _transform;

    public string name;

    /// <summary>
    /// 打开窗口
    /// </summary>
    /// <param name="wndName"></param>
    public void Open(Transform canvas, string wndName)
    {
        name = wndName;
        _transform = (GameObject.Instantiate(Resources.Load("UI/" + wndName)) as GameObject).transform;
        _transform.parent = canvas;
        _transform.localPosition = Vector3.zero;
        _transform.localScale = Vector3.one;
        _transform.name = wndName;
    }

    /// <summary>
    /// 关闭窗口
    /// </summary>
    public virtual void Close()
    {
        if(_transform != null &&  _transform.gameObject != null)
            GameObject.Destroy(_transform.gameObject);
    }

    public virtual void Update(float dt) { }
}



public class WindowManager : Singleton<WindowManager>
{
    private Transform _canvas;

    // 保存所有的打开的窗口
    private Dictionary<string, BaseWnd> _windows = new Dictionary<string, BaseWnd>();

    public Camera uiCamera;

    /// <summary>
    /// 初始化
    /// </summary>
    public void Initialize()
    {
        MonoBehaviour.DontDestroyOnLoad(GameObject.Find("UI"));
        _canvas = GameObject.Find("UI/Canvas").transform;
        uiCamera = GameObject.Find("UI/Camera").GetComponent<Camera>();
    }

    /// <summary>
    /// 打开界面
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Open<T>() where T : BaseWnd, new()
    {
        string wndName = typeof(T).Name;
        if(_windows.ContainsKey(wndName))
        {
            return _windows[wndName] as T;
        }
        else
        {
            T wnd = new T();
            wnd.Open(_canvas, wndName);
            _windows.Add(wndName, wnd);
            return wnd;
        }
    }

    /// <summary>
    /// 关闭窗口 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public void Close<T>() where T : BaseWnd
    {
        string wndName = typeof(T).Name;
        if (_windows.ContainsKey(wndName))
        {
            _windows[wndName].Close();
            _windows.Remove(wndName);
        }
    }

    public T Get<T>() where T : BaseWnd
    {
        string wndName = typeof(T).Name;
        if (_windows.ContainsKey(wndName))
        {
            return _windows[wndName] as T;
        }
        else
        {
            return null;
        }
    }

    public void Update(float dt)
    {
        foreach(BaseWnd wnd in _windows.Values)
        {
            wnd.Update(dt);
        }
    }

    //public void Clear()
    //{
    //    _windows.Clear();
    //}

    public void CloseAll()
    {
        foreach (BaseWnd wnd in _windows.Values.ToArray())
        {
            wnd.Close();

            _windows.Remove(wnd.name);
        }
    }
}
