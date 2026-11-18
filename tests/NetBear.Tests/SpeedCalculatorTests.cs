namespace NetBear.Tests;

public class SpeedCalculatorTests
{
    static Dictionary<string, (long Rx, long Tx)> Counters(params (string Id, long Rx, long Tx)[] adapters) =>
        adapters.ToDictionary(a => a.Id, a => (a.Rx, a.Tx));

    [Fact]
    public void Compute_ReturnsDeltaPerSecond()
    {
        var speed = SpeedCalculator.Compute(Counters(("wifi", 1000, 500)), Counters(("wifi", 3048, 1524)), 1.0);
        Assert.Equal(new Speed(2048, 1024), speed);
    }

    [Fact]
    public void Compute_DividesByRealElapsedTime()
    {
        var speed = SpeedCalculator.Compute(Counters(("wifi", 0, 0)), Counters(("wifi", 4096, 2048)), 2.0);
        Assert.Equal(new Speed(2048, 1024), speed);
    }

    [Fact]
    public void Compute_TreatsResetCounterAsZero()
    {
        var speed = SpeedCalculator.Compute(Counters(("wifi", 5000, 5000)), Counters(("wifi", 100, 100)), 1.0);
        Assert.Equal(new Speed(0, 0), speed);
    }

    [Fact]
    public void Compute_SumsAdaptersAndIgnoresNewOnes()
    {
        var previous = Counters(("wifi", 0, 0), ("eth", 0, 0));
        var current = Counters(("wifi", 100, 10), ("eth", 200, 20), ("usb", 9999, 9999));
        Assert.Equal(new Speed(300, 30), SpeedCalculator.Compute(previous, current, 1.0));
    }

    [Fact]
    public void Compute_ReturnsZeroWhenNoTimeElapsed()
    {
        var speed = SpeedCalculator.Compute(Counters(("wifi", 0, 0)), Counters(("wifi", 100, 100)), 0);
        Assert.Equal(new Speed(0, 0), speed);
    }

    [Fact]
    public void Total_IsDownPlusUp() => Assert.Equal(30, new Speed(10, 20).Total);
}
