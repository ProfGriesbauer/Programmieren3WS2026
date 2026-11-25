using System.Windows.Input;

namespace OOPGames
{
    internal readonly struct Gruppe3DragRaceControlCommand
    {
        public Gruppe3DragRaceControlCommand(
            bool accelerate,
            bool brake,
            bool shiftUp,
            bool shiftDown)
        {
            Accelerate = accelerate;
            Brake = brake;
            ShiftUp = shiftUp;
            ShiftDown = shiftDown;
        }

        public bool Accelerate { get; }
        public bool Brake { get; }
        public bool ShiftUp { get; }
        public bool ShiftDown { get; }
    }

    internal sealed class Gruppe3DragRaceKeyboardInput
    {
        private bool _shiftUpPressed;
        private bool _shiftDownPressed;

        public void Reset()
        {
            _shiftUpPressed = false;
            _shiftDownPressed = false;
        }

        public Gruppe3DragRaceControlCommand ReadCommand(int playerNumber)
        {
            bool accelerate;
            bool brake;
            bool shiftUpPressed;
            bool shiftDownPressed;

            if (playerNumber == 1)
            {
                accelerate = Keyboard.IsKeyDown(Key.W);
                brake = Keyboard.IsKeyDown(Key.S);
                shiftUpPressed = Keyboard.IsKeyDown(Key.Q);
                shiftDownPressed = Keyboard.IsKeyDown(Key.E);
            }
            else
            {
                accelerate = Keyboard.IsKeyDown(Key.NumPad8) ||
                    Keyboard.IsKeyDown(Key.Up);
                brake = Keyboard.IsKeyDown(Key.NumPad5) ||
                    Keyboard.IsKeyDown(Key.Down);
                shiftUpPressed = Keyboard.IsKeyDown(Key.NumPad7) ||
                    Keyboard.IsKeyDown(Key.PageUp);
                shiftDownPressed = Keyboard.IsKeyDown(Key.NumPad9) ||
                    Keyboard.IsKeyDown(Key.PageDown);
            }

            bool shiftUp = shiftUpPressed && !_shiftUpPressed;
            bool shiftDown = shiftDownPressed && !_shiftDownPressed;
            _shiftUpPressed = shiftUpPressed;
            _shiftDownPressed = shiftDownPressed;

            return new Gruppe3DragRaceControlCommand(
                accelerate, brake, shiftUp, shiftDown);
        }
    }
}
