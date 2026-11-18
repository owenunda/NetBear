using System.Globalization;

namespace NetBear;

internal static class SpeedFormatter
{
    static readonly string[] Units = ["KB/s", "MB/s", "GB/s"];

    public static string Format(double bytesPerSecond)
    {
        double bps = double.IsNaN(bytesPerSecond) ? 0 : Math.Max(0, bytesPerSecond);
        if (!(bps >= 1024))
            return $"{(long)bps} B/s";

        double value = bps / 1024;
        int unit = 0;
        while (Math.Round(value, 1) >= 1024 && unit < Units.Length - 1)
        {
            value /= 1024;
            unit++;
        }
        return value.ToString("0.0", CultureInfo.InvariantCulture) + " " + Units[unit];
    }
}
