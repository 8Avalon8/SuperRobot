using UnityEngine;

namespace SuperRobot
{
    /// <summary>
    /// 兼容性组件 - 提供与老UnitStatsComponent相同的接口
    /// 这个组件聚合了多个细粒度组件的功能，用于向后兼容
    /// </summary>
    public class UnitStatsCompatibilityComponent : IComponent
    {
        private GameEntity _entity;
        
        // 缓存组件引用以提高性能
        private UnitIdentityComponent _identity;
        private HealthComponent _health;
        private EnergyComponent _energy;
        private ArmorComponent _armor;
        private ActionPointsComponent _actionPoints;
        private MovementStatsComponent _movementStats;
        private AttackStatsComponent _attackStats;

        public void Initialize()
        {
            // 这个组件需要在其他组件之后初始化
            // 所以在这里获取组件引用
        }

        public void Initialize(GameEntity entity)
        {
            _entity = entity;
            CacheComponentReferences();
        }

        public void Cleanup()
        {
            // 清空缓存
            _identity = null;
            _health = null;
            _energy = null;
            _armor = null;
            _actionPoints = null;
            _movementStats = null;
            _attackStats = null;
        }

        private void CacheComponentReferences()
        {
            _identity = _entity?.GetComponent<UnitIdentityComponent>();
            _health = _entity?.GetComponent<HealthComponent>();
            _energy = _entity?.GetComponent<EnergyComponent>();
            _armor = _entity?.GetComponent<ArmorComponent>();
            _actionPoints = _entity?.GetComponent<ActionPointsComponent>();
            _movementStats = _entity?.GetComponent<MovementStatsComponent>();
            _attackStats = _entity?.GetComponent<AttackStatsComponent>();
        }

        // 向后兼容的属性接口
        public string UnitTemplateId 
        { 
            get => _identity?.UnitTemplateId ?? ""; 
            set { if (_identity != null) _identity.UnitTemplateId = value; } 
        }

        public string UnitName 
        { 
            get => _identity?.UnitName ?? ""; 
            set { if (_identity != null) _identity.UnitName = value; } 
        }

        public UnitType UnitType 
        { 
            get => _identity?.UnitType ?? UnitType.Infantry; 
            set { if (_identity != null) _identity.UnitType = value; } 
        }

        public int MaxHealth 
        { 
            get => _health?.MaxHealth ?? 0; 
            set { if (_health != null) _health.MaxHealth = value; } 
        }

        public int CurrentHealth 
        { 
            get => _health?.CurrentHealth ?? 0; 
            set { if (_health != null) _health.CurrentHealth = value; } 
        }

        public int MaxEnergy 
        { 
            get => _energy?.MaxEnergy ?? 0; 
            set { if (_energy != null) _energy.MaxEnergy = value; } 
        }

        public int CurrentEnergy 
        { 
            get => _energy?.CurrentEnergy ?? 0; 
            set { if (_energy != null) _energy.CurrentEnergy = value; } 
        }

        public int BaseArmor 
        { 
            get => _armor?.BaseArmor ?? 0; 
            set { if (_armor != null) _armor.BaseArmor = value; } 
        }

        public int MovementRange 
        { 
            get => _movementStats?.MovementRange ?? 0; 
            set { if (_movementStats != null) _movementStats.MovementRange = value; } 
        }

        public int MaxActionPoints 
        { 
            get => _actionPoints?.MaxActionPoints ?? 0; 
            set { if (_actionPoints != null) _actionPoints.MaxActionPoints = value; } 
        }

        public int CurrentActionPoints 
        { 
            get => _actionPoints?.CurrentActionPoints ?? 0; 
            set { if (_actionPoints != null) _actionPoints.CurrentActionPoints = value; } 
        }

        public int MovementBonus 
        { 
            get => _movementStats?.MovementBonus ?? 0; 
            set { if (_movementStats != null) _movementStats.MovementBonus = value; } 
        }

        public int RangedAttackBonus 
        { 
            get => _attackStats?.RangedAttackBonus ?? 0; 
            set { if (_attackStats != null) _attackStats.RangedAttackBonus = value; } 
        }

        public int MeleeAttackBonus 
        { 
            get => _attackStats?.MeleeAttackBonus ?? 0; 
            set { if (_attackStats != null) _attackStats.MeleeAttackBonus = value; } 
        }

        public bool RequiresPilot 
        { 
            get => _identity?.RequiresPilot ?? false; 
            set { if (_identity != null) _identity.RequiresPilot = value; } 
        }

        // 向后兼容的方法
        public void ResetActionPoints()
        {
            _actionPoints?.ResetActionPoints();
        }

        public void Repair(int healthAmount, int energyAmount)
        {
            _health?.Heal(healthAmount);
            _energy?.RestoreEnergy(energyAmount);
        }
    }
}