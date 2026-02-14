using System.Collections.Generic;
using UnityEngine;

public class UpgradeSystem
{
    private readonly Dictionary<string, UpgradeData> upgradesById = new Dictionary<string, UpgradeData>();
    private readonly Dictionary<string, int> levels = new Dictionary<string, int>();

    public event System.Action<string, int> OnUpgradePurchased;

    public UpgradeSystem(IEnumerable<UpgradeData> upgrades, IEnumerable<UpgradeLevelEntry> savedLevels)
    {
        if (upgrades != null)
        {
            foreach (var upgrade in upgrades)
            {
                if (upgrade == null || string.IsNullOrEmpty(upgrade.id))
                {
                    continue;
                }

                upgradesById[upgrade.id] = upgrade;
            }
        }

        if (savedLevels != null)
        {
            foreach (var entry in savedLevels)
            {
                if (string.IsNullOrEmpty(entry.id))
                {
                    continue;
                }
                levels[entry.id] = Mathf.Max(0, entry.level);
            }
        }
    }

    public bool PurchaseUpgrade(string upgradeId, EconomySystem economy)
    {
        if (string.IsNullOrEmpty(upgradeId) || economy == null)
        {
            return false;
        }

        var cost = GetUpgradeCost(upgradeId);
        if (!economy.Spend(cost))
        {
            return false;
        }

        var newLevel = GetLevel(upgradeId) + 1;
        levels[upgradeId] = newLevel;
        OnUpgradePurchased?.Invoke(upgradeId, newLevel);
        return true;
    }

    public double GetUpgradeCost(string upgradeId)
    {
        if (!upgradesById.TryGetValue(upgradeId, out var upgrade))
        {
            return double.MaxValue;
        }

        var level = GetLevel(upgradeId);
        return upgrade.baseCost * Mathf.Pow(upgrade.costMultiplier, level);
    }

    public int GetLevel(string upgradeId)
    {
        return levels.TryGetValue(upgradeId, out var level) ? level : 0;
    }

    public double GetGlobalMultiplier()
    {
        return GetCategoryMultiplier("income");
    }

    public double GetMenuMultiplier(string menuId)
    {
        return GetCategoryMultiplier("menu", menuId);
    }

    public double GetCategoryMultiplier(string category, string targetId = "")
    {
        double multiplier = 1.0;
        foreach (var kvp in upgradesById)
        {
            var upgrade = kvp.Value;
            if (!string.Equals(upgrade.category, category, System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!string.IsNullOrEmpty(targetId) && !string.IsNullOrEmpty(upgrade.targetId) &&
                !string.Equals(upgrade.targetId, targetId, System.StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var level = GetLevel(kvp.Key);
            if (level <= 0)
            {
                continue;
            }

            multiplier *= Mathf.Pow(1f + upgrade.effectValue, level);
        }
        return multiplier;
    }

    public List<UpgradeLevelEntry> ExportLevels()
    {
        var list = new List<UpgradeLevelEntry>();
        foreach (var kvp in levels)
        {
            list.Add(new UpgradeLevelEntry { id = kvp.Key, level = kvp.Value });
        }
        return list;
    }

    public void Reset(IEnumerable<UpgradeLevelEntry> savedLevels)
    {
        levels.Clear();
        if (savedLevels == null)
        {
            return;
        }

        foreach (var entry in savedLevels)
        {
            if (string.IsNullOrEmpty(entry.id))
            {
                continue;
            }
            levels[entry.id] = Mathf.Max(0, entry.level);
        }
    }
}
