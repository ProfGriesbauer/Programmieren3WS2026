using System;

namespace OOPGames
{
    /// <summary>
    /// Verwaltet den Kontostand (Chips) des Spielers.
    /// Alle Minispiele buchen ausschließlich über diese Klasse.
    /// </summary>
    public class G1C_MoneyManager
    {
        public const int StartBalance = 1000;

        public int Balance { get; private set; }

        public G1C_MoneyManager() : this(StartBalance)
        {
        }

        public G1C_MoneyManager(int startBalance)
        {
            if (startBalance < 0) throw new ArgumentOutOfRangeException(nameof(startBalance));
            Balance = startBalance;
        }

        public bool CanAfford(int amount)
        {
            return amount > 0 && amount <= Balance;
        }

        /// <summary>Bucht einen Einsatz ab. Gibt false zurück, wenn der Betrag ungültig oder zu hoch ist.</summary>
        public bool TryWithdraw(int amount)
        {
            if (!CanAfford(amount)) return false;
            Balance -= amount;
            return true;
        }

        public void Deposit(int amount)
        {
            if (amount < 0) throw new ArgumentOutOfRangeException(nameof(amount), "Gutschriften dürfen nicht negativ sein.");
            Balance += amount;
        }
    }
}
