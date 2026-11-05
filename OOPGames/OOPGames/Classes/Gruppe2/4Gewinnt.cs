using System;     //Bibliothek für grundlegende C# Funktionen
using System.Collections.Generic;     //Bibliothek für die Verwendung von Listen
using System.Windows;     //Bibliothek für die Erstellung von Windows-Anwendungen (WPF-Funktion für Fenster und UI)
using System.Windows.Controls;     //Bibliothek für die Verwendung von Steuerelementen wie Canvas
using System.Windows.Media;     //Bibliothek für die Verwendung von Farben und Pinseln
using System.Windows.Shapes;     //Bibliothek für die Verwendung von Formen wie Rechtecken und Ellipsen

namespace OOPGames     //Namespace für das Projekt OOPGames Damit liegen alle Klassen in einem gemeinsamen Bereich. Das verhindert Namenskonflikte mit anderen Klassen aus anderen Projekten oder Bibliotheken.
{
    internal static class Gruppe2_ConnectFourConstants     //Klasse mit Konstanten für das Spiel "Vier Gewinnt" von Gruppe 2. Diese Klasse enthält unveränderliche Werte, die im gesamten Spiel verwendet werden.
    {
        public const int Rows = 6;     //Anzahl der Reihen im Spielbrett
        public const int Columns = 7;     //Anzahl der Spalten im Spielbrett
        public const int CellSize = 50;     //Größe einer Zelle im Spielbrett in Pixeln
    }

    public class Gruppe2_ConnectFourField : I4GWFeld     //Klasse, die das Spielbrett für "Vier Gewinnt" von Gruppe 2 implementiert. Sie speichert den Zustand des Spielbretts und bietet Methoden zum Zugriff auf die Zellen.
    {
        private readonly int[,] _cells =     //ein zweidimensionales Array, das den Zustand des Spielbretts speichert. Jede Zelle kann den Wert 0 (leer), 1 (Spieler 1) oder 2 (Spieler 2) haben. readonly heißt, dass das Array kann nach dem Erstellen nicht neu zugewiesen werden, aber die Werte darin können verändert werden
            new int[Gruppe2_ConnectFourConstants.Rows, Gruppe2_ConnectFourConstants.Columns];     //Initialisierung des Arrays mit der Anzahl der Reihen und Spalten, die in den Konstanten definiert sind.

        public int this[int row, int column]     //Indexer-Property, der den Zugriff auf die Zellen des Spielbretts ermöglicht. Er überprüft, ob die angegebenen Indizes gültig sind, bevor er den Wert zurückgibt oder setzt. Wenn die Indizes ungültig sind, wird -1 zurückgegeben.
        {
            get     //Getter für den Indexer, der den Wert der Zelle an der angegebenen Position liest. Wenn die Position ungültig ist, wird -1 zurückgegeben.
            {
                if (!IsValidCell(row, column))
                {
                    return -1;
                }

                return _cells[row, column];
            }
            set     //Setter für den Indexer, der den Wert der Zelle an der angegebenen Position setzt. Wenn die Position ungültig ist, wird nichts getan.
            {
                if (IsValidCell(row, column))
                {
                    _cells[row, column] = value;     //nur gültige Positionen werden gesetzt. Ungültige Zellen werden ignoriert, um Fehler zu vermeiden.
                }
            }
        }

        public bool CanBePaintedBy(IPaintGame painter)     //Methode, die überprüft, ob das Spielbrett von einem bestimmten Painter (Zeichner) gezeichnet werden kann. In diesem Fall wird nur überprüft, ob der Painter nicht null ist.
        {
            return painter != null;
        }

        private static bool IsValidCell(int row, int column)     //Methode, die überprüft, ob die angegebenen Koordinaten innerhalb der Grenzen des Spielbretts liegen. Sie gibt true zurück, wenn die Koordinaten gültig sind, und false, wenn sie ungültig sind.
        {
            return row >= 0 && row < Gruppe2_ConnectFourConstants.Rows     //nicht negativ und kleiner als die maximale Anzahl der Reihen und Spalten.
                && column >= 0 && column < Gruppe2_ConnectFourConstants.Columns;     //wenn alles stimmt, ist das Feld gültig, sonst nicht.
        }
    }

    public class Gruppe2_ConnectFourMove : I4GWMove     //Move-Klasse für das Spiel "Vier Gewinnt" von Gruppe 2. Sie speichert die Informationen über einen Zug, einschließlich der Zeile, Spalte und Spielerzahl. Implementiert das I4GWMove-Interface.
    {
        public Gruppe2_ConnectFourMove(int row, int column, int playerNumber)
        {
            Row = row;     
            Column = column;
            PlayerNumber = playerNumber;
        }

        public int Row { get; }     //Reihe, in der der Zug gemacht wird

        public int Column { get; }     //Spalte, in der der Zug gemacht wird

