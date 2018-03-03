using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using common;


[Serializable]
public class MailData
{
    public int id;
    public int sender_id;
    public int receiver_id;
    public string subject;
    public string body;
    public string deliver_time;
    public int money;
    public int has_items;

    public MailData()
    {

    }

    public MailData(int id, int sender_id, int receiver_id, string subject, string body, string deliver_time, int money, int has_items)
    {
        this.id = id;
        this.sender_id = sender_id;
        this.receiver_id = receiver_id;
        this.subject = subject;
        this.body = body;
        this.deliver_time = deliver_time;
        this.money = money;
        this.has_items = has_items;
    }

    public static MailDTO GetMailDTO(MailData d)
    {
        MailDTO dto = new MailDTO();
        dto.id = d.id;
        dto.sender_id = d.sender_id;
        dto.receiver_id = d.receiver_id;
        dto.subject = d.subject;
        dto.body = d.body;
        dto.deliver_time = d.deliver_time;
        dto.money = d.money;
        dto.has_items = d.has_items;

        return dto;
    }
}

public partial class RedisCacheManager
{
    /// <summary>
    /// 载入邮件数据
    /// </summary>
    /// <param name="characterid"></param>
    public void LoadMailData(int characterid)
    {
        string sql = string.Format("SELECT * from mail WHERE receiver_id = {0}", characterid);
        Dictionary<int, MailData> mails = MysqlManager.instance.ExecQueryDic<MailData>(sql);
        foreach (MailData item in mails.Values)
        {
            _redis.Set(characterid, item.id, item);
        }
    }

    // 删除邮件
    public void DeleteMail(int characterid, int mailid)
    {
        _redis.Remove(characterid, mailid);
    }

    public void WriteMailData(int characterid)
    {
        string sql = string.Format("SELECT * from mail WHERE receiver_id = {0}", characterid);
        Dictionary<int, MailData> mails = MysqlManager.instance.ExecQueryDic<MailData>(sql);
        foreach (MailData data in mails.Values)
        {
            if (!mails.ContainsKey(data.id))
            {
                sql = string.Format("DELETE FROM mail WHERE id = {0}", data.id);
                MysqlManager.instance.ExecNonQuery(sql);
            }
        }
    }
}

partial class CacheManager
{
    // 邮件数据
    private Dictionary<int, List<MailData>> _mails = new Dictionary<int, List<MailData>>();

    /// <summary>
    /// 载入邮件数据
    /// </summary>
    /// <param name="characterid"></param>
    public void LoadMailData(int characterid)
    {
        string sql = string.Format("SELECT * from mail WHERE receiver_id = {0}", characterid);
        List<MailData> mails = MysqlManager.instance.ExecQuery<MailData>(sql);

        _mails.Add(characterid, mails);
    }

    // 删除邮件
    public void DeleteMail(int characterid, int mailid)
    {
        //if (mails.ContainsKey(mailid))
        //    mails.Remove(mailid);
        //else
        //    Console.WriteLine("不存在这封邮件{0}", mailid);

        List<MailData> mails = _mails[characterid];

        for (int i = 0; i < mails.Count; i++)
        {
            if (mails[i].id == mailid && mails[i].receiver_id == characterid)
                mails.Remove(mails[i]);
        }
    }

    public void WriteMailData(int characterid)
    {
        string sql = string.Format("SELECT * from mail WHERE receiver_id = {0}", characterid);
        Dictionary<int, MailData> mails = MysqlManager.instance.ExecQueryDic<MailData>(sql);
        foreach (MailData data in mails.Values)
        {
            if (!mails.ContainsKey(data.id))
            {
                sql = string.Format("DELETE FROM mail WHERE id = {0}", data.id);
                MysqlManager.instance.ExecNonQuery(sql);
            }
        }
    }

    public List<MailData> GetMailDatas(int characterid)
    {
        return _mails[characterid];
    }
}