using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SuperRobot
{







    // 技术奖励类型枚举
    public enum TechRewardType
    {
        ResourceBonus,   // 资源奖励
        UnlockUnit,      // 解锁单位
        UnlockWeapon,    // 解锁武器
        ProductionBoost  // 生产加成
    }

    // 技术奖励类
    [Serializable]
    public class TechReward
    {
        public string TechId;           // 关联的技术ID
        public TechRewardType RewardType;
        public ResourceType ResourceType;  // 用于ResourceBonus
        public int Amount;              // 通用数量值
        public string TargetId;         // 用于解锁特定单位或武器
    }


    // 立方坐标系(推荐) - 使用x,y,z三个坐标，满足x+y+z=0
    [Serializable]
    public struct HexCoord
    {
        public int q; // x轴
        public int r; // z轴 
        public int s; // y轴 (s = -q-r)
    
        public HexCoord(int q, int r)
        {
            this.q = q;
            this.r = r;
            this.s = -q - r;
        }
    
        // 确保坐标合法
        public void Validate()
        {
            if (q + r + s != 0)
            {
                Debug.LogWarning("Invalid hex coordinates: q + r + s must equal 0");
                s = -q - r;
            }
        }
    
        // 从Vector2Int转换为HexCoord (使用默认的奇数行偏移)
        public static HexCoord FromVector2Int(Vector2Int vector2Int)
        {
            return HexGridConverter.ToHexCoord(vector2Int);
        }
    
        // 转换为Vector2Int (使用默认的奇数行偏移)
        public Vector2Int ToVector2Int()
        {
            return HexGridConverter.ToVector2Int(this);
        }
    
        // 重写相等方法
        public override bool Equals(object obj)
        {
            if (!(obj is HexCoord))
                return false;
            
            HexCoord other = (HexCoord)obj;
            return q == other.q && r == other.r;
        }
    
        // 重写获取哈希码方法
        public override int GetHashCode()
        {
            return q * 31 + r;
        }
    
        // 重写ToString方法
        public override string ToString()
        {
            return $"({q}, {r}, {s})";
        }
    }

    [Serializable]
    public class UnitTemplate
    {
        public string UnitId;
        public string UnitName;
        public UnitType UnitType;
        public string Description;
        public string ModelPrefabPath;
        public bool RequiresPilot;

        // 基础属性
        public int MaxHealth;
        public int MaxEnergy;
        public int MovementRange;
        public int BaseArmor;

        // 默认武器
        public List<WeaponTemplate> DefaultWeapons;

        // 单位开发成本
        public Dictionary<ResourceType, int> DevelopmentCost;

        // 单位生产成本
        public Dictionary<ResourceType, int> ProductionCost;

        // 单位维护成本
        public Dictionary<ResourceType, int> MaintenanceCost;

        // 解锁条件
        public string[] RequiredTechnologies;

        // 特殊能力
        public List<SpecialAbility> SpecialAbilities;
    }
    // 武器模板
    [Serializable]
    public class WeaponTemplate
    {
        public string WeaponId;
        public string WeaponName;
        public WeaponType WeaponType;
        public string Description;

        // 基础属性
        public int BaseDamage;
        public int Range;
        public int EnergyCost;
        public float Accuracy = 90f;  // 基础命中率
        public float CriticalRate = 5f;  // 暴击率

        // 特殊效果
        public EffectType? EffectType;
        public int EffectValue;
        public int EffectDuration;

        // 研发和生产成本
        public Dictionary<ResourceType, int> DevelopmentCost;
        public Dictionary<ResourceType, int> ProductionCost;

        // 武器解锁条件
        public string[] RequiredTechnologies;

        // 适用的单位类型
        public List<UnitType> CompatibleUnitTypes;
    }

    // 特殊能力定义
    [Serializable]
    public class SpecialAbility
    {
        public string AbilityId;
        public string AbilityName;
        public AbilityType AbilityType;
        public string Description;

        // 能力属性
        public int EnergyCost;
        public int CooldownTurns;
        public int EffectValue;
        public int Duration;
        public int Range;

        // 特殊效果
        public EffectType? EffectType;

        // 触发条件
        public TriggerCondition TriggerCondition;

        // 能力解锁条件
        public int RequiredLevel;
        public int RequiredNTLevel;


    }

    [Serializable]
    public enum TriggerCondition
    {
        Manual,         // 手动触发
        HealthLow,      // 生命值低时
        EnemyClose,     // 敌人接近时
        AllyLow,        // 队友生命低时
        TurnStart,      // 回合开始时
        TurnEnd         // 回合结束时
    }


    [Serializable]
    public class BattleData
    {
        public List<int> ParticipantUnitIds;
        public bool IsPlayerWin;
        public Dictionary<int, int> Contribution;
    }

}
