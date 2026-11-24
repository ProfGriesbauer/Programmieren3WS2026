using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OOPGames
{
    /// <summary>
    /// Zentraler Painter (IPaintGame2) des Casino-Spiels.
    /// Zeichnet je nach Spielphase die aktive Welt-Szene mit Figuren, HUD, Dialog, Menü und
    /// Ergebnis – oder übergibt das Zeichnen an das aktive Minispiel.
    /// Gezeichnet wird ausschließlich auf das vom Framework übergebene Canvas.
    /// </summary>
    // Erweiterung durch Denis Mock: Der Painter rendert auch Zuhause und Außenwelt.
    public class G1C_CasinoPainter : IG1C_CasinoPainter
    {
        private const int MapPixelsPerTile = 48;   // Auflösung des zusammengesetzten Kartenbildes

        private readonly G1C_SpriteLibrary _sprites = new G1C_SpriteLibrary();
        private readonly G1C_Renderer _renderer;
        private readonly G1C_GameHUD _hud = new G1C_GameHUD();
        private readonly G1C_PlayerAnimation _animation = new G1C_PlayerAnimation();
        // Änderung durch: Eduard Schoenfeld – Datum: 12.11.2026 – Darstellung des NPC-Systems
        private readonly G1C_ConversationView _conversationView = new G1C_ConversationView();
        private readonly G1C_NpcOverlayView _npcOverlay = new G1C_NpcOverlayView();
        private readonly G1C_WorldCamera _camera = new G1C_WorldCamera();
        // Änderung durch: Patrick Lupierz – Datum: 12.11.2026 – Hauptmenü, Listenmenü und Handy
        private readonly G1C_StartView _startView = new G1C_StartView();
        private readonly G1C_ListMenuView _listMenuView = new G1C_ListMenuView();
        private readonly G1C_PhoneView _phoneView = new G1C_PhoneView();

        private readonly Dictionary<IG1C_WorldMap, BitmapSource> _mapImages =
            new Dictionary<IG1C_WorldMap, BitmapSource>();

        public G1C_CasinoPainter()
        {
            _renderer = new G1C_Renderer(_sprites);
        }

        public string Name { get { return "Gruppe 1 - Casino Painter (Spiel 2)"; } }

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            Paint(canvas, currentField as G1C_CasinoField);
        }

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            Paint(canvas, currentField as G1C_CasinoField);
        }

        private void Paint(Canvas canvas, G1C_CasinoField field)
        {
            if (canvas == null || field == null || field.Manager == null) return;
            if (field.KeyboardFocusRequested) FocusCanvas(canvas, field);

            G1C_Renderer r = _renderer;
            r.BeginFrame(canvas);
            G1C_GameManager game = field.Manager;
            // Änderung durch: Patrick Lupierz – Datum: 22.11.2026 – Minispiele im festen, zentrierten Rahmen;
            // der Viewport für die Mausumrechnung wird erst danach übernommen.
            bool minigame = game.State != G1C_GameState.Title && !game.IsWorldVisible && game.ActiveGame != null;
            if (minigame) r.UseFixedFrame();
            field.Viewport = r.Viewport;

            if (game.State == G1C_GameState.Title)
            {
                _startView.Draw(r, game.Start);
                return;
            }

            if (game.IsWorldVisible || game.ActiveGame == null)
            {
                DrawWorld(r, game);
                if (game.IsSleeping) DrawSleep(r, game); // Patrick Lupierz – 15.11.2026
                _hud.Draw(r, game);

                if (game.State == G1C_GameState.Dialogue) game.Dialogue.Draw(r, game.TickCount);
                else if (game.State == G1C_GameState.Menu) game.Menu.Draw(r, game);
                else if (game.State == G1C_GameState.Result) game.Result.Draw(r);
                else if (game.State == G1C_GameState.Conversation) _conversationView.Draw(r, game.Npcs); // Änderung durch: Eduard Schoenfeld
                else if (game.State == G1C_GameState.ListMenu) _listMenuView.Draw(r, game);
                else if (game.State == G1C_GameState.Phone) _phoneView.Draw(r, game);
                else if (game.State == G1C_GameState.Map) game.MapView.Draw(r, game.Exterior, game.Player);
            }
            else
            {
                game.ActiveGame.Draw(r);
            }
        }

        // Entwickler: Patrick Lupierz – Datum: 15.11.2026
        // Aufgabe: WASD-/Pfeiltasten-Reparatur. Nach "Start New Game" hat der Button den Fokus (Leertaste/Enter
        //          starten neu) und die Auswahllisten verbrauchen Pfeiltasten. Das übergebene Canvas wird deshalb
        //          fokussierbar gemacht und erhält den Tastaturfokus; KeyDown erreicht weiter die MainWindow.
        //          Es wird keine Framework-Datei geändert und kein eigenes Eingabe-Event abonniert.
        private static void FocusCanvas(Canvas canvas, G1C_CasinoField field)
        {
            canvas.Focusable = true;
            canvas.FocusVisualStyle = null;
            if (canvas.IsVisible && canvas.Focus()) field.KeyboardFocusRequested = false;
        }

        // ------------------------------------------------------------------ Aktive Welt-Szene

        // Änderung durch Denis Mock: Außenwelt mit begrenztem Kameraausschnitt, andere Szenen weiterhin zentriert.
        private void DrawWorld(G1C_Renderer r, G1C_GameManager game)
        {
            IG1C_WorldScene world = game.CurrentWorld;
            IG1C_WorldMap map = world.Grid;
            bool scrolling = game.CurrentLocation == G1C_WorldLocation.Exterior;
            r.ClipToVirtualRect(0, G1C_GameHUD.Height, G1C_Renderer.ScreenWidth,
                                G1C_Renderer.ScreenHeight - G1C_GameHUD.Height);
            Color backdrop = game.CurrentLocation == G1C_WorldLocation.Casino
                ? Color.FromRgb(0x3A, 0x2A, 0x4A)
                : game.CurrentLocation == G1C_WorldLocation.Home
                    ? Color.FromRgb(0x78, 0x58, 0x38)
                    : Color.FromRgb(0x48, 0x50, 0x58);
            r.FillRect(0, G1C_GameHUD.Height, G1C_Renderer.ScreenWidth,
                       G1C_Renderer.ScreenHeight - G1C_GameHUD.Height, backdrop);
            // Änderung durch: Patrick Lupierz – Datum: 12.11.2026 – Reparatur "Figur unsichtbar":
            // mapX/mapY bezeichneten bisher die Ecke des Kartenausschnitts, Figuren rechnen aber ab der
            // Kartenecke. Dadurch lagen sie außerhalb des Bildes, sobald die Kamera scrollte. Jetzt ist
            // mapX/mapY immer die Bildschirmposition der Kartenecke (bei scrollender Kamera negativ).
            // Das Kartenbild der Stadt wird in 16 Pixeln pro Kachel gebaut und beim Zeichnen vergrößert.
            const int sourceTile = 16;
            BitmapSource mapImage = EnsureMapImage(map, scrolling ? sourceTile : MapPixelsPerTile);
            double tileSize = world.TileSize;
            double mapX;
            double mapY;

            if (scrolling)
            {
                double scale = tileSize / sourceTile;
                double viewHeight = G1C_Renderer.ScreenHeight - G1C_GameHUD.Height;
                _camera.Follow(game.Player, world, G1C_Renderer.ScreenWidth, viewHeight);
                mapX = -_camera.X * tileSize;
                mapY = G1C_GameHUD.Height - _camera.Y * tileSize;
                int cropX = (int)System.Math.Floor(_camera.X * sourceTile);
                int cropY = (int)System.Math.Floor(_camera.Y * sourceTile);
                int cropWidth = System.Math.Min((int)(G1C_Renderer.ScreenWidth / scale) + 2, mapImage.PixelWidth - cropX);
                int cropHeight = System.Math.Min((int)(viewHeight / scale) + 2, mapImage.PixelHeight - cropY);
                CroppedBitmap viewport = new CroppedBitmap(mapImage, new Int32Rect(cropX, cropY, cropWidth, cropHeight));
                viewport.Freeze();
                r.DrawBitmap(viewport, mapX + cropX * scale, mapY + cropY * scale, cropWidth * scale, cropHeight * scale);
            }
            else
            {
                double worldWidth = map.Width * tileSize;
                double worldHeight = map.Height * tileSize;
                mapX = (G1C_Renderer.ScreenWidth - worldWidth) / 2;
                mapY = G1C_GameHUD.Height +
                       (G1C_Renderer.ScreenHeight - G1C_GameHUD.Height - worldHeight) / 2;
                r.DrawBitmap(mapImage, mapX, mapY, worldWidth, worldHeight);
            }

            // Änderung durch: Eduard Schoenfeld – Datum: 12.11.2026 – Aufgaben-Gegenstände liegen unter den Figuren
            bool inCasino = game.CurrentLocation == G1C_WorldLocation.Casino;
            if (inCasino) _npcOverlay.DrawItems(r, game.Npcs, mapX, mapY, tileSize);

            // Objekte und Figuren nach ihrer Unterkante sortieren, damit Vordergrund richtig überdeckt
            List<KeyValuePair<double, System.Action>> drawables = new List<KeyValuePair<double, System.Action>>();
            foreach (G1C_WorldObject obj in world.Objects)
            {
                G1C_WorldObject o = obj;
                G1C_Npc npc = o as G1C_Npc;
                G1C_PetAnimal pet = o as G1C_PetAnimal; // Patrick Lupierz – 16.11.2026: Haustiere laufen flüssig
                double x = npc != null ? npc.VisualX : pet != null ? pet.VisualX : o.X;
                double y = npc != null ? npc.VisualY : pet != null ? pet.VisualY : o.Y;
                if (!IsEntityVisible(o, x, y, tileSize, mapX, mapY)) continue;
                double sortY = y + o.Height;
                // Änderung durch Denis Mock – 19.11.2026 – Dach/Front vor, hintere Stützen hinter Spieler und NPC
                if (o.SpriteId == "building_pavilion_back") sortY = y + 1.0;
                else if (o.SpriteId == "building_pavilion_roof") sortY = y + 4.25;
                else if (o.SpriteId == "building_pavilion_front") sortY = y + 4.75;
                drawables.Add(new KeyValuePair<double, System.Action>(sortY,
                    () => DrawEntity(r, o.SpriteId, x, y, o.Width, o.Height, tileSize, mapX, mapY)));
            }

            G1C_Player player = game.Player;
            // Änderung durch: Patrick Lupierz – Datum: 15.11.2026 – beim Fahrradfahren sitzt die Figur auf dem Rad
            bool riding = scrolling && game.Bikes.Riding;
            // Änderung durch: Patrick Lupierz – Datum: 15.11.2026 – beim Schlafen liegt die Figur im Bett
            G1C_WorldObject bed = game.IsSleeping && game.CurrentLocation == G1C_WorldLocation.Home
                ? world.Objects.FirstOrDefault(o => o.Interaction == G1C_InteractionType.Bed) : null;
            if (bed != null)
                drawables.Add(new KeyValuePair<double, System.Action>(bed.Y + bed.Height + 0.02,
                    () => DrawSleeper(r, player.Skin, bed, tileSize, mapX, mapY)));
            else drawables.Add(new KeyValuePair<double, System.Action>(player.VisualY + 1.01, () =>
            {
                // Änderung durch: Patrick Lupierz – 16.11.2026 – Fahrrad in Blickrichtung; beim Fahren keine
                // Laufbeine (Standbild, leichtes Wippen beim Treten); von vorn liegt der Lenker vor der Figur.
                if (!riding)
                {
                    string playerSprite = _animation.GetSpriteId(player);
                    double limpOffset = player.Humping && player.IsMoving ? (game.TickCount % 2 == 0 ? 0.12 : -0.05) : 0;
                    DrawEntity(r, playerSprite, player.VisualX, player.VisualY + limpOffset, 1, 1, tileSize, mapX, mapY);
                    return;
                }
                string bike = game.Bikes.RideSpriteId(player.Facing);
                double pedal = player.IsMoving ? System.Math.Abs(System.Math.Sin(player.StepProgress * 1.2)) * 0.05 : 0;
                bool bikeInFront = player.Facing == G1C_Direction.Down;
                if (!bikeInFront) DrawBike(r, bike, player.VisualX, player.VisualY, tileSize, mapX, mapY);
                DrawEntity(r, G1C_PlayerAnimation.GetStandingSpriteId(player.Skin, player.Facing), player.VisualX,
                           player.VisualY - 0.2 - pedal, 1, 1, tileSize, mapX, mapY);
                if (bikeInFront) DrawBike(r, bike, player.VisualX, player.VisualY + 0.08, tileSize, mapX, mapY);
            }));

            foreach (KeyValuePair<double, System.Action> d in drawables.OrderBy(d => d.Key))
            {
                d.Value();
            }

            DrawDaylight(r, game, world, scrolling, tileSize, mapX, mapY);
            if (scrolling) DrawClosedSigns(r, game, tileSize, mapX, mapY); // Patrick Lupierz – 15.11.2026

            // Änderung durch: Eduard Schoenfeld – Datum: 12.11.2026 – Sprechblasen und Aufgabenanzeige
            // Änderung durch: Patrick Lupierz – Datum: 12.11.2026 – Reaktions-Sprechblasen gibt es jetzt auf jeder Karte
            _npcOverlay.DrawBubbles(r, game.Npcs, mapX, mapY, tileSize);
            DrawAnimalBubbles(r, game, tileSize, mapX, mapY);                         // Patrick Lupierz – 16.11.2026
            if (inCasino) _npcOverlay.DrawTaskStatus(r, game.Npcs);
            if (game.State == G1C_GameState.FurnitureEditor) DrawFurnitureEditor(r, game, tileSize, mapX, mapY);
            r.RestoreClip();
            if (game.State == G1C_GameState.FurnitureEditor) DrawFurnitureHelp(r, game);
            if (scrolling) game.MapView.DrawMinimap(r, game.Exterior, game.Player);
        }

        // Entwickler: Patrick Lupierz
        // Aufgabe: Tag-Nacht-Wechsel als halbtransparente Farbfläche über der Welt (die Grafiken selbst
        //          bleiben unverändert). Abends und nachts leuchten die Straßenlaternen.
        private void DrawDaylight(G1C_Renderer r, G1C_GameManager game, IG1C_WorldScene world, bool outdoors,
                                  double tileSize, double mapX, double mapY)
        {
            System.Windows.Media.Color tint = game.Clock.Tint(outdoors);
            // Auf wenige Stufen runden, damit der Renderer nicht für jeden Tick einen neuen Pinsel anlegt
            tint = System.Windows.Media.Color.FromArgb((byte)(tint.A / 6 * 6), (byte)(tint.R / 16 * 16),
                                                       (byte)(tint.G / 16 * 16), (byte)(tint.B / 16 * 16));
            if (tint.A > 0)
                r.FillRect(0, G1C_GameHUD.Height, G1C_Renderer.ScreenWidth, G1C_Renderer.ScreenHeight - G1C_GameHUD.Height, tint);

            if (!outdoors || (game.Clock.Hour >= 7 && game.Clock.Hour < 19)) return;
            foreach (G1C_WorldObject obj in world.Objects)
            {
                if (obj.SpriteId != "obj_street_lamp" || !IsEntityVisible(obj, obj.X, obj.Y, tileSize, mapX, mapY)) continue;
                double x = mapX + (obj.X + 0.5) * tileSize;
                double y = mapY + (obj.Y - 0.5) * tileSize;
                r.FillEllipse(x, y + tileSize, tileSize * 1.4, tileSize * 1.0, System.Windows.Media.Color.FromArgb(50, 255, 230, 140));
                r.FillEllipse(x, y, tileSize * 0.5, tileSize * 0.5, System.Windows.Media.Color.FromArgb(120, 255, 240, 170));
            }
        }

        // Entwickler: Patrick Lupierz – Datum: 15.11.2026
        // Aufgabe: Schlafende Figur: Front-Grafik um 90° gedreht, Kopf auf dem linken Kissen, Decke über dem Körper.
        private void DrawSleeper(G1C_Renderer r, string skin, G1C_WorldObject bed, double tileSize, double mapX, double mapY)
        {
            double bedTop = mapY + (bed.Y + bed.Height) * tileSize - bed.Width * tileSize * _sprites.GetAspect(bed.SpriteId);
            double size = tileSize * 0.95;
            double cx = mapX + bed.X * tileSize + tileSize * 0.75;
            double cy = bedTop + tileSize * 0.6;
            r.Sprite(G1C_PlayerAnimation.GetStandingSpriteId(skin, G1C_Direction.Down), cx - size / 2, cy - size / 2, size, size, -90);
            double blanketX = mapX + bed.X * tileSize + tileSize * 0.95;
            double blanketW = bed.Width * tileSize - tileSize * 1.1;
            r.FillRect(blanketX, cy - size * 0.36, blanketW, size * 0.72, Color.FromRgb(0x48, 0x78, 0xC0));
            r.FillRect(blanketX, cy - size * 0.36, tileSize * 0.12, size * 0.72, Color.FromRgb(0x88, 0xB0, 0xE8));
            r.Text("Z z", cx - tileSize * 0.2, bedTop - tileSize * 0.6, tileSize * 0.4, Color.FromRgb(0xF8, 0xF8, 0xF8));
        }

        // Entwickler: Patrick Lupierz – Datum: 16.11.2026
        // Aufgabe: Kleine Sprechblasen der Tiere (Miau, Wuff, Quak ...); Herz beim Streicheln in Rot.
        private void DrawAnimalBubbles(G1C_Renderer r, G1C_GameManager game, double tileSize, double mapX, double mapY)
        {
            foreach (KeyValuePair<G1C_WorldObject, string> bubble in game.AnimalVoices.Active)
            {
                G1C_PetAnimal pet = bubble.Key as G1C_PetAnimal;
                double ax = pet != null ? pet.VisualX : bubble.Key.X;
                double ay = pet != null ? pet.VisualY : bubble.Key.Y;
                double width = bubble.Value.Length * 5.2 + 8;
                double cx = mapX + (ax + 0.5) * tileSize;
                double top = mapY + ay * tileSize - 16;
                if (bubble.Key.SpriteId != null && bubble.Key.SpriteId.StartsWith("animal_bird")) top -= tileSize * 1.4;
                if (cx < 0 || cx > G1C_Renderer.ScreenWidth || top < G1C_GameHUD.Height - 10 || top > G1C_Renderer.ScreenHeight) continue;
                double x = cx - width / 2;
                bool love = bubble.Value.Contains("♥");
                r.DrawRect(x, top, width, 12, Color.FromRgb(0xFF, 0xFF, 0xF4), love ? Color.FromRgb(0xE0, 0x40, 0x60) : G1C_UiColors.PanelBorder, 1.2);
                r.FillRect(cx - 1.5, top + 12, 3, 3, G1C_UiColors.PanelBorder);
                r.Text(bubble.Value, x, top + 1, 8, love ? Color.FromRgb(0xD0, 0x30, 0x50) : G1C_UiColors.Text, width);
            }
        }

        // Entwickler: Patrick Lupierz – Datum: 16.11.2026
        // Aufgabe: Einrichtungsmodus über dem echten Zuhause: feines Raster, eigene Möbel hellblau umrandet,
        //          gehaltenes Möbel als Vorschau am Cursor (grün = frei, rot = blockiert).
        private void DrawFurnitureEditor(G1C_Renderer r, G1C_GameManager game, double tileSize, double mapX, double mapY)
        {
            G1C_HomeMap map = game.Home.Map;
            Color grid = Color.FromArgb(40, 255, 255, 255);
            for (int x = 1; x < map.Width; x++) r.FillRect(mapX + x * tileSize, mapY, 1, map.Height * tileSize, grid);
            for (int y = 1; y < map.Height; y++) r.FillRect(mapX, mapY + y * tileSize, map.Width * tileSize, 1, grid);
            foreach (G1C_PlacedFurniture placed in game.Home.PlacedFurniture)
            {
                G1C_Item item = G1C_ItemCatalog.Get(placed.ItemId);
                if (item == null) continue;
                r.DrawRect(mapX + placed.X * tileSize, mapY + placed.Y * tileSize, item.FurnitureWidth * tileSize,
                           item.FurnitureHeight * tileSize, Colors.Transparent, Color.FromRgb(0x60, 0xC8, 0xF8), 1.5);
            }
            G1C_FurnitureEditor editor = game.Furnishing;
            double cx = mapX + editor.CursorX * tileSize, cy = mapY + editor.CursorY * tileSize;
            if (editor.Held == null)
            {
                r.DrawRect(cx, cy, tileSize, tileSize, Colors.Transparent, G1C_UiColors.Gold, 2);
                return;
            }
            G1C_Item held = editor.Held;
            DrawEntity(r, held.FurnitureSpriteId, editor.CursorX, editor.CursorY, held.FurnitureWidth, held.FurnitureHeight, tileSize, mapX, mapY);
            bool ok = editor.CursorValid(game.Home, game.Player);
            r.DrawRect(cx, cy, held.FurnitureWidth * tileSize, held.FurnitureHeight * tileSize,
                       ok ? Color.FromArgb(70, 0x40, 0xF0, 0x60) : Color.FromArgb(90, 0xF0, 0x30, 0x30),
                       ok ? Color.FromRgb(0x40, 0xD0, 0x60) : Color.FromRgb(0xE0, 0x30, 0x30), 2);
        }

        private static void DrawFurnitureHelp(G1C_Renderer r, G1C_GameManager game)
        {
            double y = G1C_Renderer.ScreenHeight - 46;
            r.DrawPanel(4, y, G1C_Renderer.ScreenWidth - 8, 42);
            G1C_FurnitureEditor editor = game.Furnishing;
            r.Text("EINRICHTEN" + (editor.Held != null ? ": " + editor.Held.Name : " – Möbel anklicken zum Verschieben"), 12, y + 5, 10, G1C_UiColors.PanelBorder);
            r.Text("Klick/[E] setzen · WASD Cursor · [N] nächstes · [R] einlagern · [Q] fertig", 12, y + 17, 8.5, G1C_UiColors.Text);
            r.Text(editor.Message, 12, y + 28, 8.5, G1C_UiColors.Text);
        }

        // Entwickler: Patrick Lupierz – Datum: 15.11.2026
        // Aufgabe: Fahrrad unter der Figur (Seitenansicht breiter als eine Kachel, Frontansicht schmal)
        private void DrawBike(G1C_Renderer r, string spriteId, double tileX, double tileY, double tileSize, double mapX, double mapY)
        {
            bool side = spriteId.EndsWith("_side") || spriteId.EndsWith("_left") || spriteId.EndsWith("_right"); // 16.11.2026
            double width = tileSize * (side ? 1.3 : 0.6);
            double height = width * _sprites.GetAspect(spriteId);
            double bottom = mapY + (tileY + 1) * tileSize;
            r.Sprite(spriteId, mapX + (tileX + 0.5) * tileSize - width / 2, bottom - height, width, height);
        }

        // Entwickler: Patrick Lupierz – Datum: 15.11.2026
        // Aufgabe: CLOSED-Schild an der Tür jedes gerade geschlossenen Ladens. Welche Läden geschlossen sind,
        //          entscheidet allein die Spiellogik (G1C_GameManager.ClosedShops); hier wird nur gezeichnet.
        private void DrawClosedSigns(G1C_Renderer r, G1C_GameManager game, double tileSize, double mapX, double mapY)
        {
            double width = tileSize * 1.4;
            double height = width * _sprites.GetAspect("sign_closed");
            foreach (G1C_WorldLocation shop in game.ClosedShops())
            {
                int doorX = game.Exterior.Map.DoorX(shop);
                int doorY = game.Exterior.Map.DoorY(shop);
                double x = mapX + (doorX + 0.5) * tileSize - width / 2;
                double y = mapY + doorY * tileSize + tileSize * 0.2;
                if (x + width < 0 || x > G1C_Renderer.ScreenWidth || y + height < G1C_GameHUD.Height || y > G1C_Renderer.ScreenHeight) continue;
                r.Sprite("sign_closed", x, y, width, height);
            }
        }

        // Entwickler: Patrick Lupierz – Datum: 15.11.2026
        // Aufgabe: Schlafanimation – Bild dunkelt ab und wieder auf, "Z z Z" steigt über der Figur auf.
        private void DrawSleep(G1C_Renderer r, G1C_GameManager game)
        {
            int left = game.SleepUntilTick - game.TickCount;
            int elapsed = G1C_GameManager.SleepAnimationTicks - left;
            double fade = System.Math.Min(1.0, System.Math.Min(elapsed, left) / 15.0);
            byte alpha = (byte)(160 * fade); // Patrick Lupierz – schwächer, damit die schlafende Figur sichtbar bleibt
            r.FillRect(0, G1C_GameHUD.Height, G1C_Renderer.ScreenWidth, G1C_Renderer.ScreenHeight - G1C_GameHUD.Height,
                       Color.FromArgb(alpha, 8, 10, 32));
            double baseY = G1C_Renderer.ScreenHeight / 2;
            for (int i = 0; i < 3; i++)
            {
                double rise = (elapsed * 0.8 + i * 18) % 54;
                r.Text(i == 1 ? "z" : "Z", G1C_Renderer.ScreenWidth / 2 + i * 14 - 14, baseY - rise, 14 + i * 4,
                       Color.FromArgb((byte)(255 * fade), 0xF8, 0xF8, 0xF8));
            }
        }

        /// <summary>Zeichnet eine Grafik unten bündig auf ihre Kacheln (hohe Grafiken ragen nach oben).</summary>
        // Entwickler: Denis Mock
        private void DrawEntity(G1C_Renderer r, string spriteId, double tileX, double tileY, double tilesWide,
                                double tilesHigh, double tileSize, double mapX, double mapY)
        {
            double width = tilesWide * tileSize;
            double height = width * _sprites.GetAspect(spriteId);
            double bottom = mapY + (tileY + tilesHigh) * tileSize;
            r.Sprite(spriteId, mapX + tileX * tileSize, bottom - height, width, height);
        }

        // Entwickler: Denis Mock
        /// <summary>Setzt alle Bodenkacheln einmalig zu einem Bild zusammen (schneller als 240 Einzelbilder pro Tick).</summary>
        private bool IsEntityVisible(G1C_WorldObject obj, double tileX, double tileY, double tileSize,
                                     double mapX, double mapY)
        {
            double x = mapX + tileX * tileSize;
            double width = obj.Width * tileSize;
            double bottom = mapY + (tileY + obj.Height) * tileSize;
            double height = width * _sprites.GetAspect(obj.SpriteId);
            double top = bottom - height;
            return x + width > 0 && x < G1C_Renderer.ScreenWidth &&
                   bottom > G1C_GameHUD.Height && top < G1C_Renderer.ScreenHeight;
        }

        // Entwickler: Denis Mock
        /// <summary>Erstellt und speichert eine niedrig aufgel\u00f6ste Bodentextur pro Szene.</summary>
        private BitmapSource EnsureMapImage(IG1C_WorldMap map, int pixelsPerTile)
        {
            BitmapSource image;
            if (_mapImages.TryGetValue(map, out image)) return image;

            G1C_PixelCanvas canvas = new G1C_PixelCanvas(map.Width * pixelsPerTile, map.Height * pixelsPerTile);
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    canvas.DrawBitmap(_sprites.Get(map.GetTileSpriteId(x, y)),
                                      x * pixelsPerTile, y * pixelsPerTile, pixelsPerTile, pixelsPerTile);
                }
            }
            image = canvas.ToBitmap();
            _mapImages.Add(map, image);
            return image;
        }
    }
}
