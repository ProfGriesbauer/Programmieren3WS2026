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
        // Änderung durch: Eduard Schoenfeld – Datum: 12.11.2026 – Gratis-Chips des Barkeepers entfernt
        // (Geld aus dem Nichts); stattdessen bezahlter Job über das NPC-System.

        private G1C_PlayerMovement _movement;
        private readonly G1C_InteractionManager _interactions;
        private readonly G1C_SceneTransitionManager _sceneTransitions;
        private readonly G1C_ExteriorNpcRoamingSystem _exteriorNpcRoaming;
        private readonly Random _random = new Random();
        private int _gameStartBalance;
        private G1C_InteractionType _activeGameType; // Änderung durch: Eduard Schoenfeld – für NPC-Reaktionen

        public G1C_GameState State { get; private set; } = G1C_GameState.MainGame;
        public G1C_HomeWorld Home { get; } = new G1C_HomeWorld();
        public G1C_ExteriorWorld Exterior { get; } = new G1C_ExteriorWorld();
        public G1C_CasinoWorld World { get; } = new G1C_CasinoWorld();
        public G1C_WorldLocation CurrentLocation { get { return _sceneTransitions.Location; } }
        public IG1C_WorldScene CurrentWorld { get { return _sceneTransitions.CurrentWorld; } }
        public G1C_BuildingInteriorWorld Hospital { get; } = new G1C_BuildingInteriorWorld(G1C_WorldLocation.Hospital);
        public G1C_BuildingInteriorWorld Market { get; } = new G1C_BuildingInteriorWorld(G1C_WorldLocation.Market);
        public G1C_Player Player { get; }
        public G1C_MoneyManager Money { get; } = new G1C_MoneyManager();
        public G1C_DialogueBox Dialogue { get; } = new G1C_DialogueBox();
        public G1C_ResultScreen Result { get; } = new G1C_ResultScreen();
        public G1C_CasinoMenu Menu { get; } = new G1C_CasinoMenu();
        public G1C_CasinoGameBase ActiveGame { get; private set; }
        public int TickCount { get; private set; }
        public int GamesPlayed { get; private set; }

        // Änderung durch: Eduard Schoenfeld – Datum: 12.11.2026 – Charaktere, Gespräche, Aufgaben, Ruf
        public G1C_NpcSystem Npcs { get; }

        // Änderung durch: Denis Mock – Ein neues Spiel startet im Zuhause.
        public G1C_GameManager()
        {
            _sceneTransitions = new G1C_SceneTransitionManager(Home, Exterior, World, Hospital, Market);
            Player = new G1C_Player(Home.PlayerStartX, Home.PlayerStartY, G1C_Direction.Down);
            _movement = CreateMovement();
            _interactions = new G1C_InteractionManager(World);
            Npcs = new G1C_NpcSystem(World, Money, _random); // Änderung durch: Eduard Schoenfeld
            _exteriorNpcRoaming = new G1C_ExteriorNpcRoamingSystem(Exterior, Player, _random); // Erweiterung durch Denis Mock
        }

        // Änderung durch: Denis Mock – Türhinweise richten sich nach der aktuell aktiven Welt.
        public string CurrentHint
        {
            get
            {
                if (State != G1C_GameState.MainGame) return "";
                string transitionHint = _sceneTransitions.GetHint(Player);
                if (!string.IsNullOrEmpty(transitionHint)) return transitionHint;
                if (CurrentLocation == G1C_WorldLocation.Exterior)
                {
                    G1C_Npc npc = FindFacingExteriorNpc();
                    return npc != null ? "[E] Mit " + npc.Name + " sprechen" : "";
                }
                return CurrentLocation == G1C_WorldLocation.Casino ? _interactions.GetHint(Player) : "";
            }
        }

        /// <summary>Zeigt die aktive Welt-Szene (inkl. Dialog/Menü/Ergebnis darüber) an?</summary>
        // Änderung durch Denis Mock: Der zentrale Painter verwendet diese Anzeige für alle drei Orte.
        public bool IsWorldVisible
        {
            get
            {
                // Änderung durch: Eduard Schoenfeld – Gespräche liegen über der Welt
                return State == G1C_GameState.MainGame || State == G1C_GameState.Dialogue ||
                       State == G1C_GameState.Menu || State == G1C_GameState.Result ||
                       State == G1C_GameState.Conversation;
            }
        }

        // Entwickler: Denis Mock
        private G1C_PlayerMovement CreateMovement()
        {
            return new G1C_PlayerMovement(Player, new G1C_CollisionManager(CurrentWorld));
        }

        // Entwickler: Denis Mock
        private G1C_Npc FindFacingExteriorNpc()
        {
            if (Player.IsMoving || CurrentLocation != G1C_WorldLocation.Exterior) return null;
            return CurrentWorld.ObjectAt(Player.FacingTileX, Player.FacingTileY) as G1C_Npc;
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

                case G1C_GameState.Conversation:
                    HandleConversationInput(input);
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

        // Änderung durch: Denis Mock – Türen wechseln die aktive Szene; Casinointeraktionen bleiben erhalten.
        // Änderung durch Denis Mock: Stadtbewohner benutzen dasselbe Gesprächssystem wie Casino-NPCs.
        private void Interact()
        {
            if (Player.IsMoving) return;

            if (_sceneTransitions.TryTransition(Player))
            {
                _movement.ClearQueue();
                _movement = CreateMovement();
                return;
            }

            if (CurrentLocation == G1C_WorldLocation.Exterior)
            {
                G1C_Npc townNpc = FindFacingExteriorNpc();
                if (townNpc == null) return;

                townNpc.OnInteract(Player);
                if (Npcs.TryStartConversation(townNpc)) State = G1C_GameState.Conversation;
                return;
            }

            if (CurrentLocation != G1C_WorldLocation.Casino) return;

            G1C_WorldObject target = _interactions.FindTarget(Player);
            if (target == null) return;

            target.OnInteract(Player);

            // Änderung durch: Eduard Schoenfeld – Datum: 12.11.2026
            // NPCs mit Charakterprofil (auch über ihren Tisch/Tresen) führen ein Gespräch mit Antwortoptionen.
            if (Npcs.TryStartConversation(target))
            {
                State = G1C_GameState.Conversation;
                return;
            }

            switch (target.Interaction)
            {
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

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Eingaben im Gespräch weiterleiten; nach dem Ende ggf. das gewählte Minispiel starten.
        private void HandleConversationInput(G1C_InputEvent input)
        {
            G1C_Conversation conversation = Npcs.Conversation;
            if (input.Kind == G1C_InputKind.Key)
            {
                conversation.HandleKey(input.Key);
            }
            else
            {
                conversation.Choose(G1C_ConversationView.OptionAt(input.X, input.Y, conversation.Options.Count));
            }

            if (conversation.IsOpen) return;
            G1C_InteractionType? game = conversation.TakeRequestedGame();
            State = G1C_GameState.MainGame;
            if (game.HasValue) StartMinigame(game.Value);
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
            _activeGameType = type; // Änderung durch: Eduard Schoenfeld
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
            // Änderung durch: Eduard Schoenfeld – NPCs reagieren auf das tatsächliche Ergebnis
            Npcs.OnMinigameFinished(_activeGameType, Money.Balance - _gameStartBalance);
            ActiveGame = null;
            GamesPlayed++;
            State = G1C_GameState.Result;
        }

        // ------------------------------------------------------------------ Tick (alle 40 ms)

        // Erweiterung durch Denis Mock: Außenwelt-NPCs laufen ausschließlich im Framework-Tick.
        public void Update()
        {
            TickCount++;
            _movement.Update();
            if (State != G1C_GameState.MainGame) _movement.ClearQueue();
            if (ActiveGame != null) ActiveGame.Update();

            // Änderung durch: Eduard Schoenfeld – Datum: 12.11.2026 – NPC-Stimmungen, Monologe, Aufgaben-Gegenstände
            bool inCasino = CurrentLocation == G1C_WorldLocation.Casino;
            bool exploring = State == G1C_GameState.MainGame;
            Npcs.Update(TickCount, Player, inCasino, exploring);
            if (inCasino && exploring && !Player.IsMoving) Npcs.TryCollectItem(Player.TileX, Player.TileY);
            if (CurrentLocation == G1C_WorldLocation.Exterior)
            {
                if (exploring) _exteriorNpcRoaming.Update(TickCount);
                foreach (G1C_Animal animal in Exterior.Animals) animal.UpdateAnimation(TickCount);
            }
        }
    }
}
