using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SuperRobot
{

    // 游戏全局配置
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Game/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("基础游戏设置")]
        public int MaxTurns = 100;
        public int InitialFunds = 10000;
        public float DifficultyMultiplier = 1.0f;

        [Header("战斗系统设置")]
        public float BaseCriticalDamageMultiplier = 1.5f;
        public float TerrainDefenseBonus = 0.2f;
        public int DefaultActionPoints = 2;
        public int MaxBattleSize = 12;  // 每方最多单位数

        [Header("研发系统设置")]
        public float BaseResearchRate = 10f;
        public int MaxResearchProjects = 3;
        public float ResearcherEfficiency = 5f;

        [Header("生产系统设置")]
        public float BaseProductionRate = 5f;
        public int MaxProductionQueue = 5;
        public float ProductionEfficiencyMultiplier = 1.0f;

        [Header("资源系统设置")]
        public float ResourceDecayRate = 0.05f;
        public float ResourceScarcityMultiplier = 1.0f;
        public int StorageCapacityBase = 2000;

        [Header("AI系统设置")]
        public float AIAggressionBase = 0.5f;
        public float AITechProgressionRate = 0.8f;
        public float AIResourceMultiplier = 1.2f;

        [Header("事件系统设置")]
        public float RandomEventChance = 0.3f;
        public int EventCooldownTurns = 3;
        public int MaxActiveEvents = 5;

        [Header("平衡设置")]
        public float SuperRobotProductionTimeMultiplier = 2.5f;
        public float SuperRobotMaintenanceCostMultiplier = 3.0f;
        public float ConventionalForceEfficiencyMultiplier = 0.8f;

        // 资源初始分配
        public Dictionary<ResourceType, int> InitialResources = new Dictionary<ResourceType, int>
    {
        { ResourceType.Money, 10000 },
        { ResourceType.ManPower, 100 },
        { ResourceType.StandardAlloy, 500 },
        { ResourceType.RareMetal, 0 },
        { ResourceType.EnergyCrystal, 0 },
        { ResourceType.SteelRankAlloy, 0 },
        { ResourceType.BeamOre, 0 },
        { ResourceType.PsychicElement, 0 }
    };


    }
}
