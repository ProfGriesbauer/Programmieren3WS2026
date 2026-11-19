using System;
using System.Collections.Generic;
using System.Windows.Input;
using System.Windows.Media;

namespace OOPGames
{
    /// <summary>Eine Blackjack-Hand mit Wertberechnung (Ass zählt 11 oder 1).</summary>
    public class G1C_BlackjackHand
    {
        private readonly List<G1C_Card> _cards = new List<G1C_Card>();

        public IReadOnlyList<G1C_Card> Cards { get { return _cards; } }

        public void Add(G1C_Card card) { _cards.Add(card); }

        public void Clear() { _cards.Clear(); }

        public int Value { get { int aces; return Calculate(out aces); } }

        /// <summary>"Weich" = ein Ass zählt noch als 11 (die Hand kann nicht durch eine Karte überkaufen).</summary>
        public bool IsSoft { get { int aces; Calculate(out aces); return aces > 0; } }

        public bool IsBlackjack { get { return _cards.Count == 2 && Value == 21; } }

        public bool IsBust { get { return Value > 21; } }

        // Asse zunächst als 11 zählen und bei Bedarf einzeln auf 1 herabsetzen
        private int Calculate(out int acesAsEleven)
        {
            int sum = 0;
            acesAsEleven = 0;
            foreach (G1C_Card card in _cards)
            {
                if (card.Rank == 14) { sum += 11; acesAsEleven++; }
                else sum += Math.Min(10, card.Rank);
            }
            while (sum > 21 && acesAsEleven > 0)
            {
                sum -= 10;
                acesAsEleven--;
            }
            return sum;
        }
    }

    /// <summary>
    /// Blackjack: Spieler und KI-Mitspieler spielen jeweils gegen den Dealer.
    /// Ablauf: Einsatz → Austeilen → Spieler (Karte/Halten) → KI → Dealer → Abrechnung.
    /// </summary>
    public class G1C_Blackjack : G1C_CasinoGameBase
    {
        private enum Phase { Betting, PlayerTurn, AiTurn, DealerTurn, RoundOver }

        private const int ActionDelay = 18; // Pause zwischen KI-/Dealer-Zügen (ca. 0,7 s)

        private readonly G1C_BlackjackAI _ai;
        private readonly List<G1C_BlackjackHand> _playerHands = new List<G1C_BlackjackHand>();
        private readonly List<int> _playerBets = new List<int>();
        private readonly List<bool> _playerNaturals = new List<bool>();
        private readonly List<string> _playerResults = new List<string>();
        private readonly G1C_BlackjackHand _aiHand = new G1C_BlackjackHand();
        private readonly G1C_BlackjackHand _dealerHand = new G1C_BlackjackHand();
        private G1C_Deck _deck;

        private Phase _phase = Phase.Betting;
        private int _timer;
        private int _activeHand;
        private int _aiBet;
        private bool _dealerRevealed;
        private string _aiResult = "";

        public G1C_Blackjack(G1C_MoneyManager money, Random random) : base(money, random)
        {
            _deck = new G1C_Deck(Random);
            _ai = new G1C_BlackjackAI("Klara", 1000, Random);
            Message = "Einsatz mit [W]/[S] wählen, [E] Austeilen, [Q] Verlassen.";
        }

        public override string Name { get { return "Blackjack"; } }

        private G1C_Card DealerUpCard { get { return _dealerHand.Cards.Count > 0 ? _dealerHand.Cards[0] : null; } }

        /// <summary>Hausregel: Der Dealer zieht bis 16 und hält ab 17.</summary>
        public static bool DealerShouldHit(G1C_BlackjackHand dealer)
        {
            return dealer.Value < 17;
        }

        // ------------------------------------------------------------------ Eingabe

        protected override void OnKey(Key key)
        {
            switch (_phase)
            {
                case Phase.Betting:
                    if (Bet.HandleKey(key)) return;
                    if (G1C_InputManager.IsConfirm(key)) Deal();
                    else if (G1C_InputManager.IsBack(key)) RequestExit();
                    break;

                case Phase.PlayerTurn:
                    if (key == Key.H || key == Key.D1 || key == Key.NumPad1) PlayerHit();
                    else if (key == Key.S) PlayerStand();
                    else if (key == Key.D3 || key == Key.NumPad3) PlayerSplit();
                    else if (key == Key.D4 || key == Key.NumPad4) PlayerDouble();
                    break;

                case Phase.RoundOver:
                    if (G1C_InputManager.IsConfirm(key))
                    {
                        _phase = Phase.Betting;
                        Message = "Neue Runde: Einsatz wählen, [E] Austeilen.";
                    }
                    else if (G1C_InputManager.IsBack(key)) RequestExit();
                    break;
            }
        }

