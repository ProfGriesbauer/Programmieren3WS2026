using System.Collections.Generic;

namespace OOPGames
{
    //Group 10 specific, optional extensions of the shared Vier gewinnt interfaces (4GWInterfaces.cs).
    //Components of other groups simply ignore them; group 10 components check for them with "is".

    //Field that animates falling discs
    public interface IG10_AnimatedFeld : I4GWFeld
    {
        //True while discs are still falling
        bool IsAnimating { get; }

        //Discs already placed in the field whose fall is still animated; the first one is currently falling
        IEnumerable<IG10_FallingDisc> FallingDiscs { get; }
    }

    //A disc on its way down to its target row
    public interface IG10_FallingDisc
    {
        int Row { get; }

        int Column { get; }

        int PlayerNumber { get; }

        //Current vertical position in rows: -1 is above the board, Row is the final position
        double Position { get; }
    }

    //Painter that converts click positions into columns matching its own layout
    public interface IG10_Painter : IPaintGame
    {
        //Returns the column at the given canvas position, -1 if no column was hit
        int ColumnAt(int x, int y);
    }
}
