using Luna3D;
using System;
using System.Collections.Generic;



/// <summary>
/// 范围技能
/// </summary>
public class SpellAOE : Spell
{
    public SpellAOE(Character caster) : base(caster)
    {
        _needUpdate = false;
    }

    public virtual Vector3 GetAOECenter()
    {
        // 获取范围技能中心
        Vector3 center = Vector3.zero;
        if (_skillAOECfg.AreaCenter == AreaType.Self)
            center = _caster.position;
        else if (_skillAOECfg.AreaCenter == AreaType.Target)
            center = _target.position;

        return center;
    }
}

/// <summary>
/// 圆形AOE
/// </summary>
public class SpellAOECircle : SpellAOE
{
    public SpellAOECircle(Character caster) : base(caster)
    {

    }

    public override void Hit()
    {
        // 获取所有不是己方的所有单位
        Dictionary<int, Character> chs = BattleCharacters.instance.GetNonSideCharacters(_caster.side);

        Vector3 center = base.GetAOECenter();
            
        // 范围检测
        foreach(Character ch in chs.Values)
        {
            if (MathTools.PointInCircle(center, ch.position, _skillAOECfg.AreaArg1))
            {
                ch.Wound(_skillBasicCfg.BasicNum);
            }
        }
    }
}

/// <summary>
/// 扇形AOE
/// </summary>
public class SpellAOEFan : SpellAOE
{
    public SpellAOEFan(Character caster) : base(caster)
    {
    }

    public override void Hit()
    {
        // 获取所有不是己方的所有单位
        Dictionary<int, Character> chs = BattleCharacters.instance.GetNonSideCharacters(_caster.side);

        Vector3 center = GetAOECenter();

        foreach(Character ch in chs.Values)
        {
            if(MathTools.PointInFan(_caster.forward, center, ch.position, _skillAOECfg.AreaArg1, _skillAOECfg.AreaArg2))
            {
                ch.Wound(_skillBasicCfg.BasicNum);
            }
        }

    }
}

/// <summary>
/// 矩形AOE
/// </summary>
public class SpellAOERect : SpellAOE
{
    public SpellAOERect(Character caster) : base(caster)
    {
    }
}

