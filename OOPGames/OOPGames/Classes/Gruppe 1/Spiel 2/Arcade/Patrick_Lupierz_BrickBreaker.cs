using System;
using System.Windows.Input;
using System.Windows.Media;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_BrickBreaker
    // Entwickler: Patrick Lupierz
    // Erstellt: 15.11.2026
    // Beschreibung: "Ziegelbrecher" – Ball-und-Schläger-Spiel (Breakout-Genre, eigene Umsetzung).
    //               [A]/[D] bewegen den Schläger; der Abprallwinkel hängt von der Trefferstelle ab.
    //               8x5 Steine, obere Reihen zählen mehr. 3 Bälle; leeres Feld = neues, schnelleres Level.
    // =====================================================
    public sealed class G1C_BrickBreaker : G1C_ArcadeGame
    {
        public const int BrickCols = 8, BrickRows = 5;
        private const double BrickW = 34, BrickH = 10, PaddleW = 50, PaddleY = FieldH - 12, BallR = 3;

        private readonly bool[,] _bricks = new bool[BrickRows, BrickCols];
        private double _paddleX, _ballX, _ballY, _vx, _vy, _speed;
        private int _lives, _level, _serveDelay;

        public G1C_BrickBreaker(G1C_MoneyManager money, Random random) : base(money, random) { }

        public override string Name { get { return "Ziegelbrecher"; } }
        protected override string Instructions { get { return "[A]/[D] Schläger bewegen. Räum alle Steine ab – 3 Bälle."; } }
        protected override int PointsPerChip { get { return 30; } }
        public int BricksLeft { get { int n = 0; foreach (bool b in _bricks) if (b) n++; return n; } }
        public int Lives { get { return _lives; } }

        protected override void ResetRound()
        {
            _lives = 3;
            _level = 0;
            NextLevel();
        }

        private void NextLevel()
        {
            _level++;
            for (int r = 0; r < BrickRows; r++) for (int c = 0; c < BrickCols; c++) _bricks[r, c] = true;
            _speed = 2.6 + _level * 0.4;
            Serve();
        }

        private void Serve()
        {
            _paddleX = FieldW / 2;
            _ballX = FieldW / 2;
            _ballY = PaddleY - 10;
            double angle = -Math.PI / 2 + (Random.NextDouble() - 0.5) * 0.8;
            _vx = Math.Cos(angle) * _speed;
            _vy = Math.Sin(angle) * _speed;
            _serveDelay = 20;
        }

        private static double BrickX(int c) { return 14 + c * (BrickW + 0); }
        private static double BrickY(int r) { return 20 + r * (BrickH + 3); }

        protected override void OnGameKey(Key key)
        {
            if (G1C_InputManager.IsPrevious(key)) _paddleX -= 10;
            else if (G1C_InputManager.IsNext(key)) _paddleX += 10;
            _paddleX = Math.Max(PaddleW / 2, Math.Min(FieldW - PaddleW / 2, _paddleX));
        }

        protected override void Step()
        {
            if (Held(Key.A, Key.Left)) _paddleX -= 5;
            if (Held(Key.D, Key.Right)) _paddleX += 5;
            _paddleX = Math.Max(PaddleW / 2, Math.Min(FieldW - PaddleW / 2, _paddleX));
            if (_serveDelay > 0) { _serveDelay--; _ballX = _paddleX; return; }
            MoveBall();
        }

        // Entwickler: Patrick Lupierz – Ball bewegen: Wände, Schläger (Winkel nach Trefferstelle), Steine, Aus
        internal void MoveBall()
        {
            _ballX += _vx;
            _ballY += _vy;
            if (_ballX < BallR) { _ballX = BallR; _vx = Math.Abs(_vx); }
            if (_ballX > FieldW - BallR) { _ballX = FieldW - BallR; _vx = -Math.Abs(_vx); }
            if (_ballY < BallR) { _ballY = BallR; _vy = Math.Abs(_vy); }

            if (_vy > 0 && _ballY + BallR >= PaddleY && _ballY + BallR <= PaddleY + 6 && Math.Abs(_ballX - _paddleX) <= PaddleW / 2 + BallR)
            {
                double offset = (_ballX - _paddleX) / (PaddleW / 2);            // -1 .. 1
                double angle = -Math.PI / 2 + offset * 1.0;
                _vx = Math.Cos(angle) * _speed;
                _vy = Math.Sin(angle) * _speed;
                _ballY = PaddleY - BallR;
            }

            for (int r = 0; r < BrickRows; r++)
                for (int c = 0; c < BrickCols; c++)
                {
                    if (!_bricks[r, c]) continue;
                    if (_ballX + BallR < BrickX(c) || _ballX - BallR > BrickX(c) + BrickW ||
                        _ballY + BallR < BrickY(r) || _ballY - BallR > BrickY(r) + BrickH) continue;
                    _bricks[r, c] = false;
                    Score += (BrickRows - r) * 10;
                    _vy = -_vy;
                    if (BricksLeft == 0) { Score += 100; NextLevel(); }
                    return;
                }

            if (_ballY > FieldH + 10)
            {
                _lives--;
                if (_lives <= 0) EndRound("Keine Bälle mehr!");
                else Serve();
            }
        }

        // Testhilfe: Ball direkt unter einen Stein setzen
        internal void DebugAimAtBrick(int row, int col)
        {
            _serveDelay = 0;
            _ballX = BrickX(col) + BrickW / 2;
            _ballY = BrickY(row) + BrickH + BallR + 1;
            _vx = 0;
            _vy = -2;
        }

        internal void DebugDropBall()
        {
            _serveDelay = 0;
            _ballX = 5;
            _ballY = FieldH + 8;
            _vx = 0;
            _vy = 5;
            _paddleX = FieldW - PaddleW / 2;
        }

        protected override void DrawField(G1C_Renderer r, double x, double y)
        {
            Color[] colors =
            {
                Color.FromRgb(0xE8, 0x40, 0x40), Color.FromRgb(0xF8, 0x90, 0x30), Color.FromRgb(0xF8, 0xD0, 0x40),
                Color.FromRgb(0x48, 0xC0, 0x48), Color.FromRgb(0x40, 0x90, 0xF0)
            };
            for (int row = 0; row < BrickRows; row++)
                for (int c = 0; c < BrickCols; c++)
                    if (_bricks[row, c]) r.DrawRect(x + BrickX(c) + 1, y + BrickY(row), BrickW - 2, BrickH, colors[row], Color.FromRgb(0x10, 0x10, 0x10), 1);
            r.FillRect(x + _paddleX - PaddleW / 2, y + PaddleY, PaddleW, 5, Color.FromRgb(0xD0, 0xD8, 0xF0));
            r.FillEllipse(x + _ballX, y + _ballY, BallR, BallR, Color.FromRgb(0xF8, 0xF8, 0xF8));
            for (int i = 0; i < _lives; i++) r.FillEllipse(x + 8 + i * 9, y + FieldH - 5, 3, 3, Color.FromRgb(0xF8, 0xF8, 0xF8));
            r.Text("LEVEL " + _level, x + FieldW / 2 - 24, y + 2, 10, Color.FromRgb(0x50, 0xE0, 0xF8));
        }
    }
}
