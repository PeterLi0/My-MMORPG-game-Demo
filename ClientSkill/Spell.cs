using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 所有类型的法术的基类
/// </summary>
public abstract class Spell : SkillCfgObject
{
    // 是否需要更新
    protected bool _needUpdate = false;

    public bool needUpdate { get { return _needUpdate; } }

    // 技能的释放者
    protected Character _caster;

    // 技能释放目标
    protected Character _target;

    // 技能是否已经命中
    protected bool _hited = false;

    public bool hited { get { return _hited; } }

    public Character target { set { _target = value; } }

    public Spell(Character caster)
    {
        _caster = caster;
    }

    public virtual void Initialize() { }

    public virtual void Update(float dt) { }

    public virtual void Hit() { }
}