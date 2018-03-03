using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : Singleton<SkillManager>
{
    // 场景中的所有命中前对象
    private Dictionary<int, List<BeforeHit>> _beforeHits = new Dictionary<int, List<BeforeHit>>();

    // 场景中的所有法术
    private Dictionary<int, List<Spell>> _spells = new Dictionary<int, List<Spell>>();

    // 所有特效
    private List<GameObject> _effects = new List<GameObject>();

    /// <summary>
    ///  创建特效
    /// </summary>
    /// <param name="effectName">特效名称</param>
    /// <param name="position">特效播放位置</param>
    /// <param name="forward">特效的朝向</param>
    public void PlayEffect(string effectName, Vector3 position, Vector3 forward)
    {
        GameObject go = PoolManager.instance.Spawn("FX/", effectName);
        go.transform.position = position;
        go.transform.forward = forward;
        go.transform.localScale = Vector3.one;

        ParticleSystem ps = go.GetComponent<ParticleSystem>();
        ps.Clear();
        ps.time = 0;
        ps.Play();

        ParticleSystem[] pss = go.GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < pss.Length; i++)
        {
            ParticleSystem child = pss[i];
            child.Clear();
            child.time = 0;
            child.Play();
        }

        _effects.Add(go);
    }

    public void Update(float dt)
    {
        // 更新特效
        UpdateEffects();

        // 更新命中前对象
        UpdateBeforeHit(dt);

        // 更新法术
        UpdateSpell(dt);
    }

    /// <summary>
    /// 更新特效
    /// </summary>
    public void UpdateEffects()
    {
        for (int i = 0; i < _effects.Count; i++)
        {
            GameObject go = _effects[i];
            ParticleSystem ps = go.GetComponent<ParticleSystem>();
            if (!ps.isPlaying)
            {
                PoolManager.instance.Unspawn(go);
                _effects.Remove(go);
            }
        }
    }

    /// <summary>
    /// 添加命中前对象
    /// </summary>
    /// <param name="casterid">施法者的唯一ID</param>
    /// <param name="beforeHit"></param>
    public void AddBeforeHit(int casterid, BeforeHit beforeHit)
    {
        // 如果容器中有角色对应的命中前对象
        if (_beforeHits.ContainsKey(casterid))
        {
            _beforeHits[casterid].Add(beforeHit);
        }
        else
        {
            _beforeHits.Add(casterid, new List<BeforeHit>());
            _beforeHits[casterid].Add(beforeHit);
        }
    }

    /// <summary>
    /// 添加法术
    /// </summary>
    /// <param name="casterid"></param>
    /// <param name=""></param>
    public void AddSpell(int casterid, Spell spell)
    {
        if (_spells.ContainsKey(casterid))
        {
            _spells[casterid].Add(spell);
        }
        else
        {
            _spells.Add(casterid, new List<Spell>());
            _spells[casterid].Add(spell);
        }
    }

    /// <summary>
    /// 更新命中前对象
    /// </summary>
    /// <param name="dt"></param>
    private void UpdateBeforeHit(float dt)
    {
        foreach (List<BeforeHit> beforeHits in _beforeHits.Values)
        {
            for (int i = 0; i < beforeHits.Count; i++)
            {
                BeforeHit bh = beforeHits[i];
                if (bh.hit)
                    beforeHits.Remove(bh);
                else
                    bh.Update(dt);
            }
        }
    }

    /// <summary>
    /// 更新法术
    /// </summary>
    /// <param name="dt"></param>
    private void UpdateSpell(float dt)
    {
        foreach (List<Spell> spells in _spells.Values)
        {
            for (int i = 0; i < spells.Count; i++)
            {
                Spell spell = spells[i];
                if (spell.hited)
                    spells.Remove(spell);
                else
                    spell.Update(dt);
            }
        }
    }

    public void Clear()
    {
        _beforeHits.Clear();
        _spells.Clear();
        _effects.Clear();
    }
}