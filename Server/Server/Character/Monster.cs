//using System;
//using System.Collections.Generic;
//using Luna3D;

//public class Monster : Character
//{
//    public Monster(int globalid, RoleCfg roleCfg, Vector3 position) : base(globalid, roleCfg, position)
//    {
//        // 创建角色AI
//        _behavior = Activator.CreateInstance(Type.GetType(_cfg.Behavior), new object[1] { this }) as BaseBehavior;
//    }

//    public override void Update(float dt)
//    {
//        base.Update(dt);

//        _behavior.Update(dt);
//    }

//    /// <summary>
//    /// 释放技能
//    /// </summary>
//    /// <param name="caster"></param>
//    public override void Cast(SkillCaster caster)
//    {
//        caster.Cast();
//    }
//}