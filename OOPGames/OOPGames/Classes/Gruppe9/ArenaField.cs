//P06.11.2026
using System;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    // Holds world dimensions, the player, and movement/collision behavior.
    public sealed class ArenaField : IGameField
    {
        // All world positions and sizes are measured in WPF device-independent units.
        public const double MapWidth = 1800;
        public const double MapHeight = 1200;
        public const double WallThickness = 32;
        public const double MinimumZoom = 0.5;
        public const double MaximumZoom = 2.0;
        private const double ZoomStep = 0.1;

        // The field owns the current player instance.
        public PlayerCharacter Player { get; private set; }
        public bool IsGameStarted { get; private set; }
        public double ZoomLevel { get; private set; } = 1.0;

        public ArenaField()
        {
            // Create the player in the middle when a new field is constructed.
            ResetPlayer();
        }

        public void ResetPlayer()
        {
            // Resetting starts the player at the center of the arena.
            Player = new PlayerCharacter(MapWidth / 2, MapHeight / 2);
        }

        public void ResetGame()
        {
            ResetPlayer();
            ZoomLevel = 1.0;
            IsGameStarted = false;
        }

        public void StartGame()
        {
            IsGameStarted = true;
        }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            // The arena field has a matching painter that knows its layout.
            return painter is ArenaPainter;
        }

        public void UpdatePlayerPosition()
        {
            double directionX = 0;
            double directionY = 0;

            // Read all pressed directions so horizontal and vertical input can combine.
            if (Keyboard.IsKeyDown(Key.W) || Keyboard.IsKeyDown(Key.Up)) directionY -= 1;
            if (Keyboard.IsKeyDown(Key.S) || Keyboard.IsKeyDown(Key.Down)) directionY += 1;
            if (Keyboard.IsKeyDown(Key.A) || Keyboard.IsKeyDown(Key.Left)) directionX -= 1;
            if (Keyboard.IsKeyDown(Key.D) || Keyboard.IsKeyDown(Key.Right)) directionX += 1;

            // Keep input polling here while delegating movement to the entity.
            Player.Move(directionX, directionY,
                new Rect(WallThickness, WallThickness,
                    MapWidth - 2 * WallThickness, MapHeight - 2 * WallThickness));
        }

        // Handle zoom from the routed key event so short key presses are not missed between ticks.
        public void HandleZoomKey(Key key)
        {
            if (key == Key.K)
            {
                ZoomLevel = Math.Min(MaximumZoom, ZoomLevel + ZoomStep);
            }
            else if (key == Key.L)
            {
                ZoomLevel = Math.Max(MinimumZoom, ZoomLevel - ZoomStep);
            }
        }
    }

    // Adapts the real-time arena to the framework's rules lifecycle.
    public sealed class ArenaRules : IGameRules2
    {
        private readonly ArenaField _field = new ArenaField();

        public string Name { get { return "Arena Survival"; } }
        public IGameField CurrentField { get { return _field; } }
        public bool MovesPossible { get { return true; } }

        public void DoMove(IPlayMove move)
        {
            // Movement is applied directly from keyboard input, not turn-based moves.
        }

        public void ClearField()
        {
            // Resetting the field also returns the arena to its start menu.
            _field.ResetGame();
        }

        public int CheckIfPLayerWon()
        {
            // This first arena iteration has no win condition.
            return -1;
        }

        public void StartedGameCall()
        {
            // A new game first waits for the player to choose Start Game.
            ClearField();
        }

        public void TickGameCall()
        {
            // Reserved for future per-frame game logic such as enemies or a timer.
            if (_field.IsGameStarted)
            {
                _field.UpdatePlayerPosition();
            }
        }
    }
}