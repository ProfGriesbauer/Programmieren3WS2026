using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_WorldSprites
    // Entwickler: Denis Mock
    // Beschreibung: Austauschbare, eingebaute Pixelgrafiken für Stadt und Zuhause.
    // =====================================================
    public static class G1C_WorldSprites
    {
        // Entwickler: Denis Mock
        public static BitmapSource Create(string id)
        {
            switch (id)
            {
                case "tile_city_grass": return Grass(false);
                case "tile_city_grass_alt": return Grass(true);
                case "tile_city_road": return Road();
                case "tile_city_sidewalk": return Sidewalk();
                case "tile_city_path": return Path();
                case "tile_city_border": return Border();
                case "tile_city_home_entry": return Entry(false);
                case "tile_city_casino_entry": return Entry(true);
                case "tile_home_floor": return HomeFloor(false);
                case "tile_home_floor_alt": return HomeFloor(true);
                case "tile_home_wall": return HomeWall();
                case "tile_home_exit": return HomeExit();
                case "building_home": return HomeBuilding();
                case "building_hospital": return Hospital();
                case "building_market": return Market();
                case "building_casino": return Casino();
                case "obj_city_tree": return Tree();
                case "obj_home_bed": return Bed();
                case "obj_home_table": return Table();
                default: return null;
            }
        }

        // Entwickler: Denis Mock
        private static BitmapSource Grass(bool alternate)
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(16, 16);
            canvas.FillRect(0, 0, 16, 16, Rgb(alternate ? 0x78 : 0x70, 0xB8, 0x60));
            canvas.Set(alternate ? 4 : 11, 5, Rgb(0x58, 0x98, 0x50));
            canvas.Set(alternate ? 12 : 3, 12, Rgb(0x90, 0xC8, 0x70));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        private static BitmapSource Road()
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(16, 16);
            canvas.FillRect(0, 0, 16, 16, Rgb(0x70, 0x70, 0x78));
            canvas.FillRect(1, 1, 14, 14, Rgb(0x80, 0x80, 0x88));
            canvas.Set(4, 5, Rgb(0x90, 0x90, 0x98));
            canvas.Set(12, 11, Rgb(0x68, 0x68, 0x70));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        private static BitmapSource Sidewalk()
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(16, 16);
            canvas.FillRect(0, 0, 16, 16, Rgb(0xC0, 0xB8, 0xA0));
            canvas.DrawRectBorder(0, 0, 16, 16, Rgb(0xA0, 0x98, 0x88));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        private static BitmapSource Path()
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(16, 16);
            canvas.FillRect(0, 0, 16, 16, Rgb(0xB8, 0xA0, 0x78));
            canvas.Set(4, 4, Rgb(0xA8, 0x90, 0x68));
            canvas.Set(11, 12, Rgb(0xC8, 0xB0, 0x88));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        private static BitmapSource Border()
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(16, 16);
            canvas.FillRect(0, 0, 16, 16, Rgb(0x28, 0x38, 0x58));
            canvas.DrawRectBorder(1, 1, 14, 14, Rgb(0x60, 0x88, 0xB0));
            canvas.DrawRectBorder(3, 3, 10, 10, Rgb(0x18, 0x28, 0x40));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        private static BitmapSource Entry(bool casino)
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(16, 16);
            canvas.FillRect(0, 0, 16, 16, Rgb(0xB8, 0xA0, 0x78));
            canvas.FillRect(3, 2, 10, 12, casino ? Rgb(0xA8, 0x38, 0x48) : Rgb(0x70, 0x50, 0x38));
            canvas.DrawRectBorder(3, 2, 10, 12, Rgb(0x38, 0x30, 0x38));
            canvas.FillRect(6, 7, 4, 7, Rgb(0x58, 0x38, 0x30));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        private static BitmapSource HomeFloor(bool alternate)
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(16, 16);
            canvas.FillRect(0, 0, 16, 16, Rgb(alternate ? 0xD8 : 0xD0, 0xB8, 0x88));
            canvas.DrawRectBorder(0, 0, 16, 16, Rgb(0xC0, 0xA0, 0x70));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        private static BitmapSource HomeWall()
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(16, 16);
            canvas.FillRect(0, 0, 16, 16, Rgb(0x70, 0x58, 0x50));
            canvas.FillRect(0, 0, 16, 5, Rgb(0xA0, 0x68, 0x58));
            canvas.FillRect(0, 5, 16, 1, Rgb(0x48, 0x38, 0x40));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        private static BitmapSource HomeExit()
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(16, 16);
            canvas.FillRect(0, 0, 16, 16, Rgb(0xD0, 0xB8, 0x88));
            canvas.DrawRectBorder(0, 0, 16, 16, Rgb(0xC0, 0xA0, 0x70));
            canvas.FillRect(4, 2, 8, 12, Rgb(0x70, 0x48, 0x30));
            canvas.DrawRectBorder(4, 2, 8, 12, Rgb(0x38, 0x28, 0x28));
            canvas.Set(10, 8, Rgb(0xF0, 0xD0, 0x58));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        private static BitmapSource HomeBuilding()
        {
            G1C_PixelCanvas canvas = BuildingBase(Rgb(0x48, 0x68, 0xA0), Rgb(0xE0, 0xC8, 0x98));
            canvas.FillRect(24, 8, 16, 10, Rgb(0xB8, 0xD8, 0xE8));
            canvas.DrawRectBorder(24, 8, 16, 10, Rgb(0x38, 0x48, 0x68));
            canvas.FillRect(29, 8, 2, 10, Rgb(0xF0, 0xE0, 0xB8));
            canvas.FillRect(26, 34, 12, 14, Rgb(0x70, 0x48, 0x30));
            canvas.DrawRectBorder(26, 34, 12, 14, Rgb(0x38, 0x28, 0x28));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        private static BitmapSource Hospital()
        {
            G1C_PixelCanvas canvas = BuildingBase(Rgb(0x78, 0xB8, 0xC8), Rgb(0xE8, 0xE8, 0xD8));
            DrawWindow(canvas, 8, 23);
            DrawWindow(canvas, 42, 23);
            canvas.FillRect(25, 4, 14, 14, Rgb(0xF8, 0xF8, 0xF0));
            canvas.FillRect(30, 6, 4, 10, Rgb(0xD8, 0x40, 0x48));
            canvas.FillRect(27, 9, 10, 4, Rgb(0xD8, 0x40, 0x48));
            canvas.FillRect(26, 34, 12, 14, Rgb(0x78, 0x58, 0x48));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        private static BitmapSource Market()
        {
            G1C_PixelCanvas canvas = BuildingBase(Rgb(0x60, 0x90, 0x58), Rgb(0xE8, 0xD8, 0xA8));
            canvas.FillRect(6, 20, 52, 8, Rgb(0xD8, 0x48, 0x48));
            canvas.FillRect(6, 20, 8, 8, Rgb(0xF0, 0xF0, 0xD8));
            canvas.FillRect(22, 20, 8, 8, Rgb(0xF0, 0xF0, 0xD8));
            canvas.FillRect(38, 20, 8, 8, Rgb(0xF0, 0xF0, 0xD8));
            canvas.FillRect(54, 20, 4, 8, Rgb(0xF0, 0xF0, 0xD8));
            DrawWindow(canvas, 8, 30);
            DrawWindow(canvas, 42, 30);
            canvas.FillRect(26, 34, 12, 14, Rgb(0x78, 0x58, 0x48));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        private static BitmapSource Casino()
        {
            G1C_PixelCanvas canvas = BuildingBase(Rgb(0x70, 0x38, 0x70), Rgb(0xD8, 0xB0, 0x80));
            canvas.FillRect(8, 5, 48, 12, Rgb(0xE8, 0xC8, 0x58));
            canvas.DrawRectBorder(8, 5, 48, 12, Rgb(0x48, 0x28, 0x48));
            canvas.FillRect(13, 8, 38, 2, Rgb(0xD0, 0x48, 0x58));
            canvas.FillRect(13, 12, 38, 2, Rgb(0x50, 0x68, 0xB0));
            DrawWindow(canvas, 7, 22);
            DrawWindow(canvas, 43, 22);
            canvas.FillRect(26, 34, 12, 14, Rgb(0x58, 0x38, 0x38));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        private static BitmapSource Tree()
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(16, 24);
            canvas.FillRect(6, 13, 4, 11, Rgb(0x70, 0x48, 0x30));
            canvas.FillEllipse(8, 9, 7, 8, Rgb(0x28, 0x78, 0x40));
            canvas.FillEllipse(6, 7, 4, 5, Rgb(0x48, 0x98, 0x48));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        private static BitmapSource Bed()
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(32, 24);
            canvas.FillRect(1, 3, 30, 20, Rgb(0x58, 0x38, 0x30));
            canvas.DrawRectBorder(1, 3, 30, 20, Rgb(0x38, 0x28, 0x28));
            canvas.FillRect(4, 6, 24, 14, Rgb(0x68, 0x88, 0xB8));
            canvas.FillRect(5, 6, 9, 6, Rgb(0xE8, 0xE0, 0xC8));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        private static BitmapSource Table()
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(32, 16);
            canvas.FillRect(2, 2, 28, 8, Rgb(0xA0, 0x68, 0x38));
            canvas.DrawRectBorder(2, 2, 28, 8, Rgb(0x48, 0x30, 0x28));
            canvas.FillRect(5, 10, 3, 6, Rgb(0x58, 0x38, 0x28));
            canvas.FillRect(24, 10, 3, 6, Rgb(0x58, 0x38, 0x28));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        private static G1C_PixelCanvas BuildingBase(Color roof, Color wall)
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(64, 48);
            canvas.FillRect(0, 10, 64, 38, wall);
            canvas.FillRect(2, 2, 60, 12, roof);
            canvas.DrawRectBorder(2, 2, 60, 46, Rgb(0x38, 0x30, 0x38));
            canvas.FillRect(0, 14, 64, 3, Rgb(0x48, 0x38, 0x38));
            return canvas;
        }

        // Entwickler: Denis Mock
        private static void DrawWindow(G1C_PixelCanvas canvas, int x, int y)
        {
            canvas.FillRect(x, y, 14, 12, Rgb(0x78, 0xB8, 0xD0));
            canvas.DrawRectBorder(x, y, 14, 12, Rgb(0x38, 0x48, 0x58));
            canvas.FillRect(x + 6, y, 2, 12, Rgb(0xE8, 0xE0, 0xC8));
        }

        // Entwickler: Denis Mock
        private static Color Rgb(int r, int g, int b)
        {
            return Color.FromRgb((byte)r, (byte)g, (byte)b);
        }
    }
}
