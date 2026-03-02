using KbbqIdle.Sim;

var failures = new List<string>();

AssertNear(
    name: "EconomyMath multiplies all terms",
    actual: EconomyMath.ComputeIncomePerSec(
        baseIncome: 10,
        upgradeMultiplier: 2,
        staffMultiplier: 1.5,
        serviceMultiplier: 1.2,
        storeMultiplier: 1.1,
        boostMultiplier: 3,
        tipMultiplier: 1.05,
        comboMultiplier: 1.1,
        prestigeMultiplier: 1.2
    ),
    expected: 10 * 2 * 1.5 * 1.2 * 1.1 * 3 * 1.05 * 1.1 * 1.2,
    tolerance: 1e-9,
    failures: failures
);

var now = 1_700_000_000L;
var last = now - (12 * 3600);
AssertNear(
    name: "OfflineEarningsMath caps at max hours",
    actual: OfflineEarningsMath.Calculate(
        nowUnixSeconds: now,
        lastUnixSeconds: last,
        incomePerSec: 10,
        maxOfflineHours: 8,
        offlineRate: 0.6
    ),
    expected: 10 * (8 * 3600) * 0.6,
    tolerance: 1e-9,
    failures: failures
);

AssertNear(
    name: "OfflineEarningsMath returns zero when maxOfflineHours <= 0",
    actual: OfflineEarningsMath.Calculate(
        nowUnixSeconds: now,
        lastUnixSeconds: last,
        incomePerSec: 10,
        maxOfflineHours: 0,
        offlineRate: 0.6
    ),
    expected: 0,
    tolerance: 1e-9,
    failures: failures
);

var prestigeReward = PrestigeMath.CalculateReward(totalIncome: 400_000, playerLevel: 12);
AssertTrue("PrestigeMath ready state", prestigeReward.CanPrestige, failures);
AssertEqual("PrestigeMath points", prestigeReward.Points, 2, failures);

var levelAtZero = ProgressionMath.GetLevelForIncome(0);
var levelAtTenK = ProgressionMath.GetLevelForIncome(10_000);
AssertTrue("ProgressionMath monotonic", levelAtTenK >= levelAtZero, failures);

if (failures.Count > 0)
{
    Console.Error.WriteLine("[SIM-SMOKE] FAILED");
    foreach (var failure in failures)
    {
        Console.Error.WriteLine("- " + failure);
    }
    return 1;
}

Console.WriteLine("[SIM-SMOKE] PASS (all deterministic checks)");
return 0;

static void AssertNear(string name, double actual, double expected, double tolerance, ICollection<string> failures)
{
    if (double.IsNaN(actual) || Math.Abs(actual - expected) > tolerance)
    {
        failures.Add($"{name}: expected {expected}, got {actual}");
    }
}

static void AssertEqual(string name, int actual, int expected, ICollection<string> failures)
{
    if (actual != expected)
    {
        failures.Add($"{name}: expected {expected}, got {actual}");
    }
}

static void AssertTrue(string name, bool condition, ICollection<string> failures)
{
    if (!condition)
    {
        failures.Add($"{name}: expected true, got false");
    }
}
