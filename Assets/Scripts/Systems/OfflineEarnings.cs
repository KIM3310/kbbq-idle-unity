public class OfflineEarnings
{
    public double Calculate(long lastTimestamp, double incomePerSec, int maxHours = 8)
    {
        return KbbqIdle.Sim.OfflineEarningsMath.Calculate(TimeUtil.UtcNowUnix(), lastTimestamp, incomePerSec, maxHours);
    }
}
