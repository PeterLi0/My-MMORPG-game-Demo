
MessageBox = {};
local this = MessageBox;

local behavior;
local transform;
local gameObject;
local btnOK;
local text;
--构建函数--
function MessageBox.New()
	logWarn("MessageBox.New--->>");
	return this;
end

function MessageBox.Open(message)
	logWarn("MessageBox.Awake--->>");
	panelMgr:CreatePanel('MessageBox', this.OnCreate);
	text.text = message;
end

--启动事件--
function MessageBox.OnCreate(obj)
	gameObject = obj;

	transform = gameObject.transform;
	btnOK = transform:FindChild("Button").gameObject;
	text = transform:FindChild("Text"):GetComponent('InputField');

	behavior = gameObject:GetComponent('LuaBehaviour');
	behavior:AddClick(btnOK, this.OnClick);

	logWarn("Start lua--->>"..gameObject.name);
end

--单击事件--
function MessageBox.OnClick(go)
	panelMgr:ClosePanel(CtrlNames.MessageBox);
end

--关闭事件--
function MessageBox.Close()
	panelMgr:ClosePanel(CtrlNames.MessageBox);
end