using System;
using System.Collections.Generic;
using System.Linq;

namespace OOPGames
{
    public enum G1C_RouletteStrategy { Vorsichtig, Ausgewogen, Riskant }

    /// <summary>
    /// KI-Mitspieler am Roulette-Tisch.
    /// Der Zufall bestimmt nur die Kugel – die KI wählt ihre Einsätze nach Regeln:
    ///  - Vorsichtig:  Martingale auf Rot/Schwarz (nach Verlust verdoppeln),
    ///                 begrenzt auf 10 % der eigenen Kasse.
    ///  - Ausgewogen:  Dutzend-Wette auf das zuletzt seltenste Dutzend.
    ///  - Riskant:     Einzelzahl auf die häufigste ("heiße") Zahl der letzten Runden.
    /// Die Strategie hängt vom Verhältnis Kasse/Startkasse und der Verlustserie ab.
    /// </summary>
    public class G1C_RouletteAI : G1C_CasinoAI
    {
        private const int BaseBet = 10;
        private int _lossStreak;

        public G1C_RouletteStrategy Strategy { get; private set; } = G1C_RouletteStrategy.Vorsichtig;

        public G1C_RouletteAI(string name, int startBankroll, Random random)
            : base(name, startBankroll, random)
        {
        }

        /// <summary>Wählt Strategie und Einsatz, bucht den Einsatz von der KI-Kasse ab.</summary>
        public G1C_RouletteBet DecideBet(IReadOnlyList<int> history)
        {
            EnsureBankroll(BaseBet);
            ChooseStrategy();

            G1C_RouletteBet bet;
            switch (Strategy)
            {
                case G1C_RouletteStrategy.Riskant: bet = CreateRiskyBet(history); break;
                case G1C_RouletteStrategy.Ausgewogen: bet = CreateBalancedBet(history); break;
                default: bet = CreateCautiousBet(history); break;
            }

            TryPay(bet.Amount);
            LastDecision = bet.Amount + " auf " + bet.Label + " (Chance " + bet.WinProbability.ToString("P0")
                           + ", Quote " + bet.PayoutFactor + ":1)";
            return bet;
        }

        public void ApplyResult(G1C_RouletteBet bet, int result)
        {
            if (bet == null) return;
            int payout = bet.TotalReturn(result);
            Receive(payout);
            if (payout > 0)
            {
                _lossStreak = 0;
                LastDecision += " -> gewonnen";
            }
            else
            {
                _lossStreak++;
                LastDecision += " -> verloren";
            }
        }

        // Risikobewertung: viel Gewinn → mehr Risiko, wenig Geld → Absicherung
        private void ChooseStrategy()
        {
            double ratio = Bankroll / (double)StartBankroll;
            if (ratio > 1.25) Strategy = G1C_RouletteStrategy.Riskant;
            else if (_lossStreak >= 3) Strategy = G1C_RouletteStrategy.Ausgewogen;
            else Strategy = G1C_RouletteStrategy.Vorsichtig;
        }

        private G1C_RouletteBet CreateCautiousBet(IReadOnlyList<int> history)
        {
            // Martingale: Einsatz verdoppelt sich mit jeder Niederlage in Folge
            int amount = BaseBet << Math.Min(_lossStreak, 6);
            int limit = Math.Max(BaseBet, Bankroll / 10);
            if (amount > limit)
            {
                amount = BaseBet; // Risikogrenze erreicht → zurück zum Grundeinsatz
                _lossStreak = 0;
            }

            // Der Farbe folgen, die in den letzten 5 Runden öfter kam
            IEnumerable<int> recent = history.Take(5);
            int red = recent.Count(n => n != 0 && G1C_RouletteTable.IsRed(n));
            int black = recent.Count(n => n != 0 && !G1C_RouletteTable.IsRed(n));
            G1C_RouletteBetType color = red >= black ? G1C_RouletteBetType.Red : G1C_RouletteBetType.Black;
            return new G1C_RouletteBet(color, 0, Math.Min(amount, Bankroll));
        }

        private G1C_RouletteBet CreateBalancedBet(IReadOnlyList<int> history)
        {
            // Das Dutzend, das zuletzt am seltensten kam, gilt für die KI als "fällig"
            int[] counts = new int[3];
            foreach (int n in history.Take(12))
            {
                if (n > 0) counts[(n - 1) / 12]++;
            }
            int dozen = Array.IndexOf(counts, counts.Min());
            G1C_RouletteBetType type = dozen == 0 ? G1C_RouletteBetType.Dozen1
                : (dozen == 1 ? G1C_RouletteBetType.Dozen2 : G1C_RouletteBetType.Dozen3);
            int amount = Math.Min(BaseBet * 2, Math.Max(BaseBet, Bankroll / 8));
            return new G1C_RouletteBet(type, 0, Math.Min(amount, Bankroll));
        }

        private G1C_RouletteBet CreateRiskyBet(IReadOnlyList<int> history)
        {
            // "Heiße" Zahl = häufigste Zahl der letzten Runden; ohne Verlauf eine Lieblingszahl
            int number = history.Count > 0
                ? history.GroupBy(n => n).OrderByDescending(g => g.Count()).ThenBy(g => g.Key).First().Key
                : 7 + Random.Next(3) * 10;
            int amount = Math.Min(BaseBet * 2, Bankroll);
            return new G1C_RouletteBet(G1C_RouletteBetType.Number, number, amount);
        }
    }
}
