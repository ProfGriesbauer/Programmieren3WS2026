using System;

namespace OOPGames
{
    /// <summary>
    /// Basisklasse aller KI-Gegner: Name, eigene Kasse und eine lesbare
    /// Begründung der letzten Entscheidung (damit die KI nachvollziehbar bleibt).
    /// </summary>
    public abstract class G1C_CasinoAI
    {
        protected G1C_CasinoAI(string name, int startBankroll, Random random)
        {
            if (startBankroll <= 0) throw new ArgumentOutOfRangeException(nameof(startBankroll));
            Name = name;
            StartBankroll = startBankroll;
            Bankroll = startBankroll;
            Random = random ?? new Random();
        }

        public string Name { get; }
        public int StartBankroll { get; }
        public int Bankroll { get; private set; }
        public int Rebuys { get; private set; }
        public string LastDecision { get; protected set; } = "wartet ...";

        protected Random Random { get; }

        public bool TryPay(int amount)
        {
            if (amount <= 0 || amount > Bankroll) return false;
            Bankroll -= amount;
            return true;
        }

        public void Receive(int amount)
        {
            if (amount > 0) Bankroll += amount;
        }

        /// <summary>Ist die Kasse zu leer, kauft die KI neue Chips nach, damit weitergespielt werden kann.</summary>
        public void EnsureBankroll(int minimum)
        {
            if (Bankroll < minimum)
            {
                Bankroll = Math.Max(StartBankroll, minimum);
                Rebuys++;
            }
        }
    }
}
