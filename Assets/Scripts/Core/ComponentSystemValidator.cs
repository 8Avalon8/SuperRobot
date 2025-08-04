using UnityEngine;

namespace SuperRobot.Core
{
    /// <summary>
    /// 组件系统验证器 - 用于测试新的组件架构
    /// </summary>
    public static class ComponentSystemValidator
    {
        /// <summary>
        /// 验证实体的组件完整性
        /// </summary>
        public static bool ValidateUnitEntity(GameEntity entity)
        {
            if (entity == null)
            {
                Debug.LogError("Entity is null");
                return false;
            }

            // 检查必需的核心组件
            var requiredComponents = new System.Type[]
            {
                typeof(UnitIdentityComponent),
                typeof(HealthComponent),
                typeof(EnergyComponent),
                typeof(ArmorComponent),
                typeof(ActionPointsComponent),
                typeof(MovementStatsComponent),
                typeof(AttackStatsComponent),
                typeof(PositionComponent),
                typeof(MovementComponent),
                typeof(UnitStatsCompatibilityComponent)
            };

            foreach (var componentType in requiredComponents)
            {
                if (!entity.HasComponent(componentType))
                {
                    Debug.LogError($"Entity {entity.EntityId} is missing required component: {componentType.Name}");
                    return false;
                }
            }

            // 验证组件数据一致性
            return ValidateComponentConsistency(entity);
        }

        private static bool ValidateComponentConsistency(GameEntity entity)
        {
            var identity = entity.GetComponent<UnitIdentityComponent>();
            var health = entity.GetComponent<HealthComponent>();
            var compatibility = entity.GetComponent<UnitStatsCompatibilityComponent>();

            if (identity == null || health == null || compatibility == null)
            {
                Debug.LogError("Failed to get required components for validation");
                return false;
            }

            // 验证兼容性组件是否正确映射数据
            if (compatibility.UnitName != identity.UnitName)
            {
                Debug.LogError($"Unit name mismatch: Identity={identity.UnitName}, Compatibility={compatibility.UnitName}");
                return false;
            }

            if (compatibility.MaxHealth != health.MaxHealth)
            {
                Debug.LogError($"Max health mismatch: Health={health.MaxHealth}, Compatibility={compatibility.MaxHealth}");
                return false;
            }

            Debug.Log($"Entity {entity.EntityId} ({identity.UnitName}) validation passed");
            return true;
        }

        /// <summary>
        /// 测试组件系统的基本功能
        /// </summary>
        public static void RunBasicTest()
        {
            Debug.Log("Starting Component System Basic Test...");

            // 这里可以添加更多的测试逻辑
            // 由于我们无法直接创建测试实体，这个方法主要用于调试时手动调用

            Debug.Log("Component System Basic Test completed");
        }
    }

    /// <summary>
    /// 用于检查组件类型的扩展方法
    /// </summary>
    public static class GameEntityExtensions
    {
        public static bool HasComponent(this GameEntity entity, System.Type componentType)
        {
            return entity.GetComponentTypes().Contains(componentType);
        }

        private static bool Contains(this System.Collections.Generic.IEnumerable<System.Type> types, System.Type targetType)
        {
            foreach (var type in types)
            {
                if (type == targetType)
                    return true;
            }
            return false;
        }
    }
}