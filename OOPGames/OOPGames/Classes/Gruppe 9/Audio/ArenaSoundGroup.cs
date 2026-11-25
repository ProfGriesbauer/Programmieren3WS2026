using System;
using System.Collections.Generic;

namespace OOPGames
{
    // Plays a random variant of one sound event. The same variant is not repeated twice in a
    // row, and events closer together than the minimum interval are merged so many enemies
    // hitting in the same moment do not stack into one very loud sound.
    public sealed class ArenaSoundGroup
    {
        private readonly IReadOnlyList<IArenaSoundEffect> _variants;
        private readonly double _minimumIntervalSeconds;
        private readonly Random _random;
        private int _lastVariant = -1;
        private double _lastPlaySeconds = double.NegativeInfinity;

        public ArenaSoundGroup(
            IReadOnlyList<IArenaSoundEffect> variants, double minimumIntervalSeconds,
            Random random = null)
        {
            if (variants == null)
            {
                throw new ArgumentNullException(nameof(variants));
            }
            if (double.IsNaN(minimumIntervalSeconds) || minimumIntervalSeconds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumIntervalSeconds));
            }

            _variants = variants;
            _minimumIntervalSeconds = minimumIntervalSeconds;
            _random = random ?? new Random();
        }

        public int Count { get { return _variants.Count; } }

        // Returns the played variant index, or -1 when nothing was played. A forced play
        // ignores the minimum interval (e.g. the lethal hit, which must always be heard).
        public int Play(double nowSeconds, bool force = false)
        {
            if (_variants.Count == 0
                || (!force && nowSeconds - _lastPlaySeconds < _minimumIntervalSeconds))
            {
                return -1;
            }

            int variant = _random.Next(_variants.Count);
            if (_variants.Count > 1 && variant == _lastVariant)
            {
                // Shift to another variant while keeping the choice evenly distributed.
                variant = (variant + 1 + _random.Next(_variants.Count - 1)) % _variants.Count;
            }

            _lastVariant = variant;
            _lastPlaySeconds = nowSeconds;
            _variants[variant].Play();
            return variant;
        }
    }
}
