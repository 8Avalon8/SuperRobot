using UnityEngine;

/// <summary>
/// 在GameObject上关联实体ID的组件
/// </summary>
public class EntityReference : MonoBehaviour
{
    [SerializeField] private int _entityId;
    
    public int EntityId 
    { 
        get => _entityId; 
        set => _entityId = value; 
    }
}