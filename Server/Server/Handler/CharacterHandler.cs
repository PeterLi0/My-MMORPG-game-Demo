
using System;
using System.Collections.Generic;
using common;
using proto.character;
using proto.battlescene;

public class CharacterHandler : IMsgHandler
{
    public void RegisterMsg(Dictionary<MsgID, Action<UserToken, SocketModel>> handlers)
    {
        handlers.Add(MsgID.CHAR_INFO_CREQ, OnGetCharacterInfos);
        handlers.Add(MsgID.CHAR_CREATE_CREQ, OnCreateCharacter);
        handlers.Add(MsgID.CHAR_DELETE_CREQ, OnDeleteCharacter);
        handlers.Add(MsgID.CHAR_ONLINE_CREQ, OnCharacterOnline);
        handlers.Add(MsgID.CHAR_OFFLINE_CREQ, OnCharacterOffline);
    }

    // 获取该账号下的所有的角色数据
    private void OnGetCharacterInfos(UserToken token, SocketModel model)
    {
        string sql = string.Format("select * from characters where accountid = {0}", token.accountid);
        List<CharacterData> chDatas = MysqlManager.instance.ExecQuery<CharacterData>(sql);

        RespCharactersInfo resp = new RespCharactersInfo();

        for(int i = 0; i < chDatas.Count; i++)
        {
            CharacterDTO dto = new CharacterDTO();
            dto = CharacterData.GetDTO(chDatas[i]);
            resp.characters.Add(dto);
        }

        NetworkManager.Send(token, (int)MsgID.CHAR_INFO_SRES, resp);
    }

    private void OnCreateCharacter(UserToken token, SocketModel model)
    {
        ReqAddCharacter req = SerializeUtil.Deserialize<ReqAddCharacter>(model.message);

        RespAddCharacter resp = new RespAddCharacter();

        string sql = string.Format("SELECT * FROM characters WHERE `name` = '{0}'", req.character.name);
        List<CharacterData> chDatas = MysqlManager.instance.ExecQuery<CharacterData>(sql);
        if(chDatas.Count > 0)
        {
            resp.msgtips = (uint)MsgTips.NameRepeat;
        }
        else
        {
            // 往角色表中添加一个角色
            sql = string.Format("INSERT INTO characters (accountid, name, race, job, gender, level, exp, diamond, gold, pos_x, pos_y, pos_z, cfgid, mapid) VALUES ({0}, '{1}', {2}, {3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13})",
                token.accountid, req.character.name, 0, 1, 1, 1, 0, 200, 1000, 0f, 0.5f, 0f, req.character.cfgid, 1001);
            MysqlManager.instance.ExecNonQuery(sql);


            resp.msgtips = (uint)MsgTips.CharCreateSuccess;

            // 再次查询角色表获取当前角色ID
            sql = string.Format("SELECT * FROM characters WHERE `name` = '{0}'", req.character.name);
            chDatas = MysqlManager.instance.ExecQuery<CharacterData>(sql);

            CharacterDTO d = new CharacterDTO();
            d.id = chDatas[0].id;
            d.accountid = token.accountid;
            d.name = req.character.name;
            d.race = 0;
            d.job = 1;
            d.gender = 1;
            d.level = 1;
            d.exp = 0;
            d.diamond = 0;
            d.gold = 0;
            d.pos_x = 0f;
            d.pos_y = 0.5f;
            d.pos_z = 0f;
            d.cfgid = req.character.cfgid;
            d.mapid = 1001;
            resp.character = d;

            // 插入装备数据
            int[] bornequips = new int[6] { 1101, 1201, 1301, -1, -1, -1};
            for(int i = 0; i < bornequips.Length; i++)
            {
                sql = string.Format("INSERT INTO equip (characterid, slot, itemid) VALUES ({0}, {1}, {2})", chDatas[0].id, i + 1, bornequips[i]);
                MysqlManager.instance.ExecNonQuery(sql);
            }

            // 插入背包数据
            int[] borninvs = new int[10]{ 1101, 1102, 1103, 1201, 1202, 1203, 1301, 1302, 1303, 1304 };
            int[] invs = new int[50]; 
            for (int i = 0; i < 50; i++)
            {
                if (i < 10)
                    invs[i] = borninvs[i];
                else
                    invs[i] = -1;
            }

            for(int i = 0; i < invs.Length; i++)
            {
                sql = string.Format("INSERT INTO inventory (characterid, slot, itemid,num) VALUES ({0}, {1}, {2}, {3})", chDatas[0].id, i + 1, invs[i], 1);
                MysqlManager.instance.ExecNonQuery(sql);
            }

            // 插入邮件数据
            for (int i = 0; i < 10; i++)
            {
                sql = string.Format("insert into mail (sender_id, receiver_id, subject, body, deliver_time, money, has_items) values({0}, {1}, '{2}', '{3}', '{4}', {5}, {6})",
                    0, chDatas[0].id, "开服大礼包" + (i + 1).ToString(), "您收到极品装备倚天剑的碎片" + (i + 1).ToString(), "2017-06-23", 0, 0);
                MysqlManager.instance.ExecNonQuery(sql);
            }
        }

        NetworkManager.Send(token, (int)MsgID.CHAR_CREATE_SRES, resp);
    }

