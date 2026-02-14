using System;

public static class TimeUtil
{
    public static long UtcNowUnix()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    public static int UtcDayStamp()
    {
        return UtcDayStamp(DateTime.UtcNow);
    }

    public static int UtcDayStamp(DateTime utcDate)
    {
        return (utcDate.Year * 10000) + (utcDate.Month * 100) + utcDate.Day;
    }

    public static int UtcDayStampOffsetDays(int offsetDays)
    {
        return UtcDayStamp(DateTime.UtcNow.AddDays(offsetDays));
    }
}
