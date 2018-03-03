//using common;
//using System;
//using System.Collections.Generic;
//using UnityEngine;


//public class IDAgent : MonoBehaviour
//{
//    public int globalID;
//}

//public abstract class Character
//{
//    // 导航代理
//    // 导航代理
//    protected LunaNavAgent _navmeshAgent;

//    // 动画
//    protected Animation _animation;

//    // 碰撞器
//    protected CapsuleCollider _collider;

//    protected Transform _transform;

//    // 目标点
//    private Vector3 _goalPosition;

//    // 距离目标点的检测到达的距离
//    private const float _reachedDistance = 0.3f;

//    protected RoleCfg _cfg;

//    // 角色全局ID
//    private int _globalID;

//    // 角色ID代理
//    private IDAgent _idAgent;

//    // 锁定的目标
//    protected Character _lockedTarget;

//    // 原始属性
//    public CharacterAttr _originAttr;

//    // 当前属性，实时属性
//    public CharacterAttr _currAttr;

//    // 技能发射器
//    protected Dictionary<int, SkillCaster> _skillCasters;


//    public Race side;

//    // 血量改变
//    public Action<Character, int, BloodTextType> hpChanged;

//    // 角色死亡事件
//    public Action<Character> characterDie;

//    protected bool _alive = true;

//    // 角色行为
//    protected BaseBehavior _behavior;

//    //public bool online = false;
//    private RoleCfg roleCfg;
//    private Vector3 position1;

//    public int GlobalID
//    {
//        get { return _globalID; }
//        set
//        {
//            _globalID = value;
//            _idAgent.globalID = _globalID;
//        }
//    }

//    public Character lockedTarget { get { return _lockedTarget; } set { _lockedTarget = value; } }

//    // 是否活着
//    public bool alive { get { return _alive; } }

//    // 实时坐标
//    public Vector3 position { get { return _transform.position; } set { _transform.position = value; } }

//    public Vector3 forward { get { return _transform.forward; } }

//    // 总血量
//    public int totalHp { get { return _cfg.Hp; } }

//    // 当前血量
//    public int currHp { get { return _currAttr.Hp; } }

//    // 当前移动速度
//    public float speed
//    {
//        get
//        {
//            return _currAttr.MoveSpeed;
//        }
//        set
//        {
//            _currAttr.MoveSpeed = value;
//            _navmeshAgent.MaxSpeed = _currAttr.MoveSpeed;
//        }
//    }

//    // 攻击速度
//    public float attackSpeed
//    {
//        get { return _currAttr.AttackSpeed; }
//        set { _currAttr.AttackSpeed = value; }
//    }

//    // 防御力
//    public float defend
//    {
//        get { return _currAttr.Defend; }
//        set { _currAttr.Defend = value; }
//    }

//    public Transform transform { get { return _transform; } }

//    public string name { get { return _cfg.RoleName; } }

//    public float sight { get { return _currAttr.Sight; } }

//    public Dictionary<int, SkillCaster> skillCasters { get { return _skillCasters; } }

//    public Character(int globalid, RoleCfg roleCfg, Vector3 position)
//    {
//        _globalID = globalid;

//        _cfg = roleCfg;

//        // 通过配置获取原始属性
//        _originAttr = CharacterAttr.GetAttr(roleCfg);

//        // 获取当前属性
//        _currAttr = _originAttr.Clone();

//        // 对象池创建对象
//        _transform = (PoolManager.instance.Spawn("Units/", _cfg.ModelName)).transform;
//        _transform.position = position;
//        _transform.localScale = Vector3.one;


//        _animation = _transform.gameObject.GetComponent<Animation>();

//        // 在角色的GameObject上添加一个碰撞器
//        _collider = _transform.gameObject.GetComponent<CapsuleCollider>();
//        if (_collider == null)
//            _collider = _transform.gameObject.AddComponent<CapsuleCollider>();
//        _collider.center = new Vector3(0, 1, 0);
//        _collider.height = 2;

//        // 添加ID代理
//        _idAgent = _transform.gameObject.GetComponent<IDAgent>();
//        if (_idAgent == null)
//            _idAgent = _transform.gameObject.AddComponent<IDAgent>();
//        _idAgent.globalID = globalid;


