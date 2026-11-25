using System;
using System.Windows.Input;
using System.Windows.Media;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_PongDuel
    // Entwickler: Patrick Lupierz
    // Erstellt: 15.11.2026
    // Beschreibung: "Pong-Duell" gegen die Automaten-KI (G1C_PongAI). Spieler links ([W]/[S] oder Pfeile),
    //               KI rechts. Wer zuerst 5 Punkte hat, gewinnt. Punkte: 100 je eigenem Treffer, +200 bei Sieg.
    //               Der Ball wird mit jedem Schlägerkontakt etwas schneller.
    // =====================================================
    public sealed class G1C_PongDuel : G1C_ArcadeGame, IG1C_NpcDuelGame   // Änderung durch: Patrick Lupierz – 23.11.2026
    {
        public const int WinningScore = 5;
        private const double PaddleH = 36, PaddleW = 5, BallR = 3, PlayerX = 10, AiX = FieldW - 15;

        private readonly G1C_PongAI _ai;
        private double _playerY, _aiY, _ballX, _ballY, _vx, _vy;
        private int _serveDelay;

        public G1C_PongDuel(G1C_MoneyManager money, Random random) : base(money, random)
        {
            _ai = new G1C_PongAI(Random);
        }

        // Änderung durch: Patrick Lupierz – Datum: 23.11.2026 – Duell gegen einen angesprochenen NPC: beide setzen
        // Chips ein, der Sieger bekommt beide Einsätze (siehe G1C_Conversation "Zum Duell herausfordern").
        public const int DuelStake = 20;
        public string DuelOpponent { get; private set; }
        public bool IsDuel { get { return DuelOpponent != null; } }
        public bool PlayerWon { get { return IsOver && PlayerPoints >= WinningScore; } }

        public void ConfigureDuel(string opponent)
        {
            DuelOpponent = opponent;
            Message = "[E] Duell starten (Einsatz " + DuelStake + " Chips), [Q] Verlassen.";
        }

        public bool DuelDecided { get { return IsDuel && IsOver; } }
        public bool PlayerWonDuel { get { return PlayerWon; } }
        public int NpcPurseChange { get { return PlayerWon ? -DuelStake : DuelStake; } }

        protected override int EntryCost { get { return IsDuel ? DuelStake : EntryFee; } }
        protected override int ComputeReward() { return IsDuel ? (PlayerWon ? 2 * DuelStake : 0) : base.ComputeReward(); }

        private string RightLabel
        {
            get
            {
                if (!IsDuel) return "KI";
                string[] parts = DuelOpponent.Split(' ');
                return parts[parts.Length - 1];
            }
        }

        public override string Name { get { return IsDuel ? "Duell gegen " + DuelOpponent : "Pong-Duell"; } }
        protected override string Instructions { get { return "[W]/[S] bewegen. Erster mit " + WinningScore + " Punkten gewinnt."; } }
        protected override int PointsPerChip { get { return 25; } }
        public int PlayerPoints { get; private set; }
        public int AiPoints { get; private set; }

        protected override void ResetRound()
        {
            PlayerPoints = AiPoints = 0;
            _playerY = _aiY = FieldH / 2;
            Serve(1);
        }

        private void Serve(int direction)
        {
            _ballX = FieldW / 2;
            _ballY = FieldH / 2;
            double angle = (Random.NextDouble() - 0.5) * 0.9;
            _vx = Math.Cos(angle) * 3 * direction;
            _vy = Math.Sin(angle) * 3;
            _serveDelay = 25;
            _ai.NewRally();
        }

        protected override void OnGameKey(Key key)
        {
            if (G1C_InputManager.IsIncrease(key)) _playerY -= 10;
            else if (G1C_InputManager.IsDecrease(key)) _playerY += 10;
            _playerY = Clamp(_playerY);
        }

        private static double Clamp(double y) { return Math.Max(PaddleH / 2, Math.Min(FieldH - PaddleH / 2, y)); }

        protected override void Step()
        {
            if (Held(Key.W, Key.Up)) _playerY -= 4;
            if (Held(Key.S, Key.Down)) _playerY += 4;
            _playerY = Clamp(_playerY);
            _aiY = Clamp(_ai.Move(_aiY, _ballX, _ballY, _vx, _vy, AiX, FieldH));
            if (_serveDelay > 0) { _serveDelay--; return; }
            MoveBall();
        }

        // Entwickler: Patrick Lupierz – Ball, Abpraller an Schlägern (Winkel nach Trefferstelle), Punkte
        internal void MoveBall()
        {
            _ballX += _vx;
            _ballY += _vy;
            if (_ballY < BallR) { _ballY = BallR; _vy = Math.Abs(_vy); }
            if (_ballY > FieldH - BallR) { _ballY = FieldH - BallR; _vy = -Math.Abs(_vy); }

            if (_vx < 0 && _ballX - BallR <= PlayerX + PaddleW && _ballX > PlayerX && Math.Abs(_ballY - _playerY) <= PaddleH / 2 + BallR)
                Bounce(_playerY, 1);
            else if (_vx > 0 && _ballX + BallR >= AiX && _ballX < AiX + PaddleW && Math.Abs(_ballY - _aiY) <= PaddleH / 2 + BallR)
                Bounce(_aiY, -1);

            if (_ballX < -5) { AiPoints++; AfterPoint(-1); }
            else if (_ballX > FieldW + 5) { PlayerPoints++; Score += 100; AfterPoint(1); }
        }

        private void Bounce(double paddleY, int direction)
        {
            double speed = Math.Min(7, Math.Sqrt(_vx * _vx + _vy * _vy) + 0.3);
            double offset = (_ballY - paddleY) / (PaddleH / 2);
            double angle = offset * 0.9;
            _vx = Math.Cos(angle) * speed * direction;
            _vy = Math.Sin(angle) * speed;
            _ai.NewRally();
        }

        private void AfterPoint(int scorer)
        {
            string rival = IsDuel ? DuelOpponent : "die KI";
            if (PlayerPoints >= WinningScore) { Score += 200; EndRound("Gewonnen gegen " + rival + "!"); return; }
            if (AiPoints >= WinningScore) { EndRound(IsDuel ? DuelOpponent + " hat gewonnen." : "Die KI hat gewonnen."); return; }
            Serve(-scorer);
        }

        // Testhilfe: Ball unhaltbar an der KI vorbei bzw. am Spieler vorbei schicken
        internal void DebugShoot(bool towardsAi)
        {
            _serveDelay = 0;
            _ballX = towardsAi ? FieldW + 4 : -4;
            _ballY = 3;
            _vx = towardsAi ? 6 : -6;
            _vy = 0;
        }

        protected override void DrawField(G1C_Renderer r, double x, double y)
        {
            for (double my = 4; my < FieldH; my += 14) r.FillRect(x + FieldW / 2 - 1, y + my, 2, 7, Color.FromRgb(0x40, 0x40, 0x58));
            Color white = Color.FromRgb(0xF0, 0xF0, 0xF0);
            r.FillRect(x + PlayerX, y + _playerY - PaddleH / 2, PaddleW, PaddleH, Color.FromRgb(0x50, 0xE0, 0xF8));
            r.FillRect(x + AiX, y + _aiY - PaddleH / 2, PaddleW, PaddleH, Color.FromRgb(0xF8, 0x60, 0xD0));
            r.FillRect(x + _ballX - BallR, y + _ballY - BallR, BallR * 2, BallR * 2, white);
            r.Text(PlayerPoints + " : " + AiPoints, x, y + 14, 16, white, FieldW);
            r.Text("DU", x + 20, y + FieldH - 14, 9, Color.FromRgb(0x50, 0xE0, 0xF8));
            r.Text(RightLabel, x + FieldW - 64, y + FieldH - 14, 9, Color.FromRgb(0xF8, 0x60, 0xD0), 60);
        }
    }
}
