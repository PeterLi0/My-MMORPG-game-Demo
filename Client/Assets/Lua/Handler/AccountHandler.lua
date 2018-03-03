---
--- Created by shang.
--- DateTime: 2017/12/19 12:12
---

require("Common/msgid")
AccountHandler = {};
local this = AccountHandler;

function AccountHandler.Register()
    logError("Register msg");
    networkMgr:Register(1002, AccountHandler.OnRespLogin);
    networkMgr:Register(1004, AccountHandler.OnRegisterResp);
    networkMgr:Register(1006, AccountHandler.OnOffline);
end

function AccountHandler.OnRespLogin(m)

<<<<<<< .mine
    local rsp = account_pb.RespLogin();
||||||| .r21
    local msg = account_pb.RespLogin();

=======
    local msg = account_pb.RespLogin();
>>>>>>> .r23
    local data = ByteArray.GetBuffer(m.message);
    rsp:ParseFromString(data);
    if rsp.msgtips == 1003 then
        MessageBox.Open("Account or Password Error");
    end
    if rsp.msgtips == 1005 then
        MessageBox.Open("Login Success");
    end
    ---logError(rsp.msgtips);


    local msgWnd = WindowManager.GetWnd(CtrlNames.MessageBox);

    ---msgWnd.Open();
    msgWnd.Show(msg.msgtips);

end

function AccountHandler.OnRegisterResp(m)
    local rsp = account_pb.RespRegister();
    local data = ByteArray.GetBuffer(m.message);
<<<<<<< .mine
    rsp:ParseFromString(data);
||||||| .r21
    msg:ParseFromString(data);
=======
    rsp:ParseFromString(data);
    logError(rsp.msgtips);
>>>>>>> .r23

<<<<<<< .mine
    logError(rsp.msgtips);

||||||| .r21

=======
    local msgWnd = WindowManager.GetWnd(CtrlNames.MessageBox);
    msgWnd.Open();
    msgWnd.Show(rsp.msgtips);
>>>>>>> .r23

    if rsp.msgtips == 1006 then
        MessageBox:Open():Show(1006);
    end
    if rsp.msgtips == 1007 then
        MessageBox:Open():Show(1007);
    end

end

--- 下线通知
function OnOffline(m)
    local rsp = account_pb.RespOffline();

    local data = ByteArray.GetBuffer(m.message);
    rsp:ParseFromString(data);
end