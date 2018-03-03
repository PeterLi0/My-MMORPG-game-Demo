--[[
require "3rd/pblua/login_pb"
require "3rd/pbc/protobuf"

local lpeg = require "lpeg"

local json = require "cjson"
local util = require "3rd/cjson/util"

local sproto = require "3rd/sproto/sproto"
local core = require "sproto.core"
local print_r = require "3rd/sproto/print_r"
--]]

require "Logic/LuaClass"
require "Logic/CtrlManager"

require "Common/functions"
require "Common/define"
--require "Common/protocal"

require "View/LoginWnd"

require "Handler/AccountHandler"
require "Handler/CharacterHandler"

--管理器--
Game = {};
local this = Game;

local game; 
local transform;
local gameObject;
local WWW = UnityEngine.WWW;

function Game.InitViewPanels()
	for i = 1, #PanelNames do
		require ("View/"..tostring(PanelNames[i]))
	end
end

--初始化完成，发送链接服务器信息--
function Game.OnInitOK()
    --local handlerCenter = HandlerCenter.New();
    --networkMgr:SetHandlerCenter(handlerCenter);

    AccountHandler.Register();
    CharacterHandler.Register();

    AppConst.SocketPort = 6650;
    AppConst.SocketAddress = "192.168.21.4";
    networkMgr:SendConnect();

    --注册LuaView--
    --this.InitViewPanels();

    coroutine.start(this.test_coroutine);

    CtrlManager.Init();
    local ctrl = CtrlManager.GetCtrl(CtrlNames.LoginWnd);
    --if ctrl ~= nil and AppConst.ExampleMode == 1 then
        ctrl:Open();
    --end
       
    logWarn('LuaFramework InitOK--->>>');
end

--测试协同--
function Game.test_coroutine()
    logWarn("1111");
    coroutine.wait(1);
    logWarn("2222");

    local www = WWW("http://bbs.ulua.org/readme.txt");
    coroutine.www(www);
    logWarn(www.text);
end


--销毁--
function Game.OnDestroy()
	--logWarn('OnDestroy--->>>');
end

