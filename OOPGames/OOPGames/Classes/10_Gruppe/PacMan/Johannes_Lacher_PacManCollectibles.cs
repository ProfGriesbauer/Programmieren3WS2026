namespace OOPGames
{
    public enum G10_CollectEffect
    {
        None,
        FrightenGhosts,
        Fruit
    }

    // What collecting an item was worth. The item itself does not change the game;
    // the session reads this result and reacts (score, frighten ghosts, ghost house counter).
    public struct G10_CollectResult
    {
        public static readonly G10_CollectResult Nothing = new G10_CollectResult(0, G10_CollectEffect.None);

        public G10_CollectResult(int points, G10_CollectEffect effect)
        {
            Points = points;
            Effect = effect;
        }

        public int Points { get; private set; }
        public G10_CollectEffect Effect { get; private set; }
        public bool IsNothing { get { return Points == 0 && Effect == G10_CollectEffect.None; } }
    }

    public abstract class G10_Collectible
    {
        public abstract int Points { get; }
        public abstract G10_CollectResult Collect();
    }

    // Items stored in the maze grid. They have no state of their own, so all tiles share
    // one instance per kind (flyweight).
    public abstract class G10_GridCollectible : G10_Collectible
    {
        public abstract G10_CollectibleKind Kind { get; }
    }

    public sealed class G10_Dot : G10_GridCollectible
    {
        public static readonly G10_Dot Instance = new G10_Dot();

        G10_Dot() { }

        public override int Points { get { return 10; } }
        public override G10_CollectibleKind Kind { get { return G10_CollectibleKind.Dot; } }

        public override G10_CollectResult Collect()
        {
            return new G10_CollectResult(Points, G10_CollectEffect.None);
        }
    }

    public sealed class G10_PowerPellet : G10_GridCollectible
    {
        public static readonly G10_PowerPellet Instance = new G10_PowerPellet();

        G10_PowerPellet() { }

        public override int Points { get { return 50; } }
        public override G10_CollectibleKind Kind { get { return G10_CollectibleKind.PowerPellet; } }

        public override G10_CollectResult Collect()
        {
            return new G10_CollectResult(Points, G10_CollectEffect.FrightenGhosts);
        }
    }

    // Free object below the ghost house that disappears after a while.
    public sealed class G10_BonusFruit : G10_Collectible, IG10_FruitView
    {
        // Pac-Man picks the fruit up when his centre is this close to it on the same row.
        const int PickupDistance = 4;

        readonly int _Points;
        int _RemainingTicks;

        public G10_BonusFruit(G10_FruitKind kind, int points, G10_PixelPoint position, int lifetimeTicks)
        {
            Kind = kind;
            _Points = points;
            PixelX = position.X;
            PixelY = position.Y;
            _RemainingTicks = lifetimeTicks;
        }

        public G10_FruitKind Kind { get; private set; }
        public int PixelX { get; private set; }
        public int PixelY { get; private set; }
        public override int Points { get { return _Points; } }
        public bool IsExpired { get { return _RemainingTicks <= 0; } }

        public void Tick()
        {
            if (_RemainingTicks > 0) _RemainingTicks--;
        }

        public bool IsTouchedBy(G10_MovingEntity entity)
        {
            return entity.PixelY == PixelY && System.Math.Abs(entity.PixelX - PixelX) <= PickupDistance;
        }

        public override G10_CollectResult Collect()
        {
            _RemainingTicks = 0;
            return new G10_CollectResult(Points, G10_CollectEffect.Fruit);
        }
    }
}
