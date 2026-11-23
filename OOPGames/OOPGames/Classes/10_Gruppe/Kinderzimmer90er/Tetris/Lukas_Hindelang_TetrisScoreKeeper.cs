using System;
using System.Collections.Generic;

namespace OOPGames
{
    // Guideline scoring: line clears times level, combo bonus, back-to-back Tetris bonus.
    internal sealed class G10_TetrisScoreKeeper
    {
        static readonly int[] LineScores = { 0, 100, 300, 500, 800 };

        public int Score { get; private set; }
        public int Lines { get; private set; }
        public int Combo { get; private set; }
        public bool BackToBack { get; private set; }
        public int Level
        {
            get { return Math.Min(G10_TetrisConfig.MaxLevel, 1 + Lines / G10_TetrisConfig.LinesPerLevel); }
        }

        public void Reset()
        {
            Score = 0;
            Lines = 0;
            Combo = -1;
            BackToBack = false;
        }

        public void AddSoftDrop(int rows) { Score += rows; }
        public void AddHardDrop(int rows) { Score += rows * 2; }

        public G10_TetrisClearEventArgs AddLock(int clearedLines)
        {
            if (clearedLines == 0)
            {
                Combo = -1;
                return null;
            }

            int level = Level;
            Combo++;
            bool tetris = clearedLines == 4;
            bool backToBack = tetris && BackToBack;
            int points = LineScores[clearedLines] * level;
            if (backToBack) points += points / 2;
            points += 50 * Combo * level;

            BackToBack = tetris;
            Score += points;
            Lines += clearedLines;
            return new G10_TetrisClearEventArgs(clearedLines, points, Combo, backToBack, Level > level);
        }
    }
}
