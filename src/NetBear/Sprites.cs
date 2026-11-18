namespace NetBear;

// El oso dibujado a mano, 16×16, mirando a la derecha.
// .  transparente   O  contorno   B  pelaje   L  hocico
// E  ojo            N  nariz      Z  zzz
internal static class Sprites
{
    public const int Size = 16;

    const string Empty = "................";

    static readonly string[] Body =
    [
        Empty,
        Empty,
        Empty,
        "..........OO.OO.",
        ".........OBBOBBO",
        "..OOOOOOOBBBBBBO",
        ".OBBBBBBBBBBBEBO",
        "OBBBBBBBBBBBBLNO",
        "OBBBBBBBBBBBLLLO",
        "OBBBBBBBBBBBOOO.",
        ".OBBBBBBBBBBO...",
        ".OBBOOOOOOBBO...",
    ];

    // Parpadeo: el ojo (fila 6) desaparece y queda solo pelaje.
    static readonly string[] BlinkingBody = [.. Body[..6], ".OBBBBBBBBBBBBBO", .. Body[7..]];

    static string[] WithLegs(string[] body, string upper, string lower) => [.. body, upper, lower, Empty, Empty];

    static readonly string[][] Running =
    [
        WithLegs(Body, "OBO........OBO..", "OO..........OO.."), // estirado
        WithLegs(Body, "OBO......OBO....", "OO.......OO....."), // empuja
        WithLegs(Body, "...OBO..OBO.....", "....OO..OO......"), // cruzado
        WithLegs(Body, "..OBO....OBO....", "..OO.....OO....."), // recogido
        WithLegs(Body, "..OBO......OBO..", "..OO........OO.."), // alcanza
    ];

    static readonly string[] Standing = WithLegs(Body, "..OBO.....OBO...", "..OOO.....OOO...");
    static readonly string[] Blinking = WithLegs(BlinkingBody, "..OBO.....OBO...", "..OOO.....OOO...");

    static readonly string[][] Idle =
        [Standing, Standing, Standing, Standing, Standing, Standing, Standing, Standing, Standing, Standing, Standing, Blinking];

    static readonly string[] Sleeping =
    [
        Empty,
        "..........OO.OO.",
        "...OOOOOOOBBBBBO",
        "..OBBBBBBBBBBOBO",
        ".OBBBBBBBBBBBLNO",
        ".OBBBBBBBBBBBLLO",
        ".OOOOOOOOOOOOOO.",
        Empty,
        Empty,
        Empty,
    ];

    static readonly string[][] Hibernating =
    [
        [Empty, Empty, Empty, Empty, Empty, "............Z...", .. Sleeping],
        [Empty, "...........ZZZ..", "............Z...", "...........ZZZ..", Empty, Empty, .. Sleeping],
    ];

    // Barra oscura: contorno claro para que el oso se distinga.
    static readonly Dictionary<char, uint> DarkTaskbar = new()
    {
        ['O'] = 0xFFF2E6D8, ['B'] = 0xFFA0673A, ['L'] = 0xFFE8C9A0,
        ['E'] = 0xFF1A1A1A, ['N'] = 0xFF1A1A1A, ['Z'] = 0xFFFFFFFF,
    };

    // Barra clara: contorno oscuro.
    static readonly Dictionary<char, uint> LightTaskbar = new()
    {
        ['O'] = 0xFF2B1A0E, ['B'] = 0xFF8B5A2B, ['L'] = 0xFFD9B48A,
        ['E'] = 0xFF000000, ['N'] = 0xFF000000, ['Z'] = 0xFF404040,
    };

    public static string[][] For(BearState state) => state switch
    {
        BearState.Hibernating => Hibernating,
        BearState.Idle => Idle,
        _ => Running,
    };

    public static IReadOnlyDictionary<char, uint> Palette(bool lightTaskbar) => lightTaskbar ? LightTaskbar : DarkTaskbar;
}
