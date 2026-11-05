using System.Collections.Generic;
using System.Windows.Input;

namespace OOPGames
{
    public enum B5RoomPhase
    {
        Room,
        Device,
        Inserting,
        Ejecting,
        Playing
    }

    public enum B5RoomDeviceKind
    {
        Atari,
        Gameboy,
        Commodore
    }

    public interface IB5RoomMiniGame
    {
        string Title { get; }
        int Width { get; }
        int Height { get; }
        void Start();
        void Update(IReadOnlyCollection<Key> keys);
        void Draw(System.Windows.Controls.Canvas canvas);
        bool ExitRequested { get; }
    }
}
