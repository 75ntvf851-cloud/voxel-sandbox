using UnityEngine;
using System;
using System.Collections.Generic;

namespace VoxelSandbox.Quest
{
    /// <summary>
    /// Quest types for game objectives.
    /// </summary>
    public enum QuestType
    {
        Story,          // Main story quest
        Side,           // Optional side quest
        Tutorial,       // Tutorial/learning quest
        Repeatable      // Repeatable quest
    }
    
    /// <summary>
    /// Quest status tracking.
    /// </summary>
    public enum QuestStatus
    {
        NotStarted,
        Active,
        Completed,
        Failed
    }
    
    /// <summary>
    /// Quest objective that can be tracked.
    /// </summary>
    [Serializable]
    public class QuestObjective
    {
        public string description;
        public int targetCount;
        public int currentCount;
        public bool isCompleted;
        
        public QuestObjective(string description, int targetCount)
        {
            this.description = description;
            this.targetCount = targetCount;
            this.currentCount = 0;
            this.isCompleted = false;
        }
        
        public void UpdateProgress(int amount)
        {
            currentCount = Mathf.Clamp(currentCount + amount, 0, targetCount);
            isCompleted = currentCount >= targetCount;
        }
        
        public float GetProgress()
        {
            return targetCount > 0 ? (float)currentCount / targetCount : 0f;
        }
    }
    
    /// <summary>
    /// Quest definition and tracking.
    /// </summary>
    [Serializable]
    public class Quest
    {
        public string id;
        public string titleKey;             // Localization key for title
        public string descriptionKey;       // Localization key for description
        public QuestType type;
        public QuestStatus status;
        public List<QuestObjective> objectives;
        public List<string> prerequisites;  // Quest IDs that must be completed first
        
        // Rewards
        public int experienceReward;
        public Dictionary<string, int> itemRewards;
        
        public Quest(string id, string titleKey, string descriptionKey, QuestType type)
        {
            this.id = id;
            this.titleKey = titleKey;
            this.descriptionKey = descriptionKey;
            this.type = type;
            this.status = QuestStatus.NotStarted;
            this.objectives = new List<QuestObjective>();
            this.prerequisites = new List<string>();
            this.itemRewards = new Dictionary<string, int>();
        }
        
        public void AddObjective(string description, int targetCount)
        {
            objectives.Add(new QuestObjective(description, targetCount));
        }
        
        public bool CanStart(HashSet<string> completedQuests)
        {
            if (status != QuestStatus.NotStarted)
                return false;
                
            foreach (string prereq in prerequisites)
            {
                if (!completedQuests.Contains(prereq))
                    return false;
            }
            
            return true;
        }
        
        public bool IsCompleted()
        {
            if (objectives.Count == 0)
                return false;
                
            foreach (var objective in objectives)
            {
                if (!objective.isCompleted)
                    return false;
            }
            
            return true;
        }
        
        public float GetOverallProgress()
        {
            if (objectives.Count == 0)
                return 0f;
                
            float totalProgress = 0f;
            foreach (var objective in objectives)
            {
                totalProgress += objective.GetProgress();
            }
            
            return totalProgress / objectives.Count;
        }
    }
    
    /// <summary>
    /// Quest system manager for tracking and managing quests.
    /// Implements the vertical slice game loop objectives.
    /// </summary>
    public class QuestManager : MonoBehaviour
    {
        [Header("Quest Data")]
        [SerializeField] private List<Quest> allQuests = new List<Quest>();
        private Dictionary<string, Quest> questDatabase = new Dictionary<string, Quest>();
        private HashSet<string> completedQuestIds = new HashSet<string>();
        
        [Header("Current Quest")]
        private Quest activeQuest;
        
        // Events
        public event Action<Quest> OnQuestStarted;
        public event Action<Quest> OnQuestCompleted;
        public event Action<Quest, QuestObjective> OnObjectiveUpdated;
        
        private Core.LocalizationService localization;
        private UI.GameHUD gameHUD;
        
        private void Awake()
        {
            // Register with ServiceLocator
            Core.ServiceLocator.Register(this);
            
            // Initialize quest database
            InitializeQuests();
        }
        
        private void Start()
        {
            localization = Core.ServiceLocator.Get<Core.LocalizationService>();
            gameHUD = FindObjectOfType<UI.GameHUD>();
            
            // Load saved quest progress
            LoadQuestProgress();
        }
        
