using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;
using UnityEngine.AddressableAssets;

namespace SuperRobot
{
    // Enum to identify different UI screens or major panels managed by UIDocManager
    // This might differ from the old PanelType if some panels are now part of larger UXML documents
    public enum UIPanelType
    {
        MainMenu,
        GameHUD, // The main game interface (GameHUDScreen.uxml)
        PauseMenu,
        UnitInfo,
        BaseInfo,
        CommandCenter,
        // ConfirmationDialog is handled as a sub-component within GameHUD
        // Tooltip, ContextMenu, Notifications are also part of GameHUD
    }

    // For context menu items, similar to the old UIManager
    public struct ContextMenuItemData
    {
        public string          Label;
        public Action          OnSelected;
        public bool            IsEnabled;
        public string          TooltipText;  // For potential future use with UI Toolkit tooltips
        public VisualTreeAsset ItemTemplate; // Optional: UXML template for the item
    }

    public class UIDocManager : MonoBehaviour, IUIManager
    {
        private static UIDocManager _instance;
        public static  UIDocManager Instance => _instance;

        [Header("UI Document References")] [SerializeField]
        private UIDocument uiDocument; // Assign your main UI Document here

        [Header("UXML Templates")] [SerializeField]
        private VisualTreeAsset mainMenuTemplate; // MainMenuScreen.uxml

        [SerializeField] private VisualTreeAsset gameHudTemplate;            // GameHUDScreen.uxml
        [SerializeField] private VisualTreeAsset pauseMenuTemplate;          // PauseMenu.uxml
        [SerializeField] private VisualTreeAsset unitInfoPanelTemplate;      // UnitInfoPanel.uxml
        [SerializeField] private VisualTreeAsset baseInfoPanelTemplate;      // BaseInfoPanel.uxml
        [SerializeField] private VisualTreeAsset commandCenterPanelTemplate; // CommandCenterPanel.uxml
        [SerializeField] private VisualTreeAsset confirmationDialogTemplate; // ConfirmationDialog.uxml
        [SerializeField] private VisualTreeAsset contextMenuItemTemplate;    // ContextMenuItem.uxml (optional)

        [SerializeField]
        private VisualTreeAsset notificationItemTemplate; // NotificationItem.uxml (should already exist)

        private VisualElement _rootVisualElement;

        // Game HUD specific elements (cached for frequent access)
        private VisualElement _gameHudContainer;
        private Label         _moneyLabel;
        private Label         _alloyLabel;
        private Label         _turnInfoLabel;
        private VisualElement _resourceOverview;
        private VisualElement _minimapPanel;
        private VisualElement _contextMenuContainer;
        private VisualElement _actionMenuContainer;
        private VisualElement _notificationArea;
        private VisualElement _dialogContainer; // For confirmation dialogs
        private VisualElement _tooltipElement;
        private Label         _tooltipLabel;

        // Other panel roots (if loaded additively or controlled within a master document)
        private VisualElement _pauseMenuContainer;
        private VisualElement _unitInfoContainer;
        private VisualElement _baseInfoContainer;
        private VisualElement _commandCenterContainer;

        // --- Unit Info Panel Elements ---
        private Label _unitNameText;
        private Label _unitStatsText;

        // --- Base Info Panel Elements ---
        private Label _baseNameText;
        private Label _baseStatsText;

        // --- Confirmation Dialog Elements ---
        private Label  _dialogTitleLabel;
        private Label  _dialogMessageLabel;
        private Button _dialogConfirmButton;
        private Button _dialogCancelButton;
        private Action _currentDialogConfirmAction;
        private Action _currentDialogCancelAction;

        void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            if (uiDocument == null)
            {
                Debug.LogError("UIDocument is not assigned in UIDocManager!");
                enabled = false;
                return;
            }

            _rootVisualElement = uiDocument.rootVisualElement;
        }

        void Start()
        {
            // Initialize with a default screen (e.g., Main Menu)
            // ShowPanel(UIPanelType.MainMenu);
            // For now, let's assume GameHUD is loaded by default if game starts directly
            if (_rootVisualElement.childCount == 0 && gameHudTemplate != null) // If root is empty, load GameHUD
            {
                LoadScreen(gameHudTemplate);
                InitializeGameHudReferences();
            }
            else if (_rootVisualElement.childCount > 0) // If UXML is already loaded via UIDocument's source asset
            {
                InitializeGameHudReferences(); // Attempt to init if GameHUD is the one loaded
            }

            HideAllOverlayPanels(); // Ensure panels that overlay GameHUD are initially hidden
        }

