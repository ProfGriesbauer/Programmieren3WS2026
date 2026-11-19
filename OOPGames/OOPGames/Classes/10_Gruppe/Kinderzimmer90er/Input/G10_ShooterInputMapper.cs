namespace OOPGames
{
    internal sealed class G10_ShooterInputMapper
    {
        readonly IG10_KeyStateSource _KeyState;
        readonly G10_ShooterKeyBindings _Bindings;
        readonly G10_KeyAxis _Horizontal;
        readonly G10_KeyAxis _Vertical;
        readonly G10_RisingEdge _RestartEdge = new G10_RisingEdge();
        bool _SuppressUntilRelease;

        public G10_ShooterInputMapper(IG10_KeyStateSource keyState, G10_ShooterKeyBindings bindings)
        {
            _KeyState = keyState;
            _Bindings = bindings;
            _Horizontal = new G10_KeyAxis(
                bindings.HorizontalNegative,
                bindings.HorizontalNegativeAlternate,
                bindings.HorizontalPositive,
                bindings.HorizontalPositiveAlternate);
            _Vertical = new G10_KeyAxis(
                bindings.VerticalNegative,
                bindings.VerticalNegativeAlternate,
                bindings.VerticalPositive,
                bindings.VerticalPositiveAlternate);
        }

        public bool Read(out G10_SolarShooterInput input)
        {
            bool restartDown = _KeyState.IsDown(_Bindings.Restart);
            bool restartPressed = _RestartEdge.Update(restartDown);

            if (_SuppressUntilRelease)
            {
                if (!AreGameplayKeysDown())
                    _SuppressUntilRelease = false;

                input = G10_SolarShooterInput.Neutral;
                return false;
            }

            input = new G10_SolarShooterInput(
                _Horizontal.Read(_KeyState),
                _Vertical.Read(_KeyState),
                _KeyState.IsDown(_Bindings.FirePrimary) ||
                _KeyState.IsDown(_Bindings.FireAlternate));
            return restartPressed;
        }

        public void Reset()
        {
            _RestartEdge.Reset(_KeyState.IsDown(_Bindings.Restart));
            _SuppressUntilRelease = AreGameplayKeysDown();
        }

        bool AreGameplayKeysDown()
        {
            return _Horizontal.IsAnyKeyDown(_KeyState) ||
                   _Vertical.IsAnyKeyDown(_KeyState) ||
                   _KeyState.IsDown(_Bindings.FirePrimary) ||
                   _KeyState.IsDown(_Bindings.FireAlternate) ||
                   _KeyState.IsDown(_Bindings.Restart);
        }
    }

    internal sealed class G10_RisingEdge
    {
        bool _WasDown;

        public bool Update(bool isDown)
        {
            bool pressed = isDown && !_WasDown;
            _WasDown = isDown;
            return pressed;
        }

        public void Reset(bool isDown)
        {
            _WasDown = isDown;
        }
    }
}
