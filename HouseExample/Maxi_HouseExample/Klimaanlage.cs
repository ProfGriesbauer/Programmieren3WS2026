namespace Maxi_HouseExample;

public class Klimaanlage
{
    private readonly Func<bool> _stromVerfuegbar;

    internal Klimaanlage(Func<bool> stromVerfuegbar)
    {
        _stromVerfuegbar = stromVerfuegbar;
    }

    public bool An { get; private set; }

    public void Einschalten()
    {
        if (!_stromVerfuegbar())
            throw new InvalidOperationException("Die Klimaanlage kann ohne Strom nicht eingeschaltet werden.");

        An = true;
    }

    public void Ausschalten()
    {
        An = false;
    }
}
