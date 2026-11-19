using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace OOPGames
{
    internal enum G10_TetrisBannerSlot
    {
        Clear,
        LevelUp
    }

    internal sealed class G10_TetrisBanner
    {
        public G10_TetrisBanner(G10_TetrisBannerSlot slot, string title, string detail, Color color, float startSeconds)
        {
            Slot = slot;
            Title = title;
            Detail = detail;
            Color = color;
            StartSeconds = startSeconds;
        }

        public G10_TetrisBannerSlot Slot { get; }
        public string Title { get; }
        public string Detail { get; }
        public Color Color { get; }
        public float StartSeconds { get; }
    }

    internal readonly struct G10_TetrisDropTrail
    {
        public G10_TetrisDropTrail(int column, int fromRow, int toRow, Color color, float startSeconds)
        {
            Column = column;
            FromRow = fromRow;
            ToRow = toRow;
            Color = color;
            StartSeconds = startSeconds;
        }

        public int Column { get; }
        public int FromRow { get; }
        public int ToRow { get; }
        public Color Color { get; }
        public float StartSeconds { get; }
    }

    // Short-lived visual feedback that is not part of the game state. Ages are measured
    // in the session's play time, so effects freeze while the game is paused.
    internal sealed class G10_TetrisEffects
    {
        public const float BannerSeconds = 1.3f;
        public const float TrailSeconds = 0.22f;

        static readonly string[] ClearTitles = { "", "SINGLE", "DOUBLE", "TRIPLE", "TETRIS!" };
        static readonly Color ClearColor = Color.FromRgb(236, 242, 255);
        static readonly Color TetrisColor = Color.FromRgb(255, 214, 92);
        static readonly Color LevelColor = Color.FromRgb(110, 232, 160);

        readonly List<G10_TetrisBanner> _Banners = new List<G10_TetrisBanner>();
        readonly List<G10_TetrisDropTrail> _Trails = new List<G10_TetrisDropTrail>();
        IG10_TetrisView _Session;

        public IReadOnlyList<G10_TetrisBanner> Banners { get { return _Banners; } }
        public IReadOnlyList<G10_TetrisDropTrail> Trails { get { return _Trails; } }

        public void Attach(IG10_TetrisView session)
        {
            if (ReferenceEquals(_Session, session)) return;
            if (_Session != null)
            {
                _Session.LinesCleared -= OnLinesCleared;
                _Session.HardDropped -= OnHardDropped;
            }

            _Session = session;
            _Banners.Clear();
            _Trails.Clear();
            session.LinesCleared += OnLinesCleared;
            session.HardDropped += OnHardDropped;
        }

        public void Update(float now)
        {
            // A restart sets the play time back to zero; old effects must not stay forever.
            _Banners.RemoveAll(banner => now < banner.StartSeconds || now - banner.StartSeconds > BannerSeconds);
            _Trails.RemoveAll(trail => now < trail.StartSeconds || now - trail.StartSeconds > TrailSeconds);
        }

        void OnLinesCleared(object sender, G10_TetrisClearEventArgs args)
        {
            float now = _Session.PlaySeconds;
            string detail = "+" + args.Points;
            if (args.BackToBack) detail = "BACK-TO-BACK " + detail;
            else if (args.Combo > 0) detail = "COMBO x" + args.Combo + " " + detail;

            _Banners.RemoveAll(banner => banner.Slot == G10_TetrisBannerSlot.Clear);
            _Banners.Add(new G10_TetrisBanner(G10_TetrisBannerSlot.Clear, ClearTitles[args.Lines], detail,
                args.Lines == 4 ? TetrisColor : ClearColor, now));
            if (args.LevelUp)
                _Banners.Add(new G10_TetrisBanner(G10_TetrisBannerSlot.LevelUp, "LEVEL UP",
                    "LEVEL " + _Session.Level, LevelColor, now));
        }

        void OnHardDropped(object sender, G10_TetrisHardDropEventArgs args)
        {
            if (args.Distance == 0) return;

            G10_FallingPiece piece = args.Piece;
            for (int column = 0; column < G10_TetrisConfig.Columns; column++)
            {
                int top = int.MaxValue;
                foreach (G10_TetrominoCell cell in piece.Cells)
                    if (piece.X + cell.X == column) top = Math.Min(top, piece.Y + cell.Y);
                if (top == int.MaxValue) continue;
                _Trails.Add(new G10_TetrisDropTrail(column, top - args.Distance, top,
                    piece.Shape.Color, _Session.PlaySeconds));
            }
        }
    }
}
