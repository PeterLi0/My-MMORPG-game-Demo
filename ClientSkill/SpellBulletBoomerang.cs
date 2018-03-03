using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 回旋镖
/// </summary>
public class SpellBulletBoomerang : SpellBullet
{
    private Transform _transform;

    // 结束坐标
    private Vector3 _endPosition;

    // 当前朝向
    private Vector3 _dir;

    // 当前朝向是否为向前
    private bool _forward = true;

    // 所有的敌方单位
    private Dictionary<int, Character> _enemies;

    public SpellBulletBoomerang(Character caster) : base(caster)
    {

    }

    public override void Initialize()
    {
        _transform = PoolManager.instance.Spawn("FX/", _skillBulletCfg.BulletEffect).transform;

        // 求结束点
        _endPosition = _target.position + new Vector3(0, 1, 0);

        _transform.position = _caster.position + new Vector3(0, 1, 0); ;
        _transform.localScale = Vector3.one;

        // 求朝向
        _dir = (_endPosition - _transform.position).normalized;

        // 查找所有敌方单位
        _enemies = CharacterManager.instance.GetNonSideCharacters(_caster.side);
    }

    public override void Update(float dt)
    {
        // 重置朝向
        if (_forward && Vector3.Distance(_transform.position, _endPosition) <= 0.5f)
        {
            _forward = false;
            _enemies = CharacterManager.instance.GetNonSideCharacters(_caster.side);
        }

        if (!_forward && Vector3.Distance(_transform.position, _caster.position + new Vector3(0, 1, 0)) <= 0.5f)
            End();

        // 更新命中
        UpdateHit(dt);

        // 更新飞行轨迹
        UpdateTrajectory(dt);
    }

    /// <summary>
    /// 更新命中
    /// </summary>
    private void UpdateHit(float dt)
    {
        foreach (Character ch in _enemies.Values.ToArray())
        {
            float distance = Vector3.Distance(_transform.position, ch.position + new Vector3(0, 1, 0));

            // 子弹与目标距离小于一定距离，判断命中
            if (distance <= 2f)
            {
                // 受伤
                ch.Wound(_skillBasicCfg.BasicNum);
                _enemies.Remove(ch.GlobalID);

                // 非穿透类型的子弹直接消失
                // if (!_skillBulletCfg.FlyPierce) End();
            }
        }
    }

    /// <summary>
    /// 更新弹道
    /// </summary>
    public void UpdateTrajectory(float dt)
    {
        if (!_forward) _dir = (_caster.position + new Vector3(0, 1, 0) - _transform.position).normalized;
        _transform.position += _skillBulletCfg.FlySpeedH * dt * _dir;
    }


    /// <summary>
    /// 结束
    /// </summary>
    public void End()
    {
        _hited = true;
        PoolManager.instance.Unspawn(_transform.gameObject);
    }
}