        private void LoadScreen(VisualTreeAsset newScreenTemplate)
        {
            if (newScreenTemplate == null) return;
            _rootVisualElement.Clear();
            newScreenTemplate.CloneTree(_rootVisualElement);
        }

        private void InitializeGameHudReferences()
        {
            _gameHudContainer = _rootVisualElement.Q("game-hud-screen");
            if (_gameHudContainer == null)
            {
                //Debug.LogWarning("Game HUD container ('game-hud-screen') not found. UI might not function correctly.");
                return; // Not on GameHUD screen or UXML is different
            }

            _moneyLabel           = _gameHudContainer.Q<Label>("money-label");
            _alloyLabel           = _gameHudContainer.Q<Label>("alloy-label");
            _turnInfoLabel        = _gameHudContainer.Q<Label>("turn-info");
            _resourceOverview     = _gameHudContainer.Q("resource-overview");
            _minimapPanel         = _gameHudContainer.Q("minimap-panel");
            _contextMenuContainer = _gameHudContainer.Q("context-menu-container");
            _actionMenuContainer  = _gameHudContainer.Q("action-menu-container");
            _notificationArea = _gameHudContainer.Q("notification-area-instance") ??
                                _gameHudContainer.Q("notification-area"); // Check for instance first
            _dialogContainer = _gameHudContainer.Q("dialog-container");
            _tooltipElement  = _gameHudContainer.Q("tooltip-display-instance") ?? _gameHudContainer.Q("tooltip");
            if (_tooltipElement != null)
                _tooltipLabel = _tooltipElement.Q<Label>(); // Assuming tooltip has a label inside

            // Ensure these overlaying containers are initially hidden
            SetElementDisplay(_contextMenuContainer, false);
            SetElementDisplay(_actionMenuContainer, false);
            SetElementDisplay(_dialogContainer, false);
            SetElementDisplay(_tooltipElement, false);
        }

        private void HideAllOverlayPanels()
        {
            SetElementDisplay(_pauseMenuContainer, false);
            SetElementDisplay(_unitInfoContainer, false);
            SetElementDisplay(_baseInfoContainer, false);
            SetElementDisplay(_commandCenterContainer, false);
        }

        public void ShowPanel(UIPanelType panelType)
        {
            // HideAllOverlayPanels(); // Optional: Hide other overlays when a new main one is shown

            switch (panelType)
            {
                case UIPanelType.MainMenu:
                    LoadScreen(mainMenuTemplate);
                    // InitializeMainMenuReferences(); // If any
                    break;
                case UIPanelType.GameHUD:
                    LoadScreen(gameHudTemplate);
                    InitializeGameHudReferences();
                    break;
                case UIPanelType.PauseMenu:
                    _pauseMenuContainer =
                        LoadPanelIntoRoot(pauseMenuTemplate, _pauseMenuContainer, "pause-menu-screen");
                    SetElementDisplay(_pauseMenuContainer, true);
                    break;
                case UIPanelType.UnitInfo:
                    _unitInfoContainer =
                        LoadPanelIntoRoot(unitInfoPanelTemplate, _unitInfoContainer, "unit-info-panel");
                    SetElementDisplay(_unitInfoContainer, true);
                    break;
                case UIPanelType.BaseInfo:
                    _baseInfoContainer =
                        LoadPanelIntoRoot(baseInfoPanelTemplate, _baseInfoContainer, "base-info-panel");
                    SetElementDisplay(_baseInfoContainer, true);
                    var styleSheet = Addressables.LoadAssetAsync<StyleSheet>("Assets/UI Toolkit/StyleSheets/Components/BaseInfoPanel.uss").WaitForCompletion();
                    _baseInfoContainer.styleSheets.Add(styleSheet);
                    break;
                case UIPanelType.CommandCenter:
                    _commandCenterContainer =
                        LoadPanelIntoRoot(commandCenterPanelTemplate, _commandCenterContainer, "command-center-panel");
                    SetElementDisplay(_commandCenterContainer, true);
                    break;
            }
        }

        public void HidePanel(UIPanelType panelType)
        {
            switch (panelType)
            {
                case UIPanelType.PauseMenu:
                    SetElementDisplay(_pauseMenuContainer, false);
                    break;
                case UIPanelType.UnitInfo:
                    SetElementDisplay(_unitInfoContainer, false);
                    break;
                case UIPanelType.BaseInfo:
                    SetElementDisplay(_baseInfoContainer, false);
                    break;
                case UIPanelType.CommandCenter:
                    SetElementDisplay(_commandCenterContainer, false);
                    break;
                // MainMenu and GameHUD are typically not "hidden" but replaced
            }
        }

