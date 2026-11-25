using System.Windows.Controls;
using System.Windows.Media;

namespace OOPGames
{
    // Draws a Pong match in the four green shades of the original Game Boy.
    public sealed class G10_PongRenderer
    {
        public const int ScreenWidth = 160;
        public const int ScreenHeight = 144;
        const double Center = ScreenWidth / 2.0;

        static readonly Color Lightest = G10_GameBoyColors.Lightest;
        static readonly Color Light = G10_GameBoyColors.Light;
        static readonly Color Dark = G10_GameBoyColors.Dark;
        static readonly Color Darkest = G10_GameBoyColors.Darkest;

        public void Draw(Canvas canvas, IG10_PongView game)
        {
            canvas.Children.Clear();
            G10_RoomDraw.Rect(canvas, 0, 0, ScreenWidth, ScreenHeight, Light);
            if (game.Phase != G10_PongPhase.Menu) DrawHud(canvas, game);
            DrawField(canvas);
            DrawPaddle(canvas, G10_PongConfig.LeftPaddleX, game.LeftPaddleY);
            DrawPaddle(canvas, G10_PongConfig.RightPaddleX, game.RightPaddleY);
            DrawBall(canvas, game);
            DrawOverlay(canvas, game);
        }

        void DrawHud(Canvas canvas, IG10_PongView game)
        {
            G10_RoomDraw.Text(canvas, LeftName(game), 4, 4, 7, Dark);
            string right = RightName(game);
            G10_RoomDraw.Text(canvas, right, ScreenWidth - 4 - right.Length * 7 * 0.55, 4, 7, Dark);
            G10_RoomDraw.TextCentered(canvas, game.LeftScore.ToString(), Center - 14, 0, 13, Darkest);
            G10_RoomDraw.TextCentered(canvas, ":", Center, 0, 13, Dark);
            G10_RoomDraw.TextCentered(canvas, game.RightScore.ToString(), Center + 14, 0, 13, Darkest);
        }

        void DrawField(Canvas canvas)
        {
            double top = G10_PongConfig.Top;
            double bottom = G10_PongConfig.Bottom;
            G10_RoomDraw.Rect(canvas, 0, top - 2, ScreenWidth, 2, Darkest);
            G10_RoomDraw.Rect(canvas, 0, top, ScreenWidth, bottom - top, Lightest);
            G10_RoomDraw.Rect(canvas, 0, bottom, ScreenWidth, 2, Darkest);
            for (double y = top + 2; y < bottom - 2; y += 8)
                G10_RoomDraw.Rect(canvas, Center - 1, y, 2, 4, Light);
        }

        void DrawPaddle(Canvas canvas, double x, double y)
        {
            G10_RoomDraw.Rect(canvas, x, y, G10_PongConfig.PaddleWidth, G10_PongConfig.PaddleHeight, Darkest);
            G10_RoomDraw.Rect(canvas, x + 1, y + 2, 1, G10_PongConfig.PaddleHeight - 4, Dark);
        }

        void DrawBall(Canvas canvas, IG10_PongView game)
        {
            if (game.Phase == G10_PongPhase.Menu || game.Phase == G10_PongPhase.GameOver) return;
            if (game.Phase == G10_PongPhase.Serving && game.PhaseTicks / 5 % 2 == 1) return;

            double x = game.BallX;
            double y = game.BallY;
            if (game.Phase == G10_PongPhase.Playing)
            {
                // A short trail shows where the ball is going.
                G10_RoomDraw.Rect(canvas, x - game.BallVelocityX * 2 + 1, y - game.BallVelocityY * 2 + 1, 2, 2, Light);
                G10_RoomDraw.Rect(canvas, x - game.BallVelocityX + 0.5, y - game.BallVelocityY + 0.5, 3, 3, Dark);
            }
            G10_RoomDraw.Rect(canvas, x + 1, y, 2, 4, Darkest);
            G10_RoomDraw.Rect(canvas, x, y + 1, 4, 2, Darkest);

            if (game.Phase == G10_PongPhase.Playing && game.Rally >= 2)
                G10_RoomDraw.TextCentered(canvas, "RALLY " + game.Rally, Center, G10_PongConfig.Top + 2, 7, Dark);
        }