        public int PlayerNumber { get; }     //Nummer des Spielers, der den Zug macht (1 oder 2)
    }     //Datenasatz, der später im Spiel verwendet wird, um die Züge der Spieler zu speichern und zu verarbeiten.

    public class Gruppe2_ConnectFourRules : IGameRules     //Regelklasse für das Spiel "Vier Gewinnt" von Gruppe 2. Sie implementiert das IGameRules-Interface und enthält die Logik zur Überprüfung von Zügen, Gewinnern und Unentschieden.
    {
        private readonly I4GWFeld _field = new Gruppe2_ConnectFourField();     //Instanz des Spielbretts, das als private Variable erzeugt wird.

        public string Name { get { return "Group 2 - Connect Four Rules"; } }     //Name der Regelklasse, die im Spiel angezeigt wird

        public I4GWFeld ConnectFourField { get { return _field; } }     //Property, die das Spielbrett zurückgibt. Sie ist vom Typ I4GWFeld, sodass sie von anderen Klassen verwendet werden kann, die das Interface implementieren.

        public IGameField CurrentField { get { return _field; } }     //Property, die das aktuelle Spielbrett zurückgibt. Sie ist vom Typ IGameField, sodass sie von anderen Klassen verwendet werden kann, die das Interface implementieren.

        public bool MovesPossible     //Property, die überprüft, ob noch Züge möglich sind. Sie gibt true zurück, wenn es noch freie Zellen gibt und kein Spieler gewonnen hat, sonst false.
        {
            get
            {
                if (CheckIfPLayerWon() > 0)
                {
                    return false;
                }

                for (int column = 0; column < Gruppe2_ConnectFourConstants.Columns; column++)
                {
                    if (Gruppe2_ConnectFourLogic.FindAvailableRow(_field, column) >= 0)     //liefert die nächste freie Zeile in dieser Spalte.
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public void DoMove(IPlayMove move)     //Methode, die einen Zug ausführt. Sie überprüft, ob der Zug gültig ist und ob noch Züge möglich sind. Wenn ja, wird der Zug auf dem Spielbrett ausgeführt.
        {
            if (!(move is I4GWMove connectFourMove)
                || connectFourMove.PlayerNumber < 1     //Wenn der Spieler nicht 1 oder 2 ist, kann kein Zug gemacht werden.
                || connectFourMove.PlayerNumber > 2
                || CheckIfPLayerWon() > 0)     //Wenn der Spieler gewonnen hat, kann kein Zug mehr gemacht werden.
            {
                return;
            }

            int availableRow = Gruppe2_ConnectFourLogic.FindAvailableRow(_field, connectFourMove.Column);    //liefert die nächste freie Stelle in dieser Spalte. Wenn keine freie Stelle vorhanden ist, wird -1 zurückgegeben.
            if (availableRow < 0 || connectFourMove.Row != availableRow)
            {
                return;     //Wenn die angegebene Zeile nicht der nächsten freien Zeile entspricht, wird der Zug nicht ausgeführt.
            }

            _field[availableRow, connectFourMove.Column] = connectFourMove.PlayerNumber;     //Der Zug wird auf dem Spielbrett ausgeführt, indem die Zelle mit der Spielerzahl gesetzt wird.
        }

        public void ClearField()     //Methode, die das Spielbrett zurücksetzt. Sie setzt alle Zellen auf 0 (leer) und bereitet das Spiel für einen neuen Durchgang vor.
        {
            for (int row = 0; row < Gruppe2_ConnectFourConstants.Rows; row++)
            {
                for (int column = 0; column < Gruppe2_ConnectFourConstants.Columns; column++)
                {
                    _field[row, column] = 0;
                }
            }
        }

        public int CheckIfPLayerWon()     //Methode, die überprüft, ob ein Spieler gewonnen hat. Sie ruft die statische Methode FindWinner der Logikklasse auf und gibt die Spielerzahl zurück, wenn ein Spieler gewonnen hat, sonst -1.
        {
            return Gruppe2_ConnectFourLogic.FindWinner(_field);     //liefert die Spielerzahl, wenn ein Spieler gewonnen hat, sonst -1.
        }

        public bool IsDraw     //Property, die überprüft, ob das Spiel unentschieden ist. Sie gibt true zurück, wenn keine Züge mehr möglich sind und kein Spieler gewonnen hat, sonst false.
        {
            get { return !MovesPossible && CheckIfPLayerWon() < 0; }
        }
    }

    internal static class Gruppe2_ConnectFourLogic     //Statische Logikklasse für das Spiel "Vier Gewinnt" von Gruppe 2. Sie enthält Methoden zur Überprüfung von Gewinnern, verfügbaren Zügen und Verbindungen von vier Spielsteinen. Diese Klasse ist intern, da sie nur innerhalb des Projekts verwendet wird.
    {
        private static readonly int[,] Directions =     //erlaubt die Überprüfung in vier Richtungen: horizontal, vertikal und diagonal. Jede Richtung wird durch ein Paar von Schritten (Zeilen- und Spaltenänderung) dargestellt.
        {
            { 0, 1 },     // horizontal
            { 1, 0 },     // vertical
            { 1, 1 },     // diagonal nach unten rechts
            { 1, -1 }     // diagonal nach unten links
        };

        public static bool ConnectsFour(I4GWFeld field, int row, int column, int playerNumber)     //Methode, die überprüft, ob ein Spieler vier Spielsteine in einer Reihe hat. Sie überprüft alle vier Richtungen und zählt die Anzahl der aufeinanderfolgenden Spielsteine des Spielers. Wenn die Anzahl größer oder gleich 4 ist, gibt sie true zurück, sonst false.
        {
            for (int direction = 0; direction < Directions.GetLength(0); direction++)
            {
                int rowStep = Directions[direction, 0];
                int columnStep = Directions[direction, 1];
                int count = 1     //Zählt den aktuellen Spielstein
                    + CountDirection(field, row, column, rowStep, columnStep, playerNumber)     //Zählt die Spielsteine in der positiven Richtung
                    + CountDirection(field, row, column, -rowStep, -columnStep, playerNumber);     //Zählt die Spielsteine in der negativen Richtung

                if (count >= 4)     //Wenn die Anzahl der aufeinanderfolgenden Spielsteine größer oder gleich 4 ist, hat der Spieler gewonnen.
                {
                    return true;
                }
            }

            return false;
        }

        public static int FindWinner(I4GWFeld field)     //Methode, die überprüft, ob ein Spieler gewonnen hat. Sie durchläuft alle Zellen des Spielbretts und ruft die Methode ConnectsFour auf, um zu überprüfen, ob ein Spieler vier Spielsteine in einer Reihe hat. Wenn ja, gibt sie die Spielerzahl zurück, sonst -1.
        {
            for (int row = 0; row < Gruppe2_ConnectFourConstants.Rows; row++)
            {
                for (int column = 0; column < Gruppe2_ConnectFourConstants.Columns; column++)
                {
                    int playerNumber = field[row, column];
                    if (playerNumber > 0
                        && ConnectsFour(field, row, column, playerNumber))     //Wenn die Zelle nicht leer ist und der Spieler vier Spielsteine in einer Reihe hat, wird die Spielerzahl zurückgegeben.
                    {
                        return playerNumber;     //Spielerzahl wird zurückgegeben.
                    }
                }
            }

            return -1;     //Wenn kein Spieler gewonnen hat, wird -1 zurückgegeben.
        }

        public static int FindAvailableRow(I4GWFeld field, int column)     //Methode, die die nächste verfügbare Zeile in einer Spalte findet. Sie durchläuft die Zeilen von unten nach oben und überprüft, ob die Zelle leer ist. Wenn ja, gibt sie die Zeilennummer zurück, sonst -1.
        {
            if (column < 0 || column >= Gruppe2_ConnectFourConstants.Columns)     //von unten nach oben, da die Spielsteine nach unten fallen. Wenn die Spalte ungültig ist, wird -1 zurückgegeben.
            {
                return -1;     //Wenn die Spalte ungültig ist, wird -1 zurückgegeben.
            }

            for (int row = Gruppe2_ConnectFourConstants.Rows - 1; row >= 0; row--)
            {
                if (field[row, column] == 0)
                {
                    return row;
                }
            }

            return -1;
        }

        private static int CountDirection(     //Methode, die die Anzahl der aufeinanderfolgenden Spielsteine eines Spielers in einer bestimmten Richtung zählt. Sie wird von der Methode ConnectsFour aufgerufen.
            I4GWFeld field,     //Das Spielbrett, auf dem die Überprüfung durchgeführt wird
            int row,     //Die Startzeile, von der aus die Überprüfung beginnt
            int column,     //Die Startspalte, von der aus die Überprüfung beginnt
            int rowStep,     //Die Zeilenänderung, die angibt, in welche Richtung die Überprüfung erfolgt (z. B. 1 für nach unten, -1 für nach oben)
            int columnStep,     //Die Spaltenänderung, die angibt, in welche Richtung die Überprüfung erfolgt (z. B. 1 für nach rechts, -1 für nach links)
            int playerNumber)    //Die Spielerzahl, die überprüft wird (1 oder 2)
        
        {
            int count = 0;     //Anzahl der aufeinanderfolgenden Spielsteine des Spielers wird auf 0 gesetzt, da noch keine Spielsteine gezählt wurden.
            int currentRow = row + rowStep;     //Startpunkt für die Überprüfung in der angegebenen Richtung
            int currentColumn = column + columnStep;     //Startpunkt für die Überprüfung in der angegebenen Richtung

            while (currentRow >= 0     //Solange die aktuelle Zeile und Spalte innerhalb der Grenzen des Spielbretts liegen und die Zelle dem Spieler gehört, wird gezählt.
                && currentRow < Gruppe2_ConnectFourConstants.Rows
                && currentColumn >= 0
                && currentColumn < Gruppe2_ConnectFourConstants.Columns
                && field[currentRow, currentColumn] == playerNumber)
            {
                count++;     //Zählt die aufeinanderfolgenden Spielsteine des Spielers in der angegebenen Richtung
                currentRow += rowStep;     //Bewegt sich in der angegebenen Richtung weiter, um die nächste Zelle zu überprüfen
                currentColumn += columnStep;     //Bewegt sich in der angegebenen Richtung weiter, um die nächste Zelle zu überprüfen
            }

            return count;
        }
    }

    public class Gruppe2_ConnectFourPainter : IPaintGame2, IConnectFourPainter     //Klasse, die das Spielbrett für "Vier Gewinnt" von Gruppe 2 zeichnet und die Animation fallender Spielsteine verwaltet.
    {
        private const double FallDurationMilliseconds = 300;
        private static readonly int[,] WinningLineDirections =
        {
            { 0, 1 },
            { 1, 0 },
            { 1, 1 },
            { 1, -1 }
        };
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

        public string Name { get { return "Group 2 - Connect Four Painter"; } }   

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

            DrawWinningLines(canvas, field);

            if (IsDraw(field))     //Wenn das Spiel unentschieden ist, wird eine Nachricht angezeigt, die den Spielern mitteilt, dass das Spiel unentschieden ist.
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
            for (int row = 0; row < Gruppe2_ConnectFourConstants.Rows; row++)
            {
                for (int column = 0; column < Gruppe2_ConnectFourConstants.Columns; column++)
                {
                    int playerNumber = field[row, column];
                    if (playerNumber <= 0)
                    {
                        continue;
                    }

                    for (int direction = 0; direction < WinningLineDirections.GetLength(0); direction++)
                    {
                        int rowStep = WinningLineDirections[direction, 0];
                        int columnStep = WinningLineDirections[direction, 1];
                        int previousRow = row - rowStep;
                        int previousColumn = column - columnStep;
                        if (IsInsideBoard(previousRow, previousColumn)
                            && field[previousRow, previousColumn] == playerNumber)
                        {
                            continue;
                        }

                        int endRow = row;
                        int endColumn = column;
                        int count = 1;
                        while (IsInsideBoard(endRow + rowStep, endColumn + columnStep)
                            && field[endRow + rowStep, endColumn + columnStep] == playerNumber)
                        {
                            endRow += rowStep;
                            endColumn += columnStep;
                            count++;
                        }

                        if (count < 4)
                        {
                            continue;
                        }

                        Line winningLine = new Line
                        {
                            X1 = column * Gruppe2_ConnectFourConstants.CellSize
                                + Gruppe2_ConnectFourConstants.CellSize / 2.0,
                            Y1 = row * Gruppe2_ConnectFourConstants.CellSize
                                + Gruppe2_ConnectFourConstants.CellSize / 2.0,
                            X2 = endColumn * Gruppe2_ConnectFourConstants.CellSize
                                + Gruppe2_ConnectFourConstants.CellSize / 2.0,
                            Y2 = endRow * Gruppe2_ConnectFourConstants.CellSize
                                + Gruppe2_ConnectFourConstants.CellSize / 2.0,
                            Stroke = Brushes.Gold,
                            StrokeThickness = 5,
                            StrokeStartLineCap = PenLineCap.Round,
                            StrokeEndLineCap = PenLineCap.Round
                        };
                        canvas.Children.Add(winningLine);
                    }
                }
            }
        }

        private static bool IsInsideBoard(int row, int column)
        {
            return row >= 0
                && row < Gruppe2_ConnectFourConstants.Rows
                && column >= 0
                && column < Gruppe2_ConnectFourConstants.Columns;
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

        private static bool IsDraw(I4GWFeld field)     //Methode, die überprüft, ob das Spiel unentschieden ist. Sie ruft die Methode FindWinner auf, um zu überprüfen, ob ein Spieler gewonnen hat. Wenn kein Spieler gewonnen hat und keine Züge mehr möglich sind, gibt sie true zurück, sonst false.
        {
            if (Gruppe2_ConnectFourLogic.FindWinner(field) > 0)     //Wenn ein Spieler gewonnen hat, kann das Spiel nicht unentschieden sein.
            {
                return false;
            }

            for (int column = 0; column < Gruppe2_ConnectFourConstants.Columns; column++)     //Überprüft jede Spalte, um zu sehen, ob noch Züge möglich sind. Wenn eine Spalte noch eine freie Zelle hat, kann das Spiel nicht unentschieden sein.
            {
                if (Gruppe2_ConnectFourLogic.FindAvailableRow(field, column) >= 0)     //Wenn eine Spalte noch eine freie Zelle hat, kann das Spiel nicht unentschieden sein.
                {
                    return false;
                }
            }

            return true;
        }
    }

    public class Gruppe2_ConnectFourHumanPlayer : IHumanGamePlayer     //Klasse, die einen menschlichen Spieler für das Spiel "Vier Gewinnt" von Gruppe 2 implementiert. Sie implementiert das IHumanGamePlayer-Interface und enthält die Logik zur Auswahl eines Zuges durch den Spieler.
    {
        private int _playerNumber;     //private Variable, die die Spielerzahl speichert (1 oder 2). Sie wird durch die Methode SetPlayerNumber gesetzt und durch die Property PlayerNumber zurückgegeben.

        public string Name { get { return "Group 2 - Connect Four Human Player"; } }     //Name der Spielerklasse, die im Spiel angezeigt wird

        public int PlayerNumber { get { return _playerNumber; } }     //Property, die die Spielerzahl zurückgibt. Sie ist vom Typ int, sodass sie von anderen Klassen verwendet werden kann, die das Interface implementieren.

        public void SetPlayerNumber(int playerNumber)     //Methode, die die Spielerzahl setzt. Sie überprüft, ob die Spielerzahl gültig ist (1 oder 2), bevor sie die private Variable _playerNumber setzt. Damit weiß der Mensch-Spieler, ob er Spieler 1 oder 2 ist.
        {
            if (playerNumber == 1 || playerNumber == 2)
            {
                _playerNumber = playerNumber;     //setzt die Spielerzahl, wenn sie gültig ist. Ungültige Spielerzahlen werden ignoriert, um Fehler zu vermeiden.
            }
        }

        public IGamePlayer Clone()     //Methode, die eine Kopie des menschlichen Spielers erstellt. Sie wird verwendet, um mehrere Instanzen desselben Spielers zu erstellen, z. B. wenn mehrere Spiele gleichzeitig gespielt werden. Die Spielerzahl wird auf die Kopie übertragen, damit der neue Spieler dieselbe Spielerzahl hat wie der ursprüngliche Spieler.
        {
            Gruppe2_ConnectFourHumanPlayer clone = new Gruppe2_ConnectFourHumanPlayer();
            clone.SetPlayerNumber(_playerNumber);    //setzt die Spielerzahl der Kopie auf die Spielerzahl des ursprünglichen Spielers
            return clone;
        }

        public bool CanBeRuledBy(IGameRules rules)     //Methode, die überprüft, ob die Regelklasse das Spielbrett des menschlichen Spielers regeln kann. Sie gibt true zurück, wenn die Regelklasse nicht null ist und das aktuelle Spielbrett vom Typ I4GWFeld ist, sonst false.
        {
            return rules != null && rules.CurrentField is I4GWFeld;
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)     //Methode, die den Zug des menschlichen Spielers basierend auf der Auswahl des Spielers und dem aktuellen Spielbrett zurückgibt. Sie überprüft, ob die Auswahl und das Spielbrett gültig sind, und berechnet dann die Zeile und Spalte des Zuges. Wenn der Zug gültig ist, wird ein I4GWMove-Objekt zurückgegeben, sonst null.
        {
            if (!(selection is IClickSelection click)     //Wenn die Auswahl nicht vom Typ IClickSelection ist, wird null zurückgegeben, da der menschliche Spieler nur Klicks verwenden kann.
                || !(field is I4GWFeld connectFourField)     //Wenn das aktuelle Spielbrett nicht vom Typ I4GWFeld ist, wird null zurückgegeben, da der menschliche Spieler nur auf einem gültigen Spielbrett spielen kann.
                || _playerNumber < 1
                || _playerNumber > 2)
            {
                return null;
            }

            IConnectFourPainter painter =
                OOPGamesManager.Singleton.ActivePainter as IConnectFourPainter;
            if (painter == null)
            {
                return null;
            }

            int column = painter.ColumnAt(click.XClickPos, click.YClickPos);
            int row = Gruppe2_ConnectFourLogic.FindAvailableRow(connectFourField, column);     //Findet die nächste verfügbare Zeile in der berechneten Spalte. Wenn keine freie Zeile vorhanden ist, wird -1 zurückgegeben.
            if (row < 0)     //Wenn keine freie Zeile vorhanden ist, wird null zurückgegeben, da der menschliche Spieler keinen Zug machen kann.
            {
                return null;
            }

            return new Gruppe2_ConnectFourMove(row, column, _playerNumber);     //Gibt ein I4GWMove-Objekt zurück, das die Zeile, Spalte und Spielerzahl des Zuges enthält. Dieses Objekt wird später im Spiel verwendet, um den Zug auszuführen.
        }
    }

    public class Gruppe2_ConnectFourComputerPlayer : IComputerGamePlayer     //Klasse, die einen Computer-Spieler für das Spiel "Vier Gewinnt" von Gruppe 2 implementiert. Sie implementiert das IComputerGamePlayer-Interface und enthält die Logik zur Berechnung des besten Zuges für den Computer-Spieler.
    {
        private static readonly int[] PreferredColumns = { 3, 2, 4, 1, 5, 0, 6 };     //Array, das die bevorzugte Reihenfolge der Spalten für die Suche nach dem besten Zug angibt. Die mittlere Spalte (3) wird zuerst überprüft, da sie die meisten Möglichkeiten bietet, vier Spielsteine in einer Reihe zu verbinden. Danach werden die Spalten 2 und 4 überprüft, dann 1 und 5, und schließlich 0 und 6. Diese Reihenfolge hilft dem Computer-Spieler, bessere Züge zu finden.
        private static readonly int[,] SearchDirections =     //Array, das die Richtungen für die Suche nach vier Spielsteinen in einer Reihe angibt. Jede Richtung wird durch ein Paar von Schritten (Zeilen- und Spaltenänderung) dargestellt. Die Richtungen sind horizontal, vertikal und diagonal.
        {
            { 0, 1 },
            { 1, 0 },
            { 1, 1 },
            { 1, -1 }
        };
        private const int SearchDepth = 5;     //Maximale Tiefe der Suche nach dem besten Zug. Der Computer-Spieler wird bis zu 5 Züge im Voraus berechnen, um den besten Zug zu finden. Eine höhere Tiefe kann bessere Züge liefern, aber auch die Berechnungszeit erhöhen.
        private const int WinScore = 1000000;     //Punktzahl, die vergeben wird, wenn der Computer-Spieler einen Gewinnzug findet. Diese Punktzahl ist sehr hoch, um sicherzustellen, dass der Computer-Spieler immer den Gewinnzug wählt, wenn er verfügbar ist.
        private int _playerNumber;     //private Variable, die die Spielerzahl des Computer-Spielers speichert (1 oder 2). Sie wird durch die Methode SetPlayerNumber gesetzt und durch die Property PlayerNumber zurückgegeben.

        public string Name { get { return "Group 2 - Connect Four Computer Player"; } }

        public int PlayerNumber { get { return _playerNumber; } }     //Property, die die Spielerzahl zurückgibt. Sie ist vom Typ int, sodass sie von anderen Klassen verwendet werden kann, die das Interface implementieren.

        public void SetPlayerNumber(int playerNumber)     //Methode, die die Spielerzahl setzt. Sie überprüft, ob die Spielerzahl gültig ist (1 oder 2), bevor sie die private Variable _playerNumber setzt. Damit weiß der Computer-Spieler, ob er Spieler 1 oder 2 ist.
        {
            _playerNumber = playerNumber;    //setzt die Spielerzahl, auch wenn sie ungültig ist. Ungültige Spielerzahlen werden später in der GetMove-Methode überprüft, um Fehler zu vermeiden.
        }

        public IGamePlayer Clone()     //Methode, die eine Kopie des Computer-Spielers erstellt. Sie wird verwendet, um mehrere Instanzen desselben Spielers zu erstellen, z. B. wenn mehrere Spiele gleichzeitig gespielt werden. Die Spielerzahl wird auf die Kopie übertragen, damit der neue Spieler dieselbe Spielerzahl hat wie der ursprüngliche Spieler.
        {
            Gruppe2_ConnectFourComputerPlayer clone = new Gruppe2_ConnectFourComputerPlayer();     //erstellt eine neue Instanz des Computer-Spielers
            clone.SetPlayerNumber(_playerNumber);     //setzt die Spielerzahl der Kopie auf die Spielerzahl des ursprünglichen Spielers
            return clone;
        }

        public bool CanBeRuledBy(IGameRules rules)     //Methode, die überprüft, ob die Regelklasse das Spielbrett des Computer-Spielers regeln kann. Sie gibt true zurück, wenn die Regelklasse nicht null ist und das aktuelle Spielbrett vom Typ I4GWFeld ist, sonst false.
        {
            return rules != null && rules.CurrentField is I4GWFeld;     //Der Computer-Spieler kann nur auf einem gültigen Spielbrett spielen, das das I4GWFeld-Interface implementiert.
        }

        public IPlayMove GetMove(IGameField field)     //Methode, die den besten Zug des Computer-Spielers basierend auf dem aktuellen Spielbrett berechnet. Sie überprüft, ob das Spielbrett gültig ist und ob der Computer-Spieler eine gültige Spielerzahl hat. Dann wird ein Minimax-Algorithmus mit Alpha-Beta-Pruning verwendet, um den besten Zug zu finden. Wenn ein gültiger Zug gefunden wird, wird ein I4GWMove-Objekt zurückgegeben, sonst null.
        {
            if (!(field is I4GWFeld connectFourField)     //Wenn das aktuelle Spielbrett nicht vom Typ I4GWFeld ist, wird null zurückgegeben, da der Computer-Spieler nur auf einem gültigen Spielbrett spielen kann.
                || _playerNumber < 1     //Wenn die Spielerzahl ungültig ist, wird null zurückgegeben, da der Computer-Spieler nur mit einer gültigen Spielerzahl spielen kann.
                || _playerNumber > 2
                || Gruppe2_ConnectFourLogic.FindWinner(connectFourField) > 0)     //Wenn ein Spieler bereits gewonnen hat, wird null zurückgegeben, da der Computer-Spieler keinen Zug mehr machen kann.
            {
                return null;
            }

            int[,] board = new int[     //Erstellt ein zweidimensionales Array, das das Spielbrett für die Berechnung des besten Zuges speichert ohne das echte Spielbrett zu verändern. Die Größe des Arrays entspricht der Anzahl der Reihen und Spalten des Spielbretts.
                Gruppe2_ConnectFourConstants.Rows,
                Gruppe2_ConnectFourConstants.Columns];
            for (int row = 0; row < Gruppe2_ConnectFourConstants.Rows; row++)     //Kopiert den aktuellen Zustand des Spielbretts in das Array, um die Berechnung des besten Zuges zu ermöglichen.
            {
                for (int column = 0; column < Gruppe2_ConnectFourConstants.Columns; column++)    
                {
                    board[row, column] = connectFourField[row, column];     
                }
            }

            int opponentNumber = _playerNumber == 1 ? 2 : 1;     //Bestimmt die Spielerzahl des Gegners basierend auf der Spielerzahl des Computer-Spielers. Wenn der Computer-Spieler Spieler 1 ist, ist der Gegner Spieler 2, und umgekehrt.
            int bestColumn = -1;     //Variable, die die Spalte des besten Zuges speichert. Sie wird auf -1 gesetzt, um anzuzeigen, dass noch kein bester Zug gefunden wurde.
            int bestScore = int.MinValue;     //Variable, die die Punktzahl des besten Zuges speichert. Sie wird auf den kleinsten möglichen Wert gesetzt, um sicherzustellen, dass jeder gefundene Zug eine höhere Punktzahl hat.
            int alpha = int.MinValue;     //Variable, die den Alpha-Wert für das Alpha-Beta-Pruning speichert. Sie wird auf den kleinsten möglichen Wert gesetzt, um sicherzustellen, dass jeder gefundene Zug eine höhere Punktzahl hat.

            foreach (int column in PreferredColumns)     //
            {
                int row = FindAvailableRow(board, column);     //Findet die nächste verfügbare Zeile in der aktuellen Spalte. Wenn keine freie Zeile vorhanden ist, wird -1 zurückgegeben.
                if (row < 0)
                {
                    continue;
                }

                board[row, column] = _playerNumber;     //Setzt die Spielerzahl des Computer-Spielers in die gefundene freie Zelle, um den Zug zu simulieren und die Punktzahl zu berechnen.
                int score = ConnectsFour(board, row, column, _playerNumber)
                    ? WinScore + SearchDepth     //Wenn der Computer-Spieler einen Gewinnzug gefunden hat, wird die Punktzahl auf WinScore + SearchDepth gesetzt, um sicherzustellen, dass der Gewinnzug immer gewählt wird.
                    : Search(board, SearchDepth - 1, opponentNumber, _playerNumber, alpha, int.MaxValue);     //Wenn kein Gewinnzug gefunden wurde, wird die Punktzahl durch den Aufruf der Search-Methode berechnet, die den Minimax-Algorithmus mit Alpha-Beta-Pruning implementiert. Die Suche wird um eine Tiefe reduziert, da der aktuelle Zug bereits simuliert wurde.
                board[row, column] = 0;     //Setzt die Zelle wieder auf 0 (leer), um den simulierten Zug rückgängig zu machen und das Spielbrett für die nächste Spalte vorzubereiten.

                if (score > bestScore)
                {
                    bestScore = score;
                    bestColumn = column;     //Speichert die Spalte des besten Zuges, wenn die Punktzahl höher ist als die bisher beste Punktzahl.
                }

                alpha = Math.Max(alpha, bestScore);     //Aktualisiert den Alpha-Wert für das Alpha-Beta-Pruning, um die Suche effizienter zu gestalten und unnötige Berechnungen zu vermeiden.
            }

            int bestRow = bestColumn >= 0 ? FindAvailableRow(board, bestColumn) : -1;     //Findet die nächste verfügbare Zeile in der besten Spalte, um den endgültigen Zug des Computer-Spielers zu bestimmen. Wenn keine beste Spalte gefunden wurde, wird -1 zurückgegeben.
            return bestRow >= 0
                ? new Gruppe2_ConnectFourMove(bestRow, bestColumn, _playerNumber)
                : null;
        }

        private static int Search(     //Methode, die den Minimax-Algorithmus mit Alpha-Beta-Pruning implementiert, um die Punktzahl eines Zuges zu berechnen. Sie wird rekursiv aufgerufen, um die Punktzahl für alle möglichen Züge bis zur angegebenen Tiefe zu berechnen.
            int[,] board,
            int depth,
            int currentPlayer,
            int computerPlayer,
            int alpha,
            int beta)
        {
            if (depth == 0)
            {
                return HasAvailableMove(board) ? EvaluatePosition(board, computerPlayer) : 0;
            }

            bool foundMove = false;     //Variable, die angibt, ob ein gültiger Zug gefunden wurde. Sie wird auf false gesetzt, um anzuzeigen, dass noch kein gültiger Zug gefunden wurde.
            bool maximizing = currentPlayer == computerPlayer;     //Variable, die angibt, ob der aktuelle Spieler maximiert oder minimiert. Wenn der aktuelle Spieler der Computer-Spieler ist, wird maximiert, sonst minimiert.
            int bestScore = maximizing ? int.MinValue : int.MaxValue;

            foreach (int column in PreferredColumns)
            {
                int row = FindAvailableRow(board, column);
                if (row < 0)
                {
                    continue;
                }

                foundMove = true;
                board[row, column] = currentPlayer;

                int score;
                if (ConnectsFour(board, row, column, currentPlayer))
                {
                    score = currentPlayer == computerPlayer
                        ? WinScore + depth
                        : -WinScore - depth;
                }
                else
                {
                    int nextPlayer = currentPlayer == 1 ? 2 : 1;
                    score = Search(board, depth - 1, nextPlayer, computerPlayer, alpha, beta);
                }

                board[row, column] = 0;

                if (maximizing)
                {
                    bestScore = Math.Max(bestScore, score);
                    alpha = Math.Max(alpha, bestScore);
                }
                else
                {
                    bestScore = Math.Min(bestScore, score);
                    beta = Math.Min(beta, bestScore);
                }

                if (beta <= alpha)
                {
                    break;
                }
            }

            return foundMove ? bestScore : 0;
        }

        private static bool HasAvailableMove(int[,] board)
        {
            foreach (int column in PreferredColumns)
            {
                if (FindAvailableRow(board, column) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static int EvaluatePosition(int[,] board, int computerPlayer)
        {
            int opponentNumber = computerPlayer == 1 ? 2 : 1;
            int score = 0;

            for (int row = 0; row < Gruppe2_ConnectFourConstants.Rows; row++)
            {
                if (board[row, Gruppe2_ConnectFourConstants.Columns / 2] == computerPlayer)
                {
                    score += 3;
                }
                else if (board[row, Gruppe2_ConnectFourConstants.Columns / 2] == opponentNumber)
                {
                    score -= 3;
                }
            }

            for (int row = 0; row < Gruppe2_ConnectFourConstants.Rows; row++)
            {
                for (int column = 0; column < Gruppe2_ConnectFourConstants.Columns; column++)
                {
                    for (int direction = 0; direction < SearchDirections.GetLength(0); direction++)
                    {
                        int rowStep = SearchDirections[direction, 0];
                        int columnStep = SearchDirections[direction, 1];
                        int endRow = row + 3 * rowStep;
                        int endColumn = column + 3 * columnStep;

                        if (endRow < 0 || endRow >= Gruppe2_ConnectFourConstants.Rows
                            || endColumn < 0 || endColumn >= Gruppe2_ConnectFourConstants.Columns)
                        {
                            continue;
                        }

                        int computerPieces = 0;
                        int opponentPieces = 0;
                        for (int offset = 0; offset < 4; offset++)
                        {
                            int cell = board[row + offset * rowStep, column + offset * columnStep];
                            if (cell == computerPlayer)
                            {
                                computerPieces++;
                            }
                            else if (cell == opponentNumber)
                            {
                                opponentPieces++;
                            }
                        }

                        if (opponentPieces == 0)
                        {
                            score += computerPieces switch
                            {
                                1 => 1,
                                2 => 10,
                                3 => 80,
                                _ => 0
                            };
                        }
                        else if (computerPieces == 0)
                        {
                            score -= opponentPieces switch
                            {
                                1 => 1,
                                2 => 10,
                                3 => 100,
                                _ => 0
                            };
                        }
                    }
                }
            }

            return score;
        }

        private static int FindAvailableRow(int[,] board, int column)
        {
            for (int row = Gruppe2_ConnectFourConstants.Rows - 1; row >= 0; row--)
            {
                if (board[row, column] == 0)
                {
                    return row;
                }
            }

            return -1;
        }

        private static bool ConnectsFour(int[,] board, int row, int column, int playerNumber)
        {
            for (int direction = 0; direction < SearchDirections.GetLength(0); direction++)
            {
                int rowStep = SearchDirections[direction, 0];
                int columnStep = SearchDirections[direction, 1];
                int count = 1;

                for (int sign = -1; sign <= 1; sign += 2)
                {
                    int currentRow = row + sign * rowStep;
                    int currentColumn = column + sign * columnStep;
                    while (currentRow >= 0
                        && currentRow < Gruppe2_ConnectFourConstants.Rows
                        && currentColumn >= 0
                        && currentColumn < Gruppe2_ConnectFourConstants.Columns
                        && board[currentRow, currentColumn] == playerNumber)
                    {
                        count++;
                        currentRow += sign * rowStep;
                        currentColumn += sign * columnStep;
                    }
                }

                if (count >= 4)
                {
                    return true;
                }
            }

            return false;
        }
    }
}