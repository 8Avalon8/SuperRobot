using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SuperRobot
{
   /// <summary>
    /// 武器数据库，存储所有武器模板
    /// </summary>
    [CreateAssetMenu(fileName = "WeaponDatabase", menuName = "Game/WeaponDatabase")]
    public class WeaponDatabase : ScriptableObject
    {
        [SerializeField] private List<WeaponTemplate> _weaponTemplates = new List<WeaponTemplate>();

        public List<WeaponTemplate> WeaponTemplates => _weaponTemplates;

        /// <summary>
        /// 获取特定ID的武器模板
        /// </summary>
        public WeaponTemplate GetWeaponTemplate(string weaponId)
        {
            return _weaponTemplates.Find(t => t.WeaponId == weaponId);
        }

        /// <summary>
        /// 获取特定类型的所有武器模板
        /// </summary>
        public List<WeaponTemplate> GetWeaponsByType(WeaponType type)
        {
            return _weaponTemplates.FindAll(t => t.WeaponType == type);
        }

        /// <summary>
        /// 获取适用于特定单位类型的武器模板
        /// </summary>
        public List<WeaponTemplate> GetWeaponsForUnitType(UnitType unitType)
        {
            return _weaponTemplates.FindAll(t => t.CompatibleUnitTypes.Contains(unitType));
        }

        /// <summary>
        /// 查找可用于升级的武器（比指定武器更高级）
        /// </summary>
        public List<WeaponTemplate> GetUpgradeOptions(string currentWeaponId)
        {
            WeaponTemplate current = GetWeaponTemplate(currentWeaponId);
            if (current == null)
                return new List<WeaponTemplate>();

            return _weaponTemplates.FindAll(t =>
                t.WeaponType == current.WeaponType &&
                t.BaseDamage > current.BaseDamage &&
                !string.Equals(t.WeaponId, currentWeaponId));
        }

        /// <summary>
        /// 获取已解锁的武器模板
        /// </summary>
        public List<WeaponTemplate> GetUnlockedWeapons(TechTreeSystem techSystem)
        {
            List<WeaponTemplate> unlockedWeapons = new List<WeaponTemplate>();

            foreach (var template in _weaponTemplates)
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
                    unlockedWeapons.Add(template);
                }
            }

            return unlockedWeapons;
        }

        [Button("初始化默认武器")]
        // 示例武器数据初始化方法（仅用于编辑器）
        public void InitializeDefaultWeapons()
        {
            _weaponTemplates.Clear();

            // 近战武器
            _weaponTemplates.Add(new WeaponTemplate
            {
                WeaponId = "beam_saber_basic",
                WeaponName = "基础光束剑",
                WeaponType = WeaponType.Melee,
                Description = "基础近战能量武器，适用于多种机体",
                BaseDamage = 100,
                Range = 1,
                EnergyCost = 10,
                Accuracy = 90f,
                CriticalRate = 5f,
                DevelopmentCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 1000 },
                { ResourceType.StandardAlloy, 50 }
            },
                ProductionCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 500 },
                { ResourceType.StandardAlloy, 25 }
            },
                RequiredTechnologies = new string[] { "melee_weapon_tech_1" },
                CompatibleUnitTypes = new List<UnitType>
            {
                UnitType.MassProdRobot,
                UnitType.Gundam,
                UnitType.SolarPowered
            }
            });

            _weaponTemplates.Add(new WeaponTemplate
            {
                WeaponId = "beam_saber_advanced",
                WeaponName = "高级光束剑",
                WeaponType = WeaponType.Melee,
                Description = "高能量输出的近战武器，更高伤害但能耗更大",
                BaseDamage = 150,
                Range = 1,
                EnergyCost = 15,
                Accuracy = 90f,
                CriticalRate = 8f,
                EffectType = EffectType.Shield,
                EffectValue = 10,
                EffectDuration = 1,
                DevelopmentCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 2000 },
                { ResourceType.StandardAlloy, 80 },
                { ResourceType.EnergyCrystal, 15 }
            },
                ProductionCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 1000 },
                { ResourceType.StandardAlloy, 40 },
                { ResourceType.EnergyCrystal, 8 }
            },
                RequiredTechnologies = new string[] { "melee_weapon_tech_2" },
                CompatibleUnitTypes = new List<UnitType>
            {
                UnitType.Gundam,
                UnitType.SolarPowered,
                UnitType.Getter
            }
            });

            // 远程武器
            _weaponTemplates.Add(new WeaponTemplate
            {
                WeaponId = "beam_rifle_basic",
                WeaponName = "基础光束步枪",
                WeaponType = WeaponType.Ranged,
                Description = "标准配置的远程能量武器",
                BaseDamage = 80,
                Range = 5,
                EnergyCost = 8,
                Accuracy = 85f,
                CriticalRate = 5f,
                DevelopmentCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 1200 },
                { ResourceType.StandardAlloy, 60 }
            },
                ProductionCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 600 },
                { ResourceType.StandardAlloy, 30 }
            },
                RequiredTechnologies = new string[] { "ranged_weapon_tech_1" },
                CompatibleUnitTypes = new List<UnitType>
            {
                UnitType.MassProdRobot,
                UnitType.Gundam,
                UnitType.Aircraft
            }
            });

            // 炮击武器
            _weaponTemplates.Add(new WeaponTemplate
            {
                WeaponId = "mega_cannon_basic",
                WeaponName = "基础巨炮",
                WeaponType = WeaponType.Area,
                Description = "大范围轰炸武器，能对多个目标造成伤害",
                BaseDamage = 120,
                Range = 4,
                EnergyCost = 25,
                Accuracy = 75f,
                CriticalRate = 3f,
                DevelopmentCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 2500 },
                { ResourceType.StandardAlloy, 100 },
                { ResourceType.RareMetal, 20 }
            },
                ProductionCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 1200 },
                { ResourceType.StandardAlloy, 50 },
                { ResourceType.RareMetal, 10 }
            },
                RequiredTechnologies = new string[] { "heavy_weapon_tech_1" },
                CompatibleUnitTypes = new List<UnitType>
            {
                UnitType.Tank,
                UnitType.Ship,
                UnitType.SolarPowered
            }
            });

            // 光束武器
            _weaponTemplates.Add(new WeaponTemplate
            {
                WeaponId = "hyper_beam_cannon",
                WeaponName = "超级光束炮",
                WeaponType = WeaponType.Beam,
                Description = "高能直线光束武器，贯穿力极强",
                BaseDamage = 200,
                Range = 6,
                EnergyCost = 40,
                Accuracy = 70f,
                CriticalRate = 10f,
                DevelopmentCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 3500 },
                { ResourceType.StandardAlloy, 150 },
                { ResourceType.EnergyCrystal, 30 },
                { ResourceType.BeamOre, 20 }
            },
                ProductionCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 1800 },
                { ResourceType.StandardAlloy, 75 },
                { ResourceType.EnergyCrystal, 15 },
                { ResourceType.BeamOre, 10 }
            },
                RequiredTechnologies = new string[] { "beam_weapon_tech_2" },
                CompatibleUnitTypes = new List<UnitType>
            {
                UnitType.Gundam,
                UnitType.SolarPowered,
                UnitType.BioAdaptive
            }
            });

            // 导弹武器
            _weaponTemplates.Add(new WeaponTemplate
            {
                WeaponId = "micro_missile_pod",
                WeaponName = "微型导弹发射器",
                WeaponType = WeaponType.Missile,
                Description = "发射多枚小型导弹，高命中率",
                BaseDamage = 60,
                Range = 4,
                EnergyCost = 15,
                Accuracy = 95f,
                CriticalRate = 2f,
                DevelopmentCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 1800 },
                { ResourceType.StandardAlloy, 80 },
                { ResourceType.RareMetal, 15 }
            },
                ProductionCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 900 },
                { ResourceType.StandardAlloy, 40 },
                { ResourceType.RareMetal, 8 }
            },
                RequiredTechnologies = new string[] { "missile_weapon_tech_1" },
                CompatibleUnitTypes = new List<UnitType>
            {
                UnitType.Aircraft,
                UnitType.Gundam,
                UnitType.Transformer
            }
            });

            // 特殊武器
            _weaponTemplates.Add(new WeaponTemplate
            {
                WeaponId = "rocket_punch",
                WeaponName = "火箭拳",
                WeaponType = WeaponType.Special,
                Description = "发射可控的火箭拳进行远程打击",
                BaseDamage = 160,
                Range = 3,
                EnergyCost = 20,
                Accuracy = 80f,
                CriticalRate = 15f,
                EffectType = EffectType.Stun,
                EffectValue = 1,
                EffectDuration = 1,
                DevelopmentCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 2500 },
                { ResourceType.StandardAlloy, 100 },
                { ResourceType.RareMetal, 25 },
                { ResourceType.SteelRankAlloy, 10 }
            },
                ProductionCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 1200 },
                { ResourceType.StandardAlloy, 50 },
                { ResourceType.RareMetal, 12 },
                { ResourceType.SteelRankAlloy, 5 }
            },
                RequiredTechnologies = new string[] { "special_weapon_tech_1" },
                CompatibleUnitTypes = new List<UnitType>
            {
                UnitType.SolarPowered
            }
            });

            _weaponTemplates.Add(new WeaponTemplate
            {
                WeaponId = "psychic_wave",
                WeaponName = "精神波",
                WeaponType = WeaponType.Special,
                Description = "发射精神力量冲击波，无视部分护甲",
                BaseDamage = 180,
                Range = 4,
                EnergyCost = 35,
                Accuracy = 85f,
                CriticalRate = 10f,
                EffectType = EffectType.Debuff,
                EffectValue = 20,
                EffectDuration = 2,
                DevelopmentCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 3000 },
                { ResourceType.EnergyCrystal, 40 },
                { ResourceType.PsychicElement, 15 }
            },
                ProductionCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 1500 },
                { ResourceType.EnergyCrystal, 20 },
                { ResourceType.PsychicElement, 8 }
            },
                RequiredTechnologies = new string[] { "psychic_tech_2" },
                CompatibleUnitTypes = new List<UnitType>
            {
                UnitType.Gundam,
                UnitType.BioAdaptive
            }
            });
        }
    }
}
