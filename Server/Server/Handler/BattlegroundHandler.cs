using System;
using System.Collections.Generic;
using common;


public class BattlegroundHandler : IMsgHandler
{
    public void RegisterMsg(Dictionary<MsgID, Action<UserToken, SocketModel>> handlers)
    {
        handlers.Add(MsgID.ReqMatchBattleground, ReqMatchBattleground);
        handlers.Add(MsgID.ReqCancelMatchBattleground, ReqCancelMatchBattleground);
        handlers.Add(MsgID.ReqStartBattleground, ReqStartBattleground);
        handlers.Add(MsgID.RepEnterBattleground, RepEnterBattleground);
        handlers.Add(MsgID.ReqExitBattleground, ReqExitBattleground);
    }

    // 匹配竞技场战斗请求
    private void ReqMatchBattleground(UserToken token, SocketModel model)
    {

    }

    // 取消竞技场战斗匹配请求
    private void ReqCancelMatchBattleground(UserToken token, SocketModel model)
    {

    }

    // 开始竞技场战斗请求
    private void ReqStartBattleground(UserToken token, SocketModel model)
    {

    }

    // 报告服务器进入竞技场
    private void RepEnterBattleground(UserToken token, SocketModel model)
    {

    }

    // 退出竞技场战斗请求
    private void ReqExitBattleground(UserToken token, SocketModel model)
    {

    }
}