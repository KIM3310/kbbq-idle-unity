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
        new StoryGuestDefinition("alley_jun", "alley", "Jun the Courier", "ALLEY CAPTAIN", "Courier Badge", "Clear the plate before the street cools. Speed is the whole point.", 0, false, false, false, false, false, false, 1, 1, false, 1.35f, 12f, "The alley runner heard your grill can close a plate faster than a bike lane closes a light.", "Jun leaves saying the alley finally has a kitchen worth crossing town for.", "Too slow. In this alley, speed is flavor.", "First test clear. The alley starts believing."),
        new StoryGuestDefinition("hongdae_sori", "hongdae", "Sori the Busker", "NEON LEADER", "Encore Badge", "Make it camera-worthy. Fast, hot, and impossible to ignore.", 1, true, false, false, false, false, false, 1, 1, false, 1.55f, 10f, "A Hongdae busker wants the kind of plate that makes the crowd stop filming the stage and film the table.", "Sori posts the plate and your queue gets louder by the minute.", "That was not headline material. One more time, brighter.", "Encore clear. The room starts chanting for more."),
        new StoryGuestDefinition("gangnam_park", "gangnam", "Director Park", "VELVET LEADER", "Velvet Badge", "Exact cut. No excuses. Premium rooms only remember precision.", 2, true, false, false, false, true, false, 1, 1, true, 1.75f, 9f, "A private-room regular wants an exact-cut plate before the investor table cools.", "Director Park quietly upgrades you from rumor to recommendation.", "Not clean enough. Precision is the only language here.", "The velvet room stops doubting you."),
        new StoryGuestDefinition("hanok_sunwoo", "hanok", "Sunwoo the Keeper", "FIREKEEPER", "Ember Badge", "Two plates, exact cuts, zero panic. Discipline is the whole test.", 3, false, true, false, false, true, false, 2, 2, true, 1.62f, 11f, "An elder from the Hanok district wants proof the room still respects precision under pressure.", "Sunwoo nods once. In this house, that counts as applause.", "The flame shook. Control the room and try again.", "One phase cleared. The house gets quieter."),
        new StoryGuestDefinition("global_niko", "global", "Producer Niko", "STAGE LEADER", "Headline Badge", "Two plates under pressure. The room must feel like prime time.", 4, true, true, false, false, true, false, 2, 2, true, 1.92f, 10f, "The world-stage producer wants a two-plate showcase that feels like a finale rehearsal.", "Niko stops scouting and starts booking.", "That was rehearsal, not showtime. Reset and take the stage again.", "Phase clear. The room is leaning in now."),
        new StoryGuestDefinition("global_amira_finale", "global", "Amira Finale Table", "RIVAL BOSS", "Champion Badge", "This is the champion table. Perfect service or it does not count.", 4, true, true, true, true, true, false, 2, 2, true, 2.10f, 8f, "The finale table is seated. This is the room that decides whether the season deserves a relaunch.", "The finale table clears and the next season feels inevitable.", "No badge. The champion table wants perfection, not survival.", "Champion phase clear. The room feels historic."),
        new StoryGuestDefinition("champion_mirae", "global", "Champion Mirae", "HOUSE CHAMPION", "Legend Crown", "Three phases. Exact cuts. Zero panic. This is the full league test.", 4, true, true, true, true, true, true, 3, 2, true, 2.35f, 8f, "Champion Mirae steps in with the last table of the season. The whole house goes still.", "Mirae smiles. The league is yours now.", "The crown stays out of reach. A champion run must be perfect.", "Champion phase clear. One more and the crown is yours."),
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

        ClearRetryCount(encounterId);
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

    public bool TryGetEncounterSnapshot(string encounterId, out StoryGuestEncounter encounter)
    {
        return TryGetEncounter(encounterId, out encounter);
    }

    public bool IsResolved(string encounterId)
    {
        return saveData.resolvedStoryGuestIds != null && saveData.resolvedStoryGuestIds.Contains(encounterId);
    }

    public int RecordRetry(string encounterId)
    {
        if (string.IsNullOrEmpty(encounterId))
        {
            return 0;
        }

        if (saveData.storyGuestRetries == null)
        {
            saveData.storyGuestRetries = new List<StoryGuestRetryState>();
        }

        for (int i = 0; i < saveData.storyGuestRetries.Count; i++)
        {
            var retry = saveData.storyGuestRetries[i];
            if (!string.Equals(retry.id, encounterId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            retry.count += 1;
            return retry.count;
        }

        saveData.storyGuestRetries.Add(new StoryGuestRetryState
        {
            id = encounterId,
            count = 1,
        });
        return 1;
    }

    public string GetRetryLine(StoryGuestEncounter encounter, int retryCount)
    {
        if (retryCount <= 1)
        {
            return string.IsNullOrEmpty(encounter.retryLine) ? "No badge yet. Perfect service required." : encounter.retryLine;
        }

        if (retryCount == 2)
        {
            return "The leader folds their arms. \"" + encounter.signatureRule + "\"";
        }

        return "The table is still waiting. This badge only moves for a clean phase win.";
    }

    public BadgeBoardUiState GetBadgeBoardUiState()
    {
        var total = 0;
        var earned = 0;
        var earnedNames = new List<string>();
        string nextBadge = string.Empty;
        var championUnlocked = false;
        var championCleared = false;

        for (int i = 0; i < Definitions.Length; i++)
        {
            var def = Definitions[i];
            if (string.IsNullOrEmpty(def.badgeName))
            {
                continue;
            }

            total++;
            if (IsResolved(def.id))
            {
                earned++;
                earnedNames.Add(def.badgeName);
            }
            else if (string.IsNullOrEmpty(nextBadge))
            {
                nextBadge = def.badgeName;
            }

            if (string.Equals(def.id, "champion_mirae", StringComparison.OrdinalIgnoreCase))
            {
                championUnlocked = HasAllOtherBadges(def.id) && currentCanPrestige;
                championCleared = IsResolved(def.id);
            }
        }

        var complete = total > 0 && earned >= total;
        return new BadgeBoardUiState
        {
            title = complete ? "CHAMPION BOARD" : "LEADER BADGES",
            progressLine = earned + "/" + total + " badges claimed",
            detailLine = complete
                ? "Every district leader cleared. The full board is yours."
                : championCleared
                    ? "Champion cleared. The league remembers your name now."
                    : championUnlocked
                        ? "Champion unlocked. Mirae is waiting for the final league test."
                        : (string.IsNullOrEmpty(nextBadge) ? "Keep chasing story guests." : "Next badge: " + nextBadge),
            badgeLine = earnedNames.Count == 0
                ? "No badges yet"
                : string.Join(" · ", earnedNames),
            accent01 = total <= 0 ? 0f : (float)earned / total,
            visible = true,
        };
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

            if (def.requiresAllBadges && !HasAllOtherBadges(def.id))
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
                badgeName = def.badgeName,
                signatureRule = def.signatureRule,
                isVip = def.isVip,
                isCritic = def.isCritic,
                isFinaleGuest = def.isFinaleGuest,
                isBossGuest = def.isBossGuest,
                bossPhases = def.bossPhases,
                requestedServings = def.requestedServings,
                requiresExactCut = def.requiresExactCut,
                tipMultiplier = def.tipMultiplier,
                patienceSeconds = def.patienceSeconds,
                arrivalLine = def.arrivalLine,
                resolvedLine = def.resolvedLine,
                retryLine = def.retryLine,
                phaseClearLine = def.phaseClearLine,
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

    private void ClearRetryCount(string encounterId)
    {
        if (saveData.storyGuestRetries == null || string.IsNullOrEmpty(encounterId))
        {
            return;
        }

        for (int i = saveData.storyGuestRetries.Count - 1; i >= 0; i--)
        {
            var retry = saveData.storyGuestRetries[i];
            if (retry != null && string.Equals(retry.id, encounterId, StringComparison.OrdinalIgnoreCase))
            {
                saveData.storyGuestRetries.RemoveAt(i);
            }
        }
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
                badgeName = def.badgeName,
                signatureRule = def.signatureRule,
                isVip = def.isVip,
                isCritic = def.isCritic,
                isFinaleGuest = def.isFinaleGuest,
                isBossGuest = def.isBossGuest,
                bossPhases = def.bossPhases,
                requestedServings = def.requestedServings,
                requiresExactCut = def.requiresExactCut,
                tipMultiplier = def.tipMultiplier,
                patienceSeconds = def.patienceSeconds,
                arrivalLine = def.arrivalLine,
                resolvedLine = def.resolvedLine,
                retryLine = def.retryLine,
                phaseClearLine = def.phaseClearLine,
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
        public readonly string badgeName;
        public readonly string signatureRule;
        public readonly int requiredTierIndex;
        public readonly bool isVip;
        public readonly bool isCritic;
        public readonly bool isFinaleGuest;
        public readonly bool requiresPrestigeReady;
        public readonly bool isBossGuest;
        public readonly bool requiresAllBadges;
        public readonly int bossPhases;
        public readonly int requestedServings;
        public readonly bool requiresExactCut;
        public readonly float tipMultiplier;
        public readonly float patienceSeconds;
        public readonly string arrivalLine;
        public readonly string resolvedLine;
        public readonly string retryLine;
        public readonly string phaseClearLine;

        public StoryGuestDefinition(
            string id,
            string districtId,
            string displayName,
            string label,
            string badgeName,
            string signatureRule,
            int requiredTierIndex,
            bool isVip,
            bool isCritic,
            bool isFinaleGuest,
            bool requiresPrestigeReady,
            bool isBossGuest,
            bool requiresAllBadges,
            int bossPhases,
            int requestedServings,
            bool requiresExactCut,
            float tipMultiplier,
            float patienceSeconds,
            string arrivalLine,
            string resolvedLine,
            string retryLine,
            string phaseClearLine)
        {
            this.id = id;
            this.districtId = districtId;
            this.displayName = displayName;
            this.label = label;
            this.badgeName = badgeName;
            this.signatureRule = signatureRule;
            this.requiredTierIndex = requiredTierIndex;
            this.isVip = isVip;
            this.isCritic = isCritic;
            this.isFinaleGuest = isFinaleGuest;
            this.requiresPrestigeReady = requiresPrestigeReady;
            this.isBossGuest = isBossGuest;
            this.requiresAllBadges = requiresAllBadges;
            this.bossPhases = bossPhases;
            this.requestedServings = requestedServings;
            this.requiresExactCut = requiresExactCut;
            this.tipMultiplier = tipMultiplier;
            this.patienceSeconds = patienceSeconds;
            this.arrivalLine = arrivalLine;
            this.resolvedLine = resolvedLine;
            this.retryLine = retryLine;
            this.phaseClearLine = phaseClearLine;
        }
    }

    private bool HasAllOtherBadges(string currentEncounterId)
    {
        for (int i = 0; i < Definitions.Length; i++)
        {
            var def = Definitions[i];
            if (string.IsNullOrEmpty(def.badgeName) || string.Equals(def.id, currentEncounterId, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (!IsResolved(def.id))
            {
                return false;
            }
        }

        return true;
    }
}

public struct StoryGuestEncounter
{
    public string id;
    public string districtId;
    public string displayName;
    public string label;
    public string badgeName;
    public string signatureRule;
    public bool isVip;
    public bool isCritic;
    public bool isFinaleGuest;
    public bool isBossGuest;
    public int bossPhases;
    public int requestedServings;
    public bool requiresExactCut;
    public float tipMultiplier;
    public float patienceSeconds;
    public string arrivalLine;
    public string resolvedLine;
    public string retryLine;
    public string phaseClearLine;
}
