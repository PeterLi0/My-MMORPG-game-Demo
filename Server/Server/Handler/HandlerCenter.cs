using System;
using System.Collections.Generic;


public interface IMsgHandler
{
    void RegisterMsg(Dictionary<MsgID, Action<UserToken, SocketModel>> handlers);
}

public class HandlerCenter : IHandlerCenter
{
    private Dictionary<MsgID, Action<UserToken, SocketModel>> _handlers = new Dictionary<MsgID, Action<UserToken, SocketModel>>();

    private AccountHandler _accountHandler = new AccountHandler();

    private CharacterHandler _characterHandler = new CharacterHandler();

    private MailHandler _mailHandler = new MailHandler();

    private InventoryHandler _inventoryHandler = new InventoryHandler();

    private BattleSceneHandler _worldHandler = new BattleSceneHandler();

    private MallHandler _mallHandler = new MallHandler();

    private BattleSyncHandler _battleSyncHandler = new BattleSyncHandler();

    public void Initialize()
    {
        _accountHandler.RegisterMsg(_handlers);
        _characterHandler.RegisterMsg(_handlers);
        _worldHandler.RegisterMsg(_handlers);
        _mailHandler.RegisterMsg(_handlers);
        _inventoryHandler.RegisterMsg(_handlers);
        _mallHandler.RegisterMsg(_handlers);
        _battleSyncHandler.RegisterMsg(_handlers);
    }

    /// <summary>
    /// 客户端连接到服务器
    /// </summary>
    /// <param name="token"></param>
    public void ClientConnect(UserToken token)
    {
        Console.WriteLine(string.Format("{0} Connnect...", token.address.ToString()));
    }

    /// <summary>
    /// 客户端断开连接
    /// </summary>
    /// <param name="token"></param>
    /// <param name="error"></param>
    public void ClientClose(UserToken token, string error)
    {
        Console.WriteLine(string.Format("{0} Disconnect...", token.address.ToString()));

        // 玩家下线，移除账号数据缓存
        if(token.accountid != 0 && CacheManager.instance.IsAccountOnline(token.accountid))
        {
            AccountData acc = CacheManager.instance.GetAccount(token.accountid);
            CacheManager.instance.AccountOffline(acc.id);
        }

        // 玩家下线，移除角色数据缓存
        if(token.characterid != 0 && CacheManager.instance.IsCharOnline(token.characterid))
        {
            CharacterData ch = CacheManager.instance.GetCharData(token.characterid);
            CacheManager.instance.CharOffline(ch.id);
        }

        // 离开世界
        //if(token.characterid != 0 && Scene.instance.players.ContainsKey(token.characterid))
        //{
        //    Scene.instance.LeaveWorld(token.characterid);
        //}
    }

    /// <summary>
    /// 服务端在收到客户端的消息之后要执行的方法
    /// </summary>
    /// <param name="token"></param>
    /// <param name="message"></param>
    public void MessageReceive(UserToken token, object message)
    {
        SocketModel model = message as SocketModel;

        Console.WriteLine(token.accountid +", "+ (MsgID)model.command);

        Action<UserToken, SocketModel> handler = _handlers[(MsgID)model.command];
        handler(token, model);
    }
}