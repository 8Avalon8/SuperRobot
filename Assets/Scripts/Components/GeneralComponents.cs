using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
namespace SuperRobot
{

    // 单位基础属性组件
    public class UnitStatsComponent : IComponent
    {
        // 基础属性
        public string UnitTemplateId { get; set; }
        public string UnitName { get; set; }
        public UnitType UnitType { get; set; }

        // 战斗属性
        public int MaxHealth { get; set; }
        public int CurrentHealth { get; set; }
        public int MaxEnergy { get; set; }
        public int CurrentEnergy { get; set; }
        public int BaseArmor { get; set; }
        public int MovementRange { get; set; }

        // 行动点
        public int MaxActionPoints { get; set; }
        public int CurrentActionPoints { get; set; }

        // 各种加成
        public int MovementBonus { get; set; }
        public int RangedAttackBonus { get; set; }
        public int MeleeAttackBonus { get; set; }
        // RequiresPilot
        public bool RequiresPilot { get; set; }

        public void Initialize()
        {
            // 默认初始化
            MaxActionPoints = 2;
            CurrentActionPoints = MaxActionPoints;
            CurrentHealth = MaxHealth;
            CurrentEnergy = MaxEnergy;
        }

        public void Initialize(UnitTemplate template)
        {
            UnitTemplateId = template.UnitId;
            UnitName = template.UnitName;
            UnitType = template.UnitType;

            MaxHealth = template.MaxHealth;
            CurrentHealth = MaxHealth;
            MaxEnergy = template.MaxEnergy;
            CurrentEnergy = MaxEnergy;
            BaseArmor = template.BaseArmor;
            MovementRange = template.MovementRange;

            MaxActionPoints = 2;
            CurrentActionPoints = MaxActionPoints;
        }

        public void Cleanup()
        {
            // 清理资源（如果有的话）
        }

        // 重置行动点（回合开始时调用）
        public void ResetActionPoints()
        {
            CurrentActionPoints = MaxActionPoints;
        }

        // 恢复能量和生命值
        public void Repair(int healthAmount, int energyAmount)
        {
            CurrentHealth = Math.Min(CurrentHealth + healthAmount, MaxHealth);
            CurrentEnergy = Math.Min(CurrentEnergy + energyAmount, MaxEnergy);
        }
    }
    
    // 地面移动组件
    public class GroundMovementComponent : IComponent
    {
        public int MovementRange { get; set; }
        public int RemainingMovement { get; set; }
        public List<Vector2Int> Path { get; set; }

        public void Initialize()
        {
            MovementRange = 0;
            RemainingMovement = 0;
            Path = new List<Vector2Int>();
        }

        public void Cleanup()
        {
            Path.Clear();
        }

        // 设置移动范围
        public void SetMovementRange(int range)
        {
            MovementRange = range;
            RemainingMovement = range;
        }

        // 重置移动范围
        public void ResetMovement()
        {
            RemainingMovement = MovementRange;
            Path.Clear();
        }

        // 添加路径点
        public void AddPathPoint(Vector2Int point)
        {
            Path.Add(point);
        }

        // 移除最后一个路径点
        public void RemoveLastPathPoint()
        {
            if (Path.Count > 0)
            {
                Path.RemoveAt(Path.Count - 1);
            }
        }

        // 检查是否可以移动到指定位置
        public bool CanMoveTo(Vector2Int position)
        {
            return RemainingMovement > 0 && Path.Contains(position);
        }

        // 移动到指定位置
        public void MoveTo(Vector2Int position)
        {
            if (RemainingMovement > 0 && Path.Contains(position))
            {
                RemainingMovement -= 1;
            }
        }
    }
    
    // 飞行移动组件
    public class AirMovementComponent : IComponent
    {
        public int MovementRange { get; set; }
        public int RemainingMovement { get; set; }
        public List<Vector2Int> Path { get; set; }

        public void Initialize()
        {
            MovementRange = 0;
            RemainingMovement = 0;
            Path = new List<Vector2Int>();
        }

