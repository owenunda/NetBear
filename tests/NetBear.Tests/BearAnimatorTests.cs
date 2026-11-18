namespace NetBear.Tests;

public class BearAnimatorTests
{
    [Theory]
    [InlineData(false, 0, BearState.Hibernating)]
    [InlineData(false, 50_000_000, BearState.Hibernating)]
    [InlineData(true, 0, BearState.Idle)]
    [InlineData(true, 10239, BearState.Idle)]
    [InlineData(true, 10240, BearState.Running)]
    [InlineData(true, 50_000_000, BearState.Running)]
    public void GetState_ChoosesByConnectionAndTraffic(bool connected, double totalBps, BearState expected) =>
        Assert.Equal(expected, BearAnimator.GetState(connected, totalBps));

    [Theory]
    [InlineData(BearState.Hibernating, 0, 500)]
    [InlineData(BearState.Idle, 0, 250)]
    [InlineData(BearState.Running, 0, 200)]              // por debajo del umbral se limita a lo más lento
    [InlineData(BearState.Running, 10240, 200)]          // 10 KB/s
    [InlineData(BearState.Running, 327680, 120)]         // 320 KB/s: punto medio logarítmico
    [InlineData(BearState.Running, 10485760, 40)]        // 10 MB/s
    [InlineData(BearState.Running, 1e12, 40)]            // por encima del máximo se limita
    public void GetFrameIntervalMs_UsesLogScaleWhenRunning(BearState state, double totalBps, int expected) =>
        Assert.Equal(expected, BearAnimator.GetFrameIntervalMs(state, totalBps));
}
