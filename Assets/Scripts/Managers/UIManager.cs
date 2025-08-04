using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SuperRobot
{
    public class UIManager : MonoBehaviour, IUIManager
    {
        private static UIManager _instance;
        public static UIManager Instance => _instance;

        [Header("UI Elements")]
        [SerializeField] private GameObject m_MainMenuPanel;
        [SerializeField] private GameObject m_GameUIPanel;
        [SerializeField] private GameObject m_PauseMenuPanel;
        [SerializeField] private GameObject m_UnitInfoPanel;
        [SerializeField] private GameObject m_BaseInfoPanel;
        [SerializeField] private GameObject m_ContextMenuPanel;
        [SerializeField] private GameObject m_ActionMenuPanel;
        [SerializeField] private GameObject m_NotificationPanel;
        [SerializeField] private GameObject m_ConfirmationDialogPanel;
        [SerializeField] private GameObject m_TooltipPanel;
        [SerializeField] private GameObject m_ResourcePanel;
        [SerializeField] private GameObject m_TurnInfoPanel;
        [SerializeField] private GameObject m_MiniMapPanel;
        [SerializeField] private GameObject m_CommandCenterPanel;

        [Header("UI Prefabs")]
        [SerializeField] private GameObject m_ContextMenuItemPrefab;
        [SerializeField] private GameObject m_ActionMenuItemPrefab;
        [SerializeField] private GameObject m_NotificationPrefab;

        // References to frequently accessed components
        private TMP_Text m_UnitNameText;
        private TMP_Text m_UnitStatsText;
        private TMP_Text m_BaseNameText;
        private TMP_Text m_BaseStatsText;
        private TMP_Text m_TooltipText;
        private TMP_Text m_TurnInfoText;

        // Current selection state
        private int _selectedEntityId = -1;
        private Dictionary<HighlightType, List<Vector2Int>> _highlightedCells = new Dictionary<HighlightType, List<Vector2Int>>();
        
        // Animation parameters
        private float _notificationDuration = 3f;
        private float _tooltipFadeTime = 0.2f;
        
        // External dependencies
        private IMapManager _mapManager => GameManager.Instance.MapManager;
        private EntityManager _entityManager => GameManager.Instance.EntityManager;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            Initialize();
        }

        #region IUIManager Implementation

        public void Initialize()
        {
            // Initialize highlight cell dictionaries
            foreach (HighlightType type in Enum.GetValues(typeof(HighlightType)))
            {
                if (type != HighlightType.All)
                {
                    _highlightedCells[type] = new List<Vector2Int>();
                }
            }

            // Get references to frequently accessed components
            if (m_UnitInfoPanel != null)
            {
                m_UnitNameText = m_UnitInfoPanel.transform.Find("UnitName").GetComponent<TMP_Text>();
                m_UnitStatsText = m_UnitInfoPanel.transform.Find("UnitStats").GetComponent<TMP_Text>();
            }

            if (m_BaseInfoPanel != null)
            {
                m_BaseNameText = m_BaseInfoPanel.transform.Find("BaseName").GetComponent<TMP_Text>();
                m_BaseStatsText = m_BaseInfoPanel.transform.Find("BaseStats").GetComponent<TMP_Text>();
            }

            if (m_TooltipPanel != null)
            {
                m_TooltipText = m_TooltipPanel.GetComponentInChildren<TMP_Text>();
            }

            if (m_TurnInfoPanel != null)
            {
                m_TurnInfoText = m_TurnInfoPanel.GetComponentInChildren<TMP_Text>();
            }

            // Hide all UI panels initially
            HideAllUI();
        }

        public void ShowMainMenu()
        {
            HideAllUI();
            if (m_MainMenuPanel != null)
                m_MainMenuPanel.SetActive(true);
        }

        public void ShowGameUI()
        {
            HideAllUI();
            if (m_GameUIPanel != null)
                m_GameUIPanel.SetActive(true);
            
            // Show default game panels
            ShowPanel(PanelType.ResourceOverview);
            ShowPanel(PanelType.TurnInfo);
            ShowPanel(PanelType.MiniMap);
        }

        public void ShowPauseMenu()
        {
            if (m_PauseMenuPanel != null)
                m_PauseMenuPanel.SetActive(true);
        }

        public void HideAllUI()
        {
            // Hide all main panels
            if (m_MainMenuPanel != null) m_MainMenuPanel.SetActive(false);
            if (m_GameUIPanel != null) m_GameUIPanel.SetActive(false);
            if (m_PauseMenuPanel != null) m_PauseMenuPanel.SetActive(false);
            
            // Hide all information panels
            if (m_UnitInfoPanel != null) m_UnitInfoPanel.SetActive(false);
            if (m_BaseInfoPanel != null) m_BaseInfoPanel.SetActive(false);
            if (m_ContextMenuPanel != null) m_ContextMenuPanel.SetActive(false);
            if (m_ActionMenuPanel != null) m_ActionMenuPanel.SetActive(false);
            if (m_NotificationPanel != null) m_NotificationPanel.SetActive(false);
            if (m_ConfirmationDialogPanel != null) m_ConfirmationDialogPanel.SetActive(false);
            if (m_TooltipPanel != null) m_TooltipPanel.SetActive(false);
            
            // Hide game panels
            if (m_ResourcePanel != null) m_ResourcePanel.SetActive(false);
            if (m_TurnInfoPanel != null) m_TurnInfoPanel.SetActive(false);
            if (m_MiniMapPanel != null) m_MiniMapPanel.SetActive(false);
            if (m_CommandCenterPanel != null) m_CommandCenterPanel.SetActive(false);
        }

        public void ShowContextMenu(Vector2 screenPosition, List<ContextMenuItem> menuItems)
        {
            // Clear previous items
            if (m_ContextMenuPanel != null)
            {
                var root = m_ContextMenuPanel.transform.Find("Root");
                if (root != null)
                {
                    foreach (Transform child in root)
                    {
                        Destroy(child.gameObject);
                    }
                }

                // Create new items
                float yOffset = 0;
                foreach (var item in menuItems)
                {
                    GameObject menuItemObj = Instantiate(m_ContextMenuItemPrefab, root);
                    Button button = menuItemObj.GetComponent<Button>();
                    TMP_Text text = menuItemObj.GetComponentInChildren<TMP_Text>();
                    Image icon = menuItemObj.transform.Find("Icon")?.GetComponent<Image>();

                    // Set text, icon, and callback
                    text.text = item.Label;
                    if (icon != null && item.Icon != null)
                        icon.sprite = item.Icon;

                    // Add click event
                    button.onClick.RemoveAllListeners();
                    button.onClick.AddListener(() => {
                        item.OnSelected?.Invoke();
                        HideContextMenu();
                    });

                    // Set position
                    RectTransform rect = menuItemObj.GetComponent<RectTransform>();
                    rect.anchoredPosition = new Vector2(0, yOffset);
                    yOffset -= rect.sizeDelta.y;

                    // Set enabled state
                    button.interactable = item.IsEnabled;

                    // Add tooltip event
                    if (!string.IsNullOrEmpty(item.TooltipText))
                    {
                        EventTrigger trigger = menuItemObj.AddComponent<EventTrigger>();
                        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
                        enterEntry.eventID = EventTriggerType.PointerEnter;
                        enterEntry.callback.AddListener((data) => {
                            ShowTooltip(item.TooltipText, Input.mousePosition);
                        });
                        trigger.triggers.Add(enterEntry);

                        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
                        exitEntry.eventID = EventTriggerType.PointerExit;
                        exitEntry.callback.AddListener((data) => {
                            HideTooltip();
                        });
                        trigger.triggers.Add(exitEntry);
                    }
                }

                // Position the menu
                RectTransform menuRect = m_ContextMenuPanel.GetComponent<RectTransform>();
                menuRect.position = screenPosition;

                // Ensure menu stays within screen bounds
                Vector2 size = menuRect.sizeDelta;
                Vector2 position = menuRect.position;
                
                if (position.x + size.x > Screen.width)
                    position.x = Screen.width - size.x;
                if (position.y + size.y > Screen.height)
                    position.y = Screen.height - size.y;
                if (position.y < size.y)
                    position.y = size.y;
                
                menuRect.position = position;

                // Show the menu
                m_ContextMenuPanel.SetActive(true);
            }
        }

        public void HideContextMenu()
        {
            if (m_ContextMenuPanel != null)
                m_ContextMenuPanel.SetActive(false);
        }

        public void SelectEntity(int entityId)
        {
            // Deselect previous entity
            DeselectEntity();
            
            // Set new selection
            _selectedEntityId = entityId;
            
            // Get entity
            var entity = _entityManager.GetEntity(entityId);
            if (entity == null)
                return;
                
            // Determine entity type and handle accordingly
            if (entity.HasComponent<UnitStatsComponent>())
            {
                // Handle unit selection
                var statsComp = entity.GetComponent<UnitStatsComponent>();
                var posComp = entity.GetComponent<PositionComponent>();
                
                if (posComp != null)
                {
                    // Highlight selected unit position
                    HighlightCells(new List<Vector2Int> { posComp.Position }, HighlightType.Selection);
                    
                    // Show unit info
                    ShowUnitInfo(entityId, Input.mousePosition);
                    
                    // Play selection effect
                    PlaySelectionEffect(entityId);
                }
            }
            else if (entity.HasComponent<BaseComponent>())
            {
                // Handle base selection
                var baseComp = entity.GetComponent<BaseComponent>();
                var posComp = entity.GetComponent<PositionComponent>();
                
                if (posComp != null)
                {
                    // Highlight selected base position
                    HighlightCells(new List<Vector2Int> { posComp.Position }, HighlightType.Selection);
                    
                    // Show base info
                    ShowBaseInfo(entityId, Input.mousePosition);
                    
                    // Play selection effect
                    PlaySelectionEffect(entityId);
                }
            }
            
            // Trigger selection event
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
                // Clear any selection highlights
                ClearHighlights(HighlightType.Selection);
                ClearHighlights(HighlightType.Movement);
                ClearHighlights(HighlightType.Attack);
                ClearHighlights(HighlightType.Path);
                
                // Hide info panels
                if (m_UnitInfoPanel != null) m_UnitInfoPanel.SetActive(false);
                if (m_BaseInfoPanel != null) m_BaseInfoPanel.SetActive(false);
                
                _selectedEntityId = -1;
            }
        }

        public void HighlightCells(List<Vector2Int> cells, HighlightType type)
        {
            // Clear previous highlights of this type
            ClearHighlights(type);
            
            // Store highlighted cells
            _highlightedCells[type] = new List<Vector2Int>(cells);
            
            // Apply highlights based on type
            Color highlightColor = GetHighlightColor(type);
            _mapManager.HighlightCells(cells);
        }

        public void ClearHighlights(HighlightType type = HighlightType.All)
        {
            if (type == HighlightType.All)
            {
                // Clear all highlights
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
                // Clear specific highlight type
                if (_highlightedCells.ContainsKey(type) && _highlightedCells[type].Count > 0)
                {
                    _mapManager.ClearHighlightCells();
                    _highlightedCells[type].Clear();
                }
            }
        }

        public void ShowMovementRange(int unitId)
        {
            var unitSystem = GameManager.Instance.SystemManager.GetSystem<UnitManagementSystem>();
            var unit = unitSystem.GetUnit(unitId);
            
            if (unit != null)
            {
                var statsComp = unit.GetComponent<UnitStatsComponent>();
                var posComp = unit.GetComponent<PositionComponent>();
                
                if (statsComp != null && posComp != null)
                {
                    // Get movement range
                    var moveRange = unitSystem.GetMovementRange(unitId);
                    
                    // Highlight movement range
                    HighlightCells(moveRange, HighlightType.Movement);
                }
            }
        }

        public void ShowAttackRange(int unitId)
        {
            var unitSystem = GameManager.Instance.SystemManager.GetSystem<UnitManagementSystem>();
            var battleSystem = GameManager.Instance.SystemManager.GetSystem<BattleSystem>();
            var unit = unitSystem.GetUnit(unitId);
            
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
            if (m_UnitInfoPanel != null)
            {
                var unitSystem = GameManager.Instance.SystemManager.GetSystem<UnitManagementSystem>();
                var unit = unitSystem.GetUnit(unitId);
                
                if (unit != null)
                {
                    var statsComp = unit.GetComponent<UnitStatsComponent>();
                    
                    if (statsComp != null)
                    {
                        // Set unit name and stats
                        if (m_UnitNameText != null)
                            m_UnitNameText.text = statsComp.UnitName;
                            
                        if (m_UnitStatsText != null)
                        {
                            string stats = $"Type: {statsComp.UnitType}\n" +
                                          $"HP: {statsComp.CurrentHealth}/{statsComp.MaxHealth}\n" +
                                          $"EN: {statsComp.CurrentEnergy}/{statsComp.MaxEnergy}\n" +
                                          $"Move: {statsComp.MovementRange}\n" +
                                          $"Armor: {statsComp.BaseArmor}";
                                          
                            // Add weapon info
                            var weapons = unit.GetComponents<WeaponComponent>();
                            if (weapons.Count > 0)
                            {
                                stats += "\n\nWeapons:";
                                foreach (var weapon in weapons)
                                {
                                    stats += $"\n- {weapon.WeaponName} ({weapon.BaseDamage} dmg, {weapon.Range} range)";
                                }
                            }
                            
                            m_UnitStatsText.text = stats;
                        }
                        
                        // Show panel
                        m_UnitInfoPanel.SetActive(true);
                    }
                }
            }
        }

        public void ShowBaseInfo(int baseId, Vector2 screenPosition)
        {
            if (m_BaseInfoPanel != null)
            {
                var baseSystem = GameManager.Instance.SystemManager.GetSystem<BaseManagementSystem>();
                var baseEntity = baseSystem.GetBase(baseId);
                
                if (baseEntity != null)
                {
                    var baseComp = baseEntity.GetComponent<BaseComponent>();
                    
                    if (baseComp != null)
                    {
                        // Set base name and stats
                        if (m_BaseNameText != null)
                            m_BaseNameText.text = baseComp.BaseName;
                            
                        if (m_BaseStatsText != null)
                        {
                            string stats = $"Type: {baseComp.BaseType}\n" +
                                          $"Level: {baseComp.Level}/{baseComp.MaxLevel}\n" +
                                          $"HP: {baseComp.Health}/{baseComp.MaxHealth}\n" +
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
                            
                            m_BaseStatsText.text = stats;
                        }
                        
                        // Show panel
                        m_BaseInfoPanel.SetActive(true);
                    }
                }
            }
        }

        public void ShowTerrainInfo(Vector2Int position, Vector2 screenPosition)
        {
            // Similar to the other info panels but for terrain
            // This could display terrain type, movement costs, etc.
            ShowTooltip($"Terrain at {position}", screenPosition);
        }

        public void ShowResourceInfo(Dictionary<ResourceType, int> resources)
        {
            if (m_ResourcePanel != null)
            {
                var resourceText = m_ResourcePanel.GetComponentInChildren<TMP_Text>();
                if (resourceText != null)
                {
                    // 格式化资源显示文本
                    var text = "";
                    foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
                    {
                        if (resources.TryGetValue(type, out int amount))
                        {
                            text += $"{type}: {amount}\n";  
                        }
                    }
                    resourceText.text = text;
                    
                    // 显示面板（如果尚未激活）
                    m_ResourcePanel.SetActive(true);
                }
            }
        }

        public void ShowTurnInfo(int turnNumber, TurnPhase phase)
        {
            if (m_TurnInfoPanel != null && m_TurnInfoText != null)
            {
                m_TurnInfoText.text = $"Turn {turnNumber} - {phase}";
                m_TurnInfoPanel.SetActive(true);
            }
        }

        public void ShowActionMenu(int entityId, Vector2 screenPosition, List<ActionMenuItem> actions)
        {
            // Similar to context menu but with hierarchy and action-specific styling
            if (m_ActionMenuPanel != null)
            {
                // Clear previous items
                var root = m_ActionMenuPanel.transform.Find("Root");
                if (root != null)
                {
                    foreach (Transform child in root)
                    {
                        Destroy(child.gameObject);
                    }
                }
                
                // Create action menu items
                CreateActionMenuItems(actions, root, 0);
                
                // Position menu
                RectTransform rect = m_ActionMenuPanel.GetComponent<RectTransform>();
                rect.position = screenPosition;
                
                // Show menu
                m_ActionMenuPanel.SetActive(true);
            }
        }

        private void CreateActionMenuItems(List<ActionMenuItem> actions, Transform parent, float indent)
        {
            float yOffset = 0;
            
            foreach (var action in actions)
            {
                GameObject menuItemObj = Instantiate(m_ActionMenuItemPrefab, parent);
                Button button = menuItemObj.GetComponent<Button>();
                TMP_Text text = menuItemObj.GetComponentInChildren<TMP_Text>();
                Image icon = menuItemObj.transform.Find("Icon")?.GetComponent<Image>();
                
                // Set text, icon, and callback
                text.text = action.Label;
                if (icon != null && action.Icon != null)
                    icon.sprite = action.Icon;
                    
                // Indent submenu items
                RectTransform itemRect = menuItemObj.GetComponent<RectTransform>();
                itemRect.offsetMin = new Vector2(indent, itemRect.offsetMin.y);
                
                // Set position
                itemRect.anchoredPosition = new Vector2(0, yOffset);
                yOffset -= itemRect.sizeDelta.y;
                
                // Check for submenu
                bool hasSubMenu = action.SubActions != null && action.SubActions.Count > 0;
                Transform subMenuIndicator = menuItemObj.transform.Find("SubMenuIndicator");
                if (subMenuIndicator != null)
                    subMenuIndicator.gameObject.SetActive(hasSubMenu);
                
                // Set callback
                button.onClick.AddListener(() => {
                    if (hasSubMenu)
                    {
                        // Show submenu
                        GameObject subMenuObj = new GameObject("SubMenu");
                        subMenuObj.transform.SetParent(menuItemObj.transform);
                        RectTransform subMenuRect = subMenuObj.AddComponent<RectTransform>();
                        subMenuRect.anchoredPosition = new Vector2(itemRect.sizeDelta.x, 0);
                        
                        CreateActionMenuItems(action.SubActions, subMenuObj.transform, 0);
                    }
                    else
                    {
                        // Execute action
                        action.OnSelected?.Invoke();
                        m_ActionMenuPanel.SetActive(false);
                    }
                });
                
                // Set enabled state
                button.interactable = action.IsEnabled;
                
                // Add tooltip
                if (!string.IsNullOrEmpty(action.TooltipText))
                {
                    EventTrigger trigger = menuItemObj.AddComponent<EventTrigger>();
                    EventTrigger.Entry enterEntry = new EventTrigger.Entry();
                    enterEntry.eventID = EventTriggerType.PointerEnter;
                    enterEntry.callback.AddListener((data) => {
                        ShowTooltip(action.TooltipText, Input.mousePosition);
                    });
                    trigger.triggers.Add(enterEntry);
                    
                    EventTrigger.Entry exitEntry = new EventTrigger.Entry();
                    exitEntry.eventID = EventTriggerType.PointerExit;
                    exitEntry.callback.AddListener((data) => {
                        HideTooltip();
                    });
                    trigger.triggers.Add(exitEntry);
                }
            }
        }

        public void ShowConfirmationDialog(string message, Action onConfirm, Action onCancel)
        {
            if (m_ConfirmationDialogPanel != null)
            {
                // Set message
                TMP_Text messageText = m_ConfirmationDialogPanel.transform.Find("Message").GetComponent<TMP_Text>();
                if (messageText != null)
                    messageText.text = message;
                    
                // Set button callbacks
                Button confirmButton = m_ConfirmationDialogPanel.transform.Find("ConfirmButton").GetComponent<Button>();
                Button cancelButton = m_ConfirmationDialogPanel.transform.Find("CancelButton").GetComponent<Button>();
                
                confirmButton.onClick.RemoveAllListeners();
                cancelButton.onClick.RemoveAllListeners();
                
                confirmButton.onClick.AddListener(() => {
                    onConfirm?.Invoke();
                    m_ConfirmationDialogPanel.SetActive(false);
                });
                
                cancelButton.onClick.AddListener(() => {
                    onCancel?.Invoke();
                    m_ConfirmationDialogPanel.SetActive(false);
                });
                
                // Show dialog
                m_ConfirmationDialogPanel.SetActive(true);
            }
        }

        public void ShowNotification(string message, NotificationType type, float duration = 3f)
        {
            if (m_NotificationPanel != null && m_NotificationPrefab != null)
            {
                // Create notification
                GameObject notificationObj = Instantiate(m_NotificationPrefab, m_NotificationPanel.transform);
                TMP_Text text = notificationObj.GetComponentInChildren<TMP_Text>();
                Image background = notificationObj.GetComponent<Image>();
                
                // Set text and style based on type
                text.text = message;
                
                Color backgroundColor = Color.white;
                switch (type)
                {
                    case NotificationType.Info:
                        backgroundColor = new Color(0.2f, 0.2f, 0.8f, 0.8f);
                        break;
                    case NotificationType.Warning:
                        backgroundColor = new Color(0.8f, 0.8f, 0.2f, 0.8f);
                        break;
                    case NotificationType.Error:
                        backgroundColor = new Color(0.8f, 0.2f, 0.2f, 0.8f);
                        break;
                    case NotificationType.Success:
                        backgroundColor = new Color(0.2f, 0.8f, 0.2f, 0.8f);
                        break;
                }
                
                background.color = backgroundColor;
                
                // Position the notification
                RectTransform rect = notificationObj.GetComponent<RectTransform>();
                float height = rect.sizeDelta.y;
                
                // Count existing notifications
                int notificationCount = m_NotificationPanel.transform.childCount - 1; // -1 because we just added one
                rect.anchoredPosition = new Vector2(0, -height * notificationCount);
                
                // Show panel
                m_NotificationPanel.SetActive(true);
                
                // Destroy after duration
                StartCoroutine(DestroyAfterDelay(notificationObj, duration));
            }
        }

        private System.Collections.IEnumerator DestroyAfterDelay(GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            // Fade out
            CanvasGroup group = obj.GetComponent<CanvasGroup>();
            if (group != null)
            {
                float fadeTime = 0.5f;
                float startTime = Time.time;
                
                while (Time.time < startTime + fadeTime)
                {
                    float t = (Time.time - startTime) / fadeTime;
                    group.alpha = 1 - t;
                    yield return null;
                }
            }
            
            // Destroy
            Destroy(obj);
            
            // Hide panel if no more notifications
            if (m_NotificationPanel.transform.childCount == 0)
                m_NotificationPanel.SetActive(false);
        }

        public void ShowTooltip(string text, Vector2 position)
        {
            if (m_TooltipPanel != null && m_TooltipText != null)
            {
                // Set text
                m_TooltipText.text = text;
                
                // Position tooltip
                RectTransform rect = m_TooltipPanel.GetComponent<RectTransform>();
                rect.position = position;
                
                // Ensure tooltip stays within screen bounds
                Canvas.ForceUpdateCanvases(); // Force layout update to get correct size
                Vector2 size = rect.sizeDelta;
                Vector2 pos = rect.position;
                
                // 检查右边界
                if (pos.x + size.x > Screen.width)
                    pos.x = Screen.width - size.x;
                // 检查左边界
                if (pos.x < 0)
                    pos.x = 0;
                // 检查上边界
                if (pos.y + size.y > Screen.height)
                    pos.y = Screen.height - size.y;
                // 检查下边界
                if (pos.y < 0)
                    pos.y = 0;
                
                rect.position = pos;
                
                // Show tooltip
                m_TooltipPanel.SetActive(true);
                
                // Fade in
                CanvasGroup group = m_TooltipPanel.GetComponent<CanvasGroup>();
                if (group != null)
                    StartCoroutine(FadeCanvasGroup(group, 0, 1, _tooltipFadeTime));
            }
        }

        public void HideTooltip()
        {
            if (m_TooltipPanel != null)
            {
                // Fade out
                CanvasGroup group = m_TooltipPanel.GetComponent<CanvasGroup>();
                if (group != null)
                    StartCoroutine(FadeCanvasGroup(group, 1, 0, _tooltipFadeTime, () => m_TooltipPanel.SetActive(false)));
                else
                    m_TooltipPanel.SetActive(false);
            }
        }

        private System.Collections.IEnumerator FadeCanvasGroup(
            CanvasGroup group, float startAlpha, float endAlpha, float duration, Action onComplete = null)
        {
            float startTime = Time.time;
            group.alpha = startAlpha;
            
            while (Time.time < startTime + duration)
            {
                float t = (Time.time - startTime) / duration;
                group.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
                yield return null;
            }
            
            group.alpha = endAlpha;
            onComplete?.Invoke();
        }

        public void PlaySelectionEffect(int entityId)
        {
            // Play selection effect on entity
            // This might involve animations, particles, or other visual effects
        }

        public void PlayActionEffect(ActionType action, Vector2Int position)
        {
            // Play action effect at position
            // This might involve animations, particles, or other visual effects
        }

        public void ShowPanel(PanelType panelType)
        {
            GameObject panel = GetPanelByType(panelType);
            if (panel != null)
                panel.SetActive(true);
        }

        public void HidePanel(PanelType panelType)
        {
            GameObject panel = GetPanelByType(panelType);
            if (panel != null)
                panel.SetActive(false);
        }

        public void TogglePanel(PanelType panelType)
        {
            GameObject panel = GetPanelByType(panelType);
            if (panel != null)
                panel.SetActive(!panel.activeSelf);
        }

        private GameObject GetPanelByType(PanelType panelType)
        {
            switch (panelType)
            {
                case PanelType.UnitInfo:
                    return m_UnitInfoPanel;
                case PanelType.BaseInfo:
                    return m_BaseInfoPanel;
                case PanelType.ResourceOverview:
                    return m_ResourcePanel;
                case PanelType.MiniMap:
                    return m_MiniMapPanel;
                case PanelType.CommandCenter:
                    return m_CommandCenterPanel;
                default:
                    return null;
            }
        }

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
                case HighlightType.Terrain:
                    return new Color(0.5f, 0.5f, 0.5f, 0.5f);
                default:
                    return Color.white;
            }
        }

        #endregion
    }
}