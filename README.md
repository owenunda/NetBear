# NetBear 🐻

Un oso en pixel art que vive en la bandeja de Windows y corre según tu velocidad de internet.

- **Corre** cuando hay tráfico: mientras más rápido descargas, más rápido corre.
- **Se queda quieto** (y parpadea) cuando casi no hay tráfico.
- **Hiberna** 💤 cuando no hay conexión.
- Pasa el mouse por encima para ver `↓ descarga  ↑ subida`.

Súper liviano: un solo `.exe` de unos MB, sin dependencias ni runtime que instalar.

## Instalar

Descarga la última versión desde [Releases](https://github.com/owenunda/NetBear/releases):

- **`NetBearSetup.exe`**: instalador (sin permisos de administrador). Agrega NetBear al menú Inicio y, si quieres, lo inicia con Windows.
- **`NetBear-portable.zip`**: solo el `.exe`. Descomprímelo y ábrelo. Desde el menú del clic derecho puedes activar "Iniciar con Windows".

Requiere Windows 10/11 de 64 bits.

Como la app todavía no está firmada, Windows SmartScreen puede mostrar "Windows protegió tu PC": elige "Más información → Ejecutar de todas formas".

## Limitaciones

- Con adaptadores como vEthernet de Hyper-V/WSL o VPN que Windows reporta como conectados, el oso puede no hibernar.
- "Sin conexión" solo significa que no hay ninguna red conectada, no que el router tenga internet.

## Desarrollo

Necesitas el [SDK de .NET 10](https://dotnet.microsoft.com/download).

```powershell
dotnet run --project src/NetBear   # abre el oso en la bandeja
dotnet test                        # corre los tests
```

Para compilar el `.exe` Native AOT localmente necesitas además las *C++ Build Tools* de Visual Studio:
`dotnet publish src/NetBear -c Release -r win-x64`. Si no las tienes, GitHub Actions lo compila en cada push.

## Dibujar al oso

Los cuadros de la animación están como texto en [`src/NetBear/Sprites.cs`](src/NetBear/Sprites.cs): cada carácter es un pixel
(`.` es transparente; `O`, `B`, `L`, `E`, `N` y `Z` son colores de la paleta). Edita los cuadros, corre `dotnet test` para
comprobar que siguen midiendo 16×16 y ábrelo con `dotnet run`. ¡Los PR con mascotas nuevas son bienvenidos!

## Publicar una versión

```powershell
git tag v1.0.0
git push origin v1.0.0
```

GitHub Actions corre los tests y genera el instalador y el zip portable en una Release nueva.

## Licencia

[MIT](LICENSE)
