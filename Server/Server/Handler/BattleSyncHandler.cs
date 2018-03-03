using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using proto.battlesync;

public class BattleSyncHandler : IMsgHandler
{
    public void RegisterMsg(Dictionary<MsgID, Action<UserToken, SocketModel>> handlers)
    {
        handlers.Add(MsgID.ReqCharacterMove, OnReqCharacterMove);
    }

    // 角色移动请求
    public void OnReqCharacterMove(UserToken token, SocketModel model)
    {
        ReqCharacterMove req = SerializeUtil.Deserialize<ReqCharacterMove>(model.message);

        BattleData battleData = CacheManager.instance.GetBattleData(token.characterid);
        Battle b = GetCurrentBattle(battleData.battleType, battleData.battleid);
        Player p = b.GetCharacter(token.characterid) as Player;
        p.Move(new Luna3D.Vector3(req.dest.x, req.dest.y, req.dest.z));


        NotifyCharacterMove notify = new NotifyCharacterMove();
        notify.characterid = (uint)token.characterid;
        notify.position = req.dest;
        Dictionary<int, Player> chs = b.GetTypeChar<Player>(common.CharacterType.Player);

        foreach (Player ch in chs.Values)
        {
            NetworkManager.Send(ch.token, (int)MsgID.NotifyCharacterMove, notify);
        }
    }

    // 角色攻击请求
    public void OnReqCharacterAttack(UserToken token, SocketModel model)
    {

    }

    public Battle GetCurrentBattle(common.BattleType type, int battleid)
    {
        Battle b = null ;
        switch(type)
        {
            case common.BattleType.Arena:
                b = ArenaManager.instance.GetArena((uint)battleid);
                break;
            case common.BattleType.Battleground:
                b = BattleGroundManager.instance.GetBattleGround((uint)battleid);
                break;
            case common.BattleType.Dungeon:
                b = DungeonManager.instance.GetDungeon((uint)battleid);
                break;
            case common.BattleType.World:
                b = SceneManager.instance.GetScene((uint)battleid);
                break;
        }

        return b;
    }
}