using System.Collections.Generic;
using System.Windows.Input;

namespace OOPGames
{
    public enum G10_RoomPhase
    {
        Room,
        Device,
        Inserting,
        Ejecting,
        Focusing,
        Playing
    }

    public enum G10_RoomDeviceKind
    {
        Atari,
        Gameboy,
        Commodore
    }

    public interface IG10_RoomMiniGame
    {
        string Title { get; }
        int Width { get; }
        int Height { get; }
        void Start();
        void Update(IReadOnlyCollection<Key> keys);
        void Draw(System.Windows.Controls.Canvas canvas);
        bool ExitRequested { get; }
    }

    // Mini games with their own pause menu: Esc / X are passed to Update instead of closing
    // the game. Such a game leaves through ExitRequested.
    public interface IG10_RoomOwnBackKey : IG10_RoomMiniGame
    {
    }
}
