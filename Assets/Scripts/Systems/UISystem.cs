using System;
using System.Collections.Generic;
using UnityEngine;

namespace SuperRobot
{
    // UI系统
    public class UISystem : GameSystem
    {
        // UI状态
        private UIState _currentState;

        // UI显示数据
        private Dictionary<ResourceType, int> _displayResources;
        private string                        _currentTooltip;
        private string                        _notificationMessage;
        private float                         _notificationTimer;

        // 当前选择的实体
        private int _selectedUnitId;
        private int _selectedBaseId;
        private Vector2Int _selectedGridPosition;

        // UI组件引用
        private GameObject _strategyUI;
        private GameObject _battleUI;
        private GameObject _eventUI;
        private GameObject _researchUI;
        private GameObject _productionUI;
        private GameObject _pilotUI;

        private UIManager _uiManager => UIManager.Instance;
        private IMapManager _mapManager => GameManager.Instance.MapManager;

        // UI预制体路径
        private const string STRATEGY_UI_PATH = "UI/StrategyUI";
        private const string BATTLE_UI_PATH = "UI/BattleUI";
        private const string EVENT_UI_PATH = "UI/EventUI";
        private const string RESEARCH_UI_PATH = "UI/ResearchUI";
        private const string PRODUCTION_UI_PATH = "UI/ProductionUI";
        private const string PILOT_UI_PATH = "UI/PilotUI";

        public enum UIState
        {
            Strategy,
            Battle,
            Event,
            Research,
            Production,
            Pilot,
            MainMenu,
            Loading,
            GameOver
        }

        public override void Initialize()
        {
            // 初始化UI状态
            _currentState = UIState.MainMenu;

            // 初始化资源显示
            _displayResources = new Dictionary<ResourceType, int>();
            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                _displayResources[type] = 0;
            }

            // 重置选择状态
            _selectedUnitId = -1;
            _selectedBaseId = -1;
            _selectedGridPosition = new Vector2Int(-1, -1);

            // 加载UI预制体
            LoadUIComponents();

            // 订阅事件
            SubscribeToEvents();
        }

        public override void Execute()
        {
            // 更新通知计时器
            if (_notificationTimer > 0)
            {
                _notificationTimer -= Time.deltaTime;
                if (_notificationTimer <= 0)
                {
                    _notificationMessage = "";
                }
            }

            // 根据当前UI状态更新UI组件
            UpdateUIComponents();
        }

        /// <summary>
        /// 加载UI预制体
        /// </summary>
        private void LoadUIComponents()
        {
            _strategyUI = Resources.Load<GameObject>(STRATEGY_UI_PATH);
            _battleUI = Resources.Load<GameObject>(BATTLE_UI_PATH);
            _eventUI = Resources.Load<GameObject>(EVENT_UI_PATH);
            _researchUI = Resources.Load<GameObject>(RESEARCH_UI_PATH);
            _productionUI = Resources.Load<GameObject>(PRODUCTION_UI_PATH);
            _pilotUI = Resources.Load<GameObject>(PILOT_UI_PATH);

            // 初始创建UI对象（但保持未激活状态）
            if (_strategyUI != null) _strategyUI = GameObject.Instantiate(_strategyUI);
            if (_battleUI != null) _battleUI = GameObject.Instantiate(_battleUI);
            if (_eventUI != null) _eventUI = GameObject.Instantiate(_eventUI);
            if (_researchUI != null) _researchUI = GameObject.Instantiate(_researchUI);
            if (_productionUI != null) _productionUI = GameObject.Instantiate(_productionUI);
            if (_pilotUI != null) _pilotUI = GameObject.Instantiate(_pilotUI);

            // 设置所有UI为非活动状态
            SetAllUIInactive();
        }

