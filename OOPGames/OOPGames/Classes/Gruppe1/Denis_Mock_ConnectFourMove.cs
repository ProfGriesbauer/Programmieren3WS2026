namespace OOPGames
{
    public sealed class Gruppe1_ConnectFourMove : IG1_ConnectFourMove
    {
        public Gruppe1_ConnectFourMove(int row, int column, int playerNumber)
        {
            Row = row;
            Column = column;
            PlayerNumber = playerNumber;
        }

        public int Row { get; private set; }

        public int Column { get; private set; }

        public int PlayerNumber { get; private set; }
    }
}