        public void Cleanup()
        {
            Path.Clear();
        }

        // 设置移动范围
        public void SetMovementRange(int range)
        {
            MovementRange = range;
            RemainingMovement = range;
        }

        // 重置移动范围
        public void ResetMovement()
        {
            RemainingMovement = MovementRange;
            Path.Clear();
        }

        // 添加路径点
        public void AddPathPoint(Vector2Int point)
        {
            Path.Add(point);
        }

        // 移除最后一个路径点
        public void RemoveLastPathPoint()
        {
            if (Path.Count > 0)
            {
                Path.RemoveAt(Path.Count - 1);
            }
        }

        // 检查是否可以移动到指定位置
        public bool CanMoveTo(Vector2Int position)
        {
            return RemainingMovement > 0 && Path.Contains(position);
        }

        // 移动到指定位置
        public void MoveTo(Vector2Int position)
        {
            if (RemainingMovement > 0 && Path.Contains(position))
            {
                RemainingMovement -= 1;
            }
        }
    }
    
    // SuperRobot组件
    public class SuperRobotComponent : IComponent
    {
        public int NTBonus { get; set; }
        public List<SpecialAbility> SpecialAbilities { get; set; }
        public bool IsBerserkMode { get; set; }

        public void Initialize()
        {
            NTBonus = 0;
            SpecialAbilities = new List<SpecialAbility>();
            IsBerserkMode = false;
        }

        public void Cleanup()
        {
            SpecialAbilities.Clear();
        }

        // 激活特殊能力
        public bool ActivateAbility(string abilityId, GameEntity unit)
        {
            var ability = SpecialAbilities.Find(a => a.AbilityId == abilityId);
            if (ability == null)
                return false;

            var statsComp = unit.GetComponent<UnitStatsComponent>();
            if (statsComp == null)
                return false;

            // 检查能量是否足够
            if (statsComp.CurrentEnergy < ability.EnergyCost)
                return false;

            // 消耗能量
            statsComp.CurrentEnergy -= ability.EnergyCost;

            // 应用能力效果
            ApplyAbilityEffect(ability, unit);

            return true;
        }

        private void ApplyAbilityEffect(SpecialAbility ability, GameEntity unit)
        {
            // 根据能力类型应用不同效果
            switch (ability.AbilityType)
            {
                case AbilityType.Berserk:
                    IsBerserkMode = true;
                    break;
                case AbilityType.Healing:
                    var statsComp = unit.GetComponent<UnitStatsComponent>();
                    statsComp.CurrentHealth = Math.Min(statsComp.CurrentHealth + ability.EffectValue, statsComp.MaxHealth);
                    break;
                case AbilityType.Shield:
                    // 添加临时护盾效果
                    var shieldComp = unit.AddComponent<TemporaryEffectComponent>();
                    shieldComp.EffectType = EffectType.Shield;
                    shieldComp.EffectValue = ability.EffectValue;
                    shieldComp.Duration = ability.Duration;
                    break;
                    // 更多能力类型...
            }
        }
    }

    // 位置组件
    public class PositionComponent : IComponent
    {
        public Vector2Int Position         { get; set; }
        public Vector2Int PreviousPosition { get; set; }
        
        public HexCoord HexPosition
        {
            get { return HexGridConverter.ToHexCoord(Position); }
        }
        
        public HexCoord PreviousHexPosition
        {
            get { return HexGridConverter.ToHexCoord(PreviousPosition); }
        }
    
        public void Initialize()
        {
            Position         = new Vector2Int(0, 0);
            PreviousPosition = new Vector2Int(0, 0);
        }
    
        public void Cleanup()
        {
            // 无需清理
        }
    
        // 设置位置并记录上一个位置
        public void SetPosition(HexCoord newPosition)
        {
            PreviousPosition = Position;
            Position         = newPosition.ToVector2Int();
        }
        
