using System;
using System.Collections.Generic;

namespace OOPGames
{
    public enum G1C_Suit { Clubs, Diamonds, Hearts, Spades }

    /// <summary>Eine Spielkarte. Rang 2-10, 11 = Bube, 12 = Dame, 13 = König, 14 = Ass.</summary>
    public class G1C_Card : IEquatable<G1C_Card>
    {
        public G1C_Card(int rank, G1C_Suit suit)
        {
            if (rank < 2 || rank > 14) throw new ArgumentOutOfRangeException(nameof(rank));
            Rank = rank;
            Suit = suit;
        }

        public int Rank { get; }
        public G1C_Suit Suit { get; }

        public bool IsRed { get { return Suit == G1C_Suit.Hearts || Suit == G1C_Suit.Diamonds; } }

        public string RankText
        {
            get
            {
                switch (Rank)
                {
                    case 11: return "J";
                    case 12: return "D";
                    case 13: return "K";
                    case 14: return "A";
                    default: return Rank.ToString();
                }
            }
        }

        public string SuitSymbol
        {
            get
            {
                switch (Suit)
                {
                    case G1C_Suit.Clubs: return "♣";
                    case G1C_Suit.Diamonds: return "♦";
                    case G1C_Suit.Hearts: return "♥";
                    default: return "♠";
                }
            }
        }

        public bool Equals(G1C_Card other)
        {
            return other != null && other.Rank == Rank && other.Suit == Suit;
        }

        public override bool Equals(object obj) { return Equals(obj as G1C_Card); }

        public override int GetHashCode() { return Rank * 4 + (int)Suit; }

        public override string ToString() { return RankText + SuitSymbol; }
    }

    /// <summary>Ein gemischtes Kartendeck mit 52 Karten (für Poker und Blackjack).</summary>
    public class G1C_Deck
    {
        private readonly List<G1C_Card> _cards = new List<G1C_Card>();
        private readonly Random _random;

        public G1C_Deck(Random random)
        {
            _random = random ?? new Random();
            Reset();
        }

        public int Remaining { get { return _cards.Count; } }

        public static List<G1C_Card> CreateFullDeck()
        {
            List<G1C_Card> cards = new List<G1C_Card>(52);
            foreach (G1C_Suit suit in Enum.GetValues(typeof(G1C_Suit)))
            {
                for (int rank = 2; rank <= 14; rank++)
                {
                    cards.Add(new G1C_Card(rank, suit));
                }
            }
            return cards;
        }

        public void Reset()
        {
            _cards.Clear();
            _cards.AddRange(CreateFullDeck());

            // Fisher-Yates-Mischen: jede Reihenfolge ist gleich wahrscheinlich
            for (int i = _cards.Count - 1; i > 0; i--)
            {
                int j = _random.Next(i + 1);
                G1C_Card tmp = _cards[i];
                _cards[i] = _cards[j];
                _cards[j] = tmp;
            }
        }

        public G1C_Card Draw()
        {
            if (_cards.Count == 0) Reset();
            G1C_Card card = _cards[_cards.Count - 1];
            _cards.RemoveAt(_cards.Count - 1);
            return card;
        }
    }
}
