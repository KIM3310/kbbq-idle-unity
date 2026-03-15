using System;
using System.Collections.Generic;

public class StoryQuestSystem
{
    private readonly SaveData saveData;
    private readonly Queue<StoryQuestUpdate> pendingUpdates = new Queue<StoryQuestUpdate>();
    private int currentTierIndex;
    private int currentPlayerLevel;
    private int currentPrestigeLevel;
    private bool currentCanPrestige;
    private double rewardSeed = 1d;

    private static readonly StoryQuestDefinition[] Definitions =
    {
        new StoryQuestDefinition("alley_first_regulars", "alley", 0, "ACT I · SMOKE IN THE ALLEY", "First Regulars", "Mina", "Close six plates before midnight so the alley keeps talking about your grill.", "Mina", "The alley finally starts saying your name out loud.", StoryObjectiveType.ServeOrders, 6, 18),
        new StoryQuestDefinition("alley_keep_coals", "alley", 0, "ACT I · SMOKE IN THE ALLEY", "Keep The Coals Alive", "Mr. Han", "Hit the grill boost twice to prove you can survive the first rush.", "Mr. Han", "The first rush bends around your rhythm instead of breaking it.", StoryObjectiveType.UseBoosts, 2, 20),
        new StoryQuestDefinition("alley_house_special", "alley", 0, "ACT I · SMOKE IN THE ALLEY", "House Special Rumor", "Jisu", "Serve today's special four times and let the block find its first obsession.", "Jisu", "Now there is a secret menu whisper traveling through the alley.", StoryObjectiveType.DailySpecialServes, 4, 22),
        new StoryQuestDefinition("hongdae_neon_arrival", "hongdae", 1, "ACT II · NEON QUEUE", "Neon Arrival", "DJ Yuna", "Break out of the alley and bring the grill to Hongdae after dark.", "DJ Yuna", "The pop-up is real now. The crowd expects a show, not just a meal.", StoryObjectiveType.ReachDistrict, 1, 24),
        new StoryQuestDefinition("hongdae_open_mic", "hongdae", 1, "ACT II · NEON QUEUE", "Open Mic Crowd", "MC Dae", "Serve two spotlight tables and turn the queue into a rumor machine.", "MC Dae", "A loud room is starting to orbit your table.", StoryObjectiveType.SpotlightServes, 2, 24),
        new StoryQuestDefinition("gangnam_premium_arrival", "gangnam", 2, "ACT III · PREMIUM FIRE", "Velvet Rope Service", "Manager Seo", "Push the restaurant into Gangnam and take the late-night premium crowd seriously.", "Manager Seo", "You are no longer fighting for attention. Now you are expected to deliver.", StoryObjectiveType.ReachDistrict, 2, 28),
        new StoryQuestDefinition("gangnam_silver_service", "gangnam", 2, "ACT III · PREMIUM FIRE", "Silver Service Timing", "Hana", "Land four perfect serves while the premium room is watching.", "Hana", "Precision became the brand. That changes what people will pay.", StoryObjectiveType.PerfectServes, 4, 28),
        new StoryQuestDefinition("gangnam_expansion", "gangnam", 2, "ACT III · PREMIUM FIRE", "Expand The House", "Manager Seo", "Buy four upgrades and turn the line into a polished operation.", "Manager Seo", "The kitchen is starting to feel like a machine built to impress.", StoryObjectiveType.BuyUpgrades, 4, 30),
        new StoryQuestDefinition("hanok_fire_arrival", "hanok", 3, "ACT IV · FIREKEEPER", "Hanok Firekeeper", "Master Hyeon", "Move into Hanok and prove your grill can carry heritage, not just noise.", "Master Hyeon", "The house gets quieter here. Every mistake gets louder.", StoryObjectiveType.ReachDistrict, 3, 34),
        new StoryQuestDefinition("hanok_ember_ritual", "hanok", 3, "ACT IV · FIREKEEPER", "Ember Ritual", "Master Hyeon", "Trigger Chef Fever twice and show that control can still look dramatic.", "Master Hyeon", "The room sees discipline now, not panic.", StoryObjectiveType.TriggerChefFever, 2, 34),
        new StoryQuestDefinition("hanok_signature", "hanok", 3, "ACT IV · FIREKEEPER", "Signature Ritual", "Grandmother Ok", "Serve today's special five times and make it feel ceremonial.", "Grandmother Ok", "The special stopped feeling seasonal. It started feeling inevitable.", StoryObjectiveType.DailySpecialServes, 5, 36),
        new StoryQuestDefinition("global_world_arrival", "global", 4, "ACT V · WORLD STAGE", "World Stage", "Producer Niko", "Unlock the global district and take your local heat onto a bigger floor.", "Producer Niko", "Now every table feels like a different city with the same hunger.", StoryObjectiveType.ReachDistrict, 4, 42),
        new StoryQuestDefinition("global_headliner", "global", 4, "ACT V · WORLD STAGE", "Headliner Tables", "Amira", "Serve four spotlight tables and make the whole room feel like prime time.", "Amira", "The restaurant has become an event instead of a stop.", StoryObjectiveType.SpotlightServes, 4, 42),
        new StoryQuestDefinition("global_finale", "global", 4, "ACT V · WORLD STAGE", "Season Finale", "Producer Niko", "Push this run until prestige is ready. The next season is almost in your hands.", "Producer Niko", "The house has gone as far as this season can take it. Time to relaunch bigger.", StoryObjectiveType.ReachPrestigeReady, 1, 48),
        new StoryQuestDefinition("global_relaunch", "global", 4, "ACT V · WORLD STAGE", "Restart Stronger", "Amira", "Prestige once and prove this brand survives the reset.", "Amira", "You did not start over. You came back larger than the last version of yourself.", StoryObjectiveType.PrestigeTimes, 1, 56),
    };

