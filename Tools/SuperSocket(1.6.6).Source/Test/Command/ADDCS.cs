﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Test.Command
{
    public class ADDCS : StringCommandBase<TestSession>
    {
        public override void ExecuteCommand(TestSession session, StringRequestInfo requestInfo)
        {
            session.Send((Convert.ToInt32(requestInfo[0]) + Convert.ToInt32(requestInfo[1])).ToString());
        }
    }
}
