using System;
using System.Collections.Generic;

public class StoryGuestDirector
{
    private readonly SaveData saveData;
    private readonly Queue<StoryGuestEncounter> pendingEncounters = new Queue<StoryGuestEncounter>();
    private int currentTierIndex;
    private bool currentCanPrestige;

    private static readonly StoryGuestDefinition[] Definitions =
    {
        new StoryGuestDefinition("alley_jun", "alley", "Jun the Courier", "NIGHT COURIER", 0, false, false, false, false, false, 1, false, 1.35f, 12f, "The alley runner heard your grill can close a plate faster than a bike lane closes a light.", "Jun leaves saying the alley finally has a kitchen worth crossing town for."),
        new StoryGuestDefinition("hongdae_sori", "hongdae", "Sori the Busker", "BUSKER STAR", 1, true, false, false, false, false, 1, false, 1.55f, 10f, "A Hongdae busker wants the kind of plate that makes the crowd stop filming the stage and film the table.", "Sori posts the plate and your queue gets louder by the minute."),
        new StoryGuestDefinition("gangnam_park", "gangnam", "Director Park", "PRIVATE ROOM VIP", 2, true, false, false, false, false, 1, true, 1.75f, 9f, "A private-room regular wants an exact-cut plate before the investor table cools.", "Director Park quietly upgrades you from rumor to recommendation."),
        new StoryGuestDefinition("hanok_sunwoo", "hanok", "Sunwoo the Keeper", "FIREKEEPER", 3, false, true, false, false, false, 2, true, 1.62f, 11f, "An elder from the Hanok district wants proof the room still respects precision under pressure.", "Sunwoo nods once. In this house, that counts as applause."),
        new StoryGuestDefinition("global_niko", "global", "Producer Niko", "HEADLINER", 4, true, true, false, false, false, 2, true, 1.92f, 10f, "The world-stage producer wants a two-plate showcase that feels like a finale rehearsal.", "Niko stops scouting and starts booking."),
        new StoryGuestDefinition("global_amira_finale", "global", "Amira Finale Table", "RIVAL BOSS", 4, true, true, true, true, true, 2, true, 2.10f, 8f, "The finale table is seated. This is the room that decides whether the season deserves a relaunch.", "The finale table clears and the next season feels inevitable."),
    };

    public StoryGuestDirector(SaveData saveData)
    {
        this.saveData = saveData ?? new SaveData();
    }

    public void SyncMetaState(int tierIndex, bool canPrestige)
    {
        currentTierIndex = Math.Max(0, tierIndex);
        currentCanPrestige = canPrestige;
        EnsurePendingEncounters();
    }

    public bool TryDequeueEncounter(out StoryGuestEncounter encounter)
    {
        if (pendingEncounters.Count > 0)
        {
            encounter = pendingEncounters.Dequeue();
            return true;
        }

        encounter = default;
        return false;
    }

    public void MarkResolved(string encounterId)
    {
        if (string.IsNullOrEmpty(encounterId))
        {
            return;
        }

        if (saveData.resolvedStoryGuestIds == null)
        {
            saveData.resolvedStoryGuestIds = new List<string>();
        }

        if (!saveData.resolvedStoryGuestIds.Contains(encounterId))
        {
            saveData.resolvedStoryGuestIds.Add(encounterId);
        }
    }

    public bool TryResolveEncounter(string encounterId, out StoryGuestEncounter encounter)
    {
        if (TryGetEncounter(encounterId, out encounter))
        {
            MarkResolved(encounterId);
            return true;
        }

        return false;
    }

    public bool IsResolved(string encounterId)
    {
        return saveData.resolvedStoryGuestIds != null && saveData.resolvedStoryGuestIds.Contains(encounterId);
    }

