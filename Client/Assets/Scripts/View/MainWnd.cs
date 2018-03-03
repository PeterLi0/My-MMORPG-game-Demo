using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using proto.inventory;
using proto.character;
using proto.mail;
using proto.mall;


public class MainWnd : BaseWnd
{
    public void Initialize()
    {
        Button bagBtn = _transform.Find("BtnBag").GetComponent<Button>();
        bagBtn.onClick.AddListener(OnBagBtnClicked);

        Button storeBtn = _transform.Find("BtnMall").GetComponent<Button>();
        storeBtn.onClick.AddListener(OnStoreBtnClicked);

        // 战斗按钮
        Button battleBtn = _transform.Find("BtnBattle").GetComponent<Button>();
        battleBtn.onClick.AddListener(OnBattleBtnClick);

        // 返回按钮
        Button returnBtn = _transform.Find("BtnReturn").GetComponent<Button>();
        returnBtn.onClick.AddListener(OnReturnBtnClick);

        Button btnMail = _transform.Find("BtnMail").GetComponent<Button>();
        btnMail.onClick.AddListener(OnBtnMailClick);
    }

    private void OnBagBtnClicked()
    {
        ReqGetItemInfo req = new ReqGetItemInfo();
        Net.instance.Send((int)MsgID.INV_ItemInfos_CREQ,req);
    }

    private void OnStoreBtnClicked()
    {
        ReqMallInfo req = new ReqMallInfo();
        Net.instance.Send((int)MsgID.MallInfo_CREQ, req);
    }

    private void OnBattleBtnClick()
    {
        //WindowManager.instance.Open<SelectLevelWnd>().Initialize();
    }

    private void OnBtnMailClick()
    {
        ReqMailInfos req = new ReqMailInfos();
        Net.instance.Send((int)MsgID.MailInfos_CREQ, req);
    }

    private void OnReturnBtnClick()
    {
        // 请求角色离线
        ReqCharacterOffline req = new ReqCharacterOffline();
        Net.instance.Send((int)MsgID.CHAR_OFFLINE_CREQ, req);
    }
}