public class Haus
{
    private readonly double stromverbrauch;
    private readonly double wasserverbrauch;
    private readonly double heizungsleistung;
    private readonly List<IRaum> räume = [];
    private bool heizungAn;
    private bool hauptstromschalterAn;

    public Haus(double stromverbrauch, double wasserverbrauch, double heizungsleistung)
    {
        if (stromverbrauch < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stromverbrauch));
        }

        if (wasserverbrauch < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(wasserverbrauch));
        }

        if (heizungsleistung < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(heizungsleistung));
        }

        this.stromverbrauch = stromverbrauch;
        this.wasserverbrauch = wasserverbrauch;
        this.heizungsleistung = heizungsleistung;

        for (var i = 0; i < 4; i++)
        {
            räume.Add(new Raum());
        }
    }

    public IReadOnlyList<IRaum> Räume => räume;

    public double Stromverbrauch => hauptstromschalterAn ? stromverbrauch : 0;

    public double Wasserverbrauch => wasserverbrauch;

    public double Heizungsleistung => heizungAn ? heizungsleistung : 0;

    public bool HeizungAn
    {
        get => heizungAn;
        set => heizungAn = value && hauptstromschalterAn;
    }

    public bool HauptstromschalterAn
    {
        get => hauptstromschalterAn;
        set
        {
            hauptstromschalterAn = value;

            if (!hauptstromschalterAn)
            {
                heizungAn = false;
            }
        }
    }

    public double BerechneWasserverbrauch(TimeSpan zeitraum)
    {
        if (zeitraum < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(zeitraum));
        }

        return wasserverbrauch * zeitraum.TotalHours;
    }
}
