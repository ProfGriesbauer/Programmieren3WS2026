using System.Windows.Input;

namespace OOPGames
{
    public enum G1C_InputKind { Key, Click }

    /// <summary>Eine Eingabe in spielinterner Form (Taste oder Klick in virtuellen Koordinaten).</summary>
    public class G1C_InputEvent
    {
        public G1C_InputKind Kind { get; private set; }
        public Key Key { get; private set; }
        public double X { get; private set; }
        public double Y { get; private set; }

        public static G1C_InputEvent FromKey(Key key)
        {
            return new G1C_InputEvent { Kind = G1C_InputKind.Key, Key = key };
        }

        public static G1C_InputEvent FromClick(double x, double y)
        {
            return new G1C_InputEvent { Kind = G1C_InputKind.Click, X = x, Y = y };
        }
    }

    /// <summary>
    /// Übersetzt die Framework-Eingaben (IKeySelection / IClickSelection) in G1C_InputEvents
    /// und legt die Tastenbelegung zentral fest.
    ///
    /// Hinweis: Leertaste und Enter werden bewusst NICHT verwendet, weil nach dem Klick auf
    /// "Start New Game" der Button den Fokus hat und diese Tasten das Spiel neu starten würden.
    /// </summary>
    public class G1C_InputManager
    {
        public G1C_InputEvent Translate(IMoveSelection selection, G1C_Viewport viewport)
        {
            IKeySelection keySelection = selection as IKeySelection;
            if (keySelection != null)
            {
                return G1C_InputEvent.FromKey(keySelection.Key);
            }

            IClickSelection click = selection as IClickSelection;
            if (click != null)
            {
                // Nur linke Maustaste; ohne gültigen Viewport kann nicht umgerechnet werden
                if (click.ChangedButton != (int)MouseButton.Left) return null;
                if (viewport == null || viewport.Scale <= 0) return null;
                return G1C_InputEvent.FromClick(viewport.ToVirtualX(click.XClickPos), viewport.ToVirtualY(click.YClickPos));
            }

            return null;
        }

        public static G1C_Direction? ToDirection(Key key)
        {
            switch (key)
            {
                case Key.W: case Key.Up: return G1C_Direction.Up;
                case Key.S: case Key.Down: return G1C_Direction.Down;
                case Key.A: case Key.Left: return G1C_Direction.Left;
                case Key.D: case Key.Right: return G1C_Direction.Right;
                default: return null;
            }
        }

        public static bool IsConfirm(Key key) { return key == Key.E; }

        public static bool IsBack(Key key) { return key == Key.Q || key == Key.Escape; }

        public static bool IsIncrease(Key key) { return key == Key.W || key == Key.Up || key == Key.OemPlus || key == Key.Add; }

        public static bool IsDecrease(Key key) { return key == Key.S || key == Key.Down || key == Key.OemMinus || key == Key.Subtract; }

        public static bool IsPrevious(Key key) { return key == Key.A || key == Key.Left; }

        public static bool IsNext(Key key) { return key == Key.D || key == Key.Right; }

        /// <summary>Zifferntaste 0-9 (Hauptfeld oder Nummernblock), sonst -1.</summary>
        public static int ToNumber(Key key)
        {
            if (key >= Key.D0 && key <= Key.D9) return key - Key.D0;
            if (key >= Key.NumPad0 && key <= Key.NumPad9) return key - Key.NumPad0;
            return -1;
        }
    }
}