        /// <summary>
        /// 设置所有UI为非活动状态
        /// </summary>
        private void SetAllUIInactive()
        {
            if (_strategyUI != null) _strategyUI.SetActive(false);
            if (_battleUI != null) _battleUI.SetActive(false);
            if (_eventUI != null) _eventUI.SetActive(false);
            if (_researchUI != null) _researchUI.SetActive(false);
            if (_productionUI != null) _productionUI.SetActive(false);
            if (_pilotUI != null) _pilotUI.SetActive(false);
        }

        /// <summary>
        /// 切换UI状态
        /// </summary>
        private void ChangeUIState(UIState newState)
        {
            if (_currentState == newState)
                return;

            _currentState = newState;

            // 先禁用所有UI
            SetAllUIInactive();

            // 激活相应的UI
            switch (newState)
            {
                case UIState.Strategy:
                    if (_strategyUI != null) _strategyUI.SetActive(true);
                    break;
                case UIState.Battle:
                    if (_battleUI != null) _battleUI.SetActive(true);
                    break;
                case UIState.Event:
                    if (_eventUI != null) _eventUI.SetActive(true);
                    break;
                case UIState.Research:
                    if (_researchUI != null) _researchUI.SetActive(true);
                    break;
                case UIState.Production:
                    if (_productionUI != null) _productionUI.SetActive(true);
                    break;
                case UIState.Pilot:
                    if (_pilotUI != null) _pilotUI.SetActive(true);
                    break;
                default:
                    // 其他状态（如主菜单）使用独立场景，无需在此处理
                    break;
            }

            // 触发UI状态改变事件
            EventManager.TriggerEvent(new UIStateChangedEvent
            {
                PreviousState = _currentState,
                CurrentState = newState
            });
        }

        /// <summary>
        /// 更新UI组件
        /// </summary>
        private void UpdateUIComponents()
        {
            switch (_currentState)
            {
                case UIState.Strategy:
                    UpdateStrategyUI();
                    break;
                case UIState.Battle:
                    UpdateBattleUI();
                    break;
                case UIState.Event:
                    UpdateEventUI();
                    break;
                case UIState.Research:
                    UpdateResearchUI();
                    break;
                case UIState.Production:
                    UpdateProductionUI();
                    break;
                case UIState.Pilot:
                    UpdatePilotUI();
                    break;
            }
        }

        /// <summary>
        /// 更新策略UI
        /// </summary>
        private void UpdateStrategyUI()
        {
            if (_strategyUI == null)
                return;

            // 更新资源显示
            var resourceSystem = SystemManager.GetSystem<ResourceSystem>();
            foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
            {
                _displayResources[type] = resourceSystem.GetResource(type);
            }

            // 更新回合显示
            int currentTurn = GameManager.Instance.CurrentTurn;

            // 更新选中单位信息
            UpdateSelectedUnitInfo();

            // 更新选中基地信息
            UpdateSelectedBaseInfo();
        }

        /// <summary>
        /// 更新选中单位信息
        /// </summary>
        private void UpdateSelectedUnitInfo()
        {
            if (_selectedUnitId < 0)
                return;

            var unitSystem = SystemManager.GetSystem<UnitManagementSystem>();
            var unit = unitSystem.GetUnit(_selectedUnitId);

            if (unit == null)
            {
                _selectedUnitId = -1;
                isSelectMove = false;
                return;
            }

            var statsComp = unit.GetComponent<UnitStatsComponent>();
            if (statsComp == null)
            {
                _selectedUnitId = -1;
                isSelectMove = false;
                return;
            }

            // 更新单位信息面板
            // 这里仅作为示例，实际实现需要通过UI引用更新具体UI元素

            // 显示可移动范围
            ShowMoveRange(_selectedUnitId);
        }

        // 在UI控制器或输入处理器中
        public void OnMapClick(Vector2Int targetPosition)
        {
            // 如果有单位被选中
            if (_selectedUnitId >= 0)
            {
                // 发送移动请求
                EventManager.Instance.TriggerEvent(new UnitMoveRequestEvent
                {
                    UnitId = _selectedUnitId,
                    TargetPosition = targetPosition
                });
            }
        }

