using System;
using Sirenix.OdinInspector;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using SuperRobot;

[ExecuteInEditMode]
public class GameDebugPanel : MonoBehaviour
{
    [Title("超级机器人战略模拟游戏 - 调试面板")] [InfoBox("使用此面板测试各个系统功能，无需UI")] [TabGroup("系统状态")] [ReadOnly]
    public int CurrentTurn;

    [TabGroup("系统状态")] [ReadOnly] public TurnPhase CurrentPhase;

    [TabGroup("系统状态")] [ReadOnly] public GameState CurrentGameState;

    private void Update()
    {
        // 实时更新状态信息
        if (Application.isPlaying)
        {
            CurrentTurn      = TurnManager.CurrentTurn;
            CurrentPhase     = TurnManager.CurrentPhase;
            CurrentGameState = GameManager.Instance?.CurrentState ?? GameState.MainMenu;
        }
    }

    [Button("初始化游戏"), TabGroup("游戏控制")]
    public void InitializeGame()
    {
        if (GameManager.Instance == null)
        {
            GameObject managerObject = new GameObject("GameManager");
            managerObject.AddComponent<GameManager>();
            Debug.Log("GameManager 已创建");
        }
        else
        {
            Debug.Log("GameManager 已存在");
        }
    }

    [Button("开始新游戏"), TabGroup("游戏控制")]
    public void StartNewGame()
    {
        // 确保GameManager已初始化
        InitializeGame();

        // 设置初始游戏状态
        GameManager.Instance.StartNewGame();
        Debug.Log("新游戏已开始");
    }

    [Button("下一回合"), TabGroup("游戏控制")]
    public void NextTurn()
    {
        TurnManager.NextTurn();
        Debug.Log($"已进入回合 {TurnManager.CurrentTurn}");
    }

    [Button("下一阶段"), TabGroup("游戏控制")]
    public void NextPhase()
    {
        TurnManager.NextPhase();
        Debug.Log($"已进入阶段 {TurnManager.CurrentPhase}");
    }

    [Serializable]
    public class ResourceDebugData
    {
        [HorizontalGroup("Resource"), LabelWidth(100)]
        public ResourceType Type;

        [HorizontalGroup("Resource"), LabelWidth(100)]
        public int Amount;
    }

    [TabGroup("资源系统")] [SerializeField] private ResourceDebugData _resourceData = new ResourceDebugData
    {
        Type   = ResourceType.Money,
        Amount = 1000
    };

    [Button("添加资源"), TabGroup("资源系统")]
    public void AddResource()
    {
        var resourceSystem = GameManager.Instance?.SystemManager.GetSystem<ResourceSystem>();
        if (resourceSystem != null)
        {
            resourceSystem.AddResource(_resourceData.Type, _resourceData.Amount);
            Debug.Log($"已添加 {_resourceData.Amount} {_resourceData.Type}");
        }
        else
        {
            Debug.LogError("ResourceSystem 不可用");
        }
    }

    [Button("显示所有资源"), TabGroup("资源系统")]
    public void ShowAllResources()
    {
        var resourceSystem = GameManager.Instance?.SystemManager.GetSystem<ResourceSystem>();
        if (resourceSystem != null)
        {
            string resources = "当前资源:\n";
            foreach (ResourceType type in System.Enum.GetValues(typeof(ResourceType)))
            {
                resources += $"{type}: {resourceSystem.GetResource(type)}\n";
            }

            Debug.Log(resources);
        }
        else
        {
            Debug.LogError("ResourceSystem 不可用");
        }
    }

    [TabGroup("技术系统")] [ValueDropdown("GetAvailableTechs"), LabelText("选择技术")]
    public string SelectedTechId;

    [Button("开始研究"), TabGroup("技术系统")]
    public void StartResearch()
    {
        var techSystem = GameManager.Instance?.SystemManager.GetSystem<TechTreeSystem>();
        if (techSystem != null)
        {
            if (techSystem.CanResearch(SelectedTechId))
            {
                techSystem.StartResearch(SelectedTechId);
                Debug.Log($"开始研究: {SelectedTechId}");
            }
            else
            {
                Debug.LogWarning($"无法研究技术: {SelectedTechId} (前置条件未满足或资源不足)");
            }
        }
        else
        {
            Debug.LogError("TechTreeSystem 不可用");
        }
    }

    [Button("直接解锁"), TabGroup("技术系统")]
    public void UnlockTechnology()
    {
        var techSystem = GameManager.Instance?.SystemManager.GetSystem<TechTreeSystem>();
        if (techSystem != null)
        {
            techSystem.UnlockTechnology(SelectedTechId);
            Debug.Log($"已解锁技术: {SelectedTechId}");
        }
        else
        {
            Debug.LogError("TechTreeSystem 不可用");
        }
    }

