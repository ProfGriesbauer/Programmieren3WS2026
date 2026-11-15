namespace OOPGames
{
    public sealed class G1C_FinanceManager
    {
        public const int LoanAmount = 250;
        public const int LoanRepayment = 300;

        public int Savings { get; private set; }
        public int LoanOwed { get; private set; }

        public bool Deposit(G1C_MoneyManager money, int amount)
        {
            if (money == null || amount <= 0 || !money.TryWithdraw(amount)) return false;
            Savings += amount;
            return true;
        }

        public bool Withdraw(G1C_MoneyManager money, int amount)
        {
            if (money == null || amount <= 0 || Savings < amount) return false;
            Savings -= amount;
            money.Deposit(amount);
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

        public void Restore(int savings, int loanOwed)
        {
            Savings = System.Math.Max(0, savings);
            LoanOwed = System.Math.Max(0, loanOwed);
        }
    }
}
