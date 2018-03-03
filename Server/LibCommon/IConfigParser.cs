//using UnityEngine;
using System.Collections.Generic;
using System;
using System.Xml;
using System.Reflection;


public abstract class IConfigParser
{
    public abstract Dictionary<int, T> LoadConfig<T>(string tablename);

    protected T GreateAndSetValue<T>(XmlElement node)
    {
        // 通过类型创建一个对象实例
        T obj = Activator.CreateInstance<T>();

        // 获取一个类的所有字段
        FieldInfo[] fields = typeof(T).GetFields();

        for (int i = 0; i < fields.Length; i++)
        {
            string name = fields[i].Name;
            if (string.IsNullOrEmpty(name)) continue;

            string fieldValue = node.GetAttribute(name);
            if (string.IsNullOrEmpty(fieldValue)) continue;

            try
            {
                ParsePropertyValue<T>(obj, fields[i], fieldValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("XML读取错误：对象类型({2}) => 属性名({0}) => 属性类型({3}) => 属性值({1})",
                    fields[i].Name, fieldValue, typeof(T).ToString(), fields[i].FieldType.ToString()));
            }
        }
        return obj;
    }


    private void ParsePropertyValue<T>(T obj, FieldInfo fieldInfo, string valueStr)
    {
        System.Object value = valueStr;

        // 将字符串解析为类中定义的类型
        if (fieldInfo.FieldType.IsEnum)
            value = Enum.Parse(fieldInfo.FieldType, valueStr);
        else
        {
            if (fieldInfo.FieldType == typeof(int))
                value = int.Parse(valueStr);
            else if (fieldInfo.FieldType == typeof(byte))
                value = byte.Parse(valueStr);
            else if (fieldInfo.FieldType == typeof(bool))
                value = bool.Parse(valueStr);
            else if (fieldInfo.FieldType == typeof(float))
                value = float.Parse(valueStr);
            else if (fieldInfo.FieldType == typeof(double))
                value = double.Parse(valueStr);
            else if (fieldInfo.FieldType == typeof(uint))
                value = uint.Parse(valueStr);
            else if (fieldInfo.FieldType == typeof(ulong))
                value = ulong.Parse(valueStr);
        }

        if (value == null)
            return;

        fieldInfo.SetValue(obj, value);
    }
}