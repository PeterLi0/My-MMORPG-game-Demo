using System;
using System.Collections.Generic;
using System.IO;


/// <summary>
/// 将数据写入成二进制
/// </summary>
public class ByteArray
{
    MemoryStream ms = new MemoryStream();

    BinaryWriter bw;
    BinaryReader br;

    public void Close()
    {
        bw.Close();
        br.Close();
        ms.Close();
    }

    /// <summary>
    /// 支持传入初始数据的构造
    /// </summary>
    /// <param name="buff"></param>
    public ByteArray(byte[] buff)
    {
        ms = new MemoryStream(buff);
        bw = new BinaryWriter(ms);
        br = new BinaryReader(ms);
    }

    /// <summary>
    /// 获取当前数据 读取到的下标位置
    /// </summary>
    public int Position
    {
        get { return (int)ms.Position; }
    }

    /// <summary>
    /// 获取当前数据长度
    /// </summary>
    public int Length
    {
        get { return (int)ms.Length; }
    }
    /// <summary>
    /// 当前是否还有数据可以读取
    /// </summary>
    public bool Readnable
    {
        get { return ms.Length > ms.Position; }
    }

    /// <summary>
    /// 默认构造
    /// </summary>
    public ByteArray()
    {
        bw = new BinaryWriter(ms);
        br = new BinaryReader(ms);
    }

    public void write(int value)
    {
        bw.Write(value);
    }
    public void write(byte value)
    {
        bw.Write(value);
    }
    public void write(bool value)
    {
        bw.Write(value);
    }
    public void write(string value)
    {
        bw.Write(value);
    }
    public void write(byte[] value)
    {
        bw.Write(value);
    }

    public void write(double value)
    {
        bw.Write(value);
    }
    public void write(float value)
    {
        bw.Write(value);
    }
    public void write(long value)
    {
        bw.Write(value);
    }


    public void read(out int value)
    {
        value = br.ReadInt32();
    }
    public void read(out byte value)
    {
        value = br.ReadByte();
    }
    public void read(out bool value)
    {
        value = br.ReadBoolean();
    }
    public void read(out string value)
    {
        value = br.ReadString();
    }
    public void read(out byte[] value, int length)
    {
        value = br.ReadBytes(length);
    }

    public void read(out double value)
    {
        value = br.ReadDouble();
    }
    public void read(out float value)
    {
        value = br.ReadSingle();
    }
    public void read(out long value)
    {
        value = br.ReadInt64();
    }

    public void reposition()
    {
        ms.Position = 0;
    }

    /// <summary>
    /// 获取数据
    /// </summary>
    /// <returns></returns>
    public byte[] getBuff()
    {
        byte[] result = new byte[ms.Length];
        Buffer.BlockCopy(ms.GetBuffer(), 0, result, 0, (int)ms.Length);
        return result;
    }
}



public class Encoding
{
    /// <summary>
    /// 粘包长度编码
    /// </summary>
    /// <param name="buff"></param>
    /// <returns></returns>
    public static byte[] LengthEncode(byte[] buff)
    {
        MemoryStream ms = new MemoryStream();//创建内存流对象
        BinaryWriter sw = new BinaryWriter(ms);//写入二进制对象流

        //写入消息长度
        sw.Write(buff.Length);

        //写入消息体
        sw.Write(buff);
        byte[] result = new byte[ms.Length];
        Buffer.BlockCopy(ms.GetBuffer(), 0, result, 0, (int)ms.Length);
        sw.Close();
        ms.Close();
        return result;

    }

    /// <summary>
    /// 粘包长度解码
    /// </summary>
    /// <param name="cache"></param>
    /// <returns></returns>
    public static byte[] LengthDecode(ref List<byte> cache)
    {
        if (cache.Count < 4) return null;

        // 创建内存流对象，并将缓存数据写入进去
        MemoryStream ms = new MemoryStream(cache.ToArray());

        // 二进制读取流
        BinaryReader br = new BinaryReader(ms);

        // 从缓存中读取int型消息体长度
        int length = br.ReadInt32();

        // 如果消息体长度 大于缓存中数据长度 说明消息没有读取完 等待下次消息到达后再次处理
        if (length > ms.Length - ms.Position)
        {
            return null;
        }

        // 读取正确长度的数据
        byte[] result = br.ReadBytes(length);

        // 清空缓存
        cache.Clear();

        // 将读取后的剩余数据写入缓存
        cache.AddRange(br.ReadBytes((int)(ms.Length - ms.Position)));

        br.Close();
        ms.Close();
        return result;
    }

    /// <summary>
    /// 消息体序列化
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static byte[] MsgEncode(object value)
    {
        SocketModel model = value as SocketModel;
        ByteArray ba = new ByteArray();
        //ba.write(model.type);
        //ba.write(model.area);
        ba.write(model.command);

        //判断消息体是否为空  不为空则序列化后写入
        if (model.message.Length > 0)
        {
            ba.write(model.message);
        }
        byte[] result = ba.getBuff();
        ba.Close();
        return result;
    }

    /// <summary>
    /// 消息体反序列化
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static object MsgDecode(byte[] value)
    {
        ByteArray ba = new ByteArray(value);
        SocketModel model = new SocketModel();
        int command;
        ba.read(out command);
        model.command = command;

        //判断读取完协议后 是否还有数据需要读取 是则说明有消息体 进行消息体读取
        if (ba.Readnable)
        {
            byte[] message;

            //将剩余数据全部读取出来
            ba.read(out message, ba.Length - ba.Position);

            //反序列化剩余数据为消息体
            model.message = message;
        }

        ba.Close();
        return model;
    }
}