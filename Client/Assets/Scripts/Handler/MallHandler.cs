using System;
using System.Collections.Generic;
using common;
using proto.mall;

public class MallHandler 
{
    public void RegisterMsg(Dictionary<MsgID, Action<SocketModel>> handlers)
    {
        handlers.Add(MsgID.BuyGoods_SRES, OnBuyGoodsRsp);
        handlers.Add(MsgID.MallInfo_SRES, OnMallInfoRsp);
    }

    private void OnMallInfoRsp(SocketModel model)
    {
        RspMallInfo rsp = SerializeUtil.Deserialize<RspMallInfo>(model.message);
        WindowManager.instance.Open<MallWnd>().Init(rsp.goods);
    } 

    private void OnBuyGoodsRsp(SocketModel model)
    {
        RspBuyGoods rsp = SerializeUtil.Deserialize<RspBuyGoods>(model.message);
        if(rsp.msgtips == (uint)MsgTips.BuyGoodsSuccess)
        {
            WindowManager.instance.Close<BuyTypeWnd>();
            DataCache.instance.currentCharacter.gold = (int)rsp.gold;
            DataCache.instance.currentCharacter.diamond = (int)rsp.diamond;
            WindowManager.instance.Get<MallWnd>().UpdateGoldDiamond((int)rsp.gold, (int)rsp.diamond);
        }
        else if(rsp.msgtips == (uint)MsgTips.DiamondNotEnough)
        {
            MessageBox.Show("钻石不足");
        }
        else if (rsp.msgtips == (uint)MsgTips.GoldNotEnough)
        {
            MessageBox.Show("金币不足");
        }
    }
}