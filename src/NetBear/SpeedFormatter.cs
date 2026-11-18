using System.Globalization;

namespace NetBear;

internal static class SpeedFormatter
{
    static readonly string[] Units = ["KB/s", "MB/s", "GB/s"];

    public static string Format(double bytesPerSecond)
    {
        if (bytesPerSecond < 1024)
            return $"{(long)Math.Max(0, bytesPerSecond)} B/s";

        double value = bytesPerSecond / 1024;
        int unit = 0;
        while (value >= 1024 && unit < Units.Length - 1)
        {
            value /= 1024;
            unit++;
        }
        return value.ToString("0.0", CultureInfo.InvariantCulture) + " " + Units[unit];
    }
}
