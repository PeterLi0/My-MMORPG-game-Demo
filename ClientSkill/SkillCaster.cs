using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 技能释放器
/// </summary>
public class SkillCaster : SkillCfgObject
{
    // 施法者
    private Character _caster;

    // 上次释放这个技能的时间
    private float _lastCastTime = 0f;


    // 是否冷却完成
    public bool cooldown
    {
        get
        {
            return Time.time - _lastCastTime >= _skillBasicCfg.CD;
        }
    }

    // 最大施法范围
    public float maxRange { get { return _skillBasicCfg.MaxRange; } }

    // 是否正在施法
    public bool casting { get { return Time.time - _lastCastTime <= _skillBasicCfg.HitTime; } }

    // 施法时是否可移动
    public bool canMove { get { return _skillBasicCfg.CanMove; } }

    public SkillCaster(Character caster)
    {
        _caster = caster;
    }

    /// <summary>
    /// 释放技能
    /// </summary>
    public void Cast()
    {
        // 记录这次释放技能的时间
        _lastCastTime = Time.time;

        // 创建一个命中前对象
        BeforeHit beforeHit = new BeforeHit(_caster);
        beforeHit.SkillBasicCfg = _skillBasicCfg;
        beforeHit.SkillBulletCfg = _skillBulletCfg;
        beforeHit.SkillAOECfg = _skillAOECfg;
        beforeHit.SkillBuffCfg = _skillBuffCfg;
        beforeHit.SkillTrapCfg = _skillTrapCfg;
        SkillManager.instance.AddBeforeHit(_caster.GlobalID, beforeHit);
    }
}