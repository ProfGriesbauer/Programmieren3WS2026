using System;
using System.Collections.Generic;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_NpcSystem
    // Entwickler: Eduard Schoenfeld
    // Erstellt: 12.11.2026
    // Beschreibung: Zentrale Anbindung des NPC-Systems an den GameManager. Hält die Charaktere,
    //               den globalen Ruf, Gespräch, Sprechblasen, Aufgaben und Ereignisreaktionen.
    //               Wird nur über Framework-Eingaben (HandleInput) und den 40-ms-Tick (Update) bewegt.
    // =====================================================
    public class G1C_NpcSystem
    {
        private readonly List<G1C_NpcCharacter> _characters;
        private readonly Dictionary<G1C_NpcCharacter, G1C_Npc> _worldNpcs = new Dictionary<G1C_NpcCharacter, G1C_Npc>();

        // Entwickler: Eduard Schoenfeld
        public G1C_NpcSystem(G1C_CasinoWorld casino, G1C_MoneyManager money, Random random)
        {
            if (casino == null) throw new ArgumentNullException(nameof(casino));
            Money = money ?? throw new ArgumentNullException(nameof(money));
            Random = random ?? new Random();

            // Erweiterung durch Denis Mock: Profile der Stadtbewohner im gemeinsamen Gesprächssystem.
            _characters = G1C_NpcRoster.CreateCasinoNpcs();
            _characters.AddRange(G1C_CityNpcRoster.Create()); // Erweiterung durch Denis Mock: Gespräche mit Stadtbewohnern.
            foreach (G1C_WorldObject obj in casino.Objects)
            {
                G1C_Npc npc = obj as G1C_Npc;
                G1C_NpcCharacter c = npc != null ? Find(npc.Name) : null;
                if (c != null) _worldNpcs[c] = npc;
            }

            Bubbles = new G1C_SpeechBubbleSystem(Random);
            Tasks = new G1C_TaskBoard();
            MoneyRequests = new G1C_NpcMoneyService();
            Events = new G1C_CasinoEventReactor(this);
            Conversation = new G1C_Conversation(this);
        }

        public G1C_MoneyManager Money { get; }
        public Random Random { get; }
        public int Tick { get; private set; }
        public int Reputation { get; private set; } = 50;
        public G1C_SpeechBubbleSystem Bubbles { get; }
        public G1C_TaskBoard Tasks { get; }
        public G1C_NpcMoneyService MoneyRequests { get; }
        public G1C_CasinoEventReactor Events { get; }
        public G1C_Conversation Conversation { get; }
        public IReadOnlyList<G1C_NpcCharacter> Characters { get { return _characters; } }

        public IEnumerable<KeyValuePair<G1C_Npc, G1C_NpcCharacter>> PlacedNpcs
        {
            get
            {
                foreach (KeyValuePair<G1C_NpcCharacter, G1C_Npc> pair in _worldNpcs)
                    yield return new KeyValuePair<G1C_Npc, G1C_NpcCharacter>(pair.Value, pair.Key);
            }
        }

        public G1C_NpcCharacter Find(string name)
        {
            return name == null ? null : _characters.Find(c => c.Name == name);
        }

        public G1C_Npc WorldNpcOf(G1C_NpcCharacter character)
        {
            G1C_Npc npc;
            return character != null && _worldNpcs.TryGetValue(character, out npc) ? npc : null;
        }

        // Entwickler: Eduard Schoenfeld
        public void ChangeReputation(int delta)
        {
            Reputation = Math.Max(0, Math.Min(100, Reputation + delta));
        }

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Gespräch mit dem NPC (oder dessen Tisch/Tresen) öffnen. False, wenn kein Profil existiert.
        // Anpassung durch Denis Mock: Außenwelt-NPCs besitzen kein Casino-Sprechblasenobjekt.
        public bool TryStartConversation(G1C_WorldObject target)
        {
            G1C_NpcCharacter character = target != null ? Find(target.Name) : null;
            if (character == null) return false;

            G1C_InteractionType interaction = target.Interaction;
            bool offersGame = interaction == G1C_InteractionType.Roulette || interaction == G1C_InteractionType.Blackjack ||
                              interaction == G1C_InteractionType.Poker;
            G1C_Npc worldNpc = WorldNpcOf(character);
            if (worldNpc != null) Bubbles.Remove(worldNpc);
            Conversation.Open(character, offersGame ? interaction : (G1C_InteractionType?)null);
            return true;
        }

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Tick: Stimmungen, Hinweise und (nur im Casino, während der Erkundung) Monologe.
        public void Update(int tick, G1C_Player player, bool inCasino, bool exploring)
        {
            Tick = tick;
            foreach (G1C_NpcCharacter c in _characters) c.UpdateMood(tick);
            Conversation.UpdateNotices(tick);

            if (!inCasino)
            {
                Bubbles.Clear();
                return;
            }
            Bubbles.Update(tick, exploring, player, PlacedNpcs);
        }

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Aufgaben-Gegenstand auf der Kachel des Spielers einsammeln.
        public void TryCollectItem(int x, int y)
        {
            G1C_CollectTask task = Tasks.TryCollectAt(x, y);
            if (task == null) return;

            G1C_NpcCharacter giver = Find(task.GiverName);
            if (task.AllCollected)
                Bubbles.ShowReaction(WorldNpcOf(giver), "Alles da? Bring es mir!", Tick);
        }

        public void OnMinigameFinished(G1C_InteractionType game, int balanceDiff)
        {
            Events.OnMinigameFinished(game, balanceDiff);
        }
    }
}
