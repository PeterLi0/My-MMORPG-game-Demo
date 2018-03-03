using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


class DungeonManager : Singleton<DungeonManager>
{
    private uint _idcounter = 1001;

    private Dictionary<uint, BattleDungeon> _battles = new Dictionary<uint, BattleDungeon>();

    public void Update(float dt)
    {
        foreach (BattleDungeon battle in _battles.Values.ToArray())
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
    public BattleDungeon Create(common.BattleType type, string mapName, int limitNumber)
    {
        BattleDungeon level = new BattleDungeon();
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

    public BattleDungeon GetDungeon(uint UniID)
    {
        if (_battles.ContainsKey(UniID))
        {
            return _battles[UniID];
        }
        return null;
    }
}
