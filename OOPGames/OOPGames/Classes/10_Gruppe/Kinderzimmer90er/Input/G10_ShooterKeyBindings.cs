using System.Windows.Input;

namespace OOPGames
{
    internal sealed class G10_ShooterKeyBindings
    {
        public static readonly G10_ShooterKeyBindings Default = new G10_ShooterKeyBindings();

        G10_ShooterKeyBindings()
        {
        }

        public Key HorizontalNegative { get { return Key.Left; } }
        public Key HorizontalNegativeAlternate { get { return Key.A; } }
        public Key HorizontalPositive { get { return Key.Right; } }
        public Key HorizontalPositiveAlternate { get { return Key.D; } }
        public Key VerticalNegative { get { return Key.Up; } }
        public Key VerticalNegativeAlternate { get { return Key.W; } }
        public Key VerticalPositive { get { return Key.Down; } }
        public Key VerticalPositiveAlternate { get { return Key.S; } }
        public Key FirePrimary { get { return Key.Z; } }
        public Key FireAlternate { get { return Key.Space; } }
        public Key Restart { get { return Key.Enter; } }
        public string RestartPrompt { get { return "PRESS ENTER"; } }
    }
}