        // Helper to load a panel template into the root if it's not already loaded
        private VisualElement LoadPanelIntoRoot(VisualTreeAsset template, VisualElement currentContainer,
                                                string          containerName)
        {
            if (template == null) return currentContainer;
            if (currentContainer != null && currentContainer.parent == _rootVisualElement)
                return currentContainer; // Already loaded

            var newInstance = template.Instantiate();
            currentContainer =
                newInstance.Q(containerName) ?? newInstance; // Get the named container or the root of the template
            _rootVisualElement.Add(currentContainer);
            return currentContainer;
        }

        // Toggle for sections within GameHUD like Minimap, ResourceOverview
        public void ToggleGameHudSection(string sectionElementName, bool forceState = false, bool useForce = false)
        {
            if (_gameHudContainer == null) InitializeGameHudReferences();
            if (_gameHudContainer == null) return; // Still null, not on GameHUD

            VisualElement section = _gameHudContainer.Q(sectionElementName);
            if (section != null)
            {
                bool newState = useForce ? forceState : section.style.display == DisplayStyle.None;
                SetElementDisplay(section, newState);
            }
        }

        public void UpdateResourceDisplay(int money, int alloy)
        {
            if (_moneyLabel != null) _moneyLabel.text = $"金钱: {money}";
            if (_alloyLabel != null) _alloyLabel.text = $"合金: {alloy}";
        }

        public void UpdateTurnInfo(int currentTurn, string phaseName)
        {
            if (_turnInfoLabel != null) _turnInfoLabel.text = $"回合: {currentTurn} / {phaseName}";
        }

        public void ShowContextMenu(Vector2 screenPosition, List<ContextMenuItemData> menuItems)
        {
            if (_contextMenuContainer == null) return;
            _contextMenuContainer.Clear(); // Clear previous items

            foreach (var itemData in menuItems)
            {
                VisualElement menuItemVE;
                if (itemData.ItemTemplate != null)
                {
                    menuItemVE = itemData.ItemTemplate.Instantiate();
                    // TODO: Populate template fields if any (e.g., icon)
                }
                else if (contextMenuItemTemplate != null) // Fallback to manager's default template
                {
                    menuItemVE = contextMenuItemTemplate.Instantiate();
                }
                else // Create a simple button if no template
                {
                    menuItemVE = new Button() { text = itemData.Label };
                }

                var button = menuItemVE.Q<Button>() ?? menuItemVE as Button;
                if (button != null)
                {
                    button.text = itemData.Label;
                    button.SetEnabled(itemData.IsEnabled);
                    button.clicked += () =>
                    {
                        itemData.OnSelected?.Invoke();
                        HideContextMenu();
                    };
                    // TODO: Add tooltip support if needed
                }

                _contextMenuContainer.Add(menuItemVE);
            }

            // Position and show (simple version, might need screen to UI space conversion)
            _contextMenuContainer.style.left = screenPosition.x;
            _contextMenuContainer.style.top =
                screenPosition.y; // This is screen space, likely needs conversion for robust positioning
            SetElementDisplay(_contextMenuContainer, true);
        }

        public void Initialize()
        {
            // 初始化高亮单元格字典
            foreach (HighlightType type in Enum.GetValues(typeof(HighlightType)))
            {
                if (type != HighlightType.All)
                {
                    _highlightedCells[type] = new List<Vector2Int>();
                }
            }

            // 初始化UI元素引用
            if (_rootVisualElement == null && uiDocument != null)
            {
                _rootVisualElement = uiDocument.rootVisualElement;
            }

            // 初始化GameHUD引用
            InitializeGameHudReferences();

            // 隐藏所有UI面板
            HideAllUI();
        }

        public void ShowMainMenu()
        {
            HideAllUI();
            LoadScreen(mainMenuTemplate);
        }

        public void ShowGameUI()
        {
            HideAllUI();
            LoadScreen(gameHudTemplate);
            InitializeGameHudReferences();

            // 显示默认游戏面板
            ToggleGameHudSection("resource-overview", true, true);
            ToggleGameHudSection("turn-info", true, true);
            ToggleGameHudSection("minimap-panel", true, true);
        }

        public void ShowPauseMenu()
        {
            ShowPanel(UIPanelType.PauseMenu);

            // 初始化暂停菜单按钮事件
            if (_pauseMenuContainer != null)
            {
                var resumeButton = _pauseMenuContainer.Q<Button>("resume-button");
                if (resumeButton != null)
                {
                    resumeButton.clicked -= HidePauseMenu;
                    resumeButton.clicked += HidePauseMenu;
                }

                var exitToMainMenuButton = _pauseMenuContainer.Q<Button>("exit-to-main-menu-button");
                if (exitToMainMenuButton != null)
                {
                    exitToMainMenuButton.clicked -= () => ShowMainMenu();
                    exitToMainMenuButton.clicked += () => ShowMainMenu();
                }
            }
        }

