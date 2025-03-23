using UnityEngine;
using System;
using System.Collections.Generic;
namespace SuperRobot
{
    public class ResourceSystem : GameSystem
    {
        private Dictionary<ResourceType, int> _resources = new Dictionary<ResourceType, int>();

        public override void Initialize()
        {
            // 初始化资源
            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                _resources[type] = 0;
            }

            // 设置初始资源
            _resources[ResourceType.Money] = 10000;
            _resources[ResourceType.StandardAlloy] = 500;
            _resources[ResourceType.ManPower] = 100;
        }

        public bool ConsumeResource(ResourceType type, int amount)
        {
            if (_resources[type] >= amount)
            {
                _resources[type] -= amount;
                return true;
            }
            return false;
        }

        public void AddResource(ResourceType type, int amount)
        {
            _resources[type] += amount;
        }

        public override void Execute()
        {
            // 每回合资源结算逻辑
            if (TurnManager.IsNewTurn)
            {
                CalculateResourceIncome();
            }
        }

        private void CalculateResourceIncome()
        {
            // 基于控制点和设施计算资源收入
            var bases = EntityManager.GetEntitiesWithComponent<BaseComponent>();

            foreach (var baseEntity in bases)
            {
                var baseComp = baseEntity.GetComponent<BaseComponent>();

                // 基础收入
                AddResource(ResourceType.Money, baseComp.MoneyProduction);

                // 特殊资源收入
                foreach (var resourceProduction in baseComp.ResourceProduction)
                {
                    AddResource(resourceProduction.Key, resourceProduction.Value);
                }
            }
        }

        /// <summary>
        /// 获取资源当前数量
        /// </summary>
        public int GetResource(ResourceType type)
        {
            if (_resources.TryGetValue(type, out var amount))
            {
                return amount;
            }

            return 0;
        }

        /// <summary>
        /// 检查是否有足够的资源
        /// </summary>
        public bool HasEnoughResource(ResourceType type, int amount)
        {
            return GetResource(type) >= amount;
        }
    }
}