    [Button("推进研究进度"), TabGroup("技术系统")]
    public void BoostResearch()
    {
        var techSystem = GameManager.Instance?.SystemManager.GetSystem<TechTreeSystem>();
        if (techSystem != null)
        {
            techSystem.BoostResearch(SelectedTechId, 50);
            Debug.Log($"已推进研究: {SelectedTechId} +50点");
        }
        else
        {
            Debug.LogError("TechTreeSystem 不可用");
        }
    }

    [Button("显示研究状态"), TabGroup("技术系统")]
    public void ShowResearchStatus()
    {
        var techSystem = GameManager.Instance?.SystemManager.GetSystem<TechTreeSystem>();
        if (techSystem != null)
        {
            var researchable    = techSystem.GetResearchableTechs();
            var currentResearch = techSystem.GetCurrentResearch();

            string status = "技术研究状态:\n";

            if (currentResearch != null)
            {
                status +=
                    $"当前研究: {currentResearch.TechId}, 进度: {techSystem.GetResearchProgress()}/{currentResearch.ResearchCost}\n";
            }
            else
            {
                status += "当前没有正在研究的技术\n";
            }

            status += "\n可研究的技术:\n";
            foreach (var tech in researchable)
            {
                status += $"- {tech}\n";
            }

            Debug.Log(status);
        }
        else
        {
            Debug.LogError("TechTreeSystem 不可用");
        }
    }

// 辅助方法，获取可用技术列表
    private IEnumerable<string> GetAvailableTechs()
    {
        var techSystem = GameManager.Instance?.SystemManager.GetSystem<TechTreeSystem>();
        if (techSystem != null)
        {
            return techSystem.GetAllTechIds();
        }

        return new List<string> { "基础技术" };
    }

    [TabGroup("单位系统")] [ValueDropdown("GetUnitTemplates"), LabelText("单位模板")]
    public string UnitTemplateId;

    [TabGroup("单位系统")] [LabelText("生成位置")] public Vector2Int SpawnPosition = new Vector2Int(50, 50);

    [TabGroup("单位系统")] [ReadOnly] public List<int> CreatedUnitIds = new List<int>();

    [Button("创建单位"), TabGroup("单位系统")]
    public void CreateUnit()
    {
        var unitSystem = GameManager.Instance?.SystemManager.GetSystem<UnitManagementSystem>();
        if (unitSystem != null)
        {
            var unit = unitSystem.CreateUnit(UnitTemplateId, SpawnPosition);
            if (unit != null)
            {
                var statsComp = unit.GetComponent<UnitStatsComponent>();
                if (statsComp != null)
                {
                    CreatedUnitIds.Add(unit.EntityId);
                    Debug.Log($"已创建单位: {statsComp.UnitName} (ID: {unit.EntityId}) 在位置 {SpawnPosition}");
                }
            }
        }
        else
        {
            Debug.LogError("UnitManagementSystem 不可用");
        }
    }

    [TabGroup("单位系统")] [ValueDropdown("CreatedUnitIds"), LabelText("选择单位")]
    public int SelectedUnitId;

    [TabGroup("单位系统")] [LabelText("目标位置")] public Vector2Int TargetPosition = new Vector2Int(55, 55);

    [Button("移动单位"), TabGroup("单位系统")]
    public void MoveUnit()
    {
        var unitSystem = GameManager.Instance?.SystemManager.GetSystem<UnitManagementSystem>();
        if (unitSystem != null)
        {
            // 触发单位移动请求事件
            EventManager.Instance.TriggerEvent(new UnitMoveRequestEvent
            {
                UnitId         = SelectedUnitId,
                TargetPosition = TargetPosition
            });

            Debug.Log($"单位 {SelectedUnitId} 移动请求已发送到位置 {TargetPosition}");
        }
        else
        {
            Debug.LogError("UnitManagementSystem 不可用");
        }
    }

