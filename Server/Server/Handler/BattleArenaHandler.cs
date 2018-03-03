using System;
using System.Collections.Generic;
using common;


public class BattleArenaHandler : IMsgHandler
{
    public void RegisterMsg(Dictionary<MsgID, Action<UserToken, SocketModel>> handlers)
    {
        handlers.Add(MsgID.ReqMatchArena, ReqMatchArena);
        handlers.Add(MsgID.ReqCancelMatchArena, ReqCancelMatchArena);
        handlers.Add(MsgID.ReqStartArena, ReqStartArena);
        handlers.Add(MsgID.RepEnterArena, RepEnterArena);
        handlers.Add(MsgID.ReqExitArena, ReqExitArena);
    }

    // 匹配竞技场战斗请求
    private void ReqMatchArena(UserToken token, SocketModel model)
    {
         
    }

    // 取消竞技场战斗匹配请求
    private void ReqCancelMatchArena(UserToken token, SocketModel model)
    {

    }

    // 开始竞技场战斗请求
    private void ReqStartArena(UserToken token, SocketModel model)
    {

    }

    // 报告服务器进入竞技场
    private void RepEnterArena(UserToken token, SocketModel model)
    {

    }

    // 退出竞技场战斗请求
    private void ReqExitArena(UserToken token, SocketModel model)
    {

    }
   
}