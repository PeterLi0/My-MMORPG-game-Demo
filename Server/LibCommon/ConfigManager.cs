
using System.Collections.Generic;

/// <summary>
/// 商品配置信息
/// </summary>
public class MallCfg
{
    public int ID;
    public int ItemID;
    public int Gold;
    public int Diamond;
}



/// <summary>
/// 消息提示配置
/// </summary>
public class MsgTipsCfg
{
    public int ID;
    public string Value;
}

public class ItemCfg
{
    public int ID;
    public ItemType ItemType;   // 物品类型
    public EquipType EquipType; // 装备类型
    public string Name;         // 物品名
    public string Icon;         // 图标名称
    public int AttrType;        // 增加的属性类型
    public int AttrValue;       // 增加的属性值
}

/// <summary>
/// 游戏配置管理器
/// </summary>
public partial class ConfigManager : Singleton<ConfigManager>
{
    private IConfigParser _configParser;

    // 消息提示配置
    public Dictionary<int, MsgTipsCfg> _msgTipsCfgs = new Dictionary<int, MsgTipsCfg>();

    // 物品配置
    private Dictionary<int, ItemCfg> _itemCfgs = new Dictionary<int, ItemCfg>();

    // 商城配置
    public Dictionary<int, MallCfg> mallCfgs = new Dictionary<int, MallCfg>();

    public void Initialize(IConfigParser configParser)
    {
        _configParser = configParser;
        LoadAllConfigs();
    }

    /// <summary>
    /// 载入所有游戏配置
    /// </summary>
    public void LoadAllConfigs()
    {
        _bornPoints = _configParser.LoadConfig<BornPointCfg>("MapScene");
        _roleCfgs = _configParser.LoadConfig<RoleCfg>("Role");
        _skillGroupCfgs = _configParser.LoadConfig<SkillGroupCfg>("SkillGroup");
        _skillBasicCfgs = _configParser.LoadConfig<SkillBasicCfg>("SkillBasic");
        _skillBulletCfgs = _configParser.LoadConfig<SkillBulletCfg>("SkillBullet");
        _skillAOECfgs = _configParser.LoadConfig<SkillAOECfg>("SkillAOE");
        _skillBuffCfgs = _configParser.LoadConfig<SkillBuffCfg>("SkillBuff");
        _skillTrapCfgs = _configParser.LoadConfig<SkillTrapCfg>("SkillTrap");
        _msgTipsCfgs = _configParser.LoadConfig<MsgTipsCfg>("MsgTips");
        _itemCfgs = _configParser.LoadConfig<ItemCfg>("Item");
        mallCfgs = _configParser.LoadConfig<MallCfg>("Mall");

        _arenas = _configParser.LoadConfig<ArenaCfg>("BattleArena");
        _battlegrounds = _configParser.LoadConfig<BattleGroundCfg>("BattleGround");
        _dungeons = _configParser.LoadConfig<DungeonCfg>("BattleDungeon");
        _scenes = _configParser.LoadConfig<SceneCfg>("BattleScene");
    }


    /// <summary>
    /// 获取消息提示信息的值
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string GetMsgTips(uint key)
    {
        return _msgTipsCfgs[(int)key].Value;
    }

    public ItemCfg GetItemCfg(int key)
    {
        return _itemCfgs[key];
    }
}
