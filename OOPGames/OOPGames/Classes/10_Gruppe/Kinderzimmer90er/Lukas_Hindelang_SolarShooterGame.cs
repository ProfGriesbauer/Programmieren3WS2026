using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace OOPGames
{
    public sealed class G10_SolarShooterGame : IB5RoomMiniGame
    {
        readonly G10_VoidCourierRenderer _Renderer;
        readonly G10_ShooterInputMapper _InputMapper;
        G10_SolarShooterSession _Session;

        public G10_SolarShooterGame()
            : this(new G10_VoidCourierRenderer(),
                new G10_ShooterInputMapper(
                    new G10_WpfKeyStateSource(),
                    G10_ShooterKeyBindings.Default)) { }

        internal G10_SolarShooterGame(
            G10_VoidCourierRenderer renderer,
            G10_ShooterInputMapper inputMapper)
        {
            _Renderer = renderer;
            _InputMapper = inputMapper;
        }

        public string Title { get { return "Void Courier"; } }
        public int Width { get { return (int)G10_ShooterConfig.WorldWidth; } }
        public int Height { get { return (int)G10_ShooterConfig.WorldHeight; } }
        public bool ExitRequested { get { return false; } }

        public void Start()
        {
            _Session = new G10_SolarShooterSession();
            _InputMapper.Reset();
        }

        public void Update(System.Collections.Generic.IReadOnlyCollection<Key> keys)
        {
            if (_Session == null) Start();

            G10_SolarShooterInput input;
            bool restart = _InputMapper.Read(out input);

            if ((_Session.IsGameOver || _Session.IsVictory) && restart)
            {
                Start();
                return;
            }

            _Session.Update(G10_ShooterConfig.FixedDeltaSeconds, input);
        }

        public void Draw(Canvas canvas)
        {
            if (_Session == null) Start();
            _Renderer.Draw(canvas, _Session);
        }
    }
}
