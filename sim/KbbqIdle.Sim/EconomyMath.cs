namespace KbbqIdle.Sim;

public static class EconomyMath
{
    public static double ComputeIncomePerSec(
        double baseIncome,
        double upgradeMultiplier,
        double staffMultiplier,
        double serviceMultiplier,
        double storeMultiplier,
        double boostMultiplier,
        double tipMultiplier,
        double comboMultiplier,
        double prestigeMultiplier
    )
    {
        // Matches the in-game formula (EconomySystem.CalculateIncomePerSec).
        return baseIncome
            * upgradeMultiplier
            * staffMultiplier
            * serviceMultiplier
            * storeMultiplier
            * boostMultiplier
            * tipMultiplier
            * comboMultiplier
            * prestigeMultiplier;
    }
}

