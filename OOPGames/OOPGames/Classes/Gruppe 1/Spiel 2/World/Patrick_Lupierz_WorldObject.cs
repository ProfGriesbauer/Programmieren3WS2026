namespace OOPGames
{
    /// <summary>Was passiert, wenn der Spieler mit [E] interagiert.</summary>
    public enum G1C_InteractionType { None, Talk, Bartender, Roulette, Blackjack, Poker, Slots }

    /// <summary>
    /// Ein Objekt in einer Welt-Szene (Tisch, Automat, Gebäude, Pflanze, ...).
    /// Belegt ein Rechteck aus Kacheln und kann massiv (nicht begehbar) sein.
    /// </summary>
    // Änderung durch Denis Mock: Weltobjekte werden auch in Stadt und Zuhause verwendet.
    public class G1C_WorldObject
    {
        private readonly string _spriteId;

        public string Name { get; }
        public int X { get; }
        public int Y { get; }
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

        public bool Occupies(int tileX, int tileY)
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
        public string Skin { get; }
        public G1C_Direction Facing { get; private set; }

        public G1C_Npc(string name, string skin, int x, int y, G1C_Direction facing,
                       G1C_InteractionType interaction, params string[] lines)
            : base(name, null, x, y, 1, 1, true, interaction, lines)
        {
            Skin = skin;
            Facing = facing;
        }

        public override string SpriteId
        {
            get { return G1C_PlayerAnimation.GetStandingSpriteId(Skin, Facing); }
        }

        /// <summary>Der NPC dreht sich zum Spieler, wenn er angesprochen wird.</summary>
        public override void OnInteract(G1C_Player player)
        {
            Facing = G1C_DirectionHelper.Opposite(player.Facing);
        }
    }
}
