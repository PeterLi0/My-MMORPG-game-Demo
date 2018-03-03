using System.Collections.Generic;
using System.Xml;
using System.Reflection;

/// <summary>
/// 游戏配置解析器 
/// </summary>
public class ConfigParser : IConfigParser
{
    /// <summary>
    /// 载入xml配置
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="tablename"></param>
    /// <returns></returns>
    public override Dictionary<int, T> LoadConfig<T>(string tablename)
    {
        // 定义配置字典
        Dictionary<int, T> dic = new Dictionary<int, T>();

        //// 定义xml文档
        //XmlDocument doc = new XmlDocument();
        
        //// 动态加载xml文档并强制转化为TextAsset
        //TextAsset text = Resources.Load("Config/" + tablename) as TextAsset;

        //// 载入文本资源的文本信息
        //doc.LoadXml(text.text);

        // 定义xml文档
        XmlDocument doc = new XmlDocument();

        string path = "Config/" + tablename + ".xml";
        doc.Load(path);

        // 通过节点路径获取配置的节点列表
        XmlNodeList nodeList = doc.SelectNodes("Nodes/Node");


		// 遍历节点列表，并获取列表中的所有数据
		for (int i = 0; i < nodeList.Count; i++)
		{
			// 获取节点列表的一个子节点，并强制转化为Xml元素;
			XmlNode node = nodeList[i];
			XmlElement elem = (XmlElement)node;

			// 生成一个配置对象，并将对象的成员赋值
			T obj = GreateAndSetValue<T>(elem);

			// 获取对象类型，并通过ID获取域的值
			FieldInfo fieldInfo = obj.GetType().GetField("ID");

			int ID = (int)fieldInfo.GetValue(obj);


			// 将读出的对象添加到配置字典中
			if (!dic.ContainsKey(ID))
			{
				dic.Add(ID, obj);
			}
		}

        //LogManager.Log(string.Format("Load {0} Configs...\n", typeof(T).Name));

        return dic;
    }
}