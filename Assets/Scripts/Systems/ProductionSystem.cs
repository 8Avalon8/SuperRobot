using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SuperRobot
{
    // 生产系统
public class ProductionSystem : GameSystem
{
    // 模板数据
    private Dictionary<string, UnitTemplate> _unitTemplates;
    private Dictionary<string, WeaponTemplate> _weaponTemplates;
    private Dictionary<string, Facility> _facilityTemplates;
    
    public override void Initialize()
    {
        // 加载模板数据
        LoadTemplates();
        
        // 订阅事件
        EventManager.Subscribe<AddToProductionQueueRequestEvent>(OnAddToProductionQueueRequest);
        EventManager.Subscribe<RemoveFromProductionQueueRequestEvent>(OnRemoveFromProductionQueueRequest);
        EventManager.Subscribe<ProductionPriorityChangeRequestEvent>(OnProductionPriorityChangeRequest);
    }
    
    public override void Execute()
    {
        // 主要逻辑在事件处理程序和BaseManagementSystem中
    }
    
    /// <summary>
    /// 加载模板数据
    /// </summary>
    private void LoadTemplates()
    {
        // 从ScriptableObject加载模板数据
        var unitData = Resources.Load<UnitDatabase>("Data/UnitDatabase");
        var weaponData = Resources.Load<WeaponDatabase>("Data/WeaponDatabase");
        var facilityData = Resources.Load<FacilityDatabase>("Data/FacilityDatabase");
        
        // 初始化模板字典
        _unitTemplates = new Dictionary<string, UnitTemplate>();
        foreach (var template in unitData.UnitTemplates)
        {
            _unitTemplates[template.UnitId.ToString()] = template;
        }
        
        _weaponTemplates = new Dictionary<string, WeaponTemplate>();
        foreach (var template in weaponData.WeaponTemplates)
        {
            _weaponTemplates[template.WeaponId] = template;
        }
        
        _facilityTemplates = new Dictionary<string, Facility>();
        foreach (var template in facilityData.FacilityTemplates)
        {
            _facilityTemplates[template.FacilityId] = template;
        }
    }
    
    /// <summary>
    /// 获取单位生产成本和时间
    /// </summary>
    public (Dictionary<ResourceType, int> cost, int turns) GetUnitProductionInfo(string unitId)
    {
        if (!_unitTemplates.TryGetValue(unitId, out var template))
        {
            return (new Dictionary<ResourceType, int>(), 0);
        }
        
        // 超级机器人生产时间更长
        int baseTurns = template.UnitType == UnitType.Gundam ||
                        template.UnitType == UnitType.SolarPowered ||
                        template.UnitType == UnitType.Getter ||
                        template.UnitType == UnitType.BioAdaptive ||
                        template.UnitType == UnitType.Transformer
                        ? 5 : 3;
        
        // 应用游戏配置的修正
        var gameConfig = GameManager.Instance.GameConfig;
        if (baseTurns == 5) // 超级机器人
        {
            baseTurns = (int)(baseTurns * gameConfig.SuperRobotProductionTimeMultiplier);
        }
        
        return (template.ProductionCost, baseTurns);
    }
    
    /// <summary>
    /// 获取设施生产成本和时间
    /// </summary>
    public (Dictionary<ResourceType, int> cost, int turns) GetFacilityProductionInfo(string facilityId)
    {
        if (!_facilityTemplates.TryGetValue(facilityId, out var template))
        {
            return (new Dictionary<ResourceType, int>(), 0);
        }
        
        // 设施建造一般需要2回合
        return (template.BuildCost, 2);
    }
    
    /// <summary>
    /// 获取武器生产成本和时间
    /// </summary>
    public (Dictionary<ResourceType, int> cost, int turns) GetWeaponProductionInfo(string weaponId)
    {
        if (!_weaponTemplates.TryGetValue(weaponId, out var template))
        {
            return (new Dictionary<ResourceType, int>(), 0);
        }
        
        // 武器生产时间根据类型不同
        int turns = template.WeaponType == WeaponType.Special ? 4 : 2;
        
        return (template.ProductionCost, turns);
    }
    
    /// <summary>
    /// 创建生产项目
    /// </summary>
    public ProductionItem CreateProductionItem(ProductionItem.ProductionType type, string itemId)
    {
        ProductionItem item = new ProductionItem
        {
            Type = type,
            ItemId = itemId,
            Priority = 1 // 默认优先级
        };
        
        switch (type)
        {
            case ProductionItem.ProductionType.Unit:
                if (_unitTemplates.TryGetValue(itemId, out var unitTemplate))
                {
                    item.ItemName = unitTemplate.UnitName;
                    (item.ResourceCost, item.TotalTurns) = GetUnitProductionInfo(itemId);
                }
                break;
                
            case ProductionItem.ProductionType.Facility:
                if (_facilityTemplates.TryGetValue(itemId, out var facilityTemplate))
                {
                    item.ItemName = facilityTemplate.FacilityName;
                    (item.ResourceCost, item.TotalTurns) = GetFacilityProductionInfo(itemId);
                }
                break;
                
            case ProductionItem.ProductionType.Weapon:
                if (_weaponTemplates.TryGetValue(itemId, out var weaponTemplate))
                {
                    item.ItemName = weaponTemplate.WeaponName;
                    (item.ResourceCost, item.TotalTurns) = GetWeaponProductionInfo(itemId);
                }
                break;
                
            case ProductionItem.ProductionType.Upgrade:
                // 升级项目需要特殊处理
                item.ItemName = "Upgrade: " + itemId;
                item.ResourceCost = new Dictionary<ResourceType, int>
                {
                    { ResourceType.Money, 1000 },
                    { ResourceType.StandardAlloy, 50 }
                };
                item.TotalTurns = 3;
                break;
        }
        
        item.TurnsRemaining = item.TotalTurns;
        
        return item;
    }
    
    /// <summary>
    /// 事件处理程序
    /// </summary>
    private void OnAddToProductionQueueRequest(AddToProductionQueueRequestEvent evt)
    {
        var item = CreateProductionItem(evt.ProductionType, evt.ItemId);
        
        var baseSystem = SystemManager.GetSystem<BaseManagementSystem>();
        baseSystem.AddProductionItem(evt.BaseId, item);
    }
    
    private void OnRemoveFromProductionQueueRequest(RemoveFromProductionQueueRequestEvent evt)
    {
        // 获取基地实体
        var baseSystem = SystemManager.GetSystem<BaseManagementSystem>();
        var baseEntity = baseSystem.GetBase(evt.BaseId);
        if (baseEntity == null)
            return;
        
        var baseComp = baseEntity.GetComponent<BaseComponent>();
        if (baseComp == null || baseComp.ProductionQueue.Count <= evt.QueueIndex)
            return;
        
        // 如果不是第一个项目（已经开始生产），退还部分资源
        if (evt.QueueIndex == 0 && baseComp.ProductionQueue[0].TurnsRemaining < baseComp.ProductionQueue[0].TotalTurns)
        {
            var item = baseComp.ProductionQueue[0];
            float refundRate = (float)item.TurnsRemaining / item.TotalTurns * 0.8f; // 80%的按比例退款
            
            var resourceSystem = SystemManager.GetSystem<ResourceSystem>();
            foreach (var resource in item.ResourceCost)
            {
                int refundAmount = (int)(resource.Value * refundRate);
                resourceSystem.AddResource(resource.Key, refundAmount);
            }
        }
        // 如果是队列中尚未开始的项目，全额退款
        else if (evt.QueueIndex > 0)
        {
            var item = baseComp.ProductionQueue[evt.QueueIndex];
            
            var resourceSystem = SystemManager.GetSystem<ResourceSystem>();
            foreach (var resource in item.ResourceCost)
            {
                resourceSystem.AddResource(resource.Key, resource.Value);
            }
        }
        
        // 从队列中移除
        baseComp.ProductionQueue.RemoveAt(evt.QueueIndex);
    }
    
    private void OnProductionPriorityChangeRequest(ProductionPriorityChangeRequestEvent evt)
    {
        // 获取基地实体
        var baseSystem = SystemManager.GetSystem<BaseManagementSystem>();
        var baseEntity = baseSystem.GetBase(evt.BaseId);
        if (baseEntity == null)
            return;
        
        var baseComp = baseEntity.GetComponent<BaseComponent>();
        if (baseComp == null || baseComp.ProductionQueue.Count <= evt.QueueIndex)
            return;
        
        // 不能修改当前正在生产的项目优先级
        if (evt.QueueIndex == 0)
            return;
        
        // 修改优先级
        baseComp.ProductionQueue[evt.QueueIndex].Priority = evt.NewPriority;
        
        // 根据优先级重新排序（优先级高的排前面，同优先级按添加顺序）
        baseComp.ProductionQueue = baseComp.ProductionQueue
            .Take(1) // 保持当前生产项目不变
            .Concat(baseComp.ProductionQueue.Skip(1).OrderByDescending(p => p.Priority))
            .ToList();
    }
    
    public override void Cleanup()
    {
        // 取消订阅事件
        EventManager.Instance.Unsubscribe<AddToProductionQueueRequestEvent>(OnAddToProductionQueueRequest);
        EventManager.Instance.Unsubscribe<RemoveFromProductionQueueRequestEvent>(OnRemoveFromProductionQueueRequest);
        EventManager.Instance.Unsubscribe<ProductionPriorityChangeRequestEvent>(OnProductionPriorityChangeRequest);
    }
}
}
