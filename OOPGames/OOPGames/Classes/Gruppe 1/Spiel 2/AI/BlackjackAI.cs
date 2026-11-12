using System;

namespace OOPGames
{
    /// <summary>
    /// KI-Mitspieler beim Blackjack. Entscheidet nach der "Basic Strategy"
    /// (eigene Hand + sichtbare Dealer-Karte) und passt den Einsatz an Gewinnserien an.
    /// </summary>
    public class G1C_BlackjackAI : G1C_CasinoAI
    {
        private const int BaseBet = 25;
        private int _streak; // > 0: Gewinne in Folge, < 0: Niederlagen in Folge

        public G1C_BlackjackAI(string name, int startBankroll, Random random)
            : base(name, startBankroll, random)
        {
        }

        /// <summary>Einsatz festlegen und von der KI-Kasse abbuchen.</summary>
        public int DecideBet()
        {
            EnsureBankroll(BaseBet);
            int bet = BaseBet;
            string reason = "Grundeinsatz";
            if (_streak >= 2)
            {
                bet = BaseBet * 2;
                reason = "Gewinnserie, doppelter Einsatz";
            }
            else if (_streak <= -3)
            {
                bet = Math.Max(10, BaseBet / 2);
                reason = "Pechsträhne, halber Einsatz";
            }
            bet = Math.Min(bet, Bankroll);
            TryPay(bet);
            LastDecision = "Setzt " + bet + " (" + reason + ")";
            return bet;
        }

        /// <summary>
        /// Basic Strategy (vereinfacht, ohne Teilen/Verdoppeln):
        ///  weich ≤ 17 → ziehen; weich 18 → ziehen gegen 9, 10, Ass; sonst halten
        ///  hart ≤ 11 → ziehen; hart 12 → halten gegen 4-6; hart 13-16 → halten gegen 2-6; ab 17 halten
        /// </summary>
        public bool ShouldHit(G1C_BlackjackHand hand, G1C_Card dealerUpCard)
        {
            int value = hand.Value;
            int dealer = dealerUpCard == null ? 10 : (dealerUpCard.Rank == 14 ? 11 : Math.Min(10, dealerUpCard.Rank));
            bool hit;

            if (hand.IsSoft)
            {
                if (value <= 17) hit = true;
                else if (value == 18) hit = dealer >= 9;
                else hit = false;
            }
            else
            {
                if (value <= 11) hit = true;
                else if (value == 12) hit = dealer < 4 || dealer > 6;
                else if (value <= 16) hit = dealer > 6;
                else hit = false;
            }

            LastDecision = (hand.IsSoft ? "Weiche " : "Harte ") + value + " gegen Dealer-" + dealer
                           + " -> " + (hit ? "Karte" : "Halten");
            return hit;
        }

        public void RecordResult(int net)
        {
            if (net > 0) _streak = _streak > 0 ? _streak + 1 : 1;
            else if (net < 0) _streak = _streak < 0 ? _streak - 1 : -1;
        }
    }
}
