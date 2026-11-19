using System;

namespace OOPGames
{
    internal sealed class G10_InputLatch
    {
        float _Seconds;

        public float Value { get; private set; }
        public bool IsActive { get { return _Seconds > 0; } }

        public void Latch(float value = 0)
        {
            Value = value;
            _Seconds = G10_ShooterConfig.LatchDuration;
        }

        public void Update(float deltaSeconds)
        {
            _Seconds = Math.Max(0, _Seconds - deltaSeconds);
            if (_Seconds == 0) Value = 0;
        }

        public void Clear()
        {
            _Seconds = 0;
            Value = 0;
        }

        public void Reset()
        {
            Clear();
        }
    }
}
