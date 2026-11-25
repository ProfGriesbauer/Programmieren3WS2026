using System.Windows.Input;

namespace OOPGames
{
    /// <summary>
    /// Enthält eine Eingabe samt Spieler, Rasterposition und optionaler Menüauswahl.
    /// </summary>
    public sealed class HayDayMove : IPlayMove
    {
        public HayDayMove(int playerNumber, HayDayMoveKind moveKind, int row, int column,
            int button, Key key, bool isMenuClick = false, int menuOption = -1)
        {
            PlayerNumber = playerNumber;
            MoveKind = moveKind;
            Row = row;
            Column = column;
            Button = button;
            Key = key;
            IsMenuClick = isMenuClick;
            MenuOption = menuOption;
        }

        public int PlayerNumber { get; private set; }
        public HayDayMoveKind MoveKind { get; private set; }
        public int Row { get; private set; }
        public int Column { get; private set; }
        public int Button { get; private set; }
        public Key Key { get; private set; }
        public bool IsMenuClick { get; private set; }
        public int MenuOption { get; private set; }
    }

    public enum HayDayMoveKind
    {
        Click,
        Key
    }
}
