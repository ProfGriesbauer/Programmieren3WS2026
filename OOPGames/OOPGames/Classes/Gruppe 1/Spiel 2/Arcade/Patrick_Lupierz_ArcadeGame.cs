using System;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_ArcadeGame
    // Entwickler: Patrick Lupierz
    // Erstellt: 15.11.2026
    // Beschreibung: Gemeinsame Basis der Spielhallen-Spiele (SprintPlanning C11). Ablauf je Runde:
    //               [E] startet (Einsatz 5 Chips über G1C_MoneyManager) → Spiel läuft im Framework-Tick →
    //               Spielende → Belohnung aus der Punktzahl (genau einmal pro Runde, höchstens 30 Chips).
    //               Gezeichnet wird ein Automatengehäuse mit "Bildschirm"; jedes Spiel zeichnet nur sein Feld.
    //               Keine eigene Game-Loop: Update() wird vom GameManager im Framework-Tick aufgerufen.
    // =====================================================
    public abstract class G1C_ArcadeGame : G1C_CasinoGameBase
    {
        public const int EntryFee = 5;
        public const int MaxReward = 30;
        public const double FieldW = 300;
        public const double FieldH = 196;

        protected enum ArcadePhase { Ready, Playing, Over }

        private bool _rewardPaid;

        protected G1C_ArcadeGame(G1C_MoneyManager money, Random random) : base(money, random)
        {
            Message = "[E] Spiel starten (" + EntryFee + " Chips), [Q] Verlassen.";
        }

        protected ArcadePhase Phase { get; private set; } = ArcadePhase.Ready;
        public bool IsPlaying { get { return Phase == ArcadePhase.Playing; } }
        public bool IsOver { get { return Phase == ArcadePhase.Over; } }
        public int Score { get; protected set; }
        public int BestScore { get; private set; }
        public int LastReward { get; private set; }
        protected int Ticks { get; private set; }

        protected abstract string Instructions { get; }
        protected abstract int PointsPerChip { get; }
        protected abstract void ResetRound();
        protected abstract void Step();
        protected abstract void DrawField(G1C_Renderer r, double x, double y);
        protected virtual void OnGameKey(Key key) { }
        protected override Color BackgroundColor { get { return Color.FromRgb(0x12, 0x0E, 0x1C); } }

        // Entwickler: Patrick Lupierz – Belohnung: 1 Chip je PointsPerChip Punkte, gedeckelt
        public static int RewardFor(int score, int pointsPerChip)
        {
            if (pointsPerChip <= 0 || score <= 0) return 0;
            return Math.Min(MaxReward, score / pointsPerChip);
        }

        protected override void OnKey(Key key)
        {
            if (G1C_InputManager.IsBack(key))
            {
                if (IsPlaying) EndRound("Abgebrochen.");
                RequestExit();
                return;
            }
            if (!IsPlaying)
            {
                if (G1C_InputManager.IsConfirm(key)) StartRound();
                return;
            }
            OnGameKey(key);
        }

        // Entwickler: Patrick Lupierz – Einsatz abbuchen, Runde zurücksetzen
        public bool StartRound()
        {
            if (!Money.TryWithdraw(EntryFee))
            {
                Message = "Ein Spiel kostet " + EntryFee + " Chips – du hast zu wenig.";
                return false;
            }
            Score = 0;
            Ticks = 0;
            _rewardPaid = false;
            LastReward = 0;
            ResetRound();
            Phase = ArcadePhase.Playing;
            Message = Instructions;
            return true;
        }

        // Entwickler: Patrick Lupierz – Runde beenden, Belohnung genau einmal auszahlen
        protected void EndRound(string reason)
        {
            if (!IsPlaying) return;
            Phase = ArcadePhase.Over;
            BestScore = Math.Max(BestScore, Score);
            if (!_rewardPaid)
            {
                _rewardPaid = true;
                LastReward = RewardFor(Score, PointsPerChip);
                if (LastReward > 0) Money.Deposit(LastReward);
            }
            Message = reason + " Punkte: " + Score + " – Belohnung: " + LastReward + " Chips. [E] Nochmal, [Q] Verlassen.";
        }

        public override void Update()
        {
            if (!IsPlaying) return;
            Ticks++;
            Step();
        }

        protected static bool Held(params Key[] keys)
        {
            return keys.Any(G1C_InputManager.IsKeyHeld);
        }

        protected override void DrawGame(G1C_Renderer r)
        {
            double fx = (G1C_Renderer.ScreenWidth - FieldW) / 2;
            double fy = 36;
            // Automatengehäuse mit Neonrand
            r.DrawRect(fx - 12, fy - 6, FieldW + 24, FieldH + 12, Color.FromRgb(0x30, 0x20, 0x48), Color.FromRgb(0xF8, 0x60, 0xD0), 2);
            r.FillRect(fx, fy, FieldW, FieldH, Color.FromRgb(0x04, 0x04, 0x0C));
            r.ClipToVirtualRect(fx, fy, FieldW, FieldH);
            DrawField(r, fx, fy);
            r.RestoreClip();
            r.Text("PUNKTE " + Score, fx + 4, fy + 2, 10, Color.FromRgb(0xF8, 0xF8, 0xF8));
            r.Text("REKORD " + BestScore, fx + FieldW - 90, fy + 2, 10, Color.FromRgb(0xF8, 0xD0, 0x40));
            if (!IsPlaying)
            {
                string title = IsOver ? "GAME OVER" : "INSERT COIN";
                r.Text(title, fx, fy + FieldH / 2 - 20, 20, Color.FromRgb(0xF8, 0x60, 0xD0), FieldW);
                r.Text("[E] Start – " + EntryFee + " Chips", fx, fy + FieldH / 2 + 8, 11, Color.FromRgb(0x50, 0xE0, 0xF8), FieldW);
            }
            AddButtonRow(Btn("Start [E]", Key.E, !IsPlaying), Btn("Verlassen [Q]", Key.Q));
        }
    }
}
