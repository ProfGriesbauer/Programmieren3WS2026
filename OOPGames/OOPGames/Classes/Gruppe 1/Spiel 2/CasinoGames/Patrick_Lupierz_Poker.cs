using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;

namespace OOPGames
{
    /// <summary>
    /// Heads-Up Texas Hold'em (Limit-Variante) gegen die Poker-KI.
    ///
    /// Regeln dieser Version:
    ///  - Beide zahlen zu Beginn einen Grundeinsatz (Ante = gewählte Einheit).
    ///  - 4 Setzrunden: Preflop, Flop (3 Karten), Turn (1), River (1). Der Spieler handelt zuerst.
    ///  - Pro Setzrunde höchstens ein Einsatz und eine Erhöhung, jeweils um eine Einheit.
    ///  - Dadurch ist der maximale Einsatz pro Hand 9 Einheiten – so gibt es keine Side-Pots.
    /// </summary>
    public class G1C_Poker : G1C_CasinoGameBase
    {
        private enum Street { Preflop, Flop, Turn, River }
        private enum Phase { Setup, PlayerAction, AiThinking, HandOver }

        private const int MaxBetsPerStreet = 2;
        private const int MaxUnitsPerHand = 9;
        private const int AiDelay = 22;

        private static readonly string[] StreetNames = { "Preflop", "Flop", "Turn", "River" };

        private readonly G1C_PokerAI _ai;
        private readonly List<G1C_Card> _playerHole = new List<G1C_Card>();
        private readonly List<G1C_Card> _aiHole = new List<G1C_Card>();
        private readonly List<G1C_Card> _board = new List<G1C_Card>();
        private G1C_Deck _deck;

        private Phase _phase = Phase.Setup;
        private Street _street;
        private int _unit;
        private int _pot;
        private int _timer;
        private int _playerStreet;     // Einsatz des Spielers in der aktuellen Setzrunde
        private int _aiStreet;         // Einsatz der KI in der aktuellen Setzrunde
        private int _betsThisStreet;
        private bool _playerActed;
        private bool _aiActed;
        private int _playerAggression; // Anzahl Einsätze/Erhöhungen des Spielers in dieser Hand
        private bool _showAiCards;
        private string _lastAction = "";
        private string _handInfo = "";

        public G1C_Poker(G1C_MoneyManager money, Random random) : base(money, random)
        {
            _deck = new G1C_Deck(Random);
            _ai = new G1C_PokerAI("Max", 1500, Random);
            Message = "Einheit mit [W]/[S] wählen (max. 9 Einheiten pro Hand). [E] Austeilen, [Q] Verlassen.";
        }

        public override string Name { get { return "Poker (Texas Hold'em)"; } }

        private int ToCall(bool forPlayer)
        {
            return forPlayer ? _aiStreet - _playerStreet : _playerStreet - _aiStreet;
        }

        private bool CanRaise { get { return _betsThisStreet < MaxBetsPerStreet; } }

        // ------------------------------------------------------------------ Eingabe

        protected override void OnKey(Key key)
        {
            switch (_phase)
            {
                case Phase.Setup:
                case Phase.HandOver:
                    if (Bet.HandleKey(key)) return;
                    if (G1C_InputManager.IsConfirm(key)) StartHand();
                    else if (G1C_InputManager.IsBack(key)) RequestExit();
                    break;

                case Phase.PlayerAction:
                    int number = G1C_InputManager.ToNumber(key);
                    if (number == 1 || key == Key.C) Act(true, G1C_PokerAction.CheckCall);
                    else if (number == 2 || key == Key.R) Act(true, G1C_PokerAction.BetRaise);
                    else if (number == 3 || key == Key.F) Act(true, G1C_PokerAction.Fold);
                    break;
            }
        }

        // ------------------------------------------------------------------ Spielablauf

        private void StartHand()
        {
            _unit = Bet.Amount;
            int required = _unit * MaxUnitsPerHand;
            if (Money.Balance < required)
            {
                Message = "Du brauchst mind. " + required + " Chips für diese Einheit. Senke sie mit [S].";
                return;
            }

            _ai.EnsureBankroll(required);
            Money.TryWithdraw(_unit);
            _ai.TryPay(_unit);
            _pot = 2 * _unit;

            _deck = new G1C_Deck(Random);
            _playerHole.Clear();
            _aiHole.Clear();
            _board.Clear();
            for (int i = 0; i < 2; i++)
            {
                _playerHole.Add(_deck.Draw());
                _aiHole.Add(_deck.Draw());
            }

            _street = Street.Preflop;
            _playerAggression = 0;
            _showAiCards = false;
            _lastAction = "Beide zahlen " + _unit + " Ante.";
            _handInfo = "";
            ResetStreet();
            AskPlayer();
        }

