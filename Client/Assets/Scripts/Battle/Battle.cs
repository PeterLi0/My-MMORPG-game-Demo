using System;
using System.Collections.Generic;
using System.Linq;
using common;
using proto.character;


public class Battle : Singleton<Battle>
{
    // 导航网格信息，管理导航网格的读取和实时更新
    private LunaNavmeshQuery _navmeshQuery;

    /// <summary>
    /// 关卡中的所有角色
    /// </summary>
    private Dictionary<uint, Character> _roles = new Dictionary<uint, Character>();

    public bool isonline = false;

    public void Initialize()
    {
        SceneCfg sceneCfg = ConfigManager.instance.GetSceneCfgs(DataCache.instance.currentCharacter.mapid);

        // 读取导航数据，并初始化导航网格信息
        _navmeshQuery = new LunaNavmeshQuery();
        _navmeshQuery.Initialize("Navmesh/" + sceneCfg.Scene);

        if(!isonline)
        {
            ReqCharacterOnline req = new ReqCharacterOnline();
            req.characterid = DataCache.instance.currentCharacter.id;
            Net.instance.Send<ReqCharacterOnline>((int)MsgID.CHAR_ONLINE_CREQ, req);
        }
        else
        {

        }
    }

    public void Update(float dt)
    {
        // 更新导航网格
        if(_navmeshQuery != null)
            _navmeshQuery.Update(dt);

        foreach (Character role in _roles.Values.ToArray())
        {
            role.Update(dt);
        }
    }

    public void Clear()
    {
        PoolManager.instance.Clear();

        _roles.Clear();
    }


    public Character GetRole(uint id)
    {
        return _roles[id];
    }

    public T Create<T>(CharacterDTO dto) where T : Character, new()
    {
        T ch = new T();
        CharacterAttr attr = CharacterAttr.GetAttr(dto);
        ch.Init(attr);
        ch.InitNavmeshAgent(_navmeshQuery);

        // 将创建的角色添加到字典中
        if (!_roles.ContainsKey(ch.characterid))
            _roles.Add(ch.characterid, ch);

        return ch;
    }

    public void RemoveCharacter(uint id)
    {
        _roles.Remove(id);
    }
}