    public StoryQuestSystem(SaveData saveData)
    {
        this.saveData = saveData ?? new SaveData();
    }

    public void SyncMetaState(int tierIndex, int playerLevel, bool canPrestige, int prestigeLevel, double rewardSeed, bool emitUpdates)
    {
        currentTierIndex = Math.Max(0, tierIndex);
        currentPlayerLevel = Math.Max(1, playerLevel);
        currentCanPrestige = canPrestige;
        currentPrestigeLevel = Math.Max(0, prestigeLevel);
        this.rewardSeed = Math.Max(1d, rewardSeed);

        EnsureDefinitions();
        SyncUnlocksAndAutoProgress(emitUpdates);
    }

    public void RecordServe(int servings, bool perfectServe, bool dailySpecialServed, bool spotlightServed)
    {
        var active = GetActiveQuest();
        if (active == null)
        {
            return;
        }

        switch (active.objectiveType)
        {
            case StoryObjectiveType.ServeOrders:
                active.progress = Math.Min(active.target, active.progress + Math.Max(1, servings));
                break;
            case StoryObjectiveType.PerfectServes:
                if (perfectServe)
                {
                    active.progress = Math.Min(active.target, active.progress + 1);
                }
                break;
            case StoryObjectiveType.DailySpecialServes:
                if (dailySpecialServed)
                {
                    active.progress = Math.Min(active.target, active.progress + 1);
                }
                break;
            case StoryObjectiveType.SpotlightServes:
                if (spotlightServed)
                {
                    active.progress = Math.Min(active.target, active.progress + 1);
                }
                break;
        }

        FinishActiveQuestIfReady(true);
    }

    public void RecordBoost()
    {
        RecordSimpleProgress(StoryObjectiveType.UseBoosts);
    }

    public void RecordUpgrade()
    {
        RecordSimpleProgress(StoryObjectiveType.BuyUpgrades);
    }

    public void RecordChefFever()
    {
        RecordSimpleProgress(StoryObjectiveType.TriggerChefFever);
    }

    public void RecordPrestige()
    {
        RecordSimpleProgress(StoryObjectiveType.PrestigeTimes);
    }

