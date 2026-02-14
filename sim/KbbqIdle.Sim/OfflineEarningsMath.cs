namespace KbbqIdle.Sim;

public static class OfflineEarningsMath
{
    public static double Calculate(
        long nowUnixSeconds,
        long lastUnixSeconds,
        double incomePerSec,
        int maxOfflineHours = 8,
        double offlineRate = 0.6
    )
    {
        if (incomePerSec <= 0) return 0;
        if (lastUnixSeconds <= 0) return 0;
        if (lastUnixSeconds > nowUnixSeconds) return 0;

        var offlineSeconds = nowUnixSeconds - lastUnixSeconds;
        if (offlineSeconds <= 0) return 0;

        var capSeconds = Math.Max(0, maxOfflineHours) * 3600L;
        if (capSeconds > 0 && offlineSeconds > capSeconds)
        {
            offlineSeconds = capSeconds;
        }

        if (offlineRate <= 0) return 0;

        return incomePerSec * offlineSeconds * offlineRate;
    }
}
