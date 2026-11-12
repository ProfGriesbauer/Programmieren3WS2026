using System;
using System.Collections.Generic;
using System.Linq;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_SpeechBubble
    // Entwickler: Eduard Schoenfeld
    // Erstellt: 12.11.2026
    // Beschreibung: Eine Sprechblase über einer Weltfigur.
    // =====================================================
    public class G1C_SpeechBubble
    {
        public G1C_Npc Npc { get; set; }
        public string Text { get; set; }
        public int EndTick { get; set; }
        public bool IsReaction { get; set; }
    }

    // =====================================================
    // Klasse: G1C_SpeechBubbleSystem
    // Entwickler: Eduard Schoenfeld
    // Erstellt: 12.11.2026
    // Beschreibung: Plant kurze Monologe der NPCs: höchstens 3 Blasen gleichzeitig, nahe NPCs zuerst,
    //               keine Überdeckung, automatisches Ausblenden. Reaktionen auf Ereignisse haben Vorrang.
    //               Während Gesprächen oder Minispielen entstehen keine neuen Monologe.
    // =====================================================
    public class G1C_SpeechBubbleSystem
    {
        public const int MaxVisible = 3;
        private const int MonologueTicks = 90;    // 3,6 s
        private const int ReactionTicks = 125;    // 5 s
        private const int MinPause = 200;         // 8 s
        private const int MaxPause = 500;         // 20 s

        private readonly List<G1C_SpeechBubble> _active = new List<G1C_SpeechBubble>();
        private readonly Dictionary<G1C_Npc, int> _nextMonologue = new Dictionary<G1C_Npc, int>();
        private readonly Random _random;

        public G1C_SpeechBubbleSystem(Random random)
        {
            _random = random ?? new Random();
        }

        public IReadOnlyList<G1C_SpeechBubble> Active { get { return _active; } }

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Abgelaufene Blasen entfernen und ggf. einen neuen Monolog starten.
        public void Update(int tick, bool allowMonologues, G1C_Player player,
                           IEnumerable<KeyValuePair<G1C_Npc, G1C_NpcCharacter>> npcs)
        {
            _active.RemoveAll(b => tick >= b.EndTick);
            if (!allowMonologues)
            {
                // Pausiert (Gespräch, Minispiel, Ergebnis): Blasen laufen erst danach ab
                foreach (G1C_SpeechBubble b in _active) b.EndTick++;
                return;
            }
            if (_active.Count >= MaxVisible) return;

            // Nahe NPCs haben Vorrang
            foreach (KeyValuePair<G1C_Npc, G1C_NpcCharacter> pair in npcs
                         .OrderBy(p => Math.Abs(p.Key.X - player.TileX) + Math.Abs(p.Key.Y - player.TileY)))
            {
                G1C_Npc npc = pair.Key;
                int next;
                if (!_nextMonologue.TryGetValue(npc, out next))
                {
                    _nextMonologue[npc] = tick + _random.Next(50, MaxPause); // gestaffelter Start
                    continue;
                }
                if (tick < next || HasBubble(npc) || Overlaps(npc)) continue;

                G1C_NpcCharacter c = pair.Value;
                string text = c.Personality.PickMonologue(c.Relationship.Status, c.Mood, _random);
                _nextMonologue[npc] = tick + _random.Next(MinPause, MaxPause);
                if (string.IsNullOrEmpty(text)) continue;

                _active.Add(new G1C_SpeechBubble { Npc = npc, Text = text, EndTick = tick + MonologueTicks });
                return; // höchstens eine neue Blase pro Tick
            }
        }

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Reaktion sofort anzeigen; verdrängt bei Bedarf den ältesten Monolog.
        public void ShowReaction(G1C_Npc npc, string text, int tick)
        {
            if (npc == null || string.IsNullOrEmpty(text)) return;
            _active.RemoveAll(b => b.Npc == npc);
            if (_active.Count >= MaxVisible)
            {
                G1C_SpeechBubble oldest = _active.Where(b => !b.IsReaction).OrderBy(b => b.EndTick).FirstOrDefault()
                                          ?? _active.OrderBy(b => b.EndTick).First();
                _active.Remove(oldest);
            }
            _active.Add(new G1C_SpeechBubble { Npc = npc, Text = text, EndTick = tick + ReactionTicks, IsReaction = true });
            _nextMonologue[npc] = tick + ReactionTicks + MinPause;
        }

        // Änderung durch: Patrick Lupierz – Datum: 12.11.2026 – abgelaufene Blasen entfernen, ohne neue Monologe zu starten
        public void Expire(int tick)
        {
            _active.RemoveAll(b => tick >= b.EndTick);
        }

        public void Clear()
        {
            _active.Clear();
        }

        public void Remove(G1C_Npc npc)
        {
            _active.RemoveAll(b => b.Npc == npc);
        }

        private bool HasBubble(G1C_Npc npc)
        {
            return _active.Any(b => b.Npc == npc);
        }

        // Blasen sind etwa 5 Kacheln breit: nah beieinander stehende NPCs reden nicht gleichzeitig
        private bool Overlaps(G1C_Npc npc)
        {
            return _active.Any(b => Math.Abs(b.Npc.X - npc.X) < 5 && Math.Abs(b.Npc.Y - npc.Y) <= 1);
        }
    }
}
