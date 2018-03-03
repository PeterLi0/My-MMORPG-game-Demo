using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 陷阱
/// </summary>
public abstract class SpellTrap : Spell
{
    public SpellTrap(Character caster) : base(caster)
    {
        _needUpdate = true;
    }

    public virtual Vector3 GetTrapCenter()
    {
        // 获取范围技能中心
        Vector3 center = Vector3.zero;
        if (_skillTrapCfg.AreaCenter == AreaType.Self)
            center = _caster.position;
        else if (_skillTrapCfg.AreaCenter == AreaType.Target)
            center = _target.position;

        return center;
    }
}


