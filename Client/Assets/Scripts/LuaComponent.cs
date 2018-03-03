
using UnityEngine;
using System.Collections;
using LuaFramework;
using LuaInterface;

public class LuaComponent : MonoBehaviour
{

    LuaTable luaTable;

    /// <summary>
    /// 添加Lua组件，该方法编写后将在Lua脚本中调用
    /// </summary>
    /// <param name="go"></param>
    /// <param name="luaTable"></param>
    /// <returns></returns>
    public static LuaTable Add(GameObject go, LuaTable luaTable)
    {
        //调用Lua脚本的New方法
        LuaFunction fun = luaTable.GetLuaFunction("New");
        if (fun == null)
            return null;

        //有参函数的fun.Call(luaTable)这个必须传luaTable进去
        //object[] results = fun.Call(luaTable);

        object[] results = fun.LazyCall(luaTable);
        if (results.Length != 1)
            return null;

        //给对象绑定当前组件
        LuaComponent luaCmp = go.AddComponent<LuaComponent>();
        //为组件关联相关的Lua表的一个实例
        luaCmp.luaTable = results[0] as LuaTable;
        luaCmp.CallAwake();
        return luaCmp.luaTable;

    }


    public static LuaTable Get(GameObject go, LuaTable luaTable)
    {

        string meta = luaTable.ToString();
        Debug.Log(meta);

        LuaComponent[] cmps = go.GetComponents<LuaComponent>();
        for (int i = 0; i < cmps.Length; i++)
        {
            LuaComponent cmp = cmps[i];
            string cmpMeta = cmp.luaTable.GetMetaTable().ToString();
            if (meta == cmpMeta)
            {
                return cmp.luaTable;
            }

        }

        throw new System.Exception("没有该组件");


    }

    void CallAwake()
    {
        LuaFunction fun = luaTable.GetLuaFunction("Awake");
        if (fun == null)
            return;
        fun.Call(luaTable);

    }

    void Start()
    {
        LuaFunction fun = luaTable.GetLuaFunction("Start");
        if (fun != null)
            fun.Call(luaTable);
    }


    void Update()
    {
        //效率问题有待测试和优化
        //可在lua中调用UpdateBeat替代
        LuaFunction fun = luaTable.GetLuaFunction("Update");
        if (fun != null)
            //第二个参数作为Lua函数的参数
            fun.Call(luaTable, gameObject);
    }


    void OnCollisionEnter(Collision collisionInfo)
    {
        LuaFunction fun = luaTable.GetLuaFunction("OnCollisionEnter");
        if (fun == null)
            return;
        fun.Call(luaTable, collisionInfo.gameObject.transform.position);

    }


    //更多函数略
}
