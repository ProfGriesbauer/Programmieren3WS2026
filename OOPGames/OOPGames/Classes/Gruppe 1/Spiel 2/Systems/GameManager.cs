using System;
using System.Windows.Input;

namespace OOPGames
{
    /// <summary>
    /// Hält den kompletten Spielzustand und verteilt Eingaben und Ticks
    /// an Welt, Dialog, Menü und das aktive Minispiel.
    /// Wird von G1C_CasinoRules (Eingaben, Tick) und G1C_CasinoPainter (Zeichnen) benutzt.
    /// </summary>
    public class G1C_GameManager
    {
        private const int BartenderGift = 500;
        private const int BartenderLimit = 50;

        private readonly G1C_PlayerMovement _movement;
        private readonly G1C_InteractionManager _interactions;
        private readonly Random _random = new Random();
        private int _gameStartBalance;

        public G1C_GameState State { get; private set; } = G1C_GameState.MainGame;
        public G1C_CasinoWorld World { get; } = new G1C_CasinoWorld();
        public G1C_Player Player { get; }
        public G1C_MoneyManager Money { get; } = new G1C_MoneyManager();
        public G1C_DialogueBox Dialogue { get; } = new G1C_DialogueBox();
        public G1C_ResultScreen Result { get; } = new G1C_ResultScreen();
        public G1C_CasinoMenu Menu { get; } = new G1C_CasinoMenu();
        public G1C_CasinoGameBase ActiveGame { get; private set; }
        public int TickCount { get; private set; }
        public int GamesPlayed { get; private set; }

        public G1C_GameManager()
        {
            Player = new G1C_Player(World.PlayerStartX, World.PlayerStartY, G1C_Direction.Up);
            _movement = new G1C_PlayerMovement(Player, new G1C_CollisionManager(World));
            _interactions = new G1C_InteractionManager(World);
        }

        public string CurrentHint
        {
            get { return State == G1C_GameState.MainGame ? _interactions.GetHint(Player) : ""; }
        }

        /// <summary>Zeigt die Casino-Welt (inkl. Dialog/Menü/Ergebnis darüber) an?</summary>
        public bool IsWorldVisible
        {
            get
            {
                return State == G1C_GameState.MainGame || State == G1C_GameState.Dialogue ||
                       State == G1C_GameState.Menu || State == G1C_GameState.Result;
            }
        }

        // ------------------------------------------------------------------ Eingaben

        public void HandleInput(G1C_InputEvent input)
        {
            if (input == null) return;

            switch (State)
            {
                case G1C_GameState.MainGame:
                    if (input.Kind == G1C_InputKind.Key) HandleWorldKey(input.Key);
                    break;

                case G1C_GameState.Dialogue:
                    // Ein Klick blättert wie [E] weiter
                    Dialogue.HandleKey(input.Kind == G1C_InputKind.Key ? input.Key : Key.E);
                    if (!Dialogue.IsOpen && State == G1C_GameState.Dialogue) State = G1C_GameState.MainGame;
                    break;

                case G1C_GameState.Menu:
                    if (input.Kind == G1C_InputKind.Click || Menu.ShouldClose(input.Key)) State = G1C_GameState.MainGame;
                    break;

                case G1C_GameState.Result:
                    if (input.Kind == G1C_InputKind.Click || G1C_InputManager.IsConfirm(input.Key) || G1C_InputManager.IsBack(input.Key))
                    {
                        State = G1C_GameState.MainGame;
                    }
                    break;

                default:
                    // Ein Minispiel ist aktiv: Bewegung ist deaktiviert, alle Eingaben gehen an das Spiel
                    if (ActiveGame == null) { State = G1C_GameState.MainGame; break; }
                    ActiveGame.HandleInput(input);
                    if (ActiveGame.WantsExit) EndMinigame();
                    break;
            }
        }

        private void HandleWorldKey(Key key)
        {
            G1C_Direction? direction = G1C_InputManager.ToDirection(key);
            if (direction.HasValue)
            {
                _movement.RequestStep(direction.Value);
            }
            else if (G1C_InputManager.IsConfirm(key))
            {
                Interact();
            }
            else if (key == Key.M)
            {
                State = G1C_GameState.Menu;
            }
        }

        private void Interact()
        {
            if (Player.IsMoving) return;

            G1C_WorldObject target = _interactions.FindTarget(Player);
            if (target == null)
            {
                if (_interactions.IsFacingExit(Player))
                {
                    OpenDialogue("Türsteher", new[] { "Du willst schon gehen?", "Die Nacht ist noch jung - versuch doch noch ein Spiel!" }, null);
                }
                return;
            }

            target.OnInteract(Player);
            switch (target.Interaction)
            {
                case G1C_InteractionType.Bartender:
                    TalkToBartender(target);
                    break;
                case G1C_InteractionType.Roulette:
                case G1C_InteractionType.Blackjack:
                case G1C_InteractionType.Poker:
                case G1C_InteractionType.Slots:
                    G1C_InteractionType game = target.Interaction;
                    OpenDialogue(target.Name, target.Lines, () => StartMinigame(game));
                    break;
                default:
                    OpenDialogue(target.Name, target.Lines, null);
                    break;
            }
        }

        private void TalkToBartender(G1C_WorldObject bartender)
        {
            if (Money.Balance < BartenderLimit)
            {
                Money.Deposit(BartenderGift);
                OpenDialogue(bartender.Name, new[]
                {
                    "Oh, deine Taschen sind ja fast leer ...",
                    "Hier, " + BartenderGift + " Chips aufs Haus. Viel Glück!"
                }, null);
            }
            else
            {
                OpenDialogue(bartender.Name, bartender.Lines, null);
            }
        }

        private void OpenDialogue(string speaker, string[] pages, Action onAccept)
        {
            Dialogue.Open(speaker, pages, onAccept);
            State = G1C_GameState.Dialogue;
        }

        // ------------------------------------------------------------------ Minispiele

        private void StartMinigame(G1C_InteractionType type)
        {
            _gameStartBalance = Money.Balance;
            switch (type)
            {
                case G1C_InteractionType.Roulette:
                    ActiveGame = new G1C_Roulette(Money, _random);
                    State = G1C_GameState.Roulette;
                    break;
                case G1C_InteractionType.Blackjack:
                    ActiveGame = new G1C_Blackjack(Money, _random);
                    State = G1C_GameState.Blackjack;
                    break;
                case G1C_InteractionType.Poker:
                    ActiveGame = new G1C_Poker(Money, _random);
                    State = G1C_GameState.Poker;
                    break;
                case G1C_InteractionType.Slots:
                    ActiveGame = new G1C_Slots(Money, _random);
                    State = G1C_GameState.Slots;
                    break;
            }
        }

        private void EndMinigame()
        {
            Result.Show(ActiveGame.Name, _gameStartBalance, Money.Balance);
            ActiveGame = null;
            GamesPlayed++;
            State = G1C_GameState.Result;
        }

        // ------------------------------------------------------------------ Tick (alle 40 ms)

        public void Update()
        {
            TickCount++;
            _movement.Update();
            if (State != G1C_GameState.MainGame) _movement.ClearQueue();
            if (ActiveGame != null) ActiveGame.Update();
        }
    }
}
