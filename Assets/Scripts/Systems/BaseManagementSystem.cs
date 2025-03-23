using System;
using System.Collections.Generic;
using UnityEngine;

namespace SuperRobot
{
// 基地管理系统
    public class BaseManagementSystem : GameSystem
    {
        private readonly Dictionary<int, GameEntity> _bases      = new Dictionary<int, GameEntity>();
        private          int                         _nextBaseId = 1;
        private IMapManager _mapManager => GameManager.Instance.MapManager;

        public override void Initialize()
        {
            // 订阅事件
            EventManager.Subscribe<BaseConstructionRequestEvent>(OnBaseConstructionRequest);
            EventManager.Subscribe<BaseFacilityRequestEvent>(OnBaseFacilityRequest);
            EventManager.Subscribe<BaseUpgradeRequestEvent>(OnBaseUpgradeRequest);
            EventManager.Subscribe<TurnStartedEvent>(OnTurnStarted);
        }

        public override void Execute()
        {
            // 主要逻辑在事件处理程序中
        }

        /// <summary>
        /// 创建新基地
        /// </summary>
        public GameEntity CreateBase(string baseName, BaseType baseType, Vector2Int position)
        {
            // 创建实体
            var baseEntity = EntityManager.CreateEntity("Base");

            // 添加基地组件
            var baseComp = baseEntity.AddComponent<BaseComponent>();
            baseComp.BaseId   = _nextBaseId++;
            baseComp.BaseName = baseName;
            baseComp.BaseType = baseType;
            baseComp.Position = position;

            // 添加位置组件
            var posComp = baseEntity.AddComponent<PositionComponent>();
            posComp.Position = position;

            // 初始化基地属性
            InitializeBaseByType(baseComp, baseType);

            // 注册基地
            _bases[baseComp.BaseId] = baseEntity;

            //var mapGenerator = GameManager.Instance.SystemManager.GetSystem<MapSystem>();
            var prefab = Resources.Load<GameObject>("Prefabs/Base");
            var worldPosition = _mapManager.GetCellWorldPosition(position);
            var baseObj = GameObject.Instantiate(prefab, worldPosition + new Vector3(0, 0.05f, 0), Quaternion.identity);
            baseObj.transform.localScale = new Vector3(1f, 1f, 1f);


            return baseEntity;
        }

        /// <summary>
        /// 根据类型初始化基地属性
        /// </summary>
        private void InitializeBaseByType(BaseComponent baseComp, BaseType baseType)
        {
            switch (baseType)
            {
                case BaseType.Headquarters:
                    baseComp.MoneyProduction = 500;
                    baseComp.MaxHealth       = 200;
                    baseComp.Health          = baseComp.MaxHealth;
                    break;
                case BaseType.ResearchFacility:
                    baseComp.ResearchBonus = 10;
                    baseComp.MaxHealth     = 150;
                    baseComp.Health        = baseComp.MaxHealth;
                    break;
                case BaseType.ProductionPlant:
                    baseComp.ProductionEfficiency = 15;
                    baseComp.MaxHealth            = 150;
                    baseComp.Health               = baseComp.MaxHealth;
                    break;
                case BaseType.ResourceMine:
                    baseComp.ResourceProduction[ResourceType.StandardAlloy] = 20;
                    baseComp.MaxHealth                                      = 120;
                    baseComp.Health                                         = baseComp.MaxHealth;
                    break;
                case BaseType.DefenseOutpost:
                    baseComp.DefenseStrength = 20;
                    baseComp.MaxHealth       = 180;
                    baseComp.Health          = baseComp.MaxHealth;
                    break;
            }
        }

        /// <summary>
        /// 获取基地
        /// </summary>
        public GameEntity GetBase(int baseId)
        {
            return _bases.GetValueOrDefault(baseId);
        }

        /// <summary>
        /// 获取所有基地
        /// </summary>
        public List<GameEntity> GetAllBases()
        {
            return new List<GameEntity>(_bases.Values);
        }

        /// <summary>
        /// 获取特定类型的基地
        /// </summary>
        public List<GameEntity> GetBasesByType(BaseType baseType)
        {
            List<GameEntity> result = new List<GameEntity>();

            foreach (var baseEntity in _bases.Values)
            {
                var baseComp = baseEntity.GetComponent<BaseComponent>();
                if (baseComp != null && baseComp.BaseType == baseType)
                {
                    result.Add(baseEntity);
                }
            }

            return result;
        }

        /// <summary>
        /// 升级基地
        /// </summary>
        public bool UpgradeBase(int baseId)
        {
            if (!_bases.TryGetValue(baseId, out var baseEntity))
                return false;

            var baseComp = baseEntity.GetComponent<BaseComponent>();
            if (baseComp == null)
                return false;

            // 检查等级
            if (baseComp.Level >= baseComp.MaxLevel)
                return false;

            // 计算升级成本
            Dictionary<ResourceType, int> upgradeCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 1000       * baseComp.Level },
                { ResourceType.StandardAlloy, 50 * baseComp.Level }
            };

