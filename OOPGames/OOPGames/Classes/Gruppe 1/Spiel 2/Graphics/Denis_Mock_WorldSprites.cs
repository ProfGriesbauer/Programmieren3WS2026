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
                case "tile_city_forest": return Forest();
                case "tile_city_water": return Water(false);
                case "tile_city_water_alt": return Water(true);
                case "tile_city_bank": return RiverBank();
                case "tile_city_bridge": return Bridge();
                case "tile_city_park": return Park();
                case "tile_city_home_entry": return Entry(false);
                case "tile_city_casino_entry": return Entry(true);
                case "tile_city_hospital_entry": return EntryColor(Rgb(0xD8, 0x40, 0x48));
                case "tile_city_market_entry": return EntryColor(Rgb(0xD8, 0xA8, 0x38));
                case "tile_home_floor": return HomeFloor(false);
                case "tile_home_floor_alt": return HomeFloor(true);
                case "tile_home_wall": return HomeWall();
                case "tile_home_exit": return HomeExit();
                case "tile_hospital_floor": return InteriorFloor(Rgb(0xD8, 0xE8, 0xE8));
                case "tile_market_floor": return InteriorFloor(Rgb(0xD8, 0xC8, 0x98));
                case "tile_hospital_wall": return InteriorWall(Rgb(0x78, 0xB8, 0xC8));
                case "tile_market_wall": return InteriorWall(Rgb(0x68, 0x98, 0x58));
                case "tile_interior_rug": return InteriorRug();
                case "tile_interior_exit": return HomeExit();
                case "building_home": return HomeBuilding();
                case "building_hospital": return Hospital();
                case "building_market": return Market();
                case "building_casino": return Casino();
                case "building_library": return TownBuilding(Rgb(0x48, 0x58, 0x88), Rgb(0xD8, 0xC8, 0xA0), Rgb(0x70, 0xA0, 0xD0));
                case "building_townhouse": return TownBuilding(Rgb(0xA0, 0x58, 0x48), Rgb(0xE0, 0xC8, 0x98), Rgb(0x60, 0x88, 0xA0));
                case "building_cafe": return TownBuilding(Rgb(0x70, 0x48, 0x38), Rgb(0xE8, 0xD0, 0xA0), Rgb(0xD0, 0x78, 0x48));
                case "building_flower_shop": return TownBuilding(Rgb(0x70, 0x68, 0xA0), Rgb(0xD8, 0xD0, 0xB0), Rgb(0xD8, 0x80, 0x98));
                case "building_fire_station": return TownBuilding(Rgb(0xB0, 0x38, 0x38), Rgb(0xD8, 0xD0, 0xB0), Rgb(0xD8, 0x40, 0x40));
                case "building_cinema": return TownBuilding(Rgb(0x38, 0x38, 0x58), Rgb(0xC8, 0xB8, 0x98), Rgb(0xD8, 0x40, 0x48));
                case "building_pavilion": return Pavilion();
                case "building_warehouse": return TownBuilding(Rgb(0x58, 0x68, 0x68), Rgb(0xB8, 0xA8, 0x88), Rgb(0xD0, 0xB8, 0x68));
                case "building_post": return TownBuilding(Rgb(0x48, 0x68, 0x88), Rgb(0xE0, 0xD0, 0xA0), Rgb(0xD0, 0xB0, 0x48));
                case "obj_city_tree": return Tree();
                case "obj_home_bed": return Bed();
                case "obj_home_table": return Table();
                case "obj_counter": return Counter();
                case "obj_medical_bed": return MedicalBed();
                case "obj_clinic_cabinet": return Cabinet();
                case "obj_store_shelf": return Shelf();
                case "obj_chair": return Chair();
                case "animal_duck_0": return Animal("duck", 0);
                case "animal_duck_1": return Animal("duck", 1);
                case "animal_fish_0": return Animal("fish", 0);
                case "animal_fish_1": return Animal("fish", 1);
                case "animal_cat_0": return Animal("cat", 0);
                case "animal_cat_1": return Animal("cat", 1);
                case "animal_bird_0": return Animal("bird", 0);
                case "animal_bird_1": return Animal("bird", 1);
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
        private static BitmapSource Forest()
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(16, 16);
            canvas.FillRect(0, 0, 16, 16, Rgb(0x18, 0x48, 0x38));
            canvas.FillEllipse(8, 6, 8, 7, Rgb(0x28, 0x78, 0x40));
            canvas.FillEllipse(5, 5, 5, 5, Rgb(0x40, 0x98, 0x48));
            canvas.FillEllipse(12, 9, 5, 6, Rgb(0x20, 0x68, 0x38));
            canvas.Set(5, 4, Rgb(0x88, 0xB8, 0x50));
            canvas.Set(11, 6, Rgb(0x70, 0xA8, 0x48));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        private static BitmapSource Water(bool alternate)
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(16, 16);
            canvas.FillRect(0, 0, 16, 16, alternate ? Rgb(0x38, 0x88, 0xB0) : Rgb(0x30, 0x78, 0xA8));
            int offset = alternate ? 3 : 0;
            canvas.FillRect((offset + 2) % 12, 3, 5, 1, Rgb(0x70, 0xC0, 0xD0));
            canvas.FillRect((offset + 7) % 12, 10, 6, 1, Rgb(0x50, 0xA8, 0xC8));
            canvas.Set((offset + 12) % 16, 6, Rgb(0xA0, 0xD8, 0xD8));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        private static BitmapSource RiverBank()
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(16, 16);
            canvas.FillRect(0, 0, 16, 16, Rgb(0xB8, 0xA0, 0x68));
            canvas.FillRect(1, 0, 2, 16, Rgb(0xD0, 0xB8, 0x78));
            canvas.Set(7, 4, Rgb(0x90, 0x88, 0x58));
            canvas.Set(11, 12, Rgb(0xE0, 0xC8, 0x88));
            canvas.Set(5, 10, Rgb(0xA0, 0x90, 0x60));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        private static BitmapSource Bridge()
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(16, 16);
            canvas.FillRect(0, 0, 16, 16, Rgb(0x48, 0x88, 0xB0));
            canvas.FillRect(0, 3, 16, 10, Rgb(0xA0, 0x68, 0x38));
            canvas.DrawRectBorder(0, 3, 16, 10, Rgb(0x48, 0x38, 0x30));
            for (int x = 2; x < 16; x += 4) canvas.FillRect(x, 4, 1, 8, Rgb(0xD0, 0x98, 0x58));
            canvas.FillRect(0, 2, 16, 1, Rgb(0xD0, 0xA8, 0x68));
            canvas.FillRect(0, 13, 16, 1, Rgb(0xD0, 0xA8, 0x68));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        private static BitmapSource Park()
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(16, 16);
            canvas.FillRect(0, 0, 16, 16, Rgb(0x58, 0xA0, 0x50));
            canvas.Set(4, 5, Rgb(0x88, 0xC0, 0x58));
            canvas.Set(11, 11, Rgb(0x38, 0x88, 0x48));
            canvas.FillRect(2, 13, 3, 1, Rgb(0xD8, 0xC0, 0x88));
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
        private static BitmapSource EntryColor(Color color)
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(16, 16);
            canvas.FillRect(0, 0, 16, 16, Rgb(0xB8, 0xA0, 0x78));
            canvas.FillRect(3, 2, 10, 12, color);
            canvas.DrawRectBorder(3, 2, 10, 12, Rgb(0x38, 0x30, 0x38));
            canvas.FillRect(6, 7, 4, 7, Rgb(0x58, 0x38, 0x30));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        private static BitmapSource InteriorFloor(Color color)
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(16, 16);
            canvas.FillRect(0, 0, 16, 16, color);
            canvas.DrawRectBorder(0, 0, 16, 16, Rgb(0xB0, 0xA8, 0x98));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        private static BitmapSource InteriorWall(Color color)
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(16, 16);
            canvas.FillRect(0, 0, 16, 16, color);
            canvas.FillRect(0, 0, 16, 5, Rgb(0xE0, 0xD8, 0xC0));
            canvas.FillRect(0, 5, 16, 1, Rgb(0x78, 0x68, 0x58));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        private static BitmapSource InteriorRug()
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(16, 16);
            canvas.FillRect(0, 0, 16, 16, Rgb(0x90, 0xA8, 0xA0));
            canvas.DrawRectBorder(1, 1, 14, 14, Rgb(0xD0, 0xC8, 0xA0));
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
        private static BitmapSource TownBuilding(Color roof, Color wall, Color sign)
        {
            G1C_PixelCanvas canvas = BuildingBase(roof, wall);
            DrawWindow(canvas, 7, 23);
            DrawWindow(canvas, 43, 23);
            canvas.FillRect(22, 18, 20, 7, sign);
            canvas.DrawRectBorder(22, 18, 20, 7, Rgb(0x38, 0x30, 0x38));
            canvas.FillRect(26, 34, 12, 14, Rgb(0x68, 0x48, 0x38));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        private static BitmapSource Pavilion()
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(64, 48);
            canvas.FillRect(4, 8, 56, 6, Rgb(0xB8, 0x70, 0x48));
            canvas.FillRect(8, 14, 4, 31, Rgb(0x78, 0x50, 0x38));
            canvas.FillRect(52, 14, 4, 31, Rgb(0x78, 0x50, 0x38));
            canvas.FillRect(14, 16, 36, 24, Rgb(0xD8, 0xC8, 0x98));
            canvas.DrawRectBorder(14, 16, 36, 24, Rgb(0x58, 0x48, 0x38));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        private static BitmapSource Counter()
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(48, 24);
            canvas.FillRect(1, 5, 46, 17, Rgb(0x90, 0x58, 0x38));
            canvas.FillRect(1, 5, 46, 5, Rgb(0xC0, 0x88, 0x50));
            canvas.DrawRectBorder(1, 5, 46, 17, Rgb(0x48, 0x30, 0x28));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        private static BitmapSource MedicalBed()
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(48, 32);
            canvas.FillRect(3, 5, 42, 23, Rgb(0xE8, 0xE8, 0xD8));
            canvas.DrawRectBorder(3, 5, 42, 23, Rgb(0x50, 0x68, 0x70));
            canvas.FillRect(6, 8, 11, 9, Rgb(0xF8, 0xF8, 0xF0));
            canvas.FillRect(18, 9, 27, 16, Rgb(0x88, 0xC8, 0xD0));
            canvas.FillRect(5, 28, 3, 4, Rgb(0x48, 0x58, 0x60));
            canvas.FillRect(40, 28, 3, 4, Rgb(0x48, 0x58, 0x60));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        private static BitmapSource Cabinet()
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(24, 32);
            canvas.FillRect(2, 1, 20, 30, Rgb(0xD8, 0xD0, 0xB8));
            canvas.DrawRectBorder(2, 1, 20, 30, Rgb(0x58, 0x50, 0x48));
            canvas.FillRect(4, 10, 16, 1, Rgb(0x70, 0x68, 0x60));
            canvas.FillRect(4, 20, 16, 1, Rgb(0x70, 0x68, 0x60));
            canvas.Set(17, 7, Rgb(0x48, 0x48, 0x48));
            canvas.Set(17, 17, Rgb(0x48, 0x48, 0x48));
            canvas.Set(17, 27, Rgb(0x48, 0x48, 0x48));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        private static BitmapSource Shelf()
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(32, 32);
            canvas.FillRect(1, 2, 30, 29, Rgb(0x90, 0x60, 0x38));
            canvas.DrawRectBorder(1, 2, 30, 29, Rgb(0x48, 0x30, 0x28));
            canvas.FillRect(3, 10, 26, 2, Rgb(0xD0, 0xA0, 0x68));
            canvas.FillRect(3, 20, 26, 2, Rgb(0xD0, 0xA0, 0x68));
            canvas.FillRect(6, 5, 5, 5, Rgb(0xD8, 0x70, 0x48));
            canvas.FillRect(18, 5, 7, 5, Rgb(0x78, 0xA8, 0x58));
            canvas.FillRect(5, 14, 8, 5, Rgb(0xD8, 0xC0, 0x68));
            canvas.FillRect(19, 14, 8, 5, Rgb(0x78, 0xA8, 0xD0));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        private static BitmapSource Chair()
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(16, 20);
            canvas.FillRect(2, 2, 12, 9, Rgb(0x90, 0x58, 0x38));
            canvas.DrawRectBorder(2, 2, 12, 9, Rgb(0x48, 0x30, 0x28));
            canvas.FillRect(3, 10, 10, 4, Rgb(0xB8, 0x80, 0x48));
            canvas.FillRect(4, 14, 2, 6, Rgb(0x58, 0x38, 0x28));
            canvas.FillRect(10, 14, 2, 6, Rgb(0x58, 0x38, 0x28));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        private static BitmapSource Animal(string kind, int frame)
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(16, 16);
            if (kind == "duck")
            {
                canvas.FillEllipse(8, 10, 6, 4, Rgb(0xE8, 0xC8, 0x48));
                canvas.FillEllipse(11, 6, 3, 3, Rgb(0xF0, 0xD8, 0x60));
                canvas.FillRect(13, 7, 3, 1, Rgb(0xD8, 0x78, 0x38));
                canvas.Set(11, 6, Rgb(0x28, 0x30, 0x30));
                canvas.FillEllipse(frame == 0 ? 6 : 7, 10, 3, 2, Rgb(0xD8, 0xA8, 0x38));
            }
            else if (kind == "fish")
            {
                canvas.FillEllipse(8, 8, 5, 3, Rgb(0x58, 0xB8, 0xD0));
                canvas.FillRect(frame == 0 ? 1 : 2, 6, 4, 5, Rgb(0x38, 0x88, 0xB0));
                canvas.FillEllipse(10, 7, 1, 1, Rgb(0x18, 0x30, 0x38));
                canvas.FillRect(7, 4, 3, 2, Rgb(0x88, 0xD0, 0xD8));
            }
            else if (kind == "cat")
            {
                Color fur = Rgb(0xA8, 0x88, 0x68);
                canvas.FillRect(4, 8, 9, 5, fur);
                canvas.FillRect(5, 4, 7, 6, fur);
                canvas.FillRect(5, 2, 2, 4, Rgb(0x78, 0x58, 0x48));
                canvas.FillRect(10, 2, 2, 4, Rgb(0x78, 0x58, 0x48));
                canvas.FillRect(5, 12, 2, 3, Rgb(0x78, 0x58, 0x48));
                canvas.FillRect(10, 12, 2, 3, Rgb(0x78, 0x58, 0x48));
                canvas.FillRect(frame == 0 ? 12 : 13, 9, 3, 2, Rgb(0x78, 0x58, 0x48));
                canvas.Set(7, 7, Rgb(0x28, 0x38, 0x30));
                canvas.Set(10, 7, Rgb(0x28, 0x38, 0x30));
            }
            else
            {
                canvas.FillEllipse(8, 9, 3, 4, Rgb(0x68, 0x58, 0x48));
                canvas.FillEllipse(frame == 0 ? 4 : 5, 6, 4, 2, Rgb(0x48, 0x78, 0xA0));
                canvas.FillEllipse(frame == 0 ? 12 : 11, 6, 4, 2, Rgb(0x48, 0x78, 0xA0));
                canvas.FillRect(10, 8, 3, 1, Rgb(0xD8, 0xA8, 0x48));
                canvas.FillRect(7, 12, 1, 3, Rgb(0x48, 0x38, 0x30));
                canvas.FillRect(9, 12, 1, 3, Rgb(0x48, 0x38, 0x30));
            }

            canvas.AddOutline(Rgb(0x38, 0x38, 0x38));
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