    [Button("显示单位信息"), TabGroup("单位系统")]
    public void ShowUnitInfo()
    {
        var unitSystem = GameManager.Instance?.SystemManager.GetSystem<UnitManagementSystem>();
        if (unitSystem != null)
        {
            var unit = unitSystem.GetUnit(SelectedUnitId);
            if (unit != null)
            {
                var statsComp = unit.GetComponent<UnitStatsComponent>();
                var posComp   = unit.GetComponent<PositionComponent>();

                string info = $"单位信息:\n";
                info += $"ID: {unit.EntityId}\n";
                info += $"名称: {statsComp.UnitName}\n";
                info += $"类型: {statsComp.UnitType}\n";
                info += $"位置: {posComp.Position}\n";
                info += $"生命: {statsComp.CurrentHealth}/{statsComp.MaxHealth}\n";
                info += $"能量: {statsComp.CurrentEnergy}/{statsComp.MaxEnergy}\n";
                info += $"移动范围: {statsComp.MovementRange}\n";
                info += $"装甲: {statsComp.BaseArmor}\n";

                // 显示武器信息
                var weapons = unit.GetComponents<WeaponComponent>();
                info += $"\n武器数量: {weapons.Count}\n";
                foreach (var weapon in weapons)
                {
                    info += $"- {weapon.WeaponName}: 伤害 {weapon.BaseDamage}, 射程 {weapon.Range}\n";
                }

                Debug.Log(info);
            }
            else
            {
                Debug.LogWarning($"找不到单位 ID: {SelectedUnitId}");
            }
        }
        else
        {
            Debug.LogError("UnitManagementSystem 不可用");
        }
    }

    [Button("销毁单位"), TabGroup("单位系统")]
    public void DestroyUnit()
    {
        var unitSystem = GameManager.Instance?.SystemManager.GetSystem<UnitManagementSystem>();
        if (unitSystem != null)
        {
            unitSystem.DestroyUnit(SelectedUnitId);
            CreatedUnitIds.Remove(SelectedUnitId);
            Debug.Log($"已销毁单位 ID: {SelectedUnitId}");
        }
        else
        {
            Debug.LogError("UnitManagementSystem 不可用");
        }
    }

// 辅助方法，获取单位模板列表
    private IEnumerable<string> GetUnitTemplates()
    {
        // 从资源加载UnitDatabase
        var unitDatabase = Resources.Load<UnitDatabase>("Data/UnitDatabase");
        if (unitDatabase != null)
        {
            return unitDatabase.UnitTemplates.Select(t => t.UnitId);
        }

        return new List<string> { "tank_basic", "gundam_basic" };
    }

    [Serializable]
    public class PilotDebugData
    {
        [HorizontalGroup("Pilot")] public string Name = "测试驾驶员";

        [HorizontalGroup("Pilot")] [Range(20, 90)]
        public int Reaction = 60;

        [HorizontalGroup("Pilot")] [Range(20, 90)]
        public int Shooting = 60;

        [HorizontalGroup("Pilot")] [Range(20, 90)]
        public int Melee = 60;

        [HorizontalGroup("Pilot")] [Range(0, 5)]
        public int NTLevel = 0;
    }

    [TabGroup("驾驶员系统")] [SerializeField] private PilotDebugData _pilotData = new PilotDebugData();

    [TabGroup("驾驶员系统")] [ReadOnly] public List<int> CreatedPilotIds = new List<int>();

    [Button("创建驾驶员"), TabGroup("驾驶员系统")]
    public void CreatePilot()
    {
        var pilotSystem = GameManager.Instance?.SystemManager.GetSystem<PilotManagementSystem>();
        if (pilotSystem != null)
        {
            Dictionary<string, int> stats = new Dictionary<string, int>
            {
                { "Reaction", _pilotData.Reaction },
                { "Shooting", _pilotData.Shooting },
                { "Melee", _pilotData.Melee },
                { "NTLevel", _pilotData.NTLevel }
            };

            var pilot     = pilotSystem.CreatePilot(_pilotData.Name, stats);
            var statsComp = pilot?.GetComponent<PilotStatsComponent>();

            if (statsComp != null)
            {
                CreatedPilotIds.Add(statsComp.PilotId);
                Debug.Log($"已创建驾驶员: {statsComp.PilotName} (ID: {statsComp.PilotId})");
            }
        }
        else
        {
            Debug.LogError("PilotManagementSystem 不可用");
        }
    }

    [TabGroup("驾驶员系统")] [ValueDropdown("CreatedPilotIds"), LabelText("选择驾驶员")]
    public int SelectedPilotId;

    [TabGroup("驾驶员系统")] [ValueDropdown("CreatedUnitIds"), LabelText("选择单位")]
    public int UnitForPilot;

    [Button("分配驾驶员"), TabGroup("驾驶员系统")]
    public void AssignPilot()
    {
        var pilotSystem = GameManager.Instance?.SystemManager.GetSystem<PilotManagementSystem>();
        if (pilotSystem != null)
        {
            bool result = pilotSystem.AssignPilotToUnit(SelectedPilotId, UnitForPilot);
            if (result)
            {
                Debug.Log($"驾驶员 {SelectedPilotId} 已分配到单位 {UnitForPilot}");
            }
            else
            {
                Debug.LogWarning("驾驶员分配失败");
            }
        }
        else
        {
            Debug.LogError("PilotManagementSystem 不可用");
        }
    }

