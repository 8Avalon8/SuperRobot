using UnityEngine;

namespace SuperRobot
{
    // 系统基类
public abstract class GameSystem
{
    protected EntityManager EntityManager => GameManager.Instance.EntityManager;
    protected SystemManager SystemManager => GameManager.Instance.SystemManager;
    protected EventManager EventManager => EventManager.Instance;
    
    /// <summary>
    /// 系统初始化
    /// </summary>
    public virtual void Initialize() { }
    
    /// <summary>
    /// 系统执行逻辑
    /// </summary>
    public abstract void Execute();
    
    /// <summary>
    /// 系统清理
    /// </summary>
    public virtual void Cleanup() { }
    
    /// <summary>
    /// 获取其他系统
    /// </summary>
    protected T GetSystem<T>() where T : GameSystem
    {
        return SystemManager.GetSystem<T>();
    }
}
}