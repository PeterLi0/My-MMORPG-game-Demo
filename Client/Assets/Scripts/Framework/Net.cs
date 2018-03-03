using System;
using System.Collections.Generic;
using UnityEngine;


public class Net :Singleton<Net>
{
    private string ip = "192.168.21.4";

    private int port = 6650;

    private NetClient _client;

    public void Initialize()
    {
        //HandlerCenter center = new HandlerCenter();
        //center.Initialize();

        _client = new NetClient();
        _client.log = Log;
        //_client.ConnectServer(ip, port, center);
    }

    private void Log(string content)
    {
        Debug.Log(content);
    }

    public void Send<T>( int command, T message)
    {
        Debug.LogError((MsgID)command);

        byte[] data = SerializeUtil.Serialize<T>(message);
        _client.Send(command, data);
    }

    public void Update()
    {
        _client.Update();
    }

    public void Disconnect()
    {
        _client.Disconnect();
    }
}