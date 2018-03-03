
public class SocketModel
{

    //// 一级协议 用于区分所属模块
    //public byte type { get; set; }

    //// 二级协议 用于区分 模块下所属子模块
    //public int area { get; set; }

    // 三级协议  用于区分当前处理逻辑功能
    public int command { get; set; }

    // 消息体 当前需要处理的主体数据
    public byte[] message { get; set; }

    public SocketModel() { }
    public SocketModel(/*byte t, int a, */int c, byte[] o)
    {
        //this.type = t;
        //this.area = a;
        this.command = c;
        this.message = o;
    }

    //public T GetMessage<T>()
    //{
    //    return (T)message;
    //}
}