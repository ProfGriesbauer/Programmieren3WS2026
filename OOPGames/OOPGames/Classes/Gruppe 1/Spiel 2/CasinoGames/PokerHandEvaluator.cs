using System;
using System.Collections.Generic;
using System.Linq;

namespace OOPGames
{
    public enum G1C_PokerCategory
    {
        HighCard, OnePair, TwoPair, ThreeOfAKind, Straight, Flush, FullHouse, FourOfAKind, StraightFlush
    }

    /// <summary>
    /// Bewerteter Wert einer Pokerhand. Vergleich: zuerst Kategorie,
    /// dann die Ränge in Reihenfolge ihrer Wichtigkeit (z.B. Paar-Rang, dann Beikarten).
    /// </summary>
    public class G1C_PokerHandValue : IComparable<G1C_PokerHandValue>
    {
        private static readonly string[] CategoryNames =
        {
            "Höchste Karte", "Ein Paar", "Zwei Paare", "Drilling", "Straße",
            "Flush", "Full House", "Vierling", "Straight Flush"
        };

        public G1C_PokerHandValue(G1C_PokerCategory category, params int[] ranks)
        {
            Category = category;
            Ranks = ranks;
        }

        public G1C_PokerCategory Category { get; }
        public int[] Ranks { get; }
        public string Name { get { return CategoryNames[(int)Category]; } }

        public int CompareTo(G1C_PokerHandValue other)
        {
            if (other == null) return 1;
            int c = Category.CompareTo(other.Category);
            if (c != 0) return c;
            for (int i = 0; i < Math.Min(Ranks.Length, other.Ranks.Length); i++)
            {
                c = Ranks[i].CompareTo(other.Ranks[i]);
                if (c != 0) return c;
            }
            return 0;
        }
    }

    /// <summary>Bewertet Pokerhände (beste 5 aus 5 bis 7 Karten).</summary>
    public static class G1C_PokerHandEvaluator
    {
        /// <summary>Probiert alle 5er-Kombinationen (bei 7 Karten: 21) und liefert die beste.</summary>
        public static G1C_PokerHandValue EvaluateBest(IList<G1C_Card> cards)
        {
            if (cards == null || cards.Count < 5) throw new ArgumentException("Mindestens 5 Karten nötig.", nameof(cards));

            G1C_PokerHandValue best = null;
            G1C_Card[] five = new G1C_Card[5];
            int n = cards.Count;
            for (int a = 0; a < n - 4; a++)
            for (int b = a + 1; b < n - 3; b++)
            for (int c = b + 1; c < n - 2; c++)
            for (int d = c + 1; d < n - 1; d++)
            for (int e = d + 1; e < n; e++)
            {
                five[0] = cards[a]; five[1] = cards[b]; five[2] = cards[c]; five[3] = cards[d]; five[4] = cards[e];
                G1C_PokerHandValue value = Evaluate5(five);
                if (best == null || value.CompareTo(best) > 0) best = value;
            }
            return best;
        }

        public static G1C_PokerHandValue Evaluate5(IList<G1C_Card> five)
        {
            // Bewusst ohne LINQ: die Poker-KI ruft diese Methode tausendfach pro Entscheidung auf
            int[] counts = new int[15];
            bool flush = true;
            for (int i = 0; i < 5; i++)
            {
                counts[five[i].Rank]++;
                if (five[i].Suit != five[0].Suit) flush = false;
            }

            // Ränge absteigend sortiert
            int[] ranks = new int[5];
            int k = 0;
            for (int r = 14; r >= 2; r--)
            {
                for (int c = 0; c < counts[r]; c++) ranks[k++] = r;
            }

            // Gruppen gleicher Ränge: zuerst nach Anzahl, dann nach Rang sortiert (z.B. Full House 9-9-9-4-4 → [9, 4])
            int groupCount = 0;
            int[] groupRanks = new int[5];
            int[] groupSizes = new int[5];
            for (int size = 4; size >= 1; size--)
            {
                for (int r = 14; r >= 2; r--)
                {
                    if (counts[r] == size)
                    {
                        groupRanks[groupCount] = r;
                        groupSizes[groupCount] = size;
                        groupCount++;
                    }
                }
            }
            int[] orderedGroups = new int[groupCount];
            Array.Copy(groupRanks, orderedGroups, groupCount);

            int straightHigh = groupCount == 5 ? GetStraightHigh(ranks) : 0;

            if (straightHigh > 0 && flush) return new G1C_PokerHandValue(G1C_PokerCategory.StraightFlush, straightHigh);
            if (groupSizes[0] == 4) return new G1C_PokerHandValue(G1C_PokerCategory.FourOfAKind, orderedGroups);
            if (groupSizes[0] == 3 && groupSizes[1] == 2) return new G1C_PokerHandValue(G1C_PokerCategory.FullHouse, orderedGroups);
            if (flush) return new G1C_PokerHandValue(G1C_PokerCategory.Flush, ranks);
            if (straightHigh > 0) return new G1C_PokerHandValue(G1C_PokerCategory.Straight, straightHigh);
            if (groupSizes[0] == 3) return new G1C_PokerHandValue(G1C_PokerCategory.ThreeOfAKind, orderedGroups);
            if (groupSizes[0] == 2 && groupSizes[1] == 2) return new G1C_PokerHandValue(G1C_PokerCategory.TwoPair, orderedGroups);
            if (groupSizes[0] == 2) return new G1C_PokerHandValue(G1C_PokerCategory.OnePair, orderedGroups);
            return new G1C_PokerHandValue(G1C_PokerCategory.HighCard, ranks);
        }

        // Höchste Karte der Straße oder 0 (nur für 5 verschiedene Ränge).
        // Sonderfall: A-2-3-4-5 ("Wheel") zählt mit 5 als höchster Karte.
        private static int GetStraightHigh(int[] sortedDesc)
        {
            if (sortedDesc[0] - sortedDesc[4] == 4) return sortedDesc[0];
            if (sortedDesc[0] == 14 && sortedDesc[1] == 5 && sortedDesc[4] == 2) return 5;
            return 0;
        }
    }
}
