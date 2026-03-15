using Xunit;

namespace KbbqIdle.Sim.Tests;

public class BossBattleRulesTests
{
    [Fact]
    public void BossBattleRules_ChampionPhaseTwoRequiresDailySpecial()
    {
        var rule = global::BossBattleRules.GetRule("champion_mirae", 2, 3, "fallback");
        Assert.True(rule.RequiresDailySpecial);
        Assert.Equal(1, rule.AdditionalServings);
        Assert.True(rule.ForceExactCut);
    }

    [Fact]
    public void BossBattleRules_ChampionPhaseThreeRequiresCombo()
    {
        var rule = global::BossBattleRules.GetRule("champion_mirae", 3, 3, "fallback");

        Assert.False(global::BossBattleRules.IsSatisfied(rule, perfectServe: true, dailySpecialServed: true, comboCount: 3, waitRatio: 0.1f));
        Assert.True(global::BossBattleRules.IsSatisfied(rule, perfectServe: true, dailySpecialServed: true, comboCount: 4, waitRatio: 0.1f));
    }

    [Fact]
    public void BossBattleRules_GlobalFinalePhaseOneRequiresSpecial()
    {
        var rule = global::BossBattleRules.GetRule("global_amira_finale", 1, 2, "fallback");

        Assert.False(global::BossBattleRules.IsSatisfied(rule, perfectServe: true, dailySpecialServed: false, comboCount: 4, waitRatio: 0.1f));
        Assert.True(global::BossBattleRules.IsSatisfied(rule, perfectServe: true, dailySpecialServed: true, comboCount: 1, waitRatio: 0.1f));
    }
}
