using System;
using System.Linq;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_ScheduleEntry
    // Entwickler: Patrick Lupierz
    // Erstellt: 18.11.2026
    // Beschreibung: Tagesablauf eines Stadtbewohners: von wann bis wann er draußen in der Stadt ist und ob er
    //               danach ins Casino geht. Stunden 0–23; "bis" darf über Mitternacht gehen (z. B. 17 bis 3).
    // =====================================================
    public sealed class G1C_ScheduleEntry
    {
        public string Name { get; set; }
        public int TownFrom { get; set; }
        public int TownUntil { get; set; }
        public int CasinoFrom { get; set; } = -1;
        public int CasinoUntil { get; set; } = -1;
    }

    // =====================================================
    // Klasse: G1C_NpcSchedule
    // Entwickler: Patrick Lupierz
    // Erstellt: 18.11.2026
    // Beschreibung: SprintPlanning Issue 5 ("KIs sollen einen Tagesablauf haben, z. B. abends nach Hause oder ins
    //               Casino"). Nach der Spieluhr verschwinden Stadtbewohner nachts (zu Hause) und Gast Lena wechselt
    //               abends ins Casino. Damit niemand vor den Augen des Spielers verschwindet, wird nur umgeschaltet,
    //               wenn der Spieler gerade nicht in der Nähe ist. Reine Logik; Anzeige/Kollision folgen automatisch,
    //               weil abwesende NPCs nicht in der Objektliste der Welt stehen.
    // =====================================================
    public static class G1C_NpcSchedule
    {
        public const int VisibleDistance = 9;

        public static readonly G1C_ScheduleEntry[] Entries =
        {
            new G1C_ScheduleEntry { Name = "Gast Lena", TownFrom = 9, TownUntil = 17, CasinoFrom = 17, CasinoUntil = 3 },
            new G1C_ScheduleEntry { Name = "Kurier Paul", TownFrom = 7, TownUntil = 19 },
            new G1C_ScheduleEntry { Name = "Gärtner Sam", TownFrom = 6, TownUntil = 18 },
            new G1C_ScheduleEntry { Name = "Studentin Lea", TownFrom = 10, TownUntil = 22 },
            new G1C_ScheduleEntry { Name = "Geldhai Viktor", TownFrom = 10, TownUntil = 4 },
        };

        public static bool InRange(int hour, int from, int until)
        {
            if (from < 0) return false;
            return from <= until ? hour >= from && hour < until : hour >= from || hour < until;
        }

        // Entwickler: Patrick Lupierz – wo jemand laut Plan gerade ist (auch für Gesprächsantworten nutzbar)
        public static string WhereIs(string name, int hour)
        {
            G1C_ScheduleEntry e = Entries.FirstOrDefault(x => x.Name == name);
            if (e == null) return "unterwegs";
            if (InRange(hour, e.TownFrom, e.TownUntil)) return "in der Stadt";
            if (InRange(hour, e.CasinoFrom, e.CasinoUntil)) return "im Casino";
            return "zu Hause";
        }

        // Entwickler: Patrick Lupierz
        // Aufgabe: Anwesenheit in Stadt und Casino an die Uhrzeit anpassen (nur außer Sichtweite des Spielers).
        public static void Apply(int hour, G1C_ExteriorWorld town, G1C_CasinoWorld casino, G1C_Player player,
                                 G1C_WorldLocation location)
        {
            if (town == null || casino == null || player == null) return;
            foreach (G1C_ScheduleEntry e in Entries)
            {
                G1C_Npc outside = town.TownNpcs.FirstOrDefault(n => n.Name == e.Name);
                if (outside != null)
                {
                    bool wanted = InRange(hour, e.TownFrom, e.TownUntil);
                    bool seen = location == G1C_WorldLocation.Exterior && Near(outside, player);
                    if (town.IsPresent(outside) != wanted && !seen && !outside.IsMoving &&
                        !(wanted && player.TileX == outside.X && player.TileY == outside.Y))
                        town.SetNpcPresent(outside, wanted);
                }
                if (e.CasinoFrom < 0) continue;
                G1C_Npc inside = casino.FindNpc(e.Name);
                if (inside == null) continue;
                bool inCasino = InRange(hour, e.CasinoFrom, e.CasinoUntil); // abends im Casino, nachts zu Hause
                bool seenInside = location == G1C_WorldLocation.Casino;
                if (casino.IsPresent(inside) != inCasino && !seenInside) casino.SetNpcPresent(inside, inCasino);
            }
        }

        private static bool Near(G1C_WorldObject npc, G1C_Player player)
        {
            return Math.Abs(npc.X - player.TileX) <= VisibleDistance && Math.Abs(npc.Y - player.TileY) <= VisibleDistance;
        }
    }
}