    [Button("训练驾驶员"), TabGroup("驾驶员系统")] [ValueDropdown("GetTrainingAttributes"), LabelText("训练属性")]
    public string AttributeToTrain = "Reaction";

    [Button("开始训练"), TabGroup("驾驶员系统")]
    public void TrainPilot()
    {
        var pilotSystem = GameManager.Instance?.SystemManager.GetSystem<PilotManagementSystem>();
        if (pilotSystem != null)
        {
            bool result = pilotSystem.TrainPilot(SelectedPilotId, AttributeToTrain);
            if (result)
            {
                Debug.Log($"驾驶员 {SelectedPilotId} 的 {AttributeToTrain} 属性已提升");
            }
            else
            {
                Debug.LogWarning("训练失败");
            }
        }
        else
        {
            Debug.LogError("PilotManagementSystem 不可用");
        }
    }

    [Button("显示驾驶员信息"), TabGroup("驾驶员系统")]
    public void ShowPilotInfo()
    {
        var pilotSystem = GameManager.Instance?.SystemManager.GetSystem<PilotManagementSystem>();
        if (pilotSystem != null)
        {
            var pilot     = pilotSystem.GetPilot(SelectedPilotId);
            var statsComp = pilot?.GetComponent<PilotStatsComponent>();

            if (statsComp != null)
            {
                string info = "驾驶员信息:\n";
                info += $"ID: {statsComp.PilotId}\n";
                info += $"名称: {statsComp.PilotName}\n";
                info += $"反应: {statsComp.Reaction}\n";
                info += $"射击: {statsComp.Shooting}\n";
                info += $"格斗: {statsComp.Melee}\n";
                info += $"专注: {statsComp.Focus}\n";
                info += $"适应性: {statsComp.Adaptability}\n";
                info += $"NT等级: {statsComp.NTLevel}\n";
                info += $"等级: {statsComp.Level}\n";
                info += $"疲劳度: {statsComp.Fatigue}\n";

                Debug.Log(info);
            }
            else
            {
                Debug.LogWarning($"找不到驾驶员 ID: {SelectedPilotId}");
            }
        }
        else
        {
            Debug.LogError("PilotManagementSystem 不可用");
        }
    }

// 辅助方法，获取可训练的属性
    private IEnumerable<string> GetTrainingAttributes()
    {
        return new List<string> { "Reaction", "Shooting", "Melee", "Focus", "Adaptability" };
    }

    [TabGroup("基地系统")] [ValueDropdown("GetBaseTypes"), LabelText("基地类型")]
    public BaseType BaseTypeToCreate = BaseType.Headquarters;

    [TabGroup("基地系统")] [LabelText("基地位置")] public Vector2Int BasePosition = new Vector2Int(50, 50);

    [TabGroup("基地系统")] [LabelText("基地名称")] public string BaseName = "测试基地";

    [TabGroup("基地系统")] [ReadOnly] public List<int> CreatedBaseIds = new List<int>();

    [Button("创建基地"), TabGroup("基地系统")]
    public void CreateBase()
    {
        var baseSystem = GameManager.Instance?.SystemManager.GetSystem<BaseManagementSystem>();
        if (baseSystem != null)
        {
            var baseEntity = baseSystem.CreateBase(BaseName, BaseTypeToCreate, BasePosition);
            var baseComp   = baseEntity?.GetComponent<BaseComponent>();

            if (baseComp != null)
            {
                CreatedBaseIds.Add(baseComp.BaseId);
                Debug.Log($"已创建基地: {baseComp.BaseName} (ID: {baseComp.BaseId}) 类型: {baseComp.BaseType}");
            }
        }
        else
        {
            Debug.LogError("BaseManagementSystem 不可用");
        }
    }

    [TabGroup("基地系统")] [ValueDropdown("CreatedBaseIds"), LabelText("选择基地")]
    public int SelectedBaseId;

    [TabGroup("基地系统")] [ValueDropdown("GetFacilityTypes"), LabelText("设施类型")]
    public FacilityType FacilityToAdd;

