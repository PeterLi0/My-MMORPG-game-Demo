﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SuperSocket.SocketBase.Command;
using SuperSocket.SocketBase.Protocol;

namespace SuperSocket.Test.Command
{
    public class NUM : StringCommandBase<TestSession>
    {
        public const string ReplyFormat = "325 received {0}!";

        public override void ExecuteCommand(TestSession session, StringRequestInfo requestInfo)
        {
            session.Send(string.Format(ReplyFormat, requestInfo.Body));
        }

        public override string Name
        {
            get
            {
                return "325";
            }
        }
    }
}
