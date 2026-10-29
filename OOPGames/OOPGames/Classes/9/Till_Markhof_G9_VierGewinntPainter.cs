using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    public class G9_VierGewinntPainter : IG9_4GewinntPainter
    {
        private const double BoardLeft = 20;
        private const double BoardTop = 20;
        private const double CellSize = 60;

        public string Name { get { return "G9 Till Markhof Vier Gewinnt Painter"; } }

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            if (currentField is I4GWFeld field)
            {
                Paint4GewinntField(canvas, field);
            }
        }

        public void Paint4GewinntField(Canvas canvas, I4GWFeld currentField)
        {
            if (canvas == null || currentField == null)
            {
                return;
            }

            canvas.Children.Clear();
            canvas.Background = Brushes.White;

            Rectangle board = new Rectangle
            {
                Width = G9_VierGewinntBoard.Columns * CellSize,
                Height = G9_VierGewinntBoard.Rows * CellSize,
                Fill = Brushes.RoyalBlue
            };
            Canvas.SetLeft(board, BoardLeft);
            Canvas.SetTop(board, BoardTop);
            canvas.Children.Add(board);

            for (int row = 0; row < G9_VierGewinntBoard.Rows; row++)
            {
                for (int column = 0; column < G9_VierGewinntBoard.Columns; column++)
                {
                    Brush tokenColor = currentField[row, column] == 1
                        ? Brushes.Red
                        : currentField[row, column] == 2 ? Brushes.Yellow : Brushes.White;
                    Ellipse token = new Ellipse
                    {
                        Width = CellSize - 8,
                        Height = CellSize - 8,
                        Fill = tokenColor
                    };
                    Canvas.SetLeft(token, BoardLeft + column * CellSize + 4);
                    Canvas.SetTop(token, BoardTop + row * CellSize + 4);
                    canvas.Children.Add(token);
                }
            }
        }
    }
}
