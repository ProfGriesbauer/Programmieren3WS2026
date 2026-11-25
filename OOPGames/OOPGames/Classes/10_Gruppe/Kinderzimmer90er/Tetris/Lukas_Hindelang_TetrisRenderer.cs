using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OOPGames
{
    internal enum G10_TextAlign
    {
        Left,
        Center,
        Right
    }

    // Frozen brushes for one block colour: bevel light/dark, face gradient, ghost and trail.
    internal sealed class G10_TetrisBlockStyle
    {
        public G10_TetrisBlockStyle(Color color)
        {
            Light = Freeze(new SolidColorBrush(Mix(color, Colors.White, 0.5)));
            Dark = Freeze(new SolidColorBrush(Mix(color, Colors.Black, 0.45)));
            Face = Freeze(new LinearGradientBrush(
                Mix(color, Colors.White, 0.18), Mix(color, Colors.Black, 0.12), new Point(0, 0), new Point(0.4, 1)));
            GhostFill = Freeze(new SolidColorBrush(Color.FromArgb(34, color.R, color.G, color.B)));
            GhostPen = new Pen(Freeze(new SolidColorBrush(Color.FromArgb(170, color.R, color.G, color.B))), 0.4);
            GhostPen.Freeze();
            Trail = Freeze(new LinearGradientBrush(
                Color.FromArgb(0, color.R, color.G, color.B), Color.FromArgb(120, color.R, color.G, color.B), 90));
        }

        public Brush Light { get; }
        public Brush Dark { get; }
        public Brush Face { get; }
        public Brush GhostFill { get; }
        public Pen GhostPen { get; }
        public Brush Trail { get; }

        public static Color Mix(Color from, Color to, double amount)
        {
            return Color.FromRgb(
                (byte)(from.R + (to.R - from.R) * amount),
                (byte)(from.G + (to.G - from.G) * amount),
                (byte)(from.B + (to.B - from.B) * amount));
        }

        static T Freeze<T>(T brush) where T : Brush
        {
            brush.Freeze();
            return brush;
        }
    }

    internal sealed class G10_TetrisRenderer
    {
        public const int Width = 160;
        public const int Height = 144;

        const double Cell = 6.4;
        const double BoardX = 48;
        const double BoardY = 8;
        const double BoardWidth = Cell * G10_TetrisConfig.Columns;
        const double BoardHeight = Cell * G10_TetrisConfig.VisibleRows;
        const double FrameSize = 2.2;
        const double PanelWidth = 39.8;
        const double LeftX = 3;
        const double RightX = Width - 3 - PanelWidth;
        const double PanelTop = BoardY - FrameSize;
        const double PanelBottom = BoardY + BoardHeight + FrameSize;
        const double Bevel = 1.05;
        const double GameOverFillSeconds = 0.6;

        // Panel heights per column; the remaining height is spread evenly as gaps.
        static readonly double[] LeftPanels = { 32, 22, 28, 22, 12 };
        static readonly double[] RightPanels = { 66, 22, 36 };

        static readonly Typeface LabelFont = new Typeface(
            new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
        static readonly Typeface NumberFont = new Typeface(
            new FontFamily("Consolas"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);

        static readonly Brush Background = Linear(Color.FromRgb(22, 26, 58), Color.FromRgb(8, 10, 26), 90);
        static readonly Brush BackGlow = Radial(Color.FromArgb(70, 92, 112, 255), Color.FromArgb(0, 40, 50, 140));
        static readonly Brush WellFrame = Linear(Color.FromRgb(118, 132, 214), Color.FromRgb(46, 52, 112), 90);
        static readonly Brush Well = Linear(Color.FromRgb(9, 12, 28), Color.FromRgb(17, 21, 46), 90);
        static readonly Brush WellShade = Linear(Color.FromArgb(110, 0, 0, 0), Color.FromArgb(0, 0, 0, 0), 90);
        static readonly Brush PanelFill = Linear(Color.FromArgb(235, 30, 36, 76), Color.FromArgb(235, 18, 22, 50), 90);
        static readonly Pen PanelPen = FrozenPen(Color.FromRgb(74, 86, 160), 0.45);
        static readonly Pen GridPen = FrozenPen(Color.FromArgb(18, 255, 255, 255), 0.2);
        static readonly Pen DividerPen = FrozenPen(Color.FromArgb(70, 120, 136, 220), 0.3);
        static readonly Brush Label = Solid(Color.FromRgb(140, 154, 222));
        static readonly Brush Value = Solid(Color.FromRgb(240, 244, 255));
        static readonly Brush Muted = Solid(Color.FromRgb(112, 122, 178));
        static readonly Brush KeyText = Solid(Color.FromRgb(255, 206, 102));
        static readonly Brush Gold = Linear(Color.FromRgb(255, 232, 140), Color.FromRgb(240, 150, 50), 90);
        static readonly Brush Go = Solid(Color.FromRgb(110, 232, 160));
        static readonly Brush Shadow = Solid(Color.FromArgb(170, 0, 0, 0));
        static readonly Brush White = Solid(Colors.White);
        static readonly Brush ProgressTrack = Solid(Color.FromRgb(12, 15, 36));
        static readonly Brush ProgressFill = Linear(Color.FromRgb(120, 236, 255), Color.FromRgb(90, 120, 255), 0);
        static readonly Brush Dim = Solid(Color.FromArgb(150, 9, 12, 28));
        static readonly Brush OverlayFill = Solid(Color.FromArgb(215, 8, 10, 24));
        static readonly Brush PausedFill = Linear(Color.FromRgb(20, 24, 54), Color.FromRgb(10, 12, 30), 90);
        static readonly Brush Shine = Solid(Color.FromArgb(150, 255, 255, 255));
        static readonly Dictionary<G10_TetrominoKind, Color> PieceColors = new Dictionary<G10_TetrominoKind, Color>
        {
            { G10_TetrominoKind.I, Color.FromRgb(66, 214, 236) },
            { G10_TetrominoKind.O, Color.FromRgb(248, 208, 72) },
            { G10_TetrominoKind.T, Color.FromRgb(178, 106, 232) },
            { G10_TetrominoKind.S, Color.FromRgb(104, 214, 112) },
            { G10_TetrominoKind.Z, Color.FromRgb(240, 86, 100) },
            { G10_TetrominoKind.J, Color.FromRgb(74, 128, 240) },
            { G10_TetrominoKind.L, Color.FromRgb(248, 150, 58) }
        };
        static readonly G10_TetrisBlockStyle GrayStyle =new G10_TetrisBlockStyle(Color.FromRgb(96, 102, 128));

        readonly Dictionary<Color, G10_TetrisBlockStyle> _Styles = new Dictionary<Color, G10_TetrisBlockStyle>();
        readonly G10_TetrisEffects _Effects = new G10_TetrisEffects();
        G10_DrawingVisualHost _Host;

        public void Draw(Canvas canvas, IG10_TetrisView session)
        {
            EnsureHost(canvas);
            _Effects.Attach(session);
            _Effects.Update(session.PlaySeconds);
            _Host.Render(delegate(DrawingContext drawing)
            {
                drawing.PushClip(new RectangleGeometry(new Rect(0, 0, Width, Height)));
                DrawBackground(drawing);
                DrawWell(drawing);
                DrawStack(drawing, session);
                DrawTrails(drawing, session);
                DrawActivePiece(drawing, session);
                DrawClearFlash(drawing, session);
                DrawBanners(drawing, session);
                DrawLeftPanels(drawing, session);
                DrawRightPanels(drawing, session);
                DrawStateOverlay(drawing, session);
                drawing.Pop();
            });
        }

        void EnsureHost(Canvas canvas)
        {
            if (_Host != null && canvas.Children.Contains(_Host)) return;

            _Host = new G10_DrawingVisualHost { Width = Width, Height = Height };
            canvas.Children.Clear();
            canvas.Children.Add(_Host);
        }

        static void DrawBackground(DrawingContext drawing)
        {
            drawing.DrawRectangle(Background, null, new Rect(0, 0, Width, Height));
            drawing.DrawEllipse(BackGlow, null, new Point(Width / 2.0, Height / 2.0), 70, 66);
        }

        static void DrawWell(DrawingContext drawing)
        {
            drawing.DrawRoundedRectangle(WellFrame, null, new Rect(
                BoardX - FrameSize, BoardY - FrameSize, BoardWidth + FrameSize * 2, BoardHeight + FrameSize * 2), 1.6, 1.6);
            drawing.DrawRectangle(Well, null, new Rect(BoardX, BoardY, BoardWidth, BoardHeight));
            for (int column = 1; column < G10_TetrisConfig.Columns; column++)
                drawing.DrawLine(GridPen, new Point(BoardX + column * Cell, BoardY),
                    new Point(BoardX + column * Cell, BoardY + BoardHeight));
            for (int row = 1; row < G10_TetrisConfig.VisibleRows; row++)
                drawing.DrawLine(GridPen, new Point(BoardX, BoardY + row * Cell),
                    new Point(BoardX + BoardWidth, BoardY + row * Cell));
            drawing.DrawRectangle(WellShade, null, new Rect(BoardX, BoardY, BoardWidth, 2.5));
        }

        void DrawStack(DrawingContext drawing, IG10_TetrisView session)
        {
            if (session.State == G10_TetrisState.Paused) return;

            // On game over the stack turns grey from the bottom up.
            int grayRows = session.State == G10_TetrisState.GameOver
                ? (int)Math.Ceiling(session.StateSeconds / GameOverFillSeconds * G10_TetrisConfig.VisibleRows)
                : 0;
            bool clearing = session.State == G10_TetrisState.Clearing;
            bool hideClearing = clearing && ClearProgress(session) >= 0.5;

            for (int row = G10_TetrisConfig.HiddenRows; row < G10_TetrisConfig.Rows; row++)
            {
                if (hideClearing && Contains(session.ClearingRows, row)) continue;
                bool gray = G10_TetrisConfig.Rows - row <= grayRows;
                for (int column = 0; column < G10_TetrisConfig.Columns; column++)
                {
                    G10_Tetromino shape = session.GetCell(column, row);
                    if (shape == null) continue;
                    DrawBlock(drawing, CellX(column), CellY(row), Cell, gray ? GrayStyle : StyleFor(shape));
                }
            }
        }

        void DrawTrails(DrawingContext drawing, IG10_TetrisView session)
        {
            foreach (G10_TetrisDropTrail trail in _Effects.Trails)
            {
                int fromRow = Math.Max(G10_TetrisConfig.HiddenRows, trail.FromRow);
                if (trail.ToRow <= fromRow) continue;
                double age = (session.PlaySeconds - trail.StartSeconds) / G10_TetrisEffects.TrailSeconds;
                drawing.PushOpacity(Math.Max(0, 1 - age));
                drawing.DrawRectangle(StyleFor(trail.Kind).Trail, null, new Rect(
                    CellX(trail.Column) + 0.6, CellY(fromRow), Cell - 1.2, (trail.ToRow - fromRow) * Cell));
                drawing.Pop();
            }
        }

        void DrawActivePiece(DrawingContext drawing, IG10_TetrisView session)
        {
            IG10_FallingPieceView piece = session.CurrentPiece;
            if (piece == null || session.State == G10_TetrisState.Paused) return;

            G10_TetrisBlockStyle style = session.State == G10_TetrisState.GameOver ? GrayStyle : StyleFor(piece.Shape);
            if (session.State == G10_TetrisState.Playing && session.GhostY != piece.Y)
            {
                foreach (G10_TetrominoCell cell in piece.Cells)
                {
                    int row = session.GhostY + cell.Y;
                    if (row < G10_TetrisConfig.HiddenRows) continue;
                    drawing.DrawRoundedRectangle(style.GhostFill, style.GhostPen, new Rect(
                        CellX(piece.X + cell.X) + 0.55, CellY(row) + 0.55, Cell - 1.1, Cell - 1.1), 0.6, 0.6);
                }
            }

            foreach (G10_TetrominoCell cell in piece.Cells)
            {
                int row = piece.Y + cell.Y;
                if (row < G10_TetrisConfig.HiddenRows) continue;
                DrawBlock(drawing, CellX(piece.X + cell.X), CellY(row), Cell, style);
            }

            // The piece lightens while the lock delay runs out.
            if (session.State != G10_TetrisState.Playing || session.LockProgress <= 0) return;
            drawing.PushOpacity(0.35 * session.LockProgress);
            foreach (G10_TetrominoCell cell in piece.Cells)
            {
                int row = piece.Y + cell.Y;
                if (row >= G10_TetrisConfig.HiddenRows)
                    drawing.DrawRectangle(White, null, new Rect(CellX(piece.X + cell.X) + 0.15, CellY(row) + 0.15, Cell - 0.3, Cell - 0.3));
            }
            drawing.Pop();
        }

        // First half: the full rows flash. Second half: a white bar shrinks to the middle.
        static void DrawClearFlash(DrawingContext drawing, IG10_TetrisView session)
        {
            if (session.State != G10_TetrisState.Clearing) return;

            double progress = ClearProgress(session);
            foreach (int row in session.ClearingRows)
            {
                if (row < G10_TetrisConfig.HiddenRows) continue;
                if (progress < 0.5)
                {
                    drawing.PushOpacity(0.35 + 0.45 * Math.Abs(Math.Sin(progress * Math.PI * 4)));
                    drawing.DrawRectangle(White, null, new Rect(BoardX, CellY(row), BoardWidth, Cell));
                }
                else
                {
                    double shrink = (progress - 0.5) / 0.5;
                    double width = BoardWidth * (1 - shrink);
                    drawing.PushOpacity(0.85 * (1 - shrink * 0.6));
                    drawing.DrawRectangle(White, null, new Rect(
                        BoardX + (BoardWidth - width) / 2, CellY(row) + Cell * 0.15, width, Cell * 0.7));
                }
                drawing.Pop();
            }
        }

        void DrawBanners(DrawingContext drawing, IG10_TetrisView session)
        {
            if (session.State == G10_TetrisState.Paused) return;

            foreach (G10_TetrisBanner banner in _Effects.Banners)
            {
                double age = session.PlaySeconds - banner.StartSeconds;
                double fade = age < 0.1 ? age / 0.1 : age > 0.9 ? (G10_TetrisEffects.BannerSeconds - age) / 0.4 : 1;
                double pop = 1 + 0.35 * Math.Max(0, 1 - age / 0.16);
                double centerX = BoardX + BoardWidth / 2;
                double y = BoardY + (banner.Slot == G10_TetrisBannerSlot.Clear ? 34 : 62) - age * 5;
                Brush titleBrush = StyleFor(banner.Color).Light;

                drawing.PushOpacity(Math.Max(0, Math.Min(1, fade)));
                drawing.DrawRoundedRectangle(Dim, null, new Rect(BoardX + 2, y - 1.5, BoardWidth - 4, 16), 2, 2);
                drawing.PushTransform(new ScaleTransform(pop, pop, centerX, y + 5));
                Text(drawing, banner.Title, centerX + 0.5, y + 0.5, 7.4, Shadow, LabelFont, G10_TextAlign.Center);
                Text(drawing, banner.Title, centerX, y, 7.4, titleBrush, LabelFont, G10_TextAlign.Center);
                drawing.Pop();
                Text(drawing, banner.Detail, centerX, y + 9.2, 3.4, Value, LabelFont, G10_TextAlign.Center);
                drawing.Pop();
            }
        }

        void DrawLeftPanels(DrawingContext drawing, IG10_TetrisView session)
        {
            Rect hold = PanelRect(LeftX, LeftPanels, 0);
            DrawPanel(drawing, hold, "HOLD");
            if (session.HeldPiece != null)
            {
                // Faded while hold is used up for the current piece.
                drawing.PushOpacity(session.CanHold ? 1 : 0.35);
                DrawPreview(drawing, session.HeldPiece, hold.X + hold.Width / 2, hold.Y + 19, 5, StyleFor(session.HeldPiece));
                drawing.Pop();
            }

            DrawValuePanel(drawing, PanelRect(LeftX, LeftPanels, 1), "SCORE", session.Score.ToString("D7"));

            Rect level = PanelRect(LeftX, LeftPanels, 2);
            DrawValuePanel(drawing, level, "LEVEL", session.Level.ToString(CultureInfo.InvariantCulture));
            double progress = session.Level >= G10_TetrisConfig.MaxLevel ? 1 :
                (session.Lines % G10_TetrisConfig.LinesPerLevel) / (double)G10_TetrisConfig.LinesPerLevel;
            Rect track = new Rect(level.X + 5, level.Bottom - 6, level.Width - 10, 2.2);
            drawing.DrawRoundedRectangle(ProgressTrack, null, track, 1.1, 1.1);
            if (progress > 0)
                drawing.DrawRoundedRectangle(ProgressFill, null,
                    new Rect(track.X, track.Y, Math.Max(2.2, track.Width * progress), track.Height), 1.1, 1.1);

            DrawValuePanel(drawing, PanelRect(LeftX, LeftPanels, 3), "LINES", session.Lines.ToString("D3"));

            Rect time = PanelRect(LeftX, LeftPanels, 4);
            DrawPanelBox(drawing, time);
            int seconds = (int)session.PlaySeconds;
            Text(drawing, "TIME", time.X + 3.5, time.Y + 3.4, 3.4, Label, LabelFont, G10_TextAlign.Left);
            Text(drawing, (seconds / 60).ToString("D2") + ":" + (seconds % 60).ToString("D2"),
                time.Right - 3.5, time.Y + 2.6, 4.6, Value, NumberFont, G10_TextAlign.Right);
        }

        void DrawRightPanels(DrawingContext drawing, IG10_TetrisView session)
        {
            Rect next = PanelRect(RightX, RightPanels, 0);
            DrawPanel(drawing, next, "NEXT");
            double centerX = next.X + next.Width / 2;
            IReadOnlyList<G10_Tetromino> pieces = session.NextPieces;
            DrawPreview(drawing, pieces[0], centerX, next.Y + 20, 5, StyleFor(pieces[0]));
            drawing.DrawLine(DividerPen, new Point(next.X + 6, next.Y + 32), new Point(next.Right - 6, next.Y + 32));
            for (int i = 1; i < G10_TetrisConfig.PreviewCount; i++)
                DrawPreview(drawing, pieces[i], centerX, next.Y + 30 + i * 13.5, 3.7, StyleFor(pieces[i]));

            DrawValuePanel(drawing, PanelRect(RightX, RightPanels, 1), "BEST", session.Best.ToString("D7"));

            Rect keys = PanelRect(RightX, RightPanels, 2);
            DrawPanelBox(drawing, keys);
            string[,] bindings =
            {
                { "← →", "MOVE" },
                { "↑  Z", "TURN" },
                { "↓", "SOFT" },
                { "SPACE", "DROP" },
                { "C", "HOLD" },
                { "ENTER", "PAUSE" }
            };
            for (int i = 0; i < bindings.GetLength(0); i++)
            {
                double y = keys.Y + 2.8 + i * 5.2;
                Text(drawing, bindings[i, 0], keys.X + 3.5, y, 3.1, KeyText, LabelFont, G10_TextAlign.Left);
                Text(drawing, bindings[i, 1], keys.Right - 3.5, y, 3.1, Muted, LabelFont, G10_TextAlign.Right);
            }
        }

        void DrawStateOverlay(DrawingContext drawing, IG10_TetrisView session)
        {
            double centerX = BoardX + BoardWidth / 2;
            double centerY = BoardY + BoardHeight / 2;
            Rect well = new Rect(BoardX, BoardY, BoardWidth, BoardHeight);

            if (session.State == G10_TetrisState.Ready)
            {
                bool go = session.StateSeconds >= G10_TetrisConfig.ReadySeconds * 0.62;
                double phase = go ? session.StateSeconds - G10_TetrisConfig.ReadySeconds * 0.62 : session.StateSeconds;
                double pop = 1 + 0.4 * Math.Max(0, 1 - phase / 0.18);
                drawing.DrawRectangle(Dim, null, well);
                drawing.PushTransform(new ScaleTransform(pop, pop, centerX, centerY));
                string word = go ? "GO!" : "READY";
                Text(drawing, word, centerX + 0.6, centerY - 7.4, 11, Shadow, LabelFont, G10_TextAlign.Center);
                Text(drawing, word, centerX, centerY - 8, 11, go ? Go : Value, LabelFont, G10_TextAlign.Center);
                drawing.Pop();
            }
            else if (session.State == G10_TetrisState.Paused)
            {
                drawing.DrawRectangle(PausedFill, null, well);
                Text(drawing, "PAUSE", centerX, centerY - 14, 9, Value, LabelFont, G10_TextAlign.Center);
                Text(drawing, "ENTER  RESUME", centerX, centerY + 2, 3.6, KeyText, LabelFont, G10_TextAlign.Center);
                Text(drawing, "X  QUIT", centerX, centerY + 8, 3.6, Muted, LabelFont, G10_TextAlign.Center);
            }
            else if (session.State == G10_TetrisState.GameOver && session.StateSeconds >= GameOverFillSeconds)
            {
                double fade = Math.Min(1, (session.StateSeconds - GameOverFillSeconds) / 0.25);
                drawing.PushOpacity(fade);
                drawing.DrawRectangle(OverlayFill, null, well);
                Text(drawing, "GAME", centerX, BoardY + 26, 10, Value, LabelFont, G10_TextAlign.Center);
                Text(drawing, "OVER", centerX, BoardY + 38, 10, Value, LabelFont, G10_TextAlign.Center);
                Text(drawing, "SCORE", centerX, BoardY + 60, 3.6, Label, LabelFont, G10_TextAlign.Center);
                Text(drawing, session.Score.ToString("D7"), centerX, BoardY + 65, 6.6, Value, NumberFont, G10_TextAlign.Center);
                if (session.IsNewBest)
                {
                    double pulse = 1 + 0.08 * Math.Sin(session.StateSeconds * 7);
                    drawing.PushTransform(new ScaleTransform(pulse, pulse, centerX, BoardY + 81));
                    Text(drawing, "NEW BEST!", centerX, BoardY + 77, 5.2, Gold, LabelFont, G10_TextAlign.Center);
                    drawing.Pop();
                }
                if (session.StateSeconds >= G10_TetrisConfig.GameOverInputDelay &&
                    (int)(session.StateSeconds * 2.2) % 2 == 0)
                    Text(drawing, "ENTER  RETRY", centerX, BoardY + 104, 3.8, KeyText, LabelFont, G10_TextAlign.Center);
                drawing.Pop();
            }
        }

        static void DrawValuePanel(DrawingContext drawing, Rect panel, string label, string value)
        {
            DrawPanel(drawing, panel, label);
            Text(drawing, value, panel.X + panel.Width / 2, panel.Y + 8.6, 6.4, Value, NumberFont, G10_TextAlign.Center);
        }

        static void DrawPanel(DrawingContext drawing, Rect panel, string label)
        {
            DrawPanelBox(drawing, panel);
            Text(drawing, label, panel.X + panel.Width / 2, panel.Y + 2.4, 3.6, Label, LabelFont, G10_TextAlign.Center);
        }

        static void DrawPanelBox(DrawingContext drawing, Rect panel)
        {
            drawing.DrawRoundedRectangle(PanelFill, PanelPen, panel, 2.2, 2.2);
        }

        static void DrawPreview(DrawingContext drawing, G10_Tetromino shape, double centerX, double centerY,
            double cell, G10_TetrisBlockStyle style)
        {
            int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
            foreach (G10_TetrominoCell c in shape.Cells(0))
            {
                minX = Math.Min(minX, c.X);
                maxX = Math.Max(maxX, c.X);
                minY = Math.Min(minY, c.Y);
                maxY = Math.Max(maxY, c.Y);
            }

            double left = centerX - (maxX - minX + 1) * cell / 2;
            double top = centerY - (maxY - minY + 1) * cell / 2;
            foreach (G10_TetrominoCell c in shape.Cells(0))
                DrawBlock(drawing, left + (c.X - minX) * cell, top + (c.Y - minY) * cell, cell, style);
        }

        // Classic bevelled block: light top-left edge, dark bottom-right edge, gradient face, shine.
        static void DrawBlock(DrawingContext drawing, double x, double y, double size, G10_TetrisBlockStyle style)
        {
            const double gap = 0.15;
            double bevel = Bevel * size / Cell;
            double inner = size - gap * 2;
            drawing.DrawRectangle(style.Dark, null, new Rect(x + gap, y + gap, inner, inner));
            drawing.DrawRectangle(style.Light, null, new Rect(x + gap, y + gap, inner - bevel, inner - bevel));
            drawing.DrawRectangle(style.Face, null, new Rect(x + gap + bevel, y + gap + bevel, inner - bevel * 2, inner - bevel * 2));
            drawing.DrawRectangle(Shine, null, new Rect(x + gap + bevel + 0.35, y + gap + bevel + 0.35, size * 0.18, size * 0.18));
        }

        G10_TetrisBlockStyle StyleFor(G10_Tetromino shape)
        {
            return StyleFor(shape.Kind);
        }

        G10_TetrisBlockStyle StyleFor(G10_TetrominoKind kind)
        {
            return StyleFor(PieceColors[kind]);
        }

        G10_TetrisBlockStyle StyleFor(Color color)
        {
            G10_TetrisBlockStyle style;
            if (!_Styles.TryGetValue(color, out style))
            {
                style = new G10_TetrisBlockStyle(color);
                _Styles.Add(color, style);
            }
            return style;
        }

        static Rect PanelRect(double x, double[] heights, int index)
        {
            double total = 0;
            foreach (double height in heights) total += height;
            double gap = (PanelBottom - PanelTop - total) / (heights.Length - 1);

            double y = PanelTop;
            for (int i = 0; i < index; i++) y += heights[i] + gap;
            return new Rect(x, y, PanelWidth, heights[index]);
        }

        static double ClearProgress(IG10_TetrisView session)
        {
            return Math.Min(1, session.StateSeconds / G10_TetrisConfig.LineClearSeconds);
        }

        static double CellX(int column) { return BoardX + column * Cell; }
        static double CellY(int row) { return BoardY + (row - G10_TetrisConfig.HiddenRows) * Cell; }

        static bool Contains(IReadOnlyList<int> rows, int row)
        {
            foreach (int candidate in rows)
                if (candidate == row) return true;
            return false;
        }

        static void Text(DrawingContext drawing, string text, double x, double y, double size,
            Brush brush, Typeface font, G10_TextAlign align)
        {
            FormattedText formatted = new FormattedText(
                text, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, font, size, brush, 1.0);
            if (align == G10_TextAlign.Center) x -= formatted.Width / 2;
            else if (align == G10_TextAlign.Right) x -= formatted.Width;
            drawing.DrawText(formatted, new Point(x, y));
        }

        static Brush Solid(Color color)
        {
            SolidColorBrush brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        static Brush Linear(Color first, Color second, double angle)
        {
            LinearGradientBrush brush = new LinearGradientBrush(first, second, angle);
            brush.Freeze();
            return brush;
        }

        static Brush Radial(Color center, Color edge)
        {
            RadialGradientBrush brush = new RadialGradientBrush(center, edge);
            brush.Freeze();
            return brush;
        }

        static Pen FrozenPen(Color color, double thickness)
        {
            Pen pen = new Pen(Solid(color), thickness);
            pen.Freeze();
            return pen;
        }
    }
}
