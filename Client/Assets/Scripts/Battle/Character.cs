using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class Character
{
    public uint characterid;

    public int sceneid;

    public Vector3 position;

    protected Transform _transform;

    // 导航代理
    protected LunaNavAgent _navmeshAgent;

    protected Vector3 _dest;

    protected Animation _animation;

    protected RoleCfg _cfg;

    public virtual void Init(CharacterAttr attr)
    {
        this.characterid = (uint)attr.id;
        this.position = new Vector3(attr.pos.x, attr.pos.y, attr.pos.z);

        _cfg = ConfigManager.instance.GetRoleCfg(attr.cfgid);

        // 对象池创建对象
        _transform = (PoolManager.instance.Spawn("Units/", _cfg.ModelName)).transform;
        _transform.position = position;
        _transform.localScale = Vector3.one;

        _animation = _transform.GetComponent<Animation>();
    }

    public void InitNavmeshAgent(LunaNavmeshQuery navmesh)
    {
        // 初始化导航代理
        _navmeshAgent = new LunaNavAgent();
        _navmeshAgent.Init(navmesh, _transform, Idle, Moving);
    }

    public void Moving()
    {

    }

    /// <summary>
    /// 移动到目标点
    /// </summary>
    /// <param name="dst"></param>
    public virtual void Move(Vector3 dst)
    {
        // 如果角色正在释放不可移动的技能，则返回
        //if (!CanMove()) return;

        if (_dest != dst)
        {
            if (!_navmeshAgent.enabled)
                _navmeshAgent.enabled = true;

            _dest = dst;

            _navmeshAgent.Move(dst);
            _animation.CrossFade(_cfg.MoveAnim, 0.3f);
        }
    }

    /// <summary>
    /// 待机
    /// </summary>
    public virtual void Idle()
    {
        if (_navmeshAgent.enabled)
            _navmeshAgent.enabled = false;

        _animation.CrossFade(_cfg.IdleAnim, 0.3f);
    }

    public virtual void Update(float dt)
    {
        _navmeshAgent.Update(dt);
    }

    public virtual void Clear()
    {

    }


    public virtual void Wound(int hp)
    {

    }

    public virtual void Die()
    {

    }

    public virtual void Leave()
    {

    }
}