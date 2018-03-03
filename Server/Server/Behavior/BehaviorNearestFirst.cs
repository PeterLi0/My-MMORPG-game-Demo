
//using common;

///// <summary>
///// 最先攻击可移动单位
///// </summary>
//public class BehaviorNearestFirst : BaseBehavior
//{
//    public override void Update(float dt)
//    {

//        if (!_self.alive) return;

//        _target = _self.FindNearestTypeEnemyInSight(RoleType.Soldier);

//        if(_target == null)
//            _target = _self.FindNearestTypeEnemyInSight(RoleType.Hero);

//        if (_target == null)
//            _target = _self.FindNearestTypeEnemy(RoleType.Tower);


//        if(_target != null)
//            _self.Chase(_target);        
//    }
//}
