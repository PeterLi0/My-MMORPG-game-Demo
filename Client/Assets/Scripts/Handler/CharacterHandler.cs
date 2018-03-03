using System;
using System.Collections.Generic;
using UnityEngine;
using proto.character;
using common;

public class CharacterHandler 
{
    public void RegisterMsg(Dictionary<MsgID, Action<SocketModel>> handlers)
    {
        handlers.Add(MsgID.CHAR_INFO_SRES, OnCharacterInfo);
        handlers.Add(MsgID.CHAR_CREATE_SRES, OnCreateCharacter);
        handlers.Add(MsgID.CHAR_ONLINE_SRES, OnCharacterOnline);
        handlers.Add(MsgID.CHAR_OFFLINE_SRES, OnCharacterOffline);
    }

    // 获取角色信息应答
    private void OnCharacterInfo(SocketModel model)
    {
        RespCharactersInfo resp = SerializeUtil.Deserialize<RespCharactersInfo>(model.message);
        DataCache.instance.chDtos = resp.characters;

        Login.instance.Finalise();
        Loading.instance.LoadScene("SelectRole");
    }

    // 创建角色应答
    private void OnCreateCharacter(SocketModel model)
    {
        RespAddCharacter resp = SerializeUtil.Deserialize<RespAddCharacter>(model.message);
        if(resp.msgtips == (uint)MsgTips.NameRepeat)
        {
            MessageBox.Show("角色重名");
        }
        else
        {
            // 缓存当前角色
            DataCache.instance.currentCharacter = resp.character;
            DataCache.instance.AddChracter(resp.character);

            WindowManager.instance.Close<CreateRoleWnd>();
            SelectRole.instance.ShowSelectRole();
        }
    }

    // 角色上线
    private void OnCharacterOnline(SocketModel model)
    {
        RespCharacterOnline resp = SerializeUtil.Deserialize<RespCharacterOnline>(model.message);
        DataCache.instance.currentCharacter = DataCache.instance.GetCharDTO(resp.characterid);

        // 创建玩家
        CharacterDTO ch = DataCache.instance.currentCharacter;
        RoleCfg cfg = ConfigManager.instance.GetRoleCfg(ch.cfgid);
        Vector3 pos = new Vector3(ch.pos_x, ch.pos_y, ch.pos_z);

        Battle.instance.Create<Player>(ch);

        Battle.instance.isonline = true;
    }

    // 角色下线 
    private void OnCharacterOffline(SocketModel model)
    {
        RespCharacterOffline resp = SerializeUtil.Deserialize<RespCharacterOffline>(model.message);
        if(resp.msgtips == (uint)MsgTips.CharOfflineSuccess)
        {
            Battle.instance.Clear();
            Loading.instance.LoadScene("SelectRole");

            DataCache.instance.currentCharacter = null;
        }
    }
}