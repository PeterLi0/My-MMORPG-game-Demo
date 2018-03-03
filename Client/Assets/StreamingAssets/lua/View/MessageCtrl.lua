
MessageCtrl = {};
local this = MessageCtrl;

local behavior;
local transform;
local gameObject;
local btnClose;

--构建函数--
function MessageCtrl.New()
	logWarn("MessageCtrl.New--->>");
	return this;
end

function MessageCtrl.Open()
	logWarn("MessageCtrl.Awake--->>");
	panelMgr:CreatePanel('Message', this.OnCreate);
end

--启动事件--
function MessageCtrl.OnCreate(obj)
	gameObject = obj;

	transform = gameObject.transform;
	btnClose = transform:FindChild("Button").gameObject;

	behavior = gameObject:GetComponent('LuaBehaviour');
	behavior:AddClick(btnClose, this.OnClick);

	logWarn("Start lua--->>"..gameObject.name);
end

--单击事件--
function MessageCtrl.OnClick(go)
	destroy(gameObject);
end

--关闭事件--
function MessageCtrl.Close()
	panelMgr:ClosePanel(CtrlNames.Message);
end