        private void Deal()
        {
            if (!Money.TryWithdraw(Bet.Amount))
            {
                Message = "Nicht genug Chips für " + Bet.Amount + "! Senke den Einsatz mit [S].";
                return;
            }

            Sfx("shuffle"); // Patrick Lupierz – 19.11.2026: Karten austeilen
            // Bei wenigen Restkarten neu mischen, damit kein Stapel mitten in der Runde leer wird
            if (_deck.Remaining < 20) _deck = new G1C_Deck(Random);

            _aiBet = _ai.DecideBet();
            _playerHands.Clear();
            _playerHands.Add(new G1C_BlackjackHand());
            _playerBets.Clear();
            _playerBets.Add(Bet.Amount);
            _playerNaturals.Clear();
            _playerNaturals.Add(false);
            _playerResults.Clear();
            _playerResults.Add("");
            _activeHand = 0;
            _aiHand.Clear();
            _dealerHand.Clear();
            _dealerRevealed = false;
            _aiResult = "";

            for (int i = 0; i < 2; i++)
            {
                CurrentPlayerHand.Add(_deck.Draw());
                _aiHand.Add(_deck.Draw());
                _dealerHand.Add(_deck.Draw());
            }
            _playerNaturals[0] = CurrentPlayerHand.IsBlackjack;

            if (_dealerHand.IsBlackjack)
            {
                Message = "Der Dealer hat Blackjack!";
                Resolve();
            }
            else if (CurrentPlayerHand.IsBlackjack)
            {
                Message = "Blackjack! Jetzt spielt die KI.";
                StartAiTurn();
            }
            else
            {
                _phase = Phase.PlayerTurn;
                Message = "Du hast " + CurrentPlayerHand.Value + ". [H] Karte ziehen oder [S] Halten?";
            }
        }

        private G1C_BlackjackHand CurrentPlayerHand { get { return _playerHands[_activeHand]; } }

        private void PlayerHit()
        {
            CurrentPlayerHand.Add(_deck.Draw());
            _playerNaturals[_activeHand] = false;
            if (CurrentPlayerHand.IsBust)
            {
                Message = "Hand " + (_activeHand + 1) + " überkauft mit " + CurrentPlayerHand.Value + ".";
                FinishPlayerHand();
            }
            else if (CurrentPlayerHand.Value == 21)
            {
                Message = "21 auf Hand " + (_activeHand + 1) + ".";
                FinishPlayerHand();
            }
            else
            {
                Message = "Hand " + (_activeHand + 1) + ": " + CurrentPlayerHand.Value + ". [H] Karte ziehen oder [S] Halten?";
            }
        }

        private void PlayerStand()
        {
            Message = "Du hältst Hand " + (_activeHand + 1) + " bei " + CurrentPlayerHand.Value + ".";
            FinishPlayerHand();
        }

        private void PlayerSplit()
        {
            G1C_BlackjackHand hand = CurrentPlayerHand;
            if (_playerHands.Count >= 4 || hand.Cards.Count != 2 ||
                hand.Cards[0].Rank != hand.Cards[1].Rank)
            {
                Message = "Split geht nur bei zwei gleichrangigen Karten (maximal vier Hände).";
                return;
            }
            if (!Money.TryWithdraw(_playerBets[_activeHand]))
            {
                Message = "Nicht genug Chips für den zweiten Einsatz.";
                return;
            }

            G1C_Card first = hand.Cards[0];
            G1C_Card second = hand.Cards[1];
            int bet = _playerBets[_activeHand];
            hand.Clear();
            hand.Add(first);
            hand.Add(_deck.Draw());
            G1C_BlackjackHand splitHand = new G1C_BlackjackHand();
            splitHand.Add(second);
            splitHand.Add(_deck.Draw());
            _playerHands.Insert(_activeHand + 1, splitHand);
            _playerBets.Insert(_activeHand + 1, bet);
            _playerNaturals[_activeHand] = false;
            _playerNaturals.Insert(_activeHand + 1, false);
            _playerResults.Insert(_activeHand + 1, "");
            if (CurrentPlayerHand.Value == 21)
            {
                Message = "21 auf Hand " + (_activeHand + 1) + ".";
                FinishPlayerHand();
                return;
            }
            Message = "Hand geteilt. Spiele Hand " + (_activeHand + 1) + ".";
        }

