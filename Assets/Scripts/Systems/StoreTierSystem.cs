using System.Collections.Generic;
using UnityEngine;

public class StoreTierSystem
{
    private readonly List<StoreTier> tiers = new List<StoreTier>();

    public int CurrentTierIndex { get; private set; }

    public StoreTier CurrentTier
    {
        get
        {
            if (tiers.Count == 0)
            {
                return null;
            }

            CurrentTierIndex = Mathf.Clamp(CurrentTierIndex, 0, tiers.Count - 1);
            return tiers[CurrentTierIndex];
        }
    }

    public float CurrentTierMultiplier => CurrentTier != null ? CurrentTier.incomeMultiplier : 1f;

    public StoreTierSystem(IEnumerable<StoreTier> tiers, int currentIndex)
    {
        if (tiers != null)
        {
            foreach (var tier in tiers)
            {
                if (tier == null || string.IsNullOrEmpty(tier.id))
                {
                    continue;
                }
                this.tiers.Add(tier);
            }
        }
        CurrentTierIndex = Mathf.Max(0, currentIndex);
    }

    public bool TryAdvanceTier(int playerLevel)
    {
        var nextIndex = CurrentTierIndex + 1;
        if (nextIndex >= tiers.Count)
        {
            return false;
        }

        var nextTier = tiers[nextIndex];
        if (playerLevel < nextTier.unlockLevel)
        {
            return false;
        }

        CurrentTierIndex = nextIndex;
        return true;
    }
}
