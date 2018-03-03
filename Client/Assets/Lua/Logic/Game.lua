

require "Logic/LuaClass"
require "Logic/WindowManager"

require "Common/functions"
require "Common/define"

<<<<<<< .mine
require "View/LoginWnd"
||||||| .r21
require "View/PromptCtrl"
=======
>>>>>>> .r23

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

--[[
function Game.InitViewPanels()
	for i = 1, #PanelNames do
		require ("View/"..tostring(PanelNames[i]))
	end
end
--]]

--初始化完成，发送链接服务器信息--
function Game.OnInitOK()

    AccountHandler.Register();
    CharacterHandler.Register();

    AppConst.SocketPort = 6650;
    AppConst.SocketAddress = "192.168.21.4";
    networkMgr:SendConnect();



<<<<<<< .mine
    CtrlManager.Init();
    local ctrl = CtrlManager.GetCtrl(CtrlNames.LoginWnd);
||||||| .r21
    CtrlManager.Init();
    local ctrl = CtrlManager.GetCtrl(CtrlNames.Prompt);
=======
    WindowManager.Init();
    local ctrl = WindowManager.GetWnd(CtrlNames.LoginWnd);
>>>>>>> .r23
    --if ctrl ~= nil and AppConst.ExampleMode == 1 then
        ctrl:Open();
    --end


    coroutine.start(this.test_coroutine);
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

