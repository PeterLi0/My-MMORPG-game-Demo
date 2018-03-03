using Luna3D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class Character
{
    public int characterid;

    public int sceneid;

    public CharacterAttr attr;

    public common.BattleType battleType;

    protected RoleTransform _transform;

    // 导航代理
    protected LunaNavAgent _navmeshAgent;

    protected Vector3 _dest;

    public virtual void Init(CharacterAttr attr)
    {
        this.attr = attr;
        this.characterid = attr.id;
        this.sceneid = attr.mapid;

        _transform = new RoleTransform();
    }

    public void InitNavmeshAgent(LunaNavmeshQuery navmesh)
    {
        // 初始化导航代理
        _navmeshAgent = new LunaNavAgent();
        _navmeshAgent.Init(_transform, navmesh);
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
        }
    }

    /// <summary>
    /// 待机
    /// </summary>
    public virtual void Idle()
    {
        if (_navmeshAgent.enabled)
            _navmeshAgent.enabled = false;

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