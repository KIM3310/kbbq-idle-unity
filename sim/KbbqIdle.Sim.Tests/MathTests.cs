using KbbqIdle.Sim;
using Xunit;

namespace KbbqIdle.Sim.Tests;

public class MathTests
{
    [Fact]
    public void EconomyMath_ComputeIncomePerSec_MultipliesAllTerms()
    {
        var income = EconomyMath.ComputeIncomePerSec(
            baseIncome: 10,
            upgradeMultiplier: 2,
            staffMultiplier: 1.5,
            serviceMultiplier: 1.2,
            storeMultiplier: 1.1,
            boostMultiplier: 3,
            tipMultiplier: 1.05,
            comboMultiplier: 1.1,
            prestigeMultiplier: 1.2
        );

        Assert.Equal(10 * 2 * 1.5 * 1.2 * 1.1 * 3 * 1.05 * 1.1 * 1.2, income, 10);
    }

    [Fact]
    public void OfflineEarningsMath_CapsAtMaxHours()
    {
        var now = 1_700_000_000L;
        var last = now - (12 * 3600);
        var income = OfflineEarningsMath.Calculate(
            nowUnixSeconds: now,
            lastUnixSeconds: last,
            incomePerSec: 10,
            maxOfflineHours: 8,
            offlineRate: 0.6
        );

        // 8 hours cap.
        Assert.Equal(10 * (8 * 3600) * 0.6, income, 6);
    }

    [Fact]
    public void PrestigeMath_NotReady_UnderThreshold()
    {
        Assert.False(PrestigeMath.CalculateReward(totalIncome: 49_999, playerLevel: 10).CanPrestige);
        Assert.False(PrestigeMath.CalculateReward(totalIncome: 50_000, playerLevel: 9).CanPrestige);
    }

    [Fact]
    public void PrestigeMath_Ready_ComputesPoints()
    {
        var r = PrestigeMath.CalculateReward(totalIncome: 400_000, playerLevel: 12);
        Assert.True(r.CanPrestige);
        Assert.Equal(2, r.Points); // floor(sqrt(4)) = 2
    }

    [Fact]
    public void ProgressionMath_Monotonic()
    {
        var l1 = ProgressionMath.GetLevelForIncome(0);
        var l2 = ProgressionMath.GetLevelForIncome(10_000);
        Assert.True(l2 >= l1);
    }
}
