using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct LevelEconomyEntry
{
    public int level;
    public double totalIncomeRequired;
    public double targetIncomePerSec;
    public double targetUpgradeCost;
}

[CreateAssetMenu(menuName = "KBBQ/Economy Tuning")]
public class EconomyTuning : ScriptableObject
{
    public int maxLevel = 100;
    public double baseRequirement = 50.0;
    public double requirementGrowth = 1.28;

    public double baseIncomePerSec = 1.0;
    public double incomeGrowth = 1.22;

    public double baseUpgradeCost = 10.0;
    public double upgradeGrowth = 1.3;

    public List<LevelEconomyEntry> levels = new List<LevelEconomyEntry>();

    public void RebuildTable()
    {
        levels.Clear();
        var requirement = Math.Max(1.0, baseRequirement);
        var income = Math.Max(0.1, baseIncomePerSec);
        var upgradeCost = Math.Max(1.0, baseUpgradeCost);
        var max = Math.Max(1, maxLevel);

        for (int level = 1; level <= max; level++)
        {
            levels.Add(new LevelEconomyEntry
            {
                level = level,
                totalIncomeRequired = requirement,
                targetIncomePerSec = income,
                targetUpgradeCost = upgradeCost
            });

            requirement *= Math.Max(1.01, requirementGrowth);
            income *= Math.Max(1.01, incomeGrowth);
            upgradeCost *= Math.Max(1.01, upgradeGrowth);
        }
    }

    public double GetRequirementForLevel(int level)
    {
        if (level < 1)
        {
            level = 1;
        }

        if (levels != null && levels.Count >= level)
        {
            return levels[level - 1].totalIncomeRequired;
        }

        return Math.Max(1.0, baseRequirement) * Math.Pow(Math.Max(1.01, requirementGrowth), level - 1);
    }
}
