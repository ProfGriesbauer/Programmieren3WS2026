using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OOPGames
{
    public sealed class G10_RoomGameEntry
    {
        public G10_RoomGameEntry(string title, Color cartridgeColor, Func<IG10_RoomMiniGame> create)
        {
            Title = title;
            CartridgeColor = cartridgeColor;
            Create = create;
        }

        public string Title { get; private set; }
        public Color CartridgeColor { get; private set; }
        public Func<IG10_RoomMiniGame> Create { get; private set; }
    }

    public sealed class G10_RoomDevice
    {
        public G10_RoomDevice(G10_RoomDeviceKind kind, string name, Color trimColor, Color screenColor, IList<G10_RoomGameEntry> games)
        {
            Kind = kind;
            Name = name;
            TrimColor = trimColor;
            ScreenColor = screenColor;
            Games = games;
        }

        public G10_RoomDeviceKind Kind { get; private set; }
        public string Name { get; private set; }
        public Color TrimColor { get; private set; }
        public Color ScreenColor { get; private set; }
        public IList<G10_RoomGameEntry> Games { get; private set; }
        public int DisplayWidth { get { return Kind == G10_RoomDeviceKind.Gameboy ? 160 : 160; } }
        public int DisplayHeight { get { return Kind == G10_RoomDeviceKind.Gameboy ? 144 : 120; } }
    }

    public static class G10_RoomRegistry
    {
        static readonly IList<G10_RoomDevice> _Devices = new List<G10_RoomDevice>
        {
            new G10_RoomDevice(G10_RoomDeviceKind.Atari, "ATARI-KONSOLE", Color.FromRgb(206, 117, 55), Color.FromRgb(13, 26, 36),
                new List<G10_RoomGameEntry>
                {
                    new G10_RoomGameEntry("Pac-Man", Color.FromRgb(240, 200, 40), () => new G10_PacManGame())
                }),
            new G10_RoomDevice(G10_RoomDeviceKind.Gameboy, "HANDHELD", Color.FromRgb(145, 190, 96), Color.FromRgb(107, 138, 18),
                new List<G10_RoomGameEntry>
                {
                    new G10_RoomGameEntry("Snake", Color.FromRgb(116, 158, 70), () => new G10_SnakeGame()),
                    new G10_RoomGameEntry("Pong", Color.FromRgb(202, 185, 90), () => new G10_PongGame()),
                    new G10_RoomGameEntry("SolarShooter", Color.FromRgb(92, 151, 92), () => new G10_SolarShooterGame()),
                    new G10_RoomGameEntry("Tetris", Color.FromRgb(112, 165, 201), () => new G10_TetrisGame())
                }),
            new G10_RoomDevice(G10_RoomDeviceKind.Commodore, "HEIMCOMPUTER", Color.FromRgb(124, 112, 218), Color.FromRgb(53, 40, 121),
                new List<G10_RoomGameEntry>
                {
                    new G10_RoomGameEntry("Tower Defense", Color.FromRgb(82, 105, 190), () => new G10_RoomTowerDefenseGame()),
                    new G10_RoomGameEntry("Muenzen sammeln", Color.FromRgb(226, 180, 60), () => new G10_CoinGame()),
                    new G10_RoomGameEntry("Bomberman", Color.FromRgb(159, 78, 68), () => new G10_BombermanGame())
                })
        };

        public static IList<G10_RoomDevice> Devices { get { return _Devices; } }
    }

    public sealed class G10_RoomField : IGameField
    {
        readonly Queue<Key> _Input = new Queue<Key>();
        readonly int[] _InsertedGames = { -1, -1, -1 };
        readonly IG10_RoomMiniGame[] _ActiveGames = new IG10_RoomMiniGame[3];
        int _SelectedDevice;
        double _TransitionTime;
        bool _ShowTitle;

        public G10_RoomField()
        {
            Reset();
        }

        public G10_RoomPhase Phase { get; internal set; }
        public int SelectedDevice { get { return _SelectedDevice; } internal set { _SelectedDevice = value; } }
        public int SelectedGame { get; internal set; }
        public int InsertedGame
        {
            get { return _InsertedGames[SelectedDevice]; }
            internal set { _InsertedGames[SelectedDevice] = value; }
        }
        public int GetInsertedGame(int device) { return _InsertedGames[device]; }
        public int PendingGame { get; internal set; }
        public double TransitionProgress { get { return Math.Min(1.0, _TransitionTime / (Phase == G10_RoomPhase.Ejecting ? 0.5 : 1.6)); } }
        public bool ShowTitle { get { return _ShowTitle; } }
        public IG10_RoomMiniGame ActiveGame
        {
            get { return _ActiveGames[SelectedDevice]; }
            internal set { _ActiveGames[SelectedDevice] = value; }
        }
        public G10_RoomDevice Device { get { return G10_RoomRegistry.Devices[SelectedDevice]; } }
        public bool CanBePaintedBy(IPaintGame painter) { return painter is G10_RoomPaint; }

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

        // Mouse: pointing at a device in the room view selects it, like the arrow keys.
        internal void HoverDevice(int device)
        {
            if (Phase != G10_RoomPhase.Room || device == SelectedDevice) return;
            SelectedDevice = device;
            SelectedGame = 0;
        }

        // Mouse: a click on a device (3D or tab) opens it from the room view, or switches to it in the device view.
        internal void OpenDevice(int device)
        {
            if (Phase != G10_RoomPhase.Room && Phase != G10_RoomPhase.Device) return;
            if (device != SelectedDevice)
            {
                SelectedDevice = device;
                SelectedGame = 0;
                _ShowTitle = false;
            }
            Phase = G10_RoomPhase.Device;
        }

        // Mouse: pointing at a game in the list selects it, like the up/down keys.
        internal void HoverGame(int index)
        {
            if (Phase == G10_RoomPhase.Device && index >= 0 && index < Device.Games.Count) SelectedGame = index;
        }

        internal void BeginInsert(int gameIndex)
        {
            PendingGame = gameIndex;
            _TransitionTime = 0;
            _ShowTitle = false;
            Phase = G10_RoomPhase.Inserting;
        }

        internal void Reset()
        {
            Phase = G10_RoomPhase.Room;
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
            // + / - change the volume of all games in every phase; the games never see these keys.
            keys.RemoveAll(HandleVolumeKey);
            if (Phase == G10_RoomPhase.Playing)
            {
                if (keys.Any(IsBackKey) && !(ActiveGame is IG10_RoomOwnBackKey))
                {
                    Phase = G10_RoomPhase.Device;
                    _ShowTitle = false;
                    return;
                }

                if (ActiveGame != null)
                {
                    ActiveGame.Update(keys);
                    if (ActiveGame.ExitRequested)
                    {
                        Phase = G10_RoomPhase.Device;
                        _ShowTitle = false;
                    }
                }
                return;
            }

            if (Phase == G10_RoomPhase.Focusing)
            {
                if (keys.Any(IsBackKey))
                {
                    Phase = G10_RoomPhase.Device;
                    _ShowTitle = false;
                    return;
                }

                _TransitionTime += 0.04;
                if (_TransitionTime >= 0.8)
                    Phase = G10_RoomPhase.Playing;
                return;
            }

            foreach (Key key in keys)
            {
                if (IsBackKey(key))
                {
                    if (Phase == G10_RoomPhase.Device) Phase = G10_RoomPhase.Room;
                    else if (Phase == G10_RoomPhase.Inserting || Phase == G10_RoomPhase.Ejecting)
                    {
                        Phase = G10_RoomPhase.Device;
                        _ShowTitle = false;
                        PendingGame = -1;
                    }
                    continue;
                }

                if (Phase == G10_RoomPhase.Room)
                {
                    if (G10_RoomKeys.IsLeft(key) || G10_RoomKeys.IsRight(key))
                    {
                        int direction = G10_RoomKeys.IsRight(key) ? 1 : -1;
                        SelectedDevice = (SelectedDevice + direction + G10_RoomRegistry.Devices.Count) % G10_RoomRegistry.Devices.Count;
                        SelectedGame = 0;
                    }
                    else if (IsConfirmKey(key)) Phase = G10_RoomPhase.Device;
                }
                else if (Phase == G10_RoomPhase.Device)
                {
                    int gameIndex = GameIndexFromKey(key);
                    if (gameIndex >= 0 && gameIndex < Device.Games.Count)
                    {
                        SelectedGame = gameIndex;
                    }
                    else if (G10_RoomKeys.IsUp(key) || G10_RoomKeys.IsLeft(key) || G10_RoomKeys.IsDown(key) || G10_RoomKeys.IsRight(key))
                    {
                        int direction = G10_RoomKeys.IsDown(key) || G10_RoomKeys.IsRight(key) ? 1 : -1;
                        int count = Device.Games.Count;
                        SelectedGame = (SelectedGame + direction + count) % count;
                    }
                    else if (IsConfirmKey(key))
                    {
                        if (InsertedGame >= 0 && InsertedGame != SelectedGame)
                        {
                            PendingGame = SelectedGame;
                            _TransitionTime = 0;
                            Phase = G10_RoomPhase.Ejecting;
                        }
                        else if (InsertedGame < 0) BeginInsert(SelectedGame);
                        else StartPlaying();
                    }
                }
            }

            if (Phase == G10_RoomPhase.Inserting)
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
                    _TransitionTime = 0;
                    Phase = G10_RoomPhase.Focusing;
                }
            }
            else if (Phase == G10_RoomPhase.Ejecting)
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
            Phase = G10_RoomPhase.Playing;
        }

        static bool HandleVolumeKey(Key key)
        {
            if (key == Key.OemPlus || key == Key.Add) G10_SoundVolume.Increase();
            else if (key == Key.OemMinus || key == Key.Subtract) G10_SoundVolume.Decrease();
            else return false;
            return true;
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

    public sealed class G10_RoomRules : IGameRules2
    {
        readonly G10_RoomField _Field = new G10_RoomField();
        public string Name { get { return "Gruppe 10 Kinderzimmer (90er) Regeln"; } }
        public IGameField CurrentField { get { return _Field; } }
        public bool MovesPossible { get { return true; } }
        public void DoMove(IPlayMove move) { }
        public void ClearField() { _Field.Reset(); }
        public int CheckIfPLayerWon() { return -1; }
        public void StartedGameCall() { _Field.Reset(); }
        public void TickGameCall() { _Field.Tick(); }
    }

    public sealed class G10_RoomHumanPlayer : IHumanGamePlayer
    {
        int _PlayerNumber;
        public string Name { get { return "Gruppe 10 Kinderzimmer Steuerung"; } }
        public int PlayerNumber { get { return _PlayerNumber; } }
        public void SetPlayerNumber(int playerNumber) { _PlayerNumber = playerNumber; }
        public bool CanBeRuledBy(IGameRules rules) { return rules is G10_RoomRules; }
        public IGamePlayer Clone() { return new G10_RoomHumanPlayer { _PlayerNumber = _PlayerNumber }; }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            G10_RoomField room = field as G10_RoomField;
            IKeySelection keySelection = selection as IKeySelection;
            if (room != null && keySelection != null) room.LatchKey(keySelection.Key);
            return null;
        }
    }
}
