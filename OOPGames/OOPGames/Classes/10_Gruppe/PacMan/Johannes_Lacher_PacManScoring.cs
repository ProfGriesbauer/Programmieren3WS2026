using System;

namespace OOPGames
{
    // Score, lives and the ghost combo. Saving the highscore is left to the adapter,
    // so the model does not write files.
    public sealed class G10_ScoreKeeper
    {
        public const int ExtraLifeScore = 10000;
        // Lives in reserve at the start, besides the Pac-Man on the field.
        public const int StartLives = 2;
        const int FirstGhostPoints = 200;
        const int MaxComboStep = 3;

        int _ComboStep;

        public G10_ScoreKeeper(int highScore)
        {
            HighScore = highScore;
            Lives = StartLives;
        }

        public int Score { get; private set; }
        public int HighScore { get; private set; }
        public int Lives { get; private set; }

        public void Add(int points)
        {
            bool belowExtraLife = Score < ExtraLifeScore;
            Score += points;
            if (belowExtraLife && Score >= ExtraLifeScore) Lives++;
            HighScore = Math.Max(HighScore, Score);
        }

        // 200, 400, 800, 1600 for the ghosts eaten during one power pellet.
        public int AwardGhost()
        {
            int points = FirstGhostPoints << _ComboStep;
            _ComboStep = Math.Min(_ComboStep + 1, MaxComboStep);
            Add(points);
            return points;
        }

        public void ResetGhostCombo()
        {
            _ComboStep = 0;
        }

        // Returns false when no life was left, i.e. the game is over.
        public bool LoseLife()
        {
            if (Lives == 0) return false;
            Lives--;
            return true;
        }
    }
}
