using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 圆形陷阱
/// </summary>
public class SpellTrapCircle : SpellTrap
{
    // 所有的敌方单位
    private Dictionary<int, Character> _enemies;

    // 已经过去的时间
    private float _elapsedTime = 0f;

    // 剩下的生效次数
    private int _remainingNum;

    // 陷阱中心
    private Vector3 _center;

    public SpellTrapCircle(Character caster) : base(caster)
    {
    }

    public override void Initialize()
    {
        _enemies = CharacterManager.instance.GetNonSideCharacters(_caster.side);

        // 获取陷阱中心
        _center = GetTrapCenter();

        // 如果陷阱有初始伤害
        if (_skillBasicCfg.BasicNum > 0)
            Hit(_skillBasicCfg.BasicNum);

        // 求剩余次数
        _remainingNum = (int)(_skillTrapCfg.Duration / _skillTrapCfg.Interval);

        SkillManager.instance.PlayEffect(_skillTrapCfg.Effect, _center + new Vector3(0 ,1, 0), _caster.forward);
    }

    public override void Update(float dt)
    {
        if(_elapsedTime >= _skillTrapCfg.Interval)
        {
            Hit(_skillTrapCfg.Damage);

            _remainingNum--;
            _elapsedTime = 0;

            // 剩下的次数为0, 则移除陷阱
            if (_remainingNum <= 0)
                _hited = true;
        }

        _elapsedTime += dt;
    }

    public void Hit(int damage)
    {
        // 范围检测
        foreach (Character ch in _enemies.Values)
        {
            if (MathTools.PointInCircle(_center, ch.position, _skillTrapCfg.AreaArg1))
            {
                ch.Wound(damage);
            }
        }
    }
}