        private void PlayerDouble()
        {
            if (CurrentPlayerHand.Cards.Count != 2)
            {
                Message = "Verdoppeln geht nur mit den ersten zwei Karten der Hand.";
                return;
            }
            int extraBet = _playerBets[_activeHand];
            if (!Money.TryWithdraw(extraBet))
            {
                Message = "Nicht genug Chips zum Verdoppeln.";
                return;
            }

            _playerBets[_activeHand] += extraBet;
            _playerNaturals[_activeHand] = false;
            CurrentPlayerHand.Add(_deck.Draw());
            Message = "Einsatz verdoppelt. Hand " + (_activeHand + 1) + " ist beendet.";
            FinishPlayerHand();
        }

        private void FinishPlayerHand()
        {
            _activeHand++;
            while (_activeHand < _playerHands.Count &&
                   (_playerHands[_activeHand].IsBust || _playerHands[_activeHand].Value == 21))
            {
                Message += " Hand " + (_activeHand + 1) + " ist automatisch beendet.";
                _activeHand++;
            }
            if (_activeHand < _playerHands.Count)
            {
                Message += " Weiter mit Hand " + (_activeHand + 1) + ".";
                return;
            }
            Message += " Jetzt spielt die KI.";
            StartAiTurn();
        }

        private void StartAiTurn()
        {
            _phase = Phase.AiTurn;
            _timer = ActionDelay;
        }

        // ------------------------------------------------------------------ Ablauf (Tick)

        public override void Update()
        {
            if (_phase != Phase.AiTurn && _phase != Phase.DealerTurn) return;
            if (--_timer > 0) return;
            _timer = ActionDelay;

            if (_phase == Phase.AiTurn)
            {
                if (!_aiHand.IsBust && !_aiHand.IsBlackjack && _ai.ShouldHit(_aiHand, DealerUpCard))
                {
                    _aiHand.Add(_deck.Draw());
                    Message = _ai.Name + " zieht eine Karte.";
                }
                else
                {
                    Message = _ai.Name + (_aiHand.IsBust ? " hat sich überkauft." : " hält bei " + _aiHand.Value + ".") + " Der Dealer ist dran.";
                    _phase = Phase.DealerTurn;
                    _dealerRevealed = true;
                }
                return;
            }

            // Dealer: muss nicht ziehen, wenn alle Mitspieler überkauft sind
            bool everyoneBust = _playerHands.TrueForAll(hand => hand.IsBust) && _aiHand.IsBust;
            if (!everyoneBust && DealerShouldHit(_dealerHand))
            {
                _dealerHand.Add(_deck.Draw());
                Message = "Der Dealer zieht ...";
            }
            else
            {
                Resolve();
            }
        }

        private void Resolve()
        {
            _dealerRevealed = true;

            int playerNet = 0;
            for (int i = 0; i < _playerHands.Count; i++)
            {
                int handReturn = Payout(_playerHands[i], _playerBets[i], _playerNaturals[i]);
                if (handReturn > 0) Money.Deposit(handReturn);
                playerNet += handReturn - _playerBets[i];
                _playerResults[i] = DescribeResult(handReturn, _playerBets[i]);
            }

            int aiReturn = Payout(_aiHand, _aiBet, _aiHand.IsBlackjack);
            _ai.Receive(aiReturn);
            _ai.RecordResult(aiReturn - _aiBet);
            _aiResult = DescribeResult(aiReturn, _aiBet);

            string dealerText = _dealerHand.IsBust ? "Dealer überkauft" : "Dealer: " + _dealerHand.Value;
            Message = dealerText + ". Deine Hände: " + string.Join(", ", _playerResults) +
                      " (" + (playerNet >= 0 ? "+" : "") + playerNet + " Chips). [E] Weiter, [Q] Verlassen";
            _phase = Phase.RoundOver;
        }

