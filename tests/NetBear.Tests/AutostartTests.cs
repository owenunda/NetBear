namespace NetBear.Tests;

public class AutostartTests
{
    // Los tests corren fuera de un paquete MSIX: la detección debe decir que no está empaquetado.
    [Fact]
    public void FueraDeMsixNoEstaEmpaquetado() => Assert.False(Autostart.IsPackaged);
}
