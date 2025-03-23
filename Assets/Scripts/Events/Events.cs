using System;
using System.Collections.Generic;
using UnityEngine;

namespace SuperRobot
{
    // 系统事件示例
    public struct GameStateChangedEvent
    {
        public GameState PreviousState;
        public GameState CurrentState;
    }

    public struct TurnStartedEvent
    {
        public int TurnNumber;
        public TurnPhase Phase;
    }

    public struct ResourceUpdatedEvent
    {
        public ResourceType ResourceType;
        public int PreviousAmount;
        public int CurrentAmount;
    }

    public struct UnitMovedEvent
    {
        public int UnitId;
        public Vector2Int FromPosition;
        public Vector2Int ToPosition;
    }

    public struct BattleStartedEvent
    {
        public string BattleId;
        public string MapId;
        public List<int> PlayerUnitIds;
        public List<int> EnemyUnitIds;
    }

    public struct GameStartedEvent
    {
    }

    // TurnPhaseChangedEvent
    public struct TurnPhaseChangedEvent
    {
        public TurnPhase PreviousPhase;
        public TurnPhase NewPhase;
    }

    // AIBaseDestroyedEvent
    public struct AIBaseDestroyedEvent
    {
        public int BaseId;
    }

    // AIUnitDestroyedEvent
    public struct AIUnitDestroyedEvent
    {
        public int UnitId;
    }

    // UnitDestroyedEvent
    public struct UnitDestroyedEvent
    {
        public int UnitId;
        public Vector2Int Position;
        public UnitType UnitType;
        public UnitStatsComponent Stats;
    }



    // UnitProductionCompletedEvent
    public struct UnitProductionCompletedEvent
    {
        public string UnitTemplateId;
        public int BaseId;
        public Vector2Int Position;
    }

    // FacilityConstructionCompletedEvent
    public struct FacilityConstructionCompletedEvent
    {
        public int BaseId;
        public string FacilityId;
    }

    // WeaponProductionCompletedEvent
    public struct WeaponProductionCompletedEvent
    {
        public int BaseId;
        public string WeaponId;
    }

    //UpgradeCompletedEvent
    public struct UpgradeCompletedEvent
    {
        public int BaseId;
        public string UpgradeId;
    }

    // BaseConstructionCompletedEvent
    public struct BaseConstructionCompletedEvent
    {
        public int BaseId;
        public string BaseName;
        public BaseType BaseType;
        public Vector2Int Position;
    }

    // BaseConstructionRequestEvent
    public struct BaseConstructionRequestEvent
    {
        public int BaseId;
        public string BaseName;
        public Vector2Int Position;
        public BaseType BaseType;
    }

    // BaseFacilityRequestEvent
    public struct BaseFacilityRequestEvent
    {
        public int BaseId;
        public bool IsAdding;
        public string FacilityId;
        public string FacilityName;
        public FacilityType FacilityType;
        public Dictionary<ResourceType, int> ResourceCost;
    }

    // BaseUpgradeRequestEvent
    public struct BaseUpgradeRequestEvent
    {
        public int BaseId;
        public string UpgradeId;
    }

    // UnitSelectedEvent
    public struct UnitSelectedEvent
    {
        public int UnitId;
    }

    // BaseSelectedEvent
    public struct BaseSelectedEvent
    {
        public int BaseId;
    }

    // TerrainSelectedEvent
    public struct TerrainSelectedEvent
    {
        public Vector2Int GridPosition;
    }

    // BattleEndedEvent
    public struct BattleEndedEvent
    {
        public string BattleId;
        public bool Victory;
    }

    // EventActivatedEvent
    public struct EventActivatedEvent
    {
        public string EventId;
        public string Title;
        public string Description;
        public List<string> Choices;
        public Vector2Int Location;
    }

    // EventChoiceAppliedEvent
    public struct EventChoiceAppliedEvent
    {
        public string EventId;
        public int ChoiceIndex;
        public string ResultMessage;
    }

    // TechResearchCompletedEvent
    public struct TechResearchCompletedEvent
    {
        public string TechId;
        public string TechName;
    }

    // UIStateChangedEvent
    public struct UIStateChangedEvent
    {
        public UISystem.UIState PreviousState;
        public UISystem.UIState CurrentState;
    }
    //EventChoiceSelectedEvent
    public struct EventChoiceSelectedEvent
    {
        public string EventId;
        public int ChoiceIndex;
    }
    //EventTimeoutEvent
    public struct EventTimeoutEvent
    {
        public string EventId;
    }

    // EndTurnRequestEvent
    public struct EndTurnRequestEvent
    {
    }

    // GameSpeedChangeRequestEvent
    public struct GameSpeedChangeRequestEvent
    {
        public bool SpeedUp;
    }

    // CameraZoomRequestEvent
    public struct CameraZoomRequestEvent
    {
        public bool ZoomIn;
    }

    // CameraMoveRequestEvent
    public struct CameraMoveRequestEvent
    {
        public Vector2 Delta;
    }

    // CameraRotateRequestEvent
    public struct CameraRotateRequestEvent
    {
        public Vector2 Delta;
    }

    // AITurnEndedEvent
    public struct AITurnEndedEvent
    {
    }

