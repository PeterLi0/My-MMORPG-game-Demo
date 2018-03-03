using System;
using common;
using System.Collections.Generic;

[Serializable]
public class CharacterData
{
    public int id;
    public int accountid;
    public string name;
    public int race;
    public int job;
    public int gender;
    public int level;
    public int exp;
    public int diamond;
    public int gold;
    public float pos_x;
    public float pos_y;
    public float pos_z;
    public int cfgid;
    public int mapid;

    public CharacterData()
    {

    }

    public CharacterData(int id, int accountid, string name, int race, int job, 
        int gender, int level, int exp, int diamond, int gold, float pos_x, float pos_y, float pos_z, int cfgid, int mapid)
    {
        this.id = id;
        this.accountid = accountid;
        this.name = name;
        this.race = race;
        this.job = job;
        this.gender = gender;
        this.level = level;
        this.exp = exp;
        this.diamond = diamond;
        this.gold = gold;
        this.pos_x = pos_x;
        this.pos_y = pos_y;
        this.pos_z = pos_z;
        this.cfgid = cfgid;
        this.mapid = mapid;
    }

    public static CharacterDTO GetDTO(CharacterData d)
    {
        CharacterDTO dto = new CharacterDTO();
        dto.id = d.id;
        dto.accountid = d.accountid;
        dto.name = d.name;
        dto.race = d.race;
        dto.job = d.job;
        dto.gender = d.gender;
        dto.level = d.level;
        dto.exp = d.exp;
        dto.diamond = d.diamond;
        dto.gold = d.gold;
        dto.pos_x = d.pos_x;
        dto.pos_y = d.pos_y;
        dto.pos_z = d.pos_z;
        dto.cfgid = d.cfgid;
        dto.mapid = d.mapid;
        return dto;
    }
}

public partial class RedisCacheManager
{

    /// <summary>
    /// 判断当前角色是否在线
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool IsCharOnline(int id)
    {
        CharacterData d = _redis.Get<CharacterData>(id,id);
        return d == null ? false : true;
    }

    /// <summary>
    /// 角色上线
    /// </summary>
    /// <param name="id"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public void CharOnline(int id, CharacterData data)
    {
        // 缓存角色数据
        _redis.Set(id, id, data);

        // 缓存角色邮件数据
        LoadMailData(id);

        // 缓存角色装备数据
        LoadEquipData(id);

        // 缓存角色背包数据
        LoadInvData(id);
    }

    /// <summary>
    /// 角色离线
    /// </summary>
    /// <param name="id"></param>
    public void CharOffline(int id)
    {
        CharacterData d = _redis.Get<CharacterData>(id, id);
        if (d != null)
        {
            // 移除角色数据
            _redis.Remove(id);

            // 写入角色数据
            UpdateCharData(id);

            // 写入邮件数据
            WriteMailData(id);

            // 写入装备数据
            UpdateEquipData(id);

            // 写入背包数据
            WriteInvData(id);
        }
        else
            Console.WriteLine(string.Format("角色已经下线:{0}", id));
    }

    public CharacterData GetCharData(int id)
    {
        return  _redis.Get<CharacterData>(id, id);
    }

    public void UpdateCharData(int id)
    {

    }
}


partial class CacheManager
{
    // 保存所有的已经上线的角色信息
    private Dictionary<int, CharacterData> _chars = new Dictionary<int, CharacterData>();

    /// <summary>
    /// 判断当前角色是否在线
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public bool IsCharOnline(int id)
    {
        return _chars.ContainsKey(id);
    }

    /// <summary>
    /// 角色上线
    /// </summary>
    /// <param name="id"></param>
    /// <param name="data"></param>
    /// <returns></returns>
    public void CharOnline(int id, CharacterData data)
    {
        _chars.Add(id, data);

        LoadMailData(id);
        LoadEquipData(id);
        LoadInvData(id);
    }

    /// <summary>
    /// 角色离线
    /// </summary>
    /// <param name="id"></param>
    public void CharOffline(int id)
    {
        if (_chars.ContainsKey(id))
        {
            CharacterData ch = _chars[id];
            string sql = string.Format("UPDATE characters SET name = '{0}', race = {1}, job = {2}, gender = {3}, level = {4}, exp = {5},  diamond = {6}, gold = {7}, pos_x = {8}, pos_y = {9}, pos_z = {10} WHERE id = {11}",
                ch.name, ch.race, ch.job, ch.gender, ch.level, ch.exp, ch.diamond, ch.gold, ch.pos_x, ch.pos_y, ch.pos_z, id);

            MysqlManager.instance.ExecNonQuery(sql);

            // 移除角色数据
            _chars.Remove(id);

            // 写入邮件数据
            WriteMailData(id);

            // 
            WriteEquipData(id);

            // 
            WriteInvData(id);

            _equips.Remove(id);
            _invs.Remove(id);
            _mails.Remove(id);
        }
        else
            Console.WriteLine(string.Format("角色已经下线:{0}", id));
    }

    public CharacterData GetCharData(int id)
    {
        return _chars[id];
    }
}