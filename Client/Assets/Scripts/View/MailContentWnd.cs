using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using common;
using proto.mail;

/// <summary>
/// 邮件内容界面
/// </summary>
public class MailContentWnd : BaseWnd
{
    private MailDTO _mail;

    public void Initialize(MailDTO dto)
    {
        _mail = dto;

        Button btnClose = _transform.Find("BtnClose").GetComponent<Button>();
        btnClose.onClick.AddListener(OnBtnCloseClick);

        // 标题
        Text title = _transform.Find("Title").GetComponent<Text>();
        title.text = dto.subject;

        // 内容
        Text content = _transform.Find("Content").GetComponent<Text>();
        content.text = dto.body;

        // 邮件物品


        // 领取物品

        // 删除邮件
        Button btnDelete = _transform.Find("BtnDelete").GetComponent<Button>();
        btnDelete.onClick.AddListener(OnBtnDeleteClick);
    }

    private void OnBtnCloseClick()
    {
        WindowManager.instance.Close<MailContentWnd>();
    }

    private void OnBtnDeleteClick()
    {
        ReqDeleteMail req = new ReqDeleteMail();
        req.mailid = _mail.id;
        Net.instance.Send((int)MsgID.Delete_Mail_CREQ, req);
    }
}

