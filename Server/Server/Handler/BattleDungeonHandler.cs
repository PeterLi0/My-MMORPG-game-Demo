using System;
using System.Collections.Generic;
using common;


public class BattleDungeonHandler : IMsgHandler
{
    public void RegisterMsg(Dictionary<MsgID, Action<UserToken, SocketModel>> handlers)
    {
        handlers.Add(MsgID.ReqMatchDungeon, ReqMatchDungeon);
        handlers.Add(MsgID.ReqCancelMatchDungeon, ReqCancelMatchDungeon);
        handlers.Add(MsgID.ReqStartDungeon, ReqStartDungeon);
        handlers.Add(MsgID.RepEnterDungeon, RepEnterDungeon);
        handlers.Add(MsgID.ReqExitDungeon, ReqExitDungeon);
    }

    // 匹配竞技场战斗请求
    private void ReqMatchDungeon(UserToken token, SocketModel model)
    {

    }

    // 取消竞技场战斗匹配请求
    private void ReqCancelMatchDungeon(UserToken token, SocketModel model)
    {

    }

    // 开始竞技场战斗请求
    private void ReqStartDungeon(UserToken token, SocketModel model)
    {

    }

    // 报告服务器进入竞技场
    private void RepEnterDungeon(UserToken token, SocketModel model)
    {

    }

    // 退出竞技场战斗请求
    private void ReqExitDungeon(UserToken token, SocketModel model)
    {

    }

}