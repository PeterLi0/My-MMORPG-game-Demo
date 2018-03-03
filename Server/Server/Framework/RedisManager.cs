using ServiceStack.Redis;
using ServiceStack.Redis.Support;
using ServiceStack.Text;
using System.Collections.Generic;
using System;


class RedisManager
{
    private RedisClient _rc;

    public void Init()
    {
        _rc = new RedisClient("127.0.0.1");
    }

    /// <summary>
    /// 判断某个数据是否已经被缓存
    /// </summary>
    public bool Exist<T>(int hashId, int key)
    {
        return _rc.HashContainsEntry(hashId.ToString(), key.ToString());
    }

    /// <summary>
    /// 存储数据到hash表
    /// </summary>
    private bool Set<T>(string hashId, string key, T t)
    {
        ObjectSerializer ser = new ObjectSerializer();
        byte[] d = ser.Serialize(t);
        string value = JsonSerializer.SerializeToString<byte[]>(d);
        return _rc.SetEntryInHash(hashId, key, value);
    }

    public bool Set<T>(int hashId, int key, T t)
    {
        return Set<T>(hashId.ToString(), key.ToString(), t);
    }

    /// <summary>
    /// 移除hash中的某值
    /// </summary>
    public bool Remove(int hashId, int key)
    {
        return _rc.RemoveEntryFromHash(hashId.ToString(), key.ToString());
    }

    /// <summary>
    /// 移除整个hash
    /// </summary>
    public bool Remove(int key)
    {
        return _rc.Remove(key.ToString());
    }

    /// <summary>
    /// 从hash表获取数据
    /// </summary>
    public T Get<T>(int hashid, int key) where T : class
    {
        string value = _rc.GetValueFromHash(hashid.ToString(), key.ToString());
        byte[] data = JsonSerializer.DeserializeFromString<byte[]>(value);

        ObjectSerializer ser = new ObjectSerializer();
        return ser.Deserialize(data) as T;
    }

    /// <summary>
    /// 获取整个hash的数据
    /// </summary>
    public List<T> GetAll<T>(int hashId) where T : class
    {
        List<T> result = new List<T>();
        List<string> list = _rc.GetHashValues(hashId.ToString());

        if (list != null && list.Count > 0)
        {
            for (int i = 0; i < list.Count; i++)
            {
                byte[] data = JsonSerializer.DeserializeFromString<byte[]>(list[i]);
                ObjectSerializer ser = new ObjectSerializer();

                result.Add(ser.Deserialize(data) as T);
            }
        }
        return result;
    }

    /// <summary>
    /// 设置缓存过期
    /// </summary>
    public void SetExpire(string key, DateTime datetime)
    {
        _rc.ExpireEntryAt(key, datetime);
    }

    /// <summary>
    /// 保存数据DB文件到硬盘
    /// </summary>
    public void Save()
    {
        _rc.Save();
    }
    /// <summary>
    /// 异步保存数据DB文件到硬盘
    /// </summary>
    public void SaveAsync()
    {
        _rc.SaveAsync();
    }
}
