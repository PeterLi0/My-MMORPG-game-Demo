using System;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// 子弹类技能
/// </summary>
public class SpellBullet : Spell
{
    public SpellBullet(Character caster) : base(caster)
    {
        _needUpdate = true;
    }
}



