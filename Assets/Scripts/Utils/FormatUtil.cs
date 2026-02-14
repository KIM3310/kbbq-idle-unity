using System;

public static class FormatUtil
{
    private static readonly string[] Suffixes = { "", "K", "M", "B", "T" };

    public static string FormatCurrency(double value)
    {
        if (value < 0)
        {
            return "0";
        }

        var suffixIndex = 0;
        var shortened = value;
        while (shortened >= 1000 && suffixIndex < Suffixes.Length - 1)
        {
            shortened /= 1000.0;
            suffixIndex++;
        }

        var format = shortened >= 100 ? "0" : shortened >= 10 ? "0.0" : "0.00";
        return shortened.ToString(format) + Suffixes[suffixIndex];
    }
}