            // 高级别升级可能需要更多资源
            if (baseComp.Level >= 3)
            {
                upgradeCost[ResourceType.RareMetal] = 30 * (baseComp.Level - 2);
            }

            if (baseComp.Level >= 4)
            {
                upgradeCost[ResourceType.EnergyCrystal] = 20 * (baseComp.Level - 3);
            }

            // 消耗资源
            var resourceSystem = SystemManager.GetSystem<ResourceSystem>();
            foreach (var resource in upgradeCost)
            {
                if (!resourceSystem.ConsumeResource(resource.Key, resource.Value))
                    return false;
            }

            // 升级基地
            return baseComp.UpgradeBase();
        }

        /// <summary>
        /// 添加设施到基地
        /// </summary>
        public bool AddFacility(int baseId, Facility facility)
        {
            if (!_bases.TryGetValue(baseId, out var baseEntity))
                return false;

            var baseComp = baseEntity.GetComponent<BaseComponent>();
            if (baseComp == null)
                return false;

            // 消耗资源
            var resourceSystem = SystemManager.GetSystem<ResourceSystem>();
            foreach (var resource in facility.BuildCost)
            {
                if (!resourceSystem.ConsumeResource(resource.Key, resource.Value))
                    return false;
            }

            // 添加设施
            return baseComp.AddFacility(facility);
        }

        /// <summary>
        /// 移除设施
        /// </summary>
        public bool RemoveFacility(int baseId, string facilityId)
        {
            if (!_bases.TryGetValue(baseId, out var baseEntity))
                return false;

            var baseComp = baseEntity.GetComponent<BaseComponent>();
            if (baseComp == null)
                return false;

            // 移除设施
            return baseComp.RemoveFacility(facilityId);
        }

        /// <summary>
        /// 添加生产项目到基地
        /// </summary>
        public bool AddProductionItem(int baseId, ProductionItem item)
        {
            if (!_bases.TryGetValue(baseId, out var baseEntity))
                return false;

            var baseComp = baseEntity.GetComponent<BaseComponent>();
            if (baseComp == null || !baseComp.IsOperational)
                return false;

            // 检查基地类型
            if (baseComp.BaseType != BaseType.ProductionPlant && baseComp.BaseType != BaseType.Headquarters)
                return false;

            // 检查队列长度限制
            if (baseComp.ProductionQueue.Count >= 5)
                return false;

            // 消耗资源
            var resourceSystem = SystemManager.GetSystem<ResourceSystem>();
            foreach (var resource in item.ResourceCost)
            {
                if (!resourceSystem.ConsumeResource(resource.Key, resource.Value))
                    return false;
            }

            // 添加到生产队列
            baseComp.ProductionQueue.Add(item);

            return true;
        }

        /// <summary>
        /// 处理基地每回合更新
        /// </summary>
        private void UpdateBases()
        {
            foreach (var baseEntity in _bases.Values)
            {
                var baseComp = baseEntity.GetComponent<BaseComponent>();
                if (baseComp == null || !baseComp.IsOperational)
                    continue;

                // 生产资源
                var resourceSystem = SystemManager.GetSystem<ResourceSystem>();
                resourceSystem.AddResource(ResourceType.Money, baseComp.MoneyProduction);

                foreach (var resource in baseComp.ResourceProduction)
                {
                    if (resource.Value > 0)
                    {
                        resourceSystem.AddResource(resource.Key, resource.Value);
                    }
                }

                // 处理生产队列
                UpdateProductionQueue(baseComp);

                // 修理基地（如果有损伤）
                if (baseComp.Health < baseComp.MaxHealth)
                {
                    baseComp.Repair(baseComp.MaxHealth / 20); // 每回合修复5%
                }
            }
        }

        /// <summary>
        /// 更新基地生产队列
        /// </summary>
        private void UpdateProductionQueue(BaseComponent baseComp)
        {
            if (baseComp.ProductionQueue.Count == 0)
                return;

            // 获取当前生产项目
            var currentItem = baseComp.ProductionQueue[0];

            // 减少剩余回合
            int productionBonus = baseComp.ProductionEfficiency / 10; // 每10点效率减少1回合
            int reduction       = 1 + productionBonus;
            currentItem.TurnsRemaining -= reduction;

            // 检查是否完成
            if (currentItem.TurnsRemaining <= 0)
            {
                // 完成生产
                CompleteProduction(baseComp, currentItem);

                // 从队列移除
                baseComp.ProductionQueue.RemoveAt(0);
            }
        }

        /// <summary>
        /// 完成生产项目
        /// </summary>
        private void CompleteProduction(BaseComponent baseComp, ProductionItem item)
        {
            switch (item.Type)
            {
                case ProductionItem.ProductionType.Unit:
                    // 创建单位
                    var unitSystem = SystemManager.GetSystem<UnitManagementSystem>();
                    var unit       = unitSystem.CreateUnit(item.ItemId, baseComp.Position);

                    // 触发生产完成事件
                    EventManager.TriggerEvent(new UnitProductionCompletedEvent
                    {
                        UnitTemplateId = item.ItemId,
                        BaseId         = baseComp.BaseId,
                        Position       = baseComp.Position
                    });
                    break;

                case ProductionItem.ProductionType.Facility:
                    // 创建临时设施对象
                    Facility facility = new Facility
                    {
                        FacilityId   = item.ItemId,
                        FacilityName = item.ItemName,
                        // 其他设施属性需要从某处获取，这里简化处理
                    };

                    // 添加设施到基地
                    AddFacility(baseComp.BaseId, facility);

                    // 触发设施建造完成事件
                    EventManager.TriggerEvent(new FacilityConstructionCompletedEvent
                    {
                        BaseId     = baseComp.BaseId,
                        FacilityId = item.ItemId
                    });
                    break;

                case ProductionItem.ProductionType.Weapon:
                    // 创建武器装备（由其他系统处理）
                    EventManager.TriggerEvent(new WeaponProductionCompletedEvent
                    {
                        BaseId   = baseComp.BaseId,
                        WeaponId = item.ItemId
                    });
                    break;

                case ProductionItem.ProductionType.Upgrade:
                    // 处理升级项目（由其他系统处理）
                    EventManager.TriggerEvent(new UpgradeCompletedEvent
                    {
                        BaseId    = baseComp.BaseId,
                        UpgradeId = item.ItemId
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// 事件处理程序
        /// </summary>
        private void OnBaseConstructionRequest(BaseConstructionRequestEvent evt)
        {
            // 检查位置是否可用
            foreach (var tBaseEntity in _bases.Values)
            {
                var posComp = tBaseEntity.GetComponent<PositionComponent>();
                if (posComp != null && posComp.Position == evt.Position)
                {
                    // 位置已被占用
                    return;
                }
            }

            // 计算建造成本
            Dictionary<ResourceType, int> constructionCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 2000 },
                { ResourceType.StandardAlloy, 100 }
            };

            // 特殊类型基地可能需要额外资源
            if (evt.BaseType == BaseType.ResearchFacility)
            {
                constructionCost[ResourceType.EnergyCrystal] = 10;
            }
            else if (evt.BaseType == BaseType.ResourceMine)
            {
                constructionCost[ResourceType.RareMetal] = 20;
            }

            // 消耗资源
            var resourceSystem = SystemManager.GetSystem<ResourceSystem>();
            foreach (var resource in constructionCost)
            {
                if (!resourceSystem.ConsumeResource(resource.Key, resource.Value))
                    return;
            }

            // 创建基地
            var baseEntity = CreateBase(evt.BaseName, evt.BaseType, evt.Position);

            // 触发基地建造完成事件
            EventManager.TriggerEvent(new BaseConstructionCompletedEvent
            {
                BaseId   = baseEntity.GetComponent<BaseComponent>().BaseId,
                BaseName = evt.BaseName,
                BaseType = evt.BaseType,
                Position = evt.Position
            });
        }

        private void OnBaseFacilityRequest(BaseFacilityRequestEvent evt)
        {
            if (evt.IsAdding)
            {
                // 创建临时设施对象
                Facility facility = new Facility
                {
                    FacilityId   = evt.FacilityId,
                    FacilityName = evt.FacilityName,
                    FacilityType = evt.FacilityType,
                    // 其他设施属性需要从某处获取，这里简化处理
                    BuildCost = evt.ResourceCost
                };

                AddFacility(evt.BaseId, facility);
            }
            else
            {
                RemoveFacility(evt.BaseId, evt.FacilityId);
            }
        }

        private void OnBaseUpgradeRequest(BaseUpgradeRequestEvent evt)
        {
            UpgradeBase(evt.BaseId);
        }

        private void OnTurnStarted(TurnStartedEvent evt)
        {
            // 在回合开始时更新所有基地
            UpdateBases();
        }

        public override void Cleanup()
        {
            // 取消订阅事件
            EventManager.Instance.Unsubscribe<BaseConstructionRequestEvent>(OnBaseConstructionRequest);
            EventManager.Instance.Unsubscribe<BaseFacilityRequestEvent>(OnBaseFacilityRequest);
            EventManager.Instance.Unsubscribe<BaseUpgradeRequestEvent>(OnBaseUpgradeRequest);
            EventManager.Instance.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
        }
    }
}
