using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OOPGames
{
    public class G1C_MapView
    {
        private BitmapSource _mapBitmap;
        private G1C_ExteriorMap _cachedMap;
        private int _selectedX = -1;
        private int _selectedY = -1;
        private string _selection = "Klicke auf ein Gebäude, um es zu markieren.";

        public void ClearSelection()
        {
            _selectedX = -1;
            _selectedY = -1;
            _selection = "Klicke auf ein Gebäude, um es zu markieren.";
        }

        public void SelectAt(double x, double y, G1C_ExteriorWorld world)
        {
            Rect bounds = MapBounds(world);
            if (!bounds.Contains(x, y)) return;

            double scale = bounds.Width / world.Map.Width;
            _selectedX = Math.Min(world.Map.Width - 1, (int)((x - bounds.X) / scale));
            _selectedY = Math.Min(world.Map.Height - 1, (int)((y - bounds.Y) / scale));
            G1C_WorldObject building = world.Objects.FirstOrDefault(o =>
                o.SpriteId != null && o.SpriteId.StartsWith("building_", StringComparison.Ordinal) &&
                o.Occupies(_selectedX, _selectedY));
            _selection = building != null
                ? building.Name + "  (" + (_selectedX + 1) + ", " + (_selectedY + 1) + ")"
                : TileName(world.Map.GetTileSpriteId(_selectedX, _selectedY)) +
                  "  (" + (_selectedX + 1) + ", " + (_selectedY + 1) + ")";
        }

        public void DrawMinimap(G1C_Renderer r, G1C_ExteriorWorld world, G1C_Player player)
        {
            BitmapSource map = GetMap(world);
            int cropWidth = 9;
            int cropHeight = 9;
            int left = Clamp(player.TileX - cropWidth / 2, 0, world.Map.Width - cropWidth);
            int top = Clamp(player.TileY - cropHeight / 2, 0, world.Map.Height - cropHeight);
            CroppedBitmap crop = new CroppedBitmap(map, new Int32Rect(left, top, cropWidth, cropHeight));
            crop.Freeze();

            double size = Math.Min(72, Math.Min(G1C_Renderer.ScreenWidth * 0.25, G1C_Renderer.ScreenHeight * 0.25));
            double x = G1C_Renderer.ScreenWidth - size - 10;
            double y = G1C_Renderer.ScreenHeight - size - 10;
            r.DrawRect(x - 3, y - 3, size + 6, size + 6, Color.FromRgb(0x20, 0x20, 0x28), Colors.White, 2);
            r.DrawBitmap(crop, x, y, size, size);
            r.FillEllipse(x + (player.TileX - left + 0.5) * size / cropWidth,
                          y + (player.TileY - top + 0.5) * size / cropHeight,
                          3, 3, Color.FromRgb(0xF8, 0xD0, 0x48));
            r.Text("N", x + size - 12, y + 2, 8, Colors.White);
        }

        public void Draw(G1C_Renderer r, G1C_ExteriorWorld world, G1C_Player player)
        {
            Rect bounds = MapBounds(world);
            double scale = bounds.Width / world.Map.Width;
            r.FillRect(0, 0, G1C_Renderer.ScreenWidth, G1C_Renderer.ScreenHeight,
                       Color.FromRgb(0x18, 0x20, 0x30));
            r.DrawRect(bounds.X - 2, bounds.Y - 2, bounds.Width + 4,
                       bounds.Height + 4, Color.FromRgb(0x10, 0x10, 0x18), Colors.White, 2);
            r.DrawBitmap(GetMap(world), bounds.X, bounds.Y, bounds.Width, bounds.Height);

            double playerX = bounds.X + (player.VisualX + 0.5) * scale;
            double playerY = bounds.Y + (player.VisualY + 0.5) * scale;
            r.FillEllipse(playerX, playerY, 4, 4, Color.FromRgb(0xF8, 0xD0, 0x48));
            if (_selectedX >= 0)
            {
                r.DrawRect(bounds.X + _selectedX * scale, bounds.Y + _selectedY * scale,
                           scale, scale, Colors.Transparent, Colors.White, 1);
            }

            r.Text("STADTKARTE", 0, 12, 14, G1C_UiColors.Gold, G1C_Renderer.ScreenWidth);
            r.Text(_selection, 0, G1C_Renderer.ScreenHeight - 18, 9, G1C_UiColors.TextLight, G1C_Renderer.ScreenWidth);
            r.Text("[M] / [Q] Zurück   [B] Menü", 0, G1C_Renderer.ScreenHeight - 34, 9,
                   G1C_UiColors.TextLight, G1C_Renderer.ScreenWidth);
        }

        private static Rect MapBounds(G1C_ExteriorWorld world)
        {
            double width = G1C_Renderer.ScreenWidth - 32;
            double height = G1C_Renderer.ScreenHeight - 102;
            double scale = Math.Max(0.1, Math.Min(width / world.Map.Width, height / world.Map.Height));
            double mapWidth = world.Map.Width * scale;
            double mapHeight = world.Map.Height * scale;
            double top = 40 + Math.Max(0, (G1C_Renderer.ScreenHeight - 88 - mapHeight) / 2);
            return new Rect((G1C_Renderer.ScreenWidth - mapWidth) / 2, top, mapWidth, mapHeight);
        }

        private BitmapSource GetMap(G1C_ExteriorWorld world)
        {
            G1C_ExteriorMap map = world.Map;
            if (_mapBitmap != null && ReferenceEquals(map, _cachedMap)) return _mapBitmap;

            G1C_PixelCanvas canvas = new G1C_PixelCanvas(map.Width, map.Height);
            for (int y = 0; y < map.Height; y++)
            {
                for (int x = 0; x < map.Width; x++)
                    canvas.Set(x, y, TileColor(map.GetTileSpriteId(x, y)));
            }

            foreach (G1C_WorldObject obj in world.Objects)
            {
                if (obj.SpriteId == null) continue;
                if (obj.SpriteId.StartsWith("building_", StringComparison.Ordinal))
                {
                    Color roof = obj.SpriteId == "building_home" ? Color.FromRgb(0xD8, 0x58, 0x48)
                        : obj.SpriteId == "building_casino" ? Color.FromRgb(0x88, 0x48, 0xA8)
                        : obj.SpriteId == "building_bank" ? Color.FromRgb(0x50, 0x70, 0x80)
                        : obj.SpriteId == "building_furniture" ? Color.FromRgb(0xA0, 0x78, 0x48)
                        : Color.FromRgb(0xB0, 0x98, 0x78);
                    canvas.FillRect(obj.X, obj.Y, obj.Width, obj.Height, roof);
                }
                else if (obj.SpriteId == "obj_city_tree")
                {
                    canvas.Set(obj.X, obj.Y, Color.FromRgb(0x28, 0x68, 0x38));
                }
            }
            _mapBitmap = canvas.ToBitmap();
            _cachedMap = map;
            return _mapBitmap;
        }

        private static Color TileColor(string id)
        {
            if (id.StartsWith("tile_city_water_", StringComparison.Ordinal)) return Color.FromRgb(0x38, 0x88, 0xC0);
            if (id == "tile_city_bridge") return Color.FromRgb(0xB8, 0x78, 0x38);
            if (id.StartsWith("tile_city_road_", StringComparison.Ordinal)) return Color.FromRgb(0xC8, 0xC0, 0xB0);
            if (id.StartsWith("tile_city_path_", StringComparison.Ordinal)) return Color.FromRgb(0xD8, 0xC0, 0x88);
            if (id == "tile_city_forest") return Color.FromRgb(0x28, 0x68, 0x38);
            if (id == "tile_city_park" || id == "tile_city_flowers") return Color.FromRgb(0x80, 0xC8, 0x58);
            return Color.FromRgb(0x70, 0xB8, 0x50);
        }

        private static string TileName(string id)
        {
            if (id.StartsWith("tile_city_water_", StringComparison.Ordinal)) return "Fluss";
            if (id.StartsWith("tile_city_road_", StringComparison.Ordinal)) return "Straße";
            if (id.StartsWith("tile_city_path_", StringComparison.Ordinal)) return "Weg";
            if (id == "tile_city_bridge") return "Brücke";
            if (id == "tile_city_park" || id == "tile_city_flowers") return "Park";
            return "Grünfläche";
        }

        private static int Clamp(int value, int minimum, int maximum)
        {
            return Math.Max(minimum, Math.Min(maximum, value));
        }
    }
}
