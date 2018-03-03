//using System;
//using System.Collections.Generic;
//using UnityEngine;
//using System.Linq;


//public class CharacterManager : Singleton<CharacterManager>
//{
//    // 场景中的所有角色
//    private Dictionary<int, Character> _characters = new Dictionary<int, Character>();

//    // 角色ID计数器
//    private int _roleidCounter = 1001;

//    public Character Create(int globalid,  RoleCfg roleCfg, Vector3 position, Type t)
//    {
//        Character ch = null;
//        ch = Activator.CreateInstance(t, new object[3] { globalid, roleCfg, position }) as Character;
//        _characters.Add(globalid, ch);
//        return ch;
//    }

//    private void Add(int globalid, Character ch)
//    {
//        // 将创建的角色添加到字典中
//        if (!_characters.ContainsKey(ch.GlobalID))
//            _characters.Add(ch.GlobalID, ch);
//    }

//    public void Remove(int characterid)
//    {
//        Character ch = _characters[characterid];
//        ch.Leave();
//        _characters.Remove(characterid);
//    }

//    /// <summary>
//    /// 更新角色
//    /// </summary>
//    public void Update(float dt)
//    {
//        foreach (Character ch in _characters.Values.ToList())
//        {
//            if (!ch.alive)
//            {
//                _characters.Remove(ch.GlobalID);
//            }
//            else
//                ch.Update(dt);
//        }
//    }

//    public Character GetCharacter(int globalID)
//    {
//        return _characters[globalID];
//    }

//    /// <summary>
//    /// 获取某一阵营的所有单位
//    /// </summary>
//    /// <param name="side"></param>
//    /// <returns></returns>
//    public Dictionary<int, Character> GetSideCharacters(Race side)
//    {
//        Dictionary<int, Character> chs = new Dictionary<int, Character>();

//        foreach (Character ch in _characters.Values)
//        {
//            if (ch.side == side)
//                chs.Add(ch.GlobalID, ch);
//        }

//        return chs;
//    }


//    /// <summary>
//    /// 获取非某一阵营的所有单位
//    /// </summary>
//    /// <param name="side"></param>
//    /// <returns></returns>
//    public Dictionary<int, Character> GetNonSideCharacters(Race side)
//    {
//        Dictionary<int, Character> chs = new Dictionary<int, Character>();

//        foreach (Character ch in _characters.Values)
//        {
//            if (ch.side != side)
//                chs.Add(ch.GlobalID, ch);
//        }

//        return chs;
//    }

//    public void Clear()
//    {
//        _roleidCounter = 1001;

//        foreach (Character ch in _characters.Values)
//            ch.Finalise();

//        _characters.Clear();
//    }
//}