namespace SuperRobot
{
    using System;
using System.Collections.Generic;
    using Sirenix.OdinInspector;
    using UnityEngine;

/// <summary>
/// 技术树节点，表示一项可研究的技术
/// </summary>
[Serializable]
public class TechNode
{
    public string TechId;                   // 技术唯一标识符
    public string TechName;                 // 技术显示名称
    public string Description;              // 技术描述
    public string[] Prerequisites;          // 前置技术要求
    public int ResearchCost;                // 研究所需点数
    
    // 研究所需资源
    public Dictionary<ResourceType, int> ResourceRequirements = new Dictionary<ResourceType, int>();
    
    // 完成研究后获得的奖励
    public List<TechReward> Rewards = new List<TechReward>();
    
    // 技术类别
    public TechCategory Category;
    
    // 技术等级（1-5）
    public int TechLevel = 1;
    
    // 是否已研究完成
    public bool IsResearched;
    
    // 技术图标
    public Sprite Icon;
    
    // 技术在UI树中的位置
    public Vector2 UIPosition;
    
    /// <summary>
    /// 获取研究进度的文本表示
    /// </summary>
    public string GetProgressText(int currentProgress)
    {
        return $"{currentProgress}/{ResearchCost}";
    }
    
    /// <summary>
    /// 获取研究进度的百分比
    /// </summary>
    public float GetProgressPercentage(int currentProgress)
    {
        return (float)currentProgress / ResearchCost;
    }
    
    /// <summary>
    /// 获取技术资源需求的文本描述
    /// </summary>
    public string GetResourceRequirementsText()
    {
        List<string> requirements = new List<string>();
        
        foreach (var resource in ResourceRequirements)
        {
            requirements.Add($"{resource.Key}: {resource.Value}");
        }
        
        return string.Join(", ", requirements);
    }
    
    /// <summary>
    /// 检查是否满足资源需求
    /// </summary>
    public bool CheckResourceRequirements(ResourceSystem resourceSystem)
    {
        foreach (var resource in ResourceRequirements)
        {
            if (!resourceSystem.HasEnoughResource(resource.Key, resource.Value))
            {
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// 消耗研究所需资源
    /// </summary>
    public bool ConsumeResources(ResourceSystem resourceSystem)
    {
        // 先检查资源是否足够
        if (!CheckResourceRequirements(resourceSystem))
            return false;
        
        // 消耗资源
        foreach (var resource in ResourceRequirements)
        {
            resourceSystem.ConsumeResource(resource.Key, resource.Value);
        }
        
        return true;
    }
}

/// <summary>
/// 技术类别枚举
/// </summary>
public enum TechCategory
{
    Energy,         // 能源技术
    Weapons,        // 武器技术
    Defense,        // 防御技术
    Production,     // 生产技术
    Research,       // 研究技术
    Robotics,       // 机器人技术
    Newtypes,       // 新人类技术
    Materials,      // 材料技术
    Special         // 特殊技术
}


}
