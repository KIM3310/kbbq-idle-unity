public class RateLimiter
{
    private readonly int minIntervalSeconds;
    private long lastActionTs;

    public RateLimiter(int minIntervalSeconds)
    {
        this.minIntervalSeconds = minIntervalSeconds;
    }

    public bool TryConsume()
    {
        var now = TimeUtil.UtcNowUnix();
        if (lastActionTs > 0 && (now - lastActionTs) < minIntervalSeconds)
        {
            return false;
        }

        lastActionTs = now;
        return true;
    }
}
