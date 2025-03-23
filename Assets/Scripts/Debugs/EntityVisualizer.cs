using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Linq;
using Sirenix.OdinInspector;

namespace SuperRobot
{

    /// <summary>
    /// 为实体提供调试可视化功能
    /// </summary>
    public class EntityVisualizer : MonoBehaviour
    {
        [SerializeField] private int          _entityId;
        [SerializeField] private string       _entityType         = "Unknown";
        [SerializeField] private List<string> _attachedComponents = new List<string>();

        // 实体组件缓存的关键数据，用于调试
        [ShowInInspector]
        private Dictionary<string, string> _keyData = new Dictionary<string, string>();

        // 是否显示调试标签
        [SerializeField] private bool _showDebugLabel = true;

        // 缓存引用的实体
        private GameEntity _entityRef;

        /// <summary>
        /// 初始化可视化器
        /// </summary>
        public void Initialize(GameEntity entity, int entityId, string entityType)
        {
            _entityRef  = entity;
            _entityId   = entityId;
            _entityType = entityType;

            // 设置游戏对象名称以便于识别
            gameObject.name = $"{_entityType}_{_entityId}";

            // 更新组件列表
            UpdateComponentsList();

            // 更新关键数据
            UpdateKeyData();
        }

        /// <summary>
        /// 更新组件列表
        /// </summary>
        public void UpdateComponentsList()
        {
            if (_entityRef == null)
                return;

            _attachedComponents.Clear();

            // 获取所有组件类型
            foreach (var componentType in _entityRef.GetComponentTypes())
            {
                _attachedComponents.Add(componentType.Name);
            }
        }

        /// <summary>
        /// 更新关键数据
        /// </summary>
        public void UpdateKeyData()
        {
            if (_entityRef == null)
                return;

            _keyData.Clear();

            // 获取位置组件数据
            var posComp = _entityRef.GetComponent<PositionComponent>();
            if (posComp != null)
            {
                _keyData["Position"] = posComp.Position.ToString();
            }

            // 获取单位状态组件数据
            var statsComp = _entityRef.GetComponent<UnitStatsComponent>();
            if (statsComp != null)
            {
                _keyData["Health"]       = $"{statsComp.CurrentHealth}/{statsComp.MaxHealth}";
                _keyData["Energy"]       = $"{statsComp.CurrentEnergy}/{statsComp.MaxEnergy}";
                _keyData["UnitType"]     = statsComp.UnitType.ToString();
                _keyData["ActionPoints"] = $"{statsComp.CurrentActionPoints}/{statsComp.MaxActionPoints}";
            }

            // 获取驾驶员组件数据
            var pilotComp = _entityRef.GetComponent<PilotComponent>();
            if (pilotComp != null)
            {
                _keyData["PilotId"]       = pilotComp.PilotId.ToString();
                _keyData["Compatibility"] = pilotComp.CompatibilityRate.ToString("F2");
            }

            // 获取基地组件数据
            var baseComp = _entityRef.GetComponent<BaseComponent>();
            if (baseComp != null)
            {
                _keyData["BaseType"]    = baseComp.BaseType.ToString();
                _keyData["Level"]       = baseComp.Level.ToString();
                _keyData["Health"]      = $"{baseComp.Health}/{baseComp.MaxHealth}";
                _keyData["Operational"] = baseComp.IsOperational.ToString();
            }

            // 获取NT特殊组件数据
            var superComp = _entityRef.GetComponent<SuperRobotComponent>();
            if (superComp != null)
            {
                _keyData["NTBonus"]     = superComp.NTBonus.ToString();
                _keyData["BerserkMode"] = superComp.IsBerserkMode.ToString();
            }
        }

        /// <summary>
        /// 设置是否显示调试标签
        /// </summary>
        public void SetDebugLabelVisibility(bool isVisible)
        {
            _showDebugLabel = isVisible;
        }

        private void OnDrawGizmos()
        {
            if (!_showDebugLabel)
                return;

            // 绘制实体类型图标
            Gizmos.color = GetEntityColor();
            Vector3 iconPosition = transform.position + Vector3.up * 2f;
            Gizmos.DrawSphere(iconPosition, 0.5f);

            // 通过GUI绘制标签
#if UNITY_EDITOR
            UnityEditor.Handles.BeginGUI();
            Vector3 screenPos = UnityEditor.HandleUtility.WorldToGUIPoint(iconPosition);

            GUIStyle style = new GUIStyle();
            style.normal.textColor = GetEntityColor();
            style.alignment        = TextAnchor.MiddleCenter;
            style.fontSize         = 12;
            style.fontStyle        = FontStyle.Bold;

            // 绘制ID和类型
            UnityEditor.Handles.Label(iconPosition, $"{_entityType} ({_entityId})", style);

            // 绘制关键数据
            if (_keyData.Count > 0)
            {
                string  keyDataStr = string.Join("\n", _keyData.Select(kv => $"{kv.Key}: {kv.Value}"));
                Vector3 dataPos    = iconPosition + Vector3.up * 0.5f;
                UnityEditor.Handles.Label(dataPos, keyDataStr, style);
            }

            UnityEditor.Handles.EndGUI();
#endif
        }

        /// <summary>
        /// 根据实体类型获取颜色
        /// </summary>
        private Color GetEntityColor()
        {
            switch (_entityType)
            {
                case "Unit":
                    var statsComp = _entityRef?.GetComponent<UnitStatsComponent>();
                    if (statsComp != null)
                    {
                        // 根据单位类型返回不同颜色
                        switch (statsComp.UnitType)
                        {
                            case UnitType.Tank:
                                return Color.green;
                            case UnitType.Aircraft:
                                return Color.cyan;
                            case UnitType.Ship:
                                return Color.blue;
                            case UnitType.MassProdRobot:
                                return Color.yellow;
                            case UnitType.Gundam:
                            case UnitType.SolarPowered:
                            case UnitType.Getter:
                            case UnitType.BioAdaptive:
                            case UnitType.Transformer:
                                return Color.magenta; // 超级机器人类型
                        }
                    }

                    return Color.red;

                case "Base":
                    return Color.blue;

                case "Pilot":
                    return Color.yellow;

                default:
                    return Color.white;
            }
        }

        [Button("Refresh")]
        /// <summary>
        /// 更新可视化数据
        /// </summary>
        public void Refresh()
        {
            UpdateComponentsList();
            UpdateKeyData();
        }
    }
}