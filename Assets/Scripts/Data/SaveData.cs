using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int version = 7;
    public int playerLevel = 1;
    public int prestigeLevel = 0;
    public int prestigePoints = 0;
    public double currency = 0;
    public double totalIncome = 0;
    public double lifetimeIncome = 0;
    public long lastOnlineTs = 0;
    public bool tutorialCompleted = false;
    public int storeTierIndex = 0;
    public int lastLoginDay = 0;
    public int loginStreak = 0;
    public int lastMissionDay = 0;
    public float spawnRateMultiplier = 1f;
    public float serviceRateMultiplier = 1f;
    public bool debugPanelVisible = true;
    public bool perfOverlayVisible = true;
    public int debugPresetIndex = 1;
    public bool debugVisibilityInitialized = false;
    public List<string> unlockedMenuIds = new List<string>();
    public List<UpgradeLevelEntry> upgradeLevels = new List<UpgradeLevelEntry>();
    public List<DailyMissionState> dailyMissions = new List<DailyMissionState>();
    public List<StoryQuestState> storyQuests = new List<StoryQuestState>();
    public List<DistrictSideQuestState> sideQuests = new List<DistrictSideQuestState>();
    public List<StoryLogEntry> storyLog = new List<StoryLogEntry>();
    public List<string> resolvedStoryGuestIds = new List<string>();
    public List<StoryGuestRetryState> storyGuestRetries = new List<StoryGuestRetryState>();
    public List<MeatInventoryEntry> meatInventory = new List<MeatInventoryEntry>();
    public List<GrillSlotSaveState> grillSlots = new List<GrillSlotSaveState>();

    public void Sanitize()
    {
        if (version < 1)
        {
            version = 1;
        }

        playerLevel = Math.Max(1, playerLevel);
        prestigeLevel = Math.Max(0, prestigeLevel);
        prestigePoints = Math.Max(0, prestigePoints);

        if (currency < 0) currency = 0;
        if (totalIncome < 0) totalIncome = 0;
        if (lifetimeIncome < 0) lifetimeIncome = 0;
        if (lastOnlineTs < 0) lastOnlineTs = 0;
        storeTierIndex = Math.Max(0, storeTierIndex);

        if (unlockedMenuIds == null)
        {
            unlockedMenuIds = new List<string>();
        }

        if (upgradeLevels == null)
        {
            upgradeLevels = new List<UpgradeLevelEntry>();
        }

        if (dailyMissions == null)
        {
            dailyMissions = new List<DailyMissionState>();
        }

        if (storyQuests == null)
        {
            storyQuests = new List<StoryQuestState>();
        }

        if (sideQuests == null)
        {
            sideQuests = new List<DistrictSideQuestState>();
        }

        if (storyLog == null)
        {
            storyLog = new List<StoryLogEntry>();
        }

        if (resolvedStoryGuestIds == null)
        {
            resolvedStoryGuestIds = new List<string>();
        }

        if (storyGuestRetries == null)
        {
            storyGuestRetries = new List<StoryGuestRetryState>();
        }

        if (meatInventory == null)
        {
            meatInventory = new List<MeatInventoryEntry>();
        }

        for (int i = storyGuestRetries.Count - 1; i >= 0; i--)
        {
            var retry = storyGuestRetries[i];
            if (retry == null || string.IsNullOrEmpty(retry.id))
            {
                storyGuestRetries.RemoveAt(i);
                continue;
            }

            if (retry.count < 0) retry.count = 0;
        }

        for (int i = storyLog.Count - 1; i >= 0; i--)
        {
            var entry = storyLog[i];
            if (entry == null || string.IsNullOrEmpty(entry.id))
            {
                storyLog.RemoveAt(i);
                continue;
            }

            entry.speaker = entry.speaker ?? string.Empty;
            entry.headline = entry.headline ?? string.Empty;
            entry.line = entry.line ?? string.Empty;
            entry.districtId = entry.districtId ?? string.Empty;
        }

        if (grillSlots == null)
        {
            grillSlots = new List<GrillSlotSaveState>();
        }

        for (int i = sideQuests.Count - 1; i >= 0; i--)
        {
            var quest = sideQuests[i];
            if (quest == null || string.IsNullOrEmpty(quest.id))
            {
                sideQuests.RemoveAt(i);
                continue;
            }

            if (quest.target < 0) quest.target = 0;
            if (quest.progress < 0) quest.progress = 0;
            if (quest.rewardCurrency < 0) quest.rewardCurrency = 0;
            if (quest.requiredTierIndex < 0) quest.requiredTierIndex = 0;
        }

        for (int i = storyQuests.Count - 1; i >= 0; i--)
        {
            var quest = storyQuests[i];
            if (quest == null || string.IsNullOrEmpty(quest.id))
            {
                storyQuests.RemoveAt(i);
                continue;
            }

            if (quest.target < 0) quest.target = 0;
            if (quest.progress < 0) quest.progress = 0;
            if (quest.rewardCurrency < 0) quest.rewardCurrency = 0;
            if (quest.requiredTierIndex < 0) quest.requiredTierIndex = 0;
        }

        for (int i = meatInventory.Count - 1; i >= 0; i--)
        {
            var entry = meatInventory[i];
            if (string.IsNullOrEmpty(entry.menuId))
            {
                meatInventory.RemoveAt(i);
                continue;
            }

            if (entry.rawCount < 0) entry.rawCount = 0;
            if (entry.cookedCount < 0) entry.cookedCount = 0;
            meatInventory[i] = entry;
        }

        for (int i = grillSlots.Count - 1; i >= 0; i--)
        {
            var slot = grillSlots[i];
            if (slot.slotIndex < 0 || slot.slotIndex > 3)
            {
                grillSlots.RemoveAt(i);
                continue;
            }

            if (slot.cookTime < 0f)
            {
                slot.cookTime = 0f;
            }
            if (string.IsNullOrEmpty(slot.menuId))
            {
                slot.cookTime = 0f;
                slot.flipped = false;
            }
            grillSlots[i] = slot;
        }

        if (spawnRateMultiplier <= 0f)
        {
            spawnRateMultiplier = 1f;
        }

        if (serviceRateMultiplier <= 0f)
        {
            serviceRateMultiplier = 1f;
        }

        if (debugPresetIndex < 0 || debugPresetIndex > 3)
        {
            debugPresetIndex = 1;
        }
    }

    public void ResetProgressForPrestige()
    {
        playerLevel = 1;
        currency = 0;
        totalIncome = 0;
        storeTierIndex = 0;
        unlockedMenuIds.Clear();
        upgradeLevels.Clear();
        meatInventory.Clear();
        grillSlots.Clear();
    }
}

