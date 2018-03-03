using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// 出生点配置
/// </summary>
public class BornPointCfg
{
    public int ID;
    public int LevelID;
    public int RoleID;
    public string Position;
    public string Rotation;
}

public partial class ConfigManager
{
    private Dictionary<int, BornPointCfg> _bornPoints = new Dictionary<int, BornPointCfg>();

    /// <summary>
    /// 获取某一个关卡中的所有出生点配置
    /// </summary>
    /// <param name="levelID"></param>
    /// <returns></returns>
    public Dictionary<int, BornPointCfg> GetBornPoints(int levelID)
    {
        Dictionary<int, BornPointCfg> bornPointCfgs = new Dictionary<int, BornPointCfg>();
        foreach (BornPointCfg bpc in _bornPoints.Values)
        {
            if (bpc.LevelID == levelID && !bornPointCfgs.ContainsKey(bpc.ID))
                bornPointCfgs.Add(bpc.ID, bpc);
        }

        return bornPointCfgs;
    }

}
