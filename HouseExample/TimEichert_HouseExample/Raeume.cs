using System.Collections.Generic;

namespace TimEichert_HouseExample;

public class Raeume
{
    public List<Raum> raeume = new();

    public void RaumHinzufuegen(Raum raum)
    {
        raeume.Add(raum);
    }

    public void Anzeigen()
    {
        if (raeume.Count == 0)
        {
            Console.WriteLine("  Keine Räume vorhanden.");
            return;
        }

        foreach (Raum raum in raeume)
        {
            string anzeige = $"  {raum.GetType().Name}: {raum.name}" +
                $" | Fläche: {raum.flaeche:0.##} m²" +
                $" | Heizung: {(raum.HeizungIstAn ? "an" : "aus")}" +
                $" | Licht: {(raum.LichtIstAn ? "an" : "aus")}" +
                $" | Lichtleistung: {raum.LichtleistungWatt:0.##} W";

            if (raum is IWasser wasser)
            {
                anzeige += $" | Wasser: {(wasser.WasserLaeuft ? "läuft" : "aus")}";
            }

            Console.WriteLine(anzeige);
        }
    }

    public double LichtleistungBerechnen()
    {
        double gesamt = 0;
        foreach (Raum raum in raeume)
        {
            gesamt += raum.LichtleistungWatt;
        }

        return gesamt;
    }
}