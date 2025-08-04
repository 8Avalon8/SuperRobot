namespace SuperRobot.Core
{
    /// <summary>
    /// 实体管理服务接口
    /// </summary>
    public interface IEntityManagerService
    {
        EntityManager EntityManager { get; }
    }

    /// <summary>
    /// 系统管理服务接口
    /// </summary>
    public interface ISystemManagerService
    {
        ImprovedSystemManager ImprovedSystemManager { get; }
    }

    /// <summary>
    /// 事件管理服务接口
    /// </summary>
    public interface IEventManagerService
    {
        EventManager EventManager { get; }
    }

    /// <summary>
    /// 地图管理服务接口
    /// </summary>
    public interface IMapManagerService
    {
        IMapManager MapManager { get; }
    }

    /// <summary>
    /// 游戏配置服务接口
    /// </summary>
    public interface IGameConfigService
    {
        GameConfig GameConfig { get; }
    }
}