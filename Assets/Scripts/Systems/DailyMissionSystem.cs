using System;
using System.Collections.Generic;

public class DailyMissionSystem
{
    private readonly SaveData saveData;
    private readonly EconomySystem economy;
    private readonly int missionsPerDay;

    public event Action<IReadOnlyList<DailyMissionState>> OnMissionsUpdated;

    public DailyMissionSystem(SaveData saveData, EconomySystem economy, int missionsPerDay)
    {
        this.saveData = saveData;
        this.economy = economy;
        this.missionsPerDay = Math.Max(1, missionsPerDay);
    }

    public void EnsureMissionsForToday(double incomePerSec)
    {
        var today = TimeUtil.UtcDayStamp();
        if (saveData.lastMissionDay == today && saveData.dailyMissions != null && saveData.dailyMissions.Count > 0)
        {
            return;
        }

        saveData.lastMissionDay = today;
        GenerateMissions(incomePerSec);
    }

    public void RecordEarnings(double amount)
    {
        if (amount <= 0)
        {
            return;
        }

        ApplyProgress(DailyMissionType.EarnCurrency, amount);
    }

    public void RecordBoost()
    {
        ApplyProgress(DailyMissionType.UseBoost, 1);
    }

    public void RecordUpgrade()
    {
        ApplyProgress(DailyMissionType.PurchaseUpgrade, 1);
    }

    public bool Claim(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return false;
        }

        var mission = saveData.dailyMissions.Find(m => m.id == id);
        if (mission == null || !mission.completed || mission.claimed)
        {
            return false;
        }

        mission.claimed = true;
        economy.AddCurrency(mission.reward);
        OnMissionsUpdated?.Invoke(saveData.dailyMissions);
        return true;
    }

    private void GenerateMissions(double incomePerSec)
    {
        if (saveData.dailyMissions == null)
        {
            saveData.dailyMissions = new List<DailyMissionState>();
        }

        saveData.dailyMissions.Clear();
        var baseIncome = Math.Max(1.0, incomePerSec);

        var types = new List<DailyMissionType>
        {
            DailyMissionType.EarnCurrency,
            DailyMissionType.UseBoost,
            DailyMissionType.PurchaseUpgrade
        };

        for (int i = 0; i < missionsPerDay; i++)
        {
            var type = types[Math.Min(i, types.Count - 1)];
            var mission = CreateMission(type, baseIncome, i);
            saveData.dailyMissions.Add(mission);
        }

        OnMissionsUpdated?.Invoke(saveData.dailyMissions);
    }

    private DailyMissionState CreateMission(DailyMissionType type, double baseIncome, int index)
    {
        var state = new DailyMissionState
        {
            id = type.ToString().ToLowerInvariant() + "_" + index,
            type = type,
            progress = 0,
            completed = false,
            claimed = false
        };

        switch (type)
        {
            case DailyMissionType.EarnCurrency:
                state.target = baseIncome * 120.0;
                state.reward = baseIncome * 25.0;
                break;
            case DailyMissionType.UseBoost:
                state.target = 5;
                state.reward = baseIncome * 20.0;
                break;
            case DailyMissionType.PurchaseUpgrade:
                state.target = 3;
                state.reward = baseIncome * 30.0;
                break;
            default:
                state.target = 1;
                state.reward = baseIncome * 10.0;
                break;
        }

        return state;
    }

    private void ApplyProgress(DailyMissionType type, double amount)
    {
        if (saveData.dailyMissions == null)
        {
            return;
        }

        foreach (var mission in saveData.dailyMissions)
        {
            if (mission == null || mission.type != type || mission.completed)
            {
                continue;
            }

            mission.progress = Math.Min(mission.target, mission.progress + amount);
            if (mission.progress >= mission.target)
            {
                mission.completed = true;
            }
        }

        OnMissionsUpdated?.Invoke(saveData.dailyMissions);
    }
}