    [Button("添加设施"), TabGroup("基地系统")]
    public void AddFacility()
    {
        var baseSystem = GameManager.Instance?.SystemManager.GetSystem<BaseManagementSystem>();
        if (baseSystem != null)
        {
            // 创建设施对象
            Facility facility = new Facility
            {
                FacilityId = $"{FacilityToAdd.ToString().ToLower()}_{System.Guid.NewGuid().ToString().Substring(0, 8)}",
                FacilityName = $"{FacilityToAdd} 设施",
                FacilityType = FacilityToAdd,
                EffectValue = 15,
                BuildCost = new Dictionary<ResourceType, int>
                {
                    { ResourceType.Money, 1000 },
                    { ResourceType.StandardAlloy, 50 }
                }
            };

            bool result = baseSystem.AddFacility(SelectedBaseId, facility);
            if (result)
            {
                Debug.Log($"已在基地 {SelectedBaseId} 添加 {FacilityToAdd} 设施");
            }
            else
            {
                Debug.LogWarning("添加设施失败");
            }
        }
        else
        {
            Debug.LogError("BaseManagementSystem 不可用");
        }
    }

    [Button("升级基地"), TabGroup("基地系统")]
    public void UpgradeBase()
    {
        var baseSystem = GameManager.Instance?.SystemManager.GetSystem<BaseManagementSystem>();
        if (baseSystem != null)
        {
            bool result = baseSystem.UpgradeBase(SelectedBaseId);
            if (result)
            {
                Debug.Log($"基地 {SelectedBaseId} 已升级");
            }
            else
            {
                Debug.LogWarning("升级基地失败");
            }
        }
        else
        {
            Debug.LogError("BaseManagementSystem 不可用");
        }
    }

    [TabGroup("基地系统")] [ValueDropdown("GetUnitTemplates"), LabelText("生产单位")]
    public string UnitToProducePath;

    [Button("添加生产项目"), TabGroup("基地系统")]
    public void AddProductionItem()
    {
        var baseSystem       = GameManager.Instance?.SystemManager.GetSystem<BaseManagementSystem>();
        var productionSystem = GameManager.Instance?.SystemManager.GetSystem<ProductionSystem>();

        if (baseSystem != null && productionSystem != null)
        {
            var productionItem = productionSystem.CreateProductionItem(
                ProductionItem.ProductionType.Unit,
                UnitToProducePath
            );

            bool result = baseSystem.AddProductionItem(SelectedBaseId, productionItem);
            if (result)
            {
                Debug.Log($"已添加 {UnitToProducePath} 到基地 {SelectedBaseId} 的生产队列");
            }
            else
            {
                Debug.LogWarning("添加生产项目失败");
            }
        }
        else
        {
            Debug.LogError("BaseManagementSystem 或 ProductionSystem 不可用");
        }
    }

    [Button("显示基地信息"), TabGroup("基地系统")]
    public void ShowBaseInfo()
    {
        var baseSystem = GameManager.Instance?.SystemManager.GetSystem<BaseManagementSystem>();
        if (baseSystem != null)
        {
            var baseEntity = baseSystem.GetBase(SelectedBaseId);
            var baseComp   = baseEntity?.GetComponent<BaseComponent>();

            if (baseComp != null)
            {
                string info = "基地信息:\n";
                info += $"ID: {baseComp.BaseId}\n";
                info += $"名称: {baseComp.BaseName}\n";
                info += $"类型: {baseComp.BaseType}\n";
                info += $"位置: {baseComp.Position}\n";
                info += $"等级: {baseComp.Level}/{baseComp.MaxLevel}\n";
                info += $"生命值: {baseComp.Health}/{baseComp.MaxHealth}\n";
                info += $"运作状态: {(baseComp.IsOperational ? "正常" : "停止")}\n";

                // 显示资源产出
                info += $"\n资源产出:\n";
                info += $"资金: {baseComp.MoneyProduction}\n";
                foreach (var resource in baseComp.ResourceProduction)
                {
                    if (resource.Value > 0)
                    {
                        info += $"{resource.Key}: {resource.Value}\n";
                    }
                }

                // 显示设施
                info += $"\n设施数量: {baseComp.Facilities.Count}\n";
                foreach (var facility in baseComp.Facilities)
                {
                    info += $"- {facility.FacilityName} ({facility.FacilityType})\n";
                }

                // 显示生产队列
                info += $"\n生产队列: {baseComp.ProductionQueue.Count}\n";
                for (int i = 0; i < baseComp.ProductionQueue.Count; i++)
                {
                    var item = baseComp.ProductionQueue[i];
                    info += $"{i + 1}. {item.ItemName} - {item.TurnsRemaining}/{item.TotalTurns}\n";
                }

                Debug.Log(info);
            }
            else
            {
                Debug.LogWarning($"找不到基地 ID: {SelectedBaseId}");
            }
        }
        else
        {
            Debug.LogError("BaseManagementSystem 不可用");
        }
    }

// 辅助方法
    private IEnumerable<BaseType> GetBaseTypes()
    {
        return System.Enum.GetValues(typeof(BaseType)).Cast<BaseType>();
    }

