using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 技能配置对象
/// </summary>
public abstract class SkillCfgObject
{
    // 技能基础配置
    protected SkillBasicCfg _skillBasicCfg;
    public SkillBasicCfg SkillBasicCfg
    {
        get { return _skillBasicCfg; }
        set { _skillBasicCfg = value; }
    }
    // 子弹类技能配置
    protected SkillBulletCfg _skillBulletCfg;
    public SkillBulletCfg SkillBulletCfg
    {
        get { return _skillBulletCfg; }
        set { _skillBulletCfg = value; }
    }
    // AOE类技能配置
    protected SkillAOECfg _skillAOECfg;
    public SkillAOECfg SkillAOECfg
    {
        get { return _skillAOECfg; }
        set { _skillAOECfg = value; }
    }
    // 技能Buff配置
    protected SkillBuffCfg _skillBuffCfg;
    public SkillBuffCfg SkillBuffCfg
    {
        get { return _skillBuffCfg; }
        set { _skillBuffCfg = value; }
    }
    // 技能陷阱配置
    protected SkillTrapCfg _skillTrapCfg;
    public SkillTrapCfg SkillTrapCfg
    {
        get { return _skillTrapCfg; }
        set { _skillTrapCfg = value; }
    }
}