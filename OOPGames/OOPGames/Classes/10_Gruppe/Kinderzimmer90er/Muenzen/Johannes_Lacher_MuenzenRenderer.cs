using System.Windows.Controls;
using System.Windows.Media;

namespace OOPGames
{
    // Draws "Muenzen sammeln" with the colours of the Commodore 64.
    public sealed class G10_CoinRenderer
    {
        public const int ScreenWidth = 160;
        public const int ScreenHeight = 120;
        const int Cell = 8;
        const int FieldX = 4;
        const int FieldY = 13;
        const double Center = ScreenWidth / 2.0;
        const int WarningSeconds = 10;
        // A diamond blinks during its last one and a half seconds.
        const int DiamondWarningTicks = 38;

        static readonly Color Black = Color.FromRgb(0, 0, 0);
        static readonly Color White = Color.FromRgb(255, 255, 255);
        static readonly Color Blue = Color.FromRgb(53, 40, 121);
        static readonly Color LightBlue = Color.FromRgb(124, 112, 218);
        static readonly Color Red = Color.FromRgb(129, 51, 56);
        static readonly Color LightRed = Color.FromRgb(196, 108, 113);
        static readonly Color Green = Color.FromRgb(86, 172, 77);
        static readonly Color Yellow = Color.FromRgb(237, 241, 113);
        static readonly Color Orange = Color.FromRgb(142, 80, 41);
        static readonly Color Brown = Color.FromRgb(85, 56, 0);
        static readonly Color Cyan = Color.FromRgb(117, 206, 200);
        static readonly Color Purple = Color.FromRgb(142, 60, 151);
        static readonly Color DarkGrey = Color.FromRgb(74, 74, 74);
        static readonly Color Grey = Color.FromRgb(123, 123, 123);
        static readonly Color LightGrey = Color.FromRgb(178, 178, 178);
        // Toy blocks get their colour from their position.
        static readonly Color[] BlockColors = { Red, Green, Purple, Orange };

        public void Draw(Canvas canvas, IG10_CoinView game)
        {
            canvas.Children.Clear();
            B5RoomDraw.Rect(canvas, 0, 0, ScreenWidth, ScreenHeight, Blue);
            DrawHud(canvas, game);
            B5RoomDraw.Rect(canvas, FieldX - 1, FieldY - 1, game.Columns * Cell + 2, game.Rows * Cell + 2, LightBlue);
            B5RoomDraw.Rect(canvas, FieldX, FieldY, game.Columns * Cell, game.Rows * Cell, Blue);

            for (int row = 0; row < game.Rows; row++)
                for (int col = 0; col < game.Columns; col++)
                    if (game.IsWall(new G10_Tile(col, row))) DrawBlock(canvas, col, row);

            foreach (IG10_PickupView pickup in game.Pickups)
            {
                if (pickup.Kind == G10_PickupKind.Coin) DrawCoin(canvas, pickup.Tile, game.PhaseTicks);
                else if (pickup.TicksLeft > DiamondWarningTicks || pickup.TicksLeft / 3 % 2 == 0) DrawDiamond(canvas, pickup.Tile, game.PhaseTicks);
            }
            for (int i = 0; i < game.DustBunnies.Count; i++)
                DrawDustBunny(canvas, game.DustBunnies[i], game.PhaseTicks + i * 3);
            if (IsPlayerVisible(game)) DrawPlayer(canvas, game.Player);

            DrawOverlay(canvas, game);
        }

        void DrawHud(Canvas canvas, IG10_CoinView game)
        {
            B5RoomDraw.Text(canvas, "SC " + game.Score.ToString("D6"), 3, 1, 7, LightBlue);
            DrawCoinIcon(canvas, 44, 3);
            B5RoomDraw.Text(canvas, game.CoinsCollected.ToString("D2") + "/" + game.CoinsNeeded.ToString("D2"), 51, 1, 7, White);
            B5RoomDraw.Text(canvas, "LV" + game.Level, 76, 1, 7, LightBlue);

            int seconds = (game.TicksLeft + G10_CoinConfig.TicksPerSecond - 1) / G10_CoinConfig.TicksPerSecond;
            bool hurry = seconds <= WarningSeconds && game.Phase == G10_CoinPhase.Playing;
            if (!hurry || game.PhaseTicks / 6 % 2 == 0)
                B5RoomDraw.Text(canvas, "TIME " + seconds.ToString("D2"), 95, 1, 7, hurry ? LightRed : White);

            for (int i = 0; i < game.Lives; i++) DrawHeart(canvas, 132 + i * 8, 3);
        }

