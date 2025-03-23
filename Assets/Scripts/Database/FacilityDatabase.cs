using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SuperRobot
{
    /// <summary>
    /// 设施数据库，存储所有设施模板
    /// </summary>
    [CreateAssetMenu(fileName = "FacilityDatabase", menuName = "Game/FacilityDatabase")]
    public class FacilityDatabase : ScriptableObject
    {
        [SerializeField] private List<Facility> _facilityTemplates = new List<Facility>();

        public List<Facility> FacilityTemplates => _facilityTemplates;

        /// <summary>
        /// 获取特定ID的设施
        /// </summary>
        public Facility GetFacility(string facilityId)
        {
            return _facilityTemplates.Find(f => f.FacilityId == facilityId);
        }

        /// <summary>
        /// 获取特定类型的所有设施
        /// </summary>
        public List<Facility> GetFacilitiesByType(FacilityType type)
        {
            return _facilityTemplates.FindAll(f => f.FacilityType == type);
        }

        /// <summary>
        /// 获取适用于特定基地类型的设施
        /// </summary>
        public List<Facility> GetFacilitiesForBaseType(BaseType baseType)
        {
            // 根据基地类型筛选适合的设施
            switch (baseType)
            {
                case BaseType.Headquarters:
                    // 总部可以建造所有类型的设施
                    return new List<Facility>(_facilityTemplates);

                case BaseType.ResearchFacility:
                    // 研究基地重点是研究相关设施
                    return _facilityTemplates.FindAll(f =>
                        f.FacilityType == FacilityType.ResearchLab ||
                        f.FacilityType == FacilityType.TrainingCenter);

                case BaseType.ProductionPlant:
                    // 生产工厂重点是生产相关设施
                    return _facilityTemplates.FindAll(f =>
                        f.FacilityType == FacilityType.ProductionLine ||
                        f.FacilityType == FacilityType.DefenseTurret);

                case BaseType.ResourceMine:
                    // 资源矿场重点是资源相关设施
                    return _facilityTemplates.FindAll(f =>
                        f.FacilityType == FacilityType.ResourceExtractor);

                case BaseType.DefenseOutpost:
                    // 防御哨站重点是防御相关设施
                    return _facilityTemplates.FindAll(f =>
                        f.FacilityType == FacilityType.DefenseTurret);

                default:
                    return new List<Facility>();
            }
        }

        /// <summary>
        /// 获取已解锁的设施模板
        /// </summary>
        public List<Facility> GetUnlockedFacilities(TechTreeSystem techSystem)
        {
            List<Facility> unlockedFacilities = new List<Facility>();

            foreach (var facility in _facilityTemplates)
            {
                bool allTechUnlocked = true;

                foreach (var requiredTech in facility.RequiredTechnologies)
                {
                    if (!techSystem.IsTechResearched(requiredTech))
                    {
                        allTechUnlocked = false;
                        break;
                    }
                }

                if (allTechUnlocked)
                {
                    unlockedFacilities.Add(facility);
                }
            }

            return unlockedFacilities;
        }

        [Button("初始化默认设施")]
        // 示例设施数据初始化方法（仅用于编辑器）
        public void InitializeDefaultFacilities()
        {
            _facilityTemplates.Clear();

            // 研究实验室设施
            _facilityTemplates.Add(new Facility
            {
                FacilityId = "basic_research_lab",
                FacilityName = "基础研究实验室",
                FacilityType = FacilityType.ResearchLab,
                Description = "提高基地研究效率，加快技术研发速度",
                EffectValue = 10, // 研究速度+10%
                ResourceEffects = new Dictionary<ResourceType, int>(),
                BuildCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 1000 },
                { ResourceType.StandardAlloy, 50 }
            },
                MaintenanceCost = 100,
                RequiredTechnologies = new string[] { "basic_research" }
            });

            _facilityTemplates.Add(new Facility
            {
                FacilityId = "advanced_research_lab",
                FacilityName = "高级研究实验室",
                FacilityType = FacilityType.ResearchLab,
                Description = "大幅提高基地研究效率，显著加快技术研发速度",
                EffectValue = 25, // 研究速度+25%
                ResourceEffects = new Dictionary<ResourceType, int>(),
                BuildCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 2500 },
                { ResourceType.StandardAlloy, 100 },
                { ResourceType.EnergyCrystal, 15 }
            },
                MaintenanceCost = 250,
                RequiredTechnologies = new string[] { "advanced_research" }
            });

            // 生产线设施
            _facilityTemplates.Add(new Facility
            {
                FacilityId = "basic_production_line",
                FacilityName = "基础生产线",
                FacilityType = FacilityType.ProductionLine,
                Description = "提高基地生产效率，减少单位生产时间",
                EffectValue = 15, // 生产速度+15%
                ResourceEffects = new Dictionary<ResourceType, int>(),
                BuildCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 1200 },
                { ResourceType.StandardAlloy, 60 }
            },
                MaintenanceCost = 120,
                RequiredTechnologies = new string[] { "basic_production" }
            });

            _facilityTemplates.Add(new Facility
            {
                FacilityId = "advanced_production_line",
                FacilityName = "高级生产线",
                FacilityType = FacilityType.ProductionLine,
                Description = "大幅提高基地生产效率，显著减少单位生产时间",
                EffectValue = 30, // 生产速度+30%
                ResourceEffects = new Dictionary<ResourceType, int>(),
                BuildCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 3000 },
                { ResourceType.StandardAlloy, 120 },
                { ResourceType.RareMetal, 25 }
            },
                MaintenanceCost = 300,
                RequiredTechnologies = new string[] { "advanced_production" }
            });

            // 防御炮塔设施
            _facilityTemplates.Add(new Facility
            {
                FacilityId = "basic_defense_turret",
                FacilityName = "基础防御炮塔",
                FacilityType = FacilityType.DefenseTurret,
                Description = "为基地提供基础防御能力，抵御小规模攻击",
                EffectValue = 15, // 防御力+15
                ResourceEffects = new Dictionary<ResourceType, int>(),
                BuildCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 800 },
                { ResourceType.StandardAlloy, 40 }
            },
                MaintenanceCost = 80,
                RequiredTechnologies = new string[] { "basic_defense" }
            });

            _facilityTemplates.Add(new Facility
            {
                FacilityId = "energy_shield_generator",
                FacilityName = "能量护盾发生器",
                FacilityType = FacilityType.DefenseTurret,
                Description = "为基地提供能量护盾保护，大幅提高防御能力",
                EffectValue = 35, // 防御力+35
                ResourceEffects = new Dictionary<ResourceType, int>(),
                BuildCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 2500 },
                { ResourceType.StandardAlloy, 100 },
                { ResourceType.EnergyCrystal, 30 }
            },
                MaintenanceCost = 250,
                RequiredTechnologies = new string[] { "energy_shield_tech" }
            });

            // 资源提取器设施
            _facilityTemplates.Add(new Facility
            {
                FacilityId = "standard_alloy_extractor",
                FacilityName = "标准合金提取器",
                FacilityType = FacilityType.ResourceExtractor,
                Description = "提高基地标准合金产出",
                EffectValue = 0,
                ResourceEffects = new Dictionary<ResourceType, int>
            {
                { ResourceType.StandardAlloy, 15 }
            },
                BuildCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 1000 },
                { ResourceType.StandardAlloy, 50 }
            },
                MaintenanceCost = 100,
                RequiredTechnologies = new string[] { "resource_extraction_1" }
            });

            _facilityTemplates.Add(new Facility
            {
                FacilityId = "rare_metal_extractor",
                FacilityName = "稀有金属提取器",
                FacilityType = FacilityType.ResourceExtractor,
                Description = "提供稀有金属资源产出",
                EffectValue = 0,
                ResourceEffects = new Dictionary<ResourceType, int>
            {
                { ResourceType.RareMetal, 10 }
            },
                BuildCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 1500 },
                { ResourceType.StandardAlloy, 75 },
                { ResourceType.RareMetal, 15 }
            },
                MaintenanceCost = 150,
                RequiredTechnologies = new string[] { "resource_extraction_2" }
            });

            _facilityTemplates.Add(new Facility
            {
                FacilityId = "energy_crystal_refinery",
                FacilityName = "能量晶体精炼厂",
                FacilityType = FacilityType.ResourceExtractor,
                Description = "提供能量晶体资源产出",
                EffectValue = 0,
                ResourceEffects = new Dictionary<ResourceType, int>
            {
                { ResourceType.EnergyCrystal, 8 }
            },
                BuildCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 2000 },
                { ResourceType.StandardAlloy, 100 },
                { ResourceType.EnergyCrystal, 10 }
            },
                MaintenanceCost = 200,
                RequiredTechnologies = new string[] { "energy_crystal_tech" }
            });

            // 训练中心设施
            _facilityTemplates.Add(new Facility
            {
                FacilityId = "pilot_training_center",
                FacilityName = "驾驶员训练中心",
                FacilityType = FacilityType.TrainingCenter,
                Description = "提高驾驶员训练效率，加快经验获取速度",
                EffectValue = 20, // 训练效率+20%
                ResourceEffects = new Dictionary<ResourceType, int>(),
                BuildCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 1200 },
                { ResourceType.StandardAlloy, 60 }
            },
                MaintenanceCost = 120,
                RequiredTechnologies = new string[] { "pilot_training" }
            });

            _facilityTemplates.Add(new Facility
            {
                FacilityId = "newtype_lab",
                FacilityName = "新人类研究实验室",
                FacilityType = FacilityType.TrainingCenter,
                Description = "研究和培养NT能力，提高驾驶员潜力上限",
                EffectValue = 30, // NT潜力+30%
                ResourceEffects = new Dictionary<ResourceType, int>(),
                BuildCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 3000 },
                { ResourceType.StandardAlloy, 100 },
                { ResourceType.EnergyCrystal, 25 },
                { ResourceType.PsychicElement, 10 }
            },
                MaintenanceCost = 300,
                RequiredTechnologies = new string[] { "newtype_research" }
            });
        }
    }
}
