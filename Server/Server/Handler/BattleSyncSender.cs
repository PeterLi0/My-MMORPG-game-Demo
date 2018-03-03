
using Luna3D;
using System.Collections.Generic;
using proto.battlesync;

public class BattleSyncSender
{
    /// <summary>
    /// 通知移动
    /// </summary>
    /// <param name="characterid">角色全局ID</param>
    /// <param name="position">目标点</param>
    /// <param name="accounts">所有账号</param>
    public static void NotifyMove(uint characterid, Vector3 position, Dictionary<string, Account> accounts)
    {
        NotifyCharacterMove notify = new NotifyCharacterMove();
        notify.characterid = characterid;
        notify.position = ProtoHelper.LV2PV(position);

        MsgSender.BroadCast(accounts, MsgID.NotifyCharacterMove, notify);

    }

    /// <summary>
    /// 通知待机，停止移动
    /// </summary>
    /// <param name="globalRoleID"></param>
    /// <param name="accounts"></param>
    public static void NotifyIdle(uint characterid, Dictionary<string, Account> accounts)
    {
        NotifyCharacterIdle notify = new NotifyCharacterIdle();
        notify.characterid = characterid;
        MsgSender.BroadCast(accounts, MsgID.NotifyCharacterIdle, notify);
    }

    /// <summary>
    /// 通知攻击事件
    /// </summary>
    /// <param name="globalRoleID">攻击者的全局ID</param>
    /// <param name="skillID">技能ID</param>
    /// <param name="targetid">目标的全局ID</param>
    /// <param name="position">坐标</param>
    /// <param name="accounts">所有账号</param>
    public static void NotifyAttack(uint characterid, uint skillID, uint targetid, Vector3 position, Dictionary<string, Account> accounts)
    {
        NotifyCharacterAttack  notify = new NotifyCharacterAttack();
        notify.characterid = characterid;
        notify.skillid = skillID;
        notify.pos = ProtoHelper.LV2PV(position);
        notify.targetid = targetid;
        MsgSender.BroadCast(accounts, MsgID.NotifyCharacterAttack, notify);
    }

    /// <summary>
    /// 通知血量改变
    /// </summary>
    /// <param name="targetid">目标的全局ID</param>
    /// <param name="hp">血量</param>
    /// <param name="accounts">所有账号</param>
    public static void NotifyChangeHP(uint targetid, int hp, Dictionary<string, Account> accounts)
    {
        NotifyHPChange notify = new NotifyHPChange();
        notify.targetid = targetid;
        notify.hp = hp;

        MsgSender.BroadCast(accounts, MsgID.NotifyHPChange, notify);
    }

    /// <summary>
    /// 通知角色死亡
    /// </summary>
    /// <param name="characterid">角色全局ID</param>
    /// <param name="accounts">要通知的所有账号</param>
    public static void NotifyDie(uint characterid, Dictionary<string, Account> accounts)
    {
        NotifyCharacterDie notify = new NotifyCharacterDie();
        notify.characterid = characterid;
        MsgSender.BroadCast(accounts, MsgID.NotifyCharacterDie, notify);
    }
}