using System;
using System.Collections.Generic;

namespace OOPGames
{
    // A whole game from stage 1 to GAME OVER or the last stage: score, lives, stage progress and
    // the current phase. The stage itself is played by a G10_BomberRound.
    public sealed class G10_BomberSession : IG10_BomberView
    {
        public const int StartLives = 2;
        const int PointsPerSecondLeft = 10;

        readonly Random _Random;
        readonly G10_BomberScore _Score;
        readonly G10_BomberPlayerStats _Stats = new G10_BomberPlayerStats();
        readonly List<G10_BomberSoundCue> _Cues = new List<G10_BomberSoundCue>();
        G10_BomberRound _Round;
        IG10_BomberPhase _Phase;

        public G10_BomberSession(int highScore, int seed)
        {
            _Random = new Random(seed);
            _Score = new G10_BomberScore(highScore);
            Stage = 1;
            Lives = StartLives;
            StartRound();
            _Phase = G10_BomberPhases.Intro;
        }

        public G10_BomberPhaseKind Phase { get { return _Phase.Kind; } }
        public int PhaseTicks { get; private set; }
        public int Stage { get; private set; }
        public int StageCount { get { return G10_BomberStages.Count; } }
        public int Score { get { return _Score.Score; } }
        public int HighScore { get { return _Score.HighScore; } }
        public int Lives { get; private set; }
        public int TimeBonus { get; private set; }
        public int SecondsLeft { get { return _Round.SecondsLeft; } }
        public bool TimeUpWarning { get { return _Round.TimeUpWarning; } }
        public bool ExitOpen { get { return _Round.ExitOpen; } }
        public IG10_BomberArenaView Arena { get { return _Round.Arena; } }
        public IG10_BomberPlayerView Player { get { return _Round.Player; } }
        public IReadOnlyList<IG10_BomberEnemyView> Enemies { get { return _Round.Enemies; } }
        public IReadOnlyList<IG10_BomberBombView> Bombs { get { return _Round.Bombs; } }
        public IReadOnlyList<IG10_BomberFlameView> Flames { get { return _Round.Flames; } }
        public IReadOnlyList<G10_BomberPopup> Popups { get { return _Round.Popups; } }
        public IReadOnlyList<G10_BomberSoundCue> SoundCues { get { return _Cues; } }

        public void Update(G10_BomberInput input)
        {
            _Cues.Clear();
            IG10_BomberPhase next = _Phase.Update(this, input);
            if (next == _Phase)
            {
                PhaseTicks++;
                return;
            }
            _Phase = next;
            PhaseTicks = 0;
        }

        // The methods below are the actions the phases may take.

        internal G10_BomberRoundResult TickRound(G10_BomberInput input)
        {
            return _Round.Tick(input);
        }

        internal void AwardTimeBonus()
        {
            TimeBonus = _Round.SecondsLeft * PointsPerSecondLeft;
            _Score.Add(TimeBonus);
        }

        // Returns false when no life is left.
        internal bool LoseLife()
        {
            if (Lives == 0) return false;
            Lives--;
            _Stats.LoseLife();
            StartRound();
            return true;
        }

        // Returns false after the last stage.
        internal bool AdvanceStage()
        {
            if (Stage == StageCount) return false;
            Stage++;
            StartRound();
            return true;
        }

        void StartRound()
        {
            _Round = new G10_BomberRound(G10_BomberStages.Get(Stage), _Stats, _Score, _Random, _Cues);
        }
    }
}
