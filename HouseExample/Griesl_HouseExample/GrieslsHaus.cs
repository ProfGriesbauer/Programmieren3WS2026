public class GrieslsHaus : ISerialisierbar, IDeserialisierbar, IList<IRaum>
{
    // IList<IRaum>: delegiert an die interne Raumliste
    public int Count => raeume.Count;
    public bool IsReadOnly => false;

    public IRaum this[int index]
    {
        get => raeume[index];
        set => raeume[index] = value;
    }

    public void Add(IRaum raum) => raeume.Add(raum);
    public void Clear() => raeume.Clear();
    public bool Contains(IRaum raum) => raeume.Contains(raum);
    public void CopyTo(IRaum[] array, int arrayIndex) => raeume.CopyTo(array, arrayIndex);
    public int IndexOf(IRaum raum) => raeume.IndexOf(raum);
    public void Insert(int index, IRaum raum) => raeume.Insert(index, raum);
    public bool Remove(IRaum raum) => raeume.Remove(raum);
    public void RemoveAt(int index) => raeume.RemoveAt(index);
    public IEnumerator<IRaum> GetEnumerator() => raeume.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => raeume.GetEnumerator();

    public void Deserialize(string daten)
    {
        var json = System.Text.Json.Nodes.JsonNode.Parse(daten)!.AsObject();

        name = (string?)json["Name"] ?? "";
        hausnummer = (int?)json["Hausnummer"] ?? 0;
        hauptstromAn = (bool?)json["HauptstromAn"] ?? false;
        heizungAn = (bool?)json["HeizungAn"] ?? false;
        heizungsleistung = heizungAn ? HeizungKW : 0;
        stromverbrauch = ((double?)json["Stromverbrauch"] ?? GrundStromA) - GrundStromA;
        wasserverbrauch = (double?)json["Wasserverbrauch"] ?? 0;

        raeume.Clear();
        var raumArray = json["Raeume"]?.AsArray();
        if (raumArray == null)
        {
            return;
        }
        foreach (var eintrag in raumArray)
        {
            string raumJson = eintrag!.ToJsonString();
            Raum raum = (string?)eintrag["Typ"] == nameof(Bad) ? new Bad() : new Raum();
            raum.Deserialize(raumJson);
            raeume.Add(raum);
        }
    }

    public string Serialize()
    {
        var raumArray = new System.Text.Json.Nodes.JsonArray();
        foreach (IRaum raum in raeume)
        {
            raumArray.Add(System.Text.Json.Nodes.JsonNode.Parse(raum.Serialize()));
        }

        var haus = new System.Text.Json.Nodes.JsonObject
        {
            ["Name"] = name,
            ["Hausnummer"] = hausnummer,
            ["Stromverbrauch"] = Stromverbrauch,
            ["Wasserverbrauch"] = Wasserverbrauch,
            ["Heizungsleistung"] = Heizungsleistung,
            ["HauptstromAn"] = hauptstromAn,
            ["HeizungAn"] = heizungAn,
            ["WasserverbrauchRaeume"] = WasserverbrauchRaeume(),
            ["Raeume"] = raumArray
        };
        return haus.ToJsonString();
    }

    public string name;
    public int hausnummer;

    private List<IRaum> raeume = new List<IRaum>();

    public GrieslsHaus()
    {
        raeume.Add(new Raum());
        raeume.Add(new Raum());
        raeume.Add(new Raum());
        raeume.Add(new Raum());
        raeume.Add(new Bad());
    }

    public void RaumHinzufuegen(IRaum raum)
    {
        raeume.Add(raum);
    }

    public void RaumUmbenennen(int index, string neuerName)
    {
        if (index >= 0 && index < raeume.Count)
        {
            raeume[index].Name = neuerName;
        }
    }

    // Summe des Wasserverbrauchs [m³/h] aller Räume mit Wasseranschluss
    public double WasserverbrauchRaeume()
    {
        double summe = 0;
        foreach (IRaum raum in raeume)
        {
            if (raum is Bad)
            {
                summe = summe + ((Bad)raum).Wasserverbrauch;
            }
        }
        
        return summe;
    }

    private const double GrundStromA = 2.0;
    private const double WasserM3proH = 0.5;
    private const double HeizungKW = 5.0;

    private double stromverbrauch;   // [A]
    private double wasserverbrauch;  // [m³/h]
    private double heizungsleistung; // [kW]
    private bool heizungAn;
    private bool hauptstromAn;

    public double Stromverbrauch
    {
        get
        {
            return GrundStromA + stromverbrauch;
        }
        set
        {
            stromverbrauch = value;
            if (value > 0)
            {
                hauptstromAn = true;
            }
            else
            {
                hauptstromAn = false;
                HeizungAn = false; // ohne Strom keine Heizung
            }
        }
    }

    public double Wasserverbrauch
    {
        get
        {
            return wasserverbrauch;
        }
    }

    public double Heizungsleistung
    {
        get
        {
            return heizungsleistung;
        }
    }

    public bool HauptstromAn
    {
        get
        {
            return hauptstromAn;
        }
        set
        {
            hauptstromAn = value;
            if (value)
            {
                stromverbrauch = GrundStromA;
            }
            else
            {
                stromverbrauch = 0;
                HeizungAn = false; // ohne Strom keine Heizung
            }
        }
    }

    public bool HeizungAn
    {
        get
        {
            return heizungAn;
        }
        set
        {
            if (value && !hauptstromAn)
            {
                return; // Heizung nur mit Strom einschaltbar
            }
            heizungAn = value;
            heizungsleistung = value ? HeizungKW : 0;
        }
    }

    public void WasserAufdrehen(bool an)
    {
        wasserverbrauch = an ? WasserM3proH : 0;
    }

    // Verbrauchte Wassermenge [m³] über die angegebene Dauer in Stunden
    public double BerechneWasserverbrauch(double stunden)
    {
        return wasserverbrauch * stunden;
    }
}
