using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace OOPGames
{
    // Enthält gemeinsame Konstanten und Hilfsalgorithmen für Regeln, Darstellung und Computer-Spieler.
    internal static class G5_4GWLogic
    {
        // Brettgröße und Zeichenpositionen müssen mit der Umrechnung von Mausklicks übereinstimmen.
        internal const int Rows = 6;
        internal const int Columns = 7;
        internal const double Left = 10;
        internal const double Top = 10;
        internal const double CellSize = 36;

        // Maximale Suchtiefe des Computers in einzelnen Zügen.
        internal const int SearchDepth = 5;

        // Der mittlere Zug wird zuerst geprüft, danach die benachbarten Spalten.
        private static readonly int[] PreferredColumns = { 3, 2, 4, 1, 5, 0, 6 };

        // Standardpause zwischen Computerzügen; der Painter ändert diesen Wert über den Regler.
        internal static int ComputerDelayMilliseconds { get; private set; } = 500;

        // Rundet den Reglerwert auf 100-ms-Schritte, damit Anzeige und Pause übereinstimmen.
        internal static void SetComputerDelay(double milliseconds)
        {
            ComputerDelayMilliseconds = (int)Math.Round(milliseconds / 100) * 100;
        }

        // Wartet mit laufender WPF-Ereignisverarbeitung, damit das Fenster während der Pause bedienbar bleibt.
        internal static void WaitForComputerDelay()
        {
            int delay = ComputerDelayMilliseconds;
            if (delay <= 0)
            {
                return;
            }

            // Ein DispatcherFrame erlaubt dem Timer, den Ablauf zu beenden, ohne den UI-Thread anzuhalten.
            DispatcherFrame frame = new DispatcherFrame();
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Background)
            {
                Interval = TimeSpan.FromMilliseconds(delay)
            };
            timer.Tick += (sender, args) =>
            {
                timer.Stop();
                frame.Continue = false;
            };
            timer.Start();
            Dispatcher.PushFrame(frame);
        }

        // Sucht von unten nach oben den ersten freien Platz einer Spalte.
        internal static int FindOpenRow(I4GWFeld field, int column)
        {
            // Ungültige Spalten können keinen legalen Zug aufnehmen.
            if (field == null || column < 0 || column >= Columns)
            {
                return -1;
            }

            for (int row = Rows - 1; row >= 0; row--)
            {
                if (field[row, column] == 0)
                {
                    return row;
                }
            }

            return -1;
        }

        // Prüft für jedes belegte Feld die vier möglichen Richtungen auf eine Viererreihe.
        internal static bool HasWinner(I4GWFeld field, out int winner)
        {
            for (int row = 0; row < Rows; row++)
            {
                for (int column = 0; column < Columns; column++)
                {
                    int player = field[row, column];
                    // Leere Felder können nicht Teil einer Gewinnreihe sein.
                    if (player > 0 &&
                        (HasLine(field, row, column, 0, 1, player) ||
                         HasLine(field, row, column, 1, 0, player) ||
                         HasLine(field, row, column, 1, 1, player) ||
                         HasLine(field, row, column, 1, -1, player)))
                    {
                        winner = player;
                        return true;
                    }
                }
            }

            winner = -1;
            return false;
        }

        // Liefert Startfeld und Richtung einer Gewinnreihe für die goldene Markierung im Painter.
        internal static bool TryFindWinningLine(
            I4GWFeld field,
            out int startRow,
            out int startColumn,
            out int rowStep,
            out int columnStep)
        {
            int[,] directions = { { 0, 1 }, { 1, 0 }, { 1, 1 }, { 1, -1 } };
            for (int row = 0; row < Rows; row++)
            {
                for (int column = 0; column < Columns; column++)
                {
                    int player = field[row, column];
                    if (player <= 0)
                    {
                        // Nur ein Spielstein kann eine Reihe beginnen.
                        continue;
                    }

                    for (int direction = 0; direction < 4; direction++)
                    {
                        int currentRowStep = directions[direction, 0];
                        int currentColumnStep = directions[direction, 1];
                        if (HasLine(field, row, column, currentRowStep, currentColumnStep, player))
                        {
                            startRow = row;
                            startColumn = column;
                            rowStep = currentRowStep;
                            columnStep = currentColumnStep;
                            return true;
                        }
                    }
                }
            }

            startRow = -1;
            startColumn = -1;
            rowStep = 0;
            columnStep = 0;
            return false;
        }

        // Ein Unentschieden liegt vor, wenn keine der sieben Spalten mehr Platz bietet.
        internal static bool IsFull(I4GWFeld field)
        {
            for (int column = 0; column < Columns; column++)
            {
                if (FindOpenRow(field, column) >= 0)
                {
                    return false;
                }
            }

            return true;
        }

        // Prüft einen möglichen Zug auf einem Framework-Spielfeld, ohne es zu verändern.
        internal static bool WinsAfterDrop(I4GWFeld field, int row, int column, int player)
        {
            int[,] directions = { { 0, 1 }, { 1, 0 }, { 1, 1 }, { 1, -1 } };
            for (int direction = 0; direction < 4; direction++)
            {
                int rowStep = directions[direction, 0];
                int columnStep = directions[direction, 1];
                int count = 1
                    + CountDirection(field, row, column, rowStep, columnStep, player)
                    + CountDirection(field, row, column, -rowStep, -columnStep, player);
                if (count >= 4)
                {
                    return true;
                }
            }

            return false;
        }

        // Gleiche Gewinnprüfung für das interne Array, das der Computer bei der Suche verwendet.
        internal static bool WinsAfterDrop(int[,] field, int row, int column, int player)
        {
            int[,] directions = { { 0, 1 }, { 1, 0 }, { 1, 1 }, { 1, -1 } };
            for (int direction = 0; direction < 4; direction++)
            {
                int rowStep = directions[direction, 0];
                int columnStep = directions[direction, 1];
                int count = 1
                    + CountDirection(field, row, column, rowStep, columnStep, player)
                    + CountDirection(field, row, column, -rowStep, -columnStep, player);
                if (count >= 4)
                {
                    return true;
                }
            }

            return false;
        }

        // Liefert die tiefste freie Zeile im temporären Suchbrett des Computers.
        internal static int FindOpenRow(int[,] field, int column)
        {
            if (column < 0 || column >= Columns)
            {
                return -1;
            }

            for (int row = Rows - 1; row >= 0; row--)
            {
                if (field[row, column] == 0)
                {
                    return row;
                }
            }

            return -1;
        }

        // Minimax durchsucht mögliche Folgezüge; Alpha-Beta beendet Teilbäume, die das Ergebnis
        // der aktuellen Entscheidung nicht mehr verbessern können.
        internal static int Search(int[,] field, int depth, int player, int computer,
            int lastRow, int lastColumn, int alpha, int beta)
        {
            // Der vorherige Suchzug kann das Spiel bereits beendet haben.
            int previousPlayer = player == 1 ? 2 : 1;
            if (lastRow >= 0 && WinsAfterDrop(field, lastRow, lastColumn, previousPlayer))
            {
                return previousPlayer == computer ? 100000 + depth : -100000 - depth;
            }

            // An der Suchtiefengrenze wird die Brettstellung statt weiterer Züge bewertet.
            if (depth == 0)
            {
                return Evaluate(field, computer);
            }

            // Der Computer maximiert den Wert, der Gegner versucht ihn zu minimieren.
            bool maximizing = player == computer;
            int bestScore = maximizing ? int.MinValue : int.MaxValue;
            bool foundMove = false;
            for (int index = 0; index < PreferredColumns.Length; index++)
            {
                int column = PreferredColumns[index];
                int row = FindOpenRow(field, column);
                if (row < 0)
                {
                    // Volle Spalten sind keine möglichen Folgezüge.
                    continue;
                }

                // Zug probeweise ausführen, rekursiv bewerten und danach das Brett zurücksetzen.
                foundMove = true;
                field[row, column] = player;
                int score = Search(field, depth - 1, previousPlayer, computer,
                    row, column, alpha, beta);
                field[row, column] = 0;

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
                    // Dieser Zweig kann die beste bisherige Entscheidung nicht mehr beeinflussen.
                    break;
                }
            }

            return foundMove ? bestScore : 0;
        }

        // Bewertet Brettstellungen anhand der Mitte und möglicher Viererfenster.
        private static int Evaluate(int[,] field, int computer)
        {
            int opponent = computer == 1 ? 2 : 1;
            int score = 0;

            for (int row = 0; row < Rows; row++)
            {
                // Die mittlere Spalte ist strategisch besonders wertvoll.
                if (field[row, Columns / 2] == computer)
                {
                    score += 6;
                }
                else if (field[row, Columns / 2] == opponent)
                {
                    score -= 6;
                }
            }

            int[,] directions = { { 0, 1 }, { 1, 0 }, { 1, 1 }, { 1, -1 } };
            for (int row = 0; row < Rows; row++)
            {
                for (int column = 0; column < Columns; column++)
                {
                    for (int direction = 0; direction < 4; direction++)
                    {
                        int rowStep = directions[direction, 0];
                        int columnStep = directions[direction, 1];
                        int endRow = row + 3 * rowStep;
                        int endColumn = column + 3 * columnStep;
                        if (!IsInside(endRow, endColumn))
                        {
                            // Ein Viererfenster muss vollständig auf dem Brett liegen.
                            continue;
                        }

                        int computerCount = 0;
                        int opponentCount = 0;
                        for (int offset = 0; offset < 4; offset++)
                        {
                            int cell = field[row + offset * rowStep, column + offset * columnStep];
                            if (cell == computer)
                            {
                                computerCount++;
                            }
                            else if (cell == opponent)
                            {
                                opponentCount++;
                            }
                        }

                        if (opponentCount == 0)
                        {
                            // Eigene ungestörte Dreier und Zweier werden positiv bewertet.
                            score += computerCount == 3 ? 120
                                : computerCount == 2 ? 18
                                : computerCount == 1 ? 2 : 0;
                        }
                        else if (computerCount == 0)
                        {
                            // Gegnerische ungestörte Reihen werden stärker negativ gewichtet.
                            score -= opponentCount == 3 ? 140
                                : opponentCount == 2 ? 20
                                : opponentCount == 1 ? 2 : 0;
                        }
                    }
                }
            }

            return score;
        }

        // Prüft, ob von einem Startfeld aus drei weitere passende Steine in der Richtung liegen.
        private static bool HasLine(
            I4GWFeld field,
            int row,
            int column,
            int rowStep,
            int columnStep,
            int player)
        {
            for (int index = 1; index < 4; index++)
            {
                int nextRow = row + rowStep * index;
                int nextColumn = column + columnStep * index;
                if (!IsInside(nextRow, nextColumn) ||
                    field[nextRow, nextColumn] != player)
                {
                    // Die Reihe ist unterbrochen oder würde über den Brettrand hinauslaufen.
                    return false;
                }
            }

            return true;
        }

        // Zählt direkt aneinanderliegende Steine in eine Richtung.
        private static int CountDirection(
            I4GWFeld field,
            int row,
            int column,
            int rowStep,
            int columnStep,
            int player)
        {
            int count = 0;
            int nextRow = row + rowStep;
            int nextColumn = column + columnStep;
            while (IsInside(nextRow, nextColumn) &&
                   field[nextRow, nextColumn] == player)
            {
                count++;
                nextRow += rowStep;
                nextColumn += columnStep;
            }

            return count;
        }

        // Array-Variante derselben Zählung für die interne Computersuche.
        private static int CountDirection(
            int[,] field,
            int row,
            int column,
            int rowStep,
            int columnStep,
            int player)
        {
            int count = 0;
            int nextRow = row + rowStep;
            int nextColumn = column + columnStep;
            while (IsInside(nextRow, nextColumn) && field[nextRow, nextColumn] == player)
            {
                count++;
                nextRow += rowStep;
                nextColumn += columnStep;
            }

            return count;
        }

        // Prüft, ob eine Zeile und Spalte innerhalb der festen Brettgröße liegen.
        private static bool IsInside(int row, int column)
        {
            return row >= 0 && row < Rows && column >= 0 && column < Columns;
        }
    }

    // Speichert die Spielsteine als 6x7-Feld und implementiert den gemeinsamen Feldvertrag.
    public sealed class G5_4GWFeld : IG5_4GWFeld
    {
        private readonly int[,] _cells = new int[G5_4GWLogic.Rows, G5_4GWLogic.Columns];

        // Der Indexer verwendet nullbasierte Zeilen und Spalten; außerhalb des Brettes gilt -1 als ungültig.
        public int this[int row, int column]
        {
            get
            {
                if (row < 0 || row >= G5_4GWLogic.Rows ||
                    column < 0 || column >= G5_4GWLogic.Columns)
                {
                    return -1;
                }

                return _cells[row, column];
            }
            set
            {
                if (row >= 0 && row < G5_4GWLogic.Rows &&
                    column >= 0 && column < G5_4GWLogic.Columns)
                {
                    _cells[row, column] = value;
                }
            }
        }

        // Jeder Painter wird zugelassen, der Feld und Spielsteine darstellen kann.
        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter != null;
        }
    }

    // Ein Zug speichert Zielposition und Spieler; die Regeln bestimmen beim Anwenden die tatsächliche Fallzeile.
    public sealed class G5_4GWZug : IG5_4GWMove
    {
        public G5_4GWZug(int row, int column, int playerNumber)
        {
            Row = row;
            Column = column;
            PlayerNumber = playerNumber;
        }

        public int Row { get; private set; }
        public int Column { get; private set; }
        public int PlayerNumber { get; private set; }
    }

    // Verwaltet den Spielzustand und setzt die allgemeinen OOPGames-Regeln für Vier gewinnt um.
    public sealed class G5_4GWRules : IG5_4GWRules
    {
        private readonly IG5_4GWFeld _field = new G5_4GWFeld();
        private int _nextPlayer = 1;

        // Eindeutiger Name zur Auswahl im Hauptfenster.
        public string Name { get { return "Gruppe 5 Vier gewinnt Regeln"; } }
        public IGameField CurrentField { get { return _field; } }
        public IG5_4GWFeld VierGewinntFeld { get { return _field; } }

        // Unentschieden: das Brett ist voll und keiner der Spieler hat gewonnen.
        public bool IsDraw
        {
            get
            {
                int winner;
                return !G5_4GWLogic.HasWinner(_field, out winner) &&
                    G5_4GWLogic.IsFull(_field);
            }
        }

        // Nach einem Sieg oder bei vollem Brett können keine weiteren Züge erfolgen.
        public bool MovesPossible
        {
            get
            {
                int winner;
                return !G5_4GWLogic.HasWinner(_field, out winner) &&
                    !G5_4GWLogic.IsFull(_field);
            }
        }

        // Leert alle Felder und setzt Spieler 1 als Startspieler zurück.
        public void ClearField()
        {
            for (int row = 0; row < G5_4GWLogic.Rows; row++)
            {
                for (int column = 0; column < G5_4GWLogic.Columns; column++)
                {
                    _field[row, column] = 0;
                }
            }

            _nextPlayer = 1;
        }

        // Akzeptiert nur gemeinsame Vier-gewinnt-Züge des erwarteten Spielers in eine freie Spalte.
        public void DoMove(IPlayMove move)
        {
            I4GWMove fourWinsMove = move as I4GWMove;
            // Ungültige Züge ändern den Spielzustand nicht.
            if (fourWinsMove == null ||
                fourWinsMove.PlayerNumber != _nextPlayer ||
                fourWinsMove.PlayerNumber < 1 ||
                fourWinsMove.PlayerNumber > 2 ||
                !MovesPossible)
            {
                return;
            }

            // Die Zielzeile wird hier neu berechnet, sodass auch Züge anderer Gruppen funktionieren,
            // die keine passende Zeile in ihrem Zugobjekt angeben.
            int row = G5_4GWLogic.FindOpenRow(_field, fourWinsMove.Column);
            if (row < 0)
            {
                return;
            }

            _field[row, fourWinsMove.Column] = fourWinsMove.PlayerNumber;
            _nextPlayer = _nextPlayer == 1 ? 2 : 1;
        }

        // Gibt die Spielernummer des Gewinners zurück oder -1, wenn noch niemand gewonnen hat.
        public int CheckIfPLayerWon()
        {
            int winner;
            return G5_4GWLogic.HasWinner(_field, out winner) ? winner : -1;
        }
    }

    // Zeichnet das Brett, Steine, Spaltennummern, Gewinnmarkierung und Geschwindigkeitsregler.
    public sealed class G5_4GWPainter : IG5_4GWPainter
    {
        public string Name { get { return "Gruppe 5 Vier gewinnt Darstellung"; } }

        // Baut die gesamte Canvas-Darstellung aus dem aktuellen gemeinsamen Vier-gewinnt-Feld neu auf.
        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            I4GWFeld field = currentField as I4GWFeld;
            if (canvas == null || field == null)
            {
                // Dieser Painter kann nur Vier-gewinnt-Felder darstellen.
                return;
            }

            // Ein Neuaufbau verhindert, dass alte Steine oder Bedienelemente stehen bleiben.
            canvas.Children.Clear();
            canvas.Background = Brushes.White;
            Rectangle board = new Rectangle
            {
                Width = G5_4GWLogic.CellSize * G5_4GWLogic.Columns,
                Height = G5_4GWLogic.CellSize * G5_4GWLogic.Rows,
                Fill = Brushes.RoyalBlue
            };
            Canvas.SetLeft(board, G5_4GWLogic.Left);
            Canvas.SetTop(board, G5_4GWLogic.Top);
            canvas.Children.Add(board);

            // Die gefundene Gewinnreihe wird anschließend beim Zeichnen der Steine hervorgehoben.
            int winningRow;
            int winningColumn;
            int winningRowStep;
            int winningColumnStep;
            bool hasWinningLine = G5_4GWLogic.TryFindWinningLine(
                field, out winningRow, out winningColumn,
                out winningRowStep, out winningColumnStep);

            for (int row = 0; row < G5_4GWLogic.Rows; row++)
            {
                for (int column = 0; column < G5_4GWLogic.Columns; column++)
                {
                    int player = field[row, column];
                    bool isWinningToken = false;
                    if (hasWinningLine)
                    {
                        // Prüft, ob dieses Feld zu genau den vier markierten Gewinnsteinen gehört.
                        for (int offset = 0; offset < 4; offset++)
                        {
                            if (row == winningRow + offset * winningRowStep &&
                                column == winningColumn + offset * winningColumnStep)
                            {
                                isWinningToken = true;
                                break;
                            }
                        }
                    }

                    // Spieler 1 ist rot, Spieler 2 gelb und ein freies Feld weiß.
                    Ellipse token = new Ellipse
                    {
                        Width = G5_4GWLogic.CellSize - 10,
                        Height = G5_4GWLogic.CellSize - 10,
                        Fill = player == 1
                            ? Brushes.Red
                            : player == 2 ? Brushes.Yellow : Brushes.White,
                        Stroke = isWinningToken ? Brushes.Gold : Brushes.Navy,
                        StrokeThickness = isWinningToken ? 5 : 1
                    };
                    Canvas.SetLeft(token, G5_4GWLogic.Left + column * G5_4GWLogic.CellSize + 5);
                    Canvas.SetTop(token, G5_4GWLogic.Top + row * G5_4GWLogic.CellSize + 5);
                    canvas.Children.Add(token);
                }
            }

            AddColumnNumbers(canvas);
            AddComputerSpeedControl(canvas);
        }

        // Zeichnet unter jeder Spalte ihre Zahl als Orientierung für Maus und Tastatur.
        private static void AddColumnNumbers(Canvas canvas)
        {
            for (int column = 0; column < G5_4GWLogic.Columns; column++)
            {
                TextBlock number = new TextBlock
                {
                    Text = (column + 1).ToString(),
                    Width = G5_4GWLogic.CellSize,
                    TextAlignment = TextAlignment.Center,
                    FontWeight = FontWeights.Bold,
                    FontSize = 16,
                    Foreground = Brushes.Black
                };
                Canvas.SetLeft(number, G5_4GWLogic.Left + column * G5_4GWLogic.CellSize);
                Canvas.SetTop(number, G5_4GWLogic.Top + G5_4GWLogic.Rows * G5_4GWLogic.CellSize);
                canvas.Children.Add(number);
            }
        }

        // Fügt einen Regler für die Pause zwischen Computerzügen samt aktueller Millisekundenanzeige hinzu.
        private static void AddComputerSpeedControl(Canvas canvas)
        {
            TextBlock label = new TextBlock
            {
                Text = "Computerzug-Abstand: " +
                    G5_4GWLogic.ComputerDelayMilliseconds + " ms",
                Width = G5_4GWLogic.CellSize * G5_4GWLogic.Columns,
                FontSize = 12,
                Foreground = Brushes.Black
            };
            Canvas.SetLeft(label, G5_4GWLogic.Left);
            Canvas.SetTop(label, G5_4GWLogic.Top +
                G5_4GWLogic.Rows * G5_4GWLogic.CellSize + 22);
            canvas.Children.Add(label);

            Slider delaySlider = new Slider
            {
                Minimum = 0,
                Maximum = 1500,
                Value = G5_4GWLogic.ComputerDelayMilliseconds,
                TickFrequency = 100,
                SmallChange = 100,
                LargeChange = 100,
                IsSnapToTickEnabled = true,
                Width = G5_4GWLogic.CellSize * G5_4GWLogic.Columns,
                Height = 22,
                ToolTip = "Verzögerung zwischen Computerzügen"
            };
            // Änderungen am Regler übernehmen den neuen Abstand sofort.
            delaySlider.ValueChanged += (sender, args) =>
            {
                G5_4GWLogic.SetComputerDelay(args.NewValue);
                label.Text = "Computerzug-Abstand: " +
                    G5_4GWLogic.ComputerDelayMilliseconds + " ms";
            };
            // Ein Klick auf den Regler soll nicht zusätzlich als Spielzug auf dem Canvas ankommen.
            delaySlider.AddHandler(
                UIElement.MouseDownEvent,
                new System.Windows.Input.MouseButtonEventHandler((sender, args) =>
                {
                    args.Handled = true;
                }),
                true);
            Canvas.SetLeft(delaySlider, G5_4GWLogic.Left);
            Canvas.SetTop(delaySlider, G5_4GWLogic.Top +
                G5_4GWLogic.Rows * G5_4GWLogic.CellSize + 42);
            canvas.Children.Add(delaySlider);
        }
    }

    // Übersetzt Klicks und Zahlentasten in gültige Züge für den menschlichen Spieler.
    public sealed class G5_4GWHumanPlayer : IG5_4GWHumanPlayer
    {
        private static readonly HashSet<Key> PressedNumberKeys = new HashSet<Key>();
        private static Window _keyUpWindow;
        private int _playerNumber;

        public string Name { get { return "Gruppe 5 Vier gewinnt Mensch"; } }
        public int PlayerNumber { get { return _playerNumber; } }

        // Das Hauptfenster weist vor jedem Zug die Nummer 1 oder 2 zu.
        public void SetPlayerNumber(int playerNumber)
        {
            _playerNumber = playerNumber;
        }

        // Passt zu Regeln, deren aktuelles Feld den gemeinsamen Vier-gewinnt-Vertrag implementiert.
        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules != null && rules.CurrentField is I4GWFeld;
        }

        // Erstellt eine unabhängige Kopie des Players mit derselben Spielernummer.
        public IGamePlayer Clone()
        {
            G5_4GWHumanPlayer clone = new G5_4GWHumanPlayer();
            clone.SetPlayerNumber(_playerNumber);
            return clone;
        }

        // Liefert null, wenn Auswahl, Feld, Spielernummer oder Spielzustand keinen legalen Zug erlauben.
        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            IKeySelection keySelection = selection as IKeySelection;
            if (keySelection != null &&
                IsColumnKey(keySelection.Key) &&
                !TryRegisterKeyPress(keySelection.Key))
            {
                // Ignoriert autorepeat, bis die Taste tatsächlich wieder losgelassen wurde.
                return null;
            }

            I4GWFeld fourWinsField = field as I4GWFeld;
            if (fourWinsField == null ||
                _playerNumber < 1 || _playerNumber > 2 ||
                HasWinner(fourWinsField))
            {
                return null;
            }

            // Aus der Auswahl folgt die Spalte; die Regeln bestimmen dazu die tiefste freie Zeile.
            int column = GetSelectedColumn(selection);
            int row = G5_4GWLogic.FindOpenRow(fourWinsField, column);
            return row < 0 ? null : new G5_4GWZug(row, column, _playerNumber);
        }

        // Verhindert Eingaben, nachdem bereits eine Gewinnreihe existiert.
        private static bool HasWinner(I4GWFeld field)
        {
            int winner;
            return G5_4GWLogic.HasWinner(field, out winner);
        }

        // Merkt sich gedrückte Spaltentasten und bindet das Zurücksetzen an das Loslassen im Hauptfenster.
        private static bool TryRegisterKeyPress(Key key)
        {
            Window mainWindow = Application.Current == null
                ? null
                : Application.Current.MainWindow;
            if (mainWindow == null)
            {
                return true;
            }

            if (!ReferenceEquals(_keyUpWindow, mainWindow))
            {
                if (_keyUpWindow != null)
                {
                    _keyUpWindow.PreviewKeyUp -= MainWindow_PreviewKeyUp;
                    _keyUpWindow.PreviewKeyDown -= MainWindow_PreviewKeyDown;
                }

                _keyUpWindow = mainWindow;
                _keyUpWindow.PreviewKeyUp += MainWindow_PreviewKeyUp;
                _keyUpWindow.PreviewKeyDown += MainWindow_PreviewKeyDown;
            }

            return PressedNumberKeys.Add(key);
        }

        // Verwirft WPF-Tastaturwiederholungen, bevor sie MainWindow als weiteren Zug erreichen.
        private static void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (IsColumnKey(e.Key) && e.IsRepeat)
            {
                e.Handled = true;
            }
        }

        private static void MainWindow_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            PressedNumberKeys.Remove(e.Key);
        }

        private static bool IsColumnKey(Key key)
        {
            return (key >= Key.D1 && key <= Key.D7) ||
                (key >= Key.NumPad1 && key <= Key.NumPad7);
        }

        // Bestimmt die Spalte entweder aus der Klickposition oder aus einer Taste 1 bis 7.
        private static int GetSelectedColumn(IMoveSelection selection)
        {
            IClickSelection click = selection as IClickSelection;
            if (click != null)
            {
                // Klicks außerhalb des sichtbaren Bretts sind keine gültige Auswahl.
                double boardWidth = G5_4GWLogic.CellSize * G5_4GWLogic.Columns;
                double boardHeight = G5_4GWLogic.CellSize * G5_4GWLogic.Rows;
                if (click.XClickPos < G5_4GWLogic.Left ||
                    click.XClickPos >= G5_4GWLogic.Left + boardWidth ||
                    click.YClickPos < G5_4GWLogic.Top ||
                    click.YClickPos >= G5_4GWLogic.Top + boardHeight)
                {
                    return -1;
                }

                return (int)((click.XClickPos - G5_4GWLogic.Left) /
                    G5_4GWLogic.CellSize);
            }

            // Die Haupttastenreihe und der Nummernblock ordnen Taste 1 der ersten Spalte zu.
            IKeySelection keySelection = selection as IKeySelection;
            if (keySelection == null)
            {
                return -1;
            }

            if (keySelection.Key >= Key.D1 && keySelection.Key <= Key.D7)
            {
                return (int)keySelection.Key - (int)Key.D1;
            }

            if (keySelection.Key >= Key.NumPad1 && keySelection.Key <= Key.NumPad7)
            {
                return (int)keySelection.Key - (int)Key.NumPad1;
            }

            return -1;
        }
    }

    // Sucht einen guten Zug voraus und liefert stets eine freie Spalte, solange das Spiel weitergeht.
    public sealed class G5_4GWComputerPlayer : IG5_4GWComputerPlayer
    {
        private int _playerNumber;

        public string Name { get { return "Gruppe 5 Vier gewinnt Computer"; } }
        public int PlayerNumber { get { return _playerNumber; } }

        // Das Hauptfenster legt vor Spielbeginn die Spielernummer fest.
        public void SetPlayerNumber(int playerNumber)
        {
            _playerNumber = playerNumber;
        }

        // Kann mit jeder Rules-Implementierung verwendet werden, deren Feld I4GWFeld unterstützt.
        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules != null && rules.CurrentField is I4GWFeld;
        }

        // Erstellt einen neuen Computer-Player mit derselben Spielernummer.
        public IGamePlayer Clone()
        {
            G5_4GWComputerPlayer clone = new G5_4GWComputerPlayer();
            clone.SetPlayerNumber(_playerNumber);
            return clone;
        }

        // Kopiert das Feld, probiert mögliche erste Züge durch und wählt den mit der besten Suchbewertung.
        public IPlayMove GetMove(IGameField field)
        {
            I4GWFeld fourWinsField = field as I4GWFeld;
            if (fourWinsField == null ||
                _playerNumber < 1 || _playerNumber > 2)
            {
                return null;
            }

            // Die einstellbare Pause macht Computerzüge, insbesondere Computer gegen Computer, sichtbar.
            G5_4GWLogic.WaitForComputerDelay();

            int winner;
            if (G5_4GWLogic.HasWinner(fourWinsField, out winner))
            {
                return null;
            }

            // Die Suche arbeitet auf einer Kopie, damit sie den echten Spielzustand nicht verändert.
            int[,] board = new int[G5_4GWLogic.Rows, G5_4GWLogic.Columns];
            for (int row = 0; row < G5_4GWLogic.Rows; row++)
            {
                for (int column = 0; column < G5_4GWLogic.Columns; column++)
                {
                    board[row, column] = fourWinsField[row, column];
                }
            }

            int opponent = _playerNumber == 1 ? 2 : 1;
            int bestColumn = -1;
            int bestRow = -1;
            int bestScore = int.MinValue;
            int alpha = int.MinValue;

            // Prüft freie Spalten in der Reihenfolge von der Mitte zu den Rändern.
            for (int columnIndex = 0; columnIndex < 7; columnIndex++)
            {
                int column = GetPreferredColumn(columnIndex);
                int row = G5_4GWLogic.FindOpenRow(board, column);
                if (row < 0)
                {
                    continue;
                }

                // Bewertet den Zug nach möglichen Antworten beider Spieler.
                board[row, column] = _playerNumber;
                int score = G5_4GWLogic.Search(board, G5_4GWLogic.SearchDepth - 1,
                    opponent, _playerNumber, row, column, alpha, int.MaxValue);
                board[row, column] = 0;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestColumn = column;
                    bestRow = row;
                }

                alpha = Math.Max(alpha, bestScore);
            }

            return bestColumn < 0 ? null : new G5_4GWZug(bestRow, bestColumn, _playerNumber);
        }

        // Ordnet den Suchindex der strategischen Reihenfolge Mitte, Nachbarn, Außen zu.
        private static int GetPreferredColumn(int index)
        {
            switch (index)
            {
                case 0: return 3;
                case 1: return 2;
                case 2: return 4;
                case 3: return 1;
                case 4: return 5;
                case 5: return 0;
                default: return 6;
            }
        }
    }
}
