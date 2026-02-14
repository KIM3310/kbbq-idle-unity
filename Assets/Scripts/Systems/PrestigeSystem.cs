using System;

public class PrestigeSystem
{
    public int PrestigeLevel { get; private set; }
    public int PrestigePoints { get; private set; }

    public double PrestigeMultiplier => 1.0 + (PrestigePoints * 0.02);

    public PrestigeSystem(int level = 0, int points = 0)
    {
        PrestigeLevel = Math.Max(0, level);
        PrestigePoints = Math.Max(0, points);
    }

    public PrestigeReward CalculateReward(double totalIncome, int playerLevel)
    {
        if (playerLevel < 10 || totalIncome < 50000)
        {
            return PrestigeReward.NotReady();
        }

        var points = (int)Math.Floor(Math.Sqrt(totalIncome / 100000.0));
        points = Math.Max(1, points);
        return new PrestigeReward(true, points);
    }

    public void ApplyPrestige(PrestigeReward reward)
    {
        if (!reward.canPrestige)
        {
            return;
        }

        PrestigeLevel += 1;
        PrestigePoints += reward.points;
    }
}

public struct PrestigeReward
{
    public bool canPrestige;
    public int points;

    public PrestigeReward(bool canPrestige, int points)
    {
        this.canPrestige = canPrestige;
        this.points = points;
    }

    public static PrestigeReward NotReady()
    {
        return new PrestigeReward(false, 0);
    }
}
