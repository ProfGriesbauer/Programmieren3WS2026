using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using OOPGames;

internal static class Program
{
    private static int _assertions;

    [STAThread]
    private static int Main(string[] args)
    {
        try
        {
            RunLogicAndInputTests();
            RunAiTacticalTests();

            if (args.Contains("--benchmark", StringComparer.OrdinalIgnoreCase))
            {
                RunBenchmark();
            }

            Console.WriteLine("OK: " + _assertions + " assertions passed.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }

    private static void RunLogicAndInputTests()
    {
        G1_VierGewinntRules rules = new G1_VierGewinntRules();
        I4GWFeld field = (I4GWFeld)rules.CurrentField;
        Assert(rules.MovesPossible, "new game accepts moves");

        rules.DoMove(new G1_VierGewinntMove(4, 0, 1));
        Assert(field[5, 0] == 0, "wrong landing row is rejected");
        rules.DoMove(new G1_VierGewinntMove(5, 0, 1));
        Assert(field[5, 0] == 1, "legal first move is accepted");
        rules.DoMove(new G1_VierGewinntMove(5, 1, 1));
        Assert(field[5, 1] == 0, "out-of-turn move is rejected");
        rules.DoMove(new G1_VierGewinntMove(5, 1, 2));
        Assert(field[5, 1] == 2, "player switches after a legal move");

        rules.ClearField();
        field = (I4GWFeld)rules.CurrentField;
        Assert(field[5, 0] == 0 && rules.MovesPossible,
            "reset clears the board and restores play");
        G1_VierGewinntPaint painter = new G1_VierGewinntPaint();
        Canvas canvas = new Canvas();
        painter.PaintGameField(canvas, rules.CurrentField);
        Assert(canvas.Children.OfType<TextBlock>().Any(label =>
                label.Text == "1" && Canvas.GetLeft(label) == 340),
            "new game displays player one");
        rules.DoMove(new G1_VierGewinntMove(5, 0, 1));
        painter.PaintGameField(canvas, rules.CurrentField);
        Assert(canvas.Children.OfType<TextBlock>().Any(label =>
                label.Text == "2" && Canvas.GetLeft(label) == 340),
            "successful move updates the current-player display");

        AssertWinner(0, 1, 5, 0);
        AssertWinner(1, 0, 0, 3);
        AssertWinner(1, 1, 2, 0);
        AssertWinner(1, -1, 2, 3);

        G1_VierGewinntRules terminalRules = new G1_VierGewinntRules();
        I4GWFeld terminalField = (I4GWFeld)terminalRules.CurrentField;
        for (int column = 0; column < 4; column++)
        {
            terminalField[5, column] = 1;
        }

        terminalRules.DoMove(new G1_VierGewinntMove(5, 4, 1));
        Assert(terminalRules.CheckIfPLayerWon() == 1 && !terminalRules.MovesPossible,
            "winning state prevents further moves");
        Assert(terminalField[5, 4] == 0, "post-win move is rejected");

        G1_VierGewinntRules drawRules = new G1_VierGewinntRules();
        I4GWFeld drawField = (I4GWFeld)drawRules.CurrentField;
        int[,] drawBoard =
        {
            { 1, 2, 2, 2, 1, 1, 2 },
            { 1, 1, 1, 2, 1, 2, 1 },
            { 1, 2, 2, 2, 1, 2, 1 },
            { 2, 1, 1, 1, 2, 1, 1 },
            { 2, 2, 1, 2, 2, 1, 2 },
            { 2, 2, 1, 2, 1, 2, 1 }
        };
        for (int row = 0; row < 6; row++)
        {
            for (int column = 0; column < 7; column++)
            {
                drawField[row, column] = drawBoard[row, column];
            }
        }

        Assert(drawRules.CheckIfPLayerWon() == -1 && !drawRules.MovesPossible,
            "full board without a line is a draw");

        G1_VierGewinntHumanPlayer human = new G1_VierGewinntHumanPlayer();
        human.SetPlayerNumber(1);
        field = new G1_VierGewinntField();
        IPlayMove mouseMove = human.GetMove(new ClickSelection(20 + 4 * 50 + 10, 60, 0), field);
        Assert(mouseMove is I4GWMove && ((I4GWMove)mouseMove).Column == 4 &&
               ((I4GWMove)mouseMove).Row == 5,
            "clicking a column header selects its lowest free cell");
        IPlayMove keyboardMove = human.GetMove(new KeySelection(Key.D7), field);
        Assert(keyboardMove is I4GWMove && ((I4GWMove)keyboardMove).Column == 6,
            "number-row keyboard selection remains supported");
        keyboardMove = human.GetMove(new KeySelection(Key.NumPad1), field);
        Assert(keyboardMove is I4GWMove && ((I4GWMove)keyboardMove).Column == 0,
            "numeric keypad selection remains supported");
        Assert(human.GetMove(new ClickSelection(370, 60, 0), field) == null,
            "click outside the seven columns is rejected");

        for (int row = 0; row < 6; row++)
        {
            field[row, 2] = row % 2 + 1;
        }

        Assert(human.GetMove(new ClickSelection(20 + 2 * 50 + 10, 80, 0), field) == null,
            "full column rejects mouse input");

        painter.PaintGameField(canvas, rules.CurrentField);
        Assert(canvas.Children.OfType<TextBlock>().Any(label => label.Text == "Spieler"),
            "board labels the current player");

        painter.PaintGameField(canvas, drawRules.CurrentField);
        Assert(canvas.Children.OfType<TextBlock>().Any(label => label.Text == "Unentschieden"),
            "board displays draw status");
    }

    private static void AssertWinner(int rowStep, int columnStep, int startRow, int startColumn)
    {
        G1_VierGewinntRules rules = new G1_VierGewinntRules();
        I4GWFeld field = (I4GWFeld)rules.CurrentField;
        for (int index = 0; index < 4; index++)
        {
            field[startRow + index * rowStep, startColumn + index * columnStep] = 1;
        }

        Assert(rules.CheckIfPLayerWon() == 1, "winner found in direction " +
            rowStep + "," + columnStep);
    }

    private static void RunAiTacticalTests()
    {
        G1_VierGewinntComputerPlayer ai = new G1_VierGewinntComputerPlayer();
        ai.SetPlayerNumber(1);
        G1_VierGewinntField field = new G1_VierGewinntField();
        field[5, 0] = 1;
        field[5, 1] = 1;
        field[5, 2] = 1;
        I4GWMove winningMove = ai.GetMove(field) as I4GWMove;
        Assert(winningMove != null && winningMove.Column == 3,
            "AI takes an immediate horizontal win");

        field = new G1_VierGewinntField();
        field[5, 0] = 2;
        field[5, 1] = 2;
        field[5, 2] = 2;
        I4GWMove blockingMove = ai.GetMove(field) as I4GWMove;
        Assert(blockingMove != null && blockingMove.Column == 3,
            "AI blocks a single immediate opponent win");
    }

    private static void RunBenchmark()
    {
        int[][] openings =
        {
            new int[0],
            new[] { 3 },
            new[] { 3, 2, 4, 3 }
        };
        int g1Wins = 0;
        int fgWins = 0;
        int draws = 0;
        TimeSpan g1Time = TimeSpan.Zero;
        TimeSpan fgTime = TimeSpan.Zero;
        int g1Moves = 0;
        int fgMoves = 0;

        for (int openingIndex = 0; openingIndex < openings.Length; openingIndex++)
        {
            for (int orientation = 0; orientation < 2; orientation++)
            {
                bool g1Starts = orientation == 0;
                MatchResult result = PlayMatch(openings[openingIndex], g1Starts);
                if (result.Winner == 0)
                {
                    draws++;
                }
                else if (result.Winner == result.G1PlayerNumber)
                {
                    g1Wins++;
                }
                else
                {
                    fgWins++;
                }

                g1Time += result.G1Time;
                fgTime += result.FgTime;
                g1Moves += result.G1Moves;
                fgMoves += result.FgMoves;
                Console.WriteLine("Benchmark opening " + openingIndex +
                    ", Gruppe 1 als Spieler " + result.G1PlayerNumber +
                    ": Sieger " + (result.Winner == 0 ? "Unentschieden" : result.Winner.ToString()) +
                    ", " + result.Moves + " Züge.");
            }
        }

        Console.WriteLine("Ergebnis: Gruppe 1 " + g1Wins + " Siege, FG " +
            fgWins + " Siege, " + draws + " Remis.");
        Console.WriteLine("KI-Zeit: Gruppe 1 " + g1Time.TotalMilliseconds.ToString("F0") +
            " ms / " + g1Moves + " Züge; FG " + fgTime.TotalMilliseconds.ToString("F0") +
            " ms / " + fgMoves + " Züge.");
    }

    private static MatchResult PlayMatch(int[] opening, bool g1Starts)
    {
        G1_VierGewinntRules g1Rules = new G1_VierGewinntRules();
        FG_ConnectFourRules fgRules = new FG_ConnectFourRules();
        for (int index = 0; index < opening.Length; index++)
        {
            ApplyMove(g1Rules, fgRules, opening[index], index % 2 + 1);
        }

        G1_VierGewinntComputerPlayer g1 = new G1_VierGewinntComputerPlayer();
        FG_ConnectFourComputerPlayer fg = new FG_ConnectFourComputerPlayer();
        int g1PlayerNumber = g1Starts ? 1 : 2;
        int fgPlayerNumber = g1Starts ? 2 : 1;
        g1.SetPlayerNumber(g1PlayerNumber);
        fg.SetPlayerNumber(fgPlayerNumber);

        TimeSpan g1Time = TimeSpan.Zero;
        TimeSpan fgTime = TimeSpan.Zero;
        int g1Moves = 0;
        int fgMoves = 0;
        int nextPlayer = opening.Length % 2 == 0 ? 1 : 2;

        while (g1Rules.MovesPossible && g1Rules.CheckIfPLayerWon() < 0)
        {
            int column;
            Stopwatch timer = Stopwatch.StartNew();
            if (nextPlayer == g1PlayerNumber)
            {
                I4GWMove move = g1.GetMove(g1Rules.CurrentField) as I4GWMove;
                timer.Stop();
                g1Time += timer.Elapsed;
                g1Moves++;
                if (move == null)
                {
                    throw new InvalidOperationException("Gruppe-1-KI lieferte trotz möglichem Zug keinen Zug.");
                }

                column = move.Column;
            }
            else
            {
                IFG_ConnectFourMove move =
                    fg.GetMove(fgRules.CurrentField) as IFG_ConnectFourMove;
                timer.Stop();
                fgTime += timer.Elapsed;
                fgMoves++;
                if (move == null)
                {
                    throw new InvalidOperationException("FG-KI lieferte trotz möglichem Zug keinen Zug.");
                }

                column = move.Column;
            }

            ApplyMove(g1Rules, fgRules, column, nextPlayer);
            nextPlayer = nextPlayer == 1 ? 2 : 1;
            int winner = g1Rules.CheckIfPLayerWon();
            if (winner > 0)
            {
                return new MatchResult(winner, g1PlayerNumber, g1Time, fgTime,
                    g1Moves, fgMoves, g1Moves + fgMoves + opening.Length);
            }
        }

        return new MatchResult(0, g1PlayerNumber, g1Time, fgTime, g1Moves, fgMoves,
            g1Moves + fgMoves + opening.Length);
    }

    private static void ApplyMove(G1_VierGewinntRules g1Rules,
        FG_ConnectFourRules fgRules, int column, int player)
    {
        I4GWFeld g1Field = (I4GWFeld)g1Rules.CurrentField;
        int row = 5;
        while (row >= 0 && g1Field[row, column] != 0)
        {
            row--;
        }

        if (row < 0)
        {
            throw new InvalidOperationException("Benchmark attempted to play in a full column.");
        }

        g1Rules.DoMove(new G1_VierGewinntMove(row, column, player));
        fgRules.DoMove(new FG_ConnectFourMove(column, player));
        if (g1Rules.CheckIfPLayerWon() != fgRules.CheckIfPLayerWon())
        {
            throw new InvalidOperationException("The two rule implementations disagree on the winner.");
        }
    }

    private static void Assert(bool condition, string description)
    {
        _assertions++;
        if (!condition)
        {
            throw new InvalidOperationException("Assertion failed: " + description);
        }
    }

    private sealed class MatchResult
    {
        internal MatchResult(int winner, int g1PlayerNumber, TimeSpan g1Time,
            TimeSpan fgTime, int g1Moves, int fgMoves, int moves)
        {
            Winner = winner;
            G1PlayerNumber = g1PlayerNumber;
            G1Time = g1Time;
            FgTime = fgTime;
            G1Moves = g1Moves;
            FgMoves = fgMoves;
            Moves = moves;
        }

        internal int Winner { get; private set; }
        internal int G1PlayerNumber { get; private set; }
        internal TimeSpan G1Time { get; private set; }
        internal TimeSpan FgTime { get; private set; }
        internal int G1Moves { get; private set; }
        internal int FgMoves { get; private set; }
        internal int Moves { get; private set; }
    }
}
