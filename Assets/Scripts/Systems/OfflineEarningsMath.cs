using System;

namespace KbbqIdle.Sim
{
    // Shared by the Unity runtime and the executable .NET regression suite.
    public static class OfflineEarningsMath
    {
        public static double Calculate(long nowUnixSeconds, long lastUnixSeconds,
            double incomePerSec, int maxOfflineHours = 8, double offlineRate = 0.6)
        {
            if (lastUnixSeconds <= 0 || nowUnixSeconds <= lastUnixSeconds || maxOfflineHours <= 0)
                return 0;
            if (double.IsNaN(incomePerSec) || double.IsInfinity(incomePerSec) || incomePerSec <= 0 ||
                double.IsNaN(offlineRate) || double.IsInfinity(offlineRate) || offlineRate <= 0 || offlineRate > 1)
                return 0;
            long elapsed = Math.Min(nowUnixSeconds - lastUnixSeconds, (long)maxOfflineHours * 3600);
            double reward = incomePerSec * elapsed * offlineRate;
            return double.IsInfinity(reward) || double.IsNaN(reward) ? 0 : reward;
        }
    }
}
