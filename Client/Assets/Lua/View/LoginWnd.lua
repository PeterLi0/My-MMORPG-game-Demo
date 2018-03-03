
require "Common/define"
require "3rd/pbc/protobuf"
require "3rd/pblua/account_pb"


LoginWnd = {};
local this = LoginWnd;


local behavior;
local transform;
local gameObject;
local btnLogin;
local btnRegister;
local acc;
local pwd;
local inputacc;
local inputpwd;
local btnRegister;
local btnLogin;

--构建函数--
function LoginWnd.New()
<<<<<<< .mine
	logWarn("LoginWnd.New--->>");
||||||| .r21
	logWarn("PromptCtrl.New--->>");
=======
>>>>>>> .r23

	return this;
end

function LoginWnd.Open()
<<<<<<< .mine
	logWarn("LoginWnd.Awake--->>");
||||||| .r21
	logWarn("PromptCtrl.Awake--->>");
=======

>>>>>>> .r23
	panelMgr:CreatePanel('LoginWnd', this.OnCreate);
end

--启动事件--
function LoginWnd.OnCreate(obj)

	gameObject = obj;
	transform = obj.transform;
--[[
	this.inputacc = transform:FindChild("Acc"):GetComponent("InputField");
	this.inputpwd = transform:FindChild("Pwd"):GetComponent("InputField");
	this.btnRegister = transform:FindChild("BtnRegister").gameObject;
	this.btnLogin = transform:FindChild("BtnLogin").gameObject;
--]]
	btnLogin = transform:FindChild("BtnLogin").gameObject;
	btnRegister = transform:FindChild("BtnRegister").gameObject;

<<<<<<< .mine
	acc = transform:FindChild("Acc"):GetComponent('InputField');
	pwd = transform:FindChild("Pwd"):GetComponent('InputField');

	--panel = transform:GetComponent('UIPanel');
||||||| .r21
	--panel = transform:GetComponent('UIPanel');
=======
>>>>>>> .r23
	behavior = transform:GetComponent('LuaBehaviour');
<<<<<<< .mine
	--logWarn("Start lua--->>"..gameObject.name);

	behavior:AddClick(btnLogin, this.OnLoginClick);
	behavior:AddClick(btnRegister, this.OnRegisterClick);
	--resMgr:LoadPrefab('PromptItem', { 'PromptItem' }, this.InitPanel);

||||||| .r21
	--logWarn("Start lua--->>"..gameObject.name);

	behavior:AddClick(this.btnOpen, this.OnClick);
	resMgr:LoadPrefab('PromptItem', { 'PromptItem' }, this.InitPanel);

=======
	behavior:AddClick(this.btnRegister, this.OnRegisterBtnClick);
	behavior:AddClick(this.btnLogin, this.OnLoginBtnClick);
>>>>>>> .r23
end

--初始化面板--
function LoginWnd.InitPanel(objs)
	local count = 100; 
	local parent = this.gridParent;
	for i = 1, count do
		local go = newObject(objs[0]);
		go.name = 'Item'..tostring(i);
		go.transform:SetParent(parent);
		go.transform.localScale = Vector3.one;
		go.transform.localPosition = Vector3.zero;
		behavior:AddClick(go, this.OnItemClick);

	    local label = go.transform:Find('Text');
	    label:GetComponent('Text').text = tostring(i);
	end
end

--滚动项单击--
function LoginWnd.OnItemClick(go)
    log(go.name);
end

<<<<<<< .mine
--登录事件--
function LoginWnd.OnLoginClick(go)
||||||| .r21
--单击事件--
function LoginWnd.OnClick(go)
=======
function LoginWnd.OnRegisterBtnClick(go)
	local req = account_pb.ReqRegister();
	req.account = this.inputacc.text;
	req.password = this.inputpwd.text;
>>>>>>> .r23

	local msg = req:SerializeToString();

	local buffer = ByteArray.New();
	buffer:write(msg);
	networkMgr:SendMessage(1003, buffer);

end


function LoginWnd.OnLoginBtnClick(go)

	local req = account_pb.ReqLogin();
<<<<<<< .mine
	req.account = acc.text;
	req.password = pwd.text;
||||||| .r21
	req.account = "aaaa";
	req.password = "1234";
=======
	req.account = this.inputacc.text;
	req.password = this.inputpwd.text;
>>>>>>> .r23

	local msg = req:SerializeToString();

	local buffer = ByteArray.New();
	buffer:write(msg);
	networkMgr:SendMessage(1001, buffer);
    logWarn("OnClick---->>>"..go.name);
end

--注册事件--
function LoginWnd.OnRegisterClick(go)
	local req = account_pb.ReqRegister();
	req.account = acc.text;
	req.password = pwd.text;
	local msg = req:SerializeToString();

	local buffer = ByteArray.New();
	buffer:write(msg);
	networkMgr:SendMessage(1003, buffer);
end

--关闭事件--
function LoginWnd.Close()
	panelMgr:ClosePanel(CtrlNames.LoginWnd);
end