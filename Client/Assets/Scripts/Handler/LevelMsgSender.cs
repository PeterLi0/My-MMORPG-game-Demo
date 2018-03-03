using System;
using System.Collections.Generic;
using UnityEngine;
using proto.battlesync;

public class LevelMsgSender
{
    public static void RoleMove(Vector3 dest)
    {
        ReqCharacterMove req = new ReqCharacterMove();
        req.dest = ProtoHelper.UV2PV(dest);

        Net.instance.Send((int)MsgID.ReqCharacterMove, req);
    }

    public static void RoleAttack(uint skillid, uint targetid)
    {

        ReqCharacterAttack req = new ReqCharacterAttack();
        req.skillid = skillid;
        req.targetID = targetid;
        Net.instance.Send((int)MsgID.ReqCharacterAttack, req);
    }
}