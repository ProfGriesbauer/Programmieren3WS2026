namespace OOPGames
{
    public interface IG1_ConnectFourField : I4GWFeld
    {
    }

    public interface IG1_ConnectFourMove : I4GWMove
    {
    }

    public interface IG1_ConnectFourPainter : IPaintGame2
    {
        int ColumnAt(int x, int y);
    }
}
