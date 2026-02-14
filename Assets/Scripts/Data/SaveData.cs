using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int version = 1;
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
    public string id;
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
