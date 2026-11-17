using System;
using System.Collections.Generic;
using System.Linq;

namespace OOPGames
{
    public enum G1C_PetKind { Cat, Dog }

    // =====================================================
    // Klasse: G1C_PetAnimal
    // Entwickler: Patrick Lupierz
    // Erstellt: 16.11.2026
    // Beschreibung: Katze oder Hund, der um seinen Startplatz herumläuft, nicht begehbar ist (massiv) und
    //               gestreichelt werden kann. Katzen gibt es in 5 Fellvarianten (grey, calico, orange, black,
    //               white), Hunde in 3 (brown, dalmatian, golden). Bewegung wie bei NPCs: logische Kachel sofort,
    //               Darstellung interpoliert; während des Schritts ist auch die Ausgangskachel belegt.
    //               Grafik-ID: pet_<cat|dog>_<variante>_<l|r>_<bild>, erzeugt in G1C_AnimalSprites.
    // =====================================================
    public sealed class G1C_PetAnimal : G1C_WorldObject
    {
        public const int StepDuration = 10;
        public static readonly string[] CatVariants = { "grey", "calico", "orange", "black", "white" };
        public static readonly string[] DogVariants = { "brown", "dalmatian", "golden" };

        private int _fromX;
        private int _fromY;
        private int _movementTicks = StepDuration;
        private int _restTicks;

        public G1C_PetAnimal(string name, G1C_PetKind kind, string variant, int x, int y, int roamRadius)
            : base(name, null, x, y, 1, 1, true, G1C_InteractionType.Pet)
        {
            string[] allowed = kind == G1C_PetKind.Cat ? CatVariants : DogVariants;
            if (!allowed.Contains(variant)) throw new ArgumentException("Unbekannte Fellvariante: " + variant, nameof(variant));
            Kind = kind;
            Variant = variant;
            HomeX = x;
            HomeY = y;
            RoamRadius = Math.Max(0, roamRadius);
            _fromX = x;
            _fromY = y;
        }

        public G1C_PetKind Kind { get; }
        public string Variant { get; }
        public int HomeX { get; }
        public int HomeY { get; }
        public int RoamRadius { get; }
        public bool FacingLeft { get; private set; }
        public int PetCount { get; private set; }
        public int LastTick { get; private set; }
        public bool IsMoving { get { return _movementTicks < StepDuration; } }
        public double VisualX { get { return IsMoving ? _fromX + (X - _fromX) * (_movementTicks / (double)StepDuration) : X; } }
        public double VisualY { get { return IsMoving ? _fromY + (Y - _fromY) * (_movementTicks / (double)StepDuration) : Y; } }

        public string KindName { get { return Kind == G1C_PetKind.Cat ? "Katze" : "Hund"; } }

        public override string SpriteId
        {
            get
            {
                int frame = IsMoving ? 1 + (_movementTicks / 3) % 2 : 0;
                return "pet_" + (Kind == G1C_PetKind.Cat ? "cat" : "dog") + "_" + Variant + "_" + (FacingLeft ? "l" : "r") + "_" + frame;
            }
        }

        public override bool Occupies(int tileX, int tileY)
        {
            return base.Occupies(tileX, tileY) || (IsMoving && tileX == _fromX && tileY == _fromY);
        }

        // Entwickler: Patrick Lupierz
        // Aufgabe: Einen Tick weiterlaufen. Im Stand wartet das Tier eine zufällige Zeit und versucht dann einen
        //          Schritt auf eine freie Nachbarkachel innerhalb seines Reviers. isFree entscheidet die Spiellogik
        //          (Kollision, Spieler, Türbereiche) – das Tier kennt die Welt nicht.
        public void Update(int tick, Func<int, int, bool> isFree, Random random)
        {
            LastTick = tick;
            if (IsMoving) { _movementTicks++; return; }
            if (RoamRadius == 0 || isFree == null || random == null) return;
            if (_restTicks > 0) { _restTicks--; return; }

            _restTicks = 25 + random.Next(75);
            int direction = random.Next(4);
            int dx = direction == 0 ? 1 : direction == 1 ? -1 : 0;
            int dy = direction == 2 ? 1 : direction == 3 ? -1 : 0;
            int tx = X + dx, ty = Y + dy;
            if (Math.Abs(tx - HomeX) > RoamRadius || Math.Abs(ty - HomeY) > RoamRadius || !isFree(tx, ty)) return;
            if (dx != 0) FacingLeft = dx < 0;
            _fromX = X;
            _fromY = Y;
            SetPosition(tx, ty);
            _movementTicks = 0;
        }

