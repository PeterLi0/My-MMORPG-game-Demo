using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class MsgSender
{
    public static void BroadCast<T>(Dictionary<string, Account> accs, MsgID id, T msg)
    {
        foreach (Account acc in accs.Values)
        {
            NetworkManager.Send(acc.client, (int)id, msg);
        }
    }

    public static void BroadCastToOther<T>(Dictionary<string, Account> accs, uint accountid, MsgID id, T msg)
    {
        foreach (Account acc in accs.Values)
        {
            if(acc.client.accountid != accountid)
                NetworkManager.Send(acc.client, (int)id, msg);
        }
    }
}