        /// <summary>
        /// 显示单位移动范围
        /// </summary>
        private void ShowMoveRange(int unitId)
        {
            var unitSystem = SystemManager.GetSystem<UnitManagementSystem>();
            var moveRange = unitSystem.GetMovementRange(unitId);

            // 显示移动范围（通过材质、特效等可视化）
        }

        /// <summary>
        /// 更新选中基地信息
        /// </summary>
        private void UpdateSelectedBaseInfo()
        {
            if (_selectedBaseId < 0)
                return;

            var baseSystem = SystemManager.GetSystem<BaseManagementSystem>();
            var baseEntity = baseSystem.GetBase(_selectedBaseId);

            if (baseEntity == null)
            {
                _selectedBaseId = -1;
                return;
            }

            var baseComp = baseEntity.GetComponent<BaseComponent>();
            if (baseComp == null)
                return;

            // 更新基地信息面板
            // 这里仅作为示例，实际实现需要通过UI引用更新具体UI元素
        }

        /// <summary>
        /// 更新战斗UI
        /// </summary>
        private void UpdateBattleUI()
        {
            if (_battleUI == null)
                return;

            // 更新战斗信息
            // ...
        }

        /// <summary>
        /// 更新事件UI
        /// </summary>
        private void UpdateEventUI()
        {
            if (_eventUI == null)
                return;

            // 获取活跃事件
            var eventSystem = SystemManager.GetSystem<EventSystem>();
            List<GameEvent> activeEvents = eventSystem.GetActiveEvents();

            // 更新事件显示
            // ...
        }

        /// <summary>
        /// 更新研究UI
        /// </summary>
        private void UpdateResearchUI()
        {
            if (_researchUI == null)
                return;

            // 获取技术树信息
            var techSystem = SystemManager.GetSystem<TechTreeSystem>();
            // 更新技术树显示
            // ...
        }

        /// <summary>
        /// 更新生产UI
        /// </summary>
        private void UpdateProductionUI()
        {
            if (_productionUI == null)
                return;

            // 获取生产队列信息
            var baseSystem = SystemManager.GetSystem<BaseManagementSystem>();
            var selectedBase = baseSystem.GetBase(_selectedBaseId);

            if (selectedBase == null)
            {
                ChangeUIState(UIState.Strategy);
                return;
            }

            var baseComp = selectedBase.GetComponent<BaseComponent>();
            if (baseComp == null)
            {
                ChangeUIState(UIState.Strategy);
                return;
            }

            // 更新生产队列显示
            // ...
        }

        /// <summary>
        /// 更新驾驶员UI
        /// </summary>
        private void UpdatePilotUI()
        {
            if (_pilotUI == null)
                return;

            // 获取驾驶员信息
            var pilotSystem = SystemManager.GetSystem<PilotManagementSystem>();
            var allPilots = pilotSystem.GetAllPilots();

            // 更新驾驶员列表显示
            // ...
        }

        /// <summary>
        /// 显示通知
        /// </summary>
        public void ShowNotification(string message, float duration = 3f)
        {
            _notificationMessage = message;
            _notificationTimer = duration;

            // 更新通知UI
            // ...
        }

        /// <summary>
        /// 订阅事件
        /// </summary>
        private void SubscribeToEvents()
        {
            // 游戏状态相关事件
            EventManager.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
            EventManager.Subscribe<TurnStartedEvent>(OnTurnStarted);
            EventManager.Subscribe<TurnPhaseChangedEvent>(OnTurnPhaseChanged);

            // 选择相关事件
            EventManager.Subscribe<UnitSelectedEvent>(OnUnitSelected);
            EventManager.Subscribe<BaseSelectedEvent>(OnBaseSelected);
            EventManager.Subscribe<TerrainSelectedEvent>(OnTerrainSelected);

            // 战斗相关事件
            EventManager.Subscribe<BattleStartedEvent>(OnBattleStarted);
            EventManager.Subscribe<BattleEndedEvent>(OnBattleEnded);

            // 事件系统相关事件
            EventManager.Subscribe<EventActivatedEvent>(OnEventActivated);
            EventManager.Subscribe<EventChoiceAppliedEvent>(OnEventChoiceApplied);

            // 资源相关事件
            EventManager.Subscribe<ResourceUpdatedEvent>(OnResourceUpdated);

            // 研究相关事件
            EventManager.Subscribe<TechResearchCompletedEvent>(OnTechResearchCompleted);

            // 生产相关事件
            EventManager.Subscribe<UnitProductionCompletedEvent>(OnUnitProductionCompleted);
        }

