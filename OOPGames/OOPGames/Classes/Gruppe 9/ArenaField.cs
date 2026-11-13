//P06.11.2026
using System;
using System.Windows;
using System.Windows.Input;

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
        public bool HasActiveGame { get; private set; }
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
            HasActiveGame = false;
        }

        public void StartNewGame()
        {
            ResetGame();
            StartGame();
        }

        public void StartGame()
        {
            IsGameStarted = true;
            HasActiveGame = true;
        }

        public bool ContinueGame()
        {
            if (!HasActiveGame)
            {
                return false;
            }

            StartGame();
            return true;
        }

        public void ReturnToStartMenu()
        {
            IsGameStarted = false;
        }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            // The arena field has a matching painter that knows its layout.
            return painter is PainterG9;
        }

        public void UpdatePlayerPosition()
        {
            double directionX = 0;
            double directionY = 0;

            if (Keyboard.IsKeyDown(Key.W) || Keyboard.IsKeyDown(Key.Up)) directionY -= 1;
            if (Keyboard.IsKeyDown(Key.S) || Keyboard.IsKeyDown(Key.Down)) directionY += 1;
            if (Keyboard.IsKeyDown(Key.A) || Keyboard.IsKeyDown(Key.Left)) directionX -= 1;
            if (Keyboard.IsKeyDown(Key.D) || Keyboard.IsKeyDown(Key.Right)) directionX += 1;

            Player.Move(directionX, directionY,
                new Rect(WallThickness, WallThickness,
                    MapWidth - 2 * WallThickness, MapHeight - 2 * WallThickness));
        }

        // Handle zoom from the routed key event so short key presses are not missed between ticks.
        public void HandleZoomKey(Key key)
        {
            if (key == Key.K)
            {
                ChangeZoom(1);
            }
            else if (key == Key.L)
            {
                ChangeZoom(-1);
            }
        }

        public void HandleZoomWheel(int delta)
        {
            if (delta != 0)
            {
                ChangeZoom(Math.Sign(delta));
            }
        }

        private void ChangeZoom(int direction)
        {
            ZoomLevel = Math.Max(MinimumZoom,
                Math.Min(MaximumZoom, ZoomLevel + direction * ZoomStep));
        }
    }
}