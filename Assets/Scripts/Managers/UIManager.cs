using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SuperRobot
{
    public class UIManager : MonoBehaviour
    {
        private static UIManager _instance;
        public static UIManager Instance => _instance;

        public GameObject m_HintInfo;

        public GameObject m_Selections;

        public TMP_Text m_ResourceInfo;

        public GameObject m_Description;

        private IMapManager _mapManager => GameManager.Instance.MapManager;


        private void Awake()
        {
            _instance = this;
        }
        public void HighlightCells(List<Vector2Int> hexes, Color color)
        {
            _mapManager.HighlightCells(hexes);
        }

        public void ClearHighlights()
        {
            _mapManager.ClearHighlightCells();
        }

        public void ShowPath(List<Vector2Int> path)
        {
            // 显示路径
            _mapManager.ShowPath(path);
        }

        public void UpdateUnitInfo(int unitId)
        {
            // 更新单位信息
            m_Description.SetActive(true);
            var unit = GameManager.Instance.EntityManager.GetEntity(unitId);
            var stats = unit.GetComponent<UnitStatsComponent>();
            var pilot = unit.GetComponent<PilotComponent>();
            m_Description.GetComponentInChildren<TMP_Text>().text = $"单位名称: {stats.UnitName}\n" +
                $"单位驾驶员: {pilot?.PilotName}\n" +
                $"单位类型: {stats.UnitType}\n" +
                $"单位生命值: {stats.MaxHealth}\n" +
                $"单位能量值: {stats.MaxEnergy}\n" +
                $"单位移动范围: {stats.MovementRange}\n" +
                $"单位行动点: {stats.MaxActionPoints}\n" +
                $"单位移动加成: {stats.MovementBonus}\n" +
                $"单位攻击加成: {stats.RangedAttackBonus}\n" +
                $"单位防御加成: {stats.MeleeAttackBonus}\n";
        }

        public void ShowSimpleInfo(string info)
        {
            var componentInChildren = m_HintInfo.GetComponentInChildren<TMP_Text>();
            if (string.IsNullOrEmpty(info))
            {
                componentInChildren.text = "";
                return;
            }

            componentInChildren.text = info;
        }

        public void ShowSelections(List<string> selections,Action<int> onSelection)
        {
            // 显示选择
            m_Selections.SetActive(true);
            // 子物体先全部隐藏
            foreach (Transform child in m_Selections.transform)
            {
                child.gameObject.SetActive(false);
            }
            // 根据selections的数量显示对应的子物体
            for (int i = 0; i < selections.Count; i++)
            {
                m_Selections.transform.GetChild(i).gameObject.SetActive(true);
                m_Selections.transform.GetChild(i).GetComponentInChildren<TMP_Text>().text = selections[i];
                var index     = i;
                var component = m_Selections.transform.GetChild(i).GetComponent<Button>();
                component.onClick.RemoveAllListeners();
                component.onClick.AddListener(() => {
                    onSelection(index);
                    // 下一帧隐藏
                    
                    m_Selections.SetActive(false);
                });
            }
        }

        public void UpdateResourceInfo(int resource)
        {
            m_ResourceInfo.text = resource.ToString();
        }
    }
}
