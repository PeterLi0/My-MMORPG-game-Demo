//using System;
//using common;
//using proto.battlesync;
//using proto.battlescene;
//using Luna3D;

//public class Player : Character
//{

//    // 相机偏移
//    private Vector3 _cameraOffset;

//    // 目标拣选标记
//    private Transform _targetTip;


//    public Player(int globalid, RoleCfg roleCfg, Vector3 position) 
//        : base(globalid, roleCfg, position)
//    {
//    }

//    public override void Update(float dt)
//    {
//        base.Update(dt);

//    }

//    private void OnJoystickMove(Vector2 vec)
//    {
//        Vector3 pos = position + new Vector3(vec.x, 0, vec.y) * 3;
//        Move(pos);
//    }


//    /// <summary>
//    /// 释放技能
//    /// </summary>
//    /// <param name="index">技能索引</param>
//    public void Cast(int index)
//    {
//        // 技能组配置
//        SkillGroupCfg cfg = ConfigManager.instance.GetSkillGroupCfg(_cfg.ID, index);

//        // 技能基础配置
//        SkillBasicCfg basicCfg = ConfigManager.instance.GetSkillBasicCfg(cfg.ID);

//        // 技能发射器
//        SkillCaster skillCaster = _skillCasters[cfg.ID];


//        if (basicCfg.NeedTarget && _lockedTarget == null)
//        {
//            return;
//        }

//        if (basicCfg.NeedTarget && _lockedTarget != null && basicCfg.AffectSide == AffectSide.Enemy && _lockedTarget.side == side)
//        {
//            return;
//        }

//        if (basicCfg.NeedTarget && _lockedTarget != null && basicCfg.AffectSide == AffectSide.Friend && _lockedTarget.side != side)
//        {
//            return;
//        }

//        if (!skillCaster.cooldown)
//        {
//            return;
//        }

//        if (basicCfg.NeedTarget && _lockedTarget != null && Vector3.Distance(_transform.position, _lockedTarget.position) >= basicCfg.MaxRange)
//        {

//            return;
//        }

//        skillCaster.Cast();
//    }

//    public override void Finalise()
//    {
//        base.Finalise();
//    }

//    public override void Die()
//    {
//        base.Die();
//    }
//}