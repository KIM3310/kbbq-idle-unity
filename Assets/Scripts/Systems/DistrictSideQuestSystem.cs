using System;
using System.Collections.Generic;

public class DistrictSideQuestSystem
{
    private readonly SaveData saveData;
    private readonly Queue<DistrictSideQuestUpdate> pendingUpdates = new Queue<DistrictSideQuestUpdate>();
    private int currentTierIndex;
    private bool currentCanPrestige;
    private int currentPrestigeLevel;
    private double rewardSeed = 1d;

    private static readonly SideQuestDefinition[] Definitions =
    {
        new SideQuestDefinition("alley_after_shift", "alley", 0, "Mina", "After-Shift Staff Meal", "Serve three more plates after the first rush so the alley workers start trusting your timing.", "The alley workers start leaving with grease on their smiles.", StoryObjectiveType.ServeOrders, 3, 14),
        new SideQuestDefinition("alley_smoke_signal", "alley", 0, "Mr. Han", "Smoke Signal", "Fire the boost one more time and let the block know the grill is not cooling down tonight.", "The alley sees the flame jump and stops walking past.", StoryObjectiveType.UseBoosts, 1, 16),
        new SideQuestDefinition("hongdae_busker_crowd", "hongdae", 1, "DJ Yuna", "Busker Crowd", "Trigger Chef Fever once while the Hongdae floor is packed and noisy.", "Now the crowd treats the grill like part of the set list.", StoryObjectiveType.TriggerChefFever, 1, 22),
        new SideQuestDefinition("hongdae_fancam", "hongdae", 1, "MC Dae", "Fancam Table", "Serve one spotlight table and make it look clip-worthy.", "Someone records the plate landing and the clip starts making rounds.", StoryObjectiveType.SpotlightServes, 1, 22),
        new SideQuestDefinition("gangnam_afterparty", "gangnam", 2, "Manager Seo", "Afterparty Seating", "Land two perfect serves while the premium room stays full.", "The afterparty tables stop testing you and start requesting you.", StoryObjectiveType.PerfectServes, 2, 28),
        new SideQuestDefinition("gangnam_private_room", "gangnam", 2, "Hana", "Private Room Whisper", "Serve the daily special three times so the private tables start treating it like code.", "The special becomes a password, not just an order.", StoryObjectiveType.DailySpecialServes, 3, 30),
        new SideQuestDefinition("hanok_family_table", "hanok", 3, "Grandmother Ok", "Family Table", "Buy two upgrades that make the house feel calmer and tighter.", "The room stops feeling temporary. It starts feeling inherited.", StoryObjectiveType.BuyUpgrades, 2, 34),
        new SideQuestDefinition("hanok_quiet_perfection", "hanok", 3, "Master Hyeon", "Quiet Perfection", "Land three perfect serves without breaking the room's rhythm.", "Silence in the house starts sounding like respect.", StoryObjectiveType.PerfectServes, 3, 34),
        new SideQuestDefinition("global_press_preview", "global", 4, "Producer Niko", "Press Preview", "Serve two spotlight tables and one perfect plate on the world stage.", "The room starts calling your service a headline, not a dinner.", StoryObjectiveType.SpotlightServes, 2, 40),
        new SideQuestDefinition("global_last_push", "global", 4, "Amira", "Last Push", "Reach prestige-ready and prove the restaurant can hold a finale crowd.", "Even before the reset, the next season is already being talked about.", StoryObjectiveType.ReachPrestigeReady, 1, 46),
    };

    public DistrictSideQuestSystem(SaveData saveData)
    {
        this.saveData = saveData ?? new SaveData();
    }

    public void SyncMetaState(int tierIndex, bool canPrestige, int prestigeLevel, double rewardSeed, bool emitUpdates)
    {
        currentTierIndex = Math.Max(0, tierIndex);
        currentCanPrestige = canPrestige;
        currentPrestigeLevel = Math.Max(0, prestigeLevel);
        this.rewardSeed = Math.Max(1d, rewardSeed);

        EnsureDefinitions();
        UnlockEligible(emitUpdates);
        AutoCompleteMetaSideQuest(emitUpdates);
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

        FinishIfReady(true);
    }

    public void RecordBoost()
    {
        AdvanceSimple(StoryObjectiveType.UseBoosts);
    }

