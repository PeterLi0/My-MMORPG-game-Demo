using System;
using System.Collections.Generic;
using proto.mall;
using common;

public class MallHandler : IMsgHandler
{
    public void RegisterMsg(Dictionary<MsgID, Action<UserToken, SocketModel>> handlers)
    {
        handlers.Add(MsgID.MallInfo_CREQ, OnReqMallInfo);
        handlers.Add(MsgID.BuyGoods_CREQ, OnReqBuyGoods);
    }

    /// <summary>
    /// 请求商城信息的消息处理
    /// </summary>
    /// <param name="token"></param>
    /// <param name="model"></param>
    private void OnReqMallInfo(UserToken token, SocketModel model)
    {
        ReqMallInfo req = SerializeUtil.Deserialize<ReqMallInfo>(model.message);

        RspMallInfo rsp = new RspMallInfo();

        foreach (MallCfg item in ConfigManager.instance.mallCfgs.Values)
        {
            rsp.goods.Add((uint)item.ID);
        }

        NetworkManager.Send(token, (int)MsgID.MallInfo_SRES, rsp);
    }

    /// <summary>
    /// 请求购买商品的消息处理
    /// </summary>
    /// <param name="token"></param>
    /// <param name="model"></param>
    private void OnReqBuyGoods(UserToken token, SocketModel model)
    {
        ReqBuyGoods req = SerializeUtil.Deserialize<ReqBuyGoods>(model.message);
        
        MallCfg cfg = ConfigManager.instance.mallCfgs[(int)req.goodid];
        CharacterData ch = CacheManager.instance.GetCharData(token.characterid);

        RspBuyGoods rsp = new RspBuyGoods();

        // 金币不足
        if(req.buyType == BuyType.Gold )
        {
            if(ch.gold < cfg.Gold)
                rsp.msgtips = (uint)MsgTips.GoldNotEnough;
            else
            {
                // 更新缓存
                ch.gold -= cfg.Gold;
                rsp.msgtips = (uint)MsgTips.BuyGoodsSuccess;
            }
        }
        // 钻石不足
        else if(req.buyType == BuyType.Diamon)
        {
            if (ch.diamond < cfg.Diamond)
                rsp.msgtips = (uint)MsgTips.DiamondNotEnough;
            else
            {
                ch.diamond -= cfg.Diamond;
                rsp.msgtips = (uint)MsgTips.BuyGoodsSuccess;
            }
        }

        // 同步角色金币数据
        rsp.gold = (uint)ch.gold;
        rsp.diamond = (uint)ch.diamond;

        // 更新背包缓存
        int firstEmptySlot = CacheManager.instance.GetFirstEmptySlot(token.characterid);
        InventoryData  invData = CacheManager.instance.GetInvData(token.characterid, firstEmptySlot);
        invData.itemid = cfg.ItemID;
        invData.num = 1;

        rsp.inv = InventoryData.GetInvDTO(invData);

        NetworkManager.Send(token, (int)MsgID.BuyGoods_SRES, rsp);
    }
}