        public void SetPosition(Vector2Int newPosition)
        {
            PreviousPosition = Position;
            Position         = newPosition;
        }
    }

    // 武器组件
    public class WeaponComponent : IComponent
    {
        public string WeaponId { get; set; }
        public string WeaponName { get; set; }
        public WeaponType WeaponType { get; set; }

        // 武器属性
        public int BaseDamage { get; set; }
        public int Range { get; set; }
        public int EnergyCost { get; set; }
        public float Accuracy { get; set; }
        public float CriticalRate { get; set; }

        // 武器状态
        public bool IsEnabled { get; set; }

        public void Initialize()
        {
            IsEnabled = true;
        }

        public void Initialize(WeaponTemplate template)
        {
            WeaponId = template.WeaponId;
            WeaponName = template.WeaponName;
            WeaponType = template.WeaponType;

            BaseDamage = template.BaseDamage;
            Range = template.Range;
            EnergyCost = template.EnergyCost;
            Accuracy = template.Accuracy;
            CriticalRate = template.CriticalRate;

            IsEnabled = true;
        }

        public void Cleanup()
        {
            // 无需清理
        }

        // 计算预期伤害
        public int CalculateExpectedDamage(GameEntity attacker, GameEntity target)
        {
            var attackerStats = attacker.GetComponent<UnitStatsComponent>();
            var targetStats = target.GetComponent<UnitStatsComponent>();

            if (attackerStats == null || targetStats == null)
                return 0;

            // 基础伤害
            float damage = BaseDamage;

            // 应用攻击加成
            if (WeaponType == WeaponType.Ranged)
            {
                damage *= (1 + attackerStats.RangedAttackBonus / 100f);
            }
            else if (WeaponType == WeaponType.Melee)
            {
                damage *= (1 + attackerStats.MeleeAttackBonus / 100f);
            }

            // 应用护甲减伤
            float damageReduction = targetStats.BaseArmor / 100f;
            damage *= (1 - damageReduction);

            return Mathf.RoundToInt(damage);
        }
    }

    // 驾驶员组件
    public class PilotComponent : IComponent
    {
        public int PilotId { get; set; }
        public string PilotName { get; set; }
        public float CompatibilityRate { get; set; }

        public void Initialize()
        {
            PilotId = -1;
            PilotName = "无";
            CompatibilityRate = 1.0f;
        }

        public void Cleanup()
        {
            // 无需清理
        }

        // 计算与机体的兼容性
        public void CalculateCompatibility(GameEntity unit, GameEntity pilot)
        {
            var unitStats = unit.GetComponent<UnitStatsComponent>();
            var pilotStats = pilot.GetComponent<PilotStatsComponent>();

            if (unitStats == null || pilotStats == null)
                return;

            // 兼容性计算公式
            CompatibilityRate = 0.6f * pilotStats.Adaptability / 99f +
                            0.3f * pilotStats.GetSpecialtyRate(unitStats.UnitType) +
                            0.1f * pilotStats.NTLevel / 5f;

            // 确保范围在0.5-1.5之间
            CompatibilityRate = Mathf.Clamp(CompatibilityRate, 0.5f, 1.5f);
        }
    }

    // 驾驶员属性组件
    public class PilotStatsComponent : IComponent
    {
        // 基本信息
        public int PilotId { get; set; }
        public string PilotName { get; set; }

        // 基本属性（0-99值范围）
        public int Reaction { get; set; }        // 反应速度
        public int Shooting { get; set; }        // 射击技巧
        public int Melee { get; set; }           // 格斗技巧
        public int Focus { get; set; }           // 专注力
        public int Adaptability { get; set; }    // 适应性

        // 特殊属性
        public int NTLevel { get; set; }         // NT值（0-5）
        public int Leadership { get; set; }      // 领导力
        public int MentalStrength { get; set; }  // 精神强度

        // 专精类型
        public Dictionary<UnitType, float> TypeSpecialties { get; set; }

