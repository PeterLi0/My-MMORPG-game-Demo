using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ProtoBuf;

public class SerializeUtil
{
    /// <summary>
    /// 对象序列化
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static byte[] Serialize(object value)
    {
        MemoryStream ms = new MemoryStream();        // 创建编码解码的内存流对象
        BinaryFormatter bw = new BinaryFormatter();  // 二进制流序列化对象

        //将obj对象序列化成二进制数据 写入到 内存流
        bw.Serialize(ms, value);
        byte[] result = new byte[ms.Length];

        //将流数据 拷贝到结果数组
        Buffer.BlockCopy(ms.GetBuffer(), 0, result, 0, (int)ms.Length);
        ms.Close();
        return result;
    }



    /// <summary>
    /// 反序列化对象
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static object Deserialize(byte[] value)
    {
        MemoryStream ms = new MemoryStream(value);   //创建编码解码的内存流对象 并将需要反序列化的数据写入其中
        BinaryFormatter bw = new BinaryFormatter();  //二进制流序列化对象

        //将流数据反序列化为obj对象
        object result = bw.Deserialize(ms);
        ms.Close();
        return result;
    }

    /// <summary>
    /// 序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static byte[] Serialize<T>(T value)
    {
        MemoryStream ms = new MemoryStream();
        Serializer.Serialize<T>(ms, value);
        byte[] data = ms.ToArray();//length=27  709

        return data;
    }
    /// <summary>
    /// 反序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static T Deserialize<T>(byte[] value) where T : new()
    {
        if (value == null)
        {
            return new T();
        }
        else
        {
            MemoryStream ms1 = new MemoryStream(value);
            T p1 = Serializer.Deserialize<T>(ms1);
            return p1;
        }
    }
}