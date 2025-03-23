using System;
using System.Collections.Generic;
using UnityEngine;

namespace SuperRobot
{
    public class DebugConsole : MonoBehaviour
    {
        private bool _showConsole = false;
        private string _input = "";
        private List<string> _logMessages = new List<string>();

        private void OnGUI()
        {
            if (!_showConsole)
                return;

            // 绘制控制台窗口
            GUI.Box(new Rect(0, 0, Screen.width, Screen.height * 0.3f), "Debug Console");

            // 命令输入
            GUI.Label(new Rect(10, Screen.height * 0.3f - 30, 50, 20), "Command:");
            _input = GUI.TextField(new Rect(70, Screen.height * 0.3f - 30, Screen.width - 80, 20), _input);

            if (GUI.Button(new Rect(Screen.width - 80, Screen.height * 0.3f - 30, 70, 20), "Execute") ||
                (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Return))
            {
                ExecuteCommand(_input);
                _input = "";
            }

            // 显示日志
            float yPos = 20;
            foreach (var message in _logMessages)
            {
                GUI.Label(new Rect(10, yPos, Screen.width - 20, 20), message);
                yPos += 20;
            }
        }

        private void Update()
        {
            // 按Tab键切换控制台显示
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                _showConsole = !_showConsole;
            }
        }

        private void ExecuteCommand(string command)
        {
            _logMessages.Add("> " + command);

            // 解析命令
            string[] parts = command.Split(' ');
            if (parts.Length == 0)
                return;

            switch (parts[0].ToLower())
            {
                case "addresource":
                    if (parts.Length >= 3 && Enum.TryParse(parts[1], out ResourceType resType) && int.TryParse(parts[2], out int amount))
                    {
                        var resourceSystem = GameManager.Instance.SystemManager.GetSystem<ResourceSystem>();
                        resourceSystem.AddResource(resType, amount);
                        _logMessages.Add($"Added {amount} {resType}");
                    }
                    break;

                case "nextturn":
                    TurnManager.NextTurn();
                    _logMessages.Add("Advanced to next turn");
                    break;

                    // 添加更多调试命令...
            }
        }
    }
}