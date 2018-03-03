using System;
using System.Collections.Generic;
using System.Text;


/// <summary>
/// 技能组配置
/// </summary>
public class SkillGroupCfg
{
    public int ID;
    public int RoleID;
    public int Index;
}


/// <summary>
/// 技能基础配置
/// </summary>
public class SkillBasicCfg
{
    public int ID;
    public string Name;
    public string Desc;                 // 技能描述(descripe)
    public bool NeedTarget;             // 是否需要攻击目标
    public bool CanMove;                // 施法时是否可移动
    public int BasicNum;                // 技能伤害
    public string AnimName;             // 动画剪辑名
    public float CD;                    // cooling down 冷却时间
    public float HitTime;               // 命中时间
    public int HitNum;                  // 攻击次数
    public float HitInterval;           // 攻击间隔
    public SkillType SkillType;         // 技能类型
    public float MaxRange;              // 最大攻击范围
    public AffectSide AffectSide;       // 作用的阵营
    public IgnoreType IgnoreTarget;     // 忽略的目标类型
    public string CastEffect;           // 释放效果

    public float AreaArg1;
    public float AreaArg2;

    public AreaType AreaCenter;
    public AreaShape AreaShape;
}

/// <summary>
/// 子弹配置
/// </summary>
public class SkillBulletCfg
{
    public int ID;
    public BulletType BulletType;
    public int SplitNum;                // 分裂箭的数量
    public float SplitAngle;            // 分裂箭的角度
    public float FlyRange;              // 最大飞行距离
    public bool FlyPierce;              // 可穿透
    public float FlySpeedH;             // 水平速度
    public float FlySpeedV;             // 垂直速度
    public bool FlyTrack;               // 是否跟踪
    public int FlyBounceNum;            // 弹射次数

    public string BulletEffect;         // 子弹特效
}

/// <summary>
/// 范围技能配置
/// </summary>
public class SkillAOECfg
{
    public int ID;
    public AreaType AreaCenter;         // 范围中心类型
    public AreaShape AreaShape;         // 范围形状
    public float AreaArg1;
    public float AreaArg2;
    public string AOEEffect;            // 释放的效果
}

/// <summary>
/// Buff配置
/// </summary>
public class SkillBuffCfg
{
    public int ID;
    public BuffType Type;               // Buff类型
    public AttrType AttrType;           // 修改的属性类型
    public float AttrValue;             // 修改的属性值
    public float Duration;              // Buff持续时间
    public ControlEffect Control;       // 控制效果
    public string Effect;               // Buff释放效果
}

/// <summary>
/// 陷阱
/// </summary>
public class SkillTrapCfg
{
    public int ID;
    public AreaType AreaCenter;         // 范围中心类型
    public AreaShape AreaShape;         // 范围形状
    public float AreaArg1;
    public float AreaArg2;
    public float Duration;              // 持续时间
    public float Interval;              // 陷阱伤害作用间隔
    public int Damage;                  // 伤害
    public string Effect;               // 陷阱效果
}

public partial class ConfigManager
{
    // 技能组配置
    private Dictionary<int, SkillGroupCfg> _skillGroupCfgs = new Dictionary<int, SkillGroupCfg>();

    // 技能基础配置
    private Dictionary<int, SkillBasicCfg> _skillBasicCfgs = new Dictionary<int, SkillBasicCfg>();

    // 子弹配置
    private Dictionary<int, SkillBulletCfg> _skillBulletCfgs = new Dictionary<int, SkillBulletCfg>();

    // AOE配置
    private Dictionary<int, SkillAOECfg> _skillAOECfgs = new Dictionary<int, SkillAOECfg>();

    // Buff配置
    private Dictionary<int, SkillBuffCfg> _skillBuffCfgs = new Dictionary<int, SkillBuffCfg>();

    // 陷阱配置
    private Dictionary<int, SkillTrapCfg> _skillTrapCfgs = new Dictionary<int, SkillTrapCfg>();

    /// <summary>
    /// 通过技能ID获取技能组信息
    /// </summary>
    /// <param name="roleid"></param>
    /// <returns></returns>
    public Dictionary<int, SkillGroupCfg> GetSkillGroupCfgs(int roleid)
    {
        Dictionary<int, SkillGroupCfg> cfgs = new Dictionary<int, SkillGroupCfg>();
        foreach (SkillGroupCfg cfg in _skillGroupCfgs.Values)
        {
            if (cfg.RoleID == roleid)
                cfgs.Add(cfg.ID, cfg);
        }

        return cfgs;
    }

    /// <summary>
    /// 通过技能ID获取技能基础配置
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public SkillBasicCfg GetSkillBasicCfg(int skillID)
    {
        return _skillBasicCfgs[skillID];
    }

    /// <summary>
    /// 通过技能ID获取子弹类技能配置
    /// </summary>
    /// <param name="skillID"></param>
    /// <returns></returns>
    public SkillBulletCfg GetSkillBulletCfg(int skillID)
    {
        return _skillBulletCfgs.ContainsKey(skillID) ? _skillBulletCfgs[skillID] : null;
    }

    public SkillAOECfg GetSkillAOECfg(int skillID)
    {
        return _skillAOECfgs.ContainsKey(skillID) ? _skillAOECfgs[skillID] : null;
    }

    public SkillBuffCfg GetSkillBuffCfg(int skillID)
    {
        return _skillBuffCfgs.ContainsKey(skillID) ? _skillBuffCfgs[skillID] : null;
    }

    public SkillTrapCfg GetSkillTrapCfg(int skillID)
    {
        return _skillTrapCfgs.ContainsKey(skillID) ? _skillTrapCfgs[skillID] : null;
    }

    /// <summary>
    /// 通过角色ID和技能索引获取技能组配置
    /// </summary>
    /// <param name="roleid"></param>
    /// <param name="skillindex"></param>
    /// <returns></returns>
    public SkillGroupCfg GetSkillGroupCfg(int roleid, int skillindex)
    {
        foreach (SkillGroupCfg cfg in _skillGroupCfgs.Values)
        {
            if (cfg.RoleID == roleid && cfg.Index == skillindex)
                return cfg;
        }
        return null;
    }

    /// <summary>
    /// 通过角色ID和技能索引获取技能ID
    /// </summary>
    /// <param name="roleID"></param>
    /// <param name="skillIndex"></param>
    /// <returns></returns>
    public int GetSkillID(int roleID, int skillIndex)
    {
        foreach (SkillGroupCfg cfg in _skillGroupCfgs.Values)
        {
            if (cfg.RoleID == roleID && cfg.Index == skillIndex)
            {
                return cfg.ID;
            }
        }

        return 0;
    }
}