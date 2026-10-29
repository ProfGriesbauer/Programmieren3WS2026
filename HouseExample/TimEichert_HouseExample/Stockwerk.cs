namespace TimEichert_HouseExample;

public class Stockwerk
{
    public int nummer;
    public Raeume raeume = new Raeume();

    public void RaumHinzufuegen(Raum raum)
    {
        raeume.RaumHinzufuegen(raum);
    }

    public void Anzeigen()
    {
        Console.WriteLine($"Stockwerk {nummer}:");
        raeume.Anzeigen();
    }

    public double LichtleistungBerechnen()
    {
        return raeume.LichtleistungBerechnen();
    }
}