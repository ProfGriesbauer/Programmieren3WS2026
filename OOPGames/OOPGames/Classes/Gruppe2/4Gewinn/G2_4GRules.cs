namespace OOPGames
{
    public class Gruppe2_ConnectFourRules : IGameRules     //Regelklasse für das Spiel "Vier Gewinnt" von Gruppe 2. Sie implementiert das IGameRules-Interface und enthält die Logik zur Überprüfung von Zügen, Gewinnern und Unentschieden.
    {
        private readonly I4GWFeld _field = new Gruppe2_ConnectFourField();     //Instanz des Spielbretts, das als private Variable erzeugt wird.

        public string Name { get { return "Group 2 - Connect Four Rules"; } }     //Name der Regelklasse, die im Spiel angezeigt wird

        public I4GWFeld ConnectFourField { get { return _field; } }     //Property, die das Spielbrett zurückgibt. Sie ist vom Typ I4GWFeld, sodass sie von anderen Klassen verwendet werden kann, die das Interface implementieren.

        public IGameField CurrentField { get { return _field; } }     //Property, die das aktuelle Spielbrett zurückgibt. Sie ist vom Typ IGameField, sodass sie von anderen Klassen verwendet werden kann, die das Interface implementieren.

        public bool MovesPossible     //Property, die überprüft, ob noch Züge möglich sind. Sie gibt true zurück, wenn es noch freie Zellen gibt und kein Spieler gewonnen hat, sonst false.
        {
            get { return CheckIfPLayerWon() < 0 && Gruppe2_ConnectFourLogic.HasAvailableMove(_field); }
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
            get { return Gruppe2_ConnectFourLogic.IsDraw(_field); }
        }
    }
}
