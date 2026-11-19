using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace OOPGames
{
    // Input of one tick, already free of keys.
    public struct G10_PongInput
    {
        public G10_PongInput(int left, int right, int menuStep, int modeKey, bool confirm)
        {
            Left = left;
            Right = right;
            MenuStep = menuStep;
            ModeKey = modeKey;
            Confirm = confirm;
        }

        // -1 up, 0 stop, 1 down, for the left and the right paddle.
        public int Left { get; private set; }
        public int Right { get; private set; }
        // -1 / 1 when up or down was pressed this tick (no key repeat), for the menu.
        public int MenuStep { get; private set; }
        // 1 or 2 when a number key chose the number of players, otherwise 0.
        public int ModeKey { get; private set; }
        // Enter / Z / Space / P: choose, pause, resume, back to the menu after the match.
        public bool Confirm { get; private set; }
    }

    // Turns the keys of a tick into G10_PongInput. Alone, W/S and the arrow keys both move the
    // left paddle; with two players W/S move the left and the arrow keys the right paddle.
    // The framework reports key presses only, including the repeats of a held key, so the
    // keyboard state is read as well: a paddle moves as long as its key is held, and a press
    // of a key that was already held is a repeat.
    internal sealed class G10_PongInputReader
    {
        static readonly Key[] TrackedKeys = { Key.W, Key.S, Key.Up, Key.Down, Key.Enter, Key.Space, Key.P, Key.Z };

        readonly IG10_KeyStateSource _KeyState;
        readonly List<Key> _HeldBefore = new List<Key>();

        public G10_PongInputReader(IG10_KeyStateSource keyState)
        {
            if (keyState == null) throw new ArgumentNullException("keyState");
            _KeyState = keyState;
        }

        public void Reset()
        {
            _HeldBefore.Clear();
        }

        public G10_PongInput Read(IReadOnlyCollection<Key> keys, bool twoPlayers)
        {
            int menuStep = 0;
            int modeKey = 0;
            bool confirm = false;
            foreach (Key key in keys)
            {
                if (key == Key.D1 || key == Key.NumPad1) modeKey = 1;
                else if (key == Key.D2 || key == Key.NumPad2) modeKey = 2;
                if (_HeldBefore.Contains(key)) continue;
                if (B5RoomKeys.IsUp(key)) menuStep = -1;
                else if (B5RoomKeys.IsDown(key)) menuStep = 1;
                else if (key == Key.Enter || key == Key.Z || key == Key.Space || key == Key.P) confirm = true;
            }

            int keysWS = Axis(keys, Key.W, Key.S);
            int arrows = Axis(keys, Key.Up, Key.Down);
            _HeldBefore.Clear();
            foreach (Key key in TrackedKeys)
                if (_KeyState.IsDown(key)) _HeldBefore.Add(key);

            if (twoPlayers) return new G10_PongInput(keysWS, arrows, menuStep, modeKey, confirm);
            return new G10_PongInput(Math.Sign(keysWS + arrows), 0, menuStep, modeKey, confirm);
        }

        // A key counts while it is held, and for one tick if it was tapped between two ticks.
        int Axis(IReadOnlyCollection<Key> keys, Key up, Key down)
        {
            bool upActive = _KeyState.IsDown(up) || Contains(keys, up);
            bool downActive = _KeyState.IsDown(down) || Contains(keys, down);
            if (upActive == downActive) return 0;
            return upActive ? -1 : 1;
        }

        static bool Contains(IReadOnlyCollection<Key> keys, Key key)
        {
            foreach (Key pressed in keys)
                if (pressed == key) return true;
            return false;
        }
    }

    // Adapter between the Kinderzimmer hub and the game: reads the keys, saves the best rally
    // and hands the game state to the renderer. The only Pong class that knows about keys.
    public sealed class G10_PongGame : IB5RoomMiniGame
    {
        readonly G10_PongRenderer _Renderer = new G10_PongRenderer();
        readonly G10_PongInputReader _Input = new G10_PongInputReader(new G10_WpfKeyStateSource());
        G10_PongSession _Session;

        public string Title { get { return "Pong"; } }
        public int Width { get { return G10_PongRenderer.ScreenWidth; } }
        public int Height { get { return G10_PongRenderer.ScreenHeight; } }
        public bool ExitRequested { get { return false; } }

        public void Start()
        {
            _Input.Reset();
            _Session = new G10_PongSession(B5RoomHighscores.Get(Title), Environment.TickCount);
        }

        public void Update(IReadOnlyCollection<Key> keys)
        {
            bool twoPlayers = _Session.Phase != G10_PongPhase.Menu && _Session.Mode == G10_PongMode.TwoPlayers;
            _Session.Update(_Input.Read(keys, twoPlayers));
            // Saved at once, because Esc can leave the game at any time.
            if (_Session.IsNewBestRally) B5RoomHighscores.Save(Title, _Session.BestRally);
        }

        public void Draw(Canvas canvas)
        {
            _Renderer.Draw(canvas, _Session);
        }
    }
}
