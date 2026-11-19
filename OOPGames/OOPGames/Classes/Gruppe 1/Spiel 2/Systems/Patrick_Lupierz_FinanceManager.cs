namespace OOPGames
{
    // Änderung durch: Patrick Lupierz – Datum: 15.11.2026 – Änderung: Bankzinsen (2 % pro Spieltag) mit
    // Schutz gegen doppelte Zinszahlung am selben Tag; Kontoauszug der letzten Buchungen.
    public sealed class G1C_FinanceManager
    {
        public const int LoanAmount = 250;
        public const int LoanRepayment = 300;
        public const int InterestPercentPerDay = 2;
            public const int LoanMaxAmount = 5000;
            public const int LoanInterestPercentPerDay = 50;
        private const int MaxStatementLines = 6;

        private readonly System.Collections.Generic.List<string> _statement = new System.Collections.Generic.List<string>();

        public int Savings { get; private set; }
        public int LoanOwed { get; private set; }
        public int LoanInterestLastDay { get; private set; } = 1;
        public int LoanRespectLastDay { get; private set; } = 1;
        public int LoanSmsLastDay { get; private set; } = 0;
        public int LoanSmsCount { get; private set; }

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
            return Borrow(money, LoanAmount, 1);
        }

        // Änderung durch: Patrick Lupierz – Datum: 19.11.2026 – Freie Kreditwahl und bestehendes Schuldsystem
        public string Borrow(G1C_MoneyManager money, int amount, int currentDay)
        {
            if (LoanOwed > 0) return "Du hast noch " + LoanOwed + " Chips Schulden.";
            if (money == null) return "Der Kredit kann gerade nicht ausgezahlt werden.";
            amount = System.Math.Max(0, System.Math.Min(LoanMaxAmount, amount));
            if (amount <= 0) return "Waehle einen gueltigen Kreditbetrag.";
            money.Deposit(amount);
            LoanOwed = amount;
            LoanInterestLastDay = currentDay;
            LoanRespectLastDay = currentDay;
            LoanSmsLastDay = 0;
            LoanSmsCount = 0;
            Log("Geldhai-Kredit +" + amount);
            return "Du erhältst " + amount + " Chips. Offene Schuld: " + LoanOwed + " Chips.";
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

        // Änderung durch: Patrick Lupierz – Datum: 19.11.2026 – 50 Prozent Zinsen einmal pro Spieltag
        public int ApplyLoanInterest(int currentDay)
        {
            if (LoanOwed <= 0) { LoanInterestLastDay = currentDay; return 0; }
            if (currentDay <= LoanInterestLastDay) return 0;
            int before = LoanOwed;
            for (int day = LoanInterestLastDay + 1; day <= currentDay; day++)
            {
                // 50 % Zinsen auf ganze Chips; 1000 -> 1500, 1500 -> 2250.
                LoanOwed = LoanOwed * 3 / 2;
            }
            LoanInterestLastDay = currentDay;
            Log("Geldhai-Zinsen Tag " + currentDay);
            return LoanOwed - before;
        }

        public bool ApplyLoanRespectDay(int currentDay)
        {
            if (LoanOwed <= 0 || currentDay <= LoanRespectLastDay) return false;
            LoanRespectLastDay = currentDay;
            return true;
        }

        public bool ApplyLoanSmsDay(int currentDay)
        {
            if (LoanOwed <= 0 || currentDay <= LoanSmsLastDay) return false;
            LoanSmsLastDay = currentDay;
            LoanSmsCount++;
            return true;
        }

        public void RestoreLoanState(int interestDay, int respectDay, int smsDay, int smsCount, int currentDay)
        {
            LoanInterestLastDay = interestDay > 0 ? System.Math.Min(interestDay, currentDay) : currentDay;
            LoanRespectLastDay = respectDay > 0 ? System.Math.Min(respectDay, currentDay) : currentDay;
            LoanSmsLastDay = smsDay > 0 ? System.Math.Min(smsDay, currentDay) : 0;
            LoanSmsCount = System.Math.Max(0, smsCount);
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