        /// <summary>
        /// 游戏状态改变事件处理
        /// </summary>
        private void OnGameStateChanged(GameStateChangedEvent evt)
        {
            switch (evt.CurrentState)
            {
                case GameState.MainMenu:
                    ChangeUIState(UIState.MainMenu);
                    break;
                case GameState.Playing:
                    ChangeUIState(UIState.Strategy);
                    break;
                case GameState.Battle:
                    ChangeUIState(UIState.Battle);
                    break;
                case GameState.GameOver:
                    ChangeUIState(UIState.GameOver);
                    break;
            }
        }

        /// <summary>
        /// 回合开始事件处理
        /// </summary>
        private void OnTurnStarted(TurnStartedEvent evt)
        {
            // 显示回合开始通知
            ShowNotification($"回合 {evt.TurnNumber} 开始");
        }

        /// <summary>
        /// 回合阶段改变事件处理
        /// </summary>
        private void OnTurnPhaseChanged(TurnPhaseChangedEvent evt)
        {
            // 显示阶段改变通知
            string phaseName = "";
            switch (evt.NewPhase)
            {
                case TurnPhase.Strategy:
                    phaseName = "策略阶段";
                    break;
                case TurnPhase.Action:
                    phaseName = "行动阶段";
                    break;
                case TurnPhase.Event:
                    phaseName = "事件阶段";
                    break;
                case TurnPhase.Enemy:
                    phaseName = "敌方阶段";
                    break;
                case TurnPhase.Settlement:
                    phaseName = "结算阶段";
                    break;
            }

            ShowNotification($"进入{phaseName}");
        }

        private bool isSelectMove = false;

        /// <summary>
        /// 单位选择事件处理
        /// </summary>
        private void OnUnitSelected(UnitSelectedEvent evt)
        {
            _selectedUnitId = evt.UnitId;
            _selectedBaseId = -1; // 清除基地选择

            // 获取单位
            var unitSystem = GameManager.Instance.SystemManager.GetSystem<UnitManagementSystem>();
            var unit = unitSystem.GetUnit(_selectedUnitId);
            
            if (unit != null)
            {
                // 显示操作按钮
                _uiManager.ShowSelections(new List<string> { "移动"},(index)=>{
                    if(index == 0)
                    {
                        // 获取移动范围
                        var movementSystem = GameManager.Instance.SystemManager.GetSystem<UnitMovementSystem>();
                        var posComp = unit.GetComponent<PositionComponent>();
                        isSelectMove = true;
                        
                        if (posComp != null)
                        {
                            // 显示移动范围
                            ShowMovementRange(unit);
                        }
                    }
                });
                
            }
        }

        private void ShowMovementRange(GameEntity unit)
        {
            var statsComp = unit.GetComponent<UnitStatsComponent>();
            var posComp = unit.GetComponent<PositionComponent>();
            
            if (statsComp == null || posComp == null)
                return;
                
            // 获取移动范围
            int moveRange = statsComp.MovementRange;

            // 获取可移动范围
            var ranges = _mapManager.GetMoveRange(posComp.Position, moveRange);

            // 高亮显示可移动范围
            _uiManager.HighlightCells(ranges, Color.green);
        }

