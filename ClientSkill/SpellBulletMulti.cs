using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 多重箭
/// </summary>
public class SpellBulletMulti : SpellBullet
{
    // 从技能创建到现在的时间
    private float _elapsedTime = 0f;

    // 还没有发射的子弹数量
    private int _remainingNum;

    public SpellBulletMulti(Character caster) : base(caster)
    {
    }

    public override void Initialize()
    {
        _remainingNum = _skillBasicCfg.HitNum;
    }


    public override void Update(float dt)
    {
        if(_elapsedTime >= _skillBasicCfg.HitInterval)
        {
            Spell spell = new SpellBulletTrack(_caster);
            spell.SkillBasicCfg = _skillBasicCfg;
            spell.SkillBulletCfg = _skillBulletCfg;
            spell.target = _target;
            spell.Initialize();
            SkillManager.instance.AddSpell(_caster.GlobalID, spell);

            _remainingNum--;
            _elapsedTime = 0;

            if (_remainingNum <= 0)
                _hited = true;
        }

        _elapsedTime += dt;
    }
}