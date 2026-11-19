using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OOPGames
{
    internal sealed class G10_TetrisRenderer
    {
        const int Width = 160;
        const int Height = 144;
        const int CellSize = 6;
        const int BoardX = 10;
        const int BoardY = 18;

        static readonly Color[] PieceColors =
        {
            Color.FromRgb(62, 215, 224),
            Color.FromRgb(239, 209, 94),
            Color.FromRgb(180, 119, 222),
            Color.FromRgb(115, 214, 143),
            Color.FromRgb(235, 105, 117),
            Color.FromRgb(105, 145, 232),
            Color.FromRgb(236, 157, 83)
        };

        static readonly Color Background = Color.FromRgb(10, 19, 30);
        static readonly Color BoardBackground = Color.FromRgb(15, 30, 43);
        static readonly Color Grid = Color.FromRgb(30, 49, 61);
        static readonly Color Text = Color.FromRgb(210, 232, 210);
        static readonly Color Accent = Color.FromRgb(135, 192, 119);

        public void Draw(Canvas canvas, G10_TetrisSession session)
        {
            canvas.Children.Clear();
            B5RoomDraw.Rect(canvas, 0, 0, Width, Height, Background);
            B5RoomDraw.Text(canvas, "BLOCKFALL", 5, 3, 9, Accent);
            B5RoomDraw.Text(canvas, "SCORE " + session.Score.ToString("D6"), 5, 12, 7, Text);

            B5RoomDraw.Rect(canvas, BoardX - 2, BoardY - 2,
                G10_TetrisConfig.Columns * CellSize + 4,
                G10_TetrisConfig.Rows * CellSize + 4,
                Color.FromRgb(59, 91, 83));
            B5RoomDraw.Rect(canvas, BoardX, BoardY,
                G10_TetrisConfig.Columns * CellSize,
                G10_TetrisConfig.Rows * CellSize,
                BoardBackground);

            for (int y = 0; y < G10_TetrisConfig.Rows; y++)
            {
                for (int x = 0; x < G10_TetrisConfig.Columns; x++)
                {
                    byte value = session.GetCell(x, y);
                    double px = BoardX + x * CellSize;
                    double py = BoardY + y * CellSize;
                    if (value == 0)
                    {
                        B5RoomDraw.Rect(canvas, px, py, CellSize - 0.4, CellSize - 0.4, Grid);
                    }
                    else
                    {
                        DrawBlock(canvas, px, py, value);
                    }
                }
            }

            B5RoomDraw.Text(canvas, "NEXT", 82, 22, 8, Accent);
            DrawPreview(canvas, session.NextPiece);
            B5RoomDraw.Text(canvas, "LINES", 82, 66, 7, Text);
            B5RoomDraw.Text(canvas, session.Lines.ToString("D3"), 82, 76, 9, Text);
            B5RoomDraw.Text(canvas, "LEVEL", 82, 96, 7, Text);
            B5RoomDraw.Text(canvas, session.Level.ToString("D2"), 82, 106, 9, Text);

            if (session.IsGameOver)
            {
                B5RoomDraw.Rect(canvas, 20, 58, 120, 29, Color.FromArgb(238, 6, 14, 23));
                B5RoomDraw.Text(canvas, "GAME OVER", 46, 63, 10, Color.FromRgb(246, 139, 113));
                B5RoomDraw.Text(canvas, "PRESS ENTER", 45, 77, 8, Text);
            }
            else
            {
                B5RoomDraw.Text(canvas, "ARROWS MOVE", 82, 122, 6, Text);
                B5RoomDraw.Text(canvas, "UP/Z TURN", 82, 131, 6, Text);
            }
        }

        void DrawPreview(Canvas canvas, G10_Tetromino piece)
        {
            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 4; x++)
                    if (G10_TetrominoShapes.IsPreviewCell(piece, x, y))
                        DrawBlock(canvas, 84 + x * 7, 34 + y * 7, (byte)((int)piece + 1));
        }

        void DrawBlock(Canvas canvas, double x, double y, byte value)
        {
            Color color = PieceColors[value - 1];
            B5RoomDraw.Rect(canvas, x, y, CellSize - 0.45, CellSize - 0.45, color);
            B5RoomDraw.Rect(canvas, x + 0.8, y + 0.8, CellSize - 2.1, 0.55,
                Color.FromRgb(255, 242, 211));
            B5RoomDraw.Rect(canvas, x + 0.8, y + CellSize - 1.4, CellSize - 2.1, 0.55,
                Color.FromRgb(40, 55, 62));
        }
    }
}
