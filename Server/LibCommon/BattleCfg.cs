
using System.Collections.Generic;

public class ArenaCfg
{
    public int ID;
    public int LimitNumber;
    public string Name;
    public string Scene;
    public string BluePosition;
    public string RedPosition;
}

public class DungeonCfg
{
    public int ID;
    public string Name;
    public string Scene;
}

public class BattleGroundCfg
{
    public int ID;
    public int LimitNumber;
    public string Name;
    public string Scene;
    public string BluePosition;
    public string RedPosition;
}

// 阵营
public enum Race
{
    Neutrality = 0,         // 中立
    Alliance = 1,           // 联盟
    Horde = 2,			    // 部落
}

public class SceneCfg
{
    public int ID;
    public Race Race;
    public string Name;
    public string Scene;
    public string BornPosition;
}

public partial class ConfigManager
{
    private Dictionary<int, ArenaCfg> _arenas = new Dictionary<int, ArenaCfg>();

    private Dictionary<int, BattleGroundCfg> _battlegrounds = new Dictionary<int, BattleGroundCfg>();

    private Dictionary<int, DungeonCfg> _dungeons = new Dictionary<int, DungeonCfg>();

    private Dictionary<int, SceneCfg> _scenes = new Dictionary<int, SceneCfg>();



    public Dictionary<int, SceneCfg> GetAllScenes()
    {
        return _scenes;
    }

    public SceneCfg GetSceneCfgs(int battleid)
    {
        return _scenes[battleid];
    }
}