        /// <summary>Gesamtauszahlung inkl. Einsatz: Blackjack 3:2, Sieg 1:1, Gleichstand = Einsatz zurück.</summary>
        private int Payout(G1C_BlackjackHand hand, int bet, bool natural)
        {
            if (hand.IsBust) return 0;
            if (_dealerHand.IsBlackjack) return natural && hand.IsBlackjack ? bet : 0;
            if (natural && hand.IsBlackjack) return bet + bet * 3 / 2;
            if (_dealerHand.IsBust || hand.Value > _dealerHand.Value) return bet * 2;
            if (hand.Value == _dealerHand.Value) return bet;
            return 0;
        }

        private static string DescribeResult(int payout, int bet)
        {
            if (payout == 0) return "Verloren";
            if (payout == bet) return "Unentschieden";
            if (payout == bet * 2) return "Gewonnen";
            return "Blackjack!";
        }

        // ------------------------------------------------------------------ Darstellung

        protected override void DrawGame(G1C_Renderer r)
        {
            bool inRound = _phase != Phase.Betting;

            // Dealer
            r.Text("Dealer Ben", 20, 34, 12, G1C_UiColors.Gold);
            if (inRound)
            {
                G1C_CardView.DrawHand(r, _dealerHand.Cards, 20, 52, 30, _dealerRevealed ? int.MaxValue : 1);
                string dealerValue = _dealerRevealed ? _dealerHand.Value.ToString() : "?";
                r.Text("Wert: " + dealerValue, 20 + Math.Max(2, _dealerHand.Cards.Count) * 30 + 20, 72, 12, Colors.White);
            }
            r.Text("Dealer hält ab 17 - Blackjack zahlt 3:2", 250, 36, 9, G1C_UiColors.PanelFill);

            // KI-Mitspieler
            r.Text("KI " + _ai.Name + "  (Kasse " + _ai.Bankroll + ")", 20, 134, 11, G1C_UiColors.Gold);
            if (inRound)
            {
                r.Text("Einsatz " + _aiBet + "  Wert " + _aiHand.Value + "  " + _aiResult, 20, 150, 10, Colors.White);
                G1C_CardView.DrawHand(r, _aiHand.Cards, 20, 166, 28);
            }
            r.Text(_ai.LastDecision, 20, 228, 9, G1C_UiColors.PanelFill);

            // Spieler
            r.Text("Du", 260, 134, 11, G1C_UiColors.Gold);
            if (inRound)
            {
                for (int i = 0; i < _playerHands.Count; i++)
                {
                    G1C_BlackjackHand hand = _playerHands[i];
                    double y = 154 + i * 26;
                    r.Text((i == _activeHand && _phase == Phase.PlayerTurn ? "► " : "") +
                           (i + 1) + ": " + hand.Value + " / " + _playerBets[i] + " " + _playerResults[i],
                           260, y, 8, Colors.White);
                    double cardAreaWidth = G1C_Renderer.ScreenWidth - 260;
                    double cardX = 260 + System.Math.Max(0, (cardAreaWidth - System.Math.Max(1, hand.Cards.Count) * 16) / 2);
                    G1C_CardView.DrawHand(r, hand.Cards, cardX, y - 2, 16);
                }
            }
            else
            {
                r.Text("Einsatz: " + Bet.Amount, 260, 150, 14, Colors.White);
            }

            switch (_phase)
            {
                case Phase.Betting:
                    AddButtonRow(Btn("[W] Einsatz +", Key.W), Btn("[S] Einsatz -", Key.S),
                                 Btn("[E] Austeilen", Key.E), Btn("[Q] Verlassen", Key.Q));
                    break;
                case Phase.PlayerTurn:
                    AddButtonRow(Btn("[H] Karte", Key.H), Btn("[S] Halten", Key.S),
                                 Btn("[3] Split", Key.D3, CanSplit), Btn("[4] Double", Key.D4, CanDouble));
                    break;
                case Phase.RoundOver:
                    AddButtonRow(Btn("[E] Neue Runde", Key.E), Btn("[Q] Verlassen", Key.Q));
                    break;
                default:
                    AddButtonRow(Btn("Bitte warten ...", Key.None, false));
                    break;
            }
        }

        private bool CanSplit
        {
            get
            {
                return _playerHands.Count < 4 && CurrentPlayerHand.Cards.Count == 2 &&
                       CurrentPlayerHand.Cards[0].Rank == CurrentPlayerHand.Cards[1].Rank;
            }
        }

        private bool CanDouble { get { return CurrentPlayerHand.Cards.Count == 2 && CurrentPlayerHand.Value != 21; } }
    }
}
