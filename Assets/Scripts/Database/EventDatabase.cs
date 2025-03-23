using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SuperRobot
{
    /// <summary>
    /// 事件数据库，存储所有事件剧情线
    /// </summary>
    [CreateAssetMenu(fileName = "EventDatabase", menuName = "Game/EventDatabase")]
    public class EventDatabase : ScriptableObject
    {
        [SerializeField] private List<EventStoryline> _storylines = new List<EventStoryline>();

        public List<EventStoryline> Storylines => _storylines;

        /// <summary>
        /// 获取指定ID的剧情线
        /// </summary>
        public EventStoryline GetStoryline(string storylineId)
        {
            return _storylines.Find(s => s.StorylineId == storylineId);
        }

        /// <summary>
        /// 获取指定ID的事件
        /// </summary>
        public GameEvent GetEvent(string eventId)
        {
            foreach (var storyline in _storylines)
            {
                foreach (var gameEvent in storyline.Events)
                {
                    if (gameEvent.EventId == eventId)
                    {
                        return gameEvent;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 获取所有随机事件
        /// </summary>
        public List<GameEvent> GetRandomEvents()
        {
            List<GameEvent> randomEvents = new List<GameEvent>();

            foreach (var storyline in _storylines)
            {
                foreach (var gameEvent in storyline.Events)
                {
                    if (gameEvent.EvtType == GameEvent.EventType.Random)
                    {
                        randomEvents.Add(gameEvent);
                    }
                }
            }

            return randomEvents;
        }

        /// <summary>
        /// 获取所有灾难事件
        /// </summary>
        public List<GameEvent> GetDisasterEvents()
        {
            List<GameEvent> disasterEvents = new List<GameEvent>();

            foreach (var storyline in _storylines)
            {
                foreach (var gameEvent in storyline.Events)
                {
                    if (gameEvent.EvtType == GameEvent.EventType.Disaster)
                    {
                        disasterEvents.Add(gameEvent);
                    }
                }
            }

            return disasterEvents;
        }

        [Button("初始化默认事件")]
        // 示例事件数据初始化方法（仅用于编辑器）
        public void InitializeDefaultEvents()
        {
            _storylines.Clear();

            // 外星人入侵剧情线
            EventStoryline alienInvasion = new EventStoryline
            {
                StorylineId = "alien_invasion",
                Name = "外星人入侵",
                Description = "地球面临来自外星文明的入侵威胁",
                OneTimeOnly = true,
                Events = new List<GameEvent>()
            };

            // 添加事件到剧情线
            alienInvasion.Events.Add(new GameEvent
            {
                EventId = "alien_first_contact",
                Title = "首次接触",
                Description = "天文台探测到不明飞行物接近地球轨道，这可能是人类首次与外星文明接触。",
                Choices = new List<EventChoice>
            {
                new EventChoice
                {
                    Description = "派出研究小组调查",
                    ResultDescription = "研究小组成功收集了外星文明的基本数据，这将有助于未来的防御和交流。",
                    ResourceEffects = new List<ResourceEffect>
                    {
                        new ResourceEffect { ResourceType = ResourceType.Money, Amount = -500 }
                    },
                    TechnologyEffects = new List<TechnologyEffect>
                    {
                        new TechnologyEffect { TechnologyId = "alien_communication_tech", Unlock = true }
                    }
                },
                new EventChoice
                {
                    Description = "派出军事力量戒备",
                    ResultDescription = "军事部队进入高度戒备状态，虽然消耗了大量资源，但增强了防御力。",
                    ResourceEffects = new List<ResourceEffect>
                    {
                        new ResourceEffect { ResourceType = ResourceType.Money, Amount = -1000 },
                        new ResourceEffect { ResourceType = ResourceType.StandardAlloy, Amount = -100 }
                    }
                },
                new EventChoice
                {
                    Description = "尝试建立通信",
                    ResultDescription = "通信尝试似乎被接收了，但没有明确回应。这次接触引起了公众的广泛关注。",
                    TriggerEventId = "alien_communication_attempt"
                }
            },
                Duration = 5,
                StorylineId = "alien_invasion",
                Location = new Vector2Int(50, 50),
                MinTurn = 10,
                MaxTurn = 0,
                PrerequisiteEvents = new List<string>(),
                RequiredTechnologies = new List<string>(),
                EvtType = GameEvent.EventType.Story
            });

            // 添加更多事件...

            _storylines.Add(alienInvasion);

            // 添加更多剧情线...
        }
    }
}