    private void OnDeleteCharacter(UserToken token, SocketModel model)
    {

    }

    // 角色上线
    private void OnCharacterOnline(UserToken token, SocketModel model)
    {
        ReqCharacterOnline req = SerializeUtil.Deserialize<ReqCharacterOnline>(model.message);

        RespCharacterOnline resp = new RespCharacterOnline();
        resp.characterid = req.characterid;

        // 角色上线
        if(CacheManager.instance.IsCharOnline(req.characterid))
        {
            resp.msgtips = (int)MsgTips.CharHasOnline;
        }
        else
        {
            token.characterid = req.characterid;

            // 查询角色数据
            string sql = string.Format("SELECT * FROM characters WHERE id = {0}", req.characterid);
            List<CharacterData> chDatas = MysqlManager.instance.ExecQuery<CharacterData>(sql);

            CacheManager.instance.CharOnline(req.characterid, chDatas[0]);


            // 进入世界
            CharacterData data = CacheManager.instance.GetCharData(req.characterid);
            CharacterDTO dto = CharacterData.GetDTO(data);
            Vector pos = new Vector { x = data.pos_x, y = data.pos_y, z = data.pos_z };


            // 在服务端创建一个角色
            BattleScene scene = SceneManager.instance.GetScene(1001);
            Player player = scene.Create<Player>(dto);
            player.token = token;
            player.battleType = BattleType.World;



            CacheManager.instance.AddBattleData(new BattleData(req.characterid, player.battleType, player.sceneid));


            // 给场景中的其他玩家发送该玩家已经上线
            Dictionary<int, Player> chs = scene.GetTypeChar<Player>(CharacterType.Player);
            foreach (Player p in chs.Values)
            {
                if (p.characterid == req.characterid) continue;

                NotifyCharacterOnline notify = new NotifyCharacterOnline();
                notify.character = dto;
                NetworkManager.Send(p.token, (int)MsgID.CHAR_CharOnline_NOTIFY, notify);
            }
        }

        // 给当前玩家发送应答
        NetworkManager.Send(token, (int)MsgID.CHAR_ONLINE_SRES, resp);
    }

    // 角色离线
    private void OnCharacterOffline(UserToken token, SocketModel model)
    {
        ReqCharacterOffline req = SerializeUtil.Deserialize<ReqCharacterOffline>(model.message);

        RespCharacterOffline resp = new RespCharacterOffline();

        // 移除缓存
        if (CacheManager.instance.IsCharOnline(token.characterid))
        {
            CacheManager.instance.CharOffline(token.characterid);
            resp.msgtips = (uint)MsgTips.CharOfflineSuccess;
        }

        // 离开世界
        //if(Scene.instance.players.ContainsKey(token.characterid))
        //{
        //    Scene.instance.LeaveWorld(token.characterid);
        //}

        CacheManager.instance.RemoveBattleData(token.characterid);

        // 给当前玩家发离线应答
        NetworkManager.Send(token, (int)MsgID.CHAR_OFFLINE_SRES, resp);
    }
}