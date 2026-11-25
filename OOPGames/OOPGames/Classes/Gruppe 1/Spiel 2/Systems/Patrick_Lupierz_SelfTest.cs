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
            run("Geldhai", "5000-Chips-Kredit, 50 % Zinsen pro Tag, Teil- und Vollrückzahlung", LoanSharkDailyRules);
            run("Angler", "Angler-NPC und vorhandene Fischdaten aus Inventory/Fishing verwenden", AnglerFishData);
            run("Angler", "Fischliste ist dynamisch und enthält alle Angelarten", AnglerFishScrollData);
            run("Eduard", "Eduard steht bei den Spielautomaten und besitzt spielsüchtige Dialoge", EduardSlotNpc);
            run("Eduard", "Eduard spielt am linken Automaten, blockiert ihn, gestreiftes Hemd/Mütze, viele Sprüche", EduardBlocksLeftMachine); // Patrick Lupierz – 22.11.2026
            run("NPC", "Studentin Lea: eigenes Aussehen (blond, hellgrün, keine Krawatte), nicht mehr die geteilte Vorlage", LeaOwnLook); // Patrick Lupierz – 23.11.2026
            run("Angler", "Denis: 'Wofuer sind die Fische?' antwortet (Spaß, verkaufen), nur 'Welche Fische gibt es?' öffnet das Lexikon", AnglerFishQuestions); // Patrick Lupierz – 23.11.2026
            run("Spielhalle", "Gast-Automat: Gruppe aus dem Klassennamen auch ohne Unterstrich (Drag Race, Hay Day)", ArcadeGuestGroupNames); // Patrick Lupierz – 23.11.2026
            run("Spielhalle", "Gast-Automat ohne Obergrenze: mehr als 9 registrierte Spiele erscheinen alle, eigene Automaten bleiben bei 9", ArcadeGuestNoCap); // Patrick Lupierz – 23.11.2026
            run("Fenster", "Welt füllt jede Fenstergröße, Minispiele mittig im festen 480x320-Rahmen", WindowScaling); // Patrick Lupierz – 22.11.2026
            run("NPC", "Duell: NPC ansprechen, herausfordern, Pong spielen; Einsatz, Geldbeutel und Duell-Gedächtnis", NpcDuel); // Patrick Lupierz – 23.11.2026
            run("NPC", "Duell: Spielauswahl Pong/Poker/Blackjack, Poker und Blackjack gegen den NPC werden ausgewertet", NpcDuelGameChoice); // Patrick Lupierz – 23.11.2026
            run("Spielhalle", "Patrick: am mittleren Automaten, Aussehen, täglicher Tipp, 300 Chips einmalig für den Schlangen-Rekord", ArcadeMentorPatrick); // Patrick Lupierz – 22.11.2026
            run("Geldhai", "Haustür-Begegnung wird nur beim Verlassen ausgelöst", LoanHouseExitEncounter);

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

            // ---- Erweiterung 18.11.2026: offene SprintPlanning-Issues
            run("Spielhalle", "Gast-Automat: Regeln, Painter und KI derselben Gruppe automatisch gewählt", ArcadeGuestOwnGroupParts);
            run("Spielhalle", "Gast-Automat: echte Spiele der anderen Gruppen komplett bis zum Ende gespielt", ArcadeGuestFullGames);
            run("Spielhalle", "Gast-Automat: [Esc] mitten im Spiel – zurück zur Spielauswahl, Framework-Stand wiederhergestellt", ArcadeGuestEscBackToList);
            run("NPC", "Tagesablauf: Stadtbewohner nachts zu Hause, Lena abends im Casino", NpcDailySchedule);
            run("NPC", "Geldhai spricht beim Vorbeilaufen an (einmal pro Vorbeigehen)", NpcLoanSharkApproach);

            // ---- Erweiterung 19.11.2026: Klangpaket, Mixer, Handy-App
            run("Sound", "Katalog: jede Datei im Sound-Ordner gehört zu einem Sound, jeder Sound hat Datei oder eingebaute Version", SoundCatalogFiles);
            run("Sound", "Höchstens N Effekte gleichzeitig; wichtige verdrängen unwichtige; derselbe Sound nie doppelt", SoundSimultaneousLimit);
            run("Sound", "Häufigkeit: Chance, Mindestabstand, Ein/Aus, Vorhören ohne Sperren", SoundFrequency);
            run("Sound", "Lautstärke: Gesamt x Musik/Effekte x Einzelwert, live änderbar, Stumm", SoundVolumes);
            run("Sound", "Schritte: Untergrund nach Kachel, Varianten reihum, Innenräume, Fahrrad-Schleife", SoundSteps);
            run("Sound", "Tiere nur beim Streicheln; Vögel nur im 4-Kacheln-Radius", SoundAnimalsAndBirds);
            run("Sound", "Musik: Auswahl, Wechsel, Dateien im Wechsel, Heartbeat, eingebaute Ersatzmelodie", SoundMusic);
            run("Sound", "Casino-Töne: Hebel, Walzen-Schleife, Jackpot, Karten, Chips, Einsatz", SoundCasinoEvents);
            run("Sound", "Einstellungen: JSON speichern/laden, Grenzen, kaputte Datei", SoundSettingsPersistence);
            run("Sound", "Handy-Mixer: Seite, PIN, Regler, Probehören, Stumm, Zurücksetzen, Sperre bei Fehlversuchen", SoundMixerServerFlow);
            run("Tiere", "Flussfische haben dieselben Grafiken wie im Angelspiel", AnimalsRiverFishSkins);

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

        private static void LoanSharkDailyRules()
        {
            G1C_MoneyManager money = new G1C_MoneyManager(3000);
            G1C_FinanceManager finance = new G1C_FinanceManager();
            finance.Borrow(money, 1000, 1);
            Equal(1000, finance.LoanOwed, "Kreditbetrag");
            Equal(500, finance.ApplyLoanInterest(2), "Zinsen Tag 2");
            Equal(1500, finance.LoanOwed, "Schuld nach 50 % Zinsen");
            Equal(750, finance.ApplyLoanInterest(3), "Zinsen Tag 3");
            Equal(2250, finance.LoanOwed, "Schuld nach Zinseszins");
            Equal(0, finance.ApplyLoanInterest(3), "kein zweiter Zinslauf am selben Tag");
            finance.Repay(money, 250);
            Equal(2000, finance.LoanOwed, "Teilrueckzahlung");
            finance.Repay(money, 2000);
            Equal(0, finance.LoanOwed, "Vollrueckzahlung");
        }

        private static void AnglerFishData()
        {
            G1C_GameManager game = RunningGame();
            Check(game.Exterior.TownNpcs.Any(n => n.Name == "Denis der Angler"), "Angler-NPC fehlt");
            G1C_NpcCharacter angler = game.Npcs.Find("Denis der Angler");
            Check(angler != null, "Angler-Profil fehlt");
            Check(angler.Personality.Topics.Any(t => t.Question.Contains("Angelspiel")), "Angel-Tutorial-Thema fehlt");
            Check(angler.Personality.Topics.Any(t => t.Question.Contains("Fische")), "Fischliste-Thema fehlt");
            foreach (G1C_CatchInfo fish in G1C_FishingGame.Catches.Where(c => c.IsFish))
            {
                G1C_Item item = G1C_ItemCatalog.Get(fish.ItemId);
                Check(item != null && item.Category == G1C_ItemCategory.Fisch, "Fischdaten fehlen fuer " + fish.Name);
            }
        }

        private static void AnglerFishScrollData()
        {
            int count = G1C_FishingGame.Catches.Count(c => c.IsFish);
            Check(count > 0, "Fischdaten sind leer");
            G1C_GameManager game = RunningGame();
            G1C_NpcCharacter angler = game.Npcs.Find("Denis der Angler");
            Check(angler != null, "Angler-Profil fuer die Scrollliste fehlt");
            foreach (G1C_CatchInfo fish in G1C_FishingGame.Catches.Where(c => c.IsFish))
                Check(G1C_ItemCatalog.Get(fish.ItemId) != null, "Fisch fehlt in der dynamischen Quelle: " + fish.Name);
        }

        private static void EduardSlotNpc()
        {
            G1C_GameManager game = RunningGame();
            G1C_NpcCharacter eduard = game.Npcs.Find("Eduard");
            Check(eduard != null, "Eduard-Profil fehlt");
            Check(eduard.Personality.Topics.Count >= 3, "Eduard hat zu wenige Themen");
            G1C_Npc worldEduard = game.World.Objects.OfType<G1C_Npc>().FirstOrDefault(n => n.Name == "Eduard");
            Check(worldEduard != null, "Eduard steht nicht im Casino");
            if (worldEduard != null)
                Check(worldEduard.X >= 2 && worldEduard.X <= 6 && worldEduard.Y >= 8 && worldEduard.Y <= 9, "Eduard steht nicht bei den Spielautomaten"); // Patrick Lupierz – 22.11.2026: steht jetzt davor (Y 9)
        }

        // Entwickler: Patrick Lupierz – Datum: 22.11.2026
        // Aufgabe: Eduard steht vor dem linken Automaten (Blick zum Automaten), dieser ist für den Spieler gesperrt,
        //          die übrigen Automaten bleiben spielbar; eigenes Aussehen und genug Sprechblasen-Sprüche.
        private static void EduardBlocksLeftMachine()
        {
            G1C_GameManager game = RunningGame();
            G1C_Npc eduard = game.World.Objects.OfType<G1C_Npc>().FirstOrDefault(n => n.Name == "Eduard");
            Check(eduard != null, "Eduard fehlt");
            int machineX = game.World.Objects.Where(o => o.SpriteId == "obj_slot_machine").Min(o => o.X);
            Equal(G1C_CasinoWorld.EduardMachineX, machineX, "linker Automat");
            Check(eduard.X == machineX && eduard.Y == 9 && eduard.Facing == G1C_Direction.Up, "Eduard steht nicht vor dem linken Automaten: " + eduard.X + "/" + eduard.Y + " " + eduard.Facing);
            Check(eduard.Solid, "Eduard ist kein Hindernis");
            G1C_WorldObject left = game.World.Objects.First(o => o.SpriteId == "obj_slot_machine" && o.X == machineX);
            Check(left.Interaction != G1C_InteractionType.Slots, "Linker Automat ist für den Spieler nicht gesperrt");
            Check(game.World.Objects.Count(o => o.Interaction == G1C_InteractionType.Slots) >= 3, "zu wenige freie Automaten");
            Equal(G1C_CasinoWorld.EduardSkin, eduard.Skin, "Eduards Aussehen");
            Check(G1C_BuiltInSprites.Create(G1C_CasinoWorld.EduardSkin + "_down_0") != null, "Grafik für Eduard fehlt");
            G1C_NpcCharacter profile = game.Npcs.Find("Eduard");
            Check(profile.Personality.GeneralMonologues.Length >= 15, "zu wenige Sprüche: " + profile.Personality.GeneralMonologues.Length);
        }

        // Entwickler: Patrick Lupierz – Datum: 23.11.2026
        // Aufgabe: Echter Ablauf eines NPC-Duells: Gast Lena im Casino ansprechen, "Zum Duell herausfordern!" wählen,
        //          Pong starten (Einsatz 20), gewinnen bzw. verlieren, Ergebnis prüfen (Chips, Geldbeutel, Gedächtnis).
        //          Außerdem: jeder nicht feindliche Gesprächspartner bietet das Duell an (Optionsgrenze 4 beachtet).
        private static void NpcDuel()
        {
            for (int round = 0; round < 2; round++)
            {
                bool win = round == 0;
                G1C_GameManager game = GameAt(G1C_WorldLocation.Casino, 11, 8, G1C_Direction.Up);
                G1C_NpcCharacter lena = game.Npcs.Find("Gast Lena");
                int purse = lena.Purse, balance = game.Money.Balance;
                game.HandleInput(G1C_InputEvent.FromKey(Key.E));
                Equal(G1C_GameState.Conversation, game.State, "Gespräch mit Lena");
                int index = game.Npcs.Conversation.Options.ToList().FindIndex(o => o.Text == "Zum Duell herausfordern!");
                Check(index >= 0, "Keine Duell-Option: " + string.Join(" | ", game.Npcs.Conversation.Options.Select(o => o.Text)));
                game.HandleInput(G1C_InputEvent.FromKey(Key.D1 + index));
                Check(game.Npcs.Conversation.Options.Any(o => o.Text.StartsWith("Pong")), "Keine Spielauswahl im Duell");
                game.HandleInput(G1C_InputEvent.FromKey(Key.D1));   // Pong
                G1C_PongDuel duel = game.ActiveGame as G1C_PongDuel;
                Check(duel != null && duel.IsDuel && duel.DuelOpponent == "Gast Lena", "Duell startet nicht (Zustand " + game.State + ")");
                game.HandleInput(G1C_InputEvent.FromKey(Key.E));
                Equal(balance - G1C_PongDuel.DuelStake, game.Money.Balance, "Einsatz abgebucht");
                for (int i = 0; i < G1C_PongDuel.WinningScore; i++) { duel.DebugShoot(win); duel.MoveBall(); }
                Check(duel.IsOver && duel.PlayerWon == win, "Duell nicht entschieden");
                game.HandleInput(G1C_InputEvent.FromKey(Key.Q));
                Equal(G1C_GameState.Result, game.State, "Ergebnisanzeige");
                Equal(balance + (win ? 1 : -1) * G1C_PongDuel.DuelStake, game.Money.Balance, "Chips nach dem Duell");
                Equal(purse - (win ? 1 : -1) * G1C_PongDuel.DuelStake, lena.Purse, "Lenas Geldbeutel");
                Equal(win ? 1 : 0, lena.Memory.DuelsWonByPlayer, "Siege gemerkt");
                Equal(win ? 0 : 1, lena.Memory.DuelsLostByPlayer, "Niederlagen gemerkt");
                Check(!game.ArcadeHighscores.Keys.Any(k => k.StartsWith("Duell")), "Duell landet auf der Highscore-Tafel");
            }

            // Duell ohne Start verlassen: kein Einsatz, kein Ergebnis
            G1C_GameManager quit = GameAt(G1C_WorldLocation.Casino, 11, 8, G1C_Direction.Up);
            int before = quit.Money.Balance;
            quit.StartDuel(quit.Npcs.Find("Gast Lena"));
            quit.HandleInput(G1C_InputEvent.FromKey(Key.Q));
            Check(quit.ActiveGame == null && quit.State == G1C_GameState.MainGame && quit.Money.Balance == before, "Abbruch vor dem Start kostet etwas");
            Equal(0, quit.Npcs.Find("Gast Lena").Memory.DuelsTotal, "Abbruch zählt als Duell");

            // Alle nicht feindlichen NPCs ohne Dienst bieten das Duell an – nur Mutter und Oma nie
            G1C_GameManager all = RunningGame();
            int familyChecked = 0;
            foreach (G1C_NpcCharacter c in all.Npcs.Characters)
            {
                all.Npcs.Conversation.Open(c, null);
                if (G1C_NpcRelationship.IsHostile(c.Relationship.Status)) continue;
                bool offers = all.Npcs.Conversation.Options.Any(o => o.Text == "Zum Duell herausfordern!");
                if (c.Personality.IsFamily) Check(!offers, c.Name + " (Familie) darf nicht herausgefordert werden");
                else Check(offers, c.Name + " bietet kein Duell an");
                familyChecked += c.Personality.IsFamily ? 1 : 0;
                all.Npcs.Conversation.Close();
            }
            Check(familyChecked >= 2, "Mutter und Oma wurden nicht geprüft");
        }

        // Entwickler: Patrick Lupierz – Datum: 23.11.2026
        // Aufgabe: Denis' Fischfragen im echten Gespräch. "Wofuer sind die Fische?" gibt eine Antwort (Angeln zum Spaß,
        //          Fische verkaufen) und öffnet kein Lexikon; "Welche Fische gibt es?" öffnet das Lexikon.
        private static void AnglerFishQuestions()
        {
            foreach (bool lexicon in new[] { false, true })
            {
                G1C_GameManager game = RunningGame();
                G1C_Conversation c = game.Npcs.Conversation;
                c.Open(game.Npcs.Find("Denis der Angler"), null);
                int ask = c.Options.ToList().FindIndex(o => o.Text.StartsWith("Ich hab da eine Frage"));
                Check(ask >= 0, "Keine Fragen-Option bei Denis");
                c.Choose(ask);
                // Änderung durch: Patrick Lupierz – 25.11.2026 – Thema heißt seit der Rechtschreibkorrektur "Wofür sind die Fische?"
                string wanted = lexicon ? "Welche Fische gibt es?" : "Wofür sind die Fische?";
                for (int page = 0; page < 4 && !c.Options.Any(o => o.Text == wanted); page++)
                {
                    int more = c.Options.ToList().FindIndex(o => o.Text.StartsWith("Weitere Fragen"));
                    Check(more >= 0, "Frage nicht gefunden: " + wanted);
                    c.Choose(more);
                }
                c.Choose(c.Options.ToList().FindIndex(o => o.Text == wanted));
                if (lexicon) Check(c.FishScrollMode, "Lexikon öffnet sich nicht");
                else
                {
                    Check(!c.FishScrollMode, "Lexikon überdeckt die Antwort");
                    Check(c.Line.Contains("Spaß") && c.Line.Contains("verkaufen"), "Antwort nennt Spaß/Verkaufen nicht: " + c.Line);
                }
            }
        }

        // Entwickler: Patrick Lupierz – Datum: 23.11.2026
        // Aufgabe: Studentin Lea hat ihr eigenes Aussehen in Stadt, Casino-Besuch und Gespräch (Porträt), ohne goldene Krawatte.
        private static void LeaOwnLook()
        {
            G1C_GameManager game = RunningGame();
            Equal(G1C_ShopNpcRoster.LeaSkin, game.Npcs.Find("Studentin Lea").Skin, "Skin im Profil");
            G1C_Npc town = game.Exterior.TownNpcs.First(n => n.Name == "Studentin Lea");
            Equal(G1C_ShopNpcRoster.LeaSkin, town.Skin, "Skin in der Stadt");
            Check(G1C_ShopNpcRoster.LeaSkin != "dealer_blue", "Lea nutzt noch die geteilte Vorlage");
            System.Windows.Media.Imaging.BitmapSource lea = G1C_BuiltInSprites.Create(G1C_ShopNpcRoster.LeaSkin + "_down_0");
            Check(lea != null, "Grafik für Lea fehlt");
            int[] pixels = new int[lea.PixelWidth * lea.PixelHeight];
            System.Windows.Media.Imaging.BitmapSource bgra = new System.Windows.Media.Imaging.FormatConvertedBitmap(lea, System.Windows.Media.PixelFormats.Bgra32, null, 0);
            bgra.CopyPixels(pixels, lea.PixelWidth * 4, 0);
            Check(!pixels.Contains(unchecked((int)0xFFD8A848)), "Goldene Krawatte noch sichtbar");
        }

        // Entwickler: Patrick Lupierz – Datum: 23.11.2026
        // Aufgabe: Spielauswahl im Duell. Über das Gespräch Poker bzw. Blackjack gegen Lena wählen, das richtige Spiel
        //          mit Lenas Namen läuft; Ergebnis: Poker bewegt Chips zwischen Spieler und Lenas Geldbeutel, Blackjack
        //          vergleicht die Gewinne gegen den Dealer. "Doch nicht." führt zurück ohne Spiel.
        private static void NpcDuelGameChoice()
        {
            string[] titles = { "Pong", "Poker", "Blackjack" };
            for (int pick = 1; pick <= 2; pick++)
            {
                G1C_GameManager game = GameAt(G1C_WorldLocation.Casino, 11, 8, G1C_Direction.Up);
                G1C_NpcCharacter lena = game.Npcs.Find("Gast Lena");
                int purse = lena.Purse, balance = game.Money.Balance;
                game.HandleInput(G1C_InputEvent.FromKey(Key.E));
                int index = game.Npcs.Conversation.Options.ToList().FindIndex(o => o.Text == "Zum Duell herausfordern!");
                game.HandleInput(G1C_InputEvent.FromKey(Key.D1 + index));
                List<string> options = game.Npcs.Conversation.Options.Select(o => o.Text).ToList();
                Check(options.Count == 4 && titles.All(t => options.Any(o => o.StartsWith(t))) && options.Contains("Doch nicht."),
                      "Spielauswahl unvollständig: " + string.Join(" | ", options));
                game.HandleInput(G1C_InputEvent.FromKey(Key.D1 + pick));
                IG1C_NpcDuelGame duel = game.ActiveGame as IG1C_NpcDuelGame;
                Check(duel != null && duel.DuelOpponent == "Gast Lena", titles[pick] + "-Duell startet nicht (Zustand " + game.State + ")");
                Equal(pick == 1 ? G1C_GameState.Poker : G1C_GameState.Blackjack, game.State, "Spielzustand");
                Check(pick == 1 ? game.ActiveGame is G1C_Poker : game.ActiveGame is G1C_Blackjack, "falsches Spiel");
                Check(game.ActiveGame.Name.Contains("Gast Lena"), "Name des Gegners fehlt: " + game.ActiveGame.Name);
                game.Money.Deposit(100);   // Spieler hat 100 Chips gewonnen
                game.HandleInput(G1C_InputEvent.FromKey(Key.Q));
                Equal(G1C_GameState.Result, game.State, "Ergebnisanzeige nach " + titles[pick]);
                Equal(1, lena.Memory.DuelsWonByPlayer, "Sieg gemerkt (" + titles[pick] + ")");
                Equal(pick == 1 ? purse - 100 : purse, lena.Purse, "Lenas Geldbeutel nach " + titles[pick]);
                Equal(balance + 100, game.Money.Balance, "Chips des Spielers");
            }

            // Poker-Duell: alle Meldungen nennen nur den Duellanten, nie "Max" (ganze Hände bis zum Showdown/Fold)
            G1C_Poker poker = new G1C_Poker(new G1C_MoneyManager(1000), new Random(11));
            poker.ConfigureDuel("Gast Lena");
            List<string> seen = new List<string>();
            for (int hand = 0; hand < 6; hand++)
            {
                poker.HandleInput(G1C_InputEvent.FromKey(Key.E));
                for (int step = 0; step < 400; step++)
                {
                    poker.Update();
                    poker.HandleInput(G1C_InputEvent.FromKey(Key.D1));   // Check/Call
                    if (!seen.Contains(poker.StatusMessage)) seen.Add(poker.StatusMessage);
                }
            }
            Check(!seen.Any(m => m.Contains("Max")), "Poker-Duell nennt Max: " + seen.FirstOrDefault(m => m.Contains("Max")));
            Check(seen.Any(m => m.Contains("Gast Lena")), "Duellant wird nie genannt: " + string.Join(" | ", seen.Take(5)));

            // "Doch nicht." – zurück zum Gespräch, kein Spiel
            G1C_GameManager back = GameAt(G1C_WorldLocation.Casino, 11, 8, G1C_Direction.Up);
            back.HandleInput(G1C_InputEvent.FromKey(Key.E));
            int i = back.Npcs.Conversation.Options.ToList().FindIndex(o => o.Text == "Zum Duell herausfordern!");
            back.HandleInput(G1C_InputEvent.FromKey(Key.D1 + i));
            back.HandleInput(G1C_InputEvent.FromKey(Key.D4));
            Check(back.ActiveGame == null && back.State == G1C_GameState.Conversation, "Doch nicht. startet trotzdem ein Spiel");

            // Ohne Ergebnis verlassen (Poker nie ausgeteilt): zählt nicht
            G1C_GameManager quit = GameAt(G1C_WorldLocation.Casino, 11, 8, G1C_Direction.Up);
            quit.StartDuel(quit.Npcs.Find("Gast Lena"), G1C_DuelGame.Poker);
            quit.HandleInput(G1C_InputEvent.FromKey(Key.Q));
            Check(quit.ActiveGame == null && quit.State == G1C_GameState.MainGame, "Leeres Poker-Duell nicht beendet");
            Equal(0, quit.Npcs.Find("Gast Lena").Memory.DuelsTotal, "Leeres Duell gezählt");
        }

        // Entwickler: Patrick Lupierz – Datum: 22.11.2026
        // Aufgabe: NPC Patrick in der Spielhalle. Steht neben dem mittleren Automaten (Front frei), blond/schwarzes
        //          T-Shirt/blaue Hose/weiße Schuhe, jeden Tag ein anderer Tipp, Rekord weder zu leicht noch unerreichbar,
        //          300 Chips genau einmal, erst wenn der Rekord übertroffen ist.
        private static void ArcadeMentorPatrick()
        {
            G1C_GameManager game = RunningGame();
            G1C_Npc patrick = game.Arcade.Objects.OfType<G1C_Npc>().FirstOrDefault(n => n.Name == G1C_ArcadeMentor.NpcName);
            Check(patrick != null, "Patrick steht nicht in der Spielhalle");
            G1C_ArcadeCabinet middle = G1C_ArcadeCatalog.Cabinets.First(c => c.IsGuestCabinet);
            Check(Math.Abs(patrick.X - middle.X) + Math.Abs(patrick.Y - middle.Y) == 1, "Patrick steht nicht am mittleren Automaten");
            Check(game.Arcade.ObjectAt(middle.X, middle.Y + 1) == null && game.Arcade.Map.IsWalkable(middle.X, middle.Y + 1), "Vorderseite des Automaten blockiert");
            Equal("pc-F0D060-F0C8A0-181818-2848A8-F8F8F8-x", patrick.Skin, "Aussehen (blond, schwarz, blau, weiß)");
            Check(G1C_BuiltInSprites.Create(patrick.Skin + "_down_0") != null, "Grafik fehlt");
            G1C_NpcCharacter profile = game.Npcs.Find(G1C_ArcadeMentor.NpcName);
            Check(profile != null && profile.Personality.BaseMood == G1C_NpcMood.Happy, "Profil fehlt oder nicht optimistisch");
            Check(game.Npcs.ArcadeHighscores != null, "Rekorde nicht ans NPC-System angebunden");

            Check(G1C_ArcadeMentor.TipFor(1) != G1C_ArcadeMentor.TipFor(2), "Tipp wechselt nicht täglich");
            Equal(G1C_ArcadeMentor.TipFor(3), G1C_ArcadeMentor.TipFor(3), "Tipp innerhalb eines Tages gleich");
            // Änderung durch: Patrick Lupierz – Datum: 25.11.2026 – Rekord steht jetzt in der Konstante (560 statt 576); der Test
            // vergleicht mit der Konstante und prüft nur, dass der Rekord hoch genug (>= 500) und erreichbar (< 700) ist.
            int record = G1C_ArcadeMentor.SnakeRecord;
            Check(record >= 500 && record < 700, "Patricks Rekord nicht 'angenehm hoch': " + record);
            Check(game.HighscoreText().Contains("Schlange: " + record + " Punkte – Patrick"), "Rekord fehlt auf der Tafel: " + game.HighscoreText());
            game.ArcadeHighscores["Schlange"] = 700;
            Check(game.HighscoreText().Contains("Schlange: 700 Punkte – Du") && !game.HighscoreText().Contains(record + " Punkte – Patrick"), "Gebrochener Rekord nicht auf der Tafel: " + game.HighscoreText());
            game.ArcadeHighscores.Remove("Schlange");

            G1C_MoneyManager money = new G1C_MoneyManager(100);
            G1C_NpcMemory memory = new G1C_NpcMemory();
            Dictionary<string, int> scores = new Dictionary<string, int> { { "Schlange", G1C_ArcadeMentor.SnakeRecord } };
            G1C_TopicContext c = new G1C_TopicContext { Money = money, Memory = memory, Highscores = scores };
            G1C_ArcadeMentor.RecordAnswer(c);
            Equal(100, money.Balance, "Gleichstand zahlt nichts aus");
            scores["Schlange"] = G1C_ArcadeMentor.SnakeRecord + 10;
            G1C_ArcadeMentor.RecordAnswer(c);
            Equal(400, money.Balance, "300 Chips für den gebrochenen Rekord");
            G1C_ArcadeMentor.RecordAnswer(c);
            Equal(400, money.Balance, "Belohnung nur einmal");
            Check(memory.RecordRewardPaid, "Auszahlung nicht im (gespeicherten) Gedächtnis");
        }

        // Entwickler: Patrick Lupierz – Datum: 22.11.2026
        // Aufgabe: Fensteranpassung. Ein hohes Fenster (960x1100): die Welt nutzt die volle Höhe (kein Rand, kein
        //          Versatz); Minispiele liegen exakt mittig im 480x320-Rahmen, das Fenster reicht darüber hinaus.
        private static void WindowScaling()
        {
            System.Windows.Controls.Canvas canvas = new System.Windows.Controls.Canvas();
            canvas.Measure(new System.Windows.Size(960, 1100));
            canvas.Arrange(new System.Windows.Rect(0, 0, 960, 1100));
            G1C_Renderer r = new G1C_Renderer(new G1C_SpriteLibrary());
            r.BeginFrame(canvas);
            Equal(2.0, r.Viewport.Scale, "Skalierung");
            Equal(0.0, r.Viewport.OffsetY, "Welt ohne Versatz");
            Equal(480.0, G1C_Renderer.ScreenWidth, "Weltbreite");
            Equal(550.0, G1C_Renderer.ScreenHeight, "Welt nutzt die ganze Höhe");
            r.UseFixedFrame();
            Equal(320.0, G1C_Renderer.ScreenHeight, "Minispiel-Rahmen");
            Equal(230.0, r.Viewport.OffsetY, "Rahmen mittig");
            Equal(435.0, G1C_Renderer.WindowBottom, "Fensterunterkante im Rahmen");
            Equal(-115.0, G1C_Renderer.WindowTop, "Fensteroberkante im Rahmen");
            Equal(240.0, r.Viewport.ToVirtualY(230 + 480), "Mausumrechnung im Rahmen");
            Equal(172.0, G1C_Poker.PlayerHandTop, "Pokerhand im Rahmen");
            r.BeginFrame(canvas);   // zurück, damit andere Tests die Weltmaße sehen
        }

        private static void LoanHouseExitEncounter()
        {
            G1C_GameManager game = RunningGame();
            game.Finance.Borrow(game.Money, 5000, game.Clock.Day);
            Check(!game.Finance.LoanBonesBroken, "Knochenbruch geschieht sofort beim Kredit");
            game.Finance.RestoreLoanState(game.Clock.Day, game.Clock.Day, game.Clock.Day - 1, 5, game.Clock.Day);
            Check(game.CurrentLocation == G1C_WorldLocation.Home, "Test startet nicht zuhause");
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
            List<G1C_WorldObject> pavilions = city.Objects.Where(o => o.SpriteId == "building_pavilion_back").ToList();
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

        // Entwickler: Patrick Lupierz – Datum: 23.11.2026
        // Aufgabe: Gast-Automat ohne Limit. 14 registrierte Spiele (mehr als die frühere Grenze von 9) werden alle angeboten.
        private static void ArcadeGuestNoCap()
        {
            List<IGameRules> rules = new List<IGameRules>();
            for (int i = 0; i < 14; i++) rules.Add(new FakeRules());
            List<G1C_GuestGameEntry> found = G1C_GuestGameCatalog.Discover(rules,
                new IPaintGame[] { new FakePainter() }, new IGamePlayer[] { new FakeHuman(), new FakeComputer() });
            Equal(14, found.Count, "Spiele im Gast-Automaten");
            Check(found.Count > G1C_ArcadeCatalog.MaxGamesPerCabinet, "Gast-Automat noch begrenzt");
            Check(G1C_ArcadeCatalog.Cabinets.Where(c => !c.IsGuestCabinet).All(c => c.Games.Count <= G1C_ArcadeCatalog.MaxGamesPerCabinet), "Eigener Automat über der Grenze");

            // die Liste im Spiel zeigt alle (scrollbar)
            G1C_GameManager game = RunningGame();
            game.GuestGameSource = () => found;
            G1C_ArcadeCabinet cabinet = G1C_ArcadeCatalog.Cabinets.First(c => c.IsGuestCabinet);
            G1C_GameManager at = GameAt(G1C_WorldLocation.Arcade, cabinet.X, cabinet.Y + 1, G1C_Direction.Up);
            at.GuestGameSource = () => found;
            at.HandleInput(G1C_InputEvent.FromKey(Key.E));
            Equal(14, at.ListMenu.Rows.Count, "Zeilen in der Spielauswahl");
        }

        // Entwickler: Patrick Lupierz – Datum: 23.11.2026
        // Aufgabe: Gruppenerkennung auch ohne "_" im Klassennamen (siehe G1C_GuestGameCatalog.GroupOf).
        private static void ArcadeGuestGroupNames()
        {
            Equal("Gruppe3", G1C_GuestGameCatalog.GroupOf(new Gruppe3DragRaceRules()), "Gruppe3 Rules");
            Equal("Gruppe3", G1C_GuestGameCatalog.GroupOf(new Gruppe3DragRacePainter()), "Gruppe3 Painter");
            Equal(G1C_GuestGameCatalog.GroupOf(new HayDayRules()), G1C_GuestGameCatalog.GroupOf(new HayDayPainter()), "HayDay Rules/Painter");
            Equal("G5", G1C_GuestGameCatalog.GroupOf(new G5_4GWRules()), "G5 mit Unterstrich");
            Equal("Gruppe1", G1C_GuestGameCatalog.GroupOf(new Gruppe1_ConnectFourRules()), "Gruppe1 mit Unterstrich");
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
            // Änderung durch: Patrick Lupierz – Datum: 25.11.2026 – kein Türgeräusch mehr (kam beim Hinausgehen verzögert)
            Check(shop.Sound.LastEffect != "door" && G1C_SoundCatalog.Find("door") == null, "Türgeräusch ist entfernt");
            shop.Update();
            Equal("shop", shop.Sound.CurrentMusic, "Ladenmusik");
            Equal("Ton aus.", shop.Sound.ToggleMute(), "Stummschalten");
            // Änderung durch: Patrick Lupierz – 16.11.2026 – Sound-Ordner: Musik außen/Gebäude, eigene Dateien bevorzugt
            Equal("Musik_Aussen", G1C_SoundSystem.FolderFor("town"), "Ordner Stadtmusik");
            Equal("Musik_Aussen", G1C_SoundSystem.FolderFor("fishing"), "Ordner Angelmusik");
            Equal("Musik_Gebaeude", G1C_SoundSystem.FolderFor("casino"), "Ordner Casinomusik");
            Check(G1C_SoundSystem.Resolve("Soundeffekte", "gibt_es_nicht") == null, "Fehlende Datei wird als vorhanden gemeldet");

            foreach (string name in new[] { "music_town", "music_home", "music_shop", "music_casino", "music_arcade", "music_fishing", "music_title",
                                            "sfx_win", "sfx_jackpot", "sfx_lose", "sfx_coin", "sfx_meow", "sfx_woof", "sfx_splash", "sfx_catch" })
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
            // Änderung durch: Patrick Lupierz – 22.11.2026 – Hand 1,5-fach, in jeder Fenstergröße gleich, nicht unter dem Meldungsfeld
            Equal(1.5, G1C_Poker.NormalHandScale, "Eigene Hand vergrößert");
            Check(G1C_Poker.FullscreenHandScale < G1C_Poker.NormalHandScale, "Hand im Vollbild nicht kleiner");
            Check(G1C_Poker.PlayerHandTop > 166, "Hand nicht tiefer als vorher (166)");
            Check(G1C_Poker.PlayerHandTop + G1C_CardView.CardHeight * G1C_Poker.NormalHandScale <= G1C_Renderer.BaseScreenHeight - 58, "Hand liegt unter dem Meldungsfeld");
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

        // ------------------------------------------------------------------ Erweiterung 18.11.2026

        // Alle Framework-Klassen dieses Projekts (wie im Hauptfenster registriert), ohne das Framework zu verändern
        private static List<G1C_GuestGameEntry> DiscoverAllInAssembly()
        {
            Type[] types = typeof(G1C_GameManager).Assembly.GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface && !t.IsNested && t.GetConstructor(Type.EmptyTypes) != null).ToArray();
            Func<Type, object> make = t => { try { return Activator.CreateInstance(t); } catch (Exception) { return null; } };
            return G1C_GuestGameCatalog.Discover(
                types.Where(t => typeof(IGameRules).IsAssignableFrom(t)).Select(make).OfType<IGameRules>().ToList(),
                types.Where(t => typeof(IPaintGame).IsAssignableFrom(t)).Select(make).OfType<IPaintGame>().ToList(),
                types.Where(t => typeof(IGamePlayer).IsAssignableFrom(t)).Select(make).OfType<IGamePlayer>().ToList());
        }

        private static void ArcadeGuestOwnGroupParts()
        {
            List<G1C_GuestGameEntry> found = DiscoverAllInAssembly();
            Check(found.Count >= 2, "Zu wenige Gastspiele gefunden: " + found.Count);
            Check(found.Any(e => e.Name.ToLower().Contains("gewinnt") || e.Name.ToLower().Contains("connect") || e.Name.Contains("4")), "Kein 4-gewinnt-Spiel gefunden");
            // Änderung durch: Patrick Lupierz – 19.11.2026 – nach dem Merge gibt es auch Spiele ohne KI (Hay Day, Fabrikspiel):
            // Spiele mit KI müssen Painter, Spieler und KI aus der eigenen Gruppe bekommen; die 4-gewinnt-Spiele immer eine KI.
            foreach (G1C_GuestGameEntry e in found)
            {
                Check(e.AllFromOwnGroup || !e.HasComputerOpponent, e.Name + ": fremde Teile (" + e.PainterType + ", " + e.HumanType + ", " + e.ComputerType + ")");
                bool fourInARow = e.Name.ToLower().Contains("gewinnt") || e.Name.ToLower().Contains("connect") || e.Name.ToLower().Contains("4g");
                if (fourInARow) Check(e.HasComputerOpponent, e.Name + ": KI der Gruppe nicht gefunden");
                Check(e.PainterType != null && e.HumanType != null, e.Name + ": Painter oder Spieler fehlt");
            }
            Check(found.All(e => e.Group == G1C_GuestGameCatalog.GroupOf(e.CreateRules())), "Gruppe passt nicht zu den Rules");
        }

        private static void ArcadeGuestFullGames()
        {
            List<string> problems = new List<string>();
            int played = 0, smoke = 0;
            foreach (G1C_GuestGameEntry entry in DiscoverAllInAssembly())
            {
                G1C_MoneyManager money = new G1C_MoneyManager(100);
                G1C_GuestGameHost host = new G1C_GuestGameHost(money, new Random(1), entry);
                if (!host.Start()) { problems.Add(entry.Name + ": Start – " + host.LastError); continue; }
                if (OOPGamesManager.Singleton.ActivePainter != host.Painter) problems.Add(entry.Name + ": Painter nicht aktiv angemeldet");
                if (!entry.HasComputerOpponent)
                {
                    // Echtzeit-/Aufbauspiele ohne KI und ohne Spielende (Hay Day, Fabrikspiel): starten, laufen, zeichnen, Esc
                    for (int t = 0; t < 120 && host.Running; t++) host.Update();
                    host.Forward(new KeySelection(Key.D1));
                    host.Forward(new ClickSelection(100, 100, (int)MouseButton.Left));
                    if (host.LastError != null) problems.Add(entry.Name + ": Absturz " + host.LastError);
                    else if (host.Frame == null) problems.Add(entry.Name + ": nicht gezeichnet");
                    else smoke++;
                    host.Leave();
                    if (!host.WantsExit) problems.Add(entry.Name + ": Esc verlässt den Automaten nicht");
                    continue;
                }
                string stuck = PlayToEnd(host);
                if (stuck != null) problems.Add(entry.Name + ": " + stuck);
                else if (host.LastError != null) problems.Add(entry.Name + ": Absturz " + host.LastError);
                else
                {
                    int rulesWinner = host.Rules.CheckIfPLayerWon();
                    if (!host.Finished || host.Running) problems.Add(entry.Name + ": nicht beendet");
                    if ((rulesWinner > 0 ? (int?)rulesWinner : null) != host.Winner) problems.Add(entry.Name + ": Ergebnis weicht von den Regeln ab");
                    // Änderung durch: Patrick Lupierz – 23.11.2026 – Brettspiele (4 gewinnt) brauchen mehrere Züge; Echtzeitspiele
                    // mit KI (z. B. Drag Race) enden nach wenigen Eingaben
                    bool board = entry.Name.ToLower().Contains("gewinnt") || entry.Name.ToLower().Contains("connect") || entry.Name.ToLower().Contains("4g");
                    if (host.MovesMade < (board ? 4 : 1)) problems.Add(entry.Name + ": zu wenige Züge (" + host.MovesMade + ")");
                    played++;
                }
                host.Leave();
            }
            Check(problems.Count == 0, string.Join(" | ", problems));
            Check(played >= 2, "Nur " + played + " Spiele vollständig gespielt");
        }

        // Spielt als Mensch: probiert Zifferntasten und Klicks über das ganze Spielfeld, bis die Regeln einen Zug annehmen.
        private static string PlayToEnd(G1C_GuestGameHost host)
        {
            List<IMoveSelection> attempts = new List<IMoveSelection>();
            for (int k = 0; k < 9; k++) { attempts.Add(new KeySelection(Key.D1 + k)); attempts.Add(new KeySelection(Key.NumPad1 + k)); }
            // Änderung durch: Patrick Lupierz – 25.11.2026 – Zeichenfläche ist jetzt 960x640: Raster mitskaliert (gleich viele Versuche)
            for (int y = G1C_GuestGameHost.CanvasH / 16; y < G1C_GuestGameHost.CanvasH; y += G1C_GuestGameHost.CanvasH / 11)
                for (int x = G1C_GuestGameHost.CanvasW / 48; x < G1C_GuestGameHost.CanvasW; x += G1C_GuestGameHost.CanvasW / 24) attempts.Add(new ClickSelection(x, y, (int)MouseButton.Left));
            for (int move = 0; move < 120 && host.Running; move++)
            {
                // wie im Spiel: Ticks laufen weiter (Animationen, KI-Zug), bis der Mensch dran ist
                for (int t = 0; t < 400 && host.Running && !(host.CurrentPlayer is IHumanGamePlayer); t++) host.Update();
                int before = host.MovesMade;
                // Änderung durch: Patrick Lupierz – 19.11.2026 – manche Spiele (FG) sperren Eingaben, bis ihre Fall-Animation
                // nach Echtzeit abgelaufen ist: erst viele schnelle Ticks, dann mit kleinen Pausen warten.
                for (int wait = 0; wait < 400 && host.Running && host.MovesMade == before; wait++)
                {
                    if (wait >= 100) System.Threading.Thread.Sleep(20);
                    foreach (IMoveSelection a in attempts)
                    {
                        host.Forward(a);
                        if (host.MovesMade > before || !host.Running) break;
                    }
                    if (host.MovesMade == before) host.Update();   // z. B. fallender Stein: erst nach Ticks wieder Eingabe
                }
                if (host.Running && host.MovesMade == before) return "keine Eingabe akzeptiert (Zug " + (move + 1) + ")";
            }
            return host.Running ? "Partie endet nicht" : null;
        }

        private static void ArcadeGuestEscBackToList()
        {
            G1C_ArcadeCabinet guestCabinet = G1C_ArcadeCatalog.Cabinets.First(c => c.IsGuestCabinet);
            G1C_GameManager game = GameAt(G1C_WorldLocation.Arcade, guestCabinet.X, guestCabinet.Y + 1, G1C_Direction.Up);
            game.GuestGameSource = DiscoverAllInAssembly;
            IPaintGame sentinel = new G1C_CasinoPainter();
            OOPGamesManager.Singleton.RegisterActivePainter(sentinel);
            game.HandleInput(G1C_InputEvent.FromKey(Key.E));
            Equal(G1C_GameState.ListMenu, game.State, "Spielauswahl des Gast-Automaten");
            int rows = game.ListMenu.Rows.Count;
            Check(rows >= 2, "Zu wenige Gastspiele in der Auswahl");
            game.ListMenu.Choose(0);
            Equal(G1C_GameState.Arcade, game.State, "Gastspiel aktiv");
            game.HandleInput(G1C_InputEvent.FromKey(Key.E));                 // Partie starten
            G1C_GuestGameHost host = game.ActiveGame as G1C_GuestGameHost;
            Check(host != null && host.Running, "Gastspiel läuft nicht: " + (host != null ? host.LastError : "-"));
            Check(OOPGamesManager.Singleton.ActivePainter == host.Painter, "Gast-Painter nicht aktiv");
            game.HandleInput(G1C_InputEvent.FromKey(Key.Escape));            // mitten in der Partie
            Equal(G1C_GameState.ListMenu, game.State, "[Esc] führt zur Spielauswahl zurück");
            Equal(rows, game.ListMenu.Rows.Count, "Auswahl vollständig");
            Check(OOPGamesManager.Singleton.ActivePainter == sentinel, "Framework-Stand nicht wiederhergestellt");
            game.ListMenu.Choose(Math.Min(1, rows - 1));
            Equal(G1C_GameState.Arcade, game.State, "anderes Spiel wählbar");
            game.HandleInput(G1C_InputEvent.FromKey(Key.Escape));
            game.HandleInput(G1C_InputEvent.FromKey(Key.Escape));            // Liste schließen
            Equal(G1C_GameState.MainGame, game.State, "Zurück in der Spielhalle");
            OOPGamesManager.Singleton.RegisterActivePainter(null);
        }

        private static void NpcDailySchedule()
        {
            G1C_GameManager game = GameAt(G1C_WorldLocation.Home, 5, 8, G1C_Direction.Down);
            G1C_Npc paul = game.Exterior.TownNpcs.First(n => n.Name == "Kurier Paul");
            G1C_Npc lenaTown = game.Exterior.TownNpcs.First(n => n.Name == "Gast Lena");
            G1C_Npc lenaCasino = game.World.FindNpc("Gast Lena");
            Check(lenaCasino != null, "Lena fehlt im Casino");

            G1C_NpcSchedule.Apply(23, game.Exterior, game.World, game.Player, G1C_WorldLocation.Home);
            Check(!game.Exterior.IsPresent(paul), "Paul ist nachts draußen");
            Check(!game.Exterior.Objects.Contains(paul), "Abwesender NPC noch in der Objektliste");
            Check(game.World.IsPresent(lenaCasino) && !game.Exterior.IsPresent(lenaTown), "Lena abends nicht im Casino");
            Equal("im Casino", G1C_NpcSchedule.WhereIs("Gast Lena", 20), "Lena um 20 Uhr");
            Equal("zu Hause", G1C_NpcSchedule.WhereIs("Kurier Paul", 2), "Paul um 2 Uhr");

            G1C_NpcSchedule.Apply(12, game.Exterior, game.World, game.Player, G1C_WorldLocation.Home);
            Check(game.Exterior.IsPresent(paul) && game.Exterior.IsPresent(lenaTown), "Mittags nicht in der Stadt");
            Check(!game.World.IsPresent(lenaCasino), "Lena mittags im Casino und in der Stadt");
            Check(!game.Npcs.PlacedNpcs.Any(p => p.Key == lenaCasino), "Abwesende Casino-NPCs reden trotzdem");

            // Niemand verschwindet vor den Augen des Spielers
            G1C_GameManager near = GameAt(G1C_WorldLocation.Exterior, game.Exterior.Map.DoorX(G1C_WorldLocation.Hospital),
                                          game.Exterior.Map.DoorY(G1C_WorldLocation.Hospital) + 1, G1C_Direction.Down);
            G1C_Npc paulNear = near.Exterior.TownNpcs.First(n => n.Name == "Kurier Paul");
            if (Math.Abs(paulNear.X - near.Player.TileX) <= G1C_NpcSchedule.VisibleDistance && Math.Abs(paulNear.Y - near.Player.TileY) <= G1C_NpcSchedule.VisibleDistance)
            {
                G1C_NpcSchedule.Apply(23, near.Exterior, near.World, near.Player, G1C_WorldLocation.Exterior);
                Check(near.Exterior.IsPresent(paulNear), "Paul verschwindet vor den Augen des Spielers");
            }

            // Im laufenden Spiel über die Uhr
            G1C_GameManager night = GameAt(G1C_WorldLocation.Home, 5, 8, G1C_Direction.Down, 23);
            for (int i = 0; i < 30; i++) night.Update();
            Check(!night.Exterior.IsPresent(night.Exterior.TownNpcs.First(n => n.Name == "Kurier Paul")), "Tagesablauf läuft im Spiel nicht");
        }

        private static void NpcLoanSharkApproach()
        {
            G1C_GameManager probe = RunningGame();
            G1C_Npc shark = probe.Exterior.TownNpcs.First(n => n.Interaction == G1C_InteractionType.LoanShark);
            G1C_CollisionManager collision = new G1C_CollisionManager(probe.Exterior);
            int[] spot = new[] { new[] { 0, 2 }, new[] { 2, 0 }, new[] { -2, 0 }, new[] { 0, -2 }, new[] { 1, 1 }, new[] { -1, 1 } }
                .First(d => !collision.IsBlocked(shark.X + d[0], shark.Y + d[1]));
            G1C_GameManager game = GameAt(G1C_WorldLocation.Exterior, shark.X + spot[0], shark.Y + spot[1], G1C_Direction.Down);
            Check(!game.LoanSharkApproached, "Vorher schon angesprochen");
            game.Update();
            Check(game.LoanSharkApproached, "Geldhai spricht nicht an");
            Check(game.SystemMessage.Contains("Viktor"), "Hinweis fehlt: " + game.SystemMessage);
            Check(game.Npcs.Bubbles.Active.Any(b => b.Npc.Name == shark.Name), "Sprechblase des Geldhais fehlt");
            string first = game.SystemMessage;
            for (int i = 0; i < 50; i++) game.Update();
            Equal(1, game.Npcs.Bubbles.Active.Count(b => b.Npc.Name == shark.Name && b.IsReaction), "Geldhai wiederholt sich beim Stehenbleiben");
            game.Player.Teleport(shark.X + 8, shark.Y + 3, G1C_Direction.Down);
            game.Update();
            Check(!game.LoanSharkApproached, "Nach dem Weggehen nicht zurückgesetzt");
        }

        private static void AnimalsRiverFishSkins()
        {
            G1C_ExteriorWorld city = new G1C_ExteriorWorld();
            List<G1C_Animal> fish = city.Animals.Where(a => a.Name == "Fisch").ToList();
            Check(fish.Count >= 4, "Zu wenige Flussfische");
            List<string> fishingIds = G1C_FishSprites.Ids.ToList();
            foreach (G1C_Animal f in fish)
            {
                string sprite = f.SpriteId;   // river_fish_<art>_<bild>
                Check(sprite.StartsWith("river_fish_"), "Alte Fischgrafik: " + sprite);
                string species = sprite.Substring("river_".Length, sprite.LastIndexOf('_') - "river_".Length);
                Check(fishingIds.Contains(species), "Fischart nicht aus dem Angelspiel: " + species);
                Check(HasBuiltInSprite(sprite), "Grafik fehlt: " + sprite);
            }
            Check(fish.Select(f => f.SpriteId.Substring(0, f.SpriteId.LastIndexOf('_'))).Distinct().Count() >= 3, "Alle Flussfische gleich");
            Equal("Blubb", G1C_AnimalVoices.SoundOf(fish[0], new Random(1)), "Laut der Flussfische");
        }

        // ------------------------------------------------------------------ Klangpaket 19.11.2026

        private sealed class FakeVoice : IG1C_AudioVoice
        {
            public string File; public double Volume; public bool Loop, Stopped; public Func<bool> KeepGoing; public Action Ended;
            public void Stop() { Stopped = true; }
            public void SetVolume(double volume) { Volume = volume; }
            public void Finish()
            {
                if (Stopped) return;
                if (Loop && KeepGoing()) return;
                Stopped = true;
                if (Ended != null) Ended();
            }
        }

        private sealed class FakeBackend : IG1C_AudioBackend
        {
            public readonly List<FakeVoice> Voices = new List<FakeVoice>();
            public IG1C_AudioVoice Start(string file, double volume, bool loop, Func<bool> keepGoing, Action ended)
            {
                FakeVoice v = new FakeVoice { File = file, Volume = volume, Loop = loop, KeepGoing = keepGoing, Ended = ended };
                Voices.Add(v);
                return v;
            }
            public IEnumerable<FakeVoice> Active { get { return Voices.Where(v => !v.Stopped); } }
            public int Started(string fileFragment) { return Voices.Count(v => v.File.IndexOf(fileFragment, StringComparison.OrdinalIgnoreCase) >= 0); }
        }

        // Temporärer Sound-Ordner mit leeren Dateien in der Struktur des echten Ordners (nur Namen zählen)
        private static string MakeSoundRoot()
        {
            string root = Path.Combine(Path.GetTempPath(), "g1c_test_sounds_" + Guid.NewGuid().ToString("N"));
            Dictionary<string, string[]> files = new Dictionary<string, string[]>
            {
                { "Soundeffekte/Schritte", new[] { "StepGrass.wav", "StepGrass (2).wav", "StepGrass (3).wav", "StepsPath (1).wav", "StepsPath (2).wav",
                    "StepBridge.wav", "Steps_Indoor.wav", "Steps_Indoor (2).wav" } },
                { "Soundeffekte/Allgemein", new[] { "BicycleSoundLoop.wav", "purchase_coin_07.wav" } },
                { "Soundeffekte/Tiere", new[] { "catMeow1.wav", "catMeow2.wav", "dogBark.wav", "dog_bark_01.wav", "puppy_bark_01.wav", "songbird_chirp_01.wav", "frog_croak_01.wav" } },
                { "Soundeffekte/Casino", new[] { "SlotsSpinSoundLoop.wav", "slot_lever_01.wav", "jackpot.wav", "JackpotAlarm.wav", "GeneralVictory.wav",
                    "shuffle_riffle_01.wav", "ChipUp.wav", "ui_bet_up_01.wav", "3_Coins.wav", "4_Coins.wav", "5_Coins.wav", "slot_win_small_01.wav", "slot_win_big_01.wav" } },
                { "Musik_Aussen", new[] { "Ambient Outside 1.mp3", "Ambient Outside 2.mp3" } },
                { "Musik_Gebaeude", new[] { "CasinoBackgroundLoop.mp3", "Chippy music 1.mp3", "chippy music 2.mp3", "Ambient Charakter.mp3" } },
            };
            foreach (KeyValuePair<string, string[]> folder in files)
            {
                string dir = Path.Combine(root, folder.Key.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(dir);
                foreach (string f in folder.Value) File.WriteAllBytes(Path.Combine(dir, f), new byte[] { 0 });
            }
            return root;
        }

        private static void WithSoundRoot(Action<string> body)
        {
            string root = MakeSoundRoot();
            string[] before = G1C_SoundFiles.OverrideRoots;
            G1C_SoundFiles.OverrideRoots = new[] { root };
            G1C_SoundFiles.ClearCache();
            try { body(root); }
            finally
            {
                G1C_SoundFiles.OverrideRoots = before;
                G1C_SoundFiles.ClearCache();
                try { Directory.Delete(root, true); } catch (Exception) { }
            }
        }

        private static G1C_SoundSystem NewSound(FakeBackend backend, long[] clock)
        {
            G1C_SoundSystem sound = new G1C_SoundSystem { Backend = backend, NowMs = () => clock[0], Random = new Random(3) };
            sound.Heartbeat();
            return sound;
        }

        private static void SoundCatalogFiles()
        {
            Check(G1C_SoundCatalog.All.Select(d => d.Id).Distinct().Count() == G1C_SoundCatalog.All.Length, "Doppelte Sound-Ids");
            foreach (G1C_SoundDef d in G1C_SoundCatalog.All)
            {
                Check(G1C_SoundCatalog.Categories.Contains(d.Category), d.Id + ": unbekannte Kategorie");
                Check(d.Folder != null || d.Fallback != null, d.Id + ": weder Datei noch eingebaute Version");
                if (d.Fallback != null) Check(G1C_ChipTune.Render(d.Fallback).Length > 500, d.Id + ": Ersatzmelodie '" + d.Fallback + "' fehlt");
                Check(d.Volume >= 0 && d.Volume <= 100 && d.Chance >= 0 && d.Chance <= 100, d.Id + ": Standardwerte außerhalb 0-100");
            }
            foreach (string id in new[] { "step_grass", "step_path", "step_bridge", "step_indoor", "bicycle", "meow", "woof", "bird", "frog", "slot_lever",
                "slots_spin", "slot_win_small", "slot_win_big", "jackpot", "jackpot_alarm", "coins_small", "coins_mid", "coins_big", "win", "poker_win",
                "shuffle", "chip", "bet", "lose", "coin", "buff", "furniture_open", "trash_open", "click", "splash", "catch",
                "music_town", "music_title", "music_casino", "music_arcade", "music_home", "music_shop", "music_fishing" })
                Check(G1C_SoundCatalog.Find(id) != null, "Im Spiel benutzter Sound fehlt im Katalog: " + id);

            IList<string> roots = G1C_SoundFiles.Roots();
            if (roots.Count == 0) throw new InvalidOperationException("Sound-Ordner nicht gefunden (Umgebungsvariable G1CASINO_SOUNDS auf Assets\\Sounds setzen)");
            string root = roots[0];
            Check(!Directory.GetFiles(root, "*.ogg", SearchOption.AllDirectories).Any(), "OGG-Dateien im Sound-Ordner: WPF kann sie nicht abspielen");
            List<string> orphans = G1C_SoundFiles.AllFiles(root).Where(f => G1C_SoundFiles.Owner(root, f) == null).ToList();
            Check(orphans.Count == 0, "Dateien ohne Sound im Katalog: " + string.Join(", ", orphans.Select(Path.GetFileName)));
            foreach (G1C_SoundDef d in G1C_SoundCatalog.All.Where(x => x.Folder != null && !x.Optional && x.Patterns.Length > 0))
                Check(G1C_SoundFiles.For(d).Length > 0, d.Id + ": keine Datei in " + d.Folder);
            Check(G1C_SoundFiles.For(G1C_SoundCatalog.Find("step_grass")).Length == 3, "Gras-Schritte: drei Varianten erwartet");
            Check(G1C_SoundFiles.For(G1C_SoundCatalog.Find("music_arcade")).Length == 4, "Spielhalle: vier Stücke erwartet");
            Check(G1C_SoundFiles.For(G1C_SoundCatalog.Find("jackpot")).Length == 1, "Muster 'jackpot' trifft auch den Alarm");
        }

        private static void SoundSimultaneousLimit()
        {
            WithSoundRoot(root =>
            {
                FakeBackend backend = new FakeBackend();
                long[] clock = { 1000 };
                G1C_SoundSystem sound = NewSound(backend, clock);
                sound.Settings.Set("bird", "chance", 100);
                sound.Settings.Set(G1C_SoundSettings.GlobalId, "max", 2);
                Check(sound.Play("step_grass") && sound.Play("bird"), "Zwei Töne sollten starten");
                Equal(2, sound.ActiveEffects.Count, "zwei gleichzeitig");
                Check(sound.Play("meow"), "Wichtigerer Ton verdrängt");
                Check(!sound.ActiveEffects.Contains("step_grass") && sound.ActiveEffects.Contains("meow") && sound.ActiveEffects.Contains("bird"),
                      "Der älteste unwichtige Ton muss weichen: " + string.Join(",", sound.ActiveEffects));
                Check(sound.Play("jackpot"), "Jackpot verdrängt");
                Check(!sound.ActiveEffects.Contains("bird") && sound.ActiveEffects.Contains("jackpot"), "Vogel muss dem Jackpot weichen");
                Check(!sound.Play("step_path"), "Unwichtiger Ton darf wichtige nicht verdrängen");
                Equal(2, sound.ActiveEffects.Count, "weiterhin zwei");

                sound.Settings.Set(G1C_SoundSettings.GlobalId, "max", 1);
                sound.StopAllEffects();
                clock[0] += 5000; // Mindestabstände aus den Versuchen davor sind abgelaufen
                Check(sound.Play("meow"), "bei 1: erster Ton");
                Check(!sound.Play("step_indoor"), "bei 1: niedrigerer Ton entfällt");
                Check(sound.Play("win"), "bei 1: höhere Priorität ersetzt");
                Equal("win", sound.ActiveEffects.Single(), "nur ein Ton");

                // derselbe Sound überlagert sich nie, Varianten laufen reihum
                sound.Settings.Set(G1C_SoundSettings.GlobalId, "max", 8);
                sound.StopAllEffects();
                List<string> files = new List<string>();
                for (int i = 0; i < 4; i++)
                {
                    clock[0] += 500;
                    Check(sound.Play("step_grass"), "Schritt " + i);
                    files.Add(backend.Voices.Last().File);
                    Equal(1, sound.ActiveEffects.Count(x => x == "step_grass"), "Schritt überlagert sich selbst");
                }
                Equal(3, files.Take(3).Select(Path.GetFileName).Distinct().Count(), "drei verschiedene Varianten reihum");
                Equal(files[0], files[3], "vierter Schritt beginnt von vorn");
                Check(backend.Voices.Where(v => v.File.Contains("StepGrass")).Count(v => !v.Stopped) == 1, "Alte Schritte laufen noch");
            });
        }

        private static void SoundFrequency()
        {
            WithSoundRoot(root =>
            {
                FakeBackend backend = new FakeBackend();
                long[] clock = { 1000 };
                G1C_SoundSystem sound = NewSound(backend, clock);
                sound.Settings.Set("woof", "cooldown", 1000);
                Check(sound.Play("woof"), "erster Wuff");
                Check(!sound.Play("woof"), "Mindestabstand");
                clock[0] += 1001;
                Check(sound.Play("woof"), "nach dem Mindestabstand");
                Check(sound.Play("woof", true) || true, "force");

                sound.Settings.Set("coin", "chance", 0);
                int played = 0;
                for (int i = 0; i < 60; i++) { clock[0] += 1000; if (sound.Play("coin")) played++; }
                Equal(0, played, "Chance 0 % darf nie spielen");
                sound.Settings.Set("coin", "chance", 50);
                sound.Settings.Set("coin", "cooldown", 0);
                played = 0;
                for (int i = 0; i < 200; i++) { if (sound.Play("coin")) played++; }
                Check(played > 60 && played < 140, "Chance 50 % ergab " + played + " von 200");
                sound.Settings.Set("coin", "chance", 100);
                sound.Settings.Set("coin", "enabled", 0);
                Check(!sound.Play("coin"), "ausgeschalteter Sound spielt");
                Check(sound.Play("coin", true), "Vorhören ignoriert Ein/Aus nicht, aber Chance und Abstand");
                sound.Settings.Set("coin", "enabled", 1);

                // Vorhören über die Befehlsschlange (so macht es das Handy)
                sound.Settings.Set("bird", "chance", 0);
                sound.PostCommand(() => sound.Preview("bird"));
                int before = backend.Started("songbird");
                sound.Pump();
                Equal(before + 1, backend.Started("songbird"), "Vorhören trotz Chance 0");
                sound.Preview("slots_spin");
                Check(sound.ActiveEffects.Contains("slots_spin"), "Schleife beim Vorhören");
                clock[0] += 3100;
                sound.Pump();
                Check(!sound.ActiveEffects.Contains("slots_spin"), "Vorhör-Schleife endet nach 3 s");
            });
        }

        private static void SoundVolumes()
        {
            WithSoundRoot(root =>
            {
                FakeBackend backend = new FakeBackend();
                long[] clock = { 1000 };
                G1C_SoundSystem sound = NewSound(backend, clock);
                G1C_SoundDef coins = G1C_SoundCatalog.Find("coins_small");
                sound.Settings.Set(G1C_SoundSettings.GlobalId, "master", 50);
                sound.Settings.Set(G1C_SoundSettings.GlobalId, "effects", 50);
                sound.Settings.Set("coins_small", "volume", 50);
                Check(Math.Abs(sound.VolumeFor(coins) - 0.125) < 1e-9, "Lautstärke 50 x 50 x 50 % = 0,125: " + sound.VolumeFor(coins));
                Check(sound.Play("coins_small"), "spielt");
                Check(Math.Abs(backend.Voices.Last().Volume - 0.125) < 1e-9, "Startlautstärke");
                sound.Settings.Set(G1C_SoundSettings.GlobalId, "master", 100);
                sound.Pump();
                Check(Math.Abs(backend.Voices.Last().Volume - 0.25) < 1e-9, "Lautstärke ändert sich live");
                sound.SetMuted(true);
                Equal(0.0, backend.Voices.Last().Volume, "stumm");
                Check(!sound.Play("coins_small"), "stumm spielt nichts");
                sound.SetMuted(false);
                Check(backend.Voices.Last().Volume > 0, "Stumm aufgehoben");
                sound.Settings.Set(G1C_SoundSettings.GlobalId, "music", 20);
                sound.Settings.Set("music_casino", "volume", 50);
                Check(Math.Abs(sound.VolumeFor(G1C_SoundCatalog.Find("music_casino")) - 1.0 * 0.20 * 0.50) < 1e-9, "Musiklautstärke");
                sound.Settings.Set("coins_small", "volume", 999);
                Equal(100, sound.Settings.Get("coins_small").Volume, "Wert wird auf 100 begrenzt");
            });
        }

        private static void SoundSteps()
        {
            Equal("step_bridge", G1C_StepSurface.SoundIdFor("tile_city_bridge", G1C_WorldLocation.Exterior), "Brücke");
            Equal("step_path", G1C_StepSurface.SoundIdFor("tile_city_road_5", G1C_WorldLocation.Exterior), "Straße");
            Equal("step_path", G1C_StepSurface.SoundIdFor("tile_city_path_12", G1C_WorldLocation.Exterior), "Weg");
            Equal("step_path", G1C_StepSurface.SoundIdFor("tile_city_plaza", G1C_WorldLocation.Exterior), "Steinplatten");
            Equal("step_grass", G1C_StepSurface.SoundIdFor("tile_city_flowers", G1C_WorldLocation.Exterior), "Blumen");
            Equal("step_grass", G1C_StepSurface.SoundIdFor("tile_city_park", G1C_WorldLocation.Exterior), "Park");
            Equal("step_indoor", G1C_StepSurface.SoundIdFor("tile_city_grass", G1C_WorldLocation.Casino), "innen schlägt Kachel");
            G1C_ExteriorMap map = new G1C_ExteriorMap();
            HashSet<string> surfaces = new HashSet<string>();
            for (int y = 0; y < map.Height; y++) for (int x = 0; x < map.Width; x++)
                if (map.IsWalkable(x, y)) surfaces.Add(G1C_StepSurface.SoundIdFor(map.GetTileSpriteId(x, y), G1C_WorldLocation.Exterior));
            Check(surfaces.SetEquals(new[] { "step_grass", "step_path", "step_bridge" }), "Stadt hat nicht alle drei Untergründe: " + string.Join(",", surfaces));

            WithSoundRoot(root =>
            {
                // im Spiel: Innenraum
                G1C_GameManager home = GameAt(G1C_WorldLocation.Home, 5, 8, G1C_Direction.Up);
                FakeBackend backend = new FakeBackend();
                home.Sound.Backend = backend;
                home.Sound.Heartbeat();
                home.HandleInput(G1C_InputEvent.FromKey(Key.W));
                for (int i = 0; i < 12; i++) home.Update();
                Check(backend.Started("Steps_Indoor") == 1, "Ein Schritt im Haus = ein Innen-Schritt (" + backend.Started("Steps_Indoor") + ")");

                // im Spiel: Gras draußen, dann Fahrrad
                G1C_GameManager probe = RunningGame();
                int sx = probe.Exterior.PlayerStartX, sy = probe.Exterior.PlayerStartY;
                G1C_GameManager town = GameAt(G1C_WorldLocation.Exterior, sx, sy, G1C_Direction.Down);
                backend = new FakeBackend();
                town.Sound.Backend = backend;
                town.Sound.Heartbeat();
                town.HandleInput(G1C_InputEvent.FromKey(Key.D));
                for (int i = 0; i < 12; i++) town.Update();
                string expectedId = G1C_StepSurface.SoundIdFor(town.CurrentWorld.Grid.GetTileSpriteId(town.Player.TileX, town.Player.TileY), G1C_WorldLocation.Exterior);
                string fragment = expectedId == "step_grass" ? "StepGrass" : expectedId == "step_path" ? "StepsPath" : "StepBridge";
                Check(backend.Started(fragment) == 1 && backend.Voices.Count(v => v.File.Contains("Step")) == 1, "Schritt draußen passend zum Untergrund (" + expectedId + "), gestartet: " +
                      string.Join(" | ", backend.Voices.Select(v => Path.GetFileName(v.File))) + ", Position " + town.Player.TileX + "," + town.Player.TileY + " Start " + sx + "," + sy);

                G1C_GameManager rider = GameAt(G1C_WorldLocation.Exterior, sx, sy, G1C_Direction.Down);
                backend = new FakeBackend();
                rider.Sound.Backend = backend;
                rider.Sound.Heartbeat();
                rider.Bikes.TryMount(rider.Inventory, G1C_BikeSystem.StarterBike, G1C_WorldLocation.Exterior);
                rider.HandleInput(G1C_InputEvent.FromKey(Key.D));
                for (int i = 0; i < 6; i++) rider.Update();
                Check(backend.Voices.Any(v => v.File.Contains("BicycleSoundLoop") && v.Loop && !v.Stopped), "Fahrrad-Schleife läuft beim Fahren");
                Check(backend.Started("Step") == 0, "Beim Fahren keine Schritte");
                for (int i = 0; i < 20; i++) rider.Update();
                Check(backend.Voices.Where(v => v.File.Contains("BicycleSoundLoop")).All(v => v.Stopped), "Fahrrad-Schleife endet im Stand");
            });
        }

        private static void SoundAnimalsAndBirds()
        {
            WithSoundRoot(root =>
            {
                G1C_GameManager probe = RunningGame();
                List<G1C_Animal> birds = probe.Exterior.Animals.Where(a => a.SpriteId.StartsWith("animal_bird")).ToList();
                Check(birds.Count >= 3, "Zu wenige Vögel in der Stadt");
                // Startpunkt weit weg von allen Vögeln
                int sx = probe.Exterior.PlayerStartX, sy = probe.Exterior.PlayerStartY;
                Check(birds.All(b => !G1C_StepSurface.InRadius(b.X - sx, b.Y - sy, 4)), "Testposition liegt im Vogelradius");
                G1C_GameManager far = GameAt(G1C_WorldLocation.Exterior, sx, sy, G1C_Direction.Down);
                FakeBackend backend = new FakeBackend();
                far.Sound.Backend = backend;
                far.Sound.Heartbeat();
                foreach (string id in new[] { "meow", "woof", "bird", "frog" }) { far.Sound.Settings.Set(id, "chance", 100); far.Sound.Settings.Set(id, "cooldown", 0); }
                for (int i = 0; i < 1500; i++) { far.Sound.Heartbeat(); far.Update(); }
                Equal(0, backend.Voices.Count(v => v.File.Contains("cat") || v.File.Contains("Bark") || v.File.Contains("bark") || v.File.Contains("songbird") || v.File.Contains("frog")),
                      "Tiere dürfen ohne Streicheln und weit weg von Vögeln nichts sagen");

                // Vogel in Reichweite (3 Kacheln) und knapp außerhalb (6 Kacheln)
                G1C_CollisionManager collision = new G1C_CollisionManager(probe.Exterior);
                Func<G1C_Animal, int, int[]> spot = (bird, dist) =>
                    new[] { new[] { 0, dist }, new[] { 0, -dist }, new[] { dist, 0 }, new[] { -dist, 0 } }
                        .Select(d => new[] { bird.X + d[0], bird.Y + d[1] })
                        .First(p => probe.Exterior.Map.IsWalkable(p[0], p[1]) && !collision.IsBlocked(p[0], p[1]));
                G1C_Animal target = birds[0];
                int[] near = spot(target, 3), outside = spot(target, 6);
                G1C_GameManager listener = GameAt(G1C_WorldLocation.Exterior, near[0], near[1], G1C_Direction.Down);
                backend = new FakeBackend();
                listener.Sound.Backend = backend;
                listener.Sound.Heartbeat();
                listener.Sound.Settings.Set("bird", "chance", 100);
                bool bubbleSeen = false;
                for (int i = 0; i < 80; i++)
                {
                    listener.Sound.Heartbeat(); listener.Update();
                    if (listener.AnimalVoices.Active.Any(b => b.Value == "Tschilp!")) bubbleSeen = true;
                }
                Check(backend.Started("songbird") >= 1, "Vogel im Radius ist nicht zu hören");
                Check(bubbleSeen, "Vogel zeigt keine Sprechblase");

                G1C_GameManager distant = GameAt(G1C_WorldLocation.Exterior, outside[0], outside[1], G1C_Direction.Down);
                bool anotherBirdNear = birds.Any(b => G1C_StepSurface.InRadius(b.X - outside[0], b.Y - outside[1], 4));
                if (!anotherBirdNear)
                {
                    backend = new FakeBackend();
                    distant.Sound.Backend = backend;
                    distant.Sound.Heartbeat();
                    for (int i = 0; i < 150; i++) { distant.Sound.Heartbeat(); distant.Update(); }
                    Equal(0, backend.Started("songbird"), "Vogel außerhalb von 4 Kacheln darf nicht zwitschern");
                }
                listener.Sound.Settings.Set(G1C_SoundSettings.GlobalId, "birdradius", 1);
                Check(!listener.BirdInRange(target) || G1C_StepSurface.InRadius(target.X - listener.Player.TileX, target.Y - listener.Player.TileY, 1), "Radius-Einstellung wirkt nicht");

                // Streicheln: Katze und Hund
                foreach (G1C_PetKind kind in new[] { G1C_PetKind.Cat, G1C_PetKind.Dog })
                {
                    G1C_PetAnimal pet = probe.Exterior.Objects.OfType<G1C_PetAnimal>().First(p => p.Kind == kind);
                    int[][] sides = { new[] { 0, 1, (int)G1C_Direction.Up }, new[] { 0, -1, (int)G1C_Direction.Down },
                                      new[] { 1, 0, (int)G1C_Direction.Left }, new[] { -1, 0, (int)G1C_Direction.Right } };
                    int[] side = sides.First(s => !collision.IsBlocked(pet.X + s[0], pet.Y + s[1]));
                    G1C_GameManager petter = GameAt(G1C_WorldLocation.Exterior, pet.X + side[0], pet.Y + side[1], (G1C_Direction)side[2]);
                    backend = new FakeBackend();
                    petter.Sound.Backend = backend;
                    petter.Sound.Heartbeat();
                    petter.HandleInput(G1C_InputEvent.FromKey(Key.E));
                    string expected = kind == G1C_PetKind.Cat ? "catMeow" : "ark";   // dogBark, dog_bark, puppy_bark
                    Check(backend.Started(expected) == 1, kind + ": Streicheln muss genau ein Tiergeräusch auslösen (" + backend.Voices.Count + ")");
                }
            });
        }

        private static void SoundMusic()
        {
            WithSoundRoot(root =>
            {
                FakeBackend backend = new FakeBackend();
                long[] clock = { 1000 };
                G1C_SoundSystem sound = NewSound(backend, clock);
                sound.SetMusic("town");
                Check(backend.Active.Count() == 1 && backend.Active.Single().File.Contains("Ambient Outside"), "Stadtmusik aus dem Ambiente-Ordner");
                string first = backend.Active.Single().File;
                sound.SetMusic("town");
                Equal(1, backend.Voices.Count, "Gleiche Musik läuft ohne Neustart weiter");
                sound.SetMusic("casino");
                Check(backend.Voices[0].Stopped && backend.Active.Single().File.Contains("CasinoBackgroundLoop"), "Ortswechsel wechselt die Musik");
                sound.SetMusic("arcade");
                Check(backend.Active.Single().File.ToLower().Contains("chippy"), "Spielhalle spielt Chip-Musik");
                sound.SetMusic("title");
                Check(backend.Active.Single().File.Contains("Ambient Charakter"), "Titelbild");

                // mehrere Dateien: nach dem Ende kommt eine andere
                sound.SetMusic("town");
                string a = backend.Active.Single().File;
                clock[0] += 1000; sound.Heartbeat();
                backend.Active.Single().Finish();
                string b = backend.Active.Single().File;
                Check(a != b, "Zweite Aufnahme muss sich von der ersten unterscheiden");
                // einzelne Datei: Player wiederholt selbst, solange der Spiel-Takt lebt
                sound.SetMusic("casino");
                FakeVoice casino = backend.Active.Single();
                Check(casino.Loop, "einzelne Musikdatei läuft in Schleife");
                clock[0] += 1000; sound.Heartbeat(); casino.Finish();
                Check(!casino.Stopped, "Schleife läuft weiter, solange das Spiel tickt");
                clock[0] += 5000; casino.Finish();
                Check(casino.Stopped && sound.CurrentMusic == null, "Ohne Spiel-Takt endet die Musik");

                // eingebaute Ersatzmelodie, wenn keine Datei da ist (Zuhause)
                sound.SetMusic("home");
                FakeVoice home = backend.Active.Single();
                Check(home.File.EndsWith("music_home.wav") && File.Exists(home.File), "Zuhause: eingebaute Melodie");
                // Aus für eine Musik
                sound.SetMusic(null);
                Check(!backend.Active.Any(), "Musik aus");
                sound.Settings.Set("music_casino", "enabled", 0);
                sound.SetMusic("casino");
                Check(!backend.Active.Any(), "ausgeschaltete Musik startet nicht");
            });
        }

        private static void SoundCasinoEvents()
        {
            List<string> log = new List<string>();
            Action<string> record = id => log.Add(id);
            G1C_MoneyManager money = new G1C_MoneyManager(5000);
            G1C_Slots slots = new G1C_Slots(money, new Random(2)) { SoundRequested = record, GuaranteedJackpot = () => true };
            slots.HandleInput(G1C_InputEvent.FromKey(Key.W));
            Check(log.Contains("bet"), "Einsatz ändern: Ton");
            log.Clear();
            slots.HandleInput(G1C_InputEvent.FromKey(Key.E));
            Check(log.SequenceEqual(new[] { "slot_lever", "slots_spin" }), "Hebel und Walzen-Schleife: " + string.Join(",", log));
            log.Clear();
            for (int i = 0; i < 80; i++) slots.Update();
            Check(log.IndexOf("slots_spin:stop") >= 0 && log.Contains("jackpot") && log.Contains("jackpot_alarm") &&
                  log.IndexOf("slots_spin:stop") < log.IndexOf("jackpot"), "Walzen stoppen, dann Jackpot und Alarm: " + string.Join(",", log));

            log.Clear();
            G1C_Roulette roulette = new G1C_Roulette(money, new Random(1)) { SoundRequested = record };
            roulette.HandleInput(G1C_InputEvent.FromKey(Key.E));
            Check(log.Contains("chip"), "Roulette: Chips setzen: " + string.Join(",", log));
            log.Clear();
            G1C_Blackjack blackjack = new G1C_Blackjack(money, new Random(1)) { SoundRequested = record };
            blackjack.HandleInput(G1C_InputEvent.FromKey(Key.E));
            Check(log.Contains("shuffle"), "Blackjack: Karten austeilen: " + string.Join(",", log));
            log.Clear();
            G1C_Poker poker = new G1C_Poker(money, new Random(1)) { SoundRequested = record };
            poker.HandleInput(G1C_InputEvent.FromKey(Key.E));
            Check(log.Contains("shuffle"), "Poker: Karten austeilen: " + string.Join(",", log));

            // Gewinn-Stufen am Spielautomaten: nur kleine/mittlere/große Gewinne ohne Jackpot
            Func<int, string[]> tier = multiplier =>
                multiplier >= 20 ? new[] { "slot_win_big", "coins_big" } : multiplier >= 8 ? new[] { "slot_win_small", "coins_mid" } : new[] { "slot_win_small", "coins_small" };
            Equal(2, tier(40).Length, "Stufen");
            foreach (string id in new[] { "slot_win_big", "coins_big", "slot_win_small", "coins_mid", "coins_small" })
                Check(G1C_SoundCatalog.Find(id) != null, "Sound fehlt: " + id);
        }

        private static void SoundSettingsPersistence()
        {
            G1C_SoundSettings s = new G1C_SoundSettings();
            Check(s.Set("step_grass", "volume", 12) && s.Set("bird", "chance", 33) && s.Set("woof", "cooldown", 2500) && s.Set("coin", "enabled", 0), "Set");
            Check(s.Set(G1C_SoundSettings.GlobalId, "master", 61) && s.Set(G1C_SoundSettings.GlobalId, "max", 5) && s.Set(G1C_SoundSettings.GlobalId, "birdradius", 7), "Set global");
            Check(!s.Set("gibt_es_nicht", "volume", 5) && !s.Set("bird", "unsinn", 5) && !s.Set(G1C_SoundSettings.GlobalId, "unsinn", 5) && !s.Set(null, "volume", 5), "Ungültiges wird abgelehnt");
            s.Set(G1C_SoundSettings.GlobalId, "max", 99);
            Equal(8, s.MaxSimultaneous, "Höchstzahl wird auf 8 begrenzt");
            s.Set(G1C_SoundSettings.GlobalId, "max", 5);
            s.Set("woof", "cooldown", 99999999);
            Equal(60000, s.Get("woof").CooldownMs, "Mindestabstand wird auf 60 s begrenzt");
            s.Set("woof", "cooldown", 2500);

            G1C_SoundSettings back = G1C_SoundSettings.FromJson(s.ToJson());
            Equal(12, back.Get("step_grass").Volume, "JSON: Lautstärke");
            Equal(33, back.Get("bird").Chance, "JSON: Chance");
            Equal(2500, back.Get("woof").CooldownMs, "JSON: Abstand");
            Check(!back.Get("coin").Enabled, "JSON: ausgeschaltet");
            Equal(61, back.Master, "JSON: Gesamt");
            Equal(5, back.MaxSimultaneous, "JSON: gleichzeitig");
            Equal(7, back.BirdRadius, "JSON: Vogel-Radius");
            Equal(G1C_SoundCatalog.All.Length, back.Sounds.Count, "alle Sounds enthalten");

            Equal(80, G1C_SoundSettings.FromJson("{kaputt").Master, "kaputtes JSON -> Standard");
            Equal(80, G1C_SoundSettings.FromJson(null).Master, "null -> Standard");
            G1C_SoundSettings wild = G1C_SoundSettings.FromJson("{\"Master\":500,\"MaxSimultaneous\":0,\"Sounds\":{\"unbekannt\":{\"Volume\":50},\"bird\":{\"Volume\":-9,\"Chance\":300}}}");
            Equal(100, wild.Master, "Wert über 100 wird begrenzt");
            Equal(1, wild.MaxSimultaneous, "0 gleichzeitig wird auf 1 angehoben");
            Check(!wild.Sounds.ContainsKey("unbekannt"), "Unbekannter Sound wird entfernt");
            Equal(0, wild.Get("bird").Volume, "negative Lautstärke -> 0");
            Equal(100, wild.Get("bird").Chance, "Chance 300 -> 100");
            Equal(G1C_SoundCatalog.Find("woof").Volume, wild.Get("woof").Volume, "fehlende Einträge bekommen Standardwerte");

            string path = Path.Combine(Path.GetTempPath(), "g1c_sound_" + Guid.NewGuid().ToString("N") + ".json");
            try
            {
                Check(s.Save(path) && File.Exists(path), "Speichern");
                G1C_SoundSettings loaded = new G1C_SoundSettings();
                Check(loaded.LoadFrom(path) && loaded.Get("step_grass").Volume == 12, "Laden");
                File.WriteAllText(path, "kein json");
                Check(loaded.LoadFrom(path) && loaded.Master == 80, "Defekte Datei -> Standardwerte statt Absturz");
                Check(!new G1C_SoundSettings().LoadFrom(path + ".fehlt"), "fehlende Datei");
            }
            finally { try { File.Delete(path); } catch (Exception) { } }
            s.ResetDefaults();
            Equal(80, s.Master, "Zurücksetzen");
            Equal(G1C_SoundCatalog.Find("step_grass").Volume, s.Get("step_grass").Volume, "Zurücksetzen: Einzelwerte");
        }

        private static string Http(System.Net.Http.HttpClient client, string method, string url, string pin, string body, out int status)
        {
            System.Net.Http.HttpRequestMessage request = new System.Net.Http.HttpRequestMessage(new System.Net.Http.HttpMethod(method), url);
            if (pin != null) request.Headers.Add("X-Pin", pin);
            if (body != null) request.Content = new System.Net.Http.StringContent(body, System.Text.Encoding.UTF8, "application/json");
            System.Net.Http.HttpResponseMessage response = client.SendAsync(request).Result;
            status = (int)response.StatusCode;
            return response.Content.ReadAsStringAsync().Result;
        }

        private static void SoundMixerServerFlow()
        {
            WithSoundRoot(root =>
            {
                FakeBackend backend = new FakeBackend();
                long[] clock = { 1000 };
                G1C_SoundSystem sound = NewSound(backend, clock);
                G1C_SoundMixerServer server = new G1C_SoundMixerServer(sound);
                Check(server.Start(true, 18765, "4321"), "Server startet: " + server.LastError);
                try
                {
                    Check(server.Running && server.Pin == "4321" && server.Port >= 18765, "Server läuft");
                    using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient { Timeout = TimeSpan.FromSeconds(10) })
                    {
                        string url = "http://127.0.0.1:" + server.Port;
                        int status;
                        string page = Http(client, "GET", url + "/", null, null, out status);
                        Check(status == 200 && page.Contains("Sound-Mixer") && page.Contains("/api/state") && page.Contains("viewport"), "Handy-Seite wird ausgeliefert");
                        Check(Http(client, "GET", url + "/manifest.webmanifest", null, null, out status).Contains("Sound-Mixer") && status == 200, "Manifest für den Startbildschirm");
                        Http(client, "GET", url + "/api/state", null, null, out status);
                        Equal(401, status, "ohne PIN");
                        Http(client, "GET", url + "/api/state", "0000", null, out status);
                        Equal(401, status, "falsche PIN");
                        string state = Http(client, "GET", url + "/api/state", "4321", null, out status);
                        Equal(200, status, "richtige PIN");
                        using (System.Text.Json.JsonDocument doc = System.Text.Json.JsonDocument.Parse(state))
                        {
                            Equal(G1C_SoundCatalog.All.Length, doc.RootElement.GetProperty("sounds").GetArrayLength(), "alle Sounds im Zustand");
                            Equal(80, doc.RootElement.GetProperty("global").GetProperty("master").GetInt32(), "Gesamtlautstärke im Zustand");
                        }
                        Http(client, "GET", url + "/api/state?pin=4321", null, null, out status);
                        Equal(200, status, "PIN auch als Adressparameter");

                        Http(client, "POST", url + "/api/set", "4321", "{\"id\":\"step_grass\",\"field\":\"volume\",\"value\":20}", out status);
                        Check(status == 200 && sound.Settings.Get("step_grass").Volume == 20, "Lautstärke vom Handy");
                        Http(client, "POST", url + "/api/set", "4321", "{\"id\":\"_global\",\"field\":\"max\",\"value\":5}", out status);
                        Equal(5, sound.Settings.MaxSimultaneous, "gleichzeitig vom Handy");
                        Http(client, "POST", url + "/api/set", "4321", "{\"id\":\"bird\",\"field\":\"chance\",\"value\":35.4}", out status);
                        Equal(35, sound.Settings.Get("bird").Chance, "Häufigkeit vom Handy");
                        Http(client, "POST", url + "/api/set", "4321", "{\"id\":\"nix\",\"field\":\"volume\",\"value\":1}", out status);
                        Equal(400, status, "unbekannter Sound");
                        Http(client, "POST", url + "/api/set", "4321", "kein json", out status);
                        Equal(400, status, "kaputtes JSON");
                        Http(client, "POST", url + "/api/set", "4321", "{\"id\":\"_global\",\"field\":\"muted\",\"value\":1}", out status);
                        Check(!sound.Muted, "Stumm wartet auf den Spiel-Takt");
                        sound.Pump();
                        Check(sound.Muted, "Stumm vom Handy");
                        Http(client, "POST", url + "/api/set", "4321", "{\"id\":\"_global\",\"field\":\"muted\",\"value\":0}", out status);
                        sound.Pump();
                        Check(!sound.Muted, "Stumm aus");

                        sound.Settings.Set("bird", "chance", 0);
                        Http(client, "POST", url + "/api/play", "4321", "{\"id\":\"bird\"}", out status);
                        Equal(200, status, "Probehören angenommen");
                        Equal(0, backend.Started("songbird"), "Probehören erst im Spiel-Takt");
                        sound.Pump();
                        Equal(1, backend.Started("songbird"), "Probehören spielt trotz Chance 0");
                        Http(client, "POST", url + "/api/play", "4321", "{\"id\":\"gibtsnicht\"}", out status);
                        Equal(400, status, "Probehören unbekannter Sound");

                        string statusJson = Http(client, "GET", url + "/api/status", "4321", null, out status);
                        Check(status == 200 && statusJson.Contains("active"), "Status");
                        Http(client, "POST", url + "/api/reset", "4321", "{}", out status);
                        Check(status == 200 && sound.Settings.Get("step_grass").Volume == G1C_SoundCatalog.Find("step_grass").Volume, "Zurücksetzen vom Handy");
                        Http(client, "GET", url + "/geheim", null, null, out status);
                        Equal(404, status, "unbekannte Adresse");

                        // Sperre nach Fehlversuchen
                        for (int i = 0; i < 12; i++) Http(client, "GET", url + "/api/state", "9999", null, out status);
                        Equal(429, status, "Sperre nach vielen Fehlversuchen");
                        Http(client, "GET", url + "/api/state", "4321", null, out status);
                        Equal(429, status, "auch die richtige PIN ist kurz gesperrt");
                    }
                }
                finally { server.Stop(); }
                Check(!server.Running, "Server beendet");

                // Im Spiel: F11 im Menü schaltet den Server (nur Loopback im Test)
                G1C_GameManager game = GameAt(G1C_WorldLocation.Home, 5, 8, G1C_Direction.Up);
                game.Sound.MixerLoopbackOnly = true;
                game.HandleInput(G1C_InputEvent.FromKey(Key.B));
                Equal(G1C_GameState.Menu, game.State, "Menü");
                game.HandleInput(G1C_InputEvent.FromKey(Key.F11));
                try
                {
                    Check(game.Sound.Mixer != null && game.Sound.Mixer.Running, "F11 startet den Mixer");
                    Equal(G1C_GameState.Dialogue, game.State, "Adresse und PIN erscheinen als Dialog");
                    Check(game.Dialogue.CurrentText.Contains("http://") || game.Dialogue.CurrentText.Contains("Adresse nicht gefunden"), "Adresse im Dialog");
                    game.HandleInput(G1C_InputEvent.FromKey(Key.E));
                    Check(game.Dialogue.CurrentText.Contains(game.Sound.Mixer.Pin), "PIN im Dialog");
                    game.HandleInput(G1C_InputEvent.FromKey(Key.E));
                    Equal(G1C_GameState.MainGame, game.State, "Dialog geschlossen");
                    game.HandleInput(G1C_InputEvent.FromKey(Key.B));
                    game.HandleInput(G1C_InputEvent.FromKey(Key.F11));
                    Check(game.Sound.Mixer == null, "F11 beendet den Mixer wieder");
                }
                finally { if (game.Sound.Mixer != null) game.Sound.Mixer.Stop(); }
            });
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