    // AIBaseConstructedEvent
    public struct AIBaseConstructedEvent
    {
        public int BaseId;
        public Vector2Int Position;
        public BaseType BaseType;
    }
    // AIAttackInitiatedEvent
    public struct AIAttackInitiatedEvent
    {
        public int TargetBaseId;
        public Vector2Int TargetPosition;
        public List<string> AttackingUnitIds;
    }

    // AIDefenseInitiatedEvent
    public struct AIDefenseInitiatedEvent
    {
        public int BaseToDefendId;
        public Vector2Int DefensePosition;
        public List<string> DefendingUnitIds;
    }
    // AIFacilityConstructedEvent
    public struct AIFacilityConstructedEvent
    {
        public int BaseId;
        public string FacilityId;
        public FacilityType FacilityType;
    }
    // AIResearchStartedEvent
    public struct AIResearchStartedEvent
    {
        public string TechId;
        public int EstimatedTurns;
    }
    // AIProductionStartedEvent
    public struct AIProductionStartedEvent
    {
        public int BaseId;
        public string ItemId;
        public ProductionItem.ProductionType ItemType;
        public int TurnsRequired;
    }
    // AIUnitMovedEvent
    public struct AIUnitMovedEvent
    {
        public int UnitId;
        public Vector2Int FromPosition;
        public Vector2Int ToPosition;
    }
    // PilotRecruiteEvent
    public struct PilotRecruiteEvent
    {
        public string PilotName;
        public Dictionary<string, int> InitialStats;
    }
    // PilotAssignmentRequestEvent
    public struct PilotAssignmentRequestEvent
    {
        public int PilotId;
        public int UnitId;
        public bool Assign;
    }
    // PilotTrainRequestEvent
    public struct PilotTrainRequestEvent
    {
        public int PilotId;
        public string AttributeToTrain;
    }

    // TurnEndedEvent
    public struct TurnEndedEvent
    {
        public List<BattleData> CompletedBattles;
    }
    // PilotRecruitedEvent
    public struct PilotRecruitedEvent
    {
        public string PilotName;
        public Dictionary<string, int> Stats;
    }

    // UnitCreatedEvent
    public struct UnitCreatedEvent
    {
        public string UnitId;
        public string UnitTemplateId;
        public Vector2Int Position;
    }

    // AddToProductionQueueRequestEvent
    public struct AddToProductionQueueRequestEvent
    {
        public int BaseId;
        public string ItemId;
        public ProductionItem.ProductionType ProductionType;
    }
    // RemoveFromProductionQueueRequest
    public struct RemoveFromProductionQueueRequestEvent
    {
        public int BaseId;
        public int QueueIndex;
    }
    // ProductionPriorityChangeRequestEvent
    public struct ProductionPriorityChangeRequestEvent
    {
        public int BaseId;
        public int QueueIndex;
        public int NewPriority;
    }
    // BattleStartEvent
    public struct BattleStartEvent
    {
        public string BattleId;
        public string MapId;
        // MapData
        public Dictionary<Vector2Int, TerrainType> TerrainMap;
        public List<int> PlayerUnits;
        public List<int> EnemyUnits;
        public int CurrentTurn;
        public BattleSystem.BattlePhase CurrentPhase;
    }
    // BattleEndEvent
    public struct BattleEndEvent
    {
        public string BattleId;
        public bool Victory;
        public Dictionary<ResourceType, int> Reward;
    }
    // UnitActionRequestEvent
    public struct UnitActionRequestEvent
    {
        public int UnitId;
        public Vector2Int TargetPosition;
        public BattleSystem.BattleActionType ActionType;
    }
    // UnitActionCompletedEvent
    public struct UnitActionCompletedEvent
    {
        public int UnitId;
        public BattleSystem.BattleActionType ActionType;
        public Vector2Int TargetPosition;
    }

    // ShowDeploymentUIEvent
    public struct ShowDeploymentUIEvent
    {
        public List<int> AvailableUnits;
        public List<Vector2Int> DeploymentZones;
    }
    //PlayBattleAnimationEvent
    public struct PlayBattleAnimationEvent
    {
        public BattleSystem.BattleAction BattleAction;
        public Action OnComplete;
    }

    // BattleTurnStartEvent
    public struct BattleTurnStartEvent
    {
        public bool IsPlayerTurn;
        public int TurnNumber;
    }

    // TechRewardAppliedEvent
    public struct TechRewardAppliedEvent
    {
        public string TechId;
        public TechRewardType RewardType;
    }

    // 单位移动请求事件
    public struct UnitMoveRequestEvent
    {
        public int UnitId;
        public Vector2Int TargetPosition;
    }

    // 单位移动开始事件
    public struct UnitMovementStartedEvent
    {
        public int UnitId;
        public List<Vector2Int> Path;
    }

    // 单位位置更新事件
    public struct UnitPositionUpdatedEvent
    {
        public int UnitId;
        public Vector2Int Position;
    }

    // 单位移动完成事件
    public struct UnitMovementCompletedEvent
    {
        public int        UnitId;
        public Vector2Int FinalPosition;
    }

    // 单位移动取消事件
    public struct UnitMovementCancelledEvent
    {
        public int UnitId;
    }

    // 单位路径未找到事件
    public struct UnitPathNotFoundEvent
    {
        public int UnitId;
    }

}
