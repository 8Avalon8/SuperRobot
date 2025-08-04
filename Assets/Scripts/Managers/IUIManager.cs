using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SuperRobot
{
    public interface IUIManager
    {
        // Base UI Management
        void Initialize();
        void ShowMainMenu();
        void ShowGameUI();
        void ShowPauseMenu();
        void HideAllUI();

        // Context-Sensitive UI
        void ShowContextMenu(Vector2 screenPosition, List<ContextMenuItem> menuItems);
        void HideContextMenu();

        // Selection & Highlighting
        void SelectEntity(int entityId);
        void DeselectEntity();
        void HighlightCells(List<Vector2Int> cells, HighlightType type);
        void ClearHighlights(HighlightType type = HighlightType.All);
        void ShowMovementRange(int unitId);
        void ShowAttackRange(int unitId);
        void ShowPath(List<Vector2Int> path);

        // Information Display
        void ShowUnitInfo(int unitId, Vector2 screenPosition);
        void ShowBaseInfo(int baseId, Vector2 screenPosition);
        void ShowTerrainInfo(Vector2Int position, Vector2 screenPosition);
        void ShowResourceInfo(Dictionary<ResourceType, int> resources);
        void ShowTurnInfo(int turnNumber, TurnPhase phase);

        // Command Selection
        void ShowActionMenu(int entityId, Vector2 screenPosition, List<ActionMenuItem> actions);
        void ShowConfirmationDialog(string message, Action onConfirm, Action onCancel);

        // Notifications
        void ShowNotification(string message, NotificationType type, float duration = 3f);
        void ShowTooltip(string text, Vector2 position);
        void HideTooltip();

        // Action Feedback
        void PlaySelectionEffect(int entityId);
        void PlayActionEffect(ActionType action, Vector2Int position);

        // Panel Management
        void ShowPanel(PanelType panelType);
        void HidePanel(PanelType panelType);
        void TogglePanel(PanelType panelType);
    }

    public enum HighlightType
    {
        Selection,
        Movement,
        Attack,
        Path,
        Terrain,
        All
    }

    public enum NotificationType
    {
        Info,
        Warning,
        Error,
        Success
    }

    public enum ActionType
    {
        Move,
        Attack,
        Build,
        Research,
        Upgrade,
        Repair,
        SpecialAbility,
        Cancel
    }

    public enum PanelType
    {
        UnitInfo,
        BaseInfo,
        Research,
        Production,
        PilotManagement,
        ResourceOverview,
        TurnInfo,
        MiniMap,
        CommandCenter,
        PauseMenu,
        MainMenu,
        GameUI,
        ContextMenu,
        ActionMenu,
        Notification,
        ConfirmationDialog,
        Tooltip,
    }

    public class ContextMenuItem
    {
        public string Label { get; set; }
        public Action OnSelected { get; set; }
        public Sprite Icon { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string TooltipText { get; set; }
    }

    public class ActionMenuItem
    {
        public string Label { get; set; }
        public ActionType ActionType { get; set; }
        public Action OnSelected { get; set; }
        public Sprite Icon { get; set; }
        public bool IsEnabled { get; set; } = true;
        public string TooltipText { get; set; }
        public List<ActionMenuItem> SubActions { get; set; } // For nested menus
    }
}
