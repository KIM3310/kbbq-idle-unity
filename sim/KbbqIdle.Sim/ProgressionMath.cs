namespace KbbqIdle.Sim;

public static class ProgressionMath
{
    public static int GetLevelForIncome(
        double totalIncome,
        double baseRequirement = 50.0,
        double growth = 1.28,
        int maxLevel = 100
    )
    {
        if (maxLevel < 1) return 1;
        if (baseRequirement < 1) baseRequirement = 1.0;
        if (growth < 1.01) growth = 1.01;

        var level = 1;
        var requirement = baseRequirement;
        while (level < maxLevel && totalIncome >= requirement)
        {
            level++;
            requirement *= growth;
        }
        return level;
    }

    public static double GetNextLevelRequirement(
        int level,
        double baseRequirement = 50.0,
        double growth = 1.28
    )
    {
        if (level < 1) level = 1;
        if (baseRequirement < 1) baseRequirement = 1.0;
        if (growth < 1.01) growth = 1.01;
        return baseRequirement * Math.Pow(growth, level - 1);
    }
}
