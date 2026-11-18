using static NetBear.Native;

namespace NetBear;

internal static unsafe class IconRenderer
{
    // Escala por vecino más cercano: el pixel art queda nítido a cualquier tamaño de icono.
    public static nint Create(string[] frame, IReadOnlyDictionary<char, uint> palette, int size)
    {
        var header = new BITMAPINFOHEADER
        {
            biSize = (uint)sizeof(BITMAPINFOHEADER),
            biWidth = size,
            biHeight = -size, // negativo = filas de arriba hacia abajo
            biPlanes = 1,
            biBitCount = 32,
        };

        void* bits;
        nint screen = GetDC(0);
        nint color = CreateDIBSection(screen, &header, 0, &bits, 0, 0);
        ReleaseDC(0, screen);
        if (color == 0 || bits == null) return 0;

        var pixels = new Span<uint>(bits, size * size);
        for (int y = 0; y < size; y++)
        {
            string row = frame[y * Sprites.Size / size];
            for (int x = 0; x < size; x++)
            {
                char c = row[x * Sprites.Size / size];
                pixels[y * size + x] = c == '.' ? 0u : palette[c];
            }
        }

        // La máscara es obligatoria pero, con color de 32 bits con alfa, Windows usa el alfa.
        var maskBits = new byte[(size + 15) / 16 * 2 * size];
        nint mask;
        fixed (byte* m = maskBits) mask = CreateBitmap(size, size, 1, 1, m);

        var info = new ICONINFO { fIcon = 1, hbmMask = mask, hbmColor = color };
        nint icon = mask == 0 ? 0 : CreateIconIndirect(&info);
        DeleteObject(color);
        if (mask != 0) DeleteObject(mask);
        return icon; // 0 si falló
    }
}
