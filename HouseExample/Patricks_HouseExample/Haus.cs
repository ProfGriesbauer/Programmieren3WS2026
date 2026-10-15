namespace PatrickHouseExample;

public class Haus
{
    private readonly List<Raum> _räume;
    private readonly decimal _wasserverbrauchProStunde;
    private decimal _stromverbrauch;

    public bool HauptstromAn { get; set; }
    public decimal Stromverbrauch => HauptstromAn ? _stromverbrauch : 0m;
    public decimal Wasserverbrauch => HauptstromAn ? _wasserverbrauchProStunde : 0m;
    public IReadOnlyList<Raum> Räume => _räume;

    public Haus(decimal wasserverbrauchProStunde)
    {
        if (wasserverbrauchProStunde < 0m)
            throw new ArgumentOutOfRangeException(nameof(wasserverbrauchProStunde));

        _wasserverbrauchProStunde = wasserverbrauchProStunde;
        _räume = new List<Raum>();
        _stromverbrauch = 0m;
    }

    public void RaumHinzufügen(Raum raum)
    {
        ArgumentNullException.ThrowIfNull(raum);
        _räume.Add(raum);
    }

    public void AlleLichterAn()
    {
        foreach (var raum in _räume)
        {
            raum.Licht.Einschalten();
        }
    }

    public void AlleLichterAus()
    {
        foreach (var raum in _räume)
        {
            raum.Licht.Ausschalten();
        }
    }

    public void ToggleLicht(string raumName)
    {
        var raum = _räume.FirstOrDefault(r => r.Name.Equals(raumName, StringComparison.OrdinalIgnoreCase));
        if (raum is null)
            throw new InvalidOperationException($"Raum '{raumName}' wurde nicht gefunden.");

        if (raum.Licht.IstAn)
        {
            raum.Licht.Ausschalten();
        }
        else
        {
            raum.Licht.Einschalten();
        }
    }

    public decimal BerechneWassermenge(decimal stunden)
    {
        if (stunden < 0m)
            throw new ArgumentOutOfRangeException(nameof(stunden));

        return Wasserverbrauch * stunden;
    }

    public void PartyModus()
    {
        HauptstromAn = true;
        _stromverbrauch = 22m;

        foreach (var raum in _räume)
        {
            raum.PartyModus();
        }
    }

    public void Energiesparmodus()
    {
        HauptstromAn = true;
        _stromverbrauch = 4m;

        foreach (var raum in _räume)
        {
            raum.EnergieSparen();
        }
    }

    public string Statusbericht()
    {
        var zeilen = new List<string>
        {
            $"Hauptstrom: {(HauptstromAn ? "An" : "Aus")}",
            $"Stromverbrauch: {Stromverbrauch} A",
            $"Wasserverbrauch: {Wasserverbrauch} m³/h",
            "Räume:"
        };

        zeilen.AddRange(_räume.Select(r => r.ToString()));
        return string.Join(Environment.NewLine, zeilen);
    }
}