        private void HidePauseMenu()
        {
            HidePanel(UIPanelType.PauseMenu);
        }

        public void HideAllUI()
        {
            // 隐藏所有主面板
            if (_gameHudContainer != null) SetElementDisplay(_gameHudContainer, false);

            // 隐藏所有信息面板
            HideAllOverlayPanels();

            // 隐藏游戏HUD内的元素
            if (_resourceOverview != null) SetElementDisplay(_resourceOverview, false);
            if (_turnInfoLabel != null && _turnInfoLabel.parent != null)
                SetElementDisplay(_turnInfoLabel.parent, false);
            if (_minimapPanel         != null) SetElementDisplay(_minimapPanel, false);
            if (_contextMenuContainer != null) SetElementDisplay(_contextMenuContainer, false);
            if (_actionMenuContainer  != null) SetElementDisplay(_actionMenuContainer, false);
            if (_notificationArea     != null) SetElementDisplay(_notificationArea, false);
            if (_dialogContainer      != null) SetElementDisplay(_dialogContainer, false);
            if (_tooltipElement       != null) SetElementDisplay(_tooltipElement, false);
        }

        // 添加这些字段到类的顶部
        private int _selectedEntityId = -1;

        private Dictionary<HighlightType, List<Vector2Int>> _highlightedCells =
            new Dictionary<HighlightType, List<Vector2Int>>();

// 外部依赖
        private IMapManager   _mapManager    => GameManager.Instance.MapManager;
        private EntityManager _entityManager => GameManager.Instance.EntityManager;

        public void ShowContextMenu(Vector2 screenPosition, List<ContextMenuItem> menuItems)
        {
            if (_contextMenuContainer == null) return;

            _contextMenuContainer.Clear(); // 清除之前的菜单项

            // 将ContextMenuItem转换为ContextMenuItemData
            var itemDataList = new List<ContextMenuItemData>();
            foreach (var item in menuItems)
            {
                itemDataList.Add(new ContextMenuItemData
                {
                    Label       = item.Label,
                    OnSelected  = item.OnSelected,
                    IsEnabled   = item.IsEnabled,
                    TooltipText = item.TooltipText
                });
            }

            // 调用现有的ShowContextMenu方法
            ShowContextMenu(screenPosition, itemDataList);
        }

        public void HideContextMenu()
        {
            SetElementDisplay(_contextMenuContainer, false);
        }

        public void SelectEntity(int entityId)
        {
            // 取消选择之前的实体
            DeselectEntity();

            // 设置新的选择
            _selectedEntityId = entityId;

            // 获取实体
            var entity = _entityManager.GetEntity(entityId);
            if (entity == null)
                return;

            // 确定实体类型并相应处理
            if (entity.HasComponent<UnitStatsComponent>())
            {
                // 处理单位选择
                var statsComp = entity.GetComponent<UnitStatsComponent>();
                var posComp   = entity.GetComponent<PositionComponent>();

                if (posComp != null)
                {
                    // 高亮选中单位位置
                    HighlightCells(new List<Vector2Int> { posComp.Position }, HighlightType.Selection);

                    // 显示单位信息
                    ShowUnitInfo(entityId, Input.mousePosition);

                    // 播放选择效果
                    PlaySelectionEffect(entityId);
                }
            }
            else if (entity.HasComponent<BaseComponent>())
            {
                // 处理基地选择
                var baseComp = entity.GetComponent<BaseComponent>();
                var posComp  = entity.GetComponent<PositionComponent>();

                if (posComp != null)
                {
                    // 高亮选中基地位置
                    HighlightCells(new List<Vector2Int> { posComp.Position }, HighlightType.Selection);

                    // 显示基地信息
                    ShowBaseInfo(entityId, Input.mousePosition);

                    // 播放选择效果
                    PlaySelectionEffect(entityId);
                }
            }

            // 触发选择事件
            if (entity.HasComponent<UnitStatsComponent>())
            {
                EventManager.Instance.TriggerEvent(new UnitSelectedEvent { UnitId = entityId });
            }
            else if (entity.HasComponent<BaseComponent>())
            {
                EventManager.Instance.TriggerEvent(new BaseSelectedEvent { BaseId = entityId });
            }
        }

        public void DeselectEntity()
        {
            if (_selectedEntityId != -1)
            {
                // 清除所有选择高亮
                ClearHighlights(HighlightType.Selection);
                ClearHighlights(HighlightType.Movement);
                ClearHighlights(HighlightType.Attack);
                ClearHighlights(HighlightType.Path);

                // 隐藏信息面板
                HidePanel(UIPanelType.UnitInfo);
                HidePanel(UIPanelType.BaseInfo);

                _selectedEntityId = -1;
            }
        }

