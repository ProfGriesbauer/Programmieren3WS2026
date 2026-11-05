using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OOPGames
{
    public sealed class B5RoomGameEntry
    {
        public B5RoomGameEntry(string title, Color cartridgeColor, Func<IB5RoomMiniGame> create)
        {
            Title = title;
            CartridgeColor = cartridgeColor;
            Create = create;
        }

        public string Title { get; private set; }
        public Color CartridgeColor { get; private set; }
        public Func<IB5RoomMiniGame> Create { get; private set; }
    }

    public sealed class B5RoomDevice
    {
        public B5RoomDevice(B5RoomDeviceKind kind, string name, Color trimColor, Color screenColor, IList<B5RoomGameEntry> games)
        {
            Kind = kind;
            Name = name;
            TrimColor = trimColor;
            ScreenColor = screenColor;
            Games = games;
        }

        public B5RoomDeviceKind Kind { get; private set; }
        public string Name { get; private set; }
        public Color TrimColor { get; private set; }
        public Color ScreenColor { get; private set; }
        public IList<B5RoomGameEntry> Games { get; private set; }
        public int DisplayWidth { get { return Kind == B5RoomDeviceKind.Gameboy ? 160 : 160; } }
        public int DisplayHeight { get { return Kind == B5RoomDeviceKind.Gameboy ? 144 : 120; } }
    }

    public static class B5RoomRegistry
    {
        static readonly IList<B5RoomDevice> _Devices = new List<B5RoomDevice>
        {
            new B5RoomDevice(B5RoomDeviceKind.Atari, "ATARI-KONSOLE", Color.FromRgb(206, 117, 55), Color.FromRgb(13, 26, 36),
                new List<B5RoomGameEntry> { new B5RoomGameEntry("Muenzen sammeln", Color.FromRgb(224, 89, 49), () => new B5RoomCoinGame("Muenzen sammeln", Color.FromRgb(13, 26, 36), Color.FromRgb(242, 151, 65))) }),
            new B5RoomDevice(B5RoomDeviceKind.Gameboy, "HANDHELD", Color.FromRgb(145, 190, 96), Color.FromRgb(107, 138, 18),
                new List<B5RoomGameEntry>
                {
                    new B5RoomGameEntry("Snake", Color.FromRgb(116, 158, 70), () => new B5RoomSnakeGame()),
                    new B5RoomGameEntry("Pong", Color.FromRgb(202, 185, 90), () => new B5RoomPongGame())
                }),
            new B5RoomDevice(B5RoomDeviceKind.Commodore, "HEIMCOMPUTER", Color.FromRgb(124, 112, 218), Color.FromRgb(53, 40, 121),
                new List<B5RoomGameEntry> { new B5RoomGameEntry("Muenzen sammeln", Color.FromRgb(124, 112, 218), () => new B5RoomCoinGame("READY. / Muenzen sammeln", Color.FromRgb(53, 40, 121), Color.FromRgb(216, 205, 132))) })
        };

        public static IList<B5RoomDevice> Devices { get { return _Devices; } }
    }

    public sealed class B5RoomField : IGameField
    {
        readonly Queue<Key> _Input = new Queue<Key>();
        readonly int[] _InsertedGames = { -1, -1, -1 };
        readonly IB5RoomMiniGame[] _ActiveGames = new IB5RoomMiniGame[3];
        int _SelectedDevice;
        double _TransitionTime;
        bool _ShowTitle;

        public B5RoomField()
        {
            Reset();
        }

        public B5RoomPhase Phase { get; internal set; }
        public int SelectedDevice { get { return _SelectedDevice; } internal set { _SelectedDevice = value; } }
        public int SelectedGame { get; internal set; }
        public int InsertedGame
        {
            get { return _InsertedGames[SelectedDevice]; }
            internal set { _InsertedGames[SelectedDevice] = value; }
        }
        public int PendingGame { get; internal set; }
        public double TransitionProgress { get { return Math.Min(1.0, _TransitionTime / (Phase == B5RoomPhase.Ejecting ? 0.5 : 1.6)); } }
        public bool ShowTitle { get { return _ShowTitle; } }
        public IB5RoomMiniGame ActiveGame
        {
            get { return _ActiveGames[SelectedDevice]; }
            internal set { _ActiveGames[SelectedDevice] = value; }
        }
        public B5RoomDevice Device { get { return B5RoomRegistry.Devices[SelectedDevice]; } }
        public bool CanBePaintedBy(IPaintGame painter) { return painter is B5RoomPaint; }

        internal void LatchKey(Key key)
        {
            _Input.Enqueue(key);
        }

        internal List<Key> DrainInput()
        {
            List<Key> keys = new List<Key>();
            while (_Input.Count > 0) keys.Add(_Input.Dequeue());
            return keys;
        }

        internal void BeginInsert(int gameIndex)
        {
            PendingGame = gameIndex;
            _TransitionTime = 0;
            _ShowTitle = false;
            Phase = B5RoomPhase.Inserting;
        }

        internal void Reset()
        {
            Phase = B5RoomPhase.Room;
            SelectedDevice = 0;
            SelectedGame = 0;
            for (int i = 0; i < _InsertedGames.Length; i++)
            {
                _InsertedGames[i] = -1;
                _ActiveGames[i] = null;
            }
            PendingGame = -1;
            _TransitionTime = 0;
            _ShowTitle = false;
            _Input.Clear();
        }

        internal void Tick()
        {
            List<Key> keys = DrainInput();
            if (Phase == B5RoomPhase.Playing)
            {
                if (keys.Any(IsBackKey))
                {
                    Phase = B5RoomPhase.Device;
                    _ShowTitle = false;
                    return;
                }

                if (ActiveGame != null)
                {
                    ActiveGame.Update(keys);
                    if (ActiveGame.ExitRequested)
                    {
                        Phase = B5RoomPhase.Device;
                        _ShowTitle = false;
                    }
                }
                return;
            }

            foreach (Key key in keys)
            {
                if (IsBackKey(key))
                {
                    if (Phase == B5RoomPhase.Device) Phase = B5RoomPhase.Room;
                    else if (Phase == B5RoomPhase.Inserting || Phase == B5RoomPhase.Ejecting)
                    {
                        Phase = B5RoomPhase.Device;
                        _ShowTitle = false;
                        PendingGame = -1;
                    }
                    continue;
                }

                if (Phase == B5RoomPhase.Room)
                {
                    if (key == Key.Left || key == Key.Right)
                    {
                        int direction = key == Key.Right ? 1 : -1;
                        SelectedDevice = (SelectedDevice + direction + B5RoomRegistry.Devices.Count) % B5RoomRegistry.Devices.Count;
                        SelectedGame = 0;
                    }
                    else if (IsConfirmKey(key)) Phase = B5RoomPhase.Device;
                }
                else if (Phase == B5RoomPhase.Device)
                {
                    int gameIndex = GameIndexFromKey(key);
                    if (gameIndex >= 0 && gameIndex < Device.Games.Count)
                    {
                        SelectedGame = gameIndex;
                    }
                    else if (IsConfirmKey(key))
                    {
                        if (InsertedGame >= 0 && InsertedGame != SelectedGame)
                        {
                            PendingGame = SelectedGame;
                            _TransitionTime = 0;
                            Phase = B5RoomPhase.Ejecting;
                        }
                        else if (InsertedGame < 0) BeginInsert(SelectedGame);
                        else StartPlaying();
                    }
                }
            }

            if (Phase == B5RoomPhase.Inserting)
            {
                _TransitionTime += 0.04;
                _ShowTitle = _TransitionTime >= 1.6;
                if (_TransitionTime >= 1.95)
                {
                    InsertedGame = PendingGame;
                    SelectedGame = PendingGame;
                    ActiveGame = Device.Games[InsertedGame].Create();
                    ActiveGame.Start();
                    _ShowTitle = false;
                    Phase = B5RoomPhase.Playing;
                }
            }
            else if (Phase == B5RoomPhase.Ejecting)
            {
                _TransitionTime += 0.04;
                if (_TransitionTime >= 0.5)
                {
                    InsertedGame = -1;
                    ActiveGame = null;
                    BeginInsert(PendingGame);
                }
            }
        }

        void StartPlaying()
        {
            if (InsertedGame < 0) return;
            ActiveGame = Device.Games[InsertedGame].Create();
            ActiveGame.Start();
            Phase = B5RoomPhase.Playing;
        }

        static bool IsBackKey(Key key) { return key == Key.Escape || key == Key.X; }
        static bool IsConfirmKey(Key key) { return key == Key.Enter || key == Key.Z; }

        static int GameIndexFromKey(Key key)
        {
            if (key >= Key.D1 && key <= Key.D9) return (int)key - (int)Key.D1;
            if (key >= Key.NumPad1 && key <= Key.NumPad9) return (int)key - (int)Key.NumPad1;
            return -1;
        }
    }

    public sealed class B5RoomRules : IGameRules2
    {
        readonly B5RoomField _Field = new B5RoomField();
        public string Name { get { return "Kinderzimmer (90er) Regeln"; } }
        public IGameField CurrentField { get { return _Field; } }
        public bool MovesPossible { get { return true; } }
        public void DoMove(IPlayMove move) { }
        public void ClearField() { _Field.Reset(); }
        public int CheckIfPLayerWon() { return -1; }
        public void StartedGameCall() { _Field.Reset(); }
        public void TickGameCall() { _Field.Tick(); }
    }

    public sealed class B5RoomHumanPlayer : IHumanGamePlayer
    {
        int _PlayerNumber;
        public string Name { get { return "Kinderzimmer Steuerung"; } }
        public int PlayerNumber { get { return _PlayerNumber; } }
        public void SetPlayerNumber(int playerNumber) { _PlayerNumber = playerNumber; }
        public bool CanBeRuledBy(IGameRules rules) { return rules is B5RoomRules; }
        public IGamePlayer Clone() { return new B5RoomHumanPlayer { _PlayerNumber = _PlayerNumber }; }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            B5RoomField room = field as B5RoomField;
            IKeySelection keySelection = selection as IKeySelection;
            if (room != null && keySelection != null) room.LatchKey(keySelection.Key);
            return null;
        }
    }
}
