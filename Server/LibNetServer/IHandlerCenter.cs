using System;
using System.Collections.Generic;


public interface IHandlerCenter
{
    // 初始化消息操纵器
    void Initialize();

    // 客户端连接
    void ClientConnect(UserToken token);

    // 客户端断开
    void ClientClose(UserToken token, string error);

    // 收到消息
    void MessageReceive(UserToken token, object message);
}