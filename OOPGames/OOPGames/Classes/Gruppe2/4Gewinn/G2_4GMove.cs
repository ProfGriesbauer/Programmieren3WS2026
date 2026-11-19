namespace OOPGames
{
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
}
