using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace OOPGames
{
    /// <summary>
    /// Hält den kompletten Spielzustand und verteilt Eingaben und Ticks
    /// an Welt, Dialog, Menü und das aktive Minispiel.
    /// Wird von G1C_CasinoRules (Eingaben, Tick) und G1C_CasinoPainter (Zeichnen) benutzt.
    /// </summary>
    // Änderung durch: Patrick Lupierz – Datum: 12.11.2026 – Hauptmenü, Charakter, Uhr, Inventar, Läden,
    // Handy, Mülleimer und Speichern/Laden angebunden. Die Systeme selbst liegen in eigenen Klassen.
    public class G1C_GameManager
    {
        // Änderung durch: Eduard Schoenfeld – Datum: 12.11.2026 – Gratis-Chips des Barkeepers entfernt
        // (Geld aus dem Nichts); stattdessen bezahlter Job über das NPC-System.

        private G1C_PlayerMovement _movement;
        private readonly G1C_InteractionManager _interactions;
        private readonly G1C_SceneTransitionManager _sceneTransitions;
        private readonly G1C_ExteriorNpcRoamingSystem _exteriorNpcRoaming;
        private readonly G1C_TrashSystem _trash = new G1C_TrashSystem();
        private readonly Random _random = new Random();
        private int _gameStartBalance;
        private G1C_InteractionType _activeGameType;
        private G1C_WorldLocation _shopLocation; // Änderung durch: Patrick Lupierz – 15.11.2026 – Laden des letzten Verkäufers // Änderung durch: Eduard Schoenfeld – für NPC-Reaktionen

        public G1C_GameState State { get; private set; } = G1C_GameState.Title;
        public G1C_HomeWorld Home { get; } = new G1C_HomeWorld();
        public G1C_ExteriorWorld Exterior { get; } = new G1C_ExteriorWorld();
        public G1C_CasinoWorld World { get; } = new G1C_CasinoWorld();
        public G1C_WorldLocation CurrentLocation { get { return _sceneTransitions.Location; } }
        public IG1C_WorldScene CurrentWorld { get { return _sceneTransitions.CurrentWorld; } }
        public G1C_BuildingInteriorWorld Hospital { get; } = new G1C_BuildingInteriorWorld(G1C_WorldLocation.Hospital);
        public G1C_BuildingInteriorWorld Market { get; } = new G1C_BuildingInteriorWorld(G1C_WorldLocation.Market);
        public G1C_BuildingInteriorWorld GasStation { get; } = new G1C_BuildingInteriorWorld(G1C_WorldLocation.GasStation);
        public G1C_BuildingInteriorWorld ClothingStore { get; } = new G1C_BuildingInteriorWorld(G1C_WorldLocation.ClothingStore);
        public G1C_BuildingInteriorWorld Bank { get; } = new G1C_BuildingInteriorWorld(G1C_WorldLocation.Bank);
        public G1C_BuildingInteriorWorld FurnitureStore { get; } = new G1C_BuildingInteriorWorld(G1C_WorldLocation.FurnitureStore);
        // Änderung durch: Patrick Lupierz – Datum: 15.11.2026 – Spielhalle, Fahrradladen, Fahrrad, Highscores
        public G1C_BuildingInteriorWorld Arcade { get; } = new G1C_BuildingInteriorWorld(G1C_WorldLocation.Arcade);
        public G1C_BuildingInteriorWorld BikeShop { get; } = new G1C_BuildingInteriorWorld(G1C_WorldLocation.BikeShop);
        public G1C_BuildingInteriorWorld GrandmaHouse { get; } = new G1C_BuildingInteriorWorld(G1C_WorldLocation.GrandmaHouse);
        public G1C_BikeSystem Bikes { get; } = new G1C_BikeSystem();
        public Dictionary<string, int> ArcadeHighscores { get; private set; } = new Dictionary<string, int>();
        public G1C_Player Player { get; }
        public G1C_MoneyManager Money { get; } = new G1C_MoneyManager();
        public G1C_FinanceManager Finance { get; } = new G1C_FinanceManager();
        public G1C_DialogueBox Dialogue { get; } = new G1C_DialogueBox();
        public G1C_ResultScreen Result { get; } = new G1C_ResultScreen();
        public G1C_CasinoMenu Menu { get; } = new G1C_CasinoMenu();
        public G1C_CasinoGameBase ActiveGame { get; private set; }
        public int TickCount { get; private set; }
        public int GamesPlayed { get; private set; }
        public string SystemMessage { get; private set; } = "";
        public int SystemMessageEndTick { get; private set; }

        // Änderung durch: Eduard Schoenfeld – Datum: 12.11.2026 – Charaktere, Gespräche, Aufgaben, Ruf
        public G1C_NpcSystem Npcs { get; }

        public G1C_GameClock Clock { get; } = new G1C_GameClock();
        public G1C_PlayerProfile Profile { get; private set; } = new G1C_PlayerProfile();
        public G1C_Inventory Inventory { get; } = new G1C_Inventory();
        public G1C_BuffSystem Buffs { get; } = new G1C_BuffSystem();
        public G1C_StartFlow Start { get; } = new G1C_StartFlow();
        public G1C_ListMenu ListMenu { get; } = new G1C_ListMenu();
        public G1C_Phone Phone { get; }
        public G1C_MapView MapView { get; } = new G1C_MapView();

        // Änderung durch: Patrick Lupierz – Datum: 16.11.2026 – Ton, Admin-Cheat, Einrichtungsmodus, Tierlaute
        public G1C_SoundSystem Sound { get; } = new G1C_SoundSystem();
        public G1C_CheatCode Cheats { get; } = new G1C_CheatCode();
        public G1C_FurnitureEditor Furnishing { get; } = new G1C_FurnitureEditor();
        public G1C_AnimalVoices AnimalVoices { get; } = new G1C_AnimalVoices();

        // Änderung durch: Patrick Lupierz – Datum: 15.11.2026 – Schlafzustand: nach dem Schlafen wird die Welt
        // kurz abgedunkelt mit "Zzz" gezeigt; währenddessen ist die Steuerung gesperrt.
        public const int SleepAnimationTicks = 75; // 3 Sekunden
        public int SleepUntilTick { get; private set; }
        public bool IsSleeping { get { return TickCount < SleepUntilTick; } }

        // Änderung durch: Denis Mock – Ein neues Spiel startet im Zuhause.
        public G1C_GameManager()
        {
            _sceneTransitions = new G1C_SceneTransitionManager(Home, Exterior, World, Hospital, Market, GasStation,
                                                               ClothingStore, Bank, FurnitureStore, Arcade, BikeShop,
                                                               GrandmaHouse);
            Player = new G1C_Player(Home.PlayerStartX, Home.PlayerStartY, G1C_Direction.Down);
            _movement = CreateMovement();
            _interactions = new G1C_InteractionManager(World);
            Npcs = new G1C_NpcSystem(World, Money, Finance, _random); // Änderung durch: Eduard Schoenfeld
            Npcs.Player = Player;
            Npcs.OnNurseTreat = TreatBrokenLegs;
            Npcs.OnLoanRepaid = ApplyLoanRepaidConsequences;
            Npcs.Clock = Clock; // Änderung durch: Patrick Lupierz – 15.11.2026
            Npcs.ArcadeHighscores = () => ArcadeHighscores; // Änderung durch: Patrick Lupierz – 22.11.2026 – Patricks Rekord
            Npcs.IsPresent = npc => World.IsPresent(npc); // Änderung durch: Patrick Lupierz – 18.11.2026 – Tagesablauf
            _exteriorNpcRoaming = new G1C_ExteriorNpcRoamingSystem(Exterior, Player, _random); // Erweiterung durch Denis Mock
            Phone = new G1C_Phone(Npcs);
            // Änderung durch: Patrick Lupierz – 15.11.2026 – Ständer zeigt das aktive Rad oder ist leer, wenn man fährt
            if (Exterior.ParkedBike != null)
                Exterior.ParkedBike.SpriteSelector = () => Bikes.Riding || Bikes.ParkedAtDoor ? "obj_bike_stand" : "obj_" + Bikes.ActiveBikeId;
        }

        // Änderung durch: Denis Mock – Türhinweise richten sich nach der aktuell aktiven Welt.
        public string CurrentHint
        {
            get
            {
                if (State != G1C_GameState.MainGame) return "";
                G1C_Shop lockedShop = ClosedShopAtFacingDoor();
                if (lockedShop != null) return "Geschlossen – geöffnet " + lockedShop.OpenFrom + " bis " + lockedShop.OpenUntil + " Uhr";
                string transitionHint = _sceneTransitions.GetHint(Player);
                if (!string.IsNullOrEmpty(transitionHint)) return transitionHint;

                G1C_WorldObject target = FindTarget();
                if (target == null) return "";
                switch (target.Interaction)
                {
                    case G1C_InteractionType.Trash: return "[E] Mülleimer durchsuchen";
                    case G1C_InteractionType.Inspect: return "[E] " + target.Name + " ansehen";
                    case G1C_InteractionType.Arcade:
                        return target.Name == G1C_ArcadeCatalog.HighscoreBoardName ? "[E] Highscores ansehen" : "[E] " + target.Name + " spielen";
                    case G1C_InteractionType.Bike: return Bikes.Riding ? "[E] Fahrrad abstellen" : "[E] Aufs Fahrrad steigen";
                    case G1C_InteractionType.Pet: return "[E] " + target.Name + " streicheln";   // Patrick Lupierz – 16.11.2026
                    case G1C_InteractionType.Fishing: return "[E] Angeln";                         // Patrick Lupierz – 16.11.2026
                    case G1C_InteractionType.Claw:
                        if (CurrentLocation == G1C_WorldLocation.Arcade) return "[E] Greifautomat";
                        break;
                    case G1C_InteractionType.Shop: return "[E] Einkaufen";
                    case G1C_InteractionType.Vending: return "[E] Automat  [F3] Kaffee kaufen";
                    case G1C_InteractionType.Wardrobe: return "[E] Umziehen";
                    case G1C_InteractionType.Bed: return "[E] Schlafen";
                    case G1C_InteractionType.Furnish: return "[E] Zuhause einrichten";
                    case G1C_InteractionType.Bank: return "[E] Mit der Bankierin sprechen";
                    case G1C_InteractionType.LoanShark: return "[E] Mit dem Geldhai sprechen";
                    case G1C_InteractionType.FurnitureShop: return "[E] Mit der Möbelhändlerin sprechen";
                }
                if (CurrentLocation == G1C_WorldLocation.Casino) return _interactions.GetHint(Player);
                return target is G1C_Npc ? "[E] Mit " + target.Name + " sprechen" : "";
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
                       State == G1C_GameState.Conversation || State == G1C_GameState.ListMenu ||
                       State == G1C_GameState.Phone || State == G1C_GameState.Map ||
                       State == G1C_GameState.FurnitureEditor; // Patrick Lupierz – 16.11.2026
            }
        }

        // Entwickler: Denis Mock
        private G1C_PlayerMovement CreateMovement()
        {
            return new G1C_PlayerMovement(Player, new G1C_CollisionManager(CurrentWorld));
        }

        // Entwickler: Patrick Lupierz – Objekt vor der Figur (im Casino nur interaktive Objekte)
        private G1C_WorldObject FindTarget()
        {
            if (Player.IsMoving) return null;
            if (CurrentLocation == G1C_WorldLocation.Casino) return _interactions.FindTarget(Player);
            G1C_WorldObject facing = CurrentWorld.ObjectAt(Player.FacingTileX, Player.FacingTileY);
            // Änderung durch: Patrick Lupierz – 16.11.2026 – begehbare Objekte (Angel): man kann nicht davor stehen
            // bleiben, weil man sofort daraufläuft. Deshalb zählt auch das Objekt auf der eigenen Kachel.
            if (facing == null || facing.Interaction == G1C_InteractionType.None)
            {
                G1C_WorldObject here = CurrentWorld.ObjectAt(Player.TileX, Player.TileY);
                if (here != null && !here.Solid && here.Interaction == G1C_InteractionType.Fishing) return here;
            }
            return facing;
        }

        // ------------------------------------------------------------------ Eingaben

        public void HandleInput(G1C_InputEvent input)
        {
            if (input == null) return;
            bool isKey = input.Kind == G1C_InputKind.Key;
            // Patrick Lupierz – 16.11.2026: Admin-Cheat nur im Spielautomaten (dort belegen A D M I N keine Funktion)
            if (isKey && ActiveGame is G1C_Slots && Cheats.Feed(input.Key)) OnCheatToggled();

            switch (State)
            {
                case G1C_GameState.Title:
                    if (isKey) Start.HandleKey(input.Key);
                    else Start.Click(G1C_StartView.RowAt(input.X, input.Y, Start));
                    if (Start.Stage == G1C_StartStage.Done)
                    {
                        if (Start.Loaded != null) ApplySave(Start.Loaded);
                        else BeginNewGame();
                    }
                    break;

                case G1C_GameState.MainGame:
                    if (isKey && !IsSleeping) HandleWorldKey(input.Key); // Patrick Lupierz – 15.11.2026: beim Schlafen keine Steuerung
                    break;

                case G1C_GameState.Dialogue:
                    // Ein Klick blättert wie [E] weiter
                    Dialogue.HandleKey(isKey ? input.Key : Key.E);
                    if (!Dialogue.IsOpen && State == G1C_GameState.Dialogue) State = G1C_GameState.MainGame;
                    break;

                case G1C_GameState.Menu:
                    // Änderung durch: Patrick Lupierz – 15.11.2026 – Selbsttest nur auf ausdrücklichen Aufruf (Menü + F12)
                    if (isKey && input.Key == Key.F12) OpenSelfTest();
                    else if (isKey && input.Key == Key.T) NotifySystemMessage(Sound.ToggleMute()); // Patrick Lupierz – 16.11.2026
                    else if (isKey && input.Key == Key.F11) ToggleSoundMixer(); // Patrick Lupierz – 19.11.2026: Handy-Mixer
                    else if (!isKey || Menu.ShouldClose(input.Key)) State = G1C_GameState.MainGame;
                    break;

                case G1C_GameState.Conversation:
                    HandleConversationInput(input);
                    break;

                case G1C_GameState.ListMenu:
                    if (!isKey) ListMenu.Choose(G1C_ListMenuView.RowAt(input.X, input.Y, ListMenu));
                    else if (ListMenu.HandleKey(input.Key)) State = G1C_GameState.MainGame;
                    break;

                case G1C_GameState.Phone:
                    if (isKey) HandlePhoneRequest(Phone.HandleKey(input.Key, Clock));
                    break;

                case G1C_GameState.Map:
                    if (isKey && input.Key == Key.B)
                        State = G1C_GameState.Menu;
                    else if (isKey && (input.Key == Key.M || G1C_InputManager.IsBack(input.Key)))
                        State = G1C_GameState.MainGame;
                    else if (!isKey) MapView.SelectAt(input.X, input.Y, Exterior);
                    break;

                case G1C_GameState.FurnitureEditor:
                    HandleFurnitureEditorInput(input); // Patrick Lupierz – 16.11.2026
                    break;

                case G1C_GameState.Result:
                    if (!isKey || G1C_InputManager.IsConfirm(input.Key) || G1C_InputManager.IsBack(input.Key))
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
                // Angetrunken: jeder vierte Schritt geht in eine zufällige Richtung
                if (Buffs.Has("drunk") && _random.Next(4) == 0) direction = (G1C_Direction)_random.Next(4);
                _movement.RequestStep(direction.Value);
            }
            else if (G1C_InputManager.IsConfirm(key))
            {
                Interact();
            }
            else if (key == Key.F3)
            {
                G1C_WorldObject target = FindTarget();
                if (target != null && target.Interaction == G1C_InteractionType.Vending)
                    BuyVendingItem(G1C_ItemCatalog.Get("coffee"), true);
            }
            else if (key == Key.M && CurrentLocation == G1C_WorldLocation.Exterior)
            {
                MapView.ClearSelection();
                State = G1C_GameState.Map;
            }
            else if (key == Key.B || key == Key.M)
            {
                State = G1C_GameState.Menu;
            }
            else if (key == Key.P)
            {
                Phone.Open(TickCount);
                State = G1C_GameState.Phone;
            }
            else if (key == Key.I)
            {
                OpenInventory();
            }
        }

        // Entwickler: Patrick Lupierz – Datum: 15.11.2026
        // Aufgabe: Gehaltene Richtung nur während der Erkundung; in Dialogen, Menüs und Minispielen läuft
        //          die Figur nicht weiter. Angetrunken geht jeder vierte Schritt in eine zufällige Richtung.
        private G1C_Direction? HeldDirection()
        {
            if (State != G1C_GameState.MainGame || IsSleeping) return null;
            G1C_Direction? held = G1C_InputManager.HeldDirection(Player.Facing);
            if (held.HasValue && Buffs.Has("drunk") && _random.Next(4) == 0) held = (G1C_Direction)_random.Next(4);
            return held;
        }

        // Änderung durch: Denis Mock – Türen wechseln die aktive Szene; Casinointeraktionen bleiben erhalten.
        // Änderung durch Denis Mock: Stadtbewohner benutzen dasselbe Gesprächssystem wie Casino-NPCs.
        // Änderung durch: Patrick Lupierz – Mülleimer, Läden, Kleiderschrank und Bett in allen Szenen.
        private void Interact()
        {
            if (Player.IsMoving) return;

            // Änderung durch: Patrick Lupierz – Datum: 15.11.2026 – geschlossene Läden sind abgeschlossen
            G1C_Shop lockedShop = ClosedShopAtFacingDoor();
            if (lockedShop != null)
            {
                OpenDialogue(lockedShop.Name, new[] { "An der Tür hängt ein Schild: CLOSED.\nGeöffnet von " + lockedShop.OpenFrom +
                                                      " bis " + lockedShop.OpenUntil + " Uhr. Jetzt ist es " + Clock.TimeText + "." }, null);
                return;
            }

            if (_sceneTransitions.TryTransition(Player))
            {
                G1C_WorldLocation leavingFrom = CurrentLocation;
                if (leavingFrom == G1C_WorldLocation.Home && Finance.LoanBonesBroken)
                {
                    TriggerHouseExitDebtEncounter();
                    return;
                }
                _movement.ClearQueue();
                _movement = CreateMovement();
                Npcs.Bubbles.Clear();
                AnimalVoices.Clear();
                Sound.Play("door"); // Patrick Lupierz – 16.11.2026
                // Änderung durch: Patrick Lupierz – 15.11.2026 – in Gebäuden wird nicht Fahrrad gefahren
                // Änderung durch: Patrick Lupierz – 16.11.2026 – Fehler "Fahrrad muss nach dem Haus zweimal angeklickt
                // werden" behoben: Das Rad wartet vor der Tür (ParkedAtDoor) und beim Hinausgehen steigt man automatisch
                // wieder auf. Vorher war man abgestiegen, das Rad wurde aber weiter als ausgerüstet angezeigt.
                if (Bikes.Riding && CurrentLocation != G1C_WorldLocation.Exterior)
                {
                    Bikes.ParkAtDoor();
                    NotifySystemMessage("Du stellst dein Fahrrad vor der Tür ab.");
                }
                else if (Bikes.ParkedAtDoor && CurrentLocation == G1C_WorldLocation.Exterior)
                {
                    Bikes.ResumeFromDoor();
                    NotifySystemMessage("Du steigst wieder auf dein Fahrrad.");
                }
                return;
            }

            G1C_WorldObject target = FindTarget();
            if (target == null) return;
            target.OnInteract(Player);
            // Änderung durch: Patrick Lupierz – 15.11.2026 – Verkaufsfenster außen nutzen das Sortiment ihres Ladens
            G1C_ShopWindow window = target as G1C_ShopWindow;
            _shopLocation = window != null ? window.ShopLocation : CurrentLocation;

            switch (target.Interaction)
            {
                case G1C_InteractionType.Trash: SearchTrash(target); return;
                case G1C_InteractionType.Inspect: OpenDialogue(target.Name, target.Lines, null); return; // Patrick Lupierz – 15.11.2026
                case G1C_InteractionType.Arcade: OpenCabinet(target); return;                            // Patrick Lupierz – 15.11.2026
                case G1C_InteractionType.Bike: UseParkedBike(); return;                                  // Patrick Lupierz – 15.11.2026
                case G1C_InteractionType.Pet: PetAnimal(target as G1C_PetAnimal); return;                // Patrick Lupierz – 16.11.2026
                case G1C_InteractionType.Fishing: StartFishing(); return;                                // Patrick Lupierz – 16.11.2026
                case G1C_InteractionType.Shop:
                    // Änderung durch: Patrick Lupierz – 15.11.2026 – Verkäufer mit Profil führen erst ein Gespräch
                    if (Npcs.TryStartConversation(target)) State = G1C_GameState.Conversation;
                    else OpenShop();
                    return;
                case G1C_InteractionType.Vending: OpenVending(); return;
                case G1C_InteractionType.Wardrobe: OpenWardrobe(); return;
                case G1C_InteractionType.Bed:
                    OpenSleepMenu();
                    return;
                case G1C_InteractionType.Furnish:
                    OpenFurnishMenu();
                    return;
            }

            // Änderung durch: Eduard Schoenfeld – Datum: 12.11.2026
            // NPCs mit Charakterprofil (auch über ihren Tisch/Tresen) führen ein Gespräch mit Antwortoptionen.
            if (Npcs.TryStartConversation(target))
            {
                State = G1C_GameState.Conversation;
                if (Buffs.Has("drunk")) Npcs.Conversation.ApplyOutsideEffect(0, -2); // riecht nach Alkohol
                return;
            }

            // Änderung durch: Patrick Lupierz – 15.11.2026 – Minispiele auch in der Spielhalle (Greifautomat)
            if (CurrentLocation != G1C_WorldLocation.Casino && CurrentLocation != G1C_WorldLocation.Arcade) return;

            switch (target.Interaction)
            {
                case G1C_InteractionType.Roulette:
                case G1C_InteractionType.Blackjack:
                case G1C_InteractionType.Poker:
                case G1C_InteractionType.Slots:
                case G1C_InteractionType.Claw:
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
            G1C_ConversationAction action = conversation.TakeRequestedAction();
            if (action == G1C_ConversationAction.BankMenu)
            {
                OpenBankMenu();
                return;
            }
            if (action == G1C_ConversationAction.FurnitureShop || action == G1C_ConversationAction.Shop)
            {
                OpenShop();
                return;
            }
            State = G1C_GameState.MainGame;
            if (game.HasValue) StartMinigame(game.Value);
        }

        private void OpenDialogue(string speaker, string[] pages, Action onAccept)
        {
            Dialogue.Open(speaker, pages, onAccept);
            State = G1C_GameState.Dialogue;
        }

        // ------------------------------------------------------------------ Mülleimer, Läden, Inventar

        // Entwickler: Patrick Lupierz
        private void SearchTrash(G1C_WorldObject can)
        {
            Sound.Play("trash_open"); // Patrick Lupierz – 19.11.2026
            string found = _trash.Search(G1C_TrashSystem.KeyOf(CurrentLocation, can), Clock.Day, _random, Money, Inventory);
            if (found == null)
            {
                OpenDialogue("Mülleimer", new[] { "Hier hast du heute schon gesucht.\nMorgen liegt vielleicht wieder etwas drin." }, null);
                return;
            }
            _trash.NotifyWitnesses(CurrentWorld, Player, Npcs, Clock.Day);
            OpenDialogue("Mülleimer", new[] { found }, null);
        }

        // Entwickler: Patrick Lupierz – Datum: 15.11.2026
        // Aufgabe: Läden mit Öffnungszeiten, die gerade geschlossen sind (für CLOSED-Schild und Türsperre).
        //          Die Tankstelle ist durchgehend geöffnet und erscheint daher nie.
        public IEnumerable<G1C_WorldLocation> ClosedShops()
        {
            foreach (G1C_WorldLocation location in G1C_Shop.Locations)
                if (!G1C_Shop.For(location).IsOpen(Clock.Hour)) yield return location;
        }

        private G1C_Shop ClosedShopAtFacingDoor()
        {
            if (CurrentLocation != G1C_WorldLocation.Exterior || Player.IsMoving) return null;
            G1C_WorldLocation? destination = CurrentWorld.Grid.GetTransitionAt(Player.FacingTileX, Player.FacingTileY);
            G1C_Shop shop = destination.HasValue ? G1C_Shop.For(destination.Value) : null;
            return shop != null && !shop.IsOpen(Clock.Hour) ? shop : null;
        }

        // Entwickler: Patrick Lupierz
        private void OpenShop()
        {
            G1C_Shop shop = G1C_Shop.For(_shopLocation); // Patrick Lupierz – 15.11.2026: auch Verkaufsfenster
            if (shop == null) return;
            if (!shop.IsOpen(Clock.Hour))
            {
                OpenDialogue(shop.Name, new[] { "Wir haben geschlossen.\nÖffnungszeiten: " + shop.OpenFrom + " bis " + shop.OpenUntil + " Uhr." }, null);
                return;
            }
            ListMenu.Open(shop.Name, () => ShopRows(shop));
            State = G1C_GameState.ListMenu;
        }

        private void OpenVending()
        {
            ListMenu.Open("Verkaufsautomat", VendingRows);
            State = G1C_GameState.ListMenu;
        }

        private void OpenBankMenu()
        {
            ListMenu.Open("Bankkonto", BankRows);
            State = G1C_GameState.ListMenu;
        }

        private List<G1C_MenuRow> BankRows()
        {
            int[] amounts = { 25, 100, 250 };
            List<G1C_MenuRow> rows = new List<G1C_MenuRow>();
            // Änderung durch: Patrick Lupierz – Datum: 15.11.2026 – Kontostand, Zinsen und Kontoauszug
            rows.Add(new G1C_MenuRow
            {
                Text = "Kontostand: " + Finance.Savings + " Chips",
                Right = G1C_FinanceManager.InterestPercentPerDay + " % pro Tag",
                Detail = "Nächster Tag: +" + G1C_FinanceManager.InterestFor(Finance.Savings) + " Chips. Bisher erhalten: " +
                         Finance.TotalInterestPaid + " Chips.",
                Run = () => ListMenu.Message = Finance.Statement.Count == 0
                    ? "Noch keine Buchungen in diesem Spiel."
                    : "Letzte Buchungen: " + string.Join(", ", Finance.Statement)
            });
            foreach (int amount in amounts)
            {
                int current = amount;
                rows.Add(new G1C_MenuRow
                {
                    Text = "Einzahlen: " + current + " Chips",
                    Right = "Bargeld " + Money.Balance,
                    Detail = "Chips vom Bargeld auf dein Bankkonto übertragen.",
                    Run = () => ListMenu.Message = Finance.Deposit(Money, current)
                        ? current + " Chips eingezahlt."
                        : "Nicht genug Bargeld."
                });
            }
            rows.Add(new G1C_MenuRow
            {
                Text = "Alles Bargeld einzahlen", Right = Money.Balance + " Chips",
                Detail = "Den gesamten Bargeldbestand aufs Bankkonto übertragen.",
                Run = () =>
                {
                    int amount = Money.Balance;
                    ListMenu.Message = amount > 0 && Finance.Deposit(Money, amount)
                        ? amount + " Chips eingezahlt."
                        : "Kein Bargeld zum Einzahlen.";
                }
            });
            foreach (int amount in amounts)
            {
                int current = amount;
                rows.Add(new G1C_MenuRow
                {
                    Text = "Abheben: " + current + " Chips",
                    Right = "Konto " + Finance.Savings,
                    Detail = "Chips vom Bankkonto ins Bargeld übertragen.",
                    Run = () => ListMenu.Message = Finance.Withdraw(Money, current)
                        ? current + " Chips abgehoben."
                        : "Nicht genug Guthaben."
                });
            }
            rows.Add(new G1C_MenuRow
            {
                Text = "Alles abheben", Right = Finance.Savings + " Chips",
                Detail = "Das gesamte Bankguthaben ins Bargeld übertragen.",
                Run = () =>
                {
                    int amount = Finance.Savings;
                    ListMenu.Message = amount > 0 && Finance.Withdraw(Money, amount)
                        ? amount + " Chips abgehoben."
                        : "Kein Guthaben zum Abheben.";
                }
            });
            return rows;
        }

        private List<G1C_MenuRow> VendingRows()
        {
            string[] itemIds = { "coffee", "water", "snack" };
            return itemIds.Select(id => G1C_ItemCatalog.Get(id)).Select(item => new G1C_MenuRow
            {
                Text = item.Name,
                Right = G1C_Shop.PriceWithMarkup(item.Price) + " Chips",
                Detail = item.Description + "  F3 kauft Kaffee mit 12 % Chance auf ein Extra.",
                SpriteId = item.IconId,
                Run = () => ListMenu.Message = BuyVendingItem(item, false)
            }).ToList();
        }

        private string BuyVendingItem(G1C_Item item, bool extraChance)
        {
            int before = Inventory.Count(item.Id);
            int balanceBefore = Money.Balance;
            string message = G1C_Shop.Buy(item, Money, Inventory);
            if (Money.Balance < balanceBefore) Sound.Play("coin"); // Patrick Lupierz – 19.11.2026
            if (extraChance && Inventory.Count(item.Id) > before && _random.NextDouble() < 0.12)
            {
                Inventory.Add(item.Id);
                NotifySystemMessage("Du hast Glück: Beim Runterfallen wurde ein weiteres Item runtergestoßen.");
                return message + " Ein weiteres Item ist heruntergefallen!";
            }
            NotifySystemMessage(message);
            return message;
        }

        // Entwickler: Patrick Lupierz – Datum: 15.11.2026
        // Aufgabe: Einziger Einstieg für den Tageswechsel. Bucht die Bankzinsen (pro Tag höchstens einmal,
        //          siehe G1C_FinanceManager) und meldet sie. Weitere tagesabhängige Systeme (Mülleimer,
        //          Taschengeld) prüfen den Spieltag selbst über Clock.Day.
        private int ApplyNewDay()
        {
            int interest = Finance.ApplyDailyInterest(Clock.Day);
            if (interest > 0) NotifySystemMessage("Tag " + Clock.Day + ": Die Bank schreibt dir " + interest + " Chips Zinsen gut.");
            int loanInterest = Finance.ApplyLoanInterest(Clock.Day);
            if (loanInterest > 0)
            {
                NotifySystemMessage("Geldhai: Deine Schuld ist um " + loanInterest + " Chips gewachsen.");
            }
            if (Finance.ApplyLoanRespectDay(Clock.Day))
            {
                G1C_NpcCharacter shark = Npcs.Find("Geldhai Viktor");
                if (shark != null)
                {
                    shark.Relationship.ChangeRespect(-5);
                    shark.Relationship.UpdateStatus(shark.Memory, shark.Personality);
                }
            }
            if (Finance.ApplyLoanSmsDay(Clock.Day))
            {
                int debt = Finance.LoanOwed;
                int age = System.Math.Max(0, Clock.Day - Finance.LoanStartDay);
                string text;
                switch (Finance.LoanWarningStage)
                {
                    case 5:
                        text = "LETZTE PHASE: " + debt + " Chips, " + age + " Tage offen, Sozialschmarotzer. Milo bleibt weg. Heute entscheidet sich, ob deine Tür oder deine Knochen dran sind.";
                        break;
                    case 4:
                        text = "DU MACHST ES SCHWERER: " + debt + " Chips sind seit " + age + " Tagen offen. Saftsack, die nächste Nacht kostet dich vielleicht das Gehen.";
                        break;
                    case 3:
                        text = "DAS IST KEINE FREUNDLICHE ERINNERUNG: " + debt + " Chips seit " + age + " Tagen. Ich stehe heute vor deiner Tür, Versager.";
                        break;
                    case 2:
                        text = "Du hast " + age + " Tage gezögert. " + debt + " Chips stehen noch offen, Lappen. Morgen wird Viktor persönlicher.";
                        break;
                    default:
                        text = "Erste Mahnung: " + debt + " Chips, " + age + " Tag(e) offen. Freundlich zahlt es sich aus. Ich will mein Geld.";
                        break;
                }
                Phone.Receive("Geldhai Viktor", text, Clock);
                if (Finance.LoanSmsCount >= 3) ApplySharkConsequences();
            }
            return interest;
        }

        private void NotifySystemMessage(string message)
        {
            SystemMessage = message;
            SystemMessageEndTick = TickCount + 125;
        }

        private List<G1C_MenuRow> ShopRows(G1C_Shop shop)
        {
            List<G1C_MenuRow> rows = new List<G1C_MenuRow>();
            foreach (G1C_Item item in shop.ItemIds.Select(G1C_ItemCatalog.Get))
            {
                G1C_Item current = item;
                bool owned = item.IsUnique && Inventory.Has(item.Id); // Patrick Lupierz – 15.11.2026: auch Fahrräder
                // Änderung durch: Patrick Lupierz – 16.11.2026 – höchstens 3 Stück je Möbel (Inventar + aufgestellt)
                int placed = Home.PlacedFurniture.Count(f => f.ItemId == item.Id);
                bool full = item.Category == G1C_ItemCategory.Moebel && Inventory.Count(item.Id) + placed >= G1C_Shop.MaxFurniturePerItem;
                rows.Add(new G1C_MenuRow
                {
                    Text = item.Name, Right = owned ? "gekauft" : full ? "max. " + G1C_Shop.MaxFurniturePerItem : G1C_Shop.PriceWithMarkup(item.Price) + " Chips",
                    Detail = item.Description + (item.Category == G1C_ItemCategory.Moebel
                        ? " Du hast " + (Inventory.Count(item.Id) + placed) + "/" + G1C_Shop.MaxFurniturePerItem + "." : ""),
                    SpriteId = item.IconId,
                    // Änderung durch: Patrick Lupierz – 15.11.2026 – Ladenschluss gilt auch bei offenem Menü
                    Run = () => ListMenu.Message = shop.IsOpen(Clock.Hour)
                        ? BuyWithSound(current, placed)
                        : "Ladenschluss! Heute wird nichts mehr verkauft (" + shop.OpenFrom + "–" + shop.OpenUntil + " Uhr)."
                });
            }
            foreach (G1C_Item item in Inventory.Items.Where(i => i.SellValue > 0))
            {
                G1C_Item current = item;
                rows.Add(new G1C_MenuRow
                {
                    Text = "Abgeben: " + item.Name + " x" + Inventory.Count(item.Id), Right = "+" + item.SellValue + " Chips",
                    Detail = item.Description, Run = () => ListMenu.Message = G1C_Shop.Sell(current, Money, Inventory)
                });
            }
            return rows;
        }

        // Entwickler: Patrick Lupierz – Datum: 19.11.2026
        // Aufgabe: Handy-Mixer (Web-App im WLAN) ein- oder ausschalten. Läuft nur auf ausdrücklichen Wunsch; die
        //          Adresse und die PIN erscheinen als Dialog, damit sie in Ruhe abgetippt werden können.
        private void ToggleSoundMixer()
        {
            if (Sound.Mixer != null && Sound.Mixer.Running)
            {
                Sound.Mixer.Stop();
                Sound.Mixer = null;
                State = G1C_GameState.MainGame;
                NotifySystemMessage("Handy-Mixer beendet.");
                return;
            }
            G1C_SoundMixerServer server = new G1C_SoundMixerServer(Sound);
            if (!server.Start(Sound.MixerLoopbackOnly))
            {
                OpenDialogue("Sound-Mixer", new[] { "Der Handy-Mixer konnte nicht starten.\n" + server.LastError }, null);
                return;
            }
            Sound.Mixer = server;
            IList<string> urls = server.Urls;
            string address = urls.Count == 0 ? "(Adresse nicht gefunden – IP des PCs mit :" + server.Port + " eingeben)"
                                             : string.Join("\n", urls.Take(2));
            OpenDialogue("Sound-Mixer fürs Handy", new[]
            {
                "Handy und PC im selben WLAN.\nÖffne im Handy-Browser:\n" + address,
                "PIN: " + server.Pin + "\nBeenden: Menü [B], dann [F11].\nTipp: Im Browser 'Zum Startbildschirm hinzufügen'."
            }, null);
        }

        // Entwickler: Patrick Lupierz – Datum: 16.11.2026 – Kauf mit Möbelgrenze; erfolgreicher Kauf klimpert
        private string BuyWithSound(G1C_Item item, int placedCount)
        {
            int before = Money.Balance;
            string message = G1C_Shop.Buy(item, Money, Inventory, placedCount);
            if (Money.Balance < before) Sound.Play("coin");
            return message;
        }

        // Entwickler: Patrick Lupierz – Datum: 16.11.2026
        // Aufgabe: Admin-Cheat umgeschaltet: Meldung im HUD und – falls gerade gespielt wird – im Spielautomaten.
        private void OnCheatToggled()
        {
            string text = Cheats.JackpotActive
                ? "ADMIN AN: Jackpot bei jeder Drehung (7-7-7) und dauerhaft Turbo-Tempo."
                : "ADMIN AUS: Jackpot-Garantie und Turbo-Tempo beendet.";
            NotifySystemMessage(text);
            G1C_Slots slots = ActiveGame as G1C_Slots;
            if (slots != null) slots.ShowNotice(text);
            Sound.Play("click");
        }

        // Entwickler: Patrick Lupierz – Datum: 16.11.2026 – Katze/Hund streicheln: Herz-Blase, Laut, Meldung
        private void PetAnimal(G1C_PetAnimal pet)
        {
            if (pet == null) return;
            string message = pet.Pet(Player);
            AnimalVoices.Say(pet, pet.Kind == G1C_PetKind.Cat ? "Schnurr ♥" : "Wuff! ♥", TickCount);
            Sound.Play(pet.Kind == G1C_PetKind.Cat ? "meow" : "woof");
            NotifySystemMessage(message);
        }

        // Entwickler: Patrick Lupierz – Datum: 16.11.2026
        // Aufgabe: Darf ein Haustier diese Kachel betreten? Nicht auf massive Objekte, nicht auf den Spieler
        //          (auch nicht auf die Kachel, von der er gerade kommt) und nicht vor Türen.
        private bool PetCanEnter(int x, int y)
        {
            if (new G1C_CollisionManager(CurrentWorld).IsBlocked(x, y)) return false;
            if (Math.Abs(Player.VisualX - x) < 1.0 && Math.Abs(Player.VisualY - y) < 1.0) return false;
            if (Player.TileX == x && Player.TileY == y) return false;
            IG1C_WorldMap map = CurrentWorld.Grid;
            if (map.GetTransitionAt(x, y) != null || map.GetTransitionAt(x, y + 1) != null || map.GetTransitionAt(x, y - 1) != null) return false;
            return CurrentLocation != G1C_WorldLocation.Exterior || !Exterior.Map.IsDoorApproach(x, y);
        }

        // Entwickler: Patrick Lupierz – Datum: 16.11.2026 – Einrichtungsmodus öffnen (null = Verschieben)
        private void OpenFurnitureEditor(G1C_Item item)
        {
            if (CurrentLocation != G1C_WorldLocation.Home) return;
            Furnishing.Begin(item, Home, Player);
            State = G1C_GameState.FurnitureEditor;
        }

        // Entwickler: Patrick Lupierz – Datum: 16.11.2026
        // Aufgabe: Eingaben im Einrichtungsmodus: Klick auf eine Kachel = Cursor dorthin + Aufstellen/Aufheben,
        //          WASD/Pfeile bewegen den Cursor, [E] aufstellen/aufheben, [R] einlagern, [N] nächstes Möbel,
        //          [Q]/[Esc] beenden (ein aufgehobenes Möbel kehrt an seinen Platz zurück).
        private void HandleFurnitureEditorInput(G1C_InputEvent input)
        {
            if (input.Kind != G1C_InputKind.Key)
            {
                int tx, ty;
                if (G1C_FurnitureEditor.TileAt(input.X, input.Y, Home, out tx, out ty))
                {
                    Furnishing.SetCursor(tx, ty, Home);
                    Furnishing.Confirm(Home, Inventory, Player);
                }
                return;
            }
            Key key = input.Key;
            G1C_Direction? direction = G1C_InputManager.ToDirection(key);
            if (direction.HasValue) Furnishing.MoveCursor(G1C_DirectionHelper.DeltaX(direction.Value), G1C_DirectionHelper.DeltaY(direction.Value), Home);
            else if (G1C_InputManager.IsConfirm(key)) Furnishing.Confirm(Home, Inventory, Player);
            else if (key == Key.R) Furnishing.Store(Inventory);
            else if (key == Key.N) Furnishing.NextFromInventory(Inventory, Home, Player);
            else if (G1C_InputManager.IsBack(key))
            {
                Furnishing.Cancel(Home);
                State = G1C_GameState.MainGame;
            }
        }

        // Entwickler: Patrick Lupierz – Datum: 15.11.2026
        // Aufgabe: Automatisierte Tests ausführen und als Liste anzeigen. Die Tests arbeiten mit eigenen,
        //          neu erzeugten Spielzuständen; der laufende Spielstand wird nicht verändert.
        private void OpenSelfTest()
        {
            List<G1C_TestResult> results = G1C_SelfTest.RunAll();
            int passed = results.Count(r => r.Status == G1C_TestStatus.Bestanden);
            ListMenu.Open("Selbsttest: " + passed + "/" + results.Count + " bestanden", () => results.Select(r => new G1C_MenuRow
            {
                Text = r.Area + ": " + r.Name,
                Right = r.StatusText,
                Detail = string.IsNullOrEmpty(r.Detail) ? "Bericht: " + G1C_SelfTest.ReportFileName + " neben der OOPGames.exe" : r.Detail,
                Run = () => ListMenu.Message = r.StatusText + (string.IsNullOrEmpty(r.Detail) ? "" : " – " + r.Detail)
            }).ToList());
            State = G1C_GameState.ListMenu;
        }

        // Entwickler: Patrick Lupierz
        private void OpenInventory()
        {
            ListMenu.Open("Inventar", InventoryRows);
            State = G1C_GameState.ListMenu;
        }

        private void OpenSleepMenu()
        {
            int[] wakeHours = { 6, 8, 12, 18 };
            ListMenu.Open("Schlafen bis ...", () => wakeHours.Select(hour => new G1C_MenuRow
            {
                Text = hour.ToString("00") + ":00 Uhr",
                Right = hour <= Clock.Hour ? "morgen" : "heute",
                Detail = "Bis " + hour.ToString("00") + ":00 Uhr schlafen.",
                Run = () =>
                {
                    // Änderung durch: Patrick Lupierz – Datum: 15.11.2026 – Schlafzustand mit Animation,
                    // Tageswechsel (Zinsen) wird sofort ausgewertet.
                    Clock.SkipUntilHour(hour);
                    int interest = ApplyNewDay();
                    SleepUntilTick = TickCount + SleepAnimationTicks;
                    ListMenu.Message = "Du bist aufgewacht. Es ist Tag " + Clock.Day + ", " + Clock.TimeText + " Uhr." +
                                       (interest > 0 ? " Zinsen: +" + interest + " Chips." : "");
                }
            }).ToList());
            State = G1C_GameState.ListMenu;
        }

        private List<G1C_MenuRow> InventoryRows()
        {
            List<G1C_MenuRow> rows = new List<G1C_MenuRow>();
            foreach (G1C_Item item in Inventory.Items)
            {
                G1C_Item current = item;
                bool worn = item.Slot != G1C_ClothingSlot.None && Profile.Equipped(item.Slot) == item.Id;
                rows.Add(new G1C_MenuRow
                {
                    Text = item.Name + " x" + Inventory.Count(item.Id),
                    Right = worn ? "angezogen" : item.Category == G1C_ItemCategory.Fahrzeug ? BikeStatus(item) // Patrick Lupierz – 16.11.2026
                          : (item.IsUsable ? "[E] benutzen" : item.Category.ToString()),
                    Detail = item.Description, SpriteId = item.IconId, Run = () => UseItem(current)
                });
            }
            return rows;
        }

        // Entwickler: Patrick Lupierz – Datum: 16.11.2026 – eindeutiger Fahrradstatus im Inventar
        private string BikeStatus(G1C_Item bike)
        {
            if (Bikes.ActiveBikeId != bike.Id) return "[E] aufsteigen";
            if (Bikes.Riding) return "fährst du – [E] absteigen";
            return Bikes.ParkedAtDoor ? "steht vor der Tür" : "[E] aufsteigen";
        }

        private void UseItem(G1C_Item item)
        {
            if (!item.IsUsable) { ListMenu.Message = item.Description; return; }
            // Änderung durch: Patrick Lupierz – 15.11.2026 – Fahrrad wird nicht verbraucht, sondern auf-/abgestiegen
            if (item.Category == G1C_ItemCategory.Fahrzeug)
            {
                if (Bikes.ParkedAtDoor && CurrentLocation != G1C_WorldLocation.Exterior)
                {
                    ListMenu.Message = "Dein Fahrrad steht draußen vor der Tür. Beim Hinausgehen steigst du automatisch auf.";
                    return;
                }
                ListMenu.Message = Bikes.Riding && Bikes.ActiveBikeId == item.Id
                    ? Bikes.Dismount()
                    : Bikes.TryMount(Inventory, item.Id, CurrentLocation);
                return;
            }
            if (Inventory.Remove(item.Id)) { ListMenu.Message = Buffs.Apply(item, Clock.TotalTicks); Sound.Play("buff"); } // Patrick Lupierz – 19.11.2026
        }

        // Entwickler: Patrick Lupierz – Kleiderschrank/Spiegel: gekaufte Kleidung sichtbar anziehen
        private void OpenWardrobe()
        {
            ListMenu.Open("Kleiderschrank", WardrobeRows);
            State = G1C_GameState.ListMenu;
        }

        private void OpenFurnishMenu()
        {
            Sound.Play("furniture_open"); // Patrick Lupierz – 19.11.2026
            ListMenu.Open("Zuhause einrichten", () => Inventory.Items
                .Where(item => item.Category == G1C_ItemCategory.Moebel)
                .Select(item =>
                {
                    G1C_Item current = item;
                    return new G1C_MenuRow
                    {
                        Text = current.Name + " x" + Inventory.Count(current.Id),
                        Right = "[E] Platzieren",
                        Detail = current.Description,
                        SpriteId = current.IconId,
                        Run = () => OpenFurnitureEditor(current) // Patrick Lupierz – 16.11.2026: klickbare Ansicht
                    };
                }).Concat(new[]
                {
                    // Änderung durch: Patrick Lupierz – 16.11.2026 – aufgestellte Möbel verschieben/einlagern
                    new G1C_MenuRow
                    {
                        Text = "Möbel verschieben / einlagern", Right = Home.PlacedFurniture.Count + " aufgestellt",
                        Detail = "Eigenes Möbel anklicken, neuen Platz wählen oder mit [R] zurück ins Inventar legen.",
                        Run = () => OpenFurnitureEditor(null)
                    }
                }).ToList());
            State = G1C_GameState.ListMenu;
        }

        private List<G1C_MenuRow> WardrobeRows()
        {
            List<G1C_MenuRow> rows = new List<G1C_MenuRow>();
            foreach (G1C_Item item in Inventory.Items.Where(i => i.Slot != G1C_ClothingSlot.None))
            {
                G1C_Item current = item;
                bool worn = Profile.Equipped(item.Slot) == item.Id;
                rows.Add(new G1C_MenuRow
                {
                    Text = item.Name, Right = worn ? "angezogen" : "", Detail = "[E] " + (worn && item.Slot == G1C_ClothingSlot.Hat ? "absetzen" : "anziehen"),
                    Run = () =>
                    {
                        Profile.Equip(current);
                        Player.Skin = Profile.SkinId;
                        ListMenu.Message = current.Name + (Profile.Equipped(current.Slot) == current.Id ? " angezogen." : " abgesetzt.");
                    }
                });
            }
            return rows;
        }

        // ------------------------------------------------------------------ Handy

        // Entwickler: Patrick Lupierz
        private void HandlePhoneRequest(G1C_PhoneRequest request)
        {
            switch (request.Kind)
            {
                case G1C_PhoneRequestKind.Close:
                    State = G1C_GameState.MainGame;
                    break;
                case G1C_PhoneRequestKind.Call:
                    Npcs.Conversation.OpenCall(request.Contact, Profile.Name);
                    State = G1C_GameState.Conversation;
                    break;
                case G1C_PhoneRequestKind.Inventory:
                    OpenInventory();
                    break;
                case G1C_PhoneRequestKind.Save:
                    Phone.Info = G1C_SaveSystem.Save(request.Slot, CreateSave())
                        ? "Spiel gespeichert."
                        : "Speichern fehlgeschlagen!";
                    Phone.RefreshSlots();
                    break;
                case G1C_PhoneRequestKind.AudioAdjust:
                    if (request.Text == "master")
                    {
                        int value = G1C_SoundSettings.Clamp(Sound.Settings.Master + request.IntValue, 0, 100);
                        Sound.Settings.Set(G1C_SoundSettings.GlobalId, "master", value);
                    }
                    else if (request.Text == "music")
                    {
                        int value = G1C_SoundSettings.Clamp(Sound.Settings.Music + request.IntValue, 0, 100);
                        Sound.Settings.Set(G1C_SoundSettings.GlobalId, "music", value);
                    }
                    else
                    {
                        int value = G1C_SoundSettings.Clamp(Sound.Settings.Effects + request.IntValue, 0, 100);
                        Sound.Settings.Set(G1C_SoundSettings.GlobalId, "effects", value);
                    }
                    Phone.Info = "Lautstaerke: " + Sound.Settings.Master + "% Gesamt.";
                    break;
                case G1C_PhoneRequestKind.AudioMute:
                    Phone.Info = Sound.ToggleMute();
                    break;
            }
        }

        // ------------------------------------------------------------------ Neues Spiel, Speichern, Laden

        // Entwickler: Patrick Lupierz
        private void BeginNewGame()
        {
            Profile = Start.Profile;
            Finance.Restore(0, 0);
            Finance.RestoreInterest(0, 0, Clock.Day); // Patrick Lupierz – 15.11.2026
            Home.RestoreFurniture(null);
            foreach (string clothing in new[] { Profile.Top, Profile.Pants, Profile.Shoes }) Inventory.Add(clothing);
            Inventory.Add(G1C_BikeSystem.StarterBike); // Patrick Lupierz – 15.11.2026: das Fahrrad vor dem Haus gehört dir
            Bikes.Restore(false, G1C_BikeSystem.StarterBike);
            Player.Skin = Profile.SkinId;
            Npcs.PlayerName = Profile.Name;
            OpenDialogue("Willkommen", new[]
            {
                "Guten Morgen, " + Profile.Name + "! Das ist dein Zuhause.\nAm Schrank ziehst du dich um, im Bett schläfst du.",
                "[P] öffnet dein Handy: Kontakte, SMS, Uhr, Aufgaben\nund Speichern. [I] öffnet das Inventar.",
                "Draußen warten Casino, Tankstelle und Läden.\nBezahlt wird überall nur mit Chips. Viel Glück!"
            }, null);
        }

        // Entwickler: Patrick Lupierz – aktuellen Zustand aller Systeme einsammeln
        public G1C_SaveData CreateSave()
        {
            return new G1C_SaveData
            {
                Profile = Profile, Location = (int)CurrentLocation, X = Player.TileX, Y = Player.TileY, Facing = (int)Player.Facing,
                Coins = Money.Balance, Inventory = new Dictionary<string, int>(Inventory.Counts), Buffs = Buffs.Active.ToList(),
                BankSavings = Finance.Savings, LoanOwed = Finance.LoanOwed, HomeFurniture = Home.PlacedFurniture.ToList(),
                ClockTicks = Clock.TotalTicks, PlayTicks = TickCount, GamesPlayed = GamesPlayed, Reputation = Npcs.Reputation,
                Npcs = Npcs.Characters.Select(c => new G1C_SaveNpc
                {
                    Name = c.Name, Trust = c.Relationship.Trust, Respect = c.Relationship.Respect, Status = (int)c.Relationship.Status,
                    Purse = c.Purse, Mood = (int)c.Mood, Memory = c.Memory
                }).ToList(),
                Tasks = Npcs.Tasks.Tasks.Select(t => t.Capture()).ToList(),
                Messages = Phone.Messages, Trash = _trash.SearchedOnDay,
                BankInterestDay = Finance.LastInterestDay, BankInterestTotal = Finance.TotalInterestPaid,
                RidingBike = Bikes.Riding, ActiveBike = Bikes.ActiveBikeId, BikeParkedAtDoor = Bikes.ParkedAtDoor,
                ArcadeHighscores = new Dictionary<string, int>(ArcadeHighscores),
                LoanInterestDay = Finance.LoanInterestLastDay, LoanRespectDay = Finance.LoanRespectLastDay,
                LoanSmsDay = Finance.LoanSmsLastDay, LoanSmsCount = Finance.LoanSmsCount,
                LoanStartDay = Finance.LoanStartDay,
                LoanWarningStage = Finance.LoanWarningStage, LoanDoorThreat = Finance.LoanDoorThreat,
                LoanBonesBroken = Finance.LoanBonesBroken, LoanCatKidnapped = Finance.LoanCatKidnapped,
                AudioMaster = Sound.Settings.Master, AudioMusic = Sound.Settings.Music,
                AudioEffects = Sound.Settings.Effects, AudioMuted = Sound.Muted
            };
        }

        // Entwickler: Patrick Lupierz
        // Aufgabe: Geladenen Spielstand auf alle Systeme verteilen. Unbekannte NPCs/Gegenstände werden
        //          übersprungen; eine ungültige Position fällt auf den Startpunkt der Karte zurück.
        public void ApplySave(G1C_SaveData data)
        {
            Profile = data.Profile;
            Money.Restore(data.Coins);
            Inventory.Restore(data.Inventory);
            Finance.Restore(data.BankSavings, data.LoanOwed);
            Home.RestoreFurniture(data.HomeFurniture);
            Buffs.Restore(data.Buffs);
            Clock.TotalTicks = data.ClockTicks;
            Finance.RestoreInterest(data.BankInterestDay, data.BankInterestTotal, Clock.Day); // Patrick Lupierz – 15.11.2026
            Finance.RestoreLoanState(data.LoanInterestDay, data.LoanRespectDay, data.LoanSmsDay, data.LoanSmsCount, Clock.Day);
            Finance.RestoreLoanStartDay(data.LoanStartDay, Clock.Day);
            if (data.LoanDoorThreat) Finance.RestoreLoanState(data.LoanInterestDay, data.LoanRespectDay, data.LoanSmsDay,
                System.Math.Max(data.LoanSmsCount, 3), Clock.Day);
            if (!data.LoanBonesBroken) Finance.ClearBoneDamage();
            if (data.LoanBonesBroken) Bikes.SetBlockedByShark(true);
            else Bikes.SetBlockedByShark(false);
            Home.SetHouseCatPresent(!data.LoanCatKidnapped);
            TickCount = Math.Max(0, data.PlayTicks);
            GamesPlayed = Math.Max(0, data.GamesPlayed);
            Npcs.RestoreReputation(data.Reputation);
            Npcs.PlayerName = Profile.Name;

            foreach (G1C_SaveNpc saved in data.Npcs ?? new List<G1C_SaveNpc>())
            {
                G1C_NpcCharacter c = saved != null ? Npcs.Find(saved.Name) : null;
                if (c == null || !Enum.IsDefined(typeof(G1C_RelationshipStatus), saved.Status) ||
                    !Enum.IsDefined(typeof(G1C_NpcMood), saved.Mood)) continue;
                c.Relationship.Restore(saved.Trust, saved.Respect, (G1C_RelationshipStatus)saved.Status);
                c.Restore(saved.Purse, (G1C_NpcMood)saved.Mood, saved.Memory);
            }
            foreach (G1C_SaveTask saved in data.Tasks ?? new List<G1C_SaveTask>())
            {
                G1C_CollectTask task = saved != null ? Npcs.Tasks.FindFor(saved.Giver) : null;
                if (task != null) task.Restore(saved);
            }
            Phone.Messages = (data.Messages ?? new List<G1C_Sms>()).Where(m => m != null).ToList();
            _trash.SearchedOnDay = data.Trash ?? new Dictionary<string, int>();
            Sound.Settings.Set(G1C_SoundSettings.GlobalId, "master", data.AudioMaster);
            Sound.Settings.Set(G1C_SoundSettings.GlobalId, "music", data.AudioMusic);
            Sound.Settings.Set(G1C_SoundSettings.GlobalId, "effects", data.AudioEffects);
            Sound.SetMuted(data.AudioMuted);

            _sceneTransitions.Restore((G1C_WorldLocation)data.Location);
            IG1C_WorldScene scene = CurrentWorld;
            // Änderung durch: Patrick Lupierz – 15.11.2026 – gleiche Regel wie beim Laufen (nicht massive Objekte wie
            // der Pavillon oder Tiere blockieren nicht)
            bool free = !new G1C_CollisionManager(scene).IsBlocked(data.X, data.Y);
            if (free) Player.Teleport(data.X, data.Y, (G1C_Direction)data.Facing);
            else Player.Teleport(scene.PlayerStartX, scene.PlayerStartY, G1C_Direction.Down);
            _movement = CreateMovement();
            Player.Skin = Profile.SkinId;
            // Änderung durch: Patrick Lupierz – 15.11.2026 – Fahrrad und Highscores; ältere Spielstände bekommen
            // das Fahrrad vor dem Haus nachträglich
            if (!Inventory.Has(G1C_BikeSystem.StarterBike)) Inventory.Add(G1C_BikeSystem.StarterBike);
            Bikes.Restore(data.RidingBike && CurrentLocation == G1C_WorldLocation.Exterior, data.ActiveBike);
            if (data.BikeParkedAtDoor && CurrentLocation != G1C_WorldLocation.Exterior) Bikes.ParkAtDoor(true); // 16.11.2026
            ArcadeHighscores = data.ArcadeHighscores != null ? new Dictionary<string, int>(data.ArcadeHighscores) : new Dictionary<string, int>();
            State = G1C_GameState.MainGame;
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
                case G1C_InteractionType.Claw:
                    ActiveGame = new G1C_ClawMachine(Money, _random, Inventory);
                    State = G1C_GameState.Claw;
                    break;
            }
            // Änderung durch: Patrick Lupierz – 16.11.2026 – Geräusche und Admin-Cheat (Jackpot-Garantie)
            if (ActiveGame != null) ActiveGame.SoundRequested = id => Sound.Play(id);
            G1C_Slots slots = ActiveGame as G1C_Slots;
            if (slots != null) slots.GuaranteedJackpot = () => Cheats.JackpotActive;
        }

        // Entwickler: Patrick Lupierz – Datum: 16.11.2026 – Angel auf der Brücke benutzen
        private void StartFishing()
        {
            _gameStartBalance = Money.Balance;
            _activeGameType = G1C_InteractionType.Fishing;
            ActiveGame = new G1C_FishingGame(Money, _random, Inventory) { SoundRequested = id => Sound.Play(id) };
            State = G1C_GameState.Fishing;
        }

        // Entwickler: Patrick Lupierz – Datum: 15.11.2026
        // Aufgabe: Spielhallen-Automat bedienen: Spielauswahl (max. 9), Gast-Automat mit Spielen anderer Gruppen,
        //          Highscore-Tafel. Die Spiele selbst laufen wie alle Minispiele über ActiveGame.
        private void OpenCabinet(G1C_WorldObject target)
        {
            if (target.Name == G1C_ArcadeCatalog.HighscoreBoardName)
            {
                OpenDialogue("Highscore-Tafel", new[] { HighscoreText() }, null);
                return;
            }
            G1C_ArcadeCabinet cabinet = G1C_ArcadeCatalog.Find(target.Name);
            if (cabinet == null) return;
            if (cabinet.IsGuestCabinet)
            {
                List<G1C_GuestGameEntry> guests = GuestGameSource();
                _lastGuestCabinet = target; // Patrick Lupierz – 18.11.2026: nach [Esc] zurück zu dieser Auswahl
                ListMenu.Open(cabinet.Name + " – Spiele anderer Gruppen", () => guests.Select(entry => new G1C_MenuRow
                {
                    Text = entry.Name, Right = G1C_GuestGameHost.EntryFee + " Chips",
                    // Änderung durch: Patrick Lupierz – 18.11.2026 – Regeln, Painter und KI derselben Gruppe
                    Detail = "Gruppe " + entry.Group + ": Regeln, Painter " + entry.PainterType +
                             (entry.HasComputerOpponent ? ", KI " + entry.ComputerType + ". Sieg: " + G1C_GuestGameHost.WinReward + " Chips."
                                                        : ". Zu zweit am Automaten (keine Belohnung)."),
                    Run = () => StartArcadeGame(new G1C_GuestGameHost(Money, _random, entry))
                }).ToList());
                if (guests.Count == 0) ListMenu.Message = "Zurzeit sind keine Spiele anderer Gruppen registriert.";
            }
            else
            {
                ListMenu.Open(cabinet.Name + " – Spiel wählen", () => cabinet.Games.Select(game => new G1C_MenuRow
                {
                    Text = game.Title, Right = G1C_ArcadeGame.EntryFee + " Chips", Detail = game.Description +
                        (ArcadeHighscores.ContainsKey(game.Title) ? " Rekord: " + ArcadeHighscores[game.Title] : ""),
                    Run = () => StartArcadeGame(game.Create(Money, _random))
                }).ToList());
            }
            State = G1C_GameState.ListMenu;
        }

        private string HighscoreText()
        {
            if (ArcadeHighscores.Count == 0) return "Noch keine Rekorde. Sei der Erste!";
            return string.Join("\n", ArcadeHighscores.OrderByDescending(h => h.Value).Take(4).Select(h => h.Key + ": " + h.Value + " Punkte"));
        }

        private G1C_WorldObject _lastGuestCabinet; // Patrick Lupierz – 18.11.2026

        /// <summary>Quelle der Gastspiele: im Spiel das Framework (registrierte Klassen); Tests setzen eine eigene.</summary>
        public Func<List<G1C_GuestGameEntry>> GuestGameSource { get; set; } = G1C_GuestGameCatalog.DiscoverRegistered;

        private void StartArcadeGame(G1C_CasinoGameBase game)
        {
            _gameStartBalance = Money.Balance;
            _activeGameType = G1C_InteractionType.Arcade;
            ActiveGame = game;
            game.SoundRequested = id => Sound.Play(id); // Patrick Lupierz – 16.11.2026
            State = G1C_GameState.Arcade;
        }

        // Entwickler: Patrick Lupierz – Datum: 15.11.2026 – Fahrradständer vor dem Haus
        private void UseParkedBike()
        {
            string message = Bikes.Riding ? Bikes.Dismount() : Bikes.TryMount(Inventory, Bikes.ActiveBikeId, CurrentLocation);
            OpenDialogue("Fahrrad", new[] { message }, null);
        }

        private void EndMinigame()
        {
            // Änderung durch: Patrick Lupierz – 18.11.2026 – Gast-Automat: [Esc] führt zurück zur Spielauswahl,
            // damit direkt ein anderes Spiel einer anderen Gruppe gewählt werden kann.
            G1C_GuestGameHost guest = ActiveGame as G1C_GuestGameHost;
            if (guest != null && _lastGuestCabinet != null)
            {
                int delta = Money.Balance - _gameStartBalance;
                Npcs.OnMinigameFinished(_activeGameType, delta);
                ActiveGame = null;
                GamesPlayed++;
                NotifySystemMessage(guest.Name + " beendet: " + (delta >= 0 ? "+" : "") + delta + " Chips.");
                OpenCabinet(_lastGuestCabinet);
                return;
            }
            // Änderung durch: Patrick Lupierz – 16.11.2026 – Angeln endet mit einer Fangübersicht statt Chips-Bilanz
            G1C_FishingGame fishing = ActiveGame as G1C_FishingGame;
            if (fishing != null)
            {
                ActiveGame = null;
                OpenDialogue("Angeln", new[] { fishing.Caught.Count == 0
                    ? "Heute hat nichts angebissen. Morgen vielleicht!"
                    : "Dein Fang: " + string.Join(", ", fishing.Caught) + ".\nFische und Fundstücke kannst du in jedem Laden verkaufen." }, null);
                return;
            }
            Sound.StopLoop("slots_spin"); // Patrick Lupierz – 19.11.2026: Schleifen beenden
            PlayResultSound();
            // Änderung durch: Patrick Lupierz – 15.11.2026 – Rekorde der Spielhalle merken
            G1C_ArcadeGame arcade = ActiveGame as G1C_ArcadeGame;
            if (arcade != null && arcade.BestScore > 0)
            {
                int best;
                ArcadeHighscores.TryGetValue(arcade.Name, out best);
                ArcadeHighscores[arcade.Name] = Math.Max(best, arcade.BestScore);
            }
            Result.Show(ActiveGame.Name, _gameStartBalance, Money.Balance);
            // Änderung durch: Eduard Schoenfeld – NPCs reagieren auf das tatsächliche Ergebnis
            Npcs.OnMinigameFinished(_activeGameType, Money.Balance - _gameStartBalance);
            ActiveGame = null;
            GamesPlayed++;
            State = G1C_GameState.Result;
        }

        // Entwickler: Patrick Lupierz – Datum: 18.11.2026
        // Aufgabe: SprintPlanning Issue 16 – der Geldhai spricht den Spieler beim Vorbeilaufen an (Sprechblase und
        //          Hinweis), höchstens einmal pro Vorbeigehen: erst wenn der Spieler sich entfernt hat, wieder.
        public const int LoanSharkRange = 2;
        public bool LoanSharkApproached { get; private set; }

        private void ApplySharkConsequences()
        {
            if (Finance.LoanDoorThreat)
            {
                G1C_Npc shark = Exterior.TownNpcs.FirstOrDefault(n => n.Interaction == G1C_InteractionType.LoanShark);
                if (shark != null)
                {
                    shark.Teleport(MapDoorX(), MapDoorY(), G1C_Direction.Down);
                    Npcs.Bubbles.ShowReaction(shark, "Hier ist deine letzte Warnung, Versager.", TickCount);
                }
            }
            if (Finance.LoanBonesBroken)
            {
                Player.Humping = true;
                Player.StepTicks = Math.Max(Player.StepTicks, 9);
                Bikes.SetBlockedByShark(true);
                NotifySystemMessage("Viktor hat dir die Knochen gebrochen. Du humpelst und kannst kein Fahrrad fahren.");
            }
            if (Finance.LoanCatKidnapped)
            {
                Home.SetHouseCatPresent(false);
                Phone.Receive("Geldhai Viktor", "Ich habe eure Katze Milo. Bezahle deine Schulden – dann bekommst du sie zurück.", Clock);
            }
        }

        private int MapDoorX() { return Exterior.Map.DoorX(G1C_WorldLocation.Home); }
        private int MapDoorY() { return Exterior.Map.DoorY(G1C_WorldLocation.Home) + 1; }

        // Änderung durch: Denis Mock – Datum: 19.11.2026 – Begegnung nur beim tatsächlichen Verlassen des Hauses
        private void TriggerHouseExitDebtEncounter()
        {
            G1C_Npc shark = Exterior.TownNpcs.FirstOrDefault(n => n.Interaction == G1C_InteractionType.LoanShark);
            if (shark != null)
            {
                shark.Teleport(MapDoorX(), MapDoorY(), G1C_Direction.Down);
                Npcs.Bubbles.ShowReaction(shark, "Ich hab dich lange genug warten lassen.", TickCount);
            }
            OpenDialogue("Geldhai Viktor", new[]
            {
                "Ich hab dich lange genug warten lassen.",
                "Du hast deine Schulden nicht bezahlt. Keine weiteren Ausreden."
            }, () =>
            {
                Finance.MarkBonesBroken();
                Player.Humping = true;
                Player.StepTicks = Math.Max(Player.StepTicks, 9);
                Bikes.SetBlockedByShark(true);
                _sceneTransitions.Restore(G1C_WorldLocation.Hospital);
                Player.Teleport(Hospital.PlayerStartX, Hospital.PlayerStartY, G1C_Direction.Down);
                _movement.ClearQueue();
                _movement = CreateMovement();
                NotifySystemMessage("Du wachst im Krankenhaus auf. Schwester Emma kann dich behandeln.");
                OpenDialogue("Krankenhaus", new[] { "Du wachst nach der Begegnung mit Viktor auf.\nSchwester Emma kümmert sich um dich." }, null);
            });
        }

        private void ApplyLoanRepaidConsequences()
        {
            Player.Humping = false;
            Player.StepTicks = G1C_Player.TicksPerStep;
            Bikes.SetBlockedByShark(false);
            Home.SetHouseCatPresent(true);
            Phone.Receive("Geldhai Viktor",
                "Schulden bezahlt. Milo ist wieder sicher. Wir sind quitt - für heute.", Clock);
            G1C_Npc shark = Exterior.TownNpcs.FirstOrDefault(n => n.Interaction == G1C_InteractionType.LoanShark);
            if (shark != null)
            {
                shark.Teleport(35, 45, G1C_Direction.Down);
                Npcs.Bubbles.ShowReaction(shark, "Bezahlt. Ich bin zufrieden.", TickCount);
            }
            NotifySystemMessage("Die Schulden sind bezahlt. Dein Humpeln endet und Milo ist zurück.");
        }

        private void TreatBrokenLegs()
        {
            Player.Humping = false;
            Player.StepTicks = G1C_Player.TicksPerStep;
            Bikes.SetBlockedByShark(false);
            Finance.ClearBoneDamage();
            NotifySystemMessage("Schwester Emma hat dich behandelt. Du kannst wieder laufen und Fahrrad fahren.");
        }

        private void UpdateLoanSharkApproach()
        {
            G1C_Npc shark = Exterior.TownNpcs.FirstOrDefault(n => n.Interaction == G1C_InteractionType.LoanShark);
            if (shark == null || !Exterior.IsPresent(shark) || Player.IsMoving) return;
            int distance = Math.Abs(shark.X - Player.TileX) + Math.Abs(shark.Y - Player.TileY);
            if (distance > LoanSharkRange + 3) { LoanSharkApproached = false; return; }
            if (LoanSharkApproached || distance > LoanSharkRange) return;
            LoanSharkApproached = true;
            string text = Finance.LoanOwed > 0 ? "Hey! Denk an meine " + Finance.LoanOwed + " Chips!"
                        : Money.Balance < 50 ? "Psst! Knapp bei Kasse? 250 Chips, sofort!"
                        : "Psst! Brauchst du Chips? Ich hab ein Angebot ...";
            Npcs.Bubbles.ShowReaction(shark, text, TickCount);
            NotifySystemMessage(shark.Name + " spricht dich an: \"" + text + "\" [E] Mit ihm reden");
        }

        // Entwickler: Patrick Lupierz – Datum: 19.11.2026
        // Aufgabe: Ton am Ende eines Minispiels nach dem Ergebnis: Gewinn = Münzen (nach Höhe) + Sieg (Poker: eigener
        //          Sound), Verlust = Verlust-Ton. Der Spielautomat spielt seine Töne schon pro Drehung.
        private void PlayResultSound()
        {
            if (ActiveGame is G1C_Slots) return;
            int net = Money.Balance - _gameStartBalance;
            if (net > 0)
            {
                Sound.Play(net >= 500 ? "coins_big" : net >= 100 ? "coins_mid" : "coins_small");
                Sound.Play(ActiveGame is G1C_Poker ? "poker_win" : "win");
            }
            else Sound.Play(net < 0 ? "lose" : "click");
        }

        // Entwickler: Patrick Lupierz – Datum: 19.11.2026
        // Aufgabe: Geräusche der Spielwelt. Schritte: ein Geräusch pro abgeschlossenem Schritt, Untergrund nach Kachel
        //          (Gras, Weg, Brücke, innen), die Dateivarianten laufen reihum. Fahrrad: Schleife, solange du fährst.
        //          Vögel: nur im Umkreis (Einstellung Vogel-Radius, Standard 4 Kacheln). Katzen, Hunde und alle anderen
        //          Tiere machen nur beim Streicheln Geräusche (siehe PetAnimal).
        private int _lastStepCount;
        private int _bikeIdleTicks;

        private void UpdateWorldAudio()
        {
            bool exploring = State == G1C_GameState.MainGame && !IsSleeping;
            if (Player.StepCount != _lastStepCount)
            {
                _lastStepCount = Player.StepCount;
                if (exploring && !Bikes.Riding)
                    Sound.Play(G1C_StepSurface.SoundIdFor(CurrentWorld.Grid.GetTileSpriteId(Player.TileX, Player.TileY), CurrentLocation));
            }
            if (exploring && Bikes.Riding && Player.IsMoving) { _bikeIdleTicks = 0; Sound.StartLoop("bicycle"); }
            else
            {
                if (_bikeIdleTicks < 1000) _bikeIdleTicks++;
                if (_bikeIdleTicks == 4) Sound.StopLoop("bicycle");
            }
            if (exploring && CurrentLocation == G1C_WorldLocation.Exterior && TickCount % 25 == 0) UpdateBirdSounds();
        }

        public bool BirdInRange(G1C_WorldObject bird)
        {
            return bird != null && bird.SpriteId != null && bird.SpriteId.StartsWith("animal_bird", StringComparison.Ordinal) &&
                   G1C_StepSurface.InRadius(bird.X - Player.TileX, bird.Y - Player.TileY, Sound.Settings.BirdRadius);
        }

        private void UpdateBirdSounds()
        {
            G1C_Animal near = Exterior.Animals.FirstOrDefault(BirdInRange);
            if (near != null && Sound.Play("bird")) AnimalVoices.Say(near, "Tschilp!", TickCount);
        }

        // ------------------------------------------------------------------ Tick (alle 40 ms)

        // Erweiterung durch Denis Mock: Außenwelt-NPCs laufen ausschließlich im Framework-Tick.
        public void Update()
        {
            Sound.Pump(); // Patrick Lupierz – 19.11.2026: Befehle des Handy-Mixers, Lautstärken, Speichern
            Sound.SetMusic(G1C_SoundSystem.MusicFor(State, CurrentLocation, ActiveGame)); // Patrick Lupierz – 16.11.2026
            if (State == G1C_GameState.Title) return; // im Hauptmenü stehen Uhr und Spielzeit

            TickCount++;
            _movement.Update(HeldDirection); // Änderung durch: Patrick Lupierz – 15.11.2026 – WASD-Reparatur
            if (State != G1C_GameState.MainGame) _movement.ClearQueue();
            UpdateWorldAudio(); // Patrick Lupierz – 19.11.2026
            if (ActiveGame != null) ActiveGame.Update();

            // Änderung durch: Patrick Lupierz – zentrale Uhr, Effekte und SMS; im Pausenmenü steht die Zeit
            if (State != G1C_GameState.Menu && State != G1C_GameState.Map)
            {
                Clock.Advance();
                ApplyNewDay();
                Buffs.Update(Clock.TotalTicks);
                if (!Player.IsMoving)
                {
                    Player.StepTicks = Bikes.StepTicks(Buffs.StepTicks); // Patrick Lupierz – Fahrrad
                    if (Finance.LoanBonesBroken) Player.StepTicks = Math.Max(Player.StepTicks, 9);
                    // Änderung durch: Patrick Lupierz – 16.11.2026 – Admin-Cheat: dauerhaft erhöhte Geschwindigkeit (endet
                    // erst, wenn ADMIN erneut getippt wird); schneller als Fahrrad und Energydrink.
                    if (Cheats.JackpotActive) Player.StepTicks = Math.Min(Player.StepTicks, G1C_CheatCode.SpeedStepTicks);
                }
                Phone.HourlyUpdate(Clock, Profile.Name, Money.Balance, _random);
            }

            // Änderung durch: Eduard Schoenfeld – Datum: 12.11.2026 – NPC-Stimmungen, Monologe, Aufgaben-Gegenstände
            bool inCasino = CurrentLocation == G1C_WorldLocation.Casino;
            bool exploring = State == G1C_GameState.MainGame;
            Npcs.Update(TickCount, Player, inCasino, exploring);
            if (inCasino && exploring && !Player.IsMoving) Npcs.TryCollectItem(Player.TileX, Player.TileY);
            // Änderung durch: Patrick Lupierz – 18.11.2026 – Tagesablauf der Stadtbewohner, Geldhai spricht an
            if (TickCount % 25 == 0) G1C_NpcSchedule.Apply(Clock.Hour, Exterior, World, Player, CurrentLocation);
            if (CurrentLocation == G1C_WorldLocation.Exterior && exploring) UpdateLoanSharkApproach();
            if (CurrentLocation == G1C_WorldLocation.Exterior)
            {
                if (exploring) _exteriorNpcRoaming.Update(TickCount);
                foreach (G1C_Animal animal in Exterior.Animals) animal.UpdateAnimation(TickCount);
            }
            // Änderung durch: Patrick Lupierz – 15.11.2026 – Tiere in Innenräumen (Omas Katzen) bewegen sich auch
            else
            {
                foreach (G1C_Animal animal in CurrentWorld.Objects.OfType<G1C_Animal>()) animal.UpdateAnimation(TickCount);
            }
            // Änderung durch: Patrick Lupierz – 16.11.2026 – Katzen und Hunde laufen umher (nicht während Dialogen),
            // Tiere in der Nähe melden sich ab und zu mit ihrem typischen Laut.
            foreach (G1C_PetAnimal pet in CurrentWorld.Objects.OfType<G1C_PetAnimal>().ToList())
                pet.Update(TickCount, PetCanEnter, exploring ? _random : null);
            // Änderung durch: Patrick Lupierz – 19.11.2026 – Vögel melden sich nur im Vogel-Radius (Standard 4 Kacheln)
            AnimalVoices.Update(TickCount, CurrentWorld.Objects.Where(o => (o is G1C_Animal || o is G1C_PetAnimal) &&
                Math.Abs(o.X - Player.TileX) <= 8 && Math.Abs(o.Y - Player.TileY) <= 5 &&
                !(o.SpriteId != null && o.SpriteId.StartsWith("animal_bird", StringComparison.Ordinal) && !BirdInRange(o))),
                exploring ? _random : null);
        }
    }
}
