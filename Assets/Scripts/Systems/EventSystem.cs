using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SuperRobot
{
    // 事件系统
    public class EventSystem : GameSystem
    {
        // 存储事件数据
        private List<GameEvent> _activeEvents = new List<GameEvent>();
        private List<string> _completedEventIds = new List<string>();

        // 事件触发概率控制
        private int _turnsSinceLastEvent = 0;
        private Dictionary<string, EventStoryline> _storylines = new Dictionary<string, EventStoryline>();

        public override void Initialize()
        {
            // 加载事件数据
            LoadEvents();

            // 订阅事件
            EventManager.Subscribe<TurnStartedEvent>(OnTurnStarted);
            EventManager.Subscribe<EventChoiceSelectedEvent>(OnEventChoiceSelected);
        }

        public override void Execute()
        {
            // 检查事件进度
            UpdateActiveEvents();
        }

        /// <summary>
        /// 加载事件数据
        /// </summary>
        private void LoadEvents()
        {
            // 从ScriptableObject加载事件数据
            var eventDatabase = Resources.Load<EventDatabase>("Data/EventDatabase");
            if (eventDatabase != null)
            {
                foreach (var storyline in eventDatabase.Storylines)
                {
                    _storylines[storyline.StorylineId] = storyline;
                }
            }
        }

        /// <summary>
        /// 检查并激活随机事件
        /// </summary>
        private void CheckForRandomEvents()
        {
            var gameConfig = GameManager.Instance.GameConfig;

            // 如果活跃事件太多或距离上次事件的时间太短，不触发新事件
            if (_activeEvents.Count >= gameConfig.MaxActiveEvents ||
                _turnsSinceLastEvent < gameConfig.EventCooldownTurns)
            {
                _turnsSinceLastEvent++;
                return;
            }

            // 随机事件触发概率
            float eventChance = gameConfig.RandomEventChance;

            // 随着无事件回合数增加，提高触发概率
            eventChance += (_turnsSinceLastEvent - gameConfig.EventCooldownTurns) * 0.05f;

            // 尝试触发随机事件
            if (UnityEngine.Random.value < eventChance)
            {
                ActivateRandomEvent();
                _turnsSinceLastEvent = 0;
            }
            else
            {
                _turnsSinceLastEvent++;
            }
        }

        /// <summary>
        /// 激活随机事件
        /// </summary>
        private void ActivateRandomEvent()
        {
            // 获取所有可用事件
            List<GameEvent> availableEvents = new List<GameEvent>();

            foreach (var storyline in _storylines.Values)
            {
                // 跳过已完成的剧情线
                if (storyline.OneTimeOnly && IsStorylineCompleted(storyline.StorylineId))
                    continue;

                // 获取剧情线中第一个未完成的事件
                GameEvent nextEvent = GetNextEvent(storyline);
                if (nextEvent != null && AreEventConditionsMet(nextEvent))
                {
                    availableEvents.Add(nextEvent);
                }
            }

            if (availableEvents.Count == 0)
                return;

            // 随机选择一个事件
            GameEvent selectedEvent = availableEvents[UnityEngine.Random.Range(0, availableEvents.Count)];

            // 激活事件
            ActivateEvent(selectedEvent);
        }

        /// <summary>
        /// 获取剧情线中下一个可用事件
        /// </summary>
        private GameEvent GetNextEvent(EventStoryline storyline)
        {
            foreach (var gameEvent in storyline.Events)
            {
                if (!_completedEventIds.Contains(gameEvent.EventId))
                {
                    return gameEvent;
                }
            }

            return null;
        }

        /// <summary>
        /// 检查是否满足事件触发条件
        /// </summary>
        private bool AreEventConditionsMet(GameEvent gameEvent)
        {
            // 检查回合数要求
            int currentTurn = GameManager.Instance.CurrentTurn;
            if (currentTurn < gameEvent.MinTurn || (gameEvent.MaxTurn > 0 && currentTurn > gameEvent.MaxTurn))
                return false;

            // 检查前置事件要求
            foreach (var prerequisiteId in gameEvent.PrerequisiteEvents)
            {
                if (!_completedEventIds.Contains(prerequisiteId))
                    return false;
            }

            // 检查技术要求
            var techSystem = SystemManager.GetSystem<TechTreeSystem>();
            foreach (var requiredTech in gameEvent.RequiredTechnologies)
            {
                if (!techSystem.IsTechResearched(requiredTech))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 激活事件
        /// </summary>
        private void ActivateEvent(GameEvent gameEvent)
        {
            // 复制事件以避免修改原始数据
            GameEvent eventInstance = new GameEvent
            {
                EventId = gameEvent.EventId,
                Title = gameEvent.Title,
                Description = gameEvent.Description,
                Choices = gameEvent.Choices,
                Duration = gameEvent.Duration,
                RemainingTurns = gameEvent.Duration,
                StorylineId = gameEvent.StorylineId,
                Location = gameEvent.Location,
                // 如果需要动态位置，这里可以计算
                EvtType = gameEvent.EvtType
            };

            // 添加到活跃事件列表
            _activeEvents.Add(eventInstance);

            // 触发事件开始事件
            EventManager.TriggerEvent(new EventActivatedEvent
            {
                EventId = eventInstance.EventId,
                Title = eventInstance.Title,
                Description = eventInstance.Description,
                Choices = eventInstance.Choices.Select(c => c.Description).ToList(),
                Location = eventInstance.Location
            });
        }

        /// <summary>
        /// 更新活跃事件
        /// </summary>
        private void UpdateActiveEvents()
        {
            for (int i = _activeEvents.Count - 1; i >= 0; i--)
            {
                var gameEvent = _activeEvents[i];

                // 减少剩余回合数
                if (gameEvent.Duration > 0)
                {
                    gameEvent.RemainingTurns--;

                    // 检查事件是否超时
                    if (gameEvent.RemainingTurns <= 0)
                    {
                        // 触发默认选项（通常是负面结果）
                        if (gameEvent.Choices.Count > 0)
                        {
                            // 找到标记为超时选项的选择，或者使用最后一个选项
                            var timeoutChoice = gameEvent.Choices.FirstOrDefault(c => c.IsTimeoutChoice) ??
                                               gameEvent.Choices.Last();

                            ApplyEventChoice(gameEvent, timeoutChoice);
                        }

                        // 将事件标记为已完成
                        _completedEventIds.Add(gameEvent.EventId);

                        // 从活跃事件中移除
                        _activeEvents.RemoveAt(i);

                        // 触发事件超时事件
                        EventManager.TriggerEvent(new EventTimeoutEvent
                        {
                            EventId = gameEvent.EventId
                        });
                    }
                }
            }
        }

        /// <summary>
        /// 应用事件选择结果
        /// </summary>
        private void ApplyEventChoice(GameEvent gameEvent, EventChoice choice)
        {
            // 应用资源效果
            var resourceSystem = SystemManager.GetSystem<ResourceSystem>();
            foreach (var resourceEffect in choice.ResourceEffects)
            {
                resourceSystem.AddResource(resourceEffect.ResourceType, resourceEffect.Amount);
            }

            // 应用技术效果
            var techSystem = SystemManager.GetSystem<TechTreeSystem>();
            foreach (var techEffect in choice.TechnologyEffects)
            {
                if (techEffect.Unlock)
                {
                    techSystem.UnlockTechnology(techEffect.TechnologyId);
                }
                else if (techEffect.BoostProgress > 0)
                {
                    techSystem.BoostResearch(techEffect.TechnologyId, techEffect.BoostProgress);
                }
            }

            // 应用单位效果
            var unitSystem = SystemManager.GetSystem<UnitManagementSystem>();
            foreach (var unitEffect in choice.UnitEffects)
            {
                if (unitEffect.Spawn)
                {
                    for (int i = 0; i < unitEffect.Amount; i++)
                    {
                        if (!string.IsNullOrEmpty(unitEffect.UnitTemplateId))
                        {
                            unitSystem.CreateUnit(unitEffect.UnitTemplateId, unitEffect.Position);
                        }
                    }
                }
                else if (unitEffect.Damage)
                {
                    // 对特定区域的单位造成伤害
                    var unitsInArea = unitSystem.GetUnitsInArea(unitEffect.Position, unitEffect.Radius);
                    foreach (var unit in unitsInArea)
                    {
                        var statsComp = unit.GetComponent<UnitStatsComponent>();
                        if (statsComp != null)
                        {
                            statsComp.CurrentHealth -= unitEffect.Amount;

                            // 检查单位是否被摧毁
                            if (statsComp.CurrentHealth <= 0)
                            {
                                unitSystem.DestroyUnit((unit.EntityId));
                            }
                        }
                    }
                }
            }

            // 应用驾驶员效果
            var pilotSystem = SystemManager.GetSystem<PilotManagementSystem>();
            foreach (var pilotEffect in choice.PilotEffects)
            {
                if (pilotEffect.Recruit)
                {
                    // 创建新驾驶员
                    pilotSystem.CreatePilot(pilotEffect.Name, pilotEffect.Stats);
                }
                else if (pilotEffect.ApplyToAll)
                {
                    // 对所有驾驶员应用效果
                    var allPilots = pilotSystem.GetAllPilots();
                    foreach (var pilot in allPilots)
                    {
                        var statsComp = pilot.GetComponent<PilotStatsComponent>();
                        if (statsComp != null)
                        {
                            if (pilotEffect.ExperienceGain > 0)
                            {
                                statsComp.AddExperience(pilotEffect.ExperienceGain);
                            }

                            if (pilotEffect.FatigueChange != 0)
                            {
                                if (pilotEffect.FatigueChange > 0)
                                    statsComp.AddFatigue(pilotEffect.FatigueChange);
                                else
                                    statsComp.RecoverFatigue(-pilotEffect.FatigueChange);
                            }
                        }
                    }
                }
            }

            // 触发后续事件
            if (!string.IsNullOrEmpty(choice.TriggerEventId))
            {
                foreach (var storyline in _storylines.Values)
                {
                    foreach (var nextEvent in storyline.Events)
                    {
                        if (nextEvent.EventId == choice.TriggerEventId)
                        {
                            ActivateEvent(nextEvent);
                            break;
                        }
                    }
                }
            }

            // 触发事件选择应用事件
            EventManager.TriggerEvent(new EventChoiceAppliedEvent
            {
                EventId = gameEvent.EventId,
                ChoiceIndex = Array.IndexOf(gameEvent.Choices.ToArray(), choice),
                ResultMessage = choice.ResultDescription
            });
        }

        /// <summary>
        /// 检查剧情线是否已完成
        /// </summary>
        private bool IsStorylineCompleted(string storylineId)
        {
            if (!_storylines.TryGetValue(storylineId, out var storyline))
                return false;

            // 检查剧情线中的所有事件是否都已完成
            foreach (var gameEvent in storyline.Events)
            {
                if (!_completedEventIds.Contains(gameEvent.EventId))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 获取当前活跃事件
        /// </summary>
        public List<GameEvent> GetActiveEvents()
        {
            return new List<GameEvent>(_activeEvents);
        }

        /// <summary>
        /// 获取已完成事件ID
        /// </summary>
        public List<string> GetCompletedEvents()
        {
            return new List<string>(_completedEventIds);
        }

        /// <summary>
        /// 事件处理程序
        /// </summary>
        private void OnTurnStarted(TurnStartedEvent evt)
        {
            // 在回合开始时检查随机事件
            if (evt.Phase == TurnPhase.Event)
            {
                CheckForRandomEvents();
            }
        }

        private void OnEventChoiceSelected(EventChoiceSelectedEvent evt)
        {
            // 根据选择处理事件
            var gameEvent = _activeEvents.Find(e => e.EventId == evt.EventId);
            if (gameEvent != null && evt.ChoiceIndex >= 0 && evt.ChoiceIndex < gameEvent.Choices.Count)
            {
                var choice = gameEvent.Choices[evt.ChoiceIndex];

                // 应用选择结果
                ApplyEventChoice(gameEvent, choice);

                // 将事件标记为已完成
                _completedEventIds.Add(gameEvent.EventId);

                // 从活跃事件中移除
                _activeEvents.Remove(gameEvent);
            }
        }

        /// <summary>
        /// 加载事件数据
        /// </summary>
        public void LoadEventsData(List<string> completedEvents, List<string> activeEventIds)
        {
            _completedEventIds = new List<string>(completedEvents);

            // 清空当前活跃事件
            _activeEvents.Clear();

            // 加载活跃事件
            foreach (var eventId in activeEventIds)
            {
                foreach (var storyline in _storylines.Values)
                {
                    foreach (var gameEvent in storyline.Events)
                    {
                        if (gameEvent.EventId == eventId)
                        {
                            // 创建事件实例并添加到活跃事件列表
                            GameEvent eventInstance = new GameEvent
                            {
                                EventId = gameEvent.EventId,
                                Title = gameEvent.Title,
                                Description = gameEvent.Description,
                                Choices = gameEvent.Choices,
                                Duration = gameEvent.Duration,
                                RemainingTurns = gameEvent.Duration, // 这里应该从存档中加载剩余回合数
                                StorylineId = gameEvent.StorylineId,
                                Location = gameEvent.Location,
                                EvtType = gameEvent.EvtType
                            };

                            _activeEvents.Add(eventInstance);
                            break;
                        }
                    }
                }
            }
        }

        public override void Cleanup()
        {
            // 取消订阅事件
            EventManager.Instance.Unsubscribe<TurnStartedEvent>(OnTurnStarted);
            EventManager.Instance.Unsubscribe<EventChoiceSelectedEvent>(OnEventChoiceSelected);
        }
    }

    // 事件相关类型
    [Serializable]
    public class EventStoryline
    {
        public string StorylineId;
        public string Name;
        public string Description;
        public bool OneTimeOnly;
        public List<GameEvent> Events;
    }

    [Serializable]
    public class GameEvent
    {
        public string EventId;
        public string Title;
        public string Description;
        public List<EventChoice> Choices;
        public int Duration; // 事件持续回合数，0表示无限期
        public int RemainingTurns; // 剩余回合数
        public string StorylineId;
        public Vector2Int Location; // 事件发生位置（如适用）
        public int MinTurn; // 最早可触发回合
        public int MaxTurn; // 最晚可触发回合，0表示无限制
        public List<string> PrerequisiteEvents; // 前置事件
        public List<string> RequiredTechnologies; // 要求已研发的技术
        public EventType EvtType;

        public enum EventType
        {
            Story,      // 剧情事件
            Random,     // 随机事件
            Disaster,   // 灾难事件
            Discovery,  // 发现事件
            Character   // 角色事件
        }
    }

    [Serializable]
    public class EventChoice
    {
        public string Description;
        public string ResultDescription;
        public bool IsTimeoutChoice; // 事件超时时是否自动选择
        public List<ResourceEffect> ResourceEffects;
        public List<TechnologyEffect> TechnologyEffects;
        public List<UnitEffect> UnitEffects;
        public List<PilotEffect> PilotEffects;
        public string TriggerEventId; // 选择后触发的事件
    }

    [Serializable]
    public class ResourceEffect
    {
        public ResourceType ResourceType;
        public int Amount;
    }

    [Serializable]
    public class TechnologyEffect
    {
        public string TechnologyId;
        public bool Unlock;
        public int BoostProgress;
    }

    [Serializable]
    public class UnitEffect
    {
        public string UnitTemplateId;
        public bool Spawn;
        public bool Damage;
        public int Amount;
        public Vector2Int Position;
        public int Radius;
    }

    [Serializable]
    public class PilotEffect
    {
        public bool Recruit;
        public bool ApplyToAll;
        public string Name;
        public Dictionary<string, int> Stats;
        public int ExperienceGain;
        public int FatigueChange;
    }

    
}
