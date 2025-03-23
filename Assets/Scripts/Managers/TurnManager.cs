namespace SuperRobot
{
    /// <summary>
/// 管理游戏回合的静态类
/// </summary>
public static class TurnManager
{
    private static int _currentTurn = 1;
    private static TurnPhase _currentPhase = TurnPhase.Strategy;
    private static bool _isNewTurn = true;
    
    public static int CurrentTurn => _currentTurn;
    public static TurnPhase CurrentPhase => _currentPhase;
    public static bool IsNewTurn => _isNewTurn;
    
    /// <summary>
    /// 设置当前回合
    /// </summary>
    public static void SetCurrentTurn(int turn)
    {
        _currentTurn = turn;
        _isNewTurn = true;
    }
    
    /// <summary>
    /// 进入下一回合
    /// </summary>
    public static void NextTurn()
    {
        _currentTurn++;
        _currentPhase = TurnPhase.Strategy;
        _isNewTurn = true;
        
        // 触发回合开始事件
        EventManager.Instance.TriggerEvent(new TurnStartedEvent
        {
            TurnNumber = _currentTurn,
            Phase = _currentPhase
        });
    }
    
    /// <summary>
    /// 进入下一阶段
    /// </summary>
    public static void NextPhase()
    {
        _isNewTurn = false;
        TurnPhase previousPhase = _currentPhase;
        
        // 阶段转换
        switch (_currentPhase)
        {
            case TurnPhase.Strategy:
                _currentPhase = TurnPhase.Action;
                break;
            case TurnPhase.Action:
                _currentPhase = TurnPhase.Event;
                break;
            case TurnPhase.Event:
                _currentPhase = TurnPhase.Enemy;
                break;
            case TurnPhase.Enemy:
                _currentPhase = TurnPhase.Settlement;
                break;
            case TurnPhase.Settlement:
                NextTurn(); // 开始新回合
                return;
        }
        
        // 触发阶段改变事件
        EventManager.Instance.TriggerEvent(new TurnPhaseChangedEvent
        {
            PreviousPhase = previousPhase,
            NewPhase = _currentPhase
        });
    }
}
}