        // 经验和等级
        public int Level { get; set; }
        public int Experience { get; set; }
        public int ExperienceToNextLevel { get; set; }

        // 专属技能
        public List<SpecialAbility> SpecialAbilities { get; set; }

        // 状态
        public bool IsAvailable { get; set; }
        public int Fatigue { get; set; }  // 疲劳度（影响性能）

        public void Initialize()
        {
            // 初始化专精字典
            TypeSpecialties = new Dictionary<UnitType, float>();
            foreach (UnitType type in Enum.GetValues(typeof(UnitType)))
            {
                TypeSpecialties[type] = 1.0f;  // 默认为标准值
            }

            // 初始化技能列表
            SpecialAbilities = new List<SpecialAbility>();

            // 设置初始值
            Level = 1;
            Experience = 0;
            ExperienceToNextLevel = 100;
            IsAvailable = true;
            Fatigue = 0;
        }

        public void Cleanup()
        {
            SpecialAbilities.Clear();
            TypeSpecialties.Clear();
        }

        /// <summary>
        /// 获取对特定单位类型的专精率
        /// </summary>
        public float GetSpecialtyRate(UnitType unitType)
        {
            if (TypeSpecialties.TryGetValue(unitType, out float rate))
            {
                return rate;
            }
            return 1.0f;  // 默认标准值
        }

        /// <summary>
        /// 增加经验值并检查升级
        /// </summary>
        public bool AddExperience(int amount)
        {
            Experience += amount;

            // 检查是否升级
            if (Experience >= ExperienceToNextLevel)
            {
                LevelUp();
                return true;
            }

            return false;
        }

        /// <summary>
        /// 驾驶员升级
        /// </summary>
        private void LevelUp()
        {
            Level++;
            Experience -= ExperienceToNextLevel;
            ExperienceToNextLevel = 100 * Level;

            // 随机增加属性值
            int pointsToDistribute = 3;
            while (pointsToDistribute > 0)
            {
                int randomAttribute = UnityEngine.Random.Range(0, 5);
                switch (randomAttribute)
                {
                    case 0:
                        if (Reaction < 99) { Reaction++; pointsToDistribute--; }
                        break;
                    case 1:
                        if (Shooting < 99) { Shooting++; pointsToDistribute--; }
                        break;
                    case 2:
                        if (Melee < 99) { Melee++; pointsToDistribute--; }
                        break;
                    case 3:
                        if (Focus < 99) { Focus++; pointsToDistribute--; }
                        break;
                    case 4:
                        if (Adaptability < 99) { Adaptability++; pointsToDistribute--; }
                        break;
                }
            }

            // 特定等级解锁NT能力
            if (Level == 10 && NTLevel == 0 && UnityEngine.Random.value < 0.2f)
            {
                NTLevel = 1;
            }
            else if (Level % 5 == 0 && NTLevel > 0 && NTLevel < 5)
            {
                NTLevel++;
            }
        }

        /// <summary>
        /// 增加疲劳度
        /// </summary>
        public void AddFatigue(int amount)
        {
            Fatigue = Mathf.Min(Fatigue + amount, 100);

            // 疲劳度超过80，驾驶员不可用
            if (Fatigue >= 80)
            {
                IsAvailable = false;
            }
        }

        /// <summary>
        /// 恢复疲劳
        /// </summary>
        public void RecoverFatigue(int amount)
        {
            Fatigue = Mathf.Max(Fatigue - amount, 0);

            // 疲劳度低于80，驾驶员变为可用
            if (Fatigue < 80)
            {
                IsAvailable = true;
            }
        }
    }

    // 基地组件
    public class BaseComponent : IComponent
    {
        // 基础信息
        public int BaseId { get; set; }
        public string BaseName { get; set; }
        public BaseType BaseType { get; set; }

        // 位置信息
        public Vector2Int Position { get; set; }

        // 基地等级和状态
        public int Level { get; set; }
        public int MaxLevel { get; set; } = 5;
        public int Health { get; set; }
        public int MaxHealth { get; set; }
        public bool IsOperational { get; set; }