    public StoryQuestUiState GetUiState()
    {
        EnsureDefinitions();
        var active = GetActiveQuest();
        if (active != null)
        {
            return new StoryQuestUiState
            {
                actTitle = active.actTitle,
                chapterTitle = active.chapterTitle,
                speakerName = active.speakerName,
                narrative = active.briefing,
                objectiveLine = BuildObjectiveLine(active),
                rewardLine = "Story Reward +" + FormatCurrency(active.rewardCurrency),
                statusLine = BuildStatusLine(active),
                accent01 = ComputeAccent(active),
                visible = true,
            };
        }

        var nextLocked = GetNextLockedQuest();
        if (nextLocked != null)
        {
            return new StoryQuestUiState
            {
                actTitle = nextLocked.actTitle,
                chapterTitle = nextLocked.chapterTitle,
                speakerName = nextLocked.speakerName,
                narrative = nextLocked.briefing,
                objectiveLine = BuildUnlockHint(nextLocked),
                rewardLine = "Story Reward +" + FormatCurrency(nextLocked.rewardCurrency),
                statusLine = "LOCKED",
                accent01 = 0.26f,
                visible = true,
            };
        }

        return new StoryQuestUiState
        {
            actTitle = "STORY COMPLETE",
            chapterTitle = "All District Arcs Resolved",
            speakerName = "The House",
            narrative = "You carried the grill from alley rumor to global headline. The next prestige run is about style, speed, and higher stakes.",
            objectiveLine = "Keep relaunching and refining the house legend.",
            rewardLine = "All chapter rewards claimed",
            statusLine = "COMPLETE",
            accent01 = 1f,
            visible = true,
        };
    }

    public StoryLogUiState GetStoryLogUiState()
    {
        if (saveData.storyLog != null && saveData.storyLog.Count > 0)
        {
            var entry = saveData.storyLog[saveData.storyLog.Count - 1];
            return new StoryLogUiState
            {
                headline = string.IsNullOrEmpty(entry.headline) ? "STORY LOG" : entry.headline,
                speaker = entry.speaker,
                line = entry.line,
                accent01 = 0.72f,
                visible = true,
            };
        }

        var active = GetActiveQuest();
        if (active != null)
        {
            return new StoryLogUiState
            {
                headline = active.chapterTitle,
                speaker = active.speakerName,
                line = active.briefing,
                accent01 = ComputeAccent(active),
                visible = true,
            };
        }

        return default;
    }

    public bool TryDequeueUpdate(out StoryQuestUpdate update)
    {
        if (pendingUpdates.Count > 0)
        {
            update = pendingUpdates.Dequeue();
            return true;
        }

        update = default;
        return false;
    }

    private void RecordSimpleProgress(StoryObjectiveType type)
    {
        var active = GetActiveQuest();
        if (active == null || active.objectiveType != type)
        {
            return;
        }

        active.progress = Math.Min(active.target, active.progress + 1);
        FinishActiveQuestIfReady(true);
    }

    private void EnsureDefinitions()
    {
        var existing = new Dictionary<string, StoryQuestState>(StringComparer.OrdinalIgnoreCase);
        if (saveData.storyQuests != null)
        {
            for (int i = 0; i < saveData.storyQuests.Count; i++)
            {
                var quest = saveData.storyQuests[i];
                if (quest != null && !string.IsNullOrEmpty(quest.id))
                {
                    existing[quest.id] = quest;
                }
            }
        }

        saveData.storyQuests = new List<StoryQuestState>();
        for (int i = 0; i < Definitions.Length; i++)
        {
            var def = Definitions[i];
            var quest = existing.TryGetValue(def.id, out var prior)
                ? prior
                : new StoryQuestState();

            quest.id = def.id;
            quest.districtId = def.districtId;
            quest.actTitle = def.actTitle;
            quest.chapterTitle = def.chapterTitle;
            quest.speakerName = def.unlockSpeaker;
            quest.briefing = def.briefing;
            quest.completionText = def.completionText;
            quest.objectiveType = def.objectiveType;
            quest.target = def.target;
            quest.requiredTierIndex = def.requiredTierIndex;
            if (quest.rewardCurrency <= 0d)
            {
                quest.rewardCurrency = Math.Max(30d, rewardSeed * def.rewardMultiplier);
            }
            quest.progress = Math.Min(quest.target, Math.Max(0d, quest.progress));
            saveData.storyQuests.Add(quest);
        }

        if (saveData.storyQuests.Count > 0 && !AnyUnlockedIncompleteQuest())
        {
            saveData.storyQuests[0].unlocked = true;
        }
    }

    private void SyncUnlocksAndAutoProgress(bool emitUpdates)
    {
        var guard = 0;
        while (guard++ < Definitions.Length + 3)
        {
            var unlocked = UnlockNextEligible(emitUpdates);
            var active = GetActiveQuest();
            if (active == null)
            {
                return;
            }

            if (IsMetaSatisfied(active))
            {
                CompleteQuest(active, emitUpdates);
                continue;
            }

            if (!unlocked)
            {
                return;
            }
        }
    }

