using System;
using System.Collections.Generic;
using common;

public class DataCache : Singleton<DataCache>
{
    // 该账号下的所有角色信息
    public List<CharacterDTO> chDtos = new List<CharacterDTO>();

    // 当前角色
    public CharacterDTO currentCharacter;

    // 当前关卡配置
    //public LevelCfg currentLevelCfg;

    // 战斗类型
    public BattleType battleType;


    public void AddChracter(CharacterDTO dto)
    {
        if(!chDtos.Contains(dto))
        {
            chDtos.Add(dto);
        }
    }

    public CharacterDTO GetCharDTO(int characterid)
    {
        CharacterDTO dto = null;
        for(int i = 0; i < chDtos.Count; i++)
        {
            if (chDtos[i].id == characterid)
                dto = chDtos[i];
        }
        return dto;
    }
}