        /// <summary>
        /// Initialize all quests in the game.
        /// This is where we define the vertical slice game loop.
        /// </summary>
        private void InitializeQuests()
        {
            // Main Story Quest: Build Pump Station
            Quest mainQuest = new Quest(
                "main_pump_station",
                "quest_pump_station_title",
                "quest_pump_station_desc",
                QuestType.Story
            );
            mainQuest.AddObjective("quest_obj_gather_materials", 10);
            mainQuest.AddObjective("quest_obj_build_pump", 1);
            mainQuest.AddObjective("quest_obj_build_pipe", 1);
            mainQuest.AddObjective("quest_obj_build_tank", 1);
            mainQuest.experienceReward = 100;
            allQuests.Add(mainQuest);
            
            // Quest 2: Travel to Water Source
            Quest travelQuest = new Quest(
                "travel_to_water",
                "quest_travel_title",
                "quest_travel_desc",
                QuestType.Story
            );
            travelQuest.prerequisites.Add("main_pump_station");
            travelQuest.AddObjective("quest_obj_find_water", 1);
            travelQuest.AddObjective("quest_obj_deploy_pump", 1);
            travelQuest.experienceReward = 50;
            allQuests.Add(travelQuest);
            
            // Quest 3: Pump Water
            Quest pumpQuest = new Quest(
                "pump_water",
                "quest_pump_title",
                "quest_pump_desc",
                QuestType.Story
            );
            pumpQuest.prerequisites.Add("travel_to_water");
            pumpQuest.AddObjective("quest_obj_pump_water", 100);
            pumpQuest.experienceReward = 75;
            allQuests.Add(pumpQuest);
            
            // Quest 4: Return to Base
            Quest returnQuest = new Quest(
                "return_to_base",
                "quest_return_title",
                "quest_return_desc",
                QuestType.Story
            );
            returnQuest.prerequisites.Add("pump_water");
            returnQuest.AddObjective("quest_obj_deliver_water", 1);
            returnQuest.AddObjective("quest_obj_power_base", 1);
            returnQuest.experienceReward = 100;
            allQuests.Add(returnQuest);
            
            // Quest 5: Activate Beacon
            Quest beaconQuest = new Quest(
                "activate_beacon",
                "quest_beacon_title",
                "quest_beacon_desc",
                QuestType.Story
            );
            beaconQuest.prerequisites.Add("return_to_base");
            beaconQuest.AddObjective("quest_obj_activate_beacon", 1);
            beaconQuest.experienceReward = 200;
            allQuests.Add(beaconQuest);
            
            // Build quest database
            foreach (var quest in allQuests)
            {
                questDatabase[quest.id] = quest;
            }
        }
        
        /// <summary>
        /// Start a quest by ID.
        /// </summary>
        public bool StartQuest(string questId)
        {
            if (!questDatabase.ContainsKey(questId))
            {
                Core.Logger.LogWarning($"Quest {questId} not found");
                return false;
            }
            
            Quest quest = questDatabase[questId];
            
            if (!quest.CanStart(completedQuestIds))
            {
                Core.Logger.LogWarning($"Quest {questId} prerequisites not met");
                return false;
            }
            
            quest.status = QuestStatus.Active;
            activeQuest = quest;
            
            OnQuestStarted?.Invoke(quest);
            
            // Update HUD
            if (gameHUD != null && localization != null)
            {
                string title = localization.GetString(quest.titleKey);
                string description = localization.GetString(quest.descriptionKey);
                gameHUD.ShowQuest(title, description, 0f);
            }
            
            Core.Logger.LogInfo($"Quest started: {questId}");
            return true;
        }
        
        /// <summary>
        /// Update objective progress for active quest.
        /// </summary>
        public void UpdateObjective(int objectiveIndex, int amount = 1)
        {
            if (activeQuest == null || activeQuest.status != QuestStatus.Active)
                return;
                
            if (objectiveIndex < 0 || objectiveIndex >= activeQuest.objectives.Count)
                return;
                
            QuestObjective objective = activeQuest.objectives[objectiveIndex];
            objective.UpdateProgress(amount);
            
            OnObjectiveUpdated?.Invoke(activeQuest, objective);
            
            // Update HUD progress
            if (gameHUD != null)
            {
                gameHUD.UpdateQuestProgress(activeQuest.GetOverallProgress());
            }
            
            // Check if quest is completed
            if (activeQuest.IsCompleted())
            {
                CompleteQuest(activeQuest.id);
            }
        }
        
        /// <summary>
        /// Complete a quest.
        /// </summary>
        private void CompleteQuest(string questId)
        {
            if (!questDatabase.ContainsKey(questId))
                return;
                
            Quest quest = questDatabase[questId];
            quest.status = QuestStatus.Completed;
            completedQuestIds.Add(questId);
            
            OnQuestCompleted?.Invoke(quest);
            
            // Grant rewards
            // TODO: Implement reward system
            
            // Hide HUD quest panel
            if (gameHUD != null)
            {
                gameHUD.HideQuest();
            }
            
            // Show completion notification
            if (gameHUD != null && localization != null)
            {
                string completionMessage = localization.GetString("quest_completed", quest.titleKey);
                gameHUD.ShowNotification(completionMessage);
            }
            
            Core.Logger.LogInfo($"Quest completed: {questId}");
            
            // Auto-start next quest in chain if available
            TryStartNextQuest();
        }
        
        /// <summary>
        /// Try to start the next available quest.
        /// </summary>
        private void TryStartNextQuest()
        {
            foreach (var quest in allQuests)
            {
                if (quest.CanStart(completedQuestIds))
                {
                    StartQuest(quest.id);
                    break;
                }
            }
        }
        
        /// <summary>
        /// Get active quest.
        /// </summary>
        public Quest GetActiveQuest()
        {
            return activeQuest;
        }
        
        /// <summary>
        /// Check if quest is completed.
        /// </summary>
        public bool IsQuestCompleted(string questId)
        {
            return completedQuestIds.Contains(questId);
        }
        
        /// <summary>
        /// Save quest progress.
        /// </summary>
        public void SaveQuestProgress()
        {
            // TODO: Implement save system integration
            // For now, just log
            Core.Logger.LogInfo("Quest progress saved");
        }
        
        /// <summary>
        /// Load quest progress.
        /// </summary>
        private void LoadQuestProgress()
        {
            // TODO: Implement save system integration
            // For now, start first quest
            if (allQuests.Count > 0)
            {
                StartQuest(allQuests[0].id);
            }
        }
        
        private void OnDestroy()
        {
            SaveQuestProgress();
            Core.ServiceLocator.Unregister<QuestManager>();
        }
    }
}
