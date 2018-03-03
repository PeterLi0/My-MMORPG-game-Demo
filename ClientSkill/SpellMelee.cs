using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 近战攻击
/// </summary>
public class SpellMelee : Spell
{
    public SpellMelee(Character caster) : base(caster)
    {
        _needUpdate = false;

    }

    public override void Hit()
    {
        // 施法者目标受伤
        _target.Wound(_skillBasicCfg.BasicNum);
    }
}