using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using proto.mall;
using common;
using System.Collections.Generic;
using System;


/// <summary>
/// 选择购买方式
/// </summary>
public class BuyTypeWnd : BaseWnd
{
    // 商品id
    private uint _goodsid;

    private Button _btnReturn;

    private Button _btnGold;

    private Button _btnDiamond;
    public void Init(uint goodsid)
    {
        _goodsid = goodsid;

        _btnReturn = _transform.Find("BtnReturn").GetComponent<Button>();
        _btnReturn.onClick.AddListener(OnBtnReturnClick);
        _btnGold = _transform.Find("BtnGold").GetComponent<Button>();
        _btnGold.onClick.AddListener(OnBtnGoldClick);
        _btnDiamond = _transform.Find("BtnDiamond").GetComponent<Button>();
        _btnDiamond.onClick.AddListener(OnBtnDiamondClick);
    }

    private void OnBtnReturnClick()
    {
        WindowManager.instance.Close<BuyTypeWnd>();
    }

    private void OnBtnGoldClick()
    {
        ReqBuyGoods req = new ReqBuyGoods();
        req.goodid = (uint)_goodsid;
        req.buyType = BuyType.Gold;
        Net.instance.Send((int)MsgID.BuyGoods_CREQ, req);
    }

    private void OnBtnDiamondClick()
    {
        ReqBuyGoods req = new ReqBuyGoods();
        req.goodid = (uint)_goodsid;
        req.buyType = BuyType.Diamon;
        Net.instance.Send((int)MsgID.BuyGoods_CREQ, req);
    }
}

/// <summary>
/// 商城界面
/// </summary>
public class MallWnd : BaseWnd
{
    public class ButtonClickListener : MonoBehaviour, IPointerClickHandler
    {
        // 商品id
        public uint goodsid;

        public void OnPointerClick(PointerEventData eventData)
        {
            WindowManager.instance.Open<BuyTypeWnd>().Init(goodsid);
        }
    }

    private Button _btnReturn;

    private Text _txtGold;

    private Text _txtDiamond;

    private Transform _content;

    private Button _btnGoods;

    public void Init(List<uint> goods)
    {
        _btnReturn = _transform.Find("Image/BtnReturn").GetComponent<Button>();
        _btnReturn.onClick.AddListener(OnBtnReturnClick);

        _txtGold = _transform.Find("Image/GoldInfo/TxtGold").GetComponent<Text>();
        _txtGold.text = DataCache.instance.currentCharacter.gold.ToString();

        _txtDiamond = _transform.Find("Image/GoldInfo/TxtDiamond").GetComponent<Text>();
        _txtDiamond.text = DataCache.instance.currentCharacter.diamond.ToString();

        _content = _transform.Find("Scroll View/Viewport/Content");
        _btnGoods = _transform.Find("Scroll View/Viewport/BtnGoods").GetComponent<Button>();

        for (int i = 0; i < goods.Count; i++)
        {
            Transform child = GameObject.Instantiate(_btnGoods.gameObject).transform;
            child.SetParent(_content);
            child.localPosition = Vector3.zero;
            child.localScale = Vector3.one;
            child.gameObject.SetActive(true);

            // 获取商品的配置信息和物品信息
            MallCfg cfg = ConfigManager.instance.mallCfgs[(int)goods[i]];
            ItemCfg itemCfg = ConfigManager.instance.GetItemCfg(cfg.ItemID);

            // 设置商品名
            Text name = child.Find("Name").GetComponent<Text>();
            name.text = itemCfg.Name;

            // 设置商品图标
            Image img = child.Find("Image").GetComponent<Image>();
            img.overrideSprite = Resources.Load<Sprite>("Icon/" + itemCfg.Icon);

            // 设置商品价值
            Text gold = child.Find("Gold").GetComponent<Text>();
            gold.text = cfg.Gold.ToString();
            Text diamond = child.Find("Diamond").GetComponent<Text>();
            diamond.text = cfg.Diamond.ToString();

            // 添加购买按钮的事件
            Button btnBuy = child.Find("BtnBuy").GetComponent<Button>();
            btnBuy.gameObject.AddComponent<ButtonClickListener>().goodsid = goods[i];
        }
    }

    private void OnBtnReturnClick()
    {
        WindowManager.instance.Close<MallWnd>();
    }

    /// <summary>
    /// 更新角色的金币和钻石信息
    /// </summary>
    /// <param name="gold"></param>
    /// <param name="diamond"></param>
    public void UpdateGoldDiamond(int gold, int diamond)
    {
        _txtGold.text = gold.ToString();
        _txtDiamond.text = diamond.ToString();
    }
}