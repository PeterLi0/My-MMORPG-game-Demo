using System;
using System.Collections.Generic;
using System.Linq;


public class SceneManager : Singleton<SceneManager>
{
    private Dictionary<uint, BattleScene> _battles = new Dictionary<uint, BattleScene>();

    public void Initialize(Dictionary<int, SceneCfg> scenes)
    {
        foreach (SceneCfg cfg in scenes.Values)
        {
            BattleScene level = new BattleScene();
            level.Init(common.BattleType.World, cfg.Scene, 1000000);
            level.globalID = (uint)cfg.ID;

            _battles.Add(level.globalID, level);
        }
    }

    public void Update(float dt)
    {
        foreach (BattleScene battle in _battles.Values.ToArray())
        {
            battle.Update(dt);
        }
    }


    public BattleScene GetScene(uint id)
    {
        if (_battles.ContainsKey(id))
        {
            return _battles[id];
        }
        return null;
    }
}
