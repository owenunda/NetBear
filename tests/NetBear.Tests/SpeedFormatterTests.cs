namespace NetBear.Tests;

public class SpeedFormatterTests
{
    [Theory]
    [InlineData(0, "0 B/s")]
    [InlineData(-5, "0 B/s")]
    [InlineData(1023, "1023 B/s")]
    [InlineData(1024, "1.0 KB/s")]
    [InlineData(1536, "1.5 KB/s")]
    [InlineData(1048576, "1.0 MB/s")]
    [InlineData(2516582.4, "2.4 MB/s")]
    [InlineData(1073741824, "1.0 GB/s")]
    [InlineData(5497558138880, "5120.0 GB/s")]
    public void Format_UsesBase1024Units(double bytesPerSecond, string expected) =>
        Assert.Equal(expected, SpeedFormatter.Format(bytesPerSecond));
}
