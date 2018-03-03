using System;
using System.Collections.Generic;
using UnityEngine;
using common;
using account;
using proto.character;

public class AccountHandler 
{
    public void RegisterMsg(Dictionary<MsgID, Action<SocketModel>> handlers)
    {
        handlers.Add(MsgID.ACC_LOGIN_SRES, OnLoginResp);
        handlers.Add(MsgID.ACC_REG_SRES, OnRegisterResp);
        handlers.Add(MsgID.ACC_OFFLINE_SRES, OnOffline);
    }

    private void OnLoginResp(SocketModel model)
    {
        RespLogin resp = SerializeUtil.Deserialize<RespLogin>(model.message);

        if(resp.msgtips == (uint)MsgTips.LoginSuccess)
        {
            // 请求获取角色信息
            ReqCharactersInfo req = new ReqCharactersInfo();
            req.characterid = 0;
            Net.instance.Send((int)MsgID.CHAR_INFO_CREQ, req);
        }
        else
        {
            MessageBox.Show(((MsgTips)resp.msgtips).ToString());
        }
    }

    private void OnRegisterResp(SocketModel model)
    {
        RespRegister req = SerializeUtil.Deserialize<RespRegister>(model.message);
        if (req.msgtips == (uint)MsgTips.RegisterSuccess)
        {
            MessageBox.Show("注册成功");
        }
        else
        {
            MessageBox.Show(((MsgTips)req.msgtips).ToString());
        }
    }

    private void OnOffline(SocketModel model)
    {
        RespOffline resp = SerializeUtil.Deserialize<RespOffline>(model.message);
        if(resp.msgtips == (uint)MsgTips.AccountOffline)
        {
            SelectRole.instance.Finalise();
            Loading.instance.LoadScene("Login");
        }
    }
}