using System.Collections.Generic;

namespace OOPGames
{
    // Applies the standard value when a value of an enemy kind is missing or out of range
    // and describes each invalid value (German, for the dev tool).
    internal sealed class EnemyValueChecker
    {
        private readonly string _prefix;
        private readonly ICollection<string> _problems;

        public EnemyValueChecker(string prefix, ICollection<string> problems)
        {
            _prefix = prefix;
            _problems = problems;
        }

        // The same problem list with a longer prefix, e.g. for one attack of a kind.
        public EnemyValueChecker For(string section)
        {
            return new EnemyValueChecker(_prefix + " " + section, _problems);
        }

        public int Int(int? value, int standard, int minimum, int maximum,
            string label, int? fallback = null)
        {
            if (!value.HasValue)
            {
                return standard;
            }
            if (value.Value < minimum || value.Value > maximum)
            {
                Report(label, "muss zwischen " + minimum + " und " + maximum + " liegen");
                return fallback ?? standard;
            }
            return value.Value;
        }

        public double Double(double? value, double standard, double minimum,
            double maximum, string label, double? fallback = null)
        {
            if (!value.HasValue)
            {
                return standard;
            }
            if (double.IsNaN(value.Value) || value.Value < minimum || value.Value > maximum)
            {
                Report(label, "muss zwischen " + minimum + " und " + maximum + " liegen");
                return fallback ?? standard;
            }
            return value.Value;
        }

        // Like Double, but the value must be greater than the minimum.
        public double Positive(double? value, double standard, double maximum, string label)
        {
            if (value.HasValue && !(value.Value > 0))
            {
                Report(label, "muss größer als 0 sein");
                return standard;
            }
            return Double(value, standard, 0, maximum, label);
        }

        public void Report(string label, string reason)
        {
            _problems?.Add(_prefix + ": " + label + " " + reason
                + " – der Standardwert wird verwendet.");
        }
    }
}
