using System;
using System.Collections.Generic;
using System.Linq;

namespace OOPGames
{
    public enum G1C_PokerAction { Fold, CheckCall, BetRaise }

    /// <summary>
    /// Poker-KI (Heads-Up Texas Hold'em).
    ///
    /// 1. Eigene Karten bewerten: Die Gewinnchance (Equity) wird per Monte-Carlo-Simulation
    ///    geschätzt – es werden viele zufällige Gegnerhände und Restkarten durchgespielt.
    /// 2. Gegner einschätzen: Jeder Einsatz/Erhöhung des Spielers senkt die geschätzte Chance,
    ///    weil aggressive Spieler im Schnitt stärkere Hände haben.
    /// 3. Entscheiden: Vergleich mit den Pot-Odds (Fold/Call) und Schwellwerten (Raise),
    ///    dazu gelegentliche Bluffs ab dem Flop.
    /// </summary>
    public class G1C_PokerAI : G1C_CasinoAI
    {
        private const int SimulationRounds = 300;
        private const double RaiseThreshold = 0.68;
        private const double BetThreshold = 0.58;
        private const double BluffRate = 0.12;
        private const double AggressionPenalty = 0.06;

        public G1C_PokerAI(string name, int startBankroll, Random random)
            : base(name, startBankroll, random)
        {
        }

        public G1C_PokerAction Decide(IList<G1C_Card> hole, IList<G1C_Card> board, int toCall, int pot,
                                      bool canRaise, int opponentAggression)
        {
            double equity = EstimateEquity(hole, board, SimulationRounds);
            double adjusted = equity - AggressionPenalty * Math.Min(opponentAggression, 3);
            double potOdds = toCall > 0 ? toCall / (double)(pot + toCall) : 0.0;

            G1C_PokerAction action;
            string reason;
            if (toCall > 0)
            {
                if (adjusted < potOdds) { action = G1C_PokerAction.Fold; reason = "zu schwach für die Pot-Odds"; }
                else if (adjusted > RaiseThreshold && canRaise) { action = G1C_PokerAction.BetRaise; reason = "starke Hand"; }
                else { action = G1C_PokerAction.CheckCall; reason = "Call lohnt sich"; }
            }
            else
            {
                if (adjusted > BetThreshold && canRaise) { action = G1C_PokerAction.BetRaise; reason = "Wert-Einsatz"; }
                else if (canRaise && board.Count >= 3 && Random.NextDouble() < BluffRate) { action = G1C_PokerAction.BetRaise; reason = "Bluff!"; }
                else { action = G1C_PokerAction.CheckCall; reason = "Check"; }
            }

            LastDecision = "Chance " + equity.ToString("P0") + " (bereinigt " + adjusted.ToString("P0")
                           + "), Pot-Odds " + potOdds.ToString("P0") + " -> " + reason;
            return action;
        }

        /// <summary>
        /// Monte-Carlo-Schätzung: Anteil gewonnener Showdowns gegen eine zufällige Gegnerhand
        /// (Unentschieden zählt halb).
        /// </summary>
        public double EstimateEquity(IList<G1C_Card> hole, IList<G1C_Card> board, int rounds)
        {
            List<G1C_Card> known = hole.Concat(board).ToList();
            List<G1C_Card> remaining = G1C_Deck.CreateFullDeck().Where(c => !known.Contains(c)).ToList();
            int missingBoard = 5 - board.Count;
            int needed = 2 + missingBoard;

            List<G1C_Card> myCards = new List<G1C_Card>(7);
            List<G1C_Card> opponentCards = new List<G1C_Card>(7);
            double score = 0;

            for (int i = 0; i < rounds; i++)
            {
                // Teilweises Fisher-Yates: die ersten "needed" Karten zufällig auswählen
                for (int k = 0; k < needed; k++)
                {
                    int j = k + Random.Next(remaining.Count - k);
                    G1C_Card tmp = remaining[k];
                    remaining[k] = remaining[j];
                    remaining[j] = tmp;
                }

                myCards.Clear();
                opponentCards.Clear();
                myCards.AddRange(hole);
                myCards.AddRange(board);
                opponentCards.Add(remaining[0]);
                opponentCards.Add(remaining[1]);
                opponentCards.AddRange(board);
                for (int k = 0; k < missingBoard; k++)
                {
                    myCards.Add(remaining[2 + k]);
                    opponentCards.Add(remaining[2 + k]);
                }

                int cmp = G1C_PokerHandEvaluator.EvaluateBest(myCards).CompareTo(G1C_PokerHandEvaluator.EvaluateBest(opponentCards));
                score += cmp > 0 ? 1.0 : (cmp == 0 ? 0.5 : 0.0);
            }
            return score / rounds;
        }
    }
}
