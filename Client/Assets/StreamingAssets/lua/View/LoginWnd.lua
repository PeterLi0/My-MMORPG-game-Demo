
require "Common/define"
require "3rd/pbc/protobuf"
require "3rd/pblua/account_pb"


LoginWnd = {};
local this = LoginWnd;

--local panel;
local behavior;
local transform;
local gameObject;
local btnLogin;
local btnRegister;
local acc;
local pwd;
--构建函数--
function LoginWnd.New()
	logWarn("LoginWnd.New--->>");

	return this;
end

function LoginWnd.Open()
	logWarn("LoginWnd.Awake--->>");
	panelMgr:CreatePanel('LoginWnd', this.OnCreate);
end

--启动事件--
function LoginWnd.OnCreate(obj)

	gameObject = obj;
	transform = obj.transform;
--[[
	this.btnOpen = transform:Find("Open").gameObject;
	this.gridParent = transform:Find('ScrollView/Grid');
--]]
	btnLogin = transform:FindChild("BtnLogin").gameObject;
	btnRegister = transform:FindChild("BtnRegister").gameObject;

	acc = transform:FindChild("Acc"):GetComponent('InputField');
	pwd = transform:FindChild("Pwd"):GetComponent('InputField');

	--panel = transform:GetComponent('UIPanel');
	behavior = transform:GetComponent('LuaBehaviour');
	--logWarn("Start lua--->>"..gameObject.name);

	behavior:AddClick(btnLogin, this.OnLoginClick);
	behavior:AddClick(btnRegister, this.OnRegisterClick);
	--resMgr:LoadPrefab('PromptItem', { 'PromptItem' }, this.InitPanel);

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

--登录事件--
function LoginWnd.OnLoginClick(go)

	local req = account_pb.ReqLogin();
	req.account = acc.text;
	req.password = pwd.text;

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
	panelMgr:ClosePanel(CtrlNames.Prompt);
end