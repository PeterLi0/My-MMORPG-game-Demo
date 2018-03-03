
//using common;

///// <summary>
///// 最先攻击可移动单位
///// </summary>
//public class BehaviorHarm : BaseBehavior
//{
//    public override void Update(float dt)
//    {

//        if (!_self.alive) return;


//        if (_self.dangerours)
//        {
//            _self.Defend();
//        }
//        else
//        {
//            _target = _self.FindNearestTypeEnemyInSight(RoleType.Hero);

//            if (_target == null)
//                _target = _self.FindNearestTypeEnemyInSight(RoleType.Soldier);

//            if (_target == null)
//                _target = _self.FindNearestTypeEnemy(RoleType.Tower);


//            if (_target != null)
//                _self.Chase(_target);
//        }        
//    }
//}
