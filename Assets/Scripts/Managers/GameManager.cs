using System;
using System.Collections.Generic;
using TGS;
using UnityEngine;
using SuperRobot.Core;

namespace SuperRobot
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance => _instance;
        
        // 核心管理器
        public EntityManager         EntityManager         { get; private set; }
        public ImprovedSystemManager SystemManager         { get; private set; }
        public ImprovedSystemManager ImprovedSystemManager { get; private set; }
        public IMapManager           MapManager            { get; private set; }
        
        // 服务容器
        private IServiceContainer _serviceContainer;
        public IServiceContainer ServiceContainer => _serviceContainer;
        
        // 游戏状态
        private GameStateMachine _stateMachine;
        public GameState CurrentState => _stateMachine.CurrentState;
        
        // 游戏配置
        [SerializeField] private GameConfig _gameConfig;
        public GameConfig GameConfig => _gameConfig;

        // 回合管理
        public int CurrentTurn => TurnManager.CurrentTurn;
        
        private void Awake()
        {
            if (_instance != null)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 初始化服务容器
            InitializeServiceContainer();
            
            // 初始化核心组件
            InitializeCoreComponents();
            
            // 初始化系统
            InitializeSystems();
        }
        
        private void InitializeServiceContainer()
        {
            _serviceContainer = new ServiceContainer();
        }
        
        private void InitializeCoreComponents()
        {
            // 加载游戏配置
            _gameConfig = Resources.Load<GameConfig>("Data/GameConfig");
            
            // 初始化管理器
            EntityManager = new EntityManager();
            
            // 设置SystemManager的服务容器
            SystemManager.SetServiceContainer(_serviceContainer);
            
            // 初始化状态机
            _stateMachine = new GameStateMachine();
            MapManager = new GameObject("MapManager").AddComponent<TGSMapManager>();
            
            // 注册核心服务
            RegisterCoreServices();
        }
        
        private void RegisterCoreServices()
        {
            _serviceContainer.RegisterService<IEntityManagerService>(new EntityManagerService(EntityManager));
            _serviceContainer.RegisterService<ISystemManagerService>(new SystemManagerService(SystemManager));
            _serviceContainer.RegisterService<IEventManagerService>(new EventManagerService(EventManager.Instance));
            _serviceContainer.RegisterService<IMapManagerService>(new MapManagerService(MapManager));
            _serviceContainer.RegisterService<IGameConfigService>(new GameConfigService(_gameConfig));
            
            // 初始化服务定位器（向后兼容）
            ServiceLocator.Initialize(_serviceContainer);
        }
        
        private void InitializeSystems()
        {
                InitializeImprovedSystems();
        }
        
        private void InitializeImprovedSystems()
        {
            // 注册核心系统（分阶段，有优先级）
            
            // 预更新阶段 - 输入和基础系统
            ImprovedSystemManager.RegisterSystem(new InputSystem(), SystemPhase.PreUpdate, SystemPriority.Critical);
            ImprovedSystemManager.RegisterSystem(new EventSystem(), SystemPhase.PreUpdate, SystemPriority.High);
            
            // 主更新阶段 - 游戏逻辑系统
            ImprovedSystemManager.RegisterSystem(new ResourceSystem(), SystemPhase.Update, SystemPriority.High);
            ImprovedSystemManager.RegisterSystem(new TechTreeSystem(), SystemPhase.Update, SystemPriority.Normal);
            ImprovedSystemManager.RegisterSystem(new UnitManagementSystem(), SystemPhase.Update, SystemPriority.High);
            ImprovedSystemManager.RegisterSystem(new PilotManagementSystem(), SystemPhase.Update, SystemPriority.Normal);
            ImprovedSystemManager.RegisterSystem(new BaseManagementSystem(), SystemPhase.Update, SystemPriority.Normal);
            ImprovedSystemManager.RegisterSystem(new ProductionSystem(), SystemPhase.Update, SystemPriority.Normal);
            ImprovedSystemManager.RegisterSystem(new BattleSystem(), SystemPhase.Update, SystemPriority.High);
            ImprovedSystemManager.RegisterSystem(new UnitMovementSystem(), SystemPhase.Update, SystemPriority.High);
            ImprovedSystemManager.RegisterSystem(new AISystem(), SystemPhase.Update, SystemPriority.Normal);
            
            // 后更新阶段 - UI和视觉系统
            ImprovedSystemManager.RegisterSystem(new UISystem(), SystemPhase.PostUpdate, SystemPriority.Low);
            
            // 启用性能分析
            ImprovedSystemManager.EnableProfiling = Application.isEditor;
        }
        
        private void InitializeLegacySystems()
        {
            // 传统的系统注册方式（向后兼容）
            SystemManager.RegisterSystem(new InputSystem());
            SystemManager.RegisterSystem(new ResourceSystem());
            SystemManager.RegisterSystem(new TechTreeSystem());
            SystemManager.RegisterSystem(new UnitManagementSystem());
            SystemManager.RegisterSystem(new PilotManagementSystem());
            SystemManager.RegisterSystem(new BaseManagementSystem());
            SystemManager.RegisterSystem(new ProductionSystem());
            SystemManager.RegisterSystem(new BattleSystem());
            SystemManager.RegisterSystem(new AISystem());
            SystemManager.RegisterSystem(new EventSystem());
            SystemManager.RegisterSystem(new UISystem());
            SystemManager.RegisterSystem(new UnitMovementSystem());
        }
        
        private void Update()
        {
            // 更新状态机
            _stateMachine.Update();
            
            // 执行系统
                ImprovedSystemManager.ExecuteAllSystems();
        }
        
        /// <summary>
        /// 开始新游戏
        /// </summary>
        public void StartNewGame()
        {
            TerrainGridSystem.instance.Redraw();
            // 重置所有系统
            SystemManager.ResetAllSystems();
            
            // 清除所有实体
            EntityManager.ClearAllEntities();
            
            // 切换到游戏初始化状态
            _stateMachine.ChangeState(GameState.Initializing);

            // 初始化玩家资源和单位
            InitializePlayerResources();

            // 创建初始基地
            CreateInitialBase();

            // 初始化AI
            InitializeAI();

            // 开始第一回合
            TurnManager.SetCurrentTurn(1);
            
            // 触发游戏开始事件
            EventManager.Instance.TriggerEvent(new GameStartedEvent());
        }
        
        /// <summary>
        /// 进入下一回合
        /// </summary>
        public void AdvanceTurn()
        {
            TurnManager.NextTurn();
        }
        
        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void PauseGame()
        {
            if (CurrentState == GameState.Playing)
            {
                _stateMachine.ChangeState(GameState.Paused);
            }
        }
        
        /// <summary>
        /// 恢复游戏
        /// </summary>
        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
            {
                _stateMachine.ChangeState(GameState.Playing);
            }
        }
        
        /// <summary>
        /// 保存游戏
        /// </summary>
        public void SaveGame(string saveName)
        {
            var saveSystem = GetComponent<SaveSystem>();
            if (saveSystem != null)
            {
                saveSystem.SaveGame(saveName);
            }
        }
        
        /// <summary>
        /// 加载游戏
        /// </summary>
        public void LoadGame(string saveName)
        {
            var saveSystem = GetComponent<SaveSystem>();
            if (saveSystem != null)
            {
                saveSystem.LoadGame(saveName);
            }
        }
        
        /// <summary>
        /// 退出游戏
        /// </summary>
        public void QuitGame()
        {
            _stateMachine.ChangeState(GameState.MainMenu);
            
            // 清理
            SystemManager.CleanupAllSystems();
            EntityManager.ClearAllEntities();
        }
        
        /// <summary>
        /// 重置游戏状态
        /// </summary>
        public void ResetGameState()
        {
            SystemManager.ResetAllSystems();
            EntityManager.ClearAllEntities();
            TurnManager.SetCurrentTurn(1);
        }

        private void InitializePlayerResources()
        {
            var resourceSystem = GameManager.Instance.SystemManager.GetSystem<ResourceSystem>();

            // 设置初始资源
            var gameConfig = GameManager.Instance.GameConfig;
            foreach (var resource in gameConfig.InitialResources)
            {
                resourceSystem.AddResource(resource.Key, resource.Value);
            }
        }

        private void CreateInitialBase()
        {
            var baseSystem = GameManager.Instance.SystemManager.GetSystem<BaseManagementSystem>();

            // 创建玩家初始基地（总部）
            var baseEntity = baseSystem.CreateBase("总部", BaseType.Headquarters, new Vector2Int(30, 30));

            // 创建初始设施
            var baseComp = baseEntity.GetComponent<BaseComponent>();
            if (baseComp != null)
            {
                // 添加基础研究实验室
                baseComp.AddFacility(new Facility
                {
                    FacilityId = "basic_research_lab",
                    FacilityName = "基础研究实验室",
                    FacilityType = FacilityType.ResearchLab,
                    EffectValue = 10
                });
            }
        }

        private void InitializeAI()
        {
            // 初始化AI系统
            var aiSystem = GameManager.Instance.SystemManager.GetSystem<AISystem>();

            // AI初始化方法...
        }
    }
}
