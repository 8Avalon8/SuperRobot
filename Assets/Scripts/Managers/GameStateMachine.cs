
using System;
using System.Collections.Generic;

namespace SuperRobot
{
        // 游戏状态枚举
    public enum GameState
    {
        MainMenu,
        Initializing,
        Playing,
        Paused,
        Battle,
        GameOver
    }

    // 回合阶段枚举
    public enum TurnPhase
    {
        Strategy,   // 策略阶段（研发、生产、调度）
        Action,     // 行动阶段（执行任务、战斗）
        Event,      // 事件阶段（处理随机事件）
        Enemy,      // 敌方阶段（AI行动）
        Settlement  // 结算阶段（资源收入、状态更新）
    }

    // 游戏状态机
    public class GameStateMachine
    {
        private GameState _currentState;
        public GameState CurrentState => _currentState;
        
        // 状态转换事件处理
        private Dictionary<GameState, Action> _enterStateActions = new Dictionary<GameState, Action>();
        private Dictionary<GameState, Action> _exitStateActions = new Dictionary<GameState, Action>();
        private Dictionary<GameState, Action> _updateStateActions = new Dictionary<GameState, Action>();
        
        public GameStateMachine()
        {
            // 初始状态为主菜单
            _currentState = GameState.MainMenu;
            
            // 注册状态处理
            RegisterStateHandlers();
        }
        
        private void RegisterStateHandlers()
        {
            // 主菜单状态
            _enterStateActions[GameState.MainMenu] = () => {
                // 启用UI系统，禁用其他系统
                var systemManager = GameManager.Instance.SystemManager;
                systemManager.EnableSystem<UISystem>();
                systemManager.DisableSystem<BattleSystem>();
                systemManager.DisableSystem<ResourceSystem>();
                systemManager.DisableSystem<AISystem>();
            };
            
            // 游戏初始化状态
            _enterStateActions[GameState.Initializing] = () => {
                // 初始化游戏数据
                // ...
                
                // 自动转换到Playing状态
                ChangeState(GameState.Playing);
            };
            
            // 游戏进行状态
            _enterStateActions[GameState.Playing] = () => {
                // 启用所有系统
                var systemManager = GameManager.Instance.SystemManager;
                systemManager.EnableSystem<ResourceSystem>();
                systemManager.EnableSystem<TechTreeSystem>();
                systemManager.EnableSystem<UnitManagementSystem>();
                systemManager.EnableSystem<PilotManagementSystem>();
                systemManager.EnableSystem<BaseManagementSystem>();
                systemManager.EnableSystem<ProductionSystem>();
                systemManager.EnableSystem<EventSystem>();
                systemManager.EnableSystem<AISystem>();
            };
            
            // 游戏暂停状态
            _enterStateActions[GameState.Paused] = () => {
                // 禁用大多数系统，仅保留UI系统
                var systemManager = GameManager.Instance.SystemManager;
                systemManager.DisableSystem<ResourceSystem>();
                systemManager.DisableSystem<TechTreeSystem>();
                systemManager.DisableSystem<UnitManagementSystem>();
                systemManager.DisableSystem<PilotManagementSystem>();
                systemManager.DisableSystem<BaseManagementSystem>();
                systemManager.DisableSystem<ProductionSystem>();
                systemManager.DisableSystem<EventSystem>();
                systemManager.DisableSystem<AISystem>();
            };
            
            // 战斗状态
            _enterStateActions[GameState.Battle] = () => {
                // 启用战斗相关系统，禁用其他系统
                var systemManager = GameManager.Instance.SystemManager;
                systemManager.EnableSystem<BattleSystem>();
                systemManager.DisableSystem<ResourceSystem>();
                systemManager.DisableSystem<TechTreeSystem>();
                systemManager.DisableSystem<ProductionSystem>();
            };
            
            // 游戏结束状态
            _enterStateActions[GameState.GameOver] = () => {
                // 禁用大多数系统，显示结果画面
                var systemManager = GameManager.Instance.SystemManager;
                systemManager.DisableSystem<ResourceSystem>();
                systemManager.DisableSystem<TechTreeSystem>();
                systemManager.DisableSystem<UnitManagementSystem>();
                systemManager.DisableSystem<PilotManagementSystem>();
                systemManager.DisableSystem<BaseManagementSystem>();
                systemManager.DisableSystem<ProductionSystem>();
                systemManager.DisableSystem<EventSystem>();
                systemManager.DisableSystem<AISystem>();
                systemManager.DisableSystem<BattleSystem>();
            };
        }
        
        /// <summary>
        /// 更新当前状态
        /// </summary>
        public void Update()
        {
            if (_updateStateActions.TryGetValue(_currentState, out var updateAction))
            {
                updateAction?.Invoke();
            }
        }
        
        /// <summary>
        /// 改变游戏状态
        /// </summary>
        public void ChangeState(GameState newState)
        {
            // 如果状态相同，不做任何操作
            if (_currentState == newState)
                return;
            
            GameState oldState = _currentState;
            
            // 执行退出旧状态的操作
            if (_exitStateActions.TryGetValue(_currentState, out var exitAction))
            {
                exitAction?.Invoke();
            }
            
            // 更新当前状态
            _currentState = newState;
            
            // 执行进入新状态的操作
            if (_enterStateActions.TryGetValue(_currentState, out var enterAction))
            {
                enterAction?.Invoke();
            }
            
            // 触发状态改变事件
            EventManager.Instance.TriggerEvent(new GameStateChangedEvent
            {
                PreviousState = oldState,
                CurrentState = newState
            });
        }
    }
}