public class Bad : Raum, IWasserRaum
{
    private const double WasserM3proH = 0.3;

    private bool wasserAn;

    public bool WasserAn
    {
        get
        {
            return wasserAn;
        }
        set
        {
            wasserAn = value;
        }
    }

    public double Wasserverbrauch
    {
        get
        {
            if (wasserAn)
            {
                return WasserM3proH;
            }
            return 0;
        }
    }
}