    private bool UnlockNextEligible(bool emitUpdates)
    {
        for (int i = 0; i < saveData.storyQuests.Count; i++)
        {
            var quest = saveData.storyQuests[i];
            if (quest == null || quest.unlocked || quest.completed)
            {
                continue;
            }

            var previousComplete = i == 0 || (saveData.storyQuests[i - 1] != null && saveData.storyQuests[i - 1].completed);
            if (!previousComplete || currentTierIndex < quest.requiredTierIndex)
            {
                return false;
            }

            quest.unlocked = true;
            if (emitUpdates)
            {
                var speaker = Definitions[i].unlockSpeaker;
                AppendLog(quest.id + "_unlock", speaker, quest.chapterTitle, quest.briefing, quest.districtId);
                pendingUpdates.Enqueue(new StoryQuestUpdate
                {
                    title = "NEW CHAPTER",
                    detail = quest.chapterTitle + " · " + quest.briefing,
                    speakerName = speaker,
                    rewardCurrency = 0,
                    accent01 = 0.56f,
                });
            }
            return true;
        }

        return false;
    }

    private void FinishActiveQuestIfReady(bool emitUpdates)
    {
        var active = GetActiveQuest();
        if (active == null)
        {
            return;
        }

        if (active.progress >= active.target)
        {
            CompleteQuest(active, emitUpdates);
            SyncUnlocksAndAutoProgress(emitUpdates);
        }
    }

    private void CompleteQuest(StoryQuestState quest, bool emitUpdates)
    {
        if (quest == null || quest.completed)
        {
            return;
        }

        quest.progress = Math.Max(quest.target, quest.progress);
        quest.completed = true;
        if (emitUpdates)
        {
            quest.speakerName = DefinitionsForId(quest.id)?.completionSpeaker ?? quest.speakerName;
            AppendLog(quest.id + "_complete", quest.speakerName, quest.chapterTitle, quest.completionText, quest.districtId);
            pendingUpdates.Enqueue(new StoryQuestUpdate
            {
                title = quest.chapterTitle.ToUpperInvariant(),
                detail = quest.completionText,
                speakerName = quest.speakerName,
                rewardCurrency = quest.rewardCurrency,
                accent01 = Math.Min(1f, 0.45f + ComputeAccent(quest) * 0.55f),
            });
        }
    }

    private StoryQuestState? GetActiveQuest()
    {
        if (saveData.storyQuests == null)
        {
            return null;
        }

        for (int i = 0; i < saveData.storyQuests.Count; i++)
        {
            var quest = saveData.storyQuests[i];
            if (quest != null && quest.unlocked && !quest.completed)
            {
                return quest;
            }
        }

        return null;
    }

    private StoryQuestState? GetNextLockedQuest()
    {
        if (saveData.storyQuests == null)
        {
            return null;
        }

        for (int i = 0; i < saveData.storyQuests.Count; i++)
        {
            var quest = saveData.storyQuests[i];
            if (quest != null && !quest.completed)
            {
                return quest;
            }
        }

        return null;
    }

