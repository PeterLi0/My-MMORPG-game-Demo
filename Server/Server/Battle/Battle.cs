using common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public abstract class Battle
{
    public uint globalID;

    private BattleType _type;

    protected LunaNavmeshQuery _navmeshQuery;

    private bool _running = false;

    public bool running { get { return _running; } }

    public void Init(BattleType type, string mapName, int playerNumber)
    {
        _running = true;

        _type = type;

        _navmeshQuery = new LunaNavmeshQuery();
        string filePath = "Navmesh/" + mapName + ".xml";
        _navmeshQuery.Initialize(filePath);
    }

    public virtual void Update(float dt)
    {
        _navmeshQuery.Update(dt);
    }

    public virtual void Clear() { }


    // 创建角色
    public abstract T Create<T>(CharacterDTO dto) where T : Character, new();

    // 从当前场景中移除角色
    public abstract void Remove(int characterid);


    public abstract Character GetCharacter(int globalID);

    public abstract Dictionary<int, T> GetTypeChar<T>(CharacterType chType) where T : Character;

    //public virtual void AddBeforeHit(int casterid, BeforeHit beforeHit) { };
    //// 添加法术
    //public virtual void AddSpell(int casterid, Spell spell) { };
}
