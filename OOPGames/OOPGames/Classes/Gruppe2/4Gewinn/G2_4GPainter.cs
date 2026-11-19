using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    public class Gruppe2_ConnectFourPainter : IPaintGame2, IConnectFourPainter     //Klasse, die das Spielbrett für "Vier Gewinnt" von Gruppe 2 zeichnet und die Animation fallender Spielsteine verwaltet.
    {
        private const double FallDurationMilliseconds = 300;
        private readonly int[,] _observedCells =
            new int[Gruppe2_ConnectFourConstants.Rows, Gruppe2_ConnectFourConstants.Columns];
        private readonly Queue<FallingPiece> _pendingPieces = new Queue<FallingPiece>();
        private FallingPiece _fallingPiece;
        private DateTime _fallStartedAt;

        private sealed class FallingPiece
        {
            public FallingPiece(int row, int column, int playerNumber)
            {
                Row = row;
                Column = column;
                PlayerNumber = playerNumber;
            }

            public int Row { get; }
            public int Column { get; }
            public int PlayerNumber { get; }
        }

        public string Name { get { return "Gruppe 2 - 4G Painter"; } }   

        public int ColumnAt(int x, int y)
        {
            if (x < 0
                || x >= Gruppe2_ConnectFourConstants.Columns * Gruppe2_ConnectFourConstants.CellSize
                || y < 0
                || y >= Gruppe2_ConnectFourConstants.Rows * Gruppe2_ConnectFourConstants.CellSize)
            {
                return -1;
            }

            return x / Gruppe2_ConnectFourConstants.CellSize;
        }

        public void PaintGameField(Canvas canvas, IGameField currentField)     //Methode, die das Spielbrett auf einem Canvas zeichnet. Sie überprüft, ob das Canvas und das Spielbrett gültig sind, und zeichnet dann das Spielbrett, die Spielsteine und eine Nachricht für ein Unentschieden.
        {
            if (canvas == null || !(currentField is I4GWFeld field))     //Wenn das Canvas null ist oder das aktuelle Spielbrett nicht vom Typ I4GWFeld ist, wird die Methode beendet und nichts gezeichnet.
            {
                return;    //Wenn das Canvas null ist oder das aktuelle Spielbrett nicht vom Typ I4GWFeld ist, wird die Methode beendet und nichts gezeichnet.
            }

            ObserveField(field);
            DrawField(canvas, field);
        }

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            if (canvas == null || !(currentField is I4GWFeld field))
            {
                return;
            }

            ObserveField(field);
            if (_fallingPiece != null
                && (DateTime.UtcNow - _fallStartedAt).TotalMilliseconds >= FallDurationMilliseconds)
            {
                _fallingPiece = null;
            }

            if (_fallingPiece == null && _pendingPieces.Count > 0)
            {
                _fallingPiece = _pendingPieces.Dequeue();
                _fallStartedAt = DateTime.UtcNow;
            }

            DrawField(canvas, field);
        }

        private void ObserveField(I4GWFeld field)
        {
            bool hasRemovedPiece = false;
            for (int row = 0; row < Gruppe2_ConnectFourConstants.Rows; row++)
            {
                for (int column = 0; column < Gruppe2_ConnectFourConstants.Columns; column++)
                {
                    if (_observedCells[row, column] != field[row, column]
                        && field[row, column] == 0)
                    {
                        hasRemovedPiece = true;
                    }
                }
            }

            if (hasRemovedPiece)
            {
                _pendingPieces.Clear();
                _fallingPiece = null;
            }

            for (int row = 0; row < Gruppe2_ConnectFourConstants.Rows; row++)
            {
                for (int column = 0; column < Gruppe2_ConnectFourConstants.Columns; column++)
                {
                    int playerNumber = field[row, column];
                    if (!hasRemovedPiece
                        && _observedCells[row, column] == 0
                        && playerNumber > 0)
                    {
                        _pendingPieces.Enqueue(new FallingPiece(row, column, playerNumber));
                    }

                    _observedCells[row, column] = playerNumber;
                }
            }
        }

        private void DrawField(Canvas canvas, I4GWFeld field)
        {
            canvas.Children.Clear();     //Löscht alle vorhandenen Elemente auf dem Canvas, um das Spielbrett neu zu zeichnen.
            canvas.Background = Brushes.White;     //Setzt den Hintergrund des Canvas auf Weiß, um einen sauberen Hintergrund für das Spielbrett zu haben.

            int width = Gruppe2_ConnectFourConstants.Columns * Gruppe2_ConnectFourConstants.CellSize;     //Berechnet die Breite des Spielbretts basierend auf der Anzahl der Spalten und der Zellgröße 50x350 px
            int height = Gruppe2_ConnectFourConstants.Rows * Gruppe2_ConnectFourConstants.CellSize;     //Berechnet die Höhe des Spielbretts basierend auf der Anzahl der Reihen und der Zellgröße 50x300 px
            Rectangle board = new Rectangle     //Erstellt ein Rechteck, das das Spielbrett darstellt. Die Breite und Höhe werden auf die berechneten Werte gesetzt, und die Füllfarbe wird auf Schwarz gesetzt.
            {
                Width = width,
                Height = height,
                Fill = Brushes.Black     //Setzt die Füllfarbe des Rechtecks auf Schwarz, um das Spielbrett darzustellen.
            };
            canvas.Children.Add(board);     //Fügt das Rechteck dem Canvas hinzu, um das Spielbrett anzuzeigen.

            for (int row = 0; row < Gruppe2_ConnectFourConstants.Rows; row++)     //Schleife, die alle Reihen des Spielbretts durchläuft, um die Spielsteine zu zeichnen.
            {
                for (int column = 0; column < Gruppe2_ConnectFourConstants.Columns; column++)
                {
                    int playerNumber = IsPendingPiece(row, column) ? 0 : field[row, column];
                    Ellipse piece = new Ellipse     //Erstellt eine Ellipse, die einen Spielstein darstellt. Die Breite und Höhe werden auf die Zellgröße minus 8 gesetzt, um einen kleinen Abstand zum Rand zu haben. Die Füllfarbe wird basierend auf dem Spieler gesetzt.
                    {
                        Width = Gruppe2_ConnectFourConstants.CellSize - 8,     //Setzt die Breite der Ellipse auf die Zellgröße minus 8, um einen kleinen Abstand zum Rand zu haben.
                        Height = Gruppe2_ConnectFourConstants.CellSize - 8,
                        Fill = GetPlayerBrush(playerNumber)     //Setzt die Füllfarbe der Ellipse basierend auf dem Spieler, der die Zelle besetzt. 0 = leer, 1 = Spieler 1 (Magenta), 2 = Spieler 2 (Türkis)
                    };
                    Canvas.SetLeft(piece, column * Gruppe2_ConnectFourConstants.CellSize + 4);     //Setzt die x-Position der Ellipse basierend auf der Spalte und der Zellgröße, plus einem kleinen Abstand von 4 Pixeln zum Rand.
                    Canvas.SetTop(piece, row * Gruppe2_ConnectFourConstants.CellSize + 4);     //Setzt die y-Position der Ellipse basierend auf der Reihe und der Zellgröße, plus einem kleinen Abstand von 4 Pixeln zum Rand.
                    canvas.Children.Add(piece);
                }
            }

            if (_fallingPiece != null)
            {
                double progress = Math.Min(
                    1,
                    (DateTime.UtcNow - _fallStartedAt).TotalMilliseconds / FallDurationMilliseconds);
                double startTop = -Gruppe2_ConnectFourConstants.CellSize;
                double endTop = _fallingPiece.Row * Gruppe2_ConnectFourConstants.CellSize + 4;
                Ellipse fallingPiece = new Ellipse
                {
                    Width = Gruppe2_ConnectFourConstants.CellSize - 8,
                    Height = Gruppe2_ConnectFourConstants.CellSize - 8,
                    Fill = GetPlayerBrush(_fallingPiece.PlayerNumber)
                };
                Canvas.SetLeft(
                    fallingPiece,
                    _fallingPiece.Column * Gruppe2_ConnectFourConstants.CellSize + 4);
                Canvas.SetTop(fallingPiece, startTop + (endTop - startTop) * progress);
                canvas.Children.Add(fallingPiece);
            }

            if (_fallingPiece == null && _pendingPieces.Count == 0)
            {
                DrawWinningLines(canvas, field);
            }

            if (Gruppe2_ConnectFourLogic.IsDraw(field))     //Wenn das Spiel unentschieden ist, wird eine Nachricht angezeigt, die den Spielern mitteilt, dass das Spiel unentschieden ist.
            {
                TextBlock drawMessage = new TextBlock     //Erstellt einen TextBlock, der die Draw-Nachricht anzeigt.
                {
                    Text = "Draw",     //Setzt den Text des TextBlocks auf "Draw", um anzuzeigen, dass das Spiel unentschieden ist.
                    FontSize = 22,     //Setzt die Schriftgröße des Textes auf 22.
                    FontWeight = FontWeights.Bold,     //Setzt die Schriftgröße und das Gewicht des Textes.
                    Foreground = Brushes.Black,     //Setzt die Schriftfarbe des Textes auf Schwarz.
                    Background = Brushes.White,     //Setzt die Hintergrundfarbe des TextBlocks auf Weiß.
                    Padding = new Thickness(8)     //Setzt den Abstand zwischen dem Text und dem Rand des TextBlocks, um ihn besser lesbar zu machen.
                };
                Canvas.SetLeft(drawMessage, 75);     //Setzt die x-Position des TextBlocks auf 75 Pixel, um ihn in der Mitte des Spielbretts anzuzeigen.
                Canvas.SetTop(drawMessage, 100);     //Setzt die y-Position des TextBlocks auf 100 Pixel, um ihn in der Mitte des Spielbretts anzuzeigen.
                canvas.Children.Add(drawMessage);     //Fügt den TextBlock dem Canvas hinzu, um die Draw-Nachricht anzuzeigen.
            }
        }

        private static void DrawWinningLines(Canvas canvas, I4GWFeld field)
        {
            foreach (Gruppe2_ConnectFourLogic.WinningLine line
                in Gruppe2_ConnectFourLogic.FindWinningLines(field))
            {
                Line winningLine = new Line
                {
                    X1 = line.StartColumn * Gruppe2_ConnectFourConstants.CellSize
                        + Gruppe2_ConnectFourConstants.CellSize / 2.0,
                    Y1 = line.StartRow * Gruppe2_ConnectFourConstants.CellSize
                        + Gruppe2_ConnectFourConstants.CellSize / 2.0,
                    X2 = line.EndColumn * Gruppe2_ConnectFourConstants.CellSize
                        + Gruppe2_ConnectFourConstants.CellSize / 2.0,
                    Y2 = line.EndRow * Gruppe2_ConnectFourConstants.CellSize
                        + Gruppe2_ConnectFourConstants.CellSize / 2.0,
                    Stroke = Brushes.Gold,
                    StrokeThickness = 5,
                    StrokeStartLineCap = PenLineCap.Round,
                    StrokeEndLineCap = PenLineCap.Round
                };
                canvas.Children.Add(winningLine);
            }
        }

        private bool IsPendingPiece(int row, int column)
        {
            if (_fallingPiece != null
                && _fallingPiece.Row == row
                && _fallingPiece.Column == column)
            {
                return true;
            }

            foreach (FallingPiece piece in _pendingPieces)
            {
                if (piece.Row == row && piece.Column == column)
                {
                    return true;
                }
            }

            return false;
        }

        private static Brush GetPlayerBrush(int playerNumber)     //Methode, die die Füllfarbe für einen Spielstein basierend auf der Spielerzahl zurückgibt. Spieler 1 = Magenta, Spieler 2 = Türkis, leer = Hellgrau.
        {
            if (playerNumber == 1)
            {
                return Brushes.Magenta;
            }

            if (playerNumber == 2)
            {
                return Brushes.Turquoise;
            }

            return Brushes.LightGray;     //Wenn die Zelle leer ist, wird Hellgrau zurückgegeben, um anzuzeigen, dass kein Spieler die Zelle besetzt.
        }

    }
}
