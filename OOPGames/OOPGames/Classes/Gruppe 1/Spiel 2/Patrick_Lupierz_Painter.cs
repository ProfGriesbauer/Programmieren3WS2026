using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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

            G1C_Renderer r = _renderer;
            r.BeginFrame(canvas);
            field.Viewport = r.Viewport;
            G1C_GameManager game = field.Manager;

            if (game.State == G1C_GameState.Title)
            {
                _startView.Draw(r, game.Start);
                return;
            }

            if (game.IsWorldVisible || game.ActiveGame == null)
            {
                DrawWorld(r, game);
                _hud.Draw(r, game);

                if (game.State == G1C_GameState.Dialogue) game.Dialogue.Draw(r, game.TickCount);
                else if (game.State == G1C_GameState.Menu) game.Menu.Draw(r, game);
                else if (game.State == G1C_GameState.Result) game.Result.Draw(r);
                else if (game.State == G1C_GameState.Conversation) _conversationView.Draw(r, game.Npcs); // Änderung durch: Eduard Schoenfeld
                else if (game.State == G1C_GameState.ListMenu) _listMenuView.Draw(r, game);
                else if (game.State == G1C_GameState.Phone) _phoneView.Draw(r, game);
            }
            else
            {
                game.ActiveGame.Draw(r);
            }
        }

        // ------------------------------------------------------------------ Aktive Welt-Szene

        // Änderung durch Denis Mock: Außenwelt mit begrenztem Kameraausschnitt, andere Szenen weiterhin zentriert.
        private void DrawWorld(G1C_Renderer r, G1C_GameManager game)
        {
            IG1C_WorldScene world = game.CurrentWorld;
            IG1C_WorldMap map = world.Grid;
            bool scrolling = game.CurrentLocation == G1C_WorldLocation.Exterior;
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
                double x = npc != null ? npc.VisualX : o.X;
                double y = npc != null ? npc.VisualY : o.Y;
                if (!IsEntityVisible(o, x, y, tileSize, mapX, mapY)) continue;
                drawables.Add(new KeyValuePair<double, System.Action>(y + o.Height,
                    () => DrawEntity(r, o.SpriteId, x, y, o.Width, o.Height, tileSize, mapX, mapY)));
            }

            G1C_Player player = game.Player;
            drawables.Add(new KeyValuePair<double, System.Action>(player.VisualY + 1.01,
                () => DrawEntity(r, _animation.GetSpriteId(player), player.VisualX, player.VisualY, 1, 1,
                                 tileSize, mapX, mapY)));

            foreach (KeyValuePair<double, System.Action> d in drawables.OrderBy(d => d.Key))
            {
                d.Value();
            }

            DrawDaylight(r, game, world, scrolling, tileSize, mapX, mapY);

            // Änderung durch: Eduard Schoenfeld – Datum: 12.11.2026 – Sprechblasen und Aufgabenanzeige
            // Änderung durch: Patrick Lupierz – Datum: 12.11.2026 – Reaktions-Sprechblasen gibt es jetzt auf jeder Karte
            _npcOverlay.DrawBubbles(r, game.Npcs, mapX, mapY, tileSize);
            if (inCasino) _npcOverlay.DrawTaskStatus(r, game.Npcs);
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