    private IEnumerable<FacilityType> GetFacilityTypes()
    {
        return System.Enum.GetValues(typeof(FacilityType)).Cast<FacilityType>();
    }

    [TabGroup("事件系统")] [ValueDropdown("GetEventIds"), LabelText("选择事件")]
    public string EventIdToActivate;

    [Button("激活事件"), TabGroup("事件系统")]
    public void ActivateEvent()
    {
        var eventSystem = GameManager.Instance?.SystemManager.GetSystem<EventSystem>();
        if (eventSystem != null)
        {
            // 从数据库加载事件
            var eventDatabase = Resources.Load<EventDatabase>("Data/EventDatabase");
            var gameEvent     = eventDatabase?.GetEvent(EventIdToActivate);

            if (gameEvent != null)
            {
                // 创建事件实例并触发
                GameEvent eventInstance = new GameEvent
                {
                    EventId        = gameEvent.EventId,
                    Title          = gameEvent.Title,
                    Description    = gameEvent.Description,
                    Choices        = gameEvent.Choices,
                    Duration       = gameEvent.Duration,
                    RemainingTurns = gameEvent.Duration,
                    StorylineId    = gameEvent.StorylineId,
                    Location       = gameEvent.Location,
                    EvtType        = gameEvent.EvtType
                };

                // 通过反射调用私有方法（非理想但用于调试）
                var activateMethod = typeof(EventSystem).GetMethod("ActivateEvent",
                                                                   System.Reflection.BindingFlags.NonPublic |
                                                                   System.Reflection.BindingFlags.Instance);

                if (activateMethod != null)
                {
                    activateMethod.Invoke(eventSystem, new object[] { eventInstance });
                    Debug.Log($"已激活事件: {gameEvent.Title}");
                }
                else
                {
                    Debug.LogError("无法访问ActivateEvent方法");
                }
            }
            else
            {
                Debug.LogWarning($"找不到事件ID: {EventIdToActivate}");
            }
        }
        else
        {
            Debug.LogError("EventSystem 不可用");
        }
    }

    [TabGroup("事件系统")] [LabelText("选择选项")] [Range(0, 3)]
    public int EventChoiceIndex = 0;

    [Button("选择事件选项"), TabGroup("事件系统")]
    public void SelectEventChoice()
    {
        var eventSystem = GameManager.Instance?.SystemManager.GetSystem<EventSystem>();
        if (eventSystem != null)
        {
            // 获取当前活跃事件
            var activeEvents = eventSystem.GetActiveEvents();
            if (activeEvents.Count > 0)
            {
                var activeEvent = activeEvents[0];

                // 确保选项索引有效
                if (EventChoiceIndex >= 0 && EventChoiceIndex < activeEvent.Choices.Count)
                {
                    // 触发选择事件
                    EventManager.Instance.TriggerEvent(new EventChoiceSelectedEvent
                    {
                        EventId     = activeEvent.EventId,
                        ChoiceIndex = EventChoiceIndex
                    });

                    Debug.Log($"已为事件 {activeEvent.Title} 选择选项 {EventChoiceIndex + 1}");
                }
                else
                {
                    Debug.LogWarning($"无效的选项索引: {EventChoiceIndex}");
                }
            }
            else
            {
                Debug.LogWarning("当前没有活跃事件");
            }
        }
        else
        {
            Debug.LogError("EventSystem 不可用");
        }
    }

    [Button("显示活跃事件"), TabGroup("事件系统")]
    public void ShowActiveEvents()
    {
        var eventSystem = GameManager.Instance?.SystemManager.GetSystem<EventSystem>();
        if (eventSystem != null)
        {
            var activeEvents = eventSystem.GetActiveEvents();

            if (activeEvents.Count > 0)
            {
                string info = $"活跃事件 ({activeEvents.Count}):\n";

                foreach (var evt in activeEvents)
                {
                    info += $"ID: {evt.EventId}\n";
                    info += $"标题: {evt.Title}\n";
                    info += $"描述: {evt.Description}\n";
                    info += $"剩余回合: {evt.RemainingTurns}/{evt.Duration}\n";

                    info += "选项:\n";
                    for (int i = 0; i < evt.Choices.Count; i++)
                    {
                        info += $"  {i + 1}. {evt.Choices[i].Description}\n";
                    }

                    info += "\n";
                }

                Debug.Log(info);
            }
            else
            {
                Debug.Log("当前没有活跃事件");
            }
        }
        else
        {
            Debug.LogError("EventSystem 不可用");
        }
    }

// 辅助方法
    private IEnumerable<string> GetEventIds()
    {
        var eventDatabase = Resources.Load<EventDatabase>("Data/EventDatabase");
        if (eventDatabase != null)
        {
            List<string> eventIds = new List<string>();

            foreach (var storyline in eventDatabase.Storylines)
            {
                foreach (var gameEvent in storyline.Events)
                {
                    eventIds.Add(gameEvent.EventId);
                }
            }

            return eventIds;
        }

        return new List<string> { "sample_event_1", "sample_event_2" };
    }

