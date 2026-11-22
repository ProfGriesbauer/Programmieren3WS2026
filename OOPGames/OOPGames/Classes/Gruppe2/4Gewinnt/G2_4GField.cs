namespace OOPGames
{
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
}