        bool IsPlayerVisible(IG10_CoinView game)
        {
            if (game.Phase == G10_CoinPhase.Hit) return game.PhaseTicks / 4 % 2 == 0;
            if (game.Phase == G10_CoinPhase.GameOver) return false;
            return !game.Player.IsInvulnerable || game.PhaseTicks / 3 % 2 == 0;
        }

        void DrawBlock(Canvas canvas, int col, int row)
        {
            double x = FieldX + col * Cell;
            double y = FieldY + row * Cell;
            Color color = BlockColors[(col * 3 + row) % BlockColors.Length];
            B5RoomDraw.Rect(canvas, x, y, Cell, Cell, Black);
            B5RoomDraw.Rect(canvas, x, y, Cell - 1, Cell - 1, color);
            B5RoomDraw.Rect(canvas, x, y, Cell - 1, 1, Lighten(color));
            B5RoomDraw.Rect(canvas, x, y, 1, Cell - 1, Lighten(color));
            B5RoomDraw.Rect(canvas, x + 3, y + 3, 1, 1, Lighten(color));
        }

        // The coin spins: it gets narrower and wider again.
        void DrawCoin(Canvas canvas, G10_Tile tile, int ticks)
        {
            int[] widths = { 6, 4, 2, 4 };
            int width = widths[(ticks / 4 + tile.Col + tile.Row) % widths.Length];
            double x = FieldX + tile.Col * Cell + (Cell - width) / 2.0;
            double y = FieldY + tile.Row * Cell;
            B5RoomDraw.Rect(canvas, x, y + 2, width, 4, Yellow);
            if (width > 2) B5RoomDraw.Rect(canvas, x + 1, y + 1, width - 2, 6, Yellow);
            if (width < 4) return;
            B5RoomDraw.Rect(canvas, x + width - 1, y + 2, 1, 4, Orange);
            B5RoomDraw.Rect(canvas, x + 1, y + 2, 1, 1, White);
        }

        void DrawDiamond(Canvas canvas, G10_Tile tile, int ticks)
        {
            double x = FieldX + tile.Col * Cell;
            double y = FieldY + tile.Row * Cell;
            B5RoomDraw.Rect(canvas, x + 1, y + 2, 6, 2, Cyan);
            B5RoomDraw.Rect(canvas, x + 2, y + 4, 4, 1, Cyan);
            B5RoomDraw.Rect(canvas, x + 3, y + 5, 2, 1, Cyan);
            B5RoomDraw.Rect(canvas, x + 2, y + 1, 4, 1, White);
            if (ticks / 6 % 3 == 0) B5RoomDraw.Rect(canvas, x + 6, y, 1, 1, White);
        }

        // A fluffy grey dust bunny whose tufts wobble.
        void DrawDustBunny(Canvas canvas, G10_Tile tile, int ticks)
        {
            double x = FieldX + tile.Col * Cell;
            double y = FieldY + tile.Row * Cell;
            bool wobble = ticks / 5 % 2 == 0;
            B5RoomDraw.Rect(canvas, x + 1, y + 2, 6, 5, LightGrey);
            B5RoomDraw.Rect(canvas, x + 2, y + 1, 4, 7, LightGrey);
            B5RoomDraw.Rect(canvas, x + 2, y + 6, 4, 1, Grey);
            B5RoomDraw.Rect(canvas, wobble ? x : x + 7, y + 3, 1, 2, LightGrey);
            B5RoomDraw.Rect(canvas, wobble ? x + 5 : x + 2, y, 1, 1, LightGrey);
            B5RoomDraw.Rect(canvas, x + 2, y + 3, 1, 1, Black);
            B5RoomDraw.Rect(canvas, x + 5, y + 3, 1, 1, Black);
        }

        // A child with a red cap.
        void DrawPlayer(Canvas canvas, IG10_CoinPlayerView player)
        {
            double x = FieldX + player.Tile.Col * Cell;
            double y = FieldY + player.Tile.Row * Cell;
            G10_Direction facing = player.Facing;
            B5RoomDraw.Rect(canvas, x + 2, y, 4, 2, Red);
            if (facing == G10_Direction.Right) B5RoomDraw.Rect(canvas, x + 6, y + 1, 2, 1, Red);
            else if (facing == G10_Direction.Left) B5RoomDraw.Rect(canvas, x, y + 1, 2, 1, Red);

            B5RoomDraw.Rect(canvas, x + 2, y + 2, 4, 2, facing == G10_Direction.Up ? Brown : LightRed);
            if (facing == G10_Direction.Down)
            {
                B5RoomDraw.Rect(canvas, x + 2.5, y + 2, 1, 1, Black);
                B5RoomDraw.Rect(canvas, x + 4.5, y + 2, 1, 1, Black);
            }
            else if (facing == G10_Direction.Left) B5RoomDraw.Rect(canvas, x + 2, y + 2, 1, 1, Black);
            else if (facing == G10_Direction.Right) B5RoomDraw.Rect(canvas, x + 5, y + 2, 1, 1, Black);

            B5RoomDraw.Rect(canvas, x + 1, y + 4, 6, 2, Green);
            bool step = player.Steps % 2 == 0;
            B5RoomDraw.Rect(canvas, x + 2, y + 6, 1, step ? 2 : 1, DarkGrey);
            B5RoomDraw.Rect(canvas, x + 5, y + 6, 1, step ? 1 : 2, DarkGrey);
        }

