using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using common;
using proto.inventory;

/// <summary>
/// 物品详细
/// </summary>
public class InventoryEquipWnd : BaseWnd
{
    private InventoryDTO dto;

    private DeleteType _deleteType;
    public void Initialize(InventoryDTO dto, DeleteType deleteType)
    {
        this.dto = dto;
        _deleteType = deleteType;

        ItemCfg item = ConfigManager.instance.GetItemCfg(dto.itemid);
        Transform content = _transform.Find("EquipInfo/Viewport/Content");
        Text name = content.Find("Text").GetComponent<Text>();
        name.text = item.Name;
        Button Btnclose = _transform.Find("BtnClose").GetComponent<Button>();
        Btnclose.onClick.AddListener(OnClickClose);

        Button Btndelete = _transform.Find("BtnDelete").GetComponent<Button>();
        Btndelete.onClick.AddListener(OnClickDelete);

        Button BtnUnload = _transform.Find("BtnUnload").GetComponent<Button>();
        BtnUnload.onClick.AddListener(OnClickUnload);
        BtnUnload.gameObject.SetActive(deleteType == DeleteType.Equip);

        Button BtnEquip = _transform.Find("BtnEquip").GetComponent<Button>();
        BtnEquip.onClick.AddListener(OnClickEquip);
        BtnEquip.gameObject.SetActive(deleteType != DeleteType.Equip);
    }
    private void OnClickClose()
    {
        WindowManager.instance.Close<InventoryEquipWnd>();
    }
    private void OnClickDelete()
    {
        ReqDeleteItem req = new ReqDeleteItem();
        req.slot = dto.slot;
        req.deleteType = _deleteType;
        Net.instance.Send((int)MsgID.INV_Delete_Item_CREQ, req);
    }
    private void OnClickUnload()
    {
        ReqUnloadItem req = new ReqUnloadItem();
        req.slot = dto.slot;
        req.itemid = dto.itemid;
        Net.instance.Send((int)MsgID.INV_Unload_CREQ, req);
    }
    private void OnClickEquip()
    {
        ReqEquipItem req = new ReqEquipItem();
        req.slot = dto.slot;
        req.itemid = dto.itemid;
        Net.instance.Send((int)MsgID.INV_Equip_CREQ, req);
    }
}

