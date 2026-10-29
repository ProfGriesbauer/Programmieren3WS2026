namespace Maxi_HouseExample;

public class Wasseranschluss
{
    private readonly Func<bool> _wasserVerfuegbar;

    internal Wasseranschluss(Func<bool> wasserVerfuegbar)
    {
        _wasserVerfuegbar = wasserVerfuegbar;
    }

    public bool An { get; private set; }

    public void Einschalten()
    {
        if (!_wasserVerfuegbar())
            throw new InvalidOperationException("Der Gartenwasseranschluss kann ohne Wasser nicht eingeschaltet werden.");

        An = true;
    }

    public void Ausschalten()
    {
        An = false;
    }
}
