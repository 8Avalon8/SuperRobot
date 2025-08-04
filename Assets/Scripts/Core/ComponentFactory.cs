using UnityEngine;

namespace SuperRobot.Core
{
    /// <summary>
    /// 组件工厂 - 简化组件创建和配置
    /// </summary>
    public static class ComponentFactory
    {
        /// <summary>
        /// 为单位创建基础组件集合
        /// </summary>
        public static void CreateUnitComponents(GameEntity entity, UnitTemplate template)
        {
            // 身份组件
            var identity = entity.AddComponent<UnitIdentityComponent>();
            identity.Initialize(template);

            // 健康组件
            var health = entity.AddComponent<HealthComponent>();
            health.Initialize(template.MaxHealth);

            // 能量组件
            var energy = entity.AddComponent<EnergyComponent>();
            energy.Initialize(template.MaxEnergy);

            // 装甲组件
            var armor = entity.AddComponent<ArmorComponent>();
            armor.Initialize(template.BaseArmor);

            // 行动点组件
            var actionPoints = entity.AddComponent<ActionPointsComponent>();
            actionPoints.Initialize(2); // 默认2个行动点

            // 移动统计组件
            var movementStats = entity.AddComponent<MovementStatsComponent>();
            movementStats.Initialize(template.MovementRange);

            // 攻击统计组件
            entity.AddComponent<AttackStatsComponent>();

            // 位置组件
            entity.AddComponent<PositionComponent>();

            // 根据单位类型添加移动组件
            CreateMovementComponent(entity, template.UnitType, template.MovementRange);

            // 如果需要驾驶员，添加驾驶员组件
            if (template.RequiresPilot)
            {
                entity.AddComponent<PilotComponent>();
            }

            // 如果是超级机器人，添加特殊组件
            if (IsSupeRobot(template.UnitType))
            {
                entity.AddComponent<SuperRobotComponent>();
            }

            // 添加武器组件
            CreateWeaponComponents(entity, template);

            // 添加向后兼容组件
            var compatibility = entity.AddComponent<UnitStatsCompatibilityComponent>();
            compatibility.Initialize(entity);
        }

        private static void CreateMovementComponent(GameEntity entity, UnitType unitType, int movementRange)
        {
            var movementType = GetMovementType(unitType);
            var movement = entity.AddComponent<MovementComponent>();
            movement.Initialize(movementType, movementRange);

            // 添加移动历史组件
            entity.AddComponent<MovementHistoryComponent>();
        }

        private static MovementType GetMovementType(UnitType unitType)
        {
            return unitType switch
            {
                UnitType.Aircraft => MovementType.Air,
                UnitType.Naval => MovementType.Amphibious,
                UnitType.Tank => MovementType.Ground,
                UnitType.Infantry => MovementType.Ground,
                UnitType.Gundam => MovementType.Ground, // 大部分高达是地面单位
                UnitType.Getter => MovementType.Air,    // 盖塔机器人通常能飞
                UnitType.BioAdaptive => MovementType.Ground,
                UnitType.Transformer => MovementType.Amphibious, // 变形金刚适应性强
                _ => MovementType.Ground
            };
        }

        private static bool IsSupeRobot(UnitType unitType)
        {
            return unitType switch
            {
                UnitType.Gundam => true,
                UnitType.Getter => true,
                UnitType.BioAdaptive => true,
                UnitType.Transformer => true,
                _ => false
            };
        }

        private static void CreateWeaponComponents(GameEntity entity, UnitTemplate template)
        {
            if (template.DefaultWeapons != null)
            {
                foreach (var weaponTemplate in template.DefaultWeapons)
                {
                    var weapon = entity.AddComponent<WeaponComponent>();
                    weapon.Initialize(weaponTemplate);
                }
            }
        }

        /// <summary>
        /// 为基地创建基础组件集合
        /// </summary>
        public static void CreateBaseComponents(GameEntity entity, string baseName, BaseType baseType, Vector2Int position)
        {
            // 基地组件
            var baseComp = entity.AddComponent<BaseComponent>();
            baseComp.BaseName = baseName;
            baseComp.BaseType = baseType;
            baseComp.Position = position;
            baseComp.Initialize();

            // 位置组件
            var positionComp = entity.AddComponent<PositionComponent>();
            positionComp.SetPosition(position);

            // 基地有自己的健康系统
            var health = entity.AddComponent<HealthComponent>();
            health.Initialize(baseComp.MaxHealth);
        }

        /// <summary>
        /// 为驾驶员创建基础组件集合
        /// </summary>
        public static void CreatePilotComponents(GameEntity entity, string pilotName)
        {
            var pilotStats = entity.AddComponent<PilotStatsComponent>();
            pilotStats.PilotName = pilotName;
            pilotStats.Initialize();
        }
    }
}