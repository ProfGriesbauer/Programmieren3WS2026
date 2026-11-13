namespace OOPGames
{
    //Survival specific game field
    public interface ISurvivalField : IGameField
    {
        //Indexer: returns 0 for a unused field, 1 for player 1, 2 for player 2, etc.
        //indexed by the row r and column c
        int this[int r, int c] { get; set; }
    }
        
}