using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using common;

/// <summary>
/// 角色属性
/// </summary>
public class CharacterAttr
{
    public int id;                  // 角色id
    public int accountid;           // 玩家所属账号
    public string name;             // 玩家名
    public Race race;                // 种族
    public Job job;                 // 职业
    public int gender;              // 性别
    public int level;               // 等级
    public int exp;                 // 经验
    public int diamond;             // 钻石
    public int gold;                // 金币
    public Luna3D.Vector3 pos;      // 初始坐标

    public int cfgid;               // 角色配置id

    public int mapid;



    public static CharacterAttr GetAttr(common.CharacterDTO dto)
    {
        CharacterAttr attr = new CharacterAttr();
        attr.id = dto.id;
        attr.accountid = dto.accountid;
        attr.name = dto.name;
        attr.race = (Race)dto.race;
        attr.job = (Job)dto.job;
        attr.gender = dto.gender;
        attr.level = dto.level;
        attr.exp = dto.exp;
        attr.diamond = dto.diamond;
        attr.gold = dto.gold;
        attr.pos = new Luna3D.Vector3(dto.pos_x, dto.pos_y, dto.pos_z);
        attr.cfgid = dto.cfgid;
        attr.mapid = dto.mapid;

        return attr;
    }

    /// <summary>
    /// 克隆角色属性
    /// </summary>
    /// <returns></returns>
    public CharacterAttr Clone()
    {
        return (CharacterAttr)this.MemberwiseClone();
    }
}