        public void HighlightCells(List<Vector2Int> cells, HighlightType type)
        {
            // 清除此类型的先前高亮
            ClearHighlights(type);

            // 存储高亮的单元格
            _highlightedCells[type] = new List<Vector2Int>(cells);

            // 根据类型应用高亮
            Color highlightColor = GetHighlightColor(type);
            _mapManager.HighlightCells(cells);
        }

        public void ClearHighlights(HighlightType type = HighlightType.All)
        {
            if (type == HighlightType.All)
            {
                // 清除所有高亮
                foreach (var kvp in _highlightedCells)
                {
                    if (kvp.Value.Count > 0)
                    {
                        _mapManager.ClearHighlightCells();
                        _highlightedCells[kvp.Key].Clear();
                    }
                }
            }
            else
            {
                // 清除特定类型的高亮
                if (_highlightedCells.ContainsKey(type) && _highlightedCells[type].Count > 0)
                {
                    _mapManager.ClearHighlightCells();
                    _highlightedCells[type].Clear();
                }
            }
        }

// 辅助方法：获取高亮颜色
        private Color GetHighlightColor(HighlightType type)
        {
            switch (type)
            {
                case HighlightType.Selection:
                    return Color.yellow;
                case HighlightType.Movement:
                    return Color.blue;
                case HighlightType.Attack:
                    return Color.red;
                case HighlightType.Path:
                    return Color.green;
                default:
                    return Color.white;
            }
        }

        public void ShowMovementRange(int unitId)
        {
            var unitSystem = GameManager.Instance.SystemManager.GetSystem<UnitManagementSystem>();
            var unit       = unitSystem.GetUnit(unitId);

            if (unit != null)
            {
                var statsComp = unit.GetComponent<UnitStatsComponent>();
                var posComp   = unit.GetComponent<PositionComponent>();

                if (statsComp != null && posComp != null)
                {
                    // 获取移动范围
                    var moveRange = unitSystem.GetMovementRange(unitId);

                    // 高亮移动范围
                    HighlightCells(moveRange, HighlightType.Movement);
                }
            }
        }

        public void ShowAttackRange(int unitId)
        {
            var unitSystem   = GameManager.Instance.SystemManager.GetSystem<UnitManagementSystem>();
            var battleSystem = GameManager.Instance.SystemManager.GetSystem<BattleSystem>();
            var unit         = unitSystem.GetUnit(unitId);

            if (unit != null && battleSystem != null)
            {
                // TODO: 实现获取攻击范围的功能
                // 目前暂时使用一个空列表作为占位符
                var attackRangeList = new List<Vector2Int>();

                // 当BattleSystem实现GetAttackRange方法后再启用此代码
                // var attackRange = battleSystem.GetAttackRange(unitId);

                // 高亮攻击范围
                HighlightCells(attackRangeList, HighlightType.Attack);
            }
        }

        public void ShowPath(List<Vector2Int> path)
        {
            HighlightCells(path, HighlightType.Path);
        }

        public void ShowUnitInfo(int unitId, Vector2 screenPosition)
        {
            var unitSystem = GameManager.Instance.SystemManager.GetSystem<UnitManagementSystem>();
            var unit       = unitSystem.GetUnit(unitId);

            if (unit != null)
            {
                var statsComp = unit.GetComponent<UnitStatsComponent>();

                if (statsComp != null)
                {
                    // 构建单位统计信息字符串
                    string stats = $"类型: {statsComp.UnitType}\n"                             +
                                   $"生命值: {statsComp.CurrentHealth}/{statsComp.MaxHealth}\n" +
                                   $"能量: {statsComp.CurrentEnergy}/{statsComp.MaxEnergy}\n"  +
                                   $"移动力: {statsComp.MovementRange}\n"                       +
                                   $"护甲: {statsComp.BaseArmor}";

                    // 添加武器信息
                    var weapons = unit.GetComponents<WeaponComponent>();
                    if (weapons.Count > 0)
                    {
                        stats += "\n\n武器:";
                        foreach (var weapon in weapons)
                        {
                            stats += $"\n- {weapon.WeaponName} ({weapon.BaseDamage} 伤害, {weapon.Range} 范围)";
                        }
                    }

                    // 使用UI Toolkit更新单位信息面板
                    UpdateUnitInfoPanel(statsComp.UnitName, stats);

                    // 定位面板到屏幕位置
                    if (_unitInfoContainer != null)
                    {
                        _unitInfoContainer.style.left = screenPosition.x + 20;
                        _unitInfoContainer.style.top  = screenPosition.y - 100;
                    }
                }
            }
        }

