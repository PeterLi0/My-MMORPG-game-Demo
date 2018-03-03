using System;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.Reflection;
using System.Collections.Generic;


public class MysqlManager  : Singleton<MysqlManager>
{
    // mysql连接对象
    private MySqlConnection _conn;

    // 数据库名
    private string _dbName = "wow";

    // 数据源
    private string _dataSource = "127.0.0.1";

    // 用户名
    private string _userid = "root";

    // 密码
    private string _pwd = "root";

    /// <summary>
    /// 建立mysql数据库链接
    /// </summary>
    /// <returns></returns>
    public void Connect()
    {
        String mysqlStr = string.Format("Database={0};Data Source={1};User Id={2};Password={3};pooling=false;CharSet=utf8;port=3306", _dbName, _dataSource, _userid, _pwd);

        _conn = new MySqlConnection(mysqlStr);
    }

    /// <summary>
    /// 执行非查询型命令
    /// </summary>
    /// <param name="sql"></param>
    public void ExecNonQuery(string sql)
    {
        MySqlCommand command = new MySqlCommand(sql, _conn);

        _conn.Open();
        command.ExecuteNonQuery();
        command.Dispose();
        _conn.Close();
    }

    /// <summary>
    /// 执行查询型命令
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <returns></returns>
    public List<T> ExecQuery<T>(string sql)
    {
        List<T> objList = new List<T>();

        // 创建一个mysql命令
        MySqlCommand command = new MySqlCommand(sql, _conn);

        MySqlDataReader reader = null;

        // 打开数据库
        _conn.Open();

        // 获取数据读取器
        reader = command.ExecuteReader();

        // 开始读
        while (reader.Read())
        {
            T obj = Activator.CreateInstance<T>();
            FieldInfo[] fields = typeof(T).GetFields();

            for (int j = 0; j < reader.FieldCount; j++)
            {
                ReadField<T>(obj, fields[j], reader, j);
            }

            objList.Add(obj);
        }

        reader.Close();
        _conn.Close();
        command.Dispose();

        return objList;
    }

    /// <summary>
    /// 执行查询型命令
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="sql"></param>
    /// <returns></returns>
    public Dictionary<int, T> ExecQueryDic<T>(string sql)
    {
        Dictionary<int, T> objList = new Dictionary<int, T>();

        // 创建一个mysql命令
        MySqlCommand command = new MySqlCommand(sql, _conn);

        MySqlDataReader reader = null;

        // 打开数据库
        _conn.Open();

        // 获取数据读取器
        reader = command.ExecuteReader();

        // 开始读
        while (reader.Read())
        {
            T obj = Activator.CreateInstance<T>();
            FieldInfo[] fields = typeof(T).GetFields();

            for (int j = 0; j < reader.FieldCount; j++)
            {
                ReadField<T>(obj, fields[j], reader, j);
            }

            // 获取对象类型，并通过ID获取域的值
            FieldInfo fieldInfo = obj.GetType().GetField("id");

            int ID = (int)fieldInfo.GetValue(obj);


            // 将读出的对象添加到配置字典中
            if (!objList.ContainsKey(ID))
            {
                objList.Add(ID, obj);
            }
        }

        reader.Close();
        _conn.Close();
        command.Dispose();

        return objList;
    }

    private void ReadField<T>(T obj, FieldInfo fieldInfo, MySqlDataReader reader, int ordinal)
    {
        Type type = fieldInfo.FieldType;
        System.Object value = null;

        if (type == typeof(int))
            value = reader.GetFieldValue<int>(ordinal);
        else if (type == typeof(byte))
            value = reader.GetFieldValue<byte>(ordinal);
        else if (type == typeof(uint))
            value = reader.GetFieldValue<uint>(ordinal);
        else if (type == typeof(string))
            value = reader.GetFieldValue<string>(ordinal);
        else if(type == typeof(float))
            value = reader.GetFieldValue<float>(ordinal);

        fieldInfo.SetValue(obj, value);
    }
}