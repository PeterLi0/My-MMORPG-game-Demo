require "Common/define"
require "View/LoginWnd"
require "View/MessageBox"

CtrlNames =
{
	MessageBox = "MessageBox",
	LoginWnd = "LoginWnd"
}


WindowManager = {};
local this = WindowManager;
local ctrlList = {};	--控制器列表--

function WindowManager.Init()
	logWarn("WindowManager.Init----->>>");
	ctrlList[CtrlNames.LoginWnd] = LoginWnd.New();
	ctrlList[CtrlNames.MessageBox] = MessageBox.New();
	return this;
end

--添加控制器--
function WindowManager.AddWnd(ctrlName, ctrlObj)
	ctrlList[ctrlName] = ctrlObj;
end

--获取控制器--
function WindowManager.GetWnd(ctrlName)
	return ctrlList[ctrlName];
end

--移除控制器--
function WindowManager.RemoveWnd(ctrlName)
	ctrlList[ctrlName] = nil;
end

--关闭控制器--
function WindowManager.Close()
	logWarn('WindowManager.Close---->>>');
end