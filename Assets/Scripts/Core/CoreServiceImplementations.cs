namespace SuperRobot.Core
{
    /// <summary>
    /// 实体管理服务实现
    /// </summary>
    public class EntityManagerService : IEntityManagerService
    {
        public EntityManager EntityManager { get; private set; }

        public EntityManagerService(EntityManager entityManager)
        {
            EntityManager = entityManager;
        }
    }

    /// <summary>
    /// 系统管理服务实现
    /// </summary>
    public class SystemManagerService : ISystemManagerService
    {
        public ImprovedSystemManager ImprovedSystemManager { get; private set; }

        public SystemManagerService(ImprovedSystemManager improvedSystemManager)
        {
            ImprovedSystemManager = improvedSystemManager;
        }
    }

    /// <summary>
    /// 事件管理服务实现
    /// </summary>
    public class EventManagerService : IEventManagerService
    {
        public EventManager EventManager { get; private set; }

        public EventManagerService(EventManager eventManager)
        {
            EventManager = eventManager;
        }
    }

    /// <summary>
    /// 地图管理服务实现
    /// </summary>
    public class MapManagerService : IMapManagerService
    {
        public IMapManager MapManager { get; private set; }

        public MapManagerService(IMapManager mapManager)
        {
            MapManager = mapManager;
        }
    }

    /// <summary>
    /// 游戏配置服务实现
    /// </summary>
    public class GameConfigService : IGameConfigService
    {
        public GameConfig GameConfig { get; private set; }

        public GameConfigService(GameConfig gameConfig)
        {
            GameConfig = gameConfig;
        }
    }
}