
using common;
using System.Collections.Generic;


/// <summary>
/// 装备栏数据
/// </summary>
public class EquipData
{
    public int id;
    public int characterid;
    public int slot;
    public int itemid;

    public EquipData()
    {

    }

    public EquipData(int id, int characterid, int slot, int itemid)
    {
        this.id = id;
        this.characterid = characterid;
        this.slot = slot;
        this.itemid = itemid;
    }

    public static InventoryDTO GetInvDTO(EquipData d)
    {
        InventoryDTO dto = new InventoryDTO();
        dto.slot = d.slot;
        dto.itemid = d.itemid;
        dto.num = 1;

        return dto;
    }
}
public partial class RedisCacheManager
{
    // 载入装备数据到缓存
    public void LoadEquipData(int characterid)
    {
        string sql = string.Format("SELECT * FROM equip WHERE characterid = {0}", characterid);
        Dictionary<int, EquipData> equips = MysqlManager.instance.ExecQueryDic<EquipData>(sql);
        foreach (EquipData item in equips.Values)
        {
            _redis.Set(characterid, item.id, item);
        }
    }

    // 删除装备
    public void DeleteEquipData(int characterid, int slot)
    {
        _redis.Remove(characterid, slot);
    }

    // 更新装备数据
    public void UpdateEquipData(int characterid)
    {
        // 删除数据库数据
        string sql = string.Format("DELETE FROM equip WHERE characterid = {0}", characterid);
        MysqlManager.instance.ExecNonQuery(sql);

        // 按照缓存写入数据库
        List<EquipData> equips = _redis.GetAll<EquipData>(characterid);
        for(int i = 0; i < equips.Count; i++)
        {
            EquipData equip = equips[i];
            sql = string.Format("INSERT INTO equip (characterid, slot, itemid) VALUES ({0}, {1}, {2})", characterid, equip.slot, equip.itemid);
            MysqlManager.instance.ExecNonQuery(sql);
        }

        // 清除缓存
        _redis.Remove(characterid);
    }

    // 根据角色id和装备栏位获取角色装备数据
    public EquipData GetEquipData(int characterid, int slot)
    {
        return _redis.Get<EquipData>(characterid, slot);
    }
}

partial class CacheManager
{
    // 保存所有的上线玩家的装备数据
    private Dictionary<int, List<EquipData>> _equips = new Dictionary<int, List<EquipData>>();

    // 载入装备数据
    public void LoadEquipData(int characterid)
    {
        string sql = string.Format("SELECT * FROM equip WHERE characterid = {0}", characterid);
        List<EquipData> equips = MysqlManager.instance.ExecQuery<EquipData>(sql);

        _equips.Add(characterid, equips);
    }


    // 写入装备数据
    public void WriteEquipData(int characterid)
    {
        List<EquipData> equips = _equips[characterid];

        // 删除数据库中的装备数据
        string sql = string.Format("DELETE FROM equip WHERE characterid = {0}", characterid);
        MysqlManager.instance.ExecNonQuery(sql);

        // 将缓存中的数据插入到数据库中
        for (int i = 0; i < equips.Count; i++)
        {
            EquipData equip = equips[i];
            sql = string.Format("INSERT INTO equip (characterid, slot, itemid) VALUES ({0}, {1}, {2})", characterid, equip.slot, equip.itemid);
            MysqlManager.instance.ExecNonQuery(sql);
        }
    }

    // 根据角色id和装备栏位获取角色装备数据
    public EquipData GetEquipData(int characterid, int slot)
    {
        List<EquipData> equips = _equips[characterid];

        foreach (EquipData data in equips)
        {
            if (data.slot == slot && data.characterid == characterid)
                return data;
        }
        return null;
    }

    /// <summary>
    /// 获取某个角色的装备
    /// </summary>
    /// <param name="characterid"></param>
    /// <returns></returns>
    public List<EquipData> GetEquipDatas(int characterid)
    {
        return _equips[characterid];
    }
}