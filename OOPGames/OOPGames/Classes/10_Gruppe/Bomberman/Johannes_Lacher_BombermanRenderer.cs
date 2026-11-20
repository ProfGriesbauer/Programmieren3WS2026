using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OOPGames
{
    // Text in the pixel font, also enlarged and centred.
    internal static class G10_BomberText
    {
        public static int Width(string text, int scale)
        {
            return G10_PixelBuffer.TextWidth(text) * scale;
        }

        public static void Draw(G10_PixelBuffer buffer, string text, int x, int y, int scale, uint color)
        {
            if (scale == 1)
            {
                buffer.DrawText(text, x, y, color);
                return;
            }
            for (int i = 0; i < text.Length; i++)
            {
                G10_Sprite glyph = G10_PixelFont.Glyph(text[i]);
                int left = x + i * G10_PixelFont.Advance * scale;
                for (int gy = 0; gy < glyph.Height; gy++)
                    for (int gx = 0; gx < glyph.Width; gx++)
                        if (glyph.Pixels[gy * glyph.Width + gx] != 0)
                            buffer.FillRect(left + gx * scale, y + gy * scale, scale, scale, color);
            }
        }

        public static void Centered(G10_PixelBuffer buffer, string text, int centerX, int y, int scale, uint color)
        {
            Draw(buffer, text, centerX - Width(text, scale) / 2, y, scale, color);
        }
    }

    // Draws a burning cell. A flame grows, burns and shrinks again; every layer is narrower
    // than the one below it (red outside, yellow, white core).
    internal static class G10_BomberFlamePainter
    {
        static readonly uint[] _Layers = { G10_C64Colors.LightRed, G10_C64Colors.Yellow, G10_C64Colors.White };

        public static void Draw(G10_PixelBuffer buffer, int x, int y, G10_BomberFlameShape shape, int ageTicks)
        {
            int half = HalfWidth(ageTicks);
            for (int layer = 0; layer < _Layers.Length; layer++)
            {
                int w = half - layer * 2;
                if (w <= 0) return;
                int inset = layer * 2;
                uint color = _Layers[layer];
                int size = G10_BomberConstants.TileSize;
                int mid = size / 2;
                switch (shape)
                {
                    case G10_BomberFlameShape.Horizontal:
                        buffer.FillRect(x, y + mid - w, size, w * 2, color);
                        break;
                    case G10_BomberFlameShape.Vertical:
                        buffer.FillRect(x + mid - w, y, w * 2, size, color);
                        break;
                    case G10_BomberFlameShape.Center:
                        buffer.FillRect(x, y + mid - w, size, w * 2, color);
                        buffer.FillRect(x + mid - w, y, w * 2, size, color);
                        break;
                    case G10_BomberFlameShape.EndRight:
                        buffer.FillRect(x, y + mid - w, 13 - inset, w * 2, color);
                        break;
                    case G10_BomberFlameShape.EndLeft:
                        buffer.FillRect(x + 3 + inset, y + mid - w, 13 - inset, w * 2, color);
                        break;
                    case G10_BomberFlameShape.EndDown:
                        buffer.FillRect(x + mid - w, y, w * 2, 13 - inset, color);
                        break;
                    case G10_BomberFlameShape.EndUp:
                        buffer.FillRect(x + mid - w, y + 3 + inset, w * 2, 13 - inset, color);
                        break;
                }
            }
        }

        static int HalfWidth(int ageTicks)
        {
            int left = G10_BomberFlame.Duration - ageTicks;
            if (ageTicks < 2 || left <= 2) return 3;
            if (ageTicks < 4 || left <= 4) return 5;
            return 7;
        }
    }

    // Top bar and the two side panels.
    internal sealed class G10_BombermanHud
    {
        public const int BarHeight = 20;
        const int LeftPanelX = 2;
        const int RightPanelX = 282;
        const uint Label = G10_C64Colors.LightBlue;
        const uint Value = G10_C64Colors.White;

        public static void DrawFrame(uint[] pixels, int width, int height, int arenaTop, int arenaHeight)
        {
            G10_PixelBuffer buffer = new G10_PixelBuffer(width, height);
            buffer.FillRect(0, 0, width, height, G10_C64Colors.LightBlue);
            buffer.FillRect(0, 0, width, BarHeight, G10_C64Colors.Blue);
            buffer.FillRect(LeftPanelX, arenaTop, 36, arenaHeight, G10_C64Colors.Blue);
            buffer.FillRect(RightPanelX, arenaTop, 36, arenaHeight, G10_C64Colors.Blue);
            Array.Copy(buffer.Pixels, pixels, pixels.Length);
        }

        public void Draw(G10_PixelBuffer buffer, IG10_BomberView view, int frame)
        {
            buffer.DrawText("SCORE", 8, 7, Label);
            buffer.DrawText(view.Score.ToString("D6"), 44, 7, Value);
            buffer.DrawText("HI", 110, 7, Label);
            buffer.DrawText(view.HighScore.ToString("D6"), 128, 7, Value);
            buffer.DrawText("TIME", 238, 7, Label);
            bool hurry = view.SecondsLeft <= 30 && (frame / 8) % 2 == 0;
            buffer.DrawText(view.SecondsLeft.ToString("D3"), 268, 7, hurry ? G10_C64Colors.LightRed : Value);

            int x = LeftPanelX + 3;
            buffer.DrawText("STAGE", x, 30, Label);
            buffer.DrawText(view.Stage + "/" + view.StageCount, x, 40, Value);
            DrawStat(buffer, G10_BomberItemKind.BombUp, view.Player.BombCapacity.ToString(), 58);
            DrawStat(buffer, G10_BomberItemKind.FireUp, view.Player.FireRange.ToString(), 78);
            DrawStat(buffer, G10_BomberItemKind.SpeedUp, view.Player.SpeedLevel.ToString(), 98);
            if (view.Player.HasRemote) DrawStat(buffer, G10_BomberItemKind.Remote, "ON", 118);

            x = RightPanelX + 3;
            buffer.DrawText("LIVES", x, 30, Label);
            buffer.Blit(G10_BombermanSprites.LifeIcon, x, 40, buffer.Bounds, 0);
            buffer.DrawText("X" + view.Lives, x + 11, 41, Value);
            buffer.DrawText("FOES", x, 58, Label);
            int foes = 0;
            foreach (IG10_BomberEnemyView enemy in view.Enemies)
                if (!enemy.IsDying) foes++;
            buffer.DrawText(foes.ToString(), x, 68, Value);
            if (view.ExitOpen) buffer.DrawText("EXIT", x, 86, (frame / 6) % 2 == 0 ? G10_C64Colors.Yellow : Value);

            DrawHint(buffer, "Z/SPC", "BOMB", 138);
            DrawHint(buffer, "C", "BOOM", 160);
            DrawHint(buffer, "ENTER", "PAUSE", 182);
            DrawHint(buffer, "M", "SOUND", 204);
        }

        static void DrawStat(G10_PixelBuffer buffer, G10_BomberItemKind item, string value, int y)
        {
            buffer.Blit(G10_BombermanSprites.Item(item), LeftPanelX + 1, y, buffer.Bounds, 0);
            buffer.DrawText(value, LeftPanelX + 20, y + 5, Value);
        }

        static void DrawHint(G10_PixelBuffer buffer, string key, string action, int y)
        {
            buffer.DrawText(key, RightPanelX + 3, y, Value);
            buffer.DrawText(action, RightPanelX + 3, y + 9, Label);
        }
    }

    // Turns the read-only game view into a 320 x 240 picture (4:3, like the C64 screen with border).
    internal sealed class G10_BombermanRenderer
    {
        public const int ScreenWidth = 320;
        public const int ScreenHeight = 240;
        const int ArenaX = 40;
        const int ArenaY = 24;
        const int Tile = G10_BomberConstants.TileSize;
        const int ShadowRows = 3;
        static readonly SolidColorBrush BlackBrush = CreateBlackBrush();

        readonly G10_PixelBuffer _Buffer = new G10_PixelBuffer(ScreenWidth, ScreenHeight);
        readonly G10_BombermanHud _Hud = new G10_BombermanHud();
        readonly Int32Rect _FullRect = new Int32Rect(0, 0, ScreenWidth, ScreenHeight);
        readonly G10_ClipRect _ArenaClip;
        WriteableBitmap _Bitmap;
        Image _Image;
        uint[] _Background;
        int _ArenaVersion = -1;
        int _Frame;

        public G10_BombermanRenderer()
        {
            _ArenaClip = new G10_ClipRect(ArenaX, ArenaY, ArenaX + G10_BomberConstants.Columns * Tile, ArenaY + G10_BomberConstants.Rows * Tile);
        }

        public void Draw(Canvas canvas, IG10_BomberView view)
        {
            AttachImage(canvas);
            Compose(view);
            _Bitmap.WritePixels(_FullRect, _Buffer.Pixels, ScreenWidth * 4, 0);
        }

        // Draws the frame into the pixel buffer only; needs no WPF objects.
        public uint[] Compose(IG10_BomberView view)
        {
            _Frame++;
            if (view.Phase == G10_BomberPhaseKind.StageIntro)
            {
                DrawIntro(view);
                return _Buffer.Pixels;
            }

            UpdateBackground(view.Arena);
            _Buffer.CopyFrom(_Background);
            DrawCells(view);
            DrawBombs(view);
            foreach (IG10_BomberFlameView flame in view.Flames)
                G10_BomberFlamePainter.Draw(_Buffer, ArenaX + flame.Cell.Col * Tile, ArenaY + flame.Cell.Row * Tile, flame.Shape, flame.AgeTicks);
            DrawEnemies(view);
            DrawPlayer(view);
            foreach (G10_BomberPopup popup in view.Popups)
            {
                string text = popup.Points.ToString();
                _Buffer.DrawText(text, ArenaX + popup.PixelX - G10_PixelBuffer.TextWidth(text) / 2, ArenaY + popup.PixelY - 4, G10_C64Colors.White);
            }
            _Hud.Draw(_Buffer, view, _Frame);
            DrawMessage(view);
            return _Buffer.Pixels;
        }

        void UpdateBackground(IG10_BomberArenaView arena)
        {
            if (_Background != null && arena.Version == _ArenaVersion) return;
            _ArenaVersion = arena.Version;
            _Background = new uint[ScreenWidth * ScreenHeight];
            G10_BombermanHud.DrawFrame(_Background, ScreenWidth, ScreenHeight, ArenaY, arena.Rows * Tile);
            G10_PixelBuffer buffer = new G10_PixelBuffer(ScreenWidth, ScreenHeight);
            buffer.CopyFrom(_Background);
            for (int row = 0; row < arena.Rows; row++)
            {
                for (int col = 0; col < arena.Columns; col++)
                {
                    G10_BomberCell cell = new G10_BomberCell(col, row);
                    int x = ArenaX + col * Tile;
                    int y = ArenaY + row * Tile;
                    if (arena.KindAt(cell) == G10_BomberCellKind.Pillar)
                    {
                        buffer.Blit(G10_BombermanSprites.Pillar, x, y, buffer.Bounds, 0);
                        continue;
                    }
                    buffer.Blit(G10_BombermanSprites.Floor, x, y, buffer.Bounds, 0);
                    if (arena.KindAt(new G10_BomberCell(col, row - 1)) == G10_BomberCellKind.Pillar)
                        buffer.FillRect(x, y, Tile, ShadowRows, G10_C64Colors.Shadow);
                }
            }
            Array.Copy(buffer.Pixels, _Background, _Background.Length);
        }

        // Bricks, burning bricks, uncovered items and the exit. Bricks throw a shadow on the floor below.
        void DrawCells(IG10_BomberView view)
        {
            IG10_BomberArenaView arena = view.Arena;
            for (int row = 1; row < arena.Rows - 1; row++)
            {
                for (int col = 1; col < arena.Columns - 1; col++)
                {
                    G10_BomberCell cell = new G10_BomberCell(col, row);
                    int x = ArenaX + col * Tile;
                    int y = ArenaY + row * Tile;
                    G10_BomberCellKind kind = arena.KindAt(cell);
                    if (kind == G10_BomberCellKind.Brick)
                    {
                        _Buffer.Blit(G10_BombermanSprites.Brick, x, y, _ArenaClip, 0);
                        continue;
                    }
                    if (kind != G10_BomberCellKind.Floor) continue;

                    if (arena.KindAt(new G10_BomberCell(col, row - 1)) == G10_BomberCellKind.Brick)
                        _Buffer.FillRect(x, y, Tile, ShadowRows, G10_C64Colors.Shadow);
                    int burn = arena.BurnTicksAt(cell);
                    if (burn > 0)
                    {
                        int progress = G10_BomberArena.BurnTicks - burn;
                        _Buffer.Blit(G10_BombermanSprites.Burn(progress * G10_BombermanSprites.BurnFrameCount / G10_BomberArena.BurnTicks), x, y, _ArenaClip, 0);
                        continue;
                    }
                    G10_BomberItemKind item = arena.VisibleItemAt(cell);
                    if (item == G10_BomberItemKind.Exit) _Buffer.Blit(DoorSprite(view.ExitOpen), x, y, _ArenaClip, 0);
                    else if (item != G10_BomberItemKind.None) DrawItem(item, x, y);
                }
            }
        }

        // Power-ups blink their frame so they stand out on the floor.
        void DrawItem(G10_BomberItemKind item, int x, int y)
        {
            _Buffer.Blit(G10_BombermanSprites.Item(item), x, y, _ArenaClip, 0);
            if ((_Frame / 5) % 3 != 0) return;
            _Buffer.FillRect(x, y, Tile, 1, G10_C64Colors.White);
            _Buffer.FillRect(x, y + Tile - 1, Tile, 1, G10_C64Colors.White);
            _Buffer.FillRect(x, y, 1, Tile, G10_C64Colors.White);
            _Buffer.FillRect(x + Tile - 1, y, 1, Tile, G10_C64Colors.White);
        }

        G10_Sprite DoorSprite(bool open)
        {
            if (!open) return G10_BombermanSprites.DoorClosed;
            return (_Frame / 4) % 2 == 0 ? G10_BombermanSprites.DoorOpenBright : G10_BombermanSprites.DoorOpenDark;
        }

        // Bombs pulse; shortly before they go off they pulse faster.
        void DrawBombs(IG10_BomberView view)
        {
            foreach (IG10_BomberBombView bomb in view.Bombs)
            {
                bool hurry = !bomb.IsRemote && bomb.AgeTicks > G10_BomberBomb.FuseTicks - 20;
                bool swollen = (bomb.AgeTicks / (hurry ? 3 : 7)) % 2 == 1;
                _Buffer.Blit(G10_BombermanSprites.Bomb(swollen, bomb.IsRemote), ArenaX + bomb.Cell.Col * Tile, ArenaY + bomb.Cell.Row * Tile, _ArenaClip, 0);
            }
        }

        // A hit enemy flashes white and then goes up in smoke.
        void DrawEnemies(IG10_BomberView view)
        {
            foreach (IG10_BomberEnemyView enemy in view.Enemies)
            {
                int x = ArenaX + enemy.PixelX - Tile / 2;
                int y = ArenaY + enemy.PixelY - Tile / 2;
                if (!enemy.IsDying)
                {
                    int frame = enemy.IsMoving ? enemy.MoveTicks / 6 : _Frame / 10;
                    _Buffer.Blit(G10_BombermanSprites.Enemy(enemy.Kind, frame), x, y, _ArenaClip, 0);
                    continue;
                }
                int half = G10_BomberEnemy.DyingDuration / 2;
                if (enemy.DyingTicks < half)
                {
                    uint tint = (enemy.DyingTicks / 2) % 2 == 0 ? G10_C64Colors.White : 0;
                    _Buffer.Blit(G10_BombermanSprites.Enemy(enemy.Kind, 0), x, y, _ArenaClip, tint);
                }
                else
                {
                    int smoke = (enemy.DyingTicks - half) * G10_BombermanSprites.SmokeFrameCount / half;
                    _Buffer.Blit(G10_BombermanSprites.Smoke(smoke), x, y, _ArenaClip, 0);
                }
            }
        }

        // When the player dies, the robot flashes red and white and then turns into smoke.
        void DrawPlayer(IG10_BomberView view)
        {
            IG10_BomberPlayerView player = view.Player;
            int x = ArenaX + player.PixelX - Tile / 2;
            int y = ArenaY + player.PixelY - Tile / 2;
            if (!player.IsDead)
            {
                int walk = player.IsMoving ? player.MoveTicks / 3 : 1;
                _Buffer.Blit(G10_BombermanSprites.Player(player.Direction, walk), x, y, _ArenaClip, 0);
                return;
            }

            const int FlashTicks = 20;
            const int SmokeTicks = 16;
            // After the death animation (e.g. under GAME OVER) the player is gone.
            if (view.Phase != G10_BomberPhaseKind.Dying) return;
            int ticks = view.PhaseTicks;
            if (ticks < FlashTicks)
            {
                uint tint = (ticks / 3) % 2 == 0 ? G10_C64Colors.LightRed : G10_C64Colors.White;
                _Buffer.Blit(G10_BombermanSprites.Player(G10_Direction.Down, 1), x, y, _ArenaClip, tint);
            }
            else if (ticks < FlashTicks + SmokeTicks)
            {
                int smoke = (ticks - FlashTicks) * G10_BombermanSprites.SmokeFrameCount / SmokeTicks;
                _Buffer.Blit(G10_BombermanSprites.Smoke(smoke), x, y, _ArenaClip, 0);
            }
        }

        // A dark-blue screen with "STAGE n" and a blinking cursor after READY., like the C64 start screen.
        void DrawIntro(IG10_BomberView view)
        {
            _Buffer.FillRect(0, 0, ScreenWidth, ScreenHeight, G10_C64Colors.LightBlue);
            _Buffer.FillRect(16, 16, ScreenWidth - 32, ScreenHeight - 32, G10_C64Colors.Blue);
            G10_BomberText.Centered(_Buffer, "BOMBERMAN", ScreenWidth / 2, 50, 3, G10_C64Colors.LightBlue);
            G10_BomberText.Centered(_Buffer, "STAGE " + view.Stage, ScreenWidth / 2, 104, 2, G10_C64Colors.White);
            G10_BomberText.Centered(_Buffer, "LIVES " + view.Lives + "   HI " + view.HighScore, ScreenWidth / 2, 138, 1, G10_C64Colors.LightBlue);
            _Buffer.DrawText("READY.", 28, 180, G10_C64Colors.LightBlue);
            if ((_Frame / 8) % 2 == 0) _Buffer.FillRect(28, 190, 6, 8, G10_C64Colors.LightBlue);
        }

        void DrawMessage(IG10_BomberView view)
        {
            switch (view.Phase)
            {
                case G10_BomberPhaseKind.Paused:
                    DrawBox("PAUSE", "PRESS ENTER", null, false);
                    break;
                case G10_BomberPhaseKind.StageClear:
                    DrawBox("STAGE CLEAR!", "TIME BONUS " + view.TimeBonus, null, false);
                    break;
                case G10_BomberPhaseKind.GameOver:
                    DrawBox("GAME OVER", "SCORE " + view.Score, "ENTER: NEW GAME", true);
                    break;
                case G10_BomberPhaseKind.Victory:
                    DrawBox("YOU WIN!", "ALL STAGES CLEAR - SCORE " + view.Score, "ENTER: NEW GAME", true);
                    break;
                default:
                    if (view.TimeUpWarning && (_Frame / 5) % 2 == 0)
                        G10_BomberText.Centered(_Buffer, "TIME UP!", ScreenWidth / 2, ArenaY + 90, 3, G10_C64Colors.LightRed);
                    break;
            }
        }

        void DrawBox(string title, string line, string prompt, bool blinkPrompt)
        {
            const int Width = 220;
            int height = prompt == null ? 52 : 66;
            int x = (ScreenWidth - Width) / 2;
            int y = ArenaY + (G10_BomberConstants.Rows * Tile - height) / 2;
            _Buffer.FillRect(x, y, Width, height, G10_C64Colors.LightBlue);
            _Buffer.FillRect(x + 2, y + 2, Width - 4, height - 4, G10_C64Colors.Blue);
            G10_BomberText.Centered(_Buffer, title, ScreenWidth / 2, y + 9, 2, G10_C64Colors.White);
            G10_BomberText.Centered(_Buffer, line, ScreenWidth / 2, y + 33, 1, G10_C64Colors.LightBlue);
            if (prompt != null && (!blinkPrompt || (_Frame / 10) % 2 == 0))
                G10_BomberText.Centered(_Buffer, prompt, ScreenWidth / 2, y + 49, 1, G10_C64Colors.Yellow);
        }

        void AttachImage(Canvas canvas)
        {
            if (_Image == null)
            {
                _Bitmap = new WriteableBitmap(ScreenWidth, ScreenHeight, 96, 96, PixelFormats.Pbgra32, null);
                _Image = new Image { Width = ScreenWidth, Height = ScreenHeight, Stretch = Stretch.Fill, Source = _Bitmap };
                RenderOptions.SetBitmapScalingMode(_Image, BitmapScalingMode.NearestNeighbor);
            }
            if (canvas.Children.Count != 1 || !ReferenceEquals(canvas.Children[0], _Image))
            {
                canvas.Children.Clear();
                canvas.Children.Add(_Image);
                Canvas.SetLeft(_Image, 0);
                Canvas.SetTop(_Image, 0);
            }
            if (!ReferenceEquals(canvas.Background, BlackBrush)) canvas.Background = BlackBrush;
        }

        static SolidColorBrush CreateBlackBrush()
        {
            SolidColorBrush brush = new SolidColorBrush(Colors.Black);
            brush.Freeze();
            return brush;
        }
    }
}
