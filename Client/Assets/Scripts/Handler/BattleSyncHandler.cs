using System;
using common;
using System.Collections.Generic;
using proto.battlesync;

public class BattleSyncHandler 
{
    public void RegisterMsg(Dictionary<MsgID, Action<SocketModel>> handlers)
    {
        handlers.Add(MsgID.NotifyCharacterDie, OnCharacterDie);
        handlers.Add(MsgID.NotifyCharacterAttack, OnCharacterAttack);
        handlers.Add(MsgID.NotifyCharacterMove, OnCharacterMove);
        handlers.Add(MsgID.NotifyCharacterIdle, OnCharacterIdle);
        handlers.Add(MsgID.NotifyHPChange, OnNotifyHpChange);
    }

    /// <summary>
    /// 角色移动应答
    /// </summary>
    /// <param name="data"></param>
    private void OnCharacterMove(SocketModel model)
    {
        NotifyCharacterMove notify = SerializeUtil.Deserialize<NotifyCharacterMove>(model.message);

        Character role = Battle.instance.GetRole(notify.characterid);
        role.Move(ProtoHelper.PV2UV(notify.position));
    }

    /// <summary>
    /// 角色停止移动
    /// </summary>
    /// <param name="data"></param>
    private void OnCharacterIdle(SocketModel model)
    {
        NotifyCharacterIdle notify = SerializeUtil.Deserialize<NotifyCharacterIdle>(model.message);
        Character role = Battle.instance.GetRole(notify.characterid);
        role.Idle();
    }

    /// <summary>
    /// 角色攻击应答
    /// </summary>
    /// <param name="data"></param>
    private void OnCharacterAttack(SocketModel model)
    {
        NotifyCharacterAttack notify = SerializeUtil.Deserialize<NotifyCharacterAttack>(model.message);

        Character role = Battle.instance.GetRole(notify.characterid);

        role.position = ProtoHelper.PV2UV(notify.pos);
        if (notify.pos == null)
        {
            LogManager.Log("OnRoleAttack, position = null");
            return;
        }

        //role.Attack((int)notify.skillid, notify.targetid);
    }

    /// <summary>
    /// 角色掉血应答
    /// </summary>
    /// <param name="data"></param>
    private void OnNotifyHpChange(SocketModel model)
    {
        //ServerMsg notify = SerializeUtil.Deserialize<ServerMsg>(model.message);
        NotifyHPChange notify = SerializeUtil.Deserialize<NotifyHPChange>(model.message);

        Character role = Battle.instance.GetRole(notify.targetid);

        int hp = Math.Abs(notify.hp);
        role.Wound(hp);
    }

    /// <summary>
    /// 角色死亡应答
    /// </summary>
    /// <param name="data"></param>
    private void OnCharacterDie(SocketModel model)
    {
        //ServerMsg notify = SerializeUtil.Deserialize<ServerMsg>(model.message);
        NotifyCharacterDie notify = SerializeUtil.Deserialize<NotifyCharacterDie>(model.message);
        Character role = Battle.instance.GetRole(notify.characterid);
        if (role != null)
            role.Die();
    }

    //private  void OnRoleBorn(SocketModel model)
    //{
    //    //ServerMsg resp = SerializeUtil.Deserialize<ServerMsg>(model.message);

    //    NotifyRoleBorn notify = SerializeUtil.Deserialize<NotifyRoleBorn>(model.message);
    //    for (int i = 0; i < notify.RoleList.Count; i++)
    //    {
    //        RoleData roledata = notify.RoleList[i];
    //        CreateRole(roledata);
    //    }
    //}

    ///// <summary>
    ///// 创建角色
    ///// </summary>
    ///// <param name="roledata"></param>
    //public static void CreateRole(RoleData roledata)
    //{
    //    // 通过服务器传来的角色ID获取当前角色的配置
    //    RoleCfg config = ConfigManager.instance.GetRoleCfg((int)roledata.RoleID);

    //    // 创建这个角色，并添加到容器中
    //    Character role = new Character(Battle.instance.navmesh, config, roledata); ;
    //    Battle.instance.AddRole(role.globalID, role);

    //    // 如果是玩家自己选的角色，
    //    //if (roledata.RoleID == DataCache.instance.MyHeroID)
    //    //if (roledata.RoleID == DataCache.instance.MyHeroID)
    //    //{
    //    //    Battle.instance.BindMainHero(role);
    //    //    WindowManager.instance.Open<BattleWnd>().Init(role);
    //    //}
    //}
}