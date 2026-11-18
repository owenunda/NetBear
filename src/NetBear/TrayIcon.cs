using System.Runtime.InteropServices;
using static NetBear.Native;

namespace NetBear;

internal sealed unsafe class TrayIcon : IDisposable
{
    const string GitHubUrl = "https://github.com/owenunda/NetBear";
    const uint TrayCallback = WM_APP + 1;
    const nuint SampleTimerId = 1, FrameTimerId = 2;
    const nuint CmdAutostart = 1, CmdGitHub = 2, CmdExit = 3;

    // El WndProc es estático (requisito de UnmanagedCallersOnly); solo hay una instancia por proceso.
    static TrayIcon? s_instance;
    static readonly uint s_taskbarCreated = RegisterWindowMessageW("TaskbarCreated");

    readonly nint _hwnd;
    readonly NetSpeedMonitor _monitor = new();
    readonly Dictionary<string[], nint> _icons = new(ReferenceEqualityComparer.Instance);
    readonly int _iconSize = GetSystemMetrics(SM_CXSMICON);
    bool _lightTaskbar = Theme.IsTaskbarLight();
    BearState _state;
    int _frame, _frameMs;
    string _speedText = "";

    public TrayIcon()
    {
        s_instance = this;
        nint instance = GetModuleHandleW(null);
        fixed (char* name = "NetBear")
        {
            var wc = new WNDCLASSEXW
            {
                cbSize = (uint)sizeof(WNDCLASSEXW),
                lpfnWndProc = &WndProc,
                hInstance = instance,
                lpszClassName = name,
            };
            RegisterClassExW(&wc);
            // Ventana normal que nunca se muestra: las de solo mensajes no reciben TaskbarCreated ni WM_SETTINGCHANGE.
            _hwnd = CreateWindowExW(0, name, name, 0, 0, 0, 0, 0, 0, 0, instance, 0);
        }

        Refresh();
        Notify(NIM_ADD);
        SetTimer(_hwnd, SampleTimerId, 1000, 0);
    }

    public void Run()
    {
        MSG msg;
        while (GetMessageW(&msg, 0, 0, 0) > 0)
        {
            TranslateMessage(&msg);
            DispatchMessageW(&msg);
        }
    }

    public void Dispose()
    {
        foreach (nint icon in _icons.Values) DestroyIcon(icon);
        _icons.Clear();
    }

    void Refresh()
    {
        var speed = _monitor.Update();
        bool connected = NetSpeedMonitor.IsConnected();
        var state = BearAnimator.GetState(connected, speed.Total);
        if (state != _state) _frame = 0;
        _state = state;
        _speedText = connected
            ? $"↓ {SpeedFormatter.Format(speed.DownBps)}  ↑ {SpeedFormatter.Format(speed.UpBps)}"
            : "Sin conexión";

        int ms = BearAnimator.GetFrameIntervalMs(_state, speed.Total);
        if (ms != _frameMs)
        {
            _frameMs = ms;
            SetTimer(_hwnd, FrameTimerId, (uint)ms, 0); // reemplaza el timer anterior con el mismo id
        }
    }

    void Notify(uint message)
    {
        var frames = Sprites.For(_state);
        var frame = frames[_frame % frames.Length];
        if (!_icons.TryGetValue(frame, out nint icon))
        {
            icon = IconRenderer.Create(frame, Sprites.Palette(_lightTaskbar), _iconSize);
            if (icon == 0) return; // no se cachea: se conserva el icono anterior
            _icons[frame] = icon;
        }

        var data = new NOTIFYICONDATAW
        {
            cbSize = (uint)sizeof(NOTIFYICONDATAW),
            hWnd = _hwnd,
            uID = 1,
            uFlags = NIF_MESSAGE | NIF_ICON | NIF_TIP,
            uCallbackMessage = TrayCallback,
            hIcon = icon,
        };
        string tip = "NetBear\n" + _speedText;
        tip.AsSpan(0, Math.Min(tip.Length, 127)).CopyTo(new Span<char>(data.szTip, 128));
        // Si el NIM_ADD inicial falla (shell aún no listo al iniciar sesión), el siguiente tick de 1 s lo reintenta.
        if (!Shell_NotifyIconW(message, &data) && message == NIM_MODIFY) Shell_NotifyIconW(NIM_ADD, &data);
    }

