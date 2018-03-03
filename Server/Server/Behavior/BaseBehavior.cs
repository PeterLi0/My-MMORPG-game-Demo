//using Luna3D;
//using System;
//using System.Collections.Generic;


///// <summary>
///// 基础行为
///// </summary>
//public abstract class BaseBehavior
//{
//    protected Character _host;

//    public BaseBehavior(Character host)
//    {
//        _host = host;
//    }
    
//    public virtual void Update(float dt)
//    {
//        if (!_host.alive) return;
//    }

//    /// <summary>
//    /// 查找视野范围内的最近的敌人
//    /// </summary>
//    /// <returns></returns>
//    public Character FindNearestEnemyInSight()
//    {
//        Dictionary<int, Character> enemies = BattleCharacters.instance.GetNonSideCharacters(_host.side);

//        Character enemy = null;
//        float nearest = float.MaxValue;

//        foreach(Character ch in enemies.Values)
//        {
//            float distance = Vector3.Distance(ch.position, _host.position);
//            if (distance <= _host.sight && distance <= nearest)
//            {
//                enemy = ch;
//                nearest = distance;
//            }
//        }

//        return enemy;
//    }

//    /// <summary>
//    /// 追击
//    /// </summary>
//    public void Chase()
//    {
//        SkillCaster caster = GetSkillCaster();
//        if(caster == null)
//        {
//            Vector3 targetPosition = _host.lockedTarget.position + (_host.position -_host.lockedTarget.position).normalized * 1f;
//            _host.Move(targetPosition);
//        }
//        else
//        {
//            _host.Cast(caster);
//        }
//    }

//    /// <summary>
//    /// 获取已经冷却完成的且满足施法范围的技能发射器
//    /// </summary>
//    /// <returns></returns>
//    public SkillCaster GetSkillCaster()
//    {
//        SkillCaster caster = null;
//        foreach(SkillCaster c in _host.skillCasters.Values)
//        {
//            float distance = Vector3.Distance(_host.position, _host.lockedTarget.position);
//            if(c.cooldown && distance <= c.maxRange)
//            {
//                caster = c;
//            }
//        }

//        return caster;
//    }
//}



