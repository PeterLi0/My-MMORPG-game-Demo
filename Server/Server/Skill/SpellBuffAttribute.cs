using System;
using System.Collections.Generic;



/// <summary>
/// 属性Buff
/// </summary>
public class SpellBuffAttribute : SpellBuff
{
    // Buff的特效
    private Transform _transform;

    // 剩下的时间
    private float _remainingTime = 0;

    public SpellBuffAttribute(Character caster) : base(caster)
    {

    }

    public override void Initialize()
    {
        // 修改属性
        if (_skillBuffCfg.AttrType == AttrType.MoveSpeed)
        {
            _target.speed += _skillBuffCfg.AttrValue;
        }
        else if (_skillBuffCfg.AttrType == AttrType.AttackSpeed)
        {
            //_target.currAttr. += _skillBuffCfg.AttrValue;
        }
        else if (_skillBuffCfg.AttrType == AttrType.Defend)
        {
            _target.currAttr.denfend += _skillBuffCfg.AttrValue;
        }


        // 剩余时间
        _remainingTime = _skillBuffCfg.Duration;
    }

    public override void Update(float dt)
    {
        if(_remainingTime <= 0)
        {
            // 修改属性
            if (_skillBuffCfg.AttrType == AttrType.MoveSpeed)
            {
                _target.speed -= _skillBuffCfg.AttrValue;
            }
            else if (_skillBuffCfg.AttrType == AttrType.AttackSpeed)
            {
                //_target.attackSpeed -= _skillBuffCfg.AttrValue;
            }
            else if (_skillBuffCfg.AttrType == AttrType.Defend)
            {
                _target.currAttr.denfend -= _skillBuffCfg.AttrValue;
            }


            _hited = true;
        }

        _remainingTime -= dt;
    }
}