[Serializable]
public struct UpgradeLevelEntry
{
    public string id;
    public int level;
}

[Serializable]
public class DailyMissionState
{
    public string id = string.Empty;
    public DailyMissionType type;
    public double target;
    public double progress;
    public double reward;
    public bool completed;
    public bool claimed;
}

public enum DailyMissionType
{
    EarnCurrency,
    UseBoost,
    PurchaseUpgrade
}

[Serializable]
public class StoryQuestState
{
    public string id = string.Empty;
    public string districtId = string.Empty;
    public string actTitle = string.Empty;
    public string chapterTitle = string.Empty;
    public string speakerName = string.Empty;
    public string briefing = string.Empty;
    public string completionText = string.Empty;
    public StoryObjectiveType objectiveType;
    public double target;
    public double progress;
    public double rewardCurrency;
    public int requiredTierIndex;
    public bool unlocked;
    public bool completed;
}

[Serializable]
public class StoryLogEntry
{
    public string id = string.Empty;
    public string speaker = string.Empty;
    public string headline = string.Empty;
    public string line = string.Empty;
    public string districtId = string.Empty;
}

[Serializable]
public class StoryGuestRetryState
{
    public string id = string.Empty;
    public int count;
}

[Serializable]
public class DistrictSideQuestState
{
    public string id = string.Empty;
    public string districtId = string.Empty;
    public string speakerName = string.Empty;
    public string title = string.Empty;
    public string briefing = string.Empty;
    public string completionText = string.Empty;
    public StoryObjectiveType objectiveType;
    public double target;
    public double progress;
    public double rewardCurrency;
    public int requiredTierIndex;
    public bool unlocked;
    public bool completed;
}

public enum StoryObjectiveType
{
    ServeOrders,
    UseBoosts,
    DailySpecialServes,
    ReachDistrict,
    SpotlightServes,
    PerfectServes,
    BuyUpgrades,
    TriggerChefFever,
    ReachPrestigeReady,
    PrestigeTimes
}

[Serializable]
public struct MeatInventoryEntry
{
    public string menuId;
    public int rawCount;
    public int cookedCount;
}

[Serializable]
public struct GrillSlotSaveState
{
    public int slotIndex;
    public string menuId;
    public float cookTime;
    public bool flipped;
}
