using System;
using System.Collections.Generic;
using System.Text;



/// <summary>
/// 角色配置
/// </summary>
public class RoleCfg
{
    public int ID;
    public string ModelName;
    public string RoleName;
    public string Behavior;         // 角色行为
    public RoleType RoleType;       // 角色类型
    public int Hp;
    public int Mp;
    public float MoveSpeed;         // 移动速度
    public float AttackSpeed;       // 攻击速度
    public float Defend;            // 防御力
    public float Sight;             // 视野
    public string IdleAnim;         // 待机动画
    public string MoveAnim;         // 移动动画
}
public partial class ConfigManager
{
    private Dictionary<int, RoleCfg> _roleCfgs = new Dictionary<int, RoleCfg>();


    /// <summary>
    /// 获取角色配置
    /// </summary>
    /// <param name="roleID"></param>
    /// <returns></returns>
    public RoleCfg GetRoleCfg(int roleID)
    {
        return _roleCfgs[roleID];
    }

    /// <summary>
    /// 获取某一类型的角色
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public Dictionary<int, RoleCfg> GetTypeRoleCfgs(RoleType type)
    {
        Dictionary<int, RoleCfg> cfgs = new Dictionary<int, RoleCfg>();
        foreach (RoleCfg cfg in _roleCfgs.Values)
        {
            if (cfg.RoleType == type)
                cfgs.Add(cfg.ID, cfg);
        }

        return cfgs;
    }

}