        private void ResetStreet()
        {
            _playerStreet = 0;
            _aiStreet = 0;
            _betsThisStreet = 0;
            _playerActed = false;
            _aiActed = false;
        }

        private void AskPlayer()
        {
            _phase = Phase.PlayerAction;
            int toCall = ToCall(true);
            Message = toCall > 0
                ? "Max hat gesetzt. [1] Call " + toCall + (CanRaise ? "  [2] Raise" : "") + "  [3] Fold"
                : "Du bist dran: [1] Check  [2] Bet " + _unit + "  [3] Fold";
        }

        /// <summary>Führt eine Aktion für Spieler oder KI aus und bestimmt, wer als Nächstes dran ist.</summary>
        private void Act(bool isPlayer, G1C_PokerAction action)
        {
            int toCall = ToCall(isPlayer);
            string who = isPlayer ? "Du" : _ai.Name;

            if (action == G1C_PokerAction.Fold && toCall == 0)
            {
                if (isPlayer)
                {
                    Message = "Kein Einsatz offen - du kannst kostenlos checken ([1]).";
                    return;
                }
                action = G1C_PokerAction.CheckCall;
            }
            if (action == G1C_PokerAction.BetRaise && !CanRaise)
            {
                action = G1C_PokerAction.CheckCall; // Limit erreicht → nur noch Call möglich
            }

            switch (action)
            {
                case G1C_PokerAction.Fold:
                    _lastAction = who + (isPlayer ? " foldest." : " foldet.");
                    EndHand(!isPlayer, _lastAction);
                    return;

                case G1C_PokerAction.CheckCall:
                    Pay(isPlayer, toCall);
                    _lastAction = who + (toCall == 0 ? " checkt." : " callt " + toCall + ".");
                    break;

                default:
                    Pay(isPlayer, toCall + _unit);
                    _betsThisStreet++;
                    if (isPlayer) { _aiActed = false; _playerAggression++; }
                    else { _playerActed = false; }
                    _lastAction = who + (toCall == 0 ? " setzt " + _unit + "." : " erhöht um " + _unit + ".");
                    break;
            }

            if (isPlayer) _playerActed = true; else _aiActed = true;

            if (_playerActed && _aiActed && _playerStreet == _aiStreet)
            {
                NextStreet();
            }
            else if (isPlayer)
            {
                _phase = Phase.AiThinking;
                _timer = AiDelay;
                Message = _lastAction + " Max überlegt ...";
            }
            else
            {
                AskPlayer();
            }
        }

        private void Pay(bool isPlayer, int amount)
        {
            if (amount <= 0) return;
            if (isPlayer)
            {
                Money.TryWithdraw(amount); // durch die 9-Einheiten-Regel immer gedeckt
                _playerStreet += amount;
            }
            else
            {
                _ai.TryPay(amount);
                _aiStreet += amount;
            }
            _pot += amount;
        }

        private void NextStreet()
        {
            switch (_street)
            {
                case Street.Preflop:
                    for (int i = 0; i < 3; i++) _board.Add(_deck.Draw());
                    break;
                case Street.Flop:
                case Street.Turn:
                    _board.Add(_deck.Draw());
                    break;
                default:
                    Showdown();
                    return;
            }
            _street++;
            ResetStreet();
            AskPlayer();
            Message = _lastAction + " " + StreetNames[(int)_street] + "! " + Message;
        }

        private void Showdown()
        {
            G1C_PokerHandValue playerValue = G1C_PokerHandEvaluator.EvaluateBest(_playerHole.Concat(_board).ToList());
            G1C_PokerHandValue aiValue = G1C_PokerHandEvaluator.EvaluateBest(_aiHole.Concat(_board).ToList());
            int cmp = playerValue.CompareTo(aiValue);
            _showAiCards = true;
            _handInfo = "Du: " + playerValue.Name + "  |  Max: " + aiValue.Name;

            if (cmp == 0)
            {
                int half = _pot / 2;
                Money.Deposit(_pot - half);
                _ai.Receive(half);
                _pot = 0;
                _phase = Phase.HandOver;
                Message = "Split Pot - gleich starke Hände! [E] Neue Hand, [Q] Verlassen";
                return;
            }
            EndHand(cmp > 0, cmp > 0 ? "Showdown: Du gewinnst!" : "Showdown: Max gewinnt.");
        }

