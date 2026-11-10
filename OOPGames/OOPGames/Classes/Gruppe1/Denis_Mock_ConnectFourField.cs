namespace OOPGames
{
    public sealed class Gruppe1_ConnectFourField : IG1_ConnectFourField
    {
        private readonly int[,] _cells =
            new int[G1_ConnectFourLogic.Rows, G1_ConnectFourLogic.Columns];

        public int this[int row, int column]
        {
            get
            {
                return G1_ConnectFourLogic.IsInside(row, column)
                    ? _cells[row, column]
                    : -1;
            }
            set
            {
                if (G1_ConnectFourLogic.IsInside(row, column))
                {
                    _cells[row, column] = value;
                }
            }
        }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter != null;
        }
    }
}
