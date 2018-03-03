using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using common;

public class BattleData
{
    public int id;
    public common.BattleType battleType;
    public int battleid;

    public BattleData(int id, BattleType battleType, int battleid)
    {
        this.id = id;
        this.battleType = battleType;
        this.battleid = battleid;
    }
}

partial class CacheManager
{
    // 保存所有的已经上线的角色信息
    private Dictionary<int, BattleData> _battleChars = new Dictionary<int, BattleData>();


    public void AddBattleData(BattleData b)
    {
        if(!_battleChars.ContainsKey(b.id))
        {
            _battleChars.Add(b.id, b);
        }
    }

    public BattleData GetBattleData(int id)
    {
        if(_battleChars.ContainsKey(id))
        {
            return _battleChars[id];
        }

        return null;
    }

    public void RemoveBattleData(int id)
    {
        _battleChars.Remove(id);
    }
}