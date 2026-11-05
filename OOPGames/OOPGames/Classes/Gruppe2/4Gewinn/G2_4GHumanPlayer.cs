namespace OOPGames
{
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
            return GetMove(
                selection,
                field,
                OOPGamesManager.Singleton.ActivePainter as IConnectFourPainter);
        }

        public IPlayMove GetMove(
            IMoveSelection selection,
            IGameField field,
            IConnectFourPainter painter)
        {
            if (!(selection is IClickSelection click)     //Wenn die Auswahl nicht vom Typ IClickSelection ist, wird null zurückgegeben, da der menschliche Spieler nur Klicks verwenden kann.
                || !(field is I4GWFeld connectFourField)     //Wenn das aktuelle Spielbrett nicht vom Typ I4GWFeld ist, wird null zurückgegeben, da der menschliche Spieler nur auf einem gültigen Spielbrett spielen kann.
                || _playerNumber < 1
                || _playerNumber > 2
                || painter == null)
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
}
