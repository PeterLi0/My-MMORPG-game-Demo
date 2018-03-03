using Luna3D;
using System;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// 指向型的子弹
/// </summary>
public class SpellBulletTrack : SpellBullet
{
    private Transform _transform;


    // 初始坐标
    private Vector3 _startPosition;

    public SpellBulletTrack(Character caster) : base(caster)
    {
    }

    public override void Initialize()
    {
        _startPosition = _caster.position + new Vector3(0, 1, 0);
        _transform.position = _startPosition;
        _transform.localScale = Vector3.one;
    }

    public override void Update(float dt)
    {
        float distance = Vector3.Distance(_transform.position, _target.position + new Vector3(0, 1, 0));

        // 子弹与目标距离小于一定距离，判断命中
        if (distance <= 0.5f) Hit();

        // 计算轨迹
        Vector3 dir = (_target.position + new Vector3(0, 1, 0) - _startPosition).normalized;
        _transform.position += _skillBulletCfg.FlySpeedH * dt * dir;
    }

    public override void Hit()
    {
        _hited = true;
        _target.Wound(_skillBasicCfg.BasicNum);
    }
}
