

using System;
using System.Collections.Generic;

[Serializable]
public class AccountData
{
    public int id;
    public string account;
    public string password;

    public AccountData()
    {

    }
    public AccountData(int id, string account, string password)
    {
        this.id = id;
        this.account = account;
        this.password = password;
    }
}


/// <summary>
/// 账号
/// </summary>
public class Account
{
    /// <summary>
    /// 唯一ID
    /// </summary>
    public uint uid;

    public string accountName;

    // 所在匹配
    public uint globalMatchID;

    // 匹配的索引
    public int matchIndex = -1;

    // 阵营
    public common.Race roleSide;

    // 是否准备完成
    public bool ready = false;

    // 所在关卡的唯一ID
    public uint globalLevelID;

    // 关卡中的角色唯一ID
    public uint globalRoleID;

    /// <summary>
    /// 选择的英雄ID
    /// </summary>
    public uint heroID;


    public UserToken client;
}

public partial class RedisCacheManager
{
    /// <summary>
    /// 判断账号是否在线
    /// </summary>
    /// <returns></returns>
    public bool IsAccountOnline(int accountid)
    {
        return _redis.Exist<AccountData>(accountid, accountid);
    }

    /// <summary>
    /// 账号上线
    /// </summary>
    /// <param name="account"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public void AccountOnline(int id,  string account, string password)
    {
        _redis.Set(id, id, new AccountData(id, account, password));
    }

    /// <summary>
    /// 账号下线
    /// </summary>
    /// <param name="account"></param>
    public void AccountOffline(int accountid)
    {
        _redis.Remove(accountid);
    }

    /// <summary>
    /// 根据id获取已登录的角色数据
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public AccountData GetAccount(int id)
    {
        return _redis.Get<AccountData>(id, id);
    }

    public List<AccountData> GetAllAccount(int id)
    {
        return _redis.GetAll<AccountData>(id);
    }
}


partial class CacheManager : Singleton<CacheManager>
{
    // 保存所有的已经登录账号信息
    private Dictionary<int, AccountData> _accounts = new Dictionary<int, AccountData>();


    /// <summary>
    /// 判断账号是否在线
    /// </summary>
    /// <returns></returns>
    public bool IsAccountOnline(int accountid)
    {
        return _accounts.ContainsKey(accountid);
    }

    /// <summary>
    /// 账号上线
    /// </summary>
    /// <param name="account"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public void AccountOnline(int id, string account, string password)
    {
        AccountData data = new AccountData();
        data.id = id;
        data.account = account;
        data.password = password;

        _accounts.Add(id, data);
    }

    /// <summary>
    /// 账号下线
    /// </summary>
    /// <param name="account"></param>
    public void AccountOffline(int accountid)
    {
        _accounts.Remove(accountid);
    }

    /// <summary>
    /// 根据id获取已登录的角色数据
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public AccountData GetAccount(int id)
    {
        return _accounts[id];
    }
}
