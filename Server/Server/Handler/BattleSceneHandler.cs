using System;
using System.Collections.Generic;

using proto.battlescene;
using proto.battlesync;

public class BattleSceneHandler : IMsgHandler
{
    public void RegisterMsg(Dictionary<MsgID, Action<UserToken, SocketModel>> handlers)
    {
        handlers.Add(MsgID.SCENE_Char_CREQ, OnSceneChar);
        //handlers.Add(MsgID.SCENE_CharMove_CREQ, OnSceneCharMove);
    }

    /// <summary>
    /// 获取世界中的其他玩家的信息
    /// </summary>
    /// <param name="token"></param>
    /// <param name="model"></param>
    private void OnSceneChar(UserToken token, SocketModel model)
    {
        RespSceneCharacters resp = new RespSceneCharacters();
        //foreach(Player p in Scene.instance.players.Values)
        //{
        //    if (p.globalid == token.characterid) continue;
        //    resp.characters.Add(p.dto);
        //}

        NetworkManager.Send(token, (int)MsgID.SCENE_Char_SRES, resp);
    }

    private void OnSceneCharMove(UserToken token, SocketModel model)
    {
        ReqCharacterMove req = SerializeUtil.Deserialize<ReqCharacterMove>(model.message);

        //NotifyCharacterMove notify = new NotifyCharacterMove();
        //notify.characterid = token.characterid;
        //notify.pos = req.pos;

        //// 发给世界中的所有玩家该角色的目标位置
        //foreach(Player p in Scene.instance.players.Values)
        //{
        //    NetworkManager.Send(p.token, (int)MsgID.SCENE_CharMove_Notify, notify);
        //}
    }
}