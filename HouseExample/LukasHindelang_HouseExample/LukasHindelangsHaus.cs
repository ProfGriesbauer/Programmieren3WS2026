using System.Collections.Generic;

public class HindelangHaus
{
    private readonly double _stromverbrauchBeiBetrieb;  // private: von außen darf nicht zugegriffen werden. double: Für Nachkommastellen.
    private readonly double _wasserverbrauchProStunde;  // readonly: speichert die eingestellen Betriebswerte (werden beim erstellen von Haus festgelegt)
    private readonly double _heizungsleistungBeiBetrieb;
    private double _stromverbrauch; // aktueller Zustand des Hauses (wird beim Betrieb verändert)
    private double _heizungsleistung;
    private bool _heizungAn;
    private bool _hauptstromAn;

// konstruktor: wird beim erstellen von Haus aufgerufen, um die Betriebswerte festzulegen.
    public HindelangHaus(
        double stromverbrauchAmpere,
        double wasserverbrauchKubikmeterProStunde,
        double heizungsleistungKw)
    {
        if (stromverbrauchAmpere < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stromverbrauchAmpere));
        }

        if (wasserverbrauchKubikmeterProStunde < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(wasserverbrauchKubikmeterProStunde));
        }

        if (heizungsleistungKw < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(heizungsleistungKw));
        }

        _stromverbrauchBeiBetrieb = stromverbrauchAmpere;
        _wasserverbrauchProStunde = wasserverbrauchKubikmeterProStunde;
        _heizungsleistungBeiBetrieb = heizungsleistungKw;
    }

    public string name = string.Empty; // können direkt gesetzt werden, da es public ist. string.Empty: initialisiert den String mit einem leeren Wert.
    public int nummer;
    public List<IRaum> Räume { get; } = new();

    // Eigenschaften erlauben das lesen aber nicht das schreiben von außen. Sie können nur innerhalb der Klasse verändert werden.

    public double Stromverbrauch => _stromverbrauch;
    public double Wasserverbrauch => _wasserverbrauchProStunde;
    public double Heizungsleistung => _heizungsleistung;

    // get liefert den aktuellen Wert der Variable zurück, set setzt den Wert der Variable. In diesem Fall wird die Variable nur verändert, wenn der Hauptstrom eingeschaltet ist. Wenn der Hauptstrom ausgeschaltet wird, wird auch die Heizung ausgeschaltet.

    public bool HauptstromAn
    {
        get => _hauptstromAn;
        set
        {
            _hauptstromAn = value;
            _stromverbrauch = value ? _stromverbrauchBeiBetrieb : 0;

            if (!value)
            {
                HeizungAn = false;
            }
        }
    }

    public bool HeizungAn
    {
        get => _heizungAn;
        set
        {
            _heizungAn = value && _hauptstromAn;  // nur wenn Bedingungen erfüllt sind.
            _heizungsleistung = _heizungAn ? _heizungsleistungBeiBetrieb : 0;
        }
    }

    public double BerechneWasserverbrauch(TimeSpan zeitraum)
    {
        if (zeitraum < TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(zeitraum), "Der Zeitraum darf nicht negativ sein.");
        }

        return _wasserverbrauchProStunde * zeitraum.TotalHours;
    }
}