        // 资源生产
        public int MoneyProduction { get; set; }
        public Dictionary<ResourceType, int> ResourceProduction { get; set; }

        // 功能相关
        public int ResearchBonus { get; set; }          // 研究加成
        public int ProductionEfficiency { get; set; }   // 生产效率
        public int DefenseStrength { get; set; }        // 防御强度

        // 设施
        public List<Facility> Facilities { get; set; }

        // 驻扎人员
        public List<int> StationedResearchers { get; set; }
        public List<int> StationedPilots { get; set; }
        public List<int> StationedUnits { get; set; }

        // 队列
        public List<ProductionItem> ProductionQueue { get; set; }
        public List<string> ResearchQueue { get; set; }

        public void Initialize()
        {
            ResourceProduction = new Dictionary<ResourceType, int>();
            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                ResourceProduction[type] = 0;
            }

            Facilities = new List<Facility>();
            StationedResearchers = new List<int>();
            StationedPilots = new List<int>();
            StationedUnits = new List<int>();

            ProductionQueue = new List<ProductionItem>();
            ResearchQueue = new List<string>();

            Level = 1;
            MaxHealth = 100;
            Health = MaxHealth;
            IsOperational = true;
        }

        public void Cleanup()
        {
            ResourceProduction.Clear();
            Facilities.Clear();
            StationedResearchers.Clear();
            StationedPilots.Clear();
            StationedUnits.Clear();
            ProductionQueue.Clear();
            ResearchQueue.Clear();
        }

        /// <summary>
        /// 基地升级
        /// </summary>
        public bool UpgradeBase()
        {
            if (Level >= MaxLevel)
                return false;

            Level++;

            // 更新最大生命值
            MaxHealth = 100 * Level;
            Health = MaxHealth;

            // 更新资源产量
            MoneyProduction = (int)(MoneyProduction * 1.5f);

            foreach (var resource in ResourceProduction.Keys.ToList())
            {
                ResourceProduction[resource] = (int)(ResourceProduction[resource] * 1.5f);
            }

            // 更新功能加成
            UpdateBonusesBasedOnLevel();

            return true;
        }

        /// <summary>
        /// 根据等级更新加成
        /// </summary>
        private void UpdateBonusesBasedOnLevel()
        {
            switch (BaseType)
            {
                case BaseType.Headquarters:
                    MoneyProduction = 500 * Level;
                    break;
                case BaseType.ResearchFacility:
                    ResearchBonus = 10 * Level;
                    break;
                case BaseType.ProductionPlant:
                    ProductionEfficiency = 15 * Level;
                    break;
                case BaseType.ResourceMine:
                    // 根据等级解锁不同资源产量
                    if (Level >= 2) ResourceProduction[ResourceType.RareMetal] = 10 * (Level - 1);
                    if (Level >= 3) ResourceProduction[ResourceType.EnergyCrystal] = 5 * (Level - 2);
                    if (Level >= 4) ResourceProduction[ResourceType.BeamOre] = 3 * (Level - 3);
                    if (Level >= 5) ResourceProduction[ResourceType.PsychicElement] = 2 * (Level - 4);
                    break;
                case BaseType.DefenseOutpost:
                    DefenseStrength = 20 * Level;
                    break;
            }
        }

        /// <summary>
        /// 添加设施
        /// </summary>
        public bool AddFacility(Facility facility)
        {
            // 检查是否已达到最大设施数量
            if (Facilities.Count >= Level + 2)
                return false;

            // 添加设施
            Facilities.Add(facility);

            // 应用设施效果
            ApplyFacilityEffect(facility, true);

            return true;
        }

        /// <summary>
        /// 移除设施
        /// </summary>
        public bool RemoveFacility(string facilityId)
        {
            var facility = Facilities.Find(f => f.FacilityId == facilityId);
            if (facility == null)
                return false;

            // 移除设施效果
            ApplyFacilityEffect(facility, false);

            // 移除设施
            Facilities.Remove(facility);

            return true;
        }