        public void ShowBaseInfo(int baseId, Vector2 screenPosition)
        {
            var baseSystem = GameManager.Instance.SystemManager.GetSystem<BaseManagementSystem>();
            var baseEntity = _entityManager.GetEntity(baseId);

            if (baseEntity != null && baseEntity.HasComponent<BaseComponent>())
            {
                var baseComp = baseEntity.GetComponent<BaseComponent>();

                string stats = $"Type: {baseComp.BaseType}\n"                   +
                               $"Level: {baseComp.Level}/{baseComp.MaxLevel}\n" +
                               $"HP: {baseComp.Health}/{baseComp.MaxHealth}\n"  +
                               $"Status: {(baseComp.IsOperational ? "Operational" : "Offline")}";

                // Add resource production info
                stats += "\n\nProduction:";
                stats += $"\nMoney: +{baseComp.MoneyProduction}";

                foreach (var resource in baseComp.ResourceProduction)
                {
                    if (resource.Value > 0)
                    {
                        stats += $"\n{resource.Key}: +{resource.Value}";
                    }
                }

                // Add facilities info
                if (baseComp.Facilities.Count > 0)
                {
                    stats += "\n\nFacilities:";
                    foreach (var facility in baseComp.Facilities)
                    {
                        stats += $"\n- {facility.FacilityName}";
                    }
                }

                // 使用UI Toolkit更新基地信息面板
                UpdateBaseInfoPanel(baseComp.BaseName, stats);

                Debug.Log($"_baseInfoContainer name: {_baseInfoContainer.name}");
                Debug.Log($"_baseInfoContainer classList: {string.Join(", ", _baseInfoContainer.GetClasses())}");
                Debug.Log($"_baseInfoContainer styleSheets non-null: {_baseInfoContainer.styleSheets != null}");
                if (_baseInfoContainer.styleSheets != null)
                {
                    Debug.Log($"_baseInfoContainer styleSheet count: {_baseInfoContainer.styleSheets.count}");
                    for(int i=0; i < _baseInfoContainer.styleSheets.count; ++i)
                    {
                        Debug.Log($"Stylesheet {i}: {_baseInfoContainer.styleSheets[i]?.name}"); // 可能会是 null
                    }
                }
                var resolvedStyle = _baseInfoContainer.resolvedStyle;
                Debug.Log($"_baseInfoContainer resolved background: {resolvedStyle.backgroundColor}");
                
                // 定位面板到屏幕位置
                if (_baseInfoContainer != null)
                {
                    
                    _baseInfoContainer.style.position        = Position.Absolute;
                    _baseInfoContainer.style.left            = screenPosition.x + 20;
                    _baseInfoContainer.style.top             = screenPosition.y - 100;
                } 
            }
        }

        public void ShowTerrainInfo(Vector2Int position, Vector2 screenPosition)
        {
            ShowTooltip($"Terrain at {position}", screenPosition);
        }

        public void ShowResourceInfo(Dictionary<ResourceType, int> resources)
        {
            int money = 0;
            int alloy = 0;

            if (resources.ContainsKey(ResourceType.Money))
                money = resources[ResourceType.Money];

            if (resources.ContainsKey(ResourceType.StandardAlloy))
                alloy = resources[ResourceType.StandardAlloy];

            UpdateResourceDisplay(money, alloy);
        }

        public void ShowTurnInfo(int turnNumber, TurnPhase phase)
        {
            string phaseName = "";

            // 将TurnPhase枚举转换为可读的字符串
            switch (phase)
            {
                case TurnPhase.Strategy:
                    phaseName = "战略阶段";
                    break;
                case TurnPhase.Action:
                    phaseName = "行动阶段";
                    break;
                case TurnPhase.Event:
                    phaseName = "事件阶段";
                    break;
                case TurnPhase.Enemy:
                    phaseName = "敌人阶段";
                    break;
                case TurnPhase.Settlement:
                    phaseName = "结算阶段";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(phase), phase, null);
            }

