using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace OOPGames
{
    // Input of one tick, already free of keys.
    public struct G10_SnakeInput
    {
        public G10_SnakeInput(IReadOnlyList<G10_Direction> turns, bool confirm)
        {
            Turns = turns;
            Confirm = confirm;
        }

        // All direction keys of the tick in the order they were pressed.
        public IReadOnlyList<G10_Direction> Turns { get; private set; }
        // Enter / Z / Space / P: start, pause, resume, new game after GAME OVER.
        public bool Confirm { get; private set; }
    }

    // Adapter between the Kinderzimmer hub and the game: translates keys, saves the highscore
    // and hands the game state to the renderer. The only Snake class that knows about keys.
    public sealed class G10_SnakeGame : IB5RoomMiniGame
    {
        readonly G10_SnakeRenderer _Renderer = new G10_SnakeRenderer();
        G10_SnakeSession _Session;

        public string Title { get { return "Snake"; } }
        public int Width { get { return G10_SnakeRenderer.ScreenWidth; } }
        public int Height { get { return G10_SnakeRenderer.ScreenHeight; } }
        public bool ExitRequested { get { return false; } }

        public void Start()
        {
            NewSession();
        }

        public void Update(IReadOnlyCollection<Key> keys)
        {
            G10_SnakeInput input = ReadInput(keys);
            if (_Session.Phase == G10_SnakePhase.GameOver && input.Confirm)
            {
                // The new game waits in Ready for its first direction key.
                NewSession();
                return;
            }
            _Session.Update(input);
            // Saved at once, because Esc can leave the game at any time.
            if (_Session.IsNewHighScore) B5RoomHighscores.Save(Title, _Session.Score);
        }

        public void Draw(Canvas canvas)
        {
            _Renderer.Draw(canvas, _Session);
        }

        void NewSession()
        {
            _Session = new G10_SnakeSession(B5RoomHighscores.Get(Title), Environment.TickCount);
        }

        static G10_SnakeInput ReadInput(IReadOnlyCollection<Key> keys)
        {
            List<G10_Direction> turns = new List<G10_Direction>();
            bool confirm = false;
            foreach (Key key in keys)
            {
                if (B5RoomKeys.IsUp(key)) turns.Add(G10_Direction.Up);
                else if (B5RoomKeys.IsRight(key)) turns.Add(G10_Direction.Right);
                else if (B5RoomKeys.IsDown(key)) turns.Add(G10_Direction.Down);
                else if (B5RoomKeys.IsLeft(key)) turns.Add(G10_Direction.Left);
                else if (key == Key.Enter || key == Key.Z || key == Key.Space || key == Key.P) confirm = true;
            }
            return new G10_SnakeInput(turns, confirm);
        }
    }
}
