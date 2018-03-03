
--[[
require "Common/define"
require "Common/protocal"
require "Common/functions"
Event = require 'events'


require "3rd/pbc/protobuf"


Network = {};
local this = Network;

Handler = {};


local transform;
local gameObject;
local islogging = false;


function Network.Start()
    logWarn("Network.Start!!");
    Event.AddListener(Protocal.Connect, this.OnConnect); 
    Event.AddListener(Protocal.Message, this.OnMessage); 
    Event.AddListener(Protocal.Exception, this.OnException); 
    Event.AddListener(Protocal.Disconnect, this.OnDisconnect);
end

--Socket消息--
function Network.OnSocket(key, data)
    Event.Brocast(tostring(key), data);
end

--当连接建立时--
function Network.OnConnect() 
    logWarn("Game Server connected!!");
end

--异常断线--
function Network.OnException() 
    islogging = false; 
    NetManager:SendConnect();
   	logError("OnException------->>>>");
end

--连接中断，或者被踢掉--
function Network.OnDisconnect() 
    islogging = false; 
    logError("OnDisconnect------->>>>");
end

--登录返回--
function Network.OnMessage(buffer) 

    this.TestLoginPbc(buffer);

    ----------------------------------------------------
    local ctrl = CtrlManager.GetCtrl(CtrlNames.Message);
    if ctrl ~= nil then
        ctrl:Awake();
    end
    logWarn('OnMessage-------->>>');
end

--PBC登录--
function Network.TestLoginPbc(buffer)
	--local protocal = buffer:ReadByte();
    local protocal = buffer:ReadInt();
	local data = buffer:ReadBuffer();

    local decode = protobuf.decode("RespLogin" , data)


    print(decode.name)
    print(decode.id)
    for _,v in ipairs(decode.phone) do
        print("\t"..v.number, v.type)
    end

    print(decode.account);
    print(decode.msgtips);

	log('TestLoginPbc: protocal:>'..protocal);
end

--卸载网络监听--
function Network.Unload()
    Event.RemoveListener(Protocal.Connect);
    Event.RemoveListener(Protocal.Message);
    Event.RemoveListener(Protocal.Exception);
    Event.RemoveListener(Protocal.Disconnect);
    logWarn('Unload Network...');
end
--]]