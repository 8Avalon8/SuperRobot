using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SuperRobot
{
    public class TechTreeSystem : GameSystem
{
    private Dictionary<string, TechNode> _techTree = new Dictionary<string, TechNode>();
    private List<string> _researchQueue = new List<string>();
    private string _currentResearch;
    private int _researchProgress;
    
    public override void Initialize()
    {
        // 从ScriptableObject加载技术树定义
        var techTreeData = Resources.Load<TechTreeData>("Data/TechTreeData");
        foreach (var techNode in techTreeData.TechNodes)
        {
            _techTree[techNode.TechId] = techNode;
        }
    }
    
    public bool CanResearch(string techId)
    {
        if (!_techTree.TryGetValue(techId, out var tech))
            return false;
            
        if (tech.IsResearched)
            return false;
            
        // 检查前置条件
        foreach (var prereq in tech.Prerequisites)
        {
            if (!_techTree[prereq].IsResearched)
                return false;
        }
        
        // 检查资源需求
        var resourceSystem = SystemManager.GetSystem<ResourceSystem>();
        foreach (var resource in tech.ResourceRequirements)
        {
            if (!resourceSystem.HasEnoughResource(resource.Key, resource.Value))
                return false;
        }
        
        return true;
    }
    
    public void StartResearch(string techId)
    {
        if (!CanResearch(techId))
            return;
            
        // 消耗资源
        var tech = _techTree[techId];
        var resourceSystem = SystemManager.GetSystem<ResourceSystem>();
        foreach (var resource in tech.ResourceRequirements)
        {
            resourceSystem.ConsumeResource(resource.Key, resource.Value);
        }
        
        _currentResearch = techId;
        _researchProgress = 0;
    }
    
    public override void Execute()
    {
        if (string.IsNullOrEmpty(_currentResearch))
            return;
            
        if (TurnManager.IsNewTurn)
        {
            AdvanceResearch();
        }
    }
    
    private void AdvanceResearch()
    {
        var tech = _techTree[_currentResearch];
        
        // 计算研究进度（考虑研究人员、设施等因素）
        int researchPoints = CalculateResearchPoints();
        _researchProgress += researchPoints;
        
        // 检查是否完成
        if (_researchProgress >= tech.ResearchCost)
        {
            CompleteResearch(_currentResearch);
            
            // 处理下一个研究项目
            if (_researchQueue.Count > 0)
            {
                _currentResearch = _researchQueue[0];
                _researchQueue.RemoveAt(0);
                _researchProgress = 0;
            }
            else
            {
                _currentResearch = null;
            }
        }
    }
    
    private void CompleteResearch(string techId)
    {
        var tech = _techTree[techId];
        tech.IsResearched = true;
        
        // 应用研究奖励
        foreach (var reward in tech.Rewards)
        {
            ApplyReward(reward);
        }
        
        // 触发研究完成事件
        EventManager.TriggerEvent(new TechResearchCompletedEvent { TechId = techId });
    }

    /// <summary>
/// 检查技术是否已研发
/// </summary>
public bool IsTechResearched(string techId)
{
    if (_techTree.TryGetValue(techId, out var tech))
    {
        return tech.IsResearched;
    }
    
    return false;
}

/// <summary>
/// 直接解锁技术（通过事件或特殊条件）
/// </summary>
public void UnlockTechnology(string techId)
{
    if (_techTree.TryGetValue(techId, out var tech) && !tech.IsResearched)
    {
        tech.IsResearched = true;
        
        // 触发技术完成事件
        EventManager.TriggerEvent(new TechResearchCompletedEvent 
        { 
            TechId = techId,
            TechName = tech.TechId // 或者存储额外的显示名称
        });
    }
}

/// <summary>
/// 加速研究进度
/// </summary>
public void BoostResearch(string techId, int boostAmount)
{
    if (string.IsNullOrEmpty(_currentResearch))
        return;
        
    if (_currentResearch == techId)
    {
        _researchProgress += boostAmount;
        
        // 检查是否已完成
        if (_techTree.TryGetValue(techId, out var tech) && 
            _researchProgress >= tech.ResearchCost)
        {
            CompleteResearch(techId);
            
            // 处理下一个研究项目
            if (_researchQueue.Count > 0)
            {
                _currentResearch = _researchQueue[0];
                _researchQueue.RemoveAt(0);
                _researchProgress = 0;
            }
            else
            {
                _currentResearch = null;
            }
        }
    }
    else if (_researchQueue.Contains(techId))
    {
        // 如果技术在队列中但不是当前研究项，从队列中移出并立即开始研究
        _researchQueue.Remove(techId);
        
        // 存储当前研究进度
        string prevResearch = _currentResearch;
        int prevProgress = _researchProgress;
        
        // 开始新研究
        _currentResearch = techId;
        _researchProgress = boostAmount;
        
        // 检查是否已完成
        if (_techTree.TryGetValue(techId, out var tech) && 
            _researchProgress >= tech.ResearchCost)
        {
            CompleteResearch(techId);
            
            // 恢复原来的研究
            _currentResearch = prevResearch;
            _researchProgress = prevProgress;
        }
        else
        {
            // 未完成，将原来的研究添加到队列前面
            if (!string.IsNullOrEmpty(prevResearch))
            {
                _researchQueue.Insert(0, prevResearch);
            }
        }
    }
}

/// <summary>
/// 获取技术等级
/// </summary>
public int GetTechLevel(string techId)
{
    if (_techTree.TryGetValue(techId, out var tech))
    {
        // 可以根据技术ID判断等级，例如从ID中提取数字
        // 简单示例：假设技术ID格式为"tech_name_X"，X为数字表示等级
        string[] parts = techId.Split('_');
        if (parts.Length > 0)
        {
            string lastPart = parts[parts.Length - 1];
            if (int.TryParse(lastPart, out int level))
            {
                return level;
            }
        }
        
        // 如果无法从ID确定等级，尝试从依赖关系确定
        // 计算前置依赖数量作为近似等级
        return tech.Prerequisites?.Length ?? 1;
    }
    
    return 1; // 默认等级
}

/// <summary>
/// 获取当前可研究的技术
/// </summary>
public List<string> GetResearchableTechs()
{
    List<string> researchable = new List<string>();
    
    foreach (var techId in _techTree.Keys)
    {
        if (CanResearch(techId))
        {
            researchable.Add(techId);
        }
    }
    
    return researchable;
}

/// <summary>
/// 获取当前研究的技术
/// </summary>
public TechNode GetCurrentResearch()
{
    if (string.IsNullOrEmpty(_currentResearch))
        return null;
        
    return _techTree[_currentResearch];
}

/// <summary>
/// 获取研究进度
/// </summary>
public int GetResearchProgress()
{
    return _researchProgress;
}

/// <summary>
/// 获取研究所需点数
/// </summary>
public int GetResearchCost(string techId)
{
    if (_techTree.TryGetValue(techId, out var tech))
    {
        return tech.ResearchCost;
    }
    
    return 0;
}

// GetAllTechIds
public List<string> GetAllTechIds()
{
    return _techTree.Keys.ToList();
}

/// <summary>
/// 计算研究点数
/// </summary>
private int CalculateResearchPoints()
{
    // 基础研究点数
    int basePoints = 10;
    
    // 研究设施加成
    var baseSystem = SystemManager.GetSystem<BaseManagementSystem>();
    var researchBases = baseSystem.GetBasesByType(BaseType.ResearchFacility);
    
    int facilityBonus = 0;
    foreach (var baseEntity in researchBases)
    {
        var baseComp = baseEntity.GetComponent<BaseComponent>();
        if (baseComp != null && baseComp.IsOperational)
        {
            facilityBonus += baseComp.ResearchBonus;
        }
    }
    
    // 研究人员加成
    var pilotSystem = SystemManager.GetSystem<PilotManagementSystem>();
    int researcherBonus = 0;
    
    // 考虑总部加成
    var headquarters = baseSystem.GetBasesByType(BaseType.Headquarters);
    foreach (var hq in headquarters)
    {
        var baseComp = hq.GetComponent<BaseComponent>();
        if (baseComp != null && baseComp.IsOperational)
        {
            facilityBonus += baseComp.ResearchBonus / 2; // 总部研究效率为一半
        }
    }
    
    // 游戏配置修正
    var gameConfig = GameManager.Instance.GameConfig;
    float configMultiplier = gameConfig.BaseResearchRate / 10f;
    
    return Mathf.RoundToInt((basePoints + facilityBonus + researcherBonus) * configMultiplier);
}

/// <summary>
/// 应用研究奖励
/// </summary>
private void ApplyReward(TechReward reward)
{
    switch (reward.RewardType)
    {
        case TechRewardType.ResourceBonus:
            // 增加资源
            var resourceSystem = SystemManager.GetSystem<ResourceSystem>();
            resourceSystem.AddResource(reward.ResourceType, reward.Amount);
            break;
            
        case TechRewardType.UnlockUnit:
            // 解锁单位
            // 在UnitDatabase中标记单位为可用
            break;
            
        case TechRewardType.UnlockWeapon:
            // 解锁武器
            // 在WeaponDatabase中标记武器为可用
            break;
            
        case TechRewardType.ProductionBoost:
            // 提升生产效率
            // 更新GameConfig中的生产效率
            GameManager.Instance.GameConfig.ProductionEfficiencyMultiplier += reward.Amount / 100f;
            break;
    }
    
    // 触发奖励应用事件
    EventManager.TriggerEvent(new TechRewardAppliedEvent
    {
        TechId = reward.TechId,
        RewardType = reward.RewardType
    });
}
}
}
