using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_SnakeGame
    // Entwickler: Patrick Lupierz
    // Erstellt: 15.11.2026
    // Beschreibung: "Schlange" – Klassiker der 90er-Handys. Raster 20x13; WASD/Pfeile lenken (keine Umkehr
    //               in die eigene Richtung). Jede Frucht +10 Punkte und schnelleres Tempo. Wand oder eigener
    //               Körper beendet die Runde.
    // =====================================================
    public sealed class G1C_SnakeGame : G1C_ArcadeGame
    {
        public const int GridW = 20, GridH = 13;
        private const double Cell = 15;

        private readonly LinkedList<int[]> _body = new LinkedList<int[]>();
        private G1C_Direction _direction, _nextDirection;
        private int[] _food;
        private int _moveTimer;

        public G1C_SnakeGame(G1C_MoneyManager money, Random random) : base(money, random) { }

        public override string Name { get { return "Schlange"; } }
        protected override string Instructions { get { return "WASD/Pfeile lenken. Friss Früchte, beiß dich nicht selbst!"; } }
        protected override int PointsPerChip { get { return 15; } }
        public int Length { get { return _body.Count; } }
        public int[] Head { get { return _body.First.Value; } }
        public int[] Food { get { return _food; } }

        protected override void ResetRound()
        {
            _body.Clear();
            for (int i = 0; i < 3; i++) _body.AddLast(new[] { 6 - i, 6 });
            _direction = _nextDirection = G1C_Direction.Right;
            _moveTimer = 0;
            PlaceFood();
        }

        protected override void OnGameKey(Key key)
        {
            G1C_Direction? d = G1C_InputManager.ToDirection(key);
            if (d.HasValue && d.Value != G1C_DirectionHelper.Opposite(_direction)) _nextDirection = d.Value;
        }

        // Entwickler: Patrick Lupierz – Tempo steigt mit der Länge (alle 6 bis 2 Ticks ein Feld)
        protected override void Step()
        {
            int interval = Math.Max(2, 6 - Score / 60);
            if (++_moveTimer < interval) return;
            _moveTimer = 0;
            Advance();
        }

        // Entwickler: Patrick Lupierz – ein Feld weiter; auch direkt vom Selbsttest aufrufbar
        internal void Advance()
        {
            _direction = _nextDirection;
            int[] head = _body.First.Value;
            int[] next = { head[0] + G1C_DirectionHelper.DeltaX(_direction), head[1] + G1C_DirectionHelper.DeltaY(_direction) };
            bool eats = _food != null && next[0] == _food[0] && next[1] == _food[1];
            int[] tail = _body.Last.Value;
            if (!eats) _body.RemoveLast(); // Schwanz rückt nach (darf vom Kopf betreten werden)

            if (next[0] < 0 || next[1] < 0 || next[0] >= GridW || next[1] >= GridH || Occupied(next[0], next[1]))
            {
                if (!eats) _body.AddLast(tail);
                EndRound(next[0] < 0 || next[1] < 0 || next[0] >= GridW || next[1] >= GridH ? "Gegen die Wand!" : "Selbst gebissen!");
                return;
            }
            _body.AddFirst(next);
            if (eats) { Score += 10; PlaceFood(); }
        }

        // Testhilfe: Frucht gezielt vor den Kopf legen
        internal void DebugPlaceFood(int x, int y) { _food = new[] { x, y }; }

        private bool Occupied(int x, int y)
        {
            foreach (int[] part in _body) if (part[0] == x && part[1] == y) return true;
            return false;
        }

        private void PlaceFood()
        {
            List<int[]> free = new List<int[]>();
            for (int y = 0; y < GridH; y++) for (int x = 0; x < GridW; x++) if (!Occupied(x, y)) free.Add(new[] { x, y });
            _food = free.Count > 0 ? free[Random.Next(free.Count)] : null;
        }

        protected override void DrawField(G1C_Renderer r, double x, double y)
        {
            for (int gy = 0; gy < GridH; gy++)
                for (int gx = 0; gx < GridW; gx++)
                    if ((gx + gy) % 2 == 0) r.FillRect(x + gx * Cell, y + gy * Cell, Cell, Cell, Color.FromRgb(0x10, 0x1C, 0x10));
            if (_food != null) r.FillEllipse(x + _food[0] * Cell + Cell / 2, y + _food[1] * Cell + Cell / 2, 5, 5, Color.FromRgb(0xE8, 0x40, 0x40));
            bool first = true;
            foreach (int[] part in _body)
            {
                Color c = first ? Color.FromRgb(0xA0, 0xF8, 0x60) : Color.FromRgb(0x48, 0xC0, 0x48);
                r.FillRect(x + part[0] * Cell + 1, y + part[1] * Cell + 1, Cell - 2, Cell - 2, c);
                first = false;
            }
        }
    }
}
