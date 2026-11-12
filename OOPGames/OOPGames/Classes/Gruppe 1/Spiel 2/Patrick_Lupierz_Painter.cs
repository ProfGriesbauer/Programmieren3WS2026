using System.Collections.Generic;
using System.Linq;
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

        private BitmapSource _mapImage;
        private IG1C_WorldMap _mapImageSource;

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

            if (game.IsWorldVisible || game.ActiveGame == null)
            {
                DrawWorld(r, game);
                _hud.Draw(r, game);

                if (game.State == G1C_GameState.Dialogue) game.Dialogue.Draw(r, game.TickCount);
                else if (game.State == G1C_GameState.Menu) game.Menu.Draw(r, game);
                else if (game.State == G1C_GameState.Result) game.Result.Draw(r);
                else if (game.State == G1C_GameState.Conversation) _conversationView.Draw(r, game.Npcs); // Änderung durch: Eduard Schoenfeld
            }
            else
            {
                game.ActiveGame.Draw(r);
            }
        }

        // ------------------------------------------------------------------ Aktive Welt-Szene

        // Änderung durch: Denis Mock – zeichnet die aktive Szene und zentriert auch quadratische Karten.
        private void DrawWorld(G1C_Renderer r, G1C_GameManager game)
        {
            IG1C_WorldScene world = game.CurrentWorld;
            IG1C_WorldMap map = world.Grid;
            EnsureMapImage(map);
            double tileSize = world.TileSize;
            double worldWidth = map.Width * tileSize;
            double worldHeight = map.Height * tileSize;
            double mapX = (G1C_Renderer.ScreenWidth - worldWidth) / 2;
            double mapY = G1C_GameHUD.Height +
                          (G1C_Renderer.ScreenHeight - G1C_GameHUD.Height - worldHeight) / 2;
            r.DrawBitmap(_mapImage, mapX, mapY, worldWidth, worldHeight);

            // Änderung durch: Eduard Schoenfeld – Datum: 12.11.2026 – Aufgaben-Gegenstände liegen unter den Figuren
            bool inCasino = game.CurrentLocation == G1C_WorldLocation.Casino;
            if (inCasino) _npcOverlay.DrawItems(r, game.Npcs, mapX, mapY, tileSize);

            // Objekte und Figuren nach ihrer Unterkante sortieren, damit Vordergrund richtig überdeckt
            List<KeyValuePair<double, System.Action>> drawables = new List<KeyValuePair<double, System.Action>>();
            foreach (G1C_WorldObject obj in world.Objects)
            {
                G1C_WorldObject o = obj;
                drawables.Add(new KeyValuePair<double, System.Action>(o.Y + o.Height,
                    () => DrawEntity(r, o.SpriteId, o.X, o.Y, o.Width, o.Height, tileSize, mapX, mapY)));
            }

            G1C_Player player = game.Player;
            drawables.Add(new KeyValuePair<double, System.Action>(player.VisualY + 1.01,
                () => DrawEntity(r, _animation.GetSpriteId(player), player.VisualX, player.VisualY, 1, 1,
                                 tileSize, mapX, mapY)));

            foreach (KeyValuePair<double, System.Action> d in drawables.OrderBy(d => d.Key))
            {
                d.Value();
            }

            // Änderung durch: Eduard Schoenfeld – Datum: 12.11.2026 – Sprechblasen und Aufgabenanzeige
            if (inCasino)
            {
                _npcOverlay.DrawBubbles(r, game.Npcs, mapX, mapY, tileSize);
                _npcOverlay.DrawTaskStatus(r, game.Npcs);
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
        private void EnsureMapImage(IG1C_WorldMap map)
        {
            if (_mapImage != null && ReferenceEquals(_mapImageSource, map)) return;

            G1C_PixelCanvas canvas = new G1C_PixelCanvas(map.Width * MapPixelsPerTile, map.Height * MapPixelsPerTile);
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                {
                    canvas.DrawBitmap(_sprites.Get(map.GetTileSpriteId(x, y)),
                                      x * MapPixelsPerTile, y * MapPixelsPerTile, MapPixelsPerTile, MapPixelsPerTile);
                }
            }
            _mapImage = canvas.ToBitmap();
            _mapImageSource = map;
        }
    }
}
