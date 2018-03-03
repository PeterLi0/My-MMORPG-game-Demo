
require "Common/define"
require "3rd/pbc/protobuf"
require "3rd/pblua/account_pb"


PromptCtrl = {};
local this = PromptCtrl;

--local panel;
local behavior;
local transform;
local gameObject;

--构建函数--
function PromptCtrl.New()
	logWarn("PromptCtrl.New--->>");

	return this;
end

function PromptCtrl.Open()
	logWarn("PromptCtrl.Awake--->>");
	panelMgr:CreatePanel('LoginWnd', this.OnCreate);
end

--启动事件--
function PromptCtrl.OnCreate(obj)
	--[[
	gameObject = obj;
	transform = obj.transform;

	this.btnOpen = transform:Find("Open").gameObject;
	this.gridParent = transform:Find('ScrollView/Grid');

	--panel = transform:GetComponent('UIPanel');
	behavior = transform:GetComponent('LuaBehaviour');
	--logWarn("Start lua--->>"..gameObject.name);

	behavior:AddClick(this.btnOpen, this.OnClick);
	resMgr:LoadPrefab('prompt', { 'PromptItem' }, this.InitPanel);
	--]]
end

--初始化面板--
function PromptCtrl.InitPanel(objs)
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
function PromptCtrl.OnItemClick(go)
    log(go.name);
end

--单击事件--
function PromptCtrl.OnClick(go)

	local req = account_pb.ReqLogin();
	req.account = "aaaa";
	req.password = "1234";

	local msg = req:SerializeToString();

	local buffer = ByteArray.New();
	buffer:write(msg);
	networkMgr:SendMessage(1001, buffer);
    logWarn("OnClick---->>>"..go.name);
end


--关闭事件--
function PromptCtrl.Close()
	panelMgr:ClosePanel(CtrlNames.Prompt);
end