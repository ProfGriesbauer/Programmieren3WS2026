using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media;

namespace OOPGames
{
    // Draws a Snake game in the four green shades of the original Game Boy.
    public sealed class G10_SnakeRenderer
    {
        public const int ScreenWidth = 160;
        public const int ScreenHeight = 144;
        const int Cell = 8;
        const int FieldX = 4;
        const int FieldY = 20;
        const double Center = ScreenWidth / 2.0;
        // The bug blinks during its last second and a half.
        const int BugWarningTicks = 38;

        static readonly Color Lightest = G10_GameBoyColors.Lightest;
        static readonly Color Light = G10_GameBoyColors.Light;
        static readonly Color Dark = G10_GameBoyColors.Dark;
        static readonly Color Darkest = G10_GameBoyColors.Darkest;

        public void Draw(Canvas canvas, IG10_SnakeView game)
        {
            canvas.Children.Clear();
            B5RoomDraw.Rect(canvas, 0, 0, ScreenWidth, ScreenHeight, Light);
            DrawHud(canvas, game);
            B5RoomDraw.Rect(canvas, FieldX - 2, FieldY - 2, game.Columns * Cell + 4, game.Rows * Cell + 4, Darkest);
            B5RoomDraw.Rect(canvas, FieldX, FieldY, game.Columns * Cell, game.Rows * Cell, Lightest);

            DrawApple(canvas, game.Apple);
            if (game.HasBug && (game.BugTicksLeft > BugWarningTicks || game.BugTicksLeft / 3 % 2 == 0))
                DrawBug(canvas, game.Bug, game.PhaseTicks);
            if (game.Phase != G10_SnakePhase.Dying || game.PhaseTicks / 3 % 2 == 0)
                DrawSnake(canvas, game);

            DrawOverlay(canvas, game);
        }

        void DrawHud(Canvas canvas, IG10_SnakeView game)
        {
            B5RoomDraw.Text(canvas, "SCORE " + game.Score.ToString("D5"), 3, 2, 8, Darkest);
            B5RoomDraw.Text(canvas, "LV" + game.Level, 70, 2, 8, Dark);
            B5RoomDraw.Text(canvas, "HI " + game.HighScore.ToString("D5"), 110, 2, 8, Darkest);
            if (game.HasBug)
            {
                double width = (game.Columns * Cell) * game.BugTicksLeft / (double)G10_SnakeConfig.BugLifetimeTicks;
                B5RoomDraw.Rect(canvas, FieldX, 13, width, 2, Dark);
            }
        }

        void DrawSnake(Canvas canvas, IG10_SnakeView game)
        {
            IReadOnlyList<G10_Tile> body = game.Body;
            for (int i = body.Count - 1; i >= 0; i--)
            {
                bool head = i == 0;
                bool tail = i == body.Count - 1 && !head;
                // The tail is drawn thinner; segments are joined towards the segment in front.
                int inset = tail ? 2 : 1;
                Color color = head ? Darkest : Dark;
                double x = FieldX + body[i].Col * Cell;
                double y = FieldY + body[i].Row * Cell;
                B5RoomDraw.Rect(canvas, x + inset, y + inset, Cell - 2 * inset, Cell - 2 * inset, color);
                if (!head) DrawJoint(canvas, body[i], body[i - 1], x, y, inset);
                if (!head && !tail && i % 3 == 0) B5RoomDraw.Rect(canvas, x + 3, y + 3, 2, 2, Darkest);
            }
            DrawFace(canvas, game);
        }

        // Fills the gap between a segment and the one in front of it. The joint overlaps both
        // segments by a pixel; edges that only touch show a thin seam once the hub scales the canvas.
        void DrawJoint(Canvas canvas, G10_Tile segment, G10_Tile front, double x, double y, int inset)
        {
            int dx = front.Col - segment.Col;
            int dy = front.Row - segment.Row;
            double size = Cell - 2 * inset;
            double length = inset + 3;
            if (dx == 1) B5RoomDraw.Rect(canvas, x + Cell - inset - 1, y + inset, length, size, Dark);
            else if (dx == -1) B5RoomDraw.Rect(canvas, x - 2, y + inset, length, size, Dark);
            else if (dy == 1) B5RoomDraw.Rect(canvas, x + inset, y + Cell - inset - 1, size, length, Dark);
            else if (dy == -1) B5RoomDraw.Rect(canvas, x + inset, y - 2, size, length, Dark);
        }

