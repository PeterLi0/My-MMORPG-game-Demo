using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LuaInterface;

namespace LuaFramework
{
    public class NetworkManager : Manager
    {
        //private SocketClient socket;
        //static readonly object m_lockObject = new object();
        //static Queue<KeyValuePair<int, ByteBuffer>> mEvents = new Queue<KeyValuePair<int, ByteBuffer>>();

        //SocketClient SocketClient
        //{
        //    get
        //    {
        //        if (socket == null)
        //            socket = new SocketClient();
        //        return socket;
        //    }
        //}


        private NetClient _client;

        void Awake()
        {
            Init();
        }

        void Init()
        {
            //SocketClient.OnRegister();


            _client = new NetClient();
            _client.log = Log;

        }

        private void Log(string content)
        {
            Debug.Log(content);
        }

        public void OnInit()
        {
            CallMethod("Start");
        }

        public void Unload()
        {
            CallMethod("Unload");
        }

        /// <summary>
        /// ִ��Lua����
        /// </summary>
        public object[] CallMethod(string func, params object[] args)
        {
            return Util.CallMethod("Network", func, args);
        }

        ///------------------------------------------------------------------------------------
        public static void AddEvent(int _event, ByteBuffer data)
        {
            //lock (m_lockObject)
            //{
            //    mEvents.Enqueue(new KeyValuePair<int, ByteBuffer>(_event, data));
            //}
        }

        /// <summary>
        /// ����Command�����ﲻ����ķ���˭��
        /// </summary>
        void Update()
        {
            //if (mEvents.Count > 0)
            //{
            //    while (mEvents.Count > 0)
            //    {
            //        KeyValuePair<int, ByteBuffer> _event = mEvents.Dequeue();
            //        facade.SendMessageCommand(NotiConst.DISPATCH_MESSAGE, _event);
            //    }
            //}

            _client.Update();
        }

        /// <summary>
        /// ������������
        /// </summary>
        public void SendConnect()
        {
            //SocketClient.SendConnect();
            //HandlerCenter center = new HandlerCenter();
            //center.Initialize();
            _client.ConnectServer(AppConst.SocketAddress, AppConst.SocketPort);
        }


        public void Register(int msgid, Action<SocketModel> action)
        {
            _client.Register(msgid, action);
        }


        /// <summary>
        /// ����SOCKET��Ϣ
        /// </summary>
        public void SendMessage(ByteBuffer buffer)
        {
            //SocketClient.SendMessage(buffer);
        }

        public void SendMessage(int command, ByteArray array)
        {
            //SocketClient.SendMessage(buffer);
            _client.Send(command, array.getBuff());
        }

        /// <summary>
        /// ��������
        /// </summary>
        new void OnDestroy()
        {
            //SocketClient.OnRemove();

            _client.Disconnect();
            Debug.Log("~NetworkManager was destroy");
        }
    }
}