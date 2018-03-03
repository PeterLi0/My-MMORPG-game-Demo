using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对象池
/// </summary>
public class Pool
{
    // 池中可用的对象
    private List<GameObject> _avaliables = new List<GameObject>();

    /// <summary>
    /// 创建一个对象
    /// </summary>
    public GameObject Spawn(string path, string name)
    {
        GameObject go = null;
        if(_avaliables.Count <= 0)
        {
            go = GameObject.Instantiate(Resources.Load(path + name)) as GameObject;
            go.name = name;
        }
        else
        {
            go = _avaliables[0];
            go.SetActive(true);
            _avaliables.Remove(go);
        }

        return go;
    }

    /// <summary>
    /// 回收一个对象
    /// </summary>
    public void Unspawn(GameObject go)
    {
        go.transform.position = new Vector3(10000, 10000, 10000);
        go.SetActive(false);
        _avaliables.Add(go);
    }

    public void Clear()
    {
        _avaliables.Clear();
    }
}

/// <summary>
/// 对象池的管理器
/// </summary>
public class PoolManager : Singleton<PoolManager>
{
    // 存储所有对象池 key-GameObject的名称, value-对象池
    private Dictionary<string, Pool> _pools = new Dictionary<string, Pool>();

    /// <summary>
    /// 创建一个对象
    /// </summary>
    /// <param name="path"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public GameObject Spawn(string path, string name)
    {
        GameObject go = null;
        if (_pools.ContainsKey(name))
        {
            go = _pools[name].Spawn(path, name);
        }
        else
        {
            Pool pool = new Pool();
            go = pool.Spawn(path, name);

            _pools.Add(name, pool);
        }

        return go;
    }

    /// <summary>
    /// 回收一个对象
    /// </summary>
    /// <param name="go"></param>
    public void Unspawn(GameObject go)
    {
        _pools[go.name].Unspawn(go);
    }

    /// <summary>
    /// 清空所有对象池和对象池中的元素
    /// </summary>
    public void Clear()
    {
        foreach(Pool pool in _pools.Values)
        {
            pool.Clear();
        }
    }
}