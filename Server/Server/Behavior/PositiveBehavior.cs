//using System;
//using System.Collections.Generic;


///// <summary>
///// 主动攻击行为
///// </summary>
//public class PositiveBehavior : BaseBehavior
//{
//    public PositiveBehavior(Character host) : base(host)
//    {

//    }

//    public override void Update(float dt)
//    {
//        base.Update(dt);

//        // 查找距离自己最近的敌方单位
//        if (_host.lockedTarget == null)
//            _host.lockedTarget = FindNearestEnemyInSight();

//        // 如果查找到了敌人，则追击
//        if (_host.lockedTarget != null)
//            Chase();
//    }
//}