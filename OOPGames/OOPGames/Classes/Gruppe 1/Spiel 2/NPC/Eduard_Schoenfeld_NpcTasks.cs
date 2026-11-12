using System;
using System.Collections.Generic;

namespace OOPGames
{
    public enum G1C_TaskState { Available, Active, Completed }

    // =====================================================
    // Klasse: G1C_CollectTask
    // Entwickler: Eduard Schoenfeld
    // Erstellt: 12.11.2026
    // Beschreibung: Sammelaufgabe im Casino: Gegenstände auf festen, begehbaren Kacheln einsammeln
    //               und beim Auftraggeber abgeben. Die Belohnung kommt aus der Hauskasse (nicht aus
    //               einem NPC-Geldbeutel) und wird pro Abschluss genau einmal ausgezahlt.
    // =====================================================
    public class G1C_CollectTask
    {
        private readonly int[,] _positions;
        private readonly bool[] _collected;
        private int _availableAgainTick = G1C_NpcMemory.Never;

        // Entwickler: Eduard Schoenfeld
        public G1C_CollectTask(string giverName, string title, string itemName, string itemSpriteId,
                               int[,] positions, int reward, int trustReward, int respectReward,
                               bool repeatable, int cooldownTicks, int requiredConversations)
        {
            if (positions == null || positions.GetLength(0) == 0 || positions.GetLength(1) != 2)
                throw new ArgumentException("Positionen müssen als {x, y}-Paare angegeben werden.", nameof(positions));
            GiverName = giverName;
            Title = title;
            ItemName = itemName;
            ItemSpriteId = itemSpriteId;
            _positions = positions;
            _collected = new bool[positions.GetLength(0)];
            Reward = reward;
            TrustReward = trustReward;
            RespectReward = respectReward;
            Repeatable = repeatable;
            CooldownTicks = cooldownTicks;
            RequiredConversations = requiredConversations;
        }

        public string GiverName { get; }
        public string Title { get; }
        public string ItemName { get; }
        public string ItemSpriteId { get; }
        public int Reward { get; }
        public int TrustReward { get; }
        public int RespectReward { get; }
        public bool Repeatable { get; }
        public int CooldownTicks { get; }
        public int RequiredConversations { get; }
        public G1C_TaskState State { get; private set; } = G1C_TaskState.Available;

        public int Total { get { return _collected.Length; } }

        public int CollectedCount
        {
            get
            {
                int count = 0;
                foreach (bool c in _collected) if (c) count++;
                return count;
            }
        }

        public bool AllCollected { get { return CollectedCount == Total; } }

        public bool CanStart(int tick, G1C_NpcCharacter giver)
        {
            return State == G1C_TaskState.Available && tick >= _availableAgainTick &&
                   giver.Memory.Conversations >= RequiredConversations &&
                   !G1C_NpcRelationship.IsHostile(giver.Relationship.Status);
        }

        public bool IsWaitingForCooldown(int tick)
        {
            return State == G1C_TaskState.Available && tick < _availableAgainTick;
        }

        // Entwickler: Eduard Schoenfeld
        public void Start()
        {
            if (State != G1C_TaskState.Available) return;
            for (int i = 0; i < _collected.Length; i++) _collected[i] = false;
            State = G1C_TaskState.Active;
        }

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Gegenstand auf der Kachel einsammeln (nur während die Aufgabe läuft).
        public bool TryCollect(int x, int y)
        {
            if (State != G1C_TaskState.Active) return false;
            for (int i = 0; i < _collected.Length; i++)
            {
                if (!_collected[i] && _positions[i, 0] == x && _positions[i, 1] == y)
                {
                    _collected[i] = true;
                    return true;
                }
            }
            return false;
        }

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Abgabe – gibt die Belohnung genau einmal zurück, sonst 0.
        public int TurnIn(int tick)
        {
            if (State != G1C_TaskState.Active || !AllCollected) return 0;
            State = Repeatable ? G1C_TaskState.Available : G1C_TaskState.Completed;
            _availableAgainTick = tick + CooldownTicks;
            return Reward;
        }

        // Änderung durch: Patrick Lupierz – Datum: 12.11.2026 – Aufgabenstand speichern und laden
        public G1C_SaveTask Capture()
        {
            return new G1C_SaveTask { Giver = GiverName, State = (int)State, Collected = (bool[])_collected.Clone(), AvailableAgainTick = _availableAgainTick };
        }

        public void Restore(G1C_SaveTask saved)
        {
            if (saved == null || saved.Collected == null || saved.Collected.Length != _collected.Length ||
                !Enum.IsDefined(typeof(G1C_TaskState), saved.State)) return;
            State = (G1C_TaskState)saved.State;
            Array.Copy(saved.Collected, _collected, _collected.Length);
            _availableAgainTick = saved.AvailableAgainTick;
        }

        public IEnumerable<KeyValuePair<int, int>> OpenItems()
        {
            if (State != G1C_TaskState.Active) yield break;
            for (int i = 0; i < _collected.Length; i++)
            {
                if (!_collected[i]) yield return new KeyValuePair<int, int>(_positions[i, 0], _positions[i, 1]);
            }
        }
    }

    // =====================================================
    // Klasse: G1C_TaskBoard
    // Entwickler: Eduard Schoenfeld
    // Erstellt: 12.11.2026
    // Beschreibung: Alle Sammelaufgaben im Casino. Positionen sind freie Teppichfelder (geprüft gegen
    //               Tische, Automaten, Bar und NPCs in G1C_CasinoWorld).
    // =====================================================
    public class G1C_TaskBoard
    {
        private readonly List<G1C_CollectTask> _tasks = new List<G1C_CollectTask>();

        // Entwickler: Eduard Schoenfeld
        public G1C_TaskBoard()
        {
            // Risikofreier, wiederholbarer Job – auch mit 0 Chips verfügbar
            _tasks.Add(new G1C_CollectTask("Barkeeper Toni", "Leere Gläser einsammeln", "Glas", "item_glass",
                new[,] { { 6, 4 }, { 12, 5 }, { 17, 6 } }, reward: 40, trustReward: 2, respectReward: 1,
                repeatable: true, cooldownTicks: 1500, requiredConversations: 1));

            // Einmalige Gefälligkeit für Bekannte
            _tasks.Add(new G1C_CollectTask("Croupier Rosa", "Verlorene Chips einsammeln", "Chip", "item_lost_chip",
                new[,] { { 1, 5 }, { 5, 6 }, { 6, 2 } }, reward: 60, trustReward: 8, respectReward: 5,
                repeatable: false, cooldownTicks: 0, requiredConversations: 2));
        }

        public IReadOnlyList<G1C_CollectTask> Tasks { get { return _tasks; } }

        public G1C_CollectTask FindFor(string giverName)
        {
            return _tasks.Find(t => t.GiverName == giverName);
        }

        public G1C_CollectTask ActiveTask
        {
            get { return _tasks.Find(t => t.State == G1C_TaskState.Active); }
        }

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Gibt die Aufgabe zurück, deren Gegenstand eingesammelt wurde (oder null).
        public G1C_CollectTask TryCollectAt(int x, int y)
        {
            foreach (G1C_CollectTask task in _tasks)
            {
                if (task.TryCollect(x, y)) return task;
            }
            return null;
        }
    }
}
