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

    protected override void LadeDaten(System.Text.Json.Nodes.JsonObject json)
    {
        base.LadeDaten(json);
        wasserAn = (bool?)json["WasserAn"] ?? false;
    }

    protected override Dictionary<string, object> SerialisierungsDaten()
    {
        var daten = base.SerialisierungsDaten();
        daten["WasserAn"] = wasserAn;
        daten["Wasserverbrauch"] = Wasserverbrauch;
        return daten;
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