        /// <summary>
        /// 应用设施效果
        /// </summary>
        private void ApplyFacilityEffect(Facility facility, bool isAdding)
        {
            int sign = isAdding ? 1 : -1;

            switch (facility.FacilityType)
            {
                case FacilityType.ResearchLab:
                    ResearchBonus += sign * facility.EffectValue;
                    break;
                case FacilityType.ProductionLine:
                    ProductionEfficiency += sign * facility.EffectValue;
                    break;
                case FacilityType.DefenseTurret:
                    DefenseStrength += sign * facility.EffectValue;
                    break;
                case FacilityType.ResourceExtractor:
                    foreach (var resourceEffect in facility.ResourceEffects)
                    {
                        ResourceProduction[resourceEffect.Key] += sign * resourceEffect.Value;
                    }
                    break;
                case FacilityType.TrainingCenter:
                    // 训练中心效果由专门的训练系统处理
                    break;
            }
        }

        /// <summary>
        /// 受到攻击，减少生命值
        /// </summary>
        public void TakeDamage(int amount)
        {
            Health -= amount;

            // 检查是否停止运行
            if (Health <= 0)
            {
                Health = 0;
                IsOperational = false;
            }
        }

        /// <summary>
        /// 修复基地
        /// </summary>
        public void Repair(int amount)
        {
            Health = Mathf.Min(Health + amount, MaxHealth);

            // 如果生命值超过25%，基地恢复运行
            if (Health > MaxHealth * 0.25f)
            {
                IsOperational = true;
            }
        }
    }

    // 设施类型
    public enum FacilityType
    {
        ResearchLab,        // 研究实验室
        ProductionLine,     // 生产线
        DefenseTurret,      // 防御炮塔
        ResourceExtractor,  // 资源提取器
        TrainingCenter      // 训练中心
    }

    // 设施定义
    [Serializable]
    public class Facility
    {
        public string FacilityId;
        public string FacilityName;
        public FacilityType FacilityType;
        public string Description;

        // 设施效果值
        public int EffectValue;

        // 资源生产效果（仅ResourceExtractor类型使用）
        public Dictionary<ResourceType, int> ResourceEffects;

        // 建造和维护成本
        public Dictionary<ResourceType, int> BuildCost;
        public int MaintenanceCost;

        // 设施解锁条件
        public string[] RequiredTechnologies;
    }

    // 临时效果组件
    public class TemporaryEffectComponent : IComponent
    {
        public EffectType EffectType { get; set; }
        public int EffectValue { get; set; }
        public int Duration { get; set; }
        public int InitialDuration { get; set; }

        public void Initialize()
        {
            InitialDuration = Duration;
        }

        public void Cleanup()
        {
            // 无需清理
        }

        /// <summary>
        /// 减少持续时间
        /// </summary>
        public bool DecreaseDuration()
        {
            Duration--;
            return Duration <= 0;
        }

        /// <summary>
        /// 获取当前效果强度（基于剩余持续时间）
        /// </summary>
        public int GetCurrentEffectValue()
        {
            // 某些效果可能随时间减弱
            if (EffectType == EffectType.Shield || EffectType == EffectType.Boost)
            {
                return (int)(EffectValue * ((float)Duration / InitialDuration));
            }

            return EffectValue;
        }
    }

    // 生产项目
    [Serializable]
    public class ProductionItem
    {
        public enum ProductionType
        {
            Unit,
            Facility,
            Weapon,
            Upgrade
        }

        public ProductionType Type;
        public string ItemId;
        public string ItemName;
        public int TurnsRemaining;
        public int TotalTurns;
        public Dictionary<ResourceType, int> ResourceCost;
        public int Priority;

        public float GetProgress()
        {
            return 1.0f - ((float)TurnsRemaining / TotalTurns);
        }
    }
}