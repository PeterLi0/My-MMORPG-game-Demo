using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
//using UnityEngine.SceneManagement;
using DG.Tweening;

public enum BloodTextType
{
    None = 0,
    Damage = 1,
    Recover = 2,

}

/// <summary>
/// 战斗界面
/// </summary>
public class BattleWnd : BaseWnd
{
    // 返回按钮
    private Button _btnReturn;

    // 提示文本
    private Text _tipsText;

    // 头顶的信息栏
    private Dictionary<Character, FloatingBar> _floatingBars;

    public BattleWnd Initialize()
    {
        _btnReturn = _transform.Find("BtnReturn").GetComponent<Button>();
        _btnReturn.onClick.AddListener(OnReturnBtnClick);

        _tipsText = _transform.Find("TipsText").GetComponent<Text>();
        _tipsText.text = string.Empty;

        _floatingBars = new Dictionary<Character, FloatingBar>();

        return this;
    }

    private void OnReturnBtnClick()
    {
        Battle.instance.Clear();
        Loading.instance.LoadScene("MainCity");
    }

    /// <summary>
    /// 创建飘血文字
    /// </summary>
    /// <param name="position">世界坐标</param>
    /// <param name="hp">血量</param>
    /// <param name="type">数值类型</param>
    public void CreateBloodText(Character ch, int hp, BloodTextType type)
    {
        // 对象池创建
        Transform transform = PoolManager.instance.Spawn("UI/", "BloodText").transform;
        transform.parent = _transform;

        transform.position = WorldToScreenPosition(ch.position + new Vector3(0, 2, 0));
        transform.localScale = Vector3.one;

        Vector3 pos = transform.localPosition;
        pos.z = 0;
        transform.localPosition = pos;


        Text text = transform.GetComponent<Text>();
        text.text = hp.ToString();

        transform.DOLocalMoveY(transform.localPosition.y + 100, 0.5f);

        TimerMgr.instance.Invoke(1.0f, () =>
        {
            PoolManager.instance.Unspawn(transform.gameObject);
        });

        //float percent = (float)ch.currHp / (float)ch.totalHp;
        //_floatingBars[ch].HpChange(percent);
    }

    /// <summary>
    /// 世界坐标转屏幕坐标
    /// </summary>
    /// <param name="position"></param>
    public static Vector3 WorldToScreenPosition(Vector3 position)
    {
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(position);
        Vector3 uipoint = WindowManager.instance.uiCamera.ScreenToWorldPoint(screenPosition);
        uipoint.z = 0;
        return uipoint;
    }

    /// <summary>
    /// 显示提示
    /// </summary>
    /// <param name="content">显示的内容</param>
    public void ShowTips(string content)
    {
        _tipsText.text = content;
        _tipsText.transform.localPosition = new Vector3(-395f, -119, 0);
        _tipsText.color = Color.red;

        TimerMgr.instance.Invoke(0.5f, () => 
        {
            _tipsText.transform.DOLocalMoveY(_tipsText.transform.position.y + 100, 0.5f);
            DOTween.ToAlpha(()=>_tipsText.color, a => _tipsText.color = a, 0f, 0.5f);
        });
    }

    /// <summary>
    /// 角色出生
    /// </summary>
    /// <param name="ch"></param>
    public void OnCharacterCreate(Character ch)
    {
        FloatingBar bar = new FloatingBar(_transform, ch);
        _floatingBars.Add(ch, bar);
    }

    /// <summary>
    /// 角色死亡
    /// </summary>
    /// <param name="ch"></param>
    public void OnCharacterDie(Character ch)
    {
        _floatingBars[ch].Unspawn();
        _floatingBars.Remove(ch);
    }

    public override void Update(float dt)
    {
        foreach(KeyValuePair<Character, FloatingBar> pair in _floatingBars)
        {
            Character ch = pair.Key;
            FloatingBar bar = pair.Value;

            bar.UpdatePosition(ch.position + new Vector3(0, 2, 0));
        }
    }

    /// <summary>
    /// 关闭界面
    /// </summary>
    public override void Close()
    {
        base.Close();

        _floatingBars.Clear();
    }
}


/// <summary>
/// 角色头顶信息栏
/// </summary>
public class FloatingBar
{
    // 血条
    private Slider _hpBar;

    // 角色名
    private Text _roleName;

    /// <summary>
    /// 构造
    /// </summary>
    /// <param name="globalid">角色全局ID</param>
    public FloatingBar(Transform parent, Character ch)
    {
        _hpBar = PoolManager.instance.Spawn("UI/", "FloatingBar").GetComponent<Slider>();
        _hpBar.transform.parent = parent;
        _hpBar.transform.localScale = Vector3.one;

        _roleName = _hpBar.transform.Find("RoleName").GetComponent<Text>();
        //_roleName.text = string.Format("{0}--{1}", ch.GlobalID, ch.name);
    }

    /// <summary>
    /// 血量改变
    /// </summary>
    /// <param name="percent">血量百分比</param>
    public void HpChange(float percent)
    {
        _hpBar.value = percent;
    }

    /// <summary>
    /// 当角色对象被回收时，信息栏也要回收
    /// </summary>
    public void Unspawn()
    {
        PoolManager.instance.Unspawn(_hpBar.gameObject);
    }

    public void UpdatePosition(Vector3 position)
    {
        _hpBar.transform.position = BattleWnd.WorldToScreenPosition(position);

        Vector3 pos = _hpBar.transform.localPosition;
        pos.z = 0;
        _hpBar.transform.localPosition = pos;
    }
}
