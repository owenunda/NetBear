using NetBear;

using var mutex = new Mutex(true, @"Local\NetBear", out bool createdNew);
if (!createdNew) return;

using var tray = new TrayIcon();
tray.Run();
