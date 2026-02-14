using System.Collections.Generic;
using UnityEngine;

public class MenuSystem
{
    private readonly List<MenuItem> allItems = new List<MenuItem>();
    private readonly HashSet<string> unlockedIds = new HashSet<string>();
    private readonly UpgradeSystem upgradeSystem;

    public MenuSystem(IEnumerable<MenuItem> items, UpgradeSystem upgradeSystem, IEnumerable<string> unlocked, int playerLevel)
    {
        this.upgradeSystem = upgradeSystem;

        if (items != null)
        {
            foreach (var item in items)
            {
                if (item == null || string.IsNullOrEmpty(item.id))
                {
                    continue;
                }
                allItems.Add(item);
            }
        }

        if (unlocked != null)
        {
            foreach (var id in unlocked)
            {
                if (!string.IsNullOrEmpty(id))
                {
                    unlockedIds.Add(id);
                }
            }
        }

        UnlockByLevel(playerLevel);
        EnsureAtLeastOneItem();
    }

    public double CalculateMenuIncome()
    {
        double total = 0;
        foreach (var item in allItems)
        {
            if (!unlockedIds.Contains(item.id))
            {
                continue;
            }

            var menuMultiplier = upgradeSystem != null ? upgradeSystem.GetMenuMultiplier(item.id) : 1.0;
            total += item.basePrice * item.bonusMultiplier * menuMultiplier;
        }
        return total;
    }

    public void UnlockByLevel(int playerLevel)
    {
        foreach (var item in allItems)
        {
            if (item.unlockLevel <= playerLevel)
            {
                unlockedIds.Add(item.id);
            }
        }
    }

    public bool UnlockItem(string menuId)
    {
        if (string.IsNullOrEmpty(menuId))
        {
            return false;
        }

        return unlockedIds.Add(menuId);
    }

    public List<string> GetUnlockedIds()
    {
        return new List<string>(unlockedIds);
    }

    public List<MenuItem> GetUnlockedItems()
    {
        var items = new List<MenuItem>();
        foreach (var item in allItems)
        {
            if (item != null && unlockedIds.Contains(item.id))
            {
                items.Add(item);
            }
        }
        return items;
    }

    public MenuItem GetRandomUnlockedItem()
    {
        var unlocked = GetUnlockedItems();
        if (unlocked.Count == 0)
        {
            return allItems.Count > 0 ? allItems[0] : null;
        }

        return unlocked[Random.Range(0, unlocked.Count)];
    }

    private void EnsureAtLeastOneItem()
    {
        if (unlockedIds.Count > 0 || allItems.Count == 0)
        {
            return;
        }

        unlockedIds.Add(allItems[0].id);
    }
}
