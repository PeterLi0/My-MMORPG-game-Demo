
//using common;

//public class BehaviorStandDefend : BaseBehavior
//{
//    public override void Update(float dt)
//    {
//        if (!_self.alive) return;

//        _target = _self.FindNearestTypeEnemyInSight(RoleType.Soldier);

//        if (_target == null)
//            _target = _self.FindNearestTypeEnemyInSight(RoleType.Hero);

//        if (_target == null)
//            _target = _self.FindNearestTypeEnemyInSight(RoleType.Monster);

//        if (_target != null)
//            _self.TryAttack();
//    }
//}