    [TabGroup("战斗系统")] [ValueDropdown("CreatedUnitIds"), LabelText("攻击单位")]
    public int AttackerUnitId;

    [TabGroup("战斗系统")] [ValueDropdown("CreatedUnitIds"), LabelText("目标单位")]
    public int TargetUnitId;

    [TabGroup("战斗系统")] [Range(0, 3), LabelText("武器索引")]
    public int WeaponIndex = 0;

    [Button("执行攻击"), TabGroup("战斗系统")]
    public void ExecuteAttack()
    {
        var battleSystem = GameManager.Instance?.SystemManager.GetSystem<BattleSystem>();
        if (battleSystem != null)
        {
            // 调用战斗系统的ExecuteAttack方法
            var attackMethod = typeof(BattleSystem).GetMethod("ExecuteAttack",
                                                              System.Reflection.BindingFlags.Public |
                                                              System.Reflection.BindingFlags.Instance);

            if (attackMethod != null)
            {
                attackMethod.Invoke(battleSystem, new object[] { AttackerUnitId, TargetUnitId, WeaponIndex });
                Debug.Log($"单位 {AttackerUnitId} 攻击单位 {TargetUnitId} 使用武器 {WeaponIndex}");
            }
            else
            {
                Debug.LogError("无法访问ExecuteAttack方法");
            }
        }
        else
        {
            Debug.LogError("BattleSystem 不可用");
        }
    }

    [TabGroup("战斗系统")] [ValueDropdown("GetMapIds"), LabelText("战场地图")]
    public string BattleMapId = "map_plain";

    [Button("开始战斗"), TabGroup("战斗系统")]
    public void StartBattle()
    {
        var battleSystem = GameManager.Instance?.SystemManager.GetSystem<BattleSystem>();
        if (battleSystem != null)
        {
            // 将选中的单位分为两队
            List<int> playerUnits = new List<int>();
            List<int> enemyUnits  = new List<int>();

            var unitSystem = GameManager.Instance.SystemManager.GetSystem<UnitManagementSystem>();
            foreach (var unitId in CreatedUnitIds)
            {
                var unit = unitSystem.GetUnit(unitId);

                // 简单规则：偶数ID为玩家，奇数ID为敌人
                if (unitId % 2 == 0)
                {
                    playerUnits.Add(unitId);
                }
                else
                {
                    enemyUnits.Add(unitId);
                }
            }

            // 触发战斗开始事件
            EventManager.Instance.TriggerEvent(new BattleStartEvent
            {
                BattleId    = System.Guid.NewGuid().ToString(),
                MapId       = BattleMapId,
                PlayerUnits = playerUnits,
                EnemyUnits  = enemyUnits
            });

            Debug.Log($"已开始战斗，玩家单位: {playerUnits.Count}，敌方单位: {enemyUnits.Count}");
        }
        else
        {
            Debug.LogError("BattleSystem 不可用");
        }
    }

    [Button("结束战斗"), TabGroup("战斗系统")]
    public void EndBattle()
    {
        // 触发战斗结束事件
        EventManager.Instance.TriggerEvent(new BattleEndEvent
        {
            Victory = true,
            Reward = new Dictionary<ResourceType, int>
            {
                { ResourceType.Money, 1000 },
                { ResourceType.StandardAlloy, 50 }
            }
        });

        Debug.Log("战斗已结束（玩家胜利）");
    }

// 辅助方法
    private IEnumerable<string> GetMapIds()
    {
        return new List<string>
        {
            "map_plain",
            "map_forest",
            "map_mountain",
            "map_urban",
            "map_desert",
            "map_ocean",
            "map_space"
        };
    }

    [TabGroup("AI系统")] [Range(0.1f, 1.0f), LabelText("侵略性")]
    public float AggressionLevel = 0.5f;

    [TabGroup("AI系统")] [Range(0.1f, 1.0f), LabelText("扩张倾向")]
    public float ExpansionPriority = 0.7f;

