namespace PatrickHouseExample;

public class Licht
{
    private bool _istAn;
    private int _helligkeit;

    public bool IstAn => _istAn;
    public int Helligkeit
    {
        get => _helligkeit;
        set => _helligkeit = Math.Clamp(value, 0, 100);
    }

    public decimal VerbrauchProStunde { get; set; }

    public void Einschalten()
    {
        _istAn = true;
        if (_helligkeit == 0)
            _helligkeit = 50;
    }

    public void Ausschalten()
    {
        _istAn = false;
        _helligkeit = 0;
    }

    public void SetzeHelligkeit(int helligkeit)
    {
        if (helligkeit == 0)
        {
            Ausschalten();
            return;
        }

        Einschalten();
        Helligkeit = helligkeit;
    }

    public override string ToString()
    {
        return $"{(IstAn ? "An" : "Aus")} ({Helligkeit}%)";
    }
}
