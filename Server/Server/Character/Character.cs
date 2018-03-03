//using common;
//using Luna3D;
//using System;
//using System.Collections.Generic;



//public abstract class Character
//{
//    protected RoleCfg _cfg;

//    // 角色全局ID
//    private int _globalID;

//    // 当前所在战斗
//    public Battle battle;

//    // 导航代理
//    private LunaNavAgent _navmeshAgent;

//    protected Transform _transform;

//    // 目标点
//    private Vector3 _goalPosition;

//    // 距离目标点的检测到达的距离
//    private const float _reachedDistance = 0.3f;

//    // 锁定的目标
//    public Character _lockedTarget;


//    // 原始战斗属性
//    public CharacterAttr originAttr;


//    // 当前战斗属性，实时属性
//    public CharacterAttr currAttr;


//    // 技能发射器
//    protected Dictionary<int, SkillCaster> _skillCasters;


//    // 角色死亡事件
//    public Action<Character> characterDie;

//    protected bool _alive = true;

//    // 角色行为
//    protected BaseBehavior _behavior;

//    public bool online = false;

//    public int GlobalID
//    {
//        get { return _globalID; }
//        set
//        {
//            _globalID = value;
//        }
//    }

//    public Character lockedTarget;

//    // 是否活着
//    public bool alive { get { return _alive; } }

//    // 实时坐标
//    public Vector3 position { get { return _transform.position; } }


//    // 总血量
//    public int totalHp { get { return _cfg.Hp; } }



//    // 当前移动速度
//    public float speed
//    {
//        get
//        {
//            return currAttr.moveSpeed;
//        }
//        set
//        {
//            currAttr.moveSpeed = value;
//            _navmeshAgent.MaxSpeed = currAttr.moveSpeed;
//        }
//    }

//    public string name { get { return _cfg.RoleName; } }

//    public float sight { get { return currAttr.sight; } }

//    public Dictionary<int, SkillCaster> skillCasters { get { return _skillCasters; } }

//    public Character(int globalid, CharacterAttr attr,  RoleCfg rolecfg)
//    {
//        _globalID = globalid;

//        _cfg = rolecfg;

//        // 通过配置获取原始属性
//        originAttr = attr;

//        // 获取当前属性
//        currAttr = originAttr.Clone();


//        _transform.position = position;
//        _transform.localScale = Vector3.one;


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
//        }
//    }

//    /// <summary>
//    /// 待机
//    /// </summary>
//    public virtual void Idle()
//    {
//        if (_navmeshAgent.enabled)
//            _navmeshAgent.enabled = false;
//    }

//    public virtual void Update(float dt)
//    {
//        CheckReach();
//    }

//    /// <summary>
//    /// 检测到达
//    /// </summary>
//    private void CheckReach()
//    {
       
//    }
//    /// <summary>
//    /// 攻击
//    /// </summary>
//    /// <param name="skillid"></param>
//    public virtual void Attack(SkillBasicCfg cfg)
//    {
//        // 攻击时不能移动
//        if (_navmeshAgent.enabled)
//            _navmeshAgent.enabled = false;

//    }

//    /// <summary>
//    /// 受伤
//    /// </summary>
//    /// <param name="hp"></param>
//    public virtual void Wound(int hp)
//    {
//        if (_navmeshAgent.enabled) _navmeshAgent.enabled = false;

//        Idle();

//        currAttr.hp -= hp;


//        if (currAttr.hp <= 0)
//            currAttr.hp = 0;

//        if (_alive && currAttr.hp <= 0) Die();
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
//    }

//    public virtual void Cast(SkillCaster caster) { }

//    public virtual void Finalise() { }
//}