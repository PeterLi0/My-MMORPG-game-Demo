using System;
using System.Collections.Generic;

using proto.inventory;
using common;

public class InventoryHandler : IMsgHandler
{
    public void RegisterMsg(Dictionary<MsgID, Action<UserToken, SocketModel>> handlers)
    {
        handlers.Add(MsgID.INV_ItemInfos_CREQ, OnInvItemInfos);
        handlers.Add(MsgID.INV_Equip_CREQ, OnInvEquip);
        handlers.Add(MsgID.INV_Unload_CREQ, OnInvUnloadEquip);
        handlers.Add(MsgID.INV_Delete_Item_CREQ, OnDeleteItem);
    }

    // 获取已有物品信息
    private void OnInvItemInfos(UserToken token, SocketModel model)
    {
        List<InventoryData> invs = CacheManager.instance.GetInvDatas(token.characterid);
        List<EquipData> equips = CacheManager.instance.GetEquipDatas(token.characterid);

        RespItemInfos resp = new RespItemInfos();

        foreach (InventoryData inv in invs)
        {
            InventoryDTO dto = InventoryData.GetInvDTO(inv);
            resp.inventorys.Add(dto);
        }

        foreach(EquipData equip in equips)
        {
            InventoryDTO dto = EquipData.GetInvDTO(equip);
            resp.equips.Add(dto);
        }

        NetworkManager.Send(token, (int)MsgID.INV_ItemInfos_SRES, resp);
    }

    // 装备物品
    private void OnInvEquip(UserToken token, SocketModel model)
    {
        ReqEquipItem req = SerializeUtil.Deserialize<ReqEquipItem>(model.message);

        // 获取背包栏位物品数据
        InventoryData invData = CacheManager.instance.GetInvData(token.characterid, req.slot);

        // 获取装备栏位物品数据
        ItemCfg itemCfg = ConfigManager.instance.GetItemCfg(req.itemid);
        EquipData equipData = CacheManager.instance.GetEquipData(token.characterid, (int)itemCfg.EquipType);

        int itemid = invData.itemid;
        invData.itemid = equipData.itemid;
        equipData.itemid = itemid;

        if (invData.itemid == -1)
            invData.num = 0;

        RespEquipItem resp = new RespEquipItem();

        resp.equip = EquipData.GetInvDTO(equipData);
        resp.inv = InventoryData.GetInvDTO(invData);

        NetworkManager.Send(token, (int)MsgID.INV_Equip_SRES, resp);
    }

    // 卸载物品
    private void OnInvUnloadEquip(UserToken token, SocketModel model)
    {
        ReqUnloadItem req = SerializeUtil.Deserialize<ReqUnloadItem>(model.message);

        // 查找第一个空的栏位
        int firstEmptySlot = CacheManager.instance.GetFirstEmptySlot(token.characterid);

        EquipData equipData = CacheManager.instance.GetEquipData(token.characterid, req.slot);

        // 获取背包栏位物品数据
        InventoryData invData = CacheManager.instance.GetInvData(token.characterid, firstEmptySlot);

        int itemid = invData.itemid;
        invData.itemid = equipData.itemid;
        equipData.itemid = itemid;

        RespUnloadItem resp = new RespUnloadItem();

        resp.equip = EquipData.GetInvDTO(equipData);
        resp.inv = InventoryData.GetInvDTO(invData);

        NetworkManager.Send(token, (int)MsgID.INV_Unload_SRES, resp);
    }

    // 删除物品
    private void OnDeleteItem(UserToken token , SocketModel model)
    {
        ReqDeleteItem req = SerializeUtil.Deserialize<ReqDeleteItem>(model.message);

        // 应答
        RespDeleteItem resp = new RespDeleteItem();

        if (req.deleteType == DeleteType.Equip)
        {
            EquipData equipData = CacheManager.instance.GetEquipData(token.characterid, req.slot);
            equipData.itemid = -1;
            resp.dto = EquipData.GetInvDTO(equipData);
            resp.deleteType = DeleteType.Equip;

            equipData = CacheManager.instance.GetEquipData(token.characterid, req.slot);
        }
        else
        {
            // 获取背包栏位物品数据
            InventoryData invData = CacheManager.instance.GetInvData(token.characterid, req.slot);
            invData.itemid = -1;
            invData.num = 0;
            resp.dto = InventoryData.GetInvDTO(invData);
            resp.deleteType = DeleteType.Inv;
        }

        NetworkManager.Send(token, (int)MsgID.INV_Delete_Item_SRES, resp);
    }
}