        void DrawCoinIcon(Canvas canvas, double x, double y)
        {
            B5RoomDraw.Rect(canvas, x, y + 1, 5, 3, Yellow);
            B5RoomDraw.Rect(canvas, x + 1, y, 3, 5, Yellow);
            B5RoomDraw.Rect(canvas, x + 4, y + 1, 1, 3, Orange);
        }

        void DrawHeart(Canvas canvas, double x, double y)
        {
            B5RoomDraw.Rect(canvas, x, y + 1, 5, 2, LightRed);
            B5RoomDraw.Rect(canvas, x + 0.5, y, 1.5, 1, LightRed);
            B5RoomDraw.Rect(canvas, x + 3, y, 1.5, 1, LightRed);
            B5RoomDraw.Rect(canvas, x + 1, y + 3, 3, 1, LightRed);
            B5RoomDraw.Rect(canvas, x + 2, y + 4, 1, 1, LightRed);
        }

        void DrawOverlay(Canvas canvas, IG10_CoinView game)
        {
            switch (game.Phase)
            {
                case G10_CoinPhase.Ready:
                    Panel(canvas, 34, 46);
                    B5RoomDraw.Text(canvas, "READY.", 22, 36, 7, LightBlue);
                    B5RoomDraw.TextCentered(canvas, "LEVEL " + game.Level, Center, 44, 11, White);
                    B5RoomDraw.TextCentered(canvas, "COLLECT " + game.CoinsNeeded + " COINS", Center, 58, 7, Yellow);
                    B5RoomDraw.TextCentered(canvas, "MIND THE DUST BUNNIES", Center, 67, 7, LightGrey);
                    break;
                case G10_CoinPhase.Paused:
                    Panel(canvas, 44, 28);
                    B5RoomDraw.TextCentered(canvas, "PAUSE", Center, 46, 11, White);
                    B5RoomDraw.TextCentered(canvas, "ENTER TO GO ON", Center, 60, 7, LightBlue);
                    break;
                case G10_CoinPhase.Hit:
                    Panel(canvas, 48, 20);
                    B5RoomDraw.TextCentered(canvas, game.TimeRanOut ? "TIME UP!" : "OUCH!", Center, 50, 11, LightRed);
                    break;
                case G10_CoinPhase.LevelCleared:
                    Panel(canvas, 40, 34);
                    B5RoomDraw.TextCentered(canvas, "LEVEL CLEAR!", Center, 42, 11, Yellow);
                    B5RoomDraw.TextCentered(canvas, "TIME BONUS +" + game.TimeBonus, Center, 59, 7, White);
                    break;
                case G10_CoinPhase.GameOver:
                    Panel(canvas, 30, 58);
                    B5RoomDraw.TextCentered(canvas, "GAME OVER", Center, 32, 11, LightRed);
                    if (game.TimeRanOut) B5RoomDraw.TextCentered(canvas, "THE TIME RAN OUT", Center, 46, 7, LightGrey);
                    B5RoomDraw.TextCentered(canvas, "SCORE " + game.Score + "  LEVEL " + game.Level, Center, 56, 7, White);
                    if (game.IsNewHighScore) B5RoomDraw.TextCentered(canvas, "NEW HIGHSCORE!", Center, 65, 7, Yellow);
                    else B5RoomDraw.TextCentered(canvas, "HI " + game.HighScore, Center, 65, 7, LightBlue);
                    if (game.PhaseTicks / 10 % 2 == 0)
                        B5RoomDraw.TextCentered(canvas, "ENTER: NEW GAME", Center, 76, 7, White);
                    break;
            }
        }

        void Panel(Canvas canvas, double y, double height)
        {
            B5RoomDraw.Rect(canvas, 18, y, ScreenWidth - 36, height, LightBlue);
            B5RoomDraw.Rect(canvas, 19, y + 1, ScreenWidth - 38, height - 2, Blue);
        }

        static Color Lighten(Color color)
        {
            return Color.FromRgb((byte)((color.R + 255) / 2), (byte)((color.G + 255) / 2), (byte)((color.B + 255) / 2));
        }
    }
}
