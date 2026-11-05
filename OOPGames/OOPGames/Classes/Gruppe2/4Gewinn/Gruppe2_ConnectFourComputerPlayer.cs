using System;

namespace OOPGames
{
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

            I4GWFeld board = new Gruppe2_ConnectFourField();
            for (int row = 0; row < Gruppe2_ConnectFourConstants.Rows; row++)     //Kopiert das aktuelle Spielbrett in die separate Simulation.
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
                int row = Gruppe2_ConnectFourLogic.FindAvailableRow(board, column);     //Findet die nächste verfügbare Zeile in der aktuellen Spalte. Wenn keine freie Zeile vorhanden ist, wird -1 zurückgegeben.
                if (row < 0)
                {
                    continue;
                }

                board[row, column] = _playerNumber;     //Setzt die Spielerzahl des Computer-Spielers in die gefundene freie Zelle, um den Zug zu simulieren und die Punktzahl zu berechnen.
                int score = Gruppe2_ConnectFourLogic.FindWinner(board) == _playerNumber
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

            int bestRow = bestColumn >= 0
                ? Gruppe2_ConnectFourLogic.FindAvailableRow(board, bestColumn)
                : -1;     //Findet die nächste verfügbare Zeile in der besten Spalte, um den endgültigen Zug des Computer-Spielers zu bestimmen. Wenn keine beste Spalte gefunden wurde, wird -1 zurückgegeben.
            return bestRow >= 0
                ? new Gruppe2_ConnectFourMove(bestRow, bestColumn, _playerNumber)
                : null;
        }

        private static int Search(     //Methode, die den Minimax-Algorithmus mit Alpha-Beta-Pruning implementiert, um die Punktzahl eines Zuges zu berechnen. Sie wird rekursiv aufgerufen, um die Punktzahl für alle möglichen Züge bis zur angegebenen Tiefe zu berechnen.
            I4GWFeld board,
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
                int row = Gruppe2_ConnectFourLogic.FindAvailableRow(board, column);
                if (row < 0)
                {
                    continue;
                }

                foundMove = true;
                board[row, column] = currentPlayer;

                int score;
                if (Gruppe2_ConnectFourLogic.FindWinner(board) == currentPlayer)
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

        private static bool HasAvailableMove(I4GWFeld board)
        {
            foreach (int column in PreferredColumns)
            {
                if (Gruppe2_ConnectFourLogic.FindAvailableRow(board, column) >= 0)
                {
                    return true;
                }
            }

            return false;
        }

        private static int EvaluatePosition(I4GWFeld board, int computerPlayer)
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

    }
}