        void DrawFace(Canvas canvas, IG10_SnakeView game)
        {
            G10_Tile head = game.Body[0];
            double x = FieldX + head.Col * Cell;
            double y = FieldY + head.Row * Cell;
            G10_Direction heading = game.Heading;
            bool dead = game.Phase == G10_SnakePhase.Dying || (game.Phase == G10_SnakePhase.GameOver && !game.Won);

            // Eyes sit on the front half of the head, side by side across the heading.
            double frontX = heading == G10_Direction.Right ? 5 : heading == G10_Direction.Left ? 2 : 0;
            double frontY = heading == G10_Direction.Down ? 5 : heading == G10_Direction.Up ? 2 : 0;
            double eyeSize = dead ? 1 : 1.5;
            if (heading.IsHorizontal())
            {
                B5RoomDraw.Rect(canvas, x + frontX, y + 2, eyeSize, eyeSize, Lightest);
                B5RoomDraw.Rect(canvas, x + frontX, y + 5, eyeSize, eyeSize, Lightest);
            }
            else
            {
                B5RoomDraw.Rect(canvas, x + 2, y + frontY, eyeSize, eyeSize, Lightest);
                B5RoomDraw.Rect(canvas, x + 5, y + frontY, eyeSize, eyeSize, Lightest);
            }

            // The tongue flicks out now and then while the snake is moving.
            if (game.Phase != G10_SnakePhase.Playing || game.PhaseTicks % 24 >= 4) return;
            if (heading == G10_Direction.Right) B5RoomDraw.Rect(canvas, x + Cell, y + 3.5, 2, 1, Darkest);
            else if (heading == G10_Direction.Left) B5RoomDraw.Rect(canvas, x - 2, y + 3.5, 2, 1, Darkest);
            else if (heading == G10_Direction.Down) B5RoomDraw.Rect(canvas, x + 3.5, y + Cell, 1, 2, Darkest);
            else B5RoomDraw.Rect(canvas, x + 3.5, y - 2, 1, 2, Darkest);
        }

        void DrawApple(Canvas canvas, G10_Tile tile)
        {
            double x = FieldX + tile.Col * Cell;
            double y = FieldY + tile.Row * Cell;
            B5RoomDraw.Rect(canvas, x + 1, y + 3, 6, 4, Darkest);
            B5RoomDraw.Rect(canvas, x + 2, y + 2, 4, 6, Darkest);
            B5RoomDraw.Rect(canvas, x + 2, y + 3, 1, 1, Light);
            B5RoomDraw.Rect(canvas, x + 4, y, 1, 2, Dark);
            B5RoomDraw.Rect(canvas, x + 5, y + 0.5, 2, 1, Dark);
        }

        // A little beetle that wiggles its legs.
        void DrawBug(Canvas canvas, G10_Tile tile, int ticks)
        {
            double x = FieldX + tile.Col * Cell;
            double y = FieldY + tile.Row * Cell;
            double legs = ticks / 4 % 2 == 0 ? 0 : 1;
            B5RoomDraw.Rect(canvas, x + 1, y + 2 + legs, 6, 1, Darkest);
            B5RoomDraw.Rect(canvas, x + 1, y + 5 - legs, 6, 1, Darkest);
            B5RoomDraw.Rect(canvas, x + 2, y + 1, 4, 6, Dark);
            B5RoomDraw.Rect(canvas, x + 3.5, y + 1, 1, 6, Darkest);
            B5RoomDraw.Rect(canvas, x + 3, y, 2, 1, Darkest);
        }

        void DrawOverlay(Canvas canvas, IG10_SnakeView game)
        {
            switch (game.Phase)
            {
                case G10_SnakePhase.Ready:
                    Panel(canvas, 44, 52);
                    B5RoomDraw.TextCentered(canvas, "SNAKE", Center, 47, 12, Darkest);
                    B5RoomDraw.TextCentered(canvas, "ARROWS / WASD STEER", Center, 64, 7, Dark);
                    B5RoomDraw.TextCentered(canvas, "ENTER PAUSE  ESC BACK", Center, 73, 7, Dark);
                    if (game.PhaseTicks / 10 % 2 == 0)
                        B5RoomDraw.TextCentered(canvas, "PRESS A DIRECTION", Center, 84, 7, Darkest);
                    break;
                case G10_SnakePhase.Paused:
                    Panel(canvas, 58, 28);
                    B5RoomDraw.TextCentered(canvas, "PAUSE", Center, 60, 11, Darkest);
                    B5RoomDraw.TextCentered(canvas, "ENTER TO GO ON", Center, 74, 7, Dark);
                    break;
                case G10_SnakePhase.GameOver:
                    Panel(canvas, 44, 56);
                    B5RoomDraw.TextCentered(canvas, game.Won ? "YOU WIN!" : "GAME OVER", Center, 47, 11, Darkest);
                    B5RoomDraw.TextCentered(canvas, "SCORE " + game.Score + "  LENGTH " + game.Body.Count, Center, 63, 7, Dark);
                    if (game.IsNewHighScore)
                        B5RoomDraw.TextCentered(canvas, "NEW HIGHSCORE!", Center, 73, 7, Darkest);
                    if (game.PhaseTicks / 10 % 2 == 0)
                        B5RoomDraw.TextCentered(canvas, "ENTER: NEW GAME", Center, 86, 7, Darkest);
                    break;
            }
        }

        void Panel(Canvas canvas, double y, double height)
        {
            B5RoomDraw.Rect(canvas, 14, y, ScreenWidth - 28, height, Darkest);
            B5RoomDraw.Rect(canvas, 16, y + 2, ScreenWidth - 32, height - 4, Lightest);
        }
    }
}
