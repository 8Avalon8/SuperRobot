namespace SuperRobot
{
    // 资源类型枚举
    public enum ResourceType
    {
        Money,              // 基础资金
        ManPower,           // 人力资源
        StandardAlloy,      // 标准合金
        RareMetal,          // 稀有金属
        EnergyCrystal,      // 能量晶体
        SteelRankAlloy,     // 钢岚合金
        BeamOre,            // 光束矿石
        PsychicElement      // 精神感应元件
    }

    // 单位类型枚举
    public enum UnitType
    {
        // 常规军力
        Infantry,
        Tank,               // 坦克
        Aircraft,           // 战机
        Ship,               // 舰船
        MassProdRobot,      // 量产机器人
        Naval, 

        // 超级机器人类型
        SolarPowered,       // 太阳能系列（魔神Z类）
        Gundam,             // 高达系列
        Getter,             // 真盖塔系列
        BioAdaptive,        // 生物适应系列（EVA类）
        Transformer         // 变形金刚系列
    }

    // 武器类型枚举
    public enum WeaponType
    {
        Melee,              // 近战武器
        Ranged,             // 远程武器
        Area,               // 范围攻击
        Beam,               // 光束武器
        Missile,            // 导弹系统
        Special             // 特殊武器/必杀技
    }

    // 能力类型枚举
    public enum AbilityType
    {
        Berserk,            // 狂暴化
        Healing,            // 自我修复
        Shield,             // 能量护盾
        AreaAttack,         // 范围攻击
        Teleport,           // 瞬间移动
        Transform,          // 形态转换
        Combine,            // 合体
        SpecialAttack,      // 特殊攻击
        StatusBoost         // 属性提升
    }

    // 效果类型枚举
    public enum EffectType
    {
        Shield,             // 护盾效果
        Stun,               // 眩晕效果
        Boost,              // 增益效果
        Debuff,             // 减益效果
        Regeneration,       // 恢复效果
        DamageOverTime      // 持续伤害
    }

    // 基地类型枚举
    public enum BaseType
    {
        Headquarters,       // 总部
        ResearchFacility,   // 研究设施
        ProductionPlant,    // 生产工厂
        ResourceMine,       // 资源矿场
        DefenseOutpost      // 防御哨站
    }

    // 战斗地形类型
    public enum TerrainType
    {
        Plain,              // 平原
        Mountain,           // 山地
        Forest,             // 森林
        Desert,             // 沙漠
        Urban,              // 城市
        Water,              // 水域
        Space               // 太空
    }
}
