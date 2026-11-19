using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Input;

namespace OOPGames
{
    public sealed class G10_SolarShooterGame : IB5RoomMiniGame
    {
        const int DisplayWidth = 160;
        const int DisplayHeight = 144;
        const float FixedDeltaSeconds = 0.04f;

        readonly G10_VoidCourierRenderer _Renderer = new G10_VoidCourierRenderer();
        G10_SolarShooterSession _Session;
        float _HorizontalHoldSeconds;
        float _VerticalHoldSeconds;
        float _FireHoldSeconds;
        float _Horizontal;
        float _Vertical;

        public string Title { get { return "Void Courier"; } }
        public int Width { get { return DisplayWidth; } }
        public int Height { get { return DisplayHeight; } }
        public bool ExitRequested { get { return false; } }

        public void Start()
        {
            _Session = new G10_SolarShooterSession();
            _HorizontalHoldSeconds = 0;
            _VerticalHoldSeconds = 0;
            _FireHoldSeconds = 0;
            _Horizontal = 0;
            _Vertical = 0;
        }

        public void Update(IReadOnlyCollection<Key> keys)
        {
            if (_Session == null) Start();

            bool restart = false;
            foreach (Key key in keys)
            {
                if (key == Key.Left || key == Key.A) LatchHorizontal(-1);
                else if (key == Key.Right || key == Key.D) LatchHorizontal(1);
                else if (key == Key.Up || key == Key.W) LatchVertical(-1);
                else if (key == Key.Down || key == Key.S) LatchVertical(1);

                if (key == Key.Z || key == Key.Space) _FireHoldSeconds = 0.12f;
                if (key == Key.Enter) restart = true;
            }

            _HorizontalHoldSeconds = Math.Max(0, _HorizontalHoldSeconds - FixedDeltaSeconds);
            _VerticalHoldSeconds = Math.Max(0, _VerticalHoldSeconds - FixedDeltaSeconds);
            _FireHoldSeconds = Math.Max(0, _FireHoldSeconds - FixedDeltaSeconds);
            G10_SolarShooterInput input = new G10_SolarShooterInput
            {
                Horizontal = _HorizontalHoldSeconds > 0 ? _Horizontal : 0,
                Vertical = _VerticalHoldSeconds > 0 ? _Vertical : 0,
                FirePressed = _FireHoldSeconds > 0
            };

            if ((_Session.IsGameOver || _Session.IsVictory) && (restart || input.FirePressed))
            {
                Start();
                return;
            }

            _Session.Update(FixedDeltaSeconds, input);
        }

        void LatchHorizontal(float value)
        {
            _Horizontal = value;
            _HorizontalHoldSeconds = 0.12f;
        }

        void LatchVertical(float value)
        {
            _Vertical = value;
            _VerticalHoldSeconds = 0.12f;
        }

        public void Draw(Canvas canvas)
        {
            if (_Session == null) Start();
            _Renderer.Draw(canvas, _Session);
        }
    }
}
