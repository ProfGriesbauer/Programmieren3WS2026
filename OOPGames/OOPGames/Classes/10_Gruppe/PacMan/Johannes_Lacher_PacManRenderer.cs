using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OOPGames
{
    // Rectangle (right and bottom exclusive) that limits where a sprite is drawn.
    internal struct G10_ClipRect
    {
        public G10_ClipRect(int left, int top, int right, int bottom)
        {
            Left = left;
            Top = top;
            Right = right;
            Bottom = bottom;
        }

        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    // A plain ARGB pixel array with the few drawing operations the renderer needs.
    internal sealed class G10_PixelBuffer
    {
        public G10_PixelBuffer(int width, int height)
        {
            Width = width;
            Height = height;
            Pixels = new uint[width * height];
            Bounds = new G10_ClipRect(0, 0, width, height);
        }

        public int Width { get; private set; }
        public int Height { get; private set; }
        public uint[] Pixels { get; private set; }
        public G10_ClipRect Bounds { get; private set; }

        public void CopyFrom(uint[] source)
        {
            Array.Copy(source, Pixels, Pixels.Length);
        }

        public void FillRect(int x, int y, int w, int h, uint color)
        {
            int x0 = Math.Max(x, 0);
            int y0 = Math.Max(y, 0);
            int x1 = Math.Min(x + w, Width);
            int y1 = Math.Min(y + h, Height);
            for (int py = y0; py < y1; py++)
            {
                int row = py * Width;
                for (int px = x0; px < x1; px++)
                {
                    Pixels[row + px] = color;
                }
            }
        }

        // Copies the non-transparent sprite pixels; tint 0 keeps the sprite colours.
        public void Blit(G10_Sprite sprite, int x, int y, G10_ClipRect clip, uint tint)
        {
            int x0 = Math.Max(Math.Max(clip.Left, 0), x);
            int y0 = Math.Max(Math.Max(clip.Top, 0), y);
            int x1 = Math.Min(Math.Min(clip.Right, Width), x + sprite.Width);
            int y1 = Math.Min(Math.Min(clip.Bottom, Height), y + sprite.Height);
            uint[] source = sprite.Pixels;
            for (int py = y0; py < y1; py++)
            {
                int src = (py - y) * sprite.Width - x;
                int dst = py * Width;
                for (int px = x0; px < x1; px++)
                {
                    uint color = source[src + px];
                    if (color != 0)
                    {
                        Pixels[dst + px] = tint != 0 ? tint : color;
                    }
                }
            }
        }

        public void DrawText(string text, int x, int y, uint color)
        {
            for (int i = 0; i < text.Length; i++)
            {
                Blit(G10_PixelFont.Glyph(text[i]), x + i * G10_PixelFont.Advance, y, Bounds, color);
            }
        }

        public static int TextWidth(string text)
        {
            return text.Length * G10_PixelFont.Advance - 1;
        }
    }

    // Builds the static part of the screen: black background, maze walls and ghost doors.
    internal static class G10_MazeBackground
    {
        private const uint DoorColor = 0xFFFFB8FF;
        // Walls are drawn as a 1 px line this far (squared distance) from the nearest walkable pixel.
        private const int LineDistanceSquaredMin = 4;
        private const int LineDistanceSquaredMax = 5;
        private const int Reach = 3;

        public static uint[] Build(IG10_MazeView maze, uint wallColor, int width, int height, int offsetY)
        {
            int cols = maze.Columns;
            int rows = maze.Rows;
            int tile = G10_PacManConstants.TileSize;
            bool[] open = new bool[cols * rows];
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    open[row * cols + col] = maze.KindAt(new G10_Tile(col, row)) != G10_TileKind.Wall;
                }
            }

            uint[] pixels = new uint[width * height];
            int mazeWidth = cols * tile;
            int mazeHeight = rows * tile;
            for (int py = 0; py < mazeHeight; py++)
            {
                for (int px = 0; px < mazeWidth; px++)
                {
                    if (open[(py / tile) * cols + px / tile])
                    {
                        continue;
                    }
                    int d = NearestOpenDistanceSquared(open, cols, rows, px, py, tile);
                    if (d >= LineDistanceSquaredMin && d <= LineDistanceSquaredMax)
                    {
                        pixels[(py + offsetY) * width + px] = wallColor;
                    }
                }
            }

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    if (maze.KindAt(new G10_Tile(col, row)) != G10_TileKind.GhostDoor)
                    {
                        continue;
                    }
                    for (int py = 3; py < 5; py++)
                    {
                        for (int px = 0; px < tile; px++)
                        {
                            pixels[(row * tile + py + offsetY) * width + col * tile + px] = DoorColor;
                        }
                    }
                }
            }
            return pixels;
        }

        private static int NearestOpenDistanceSquared(bool[] open, int cols, int rows, int px, int py, int tile)
        {
            int best = int.MaxValue;
            for (int dy = -Reach; dy <= Reach; dy++)
            {
                int qy = py + dy;
                if (qy < 0 || qy >= rows * tile)
                {
                    continue;
                }
                for (int dx = -Reach; dx <= Reach; dx++)
                {
                    int qx = px + dx;
                    if (qx < 0 || qx >= cols * tile)
                    {
                        continue;
                    }
                    int d = dx * dx + dy * dy;
                    if (d < best && open[(qy / tile) * cols + qx / tile])
                    {
                        best = d;
                    }
                }
            }
            return best;
        }
    }

    // Score column on the right side of the screen.
    internal sealed class G10_PacManHud
    {
        private const uint LabelColor = 0xFFFFFFFF;
        private const uint ValueColor = 0xFFFFE08A;
        private const uint HintColor = 0xFF8C8C8C;
        private const int Left = 230;
        private const int Right = 330;
        private const int MaxLives = 5;
        private const int MaxFruits = 7;
        private const int FruitStep = 14;

        private int _ScoreValue = -1;
        private int _HighScoreValue = -1;
        private int _LevelValue = -1;
        private string _ScoreText = "0";
        private string _HighScoreText = "0";
        private string _LevelText = "1";

        public void Draw(G10_PixelBuffer buffer, IG10_PacManView view)
        {
            if (view.Score != _ScoreValue)
            {
                _ScoreValue = view.Score;
                _ScoreText = _ScoreValue.ToString();
            }
            if (view.HighScore != _HighScoreValue)
            {
                _HighScoreValue = view.HighScore;
                _HighScoreText = _HighScoreValue.ToString();
            }
            if (view.Level != _LevelValue)
            {
                _LevelValue = view.Level;
                _LevelText = _LevelValue.ToString();
            }

            DrawLabelAndValue(buffer, "HIGH SCORE", _HighScoreText, 12);
            DrawLabelAndValue(buffer, "SCORE", _ScoreText, 38);
            DrawLabelAndValue(buffer, "LEVEL", _LevelText, 64);
            DrawLives(buffer, view.Lives);
            DrawFruits(buffer, view);
            buffer.DrawText("M SOUND", Left, 222, HintColor);
            buffer.DrawText("ENTER PAUSE", Left, 232, HintColor);
            buffer.DrawText("ESC BACK", Left, 242, HintColor);
        }

        private static void DrawLabelAndValue(G10_PixelBuffer buffer, string label, string value, int y)
        {
            buffer.DrawText(label, Left, y, LabelColor);
            buffer.DrawText(value, Right - G10_PixelBuffer.TextWidth(value), y + 10, ValueColor);
        }

        private static void DrawLives(G10_PixelBuffer buffer, int lives)
        {
            int count = Math.Min(lives, MaxLives);
            G10_Sprite icon = G10_PacManSprites.PacMan(G10_Direction.Left, 1);
            for (int i = 0; i < count; i++)
            {
                buffer.Blit(icon, Left - 2 + i * 18, 100, buffer.Bounds, 0);
            }
        }

        private static void DrawFruits(G10_PixelBuffer buffer, IG10_PacManView view)
        {
            int total = view.FruitHistory.Count;
            int start = Math.Max(0, total - MaxFruits);
            for (int i = start; i < total; i++)
            {
                G10_Sprite sprite = G10_PacManSprites.Fruit(view.FruitHistory[i]);
                buffer.Blit(sprite, Left - 2 + (i - start) * FruitStep, 198, buffer.Bounds, 0);
            }
        }
    }

    // Draws the whole Pac-Man screen into one bitmap. It only reads from the view.
    internal sealed class G10_PacManRenderer
    {
        public const int ScreenWidth = 336;
        public const int ScreenHeight = 252;

        private const int MazeOffsetY = 2;
        private const uint WallBlue = 0xFF2121DE;
        private const uint WallWhite = 0xFFFFFFFF;
        private const uint DotColor = 0xFFFFB8AE;
        private const uint ReadyColor = 0xFFFFFF00;
        private const uint GameOverColor = 0xFFFF0000;
        private const int DyingFreezeTicks = 15;
        private const int DyingAnimationTicks = 36;

        private static readonly Brush BlackBrush = CreateBlackBrush();
        private static readonly string[] PelletRows =
        {
            "..1111..",
            ".111111.",
            "11111111",
            "11111111",
            "11111111",
            "11111111",
            ".111111.",
            "..1111.."
        };

        private readonly WriteableBitmap _Bitmap;
        private readonly Image _Image;
        private readonly G10_PixelBuffer _Buffer;
        private readonly G10_PacManHud _Hud;
        private readonly Int32Rect _FullRect;
        private readonly G10_ClipRect _MazeClip;
        private uint[] _BackgroundBlue;
        private uint[] _BackgroundWhite;
        private int _MazeVersion = -1;
        private int _MazeColumns;
        private int _MazeRows;
        private int _FrameCounter;

        public G10_PacManRenderer()
        {
            _Bitmap = new WriteableBitmap(ScreenWidth, ScreenHeight, 96, 96, PixelFormats.Pbgra32, null);
            _Image = new Image();
            _Image.Width = ScreenWidth;
            _Image.Height = ScreenHeight;
            _Image.Stretch = Stretch.Fill;
            _Image.Source = _Bitmap;
            RenderOptions.SetBitmapScalingMode(_Image, BitmapScalingMode.NearestNeighbor);
            _Buffer = new G10_PixelBuffer(ScreenWidth, ScreenHeight);
            _Hud = new G10_PacManHud();
            _FullRect = new Int32Rect(0, 0, ScreenWidth, ScreenHeight);
            _MazeClip = new G10_ClipRect(0, MazeOffsetY, 28 * G10_PacManConstants.TileSize, MazeOffsetY + 31 * G10_PacManConstants.TileSize);
        }

        public void Draw(Canvas canvas, IG10_PacManView view)
        {
            AttachImage(canvas);
            _FrameCounter++;
            UpdateBackground(view.Maze);

            bool flash = view.Phase == G10_PacManPhaseKind.LevelCleared && (view.PhaseTicks / 6) % 2 == 1;
            _Buffer.CopyFrom(flash ? _BackgroundWhite : _BackgroundBlue);
            DrawCollectibles(view);

            bool dyingBlank = view.Phase == G10_PacManPhaseKind.Dying && view.PhaseTicks >= DyingFreezeTicks;
            if (!dyingBlank)
            {
                DrawFruit(view);
            }
            DrawPacMan(view);
            if (!dyingBlank && view.Phase != G10_PacManPhaseKind.LevelCleared && view.Phase != G10_PacManPhaseKind.GameOver)
            {
                DrawGhosts(view);
            }
            DrawPopups(view);
            DrawMessage(view);
            _Hud.Draw(_Buffer, view);

            _Bitmap.WritePixels(_FullRect, _Buffer.Pixels, ScreenWidth * 4, 0);
        }

        private void AttachImage(Canvas canvas)
        {
            if (canvas.Children.Count != 1 || !ReferenceEquals(canvas.Children[0], _Image))
            {
                canvas.Children.Clear();
                canvas.Children.Add(_Image);
                Canvas.SetLeft(_Image, 0);
                Canvas.SetTop(_Image, 0);
            }
            if (!ReferenceEquals(canvas.Background, BlackBrush))
            {
                canvas.Background = BlackBrush;
            }
        }

        private void UpdateBackground(IG10_MazeView maze)
        {
            if (maze.Version == _MazeVersion && _BackgroundBlue != null)
            {
                return;
            }
            _MazeVersion = maze.Version;
            _MazeColumns = maze.Columns;
            _MazeRows = maze.Rows;
            _BackgroundBlue = G10_MazeBackground.Build(maze, WallBlue, ScreenWidth, ScreenHeight, MazeOffsetY);
            _BackgroundWhite = G10_MazeBackground.Build(maze, WallWhite, ScreenWidth, ScreenHeight, MazeOffsetY);
        }

        private void DrawCollectibles(IG10_PacManView view)
        {
            bool pelletsVisible = view.Phase != G10_PacManPhaseKind.Playing || (_FrameCounter / 8) % 2 == 0;
            int tile = G10_PacManConstants.TileSize;
            for (int row = 0; row < _MazeRows; row++)
            {
                for (int col = 0; col < _MazeColumns; col++)
                {
                    G10_CollectibleKind kind = view.Maze.CollectibleAt(new G10_Tile(col, row));
                    if (kind == G10_CollectibleKind.Dot)
                    {
                        _Buffer.FillRect(col * tile + 3, row * tile + 3 + MazeOffsetY, 2, 2, DotColor);
                    }
                    else if (kind == G10_CollectibleKind.PowerPellet && pelletsVisible)
                    {
                        DrawPellet(col * tile, row * tile + MazeOffsetY);
                    }
                }
            }
        }

        private void DrawPellet(int x, int y)
        {
            for (int py = 0; py < PelletRows.Length; py++)
            {
                string line = PelletRows[py];
                for (int px = 0; px < line.Length; px++)
                {
                    if (line[px] == '1')
                    {
                        _Buffer.Pixels[(y + py) * ScreenWidth + x + px] = DotColor;
                    }
                }
            }
        }

        private void DrawActor(G10_Sprite sprite, int pixelX, int pixelY)
        {
            int x = pixelX - sprite.Width / 2;
            int y = pixelY + MazeOffsetY - sprite.Height / 2;
            _Buffer.Blit(sprite, x, y, _MazeClip, 0);
        }

        private void DrawFruit(IG10_PacManView view)
        {
            IG10_FruitView fruit = view.Fruit;
            if (fruit != null)
            {
                DrawActor(G10_PacManSprites.Fruit(fruit.Kind), fruit.PixelX, fruit.PixelY);
            }
        }

        private void DrawPacMan(IG10_PacManView view)
        {
            IG10_ActorView pacMan = view.PacMan;
            if (pacMan == null || !pacMan.Visible)
            {
                return;
            }
            G10_PacManPhaseKind phase = view.Phase;
            G10_Sprite sprite;
            if (phase == G10_PacManPhaseKind.Dying && view.PhaseTicks >= DyingFreezeTicks)
            {
                int frames = G10_PacManSprites.PacManDeathFrameCount;
                int index = (view.PhaseTicks - DyingFreezeTicks) * frames / DyingAnimationTicks;
                if (index >= frames)
                {
                    return;
                }
                sprite = G10_PacManSprites.PacManDeath(index);
            }
            else if (phase == G10_PacManPhaseKind.Ready || phase == G10_PacManPhaseKind.LevelCleared)
            {
                sprite = G10_PacManSprites.PacMan(G10_Direction.None, 0);
            }
            else
            {
                int frame = pacMan.IsMoving ? (pacMan.MoveTicks / 2) % G10_PacManSprites.PacManFrameCount : 1;
                sprite = G10_PacManSprites.PacMan(pacMan.Direction, frame);
            }
            DrawActor(sprite, pacMan.PixelX, pacMan.PixelY);
        }

        private void DrawGhosts(IG10_PacManView view)
        {
            int frame = (_FrameCounter / 4) % G10_PacManSprites.GhostFrameCount;
            bool flashWhite = view.FrightenedFlashing && (_FrameCounter / 5) % 2 == 1;
            for (int i = 0; i < view.Ghosts.Count; i++)
            {
                IG10_GhostView ghost = view.Ghosts[i];
                if (!ghost.Visible)
                {
                    continue;
                }
                G10_Sprite sprite;
                if (ghost.Mode == G10_GhostModeKind.Frightened)
                {
                    sprite = G10_PacManSprites.FrightenedGhost(frame, flashWhite);
                }
                else if (ghost.Mode == G10_GhostModeKind.Eaten)
                {
                    sprite = G10_PacManSprites.GhostEyes(ghost.Direction);
                }
                else
                {
                    sprite = G10_PacManSprites.Ghost(ghost.Kind, ghost.Direction, frame);
                }
                DrawActor(sprite, ghost.PixelX, ghost.PixelY);
            }
        }

        private void DrawPopups(IG10_PacManView view)
        {
            for (int i = 0; i < view.Popups.Count; i++)
            {
                G10_PointsPopup popup = view.Popups[i];
                DrawActor(G10_PacManSprites.Points(popup.Points), popup.PixelX, popup.PixelY);
            }
        }

        private void DrawMessage(IG10_PacManView view)
        {
            string text = null;
            uint color = ReadyColor;
            if (view.Phase == G10_PacManPhaseKind.Ready)
            {
                text = "READY!";
            }
            else if (view.Phase == G10_PacManPhaseKind.Paused)
            {
                text = "PAUSED";
            }
            else if (view.Phase == G10_PacManPhaseKind.GameOver)
            {
                text = "GAME OVER";
                color = GameOverColor;
            }
            if (text != null)
            {
                _Buffer.DrawText(text, 112 - G10_PixelBuffer.TextWidth(text) / 2, 137 + MazeOffsetY, color);
            }
        }

        private static Brush CreateBlackBrush()
        {
            SolidColorBrush brush = new SolidColorBrush(Colors.Black);
            brush.Freeze();
            return brush;
        }
    }
}