            UpdateTurnInfo(turnNumber, phaseName);
        }

        public void ShowActionMenu(int entityId, Vector2 screenPosition, List<ActionMenuItem> actions)
        {
            if (_actionMenuContainer == null) return;
            _actionMenuContainer.Clear(); // 清除之前的菜单项

            foreach (var action in actions)
            {
                Button actionButton = new Button();
                actionButton.text = action.Label;
                actionButton.SetEnabled(action.IsEnabled);

                // 添加点击事件
                actionButton.clicked += () =>
                {
                    action.OnSelected?.Invoke();
                    SetElementDisplay(_actionMenuContainer, false);
                };

                // 添加工具提示
                if (!string.IsNullOrEmpty(action.TooltipText))
                {
                    actionButton.RegisterCallback<MouseEnterEvent>((evt) =>
                    {
                        ShowTooltip(
                            action.TooltipText, evt.mousePosition);
                    });

                    actionButton.RegisterCallback<MouseLeaveEvent>((evt) => { HideTooltip(); });
                }

                _actionMenuContainer.Add(actionButton);
            }

            // 定位菜单到屏幕位置
            //_actionMenuContainer.style.left = screenPosition.x;
            //_actionMenuContainer.style.top  = screenPosition.y;
            SetElementDisplay(_actionMenuContainer, true);
        }

        public void ShowConfirmationDialog(string message, Action onConfirm, Action onCancel)
        {
            // 使用已有的ShowConfirmationDialog方法，但调整参数
            ShowConfirmationDialog("确认", message, onConfirm, onCancel);
        }

        public void ShowNotification(string message, NotificationType type, float duration = 3)
        {
            // 根据通知类型设置样式
            string styleClass = "notification";

            switch (type)
            {
                case NotificationType.Success:
                    styleClass = "notification--success";
                    break;
                case NotificationType.Warning:
                    styleClass = "notification--warning";
                    break;
                case NotificationType.Error:
                    styleClass = "notification--error";
                    break;
                case NotificationType.Info:
                default:
                    styleClass = "notification--info";
                    break;
            }

            // 调用现有的ShowNotification方法
            ShowNotification(message, duration);

            // 如果需要，可以在这里添加样式类
            if (_notificationArea != null && _notificationArea.childCount > 0)
            {
                var lastNotification = _notificationArea[_notificationArea.childCount - 1];
                lastNotification.AddToClassList(styleClass);
            }
        }

        public void ShowNotification(string message, float duration = 3f)
        {
            if (_notificationArea == null || notificationItemTemplate == null) return;

            VisualElement notificationVE = notificationItemTemplate.Instantiate();
            Label messageLabel =
                notificationVE.Q<Label>(
                    "notification-text"); // Assuming NotificationItem.uxml has a label with this name
            if (messageLabel != null) messageLabel.text = message;

            _notificationArea.Add(notificationVE);

            // Auto-hide after duration
            notificationVE.schedule.Execute(() =>
            {
                if (notificationVE != null && notificationVE.parent == _notificationArea)
                    _notificationArea.Remove(notificationVE);
            }).ExecuteLater((long)(duration * 1000));
        }

        public void ShowConfirmationDialog(string title, string message, Action onConfirm, Action onCancel = null)
        {
            if (_dialogContainer == null || confirmationDialogTemplate == null) return;

            _dialogContainer.Clear(); // Clear previous dialog
            VisualElement dialogInstance = confirmationDialogTemplate.Instantiate();
            _dialogContainer.Add(dialogInstance);

            _dialogTitleLabel    = dialogInstance.Q<Label>("dialog-title");
            _dialogMessageLabel  = dialogInstance.Q<Label>("dialog-message");
            _dialogConfirmButton = dialogInstance.Q<Button>("confirm-button");
            _dialogCancelButton  = dialogInstance.Q<Button>("cancel-button");

            if (_dialogTitleLabel   != null) _dialogTitleLabel.text   = title;
            if (_dialogMessageLabel != null) _dialogMessageLabel.text = message;

            _currentDialogConfirmAction = onConfirm;
            _currentDialogCancelAction  = onCancel;

            if (_dialogConfirmButton != null)
            {
                _dialogConfirmButton.clicked -= HandleDialogConfirm;
                _dialogConfirmButton.clicked += HandleDialogConfirm;
            }

            if (_dialogCancelButton != null)
            {
                _dialogCancelButton.clicked -= HandleDialogCancel;
                _dialogCancelButton.clicked += HandleDialogCancel;
            }

            SetElementDisplay(_dialogContainer, true);
        }

        private void HandleDialogConfirm()
        {
            _currentDialogConfirmAction?.Invoke();
            HideConfirmationDialog();
        }

        private void HandleDialogCancel()
        {
            _currentDialogCancelAction?.Invoke();
            HideConfirmationDialog();
        }

        public void HideConfirmationDialog()
        {
            SetElementDisplay(_dialogContainer, false);
        }

        public void ShowTooltip(string text, Vector2 screenPosition)
        {
            if (_tooltipElement == null || _tooltipLabel == null) return;
            _tooltipLabel.text = text;
            // Position tooltip (similar to context menu, needs robust positioning)
            _tooltipElement.style.left = screenPosition.x + 10; // Offset slightly
            _tooltipElement.style.top  = screenPosition.y + 10;
            SetElementDisplay(_tooltipElement, true);
        }

        public void HideTooltip()
        {
            SetElementDisplay(_tooltipElement, false);
        }

        public void PlaySelectionEffect(int entityId)
        {

        }

        public void PlayActionEffect(ActionType action, Vector2Int position)
        {

        }

        public void ShowPanel(PanelType panelType)
        {
            // 将PanelType转换为UIPanelType
            UIPanelType uiPanelType = ConvertPanelType(panelType);

            // 调用现有的ShowPanel方法
            ShowPanel(uiPanelType);
        }

        public void HidePanel(PanelType panelType)
        {
            // 将PanelType转换为UIPanelType
            UIPanelType uiPanelType = ConvertPanelType(panelType);

            // 调用现有的HidePanel方法
            HidePanel(uiPanelType);
        }

        public void TogglePanel(PanelType panelType)
        {
            // 获取面板元素
            VisualElement panel = GetPanelByType(panelType);

            if (panel != null)
            {
                bool isVisible = panel.style.display != DisplayStyle.None;

                if (isVisible)
                    HidePanel(panelType);
                else
                    ShowPanel(panelType);
            }
        }

