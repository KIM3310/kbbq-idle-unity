using System;

public class DailyLoginSystem
{
    private static readonly int[] RewardMinutes = { 2, 3, 4, 5, 7, 9, 12 };

    private readonly SaveData saveData;
    private readonly EconomySystem economy;

    public DailyLoginSystem(SaveData saveData, EconomySystem economy)
    {
        this.saveData = saveData;
        this.economy = economy;
    }

    public DailyLoginReward TryClaim()
    {
        var today = TimeUtil.UtcDayStamp();
        if (saveData.lastLoginDay == today)
        {
            return DailyLoginReward.None();
        }

        var yesterday = TimeUtil.UtcDayStampOffsetDays(-1);
        if (saveData.lastLoginDay == yesterday)
        {
            saveData.loginStreak = Math.Min(saveData.loginStreak + 1, RewardMinutes.Length);
        }
        else
        {
            saveData.loginStreak = 1;
        }

        saveData.lastLoginDay = today;
        var minutes = RewardMinutes[Math.Max(0, saveData.loginStreak - 1)];
        var reward = Math.Max(0, economy.IncomePerSec * minutes * 60.0);
        economy.AddCurrency(reward);
        return new DailyLoginReward(true, reward, saveData.loginStreak);
    }
}

public struct DailyLoginReward
{
    public bool granted;
    public double currency;
    public int streakDay;

    public DailyLoginReward(bool granted, double currency, int streakDay)
    {
        this.granted = granted;
        this.currency = currency;
        this.streakDay = streakDay;
    }

    public static DailyLoginReward None()
    {
        return new DailyLoginReward(false, 0, 0);
    }
}
