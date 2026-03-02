using System;

public class OfflineEarnings
{
    private const double OfflineRate = 0.6; // 60% rate
    private const int DefaultMaxHours = 8;

    public double Calculate(long lastTimestamp, double incomePerSec, int maxHours = DefaultMaxHours)
    {
        if (incomePerSec <= 0)
        {
            return 0;
        }
        if (maxHours <= 0)
        {
            return 0;
        }

        var now = TimeUtil.UtcNowUnix();
        if (lastTimestamp > now)
        {
            return 0;
        }

        var offlineSeconds = Math.Max(0, now - lastTimestamp);
        var capSeconds = maxHours * 3600;
        if (offlineSeconds > capSeconds)
        {
            offlineSeconds = capSeconds;
        }
        return incomePerSec * offlineSeconds * OfflineRate;
    }
}
