using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 分裂箭
/// </summary>
public class SpellBulletSplit : SpellBullet
{
    //临时对象
    private Transform temp;

    public SpellBulletSplit(Character caster) : base(caster)
    {

    }

    public override void Initialize()
    {
      
        temp = new GameObject().transform;
        _target = _caster.lockedTarget;
        // 求2支箭之间的角度
        int angleNum = _skillBulletCfg.SplitNum - 1;
        float angle = _skillBulletCfg.SplitAngle / angleNum;

        temp.position = _target.transform.position;
        temp.RotateAround(_caster.position, temp.up, angle * -angleNum / 2);

        // 创建每支箭
        for (int i = 0; i <= angleNum; i++)
        {
            Vector3 dir = (temp.position - _caster.position).normalized;
            temp.RotateAround(_caster.position, temp.up, angle);

            // 创建一支非指向型箭
            SpellBulletNormal bullet = new SpellBulletNormal(_caster);
            bullet.SkillBasicCfg = _skillBasicCfg;
            bullet.SkillBulletCfg = _skillBulletCfg;
            //bullet.target = _target;
            bullet.Initialize();
            bullet.SetDirection(dir);
            SkillManager.instance.AddSpell(_caster.GlobalID, bullet);
        }
        GameObject.Destroy(temp.gameObject);
    }
}