        /// <summary>
        /// 基地选择事件处理
        /// </summary>
        private void OnBaseSelected(BaseSelectedEvent evt)
        {
            _selectedBaseId = evt.BaseId;
            _selectedUnitId = -1; // 清除单位选择

            // 更新UI显示
            var baseEntity = EntityManager.GetEntity(_selectedBaseId);
            if (baseEntity != null)
            {
                var baseComp = baseEntity.GetComponent<BaseComponent>();
                if (baseComp != null)
                {
                    _uiManager.ShowSimpleInfo(baseComp.BaseName);
                    // 显示可操作按钮
                    _uiManager.ShowSelections(new List<string> { "生产单位", "研究技术" }, (index) =>
                    {
                        if (index == 0)
                        {
                            // 生产单位
                            // 生产位置在基地旁边一格
                            var posComp = baseEntity.GetComponent<PositionComponent>();
                            if (posComp == null)
                                return;
                            var range = _mapManager.GetMoveRange(posComp.Position, 1);
                            var pos = range[0];
                            EventManager.TriggerEvent(new UnitProductionCompletedEvent()
                            {
                                BaseId = _selectedBaseId,
                                UnitTemplateId = "gundam_basic",
                                Position = pos
                            });
                        }
                        else if (index == 1)
                        {
                            // 研究技术
                            EventManager.TriggerEvent(new TechResearchCompletedEvent()
                            {
                                TechId = "basic_energy_1"
                            });
                        }
                    });
                }
            }
        }

        /// <summary>
        /// 地形选择事件处理
        /// </summary>
        private void OnTerrainSelected(TerrainSelectedEvent evt)
        {
            _selectedGridPosition = evt.GridPosition;

            // 找到所有location组件的实体
            var entities = EntityManager.GetEntitiesAtPosition(evt.GridPosition);
            if (entities.Count > 0)
            {
                if (entities[0].GetComponent<BaseComponent>() != null)
                {
                    _uiManager.ShowSimpleInfo(entities[0].GetComponent<BaseComponent>().BaseName);
                    _selectedBaseId = entities[0].EntityId;
                    Debug.Log($"OnBaseSelected: {_selectedBaseId}");
                    // 发送基地选择事件
                    EventManager.TriggerEvent(new BaseSelectedEvent { BaseId = _selectedBaseId });
                }
                else if (entities[0].GetComponent<UnitStatsComponent>() != null)
                {
                    _uiManager.ShowSimpleInfo(entities[0].GetComponent<UnitStatsComponent>().UnitName);
                    _selectedUnitId = entities[0].EntityId;
                    Debug.Log($"OnUnitSelected: {_selectedUnitId}");
                }
                return;
            }

            // 如果已选择单位，尝试移动
            if (_selectedUnitId >= 0 && isSelectMove)
            {
                // 触发单位移动请求
                EventManager.TriggerEvent(new UnitMoveRequestEvent
                {
                    UnitId = _selectedUnitId,
                    TargetPosition = evt.GridPosition
                });
                isSelectMove = false;
            }
            // 清除选择
            else
            {
                _selectedUnitId = -1;
                _selectedBaseId = -1;
                isSelectMove = false;
            }

            // 更新UI显示
            var terrainType = _mapManager.GetCellType(evt.GridPosition);
            
            _uiManager.ShowSimpleInfo(terrainType);
        }

        private void OnUnitMovementCompleted(UnitMovementCompletedEvent evt)
        {
            // 清除高亮显示
            _uiManager.ClearHighlights();
            
            // 更新UI
            _uiManager.UpdateUnitInfo(evt.UnitId);
            
            // 检查是否有敌人在攻击范围内
            //CheckForEnemiesInRange(evt.UnitId);
        }

        private void OnUnitMovementStarted(UnitMovementStartedEvent evt)
        {
            // 显示移动路径
            _uiManager.ShowPath(evt.Path);
            
            // 播放移动音效
            //_audioManager.PlaySound("unit_move");
        }