        void DrawOverlay(Canvas canvas, IG10_PongView game)
        {
            switch (game.Phase)
            {
                case G10_PongPhase.Menu:
                    Panel(canvas, 30, 84);
                    G10_RoomDraw.TextCentered(canvas, "PONG", Center, 32, 15, Darkest);
                    MenuEntry(canvas, "1 PLAYER", 54, game.MenuSelection == 0, game.PhaseTicks);
                    MenuEntry(canvas, "2 PLAYERS", 65, game.MenuSelection == 1, game.PhaseTicks);
                    G10_RoomDraw.TextCentered(canvas, "ALONE: W/S OR ARROWS", Center, 80, 6, Dark);
                    G10_RoomDraw.TextCentered(canvas, "TWO: W/S  VS  ARROWS", Center, 88, 6, Dark);
                    G10_RoomDraw.TextCentered(canvas, "FIRST TO " + G10_PongConfig.PointsToWin + "  BEST RALLY " + game.BestRally, Center, 100, 6, Darkest);
                    break;
                case G10_PongPhase.Serving:
                    string text = game.HasScored
                        ? (game.LastScorer == G10_PongSide.Left ? LeftName(game) : RightName(game)) + " SCORES"
                        : "GET READY";
                    G10_RoomDraw.TextCentered(canvas, text, Center, 52, 9, Darkest);
                    break;
                case G10_PongPhase.Paused:
                    Panel(canvas, 58, 28);
                    G10_RoomDraw.TextCentered(canvas, "PAUSE", Center, 60, 11, Darkest);
                    G10_RoomDraw.TextCentered(canvas, "ENTER TO GO ON", Center, 74, 7, Dark);
                    break;
                case G10_PongPhase.GameOver:
                    Panel(canvas, 42, 58);
                    G10_RoomDraw.TextCentered(canvas, WinnerText(game), Center, 45, 11, Darkest);
                    G10_RoomDraw.TextCentered(canvas, game.LeftScore + " : " + game.RightScore, Center, 60, 9, Dark);
                    if (game.Mode == G10_PongMode.OnePlayer)
                        G10_RoomDraw.TextCentered(canvas, (game.IsNewBestRally ? "NEW BEST RALLY " : "BEST RALLY ") + game.BestRally,
                            Center, 74, 7, game.IsNewBestRally ? Darkest : Dark);
                    if (game.PhaseTicks / 10 % 2 == 0)
                        G10_RoomDraw.TextCentered(canvas, "ENTER: MENU", Center, 85, 7, Darkest);
                    break;
            }
        }

        void MenuEntry(Canvas canvas, string text, double y, bool selected, int ticks)
        {
            G10_RoomDraw.TextCentered(canvas, text, Center, y, 9, selected ? Darkest : Dark);
            if (selected && ticks / 8 % 2 == 0)
                G10_RoomDraw.Text(canvas, ">", Center - text.Length * 9 * 0.55 / 2 - 9, y, 9, Darkest);
        }

        void Panel(Canvas canvas, double y, double height)
        {
            G10_RoomDraw.Rect(canvas, 14, y, ScreenWidth - 28, height, Darkest);
            G10_RoomDraw.Rect(canvas, 16, y + 2, ScreenWidth - 32, height - 4, Lightest);
        }

        static string LeftName(IG10_PongView game)
        {
            return game.Mode == G10_PongMode.OnePlayer ? "YOU" : "P1";
        }

        static string RightName(IG10_PongView game)
        {
            return game.Mode == G10_PongMode.OnePlayer ? "CPU" : "P2";
        }

        static string WinnerText(IG10_PongView game)
        {
            if (game.Mode == G10_PongMode.TwoPlayers)
                return game.Winner == G10_PongSide.Left ? "PLAYER 1 WINS" : "PLAYER 2 WINS";
            return game.Winner == G10_PongSide.Left ? "YOU WIN!" : "CPU WINS";
        }
    }
}
