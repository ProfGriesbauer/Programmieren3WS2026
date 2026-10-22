public class Raum : IRaum, IDeserialisierbar
{
    public void Deserialize(string daten)
    {
        var json = System.Text.Json.Nodes.JsonNode.Parse(daten)!.AsObject();
        LadeDaten(json);
    }

    protected virtual void LadeDaten(System.Text.Json.Nodes.JsonObject json)
    {
        name = (string?)json["Name"] ?? "";
        lichtAn = (bool?)json["LichtAn"] ?? false;
        temperatur = (double?)json["Temperatur"] ?? 20.0;
    }

    private string name = "";
    private bool lichtAn;
    private double temperatur = 20.0;

    public string Name
    {
        get
        {
            return name;
        }
        set
        {
            name = value;
        }
    }

    public bool LichtAn
    {
        get
        {
            return lichtAn;
        }
        set
        {
            lichtAn = value;
        }
    }

    public double Temperatur
    {
        get
        {
            return temperatur;
        }
        set
        {
            temperatur = value;
        }
    }

    // Unterklassen erweitern die Daten um ihre eigenen Felder
    protected virtual Dictionary<string, object> SerialisierungsDaten()
    {
        return new Dictionary<string, object>
        {
            ["Typ"] = GetType().Name,
            ["Name"] = name,
            ["LichtAn"] = lichtAn,
            ["Temperatur"] = temperatur
        };
    }

    public string Serialize()
    {
        return System.Text.Json.JsonSerializer.Serialize(SerialisierungsDaten());
    }
}
