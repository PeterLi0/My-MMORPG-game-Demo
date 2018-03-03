---
--- Created by shang.
--- DateTime: 2017/12/19 12:31
---

msgid =
{
    ACC_LOGIN_CREQ = 1001,     --- 客户端申请登录
    ACC_LOGIN_SRES = 1002,    --- 服务器反馈给客户端 登录结果

    ACC_REG_CREQ = 1003,      --- 客户端申请注册
    ACC_REG_SRES = 1004,      --- 服务器反馈给客户端 注册结果

    ACC_OFFLINE_CREQ = 1005,   --- 账号离线请求
    ACC_OFFLINE_SRES = 1006,   --- 账号离线应答

    ---/ <summary>
    ---/ 角色协议
    ---/ </summary>

    CHAR_INFO_CREQ = 1101,     --- 获取自身数据
    CHAR_INFO_SRES = 1102,     --- 返回自身数据

    CHAR_CREATE_CREQ = 1103,   --- 申请创建角色
    CHAR_CREATE_SRES = 1104,   --- 返回创建结果

    CHAR_ONLINE_CREQ = 1105,   --- 角色上线请求
    CHAR_ONLINE_SRES = 1106,   --- 角色上线应答


    CHAR_OFFLINE_CREQ = 1107,  --- 角色离线请求
    CHAR_OFFLINE_SRES = 1108,  --- 角色离线应答

    CHAR_DELETE_CREQ = 1109,   --- 删除角色请求
    CHAR_DELETE_SRES = 1110,   --- 删除角色应答


    CHAR_CharOnline_NOTIFY = 1111,          --- 通知角色上线
    CHAR_CharOffline_NOTIFY = 1112,         --- 通知角色离线


    ---/ <summary>
    ---/ 场景协议
    ---/ </summary>
    SCENE_Char_CREQ = 1201,     --- 获取场景中所有角色信息
    SCENE_Char_SRES = 1202,     --- 获取场景中所有角色信息应答



    SCENE_ENTERMAP_CREQ = 1207,
    SCENE_ENTERMAP_SRES = 1208,



    ---/ <summary>
    ---/ 背包协议
    ---/ </summary>

    INV_ItemInfos_CREQ = 1301,          --- 获取已有物品信息
    INV_ItemInfos_SRES = 1302,

    INV_Equip_CREQ = 1303,              --- 装备物品
    INV_Equip_SRES = 1304,

    INV_Unload_CREQ = 1305,             --- 卸载物品
    INV_Unload_SRES = 1306,

    INV_Delete_Item_CREQ = 1307,        --- 销毁物品
    INV_Delete_Item_SRES = 1308,

    ---/ <summary>
    ---/ 邮件协议
    ---/ </summary>

    MailInfos_CREQ = 1401,        --- 获取邮件信息
    MailInfos_SRES = 1402,

    Delete_Mail_CREQ = 1403,           --- 删除邮件
    Delete_Mail_SRES = 1404,

    Send_CREQ = 1405,             --- 发送邮件
    Send_SRES = 1406,

    RecvItem_CREQ = 1407,         --- 领取邮件物品
    RecvItem_SRES = 1408,


    ---/ <summary>
    ---/ 任务协议
    ---/ </summary>

    Info_CREQ = 1501,             --- 获取所有任务信息
    Info_SRES = 1502,

    Receive_CREQ = 1503,          --- 接收任务
    Receive_SRES = 1504,

    Drop_CREQ = 1505,             --- 放弃任务
    Drop_SRES = 1506,

    Finish_CREQ = 1507,           --- 立即完成
    Finish_SRES = 1508,

    ---/ <summary>
    ---/ 商城协议
    ---/ </summary>
    MallInfo_CREQ = 1601,       --- 获取商城信息
    MallInfo_SRES = 1602,

    BuyGoods_CREQ = 1603,       --- 购买商品
    BuyGoods_SRES = 1604,


    ---/ <summary>
    ---/ 竞技场
    ---/ </summary>
    ReqMatchArena = 9001,           --- 匹配竞技场战斗
    RspMatchArena = 9002,

    ReqCancelMatchArena = 9003,     --- 取消竞技场战斗匹配
    RspCancelMatchArena = 9004,

    NotifyMatchComplete = 9005,     --- 通知匹配完成

    ReqStartArena       = 9007,     --- 开始竞技场战斗
    RspStartArena       = 9008,

    RepEnterArena       = 9009,     --- 报告服务器进入竞技场
    NotifyPlayerEnterArena = 9010,  --- 通知其他玩家进入竞技场

    ReqExitArena        = 9011,     --- 退出竞技场战斗
    RspExitArena        = 9012,

    NotifyExitArena     = 9013,     --- 通知其他玩家退出竞技场


    ---/ <summary>
    ---/ 战场
    ---/ </summary>
    ReqMatchBattleground = 9101,            --- 匹配战场战斗
    RspMatchBattleground = 9102,

    ReqCancelMatchBattleground = 9103,      --- 取消战场战斗匹配
    RspCancelMatchBattleground = 9104,

    NotifyBattlegroundMatchComplete  = 9105, --- 通知匹配完成

    ReqStartBattleground = 9107,            --- 开始战场战斗请求
    RspStartBattleground = 9108,

    RepEnterBattleground = 9109,            --- 报告服务器进入战场
    NotifyPlayerEnterBattleground = 9110,   --- 通知其他玩家进入战场

    ReqExitBattleground = 9111,             --- 退出战场战斗
    RspExitBattleground = 9112,

    NotifyExitBattleground = 9113,          --- 通知其他玩家退出战场

    ---/ <summary>
    ---/ 地下城
    ---/ </summary>
    ReqMatchDungeon = 9201,                 --- 匹配地下城战斗
    RspMatchDungeon = 9202,

    ReqCancelMatchDungeon = 9203,           --- 取消地下城战斗匹配
    RspCancelMatchDungeon = 9204,

    NotifyDungeonMatchComplete = 9205,      --- 通知匹配完成

    ReqStartDungeon = 9207,                 --- 开始地下城战斗
    RspStartDungeon = 9208,

    RepEnterDungeon = 9209,                 --- 报告服务器进入地下城
    NotifyPlayerEnterDungeon = 9210,        --- 通知其他玩家进入地下城

    ReqExitDungeon = 9211,                  --- 退出地下城战斗
    RspExitDungeon = 9212,

    NotifyExitDungeon = 9213,               --- 通知其他玩家退出地下城


    ---/ <summary>
    ---/ 战斗同步
    ---/ </summary>
    ReqCharacterMove    = 9901,             --- 角色移动请求
    NotifyCharacterMove = 9902,             --- 通知角色移动
    NotifyCharacterIdle = 9903,             --- 通知角色停止移动

    ReqCharacterAttack = 9904,              --- 角色攻击请求
    NotifyCharacterAttack = 9905,           --- 通知角色攻击
    NotifyHPChange      = 9906,             --- 通知角色血量改变

    NotifyCharacterDie  = 9907,             --- 通知角色死亡
}
