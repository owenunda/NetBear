using System.Diagnostics;
using System.Net.NetworkInformation;

namespace NetBear;

internal readonly record struct Speed(double DownBps, double UpBps)
{
    public double Total => DownBps + UpBps;
}

internal static class SpeedCalculator
{
    public static Speed Compute(
        IReadOnlyDictionary<string, (long Rx, long Tx)> previous,
        IReadOnlyDictionary<string, (long Rx, long Tx)> current,
        double elapsedSeconds)
    {
        if (elapsedSeconds <= 0) return default;

        long rx = 0, tx = 0;
        foreach (var (id, now) in current)
        {
            if (!previous.TryGetValue(id, out var before)) continue;
            // Un contador que baja significa que el adaptador se reconectó: cuenta como 0.
            rx += Math.Max(0, now.Rx - before.Rx);
            tx += Math.Max(0, now.Tx - before.Tx);
        }
        return new Speed(rx / elapsedSeconds, tx / elapsedSeconds);
    }
}

internal sealed class NetSpeedMonitor
{
    Dictionary<string, (long Rx, long Tx)> _previous = Sample();
    long _previousTimestamp = Stopwatch.GetTimestamp();

    public Speed Update()
    {
        var current = Sample();
        long now = Stopwatch.GetTimestamp();
        double elapsed = Stopwatch.GetElapsedTime(_previousTimestamp, now).TotalSeconds;
        // La primera llamada ocurre unas decenas de ms después de la muestra inicial: con un intervalo
        // tan corto la velocidad sale disparatada. Se descarta sin mover la línea base.
        if (elapsed < 0.5) return default;
        var speed = SpeedCalculator.Compute(_previous, current, elapsed);
        _previous = current;
        _previousTimestamp = now;
        return speed;
    }

    public static bool IsConnected() => InternetAdapters().Count > 0;

    static Dictionary<string, (long Rx, long Tx)> Sample()
    {
        var counters = new Dictionary<string, (long Rx, long Tx)>();
        foreach (var adapter in InternetAdapters())
        {
            try
            {
                var stats = adapter.GetIPStatistics();
                counters[adapter.Id] = (stats.BytesReceived, stats.BytesSent);
            }
            catch (Exception) { } // se ignora este adaptador en esta muestra
        }
        return counters;
    }

    // Solo cuentan los adaptadores con puerta de enlace: los virtuales (Hyper-V, WSL, Tailscale) no tienen,
    // así que no mantienen al oso "conectado" sin internet ni duplican el tráfico que ya pasa por el físico.
    static List<NetworkInterface> InternetAdapters()
    {
        var result = new List<NetworkInterface>();
        NetworkInterface[] adapters;
        try { adapters = NetworkInterface.GetAllNetworkInterfaces(); }
        catch (Exception) { return result; }

        foreach (var adapter in adapters)
        {
            if (adapter.OperationalStatus != OperationalStatus.Up) continue;
            if (adapter.NetworkInterfaceType is NetworkInterfaceType.Loopback or NetworkInterfaceType.Tunnel) continue;
            try
            {
                if (adapter.GetIPProperties().GatewayAddresses.Count > 0) result.Add(adapter);
            }
            catch (Exception) { } // se ignora este adaptador
        }
        return result;
    }
}
