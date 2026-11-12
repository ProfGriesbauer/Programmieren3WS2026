namespace OOPGames
{
    /// <summary>Was passiert, wenn der Spieler mit [E] interagiert.</summary>
    public enum G1C_InteractionType { None, Talk, Bartender, Roulette, Blackjack, Poker, Slots, Trash, Shop, Wardrobe, Bed }

    /// <summary>
    /// Ein Objekt in einer Welt-Szene (Tisch, Automat, Gebäude, Pflanze, ...).
    /// Belegt ein Rechteck aus Kacheln und kann massiv (nicht begehbar) sein.
    /// </summary>
    // Änderung durch Denis Mock: Weltobjekte werden auch in Stadt und Zuhause verwendet.
    public class G1C_WorldObject
    {
        private readonly string _spriteId;

        public string Name { get; }
        public int X { get; private set; }
        public int Y { get; private set; }
        public int Width { get; }
        public int Height { get; }
        public bool Solid { get; }
        public G1C_InteractionType Interaction { get; }
        public string[] Lines { get; }

        public G1C_WorldObject(string name, string spriteId, int x, int y, int width, int height,
                               bool solid, G1C_InteractionType interaction, params string[] lines)
        {
            Name = name;
            _spriteId = spriteId;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            Solid = solid;
            Interaction = interaction;
            Lines = lines ?? new string[0];
        }

        public virtual string SpriteId { get { return _spriteId; } }

        // Erweiterung durch Denis Mock: Bewegliche NPCs aktualisieren ihre logische Position.
        protected void SetPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public virtual bool Occupies(int tileX, int tileY)
        {
            return tileX >= X && tileX < X + Width && tileY >= Y && tileY < Y + Height;
        }

        /// <summary>Wird aufgerufen, bevor die Interaktion startet.</summary>
        public virtual void OnInteract(G1C_Player player)
        {
        }
    }

    /// <summary>Eine Nicht-Spieler-Figur (Croupier, Barkeeper, Gast).</summary>
    public class G1C_Npc : G1C_WorldObject
    {
        private int _fromX;
        private int _fromY;
        private int _movementTicks;
        private int _movementDuration;
        private int _stepCount;

        public string Skin { get; }
        public G1C_Direction Facing { get; private set; }
        public bool IsMoving { get { return _movementTicks < _movementDuration; } }
        public double VisualX
        {
            get { return IsMoving ? _fromX + (X - _fromX) * (_movementTicks / (double)_movementDuration) : X; }
        }
        public double VisualY
        {
            get { return IsMoving ? _fromY + (Y - _fromY) * (_movementTicks / (double)_movementDuration) : Y; }
        }

        public G1C_Npc(string name, string skin, int x, int y, G1C_Direction facing,
                       G1C_InteractionType interaction, params string[] lines)
            : base(name, null, x, y, 1, 1, true, interaction, lines)
        {
            Skin = skin;
            Facing = facing;
        }

        public override string SpriteId
        {
            get
            {
                int frame = IsMoving ? 1 + (_movementTicks / 3 + _stepCount) % 2 : 0;
                return Skin + "_" + G1C_DirectionHelper.ToSpriteName(Facing) + "_" + frame;
            }
        }

        // Entwickler: Denis Mock
        // Startet eine interpolierte Bewegung, nachdem die Patrouillenlogik das Ziel geprüft hat.
        public void BeginMoveTo(int x, int y, G1C_Direction facing, int durationTicks)
        {
            if (IsMoving) throw new System.InvalidOperationException("Ein bewegter NPC kann keinen neuen Schritt beginnen.");
            if (durationTicks <= 0) throw new System.ArgumentOutOfRangeException(nameof(durationTicks));
            if (System.Math.Abs(x - X) + System.Math.Abs(y - Y) != 1)
                throw new System.ArgumentException("Ein NPC-Schritt muss genau eine benachbarte Kachel sein.");

            _fromX = X;
            _fromY = Y;
            SetPosition(x, y);
            Facing = facing;
            _movementTicks = 0;
            _movementDuration = durationTicks;
            _stepCount++;
        }

        // Entwickler: Denis Mock
        public void AdvanceMovement()
        {
            if (IsMoving) _movementTicks++;
        }

        // Entwickler: Denis Mock
        public override bool Occupies(int tileX, int tileY)
        {
            return base.Occupies(tileX, tileY) ||
                   (IsMoving && tileX == _fromX && tileY == _fromY);
        }

        /// <summary>Der NPC dreht sich zum Spieler, wenn er angesprochen wird.</summary>
        public override void OnInteract(G1C_Player player)
        {
            Facing = G1C_DirectionHelper.Opposite(player.Facing);
        }
    }
}
