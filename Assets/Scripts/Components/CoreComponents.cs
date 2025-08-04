using UnityEngine;
using System;

namespace SuperRobot
{
    /// <summary>
    /// 基础属性组件 - 只包含核心标识信息
    /// </summary>
    public class UnitIdentityComponent : IComponent
    {
        public string UnitTemplateId { get; set; }
        public string UnitName { get; set; }
        public UnitType UnitType { get; set; }
        public bool RequiresPilot { get; set; }

        public void Initialize()
        {
            // 默认初始化
        }

        public void Initialize(UnitTemplate template)
        {
            UnitTemplateId = template.UnitId;
            UnitName = template.UnitName;
            UnitType = template.UnitType;
            RequiresPilot = template.RequiresPilot;
        }

        public void Cleanup()
        {
            // 无需清理
        }
    }

    /// <summary>
    /// 健康组件 - 管理生命值
    /// </summary>
    public class HealthComponent : IComponent
    {
        public int MaxHealth { get; set; }
        public int CurrentHealth { get; set; }

        public float HealthPercentage => MaxHealth > 0 ? (float)CurrentHealth / MaxHealth : 0f;
        public bool IsAlive => CurrentHealth > 0;
        public bool IsFullHealth => CurrentHealth >= MaxHealth;

        public void Initialize()
        {
            MaxHealth = 100;
            CurrentHealth = MaxHealth;
        }

        public void Initialize(int maxHealth)
        {
            MaxHealth = maxHealth;
            CurrentHealth = MaxHealth;
        }

        public void Cleanup()
        {
            // 无需清理
        }

        public void TakeDamage(int damage)
        {
            CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
        }

        public void Heal(int amount)
        {
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
        }

        public void SetMaxHealth(int newMaxHealth)
        {
            float healthRatio = HealthPercentage;
            MaxHealth = newMaxHealth;
            CurrentHealth = Mathf.RoundToInt(MaxHealth * healthRatio);
        }
    }

    /// <summary>
    /// 能量组件 - 管理能量值
    /// </summary>
    public class EnergyComponent : IComponent
    {
        public int MaxEnergy { get; set; }
        public int CurrentEnergy { get; set; }
        public int EnergyRegenRate { get; set; } = 10; // 每回合恢复的能量

        public float EnergyPercentage => MaxEnergy > 0 ? (float)CurrentEnergy / MaxEnergy : 0f;
        public bool HasEnoughEnergy(int required) => CurrentEnergy >= required;

        public void Initialize()
        {
            MaxEnergy = 100;
            CurrentEnergy = MaxEnergy;
        }

        public void Initialize(int maxEnergy)
        {
            MaxEnergy = maxEnergy;
            CurrentEnergy = MaxEnergy;
        }

        public void Cleanup()
        {
            // 无需清理
        }

        public bool ConsumeEnergy(int amount)
        {
            if (CurrentEnergy >= amount)
            {
                CurrentEnergy -= amount;
                return true;
            }
            return false;
        }

        public void RestoreEnergy(int amount)
        {
            CurrentEnergy = Mathf.Min(MaxEnergy, CurrentEnergy + amount);
        }

        public void RegenerateEnergy()
        {
            RestoreEnergy(EnergyRegenRate);
        }
    }

    /// <summary>
    /// 装甲组件 - 管理防御能力
    /// </summary>
    public class ArmorComponent : IComponent
    {
        public int BaseArmor { get; set; }
        public int ArmorBonus { get; set; } = 0;
        
        public int TotalArmor => BaseArmor + ArmorBonus;
        public float DamageReduction => Mathf.Clamp01(TotalArmor / 100f);

        public void Initialize()
        {
            BaseArmor = 10;
        }

        public void Initialize(int baseArmor)
        {
            BaseArmor = baseArmor;
        }

        public void Cleanup()
        {
            // 无需清理
        }

        public int ReduceDamage(int incomingDamage)
        {
            float reducedDamage = incomingDamage * (1f - DamageReduction);
            return Mathf.RoundToInt(reducedDamage);
        }
    }

    /// <summary>
    /// 行动点组件 - 管理回合行动能力
    /// </summary>
    public class ActionPointsComponent : IComponent
    {
        public int MaxActionPoints { get; set; } = 2;
        public int CurrentActionPoints { get; set; }

        public bool CanAct => CurrentActionPoints > 0;
        public bool HasActionPoints(int required) => CurrentActionPoints >= required;

        public void Initialize()
        {
            CurrentActionPoints = MaxActionPoints;
        }

        public void Initialize(int maxActionPoints)
        {
            MaxActionPoints = maxActionPoints;
            CurrentActionPoints = MaxActionPoints;
        }

        public void Cleanup()
        {
            // 无需清理
        }

        public bool ConsumeActionPoints(int amount = 1)
        {
            if (CurrentActionPoints >= amount)
            {
                CurrentActionPoints -= amount;
                return true;
            }
            return false;
        }

        public void ResetActionPoints()
        {
            CurrentActionPoints = MaxActionPoints;
        }

        public void AddActionPoints(int amount)
        {
            CurrentActionPoints = Mathf.Min(MaxActionPoints, CurrentActionPoints + amount);
        }
    }

    /// <summary>
    /// 移动能力组件 - 管理移动范围
    /// </summary>
    public class MovementStatsComponent : IComponent
    {
        public int MovementRange { get; set; }
        public int MovementBonus { get; set; } = 0;
        
        public int TotalMovementRange => MovementRange + MovementBonus;

        public void Initialize()
        {
            MovementRange = 3;
        }

        public void Initialize(int movementRange)
        {
            MovementRange = movementRange;
        }

        public void Cleanup()
        {
            // 无需清理
        }
    }

    /// <summary>
    /// 攻击能力组件 - 管理攻击加成
    /// </summary>
    public class AttackStatsComponent : IComponent
    {
        public int RangedAttackBonus { get; set; } = 0;
        public int MeleeAttackBonus { get; set; } = 0;
        public int AccuracyBonus { get; set; } = 0;
        public int CriticalBonus { get; set; } = 0;

        public void Initialize()
        {
            // 默认无加成
        }

        public void Cleanup()
        {
            // 无需清理
        }

        public int GetAttackBonus(WeaponType weaponType)
        {
            return weaponType switch
            {
                WeaponType.Ranged => RangedAttackBonus,
                WeaponType.Melee => MeleeAttackBonus,
                _ => 0
            };
        }
    }
}