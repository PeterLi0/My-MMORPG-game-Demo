using System;
using System.Collections.Generic;


public class BattleSkills : BattleCharacters
{
    //// 场景中的所有命中前对象
    //private Dictionary<int, List<BeforeHit>> _beforeHits = new Dictionary<int, List<BeforeHit>>();

    //// 场景中的所有法术
    //private Dictionary<int, List<Spell>> _spells = new Dictionary<int, List<Spell>>();

    public override void Update(float dt)
    {
        base.Update(dt);

        //// 更新命中前对象
        //UpdateBeforeHit(dt);

        //// 更新法术
        //UpdateSpell(dt);
    }

    ///// <summary>
    ///// 添加命中前对象
    ///// </summary>
    ///// <param name="casterid">施法者的唯一ID</param>
    ///// <param name="beforeHit"></param>
    //public override void AddBeforeHit(int casterid, BeforeHit beforeHit)
    //{
    //    // 如果容器中有角色对应的命中前对象
    //    if (_beforeHits.ContainsKey(casterid))
    //    {
    //        _beforeHits[casterid].Add(beforeHit);
    //    }
    //    else
    //    {
    //        _beforeHits.Add(casterid, new List<BeforeHit>());
    //        _beforeHits[casterid].Add(beforeHit);
    //    }
    //}

    ///// <summary>
    ///// 添加法术
    ///// </summary>
    ///// <param name="casterid"></param>
    ///// <param name=""></param>
    //public override void AddSpell(int casterid, Spell spell)
    //{
    //    if (_spells.ContainsKey(casterid))
    //    {
    //        _spells[casterid].Add(spell);
    //    }
    //    else
    //    {
    //        _spells.Add(casterid, new List<Spell>());
    //        _spells[casterid].Add(spell);
    //    }
    //}

    ///// <summary>
    ///// 更新命中前对象
    ///// </summary>
    ///// <param name="dt"></param>
    //private void UpdateBeforeHit(float dt)
    //{
    //    foreach (List<BeforeHit> beforeHits in _beforeHits.Values)
    //    {
    //        for (int i = 0; i < beforeHits.Count; i++)
    //        {
    //            BeforeHit bh = beforeHits[i];
    //            if (bh.hit)
    //                beforeHits.Remove(bh);
    //            else
    //                bh.Update(dt);
    //        }
    //    }
    //}

    ///// <summary>
    ///// 更新法术
    ///// </summary>
    ///// <param name="dt"></param>
    //private void UpdateSpell(float dt)
    //{
    //    foreach (List<Spell> spells in _spells.Values)
    //    {
    //        for (int i = 0; i < spells.Count; i++)
    //        {
    //            Spell spell = spells[i];
    //            if (spell.hited)
    //                spells.Remove(spell);
    //            else
    //                spell.Update(dt);
    //        }
    //    }
    //}

    public override void Clear()
    {
        base.Clear();

        //_beforeHits.Clear();
        //_spells.Clear();
    }
}