        private void EndHand(bool playerWins, string reason)
        {
            int won = _pot;
            if (playerWins) Money.Deposit(_pot);
            else _ai.Receive(_pot);
            _pot = 0;
            _phase = Phase.HandOver;
            Message = reason + (playerWins ? " +" + won + " Chips." : "") + " [E] Neue Hand, [Q] Verlassen";
        }

        public override void Update()
        {
            if (_phase != Phase.AiThinking) return;
            if (--_timer > 0) return;

            G1C_PokerAction action = _ai.Decide(_aiHole, _board, ToCall(false), _pot, CanRaise, _playerAggression);
            Act(false, action);
        }

        // ------------------------------------------------------------------ Darstellung

        protected override Color BackgroundColor { get { return Color.FromRgb(0x1E, 0x5A, 0x50); } }

        protected override void DrawGame(G1C_Renderer r)
        {
            bool inHand = _phase != Phase.Setup;

            // KI
            r.Text("KI " + _ai.Name, 20, 36, 13, G1C_UiColors.Gold);
            r.Text("Kasse: " + _ai.Bankroll, 20, 54, 10, Colors.White);
            if (inHand) G1C_CardView.DrawHand(r, _aiHole, 200, 34, 44, _showAiCards ? int.MaxValue : 0);

            // Gemeinschaftskarten
            for (int i = 0; i < 5; i++)
            {
                double x = 128 + i * 46;
                if (inHand && i < _board.Count) G1C_CardView.Draw(r, _board[i], x, 100, false);
                else r.DrawRect(x, 100, G1C_CardView.CardWidth, G1C_CardView.CardHeight, Colors.Transparent, Color.FromRgb(0x60, 0xA0, 0x90), 1);
            }
            r.Text("Pot: " + _pot, 20, 112, 14, G1C_UiColors.Gold);
            if (inHand) r.Text(StreetNames[(int)_street], 380, 112, 13, Colors.White);

            // Spieler
            r.Text("Du", 20, 176, 13, G1C_UiColors.Gold);
            r.Text("Einheit: " + (inHand ? _unit : Bet.Amount), 20, 194, 10, Colors.White);
            if (inHand)
            {
                G1C_CardView.DrawHand(r, _playerHole, 200, 166, 44);
                if (_board.Count >= 3)
                {
                    string current = G1C_PokerHandEvaluator.EvaluateBest(_playerHole.Concat(_board).ToList()).Name;
                    r.Text("Deine Hand: " + current, 300, 186, 11, Colors.White);
                }
            }

            r.Text(_lastAction + (_phase == Phase.HandOver && _handInfo.Length > 0 ? "   " + _handInfo : ""), 20, 228, 10, G1C_UiColors.PanelFill);
            if (_phase == Phase.HandOver)
            {
                // Die Überlegung der KI wird erst nach der Hand gezeigt, sonst würde sie ihre Karten verraten
                r.Text("KI: " + _ai.LastDecision, 20, 244, 9, G1C_UiColors.Gold);
            }

            switch (_phase)
            {
                case Phase.Setup:
                case Phase.HandOver:
                    AddButtonRow(Btn("[W] Einheit +", Key.W), Btn("[S] Einheit -", Key.S),
                                 Btn("[E] Austeilen", Key.E), Btn("[Q] Verlassen", Key.Q));
                    break;
                case Phase.PlayerAction:
                    int toCall = ToCall(true);
                    AddButtonRow(Btn(toCall > 0 ? "[1] Call " + toCall : "[1] Check", Key.D1),
                                 Btn(toCall > 0 ? "[2] Raise" : "[2] Bet", Key.D2, CanRaise),
                                 Btn("[3] Fold", Key.D3, toCall > 0));
                    break;
                default:
                    AddButtonRow(Btn("Max überlegt ...", Key.None, false));
                    break;
            }
        }
    }
}
