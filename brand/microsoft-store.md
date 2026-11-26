# NetBear en Microsoft Store

Textos listos para copiar en Partner Center. Hay versión en español y en inglés: agrega los dos idiomas en "Store listings".

## URLs

| Campo | Valor |
|---|---|
| Privacy policy URL | https://owenunda.github.io/NetBear/privacidad/ (EN: https://owenunda.github.io/NetBear/en/privacy/) |
| Support contact info | https://owenunda.github.io/NetBear/soporte/ (EN: https://owenunda.github.io/NetBear/en/support/) |
| Website | https://owenunda.github.io/NetBear/ |

Las URLs funcionan cuando el workflow `pages` publique `site/` (ver "Antes de enviar").

## Propiedades

- **Categoría:** Utilities & tools. Subcategoría, si la pide: Network tools
- **¿Accede, recopila o transmite información personal?** No. Igual pon la URL de privacidad, porque la Store la exige a casi todas las apps
- **Clasificación por edad (cuestionario IARC):** sin interacción entre usuarios, sin compras, sin ubicación, sin contenido generado por usuarios. Debería salir 3+ / Everyone
- **Precio:** gratis
- **Requisitos del sistema:** Windows 10 versión 1809 o posterior, x64

---

## Español

**Nombre:** NetBear

**Descripción corta** (hasta 100 caracteres, se muestra arriba en Windows 11):
> Un oso en pixel art que vive en la bandeja de Windows y corre según tu velocidad de internet.

**Descripción:**
> NetBear es un oso en pixel art que vive junto al reloj de Windows y te muestra cómo va tu internet sin abrir nada.
>
> • Corre cuando hay tráfico. Mientras más rápido descargas, más rápido corre.
> • Se queda quieto, y de vez en cuando parpadea, cuando casi no hay tráfico.
> • Hiberna cuando no hay conexión.
> • Pasa el mouse por encima para ver la velocidad de descarga y de subida.
>
> No tiene ventanas, cuentas ni anuncios. Es un solo ícono con un menú de tres opciones: iniciar con Windows, ver el proyecto en GitHub y salir. Se adapta solo a la barra de tareas clara u oscura.
>
> NetBear no envía datos a ningún servidor. Solo lee los contadores de tráfico de tus adaptadores de red, en tu equipo y en memoria. Es gratis y de código abierto (licencia MIT).

**Características del producto** (una por línea, hasta 20):
- Oso animado en la bandeja del sistema
- La velocidad de la animación sigue tu velocidad de internet
- Descarga y subida al pasar el mouse
- Hiberna cuando no hay conexión
- Ignora adaptadores virtuales como Hyper-V, WSL y VPN
- Se adapta al tema claro u oscuro de la barra de tareas
- Opción para iniciar con Windows
- No recopila ni envía datos
- Gratis y de código abierto

**Palabras clave** (hasta 7): velocidad de internet, monitor de red, bandeja del sistema, pixel art, ancho de banda, descarga, mascota

**Novedades en esta versión:** Primera versión en Microsoft Store.

---

## English

**Name:** NetBear

**Short description:**
> A pixel-art bear that lives in the Windows system tray and runs at the speed of your internet.

**Description:**
> NetBear is a pixel-art bear that lives next to the Windows clock and shows how your internet is doing without opening anything.
>
> • It runs when there's traffic. The faster you download, the faster it runs.
> • It stands still, blinking now and then, when there's barely any traffic.
> • It hibernates when you're offline.
> • Hover over it to see download and upload speed.
>
> No windows, no accounts, no ads. Just one icon with a three-option menu: start with Windows, view the project on GitHub, and exit. It adapts to a light or dark taskbar on its own.
>
> NetBear sends no data to any server. It only reads your network adapters' traffic counters, on your device and in memory. Free and open source (MIT license).
>
> The app's menu and tooltip are in Spanish.

**Product features:**
- Animated bear in the system tray
- Animation speed follows your internet speed
- Download and upload speed on hover
- Hibernates when you're offline
- Ignores virtual adapters such as Hyper-V, WSL and VPNs
- Matches a light or dark taskbar
- Optional start with Windows
- Collects and sends no data
- Free and open source

**Search terms:** internet speed, network monitor, system tray, pixel art, bandwidth, download speed, desktop pet

**What's new in this version:** First release on the Microsoft Store.

---

## Imágenes que pide la Store

- **Capturas de pantalla:** al menos 1, mínimo 1366×768. Buenas ideas: la barra de tareas con el oso corriendo y el tooltip, el menú del clic derecho, el oso hibernando. La simulación de la web también sirve de base
- **Logo de la Store (box art 1:1):** ya está en `brand/logo/store/` (`netbear-store-1080.png`, `netbear-store-2160.png` y `netbear-300.png` con fondo transparente)

## Antes de enviar

1. **Publica la web.** En GitHub ve a Settings › Pages y en "Source" elige **GitHub Actions**. Después haz push a `main` y el workflow `pages` la publica.
2. **Elige cómo empaquetar.** Esta es la decisión importante:
   - **MSIX (recomendado):** la Store firma el paquete gratis, así que no hace falta comprar un certificado y desaparece el aviso de SmartScreen. Hay que crear un paquete MSIX con el `.exe` (con el *MSIX Packaging Tool* de Microsoft o un proyecto de empaquetado). Ojo: dentro de MSIX la clave `HKCU\...\Run` no funciona, y el autoarranque se hace con una `StartupTask` en el manifiesto.
   - **EXE (el `NetBearSetup.exe` de Inno Setup):** la Store exige que el instalador esté **firmado con un certificado de una CA reconocida** y alojado en una URL HTTPS fija por versión (sirve la de GitHub Releases). También pide un parámetro de instalación silenciosa, que en Inno es `/VERYSILENT /SUPPRESSMSGBOXES /NORESTART`. Como hoy el instalador no está firmado, este camino necesita comprar un certificado.
