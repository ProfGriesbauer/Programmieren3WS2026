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
            run("NPC", "Taschengeld: einmal pro Tag, danach Nein; am nächsten Tag wieder", NpcPocketMoney);
            run("NPC", "Mama und Oma werden auch nach Beleidigungen nie Gegner", NpcFamilyNeverHostile);

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

            // ---- Erweiterung 15.11.2026: Pavillon, Bank, Fahrrad, Spielhalle
            run("Gebäude", "Pavillon: von jeder Seite betretbar, nur Ecksäulen massiv", BuildingsPavilion);
            run("Bank", "Bank hat Bankierin, Berater und Wachmann mit eigenen Themen", BankStaff);
            run("Fahrrad", "Startfahrrad, Auf-/Absteigen nur draußen, schneller, Absteigen im Gebäude", BikeRiding);
            run("Fahrrad", "Fahrradladen: alle Farben ausgestellt, Kauf nur einmal, Ladenschluss", BikeShopBuy);
            run("Spielhalle", "5 Automaten, max. 9 Spiele, Gast-Automat, Greifautomat umgezogen", ArcadeLayout);
            run("Spielhalle", "Einsatz 5 Chips, Belohnung genau einmal (Pong-Sieg gegen KI)", ArcadeFeeAndReward);
            run("Spielhalle", "Sternenjäger: Treffer zählt, Schiff verliert Leben", ArcadeStarHunter);
            run("Spielhalle", "Schlange: Frucht verlängert, Wand beendet", ArcadeSnake);
            run("Spielhalle", "Ziegelbrecher: Stein zerstört, 3 verlorene Bälle beenden", ArcadeBrickBreaker);
            run("Spielhalle", "Pong-KI berechnet Auftreffpunkt mit Wandabpraller", ArcadePongAi);
            run("Spielhalle", "Gast-Automat: fremdes Framework-Spiel finden, spielen, zeichnen, belohnen", ArcadeGuestGame);
            run("Spielhalle", "Automat im Spiel starten, Ergebnis, Rekord gespeichert", ArcadeInGameFlow);

            // ---- Erweiterung 15.11.2026 (3. Schritt): Omas Haus, Tankstelle, Innenräume, Vögel, Schlafen
            run("NPC", "Omas Haus ist betretbar, Oma und Katzen wohnen darin", NpcGrandmaHouse);
            run("Geschäfte", "Tankstelle: rot, zwei Zapfsäulen, Einkauf durchs Fenster (auch nachts)", ShopGasStationWindow);
            run("Grafik", "Innenwände links/rechts/unten und Teppichteile haben eigene Grafiken", GraphicsWallsAndRugs);
            run("Grafik", "Jeder Innenraum hat ein eigenes, untersuchbares Wandbild", GraphicsWallPictures);
            run("Grafik", "Vögel sitzen oben auf dem Baum (hohe Grafik, unten transparent)", GraphicsBirds);
            run("Zustände", "Schlafen: Schlafzustand, Steuerung gesperrt, danach wieder frei", StatesSleeping);

            // ---- Erweiterung 16.11.2026: neue SprintPlanning-Punkte (Möbel, Tiere, Sound, Fahrrad, Gespräche, Angeln,
            //      Tankstelle, Poker, Cheat, Casino-Schild, Pavillon)
            run("Möbel", "Kauflimit: höchstens 3 Stück je Möbel (Inventar + aufgestellt)", FurnitureLimit);
            run("Möbel", "Einrichtungsmodus: per Klick aufstellen, verschieben, einlagern, abbrechen", FurnitureEditorFlow);
            run("Tiere", "Katzen (5 Fellarten) und Hunde: Grafik, massiv, laufen im Revier umher", AnimalsPetsRoam);
            run("Tiere", "Streicheln: Herz-Blase und Meldung; Laute je Tierart; Enten massiv", AnimalsPettingAndVoices);
            run("Sound", "Musik je Ort, Geräusche bei Aktionen, WAV-Erzeugung, stumm im Test", SoundLogic);
            run("Fahrrad", "Grafik in alle 4 Richtungen, links = gespiegelt rechts", BikeDirections);
            run("Fahrrad", "Haus betreten/verlassen: Rad wartet vor der Tür, kein zweites Anklicken", BikeEnterLeaveBuilding);
            run("Gespräche", "Lob/Spitze passend zur Rolle, Familie ohne Beleidigung, mehr Themen", ConversationSmallTalk);
            run("Angeln", "Angeln auf den Brücken: erreichbar, startet das Angelspiel", FishingSpots);
            run("Angeln", "Drill: grüner Bereich treibt gegen den Fisch, Fang landet im Inventar und ist verkaufbar", FishingCatch);
            run("Angeln", "Drill: ohne Gegensteuern reißt sich der Fisch los", FishingEscape);
            run("Casino", "Admin-Cheat: ADMIN im Spielautomaten tippen schaltet Jackpot-Garantie an/aus: Jackpot garantiert, Münzregen nach Einsatz", CasinoCheatJackpot);
            run("Casino", "Poker: eigene Hand tiefer, gefächert, 5 % größer", CasinoPokerFan);
            run("Casino", "Umzugsschild des Greifautomaten entfernt", CasinoClawSignRemoved);
            run("Geschäfte", "Tankstelle: Retro-Look, breites Fenster, Steinplatten unter den Zapfsäulen", ShopGasStationLook);

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
                   G1C_NpcSprites.Create(id, new G1C_SpriteLibrary()) != null ||
                   G1C_AnimalSprites.Create(id) != null; // Patrick Lupierz – 16.11.2026
        }

        private static IEnumerable<IG1C_WorldScene> AllScenes(G1C_GameManager game)
        {
            return new IG1C_WorldScene[] { game.Home, game.Exterior, game.World, game.Hospital, game.Market, game.GasStation,
                                            game.ClothingStore, game.Bank, game.FurnitureStore, game.Arcade, game.BikeShop,
                                            game.GrandmaHouse };
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
            Check(game.GrandmaHouse.Objects.OfType<G1C_Npc>().Any(n => n.Name == G1C_FamilyNpcRoster.GrandmaName), "Oma ist nicht in ihrem Haus");
            Check(!game.Exterior.Objects.OfType<G1C_Npc>().Any(n => n.Name == G1C_FamilyNpcRoster.GrandmaName), "Oma läuft draußen herum");
            // Änderung durch: Patrick Lupierz – 16.11.2026 – Omas Katzen sind jetzt streichelbare Haustiere
            Equal(3, game.GrandmaHouse.Objects.OfType<G1C_PetAnimal>().Count(p => p.Kind == G1C_PetKind.Cat), "Anzahl Katzen der Oma");
            Check(game.Exterior.Objects.Any(o => o.SpriteId == "building_grandma"), "Omas Haus fehlt in der Stadt");
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

        private static int AskPocketMoney(G1C_GameManager game, string name)
        {
            G1C_Conversation c = game.Npcs.Conversation;
            c.Open(game.Npcs.Find(name), null);
            int index = c.Options.ToList().FindIndex(o => o.Text.StartsWith("Hast du Taschengeld"));
            Check(index >= 0, name + ": Option 'Hast du Taschengeld für mich?' fehlt");
            int before = game.Money.Balance;
            c.Choose(index);
            c.Close();
            return game.Money.Balance - before;
        }

        private static void NpcPocketMoney()
        {
            G1C_GameManager game = RunningGame();
            SetHour(game, 1, 12);
            Equal(25, AskPocketMoney(game, G1C_FamilyNpcRoster.MotherName), "Mama, erste Bitte");
            Equal(0, AskPocketMoney(game, G1C_FamilyNpcRoster.MotherName), "Mama, zweite Bitte am selben Tag");
            Equal(20, AskPocketMoney(game, G1C_FamilyNpcRoster.GrandmaName), "Oma, erste Bitte");
            Equal(0, AskPocketMoney(game, G1C_FamilyNpcRoster.GrandmaName), "Oma, zweite Bitte am selben Tag");

            G1C_GameManager loaded = new G1C_GameManager(); // Speichern/Laden darf nicht erneut auszahlen
            loaded.ApplySave(game.CreateSave());
            Equal(0, AskPocketMoney(loaded, G1C_FamilyNpcRoster.MotherName), "Mama nach Laden am selben Tag");
            SetHour(loaded, 2, 9);
            Equal(25, AskPocketMoney(loaded, G1C_FamilyNpcRoster.MotherName), "Mama am nächsten Tag");
        }

        private static void NpcFamilyNeverHostile()
        {
            G1C_GameManager game = RunningGame();
            foreach (string name in new[] { G1C_FamilyNpcRoster.MotherName, G1C_FamilyNpcRoster.GrandmaName })
            {
                G1C_NpcCharacter member = game.Npcs.Find(name);
                G1C_Conversation c = game.Npcs.Conversation;
                for (int i = 0; i < 8; i++)
                {
                    c.Open(member, null);
                    int talk = c.Options.ToList().FindIndex(o => o.Text.StartsWith("Lass uns plaudern"));
                    c.Choose(talk);
                    int insult = c.Options.ToList().FindIndex(o => o.Text.StartsWith("Du kannst"));
                    if (insult >= 0) c.Choose(insult);
                    c.Close();
                }
                G1C_RelationshipStatus s = member.Relationship.Status;
                Check(!G1C_NpcRelationship.IsHostile(s) && !G1C_NpcRelationship.IsRival(s), name + " ist jetzt " + s);
            }
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

        // ------------------------------------------------------------------ Erweiterung 15.11.2026

        private static G1C_GameManager GameAt(G1C_WorldLocation location, int x, int y, G1C_Direction facing, int hour = 12)
        {
            G1C_GameManager game = RunningGame();
            SetHour(game, 1, hour);
            G1C_SaveData save = game.CreateSave();
            save.Location = (int)location; save.X = x; save.Y = y; save.Facing = (int)facing;
            game.ApplySave(save);
            Check(game.Player.TileX == x && game.Player.TileY == y, "Testposition " + x + "," + y + " in " + location + " nicht begehbar");
            return game;
        }

        private static void BuildingsPavilion()
        {
            G1C_ExteriorWorld city = new G1C_ExteriorWorld();
            G1C_CollisionManager collision = new G1C_CollisionManager(city);
            List<G1C_WorldObject> pavilions = city.Objects.Where(o => o.SpriteId == "building_pavilion").ToList();
            Check(pavilions.Count == 2, "Anzahl Pavillons: " + pavilions.Count);
            foreach (G1C_WorldObject p in pavilions)
            {
                Check(!p.Solid, "Pavillon-Objekt ist massiv");
                // Änderung durch: Patrick Lupierz – 16.11.2026 – von jeder Seite betretbar: nur die vier Ecksäulen massiv
                foreach (int[] c in new[] { new[] { 0, 0 }, new[] { 3, 0 }, new[] { 0, 3 }, new[] { 3, 3 } })
                    Check(collision.IsBlocked(p.X + c[0], p.Y + c[1]), "Ecksäule fehlt bei +" + c[0] + "/+" + c[1]);
                for (int i = 1; i <= 2; i++)
                {
                    Check(!collision.IsBlocked(p.X + i, p.Y), "Eingang oben blockiert");
                    Check(!collision.IsBlocked(p.X + i, p.Y + 3), "Eingang unten blockiert");
                    Check(!collision.IsBlocked(p.X, p.Y + i), "Eingang links blockiert");
                    Check(!collision.IsBlocked(p.X + 3, p.Y + i), "Eingang rechts blockiert");
                    Check(!collision.IsBlocked(p.X + i, p.Y + 1) && !collision.IsBlocked(p.X + i, p.Y + 2), "Innenraum blockiert");
                }
            }
        }

        private static void BankStaff()
        {
            G1C_GameManager game = new G1C_GameManager();
            string[] staff = { "Bankierin Frida", "Bankberater Karl", "Wachmann Bodo" };
            foreach (string name in staff)
            {
                Check(game.Bank.Objects.OfType<G1C_Npc>().Any(n => n.Name == name), name + " steht nicht in der Bank");
                Check(game.Npcs.Find(name) != null, name + " hat kein Gesprächsprofil");
            }
            Check(game.Npcs.Find("Bankberater Karl").Personality.Topics.Count >= 3, "Karl ohne eigene Themen");
            Check(game.Bank.Objects.Count(o => o.SpriteId == "obj_chair") >= 3 && game.Bank.Objects.Any(o => o.SpriteId == "obj_home_table"),
                  "Bank ohne Tisch/Stühle");
        }

        private static void BikeRiding()
        {
            G1C_ExteriorMap map = new G1C_GameManager().Exterior.Map;
            G1C_GameManager probe = new G1C_GameManager();
            G1C_ParkedBike stand = probe.Exterior.ParkedBike;
            Check(stand != null, "Kein Fahrrad vor dem Haus");
            Check(probe.Exterior.Objects.Count(o => o.Occupies(stand.X, stand.Y)) == 1, "Fahrrad überlappt ein anderes Objekt");

            G1C_GameManager game = GameAt(G1C_WorldLocation.Exterior, stand.X, stand.Y + 1, G1C_Direction.Up);
            Check(game.Inventory.Has(G1C_BikeSystem.StarterBike), "Startfahrrad fehlt im Inventar");
            Equal("obj_bike_red", stand.SpriteId == null ? "" : game.Exterior.ParkedBike.SpriteId, "Grafik am Ständer");
            game.HandleInput(G1C_InputEvent.FromKey(Key.E));
            Check(game.Bikes.Riding, "Aufsteigen am Ständer fehlgeschlagen");
            Equal("obj_bike_stand", game.Exterior.ParkedBike.SpriteId, "Ständer leer beim Fahren");
            game.HandleInput(G1C_InputEvent.FromKey(Key.E)); // Dialog schließen
            while (game.State == G1C_GameState.Dialogue) game.HandleInput(G1C_InputEvent.FromKey(Key.E));
            game.Update();
            Equal(G1C_BikeSystem.RidingStepTicks, game.Player.StepTicks, "Schritt-Ticks mit Fahrrad");

            G1C_BikeSystem bikes = new G1C_BikeSystem();
            G1C_Inventory inventory = new G1C_Inventory();
            inventory.Add("bike_red");
            bikes.TryMount(inventory, "bike_red", G1C_WorldLocation.Market);
            Check(!bikes.Riding, "Fahren im Gebäude erlaubt");
            bikes.TryMount(inventory, "bike_blue", G1C_WorldLocation.Exterior);
            Check(!bikes.Riding, "Fremdes Fahrrad fahrbar");

            // ins Casino fahren -> automatisch absteigen
            G1C_GameManager door = GameAt(G1C_WorldLocation.Exterior, map.DoorX(G1C_WorldLocation.Casino), map.DoorY(G1C_WorldLocation.Casino) + 1, G1C_Direction.Up);
            door.Bikes.TryMount(door.Inventory, G1C_BikeSystem.StarterBike, G1C_WorldLocation.Exterior);
            door.HandleInput(G1C_InputEvent.FromKey(Key.E));
            Equal(G1C_WorldLocation.Casino, door.CurrentLocation, "Ort nach Betreten");
            Check(!door.Bikes.Riding, "Im Casino noch auf dem Fahrrad");
        }

        private static void BikeShopBuy()
        {
            G1C_GameManager game = new G1C_GameManager();
            foreach (string id in new[] { "bike_red", "bike_blue", "bike_green", "bike_yellow", "bike_black" })
                Check(game.BikeShop.Objects.Any(o => o.SpriteId == "obj_" + id), id + " nicht ausgestellt");
            Check(game.Exterior.Objects.Any(o => o.SpriteId == "building_bike_shop"), "Fahrradladen fehlt in der Stadt");
            G1C_MoneyManager money = new G1C_MoneyManager(1000);
            G1C_Inventory inventory = new G1C_Inventory();
            G1C_Item blue = G1C_ItemCatalog.Get("bike_blue");
            G1C_Shop.Buy(blue, money, inventory);
            Equal(1000 - G1C_Shop.PriceWithMarkup(blue.Price), money.Balance, "Preis blaues Fahrrad");
            G1C_Shop.Buy(blue, money, inventory);
            Equal(1, inventory.Count("bike_blue"), "Fahrrad doppelt gekauft");
            Check(!G1C_Shop.For(G1C_WorldLocation.BikeShop).IsOpen(20), "Fahrradladen um 20 Uhr offen");
            Check(game.Npcs.Find("Radhändler Rudi") != null, "Radhändler ohne Profil");
        }

        private static void ArcadeLayout()
        {
            G1C_GameManager game = new G1C_GameManager();
            Equal(5, G1C_ArcadeCatalog.Cabinets.Count, "Anzahl Automaten");
            Check(G1C_ArcadeCatalog.Cabinets.All(c => c.Games.Count <= G1C_ArcadeCatalog.MaxGamesPerCabinet), "Mehr als 9 Spiele");
            Check(G1C_ArcadeCatalog.Cabinets.Count(c => c.IsGuestCabinet) == 1, "Kein Gast-Automat");
            foreach (G1C_ArcadeCabinet c in G1C_ArcadeCatalog.Cabinets)
                Check(game.Arcade.Objects.Any(o => o.Name == c.Name && o.Interaction == G1C_InteractionType.Arcade), c.Name + " fehlt in der Spielhalle");
            Check(game.Arcade.Objects.Any(o => o.Interaction == G1C_InteractionType.Claw), "Greifautomat fehlt in der Spielhalle");
            Check(!game.World.Objects.Any(o => o.Interaction == G1C_InteractionType.Claw), "Greifautomat steht noch im Casino");
            Check(game.Exterior.Objects.Any(o => o.SpriteId == "building_arcade"), "Spielhalle fehlt in der Stadt");
            Equal("tile_arcade_floor", game.Arcade.Map.GetTileSpriteId(5, 5), "Spielhallenboden");
        }

        private static void ArcadeFeeAndReward()
        {
            G1C_MoneyManager money = new G1C_MoneyManager(100);
            G1C_PongDuel pong = new G1C_PongDuel(money, new Random(3));
            pong.HandleInput(G1C_InputEvent.FromKey(Key.E));
            Equal(95, money.Balance, "Einsatz abgebucht");
            for (int i = 0; i < G1C_PongDuel.WinningScore; i++) { pong.DebugShoot(true); pong.MoveBall(); }
            Check(pong.IsOver, "Spiel nach 5 Punkten nicht beendet");
            int expected = G1C_ArcadeGame.RewardFor(G1C_PongDuel.WinningScore * 100 + 200, 25);
            Equal(95 + expected, money.Balance, "Belohnung");
            for (int i = 0; i < 10; i++) pong.Update();
            pong.HandleInput(G1C_InputEvent.FromKey(Key.Q));
            Equal(95 + expected, money.Balance, "doppelte Belohnung");
            Check(pong.WantsExit, "Verlassen mit Q");
            G1C_MoneyManager poor = new G1C_MoneyManager(3);
            G1C_SnakeGame snake = new G1C_SnakeGame(poor, new Random(1));
            Check(!snake.StartRound() && poor.Balance == 3, "Start ohne genug Chips");
            Equal(G1C_ArcadeGame.MaxReward, G1C_ArcadeGame.RewardFor(1000000, 10), "Belohnungsdeckel");
        }

        private static void ArcadeStarHunter()
        {
            G1C_StarHunter game = new G1C_StarHunter(new G1C_MoneyManager(100), new Random(5));
            Check(game.StartRound(), "Start");
            Equal(3, game.Lives, "Leben zu Beginn");
            game.DebugPlaceTargetAboveShip();
            game.HandleInput(G1C_InputEvent.FromKey(Key.E));
            for (int i = 0; i < 40 && game.Score == 0; i++) game.Update();
            Check(game.Score >= 10, "Treffer ohne Punkte (Score " + game.Score + ")");
        }

        private static void ArcadeSnake()
        {
            G1C_SnakeGame snake = new G1C_SnakeGame(new G1C_MoneyManager(100), new Random(2));
            snake.StartRound();
            int[] head = snake.Head;
            snake.DebugPlaceFood(head[0] + 1, head[1]);
            snake.Advance();
            Equal(4, snake.Length, "Länge nach Frucht");
            Equal(10, snake.Score, "Punkte nach Frucht");
            snake.HandleInput(G1C_InputEvent.FromKey(Key.A)); // Umkehr ist verboten
            for (int i = 0; i < G1C_SnakeGame.GridW + 2 && snake.IsPlaying; i++) snake.Advance();
            Check(snake.IsOver, "Wand beendet das Spiel nicht");
        }

        private static void ArcadeBrickBreaker()
        {
            G1C_BrickBreaker game = new G1C_BrickBreaker(new G1C_MoneyManager(100), new Random(4));
            game.StartRound();
            int before = game.BricksLeft;
            game.DebugAimAtBrick(G1C_BrickBreaker.BrickRows - 1, 3);
            game.MoveBall();
            Equal(before - 1, game.BricksLeft, "Steine nach Treffer");
            Check(game.Score > 0, "Punkte nach Treffer");
            for (int i = 0; i < 3; i++) { game.DebugDropBall(); game.MoveBall(); }
            Check(game.IsOver, "Spiel nach 3 verlorenen Bällen nicht beendet");
        }

        private static void ArcadePongAi()
        {
            Equal(20.0, G1C_PongAI.PredictY(0, 10, 1, -1, 30, 100), "Auftreffpunkt mit Abpraller oben");
            Equal(50.0, G1C_PongAI.PredictY(0, 50, 2, 0, 100, 100), "gerader Ball");
            G1C_PongAI ai = new G1C_PongAI(new Random(1), 0);
            double moved = ai.Move(50, 100, 20, 3, 0, 200, 100);
            Check(Math.Abs(moved - 50) <= G1C_PongAI.MaxSpeed + 1e-9 && moved < 50, "KI bewegt sich nicht begrenzt zum Ziel");
        }

        // ---- Hilfsklassen: ein winziges Framework-Spiel "einer anderen Gruppe" für den Gast-Automaten
        private sealed class FakeField : IGameField
        {
            public int Moves;
            public bool CanBePaintedBy(IPaintGame painter) { return painter is FakePainter; }
        }

        private sealed class FakeRules : IGameRules
        {
            private FakeField _field = new FakeField();
            public string Name { get { return "Testspiel einer Gruppe"; } }
            public IGameField CurrentField { get { return _field; } }
            public bool MovesPossible { get { return _field.Moves < 4; } }
            public void DoMove(IPlayMove move) { _field.Moves++; }
            public void ClearField() { _field = new FakeField(); }
            public int CheckIfPLayerWon() { return _field.Moves >= 2 ? 1 : -1; }
        }

        private sealed class FakePainter : IPaintGame
        {
            public string Name { get { return "Testmaler"; } }
            public void PaintGameField(System.Windows.Controls.Canvas canvas, IGameField currentField)
            {
                canvas.Children.Clear();
                canvas.Children.Add(new System.Windows.Shapes.Rectangle { Width = 40, Height = 40, Fill = System.Windows.Media.Brushes.Red });
            }
        }

        private sealed class FakeMove : IPlayMove
        {
            public FakeMove(int number) { PlayerNumber = number; }
            public int PlayerNumber { get; }
        }

        private class FakePlayer : IGamePlayer
        {
            public string Name { get { return "Testspieler"; } }
            public int PlayerNumber { get; private set; }
            public void SetPlayerNumber(int playerNumber) { PlayerNumber = playerNumber; }
            public bool CanBeRuledBy(IGameRules rules) { return rules is FakeRules; }
            public virtual IGamePlayer Clone() { return new FakePlayer(); }
        }

        private sealed class FakeHuman : FakePlayer, IHumanGamePlayer
        {
            public IPlayMove GetMove(IMoveSelection selection, IGameField field) { return selection is IKeySelection ? new FakeMove(PlayerNumber) : null; }
            public override IGamePlayer Clone() { return new FakeHuman(); }
        }

        private sealed class FakeComputer : FakePlayer, IComputerGamePlayer
        {
            public IPlayMove GetMove(IGameField field) { return new FakeMove(PlayerNumber); }
            public override IGamePlayer Clone() { return new FakeComputer(); }
        }

        private static void ArcadeGuestGame()
        {
            List<G1C_GuestGameEntry> found = G1C_GuestGameCatalog.Discover(
                new IGameRules[] { new FakeRules(), new G1C_CasinoRules() },
                new IPaintGame[] { new G1C_CasinoPainter(), new FakePainter() },
                new IGamePlayer[] { new FakeHuman(), new FakeComputer(), new G1C_CasinoHumanPlayer() });
            Equal(1, found.Count, "gefundene Gastspiele (eigenes Casino muss fehlen)");
            Check(found[0].HasComputerOpponent, "Computergegner nicht erkannt");

            G1C_MoneyManager money = new G1C_MoneyManager(100);
            G1C_GuestGameHost host = new G1C_GuestGameHost(money, new Random(1), found[0]);
            host.HandleInput(G1C_InputEvent.FromKey(Key.E));
            Check(host.Running, "Gastspiel startet nicht: " + host.LastError);
            Equal(95, money.Balance, "Einsatz Gast-Automat");
            host.HandleInput(G1C_InputEvent.FromKey(Key.Space)); // Spielerzug, danach Computerzug -> Spieler 1 gewinnt
            Check(!host.Running, "Gastspiel nicht beendet");
            Equal((int?)1, host.Winner, "Gewinner");
            Equal(95 + G1C_GuestGameHost.WinReward, money.Balance, "Belohnung Gast-Automat");
            host.Update();
            Equal(95 + G1C_GuestGameHost.WinReward, money.Balance, "doppelte Belohnung Gast-Automat");
            Check(host.Frame != null && host.Frame.PixelWidth > 0, "Gastspiel wurde nicht gezeichnet");
        }

        private static void ArcadeInGameFlow()
        {
            G1C_ArcadeCabinet duel = G1C_ArcadeCatalog.Cabinets.First(c => c.Name == "Automat Duell");
            G1C_GameManager game = GameAt(G1C_WorldLocation.Arcade, duel.X, duel.Y + 1, G1C_Direction.Up);
            Check(game.CurrentHint.Contains("spielen"), "Hinweis am Automaten: '" + game.CurrentHint + "'");
            game.HandleInput(G1C_InputEvent.FromKey(Key.E));
            Equal(G1C_GameState.ListMenu, game.State, "Spielauswahl");
            game.HandleInput(G1C_InputEvent.FromKey(Key.E));
            Equal(G1C_GameState.Arcade, game.State, "Automatenspiel aktiv");
            G1C_PongDuel pong = game.ActiveGame as G1C_PongDuel;
            Check(pong != null, "falsches Spiel gestartet");
            game.HandleInput(G1C_InputEvent.FromKey(Key.E));
            for (int i = 0; i < G1C_PongDuel.WinningScore; i++) { pong.DebugShoot(true); pong.MoveBall(); }
            game.HandleInput(G1C_InputEvent.FromKey(Key.Q));
            Equal(G1C_GameState.Result, game.State, "Ergebnisanzeige nach dem Automaten");
            int record;
            Check(game.ArcadeHighscores.TryGetValue("Pong-Duell", out record) && record == 700, "Rekord nicht gespeichert");
            G1C_GameManager loaded = new G1C_GameManager();
            loaded.ApplySave(game.CreateSave());
            Check(loaded.ArcadeHighscores.ContainsKey("Pong-Duell"), "Rekord nach Laden verloren");
        }

        private static void NpcGrandmaHouse()
        {
            G1C_GameManager probe = new G1C_GameManager();
            G1C_ExteriorMap map = probe.Exterior.Map;
            G1C_GameManager game = GameAt(G1C_WorldLocation.Exterior, map.DoorX(G1C_WorldLocation.GrandmaHouse),
                                          map.DoorY(G1C_WorldLocation.GrandmaHouse) + 1, G1C_Direction.Up);
            Check(game.CurrentHint.Contains("Omas Haus"), "Hinweis vor Omas Tür: '" + game.CurrentHint + "'");
            game.HandleInput(G1C_InputEvent.FromKey(Key.E));
            Equal(G1C_WorldLocation.GrandmaHouse, game.CurrentLocation, "Ort nach Betreten");
            Check(game.CurrentWorld.Objects.OfType<G1C_Npc>().Any(n => n.Name == G1C_FamilyNpcRoster.GrandmaName), "Oma nicht zu Hause");
            G1C_GameManager talk = GameAt(G1C_WorldLocation.GrandmaHouse, 4, 3, G1C_Direction.Down);
            talk.HandleInput(G1C_InputEvent.FromKey(Key.E));
            Equal(G1C_GameState.Conversation, talk.State, "Gespräch mit Oma in ihrem Haus");
        }

        private static void ShopGasStationWindow()
        {
            G1C_GameManager probe = new G1C_GameManager();
            Equal(2, probe.Exterior.Objects.Count(o => o.SpriteId == "obj_fuel_pump"), "Anzahl Zapfsäulen");
            G1C_ShopWindow window = probe.Exterior.Objects.OfType<G1C_ShopWindow>().FirstOrDefault();
            Check(window != null && window.ShopLocation == G1C_WorldLocation.GasStation, "Verkaufsfenster fehlt");
            G1C_GameManager game = GameAt(G1C_WorldLocation.Exterior, window.X, window.Y + 1, G1C_Direction.Up, 3);
            game.HandleInput(G1C_InputEvent.FromKey(Key.E));
            Equal(G1C_GameState.Conversation, game.State, "Gespräch am Fenster");
            Equal("Tankwart Olli", game.Npcs.Conversation.Npc.Name, "Gesprächspartner am Fenster");
            game.HandleInput(G1C_InputEvent.FromKey(Key.D1));
            Equal(G1C_GameState.ListMenu, game.State, "Warenliste am Fenster");
            Check(game.ListMenu.Rows.Any(r => r.Text.Contains("Energydrink")), "Sortiment der Tankstelle fehlt");
            int before = game.Money.Balance;
            game.HandleInput(G1C_InputEvent.FromKey(Key.E));
            Check(game.Money.Balance < before, "Kauf um 3 Uhr nachts am Fenster fehlgeschlagen");
            Check(probe.Exterior.Objects.Any(o => o.SpriteId == "building_gas_station"), "Tankstellengebäude fehlt");
        }

        private static void GraphicsWallsAndRugs()
        {
            G1C_GameManager game = new G1C_GameManager();
            IG1C_WorldMap bank = game.Bank.Map;
            Check(bank.GetTileSpriteId(0, 5).EndsWith("_l") && bank.GetTileSpriteId(bank.Width - 1, 5).EndsWith("_r"), "Seitenwände Bank");
            Check(bank.GetTileSpriteId(3, bank.Height - 1).EndsWith("_b"), "Unterwand Bank");
            Check(bank.GetTileSpriteId(5, 0).StartsWith("tile_bank_wall"), "Rückwand Bank");
            Equal("tile_wallside_home_l", game.Home.Map.GetTileSpriteId(0, 4), "Seitenwand Zuhause");
            string corner = bank.GetTileSpriteId(6, 7), middle = bank.GetTileSpriteId(7, 7);
            Check(corner.StartsWith("tile_rug_bank_") && corner != middle, "Teppich besteht nicht aus Rand-/Mittelteilen");
            Check(game.Home.Map.GetTileSpriteId(4, 7).StartsWith("tile_rug_home_"), "Teppich im Zuhause fehlt");
            foreach (string id in new[] { corner, middle, "tile_wallside_bank_l", "tile_wallside_home_r", "tile_rug_home_6" })
                Check(HasBuiltInSprite(id), "Grafik fehlt: " + id);
        }

        private static void GraphicsWallPictures()
        {
            G1C_GameManager game = new G1C_GameManager();
            List<string> motifs = new List<string>();
            foreach (G1C_BuildingInteriorWorld room in new[] { game.Hospital, game.Market, game.GasStation, game.ClothingStore,
                                                                game.Bank, game.FurnitureStore, game.BikeShop, game.GrandmaHouse })
            {
                G1C_WorldObject pic = room.Objects.FirstOrDefault(o => o.SpriteId != null && o.SpriteId.StartsWith("obj_picture_"));
                Check(pic != null && pic.Interaction == G1C_InteractionType.Inspect, room.Location + " ohne Wandbild");
                Check(room.Map.IsWalkable(pic.X, pic.Y + 1) && room.ObjectAt(pic.X, pic.Y + 1) == null, room.Location + ": Bild nicht erreichbar");
                motifs.Add(pic.SpriteId);
            }
            Check(motifs.Distinct().Count() == motifs.Count, "Wandbilder wiederholen sich: " + string.Join(", ", motifs));
        }

        private static void GraphicsBirds()
        {
            G1C_SpriteLibrary sprites = new G1C_SpriteLibrary(Path.Combine(Path.GetTempPath(), "g1c_no_assets"));
            Check(sprites.GetAspect("animal_bird_0") >= 1.5, "Vogel nicht als hohe Grafik (sitzt am Boden)");
            double over = (sprites.GetAspect("animal_bird_0") - sprites.GetAspect("obj_city_tree")) * 16; // Pixel über der Krone
            Check(over >= 6 && over <= 12, "Vogel sitzt nicht auf der Kronenspitze (Überstand " + over + " px)");
            System.Windows.Media.Imaging.BitmapSource bird = sprites.Get("animal_bird_1");
            System.Windows.Media.Imaging.FormatConvertedBitmap bgra = new System.Windows.Media.Imaging.FormatConvertedBitmap(
                bird, System.Windows.Media.PixelFormats.Bgra32, null, 0);
            byte[] px = new byte[4];
            bgra.CopyPixels(new System.Windows.Int32Rect(bird.PixelWidth / 2, bird.PixelHeight - 2, 1, 1), px, 4, 0);
            Equal((byte)0, px[3], "Unterer Bereich des Vogels ist nicht transparent");
        }

        private static void StatesSleeping()
        {
            G1C_GameManager game = GameAt(G1C_WorldLocation.Home, 2, 3, G1C_Direction.Up, 23);
            game.HandleInput(G1C_InputEvent.FromKey(Key.E));            // Bett (2,2)
            Equal(G1C_GameState.ListMenu, game.State, "Schlafmenü");
            game.HandleInput(G1C_InputEvent.FromKey(Key.E));            // 06:00
            game.HandleInput(G1C_InputEvent.FromKey(Key.Escape));
            Check(game.IsSleeping, "Kein Schlafzustand");
            Equal(6, game.Clock.Hour, "Aufwachzeit");
            int x = game.Player.TileX;
            game.HandleInput(G1C_InputEvent.FromKey(Key.D));
            for (int i = 0; i < 10; i++) game.Update();
            Equal(x, game.Player.TileX, "Bewegung während des Schlafens");
            for (int i = 0; i < G1C_GameManager.SleepAnimationTicks; i++) game.Update();
            Check(!game.IsSleeping, "Schlafzustand endet nicht");
            game.HandleInput(G1C_InputEvent.FromKey(Key.D));
            for (int i = 0; i < 10; i++) game.Update();
            Equal(x + 1, game.Player.TileX, "Nach dem Aufwachen keine Bewegung");
        }

        // ------------------------------------------------------------------ Erweiterung 16.11.2026

        private static void FurnitureLimit()
        {
            G1C_MoneyManager money = new G1C_MoneyManager(5000);
            G1C_Inventory inventory = new G1C_Inventory();
            G1C_Item plant = G1C_ItemCatalog.Get("furniture_plant");
            inventory.Add("furniture_plant", 2);
            string ok = G1C_Shop.Buy(plant, money, inventory, 0);
            Equal(3, inventory.Count("furniture_plant"), "Kauf bis 3 – " + ok);
            int before = money.Balance;
            string refused = G1C_Shop.Buy(plant, money, inventory, 0);
            Equal(before, money.Balance, "Viertes Stück wurde bezahlt");
            Check(refused.Contains("3"), "Meldung beim Limit: " + refused);
            inventory.Remove("furniture_plant", 2);
            G1C_Shop.Buy(plant, money, inventory, 2);   // 1 im Inventar + 2 aufgestellt = 3
            Equal(1, inventory.Count("furniture_plant"), "Aufgestellte Möbel zählen mit");
        }

        private static void FurnitureEditorFlow()
        {
            G1C_GameManager game = GameAt(G1C_WorldLocation.Home, 8, 4, G1C_Direction.Up);
            game.Inventory.Add("furniture_plant", 2);
            game.HandleInput(G1C_InputEvent.FromKey(Key.E));                  // Einrichtungsplan
            Equal(G1C_GameState.ListMenu, game.State, "Einrichtungsplan öffnet die Liste");
            int row = game.ListMenu.Rows.FindIndex(r => r.Text.StartsWith("Topfpflanze"));
            game.ListMenu.Choose(row);
            Equal(G1C_GameState.FurnitureEditor, game.State, "Einrichtungsmodus");
            G1C_Item plant = G1C_ItemCatalog.Get("furniture_plant");
            List<G1C_FurnitureLocation> free = game.Home.AvailableFurnitureLocations(plant, game.Player);
            Check(free.Count >= 2, "Zu wenig freie Plätze");
            ClickTile(game, free[0].X, free[0].Y);
            Equal(1, game.Home.PlacedFurniture.Count, "Per Klick aufgestellt");
            Equal(1, game.Inventory.Count("furniture_plant"), "Inventar nach dem Aufstellen");
            Check(game.Home.Objects.Any(o => o.SpriteId == "obj_plant" && o.X == free[0].X && o.Y == free[0].Y), "Ansicht nicht aktualisiert");

            // Verschieben: Möbel anklicken, neuen Platz anklicken
            game.HandleInput(G1C_InputEvent.FromKey(Key.Q));
            Equal(G1C_GameState.MainGame, game.State, "[Q] beendet den Modus");
            game.HandleInput(G1C_InputEvent.FromKey(Key.E));
            game.ListMenu.Choose(game.ListMenu.Rows.FindIndex(r => r.Text.StartsWith("Möbel verschieben")));
            Equal(G1C_GameState.FurnitureEditor, game.State, "Verschiebe-Modus");
            ClickTile(game, free[0].X, free[0].Y);
            Check(game.Furnishing.IsMoving && game.Home.PlacedFurniture.Count == 0, "Möbel nicht aufgehoben");
            G1C_FurnitureLocation target = game.Home.AvailableFurnitureLocations(plant, game.Player)
                .First(l => l.X != free[0].X || l.Y != free[0].Y);
            ClickTile(game, target.X, target.Y);
            Check(game.Home.PlacedFurniture.Count == 1 && game.Home.PlacedFurniture[0].X == target.X && game.Home.PlacedFurniture[0].Y == target.Y,
                  "Möbel nicht verschoben");
            Equal(1, game.Inventory.Count("furniture_plant"), "Verschieben darf nichts kosten");

            // Aufheben und abbrechen: Möbel kehrt zurück
            ClickTile(game, target.X, target.Y);
            game.HandleInput(G1C_InputEvent.FromKey(Key.Escape));
            Check(game.Home.PlacedFurniture.Count == 1 && game.Home.PlacedFurniture[0].X == target.X, "Abbrechen stellt nicht zurück");

            // Einlagern
            game.HandleInput(G1C_InputEvent.FromKey(Key.E));
            game.ListMenu.Choose(game.ListMenu.Rows.FindIndex(r => r.Text.StartsWith("Möbel verschieben")));
            ClickTile(game, target.X, target.Y);
            game.HandleInput(G1C_InputEvent.FromKey(Key.R));
            Equal(0, game.Home.PlacedFurniture.Count, "Einlagern entfernt das Möbel");
            Equal(2, game.Inventory.Count("furniture_plant"), "Einlagern legt ins Inventar");

            // Blockierte Kachel (Wand) wird abgelehnt
            game.Furnishing.Begin(plant, game.Home, game.Player);
            game.Furnishing.SetCursor(0, 0, game.Home);
            Check(!game.Home.CanPlaceFurniture(plant, 0, 0, game.Player), "Wand gilt als frei");
        }

        private static void ClickTile(G1C_GameManager game, int x, int y)
        {
            double size = game.Home.TileSize;
            double mapX = (G1C_Renderer.ScreenWidth - game.Home.Map.Width * size) / 2;
            double mapY = G1C_GameHUD.Height + (G1C_Renderer.ScreenHeight - G1C_GameHUD.Height - game.Home.Map.Height * size) / 2;
            game.HandleInput(G1C_InputEvent.FromClick(mapX + (x + 0.5) * size, mapY + (y + 0.5) * size));
        }

        private static void AnimalsPetsRoam()
        {
            G1C_GameManager probe = RunningGame();
            G1C_GameManager game = GameAt(G1C_WorldLocation.Exterior, probe.Exterior.PlayerStartX, probe.Exterior.PlayerStartY, G1C_Direction.Down);
            List<G1C_PetAnimal> pets = game.Exterior.Objects.OfType<G1C_PetAnimal>().ToList();
            List<string> catVariants = pets.Where(p => p.Kind == G1C_PetKind.Cat).Select(p => p.Variant).Distinct().ToList();
            Equal(5, catVariants.Count, "Katzen-Fellvarianten in der Stadt");
            Check(pets.Count(p => p.Kind == G1C_PetKind.Dog) >= 3, "Zu wenige Hunde");
            foreach (string v in G1C_PetAnimal.CatVariants.Select(v => "pet_cat_" + v).Concat(G1C_PetAnimal.DogVariants.Select(v => "pet_dog_" + v)))
                foreach (string suffix in new[] { "_l_0", "_r_1", "_r_2" })
                    Check(HasBuiltInSprite(v + suffix), "Grafik fehlt: " + v + suffix);
            G1C_CollisionManager collision = new G1C_CollisionManager(game.Exterior);
            foreach (G1C_PetAnimal pet in pets) Check(pet.Solid && collision.IsBlocked(pet.X, pet.Y), pet.Name + " ist begehbar");

            Dictionary<G1C_PetAnimal, int[]> start = pets.ToDictionary(p => p, p => new[] { p.X, p.Y });
            for (int i = 0; i < 900; i++) game.Update();
            int moved = pets.Count(p => p.X != start[p][0] || p.Y != start[p][1]);
            Check(moved >= pets.Count / 2, "Nur " + moved + " von " + pets.Count + " Tieren sind gelaufen");
            foreach (G1C_PetAnimal pet in pets)
            {
                Check(Math.Abs(pet.X - pet.HomeX) <= pet.RoamRadius && Math.Abs(pet.Y - pet.HomeY) <= pet.RoamRadius, pet.Name + " hat sein Revier verlassen");
                Check(game.Exterior.Map.IsWalkable(pet.X, pet.Y), pet.Name + " steht auf einer unbegehbaren Kachel");
                Check(!game.Exterior.Objects.Any(o => o != pet && o.Solid && !(o is G1C_PetAnimal) && o.Occupies(pet.X, pet.Y)), pet.Name + " steht in einem Objekt");
            }
        }

        private static void AnimalsPettingAndVoices()
        {
            G1C_GameManager probe = RunningGame();
            G1C_PetAnimal cat = probe.Exterior.Objects.OfType<G1C_PetAnimal>().First(p => p.Kind == G1C_PetKind.Cat);
            G1C_CollisionManager collision = new G1C_CollisionManager(probe.Exterior);
            int[][] sides = { new[] { 0, 1, (int)G1C_Direction.Up }, new[] { 0, -1, (int)G1C_Direction.Down },
                              new[] { 1, 0, (int)G1C_Direction.Left }, new[] { -1, 0, (int)G1C_Direction.Right } };
            int[] side = sides.First(s => !collision.IsBlocked(cat.X + s[0], cat.Y + s[1]));
            G1C_GameManager game = GameAt(G1C_WorldLocation.Exterior, cat.X + side[0], cat.Y + side[1], (G1C_Direction)side[2]);
            Check(game.CurrentHint.Contains("streicheln"), "Hinweis: '" + game.CurrentHint + "'");
            game.HandleInput(G1C_InputEvent.FromKey(Key.E));
            G1C_PetAnimal petted = game.Exterior.Objects.OfType<G1C_PetAnimal>().First(p => p.Name == cat.Name);
            Equal(1, petted.PetCount, "Streichelzähler");
            Check(game.SystemMessage.Contains(cat.Name), "Meldung: " + game.SystemMessage);
            Check((game.AnimalVoices.TextOf(petted) ?? "").Contains("♥"), "Keine Herz-Blase");
            Equal("meow", game.Sound.LastEffect, "Geräusch beim Streicheln");

            Check(G1C_AnimalVoices.SoundOf(new G1C_PetAnimal("H", G1C_PetKind.Dog, "brown", 1, 1, 0), new Random(1)).StartsWith("W"), "Hund bellt nicht");
            Check(G1C_AnimalVoices.SoundOf(game.Exterior.Animals.First(a => a.SpriteId.StartsWith("animal_duck")), new Random(1)).StartsWith("Quak"), "Ente quakt nicht");
            Check(G1C_AnimalVoices.SoundOf(game.Exterior.Animals.First(a => a.SpriteId.StartsWith("animal_bird")), new Random(1)) != null, "Vogel stumm");
            foreach (G1C_Animal duck in game.Exterior.Animals.Where(a => a.SpriteId.StartsWith("animal_duck")))
                Check(duck.Solid, "Ente ist begehbar");

            G1C_AnimalVoices voices = new G1C_AnimalVoices();
            List<G1C_WorldObject> animals = game.Exterior.Animals.Cast<G1C_WorldObject>().ToList();
            Random random = new Random(5);
            for (int tick = 0; tick < 2000 && !voices.Active.Any(); tick++) voices.Update(tick, animals, random);
            Check(voices.Active.Any(), "Tiere melden sich nie von selbst");
            voices.Update(100000, animals, null);
            Check(!voices.Active.Any(), "Sprechblasen laufen nicht ab");
        }

        private static void SoundLogic()
        {
            Equal("title", G1C_SoundSystem.MusicFor(G1C_GameState.Title, G1C_WorldLocation.Home, null), "Titelmusik");
            Equal("town", G1C_SoundSystem.MusicFor(G1C_GameState.MainGame, G1C_WorldLocation.Exterior, null), "Stadtmusik");
            Equal("home", G1C_SoundSystem.MusicFor(G1C_GameState.MainGame, G1C_WorldLocation.Home, null), "Zuhause");
            Equal("shop", G1C_SoundSystem.MusicFor(G1C_GameState.ListMenu, G1C_WorldLocation.Market, null), "Laden");
            Equal("casino", G1C_SoundSystem.MusicFor(G1C_GameState.MainGame, G1C_WorldLocation.Casino, null), "Casino");
            Equal("arcade", G1C_SoundSystem.MusicFor(G1C_GameState.MainGame, G1C_WorldLocation.Arcade, null), "Spielhalle");
            Equal("fishing", G1C_SoundSystem.MusicFor(G1C_GameState.Fishing, G1C_WorldLocation.Exterior,
                new G1C_FishingGame(new G1C_MoneyManager(0), new Random(1), new G1C_Inventory())), "Angeln");

            G1C_GameManager probe = RunningGame();
            G1C_GameManager game = GameAt(G1C_WorldLocation.Exterior, probe.Exterior.PlayerStartX, probe.Exterior.PlayerStartY, G1C_Direction.Down);
            Check(!game.Sound.Enabled, "Ton im Test eingeschaltet");
            game.Update();
            Equal("town", game.Sound.CurrentMusic, "Musik in der Stadt");
            G1C_GameManager shop = GameAt(G1C_WorldLocation.Exterior, game.Exterior.Map.DoorX(G1C_WorldLocation.Market),
                                          game.Exterior.Map.DoorY(G1C_WorldLocation.Market) + 1, G1C_Direction.Up);
            shop.HandleInput(G1C_InputEvent.FromKey(Key.E));
            Equal("door", shop.Sound.LastEffect, "Türgeräusch");
            shop.Update();
            Equal("shop", shop.Sound.CurrentMusic, "Ladenmusik");
            Equal("Ton aus.", shop.Sound.ToggleMute(), "Stummschalten");

            foreach (string name in new[] { "music_town", "music_home", "music_shop", "music_casino", "music_arcade", "music_fishing", "music_title",
                                            "sfx_win", "sfx_jackpot", "sfx_lose", "sfx_coin", "sfx_door", "sfx_meow", "sfx_woof", "sfx_splash", "sfx_catch" })
            {
                byte[] wav = G1C_ChipTune.Render(name);
                Check(wav.Length > 2000 && wav[0] == 'R' && wav[1] == 'I' && wav[8] == 'W', "WAV ungültig: " + name);
                Equal(wav.Length - 8, BitConverter.ToInt32(wav, 4), "RIFF-Länge " + name);
            }
            Check(Math.Abs(G1C_ChipTune.Frequency("A4") - 440) < 0.01 && Math.Abs(G1C_ChipTune.Frequency("A5") - 880) < 0.01, "Tonhöhe falsch");
        }

        private static void BikeDirections()
        {
            G1C_BikeSystem bikes = new G1C_BikeSystem();
            List<string> ids = new[] { G1C_Direction.Left, G1C_Direction.Right, G1C_Direction.Up, G1C_Direction.Down }
                .Select(d => bikes.RideSpriteId(d)).ToList();
            Equal(4, ids.Distinct().Count(), "Eigene Grafik je Richtung");
            foreach (string id in ids) Check(HasBuiltInSprite(id), "Grafik fehlt: " + id);
            G1C_PixelCanvas right = G1C_DecorSprites.BikeSide("bike_red"), left = right.MirrorX();
            bool mirrored = true, different = false;
            for (int y = 0; y < right.Height; y++)
                for (int x = 0; x < right.Width; x++)
                {
                    if (left.Get(x, y) != right.Get(right.Width - 1 - x, y)) mirrored = false;
                    if (left.Get(x, y) != right.Get(x, y)) different = true;
                }
            Check(mirrored && different, "Links ist nicht die gespiegelte Rechts-Grafik");
        }

        private static void BikeEnterLeaveBuilding()
        {
            G1C_GameManager probe = RunningGame();
            G1C_ExteriorMap map = probe.Exterior.Map;
            G1C_GameManager game = GameAt(G1C_WorldLocation.Exterior, map.DoorX(G1C_WorldLocation.Market), map.DoorY(G1C_WorldLocation.Market) + 1, G1C_Direction.Up);
            game.Bikes.TryMount(game.Inventory, G1C_BikeSystem.StarterBike, game.CurrentLocation);
            Check(game.Bikes.Riding, "Aufsteigen");
            game.HandleInput(G1C_InputEvent.FromKey(Key.E));
            Equal(G1C_WorldLocation.Market, game.CurrentLocation, "Laden betreten");
            Check(!game.Bikes.Riding && game.Bikes.ParkedAtDoor, "Rad nicht vor der Tür abgestellt");
            Equal("obj_bike_stand", game.Exterior.ParkedBike.SpriteId, "Fahrradständer zuhause müsste leer sein");
            game.HandleInput(G1C_InputEvent.FromKey(Key.I));
            Check(game.ListMenu.Rows.Any(r => r.Right == "steht vor der Tür"), "Inventar zeigt Fahrradstatus nicht");
            game.HandleInput(G1C_InputEvent.FromKey(Key.Escape));

            IG1C_WorldMap inside = game.CurrentWorld.Grid;
            int ex = -1, ey = -1;
            for (int y = 0; y < inside.Height && ex < 0; y++)
                for (int x = 0; x < inside.Width; x++)
                    if (inside.GetTransitionAt(x, y) != null) { ex = x; ey = y; break; }
            Check(ex >= 0, "Ausgang nicht gefunden");
            game.Player.Teleport(ex, ey - 1, G1C_Direction.Down);
            game.HandleInput(G1C_InputEvent.FromKey(Key.E));
            Equal(G1C_WorldLocation.Exterior, game.CurrentLocation, "Laden verlassen");
            Check(game.Bikes.Riding && !game.Bikes.ParkedAtDoor, "Nach dem Verlassen nicht automatisch wieder aufgestiegen");
            game.HandleInput(G1C_InputEvent.FromKey(Key.I));
            Check(game.ListMenu.Rows.Any(r => r.Right.StartsWith("fährst du")), "Inventar zeigt 'fährst du' nicht");
            int row = game.ListMenu.Rows.FindIndex(r => r.Right.StartsWith("fährst du"));
            game.ListMenu.Choose(row);
            Check(!game.Bikes.Riding, "Ein Klick zum Absteigen reicht nicht");
        }

        private static void ConversationSmallTalk()
        {
            G1C_GameManager game = new G1C_GameManager();
            List<G1C_NpcCharacter> all = game.Npcs.Characters.ToList();
            int sameCompliment = all.Count(c => G1C_SmallTalk.ComplimentFor(c.Personality) == "Du machst das echt gut hier.");
            Check(sameCompliment <= 2, sameCompliment + " NPCs haben noch 'Du machst das echt gut hier'");
            Check(all.Count(c => (G1C_SmallTalk.InsultFor(c.Personality) ?? "").Contains("nicht spielen")) <= 1, "'Du kannst nicht spielen' bei zu vielen NPCs");
            Check(all.Select(c => G1C_SmallTalk.ComplimentFor(c.Personality)).Distinct().Count() >= 8, "Zu wenig verschiedene Lob-Sätze");
            foreach (G1C_NpcCharacter c in all.Where(c => c.Personality.IsFamily))
                Check(G1C_SmallTalk.InsultFor(c.Personality) == null, c.Name + " hat eine Beleidigungs-Option");
            foreach (G1C_NpcCharacter c in all.Where(c => !c.Personality.IsFamily))
            {
                Check(c.Personality.Topics.Count >= 2, c.Name + " hat zu wenige Themen");
                Check(c.Personality.Topics.Any(t => t.Question == "Was gibt's Neues in der Stadt?"), c.Name + " ohne Stadtklatsch");
                Check(c.Personality.Topics.Select(t => t.Question).Distinct().Count() == c.Personality.Topics.Count, c.Name + ": doppelte Fragen");
            }

            // Im Gespräch: Mama plaudern -> kein "echt gut hier", keine Beleidigung, "Erzähl mir mehr"
            G1C_GameManager home = GameAt(G1C_WorldLocation.Home, 8, 5, G1C_Direction.Right);
            home.HandleInput(G1C_InputEvent.FromKey(Key.E));
            Equal(G1C_GameState.Conversation, home.State, "Gespräch mit Mama");
            ChooseOption(home, "Lass uns plaudern.");
            List<string> options = home.Npcs.Conversation.Options.Select(o => o.Text).ToList();
            Check(!options.Contains("Du machst das echt gut hier.") && !options.Any(o => o.Contains("nicht spielen")),
                  "Standardsätze bei Mama: " + string.Join(" | ", options));
            Check(options.Contains("Ich hab dich lieb, Mama."), "Rollen-Lob fehlt: " + string.Join(" | ", options));
        }

        private static void ChooseOption(G1C_GameManager game, string text)
        {
            int index = game.Npcs.Conversation.Options.Select(o => o.Text).ToList().IndexOf(text);
            Check(index >= 0, "Option fehlt: " + text + " in " + string.Join(" | ", game.Npcs.Conversation.Options.Select(o => o.Text)));
            game.HandleInput(G1C_InputEvent.FromKey(Key.D1 + index));
        }

        private static void FishingSpots()
        {
            G1C_GameManager probe = RunningGame();
            List<G1C_WorldObject> rods = probe.Exterior.Objects.Where(o => o.Interaction == G1C_InteractionType.Fishing).ToList();
            Equal(1, rods.Count, "Es soll genau einen Angelplatz geben");
            int lowestBridgeY = Enumerable.Range(0, G1C_ExteriorMap.MapHeight).Where(y => probe.Exterior.Map.IsBridge(probe.Exterior.Map.RiverCenterXAt(y), y)).Max();
            Check(rods[0].Y >= lowestBridgeY - 1, "Angel nicht auf der untersten Brücke (Angel y=" + rods[0].Y + ", Brücke bis y=" + lowestBridgeY + ")");
            foreach (G1C_WorldObject rod in rods)
            {
                Check(!rod.Solid && !new G1C_CollisionManager(probe.Exterior).IsBlocked(rod.X, rod.Y), "Angel blockiert den Weg");
                Check(probe.Exterior.Map.IsBridge(rod.X, rod.Y), "Angel nicht auf einer Brücke");
                Check(!new G1C_CollisionManager(probe.Exterior).IsBlocked(rod.X, rod.Y + 1), "Brücke unter der Angel blockiert");
            }
            Check(HasBuiltInSprite("obj_fishing_rod"), "Grafik der Angel fehlt");
            // Angel ist begehbar: Interaktion muss auf der Angel-Kachel selbst und (mit Blick darauf) daneben gehen
            G1C_GameManager onTile = GameAt(G1C_WorldLocation.Exterior, rods[0].X, rods[0].Y, G1C_Direction.Up);
            Equal("[E] Angeln", onTile.CurrentHint, "Hinweis auf der Angel-Kachel");
            G1C_GameManager walking = GameAt(G1C_WorldLocation.Exterior, rods[0].X, rods[0].Y + 1, G1C_Direction.Up);
            walking.HandleInput(G1C_InputEvent.FromKey(Key.W));
            for (int i = 0; i < 12; i++) walking.Update();
            Equal(rods[0].Y, walking.Player.TileY, "Angel muss begehbar sein");
            walking.HandleInput(G1C_InputEvent.FromKey(Key.E));
            Equal(G1C_GameState.Fishing, walking.State, "Nach dem Daraufgehen: [E] startet das Angeln");
            G1C_GameManager game = GameAt(G1C_WorldLocation.Exterior, rods[0].X, rods[0].Y + 1, G1C_Direction.Up);
            Equal("[E] Angeln", game.CurrentHint, "Hinweis vor der Angel");
            game.HandleInput(G1C_InputEvent.FromKey(Key.E));
            Equal(G1C_GameState.Fishing, game.State, "Angelspiel startet");
            Check(game.ActiveGame is G1C_FishingGame, "Kein Angelspiel aktiv");
            game.HandleInput(G1C_InputEvent.FromKey(Key.Q));
            Equal(G1C_GameState.Dialogue, game.State, "Fangübersicht nach dem Angeln");
        }

        private static void FishingCatch()
        {
            G1C_MoneyManager money = new G1C_MoneyManager(0);
            G1C_Inventory inventory = new G1C_Inventory();
            G1C_FishingGame fishing = new G1C_FishingGame(money, new Random(3), inventory) { Steering = () => 0 };
            fishing.Cast();
            Equal(G1C_FishingGame.Phase.Waiting, fishing.State, "Auswerfen");
            for (int i = 0; i < 200 && fishing.State == G1C_FishingGame.Phase.Waiting; i++) fishing.Update();
            Equal(G1C_FishingGame.Phase.Bite, fishing.State, "Biss");
            fishing.HandleInput(G1C_InputEvent.FromKey(Key.E));
            Equal(G1C_FishingGame.Phase.Fight, fishing.State, "Anhauen");

            int opposite = 0, samples = 0;
            for (int i = 0; i < 2000 && fishing.State == G1C_FishingGame.Phase.Fight; i++)
            {
                double zone = fishing.ZoneCenter;
                fishing.UpdateFight(0);
                if (Math.Abs(fishing.FishVelocity) > 0.002 && fishing.ZoneCenter > 0.14 && fishing.ZoneCenter < 0.86)
                {
                    samples++;
                    if (Math.Sign(fishing.ZoneCenter - zone) == -Math.Sign(fishing.FishVelocity)) opposite++;
                }
                if (samples > 30) break;
            }
            Check(samples == 0 || opposite >= samples * 0.9, "Grüner Bereich treibt nicht gegen den Fisch (" + opposite + "/" + samples + ")");

            // Mit Gegensteuern (immer zum Fisch) wird gefangen
            G1C_FishingGame follow = new G1C_FishingGame(money, new Random(11), inventory);
            follow.Steering = () => Math.Sign(follow.FishPosition - follow.ZoneCenter);
            int caught = 0;
            for (int round = 0; round < 6; round++)
            {
                follow.StrikeHook();
                for (int i = 0; i < 3000 && follow.State == G1C_FishingGame.Phase.Fight; i++) follow.Update();
                if (follow.State == G1C_FishingGame.Phase.Caught) caught++;
            }
            Check(caught >= 4, "Mit Gegensteuern nur " + caught + "/6 gefangen");
            Check(follow.Caught.Count == caught, "Fangliste");
            Check(inventory.Items.Any() || money.Balance > 0, "Fang nicht im Inventar");
            G1C_Item item = inventory.Items.FirstOrDefault(i => i.SellValue > 0);
            if (item != null)
            {
                int before = money.Balance;
                G1C_Shop.Sell(item, money, inventory);
                Check(money.Balance > before, item.Name + " ist nicht verkaufbar");
            }
            foreach (G1C_CatchInfo c in G1C_FishingGame.Catches.Where(c => c.ItemId != null))
            {
                Check(G1C_ItemCatalog.Get(c.ItemId) != null && G1C_ItemCatalog.Get(c.ItemId).SellValue > 0, "Fang fehlt im Katalog: " + c.ItemId);
                Check(HasBuiltInSprite("item_" + c.ItemId), "Icon fehlt: " + c.ItemId);
                if (c.IsFish) Check(HasBuiltInSprite(c.ItemId + "_l") && HasBuiltInSprite(c.ItemId + "_r"), "Fischgrafik fehlt: " + c.ItemId);
            }
            Equal(20, G1C_FishingGame.Catches.Count(c => c.IsFish), "Fischarten im Angelspiel");
            Check(!G1C_CheatCode.Sequence.Contains(Key.E) && !G1C_CheatCode.Sequence.Contains(Key.Q) && !G1C_CheatCode.Sequence.Contains(Key.W) && !G1C_CheatCode.Sequence.Contains(Key.S), "Cheat nutzt Tasten des Spielautomaten");
            Check(G1C_FishingGame.Catches.Any(c => c.ItemId == null) && G1C_FishingGame.Catches.Any(c => c.ItemId == "lucky_coin"), "Glücksfunde fehlen");
        }

        private static void FishingEscape()
        {
            G1C_FishingGame fishing = new G1C_FishingGame(new G1C_MoneyManager(0), new Random(4), new G1C_Inventory());
            fishing.Steering = () => -Math.Sign(fishing.FishPosition - fishing.ZoneCenter);
            fishing.StrikeHook();
            for (int i = 0; i < 3000 && fishing.State == G1C_FishingGame.Phase.Fight; i++) fishing.Update();
            Equal(G1C_FishingGame.Phase.Escaped, fishing.State, "Fisch entkommt beim Wegsteuern");
            fishing.HandleInput(G1C_InputEvent.FromKey(Key.E));
            Equal(G1C_FishingGame.Phase.Waiting, fishing.State, "Neu auswerfen");
        }

        private static void CasinoCheatJackpot()
        {
            G1C_CheatCode code = new G1C_CheatCode();
            foreach (Key k in new[] { Key.A, Key.D, Key.M, Key.I, Key.X }) code.Feed(k);
            Check(!code.JackpotActive, "Falsche Folge aktiviert den Cheat");
            bool completed = false;
            foreach (Key k in G1C_CheatCode.Sequence) completed = code.Feed(k);
            Check(completed && code.JackpotActive, "Richtige Folge aktiviert nicht");
            foreach (Key k in G1C_CheatCode.Sequence) code.Feed(k);
            Check(!code.JackpotActive, "Zweite Eingabe schaltet nicht aus");

            G1C_GameManager start = RunningGame();
            G1C_GameManager game = GameAt(G1C_WorldLocation.Exterior, start.Exterior.PlayerStartX, start.Exterior.PlayerStartY, G1C_Direction.Down);
            foreach (Key k in G1C_CheatCode.Sequence) game.HandleInput(G1C_InputEvent.FromKey(k));
            Check(!game.Cheats.JackpotActive, "Cheat darf außerhalb des Spielautomaten nicht wirken");
            // echter Weg: im Casino vor den Spielautomaten, Dialog bestätigen, bis der Automat läuft
            game = GameAt(G1C_WorldLocation.Casino, 3, 9, G1C_Direction.Up);
            for (int i = 0; i < 4 && !(game.ActiveGame is G1C_Slots); i++) game.HandleInput(G1C_InputEvent.FromKey(Key.E));
            Check(game.ActiveGame is G1C_Slots, "Spielautomat konnte nicht gestartet werden (Zustand " + game.State + ")");
            foreach (Key k in G1C_CheatCode.Sequence) game.HandleInput(G1C_InputEvent.FromKey(k));
            Check(game.Cheats.JackpotActive && game.SystemMessage.Contains("ADMIN"), "Cheat im Spielautomaten nicht aktiv: " + game.SystemMessage);
            for (int i = 0; i < 3; i++) game.Update();
            Equal(G1C_CheatCode.SpeedStepTicks, game.Player.StepTicks, "Cheat: Tempo nicht erhöht");
            Check(G1C_CheatCode.SpeedStepTicks < G1C_BikeSystem.RidingStepTicks, "Cheat-Tempo nicht schneller als Fahrrad");
            game.HandleInput(G1C_InputEvent.FromKey(Key.Q)); // Automaten verlassen: Tempo bleibt "unendlich" bestehen
            for (int i = 0; i < 400; i++) game.Update();
            Equal(G1C_CheatCode.SpeedStepTicks, game.Player.StepTicks, "Cheat-Tempo endet von selbst");
            game = GameAt(G1C_WorldLocation.Casino, 3, 9, G1C_Direction.Up);
            for (int i = 0; i < 4 && !(game.ActiveGame is G1C_Slots); i++) game.HandleInput(G1C_InputEvent.FromKey(Key.E));
            foreach (Key k in G1C_CheatCode.Sequence) game.HandleInput(G1C_InputEvent.FromKey(k));
            foreach (Key k in G1C_CheatCode.Sequence) game.HandleInput(G1C_InputEvent.FromKey(k));
            Check(!game.Cheats.JackpotActive, "ADMIN erneut schaltet nicht aus");
            for (int i = 0; i < 3; i++) game.Update();
            Equal(5, game.Player.StepTicks, "Nach dem Ausschalten Tempo nicht zurückgesetzt");
            for (int i = 0; i < 4 && !(game.ActiveGame is G1C_Slots); i++) game.HandleInput(G1C_InputEvent.FromKey(Key.E));
            foreach (Key k in G1C_CheatCode.Sequence) game.HandleInput(G1C_InputEvent.FromKey(k));
            Check(game.Cheats.JackpotActive, "ADMIN schaltet nicht wieder ein");

            G1C_MoneyManager money = new G1C_MoneyManager(1000);
            G1C_Slots slots = new G1C_Slots(money, new Random(2)) { GuaranteedJackpot = () => game.Cheats.JackpotActive };
            for (int spin = 0; spin < 3; spin++)
            {
                int before = money.Balance;
                slots.HandleInput(G1C_InputEvent.FromKey(Key.E));
                for (int i = 0; i < 80; i++) slots.Update();
                Check(slots.LastWasJackpot, "Kein Jackpot trotz Cheat (Drehung " + (spin + 1) + ")");
                Equal(before + 25 * 99, money.Balance, "Jackpot-Auszahlung (Einsatz 25)");
            }
            slots.HandleInput(G1C_InputEvent.FromKey(Key.E));
            for (int i = 0; i < 40; i++) slots.Update();
            Equal(G1C_Slots.CoinsForBet(25), slots.CoinRainCount, "Münzregen-Menge");
            Check(G1C_Slots.CoinsForBet(500) > G1C_Slots.CoinsForBet(100) && G1C_Slots.CoinsForBet(100) > G1C_Slots.CoinsForBet(10), "Münzregen wächst nicht mit dem Einsatz");
        }

        private static void CasinoPokerFan()
        {
            Equal(1.05, G1C_Poker.PlayerHandScale, "Skalierung der eigenen Hand");
            Check(G1C_Poker.PlayerHandTop > 166, "Hand nicht tiefer als vorher (166)");
            Check(G1C_CardView.FanAngle(0, 2) < 0 && G1C_CardView.FanAngle(1, 2) > 0 &&
                  Math.Abs(G1C_CardView.FanAngle(0, 2) + G1C_CardView.FanAngle(1, 2)) < 0.001, "Fächer nicht symmetrisch");
            Equal(0.0, G1C_CardView.FanAngle(2, 5), "Mittlere Karte gerade");
        }

        private static void CasinoClawSignRemoved()
        {
            G1C_GameManager game = new G1C_GameManager();
            Check(!game.World.Objects.Any(o => o.Lines.Any(l => l.Contains("umgezogen"))), "Umzugsschild steht noch im Casino");
            Check(!game.World.Objects.Any(o => o.Interaction == G1C_InteractionType.Claw), "Greifautomat steht noch im Casino");
        }

        private static void ShopGasStationLook()
        {
            G1C_GameManager game = new G1C_GameManager();
            G1C_ExteriorMap map = game.Exterior.Map;
            G1C_ShopWindow window = game.Exterior.Objects.OfType<G1C_ShopWindow>().First();
            Equal(2, window.Width, "Breites Verkaufsfenster");
            foreach (G1C_WorldObject pump in game.Exterior.Objects.Where(o => o.SpriteId == "obj_fuel_pump"))
                Equal("tile_city_plaza", map.GetTileSpriteId(pump.X, pump.Y), "Zapfsäule nicht auf Steinplatten");
            G1C_SpriteLibrary sprites = new G1C_SpriteLibrary(Path.Combine(Path.GetTempPath(), "g1c_no_assets"));
            System.Windows.Media.Imaging.BitmapSource b = sprites.Get("building_gas_station");
            System.Windows.Media.Imaging.FormatConvertedBitmap bgra = new System.Windows.Media.Imaging.FormatConvertedBitmap(b, System.Windows.Media.PixelFormats.Bgra32, null, 0);
            Func<int, int, byte[]> px = (x, y) => { byte[] p = new byte[4]; bgra.CopyPixels(new System.Windows.Int32Rect(x, y, 1, 1), p, 4, 0); return p; };
            byte[] roof = px(48, 20), wall = px(20, 46), door = px(55, 70);
            Check(roof[2] > 200 && roof[1] < 80, "Dach nicht rot");
            Check(wall[0] > 230 && wall[1] > 230 && wall[2] > 230, "Wand nicht weiß");
            Check(door[2] > door[0] + 40 && door[2] < 180, "Tür nicht braun (Holz)");
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
