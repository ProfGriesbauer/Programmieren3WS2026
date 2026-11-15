using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace OOPGames
{
    public enum G1C_TestStatus { Bestanden, Fehlgeschlagen, NichtAusfuehrbar, NichtVorhanden }

    // Entwickler: Patrick Lupierz – Datum: 15.11.2026 – Ergebnis eines einzelnen automatisierten Tests
    public sealed class G1C_TestResult
    {
        public string Area { get; set; } = "";
        public string Name { get; set; } = "";
        public G1C_TestStatus Status { get; set; }
        public string Detail { get; set; } = "";

        public string StatusText
        {
            get
            {
                switch (Status)
                {
                    case G1C_TestStatus.Bestanden: return "BESTANDEN";
                    case G1C_TestStatus.Fehlgeschlagen: return "FEHLGESCHLAGEN";
                    case G1C_TestStatus.NichtAusfuehrbar: return "NICHT AUSFÜHRBAR";
                    default: return "NICHT VORHANDEN";
                }
            }
        }
    }

    // =====================================================
    // Klasse: G1C_SelfTest
    // Entwickler: Patrick Lupierz
    // Erstellt: 15.11.2026
    // Beschreibung: Automatisierte Logiktests des Casino-Spiels (Agents.md / Arbeitsauftrag Abschnitt 17).
    //               Läuft NUR auf ausdrücklichen Aufruf: im Spiel Menü [B] öffnen und [F12] drücken.
    //               Ein normaler Spielstart führt keinen Test aus. Die Tests benutzen ausschließlich die
    //               Spiellogik (keine eigene Game-Loop, kein eigenes Fenster) und schreiben zusätzlich einen
    //               Bericht "G1Casino_SelfTest.txt" neben die OOPGames.exe.
    //               Manuelle Prüfungen (Optik, Tastaturfokus, Animationsgefühl) sind hier NICHT enthalten.
    // =====================================================
    public static class G1C_SelfTest
    {
        public const string ReportFileName = "G1Casino_SelfTest.txt";

        private sealed class TestFailure : Exception
        {
            public TestFailure(string message) : base(message) { }
        }

        // Entwickler: Patrick Lupierz
        // Aufgabe: Alle Tests ausführen. Eine Ausnahme in einem Test beendet nur diesen Test.
        public static List<G1C_TestResult> RunAll()
        {
            List<G1C_TestResult> results = new List<G1C_TestResult>();
            Action<string, string, Action> run = (area, name, test) =>
            {
                G1C_TestResult result = new G1C_TestResult { Area = area, Name = name };
                try { test(); result.Status = G1C_TestStatus.Bestanden; }
                catch (TestFailure f) { result.Status = G1C_TestStatus.Fehlgeschlagen; result.Detail = f.Message; }
                catch (Exception e) { result.Status = G1C_TestStatus.NichtAusfuehrbar; result.Detail = e.GetType().Name + ": " + e.Message; }
                results.Add(result);
            };

            // ---- Bewegung (G1C_PlayerMovement, G1C_CollisionManager)
            run("Bewegung", "Ein Tastendruck = genau ein Schritt, danach Stillstand", MovementSingleStep);
            run("Bewegung", "Gehaltene Taste läuft ohne Pause weiter", MovementHeldContinues);
            run("Bewegung", "Wiederholte Taste läuft nach dem Loslassen nicht nach", MovementNoOvershoot);
            run("Bewegung", "Richtungswechsel während eines Schritts", MovementDirectionChange);
            run("Bewegung", "Wand und Bett blockieren, Figur dreht sich trotzdem", MovementCollision);
            run("Bewegung", "Keine Bewegung während eines Dialogs", MovementBlockedInDialogue);
            run("Bewegung", "Bäume, Gebäude, Pavillons und NPCs blockieren in der Stadt", MovementCityCollision);

            // ---- Grafiken
            run("Grafik", "Spielfigur: 4 Richtungen x 3 Frames vorhanden", GraphicsPlayerFrames);
            run("Grafik", "Alle Objekte aller Szenen haben eine Grafik", GraphicsAllSceneObjects);
            run("Grafik", "Neue IDs: sign_closed, Poster, Sticker, Mama, Oma", GraphicsNewIds);

            // ---- NPCs und Dialoge
            run("NPC", "Mama und Oma existieren, stehen in der Welt und sind im Handy", NpcFamilyPresent);
            run("NPC", "Mama und Oma haben eigene Fragen und Antworten", NpcFamilyIndividualTopics);
            run("NPC", "Jede Weltfigur hat ein Gesprächsprofil", NpcEveryWorldNpcHasProfile);
            run("NPC", "Gespräch mit Mama bietet Fragen an, Wiederholung ändert Antwort", NpcConversationTopics);
            run("NPC", "Taschengeld nur nach Gespräch mit beiden, einmal pro Tag", NpcPocketMoney);

            // ---- Gebäude und Geschäfte
            run("Geschäfte", "Öffnungszeiten der Läden, Tankstelle durchgehend", ShopHours);
            run("Geschäfte", "Geschlossener Laden: Tür zu, Hinweis, CLOSED-Liste", ShopClosedDoor);
            run("Geschäfte", "Kauf bucht Preis + 5 % ab und legt Item ins Inventar", ShopBuy);
            run("Geschäfte", "Verkäuferin: Gespräch, dann Warenliste; nach Ladenschluss kein Verkauf", ShopSellerConversation);
            run("Gebäude", "Schilder, Poster und Sticker sind untersuchbar", BuildingsInspectables);

            // ---- Casino
            run("Casino", "Roulette: Zahl 35:1, Rot 1:1, Null verliert Außenwette", CasinoRoulette);
            run("Casino", "Blackjack: Werte, Blackjack, Bust, Dealer zieht bis 16", CasinoBlackjack);
            run("Casino", "Poker: Handbewertung und Vergleich", CasinoPoker);
            run("Casino", "Spielautomat: Gewinnkombinationen", CasinoSlots);
            run("Casino", "Guthaben: keine negativen Werte, keine Überziehung", CasinoMoney);

            // ---- Inventar und Bank
            run("Inventar", "Einlagern und Entfernen", InventoryAddRemove);
            run("Bank", "Einzahlen und Abheben", BankDepositWithdraw);
            run("Bank", "2 % Zinsen pro Spieltag, kein Doppelzins", BankInterest);
            run("Bank", "Zinsen beim Tageswechsel im laufenden Spiel genau einmal", BankInterestInGame);

            // ---- Spielzustände
            run("Zustände", "Inventar und Handy öffnen und schließen", StatesMenus);

            WriteReport(results);
            return results;
        }

        // ------------------------------------------------------------------ Hilfen

        private static void Check(bool condition, string message)
        {
            if (!condition) throw new TestFailure(message);
        }

        private static void Equal<T>(T expected, T actual, string what)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
                throw new TestFailure(what + ": erwartet " + expected + ", erhalten " + actual);
        }

        private static G1C_PlayerMovement MovementAt(int x, int y, out G1C_Player player)
        {
            player = new G1C_Player(x, y, G1C_Direction.Down);
            return new G1C_PlayerMovement(player, new G1C_CollisionManager(new G1C_HomeWorld()));
        }

        private static void Ticks(G1C_PlayerMovement movement, int count, Func<G1C_Direction?> held = null)
        {
            for (int i = 0; i < count; i++) movement.Update(held);
        }

        // Laufendes Spiel ohne Hauptmenü (Spielstand eines neuen Spiels wird angewendet)
        private static G1C_GameManager RunningGame()
        {
            G1C_GameManager game = new G1C_GameManager();
            game.ApplySave(game.CreateSave());
            return game;
        }

        private static void SetHour(G1C_GameManager game, int day, int hour)
        {
            game.Clock.TotalTicks = (day - 1) * G1C_GameClock.TicksPerDay + hour * G1C_GameClock.TicksPerHour;
        }

        private static bool HasBuiltInSprite(string id)
        {
            return G1C_BuiltInSprites.Create(id) != null || G1C_TownSprites.Create(id) != null ||
                   G1C_WorldSprites.Create(id) != null || G1C_DecorSprites.Create(id) != null ||
                   G1C_NpcSprites.Create(id, new G1C_SpriteLibrary()) != null;
        }

        private static IEnumerable<IG1C_WorldScene> AllScenes(G1C_GameManager game)
        {
            return new IG1C_WorldScene[] { game.Home, game.Exterior, game.World, game.Hospital, game.Market, game.GasStation,
                                            game.ClothingStore, game.Bank, game.FurnitureStore };
        }

        // ------------------------------------------------------------------ Bewegung

        private static void MovementSingleStep()
        {
            G1C_Player p;
            G1C_PlayerMovement m = MovementAt(5, 8, out p);
            m.RequestStep(G1C_Direction.Right);
            Check(p.IsMoving, "Schritt wurde nicht gestartet");
            Ticks(m, p.StepTicks + 3);
            Equal(6, p.TileX, "TileX nach einem Schritt");
            Check(!p.IsMoving, "Figur läuft ohne gehaltene Taste weiter");
        }

        private static void MovementHeldContinues()
        {
            G1C_Player p;
            G1C_PlayerMovement m = MovementAt(3, 8, out p);
            m.RequestStep(G1C_Direction.Right);
            Ticks(m, p.StepTicks, () => G1C_Direction.Right);
            Check(p.IsMoving, "Nach dem ersten Schritt entsteht eine Pause");
            Equal(5, p.TileX, "Zielkachel des zweiten Schritts");
        }

        private static void MovementNoOvershoot()
        {
            G1C_Player p;
            G1C_PlayerMovement m = MovementAt(3, 8, out p);
            m.RequestStep(G1C_Direction.Right);
            m.RequestStep(G1C_Direction.Right); // Tastenwiederholung während des Schritts
            m.RequestStep(G1C_Direction.Right);
            Ticks(m, p.StepTicks * 3); // Taste losgelassen (keine gehaltene Richtung)
            Equal(4, p.TileX, "Figur ist nach dem Loslassen nachgelaufen");
        }

        private static void MovementDirectionChange()
        {
            G1C_Player p;
            G1C_PlayerMovement m = MovementAt(5, 8, out p);
            m.RequestStep(G1C_Direction.Right);
            m.RequestStep(G1C_Direction.Up);
            Ticks(m, p.StepTicks);
            Equal(G1C_Direction.Up, p.Facing, "Blickrichtung nach Richtungswechsel");
            Equal(7, p.TileY, "Zielkachel nach Richtungswechsel");
        }

        private static void MovementCollision()
        {
            G1C_Player p;
            G1C_PlayerMovement m = MovementAt(1, 1, out p);
            m.RequestStep(G1C_Direction.Up); // Wand
            Check(!p.IsMoving, "Figur läuft in die Wand");
            Equal(G1C_Direction.Up, p.Facing, "Figur dreht sich zur Wand");

            G1C_PlayerMovement bed = MovementAt(2, 3, out p);
            bed.RequestStep(G1C_Direction.Up); // Bett auf (2,2)
            Check(!p.IsMoving, "Figur läuft durch das Bett");
        }

        private static void MovementCityCollision()
        {
            G1C_ExteriorWorld city = new G1C_ExteriorWorld();
            G1C_CollisionManager collision = new G1C_CollisionManager(city);
            List<string> walkable = new List<string>();
            foreach (G1C_WorldObject obj in city.Objects.Where(o => o.Solid))
                for (int x = obj.X; x < obj.X + obj.Width; x++)
                    for (int y = obj.Y; y < obj.Y + obj.Height; y++)
                        if (!collision.IsBlocked(x, y) && !walkable.Contains(obj.Name)) walkable.Add(obj.Name);
            Check(walkable.Count == 0, "Begehbar trotz Kollision: " + string.Join(", ", walkable));
            Check(city.Objects.Any(o => o.Name == "Baum"), "Keine Bäume gefunden");
        }

        private static void MovementBlockedInDialogue()
        {
            G1C_GameManager game = RunningGame();
            SetHour(game, 1, 23);
            // Vor die Tür des (geschlossenen) Bekleidungsgeschäfts stellen -> Dialog
            G1C_ExteriorMap map = game.Exterior.Map;
            G1C_SaveData save = game.CreateSave();
            save.Location = (int)G1C_WorldLocation.Exterior;
            save.X = map.DoorX(G1C_WorldLocation.ClothingStore);
            save.Y = map.DoorY(G1C_WorldLocation.ClothingStore) + 1;
            save.Facing = (int)G1C_Direction.Up;
            game.ApplySave(save);
            game.HandleInput(G1C_InputEvent.FromKey(Key.E));
            Equal(G1C_GameState.Dialogue, game.State, "Zustand nach Klopfen an geschlossener Tür");
            int x = game.Player.TileX;
            int y = game.Player.TileY;
            game.HandleInput(G1C_InputEvent.FromKey(Key.S));
            for (int i = 0; i < 10; i++) game.Update();
            Check(game.Player.TileX == x && game.Player.TileY == y && !game.Player.IsMoving, "Figur bewegt sich im Dialog");
        }

        // ------------------------------------------------------------------ Grafik

        private static void GraphicsPlayerFrames()
        {
            List<string> missing = new List<string>();
            foreach (string dir in new[] { "up", "down", "left", "right" })
                for (int frame = 0; frame <= 2; frame++)
                    if (!HasBuiltInSprite("player_" + dir + "_" + frame)) missing.Add("player_" + dir + "_" + frame);
            Check(missing.Count == 0, "Fehlend: " + string.Join(", ", missing));
        }

        private static void GraphicsAllSceneObjects()
        {
            G1C_GameManager game = new G1C_GameManager();
            List<string> missing = new List<string>();
            foreach (IG1C_WorldScene scene in AllScenes(game))
                foreach (G1C_WorldObject obj in scene.Objects)
                    if (!HasBuiltInSprite(obj.SpriteId) && !missing.Contains(obj.SpriteId)) missing.Add(obj.SpriteId);
            Check(missing.Count == 0, "Ohne Grafik: " + string.Join(", ", missing));
        }

        private static void GraphicsNewIds()
        {
            string[] ids =
            {
                "sign_closed", "obj_poster_ad", "obj_sticker_bw",
                G1C_FamilyNpcRoster.MotherSkin + "_down_0", G1C_FamilyNpcRoster.GrandmaSkin + "_left_2",
                G1C_NpcSprites.PortraitId(G1C_FamilyNpcRoster.GrandmaSkin, G1C_NpcExpression.Happy)
            };
            string[] missing = ids.Where(id => !HasBuiltInSprite(id)).ToArray();
            Check(missing.Length == 0, "Fehlend: " + string.Join(", ", missing));
        }

        // ------------------------------------------------------------------ NPC

        private static void NpcFamilyPresent()
        {
            G1C_GameManager game = new G1C_GameManager();
            foreach (string name in new[] { G1C_FamilyNpcRoster.MotherName, G1C_FamilyNpcRoster.GrandmaName })
            {
                G1C_NpcCharacter c = game.Npcs.Find(name);
                Check(c != null, name + " fehlt im NPC-System");
                Check(game.Phone.Contacts.Contains(c), name + " fehlt im Handy");
            }
            Check(game.Home.Objects.OfType<G1C_Npc>().Any(n => n.Name == G1C_FamilyNpcRoster.MotherName), "Mama steht nicht im Zuhause");
            Check(game.Exterior.Objects.OfType<G1C_Npc>().Any(n => n.Name == G1C_FamilyNpcRoster.GrandmaName), "Oma steht nicht in der Stadt");
            Equal(3, game.Exterior.Objects.Count(o => o.Name == "Omas Katze"), "Anzahl Katzen der Oma");
        }

        private static void NpcFamilyIndividualTopics()
        {
            G1C_GameManager game = new G1C_GameManager();
            List<G1C_DialogueTopic> mother = game.Npcs.Find(G1C_FamilyNpcRoster.MotherName).Personality.Topics;
            List<G1C_DialogueTopic> grandma = game.Npcs.Find(G1C_FamilyNpcRoster.GrandmaName).Personality.Topics;
            Check(mother.Count >= 4 && grandma.Count >= 4, "Zu wenige Fragen");
            G1C_TopicContext context = new G1C_TopicContext { Money = new G1C_MoneyManager(), Hour = 12 };
            string a = mother.First(t => t.Question.Contains("Casino")).Ask(context);
            string b = grandma.First(t => t.Question.Contains("Casino")).Ask(context);
            Check(a != b, "Mama und Oma antworten identisch auf die Casino-Frage");
        }

        private static void NpcEveryWorldNpcHasProfile()
        {
            G1C_GameManager game = new G1C_GameManager();
            List<string> missing = new List<string>();
            foreach (IG1C_WorldScene scene in AllScenes(game))
                foreach (G1C_Npc npc in scene.Objects.OfType<G1C_Npc>())
                    if (game.Npcs.Find(npc.Name) == null && !missing.Contains(npc.Name)) missing.Add(npc.Name);
            Check(missing.Count == 0, "Ohne Profil (nur Standardtext): " + string.Join(", ", missing));
        }

        private static void NpcConversationTopics()
        {
            G1C_GameManager game = new G1C_GameManager();
            G1C_NpcCharacter mother = game.Npcs.Find(G1C_FamilyNpcRoster.MotherName);
            G1C_Conversation c = game.Npcs.Conversation;
            c.Open(mother, null);
            int index = c.Options.ToList().FindIndex(o => o.Text.StartsWith("Ich hab da eine Frage"));
            Check(index >= 0, "Option 'Ich hab da eine Frage' fehlt");
            c.Choose(index);
            int how = c.Options.ToList().FindIndex(o => o.Text.StartsWith("Wie geht's dir"));
            Check(how >= 0, "Frage 'Wie geht's dir, Mama?' fehlt");
            c.Choose(how);
            string first = c.Line;
            c.Choose(c.Options.ToList().FindIndex(o => o.Text.StartsWith("Wie geht's dir")));
            Check(first != c.Line && c.Line.Length > 0, "Wiederholte Frage liefert keine neue Antwort");
        }

        private static void NpcPocketMoney()
        {
            G1C_FamilyPocketMoney pocket = new G1C_FamilyPocketMoney();
            G1C_MoneyManager money = new G1C_MoneyManager(100);
            Equal((bool?)false, pocket.TryGive(1, money, new Random(1)), "ohne Gespräche");
            pocket.MarkTalk(true, 1);
            pocket.MarkTalk(false, 1);
            bool? first = pocket.TryGive(1, money, new FixedRandom(0.1));
            Equal((bool?)true, first, "nach Gespräch mit beiden (Glückswurf)");
            Equal(125, money.Balance, "Kontostand nach Taschengeld");
            Equal((bool?)null, pocket.TryGive(1, money, new FixedRandom(0.1)), "zweite Bitte am selben Tag");
            Equal(125, money.Balance, "kein doppeltes Taschengeld");
        }

        private sealed class FixedRandom : Random
        {
            private readonly double _value;
            public FixedRandom(double value) { _value = value; }
            public override double NextDouble() { return _value; }
        }

        // ------------------------------------------------------------------ Geschäfte

        private static void ShopHours()
        {
            Check(G1C_Shop.For(G1C_WorldLocation.ClothingStore).IsOpen(10), "Bekleidung um 10 Uhr zu");
            Check(!G1C_Shop.For(G1C_WorldLocation.ClothingStore).IsOpen(20), "Bekleidung um 20 Uhr offen");
            Check(!G1C_Shop.For(G1C_WorldLocation.Market).IsOpen(6), "Supermarkt um 6 Uhr offen");
            foreach (int hour in new[] { 0, 3, 12, 23 })
                Check(G1C_Shop.For(G1C_WorldLocation.GasStation).IsOpen(hour), "Tankstelle um " + hour + " Uhr zu");
        }

        private static void ShopClosedDoor()
        {
            G1C_GameManager game = RunningGame();
            SetHour(game, 1, 23);
            List<G1C_WorldLocation> closed = game.ClosedShops().ToList();
            Check(closed.Contains(G1C_WorldLocation.ClothingStore) && closed.Contains(G1C_WorldLocation.Market) &&
                  closed.Contains(G1C_WorldLocation.FurnitureStore), "Nicht alle Läden um 23 Uhr geschlossen");
            Check(!closed.Contains(G1C_WorldLocation.GasStation), "Tankstelle als geschlossen gemeldet");

            G1C_ExteriorMap map = game.Exterior.Map;
            G1C_SaveData save = game.CreateSave();
            save.Location = (int)G1C_WorldLocation.Exterior;
            save.X = map.DoorX(G1C_WorldLocation.ClothingStore);
            save.Y = map.DoorY(G1C_WorldLocation.ClothingStore) + 1;
            save.Facing = (int)G1C_Direction.Up;
            game.ApplySave(save);
            Check(game.CurrentHint.StartsWith("Geschlossen"), "Hinweis vor geschlossener Tür: '" + game.CurrentHint + "'");
            game.HandleInput(G1C_InputEvent.FromKey(Key.E));
            Equal(G1C_WorldLocation.Exterior, game.CurrentLocation, "Ort nach Klopfen um 23 Uhr");

            game.HandleInput(G1C_InputEvent.FromKey(Key.E)); // Dialog schließen
            while (game.State == G1C_GameState.Dialogue) game.HandleInput(G1C_InputEvent.FromKey(Key.E));
            SetHour(game, 2, 12);
            Check(!game.ClosedShops().Any(), "Läden um 12 Uhr geschlossen");
            game.HandleInput(G1C_InputEvent.FromKey(Key.E));
            Equal(G1C_WorldLocation.ClothingStore, game.CurrentLocation, "Ort nach Betreten um 12 Uhr");
        }

        private static void ShopSellerConversation()
        {
            G1C_GameManager game = RunningGame();
            SetHour(game, 1, 12);
            G1C_SaveData save = game.CreateSave();
            save.Location = (int)G1C_WorldLocation.Market;
            save.X = 10; save.Y = 5; save.Facing = (int)G1C_Direction.Right; // Kassiererin Jana steht auf (11,5)
            game.ApplySave(save);
            Check(game.Player.TileX == 10 && game.Player.TileY == 5, "Testposition vor der Kasse nicht begehbar");
            game.HandleInput(G1C_InputEvent.FromKey(Key.E));
            Equal(G1C_GameState.Conversation, game.State, "Zustand nach Ansprechen der Kassiererin");
            Check(game.Npcs.Conversation.Options.Any(o => o.Text.StartsWith("Ich hab da eine Frage")), "Verkäuferin ohne eigene Fragen");
            game.HandleInput(G1C_InputEvent.FromKey(Key.D1)); // "Ich möchte einkaufen."
            Equal(G1C_GameState.ListMenu, game.State, "Warenliste nach 'einkaufen'");

            int before = game.Money.Balance;
            SetHour(game, 1, 22); // Supermarkt schließt um 21 Uhr, Menü ist noch offen
            game.HandleInput(G1C_InputEvent.FromKey(Key.E));
            Equal(before, game.Money.Balance, "Kauf nach Ladenschluss bei offenem Menü");
        }

        private static void BuildingsInspectables()
        {
            G1C_GameManager game = RunningGame();
            Func<IG1C_WorldScene, string, bool> hasText = (scene, text) => scene.Objects.Any(o =>
                o.Interaction == G1C_InteractionType.Inspect && o.Lines != null && o.Lines.Any(l => l.Contains(text)));
            Check(hasText(game.Home, "Hier könnte Ihre Werbung stehen"), "Werbeposter fehlt");
            Check(hasText(game.GasStation, "Nett hier, aber waren Sie schon mal in Baden-Württemberg"), "Sticker fehlt");
            int signs = game.Exterior.Objects.Count(o => o.SpriteId == "obj_sign_post" && o.Interaction == G1C_InteractionType.Inspect);
            Check(signs >= 3, "Nur " + signs + " Schilder in der Stadt");

            G1C_SaveData save = game.CreateSave();
            save.Location = (int)G1C_WorldLocation.Home;
            save.X = 9; save.Y = 1; save.Facing = (int)G1C_Direction.Up; // Poster hängt auf (9,0)
            game.ApplySave(save);
            Check(game.CurrentHint.Contains("ansehen"), "Kein Hinweis vor dem Poster: '" + game.CurrentHint + "'");
            game.HandleInput(G1C_InputEvent.FromKey(Key.E));
            Equal(G1C_GameState.Dialogue, game.State, "Zustand nach Untersuchen des Posters");
        }

        private static void ShopBuy()
        {
            G1C_MoneyManager money = new G1C_MoneyManager(100);
            G1C_Inventory inventory = new G1C_Inventory();
            G1C_Item water = G1C_ItemCatalog.Get("water");
            G1C_Shop.Buy(water, money, inventory);
            Equal(100 - G1C_Shop.PriceWithMarkup(water.Price), money.Balance, "Kontostand nach Kauf");
            Equal(1, inventory.Count("water"), "Wasser im Inventar");
            Equal(105, G1C_Shop.PriceWithMarkup(100), "Aufschlag 5 %");
            G1C_MoneyManager poor = new G1C_MoneyManager(0);
            G1C_Shop.Buy(water, poor, inventory);
            Equal(0, poor.Balance, "Kauf ohne Geld");
            Equal(1, inventory.Count("water"), "Item ohne Bezahlung erhalten");
        }

        // ------------------------------------------------------------------ Casino

        private static void CasinoRoulette()
        {
            Equal(360, G1C_Roulette.CreateBet(17, 10).TotalReturn(17), "Zahl 17 trifft (inkl. Einsatz)");
            Equal(0, G1C_Roulette.CreateBet(17, 10).TotalReturn(18), "Zahl 17 verfehlt");
            G1C_RouletteBet red = new G1C_RouletteBet(G1C_RouletteBetType.Red, 0, 10);
            Equal(20, red.TotalReturn(1), "Rot auf 1");
            Equal(0, red.TotalReturn(2), "Rot auf 2 (schwarz)");
            Equal(0, red.TotalReturn(0), "Rot auf Null");
            Equal(30, new G1C_RouletteBet(G1C_RouletteBetType.Dozen3, 0, 10).TotalReturn(36), "3. Dutzend 2:1");
            Equal(37, G1C_RouletteTable.WheelOrder.Length, "Felder am europäischen Rad");
        }

        private static G1C_BlackjackHand Hand(params int[] ranks)
        {
            G1C_BlackjackHand hand = new G1C_BlackjackHand();
            foreach (int rank in ranks) hand.Add(new G1C_Card(rank, G1C_Suit.Spades));
            return hand;
        }

        private static void CasinoBlackjack()
        {
            Check(Hand(14, 13).IsBlackjack, "Ass + König ist kein Blackjack");
            Equal(21, Hand(14, 14, 9).Value, "Ass + Ass + 9");
            Equal(12, Hand(14, 14).Value, "Ass + Ass");
            Check(Hand(13, 12, 5).IsBust, "König + Dame + 5 nicht bust");
            Check(!Hand(7, 7, 7).IsBlackjack, "7-7-7 als Blackjack gewertet");
            Check(G1C_Blackjack.DealerShouldHit(Hand(10, 6)), "Dealer hält bei 16");
            Check(!G1C_Blackjack.DealerShouldHit(Hand(10, 7)), "Dealer zieht bei 17");
        }

        private static List<G1C_Card> Cards(params string[] cards)
        {
            return cards.Select(c => new G1C_Card(int.Parse(c.Substring(0, c.Length - 1)),
                c.EndsWith("h") ? G1C_Suit.Hearts : c.EndsWith("d") ? G1C_Suit.Diamonds : c.EndsWith("c") ? G1C_Suit.Clubs : G1C_Suit.Spades)).ToList();
        }

        private static void CasinoPoker()
        {
            G1C_PokerHandValue royal = G1C_PokerHandEvaluator.EvaluateBest(Cards("10h", "11h", "12h", "13h", "14h", "2c", "3d"));
            Equal(G1C_PokerCategory.StraightFlush, royal.Category, "Royal Flush");
            G1C_PokerHandValue quads = G1C_PokerHandEvaluator.EvaluateBest(Cards("9h", "9d", "9c", "9s", "2h"));
            Equal(G1C_PokerCategory.FourOfAKind, quads.Category, "Vierling");
            Check(royal.CompareTo(quads) > 0, "Straight Flush schlägt Vierling nicht");
            Equal(G1C_PokerCategory.Straight, G1C_PokerHandEvaluator.EvaluateBest(Cards("14h", "2d", "3c", "4s", "5h")).Category, "Ass-5-Straße");
            G1C_PokerHandValue twoPair = G1C_PokerHandEvaluator.EvaluateBest(Cards("8h", "8d", "4c", "4s", "13h"));
            G1C_PokerHandValue pair = G1C_PokerHandEvaluator.EvaluateBest(Cards("14h", "14d", "4c", "7s", "13h"));
            Check(twoPair.CompareTo(pair) > 0, "Zwei Paare schlagen ein Paar nicht");
            G1C_PokerHandValue pairKing = G1C_PokerHandEvaluator.EvaluateBest(Cards("14h", "14d", "4c", "7s", "13h"));
            G1C_PokerHandValue pairQueen = G1C_PokerHandEvaluator.EvaluateBest(Cards("14c", "14s", "4d", "7h", "12h"));
            Check(pairKing.CompareTo(pairQueen) > 0, "Beikarte entscheidet nicht");
        }

        private static void CasinoSlots()
        {
            Equal(100, G1C_Slots.GetMultiplier(new[] { G1C_SlotSymbol.Seven, G1C_SlotSymbol.Seven, G1C_SlotSymbol.Seven }), "777");
            Equal(5, G1C_Slots.GetMultiplier(new[] { G1C_SlotSymbol.Seven, G1C_SlotSymbol.Bell, G1C_SlotSymbol.Seven }), "zwei Siebener");
            Equal(2, G1C_Slots.GetMultiplier(new[] { G1C_SlotSymbol.Cherry, G1C_SlotSymbol.Cherry, G1C_SlotSymbol.Bar }), "zwei Kirschen");
            Equal(0, G1C_Slots.GetMultiplier(new[] { G1C_SlotSymbol.Lemon, G1C_SlotSymbol.Bell, G1C_SlotSymbol.Bar }), "keine Kombination");
        }

        private static void CasinoMoney()
        {
            G1C_MoneyManager money = new G1C_MoneyManager(50);
            Check(!money.TryWithdraw(60), "Überziehung erlaubt");
            Check(!money.TryWithdraw(0) && !money.TryWithdraw(-5), "Einsatz 0/negativ akzeptiert");
            Equal(50, money.Balance, "Kontostand nach abgelehnten Einsätzen");
            bool threw = false;
            try { money.Deposit(-1); } catch (ArgumentOutOfRangeException) { threw = true; }
            Check(threw, "Negative Gutschrift akzeptiert");
        }

        // ------------------------------------------------------------------ Inventar und Bank

        private static void InventoryAddRemove()
        {
            G1C_Inventory inventory = new G1C_Inventory();
            inventory.Add("apple", 2);
            Equal(2, inventory.Count("apple"), "Äpfel nach Einlagern");
            Check(inventory.Remove("apple"), "Entfernen fehlgeschlagen");
            Equal(1, inventory.Count("apple"), "Äpfel nach Entfernen");
            Check(!inventory.Remove("water"), "Nicht vorhandenes Item entfernt");
        }

        private static void BankDepositWithdraw()
        {
            G1C_MoneyManager money = new G1C_MoneyManager(300);
            G1C_FinanceManager bank = new G1C_FinanceManager();
            Check(bank.Deposit(money, 200), "Einzahlung abgelehnt");
            Equal(100, money.Balance, "Bargeld nach Einzahlung");
            Equal(200, bank.Savings, "Konto nach Einzahlung");
            Check(!bank.Withdraw(money, 250), "Abhebung über Guthaben erlaubt");
            Check(!bank.Deposit(money, 500), "Einzahlung ohne Bargeld erlaubt");
            Check(bank.Withdraw(money, 50), "Abhebung abgelehnt");
            Equal(150, money.Balance, "Bargeld nach Abhebung");
            Equal(150, bank.Savings, "Konto nach Abhebung");
        }

        private static void BankInterest()
        {
            Equal(20, G1C_FinanceManager.InterestFor(1000), "Zins auf 1000");
            G1C_MoneyManager money = new G1C_MoneyManager(1000);
            G1C_FinanceManager bank = new G1C_FinanceManager();
            bank.RestoreInterest(0, 0, 1);
            bank.Deposit(money, 1000);
            Equal(20, bank.ApplyDailyInterest(2), "Zinsen für Tag 2");
            Equal(1020, bank.Savings, "Kontostand nach einem Tag");
            Equal(0, bank.ApplyDailyInterest(2), "zweite Zinszahlung am selben Tag");
            Equal(0, bank.ApplyDailyInterest(1), "Zinsen für einen vergangenen Tag");
            Equal(20, bank.ApplyDailyInterest(3), "Zinsen für Tag 3 (1020 * 2 % abgerundet)");
            Equal(1040, bank.Savings, "Kontostand nach zwei Tagen");
            Equal(0, money.Balance, "Zinsen dürfen nicht aufs Bargeld gehen");
        }

        private static void BankInterestInGame()
        {
            G1C_GameManager game = RunningGame();
            game.Finance.Deposit(game.Money, 500);
            SetHour(game, 2, 0);
            game.Clock.TotalTicks -= 1; // kurz vor Mitternacht Tag 1 -> nächster Tick ist Tag 2
            game.Update();
            Equal(510, game.Finance.Savings, "Konto nach Mitternacht");
            for (int i = 0; i < 20; i++) game.Update();
            Equal(510, game.Finance.Savings, "Konto nach weiteren Ticks am selben Tag");
            G1C_GameManager loaded = new G1C_GameManager();
            loaded.ApplySave(game.CreateSave());
            loaded.Update();
            Equal(510, loaded.Finance.Savings, "Konto nach Speichern/Laden am selben Tag");
        }

        // ------------------------------------------------------------------ Zustände

        private static void StatesMenus()
        {
            G1C_GameManager game = RunningGame();
            Equal(G1C_GameState.MainGame, game.State, "Start nach Laden");
            game.HandleInput(G1C_InputEvent.FromKey(Key.I));
            Equal(G1C_GameState.ListMenu, game.State, "Inventar öffnen");
            game.HandleInput(G1C_InputEvent.FromKey(Key.Escape));
            Equal(G1C_GameState.MainGame, game.State, "Inventar schließen");
            game.HandleInput(G1C_InputEvent.FromKey(Key.P));
            Equal(G1C_GameState.Phone, game.State, "Handy öffnen");
            game.HandleInput(G1C_InputEvent.FromKey(Key.P));
            Equal(G1C_GameState.MainGame, game.State, "Handy schließen");
            game.HandleInput(G1C_InputEvent.FromKey(Key.B));
            Equal(G1C_GameState.Menu, game.State, "Menü öffnen");
            game.HandleInput(G1C_InputEvent.FromKey(Key.B));
            Equal(G1C_GameState.MainGame, game.State, "Menü schließen");
        }

        // ------------------------------------------------------------------ Bericht

        // Entwickler: Patrick Lupierz – Bericht neben die exe schreiben; Schreibfehler stören das Spiel nicht.
        private static void WriteReport(List<G1C_TestResult> results)
        {
            try
            {
                List<string> lines = new List<string>
                {
                    "G1C Selbsttest – " + DateTime.Now.ToString("dd.MM.yyyy HH:mm"),
                    "Bestanden: " + results.Count(r => r.Status == G1C_TestStatus.Bestanden) + " / " + results.Count,
                    ""
                };
                lines.AddRange(results.Select(r => "[" + r.StatusText + "] " + r.Area + " – " + r.Name +
                                                   (string.IsNullOrEmpty(r.Detail) ? "" : " – " + r.Detail)));
                File.WriteAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ReportFileName), lines);
            }
            catch (Exception)
            {
                // Bericht ist optional; Ergebnisse werden zusätzlich im Spiel angezeigt.
            }
        }
    }
}
