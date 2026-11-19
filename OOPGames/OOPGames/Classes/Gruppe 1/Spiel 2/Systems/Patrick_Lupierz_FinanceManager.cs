namespace OOPGames
{
    // Änderung durch: Patrick Lupierz – Datum: 15.11.2026 – Änderung: Bankzinsen (2 % pro Spieltag) mit
    // Schutz gegen doppelte Zinszahlung am selben Tag; Kontoauszug der letzten Buchungen.
    public sealed class G1C_FinanceManager
    {
        public const int LoanAmount = 250;
        public const int LoanRepayment = 300;
        public const int InterestPercentPerDay = 2;
        private const int MaxStatementLines = 6;

        private readonly System.Collections.Generic.List<string> _statement = new System.Collections.Generic.List<string>();

        public int Savings { get; private set; }
        public int LoanOwed { get; private set; }

        /// <summary>Letzter Spieltag, für den bereits Zinsen gutgeschrieben wurden.</summary>
        public int LastInterestDay { get; private set; } = 1;

        /// <summary>Summe aller bisher gutgeschriebenen Zinsen (Anzeige am Bankschalter).</summary>
        public int TotalInterestPaid { get; private set; }

        public System.Collections.Generic.IReadOnlyList<string> Statement { get { return _statement; } }

        public bool Deposit(G1C_MoneyManager money, int amount)
        {
            if (money == null || amount <= 0 || !money.TryWithdraw(amount)) return false;
            Savings += amount;
            Log("Einzahlung +" + amount);
            return true;
        }

        public bool Withdraw(G1C_MoneyManager money, int amount)
        {
            if (money == null || amount <= 0 || Savings < amount) return false;
            Savings -= amount;
            money.Deposit(amount);
            Log("Abhebung -" + amount);
            return true;
        }

        public string Borrow(G1C_MoneyManager money)
        {
            if (LoanOwed > 0) return "Du hast noch " + LoanOwed + " Chips Schulden.";
            if (money == null) return "Der Kredit kann gerade nicht ausgezahlt werden.";
            money.Deposit(LoanAmount);
            LoanOwed = LoanRepayment;
            return "Du erhältst 250 Chips. Zurückzuzahlen sind insgesamt 300 Chips.";
        }

        public string Repay(G1C_MoneyManager money, int amount)
        {
            if (LoanOwed <= 0) return "Du hast keine offenen Schulden.";
            int payment = System.Math.Min(System.Math.Max(0, amount), LoanOwed);
            if (money == null || payment == 0 || !money.TryWithdraw(payment))
                return "Nicht genug Chips für diese Rückzahlung.";
            LoanOwed -= payment;
            return payment + " Chips zurückgezahlt. Offen: " + LoanOwed + " Chips.";
        }

        // Entwickler: Patrick Lupierz – Datum: 15.11.2026
        // Aufgabe: Zinsen für jeden seit der letzten Gutschrift begonnenen Spieltag buchen (2 % vom
        //          tatsächlichen Bankguthaben, Zinseszins, auf ganze Chips abgerundet). Ein Tag wird
        //          höchstens einmal verzinst, auch wenn die Methode mehrfach aufgerufen wird.
        //          Rückgabe: insgesamt gutgeschriebene Chips (0 = nichts gebucht).
        public int ApplyDailyInterest(int currentDay)
        {
            if (currentDay <= LastInterestDay) return 0;
            int paid = 0;
            for (int day = LastInterestDay + 1; day <= currentDay; day++)
            {
                int interest = InterestFor(Savings);
                Savings += interest;
                paid += interest;
            }
            LastInterestDay = currentDay;
            TotalInterestPaid += paid;
            if (paid > 0) Log("Zinsen Tag " + currentDay + " +" + paid);
            return paid;
        }

        // Entwickler: Patrick Lupierz – reine Berechnung, z. B. 1000 -> 20
        public static int InterestFor(int savings)
        {
            return savings <= 0 ? 0 : savings * InterestPercentPerDay / 100;
        }

        public void Restore(int savings, int loanOwed)
        {
            Savings = System.Math.Max(0, savings);
            LoanOwed = System.Math.Max(0, loanOwed);
            _statement.Clear();
        }

        // Entwickler: Patrick Lupierz – Datum: 15.11.2026
        // Aufgabe: Zinsstand aus Spielstand/Neuem Spiel setzen. Alte Spielstände ohne Zinstag (0) erhalten
        //          den aktuellen Tag, damit nicht rückwirkend für alle Tage gezahlt wird.
        public void RestoreInterest(int lastInterestDay, int totalInterestPaid, int currentDay)
        {
            LastInterestDay = lastInterestDay > 0 ? System.Math.Min(lastInterestDay, currentDay) : currentDay;
            TotalInterestPaid = System.Math.Max(0, totalInterestPaid);
        }

        private void Log(string line)
        {
            _statement.Insert(0, line);
            if (_statement.Count > MaxStatementLines) _statement.RemoveAt(_statement.Count - 1);
        }
    }
}
