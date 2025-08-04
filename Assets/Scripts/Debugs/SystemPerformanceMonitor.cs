using UnityEngine;
using SuperRobot.Core;
using SystemInfo = SuperRobot.Core.SystemInfo;

namespace SuperRobot.Debugs
{
    /// <summary>
    /// 系统性能监控器 - 用于调试和优化
    /// </summary>
    public class SystemPerformanceMonitor : MonoBehaviour
    {
        [Header("监控设置")]
        public bool EnableMonitoring = true;
        public float UpdateInterval = 1f;
        public bool ShowInConsole = false;
        public bool ShowOnScreen = true;

        [Header("显示设置")]
        public KeyCode ToggleKey = KeyCode.F1;
        public int FontSize = 14;
        public Color TextColor = Color.white;
        public Color BackgroundColor = new Color(0, 0, 0, 0.7f);

        private ImprovedSystemManager _systemManager;
        private float _lastUpdateTime;
        private bool _showUI = false;
        private GUIStyle _textStyle;
        private GUIStyle _backgroundStyle;

        private void Start()
        {
            _systemManager = GameManager.Instance.ImprovedSystemManager;
            
            if (_systemManager != null)
            {
                _systemManager.EnableProfiling = EnableMonitoring;
            }

            InitializeGUIStyles();
        }

        private void InitializeGUIStyles()
        {
            _textStyle = new GUIStyle();
            _textStyle.fontSize = FontSize;
            _textStyle.normal.textColor = TextColor;
            _textStyle.wordWrap = false;

            _backgroundStyle = new GUIStyle();
            _backgroundStyle.normal.background = CreateTexture(BackgroundColor);
        }

        private Texture2D CreateTexture(Color color)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }

        private void Update()
        {
            if (Input.GetKeyDown(ToggleKey))
            {
                _showUI = !_showUI;
            }

            if (EnableMonitoring && Time.time - _lastUpdateTime > UpdateInterval)
            {
                _lastUpdateTime = Time.time;
                
                if (ShowInConsole)
                {
                    LogPerformanceStats();
                }
            }
        }

        private void LogPerformanceStats()
        {
            if (_systemManager == null) return;

            _systemManager.PrintPerformanceStats();
        }

        private void OnGUI()
        {
            if (!_showUI || !EnableMonitoring || _systemManager == null) return;

            var systemInfos = _systemManager.GetAllSystemInfos();
            if (systemInfos.Count == 0) return;

            float windowWidth = 400f;
            float windowHeight = 300f;
            Rect windowRect = new Rect(10, 10, windowWidth, windowHeight);

            // 绘制背景
            GUI.Box(windowRect, "", _backgroundStyle);

            GUILayout.BeginArea(windowRect);
            GUILayout.BeginVertical();

            GUILayout.Label("系统性能监控", _textStyle);
            GUILayout.Space(10);

            foreach (var phaseKvp in systemInfos)
            {
                var phase = phaseKvp.Key;
                var systems = phaseKvp.Value;

                if (systems.Count == 0) continue;

                var phaseTime = _systemManager.GetPhaseExecutionTime(phase);
                GUILayout.Label($"{phase} 阶段: {phaseTime * 1000:F2}ms", _textStyle);

                foreach (var systemInfo in systems)
                {
                    if (!systemInfo.IsEnabled) continue;

                    var statusColor = GetSystemStatusColor(systemInfo);
                    var originalColor = _textStyle.normal.textColor;
                    _textStyle.normal.textColor = statusColor;

                    var systemText = $"  • {systemInfo.SystemType.Name}: {systemInfo.LastExecutionTime * 1000:F2}ms";
                    if (systemInfo.ExecutionCount > 0)
                    {
                        systemText += $" ({systemInfo.ExecutionCount})";
                    }

                    GUILayout.Label(systemText, _textStyle);
                    _textStyle.normal.textColor = originalColor;
                }

                GUILayout.Space(5);
            }

            GUILayout.FlexibleSpace();
            GUILayout.Label($"按 {ToggleKey} 切换显示", _textStyle);

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private Color GetSystemStatusColor(SystemInfo systemInfo)
        {
            // 根据系统执行时间返回不同颜色
            var executionTimeMs = systemInfo.LastExecutionTime * 1000;

            if (executionTimeMs > 10f) return Color.red;      // 超过10ms显示红色
            if (executionTimeMs > 5f) return Color.yellow;    // 超过5ms显示黄色
            if (executionTimeMs > 1f) return Color.green;     // 超过1ms显示绿色
            
            return Color.white; // 默认白色
        }

        private void OnDestroy()
        {
            if (_backgroundStyle?.normal?.background != null)
            {
                DestroyImmediate(_backgroundStyle.normal.background);
            }
        }

        /// <summary>
        /// 切换监控状态
        /// </summary>
        public void ToggleMonitoring()
        {
            EnableMonitoring = !EnableMonitoring;
            
            if (_systemManager != null)
            {
                _systemManager.EnableProfiling = EnableMonitoring;
            }
        }

        /// <summary>
        /// 重置性能统计
        /// </summary>
        public void ResetStats()
        {
            // 这里可以添加重置统计的逻辑
            Debug.Log("Performance stats reset");
        }
    }
}