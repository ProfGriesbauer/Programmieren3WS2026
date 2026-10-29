public static class GrieslsHausFabrik
{
    static string meineDaten;
    // Haus mit den Standardräumen aus dem Konstruktor
    public static GrieslsHaus Erstellen(string name, int hausnummer)
    {
        return new GrieslsHaus { name = name, hausnummer = hausnummer };
    }

    // Haus aus einem von Serialize() erzeugten JSON-String
    public static GrieslsHaus AusJson(string json)
    {
        var haus = new GrieslsHaus();
        haus.Deserialize(json);
        return haus;
    }
}
