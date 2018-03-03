
using System;
using System.Collections.Generic;

using proto.mail;
using common;

public class MailHandler : IMsgHandler
{
    public void RegisterMsg(Dictionary<MsgID, Action<UserToken, SocketModel>> handlers)
    {
        handlers.Add(MsgID.MailInfos_CREQ, OnMailInfos);
        handlers.Add(MsgID.Delete_Mail_CREQ, OnDeleteMail);
        handlers.Add(MsgID.Send_CREQ, OnSendMail);
        handlers.Add(MsgID.RecvItem_CREQ, OnRecvItem);
    }

    private void OnMailInfos(UserToken token, SocketModel model)
    {
        //List<MailData> mails = CacheManager.instance.GetPlayerData(token.characterid).mails;

        List<MailData> mails = CacheManager.instance.GetMailDatas(token.characterid);

        RespMailInfos resp = new RespMailInfos();

        foreach(MailData d in mails)
        {
            MailDTO dto = MailData.GetMailDTO(d);
            resp.mails.Add(dto);
        }

        NetworkManager.Send(token, (int)MsgID.MailInfos_SRES, resp);
    }

    private void OnDeleteMail(UserToken token,SocketModel model)
    {
        ReqDeleteMail req = SerializeUtil.Deserialize<ReqDeleteMail>(model.message);
       
        CacheManager.instance.DeleteMail(token.characterid, req.mailid);

        RespDeleteMail resp = new RespDeleteMail();
        resp.mailid = req.mailid;
        resp.msgtips = (int)MsgTips.DeleteMailSuccess;
        NetworkManager.Send(token, (int)MsgID.Delete_Mail_SRES, resp);
    }

    private void OnSendMail(UserToken token, SocketModel model)
    {

    }

    private void OnRecvItem(UserToken token, SocketModel model)
    {

    }
}