namespace NetBear.Tests;

public class SpritesTests
{
    public static TheoryData<BearState> States => [BearState.Hibernating, BearState.Idle, BearState.Running];

    [Theory]
    [MemberData(nameof(States))]
    public void EveryFrameIs16x16(BearState state)
    {
        foreach (var frame in Sprites.For(state))
        {
            Assert.Equal(Sprites.Size, frame.Length);
            Assert.All(frame, row => Assert.Equal(Sprites.Size, row.Length));
        }
    }

    [Theory]
    [MemberData(nameof(States))]
    public void EveryFrameUsesOnlyPaletteCharacters(BearState state)
    {
        foreach (bool light in new[] { false, true })
        {
            var palette = Sprites.Palette(light);
            foreach (var frame in Sprites.For(state))
                foreach (char c in string.Concat(frame))
                    Assert.True(c == '.' || palette.ContainsKey(c), $"'{c}' no está en la paleta (light={light})");
        }
    }

    [Fact]
    public void FrameCountsMatchSpec()
    {
        Assert.Equal(2, Sprites.For(BearState.Hibernating).Length);
        Assert.Equal(12, Sprites.For(BearState.Idle).Length);
        Assert.Equal(5, Sprites.For(BearState.Running).Length);
    }

    [Fact]
    public void LastIdleFrameBlinksDifferentlyFromFirst()
    {
        var idle = Sprites.For(BearState.Idle);
        Assert.NotEqual(string.Concat(idle[0]), string.Concat(idle[11]));
    }

    [Fact]
    public void LastIdleFrameHasNoEyePixel() =>
        Assert.DoesNotContain('E', string.Concat(Sprites.For(BearState.Idle)[11]));

    [Fact]
    public void PalettesDefineTheSameCharacters() =>
        Assert.Equal(Sprites.Palette(false).Keys.Order(), Sprites.Palette(true).Keys.Order());
}
