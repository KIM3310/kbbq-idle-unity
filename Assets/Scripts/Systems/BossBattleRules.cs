public struct BossPhaseRule
{
    public string RuleText;
    public string FailureText;
    public bool RequiresDailySpecial;
    public int MinComboCount;
    public float MaxWaitRatio;
    public int AdditionalServings;
    public bool ForceExactCut;
}

public static class BossBattleRules
{
    public static BossPhaseRule GetRule(string encounterId, int phase, int phaseCount, string fallbackRule)
    {
        var normalized = (encounterId ?? string.Empty).ToLowerInvariant();
        var clampedPhase = phase < 1 ? 1 : phase;

        switch (normalized)
        {
            case "gangnam_park":
                return new BossPhaseRule
                {
                    RuleText = "Exact cut. No excuses. Premium rooms only remember precision.",
                    FailureText = "Park adjusts the napkin. The velvet room only counts exact, flawless cuts.",
                    ForceExactCut = true,
                    MaxWaitRatio = 0.30f,
                };
            case "hanok_sunwoo":
                return clampedPhase == 1
                    ? new BossPhaseRule
                    {
                        RuleText = "Phase 1/2 · Exact discipline. Two plates with no wasted motion.",
                        FailureText = "Sunwoo says nothing. The first phase only counts if the room stays calm and exact.",
                        ForceExactCut = true,
                        MaxWaitRatio = 0.34f,
                    }
                    : new BossPhaseRule
                    {
                        RuleText = "Phase 2/2 · Same plates, less patience. Calm is part of the test.",
                        FailureText = "The house got tense. Sunwoo only passes a plate that feels steady under pressure.",
                        ForceExactCut = true,
                        MaxWaitRatio = 0.24f,
                    };
            case "global_niko":
                return clampedPhase == 1
                    ? new BossPhaseRule
                    {
                        RuleText = "Phase 1/2 · Prime-time opener. Two clean plates under pressure.",
                        FailureText = "Niko wants a live opener, not a warm-up. Build the combo and take the shot again.",
                        ForceExactCut = true,
                        MinComboCount = 2,
                    }
                    : new BossPhaseRule
                    {
                        RuleText = "Phase 2/2 · Headliner check. Three exact plates before the room cools.",
                        FailureText = "The room lost its edge. Niko wants a headliner run with combo and signature heat.",
                        ForceExactCut = true,
                        RequiresDailySpecial = true,
                        MinComboCount = 2,
                        AdditionalServings = 1,
                    };
            case "global_amira_finale":
                return clampedPhase == 1
                    ? new BossPhaseRule
                    {
                        RuleText = "Phase 1/2 · Champion opener. Perfect daily special only.",
                        FailureText = "Amira waves it off. The opener only counts if it lands as the house special.",
                        ForceExactCut = true,
                        RequiresDailySpecial = true,
                        MaxWaitRatio = 0.30f,
                    }
                    : new BossPhaseRule
                    {
                        RuleText = "Phase 2/2 · Three plates, exact cut, combo alive.",
                        FailureText = "The champion table wants pressure and rhythm together. Bring back the combo.",
                        ForceExactCut = true,
                        RequiresDailySpecial = true,
                        MinComboCount = 3,
                        AdditionalServings = 1,
                    };
            case "champion_mirae":
                if (clampedPhase == 1)
                {
                    return new BossPhaseRule
                    {
                        RuleText = "Phase 1/3 · Exact cut opener. Start clean and fast.",
                        FailureText = "Mirae doesn't blink. The opening phase only counts if the rhythm is immediate.",
                        ForceExactCut = true,
                        MaxWaitRatio = 0.28f,
                        MinComboCount = 2,
                    };
                }

                if (clampedPhase == 2)
                {
                    return new BossPhaseRule
                    {
                        RuleText = "Phase 2/3 · Three plates, signature menu, no drift.",
                        FailureText = "Mirae wants the house special to feel inevitable, not improvised.",
                        ForceExactCut = true,
                        RequiresDailySpecial = true,
                        AdditionalServings = 1,
                    };
                }

                return new BossPhaseRule
                {
                    RuleText = "Phase 3/3 · Final crown test. Signature menu, combo alive, zero panic.",
                    FailureText = "The crown stays out of reach. Final phase needs the special, the combo, and the nerve.",
                    ForceExactCut = true,
                    RequiresDailySpecial = true,
                    MinComboCount = 4,
                    MaxWaitRatio = 0.22f,
                    AdditionalServings = 1,
                };
            default:
                return new BossPhaseRule
                {
                    RuleText = string.IsNullOrEmpty(fallbackRule) ? ("Phase " + clampedPhase + "/" + phaseCount + " · Perfect service only.") : fallbackRule,
                    FailureText = "No badge yet. Perfect service required.",
                };
        }
    }

    public static bool IsSatisfied(BossPhaseRule rule, bool perfectServe, bool dailySpecialServed, int comboCount, float waitRatio)
    {
        if (!perfectServe)
        {
            return false;
        }
        if (rule.RequiresDailySpecial && !dailySpecialServed)
        {
            return false;
        }
        if (rule.MinComboCount > 0 && comboCount < rule.MinComboCount)
        {
            return false;
        }
        if (rule.MaxWaitRatio > 0f && waitRatio > rule.MaxWaitRatio)
        {
            return false;
        }
        return true;
    }
}