    public void RecordUpgrade()
    {
        AdvanceSimple(StoryObjectiveType.BuyUpgrades);
    }

    public void RecordChefFever()
    {
        AdvanceSimple(StoryObjectiveType.TriggerChefFever);
    }

    public void RecordPrestige()
    {
        AdvanceSimple(StoryObjectiveType.PrestigeTimes);
    }

    public DistrictSideQuestUiState GetUiState()
    {
        EnsureDefinitions();
        var active = GetActiveQuest();
        if (active != null)
        {
            return new DistrictSideQuestUiState
            {
                districtTitle = string.IsNullOrEmpty(active.districtId) ? "SIDE STORY" : active.districtId.ToUpperInvariant() + " SIDE STORY",
                speakerName = active.speakerName,
                chapterTitle = active.title,
                objectiveLine = BuildObjectiveLine(active),
                rewardLine = "Side Reward +" + FormatCurrency(active.rewardCurrency),
                statusLine = BuildStatusLine(active),
                accent01 = ComputeAccent(active),
                visible = true,
            };
        }

        return default;
    }

    public bool TryDequeueUpdate(out DistrictSideQuestUpdate update)
    {
        if (pendingUpdates.Count > 0)
        {
            update = pendingUpdates.Dequeue();
            return true;
        }

        update = default;
        return false;
    }

    private void AdvanceSimple(StoryObjectiveType type)
    {
        var active = GetActiveQuest();
        if (active == null || active.objectiveType != type)
        {
            return;
        }

        active.progress = Math.Min(active.target, active.progress + 1);
        FinishIfReady(true);
    }

    private void EnsureDefinitions()
    {
        var existing = new Dictionary<string, DistrictSideQuestState>(StringComparer.OrdinalIgnoreCase);
        if (saveData.sideQuests != null)
        {
            for (int i = 0; i < saveData.sideQuests.Count; i++)
            {
                var quest = saveData.sideQuests[i];
                if (quest != null && !string.IsNullOrEmpty(quest.id))
                {
                    existing[quest.id] = quest;
                }
            }
        }

        saveData.sideQuests = new List<DistrictSideQuestState>();
        for (int i = 0; i < Definitions.Length; i++)
        {
            var def = Definitions[i];
            var quest = existing.TryGetValue(def.id, out var prior)
                ? prior
                : new DistrictSideQuestState();

            quest.id = def.id;
            quest.districtId = def.districtId;
            quest.speakerName = def.speakerName;
            quest.title = def.title;
            quest.briefing = def.briefing;
            quest.completionText = def.completionText;
            quest.objectiveType = def.objectiveType;
            quest.target = def.target;
            quest.requiredTierIndex = def.requiredTierIndex;
            if (quest.rewardCurrency <= 0d)
            {
                quest.rewardCurrency = Math.Max(18d, rewardSeed * def.rewardMultiplier);
            }
            saveData.sideQuests.Add(quest);
        }
    }

    private void UnlockEligible(bool emitUpdates)
    {
        for (int i = 0; i < saveData.sideQuests.Count; i++)
        {
            var quest = saveData.sideQuests[i];
            if (quest == null || quest.unlocked || quest.completed || currentTierIndex < quest.requiredTierIndex)
            {
                continue;
            }

            var previousInDistrictDone = true;
            for (int j = 0; j < i; j++)
            {
                var prev = saveData.sideQuests[j];
                if (prev != null && string.Equals(prev.districtId, quest.districtId, StringComparison.OrdinalIgnoreCase) && !prev.completed)
                {
                    previousInDistrictDone = false;
                    break;
                }
            }

            if (!previousInDistrictDone)
            {
                continue;
            }

            quest.unlocked = true;
            if (emitUpdates)
            {
                pendingUpdates.Enqueue(new DistrictSideQuestUpdate
                {
                    title = "SIDE STORY",
                    detail = quest.title + " · " + quest.briefing,
                    speakerName = quest.speakerName,
                    rewardCurrency = 0,
                    accent01 = 0.48f,
                });
            }
        }
    }

