using System;

namespace OOPGames
{
    // The child collecting the coins. Moves one tile per key press; holding the key keeps
    // walking after a short delay.
    public sealed class G10_CoinPlayer : IG10_CoinPlayerView
    {
        const int RepeatDelay = 6;
        const int RepeatInterval = 3;
        int _RepeatTicks;
        int _InvulnerableTicks;

        public G10_Tile Tile { get; private set; }
        public G10_Direction Facing { get; private set; }
        public int Steps { get; private set; }
        public bool IsInvulnerable { get { return _InvulnerableTicks > 0; } }

        public void Respawn(G10_Tile tile, bool invulnerable)
        {
            Tile = tile;
            Facing = G10_Direction.Down;
            _RepeatTicks = 0;
            _InvulnerableTicks = invulnerable ? G10_CoinConfig.InvulnerableTicks : 0;
        }

        // Returns true when the player moved to another tile.
        public bool Update(G10_CoinInput input, G10_CoinArena arena)
        {
            if (_InvulnerableTicks > 0) _InvulnerableTicks--;
            if (input.Pressed != G10_Direction.None)
            {
                _RepeatTicks = RepeatDelay;
                return TryMove(input.Pressed, arena);
            }
            if (input.Held == G10_Direction.None) return false;
            if (--_RepeatTicks > 0) return false;
            _RepeatTicks = RepeatInterval;
            return TryMove(input.Held, arena);
        }

        bool TryMove(G10_Direction direction, G10_CoinArena arena)
        {
            Facing = direction;
            G10_Tile next = Tile.Neighbour(direction);
            if (arena.IsBlocked(next)) return false;
            Tile = next;
            Steps++;
            return true;
        }
    }

    // Wanders diagonally through the room and bounces off walls like a ball. Now and then it
    // sidesteps to a neighbour tile and takes a new direction: diagonal steps alone never
    // change the colour of the tile on a chessboard, so it could only ever reach half of the
    // floor, and it could run the same loop in one corner forever.
    public sealed class G10_DustBunny
    {
        const int TurnChance = 10;
        readonly Random _Random;
        int _DeltaX;
        int _DeltaY;
        int _Wait;

        public G10_DustBunny(G10_Tile tile, Random random)
        {
            Tile = tile;
            _Random = random;
            _DeltaX = random.Next(2) == 0 ? -1 : 1;
            _DeltaY = random.Next(2) == 0 ? -1 : 1;
        }

        public G10_Tile Tile { get; private set; }

        public void Update(G10_CoinArena arena, int stepTicks)
        {
            if (++_Wait < stepTicks) return;
            _Wait = 0;
            if (_Random.Next(TurnChance) == 0 && TrySidestep(arena)) return;

            // Mirror the direction at the walls it runs into.
            if (arena.IsBlocked(Tile.Offset(_DeltaX, 0))) _DeltaX = -_DeltaX;
            if (arena.IsBlocked(Tile.Offset(0, _DeltaY))) _DeltaY = -_DeltaY;
            if (TryMove(arena, _DeltaX, _DeltaY)) return;

            // It hit the corner of a block. Turning straight back would let it shuttle between
            // two blocks forever, so it glances off one side at random and only turns back if
            // both sides are closed.
            bool mirrorX = _Random.Next(2) == 0;
            if (TryMove(arena, mirrorX ? -_DeltaX : _DeltaX, mirrorX ? _DeltaY : -_DeltaY)) return;
            if (TryMove(arena, mirrorX ? _DeltaX : -_DeltaX, mirrorX ? -_DeltaY : _DeltaY)) return;
            TryMove(arena, -_DeltaX, -_DeltaY);
        }

        bool TrySidestep(G10_CoinArena arena)
        {
            _DeltaX = _Random.Next(2) == 0 ? -1 : 1;
            _DeltaY = _Random.Next(2) == 0 ? -1 : 1;
            G10_Tile next = _Random.Next(2) == 0 ? Tile.Offset(_DeltaX, 0) : Tile.Offset(0, _DeltaY);
            if (arena.IsBlocked(next)) return false;
            Tile = next;
            return true;
        }

        bool TryMove(G10_CoinArena arena, int deltaX, int deltaY)
        {
            G10_Tile next = Tile.Offset(deltaX, deltaY);
            if (arena.IsBlocked(next)) return false;
            _DeltaX = deltaX;
            _DeltaY = deltaY;
            Tile = next;
            return true;
        }
    }

    public sealed class G10_CoinPickup : IG10_PickupView
    {
        public G10_CoinPickup(G10_Tile tile, G10_PickupKind kind)
        {
            Tile = tile;
            Kind = kind;
            TicksLeft = kind == G10_PickupKind.Diamond ? G10_CoinConfig.DiamondLifetime : 0;
        }

        public G10_Tile Tile { get; private set; }
        public G10_PickupKind Kind { get; private set; }
        public int TicksLeft { get; private set; }
        public bool IsGone { get { return Kind == G10_PickupKind.Diamond && TicksLeft <= 0; } }
        public int Points { get { return Kind == G10_PickupKind.Coin ? G10_CoinConfig.CoinPoints : G10_CoinConfig.DiamondPoints; } }

        public void Tick()
        {
            if (Kind == G10_PickupKind.Diamond) TicksLeft--;
        }
    }
}