//        // 创建角色拥有的技能发射器
//        _skillCasters = new Dictionary<int, SkillCaster>();
//        Dictionary<int, SkillGroupCfg> skillGroupCfgs = ConfigManager.instance.GetSkillGroupCfgs(_cfg.ID);
//        foreach (SkillGroupCfg cfg in skillGroupCfgs.Values)
//        {
//            SkillCaster caster = new SkillCaster(this);
//            caster.SkillBasicCfg = ConfigManager.instance.GetSkillBasicCfg(cfg.ID);
//            caster.SkillBulletCfg = ConfigManager.instance.GetSkillBulletCfg(cfg.ID);
//            caster.SkillAOECfg = ConfigManager.instance.GetSkillAOECfg(cfg.ID);
//            caster.SkillBuffCfg = ConfigManager.instance.GetSkillBuffCfg(cfg.ID);
//            caster.SkillTrapCfg = ConfigManager.instance.GetSkillTrapCfg(cfg.ID);

//            if (!_skillCasters.ContainsKey(cfg.ID))
//                _skillCasters.Add(cfg.ID, caster);
//        }
//    }

//    public void InitNavmeshAgent(LunaNavmeshQuery navmesh)
//    {
//        // 初始化导航代理
//        _navmeshAgent = new LunaNavAgent();
//        _navmeshAgent.Init(navmesh, _transform, Idle, Moving);
//    }



//    public void Moving()
//    {

//    }

//    /// <summary>
//    /// 是否可移动
//    /// </summary>
//    /// <returns></returns>
//    public bool CanMove()
//    {
//        bool canMove = true;
//        foreach(SkillCaster skillCaster in _skillCasters.Values)
//        {
//            // 如果这个技能是不能移动施法的， 且当前技能正在释放，则角色不能移动
//            if (!skillCaster.canMove && skillCaster.casting)
//                canMove = false;
//        }

//        return canMove;
//    }

//    /// <summary>
//    /// 移动到目标点
//    /// </summary>
//    /// <param name="dst"></param>
//    public virtual void Move(Vector3 dst)
//    {
//        // 如果角色正在释放不可移动的技能，则返回
//        if (!CanMove()) return;

//        if (_goalPosition != dst)
//        {
//            if (!_navmeshAgent.enabled)
//                _navmeshAgent.enabled = true;

//            _goalPosition = dst;

//            _navmeshAgent.Move(dst);
//            _animation.CrossFade(_cfg.MoveAnim, 0.3f);
//        }
//    }

//    /// <summary>
//    /// 待机
//    /// </summary>
//    public virtual void Idle()
//    {
//        if (_navmeshAgent.enabled)
//            _navmeshAgent.enabled = false;

//        _animation.CrossFade(_cfg.IdleAnim, 0.3f);
//    }

//    public virtual void Update(float dt)
//    {
//        //CheckReach();
//        CheckAnimation();

//    }

//    private void CheckAnimation()
//    {
//        if (!_animation.isPlaying)
//            _animation.CrossFade(_cfg.IdleAnim);
//    }

//    /// <summary>
//    /// 攻击
//    /// </summary>
//    /// <param name="skillid"></param>
//    public virtual void Attack(SkillBasicCfg cfg)
//    {
//        // 攻击时不能移动
//        if (_navmeshAgent.enabled) _navmeshAgent.enabled = false;

//        // 设置攻击朝向
//        if (cfg.NeedTarget && _globalID != _lockedTarget.GlobalID)
//            _transform.forward = (_lockedTarget.position - _transform.position).normalized;

//        // 播放动画
//        _animation.CrossFade(cfg.AnimName);
//    }

//    /// <summary>
//    /// 受伤
//    /// </summary>
//    /// <param name="hp"></param>
//    public virtual void Wound(int hp)
//    {
//        if (_navmeshAgent.enabled) _navmeshAgent.enabled = false;

//        Idle();
//        if (_animation["Wound"] != null)
//            _animation.CrossFade("Wound");

//        _currAttr.Hp -= hp;

//        // 血量改变
//        hpChanged(this, hp, BloodTextType.Damage);

//        if (_currAttr.Hp <= 0)
//            _currAttr.Hp = 0;

//        if (_alive && _currAttr.Hp <= 0) Die();
//    }

//    /// <summary>
//    /// 恢复
//    /// </summary>
//    /// <param name="hp"></param>
//    public virtual void Recover(int hp)
//    {

//    }

//    /// <summary>
//    /// 死亡
//    /// </summary>
//    public virtual void Die()
//    {
//        _alive = false;

//        _animation.CrossFade("Death");

//        TimerManager.instance.Invoke(1.5f, () =>
//        {
//            characterDie(this);
//            PoolManager.instance.Unspawn(_transform.gameObject);
//        });
//    }

//    public virtual void Cast(SkillCaster caster) { }

//    public virtual void Finalise() { }

//    public virtual void Leave()
//    {
//        PoolManager.instance.Unspawn(_transform.gameObject);
//    }
//}