    private void AutoCompleteMetaSideQuest(bool emitUpdates)
    {
        var active = GetActiveQuest();
        if (active == null)
        {
            return;
        }

        if (active.objectiveType == StoryObjectiveType.ReachPrestigeReady && currentCanPrestige)
        {
            active.progress = active.target;
            FinishIfReady(emitUpdates);
            return;
        }

        if (active.objectiveType == StoryObjectiveType.PrestigeTimes && currentPrestigeLevel >= active.target)
        {
            active.progress = active.target;
            FinishIfReady(emitUpdates);
        }
    }

    private void FinishIfReady(bool emitUpdates)
    {
        var active = GetActiveQuest();
        if (active == null || active.progress < active.target)
        {
            return;
        }

        active.completed = true;
        active.progress = Math.Max(active.progress, active.target);
        if (emitUpdates)
        {
            pendingUpdates.Enqueue(new DistrictSideQuestUpdate
            {
                title = active.title.ToUpperInvariant(),
                detail = active.completionText,
                speakerName = active.speakerName,
                rewardCurrency = active.rewardCurrency,
                accent01 = 0.72f,
            });
        }

        UnlockEligible(emitUpdates);
        AutoCompleteMetaSideQuest(emitUpdates);
    }

    private DistrictSideQuestState? GetActiveQuest()
    {
        if (saveData.sideQuests == null)
        {
            return null;
        }

        for (int i = 0; i < saveData.sideQuests.Count; i++)
        {
            var quest = saveData.sideQuests[i];
            if (quest != null && quest.unlocked && !quest.completed)
            {
                return quest;
            }
        }

        return null;
    }

    private string BuildObjectiveLine(DistrictSideQuestState quest)
    {
        var progress = Math.Min(quest.target, quest.progress);
        switch (quest.objectiveType)
        {
            case StoryObjectiveType.ServeOrders:
                return "Serve " + (int)progress + "/" + (int)quest.target + " extra plates";
            case StoryObjectiveType.UseBoosts:
                return "Use boost " + (int)progress + "/" + (int)quest.target + " time";
            case StoryObjectiveType.DailySpecialServes:
                return "Serve today's special " + (int)progress + "/" + (int)quest.target;
            case StoryObjectiveType.SpotlightServes:
                return "Serve VIP/Critic tables " + (int)progress + "/" + (int)quest.target;
            case StoryObjectiveType.PerfectServes:
                return "Land perfect serves " + (int)progress + "/" + (int)quest.target;
            case StoryObjectiveType.BuyUpgrades:
                return "Buy upgrades " + (int)progress + "/" + (int)quest.target;
            case StoryObjectiveType.TriggerChefFever:
                return "Trigger Chef Fever " + (int)progress + "/" + (int)quest.target;
            case StoryObjectiveType.ReachPrestigeReady:
                return "Push this run until prestige is ready";
            default:
                return "Keep the side story moving";
        }
    }

    private string BuildStatusLine(DistrictSideQuestState quest)
    {
        if (quest.completed)
        {
            return "COMPLETE";
        }

        return quest.progress.ToString("0") + "/" + quest.target.ToString("0");
    }

    private float ComputeAccent(DistrictSideQuestState quest)
    {
        if (quest == null || quest.target <= 0)
        {
            return 0.25f;
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

    private sealed class SideQuestDefinition
    {
        public readonly string id;
        public readonly string districtId;
        public readonly int requiredTierIndex;
        public readonly string speakerName;
        public readonly string title;
        public readonly string briefing;
        public readonly string completionText;
        public readonly StoryObjectiveType objectiveType;
        public readonly int target;
        public readonly double rewardMultiplier;

        public SideQuestDefinition(
            string id,
            string districtId,
            int requiredTierIndex,
            string speakerName,
            string title,
            string briefing,
            string completionText,
            StoryObjectiveType objectiveType,
            int target,
            double rewardMultiplier)
        {
            this.id = id;
            this.districtId = districtId;
            this.requiredTierIndex = requiredTierIndex;
            this.speakerName = speakerName;
            this.title = title;
            this.briefing = briefing;
            this.completionText = completionText;
            this.objectiveType = objectiveType;
            this.target = target;
            this.rewardMultiplier = rewardMultiplier;
        }
    }
}

public struct DistrictSideQuestUpdate
{
    public string title;
    public string detail;
    public string speakerName;
    public double rewardCurrency;
    public float accent01;
}
