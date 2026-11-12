using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace OOPGames
{
    /// <summary>
    /// Zentraler Painter (IPaintGame2) des Casino-Spiels.
    /// Zeichnet je nach Spielphase die Casino-Welt mit Figuren, HUD, Dialog, Menü und
    /// Ergebnis – oder übergibt das Zeichnen an das aktive Minispiel.
    /// Gezeichnet wird ausschließlich auf das vom Framework übergebene Canvas.
    /// </summary>
    public class G1C_CasinoPainter : IPaintGame2
    {
        private const double TileSize = 24;        // virtuelle Einheiten pro Kachel (20 x 12 Kacheln = 480 x 288)
        private const int MapPixelsPerTile = 48;   // Auflösung des zusammengesetzten Kartenbildes

        private readonly G1C_SpriteLibrary _sprites = new G1C_SpriteLibrary();
        private readonly G1C_Renderer _renderer;
        private readonly G1C_GameHUD _hud = new G1C_GameHUD();
        private readonly G1C_PlayerAnimation _animation = new G1C_PlayerAnimation();

        private BitmapSource _mapImage;
        private G1C_CasinoMap _mapImageSource;

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
            }
            else
            {
                game.ActiveGame.Draw(r);
            }
        }

        // ------------------------------------------------------------------ Casino-Welt

        private void DrawWorld(G1C_Renderer r, G1C_GameManager game)
        {
            G1C_CasinoMap map = game.World.Map;
            EnsureMapImage(map);
            r.DrawBitmap(_mapImage, 0, G1C_GameHUD.Height, map.Width * TileSize, map.Height * TileSize);

            // Objekte und Figuren nach ihrer Unterkante sortieren, damit Vordergrund richtig überdeckt
            List<KeyValuePair<double, System.Action>> drawables = new List<KeyValuePair<double, System.Action>>();
            foreach (G1C_WorldObject obj in game.World.Objects)
            {
                G1C_WorldObject o = obj;
                drawables.Add(new KeyValuePair<double, System.Action>(o.Y + o.Height,
                    () => DrawEntity(r, o.SpriteId, o.X, o.Y, o.Width, o.Height)));
            }

            G1C_Player player = game.Player;
            drawables.Add(new KeyValuePair<double, System.Action>(player.VisualY + 1.01,
                () => DrawEntity(r, _animation.GetSpriteId(player), player.VisualX, player.VisualY, 1, 1)));

            foreach (KeyValuePair<double, System.Action> d in drawables.OrderBy(d => d.Key))
            {
                d.Value();
            }
        }

        /// <summary>Zeichnet eine Grafik unten bündig auf ihre Kacheln (hohe Grafiken ragen nach oben).</summary>
        private void DrawEntity(G1C_Renderer r, string spriteId, double tileX, double tileY, double tilesWide, double tilesHigh)
        {
            double width = tilesWide * TileSize;
            double height = width * _sprites.GetAspect(spriteId);
            double bottom = G1C_GameHUD.Height + (tileY + tilesHigh) * TileSize;
            r.Sprite(spriteId, tileX * TileSize, bottom - height, width, height);
        }

        /// <summary>Setzt alle Bodenkacheln einmalig zu einem Bild zusammen (schneller als 240 Einzelbilder pro Tick).</summary>
        private void EnsureMapImage(G1C_CasinoMap map)
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
