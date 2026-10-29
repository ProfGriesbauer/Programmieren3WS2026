namespace OOPGames
{
    //Vier gewinnt specific game field
    public interface I4GWFeld : IGameField
    {
        //Indexer: returns 0 for a unused field, 1 for player 1, 2 for player 2, etc.
        //indexed by the row r and column c
        int this[int r, int c] { get; set; }
    }

    //Vier gewinnt specific move
    public interface I4GWMove : IRowMove, IColumnMove
    {
    }
}
