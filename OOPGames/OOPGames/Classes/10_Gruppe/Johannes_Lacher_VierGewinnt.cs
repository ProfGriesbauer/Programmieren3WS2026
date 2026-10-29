using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    //Game logic used by rules, painter and players; works on the field of any group.
    //The shared interfaces define no field size, so the standard size 6 x 7 is assumed (row 0 = top row).
    public static class G10_VierGewinntLogic
    {
        public const int Rows = 6;
        public const int Columns = 7;
        public const int Empty = 0;

        public static bool IsInside(int r, int c)
        {
            return r >= 0 && r < Rows && c >= 0 && c < Columns;
        }

        //Returns the row a disc dropped into the given column lands in, -1 if the column is full or invalid
        public static int FreeRow(I4GWFeld field, int column)
        {
            if (column < 0 || column >= Columns)
            {
                return -1;
            }

            for (int r = Rows - 1; r >= 0; r--)
            {
                if (field[r, column] == Empty)
                {
                    return r;
                }
            }

            return -1;
        }

        public static bool IsFull(I4GWFeld field)
        {
            for (int c = 0; c < Columns; c++)
            {
                if (FreeRow(field, c) >= 0)
                {
                    return false;
                }
            }

            return true;
        }

        //Returns the number of the player with four in a row, -1 if nobody has won.
        //Positions outside the field are never read, since fields of other groups may not support them.
        public static int Winner(I4GWFeld field)
        {
            int[,] directions = { { 0, 1 }, { 1, 0 }, { 1, 1 }, { 1, -1 } };

            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    int player = field[r, c];
                    if (player == Empty)
                    {
                        continue;
                    }

                    for (int d = 0; d < 4; d++)
                    {
                        int count = 1;
                        while (count < 4 &&
                               IsInside(r + count * directions[d, 0], c + count * directions[d, 1]) &&
                               field[r + count * directions[d, 0], c + count * directions[d, 1]] == player)
                        {
                            count++;
                        }

                        if (count == 4)
                        {
                            return player;
                        }
                    }
                }
            }

            return -1;
        }
    }

    //Default board geometry of the group 10 painter. Also used by the group 10 human player for painters
    //of other groups, as long as no shared geometry has been agreed on.
    public static class G10_VierGewinntLayout
    {
        public const int Left = 20;
        public const int Top = 80;
        public const int Cell = 50;

        public static int ColumnAt(int x, int y)
        {
            if (x < Left)
            {
                return -1;
            }

            int column = (x - Left) / Cell;
            return column < G10_VierGewinntLogic.Columns ? column : -1;
        }
    }

    public class G10_FallingDisc : IG10_FallingDisc
    {
        int _Row = 0;
        int _Column = 0;
        int _PlayerNumber = 0;
        double _Position = -1;
        double _Speed = 0;

        public G10_FallingDisc(int row, int column, int playerNumber)
        {
            _Row = row;
            _Column = column;
            _PlayerNumber = playerNumber;
        }

        public int Row { get { return _Row; } }

        public int Column { get { return _Column; } }

        public int PlayerNumber { get { return _PlayerNumber; } }

        public double Position { get { return _Position; } }

        //Moves the disc one tick further; returns true once it has come to rest
        public bool Tick()
        {
            const double Gravity = 0.06;

            _Speed += Gravity;
            _Position += _Speed;
            if (_Position >= _Row)
            {
                _Position = _Row;
                if (_Speed < 0.3)
                {
                    return true;
                }

                //Small bounce on impact
                _Speed = -_Speed * 0.25;
            }

            return false;
        }
    }

    public class G10_VierGewinntField : IG10_AnimatedFeld
    {
        int[,] _Field = new int[G10_VierGewinntLogic.Rows, G10_VierGewinntLogic.Columns];
        Queue<G10_FallingDisc> _FallingDiscs = new Queue<G10_FallingDisc>();

        public G10_VierGewinntField()
        {
        }

        //Copy constructor, used by the computer player to try out moves on the field of any group
        public G10_VierGewinntField(I4GWFeld other)
        {
            for (int r = 0; r < G10_VierGewinntLogic.Rows; r++)
            {
                for (int c = 0; c < G10_VierGewinntLogic.Columns; c++)
                {
                    _Field[r, c] = other[r, c];
                }
            }
        }

        //Returns -1 for positions outside of the field, ignores setting them
        public int this[int r, int c]
        {
            get
            {
                if (G10_VierGewinntLogic.IsInside(r, c))
                {
                    return _Field[r, c];
                }

                return -1;
            }

            set
            {
                if (G10_VierGewinntLogic.IsInside(r, c))
                {
                    _Field[r, c] = value;
                }
            }
        }

        public bool IsAnimating { get { return _FallingDiscs.Count > 0; } }

        public IEnumerable<IG10_FallingDisc> FallingDiscs { get { return _FallingDiscs; } }

        //Places the disc immediately and queues its falling animation
        public void DropDisc(int row, int column, int playerNumber)
        {
            _Field[row, column] = playerNumber;
            _FallingDiscs.Enqueue(new G10_FallingDisc(row, column, playerNumber));
        }

        public void TickAnimation()
        {
            if (IsAnimating && _FallingDiscs.Peek().Tick())
            {
                _FallingDiscs.Dequeue();
            }
        }

        //The shared interfaces define no painter interface, so every painter is accepted;
        //painters that cannot handle an I4GWFeld simply paint nothing
        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter != null;
        }
    }

    public class G10_VierGewinntMove : I4GWMove
    {
        int _Row = 0;
        int _Column = 0;
        int _PlayerNumber = 0;

        public G10_VierGewinntMove(int row, int column, int playerNumber)
        {
            _Row = row;
            _Column = column;
            _PlayerNumber = playerNumber;
        }

        public int Row { get { return _Row; } }

        public int Column { get { return _Column; } }

        public int PlayerNumber { get { return _PlayerNumber; } }
    }

    //IGameRules2 is used to advance the falling animation every tick
    public class G10_VierGewinntRules : IGameRules2
    {
        G10_VierGewinntField _Field = new G10_VierGewinntField();

        //Player 1 starts, then the players alternate
        int _NextPlayer = 1;

        public string Name { get { return "10 Lacher VierGewinnt Rules"; } }

        public IGameField CurrentField { get { return _Field; } }

        //No more moves after a win or on a full field (draw)
        public bool MovesPossible { get { return G10_VierGewinntLogic.Winner(_Field) < 0 && !G10_VierGewinntLogic.IsFull(_Field); } }

        public void ClearField()
        {
            _Field = new G10_VierGewinntField();
            _NextPlayer = 1;
        }

        public void StartedGameCall()
        {
        }

        public void TickGameCall()
        {
            _Field.TickAnimation();
        }

        //Accepts moves of any group. Rejects moves after a win, of the wrong player and into invalid or full columns.
        //Row is ignored and the landing row is computed instead: other groups may count rows differently, and
        //rejecting all their moves would freeze MainWindow, which hands the turn on after every move.
        public void DoMove(IPlayMove move)
        {
            if (!(move is I4GWMove) || !MovesPossible)
            {
                return;
            }

            I4GWMove m = (I4GWMove)move;
            int row = G10_VierGewinntLogic.FreeRow(_Field, m.Column);
            if (m.PlayerNumber != _NextPlayer || row < 0)
            {
                return;
            }

            _Field.DropDisc(row, m.Column, m.PlayerNumber);
            _NextPlayer = _NextPlayer == 1 ? 2 : 1;
        }

        public int CheckIfPLayerWon()
        {
            return G10_VierGewinntLogic.Winner(_Field);
        }
    }

    //Implements IPaintGame2 so that moves made via keyboard are painted as well
    public class G10_VierGewinntPaint : IG10_Painter, IPaintGame2
    {
        const int Left = G10_VierGewinntLayout.Left;
        const int Top = G10_VierGewinntLayout.Top;
        const int Cell = G10_VierGewinntLayout.Cell;
        const int Rows = G10_VierGewinntLogic.Rows;
        const int Columns = G10_VierGewinntLogic.Columns;

        public string Name { get { return "10 Lacher VierGewinnt Painter"; } }

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            if (currentField is I4GWFeld)
            {
                PaintVierGewinntField(canvas, (I4GWFeld)currentField);
            }
        }

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            PaintGameField(canvas, currentField);
        }

        public int ColumnAt(int x, int y)
        {
            return G10_VierGewinntLayout.ColumnAt(x, y);
        }

        void PaintVierGewinntField(Canvas canvas, I4GWFeld currentField)
        {
            canvas.Children.Clear();
            canvas.Background = Brushes.White;

            //The result is only shown once all discs have landed
            bool animating = currentField is IG10_AnimatedFeld && ((IG10_AnimatedFeld)currentField).IsAnimating;
            int winner = G10_VierGewinntLogic.Winner(currentField);
            string header = "Klick in eine Spalte oder Taste 1-7";
            if (!animating && winner > 0)
            {
                header = "Spieler " + winner + " hat gewonnen!";
            }
            else if (!animating && G10_VierGewinntLogic.IsFull(currentField))
            {
                header = "Unentschieden!";
            }
            AddText(canvas, header, Left, 0, 14);

            for (int c = 0; c < Columns; c++)
            {
                AddText(canvas, (c + 1).ToString(), Left + c * Cell + Cell / 2 - 4, Top - 20, 14);
            }

            //Falling discs only exist for group 10 fields; fields of other groups are painted without animation
            List<IG10_FallingDisc> falling = new List<IG10_FallingDisc>();
            if (currentField is IG10_AnimatedFeld)
            {
                falling = ((IG10_AnimatedFeld)currentField).FallingDiscs.ToList();
            }

            //Discs are painted first and the board with holes on top, so falling discs disappear behind the board
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    if (currentField[r, c] != G10_VierGewinntLogic.Empty && !falling.Any(d => d.Row == r && d.Column == c))
                    {
                        AddDisc(canvas, r, c, currentField[r, c]);
                    }
                }
            }

            if (falling.Count > 0)
            {
                AddDisc(canvas, falling[0].Position, falling[0].Column, falling[0].PlayerNumber);
            }

            GeometryGroup boardShape = new GeometryGroup() { FillRule = FillRule.EvenOdd };
            boardShape.Children.Add(new RectangleGeometry(new Rect(Left, Top, Columns * Cell, Rows * Cell), 8, 8));
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    Point center = new Point(Left + c * Cell + Cell / 2.0, Top + r * Cell + Cell / 2.0);
                    boardShape.Children.Add(new EllipseGeometry(center, Cell / 2.0 - 5, Cell / 2.0 - 5));
                }
            }
            canvas.Children.Add(new Path() { Data = boardShape, Fill = Brushes.RoyalBlue, Stroke = Brushes.DarkBlue, StrokeThickness = 1 });
        }

        //row may be fractional while the disc is falling
        void AddDisc(Canvas canvas, double row, int column, int playerNumber)
        {
            Ellipse disc = new Ellipse()
            {
                Width = Cell - 10,
                Height = Cell - 10,
                Fill = playerNumber == 1 ? Brushes.Red : Brushes.Gold,
                Stroke = playerNumber == 1 ? Brushes.DarkRed : Brushes.DarkGoldenrod,
                StrokeThickness = 2
            };
            Canvas.SetLeft(disc, Left + column * Cell + 5);
            Canvas.SetTop(disc, Top + row * Cell + 5);
            canvas.Children.Add(disc);
        }

        void AddText(Canvas canvas, string text, double x, double y, double size)
        {
            TextBlock tb = new TextBlock() { Text = text, FontSize = size };
            Canvas.SetLeft(tb, x);
            Canvas.SetTop(tb, y);
            canvas.Children.Add(tb);
        }
    }

    public class G10_VierGewinntHumanPlayer : IHumanGamePlayer
    {
        int _PlayerNumber = 0;

        public string Name { get { return "10 Lacher VierGewinnt Human Player"; } }

        public int PlayerNumber { get { return _PlayerNumber; } }

        public void SetPlayerNumber(int playerNumber)
        {
            _PlayerNumber = playerNumber;
        }

        //The shared interfaces define no rules interface, so the rules are recognised by their field
        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules != null && rules.CurrentField is I4GWFeld;
        }

        public IGamePlayer Clone()
        {
            G10_VierGewinntHumanPlayer p = new G10_VierGewinntHumanPlayer();
            p.SetPlayerNumber(_PlayerNumber);
            return p;
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (!(field is I4GWFeld))
            {
                return null;
            }

            //Optional group 10 extension: no input while discs are still falling
            if (field is IG10_AnimatedFeld && ((IG10_AnimatedFeld)field).IsAnimating)
            {
                return null;
            }

            int column = -1;
            if (selection is IClickSelection)
            {
                //A group 10 painter knows its own layout; for other painters the default layout is assumed
                IClickSelection sel = (IClickSelection)selection;
                IG10_Painter painter = OOPGamesManager.Singleton.ActivePainter as IG10_Painter;
                column = painter != null
                    ? painter.ColumnAt(sel.XClickPos, sel.YClickPos)
                    : G10_VierGewinntLayout.ColumnAt(sel.XClickPos, sel.YClickPos);
            }
            else if (selection is IKeySelection)
            {
                column = ColumnFromKey(((IKeySelection)selection).Key);
            }

            int row = G10_VierGewinntLogic.FreeRow((I4GWFeld)field, column);
            if (row >= 0)
            {
                return new G10_VierGewinntMove(row, column, _PlayerNumber);
            }

            return null;
        }

        int ColumnFromKey(Key key)
        {
            if (key >= Key.D1 && key <= Key.D7)
            {
                return key - Key.D1;
            }
            if (key >= Key.NumPad1 && key <= Key.NumPad7)
            {
                return key - Key.NumPad1;
            }

            return -1;
        }
    }

    public class G10_VierGewinntComputerPlayer : IComputerGamePlayer
    {
        int _PlayerNumber = 0;
        Random _Random = new Random();

        public string Name { get { return "10 Lacher VierGewinnt Computer Player"; } }

        public int PlayerNumber { get { return _PlayerNumber; } }

        public void SetPlayerNumber(int playerNumber)
        {
            _PlayerNumber = playerNumber;
        }

        //The shared interfaces define no rules interface, so the rules are recognised by their field
        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules != null && rules.CurrentField is I4GWFeld;
        }

        public IGamePlayer Clone()
        {
            G10_VierGewinntComputerPlayer p = new G10_VierGewinntComputerPlayer();
            p.SetPlayerNumber(_PlayerNumber);
            return p;
        }

        //Strategy: 1. win if possible, 2. block the opponent, 3. avoid moves that let the opponent win,
        //4. otherwise pick a random column, preferring the center
        public IPlayMove GetMove(IGameField field)
        {
            if (!(field is I4GWFeld))
            {
                return null;
            }

            I4GWFeld f = (I4GWFeld)field;
            int opponent = _PlayerNumber == 1 ? 2 : 1;

            List<int> freeColumns = new List<int>();
            for (int c = 0; c < G10_VierGewinntLogic.Columns; c++)
            {
                if (G10_VierGewinntLogic.FreeRow(f, c) >= 0)
                {
                    freeColumns.Add(c);
                }
            }

            if (freeColumns.Count == 0)
            {
                return null;
            }

            foreach (int c in freeColumns)
            {
                if (WinsAfterDrop(f, c, _PlayerNumber))
                {
                    return CreateMove(f, c);
                }
            }

            foreach (int c in freeColumns)
            {
                if (WinsAfterDrop(f, c, opponent))
                {
                    return CreateMove(f, c);
                }
            }

            List<int> safeColumns = freeColumns.Where(c => !GivesOpponentWin(f, c, opponent)).ToList();
            List<int> candidates = safeColumns.Count > 0 ? safeColumns : freeColumns;

            //Weight 4 for the center column down to 1 for the outer columns
            List<int> weighted = new List<int>();
            foreach (int c in candidates)
            {
                for (int w = 0; w < 4 - Math.Abs(c - G10_VierGewinntLogic.Columns / 2); w++)
                {
                    weighted.Add(c);
                }
            }

            return CreateMove(f, weighted[_Random.Next(weighted.Count)]);
        }

        G10_VierGewinntMove CreateMove(I4GWFeld field, int column)
        {
            return new G10_VierGewinntMove(G10_VierGewinntLogic.FreeRow(field, column), column, _PlayerNumber);
        }

        //Works on a private copy, so the field of the rules (from any group) is never changed
        bool WinsAfterDrop(I4GWFeld field, int column, int player)
        {
            G10_VierGewinntField copy = new G10_VierGewinntField(field);
            copy[G10_VierGewinntLogic.FreeRow(copy, column), column] = player;
            return G10_VierGewinntLogic.Winner(copy) == player;
        }

        //True if dropping into the column allows the opponent to win by dropping on top of it
        bool GivesOpponentWin(I4GWFeld field, int column, int opponent)
        {
            G10_VierGewinntField copy = new G10_VierGewinntField(field);
            copy[G10_VierGewinntLogic.FreeRow(copy, column), column] = _PlayerNumber;
            return G10_VierGewinntLogic.FreeRow(copy, column) >= 0 && WinsAfterDrop(copy, column, opponent);
        }
    }
}
