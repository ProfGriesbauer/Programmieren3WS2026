using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_StarHunter
    // Entwickler: Patrick Lupierz
    // Erstellt: 15.11.2026
    // Beschreibung: "Sternenjäger" – Weltraum-Shooter im Stil der 80er/90er (Galaga-Genre, eigene Umsetzung).
    //               Eine Formation (3 Reihen x 7) schwenkt hin und her und rückt am Rand nach unten; zufällige
    //               Gegner der untersten Reihe schießen, einzelne stürzen sich im Bogen auf das Schiff.
    //               [A]/[D] oder Pfeile steuern, [E] schießt. 3 Leben. Neue Welle = schneller.
    // =====================================================
    public sealed class G1C_StarHunter : G1C_ArcadeGame
    {
        private const int Rows = 3, Cols = 7;
        private const double ShipY = FieldH - 16, EnemyW = 14, EnemyH = 10;

        private sealed class Shot { public double X, Y, Vy; }
        private sealed class Diver { public int Row, Col; public double X, Y, Phase; }

        private readonly bool[,] _alive = new bool[Rows, Cols];
        private readonly List<Shot> _shots = new List<Shot>();
        private readonly List<Shot> _enemyShots = new List<Shot>();
        private readonly List<Diver> _divers = new List<Diver>();
        private double _shipX, _formX, _formY, _formDir;
        private int _lives, _wave, _cooldown, _invulnerable;

        public G1C_StarHunter(G1C_MoneyManager money, Random random) : base(money, random) { }

        public override string Name { get { return "Sternenjäger"; } }
        protected override string Instructions { get { return "[A]/[D] steuern, [E] schießen. Rette die Galaxie – 3 Leben."; } }
        protected override int PointsPerChip { get { return 30; } }
        public int Lives { get { return _lives; } }
        public int EnemiesLeft { get { int n = 0; foreach (bool a in _alive) if (a) n++; return n + _divers.Count; } }

        protected override void ResetRound()
        {
            _shipX = FieldW / 2;
            _lives = 3;
            _wave = 0;
            NextWave();
        }

        private void NextWave()
        {
            _wave++;
            for (int r = 0; r < Rows; r++) for (int c = 0; c < Cols; c++) _alive[r, c] = true;
            _shots.Clear(); _enemyShots.Clear(); _divers.Clear();
            _formX = 40; _formY = 24; _formDir = 1;
        }

        private double EnemyX(int c) { return _formX + c * 30; }
        private double EnemyY(int r) { return _formY + r * 18; }

        protected override void OnGameKey(Key key)
        {
            if (G1C_InputManager.IsPrevious(key)) _shipX -= 8;
            else if (G1C_InputManager.IsNext(key)) _shipX += 8;
            else if (G1C_InputManager.IsConfirm(key)) Fire();
            _shipX = Math.Max(10, Math.Min(FieldW - 10, _shipX));
        }

        private void Fire()
        {
            if (_cooldown > 0 || _shots.Count >= 2) return;
            _shots.Add(new Shot { X = _shipX, Y = ShipY - 8, Vy = -7 });
            _cooldown = 6;
        }

        // Entwickler: Patrick Lupierz – ein Tick: Steuerung, Formation, Sturzflüge, Schüsse, Treffer
        protected override void Step()
        {
            if (Held(Key.A, Key.Left)) _shipX = Math.Max(10, _shipX - 4);
            if (Held(Key.D, Key.Right)) _shipX = Math.Min(FieldW - 10, _shipX + 4);
            if (Held(Key.E)) Fire();
            if (_cooldown > 0) _cooldown--;
            if (_invulnerable > 0) _invulnerable--;

            // Formation schwenkt, am Rand eine Zeile tiefer
            double speed = 0.5 + 0.25 * _wave;
            _formX += _formDir * speed;
            if (_formX < 10 || _formX + (Cols - 1) * 30 + EnemyW > FieldW - 10) { _formDir = -_formDir; _formY += 6; }

            // gelegentlich ein Sturzflug
            if (Ticks % Math.Max(30, 90 - _wave * 10) == 0) StartDive();
            for (int i = _divers.Count - 1; i >= 0; i--)
            {
                Diver d = _divers[i];
                d.Phase += 0.08;
                d.Y += 2.2 + _wave * 0.2;
                d.X += Math.Sin(d.Phase) * 2.5 + Math.Sign(_shipX - d.X) * 0.6;
                if (Hits(d.X, d.Y, _shipX, ShipY, 10)) { _divers.RemoveAt(i); LoseLife(); if (!IsPlaying) return; continue; }
                if (d.Y > FieldH + 10) { _divers.RemoveAt(i); _alive[d.Row, d.Col] = true; } // fliegt zurück in die Formation
            }

            // Gegner schießen
            if (Ticks % Math.Max(14, 40 - _wave * 4) == 0) EnemyFire();

            MoveShots();
            if (!IsPlaying) return;

            if (EnemiesLeft == 0) { Score += 50; NextWave(); return; }
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    if (_alive[r, c] && EnemyY(r) + EnemyH >= ShipY - 6) { EndRound("Die Invasoren sind gelandet!"); return; }
        }

        private void StartDive()
        {
            List<int[]> candidates = new List<int[]>();
            for (int r = 0; r < Rows; r++) for (int c = 0; c < Cols; c++) if (_alive[r, c]) candidates.Add(new[] { r, c });
            if (candidates.Count == 0) return;
            int[] pick = candidates[Random.Next(candidates.Count)];
            _alive[pick[0], pick[1]] = false;
            _divers.Add(new Diver { Row = pick[0], Col = pick[1], X = EnemyX(pick[1]) + EnemyW / 2, Y = EnemyY(pick[0]), Phase = Random.NextDouble() * 6 });
        }

        private void EnemyFire()
        {
            int c = Random.Next(Cols);
            for (int r = Rows - 1; r >= 0; r--)
            {
                if (!_alive[r, c]) continue;
                _enemyShots.Add(new Shot { X = EnemyX(c) + EnemyW / 2, Y = EnemyY(r) + EnemyH, Vy = 3 + _wave * 0.3 });
                return;
            }
        }

        private void MoveShots()
        {
            for (int i = _shots.Count - 1; i >= 0; i--)
            {
                Shot s = _shots[i];
                s.Y += s.Vy;
                if (s.Y < 0) { _shots.RemoveAt(i); continue; }
                if (HitEnemy(s)) _shots.RemoveAt(i);
            }
            for (int i = _enemyShots.Count - 1; i >= 0; i--)
            {
                Shot s = _enemyShots[i];
                s.Y += s.Vy;
                if (s.Y > FieldH) { _enemyShots.RemoveAt(i); continue; }
                if (Hits(s.X, s.Y, _shipX, ShipY, 8)) { _enemyShots.RemoveAt(i); LoseLife(); if (!IsPlaying) return; }
            }
        }

        private bool HitEnemy(Shot s)
        {
            for (int r = 0; r < Rows; r++)
                for (int c = 0; c < Cols; c++)
                    if (_alive[r, c] && s.X >= EnemyX(c) && s.X <= EnemyX(c) + EnemyW && s.Y >= EnemyY(r) && s.Y <= EnemyY(r) + EnemyH)
                    {
                        _alive[r, c] = false;
                        Score += r == 0 ? 30 : 10;
                        return true;
                    }
            for (int i = 0; i < _divers.Count; i++)
                if (Hits(s.X, s.Y, _divers[i].X, _divers[i].Y, 8)) { _divers.RemoveAt(i); Score += 40; return true; }
            return false;
        }

        private static bool Hits(double ax, double ay, double bx, double by, double radius)
        {
            return Math.Abs(ax - bx) <= radius && Math.Abs(ay - by) <= radius;
        }

        private void LoseLife()
        {
            if (_invulnerable > 0) return;
            _lives--;
            _enemyShots.Clear();
            _invulnerable = 40;
            if (_lives <= 0) EndRound("Abgeschossen!");
        }

        // Testhilfe: einen Gegner direkt über dem Schiff platzieren (nur für G1C_SelfTest)
        internal void DebugPlaceTargetAboveShip()
        {
            for (int r = 0; r < Rows; r++) for (int c = 0; c < Cols; c++) _alive[r, c] = false;
            _alive[Rows - 1, 3] = true;
            _formDir = 0;
            _shipX = EnemyX(3) + EnemyW / 2;
        }

        protected override void DrawField(G1C_Renderer r, double x, double y)
        {
            for (int i = 0; i < 40; i++)
            {
                double sx = (i * 73) % FieldW, sy = (i * 41 + Ticks * (1 + i % 3)) % FieldH;
                r.FillRect(x + sx, y + sy, 1, 1, Color.FromRgb(0x90, 0x90, 0xB8));
            }
            Color[] rowColors = { Color.FromRgb(0xF8, 0x50, 0x50), Color.FromRgb(0xF8, 0xB0, 0x40), Color.FromRgb(0x60, 0xD0, 0xF8) };
            bool flap = Ticks / 8 % 2 == 0;
            for (int row = 0; row < Rows; row++)
                for (int c = 0; c < Cols; c++)
                    if (_alive[row, c]) DrawBug(r, x + EnemyX(c), y + EnemyY(row), rowColors[row], flap);
            foreach (Diver d in _divers) DrawBug(r, x + d.X - EnemyW / 2, y + d.Y, rowColors[d.Row], !flap);
            foreach (Shot s in _shots) r.FillRect(x + s.X - 1, y + s.Y, 2, 6, Color.FromRgb(0xF8, 0xF8, 0x80));
            foreach (Shot s in _enemyShots) r.FillRect(x + s.X - 1, y + s.Y, 2, 5, Color.FromRgb(0xF8, 0x60, 0xD0));
            if (_invulnerable / 4 % 2 == 0)
            {
                r.FillRect(x + _shipX - 2, y + ShipY - 8, 4, 6, Color.FromRgb(0xF8, 0xF8, 0xF8));
                r.FillRect(x + _shipX - 8, y + ShipY - 2, 16, 6, Color.FromRgb(0xD0, 0xD8, 0xF0));
                r.FillRect(x + _shipX - 8, y + ShipY + 2, 3, 4, Color.FromRgb(0xE8, 0x40, 0x40));
                r.FillRect(x + _shipX + 5, y + ShipY + 2, 3, 4, Color.FromRgb(0xE8, 0x40, 0x40));
            }
            for (int i = 0; i < _lives; i++) r.FillRect(x + 6 + i * 10, y + FieldH - 8, 6, 4, Color.FromRgb(0xD0, 0xD8, 0xF0));
            r.Text("WELLE " + _wave, x + FieldW / 2 - 24, y + 2, 10, Color.FromRgb(0x50, 0xE0, 0xF8));
        }

        private static void DrawBug(G1C_Renderer r, double x, double y, Color c, bool flap)
        {
            r.FillRect(x + 4, y, 6, 8, c);
            r.FillRect(x + (flap ? 0 : 1), y + 2, 4, 5, c);
            r.FillRect(x + (flap ? 10 : 9), y + 2, 4, 5, c);
            r.FillRect(x + 5, y + 2, 1, 2, Color.FromRgb(0x10, 0x10, 0x10));
            r.FillRect(x + 8, y + 2, 1, 2, Color.FromRgb(0x10, 0x10, 0x10));
        }
    }
}
