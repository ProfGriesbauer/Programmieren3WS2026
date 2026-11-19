using System.Windows.Input;

namespace OOPGames
{
    internal sealed class G10_KeyAxis
    {
        readonly Key _Negative;
        readonly Key _NegativeAlternate;
        readonly Key _Positive;
        readonly Key _PositiveAlternate;

        public G10_KeyAxis(Key negative, Key negativeAlternate, Key positive, Key positiveAlternate)
        {
            _Negative = negative;
            _NegativeAlternate = negativeAlternate;
            _Positive = positive;
            _PositiveAlternate = positiveAlternate;
        }

        public float Read(IG10_KeyStateSource keyState)
        {
            bool negative = keyState.IsDown(_Negative) || keyState.IsDown(_NegativeAlternate);
            bool positive = keyState.IsDown(_Positive) || keyState.IsDown(_PositiveAlternate);

            if (negative == positive) return 0;
            return negative ? -1 : 1;
        }

        public bool IsAnyKeyDown(IG10_KeyStateSource keyState)
        {
            return keyState.IsDown(_Negative) ||
                   keyState.IsDown(_NegativeAlternate) ||
                   keyState.IsDown(_Positive) ||
                   keyState.IsDown(_PositiveAlternate);
        }
    }
}
