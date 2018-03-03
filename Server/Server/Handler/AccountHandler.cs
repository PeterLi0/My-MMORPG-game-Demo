using System;
using System.Collections.Generic;

using common;
using account;

public class AccountHandler : IMsgHandler
{
    public void RegisterMsg(Dictionary<MsgID, Action<UserToken, SocketModel>> handlers)
    {
        // 注册登录请求消息
        handlers.Add(MsgID.ACC_LOGIN_CREQ, OnLogin);
        handlers.Add(MsgID.ACC_REG_CREQ, OnRegister);
        handlers.Add(MsgID.ACC_OFFLINE_CREQ, OnOffline);
    }

    private void OnRegister(UserToken token, SocketModel model)
    {
        // 解析消息
        ReqRegister req = SerializeUtil.Deserialize<ReqRegister>(model.message);

        // 从数据库里查询有没有这个账号
        string sql = string.Format("select * from account where account = '{0}'", req.account);
        List<AccountData> accData = MysqlManager.instance.ExecQuery<AccountData>(sql);

        RespRegister resp = new RespRegister();
        if (accData.Count > 0)
        {
            resp.msgtips = (uint)MsgTips.AccountRepeat;
        }
        else
        {
            resp.msgtips = (uint)MsgTips.RegisterSuccess;

            // 往数据库里插入一个账号
            //sql = string.Format("insert into account(account,password) values('{0}','{1}')", req.account, req.password);
            //MysqlManager.instance.ExecNonQuery(sql);
        }

        // 给客户端发送一个应答
        NetworkManager.Send(token, (int)MsgID.ACC_REG_SRES, resp);
    }
    private void OnLogin(UserToken token, SocketModel model)
    {
        ReqLogin req = SerializeUtil.Deserialize<ReqLogin>(model.message);

        RespLogin resp = new RespLogin();

        string sql = string.Format("select * from account where account = '{0}'", req.account);
        List<AccountData> accDatas = MysqlManager.instance.ExecQuery<AccountData>(sql);
        if(accDatas.Count <= 0)         // 没有这个账号
        {
            resp.msgtips = (uint)MsgTips.NoAccount;
        }
        else                            // 有这个账号
        {
            AccountData acc = accDatas[0];

            if (CacheManager.instance.IsAccountOnline(acc.id))     // 账号已经在线，就不让再登录了
            {
                resp.msgtips = (uint)MsgTips.AccountHasOnline;
            }
            else
            {
                if (acc.password == req.password)
                {
                    resp.msgtips = (uint)MsgTips.LoginSuccess;

                    CacheManager.instance.AccountOnline(acc.id, acc.account, acc.password);

                    token.accountid = acc.id;
                }
                else
                {
                    resp.msgtips = (uint)MsgTips.PasswordError;
                }
            }
        }

        NetworkManager.Send(token, (int)MsgID.ACC_LOGIN_SRES, resp);
    }

    // 角色离线
    private void OnOffline(UserToken token, SocketModel model)
    {
        AccountData data = CacheManager.instance.GetAccount(token.accountid);
        CacheManager.instance.AccountOffline(data.id);

        RespOffline resp = new RespOffline();
        resp.msgtips = (uint)MsgTips.AccountOffline;
        NetworkManager.Send(token, (int)MsgID.ACC_OFFLINE_SRES, resp);
    }
}