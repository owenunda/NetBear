# NetBear, Diseño v1

Microapp open source para Windows 11: un oso en pixel art en la bandeja del sistema
que corre más rápido cuanto mayor es el tráfico de internet (al estilo RunCat).

## Objetivos

- Mostrar el tráfico de red en tiempo real (descarga y subida) sin consumir datos.
- Ser súper liviana: un solo `.exe` sin dependencias, ~2–3 MB, ~5–10 MB de RAM.
- Ser fácil de instalar: instalador de un clic y versión portable.

## Fuera de alcance (v1)

- Test de velocidad (estilo speedtest).
- Ventana flotante con números siempre visibles.
- Detectar "conectado al WiFi pero sin internet" (sin ping ni sondeos).
- Ventana de configuración y archivo de ajustes.
- Mascotas o skins adicionales y publicación en winget.

## Stack

- C# en .NET 10, con Win32 directo vía P/Invoke (sin WinForms ni WPF).
- Publicación con Native AOT: un único `.exe` autocontenido, x64.
- Desarrollo local con `dotnet run` (JIT). El `.exe` AOT se compila en GitHub Actions,
  porque el AOT local requiere las C++ Build Tools de Visual Studio, que son opcionales.

## Comportamiento

### Medición

- Cada 1 s se suman los bytes recibidos y enviados de las interfaces activas
  (`OperationalStatus.Up`) que tienen puerta de enlace, excluyendo loopback y túneles. Los adaptadores
  virtuales sin gateway (Hyper-V, WSL, Tailscale) no cuentan: su tráfico ya pasa por el adaptador físico.
- La velocidad es la diferencia con la muestra anterior, dividida por el tiempo transcurrido real.
- Si un contador baja (por reconexión del adaptador), la diferencia de ese adaptador cuenta como 0.
- Si leer un adaptador lanza una excepción, ese adaptador se ignora en esa muestra.

### Estados del oso

| Condición                                 | Estado        | Animación                                 |
|-------------------------------------------|---------------|-------------------------------------------|
| Ningún adaptador activo con puerta de enlace | Hibernando | 2 cuadros con zZz, 500 ms por cuadro      |
| Descarga + subida < 10 KB/s               | Quieto (de pie) | Cuadros de 250 ms; parpadea 1 de cada 12 (~cada 3 s) |
| Descarga + subida ≥ 10 KB/s               | Corriendo     | 5 cuadros, intervalo según la velocidad   |

Intervalo de carrera según el total `t` en bytes/s, con escala logarítmica:

- `t = 10 KB/s` → 200 ms por cuadro
- `t ≥ 10 MB/s` → 40 ms por cuadro
- Interpolación lineal sobre `log10(t)` entre ambos extremos, con límites en esos valores.

### Tooltip y menú

- Tooltip: `NetBear\n↓ 2.4 MB/s  ↑ 120 KB/s` (o `NetBear\nSin conexión` mientras hiberna).
- Formato: B/s, KB/s, MB/s y GB/s en base 1024; un decimal desde KB/s en adelante.
- Menú contextual (clic derecho):
  - Velocidad actual (deshabilitado, solo informativo)
  - Iniciar con Windows (check; valor en `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`, nombre `NetBear`)
  - Ver en GitHub
  - Salir

### Tema

- Se lee `HKCU\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize\SystemUsesLightTheme`.
- Paleta oscura (oso claro) si vale 0; paleta clara (oso oscuro) si vale 1.
- Se vuelve a leer al recibir `WM_SETTINGCHANGE` con `"ImmersiveColorSet"`.

### Sprites

- Cada cuadro es de 16×16, definido como 16 strings de 16 caracteres en `Sprites.cs`.
- `.` = transparente; el resto de caracteres son índices de la paleta activa.
- El render convierte el cuadro a un HICON de 32 bpp con alfa (`CreateIconIndirect`),
  escalado por vecino más cercano al tamaño `SM_CXSMICON` del sistema.
- Los íconos de cada estado y paleta se generan una vez y se cachean;
  al cambiar el tema se regeneran.

### Robustez

- Instancia única mediante un mutex con nombre. Una segunda instancia se cierra sin hacer nada.
- El icono se vuelve a registrar al recibir el mensaje `TaskbarCreated` (por ejemplo, cuando se reinicia el Explorador).
- Al salir, el icono se elimina (`NIM_DELETE`) y se liberan los HICON.

## Estructura

```
NetBear/
  src/NetBear/
    Program.cs          entrada y mutex de instancia única
    TrayIcon.cs         ventana oculta, bucle de mensajes, timers, Shell_NotifyIcon, menú, TaskbarCreated
    NetSpeedMonitor.cs  muestreo de bytes y cálculo de velocidades
    BearAnimator.cs     estado + intervalo de cuadro (lógica pura)
    SpeedFormatter.cs   formateo de velocidades (lógica pura)
    Sprites.cs          cuadros y paletas
    IconRenderer.cs     sprite → HICON
    Autostart.cs        clave Run en HKCU
    Theme.cs            detección de tema claro u oscuro
    Native.cs           declaraciones P/Invoke
  tests/NetBear.Tests/  xUnit
  installer/NetBear.iss Inno Setup
  .github/workflows/build.yml
  README.md, LICENSE (MIT)
```

## Distribución

- Workflow `build.yml` en `windows-latest`. En cada push o PR a `main`: tests + `dotnet publish -c Release -r win-x64` (AOT),
  para detectar pronto problemas de AOT que no se pueden compilar localmente. En tags `v*`, además genera
  `NetBear-portable.zip`, compila `NetBearSetup.exe` con Inno Setup y crea la GitHub Release.
- Repositorio público asumido: `https://github.com/owenunda/NetBear` (constante única en `TrayIcon.cs` y en el `.iss`).
- El instalador: instala por usuario (sin admin) en `{localappdata}\Programs\NetBear`, crea
  un acceso directo en el menú Inicio, ofrece la tarea "Iniciar con Windows" (marcada por defecto,
  escribe la misma clave Run que usa la app) y aparece en Agregar o quitar programas.
  Al desinstalar, borra la clave Run.

## Pruebas

xUnit, solo sobre lógica pura:
- `SpeedFormatter`: límites entre unidades y decimales.
- Cálculo de deltas: diferencia normal, contador reiniciado y tiempo transcurrido distinto de 1 s.
- `BearAnimator`: umbrales de estado y escala logarítmica (extremos, punto medio y límites).
- `Sprites`: cada cuadro mide 16×16 y solo usa caracteres definidos en la paleta.

La bandeja, el tema y el autostart se prueban a mano en Windows 11.
