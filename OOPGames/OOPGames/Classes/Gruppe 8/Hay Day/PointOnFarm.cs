namespace OOPGames
{
    public sealed class PointOnFarm
    {
        public PointOnFarm(int row, int column)
        {
            Row = row;
            Column = column;
        }

        public int Row { get; private set; }
        public int Column { get; private set; }
    }
}
