using UnityEngine;

namespace SuperRobot
{
    public class UnitVisualController : MonoBehaviour
    {
        // 单位引用
        [SerializeField] private GameObject _unitModel;

        // 移动动画
        [SerializeField] private Animator _animator;

        // 移动参数
        [SerializeField] private float _moveHeight = 0.2f; // 单位在移动时的高度偏移

        // 动画参数名称
        private readonly string _moveAnimParam = "IsMoving";

        private EntityReference _entityRef;
        
        public void Bind(int unityId)
        {
            _unitModel = this.gameObject;
            _animator  = _unitModel.GetComponent<Animator>();
            
            _entityRef = GetComponent<EntityReference>();
            if (_entityRef == null)
            {
                _entityRef = gameObject.AddComponent<EntityReference>();
            }
            _entityRef.EntityId = unityId;
        }

        private void Awake()
        {
            
            // 订阅事件
            EventManager.Instance.Subscribe<UnitMovementStartedEvent>(OnMovementStarted);
            EventManager.Instance.Subscribe<UnitMovementCompletedEvent>(OnMovementCompleted);
            EventManager.Instance.Subscribe<UnitPositionUpdatedEvent>(OnPositionUpdated);
        }

        private void OnMovementStarted(UnitMovementStartedEvent evt)
        {
            // 获取单位ID
            if (_entityRef == null || _entityRef.EntityId != evt.UnitId)
                return;
            // 开始移动动画
            if (_animator != null)
            {
                _animator.SetBool(_moveAnimParam, true);
            }

            // 添加一点高度，让单位看起来是"漂浮"移动的
            Vector3 pos = transform.position;
            pos.y += _moveHeight;
            transform.position = pos;
        }

        private void OnMovementCompleted(UnitMovementCompletedEvent evt)
        {
            // 获取单位ID
            if (_entityRef == null || _entityRef.EntityId != evt.UnitId)
                return;

            // 停止移动动画
            if (_animator != null)
            {
                _animator.SetBool(_moveAnimParam, false);
            }

            // 恢复正常高度
            Vector3 pos = transform.position;
            pos.y -= _moveHeight;
            transform.position = pos;
        }

        private void OnPositionUpdated(UnitPositionUpdatedEvent evt)
        {
            // 获取单位ID
            if (_entityRef == null || _entityRef.EntityId != evt.UnitId)
                return;

            // 这里不需要做什么，因为位置更新由UnitMovementSystem处理
            // 但可以添加一些视觉效果，如粒子等
        }

        private int GetUnitId()
        {
            return _entityRef.EntityId;
        }

        private void OnDestroy()
        {
            // 取消订阅事件
            EventManager.Instance.Unsubscribe<UnitMovementStartedEvent>(OnMovementStarted);
            EventManager.Instance.Unsubscribe<UnitMovementCompletedEvent>(OnMovementCompleted);
            EventManager.Instance.Unsubscribe<UnitPositionUpdatedEvent>(OnPositionUpdated);
        }

        // 当鼠标点击时，显示单位信息
        private void OnMouseDown()
        {
            EventManager.Instance.TriggerEvent(new UnitSelectedEvent { UnitId = GetUnitId() });
        }
    }

}
