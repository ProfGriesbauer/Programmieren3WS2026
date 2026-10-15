namespace TimEichert_HouseExample;

using System.Collections.Generic;

public class Stockwerke
{
    public List<Stockwerk> stockwerke = new();

    public Stockwerk StockwerkHinzufuegen(int nummer)
    {
        Stockwerk stockwerk = new Stockwerk { nummer = nummer };
        stockwerke.Add(stockwerk);
        return stockwerk;
    }

    public void Anzeigen()
    {
        foreach (Stockwerk stockwerk in stockwerke)
        {
            stockwerk.Anzeigen();
        }
    }

    public double LichtleistungBerechnen()
    {
        double gesamt = 0;
        foreach (Stockwerk stockwerk in stockwerke)
        {
            gesamt += stockwerk.LichtleistungBerechnen();
        }

        return gesamt;
    }
}