    void RemoveIcon()
    {
        var data = new NOTIFYICONDATAW { cbSize = (uint)sizeof(NOTIFYICONDATAW), hWnd = _hwnd, uID = 1 };
        Shell_NotifyIconW(NIM_DELETE, &data);
    }

    void ReloadTheme()
    {
        bool light = Theme.IsTaskbarLight();
        if (light == _lightTaskbar) return;
        _lightTaskbar = light;
        Dispose(); // los iconos cacheados usan la paleta anterior
        Notify(NIM_MODIFY);
    }

    void ShowMenu()
    {
        nint menu = CreatePopupMenu();
        AppendMenuW(menu, MF_STRING | MF_GRAYED, 0, _speedText);
        AppendMenuW(menu, MF_SEPARATOR, 0, null);
        AppendMenuW(menu, MF_STRING | (Autostart.IsEnabled() ? MF_CHECKED : 0), CmdAutostart, "Iniciar con Windows");
        AppendMenuW(menu, MF_STRING, CmdGitHub, "Ver en GitHub");
        AppendMenuW(menu, MF_STRING, CmdExit, "Salir");

        GetCursorPos(out POINT point);
        SetForegroundWindow(_hwnd); // sin esto el menú no se cierra al hacer clic afuera
        nuint command = (nuint)TrackPopupMenu(menu, TPM_RETURNCMD | TPM_RIGHTBUTTON | TPM_NONOTIFY, point.X, point.Y, 0, _hwnd, 0);
        PostMessageW(_hwnd, WM_NULL, 0, 0);
        DestroyMenu(menu);

        switch (command)
        {
            case CmdAutostart:
                try { Autostart.Set(!Autostart.IsEnabled()); }
                catch (Exception) { } // el estado queda como estaba
                break;
            case CmdGitHub: ShellExecuteW(0, "open", GitHubUrl, null, null, SW_SHOWNORMAL); break;
            case CmdExit: DestroyWindow(_hwnd); break;
        }
    }

    [UnmanagedCallersOnly]
    static nint WndProc(nint hwnd, uint msg, nint wParam, nint lParam)
    {
        // Durante CreateWindowExW todavía no hay _hwnd asignado: esos mensajes van al manejo por defecto.
        if (s_instance is { } self && self._hwnd == hwnd)
        {
            // Una excepción que escapa de un UnmanagedCallersOnly mata el proceso: se traga.
            try
            {
                switch (msg)
                {
                    case WM_TIMER when (nuint)wParam == SampleTimerId:
                        self.Refresh();
                        self.Notify(NIM_MODIFY);
                        return 0;
                    case WM_TIMER when (nuint)wParam == FrameTimerId:
                        self._frame = (self._frame + 1) % 60; // 60 = mcm de 2, 12 y 5 cuadros
                        self.Notify(NIM_MODIFY);
                        return 0;
                    case TrayCallback when (uint)(lParam & 0xFFFF) == WM_RBUTTONUP:
                        self.ShowMenu();
                        return 0;
                    case WM_SETTINGCHANGE when lParam != 0 && new string((char*)lParam) == "ImmersiveColorSet":
                        self.ReloadTheme();
                        return 0;
                    case WM_ENDSESSION when wParam != 0: // Restart Manager (instalador) pide cerrar
                        DestroyWindow(hwnd);
                        return 0;
                    case WM_DESTROY:
                        self.RemoveIcon();
                        PostQuitMessage(0);
                        return 0;
                }
                if (msg == s_taskbarCreated) // el Explorador se reinició: hay que volver a agregar el icono
                {
                    self.Notify(NIM_ADD);
                    return 0;
                }
            }
            catch (Exception)
            {
                return 0;
            }
        }
        return DefWindowProcW(hwnd, msg, wParam, lParam);
    }
}
