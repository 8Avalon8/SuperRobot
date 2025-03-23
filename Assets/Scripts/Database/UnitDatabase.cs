using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SuperRobot
{
    /// <summary>
    /// 单位数据库，存储所有单位模板
    /// </summary>
    [CreateAssetMenu(fileName = "UnitDatabase", menuName = "Game/UnitDatabase")]
    public class UnitDatabase : ScriptableObject
    {
        [SerializeField] private List<UnitTemplate> _unitTemplates = new List<UnitTemplate>();

        public List<UnitTemplate> UnitTemplates => _unitTemplates;

        /// <summary>
        /// 获取特定ID的单位模板
        /// </summary>
        public UnitTemplate GetUnitTemplate(string unitId)
        {
            return _unitTemplates.Find(t => t.UnitId == unitId);
        }

        /// <summary>
        /// 获取特定类型的所有单位模板
        /// </summary>
        public List<UnitTemplate> GetUnitsByType(UnitType type)
        {
            return _unitTemplates.FindAll(t => t.UnitType == type);
        }

        /// <summary>
        /// 获取已解锁的单位模板
        /// </summary>
        public List<UnitTemplate> GetUnlockedUnits(TechTreeSystem techSystem)
        {
            List<UnitTemplate> unlockedUnits = new List<UnitTemplate>();

            foreach (var template in _unitTemplates)
            {
                bool allTechUnlocked = true;

                foreach (var requiredTech in template.RequiredTechnologies)
                {
                    if (!techSystem.IsTechResearched(requiredTech))
                    {
                        allTechUnlocked = false;
                        break;
                    }
                }

                if (allTechUnlocked)
                {
                    unlockedUnits.Add(template);
                }
            }

            return unlockedUnits;
        }

        [Button("初始化默认单位")]
        /// <summary>
        /// 初始化默认单位数据
        /// </summary>
        public void InitializeDefaultUnits()
        {
            _unitTemplates.Clear();

            // ===== 常规军力 =====

            // 坦克单位
            _unitTemplates.Add(new UnitTemplate
            {
                UnitId = "tank_basic",
                UnitName = "M-101 主战坦克",
                UnitType = UnitType.Tank,
                Description = "基础地面作战单位，配备75mm主炮，适合地形防御战",
                ModelPrefabPath = "Prefabs/Units/Tank_Basic",
                RequiresPilot = false,

                // 基础属性
                MaxHealth = 120,
                MaxEnergy = 50,
                MovementRange = 3,
                BaseArmor = 30,

                // 默认武器
                DefaultWeapons = new List<WeaponTemplate>
            {
                // 这里需要武器模板引用，实际使用时需要从WeaponDatabase中获取
                new WeaponTemplate
                {
                    WeaponId = "tank_cannon",
                    WeaponName = "75mm坦克炮",
                    WeaponType = WeaponType.Ranged,
                    BaseDamage = 50,
                    Range = 4,
                    EnergyCost = 5,
                    Accuracy = 85f
                }
            },

                // 开发成本
                DevelopmentCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 800 },
                { ResourceType.StandardAlloy, 40 }
            },

                // 生产成本
                ProductionCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 400 },
                { ResourceType.StandardAlloy, 20 }
            },

                // 维护成本
                MaintenanceCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 50 }
            },

                // 解锁条件
                RequiredTechnologies = new string[] { "basic_ground_tech" },

                // 特殊能力
                SpecialAbilities = new List<SpecialAbility>()
            });

            // 高级坦克
            _unitTemplates.Add(new UnitTemplate
            {
                UnitId = "tank_advanced",
                UnitName = "MBT-200 重型坦克",
                UnitType = UnitType.Tank,
                Description = "重型地面单位，配备120mm光束炮和加强型装甲，适合攻坚战",
                ModelPrefabPath = "Prefabs/Units/Tank_Heavy",
                RequiresPilot = true,

                // 基础属性
                MaxHealth = 200,
                MaxEnergy = 80,
                MovementRange = 2,
                BaseArmor = 50,

                // 默认武器
                DefaultWeapons = new List<WeaponTemplate>
            {
                new WeaponTemplate
                {
                    WeaponId = "heavy_beam_cannon",
                    WeaponName = "120mm光束炮",
                    WeaponType = WeaponType.Beam,
                    BaseDamage = 90,
                    Range = 5,
                    EnergyCost = 15,
                    Accuracy = 80f
                }
            },

                // 开发成本
                DevelopmentCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 2000 },
                { ResourceType.StandardAlloy, 100 },
                { ResourceType.RareMetal, 30 }
            },

                // 生产成本
                ProductionCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 1000 },
                { ResourceType.StandardAlloy, 50 },
                { ResourceType.RareMetal, 15 }
            },

                // 维护成本
                MaintenanceCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 120 }
            },

                // 解锁条件
                RequiredTechnologies = new string[] { "advanced_ground_tech" },

                // 特殊能力
                SpecialAbilities = new List<SpecialAbility>
            {
                new SpecialAbility
                {
                    AbilityId = "terrain_crush",
                    AbilityName = "地形压制",
                    AbilityType = AbilityType.SpecialAttack,
                    Description = "碾压地形障碍并造成范围伤害",
                    EnergyCost = 20,
                    CooldownTurns = 3,
                    EffectValue = 40,
                    Range = 1
                }
            }
            });

            // 飞机单位
            _unitTemplates.Add(new UnitTemplate
            {
                UnitId = "aircraft_basic",
                UnitName = "F-200 战斗机",
                UnitType = UnitType.Aircraft,
                Description = "标准空中单位，高机动性但防御力较弱",
                ModelPrefabPath = "Prefabs/Units/Aircraft_Basic",
                RequiresPilot = true,

                // 基础属性
                MaxHealth = 80,
                MaxEnergy = 100,
                MovementRange = 7,
                BaseArmor = 15,

                // 默认武器
                DefaultWeapons = new List<WeaponTemplate>
            {
                new WeaponTemplate
                {
                    WeaponId = "air_missiles",
                    WeaponName = "空对空导弹",
                    WeaponType = WeaponType.Missile,
                    BaseDamage = 60,
                    Range = 5,
                    EnergyCost = 12,
                    Accuracy = 90f
                }
            },

                // 开发成本
                DevelopmentCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 1200 },
                { ResourceType.StandardAlloy, 60 },
                { ResourceType.RareMetal, 20 }
            },

                // 生产成本
                ProductionCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 600 },
                { ResourceType.StandardAlloy, 30 },
                { ResourceType.RareMetal, 10 }
            },

                // 维护成本
                MaintenanceCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 80 }
            },

                // 解锁条件
                RequiredTechnologies = new string[] { "basic_air_tech" },

                // 特殊能力
                SpecialAbilities = new List<SpecialAbility>
            {
                new SpecialAbility
                {
                    AbilityId = "afterburner",
                    AbilityName = "加力燃烧",
                    AbilityType = AbilityType.StatusBoost,
                    Description = "暂时提升移动范围和闪避率",
                    EnergyCost = 15,
                    CooldownTurns = 2,
                    EffectValue = 30,
                    Duration = 1
                }
            }
            });

            // 舰船单位
            _unitTemplates.Add(new UnitTemplate
            {
                UnitId = "ship_basic",
                UnitName = "DD-500 驱逐舰",
                UnitType = UnitType.Ship,
                Description = "水域作战主力，适合海洋战场",
                ModelPrefabPath = "Prefabs/Units/Ship_Basic",
                RequiresPilot = true,

                // 基础属性
                MaxHealth = 180,
                MaxEnergy = 120,
                MovementRange = 4,
                BaseArmor = 35,

                // 默认武器
                DefaultWeapons = new List<WeaponTemplate>
            {
                new WeaponTemplate
                {
                    WeaponId = "naval_cannon",
                    WeaponName = "舰炮",
                    WeaponType = WeaponType.Ranged,
                    BaseDamage = 75,
                    Range = 6,
                    EnergyCost = 18,
                    Accuracy = 75f
                }
            },

                // 开发成本
                DevelopmentCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 1500 },
                { ResourceType.StandardAlloy, 80 },
                { ResourceType.RareMetal, 25 }
            },

                // 生产成本
                ProductionCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 750 },
                { ResourceType.StandardAlloy, 40 },
                { ResourceType.RareMetal, 12 }
            },

                // 维护成本
                MaintenanceCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 100 }
            },

                // 解锁条件
                RequiredTechnologies = new string[] { "basic_naval_tech" },

                // 特殊能力
                SpecialAbilities = new List<SpecialAbility>
            {
                new SpecialAbility
                {
                    AbilityId = "sonar_sweep",
                    AbilityName = "声纳扫描",
                    AbilityType = AbilityType.AreaAttack,
                    Description = "探测并攻击附近的敌人",
                    EnergyCost = 25,
                    CooldownTurns = 3,
                    EffectValue = 30,
                    Range = 3
                }
            }
            });

            // 量产型机器人
            _unitTemplates.Add(new UnitTemplate
            {
                UnitId = "mass_robot_basic",
                UnitName = "GM-79 量产型机器人",
                UnitType = UnitType.MassProdRobot,
                Description = "标准量产型机器人，平衡的性能和适应性",
                ModelPrefabPath = "Prefabs/Units/MassRobot_Basic",
                RequiresPilot = true,

                // 基础属性
                MaxHealth = 150,
                MaxEnergy = 150,
                MovementRange = 5,
                BaseArmor = 25,

                // 默认武器
                DefaultWeapons = new List<WeaponTemplate>
            {
                new WeaponTemplate
                {
                    WeaponId = "beam_rifle_basic",
                    WeaponName = "基础光束步枪",
                    WeaponType = WeaponType.Ranged,
                    BaseDamage = 65,
                    Range = 4,
                    EnergyCost = 10,
                    Accuracy = 85f
                },
                new WeaponTemplate
                {
                    WeaponId = "beam_saber_basic",
                    WeaponName = "基础光束剑",
                    WeaponType = WeaponType.Melee,
                    BaseDamage = 80,
                    Range = 1,
                    EnergyCost = 5,
                    Accuracy = 95f
                }
            },

                // 开发成本
                DevelopmentCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 2000 },
                { ResourceType.StandardAlloy, 100 },
                { ResourceType.RareMetal, 30 },
                { ResourceType.EnergyCrystal, 10 }
            },

                // 生产成本
                ProductionCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 1000 },
                { ResourceType.StandardAlloy, 50 },
                { ResourceType.RareMetal, 15 },
                { ResourceType.EnergyCrystal, 5 }
            },

                // 维护成本
                MaintenanceCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 150 },
                { ResourceType.EnergyCrystal, 1 }
            },

                // 解锁条件
                RequiredTechnologies = new string[] { "basic_robotics_1" },

                // 特殊能力
                SpecialAbilities = new List<SpecialAbility>()
            });

            // ===== 超级机器人 =====

            // 高达系列
            _unitTemplates.Add(new UnitTemplate
            {
                UnitId = "gundam_basic",
                UnitName = "RX-78-2 高达",
                UnitType = UnitType.Gundam,
                Description = "高性能全能型机器人，出色的平衡性能",
                ModelPrefabPath = "Prefabs/Units/Gundam_Basic",
                RequiresPilot = true,

                // 基础属性
                MaxHealth = 300,
                MaxEnergy = 250,
                MovementRange = 6,
                BaseArmor = 40,

                // 默认武器
                DefaultWeapons = new List<WeaponTemplate>
            {
                new WeaponTemplate
                {
                    WeaponId = "beam_rifle_advanced",
                    WeaponName = "高级光束步枪",
                    WeaponType = WeaponType.Ranged,
                    BaseDamage = 110,
                    Range = 5,
                    EnergyCost = 20,
                    Accuracy = 90f
                },
                new WeaponTemplate
                {
                    WeaponId = "beam_saber_advanced",
                    WeaponName = "高级光束剑",
                    WeaponType = WeaponType.Melee,
                    BaseDamage = 130,
                    Range = 1,
                    EnergyCost = 10,
                    Accuracy = 95f
                }
            },

                // 开发成本
                DevelopmentCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 4000 },
                { ResourceType.StandardAlloy, 200 },
                { ResourceType.RareMetal, 80 },
                { ResourceType.EnergyCrystal, 40 },
                { ResourceType.SteelRankAlloy, 20 }
            },

                // 生产成本
                ProductionCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 2000 },
                { ResourceType.StandardAlloy, 100 },
                { ResourceType.RareMetal, 40 },
                { ResourceType.EnergyCrystal, 20 },
                { ResourceType.SteelRankAlloy, 10 }
            },

                // 维护成本
                MaintenanceCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 300 },
                { ResourceType.EnergyCrystal, 3 }
            },

                // 解锁条件
                RequiredTechnologies = new string[] { "newtype_research_1" },

                // 特殊能力
                SpecialAbilities = new List<SpecialAbility>
            {
                new SpecialAbility
                {
                    AbilityId = "core_fighter",
                    AbilityName = "核心战机",
                    AbilityType = AbilityType.Transform,
                    Description = "分离为战斗机模式，提高机动性但降低防御力",
                    EnergyCost = 30,
                    CooldownTurns = 3,
                    EffectValue = 40,
                    Duration = 2
                }
            }
            });

            // 魔神Z系列
            _unitTemplates.Add(new UnitTemplate
            {
                UnitId = "mazinger_basic",
                UnitName = "魔神Z",
                UnitType = UnitType.SolarPowered,
                Description = "搭载光子能源的超级机器人，高防御力和近战能力",
                ModelPrefabPath = "Prefabs/Units/Mazinger_Basic",
                RequiresPilot = true,

                // 基础属性
                MaxHealth = 350,
                MaxEnergy = 280,
                MovementRange = 4,
                BaseArmor = 60,

                // 默认武器
                DefaultWeapons = new List<WeaponTemplate>
            {
                new WeaponTemplate
                {
                    WeaponId = "rocket_punch",
                    WeaponName = "火箭拳",
                    WeaponType = WeaponType.Special,
                    BaseDamage = 140,
                    Range = 3,
                    EnergyCost = 25,
                    Accuracy = 85f
                },
                new WeaponTemplate
                {
                    WeaponId = "breast_fire",
                    WeaponName = "胸部火焰",
                    WeaponType = WeaponType.Area,
                    BaseDamage = 120,
                    Range = 2,
                    EnergyCost = 35,
                    Accuracy = 80f
                }
            },

                // 开发成本
                DevelopmentCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 4500 },
                { ResourceType.StandardAlloy, 220 },
                { ResourceType.RareMetal, 90 },
                { ResourceType.EnergyCrystal, 50 },
                { ResourceType.SteelRankAlloy, 25 }
            },

                // 生产成本
                ProductionCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 2200 },
                { ResourceType.StandardAlloy, 110 },
                { ResourceType.RareMetal, 45 },
                { ResourceType.EnergyCrystal, 25 },
                { ResourceType.SteelRankAlloy, 12 }
            },

                // 维护成本
                MaintenanceCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 320 },
                { ResourceType.EnergyCrystal, 4 }
            },

                // 解锁条件
                RequiredTechnologies = new string[] { "solar_energy_tech", "super_alloy_tech" },

                // 特殊能力
                SpecialAbilities = new List<SpecialAbility>
            {
                new SpecialAbility
                {
                    AbilityId = "photon_power",
                    AbilityName = "光子力量",
                    AbilityType = AbilityType.StatusBoost,
                    Description = "激活光子能量，大幅提升所有属性",
                    EnergyCost = 50,
                    CooldownTurns = 5,
                    EffectValue = 50,
                    Duration = 2
                }
            }
            });

            // 盖塔系列
            _unitTemplates.Add(new UnitTemplate
            {
                UnitId = "getter_basic",
                UnitName = "真盖塔",
                UnitType = UnitType.Getter,
                Description = "使用盖塔射线的高攻击力超级机器人，三种形态可变",
                ModelPrefabPath = "Prefabs/Units/Getter_Basic",
                RequiresPilot = true,

                // 基础属性
                MaxHealth = 280,
                MaxEnergy = 300,
                MovementRange = 5,
                BaseArmor = 35,

                // 默认武器
                DefaultWeapons = new List<WeaponTemplate>
            {
                new WeaponTemplate
                {
                    WeaponId = "getter_beam",
                    WeaponName = "盖塔射线",
                    WeaponType = WeaponType.Beam,
                    BaseDamage = 160,
                    Range = 4,
                    EnergyCost = 40,
                    Accuracy = 85f
                },
                new WeaponTemplate
                {
                    WeaponId = "getter_tomahawk",
                    WeaponName = "盖塔战斧",
                    WeaponType = WeaponType.Melee,
                    BaseDamage = 150,
                    Range = 1,
                    EnergyCost = 20,
                    Accuracy = 90f
                }
            },

                // 开发成本
                DevelopmentCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 4200 },
                { ResourceType.StandardAlloy, 210 },
                { ResourceType.RareMetal, 85 },
                { ResourceType.EnergyCrystal, 45 },
                { ResourceType.SteelRankAlloy, 22 }
            },

                // 生产成本
                ProductionCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 2100 },
                { ResourceType.StandardAlloy, 105 },
                { ResourceType.RareMetal, 42 },
                { ResourceType.EnergyCrystal, 22 },
                { ResourceType.SteelRankAlloy, 11 }
            },

                // 维护成本
                MaintenanceCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 310 },
                { ResourceType.EnergyCrystal, 3 }
            },

                // 解锁条件
                RequiredTechnologies = new string[] { "getter_rays_tech", "transform_tech" },

                // 特殊能力
                SpecialAbilities = new List<SpecialAbility>
            {
                new SpecialAbility
                {
                    AbilityId = "getter_change",
                    AbilityName = "盖塔变形",
                    AbilityType = AbilityType.Transform,
                    Description = "在三种形态间切换：鹰(远程)、熊(近战)、豹(机动)",
                    EnergyCost = 30,
                    CooldownTurns = 2,
                    EffectValue = 0,
                    Duration = 0
                },
                new SpecialAbility
                {
                    AbilityId = "shine_spark",
                    AbilityName = "闪光火花",
                    AbilityType = AbilityType.AreaAttack,
                    Description = "大范围盖塔能量爆发",
                    EnergyCost = 60,
                    CooldownTurns = 4,
                    EffectValue = 140,
                    Range = 3
                }
            }
            });

            // EVA系列
            _unitTemplates.Add(new UnitTemplate
            {
                UnitId = "eva_basic",
                UnitName = "初号机",
                UnitType = UnitType.BioAdaptive,
                Description = "生物适应型超级机器人，AT力场和高适应性",
                ModelPrefabPath = "Prefabs/Units/Eva_Basic",
                RequiresPilot = true,

                // 基础属性
                MaxHealth = 320,
                MaxEnergy = 270,
                MovementRange = 5,
                BaseArmor = 45,

                // 默认武器
                DefaultWeapons = new List<WeaponTemplate>
            {
                new WeaponTemplate
                {
                    WeaponId = "progressive_knife",
                    WeaponName = "渐进匕首",
                    WeaponType = WeaponType.Melee,
                    BaseDamage = 130,
                    Range = 1,
                    EnergyCost = 15,
                    Accuracy = 95f
                },
                new WeaponTemplate
                {
                    WeaponId = "positron_rifle",
                    WeaponName = "正电子步枪",
                    WeaponType = WeaponType.Beam,
                    BaseDamage = 170,
                    Range = 6,
                    EnergyCost = 45,
                    Accuracy = 75f
                }
            },

                // 开发成本
                DevelopmentCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 5000 },
                { ResourceType.StandardAlloy, 250 },
                { ResourceType.RareMetal, 100 },
                { ResourceType.EnergyCrystal, 50 },
                { ResourceType.SteelRankAlloy, 25 },
                { ResourceType.PsychicElement, 20 }
            },

                // 生产成本
                ProductionCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 2500 },
                { ResourceType.StandardAlloy, 125 },
                { ResourceType.RareMetal, 50 },
                { ResourceType.EnergyCrystal, 25 },
                { ResourceType.SteelRankAlloy, 12 },
                { ResourceType.PsychicElement, 10 }
            },

                // 维护成本
                MaintenanceCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 350 },
                { ResourceType.EnergyCrystal, 3 },
                { ResourceType.PsychicElement, 1 }
            },

                // 解锁条件
                RequiredTechnologies = new string[] { "at_field_tech", "bio_sync_tech" },

                // 特殊能力
                SpecialAbilities = new List<SpecialAbility>
            {
                new SpecialAbility
                {
                    AbilityId = "at_field",
                    AbilityName = "AT力场",
                    AbilityType = AbilityType.Shield,
                    Description = "展开绝对领域，大幅减少伤害",
                    EnergyCost = 40,
                    CooldownTurns = 3,
                    EffectValue = 70,
                    Duration = 2
                },
                new SpecialAbility
                {
                    AbilityId = "berserk",
                    AbilityName = "狂暴化",
                    AbilityType = AbilityType.Berserk,
                    Description = "失控状态，大幅提升攻击力但无法控制",
                    EnergyCost = 0, // 特殊条件触发
                    CooldownTurns = 10,
                    EffectValue = 100,
                    Duration = 3
                }
            }
            });

            // 变形金刚系列
            _unitTemplates.Add(new UnitTemplate
            {
                UnitId = "transformer_basic",
                UnitName = "擎天柱",
                UnitType = UnitType.Transformer,
                Description = "具有高度变形能力的机器人，可在车辆和机器人形态间切换",
                ModelPrefabPath = "Prefabs/Units/Transformer_Basic",
                RequiresPilot = false, // 自主智能

                // 基础属性
                MaxHealth = 290,
                MaxEnergy = 260,
                MovementRange = 6,
                BaseArmor = 40,

                // 默认武器
                DefaultWeapons = new List<WeaponTemplate>
            {
                new WeaponTemplate
                {
                    WeaponId = "ion_blaster",
                    WeaponName = "离子爆破枪",
                    WeaponType = WeaponType.Ranged,
                    BaseDamage = 120,
                    Range = 5,
                    EnergyCost = 25,
                    Accuracy = 85f
                },
                new WeaponTemplate
                {
                    WeaponId = "energon_axe",
                    WeaponName = "能量斧",
                    WeaponType = WeaponType.Melee,
                    BaseDamage = 140,
                    Range = 1,
                    EnergyCost = 20,
                    Accuracy = 90f
                }
            },

                // 开发成本
                DevelopmentCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 4300 },
                { ResourceType.StandardAlloy, 220 },
                { ResourceType.RareMetal, 85 },
                { ResourceType.EnergyCrystal, 45 },
                { ResourceType.SteelRankAlloy, 20 }
            },

                // 生产成本
                ProductionCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 2150 },
                { ResourceType.StandardAlloy, 110 },
                { ResourceType.RareMetal, 42 },
                { ResourceType.EnergyCrystal, 22 },
                { ResourceType.SteelRankAlloy, 10 }
            },

                // 维护成本
                MaintenanceCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 320 },
                { ResourceType.EnergyCrystal, 3 }
            },

                // 解锁条件
                RequiredTechnologies = new string[] { "transform_tech", "ai_integration" },

                // 特殊能力
                SpecialAbilities = new List<SpecialAbility>
            {
                new SpecialAbility
                {
                    AbilityId = "transform",
                    AbilityName = "变形",
                    AbilityType = AbilityType.Transform,
                    Description = "在车辆和机器人形态间切换，提高机动性或战斗力",
                    EnergyCost = 15,
                    CooldownTurns = 1,
                    EffectValue = 30,
                    Duration = 0
                },
                new SpecialAbility
                {
                    AbilityId = "matrix_power",
                    AbilityName = "领导模块",
                    AbilityType = AbilityType.StatusBoost,
                    Description = "激活领导模块能量，提升自身和周围友军能力",
                    EnergyCost = 50,
                    CooldownTurns = 5,
                    EffectValue = 40,
                    Duration = 2,
                    Range = 3
                }
            } 
            });
        }
    }
}
