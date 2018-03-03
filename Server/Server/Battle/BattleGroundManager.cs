using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class BattleGroundManager : Singleton<BattleGroundManager>
{
    private uint _idcounter = 1001;

    private Dictionary<uint, BattleGround> _battles = new Dictionary<uint, BattleGround>();

    public void Update(float dt)
    {
        foreach (BattleGround battle in _battles.Values.ToArray())
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
    public BattleGround Create(common.BattleType type, string mapName, int limitNumber)
    {
        BattleGround level = new BattleGround();
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

    public BattleGround GetBattleGround(uint UniID)
    {
        if (_battles.ContainsKey(UniID))
        {
            return _battles[UniID];
        }
        return null;
    }
}

