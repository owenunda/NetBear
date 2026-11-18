namespace NetBear;

public enum BearState { Hibernating, Idle, Running }

internal static class BearAnimator
{
    const double RunThresholdBps = 10 * 1024;
    const double MaxSpeedBps = 10 * 1024 * 1024;
    const int SlowestRunMs = 200;
    const int FastestRunMs = 40;

    public static BearState GetState(bool connected, double totalBps) =>
        !connected ? BearState.Hibernating
        : totalBps < RunThresholdBps ? BearState.Idle
        : BearState.Running;

    public static int GetFrameIntervalMs(BearState state, double totalBps) => state switch
    {
        BearState.Hibernating => 500,
        BearState.Idle => 250,
        _ => RunIntervalMs(totalBps),
    };

    // Escala logarítmica: cada x10 de velocidad acelera lo mismo, así 100 KB/s y 50 MB/s se distinguen.
    static int RunIntervalMs(double totalBps)
    {
        double clamped = Math.Clamp(totalBps, RunThresholdBps, MaxSpeedBps);
        double t = Math.Log(clamped / RunThresholdBps) / Math.Log(MaxSpeedBps / RunThresholdBps);
        return (int)Math.Round(SlowestRunMs + (FastestRunMs - SlowestRunMs) * t);
    }
}
