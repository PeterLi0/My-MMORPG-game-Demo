using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            GameServer server = new GameServer();

            //RedisCacheManager.instance.Init();
            //RedisCacheManager.instance.AccountOnline(1001, 1001, "aixi", "1234");
            //RedisCacheManager.instance.AccountOnline(1001, 1002, "gailun", "1234");
            //RedisCacheManager.instance.AccountOnline(1001, 1003, "zhaoxin", "1234");


            //List<AccountData> a = RedisCacheManager.instance.GetAllAccount(1001);

            Console.Read();
        }
    }
}
