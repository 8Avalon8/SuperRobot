using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SuperRobot
{
    /// <summary>
    /// 技术树数据库
    /// </summary>
    [CreateAssetMenu(fileName = "TechTreeData", menuName = "Game/TechTreeData")]
    public class TechTreeData : ScriptableObject
    {
        [SerializeField] private List<TechNode> _techNodes = new List<TechNode>();

        public List<TechNode> TechNodes => _techNodes;

        /// <summary>
        /// 获取特定ID的技术节点
        /// </summary>
        public TechNode GetTechNode(string techId)
        {
            return _techNodes.Find(n => n.TechId == techId);
        }

        /// <summary>
        /// 获取特定类别的所有技术节点
        /// </summary>
        public List<TechNode> GetTechsByCategory(TechCategory category)
        {
            return _techNodes.FindAll(n => n.Category == category);
        }

        /// <summary>
        /// 获取特定等级的所有技术节点
        /// </summary>
        public List<TechNode> GetTechsByLevel(int level)
        {
            return _techNodes.FindAll(n => n.TechLevel == level);
        }

        [Button("初始化默认技术树")]
        /// <summary>
        /// 初始化默认技术树（示例）
        /// </summary>
        public void InitializeDefaultTechTree()
        {
            _techNodes.Clear();

            // 能源技术线
            _techNodes.Add(new TechNode
            {
                TechId = "basic_energy_1",
                TechName = "基础能源系统",
                Description = "改进基础能源生产技术，提高机甲能量容量",
                Prerequisites = new string[0], // 无前置要求
                ResearchCost = 100,
                ResourceRequirements = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 1000 },
                { ResourceType.StandardAlloy, 50 }
            },
                Rewards = new List<TechReward>
            {
                new TechReward
                {
                    TechId = "basic_energy_1",
                    RewardType = TechRewardType.ResourceBonus,
                    ResourceType = ResourceType.EnergyCrystal,
                    Amount = 20
                }
            },
                Category = TechCategory.Energy,
                TechLevel = 1,
                UIPosition = new Vector2(100, 100)
            });

            _techNodes.Add(new TechNode
            {
                TechId = "advanced_energy_2",
                TechName = "高级能源系统",
                Description = "开发高效能源系统，大幅提高机甲能量输出",
                Prerequisites = new string[] { "basic_energy_1" },
                ResearchCost = 200,
                ResourceRequirements = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 2000 },
                { ResourceType.StandardAlloy, 100 },
                { ResourceType.EnergyCrystal, 30 }
            },
                Rewards = new List<TechReward>
            {
                new TechReward
                {
                    TechId = "advanced_energy_2",
                    RewardType = TechRewardType.UnlockWeapon,
                    TargetId = "beam_rifle_advanced",
                    Amount = 0
                }
            },
                Category = TechCategory.Energy,
                TechLevel = 2,
                UIPosition = new Vector2(200, 100)
            });

            // 武器技术线
            _techNodes.Add(new TechNode
            {
                TechId = "basic_weapons_1",
                TechName = "基础武器系统",
                Description = "开发基础机甲武器技术",
                Prerequisites = new string[0], // 无前置要求
                ResearchCost = 100,
                ResourceRequirements = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 1000 },
                { ResourceType.StandardAlloy, 50 }
            },
                Rewards = new List<TechReward>
            {
                new TechReward
                {
                    TechId = "basic_weapons_1",
                    RewardType = TechRewardType.UnlockWeapon,
                    TargetId = "beam_rifle_basic",
                    Amount = 0
                }
            },
                Category = TechCategory.Weapons,
                TechLevel = 1,
                UIPosition = new Vector2(100, 200)
            });

            // 防御技术线
            _techNodes.Add(new TechNode
            {
                TechId = "basic_defense_1",
                TechName = "基础防御系统",
                Description = "开发基础机甲装甲技术",
                Prerequisites = new string[0], // 无前置要求
                ResearchCost = 100,
                ResourceRequirements = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 1000 },
                { ResourceType.StandardAlloy, 50 }
            },
                Rewards = new List<TechReward>
            {
                new TechReward
                {
                    TechId = "basic_defense_1",
                    RewardType = TechRewardType.ProductionBoost,
                    Amount = 10
                }
            },
                Category = TechCategory.Defense,
                TechLevel = 1,
                UIPosition = new Vector2(100, 300)
            });

            // 机器人技术线
            _techNodes.Add(new TechNode
            {
                TechId = "basic_robotics_1",
                TechName = "基础机器人工程",
                Description = "开发基础机甲设计技术",
                Prerequisites = new string[0], // 无前置要求
                ResearchCost = 100,
                ResourceRequirements = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 1000 },
                { ResourceType.StandardAlloy, 50 }
            },
                Rewards = new List<TechReward>
            {
                new TechReward
                {
                    TechId = "basic_robotics_1",
                    RewardType = TechRewardType.UnlockUnit,
                    TargetId = "mass_robot_basic",
                    Amount = 0
                }
            },
                Category = TechCategory.Robotics,
                TechLevel = 1,
                UIPosition = new Vector2(100, 400)
            });

            // 新人类技术线
            _techNodes.Add(new TechNode
            {
                TechId = "newtype_research_1",
                TechName = "新人类潜能研究",
                Description = "研究人类精神力与机甲的协同系统",
                Prerequisites = new string[] { "basic_robotics_1" },
                ResearchCost = 300,
                ResourceRequirements = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 3000 },
                { ResourceType.EnergyCrystal, 50 },
                { ResourceType.PsychicElement, 10 }
            },
                Rewards = new List<TechReward>
            {
                new TechReward
                {
                    TechId = "newtype_research_1",
                    RewardType = TechRewardType.UnlockUnit,
                    TargetId = "gundam_basic",
                    Amount = 0
                }
            },
                Category = TechCategory.Newtypes,
                TechLevel = 3,
                UIPosition = new Vector2(300, 400)
            });

            // 添加更多技术节点...
        }
    }
}