    [TabGroup("AI系统")] [Range(0.1f, 1.0f), LabelText("防御倾向")]
    public float DefensePriority = 0.5f;

    [Button("设置AI参数"), TabGroup("AI系统")]
    public void SetAIParameters()
    {
        var aiSystem = GameManager.Instance?.SystemManager.GetSystem<AISystem>();
        if (aiSystem != null)
        {
            // 通过反射设置私有字段（不理想但用于调试）
            var aggressionField = typeof(AISystem).GetField("_aggressionLevel",
                                                            System.Reflection.BindingFlags.NonPublic |
                                                            System.Reflection.BindingFlags.Instance);
            var expansionField = typeof(AISystem).GetField("_expansionPriority",
                                                           System.Reflection.BindingFlags.NonPublic |
                                                           System.Reflection.BindingFlags.Instance);
            var defenseField = typeof(AISystem).GetField("_defensePriority",
                                                         System.Reflection.BindingFlags.NonPublic |
                                                         System.Reflection.BindingFlags.Instance);

            if (aggressionField != null && expansionField != null && defenseField != null)
            {
                aggressionField.SetValue(aiSystem, AggressionLevel);
                expansionField.SetValue(aiSystem, ExpansionPriority);
                defenseField.SetValue(aiSystem, DefensePriority);

                Debug.Log($"已设置AI参数: 侵略性={AggressionLevel}, 扩张={ExpansionPriority}, 防御={DefensePriority}");
            }
            else
            {
                Debug.LogError("无法访问AI系统参数");
            }
        }
        else
        {
            Debug.LogError("AISystem 不可用");
        }
    }

    [Button("创建AI基地"), TabGroup("AI系统")]
    public void CreateAIBase()
    {
        var aiSystem = GameManager.Instance?.SystemManager.GetSystem<AISystem>();
        if (aiSystem != null)
        {
            var baseSystem = GameManager.Instance.SystemManager.GetSystem<BaseManagementSystem>();

            // 创建一个新的AI基地
            var baseEntity = baseSystem.CreateBase("AI基地", BaseType.Headquarters, new Vector2Int(80, 80));

            // 通过反射获取AI基地列表并添加
            var basesField = typeof(AISystem).GetField("_aiBases",
                                                       System.Reflection.BindingFlags.NonPublic |
                                                       System.Reflection.BindingFlags.Instance);

            if (basesField != null)
            {
                var aiBases = (List<GameEntity>)basesField.GetValue(aiSystem);
                aiBases.Add(baseEntity);

                Debug.Log("已创建AI基地并添加到AI系统");
            }
            else
            {
                Debug.LogError("无法访问AI基地列表");
            }
        }
        else
        {
            Debug.LogError("AISystem 不可用");
        }
    }

    [Button("创建AI单位"), TabGroup("AI系统")]
    public void CreateAIUnit()
    {
        var aiSystem = GameManager.Instance?.SystemManager.GetSystem<AISystem>();
        if (aiSystem != null)
        {
            var unitSystem = GameManager.Instance.SystemManager.GetSystem<UnitManagementSystem>();

            // 创建一个新的AI单位
            var unit = unitSystem.CreateUnit("tank_basic", new Vector2Int(80, 80));

            // 通过反射获取AI单位列表并添加
            var unitsField = typeof(AISystem).GetField("_aiUnits",
                                                       System.Reflection.BindingFlags.NonPublic |
                                                       System.Reflection.BindingFlags.Instance);

            if (unitsField != null)
            {
                var aiUnits = (List<GameEntity>)unitsField.GetValue(aiSystem);
                aiUnits.Add(unit);

                Debug.Log("已创建AI单位并添加到AI系统");
            }
            else
            {
                Debug.LogError("无法访问AI单位列表");
            }
        }
        else
        {
            Debug.LogError("AISystem 不可用");
        }
    }

    [Button("执行AI回合"), TabGroup("AI系统")]
    public void ProcessAITurn()
    {
        var aiSystem = GameManager.Instance?.SystemManager.GetSystem<AISystem>();
        if (aiSystem != null)
        {
            // 通过反射调用私有方法
            var processMethod = typeof(AISystem).GetMethod("ProcessAITurn",
                                                           System.Reflection.BindingFlags.NonPublic |
                                                           System.Reflection.BindingFlags.Instance);

            if (processMethod != null)
            {
                processMethod.Invoke(aiSystem, null);
                Debug.Log("AI回合已执行");
            }
            else
            {
                Debug.LogError("无法访问ProcessAITurn方法");
            }
        }
        else
        {
            Debug.LogError("AISystem 不可用");
        }
    }

}