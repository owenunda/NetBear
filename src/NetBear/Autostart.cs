using Microsoft.Win32;

namespace NetBear;

internal static unsafe class Autostart
{
    const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    const string ValueName = "NetBear";

    // Dentro de un paquete MSIX (Microsoft Store) Windows ignora la clave Run: el inicio lo maneja la
    // StartupTask del manifiesto, que el usuario activa en Configuración › Aplicaciones › Inicio.
    public static bool IsPackaged { get; } = DetectPackaged();

    static bool DetectPackaged()
    {
        uint length = 0;
        return Native.GetCurrentPackageFullName(ref length, null) != Native.APPMODEL_ERROR_NO_PACKAGE;
    }

    public static bool IsEnabled()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey);
        return key?.GetValue(ValueName) is string;
    }

    public static void Set(bool enabled)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunKey);
        if (enabled) key.SetValue(ValueName, $"\"{Environment.ProcessPath}\"");
        else key.DeleteValue(ValueName, throwOnMissingValue: false);
    }
}
