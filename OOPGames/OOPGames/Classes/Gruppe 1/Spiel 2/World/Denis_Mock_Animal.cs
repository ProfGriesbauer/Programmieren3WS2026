namespace OOPGames
{
    // Entwickler: Denis Mock
    // Nicht kollidierende Umgebungsfigur; Einzelbildwechsel laufen ausschlie\u00dflich im Framework-Tick.
    public sealed class G1C_Animal : G1C_WorldObject
    {
        private readonly string _spritePrefix;
        private readonly int _frameCount;
        private readonly int _ticksPerFrame;
        private int _frame;

        // Entwickler: Denis Mock
        public G1C_Animal(string name, string spritePrefix, int x, int y, int frameCount, int ticksPerFrame)
            : base(name, null, x, y, 1, 1, false, G1C_InteractionType.None)
        {
            if (string.IsNullOrEmpty(spritePrefix)) throw new System.ArgumentException("Sprite-ID fehlt.", nameof(spritePrefix));
            if (frameCount < 1) throw new System.ArgumentOutOfRangeException(nameof(frameCount));
            if (ticksPerFrame < 1) throw new System.ArgumentOutOfRangeException(nameof(ticksPerFrame));

            _spritePrefix = spritePrefix;
            _frameCount = frameCount;
            _ticksPerFrame = ticksPerFrame;
        }

        public override string SpriteId { get { return _spritePrefix + "_" + _frame; } }

        // Entwickler: Denis Mock
        public void UpdateAnimation(int tick)
        {
            _frame = tick / _ticksPerFrame % _frameCount;
        }
    }
}