// 辅助方法：将PanelType转换为UIPanelType
        private UIPanelType ConvertPanelType(PanelType panelType)
        {
            switch (panelType)
            {
                case PanelType.MainMenu:
                    return UIPanelType.MainMenu;
                case PanelType.GameUI:
                    return UIPanelType.GameHUD;
                case PanelType.PauseMenu:
                    return UIPanelType.PauseMenu;
                case PanelType.UnitInfo:
                    return UIPanelType.UnitInfo;
                case PanelType.BaseInfo:
                    return UIPanelType.BaseInfo;
                case PanelType.CommandCenter:
                    return UIPanelType.CommandCenter;
                default:
                    return UIPanelType.GameHUD;
            }
        }

// 辅助方法：根据PanelType获取面板元素
        private VisualElement GetPanelByType(PanelType panelType)
        {
            switch (panelType)
            {
                case PanelType.MainMenu:
                    return _rootVisualElement;
                case PanelType.GameUI:
                    return _gameHudContainer;
                case PanelType.PauseMenu:
                    return _pauseMenuContainer;
                case PanelType.UnitInfo:
                    return _unitInfoContainer;
                case PanelType.BaseInfo:
                    return _baseInfoContainer;
                case PanelType.CommandCenter:
                    return _commandCenterContainer;
                case PanelType.ResourceOverview:
                    return _resourceOverview;
                case PanelType.TurnInfo:
                    return _turnInfoLabel?.parent;
                case PanelType.MiniMap:
                    return _minimapPanel;
                case PanelType.ContextMenu:
                    return _contextMenuContainer;
                case PanelType.ActionMenu:
                    return _actionMenuContainer;
                case PanelType.Notification:
                    return _notificationArea;
                case PanelType.ConfirmationDialog:
                    return _dialogContainer;
                case PanelType.Tooltip:
                    return _tooltipElement;
                default:
                    return null;
            }
        }

        public void UpdateUnitInfoPanel(string unitName, string stats)
        {
            if (_unitInfoContainer == null) ShowPanel(UIPanelType.UnitInfo); // Ensure panel is loaded
            if (_unitNameText == null && _unitInfoContainer != null)
                _unitNameText = _unitInfoContainer.Q<Label>("unit-name");
            if (_unitStatsText == null && _unitInfoContainer != null)
                _unitStatsText = _unitInfoContainer.Q<Label>("unit-stats");

            if (_unitNameText  != null) _unitNameText.text  = unitName;
            if (_unitStatsText != null) _unitStatsText.text = stats;
            SetElementDisplay(_unitInfoContainer, true);
        }

        public void UpdateBaseInfoPanel(string baseName, string stats)
        {
            if (_baseInfoContainer == null) ShowPanel(UIPanelType.BaseInfo); // Ensure panel is loaded
            if (_baseNameText == null && _baseInfoContainer != null)
                _baseNameText = _baseInfoContainer.Q<Label>("base-name");
            if (_baseStatsText == null && _baseInfoContainer != null)
                _baseStatsText = _baseInfoContainer.Q<Label>("base-stats");
            
            if (_baseNameText  != null) _baseNameText.text  = baseName;
            if (_baseStatsText != null) _baseStatsText.text = stats;
            SetElementDisplay(_baseInfoContainer, true);
        }

        // Helper to set display style (flex for visible, none for hidden)
        private void SetElementDisplay(VisualElement element, bool isVisible)
        {
            if (element != null)
            {
                element.style.display = isVisible ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
    }
}
