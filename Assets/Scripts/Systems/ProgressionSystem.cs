using System;

public class ProgressionSystem
{
    private readonly double baseRequirement;
    private readonly double growth;
    private readonly int maxLevel;
    private readonly EconomyTuning tuning;

    public ProgressionSystem(EconomyTuning tuning = null, double baseRequirement = 50.0, double growth = 1.28, int maxLevel = 100)
    {
        this.tuning = tuning;
        if (tuning != null)
        {
            this.baseRequirement = Math.Max(1.0, tuning.baseRequirement);
            this.growth = Math.Max(1.01, tuning.requirementGrowth);
            this.maxLevel = Math.Max(1, tuning.maxLevel);
            if (tuning.levels == null || tuning.levels.Count == 0)
            {
                tuning.RebuildTable();
            }
        }
        else
        {
            this.baseRequirement = Math.Max(1.0, baseRequirement);
            this.growth = Math.Max(1.01, growth);
            this.maxLevel = Math.Max(1, maxLevel);
        }
    }

    public int GetLevelForIncome(double totalIncome)
    {
        if (tuning != null && tuning.levels != null && tuning.levels.Count > 0)
        {
            var level = 1;
            for (int i = 0; i < tuning.levels.Count; i++)
            {
                if (totalIncome < tuning.levels[i].totalIncomeRequired)
                {
                    return level;
                }
                level = Math.Min(maxLevel, tuning.levels[i].level + 1);
            }
            return Math.Min(maxLevel, level);
        }

        var currentLevel = 1;
        var requirement = baseRequirement;
        while (currentLevel < maxLevel && totalIncome >= requirement)
        {
            currentLevel++;
            requirement *= growth;
        }
        return currentLevel;
    }

    public double GetNextLevelRequirement(int level)
    {
        if (level < 1)
        {
            level = 1;
        }
        if (tuning != null && tuning.levels != null && tuning.levels.Count >= level)
        {
            return tuning.levels[level - 1].totalIncomeRequired;
        }

        return baseRequirement * Math.Pow(growth, level - 1);
    }
}
