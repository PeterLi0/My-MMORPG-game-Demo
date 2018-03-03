using SuperSocket.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketEngine;
using SuperSocket.SocketBase.Protocol;
using System;


public class Server
{
    public void Start()
    {
        AppServer appServer = new AppServer();

        int port = 2000;

        if (!appServer.Setup(port))
        {
            Console.WriteLine("端口设置失败!");
            Console.ReadKey();
            return;
        }


        appServer.NewSessionConnected += new SessionHandler<AppSession>(OnSessionConnected);
        appServer.NewRequestReceived += new RequestHandler<AppSession, StringRequestInfo>(OnRequestReceived);
        appServer.SessionClosed += new SessionHandler<AppSession, CloseReason>(OnSessionClosed);


        if (!appServer.Start())
        {
            Console.WriteLine("启动服务失败!");
            Console.ReadKey();
            return;
        }

        Console.WriteLine("启动服务成功，输入exit退出!");

        while (true)
        {
            var str = Console.ReadLine();
            if (str.ToLower().Equals("exit"))
            {
                break;
            }
        }

        Console.WriteLine();

        //停止服务
        appServer.Stop();

        Console.WriteLine("服务已停止，按任意键退出!");
        Console.ReadKey();

        
    }

    private void OnSessionConnected(AppSession session)
    {
        //向对应客户端发送数据  
        session.Send("Hello User!");
    }

    private void OnRequestReceived(AppSession session, StringRequestInfo requestInfo)
    {
        /** 
         * requestInfo为客户端发送的指令，默认为命令行协议 
         * 例： 
         * 发送 ping 127.0.0.1 -n 5 
         * requestInfo.Key: "ping" 
         * requestInfo.Body: "127.0.0.1 -n 5" 
         * requestInfo.Parameters: ["127.0.0.1","-n","5"] 
         **/
        switch (requestInfo.Key.ToUpper())
        {
            case ("HELLO"):
                session.Send("Hello World!");
                break;

            default:
                session.Send("未知的指令。");
                break;
        }
    }

    private void OnSessionClosed(AppSession session, CloseReason reason)
    {

    }
}