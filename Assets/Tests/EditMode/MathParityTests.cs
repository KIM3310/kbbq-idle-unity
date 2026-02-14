using NUnit.Framework;
using UnityEngine;

public class MathParityTests
{
    [Test]
    public void OfflineEarnings_CapsAtMaxHours()
    {
        var offline = new OfflineEarnings();
        var now = TimeUtil.UtcNowUnix();
        var last = now - (12 * 3600);
        var income = offline.Calculate(last, incomePerSec: 10.0, maxHours: 8);
        Assert.AreEqual(10.0 * (8 * 3600) * 0.6, income, 0.0001);
    }

    [Test]
    public void Prestige_NotReady_UnderThreshold()
    {
        var prestige = new PrestigeSystem();
        var r1 = prestige.CalculateReward(totalIncome: 49_999, playerLevel: 10);
        var r2 = prestige.CalculateReward(totalIncome: 50_000, playerLevel: 9);
        Assert.IsFalse(r1.canPrestige);
        Assert.IsFalse(r2.canPrestige);
    }

    [Test]
    public void Prestige_Ready_ComputesPoints()
    {
        var prestige = new PrestigeSystem();
        var r = prestige.CalculateReward(totalIncome: 400_000, playerLevel: 12);
        Assert.IsTrue(r.canPrestige);
        Assert.AreEqual(2, r.points);
    }

    [Test]
    public void Progression_Monotonic()
    {
        var prog = new ProgressionSystem(null, baseRequirement: 50.0, growth: 1.28, maxLevel: 100);
        var l1 = prog.GetLevelForIncome(0);
        var l2 = prog.GetLevelForIncome(10_000);
        Assert.GreaterOrEqual(l2, l1);
    }

    [Test]
    public void SaveSystem_DetectsChecksumMismatch()
    {
        var save = new SaveSystem();
        save.Clear();

        var data = new SaveData { playerLevel = 5, currency = 123.0 };
        save.Save(data);

        var loaded = save.Load();
        Assert.AreEqual(123.0, loaded.currency, 0.0001);

        // Tamper JSON but keep old checksum -> should be rejected.
        var oldChecksum = PlayerPrefs.GetString("KBBQ_IDLE_SAVE_SHA256");
        PlayerPrefs.SetString("KBBQ_IDLE_SAVE", "{\"playerLevel\": 99, \"currency\": 999}");
        PlayerPrefs.SetString("KBBQ_IDLE_SAVE_SHA256", oldChecksum);

        var rejected = save.Load();
        Assert.AreEqual(0.0, rejected.currency, 0.0001);

        save.Clear();
    }
}

