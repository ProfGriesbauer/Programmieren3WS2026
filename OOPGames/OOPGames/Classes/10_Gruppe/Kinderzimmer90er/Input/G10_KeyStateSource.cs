using System.Windows.Input;

namespace OOPGames
{
    internal interface IG10_KeyStateSource
    {
        bool IsDown(Key key);
    }

    internal sealed class G10_WpfKeyStateSource : IG10_KeyStateSource
    {
        public bool IsDown(Key key)
        {
            return Keyboard.IsKeyDown(key);
        }
    }
}
