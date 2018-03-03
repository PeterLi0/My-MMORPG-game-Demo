
<<<<<<< .mine
CtrlNames = {
	LoginWnd = "LoginWnd",
	MessageBox = "MessageBox"
}
||||||| .r21
CtrlNames = {
	Prompt = "PromptCtrl",
	Message = "MessageCtrl"
}
=======
>>>>>>> .r23

<<<<<<< .mine
PanelNames = {
	"LoginWnd",
	"MessageBox",
}
||||||| .r21
PanelNames = {
	"PromptPanel",	
	"MessagePanel",
}
=======
>>>>>>> .r23


Util = LuaFramework.Util;
AppConst = LuaFramework.AppConst;
LuaHelper = LuaFramework.LuaHelper;
ByteBuffer = LuaFramework.ByteBuffer;


resMgr = LuaHelper.GetResManager();
panelMgr = LuaHelper.GetPanelManager();
soundMgr = LuaHelper.GetSoundManager();
networkMgr = LuaHelper.GetNetManager();

WWW = UnityEngine.WWW;
GameObject = UnityEngine.GameObject;