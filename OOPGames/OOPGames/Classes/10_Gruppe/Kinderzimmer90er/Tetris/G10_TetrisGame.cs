using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace OOPGames
{
    public sealed class G10_TetrisGame : IB5RoomMiniGame
    {
        readonly G10_TetrisSession _Session = new G10_TetrisSession();
        readonly G10_TetrisRenderer _Renderer = new G10_TetrisRenderer();
        readonly G10_TetrisInputMapper _Input = new G10_TetrisInputMapper(new G10_WpfKeyStateSource());

        public string Title { get { return "Tetris"; } }
        public int Width { get { return 160; } }
        public int Height { get { return 144; } }
        public bool ExitRequested { get { return false; } }

        public void Start()
        {
            _Input.Reset();
            _Session.Start();
        }

        public void Update(IReadOnlyCollection<Key> keys)
        {
            G10_TetrisInput input = _Input.Read();
            _Session.Update(G10_TetrisConfig.FixedDeltaSeconds, input);
        }

        public void Draw(Canvas canvas)
        {
            _Renderer.Draw(canvas, _Session);
        }
    }
}
