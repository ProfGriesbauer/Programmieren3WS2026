namespace Maxi_HouseExample;

public class Haus
{
    private readonly double _stromNormal;
    private readonly double _wasserNormal;
    private readonly double _heizNormal;
    private double _strom;
    private double _wasser;
    private double _heizleistung;
    private bool _heizungAn = true;
    private bool _hauptstromAn = true;
    private bool _wasserAn = true;

    public Haus(double strom, double wasser, double heizleistung)
    {
        _stromNormal = strom;
        _wasserNormal = wasser;
        _heizNormal = heizleistung;
        _strom = strom;
        _wasser = wasser;
        _heizleistung = heizleistung;

        Bad = new Bad(() => WasserAn);
        Wohnzimmer = new Wohnzimmer(() => HeizungAn);
        Sauna = new Sauna(() => HeizungAn);
        WohnzimmerFenster = new Fenster();
        Haustuer = new Tuer();
        Garagentor = new Garagentor();
        Jalousie = new Jalousie();
        GartenWasseranschluss = new Wasseranschluss(() => WasserAn);
        Klimaanlage = new Klimaanlage(() => HauptstromschalterAn);
    }

    public double StromverbrauchAmpere => _strom;
    public double WasserverbrauchKubikmeterProStunde => _wasser;
    public double HeizungsleistungKilowatt => _heizleistung;
    public Bad Bad { get; }
    public Wohnzimmer Wohnzimmer { get; }
    public Sauna Sauna { get; }
    public Fenster WohnzimmerFenster { get; }
    public Tuer Haustuer { get; }
    public Garagentor Garagentor { get; }
    public Jalousie Jalousie { get; }
    public Wasseranschluss GartenWasseranschluss { get; }
    public Klimaanlage Klimaanlage { get; }

    public bool WasserAn
    {
        get => _wasserAn;
        set
        {
            _wasserAn = value;
            _wasser = value ? _wasserNormal : 0;

            if (!value)
            {
                Bad.Ausschalten();
                GartenWasseranschluss.Ausschalten();
            }
        }
    }

    public bool HeizungAn
    {
        get => _heizungAn;
        set
        {
            if (value && !_hauptstromAn)
                throw new InvalidOperationException("Ohne Strom kann die Heizung nicht eingeschaltet werden.");

            _heizungAn = value;
            _heizleistung = value ? _heizNormal : 0;

            if (!value)
            {
                Wohnzimmer.Ausschalten();
                Sauna.Ausschalten();
            }
        }
    }

    public bool HauptstromschalterAn
    {
        get => _hauptstromAn;
        set
        {
            _hauptstromAn = value;
            _strom = value ? _stromNormal : 0;

            if (!value)
            {
                HeizungAn = false;
                Klimaanlage.Ausschalten();
            }
        }
    }

    public double BerechneWasserverbrauch(TimeSpan zeitraum)
    {
        if (zeitraum < TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(zeitraum), "Der Zeitraum darf nicht negativ sein.");

        return _wasser * zeitraum.TotalHours;
    }
}
