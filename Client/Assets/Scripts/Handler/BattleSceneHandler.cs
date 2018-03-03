using System;
using System.Collections.Generic;
using UnityEngine;
using common;
using proto.battlescene;
using proto.battlesync;

public class BattleSceneHandler 
{
    public void RegisterMsg(Dictionary<MsgID, Action<SocketModel>> handlers)
    {
        handlers.Add(MsgID.SCENE_Char_SRES, OnSceneChar);
        handlers.Add(MsgID.SCENE_ENTERMAP_SRES, OnEnterMap);
    }

    private void OnEnterMap(SocketModel model)
    {
        
    }

    /// <summary>
    /// 获取场景中的其他玩家信息的应答
    /// </summary>
    /// <param name="model"></param>
    private void OnSceneChar(SocketModel model)
    {
        if (model.message == null) return;
        RespSceneCharacters resp = SerializeUtil.Deserialize<RespSceneCharacters>(model.message);
        for(int i = 0; i < resp.characters.Count; i++)
        {
            CharacterDTO dto = resp.characters[i];
            RoleCfg cfg = ConfigManager.instance.GetRoleCfg(dto.cfgid);
            Vector3 pos = new Vector3(dto.pos_x, dto.pos_y, dto.pos_z);
            //CharacterManager.instance.Create(dto.id, cfg, pos, typeof(OtherPlayer));
            Battle.instance.Create<Player>(dto);
        }
    }

    ///// <summary>
    ///// 场景中进入了新的玩家
    ///// </summary>
    ///// <param name="model"></param>
    //private void OnSceneCharOnline(SocketModel model)
    //{
    //    NotifyCharacterOnline notify = SerializeUtil.Deserialize<NotifyCharacterOnline>(model.message);
    //    RoleCfg cfg = ConfigManager.instance.GetRoleCfg(notify.character.cfgid);
    //    Vector3 pos = new Vector3(notify.character.pos_x, notify.character.pos_y, notify.character.pos_z);
    //    //CharacterManager.instance.Create(notify.character.id, cfg, pos, typeof(OtherPlayer));
    //    Battle.instance.Create<OtherPlayer>(notify.character);
    //}

    ///// <summary>
    ///// 场景中离开了玩家
    ///// </summary>
    ///// <param name="model"></param>
    //private void OnSceneCharOffline(SocketModel model)
    //{
    //    NotifyCharacterOffline notify = SerializeUtil.Deserialize<NotifyCharacterOffline>(model.message);
    //    Character ch =Battle.instance.GetRole((uint)notify.characterid);
    //    ch.Leave();
    //    Battle.instance.RemoveCharacter((uint)notify.characterid);
    //}
}