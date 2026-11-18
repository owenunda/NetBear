namespace NetBear.Tests;

public class IconRendererTests
{
    [Theory]
    [InlineData(16)]
    [InlineData(20)]
    [InlineData(24)]
    public void Create_ReturnsDestroyableIcon(int size)
    {
        var frame = Sprites.For(BearState.Running)[0];
        nint icon = IconRenderer.Create(frame, Sprites.Palette(false), size);
        Assert.NotEqual(0, icon);
        Assert.True(Native.DestroyIcon(icon));
    }
}
