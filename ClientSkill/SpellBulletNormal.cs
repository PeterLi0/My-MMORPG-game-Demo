using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 非指向性子弹
/// </summary>
public class SpellBulletNormal : SpellBullet
{
    private Transform _transform;

    // 初始坐标
    private Vector3 _startPosition;

    // 子弹的朝向
    private Vector3 _dir;

    // 所有的敌方单位
    private Dictionary<int, Character> _enemies;

    public SpellBulletNormal(Character caster) : base(caster)
    {
    }

    public override void Initialize()
    {
        _transform = PoolManager.instance.Spawn("FX/", _skillBulletCfg.BulletEffect).transform;

        _startPosition = _caster.position + new Vector3(0, 1, 0);
        _transform.position = _startPosition;
        _transform.localScale = Vector3.one;

        _dir = _caster.forward;

        _enemies = CharacterManager.instance.GetNonSideCharacters(_caster.side);
    }

    /// <summary>
    /// 设置朝向
    /// </summary>
    /// <param name="dir"></param>
    public void SetDirection(Vector3 dir)
    {
        _dir = dir;
    }

    public override void Update(float dt)
    {
        // 判断超出飞行范围
        if (Vector3.Distance(_transform.position, _startPosition) >= _skillBulletCfg.FlyRange)
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
            if (distance <= 0.5f)
            {
                // 受伤
                ch.Wound(_skillBasicCfg.BasicNum);
                _enemies.Remove(ch.GlobalID);

                // 非穿透类型的子弹直接消失
                if (!_skillBulletCfg.FlyPierce)  End();
            }
        }
    }

    /// <summary>
    /// 更新弹道
    /// </summary>
    public void UpdateTrajectory(float dt)
    {
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