    private void EnsurePendingEncounters()
    {
        for (int i = 0; i < Definitions.Length; i++)
        {
            var def = Definitions[i];
            if (currentTierIndex < def.requiredTierIndex)
            {
                continue;
            }

            if (def.requiresPrestigeReady && !currentCanPrestige)
            {
                continue;
            }

            if (IsResolved(def.id) || PendingContains(def.id))
            {
                continue;
            }

            pendingEncounters.Enqueue(new StoryGuestEncounter
            {
                id = def.id,
                districtId = def.districtId,
                displayName = def.displayName,
                label = def.label,
                isVip = def.isVip,
                isCritic = def.isCritic,
                isFinaleGuest = def.isFinaleGuest,
                isBossGuest = def.isBossGuest,
                requestedServings = def.requestedServings,
                requiresExactCut = def.requiresExactCut,
                tipMultiplier = def.tipMultiplier,
                patienceSeconds = def.patienceSeconds,
                arrivalLine = def.arrivalLine,
                resolvedLine = def.resolvedLine,
            });
        }
    }

    private bool PendingContains(string encounterId)
    {
        foreach (var pending in pendingEncounters)
        {
            if (string.Equals(pending.id, encounterId, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private bool TryGetEncounter(string encounterId, out StoryGuestEncounter encounter)
    {
        for (int i = 0; i < Definitions.Length; i++)
        {
            var def = Definitions[i];
            if (!string.Equals(def.id, encounterId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            encounter = new StoryGuestEncounter
            {
                id = def.id,
                districtId = def.districtId,
                displayName = def.displayName,
                label = def.label,
                isVip = def.isVip,
                isCritic = def.isCritic,
                isFinaleGuest = def.isFinaleGuest,
                isBossGuest = def.isBossGuest,
                requestedServings = def.requestedServings,
                requiresExactCut = def.requiresExactCut,
                tipMultiplier = def.tipMultiplier,
                patienceSeconds = def.patienceSeconds,
                arrivalLine = def.arrivalLine,
                resolvedLine = def.resolvedLine,
            };
            return true;
        }

        encounter = default;
        return false;
    }

    private sealed class StoryGuestDefinition
    {
        public readonly string id;
        public readonly string districtId;
        public readonly string displayName;
        public readonly string label;
        public readonly int requiredTierIndex;
        public readonly bool isVip;
        public readonly bool isCritic;
        public readonly bool isFinaleGuest;
        public readonly bool requiresPrestigeReady;
        public readonly bool isBossGuest;
        public readonly int requestedServings;
        public readonly bool requiresExactCut;
        public readonly float tipMultiplier;
        public readonly float patienceSeconds;
        public readonly string arrivalLine;
        public readonly string resolvedLine;

        public StoryGuestDefinition(
            string id,
            string districtId,
            string displayName,
            string label,
            int requiredTierIndex,
            bool isVip,
            bool isCritic,
            bool isFinaleGuest,
            bool requiresPrestigeReady,
            bool isBossGuest,
            int requestedServings,
            bool requiresExactCut,
            float tipMultiplier,
            float patienceSeconds,
            string arrivalLine,
            string resolvedLine)
        {
            this.id = id;
            this.districtId = districtId;
            this.displayName = displayName;
            this.label = label;
            this.requiredTierIndex = requiredTierIndex;
            this.isVip = isVip;
            this.isCritic = isCritic;
            this.isFinaleGuest = isFinaleGuest;
            this.requiresPrestigeReady = requiresPrestigeReady;
            this.isBossGuest = isBossGuest;
            this.requestedServings = requestedServings;
            this.requiresExactCut = requiresExactCut;
            this.tipMultiplier = tipMultiplier;
            this.patienceSeconds = patienceSeconds;
            this.arrivalLine = arrivalLine;
            this.resolvedLine = resolvedLine;
        }
    }
}

public struct StoryGuestEncounter
{
    public string id;
    public string districtId;
    public string displayName;
    public string label;
    public bool isVip;
    public bool isCritic;
    public bool isFinaleGuest;
    public bool isBossGuest;
    public int requestedServings;
    public bool requiresExactCut;
    public float tipMultiplier;
    public float patienceSeconds;
    public string arrivalLine;
    public string resolvedLine;
}
