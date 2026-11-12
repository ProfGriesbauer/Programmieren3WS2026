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
        private G1C_InteractionType _activeGameType; // Änderung durch: Eduard Schoenfeld – für NPC-Reaktionen

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

        public G1C_GameClock Clock { get; } = new G1C_GameClock();
        public G1C_PlayerProfile Profile { get; private set; } = new G1C_PlayerProfile();
        public G1C_Inventory Inventory { get; } = new G1C_Inventory();
        public G1C_BuffSystem Buffs { get; } = new G1C_BuffSystem();
        public G1C_StartFlow Start { get; } = new G1C_StartFlow();
        public G1C_ListMenu ListMenu { get; } = new G1C_ListMenu();
        public G1C_Phone Phone { get; }

        // Änderung durch: Denis Mock – Ein neues Spiel startet im Zuhause.
        public G1C_GameManager()
        {
            _sceneTransitions = new G1C_SceneTransitionManager(Home, Exterior, World, Hospital, Market, GasStation, ClothingStore);
            Player = new G1C_Player(Home.PlayerStartX, Home.PlayerStartY, G1C_Direction.Down);
            _movement = CreateMovement();
            _interactions = new G1C_InteractionManager(World);
            Npcs = new G1C_NpcSystem(World, Money, _random); // Änderung durch: Eduard Schoenfeld
            _exteriorNpcRoaming = new G1C_ExteriorNpcRoamingSystem(Exterior, Player, _random); // Erweiterung durch Denis Mock
            Phone = new G1C_Phone(Npcs);
        }

        // Änderung durch: Denis Mock – Türhinweise richten sich nach der aktuell aktiven Welt.
        public string CurrentHint
        {
            get
            {
                if (State != G1C_GameState.MainGame) return "";
                string transitionHint = _sceneTransitions.GetHint(Player);
                if (!string.IsNullOrEmpty(transitionHint)) return transitionHint;

                G1C_WorldObject target = FindTarget();
                if (target == null) return "";
                switch (target.Interaction)
                {
                    case G1C_InteractionType.Trash: return "[E] Mülleimer durchsuchen";
                    case G1C_InteractionType.Shop: return "[E] Einkaufen";
                    case G1C_InteractionType.Wardrobe: return "[E] Umziehen";
                    case G1C_InteractionType.Bed: return "[E] Schlafen";
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
                       State == G1C_GameState.Phone;
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
            return CurrentLocation == G1C_WorldLocation.Casino
                ? _interactions.FindTarget(Player)
                : CurrentWorld.ObjectAt(Player.FacingTileX, Player.FacingTileY);
        }

        // ------------------------------------------------------------------ Eingaben

        public void HandleInput(G1C_InputEvent input)
        {
            if (input == null) return;
            bool isKey = input.Kind == G1C_InputKind.Key;

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
                    if (isKey) HandleWorldKey(input.Key);
                    break;

                case G1C_GameState.Dialogue:
                    // Ein Klick blättert wie [E] weiter
                    Dialogue.HandleKey(isKey ? input.Key : Key.E);
                    if (!Dialogue.IsOpen && State == G1C_GameState.Dialogue) State = G1C_GameState.MainGame;
                    break;

                case G1C_GameState.Menu:
                    if (!isKey || Menu.ShouldClose(input.Key)) State = G1C_GameState.MainGame;
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
            else if (key == Key.M)
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

        // Änderung durch: Denis Mock – Türen wechseln die aktive Szene; Casinointeraktionen bleiben erhalten.
        // Änderung durch Denis Mock: Stadtbewohner benutzen dasselbe Gesprächssystem wie Casino-NPCs.
        // Änderung durch: Patrick Lupierz – Mülleimer, Läden, Kleiderschrank und Bett in allen Szenen.
        private void Interact()
        {
            if (Player.IsMoving) return;

            if (_sceneTransitions.TryTransition(Player))
            {
                _movement.ClearQueue();
                _movement = CreateMovement();
                Npcs.Bubbles.Clear();
                return;
            }

            G1C_WorldObject target = FindTarget();
            if (target == null) return;
            target.OnInteract(Player);

            switch (target.Interaction)
            {
                case G1C_InteractionType.Trash: SearchTrash(target); return;
                case G1C_InteractionType.Shop: OpenShop(); return;
                case G1C_InteractionType.Wardrobe: OpenWardrobe(); return;
                case G1C_InteractionType.Bed:
                    OpenDialogue("Bett", new[] { "Bis zum nächsten Morgen (06:00 Uhr) schlafen?" }, Clock.SkipToMorning);
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

            if (CurrentLocation != G1C_WorldLocation.Casino) return;

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

        // ------------------------------------------------------------------ Mülleimer, Läden, Inventar

        // Entwickler: Patrick Lupierz
        private void SearchTrash(G1C_WorldObject can)
        {
            string found = _trash.Search(G1C_TrashSystem.KeyOf(CurrentLocation, can), Clock.Day, _random, Money, Inventory);
            if (found == null)
            {
                OpenDialogue("Mülleimer", new[] { "Hier hast du heute schon gesucht.\nMorgen liegt vielleicht wieder etwas drin." }, null);
                return;
            }
            _trash.NotifyWitnesses(CurrentWorld, Player, Npcs, Clock.Day);
            OpenDialogue("Mülleimer", new[] { found }, null);
        }

        // Entwickler: Patrick Lupierz
        private void OpenShop()
        {
            G1C_Shop shop = G1C_Shop.For(CurrentLocation);
            if (shop == null) return;
            if (!shop.IsOpen(Clock.Hour))
            {
                OpenDialogue(shop.Name, new[] { "Wir haben geschlossen.\nÖffnungszeiten: " + shop.OpenFrom + " bis " + shop.OpenUntil + " Uhr." }, null);
                return;
            }
            ListMenu.Open(shop.Name, () => ShopRows(shop));
            State = G1C_GameState.ListMenu;
        }

        private List<G1C_MenuRow> ShopRows(G1C_Shop shop)
        {
            List<G1C_MenuRow> rows = new List<G1C_MenuRow>();
            foreach (G1C_Item item in shop.ItemIds.Select(G1C_ItemCatalog.Get))
            {
                G1C_Item current = item;
                bool owned = item.Slot != G1C_ClothingSlot.None && Inventory.Has(item.Id);
                rows.Add(new G1C_MenuRow
                {
                    Text = item.Name, Right = owned ? "gekauft" : item.Price + " Chips", Detail = item.Description,
                    Run = () => ListMenu.Message = G1C_Shop.Buy(current, Money, Inventory)
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

        // Entwickler: Patrick Lupierz
        private void OpenInventory()
        {
            ListMenu.Open("Inventar", InventoryRows);
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
                    Right = worn ? "angezogen" : (item.IsUsable ? "[E] benutzen" : item.Category.ToString()),
                    Detail = item.Description, Run = () => UseItem(current)
                });
            }
            return rows;
        }

        private void UseItem(G1C_Item item)
        {
            if (!item.IsUsable) { ListMenu.Message = item.Description; return; }
            if (Inventory.Remove(item.Id)) ListMenu.Message = Buffs.Apply(item, Clock.TotalTicks);
        }

        // Entwickler: Patrick Lupierz – Kleiderschrank/Spiegel: gekaufte Kleidung sichtbar anziehen
        private void OpenWardrobe()
        {
            ListMenu.Open("Kleiderschrank", WardrobeRows);
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
                        ? "Gespeichert auf Platz " + request.Slot + "."
                        : "Speichern fehlgeschlagen!";
                    Phone.RefreshSlots();
                    break;
            }
        }

        // ------------------------------------------------------------------ Neues Spiel, Speichern, Laden

        // Entwickler: Patrick Lupierz
        private void BeginNewGame()
        {
            Profile = Start.Profile;
            foreach (string clothing in new[] { Profile.Top, Profile.Pants, Profile.Shoes }) Inventory.Add(clothing);
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
                ClockTicks = Clock.TotalTicks, PlayTicks = TickCount, GamesPlayed = GamesPlayed, Reputation = Npcs.Reputation,
                Npcs = Npcs.Characters.Select(c => new G1C_SaveNpc
                {
                    Name = c.Name, Trust = c.Relationship.Trust, Respect = c.Relationship.Respect, Status = (int)c.Relationship.Status,
                    Purse = c.Purse, Mood = (int)c.Mood, Memory = c.Memory
                }).ToList(),
                Tasks = Npcs.Tasks.Tasks.Select(t => t.Capture()).ToList(),
                Messages = Phone.Messages, Trash = _trash.SearchedOnDay
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
            Buffs.Restore(data.Buffs);
            Clock.TotalTicks = data.ClockTicks;
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

            _sceneTransitions.Restore((G1C_WorldLocation)data.Location);
            IG1C_WorldScene scene = CurrentWorld;
            bool free = scene.Grid.IsWalkable(data.X, data.Y) && scene.ObjectAt(data.X, data.Y) == null;
            if (free) Player.Teleport(data.X, data.Y, (G1C_Direction)data.Facing);
            else Player.Teleport(scene.PlayerStartX, scene.PlayerStartY, G1C_Direction.Down);
            _movement = CreateMovement();
            Player.Skin = Profile.SkinId;
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
            if (State == G1C_GameState.Title) return; // im Hauptmenü stehen Uhr und Spielzeit

            TickCount++;
            _movement.Update();
            if (State != G1C_GameState.MainGame) _movement.ClearQueue();
            if (ActiveGame != null) ActiveGame.Update();

            // Änderung durch: Patrick Lupierz – zentrale Uhr, Effekte und SMS; im Pausenmenü steht die Zeit
            if (State != G1C_GameState.Menu)
            {
                Clock.Advance();
                Buffs.Update(Clock.TotalTicks);
                if (!Player.IsMoving) Player.StepTicks = Buffs.StepTicks;
                Phone.HourlyUpdate(Clock, Profile.Name, Money.Balance, _random);
            }

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
