using System.Collections.Generic;
using UnityEngine;

public class MenuSystem
{
    private readonly List<MenuItem> allItems = new List<MenuItem>();
    private readonly HashSet<string> unlockedIds = new HashSet<string>();
    private readonly UpgradeSystem upgradeSystem;
    private string spotlightMenuId;

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

        if (!string.IsNullOrEmpty(spotlightMenuId) && Random.value < 0.38f)
        {
            for (int i = 0; i < unlocked.Count; i++)
            {
                var item = unlocked[i];
                if (item != null && string.Equals(item.id, spotlightMenuId, System.StringComparison.OrdinalIgnoreCase))
                {
                    return item;
                }
            }
        }

        return unlocked[Random.Range(0, unlocked.Count)];
    }

    public MenuItem GetPreferredUnlockedItem(string customerTypeId)
    {
        var unlocked = GetUnlockedItems();
        if (unlocked.Count == 0)
        {
            return allItems.Count > 0 ? allItems[0] : null;
        }

        var customerId = string.IsNullOrEmpty(customerTypeId) ? string.Empty : customerTypeId.ToLowerInvariant();
        var weighted = new List<(MenuItem item, float weight)>();
        var totalWeight = 0f;

        for (int i = 0; i < unlocked.Count; i++)
        {
            var item = unlocked[i];
            if (item == null)
            {
                continue;
            }

            var weight = 1f;
            var normalizedMenuId = string.IsNullOrEmpty(item.id) ? string.Empty : item.id.ToLowerInvariant();
            if (!string.IsNullOrEmpty(spotlightMenuId) &&
                string.Equals(normalizedMenuId, spotlightMenuId.ToLowerInvariant(), System.StringComparison.OrdinalIgnoreCase))
            {
                weight += 1.2f;
            }

            if (customerId == "local")
            {
                if (normalizedMenuId.Contains("pork") || normalizedMenuId.Contains("kimchi") || normalizedMenuId.Contains("soju"))
                {
                    weight += 1.4f;
                }
            }
            else if (customerId == "tourist")
            {
                if (normalizedMenuId.Contains("beef") || normalizedMenuId.Contains("seafood") || normalizedMenuId.Contains("cold_noodle"))
                {
                    weight += 1.35f;
                }
            }
            else if (customerId == "foodie")
            {
                if (normalizedMenuId.Contains("premium") || normalizedMenuId.Contains("signature") || normalizedMenuId.Contains("bingsu"))
                {
                    weight += 1.55f;
                }
            }

            weighted.Add((item, weight));
            totalWeight += weight;
        }

        if (weighted.Count == 0 || totalWeight <= 0f)
        {
            return unlocked[Random.Range(0, unlocked.Count)];
        }

        var roll = Random.Range(0f, totalWeight);
        var cursor = 0f;
        for (int i = 0; i < weighted.Count; i++)
        {
            cursor += weighted[i].weight;
            if (roll <= cursor)
            {
                return weighted[i].item;
            }
        }

        return weighted[weighted.Count - 1].item;
    }

    public void SetSpotlightMenu(string menuId)
    {
        spotlightMenuId = string.IsNullOrEmpty(menuId) ? null : menuId;
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