    private bool AnyUnlockedIncompleteQuest()
    {
        if (saveData.storyQuests == null)
        {
            return false;
        }

        for (int i = 0; i < saveData.storyQuests.Count; i++)
        {
            var quest = saveData.storyQuests[i];
            if (quest != null && quest.unlocked && !quest.completed)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsMetaSatisfied(StoryQuestState quest)
    {
        switch (quest.objectiveType)
        {
            case StoryObjectiveType.ReachDistrict:
                return currentTierIndex >= (int)quest.target;
            case StoryObjectiveType.ReachPrestigeReady:
                return currentCanPrestige;
            case StoryObjectiveType.PrestigeTimes:
                return currentPrestigeLevel >= (int)quest.target;
            default:
                return false;
        }
    }

    private string BuildObjectiveLine(StoryQuestState quest)
    {
        var progress = Math.Min(quest.target, quest.progress);
        switch (quest.objectiveType)
        {
            case StoryObjectiveType.ServeOrders:
                return "Serve " + (int)progress + "/" + (int)quest.target + " fresh plates";
            case StoryObjectiveType.UseBoosts:
                return "Use grill boost " + (int)progress + "/" + (int)quest.target + " times";
            case StoryObjectiveType.DailySpecialServes:
                return "Serve today's special " + (int)progress + "/" + (int)quest.target + " times";
            case StoryObjectiveType.ReachDistrict:
                return "Reach district tier " + (int)quest.target;
            case StoryObjectiveType.SpotlightServes:
                return "Serve VIP or Critic tables " + (int)progress + "/" + (int)quest.target;
            case StoryObjectiveType.PerfectServes:
                return "Land perfect serves " + (int)progress + "/" + (int)quest.target;
            case StoryObjectiveType.BuyUpgrades:
                return "Buy upgrades " + (int)progress + "/" + (int)quest.target;
            case StoryObjectiveType.TriggerChefFever:
                return "Trigger Chef Fever " + (int)progress + "/" + (int)quest.target + " times";
            case StoryObjectiveType.ReachPrestigeReady:
                return "Push this run until prestige is ready";
            case StoryObjectiveType.PrestigeTimes:
                return "Prestige " + (int)progress + "/" + (int)quest.target + " time";
            default:
                return "Keep the house moving";
        }
    }

    private string BuildStatusLine(StoryQuestState quest)
    {
        if (quest.completed)
        {
            return "COMPLETE";
        }

        if (quest.target <= 0)
        {
            return "LIVE";
        }

        return quest.progress.ToString("0") + "/" + quest.target.ToString("0");
    }

    private string BuildUnlockHint(StoryQuestState quest)
    {
        if (currentTierIndex < quest.requiredTierIndex)
        {
            return "Unlock the next district to continue this act.";
        }

        return "Finish the previous chapter to open this scene.";
    }

    private float ComputeAccent(StoryQuestState quest)
    {
        if (quest == null)
        {
            return 0.25f;
        }

        if (quest.target <= 0)
        {
            return 0.5f;
        }

        return (float)Math.Max(0d, Math.Min(1d, quest.progress / quest.target));
    }

    private static string FormatCurrency(double amount)
    {
        if (amount >= 1000000d)
        {
            return (amount / 1000000d).ToString("0.0") + "M";
        }
        if (amount >= 1000d)
        {
            return (amount / 1000d).ToString("0.0") + "K";
        }
        return amount.ToString("0");
    }

    private void AppendLog(string id, string speaker, string headline, string line, string districtId)
    {
        if (saveData.storyLog == null)
        {
            saveData.storyLog = new List<StoryLogEntry>();
        }

        if (saveData.storyLog.Count > 0 && string.Equals(saveData.storyLog[saveData.storyLog.Count - 1].id, id, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        saveData.storyLog.Add(new StoryLogEntry
        {
            id = id,
            speaker = speaker ?? string.Empty,
            headline = headline ?? string.Empty,
            line = line ?? string.Empty,
            districtId = districtId ?? string.Empty
        });

        while (saveData.storyLog.Count > 18)
        {
            saveData.storyLog.RemoveAt(0);
        }
    }

    private StoryQuestDefinition? DefinitionsForId(string questId)
    {
        for (int i = 0; i < Definitions.Length; i++)
        {
            if (string.Equals(Definitions[i].id, questId, StringComparison.OrdinalIgnoreCase))
            {
                return Definitions[i];
            }
        }

        return null;
    }

    private sealed class StoryQuestDefinition
    {
        public readonly string id;
        public readonly string districtId;
        public readonly int requiredTierIndex;
        public readonly string actTitle;
        public readonly string chapterTitle;
        public readonly string unlockSpeaker;
        public readonly string briefing;
        public readonly string completionSpeaker;
        public readonly string completionText;
        public readonly StoryObjectiveType objectiveType;
        public readonly int target;
        public readonly double rewardMultiplier;

        public StoryQuestDefinition(
            string id,
            string districtId,
            int requiredTierIndex,
            string actTitle,
            string chapterTitle,
            string unlockSpeaker,
            string briefing,
            string completionSpeaker,
            string completionText,
            StoryObjectiveType objectiveType,
            int target,
            double rewardMultiplier)
        {
            this.id = id;
            this.districtId = districtId;
            this.requiredTierIndex = requiredTierIndex;
            this.actTitle = actTitle;
            this.chapterTitle = chapterTitle;
            this.unlockSpeaker = unlockSpeaker;
            this.briefing = briefing;
            this.completionSpeaker = completionSpeaker;
            this.completionText = completionText;
            this.objectiveType = objectiveType;
            this.target = target;
            this.rewardMultiplier = rewardMultiplier;
        }
    }
}

public struct StoryQuestUpdate
{
    public string title;
    public string detail;
    public string speakerName;
    public double rewardCurrency;
    public float accent01;
}
