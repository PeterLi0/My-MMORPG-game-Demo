using System;
using System.Collections.Generic;
using proto.inventory;


public class InventroyHandler 
{
    public void RegisterMsg(Dictionary<MsgID, Action<SocketModel>> handlers)
    {
        handlers.Add(MsgID.INV_ItemInfos_SRES, OnInvItemInfos);
        handlers.Add(MsgID.INV_Equip_SRES, OnEquipItem);
        handlers.Add(MsgID.INV_Unload_SRES, OnUnloadItem);
        handlers.Add(MsgID.INV_Delete_Item_SRES, OnDeleteItem);
    }

    private void OnInvItemInfos(SocketModel model)
    {
        RespItemInfos resp = SerializeUtil.Deserialize<RespItemInfos>(model.message);
        WindowManager.instance.Open<InventoryWnd>().Initialize(resp.inventorys, resp.equips);
    }

    private void OnEquipItem(SocketModel model)
    {
        RespEquipItem resp = SerializeUtil.Deserialize<RespEquipItem>(model.message);

        InventoryWnd invWnd = WindowManager.instance.Get<InventoryWnd>();
        invWnd.UpdateEquip(resp.equip);
        invWnd.UpdateInv(resp.inv);

        WindowManager.instance.Close<InventoryEquipWnd>();
    }

    private void OnUnloadItem(SocketModel model)
    {
        RespUnloadItem resp = SerializeUtil.Deserialize<RespUnloadItem>(model.message);

        InventoryWnd invWnd = WindowManager.instance.Get<InventoryWnd>();
        invWnd.UpdateEquip(resp.equip);
        invWnd.UpdateInv(resp.inv);

        WindowManager.instance.Close<InventoryEquipWnd>();
    }

    // 删除物品的应答
    private void OnDeleteItem(SocketModel model)
    {
        RespDeleteItem resp = SerializeUtil.Deserialize<RespDeleteItem>(model.message);
        InventoryWnd invWnd = WindowManager.instance.Get<InventoryWnd>();
        if (resp.deleteType ==  DeleteType.Equip)
            invWnd.UpdateEquip(resp.dto);
        else
            invWnd.UpdateInv(resp.dto);

        WindowManager.instance.Close<InventoryEquipWnd>();
    }
}