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
                case "building_pavilion_back": return PavilionBack();
                case "building_pavilion_front": return PavilionFront();
                case "building_pavilion_roof": return PavilionRoof();
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
        // Änderung durch: Patrick Lupierz – Datum: 15.11.2026 – Änderung: Der Pavillon sah aus wie ein Schild
        // (helle Tafel zwischen zwei Pfosten). Jetzt: Walmdach mit Ziegelreihen, Spitze und Laterne, vier Säulen
        // und darunter freie Fläche. Der untere Bereich ist transparent, damit man die Figur darunter sieht.
        private static BitmapSource Pavilion()
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(64, 80);
            Color roof = Rgb(0xB0, 0x48, 0x38), roofLight = Rgb(0xD0, 0x68, 0x50), roofDark = Rgb(0x80, 0x30, 0x28);
            Color pillar = Rgb(0xF0, 0xE8, 0xD8), pillarShade = Rgb(0xC0, 0xB4, 0xA0), outline = Rgb(0x38, 0x28, 0x28);

            // hintere Säulen (unter dem Dach, kürzer und dunkler = weiter hinten)
            canvas.FillRect(12, 30, 3, 26, pillarShade);
            canvas.FillRect(49, 30, 3, 26, pillarShade);
            // Walmdach
            for (int y = 3; y < 30; y++)
            {
                int inset = System.Math.Max(0, (15 - y) * 2);
                Color row = y < 7 ? roofLight : (y % 5 == 0 ? roofDark : roof);
                canvas.FillRect(1 + inset, y, 62 - 2 * inset, 1, row);
            }
            canvas.FillRect(1, 29, 62, 2, Rgb(0xE8, 0xD8, 0xB8));   // Traufkante
            canvas.FillRect(1, 31, 62, 2, roofDark);                 // Schatten unter der Traufe
            canvas.FillRect(30, 0, 4, 4, Rgb(0xE8, 0xC0, 0x40));     // Dachspitze
            canvas.FillRect(31, 33, 2, 3, outline);                  // Laterne
            canvas.FillRect(29, 36, 6, 5, Rgb(0xF8, 0xD8, 0x70));
            canvas.DrawRectBorder(29, 36, 6, 5, outline);
            // vordere Säulen mit Sockel und Kapitell
            foreach (int x in new[] { 2, 56 })
            {
                canvas.FillRect(x, 33, 6, 44, pillar);
                canvas.FillRect(x + 4, 33, 2, 44, pillarShade);
                canvas.FillRect(x - 1, 33, 8, 3, Rgb(0xD8, 0xCC, 0xB8));
                canvas.FillRect(x - 1, 75, 8, 4, Rgb(0xA8, 0xA0, 0x90));
                canvas.DrawRectBorder(x, 33, 6, 44, outline);
            }
            canvas.AddOutline(outline);
            return canvas.ToBitmap();
        }

        // Änderung durch: Denis Mock – Datum: 19.11.2026 – Dach als eigener Vordergrund-Layer
        private static BitmapSource PavilionRoof()
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(64, 80);
            Color roof = Rgb(0xB0, 0x48, 0x38), roofLight = Rgb(0xD0, 0x68, 0x50), roofDark = Rgb(0x80, 0x30, 0x28);
            for (int y = 3; y < 30; y++)
            {
                int inset = System.Math.Max(0, (15 - y) * 2);
                Color row = y < 7 ? roofLight : (y % 5 == 0 ? roofDark : roof);
                canvas.FillRect(1 + inset, y, 62 - 2 * inset, 1, row);
            }
            canvas.FillRect(1, 29, 62, 2, Rgb(0xE8, 0xD8, 0xB8));
            canvas.FillRect(30, 0, 4, 4, Rgb(0xE8, 0xC0, 0x40));
            canvas.AddOutline(Rgb(0x38, 0x28, 0x28));
            return canvas.ToBitmap();
        }

        // Änderung durch: Patrick Lupierz – Datum: 19.11.2026 – Dach und hintere Stützen werden getrennt gerendert
        private static BitmapSource PavilionBack()
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(64, 80);
            Color roof = Rgb(0xB0, 0x48, 0x38), roofLight = Rgb(0xD0, 0x68, 0x50), roofDark = Rgb(0x80, 0x30, 0x28);
            Color pillarShade = Rgb(0xC0, 0xB4, 0xA0), outline = Rgb(0x38, 0x28, 0x28);
            canvas.FillRect(12, 30, 3, 26, pillarShade);
            canvas.FillRect(49, 30, 3, 26, pillarShade);
            for (int y = 3; y < 30; y++)
            {
                int inset = System.Math.Max(0, (15 - y) * 2);
                Color row = y < 7 ? roofLight : (y % 5 == 0 ? roofDark : roof);
                canvas.FillRect(1 + inset, y, 62 - 2 * inset, 1, row);
            }
            canvas.FillRect(1, 29, 62, 2, Rgb(0xE8, 0xD8, 0xB8));
            canvas.FillRect(1, 31, 62, 2, roofDark);
            canvas.FillRect(30, 0, 4, 4, Rgb(0xE8, 0xC0, 0x40));
            canvas.AddOutline(outline);
            return canvas.ToBitmap();
        }

        // Änderung durch: Patrick Lupierz – Datum: 19.11.2026 – Vorderstützen liegen über der Spielfigur
        private static BitmapSource PavilionFront()
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(64, 80);
            Color pillar = Rgb(0xF0, 0xE8, 0xD8), pillarShade = Rgb(0xC0, 0xB4, 0xA0), outline = Rgb(0x38, 0x28, 0x28);
            foreach (int x in new[] { 2, 56 })
            {
                canvas.FillRect(x, 33, 6, 44, pillar);
                canvas.FillRect(x + 4, 33, 2, 44, pillarShade);
                canvas.FillRect(x - 1, 33, 8, 3, Rgb(0xD8, 0xCC, 0xB8));
                canvas.FillRect(x - 1, 75, 8, 4, Rgb(0xA8, 0xA0, 0x90));
                canvas.DrawRectBorder(x, 33, 6, 44, outline);
            }
            canvas.AddOutline(outline);
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        // Änderung durch: Patrick Lupierz – Datum: 15.11.2026 – flachere, realistischere Theke (helle Arbeitsplatte,
        // Front mit Kassettenfüllungen, Sockelleiste). Die alte Grafik war so hoch, dass sie Verkäufer verdeckte.
        private static BitmapSource Counter()
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(48, 16);
            canvas.FillRect(0, 0, 48, 5, Rgb(0xD8, 0xA8, 0x70));                 // Arbeitsplatte
            canvas.FillRect(0, 0, 48, 1, Rgb(0xF0, 0xC8, 0x90));
            canvas.FillRect(0, 5, 48, 9, Rgb(0x90, 0x58, 0x38));                 // Front
            for (int x = 3; x < 46; x += 11) canvas.DrawRectBorder(x, 7, 8, 5, Rgb(0x68, 0x40, 0x28));
            canvas.FillRect(0, 14, 48, 2, Rgb(0x50, 0x34, 0x24));                // Sockel
            canvas.AddOutline(Rgb(0x38, 0x24, 0x20));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        // Änderung durch: Patrick Lupierz – Datum: 15.11.2026 – realistischeres Krankenbett: Metallrahmen mit
        // Gitter am Kopfende, Kissen, mintgrüne Decke mit Umschlag, Rollen, Bodenschatten.
        private static BitmapSource MedicalBed()
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(48, 32);
            Color metal = Rgb(0xB8, 0xC4, 0xC8), metalDark = Rgb(0x70, 0x80, 0x88);
            canvas.FillRect(4, 31, 40, 1, Rgb(0x50, 0x58, 0x60));
            canvas.FillRect(2, 2, 3, 26, metal);                          // Kopfgitter
            for (int y = 4; y < 26; y += 4) canvas.FillRect(2, y, 3, 1, metalDark);
            canvas.FillRect(4, 6, 41, 20, metalDark);                     // Rahmen
            canvas.FillRect(5, 7, 39, 18, Rgb(0xF4, 0xF4, 0xEC));         // Laken
            canvas.FillRect(6, 9, 10, 13, Rgb(0xFF, 0xFF, 0xFF));         // Kissen
            canvas.FillRect(15, 9, 1, 13, Rgb(0xC8, 0xC8, 0xC0));
            canvas.FillRect(18, 8, 26, 16, Rgb(0x88, 0xC8, 0xB8));        // Decke
            canvas.FillRect(18, 8, 3, 16, Rgb(0xB8, 0xE8, 0xD8));         // Umschlag
            canvas.FillRect(18, 22, 26, 2, Rgb(0x60, 0xA0, 0x90));
            canvas.FillRect(44, 6, 2, 20, metal);                         // Fußende
            foreach (int x in new[] { 6, 40 }) { canvas.FillRect(x, 26, 2, 3, metalDark); canvas.FillRect(x - 1, 29, 4, 2, Rgb(0x30, 0x30, 0x38)); }
            canvas.AddOutline(Rgb(0x38, 0x40, 0x48));
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
        // Änderung durch: Patrick Lupierz – Datum: 15.11.2026 – realistischerer Stuhl: Lehne mit Sprossen,
        // gepolsterte Sitzfläche mit Lichtkante, vier Beine (hintere dunkler), Bodenschatten.
        private static BitmapSource Chair()
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(16, 20);
            Color wood = Rgb(0x9A, 0x60, 0x38), woodDark = Rgb(0x60, 0x3C, 0x24), woodLight = Rgb(0xC8, 0x8C, 0x58);
            canvas.FillRect(3, 19, 10, 1, Rgb(0x50, 0x40, 0x38));        // Schatten
            canvas.FillRect(3, 1, 10, 2, woodLight);                     // Lehne oben
            canvas.FillRect(3, 3, 2, 8, wood); canvas.FillRect(11, 3, 2, 8, wood);
            canvas.FillRect(6, 3, 1, 7, woodDark); canvas.FillRect(9, 3, 1, 7, woodDark); // Sprossen
            canvas.FillRect(5, 13, 1, 5, woodDark); canvas.FillRect(10, 13, 1, 5, woodDark); // hintere Beine
            canvas.FillRect(2, 10, 12, 4, Rgb(0xB0, 0x40, 0x40));        // Polster
            canvas.FillRect(2, 10, 12, 1, Rgb(0xD8, 0x68, 0x60));
            canvas.FillRect(2, 13, 12, 1, Rgb(0x80, 0x28, 0x28));
            canvas.FillRect(2, 14, 2, 5, wood); canvas.FillRect(12, 14, 2, 5, wood); // vordere Beine
            canvas.AddOutline(Rgb(0x38, 0x24, 0x20));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        private static BitmapSource Animal(string kind, int frame)
        {
            if (kind == "bird") return PerchedBird(frame); // Änderung durch: Patrick Lupierz – 15.11.2026
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
        // Änderung durch: Patrick Lupierz – Datum: 15.11.2026 – realistischeres Bett: Kopfteil mit Kassetten,
        // Matratze, zwei Kissen mit Schatten, Steppdecke mit Umschlag und Karomuster, Fußteil, Bodenschatten.
        private static BitmapSource Bed()
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(32, 24);
            Color wood = Rgb(0x8A, 0x5A, 0x34), woodDark = Rgb(0x5E, 0x3A, 0x22), woodLight = Rgb(0xB8, 0x80, 0x4C);
            Color quilt = Rgb(0x48, 0x78, 0xC0), quiltLight = Rgb(0x88, 0xB0, 0xE8), quiltDark = Rgb(0x30, 0x58, 0x98);
            canvas.FillRect(2, 23, 28, 1, Rgb(0x50, 0x40, 0x38));
            canvas.FillRect(1, 0, 30, 6, wood);                          // Kopfteil
            canvas.FillRect(1, 0, 30, 1, woodLight);
            canvas.FillRect(4, 2, 10, 2, woodDark); canvas.FillRect(18, 2, 10, 2, woodDark);
            canvas.FillRect(1, 6, 30, 15, woodDark);                     // Rahmen
            canvas.FillRect(3, 6, 26, 14, Rgb(0xF0, 0xEC, 0xE0));        // Laken
            for (int x = 5; x <= 17; x += 12)
            {
                canvas.FillRect(x, 7, 10, 5, Rgb(0xFF, 0xFF, 0xFF));     // Kissen
                canvas.FillRect(x, 11, 10, 1, Rgb(0xC8, 0xC4, 0xB8));
                canvas.FillRect(x + 1, 8, 3, 1, Rgb(0xE8, 0xE4, 0xD8));
            }
            canvas.FillRect(3, 13, 26, 7, quilt);                        // Decke
            canvas.FillRect(3, 13, 26, 2, quiltLight);                   // Umschlag
            for (int y = 16; y < 20; y += 3)
                for (int x = 5; x < 28; x += 4) canvas.Set(x, y, quiltDark);
            canvas.FillRect(27, 15, 2, 5, quiltDark);                    // Seitenschatten
            canvas.FillRect(1, 20, 30, 3, wood);                         // Fußteil
            canvas.FillRect(1, 20, 30, 1, woodLight);
            canvas.AddOutline(Rgb(0x30, 0x20, 0x18));
            return canvas.ToBitmap();
        }

        // Entwickler: Denis Mock
        // Änderung durch: Patrick Lupierz – Datum: 15.11.2026 – realistischerer Tisch: Holzmaserung, Lichtkante,
        // sichtbare Zarge, vier Beine (hintere dunkler), Tischdecke-Läufer mit Vase, Bodenschatten.
        private static BitmapSource Table()
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(32, 16);
            Color wood = Rgb(0xA8, 0x70, 0x40), woodDark = Rgb(0x60, 0x3C, 0x24), woodLight = Rgb(0xD0, 0x98, 0x60);
            canvas.FillRect(3, 15, 26, 1, Rgb(0x50, 0x40, 0x38));
            canvas.FillRect(6, 10, 2, 5, woodDark); canvas.FillRect(24, 10, 2, 5, woodDark); // hintere Beine
            canvas.FillRect(1, 2, 30, 7, wood);                          // Platte
            canvas.FillRect(1, 2, 30, 1, woodLight);
            canvas.FillRect(4, 4, 9, 1, Rgb(0x90, 0x5C, 0x34)); canvas.FillRect(17, 6, 10, 1, Rgb(0x90, 0x5C, 0x34));
            canvas.FillRect(11, 3, 10, 5, Rgb(0xE8, 0xE0, 0xD0));        // Läufer
            canvas.FillRect(15, 0, 3, 4, Rgb(0x58, 0x88, 0xC8));         // Vase
            canvas.Set(16, 0, Rgb(0xE8, 0x60, 0x70));
            canvas.FillRect(1, 9, 30, 2, woodDark);                      // Zarge
            canvas.FillRect(2, 11, 2, 4, wood); canvas.FillRect(28, 11, 2, 4, wood); // vordere Beine
            canvas.AddOutline(Rgb(0x38, 0x24, 0x20));
            return canvas.ToBitmap();
        }

        // Entwickler: Patrick Lupierz – Datum: 15.11.2026
        // Aufgabe: Vogel-Korrektur. Der alte Vogel (16x16, blaue Flügelflächen ohne Kontur) lag auf dem Baumstamm
        //          am Boden und verdeckte ihn. Jetzt: kleines Rotkehlchen in Seitenansicht mit Kontur, das OBEN auf der
        //          Baumkrone sitzt (hohe, unten transparente Grafik wie der Baum, unten bündig gezeichnet).
        //          Frame 0 = sitzt, Frame 1 = Kopf geneigt und Flügel leicht geöffnet.
        private static BitmapSource PerchedBird(int frame)
        {
            G1C_PixelCanvas canvas = new G1C_PixelCanvas(16, 40); // Baum ist 16x30: Füße (Zeile 11) liegen auf der Kronenspitze
            Color back = Rgb(0x70, 0x58, 0x40), breast = Rgb(0xE8, 0x78, 0x38), belly = Rgb(0xF0, 0xE0, 0xC8);
            int head = frame == 0 ? 2 : 3;
            canvas.FillRect(5, 5, 7, 5, back);                       // Körper
            canvas.FillRect(6, 7, 5, 3, breast);                     // rote Brust
            canvas.FillRect(7, 9, 3, 1, belly);
            canvas.FillRect(9, head, 4, 4, back);                    // Kopf
            canvas.FillRect(10, head + 2, 2, 2, breast);
            canvas.Set(11, head + 1, Rgb(0x10, 0x10, 0x10));         // Auge
            canvas.FillRect(13, head + 2, 2, 1, Rgb(0xD8, 0xA8, 0x40)); // Schnabel
            canvas.FillRect(2, 5, 3, 2, back);                       // Schwanz
            if (frame == 1) canvas.FillRect(4, 3, 4, 2, Rgb(0x58, 0x44, 0x30)); // Flügel angehoben
            canvas.FillRect(7, 10, 1, 2, Rgb(0x40, 0x30, 0x20)); canvas.FillRect(9, 10, 1, 2, Rgb(0x40, 0x30, 0x20));
            canvas.AddOutline(Rgb(0x28, 0x20, 0x18));
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
