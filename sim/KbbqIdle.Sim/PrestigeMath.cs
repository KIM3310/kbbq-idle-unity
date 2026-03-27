namespace KbbqIdle.Sim;

public readonly record struct PrestigeReward(bool CanPrestige, int Points);

public static class PrestigeMath
{
    public static PrestigeReward CalculateReward(double totalIncome, int playerLevel)
    {
        if (playerLevel < 10 || totalIncome < 50_000)
        {
            return new PrestigeReward(false, 0);
        }

        var points = (int)Math.Floor(Math.Sqrt(totalIncome / 100_000.0));
        points = Math.Max(1, points);
        return new PrestigeReward(true, points);
    }
}
