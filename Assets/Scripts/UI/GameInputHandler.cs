using System.Collections.Generic;
using TGS;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SuperRobot
{
    public class GameInputHandler : MonoBehaviour
    {
        private IUIManager    _uiManager     => UIDocManager.Instance;
        private IMapManager   _mapManager    => GameManager.Instance.MapManager;
        private EntityManager _entityManager => GameManager.Instance.EntityManager;
        
        private Camera _mainCamera;
        private int _selectedEntityId = -1;
        private Vector2Int _hoveredCell = new Vector2Int(-1, -1);
        private bool _isDragging = false;
        private Vector2 _dragStartPos;
        
        private void Start()
        {
            _mainCamera = Camera.main;
        }
        
        private void Update()
        {
            // Check if we're over UI elements
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;
                
            // Handle mouse hover
            HandleMouseHover();
            
            // Handle mouse click
            if (Input.GetMouseButtonDown(0))
            {
                HandleLeftClick();
            }
            
            // Handle right click for context menu
            if (Input.GetMouseButtonDown(1))
            {
                HandleRightClick();
            }
            
            // Handle camera movement with middle mouse
            if (Input.GetMouseButtonDown(2))
            {
                _isDragging = true;
                _dragStartPos = Input.mousePosition;
            }
            
            if (Input.GetMouseButtonUp(2))
            {
                _isDragging = false;
            }
            
            if (_isDragging)
            {
                Vector2 delta = (Vector2)Input.mousePosition - _dragStartPos;
                if (delta.magnitude > 0.1f)
                {
                    // Move camera
                    EventManager.Instance.TriggerEvent(new CameraMoveRequestEvent { Delta = delta * 0.1f });
                    _dragStartPos = Input.mousePosition;
                }
            }
            
            // Handle keyboard shortcuts
            HandleKeyboardShortcuts();
        }
        
        private void HandleMouseHover()
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                // Get cell position
                Vector3 worldPos = hit.point;
                Vector2Int cellPos = WorldToGrid(worldPos);
                
                if (cellPos != _hoveredCell)
                {
                    _hoveredCell = cellPos;
                    
                    // Show terrain info
                    string terrainType = _mapManager.GetCellType(cellPos);
                    if (!string.IsNullOrEmpty(terrainType))
                    {
                        _uiManager.ShowTooltip($"Terrain: {terrainType}", Input.mousePosition);
                    }
                    
                    // Check if there's an entity at this position
                    List<GameEntity> entitiesAtPosition = _entityManager.GetEntitiesAtPosition(cellPos);
                    if (entitiesAtPosition.Count > 0)
                    {
                        // For now, just show the first entity's info
                        GameEntity entity = entitiesAtPosition[0];
                        
                        if (entity.HasComponent<UnitStatsComponent>())
                        {
                            var statsComp = entity.GetComponent<UnitStatsComponent>();
                            _uiManager.ShowTooltip($"{statsComp.UnitName}\nHP: {statsComp.CurrentHealth}/{statsComp.MaxHealth}", Input.mousePosition);
                        }
                        else if (entity.HasComponent<BaseComponent>())
                        {
                            var baseComp = entity.GetComponent<BaseComponent>();
                            _uiManager.ShowTooltip($"{baseComp.BaseName}\nType: {baseComp.BaseType}", Input.mousePosition);
                        }
                    }
                }
            }
            else
            {
                if (_hoveredCell != new Vector2Int(-1, -1))
                {
                    _hoveredCell = new Vector2Int(-1, -1);
                    _uiManager.HideTooltip();
                }
            }
        }
        
        private void HandleLeftClick()
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            
            if (Physics.Raycast(ray, out hit))
            {
                // Get cell position
                var cell = TerrainGridSystem.instance.CellGetAtScreenPosition(Input.mousePosition);
                var cellPos = new Vector2Int(cell.row, cell.column);    
                // Check if there's an entity at this position
                List<GameEntity> entitiesAtPosition = _entityManager.GetEntitiesAtPosition(cellPos);
                
                if (entitiesAtPosition.Count > 0)
                {
                    // Select the first entity
                    GameEntity entity = entitiesAtPosition[0];
                    _selectedEntityId = entity.EntityId;
                    
                    // Update UI
                    _uiManager.SelectEntity(_selectedEntityId);
                    
                    // If it's a unit, show movement range
                    if (entity.HasComponent<UnitStatsComponent>())
                    {
                        _uiManager.ShowMovementRange(_selectedEntityId);
                    }
                }
                else if (_selectedEntityId != -1)
                {
                    // Check if we're clicking on a movement range cell
                    var unitSystem = GameManager.Instance.SystemManager.GetSystem<UnitManagementSystem>();
                    var unit = unitSystem.GetUnit(_selectedEntityId);
                    
                    if (unit != null && unit.HasComponent<UnitStatsComponent>())
                    {
                        var moveRange = unitSystem.GetMovementRange(_selectedEntityId);
                        
                        if (moveRange.Contains(cellPos))
                        {
                            // Move unit
                            EventManager.Instance.TriggerEvent(new UnitMoveRequestEvent
                            {
                                UnitId = _selectedEntityId,
                                TargetPosition = cellPos
                            });
                            
                            // Clear selection after move
                            _uiManager.DeselectEntity();
                            _selectedEntityId = -1;
                            return;
                        }
                    }
                    
                    // If we click on empty space, deselect
                    _uiManager.DeselectEntity();
                    _selectedEntityId = -1;
                }
                
                // Trigger terrain selected event
                EventManager.Instance.TriggerEvent(new TerrainSelectedEvent
                {
                    GridPosition = cellPos
                });
            }
        }
        
        private void HandleRightClick()
        {
            // If there's a selected entity, show context menu
            if (_selectedEntityId != -1)
            {
                var entity = _entityManager.GetEntity(_selectedEntityId);
                
                if (entity != null)
                {
                    List<ContextMenuItem> menuItems = new List<ContextMenuItem>();
                    
                    if (entity.HasComponent<UnitStatsComponent>())
                    {
                        // Unit context menu
                        menuItems.Add(new ContextMenuItem
                        {
                            Label = "Move",
                            OnSelected = () => _uiManager.ShowMovementRange(_selectedEntityId),
                            IsEnabled = true,
                            TooltipText = "Show movement range"
                        });
                        
                        menuItems.Add(new ContextMenuItem
                        {
                            Label = "Attack",
                            OnSelected = () => _uiManager.ShowAttackRange(_selectedEntityId),
                            IsEnabled = true,
                            TooltipText = "Show attack range"
                        });
                        
                        // Add more unit-specific options
                    }
                    else if (entity.HasComponent<BaseComponent>())
                    {
                        // Base context menu
                        menuItems.Add(new ContextMenuItem
                        {
                            Label = "Production",
                            OnSelected = () => _uiManager.ShowPanel(PanelType.Production),
                            IsEnabled = true,
                            TooltipText = "Manage production queue"
                        });
                        
                        menuItems.Add(new ContextMenuItem
                        {
                            Label = "Research",
                            OnSelected = () => _uiManager.ShowPanel(PanelType.Research),
                            IsEnabled = true,
                            TooltipText = "Manage research projects"
                        });
                        
                        // Add more base-specific options
                    }
                    
                    // Add general options
                    menuItems.Add(new ContextMenuItem
                    {
                        Label = "Cancel",
                        OnSelected = () => _uiManager.DeselectEntity(),
                        IsEnabled = true,
                        TooltipText = "Cancel selection"
                    });
                    
                    _uiManager.ShowContextMenu(Input.mousePosition, menuItems);
                }
            }
            else
            {
                // If no entity selected, show general context menu
                Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit))
                {
                    // Get cell position
                    Vector3 worldPos = hit.point;
                    Vector2Int cellPos = WorldToGrid(worldPos);
                    
                    List<ContextMenuItem> menuItems = new List<ContextMenuItem>();
                    
                    // Get terrain type
                    string terrainType = _mapManager.GetCellType(cellPos);
                    
                    // Add terrain-specific options
                    if (terrainType == "Plain")
                    {
                        menuItems.Add(new ContextMenuItem
                        {
                            Label = "Build Base",
                            OnSelected = () => StartBuildingBase(cellPos),
                            IsEnabled = true,
                            TooltipText = "Build a new base on this location"
                        });
                    }
                    
                    // Add general options
                    menuItems.Add(new ContextMenuItem
                    {
                        Label = "Info",
                        OnSelected = () => _uiManager.ShowTerrainInfo(cellPos, Input.mousePosition),
                        IsEnabled = true,
                        TooltipText = "Show terrain information"
                    });
                    
                    _uiManager.ShowContextMenu(Input.mousePosition, menuItems);
                }
            }
        }
        
        private void HandleKeyboardShortcuts()
        {
            // End turn
            if (Input.GetKeyDown(KeyCode.Return))
            {
                EventManager.Instance.TriggerEvent(new EndTurnRequestEvent());
            }
            
            // Pause game
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (GameManager.Instance.CurrentState == GameState.Playing)
                {
                    GameManager.Instance.PauseGame();
                    _uiManager.ShowPauseMenu();
                }
                else if (GameManager.Instance.CurrentState == GameState.Paused)
                {
                    GameManager.Instance.ResumeGame();
                    _uiManager.HidePanel(PanelType.PauseMenu);
                }
            }
            
            // Zoom in/out
            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                EventManager.Instance.TriggerEvent(new CameraZoomRequestEvent
                {
                    ZoomIn = Input.GetAxis("Mouse ScrollWheel") > 0
                });
            }
            
            // Toggle resource panel
            if (Input.GetKeyDown(KeyCode.R))
            {
                _uiManager.TogglePanel(PanelType.ResourceOverview);
            }
            
            // Toggle minimap
            if (Input.GetKeyDown(KeyCode.M))
            {
                _uiManager.TogglePanel(PanelType.MiniMap);
            }
        }
        
        private Vector2Int WorldToGrid(Vector3 worldPosition)
        {
            // Convert world position to grid position
            // This will depend on your grid system implementation
            // For a simple example:
            int x = Mathf.RoundToInt(worldPosition.x / 10f);
            int y = Mathf.RoundToInt(worldPosition.z / 10f);
            
            return new Vector2Int(x, y);
        }
        
        private void StartBuildingBase(Vector2Int position)
        {
            // Show base type selection dialog
            List<ContextMenuItem> baseTypes = new List<ContextMenuItem>();
            
            foreach (BaseType type in System.Enum.GetValues(typeof(BaseType)))
            {
                baseTypes.Add(new ContextMenuItem
                {
                    Label = type.ToString(),
                    OnSelected = () => RequestBuildBase(position, type),
                    IsEnabled = true,
                    TooltipText = GetBaseTypeDescription(type)
                });
            }
            
            _uiManager.ShowContextMenu(Input.mousePosition, baseTypes);
        }
        
        private void RequestBuildBase(Vector2Int position, BaseType baseType)
        {
            // Show name input dialog
            // For simplicity, we'll just use a default name here
            string baseName = $"{baseType} Base";
            
            // Show confirmation dialog
            _uiManager.ShowConfirmationDialog(
                $"Build a new {baseType} base at {position}?",
                () => {
                    // Trigger base construction request
                    EventManager.Instance.TriggerEvent(new BaseConstructionRequestEvent
                    {
                        BaseId = 0, // Will be assigned by the system
                        BaseName = baseName,
                        Position = position,
                        BaseType = baseType
                    });
                },
                () => {
                    // Cancel building
                }
            );
        }
        
        private string GetBaseTypeDescription(BaseType type)
        {
            switch (type)
            {
                case BaseType.Headquarters:
                    return "Main command center with balanced capabilities";
                case BaseType.ResearchFacility:
                    return "Focused on research and technology development";
                case BaseType.ProductionPlant:
                    return "Specialized in unit and equipment production";
                case BaseType.ResourceMine:
                    return "Generates essential resources";
                case BaseType.DefenseOutpost:
                    return "Defensive installation with combat capabilities";
                default:
                    return "Unknown base type";
            }
        }
    }
}