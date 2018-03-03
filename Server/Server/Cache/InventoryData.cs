using System;
using System.Collections.Generic;
using common;

/// <summary>
/// 背包栏数据
/// </summary>
[Serializable]
public class InventoryData
{
    public int id;
    public int characterid;
    public int slot;
    public int itemid;
    public int num;

    public InventoryData()
    {

    }

    public InventoryData(int id, int characterid, int slot, int itemid, int num)
    {
        this.id = id;
        this.characterid = characterid;
        this.slot = slot;
        this.itemid = itemid;
        this.num = num;
    }

    public static InventoryDTO GetInvDTO(InventoryData d)
    {
        InventoryDTO dto = new InventoryDTO();
        dto.slot = d.slot;
        dto.itemid = d.itemid;
        dto.num = d.num;

        return dto;
    }
}

public partial class RedisCacheManager
{
    public void LoadInvData(int characterid)
    {
        string sql = string.Format("SELECT * FROM inventory WHERE characterid = {0}", characterid);
        Dictionary<int, InventoryData> equips = MysqlManager.instance.ExecQueryDic<InventoryData>(sql);
        foreach (InventoryData item in equips.Values)
        {
            _redis.Set(characterid, item.id, item);
        }
    }

    // 删除背包栏位数据
    public void DeleteInvData(int characterid, int slot)
    {
        _redis.Remove(characterid, slot);
    }

    public void WriteInvData(int characterid)
    {
        // 删除数据库数据
        string sql = string.Format("DELETE FROM inventory WHERE characterid = {0}", characterid);
        MysqlManager.instance.ExecNonQuery(sql);

        // 按照缓存写入数据库
        List<InventoryData> invs = _redis.GetAll<InventoryData>(characterid);
        for (int i = 0; i < invs.Count; i++)
        {
            InventoryData inv = invs[i];
            sql = string.Format("INSERT INTO inventory (characterid, slot, itemid, num) VALUES ({0}, {1}, {2}, {3})", characterid, inv.slot, inv.itemid, inv.num);
            MysqlManager.instance.ExecNonQuery(sql);
        }

        // 清除缓存
        _redis.Remove(characterid);
    }
}

partial class CacheManager
{    
    // 背包栏位
    private Dictionary<int, List<InventoryData>> _invs = new Dictionary<int, List<InventoryData>>();

    /// <summary>
    /// 载入角色的背包数据
    /// </summary>
    /// <param name="characterid"></param>
    public void LoadInvData(int characterid)
    {
        string sql = string.Format("SELECT * FROM inventory WHERE characterid = {0}", characterid);
        List<InventoryData> invs = MysqlManager.instance.ExecQuery<InventoryData>(sql);

        _invs.Add(characterid, invs);
    }

    /// <summary>
    /// 将背包栏位数据写入数据库
    /// </summary>
    /// <param name="characterid"></param>
    public void WriteInvData(int characterid)
    {
        string sql = string.Format("DELETE FROM inventory WHERE characterid = {0}", characterid);
        MysqlManager.instance.ExecNonQuery(sql);

        List<InventoryData> invs = _invs[characterid];

        for (int i = 0; i < invs.Count; i++)
        {
            InventoryData inv = invs[i];
            sql = string.Format("INSERT INTO inventory (characterid, slot, itemid, num) VALUES ({0}, {1}, {2}, {3})", characterid, inv.slot, inv.itemid, inv.num);
            MysqlManager.instance.ExecNonQuery(sql);
        }
    }

    // 根据栏位获取角色数据
    public InventoryData GetInvData(int characterid, int slot)
    {
        List<InventoryData> invs = _invs[characterid];
        foreach (InventoryData data in invs)
        {
            if (data.slot == slot)
                return data;
        }
        return null;
    }

    public List<InventoryData> GetInvDatas(int characterid)
    {
        return _invs[characterid];
    }


    /// <summary>
    ///  查找第一个空的栏位
    /// </summary>
    /// <param name="characterid"></param>
    /// <returns></returns>
    public int GetFirstEmptySlot(int characterid)
    {
        List<InventoryData> invs = _invs[characterid];

        int firstEmptySlot = 50;
        for (int i = 0; i < invs.Count; i++)
        {
            if (invs[i].itemid == -1)
            {
                firstEmptySlot = invs[i].slot;
                break;
            }
        }

        return firstEmptySlot;
    }
}