        // Entwickler: Patrick Lupierz – das Tier dreht sich zum Spieler und wird gestreichelt
        public string Pet(G1C_Player player)
        {
            PetCount++;
            if (player != null && player.TileX != X) FacingLeft = player.TileX < X;
            if (Kind == G1C_PetKind.Cat)
                return PetCount == 1 ? "Du streichelst " + Name + ". Sie schnurrt und drückt den Kopf an deine Hand."
                                     : Name + " schnurrt wieder. Sie mag dich (" + PetCount + "x gestreichelt).";
            return PetCount == 1 ? "Du streichelst " + Name + ". Der Schwanz wedelt wie verrückt!"
                                 : Name + " wedelt und will mehr (" + PetCount + "x gestreichelt).";
        }
    }

    // =====================================================
    // Klasse: G1C_AnimalVoices
    // Entwickler: Patrick Lupierz
    // Erstellt: 16.11.2026
    // Beschreibung: Sprechblasen der Tiere mit ihren typischen Lauten (Miau, Wuff, Quak, Tschilp, Blubb).
    //               Ab und zu meldet sich ein Tier von selbst; beim Streicheln erscheint ein Herz.
    //               Reine Logik – gezeichnet wird im G1C_CasinoPainter.
    // =====================================================
    public sealed class G1C_AnimalVoices
    {
        public const int BubbleTicks = 50;
        public const int MaxVisible = 4;

        private readonly Dictionary<G1C_WorldObject, KeyValuePair<string, int>> _bubbles =
            new Dictionary<G1C_WorldObject, KeyValuePair<string, int>>();

        public IEnumerable<KeyValuePair<G1C_WorldObject, string>> Active
        {
            get { return _bubbles.Select(b => new KeyValuePair<G1C_WorldObject, string>(b.Key, b.Value.Key)); }
        }

        // Entwickler: Patrick Lupierz – typischer Laut je Tierart (auch für Denis' Enten, Vögel und Fische)
        public static string SoundOf(G1C_WorldObject animal, Random random)
        {
            int pick = random != null ? random.Next(3) : 0;
            G1C_PetAnimal pet = animal as G1C_PetAnimal;
            if (pet != null)
                return pet.Kind == G1C_PetKind.Cat ? new[] { "Miau!", "Mrrau?", "Miaaau~" }[pick]
                                                   : new[] { "Wuff!", "Wau wau!", "Wuff wuff!" }[pick];
            string sprite = animal != null ? animal.SpriteId ?? "" : "";
            if (sprite.StartsWith("animal_duck")) return new[] { "Quak!", "Quak quak!", "Quaaak?" }[pick];
            if (sprite.StartsWith("animal_bird")) return new[] { "Tschilp!", "Piep piep!", "Tirili!" }[pick];
            if (sprite.StartsWith("animal_fish")) return "Blubb";
            if (sprite.StartsWith("animal_cat")) return "Miau!";
            return null;
        }

        public void Say(G1C_WorldObject animal, string text, int tick)
        {
            if (animal == null || string.IsNullOrEmpty(text)) return;
            _bubbles[animal] = new KeyValuePair<string, int>(text, tick + BubbleTicks);
        }

        public string TextOf(G1C_WorldObject animal)
        {
            KeyValuePair<string, int> bubble;
            return animal != null && _bubbles.TryGetValue(animal, out bubble) ? bubble.Key : null;
        }

        // Entwickler: Patrick Lupierz
        // Aufgabe: Abgelaufene Blasen entfernen; mit kleiner Chance meldet sich ein Tier der Szene von selbst.
        public void Update(int tick, IEnumerable<G1C_WorldObject> animals, Random random)
        {
            foreach (G1C_WorldObject key in _bubbles.Where(b => b.Value.Value <= tick).Select(b => b.Key).ToList())
                _bubbles.Remove(key);
            if (random == null || animals == null || _bubbles.Count >= MaxVisible || random.Next(60) != 0) return;
            List<G1C_WorldObject> candidates = animals.Where(a => !_bubbles.ContainsKey(a) && SoundOf(a, null) != null).ToList();
            if (candidates.Count == 0) return;
            G1C_WorldObject animal = candidates[random.Next(candidates.Count)];
            Say(animal, SoundOf(animal, random), tick);
        }

        public void Clear()
        {
            _bubbles.Clear();
        }
    }
}
