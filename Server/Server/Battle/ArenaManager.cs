using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;

public class ArenaManager : Singleton<ArenaManager>
{
    private uint _idcounter = 1001;

    private Dictionary<uint, Match> _matchs = new Dictionary<uint, Match>();

    private Dictionary<uint, BattleArena> _battles = new Dictionary<uint, BattleArena>();

    public void Update(float dt)
    {
        foreach (BattleArena battle in _battles.Values.ToArray())
        {
            if (battle.running)
            {
                battle.Update(dt);
            }
            else
            {
                _battles.Remove(battle.globalID);
            }
        }
    }

    /// <summary>
    /// 创建新的战斗
    /// </summary>
    /// <param name="LevelID"></param>
    /// <param name="userRole"></param>
    public BattleArena Create(common.BattleType type, string mapName, int limitNumber)
    {
        BattleArena level = new BattleArena();
        level.Init(type, mapName, limitNumber);

        level.globalID = _idcounter++;

        _battles.Add(_idcounter, level);

        return level;
    }


    /// <summary>
    /// 移除一场战斗
    /// </summary>
    /// <param name="globalID"></param>
    public void Remove(uint UniID)
    {
        if (_battles.ContainsKey(UniID))
        {
            _battles.Remove(UniID);
        }
    }

    public BattleArena GetArena(uint UniID)
    {
        if (_battles.ContainsKey(UniID))
        {
            return _battles[UniID];
        }
        return null;
    }
}
