using System;
using System.Collections.Generic;
using System.Linq;

namespace OOPGames
{
    // Entwickler: Patrick Lupierz – Datum: 15.11.2026 – ein auswählbares Spiel eines Automaten
    public sealed class G1C_ArcadeGameEntry
    {
        public G1C_ArcadeGameEntry(string title, string description, Func<G1C_MoneyManager, Random, G1C_CasinoGameBase> create)
        {
            Title = title;
            Description = description;
            Create = create ?? throw new ArgumentNullException(nameof(create));
        }

        public string Title { get; }
        public string Description { get; }
        public Func<G1C_MoneyManager, Random, G1C_CasinoGameBase> Create { get; }
    }

    // =====================================================
    // Klasse: G1C_ArcadeCabinet
    // Entwickler: Patrick Lupierz
    // Erstellt: 15.11.2026
    // Beschreibung: Daten eines Spielautomaten der Spielhalle: Name (= Weltobjekt), Grafik, Position und
    //               höchstens neun Spiele. Der Gast-Automat ermittelt seine Spiele erst beim Benutzen.
    // =====================================================
    public sealed class G1C_ArcadeCabinet
    {
        public G1C_ArcadeCabinet(string name, string spriteId, int x, int y, bool isGuest, params G1C_ArcadeGameEntry[] games)
        {
            if (games.Length > G1C_ArcadeCatalog.MaxGamesPerCabinet)
                throw new ArgumentException("Höchstens " + G1C_ArcadeCatalog.MaxGamesPerCabinet + " Spiele pro Automat.", nameof(games));
            Name = name; SpriteId = spriteId; X = x; Y = y; IsGuestCabinet = isGuest;
            Games = games;
        }

        public string Name { get; }
        public string SpriteId { get; }
        public int X { get; }
        public int Y { get; }
        public bool IsGuestCabinet { get; }
        public IReadOnlyList<G1C_ArcadeGameEntry> Games { get; }
    }

    // =====================================================
    // Klasse: G1C_ArcadeCatalog
    // Entwickler: Patrick Lupierz
    // Erstellt: 15.11.2026
    // Beschreibung: Die fünf Automaten der Spielhalle (SprintPlanning C11): vier mit eigenen Spielen an der
    //               Rückwand, einer in der Mitte für dynamisch eingebundene Spiele anderer Gruppen
    //               (G1C_GuestGameCatalog). Maximal neun Spiele pro Automat.
    // =====================================================
    public static class G1C_ArcadeCatalog
    {
        public const int MaxGamesPerCabinet = 9;
        public const string GuestCabinetName = "Gast-Automat";
        public const string HighscoreBoardName = "Highscore-Tafel";

        private static readonly G1C_ArcadeGameEntry StarHunter = new G1C_ArcadeGameEntry("Sternenjäger",
            "Weltraum-Shooter: Formation abschießen, Sturzflügen ausweichen.", (m, r) => new G1C_StarHunter(m, r));
        private static readonly G1C_ArcadeGameEntry Snake = new G1C_ArcadeGameEntry("Schlange",
            "Früchte fressen, länger werden, nicht beißen.", (m, r) => new G1C_SnakeGame(m, r));
        private static readonly G1C_ArcadeGameEntry Bricks = new G1C_ArcadeGameEntry("Ziegelbrecher",
            "Mit Ball und Schläger alle Steine abräumen.", (m, r) => new G1C_BrickBreaker(m, r));
        private static readonly G1C_ArcadeGameEntry Pong = new G1C_ArcadeGameEntry("Pong-Duell",
            "Tischtennis gegen die Automaten-KI, bis 5 Punkte.", (m, r) => new G1C_PongDuel(m, r));

        public static readonly IReadOnlyList<G1C_ArcadeCabinet> Cabinets = new List<G1C_ArcadeCabinet>
        {
            new G1C_ArcadeCabinet("Automat Sternenjäger", "obj_arcade_cabinet_stars", 2, 2, false, StarHunter),
            new G1C_ArcadeCabinet("Automat Retro-Klassiker", "obj_arcade_cabinet_retro", 4, 2, false, Snake, Bricks),
            new G1C_ArcadeCabinet("Automat Duell", "obj_arcade_cabinet_duel", 10, 2, false, Pong),
            new G1C_ArcadeCabinet("Automat 90er-Mix", "obj_arcade_cabinet_mix", 12, 2, false, StarHunter, Snake, Bricks, Pong),
            new G1C_ArcadeCabinet(GuestCabinetName, "obj_arcade_cabinet_guest", 7, 5, true),
        };

        public static G1C_ArcadeCabinet Find(string name)
        {
            return Cabinets.FirstOrDefault(c => c.Name == name);
        }
    }
}
