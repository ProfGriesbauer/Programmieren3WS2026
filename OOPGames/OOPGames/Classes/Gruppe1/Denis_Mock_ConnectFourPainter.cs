using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    public sealed class Gruppe1_ConnectFourPainter : IG1_ConnectFourPainter
    {
        public string Name { get { return "Gruppe 1 Connect Four Painter"; } }

        public int ColumnAt(int x, int y)
        {
            return G1_ConnectFourLayout.ColumnAt(x, y);
        }

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            I4GWFeld field = currentField as I4GWFeld;
            if (field == null)
            {
                canvas.Children.Clear();
                canvas.Background = Brushes.White;
                return;
            }

            canvas.Children.Clear();
            canvas.Background = Brushes.White;

            int winner = G1_ConnectFourLogic.Winner(field);
            string heading = winner > 0
                ? "Spieler " + winner + " hat gewonnen!"
                : G1_ConnectFourLogic.IsFull(field) ? "Unentschieden!" : "Klick in eine Spalte oder Taste 1-7";
            AddText(canvas, heading, G1_ConnectFourLayout.Left, 8, 14, Brushes.Black);

            for (int column = 0; column < G1_ConnectFourLogic.Columns; column++)
            {
                AddText(canvas, (column + 1).ToString(),
                    G1_ConnectFourLayout.Left + column * G1_ConnectFourLayout.CellSize
                        + G1_ConnectFourLayout.CellSize / 2 - 4,
                    G1_ConnectFourLayout.Top - 20, 14, Brushes.Black);
            }

            Rectangle board = new Rectangle
            {
                Width = G1_ConnectFourLogic.Columns * G1_ConnectFourLayout.CellSize,
                Height = G1_ConnectFourLogic.Rows * G1_ConnectFourLayout.CellSize,
                Fill = Brushes.RoyalBlue,
                Stroke = Brushes.DarkBlue,
                StrokeThickness = 2
            };
            Canvas.SetLeft(board, G1_ConnectFourLayout.Left);
            Canvas.SetTop(board, G1_ConnectFourLayout.Top);
            canvas.Children.Add(board);

            for (int row = 0; row < G1_ConnectFourLogic.Rows; row++)
            {
                for (int column = 0; column < G1_ConnectFourLogic.Columns; column++)
                {
                    int player = field[row, column];
                    Brush fill = player == 1 ? Brushes.Red
                        : player == 2 ? Brushes.Gold
                        : Brushes.WhiteSmoke;
                    Brush stroke = player == 1 ? Brushes.DarkRed
                        : player == 2 ? Brushes.DarkGoldenrod
                        : Brushes.LightSteelBlue;
                    Ellipse disc = new Ellipse
                    {
                        Width = G1_ConnectFourLayout.CellSize - 10,
                        Height = G1_ConnectFourLayout.CellSize - 10,
                        Fill = fill,
                        Stroke = stroke,
                        StrokeThickness = 2
                    };
                    Canvas.SetLeft(disc,
                        G1_ConnectFourLayout.Left + column * G1_ConnectFourLayout.CellSize + 5);
                    Canvas.SetTop(disc,
                        G1_ConnectFourLayout.Top + row * G1_ConnectFourLayout.CellSize + 5);
                    canvas.Children.Add(disc);
                }
            }
        }

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            PaintGameField(canvas, currentField);
        }

        private static void AddText(Canvas canvas, string text, double left, double top,
            double fontSize, Brush foreground)
        {
            TextBlock label = new TextBlock
            {
                Text = text,
                FontSize = fontSize,
                Foreground = foreground
            };
            Canvas.SetLeft(label, left);
            Canvas.SetTop(label, top);
            canvas.Children.Add(label);
        }
    }
}