        /// <summary>
        /// 战斗开始事件处理
        /// </summary>
        private void OnBattleStarted(BattleStartedEvent evt)
        {
            ChangeUIState(UIState.Battle);

            // 显示战斗开始通知
            ShowNotification("战斗开始");
        }

        /// <summary>
        /// 战斗结束事件处理
        /// </summary>
        private void OnBattleEnded(BattleEndedEvent evt)
        {
            ChangeUIState(UIState.Strategy);

            // 显示战斗结果通知
            ShowNotification(evt.Victory ? "战斗胜利！" : "战斗失败！");
        }

        /// <summary>
        /// 事件激活事件处理
        /// </summary>
        private void OnEventActivated(EventActivatedEvent evt)
        {
            ChangeUIState(UIState.Event);

            // 显示事件激活通知
            ShowNotification($"新事件：{evt.Title}");
        }

        /// <summary>
        /// 事件选择应用事件处理
        /// </summary>
        private void OnEventChoiceApplied(EventChoiceAppliedEvent evt)
        {
            ChangeUIState(UIState.Strategy);

            // 显示事件结果通知
            ShowNotification(evt.ResultMessage);
        }

        /// <summary>
        /// 资源更新事件处理
        /// </summary>
        private void OnResourceUpdated(ResourceUpdatedEvent evt)
        {
            _displayResources[evt.ResourceType] = evt.CurrentAmount;

            // 如果资源变化较大，显示通知
            int change = evt.CurrentAmount - evt.PreviousAmount;
            if (Math.Abs(change) > 100)
            {
                string resourceName = evt.ResourceType.ToString();
                string changeText = change > 0 ? $"+{change}" : $"{change}";
                ShowNotification($"{resourceName}: {changeText}");
            }
        }

        /// <summary>
        /// 技术研发完成事件处理
        /// </summary>
        private void OnTechResearchCompleted(TechResearchCompletedEvent evt)
        {
            // 显示研发完成通知
            ShowNotification($"研发完成：{evt.TechName}");
        }

        /// <summary>
        /// 单位生产完成事件处理
        /// </summary>
        private void OnUnitProductionCompleted(UnitProductionCompletedEvent evt)
        {
            // 显示生产完成通知
            ShowNotification($"单位生产完成：{evt.UnitTemplateId}");
        }

        public override void Cleanup()
        {
            // 取消订阅所有事件
            EventManager.Instance.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
            EventManager.Instance.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            EventManager.Instance.Unsubscribe<TurnPhaseChangedEvent>(OnTurnPhaseChanged);
            EventManager.Instance.Unsubscribe<UnitSelectedEvent>(OnUnitSelected);
            EventManager.Instance.Unsubscribe<BaseSelectedEvent>(OnBaseSelected);
            EventManager.Instance.Unsubscribe<TerrainSelectedEvent>(OnTerrainSelected);
            EventManager.Instance.Unsubscribe<BattleStartedEvent>(OnBattleStarted);
            EventManager.Instance.Unsubscribe<BattleEndedEvent>(OnBattleEnded);
            EventManager.Instance.Unsubscribe<EventActivatedEvent>(OnEventActivated);
            EventManager.Instance.Unsubscribe<EventChoiceAppliedEvent>(OnEventChoiceApplied);
            EventManager.Instance.Unsubscribe<ResourceUpdatedEvent>(OnResourceUpdated);
            EventManager.Instance.Unsubscribe<TechResearchCompletedEvent>(OnTechResearchCompleted);
            EventManager.Instance.Unsubscribe<UnitProductionCompletedEvent>(OnUnitProductionCompleted);

            // 销毁UI对象
            if (_strategyUI != null) GameObject.Destroy(_strategyUI);
            if (_battleUI != null) GameObject.Destroy(_battleUI);
            if (_eventUI != null) GameObject.Destroy(_eventUI);
            if (_researchUI != null) GameObject.Destroy(_researchUI);
            if (_productionUI != null) GameObject.Destroy(_productionUI);
            if (_pilotUI != null) GameObject.Destroy(_pilotUI);
        }
    }
}