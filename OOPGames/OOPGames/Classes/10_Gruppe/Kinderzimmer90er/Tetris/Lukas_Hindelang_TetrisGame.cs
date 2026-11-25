using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace OOPGames
{
    // Stores the best score next to the other Kinderzimmer highscores.
    internal sealed class G10_RoomHighscoreStore : IG10_HighscoreStore
    {
        readonly string _Game;

        public G10_RoomHighscoreStore(string game)
        {
            _Game = game;
        }

        public int Best { get { return G10_RoomHighscores.Get(_Game); } }

        public void Submit(int score)
        {
            G10_RoomHighscores.Save(_Game, score);
        }
    }

    public sealed class G10_TetrisGame : IG10_RoomMiniGame
    {
        const string GameTitle = "Tetris";

        readonly G10_TetrisSession _Session;
        readonly G10_TetrisRenderer _Renderer = new G10_TetrisRenderer();
        readonly G10_TetrisInputMapper _Input;

        public G10_TetrisGame()
            : this(new G10_TetrisSession(new G10_RoomHighscoreStore(GameTitle)),
                new G10_TetrisInputMapper(new G10_WpfKeyStateSource())) { }

        internal G10_TetrisGame(G10_TetrisSession session, G10_TetrisInputMapper input)
        {
            _Session = session;
            _Input = input;
        }

        public string Title { get { return GameTitle; } }
        public int Width { get { return G10_TetrisRenderer.Width; } }
        public int Height { get { return G10_TetrisRenderer.Height; } }
        public bool ExitRequested { get { return false; } }

        public void Start()
        {
            _Input.Reset();
            _Session.Start();
        }

        public void Update(IReadOnlyCollection<Key> keys)
        {
            _Session.Update(G10_TetrisConfig.FixedDeltaSeconds, _Input.Read(keys));
        }

        public void Draw(Canvas canvas)
        {
            _Renderer.Draw(canvas, _Session);
        }
    }
}
