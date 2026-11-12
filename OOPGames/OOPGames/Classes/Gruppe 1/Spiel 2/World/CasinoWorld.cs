using System.Collections.Generic;

namespace OOPGames
{
    /// <summary>
    /// Das Casino-Gebäude: Karte, Spieltische, Automaten und NPCs.
    /// Es gibt bewusst nur dieses eine Gebäude (Rules_Framework, Abschnitt 7).
    /// </summary>
    public class G1C_CasinoWorld
    {
        private readonly List<G1C_WorldObject> _objects = new List<G1C_WorldObject>();

        public G1C_CasinoMap Map { get; } = new G1C_CasinoMap();
        public IReadOnlyList<G1C_WorldObject> Objects { get { return _objects; } }

        public int PlayerStartX { get { return 9; } }
        public int PlayerStartY { get { return 10; } }

        public G1C_CasinoWorld()
        {
            BuildCasino();
        }

        private void BuildCasino()
        {
            // --- Roulette (links oben): Tisch 3x2 Kacheln, Croupier dahinter ---
            string[] rouletteLines =
            {
                "Willkommen am Roulette-Tisch!",
                "Setze auf eine Zahl (35:1), ein Dutzend (2:1)\noder auf Rot/Schwarz, Gerade/Ungerade, 1-18/19-36 (1:1).",
                "Möchtest du eine Runde spielen?"
            };
            _objects.Add(new G1C_WorldObject("Croupier Rosa", "obj_roulette_table", 2, 3, 3, 2, true, G1C_InteractionType.Roulette, rouletteLines));
            _objects.Add(new G1C_Npc("Croupier Rosa", "dealer_red", 3, 2, G1C_Direction.Down, G1C_InteractionType.Roulette, rouletteLines));

            // --- Blackjack (Mitte oben) ---
            string[] blackjackLines =
            {
                "Hallo! Hier wird Blackjack gespielt.",
                "Komm näher an 21 als ich, ohne dich zu überkaufen.\nBlackjack zahlt 3:2. Ich ziehe bis 16 und halte ab 17.",
                "Setzt du dich an den Tisch?"
            };
            _objects.Add(new G1C_WorldObject("Dealer Ben", "obj_blackjack_table", 8, 3, 3, 2, true, G1C_InteractionType.Blackjack, blackjackLines));
            _objects.Add(new G1C_Npc("Dealer Ben", "dealer_green", 9, 2, G1C_Direction.Down, G1C_InteractionType.Blackjack, blackjackLines));

            // --- Poker (rechts oben) ---
            string[] pokerLines =
            {
                "Texas Hold'em, nur du gegen mich.",
                "Jeder bekommt 2 Karten, dazu 5 Gemeinschaftskarten.\nPro Runde höchstens ein Einsatz und eine Erhöhung.",
                "Traust du dich?"
            };
            _objects.Add(new G1C_WorldObject("Pokerprofi Max", "obj_poker_table", 14, 3, 3, 2, true, G1C_InteractionType.Poker, pokerLines));
            _objects.Add(new G1C_Npc("Pokerprofi Max", "dealer_blue", 15, 2, G1C_Direction.Down, G1C_InteractionType.Poker, pokerLines));

            // --- Spielautomaten (links unten) ---
            string[] slotLines =
            {
                "Ein blinkender Spielautomat mit drei Walzen.",
                "Eine Runde spielen?"
            };
            for (int x = 2; x <= 5; x++)
            {
                _objects.Add(new G1C_WorldObject("Spielautomat", "obj_slot_machine", x, 8, 1, 1, true, G1C_InteractionType.Slots, slotLines));
            }

            // --- Bar (rechts unten) ---
            string[] barLines =
            {
                "Willkommen an der Bar!",
                "Falls dir mal die Chips ausgehen, komm zu mir.\nDann gibt es eine Runde aufs Haus."
            };
            for (int x = 14; x <= 17; x++)
            {
                _objects.Add(new G1C_WorldObject("Barkeeper Toni", "obj_bar", x, 9, 1, 1, true, G1C_InteractionType.Bartender, barLines));
            }
            _objects.Add(new G1C_Npc("Barkeeper Toni", "bartender", 15, 10, G1C_Direction.Up, G1C_InteractionType.Bartender, barLines));

            // --- Gast mit Tipps ---
            _objects.Add(new G1C_Npc("Gast Lena", "guest", 11, 7, G1C_Direction.Left, G1C_InteractionType.Talk,
                "Psst! Beim Blackjack halte ich ab 17 immer.",
                "Und beim Poker: Wenn Max erhöht, hat er meistens etwas Gutes.",
                "Mit [M] öffnest du übrigens das Menü mit allen Tasten."));

            // --- Deko ---
            _objects.Add(new G1C_WorldObject("Pflanze", "obj_plant", 1, 2, 1, 1, true, G1C_InteractionType.None));
            _objects.Add(new G1C_WorldObject("Pflanze", "obj_plant", 18, 2, 1, 1, true, G1C_InteractionType.None));
            _objects.Add(new G1C_WorldObject("Pflanze", "obj_plant", 1, 10, 1, 1, true, G1C_InteractionType.None));
            _objects.Add(new G1C_WorldObject("Pflanze", "obj_plant", 18, 10, 1, 1, true, G1C_InteractionType.None));
        }

        /// <summary>Liefert das Objekt auf einer Kachel oder null.</summary>
        public G1C_WorldObject ObjectAt(int x, int y)
        {
            for (int i = _objects.Count - 1; i >= 0; i--)
            {
                if (_objects[i].Occupies(x, y)) return _objects[i];
            }
            return null;
        }
    }
}
