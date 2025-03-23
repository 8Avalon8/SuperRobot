using System.Collections.Generic;
using UnityEngine;

namespace SuperRobot
{
    // 输入系统
public class InputSystem : GameSystem
{
    private Dictionary<string, KeyCode> _keyBindings = new Dictionary<string, KeyCode>();
    private Vector2 _lastMousePosition;
    private bool _isMouseDragging;
    private Vector2 _dragStartPosition;
    
    public override void Initialize()
    {
        // 初始化默认按键绑定
        _keyBindings["MoveCamera"] = KeyCode.Mouse1;
        _keyBindings["SelectUnit"] = KeyCode.Mouse0;
        _keyBindings["RotateCamera"] = KeyCode.Mouse2;
        _keyBindings["ZoomIn"] = KeyCode.Equals;
        _keyBindings["ZoomOut"] = KeyCode.Minus;
        _keyBindings["Pause"] = KeyCode.Space;
        _keyBindings["SpeedUp"] = KeyCode.Tab;
        _keyBindings["EndTurn"] = KeyCode.Return;
        
        // 订阅事件
        EventManager.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
    }
    
    public override void Execute()
    {
        // 只在游戏进行状态下处理输入
        if (GameManager.Instance.CurrentState != GameState.Playing &&
            GameManager.Instance.CurrentState != GameState.Battle)
            return;
        
        ProcessKeyboardInput();
        ProcessMouseInput();
    }
    
    private void ProcessKeyboardInput()
    {
        // 暂停/恢复游戏
        if (Input.GetKeyDown(_keyBindings["Pause"]))
        {
            if (GameManager.Instance.CurrentState == GameState.Playing)
                GameManager.Instance.PauseGame();
            else if (GameManager.Instance.CurrentState == GameState.Paused)
                GameManager.Instance.ResumeGame();
        }
        
        // 结束回合
        if (Input.GetKeyDown(_keyBindings["EndTurn"]))
        {
            if (GameManager.Instance.CurrentState == GameState.Playing)
            {
                EventManager.TriggerEvent(new EndTurnRequestEvent());
            }
        }
        
        // 加速游戏（仅在AI或事件回合）
        if (Input.GetKeyDown(_keyBindings["SpeedUp"]))
        {
            EventManager.TriggerEvent(new GameSpeedChangeRequestEvent { SpeedUp = true });
        }
        
        // 缩放摄像机
        if (Input.GetKey(_keyBindings["ZoomIn"]))
        {
            EventManager.TriggerEvent(new CameraZoomRequestEvent { ZoomIn = true });
        }
        else if (Input.GetKey(_keyBindings["ZoomOut"]))
        {
            EventManager.TriggerEvent(new CameraZoomRequestEvent { ZoomIn = false });
        }
    }
    
    private void ProcessMouseInput()
    {
        Vector2 currentMousePosition = Input.mousePosition;
        
        // 鼠标拖动（移动摄像机）
        if (Input.GetKeyDown(_keyBindings["MoveCamera"]))
        {
            _isMouseDragging = true;
            _dragStartPosition = currentMousePosition;
        }
        
        if (_isMouseDragging)
        {
            Vector2 deltaPosition = currentMousePosition - _lastMousePosition;
            if (deltaPosition.magnitude > 0)
            {
                EventManager.TriggerEvent(new CameraMoveRequestEvent { Delta = deltaPosition });
            }
        }
        
        if (Input.GetKeyUp(_keyBindings["MoveCamera"]))
        {
            _isMouseDragging = false;
            
            // 如果几乎没有移动，视为点击
            if (Vector2.Distance(currentMousePosition, _dragStartPosition) < 5f)
            {
                Ray ray = Camera.main.ScreenPointToRay(currentMousePosition);
                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit))
                {
                    // 点击处理
                    HandleClick(hit);
                }
            }
        }
        
        // 旋转摄像机
        if (Input.GetKey(_keyBindings["RotateCamera"]))
        {
            Vector2 deltaMouse = currentMousePosition - _lastMousePosition;
            if (deltaMouse.magnitude > 0)
            {
                EventManager.TriggerEvent(new CameraRotateRequestEvent { Delta = deltaMouse });
            }
        }
        
        _lastMousePosition = currentMousePosition;
    }
    
    private void HandleClick(RaycastHit hit)
    {
        // 处理单位选择
        if (hit.transform.CompareTag("Unit"))
        {
            int unitId = hit.transform.GetComponent<EntityReference>().EntityId;
            EventManager.TriggerEvent(new UnitSelectedEvent { UnitId = unitId });
        }
        // 处理基地选择
        else if (hit.transform.CompareTag("Base"))
        {
            int baseId = hit.transform.GetComponent<EntityReference>().EntityId;
            EventManager.TriggerEvent(new BaseSelectedEvent { BaseId = baseId });
        }
        // 处理地形选择
        else if (hit.transform.CompareTag("Terrain"))
        {
            Vector2Int gridPosition = new Vector2Int(
                Mathf.RoundToInt(hit.point.x / 10f),
                Mathf.RoundToInt(hit.point.z / 10f)
            );
            EventManager.TriggerEvent(new TerrainSelectedEvent { GridPosition = gridPosition });
        }
    }
    
    private void OnGameStateChanged(GameStateChangedEvent evt)
    {
        // 可能需要根据游戏状态调整输入处理
        if (evt.CurrentState == GameState.MainMenu || evt.CurrentState == GameState.GameOver)
        {
            _isMouseDragging = false;
        }
    }
    
    public override void Cleanup()
    {
        // 取消订阅事件
        EventManager.Instance.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
    }
}
}
