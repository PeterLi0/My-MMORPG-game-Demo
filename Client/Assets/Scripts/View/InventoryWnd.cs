using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using common;
using proto.inventory;

public class InventoryWnd : BaseWnd
{
    public class OnInvenItemClick : MonoBehaviour, IPointerClickHandler
    {
        public InventoryDTO dto;
        public void OnPointerClick(PointerEventData eventData)
        {
            if(dto != null)
                WindowManager.instance.Open<InventoryEquipWnd>().Initialize(dto, DeleteType.Inv);
        }
    }

    public class OnEquipItemClick : MonoBehaviour, IPointerClickHandler
    {
        public InventoryDTO dto;
        public void OnPointerClick(PointerEventData eventData)
        {
            if (dto != null)
                WindowManager.instance.Open<InventoryEquipWnd>().Initialize(dto, DeleteType.Equip);
        }
    }

    Transform _invContent;

    Transform _equipContent;

    const int EquipNumber = 6;

    const int SlotNumber = 50;

    Color EmptySlotColor = new Color(102 / 255f, 19 / 255f, 131 / 255f);

    public void Initialize(List<InventoryDTO> invs, List<InventoryDTO> equips)
    {
        _invContent = _transform.Find("Scroll View/Viewport/Content");

        Button btnReturn = _transform.Find("BtnClose").GetComponent<Button>();
        btnReturn.onClick.AddListener(OnReturn);

        Button btnItem = _transform.Find("Scroll View/Viewport/BtnItem").GetComponent<Button>();

        for (int i = 0; i < SlotNumber; i++)
        {
            Transform child = (GameObject.Instantiate(btnItem.gameObject) as GameObject).transform;
            child.SetParent(_invContent);
            child.localScale = Vector3.one;
            child.localPosition = Vector3.zero;
            child.gameObject.SetActive(true);

            child.gameObject.AddComponent<OnInvenItemClick>().dto = null;
        }

        // 背包栏位
        foreach (InventoryDTO item in invs)
        {
            if(item.itemid > 0)
            {
                Transform child = _invContent.GetChild(item.slot - 1);
                child.gameObject.GetComponent<OnInvenItemClick>().dto = item;
                child.Find("ItemID").GetComponent<Text>().text = item.itemid.ToString();

                ItemCfg cfg = ConfigManager.instance.GetItemCfg(item.itemid);
                Image img = child.Find("Image").GetComponent<Image>();
                img.overrideSprite = Resources.Load<Sprite>("Icon/" + cfg.Icon);
                img.color = Color.white;
            }
        }

        // 装备栏位
        Button btnEquip = _transform.Find("BtnEquip").GetComponent<Button>();


        _equipContent = _transform.Find("EquipContent");
        for (int i = 0; i < EquipNumber; i++)           // 遍历6次
        {
            Transform child = (GameObject.Instantiate(btnEquip.gameObject) as GameObject).transform;
            child.SetParent(_equipContent);
            child.localScale = Vector3.one;
            child.localPosition = Vector3.zero;
            child.gameObject.SetActive(true);

            child.gameObject.AddComponent<OnEquipItemClick>().dto = null;
        }

        for (int i = 0; i < equips.Count; i++)
        {
            InventoryDTO equip = equips[i];
            if(equip.itemid > 0)
            {
                ItemCfg cfg = ConfigManager.instance.GetItemCfg(equip.itemid);

                Transform child = _equipContent.GetChild(equip.slot - 1);
                Image img = child.Find("Image").GetComponent<Image>();
                img.overrideSprite = Resources.Load<Sprite>("Icon/" + cfg.Icon);
                img.color = Color.white;

                child.gameObject.GetComponent<OnEquipItemClick>().dto = equip;
            }
        }
    }

    public void UpdateEquip(InventoryDTO equip)
    {
        Transform child = _equipContent.GetChild(equip.slot - 1);
        child.GetComponent<OnEquipItemClick>().dto = equip;

        if (equip.itemid < 0)
        {
            Image img = child.Find("Image").GetComponent<Image>();
            img.overrideSprite = null;
            img.color = EmptySlotColor;
            child.GetComponent<OnEquipItemClick>().dto = null;
        }
        else
        {
            ItemCfg cfg = ConfigManager.instance.GetItemCfg(equip.itemid);
            Image img = child.Find("Image").GetComponent<Image>();
            img.overrideSprite = Resources.Load<Sprite>("Icon/" + cfg.Icon);
            img.color = Color.white;
        }
    }

    public void UpdateInv(InventoryDTO inv)
    {
        Transform child = _invContent.GetChild(inv.slot - 1);
        child.GetComponent<OnInvenItemClick>().dto = inv;

        if(inv.itemid < 0)
        {
            Image img = child.Find("Image").GetComponent<Image>();
            img.overrideSprite = null;
            img.color = EmptySlotColor;
            child.Find("ItemID").GetComponent<Text>().text = string.Empty;
            child.GetComponent<OnInvenItemClick>().dto = null;
        }
        else
        {
            ItemCfg cfg = ConfigManager.instance.GetItemCfg(inv.itemid);
            Image img = child.Find("Image").GetComponent<Image>(); 
            img.overrideSprite = Resources.Load<Sprite>("Icon/" + cfg.Icon);
            img.color = Color.white;
        }
    }

    private void OnReturn()
    {
        WindowManager.instance.Close<InventoryWnd>();
    }
}

