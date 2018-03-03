using System;
using System.Collections.Generic;
using System.Linq;
using common;
using Luna3D;

public class BattleCharacters : Battle
{
    // 场景中的所有角色
    private Dictionary<int, Character> _characters = new Dictionary<int, Character>();

    public override T Create<T>(CharacterDTO dto)
    {
        T ch = new T();
        CharacterAttr attr = CharacterAttr.GetAttr(dto);
        Type t = typeof(T);
        switch(t.Name)
        {
            case "Player":
                attr.type = CharacterType.Player;
                break;

            case "Monster":
                attr.type = CharacterType.Monster;
                break;

            case "Npc":
                attr.type = CharacterType.Npc;
                break;
        }

        // 初始化角色属性
        ch.Init(attr);

        // 初始化导航代理
        ch.InitNavmeshAgent(_navmeshQuery);

        // 将创建的角色添加到字典中
        if (!_characters.ContainsKey(ch.characterid))
            _characters.Add(ch.characterid, ch);

        return ch;
    }

    public override void Remove(int characterid)
    {
        _characters.Remove(characterid);
    }

    /// <summary>
    /// 更新角色
    /// </summary>
    public override void Update(float dt)
    {
        base.Update(dt);

        foreach (Character ch in _characters.Values.ToList())
        {
            if (ch.sceneid != this.globalID)
            {
                _characters.Remove(ch.characterid);
            }
            else
                ch.Update(dt);
        }
    }

    public override Character GetCharacter(int globalID)
    {
        return _characters[globalID];
    }

    ///// <summary>
    ///// 获取某一阵营的所有单位
    ///// </summary>
    ///// <param name="side"></param>
    ///// <returns></returns>
    //public Dictionary<int, Character> GetSideCharacters(Race side)
    //{
    //    Dictionary<int, Character> chs = new Dictionary<int, Character>();

    //    foreach (Character ch in _characters.Values)
    //    {
    //        if (ch.currAttr.race == side)
    //            chs.Add(ch.GlobalID, ch);
    //    }

    //    return chs;
    //}


    ///// <summary>
    ///// 获取非某一阵营的所有单位
    ///// </summary>
    ///// <param name="side"></param>
    ///// <returns></returns>
    //public Dictionary<int, Character> GetNonSideCharacters(Race side)
    //{
    //    Dictionary<int, Character> chs = new Dictionary<int, Character>();

    //    foreach (Character ch in _characters.Values)
    //    {
    //        if (ch.currAttr.race != side)
    //            chs.Add(ch.GlobalID, ch);
    //    }

    //    return chs;
    //}

    public override void Clear()
    {
        foreach (Character ch in _characters.Values)
            ch.Clear();

        _characters.Clear();
    }

    public override Dictionary<int, T> GetTypeChar<T>(CharacterType chType)
    { 
        Dictionary<int, T> chs = new Dictionary<int, T>();
        foreach (T ch in _characters.Values)
        {
            if (ch.attr.type == chType)
                chs.Add(ch.characterid, ch);
        }

        return chs;
    }
}