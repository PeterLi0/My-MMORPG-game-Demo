using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 技能释放后到命中之前的对象
/// </summary>
public class BeforeHit : SkillCfgObject
{
    // 施法者
    private Character _caster;

    // 施法目标
    private Character _target;

    // 从抬手到先现在的时间
    private float _elapsedTime = 0;

    private bool _hit = false;

    // 近战攻击是否已经命中, 或者远程攻击是否已经出手
    public bool hit { get { return _hit; } }

    public BeforeHit(Character caster)
    {
        _caster = caster;
        _target = _caster.lockedTarget;
    }

    public void Update(float dt)
    {
        if(_elapsedTime > _skillBasicCfg.HitTime)
        {
            _hit = true;
            Hit();
        }

        _elapsedTime += dt;
    }

    private void Hit()
    {
        Spell spell = null;
        
        if (_skillBulletCfg != null)                // 子弹类技能
        {
            spell = CreateBullet(_skillBulletCfg);
        }
        else if(_skillAOECfg != null)               // AOE技能
        {
            spell = CreateAOE(_skillAOECfg);
        }
        else if(_skillBuffCfg != null)              // Buff
        {
            spell = CreateBuff(_skillBuffCfg);
        }
        else if(_skillTrapCfg != null)              // 陷阱
        {
            spell = CreateTrap(_skillTrapCfg);
        } 
        else                                        // 近战攻击
        {
            spell = new SpellMelee(_caster);
        }

        spell.SkillBasicCfg = _skillBasicCfg;
        spell.SkillBulletCfg = _skillBulletCfg;
        spell.SkillAOECfg = _skillAOECfg;
        spell.SkillBuffCfg = _skillBuffCfg;
        spell.SkillTrapCfg = _skillTrapCfg;
        spell.target = _target;

        if (spell.needUpdate)
        {
            spell.Initialize();
            SkillManager.instance.AddSpell(_caster.GlobalID, spell);
        }            
        else
            spell.Hit();
    }

    /// <summary>
    /// 创建子弹类技能
    /// </summary>
    /// <param name="cfg"></param>
    /// <returns></returns>
    private Spell CreateBullet(SkillBulletCfg cfg)
    {
        Spell spell = null;
        switch(cfg.BulletType)
        {
            case BulletType.Cast:
                if (cfg.FlyTrack)
                    spell = new SpellBulletTrack(_caster);
                else
                    spell = new SpellBulletNormal(_caster);
                break;
            case BulletType.Multiple:
                spell = new SpellBulletMulti(_caster);
                break;
            case BulletType.Split:
                spell = new SpellBulletSplit(_caster);
                break;
            case BulletType.Bounce:
                break;
            case BulletType.Boomerang:
                spell = new SpellBulletBoomerang(_caster);
                break;
        }

        return spell;
    }

    /// <summary>
    /// 创建范围技能
    /// </summary>
    /// <param name="cfg"></param>
    /// <returns></returns>
    private Spell CreateAOE(SkillAOECfg cfg)
    {
        Spell spell = null;
        switch(cfg.AreaShape)
        {
            case AreaShape.Circle:
                spell = new SpellAOECircle(_caster);
                break;
            case AreaShape.Fan:
                spell = new SpellAOEFan(_caster);
                break;
            case AreaShape.Rect:
                spell = new SpellAOERect(_caster);
                break;
        }

        return spell;
    }

    /// <summary>
    /// 创建陷阱
    /// </summary>
    /// <param name="cfg"></param>
    /// <returns></returns>
    private Spell CreateTrap(SkillTrapCfg cfg)
    {
        Spell spell = null;
        switch(cfg.AreaShape)
        {
            case AreaShape.Circle:
                spell = new SpellTrapCircle(_caster);
                break;
        }

        return spell;
    }

    /// <summary>
    /// 创建Buff类技能
    /// </summary>
    /// <param name="cfg"></param>
    /// <returns></returns>
    private Spell CreateBuff(SkillBuffCfg cfg)
    {
        Spell spell = null;
        switch(cfg.Type)
        {
            case BuffType.Attribute:
                spell = new SpellBuffAttribute(_caster);
                break;
            case BuffType.Control:
                spell = new SpellBuffControl(_caster);
                break;
        }
        return spell;
    }
}