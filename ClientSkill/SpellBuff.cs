using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Buff
/// </summary>
public class SpellBuff : Spell
{
    public SpellBuff(Character caster) : base(caster)
    {
        _needUpdate = true;
    }
}


