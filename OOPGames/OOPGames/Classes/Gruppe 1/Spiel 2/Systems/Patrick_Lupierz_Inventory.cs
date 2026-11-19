using System;
using System.Collections.Generic;
using System.Linq;

namespace OOPGames
{
    // Änderung durch: Patrick Lupierz – 15.11.2026 – Fahrzeug (Fahrräder) am Ende ergänzt
    // Änderung durch: Patrick Lupierz – 16.11.2026 – Fisch (Fang beim Angeln) am Ende ergänzt
    public enum G1C_ItemCategory { Kleidung, Lebensmittel, Getraenk, Verbrauch, Quest, Moebel, Sonstiges, Fahrzeug, Fisch }

    public enum G1C_ClothingSlot { None, Top, Pants, Shoes, Hat }

    // =====================================================
    // Klasse: G1C_Item
    // Entwickler: Patrick Lupierz
    // Erstellt: 12.11.2026
    // Beschreibung: Unveränderliche Beschreibung eines Gegenstands (Preis und Wert in Chips).
    // =====================================================
    public class G1C_Item
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; } = "";
        public G1C_ItemCategory Category { get; set; }
        public int Price { get; set; }
        public int SellValue { get; set; }                 // > 0: kann im Laden abgegeben werden
        public string BuffId { get; set; }                 // Effekt beim Benutzen
        public string[] Cures { get; set; } = new string[0]; // beendet diese Effekte
        public G1C_ClothingSlot Slot { get; set; }
        public uint Color { get; set; }                    // Kleidungsfarbe 0xRRGGBB
        public string FurnitureSpriteId { get; set; }
        public int FurnitureWidth { get; set; } = 1;
        public int FurnitureHeight { get; set; } = 1;
        public string IconId { get { return "item_" + Id; } }

        public bool IsUsable { get { return BuffId != null || Cures.Length > 0 || Category == G1C_ItemCategory.Fahrzeug; } }

        /// <summary>Kleidung und Fahrräder besitzt man nur einmal.</summary>
        public bool IsUnique { get { return Slot != G1C_ClothingSlot.None || Category == G1C_ItemCategory.Fahrzeug; } }
    }

    // Entwickler: Patrick Lupierz – alle Gegenstände des Spiels an einer Stelle
    public static class G1C_ItemCatalog
    {
        private static readonly Dictionary<string, G1C_Item> Items = Build();

        public static G1C_Item Get(string id)
        {
            G1C_Item item;
            return id != null && Items.TryGetValue(id, out item) ? item : null;
        }

        public static IEnumerable<G1C_Item> Starters(G1C_ClothingSlot slot)
        {
            return Items.Values.Where(i => i.Slot == slot && i.Price == 0);
        }

        private static Dictionary<string, G1C_Item> Build()
        {
            List<G1C_Item> list = new List<G1C_Item>
            {
                Consumable("water", "Wasser", "Vertreibt Kater und Husten.", G1C_ItemCategory.Getraenk, 15, null, "hangover", "cough"),
                Consumable("coffee", "Kaffee", "60 s wach: du läufst schneller.", G1C_ItemCategory.Getraenk, 25, "awake"),
                Consumable("energy", "Energydrink", "40 s sehr schnell, danach 40 s Crash.", G1C_ItemCategory.Getraenk, 40, "rush"),
                Consumable("sandwich", "Sandwich", "Gibt Kraft: beendet Crash und Kater.", G1C_ItemCategory.Lebensmittel, 35, null, "crash", "hangover"),
                Consumable("snack", "Snack", "Kleine Stärkung: beendet einen Crash.", G1C_ItemCategory.Lebensmittel, 20, null, "crash"),
                Consumable("apple", "Apfel", "Gesund: beendet Husten.", G1C_ItemCategory.Lebensmittel, 10, null, "cough"),
                Consumable("vodka", "Wodka", "90 s angetrunken: du torkelst, NPCs verlieren Respekt. Danach Kater.", G1C_ItemCategory.Verbrauch, 80, "drunk"),
                Consumable("cigarettes", "Zigaretten", "30 s entspannt (beendet Crash), danach 60 s Husten.", G1C_ItemCategory.Verbrauch, 60, "relaxed", "crash"),
                new G1C_Item { Id = "bottle", Name = "Pfandflasche", Description = "Kann im Laden abgegeben werden.", Category = G1C_ItemCategory.Sonstiges, SellValue = 8 },
                new G1C_Item { Id = "lucky_coin", Name = "Glücksmünze", Description = "Ein seltenes Sammlerstück.", Category = G1C_ItemCategory.Sonstiges, SellValue = 150 },

                Clothing("top_blue", "Blaues Shirt", G1C_ClothingSlot.Top, 0, 0x3060C8),
                Clothing("top_yellow", "Gelbes Shirt", G1C_ClothingSlot.Top, 0, 0xE8C030),
                Clothing("top_purple", "Lila Shirt", G1C_ClothingSlot.Top, 0, 0x8850B8),
                Clothing("pants_dark", "Dunkle Hose", G1C_ClothingSlot.Pants, 0, 0x383848),
                Clothing("pants_brown", "Braune Hose", G1C_ClothingSlot.Pants, 0, 0x6A4A2A),
                Clothing("shoes_red", "Rote Schuhe", G1C_ClothingSlot.Shoes, 0, 0xD83030),

                Clothing("top_red_jacket", "Rote Jacke", G1C_ClothingSlot.Top, 250, 0xD83030),
                Clothing("top_green_hoodie", "Grüner Hoodie", G1C_ClothingSlot.Top, 180, 0x38A050),
                Clothing("top_black_shirt", "Schwarzes Hemd", G1C_ClothingSlot.Top, 150, 0x282830),
                Clothing("top_white_shirt", "Weißes Hemd", G1C_ClothingSlot.Top, 120, 0xF0F0F0),
                Clothing("pants_jeans", "Jeans", G1C_ClothingSlot.Pants, 140, 0x3858A8),
                Clothing("pants_beige", "Beige Hose", G1C_ClothingSlot.Pants, 120, 0xC8B080),
                Clothing("shoes_white", "Weiße Sneaker", G1C_ClothingSlot.Shoes, 100, 0xF0F0F0),
                Clothing("shoes_black", "Schwarze Schuhe", G1C_ClothingSlot.Shoes, 100, 0x181818),
                Clothing("hat_red_cap", "Rote Mütze", G1C_ClothingSlot.Hat, 90, 0xD83030),
                Clothing("hat_blue_cap", "Blaue Mütze", G1C_ClothingSlot.Hat, 90, 0x3060C8),
                Clothing("hat_black_beanie", "Schwarze Beanie", G1C_ClothingSlot.Hat, 110, 0x282830),

                Furniture("furniture_plant", "Topfpflanze", "obj_plant", 1, 1, 80),
                Furniture("furniture_chair", "Stuhl", "obj_chair", 1, 1, 125),
                Furniture("furniture_table", "Beistelltisch", "obj_home_table", 2, 1, 220),
                Furniture("furniture_shelf", "Bücherregal", "obj_store_shelf", 2, 2, 280),
                Furniture("furniture_bed", "Bett", "obj_home_bed", 2, 1, 350),

                // Änderung durch: Patrick Lupierz – Datum: 15.11.2026 – Fahrräder (Farbe = Rahmenfarbe der Grafik)
                Bike("bike_red", "Rotes Fahrrad", 0, 0xD83030),
                Bike("bike_blue", "Blaues Fahrrad", 320, 0x3060C8),
                Bike("bike_green", "Grünes Fahrrad", 320, 0x38A050),
                Bike("bike_yellow", "Gelbes Fahrrad", 350, 0xE8C030),
                Bike("bike_black", "Schwarzes Rennrad", 480, 0x303038),

                // Änderung durch: Patrick Lupierz – Datum: 16.11.2026 – Fang beim Angeln (im Laden verkaufbar);
                // 20 Fischarten passend zum frei nutzbaren Fisch-Grafikpaket (G1C_FishSprites)
                Catch("fish_minnow", "Elritze", "Ein kleiner Schwarmfisch. Im Laden verkaufbar.", G1C_ItemCategory.Fisch, 6),
                Catch("fish_smelt", "Stint", "Riecht angeblich nach Gurke. Im Laden verkaufbar.", G1C_ItemCategory.Fisch, 5),
                Catch("fish_bluegill", "Sonnenbarsch", "Bunt wie ein Sommertag. Im Laden verkaufbar.", G1C_ItemCategory.Fisch, 12),
                Catch("fish_perch", "Barsch", "Gestreift und stachelig. Im Laden verkaufbar.", G1C_ItemCategory.Fisch, 15),
                Catch("fish_bream", "Brasse", "Flach und silbrig. Im Laden verkaufbar.", G1C_ItemCategory.Fisch, 14),
                Catch("fish_chub", "Döbel", "Frisst einfach alles. Im Laden verkaufbar.", G1C_ItemCategory.Fisch, 18),
                Catch("fish_trout", "Regenbogenforelle", "Schillert in allen Farben. Im Laden verkaufbar.", G1C_ItemCategory.Fisch, 25),
                Catch("fish_browntrout", "Bachforelle", "Mag kaltes, klares Wasser. Im Laden verkaufbar.", G1C_ItemCategory.Fisch, 28),
                Catch("fish_bass", "Schwarzbarsch", "Ein kräftiger Räuber. Im Laden verkaufbar.", G1C_ItemCategory.Fisch, 30),
                Catch("fish_carp", "Karpfen", "Schwer und kräftig. Im Laden verkaufbar.", G1C_ItemCategory.Fisch, 40),
                Catch("fish_flounder", "Flunder", "Platt wie ein Pfannkuchen. Im Laden verkaufbar.", G1C_ItemCategory.Fisch, 35),
                Catch("fish_zander", "Zander", "Jagt in der Dämmerung. Im Laden verkaufbar.", G1C_ItemCategory.Fisch, 50),
                Catch("fish_mackerel", "Makrele", "Wie kommt die in den Fluss? Im Laden verkaufbar.", G1C_ItemCategory.Fisch, 45),
                Catch("fish_redfish", "Rotbarsch", "Leuchtend rot. Im Laden verkaufbar.", G1C_ItemCategory.Fisch, 55),
                Catch("fish_catfish", "Wels", "Mit langen Barteln. Im Laden verkaufbar.", G1C_ItemCategory.Fisch, 70),
                Catch("fish_pike", "Hecht", "Der Räuber des Flusses. Im Laden verkaufbar.", G1C_ItemCategory.Fisch, 80),
                Catch("fish_salmon", "Lachs", "Schwimmt gegen jeden Strom. Im Laden verkaufbar.", G1C_ItemCategory.Fisch, 90),
                Catch("fish_sunfish", "Mondfisch", "Riesig, rund und sehr selten. Im Laden verkaufbar.", G1C_ItemCategory.Fisch, 120),
                Catch("fish_marlin", "Blauer Marlin", "Eine Legende mit Schwert. Im Laden verkaufbar.", G1C_ItemCategory.Fisch, 200),
                Catch("fish_golden", "Glücks-Koi", "Bringt Glück – und viele Chips. Im Laden verkaufbar.", G1C_ItemCategory.Fisch, 250),
                Catch("boot", "Alter Stiefel", "Tropft noch. Irgendwer nimmt alles.", G1C_ItemCategory.Sonstiges, 1),
                Catch("treasure_ring", "Verlorener Ring", "Ein goldener Ring aus dem Fluss.", G1C_ItemCategory.Sonstiges, 120),
            };
            return list.ToDictionary(i => i.Id);
        }

        private static G1C_Item Consumable(string id, string name, string description, G1C_ItemCategory category,
                                           int price, string buff, params string[] cures)
        {
            return new G1C_Item { Id = id, Name = name, Description = description, Category = category, Price = price, BuffId = buff, Cures = cures };
        }

        private static G1C_Item Clothing(string id, string name, G1C_ClothingSlot slot, int price, uint color)
        {
            return new G1C_Item { Id = id, Name = name, Description = "Kleidung – am Kleiderschrank anziehen.", Category = G1C_ItemCategory.Kleidung, Price = price, Slot = slot, Color = color };
        }

        // Entwickler: Patrick Lupierz – 15.11.2026
        private static G1C_Item Bike(string id, string name, int price, uint color)
        {
            return new G1C_Item
            {
                Id = id, Name = name, Category = G1C_ItemCategory.Fahrzeug, Price = price, Color = color,
                Description = "Fahrrad – im Inventar [E] zum Auf-/Absteigen (nur draußen). Fährt fast doppelt so schnell."
            };
        }

        // Entwickler: Patrick Lupierz – 16.11.2026 – Fang/Fundstück: nicht kaufbar, aber verkaufbar
        private static G1C_Item Catch(string id, string name, string description, G1C_ItemCategory category, int sellValue)
        {
            return new G1C_Item { Id = id, Name = name, Description = description, Category = category, SellValue = sellValue };
        }

        private static G1C_Item Furniture(string id, string name, string sprite, int width, int height, int price)
        {
            return new G1C_Item
            {
                Id = id, Name = name, Description = "Möbelstück – im Zuhause über den Einrichtungsplan platzieren.",
                Category = G1C_ItemCategory.Moebel, Price = price, FurnitureSpriteId = sprite,
                FurnitureWidth = width, FurnitureHeight = height
            };
        }
    }

    // =====================================================
    // Klasse: G1C_Inventory
    // Entwickler: Patrick Lupierz
    // Erstellt: 12.11.2026
    // Beschreibung: Zentrales Inventar (Gegenstand -> Anzahl). Besitzt bewusst kein eigenes Geld;
    //               bezahlt wird immer über den G1C_MoneyManager.
    // =====================================================
    public class G1C_Inventory
    {
        public Dictionary<string, int> Counts { get; private set; } = new Dictionary<string, int>();

        public int Count(string id)
        {
            int count;
            return id != null && Counts.TryGetValue(id, out count) ? count : 0;
        }

        public bool Has(string id) { return Count(id) > 0; }

        public void Add(string id, int amount = 1)
        {
            if (G1C_ItemCatalog.Get(id) == null) throw new ArgumentException("Unbekannter Gegenstand: " + id, nameof(id));
            if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount));
            Counts[id] = Count(id) + amount;
        }

        public bool Remove(string id, int amount = 1)
        {
            if (amount <= 0 || Count(id) < amount) return false;
            Counts[id] -= amount;
            if (Counts[id] == 0) Counts.Remove(id);
            return true;
        }

        public IEnumerable<G1C_Item> Items
        {
            get { return Counts.Keys.Select(G1C_ItemCatalog.Get).Where(i => i != null).OrderBy(i => i.Category).ThenBy(i => i.Name); }
        }

        // Spielstand: nur bekannte Gegenstände mit positiver Anzahl übernehmen
        public void Restore(Dictionary<string, int> counts)
        {
            Counts = new Dictionary<string, int>();
            if (counts == null) return;
            foreach (KeyValuePair<string, int> pair in counts)
                if (pair.Value > 0 && G1C_ItemCatalog.Get(pair.Key) != null) Counts[pair.Key] = pair.Value;
        }
    }

    public class G1C_ActiveBuff
    {
        public string Id { get; set; }
        public int EndTick { get; set; } // Zeitpunkt auf der Ingame-Uhr
    }

    // =====================================================
    // Klasse: G1C_BuffSystem
    // Entwickler: Patrick Lupierz
    // Erstellt: 12.11.2026
    // Beschreibung: Zeitlich begrenzte Effekte von Verbrauchsgegenständen. Jeder Effekt hat eine feste
    //               Dauer auf der Ingame-Uhr, endet automatisch und kann einen Folgeeffekt (Nachteil) haben.
    //               Erneutes Benutzen stapelt nicht, sondern setzt die Dauer neu. Effekte wirken nur auf
    //               Lauftempo, Steuerung und Gespräche – nie auf die Gewinnchancen der Casinospiele.
    // =====================================================
    public class G1C_BuffSystem
    {
        private class Definition
        {
            public string Name; public int Duration; public int StepTicks; public string Next;
        }

        // 25 Ticks = 1 Sekunde. StepTicks: 0 = Lauftempo unverändert (normal: 5 Ticks pro Kachel)
        private static readonly Dictionary<string, Definition> Definitions = new Dictionary<string, Definition>
        {
            { "awake",    new Definition { Name = "Wach",        Duration = 1500, StepTicks = 4 } },
            { "rush",     new Definition { Name = "Aufgedreht",  Duration = 1000, StepTicks = 3, Next = "crash" } },
            { "crash",    new Definition { Name = "Crash",       Duration = 1000, StepTicks = 7 } },
            { "relaxed",  new Definition { Name = "Entspannt",   Duration = 750,  StepTicks = 0, Next = "cough" } },
            { "cough",    new Definition { Name = "Husten",      Duration = 1500, StepTicks = 6 } },
            { "drunk",    new Definition { Name = "Angetrunken", Duration = 2250, StepTicks = 0, Next = "hangover" } },
            { "hangover", new Definition { Name = "Kater",       Duration = 1500, StepTicks = 6 } },
        };

        public List<G1C_ActiveBuff> Active { get; private set; } = new List<G1C_ActiveBuff>();

        public bool Has(string id) { return Active.Any(b => b.Id == id); }

        // Entwickler: Patrick Lupierz – Gegenstand anwenden; gibt eine Meldung für den Spieler zurück
        public string Apply(G1C_Item item, int clockTick)
        {
            foreach (string cure in item.Cures) Active.RemoveAll(b => b.Id == cure);
            if (item.BuffId == null) return item.Name + ": " + item.Description;
            Start(item.BuffId, clockTick);
            return item.Name + ": " + Definitions[item.BuffId].Name + "!";
        }

        // Entwickler: Patrick Lupierz – abgelaufene Effekte beenden und Folgeeffekte starten
        public void Update(int clockTick)
        {
            foreach (G1C_ActiveBuff ended in Active.Where(b => clockTick >= b.EndTick).ToList())
            {
                Active.Remove(ended);
                string next = Definitions[ended.Id].Next;
                if (next != null) Start(next, clockTick);
            }
        }

        /// <summary>Ticks pro Kachel: Nachteile (langsam) haben Vorrang vor Vorteilen.</summary>
        public int StepTicks
        {
            get
            {
                List<int> speeds = Active.Select(b => Definitions[b.Id].StepTicks).Where(s => s > 0).ToList();
                if (speeds.Count == 0) return G1C_Player.TicksPerStep;
                int slowest = speeds.Max();
                return slowest > G1C_Player.TicksPerStep ? slowest : speeds.Min();
            }
        }

        public string Describe(int clockTick)
        {
            return string.Join("  ", Active.Select(b => Definitions[b.Id].Name + " " + Math.Max(0, (b.EndTick - clockTick) / 25) + "s"));
        }

        public void Restore(List<G1C_ActiveBuff> buffs)
        {
            Active = buffs == null ? new List<G1C_ActiveBuff>() : buffs.Where(b => b != null && b.Id != null && Definitions.ContainsKey(b.Id)).ToList();
        }

        private void Start(string id, int clockTick)
        {
            Active.RemoveAll(b => b.Id == id);
            Active.Add(new G1C_ActiveBuff { Id = id, EndTick = clockTick + Definitions[id].Duration });
        }
    }

    // =====================================================
    // Klasse: G1C_Shop
    // Entwickler: Patrick Lupierz
    // Erstellt: 12.11.2026
    // Beschreibung: Ein Geschäft mit Sortiment und Öffnungszeiten. Alle Läden benutzen dieselbe
    //               Kauf-/Verkaufslogik; bezahlt wird ausschließlich mit Chips aus dem MoneyManager.
    // =====================================================
    public class G1C_Shop
    {
        public string Name { get; private set; }
        public string[] ItemIds { get; private set; }
        public int OpenFrom { get; private set; }
        public int OpenUntil { get; private set; } // 24 = durchgehend

        public static G1C_Shop For(G1C_WorldLocation location)
        {
            switch (location)
            {
                case G1C_WorldLocation.GasStation:
                    return new G1C_Shop { Name = "Tankstelle", OpenFrom = 0, OpenUntil = 24,
                        ItemIds = new[] { "water", "coffee", "energy", "sandwich", "snack", "vodka", "cigarettes" } };
                case G1C_WorldLocation.ClothingStore:
                    return new G1C_Shop { Name = "Bekleidungsgeschäft", OpenFrom = 9, OpenUntil = 19,
                        ItemIds = new[] { "top_red_jacket", "top_green_hoodie", "top_black_shirt", "top_white_shirt", "pants_jeans",
                                          "pants_beige", "shoes_white", "shoes_black", "hat_red_cap", "hat_blue_cap", "hat_black_beanie" } };
                case G1C_WorldLocation.Market:
                    return new G1C_Shop { Name = "Supermarkt", OpenFrom = 7, OpenUntil = 21,
                        ItemIds = new[] { "water", "apple", "snack", "sandwich", "coffee" } };
                // Änderung durch: Patrick Lupierz – 15.11.2026 – Fahrradladen
                case G1C_WorldLocation.BikeShop:
                    return new G1C_Shop { Name = "Fahrradladen", OpenFrom = 9, OpenUntil = 18,
                        ItemIds = new[] { "bike_red", "bike_blue", "bike_green", "bike_yellow", "bike_black" } };
                case G1C_WorldLocation.FurnitureStore:
                    return new G1C_Shop { Name = "Möbelhaus", OpenFrom = 9, OpenUntil = 19,
                        ItemIds = new[] { "furniture_plant", "furniture_chair", "furniture_table", "furniture_shelf", "furniture_bed" } };
                default: return null;
            }
        }

        public bool IsOpen(int hour) { return hour >= OpenFrom && hour < OpenUntil; }

        // Entwickler: Patrick Lupierz – Datum: 15.11.2026 – alle Orte mit Laden (für CLOSED-Schilder und Selbsttest)
        public static readonly G1C_WorldLocation[] Locations =
        {
            G1C_WorldLocation.GasStation, G1C_WorldLocation.ClothingStore, G1C_WorldLocation.Market, G1C_WorldLocation.FurnitureStore,
            G1C_WorldLocation.BikeShop
        };

        // Entwickler: Patrick Lupierz – Kauf: erst abbuchen, dann ins Inventar. Kleidung gibt es nur einmal.
        public static string Buy(G1C_Item item, G1C_MoneyManager money, G1C_Inventory inventory)
        {
            if (item.IsUnique && inventory.Has(item.Id)) return "Das besitzt du schon."; // Patrick Lupierz – 15.11.2026: auch Fahrräder
            int price = PriceWithMarkup(item.Price);
            if (!money.TryWithdraw(price)) return "Nicht genug Chips.";
            inventory.Add(item.Id);
            return item.Name + " gekauft (-" + price + " Chips).";
        }

        public static int PriceWithMarkup(int price) { return (price * 105 + 99) / 100; }

        /// <summary>Höchstzahl je Möbelstück (Inventar + aufgestellt). Patrick Lupierz – 16.11.2026</summary>
        public const int MaxFurniturePerItem = 3;

        // Entwickler: Patrick Lupierz – Datum: 16.11.2026
        // Aufgabe: Kauf mit Mengengrenze für Möbel. placedCount = bereits aufgestellte Stücke dieses Möbels.
        public static string Buy(G1C_Item item, G1C_MoneyManager money, G1C_Inventory inventory, int placedCount)
        {
            if (item.Category == G1C_ItemCategory.Moebel && inventory.Count(item.Id) + placedCount >= MaxFurniturePerItem)
                return "Von " + item.Name + " hast du schon " + MaxFurniturePerItem + " Stück – mehr passt nicht ins Haus.";
            return Buy(item, money, inventory);
        }

        public static string Sell(G1C_Item item, G1C_MoneyManager money, G1C_Inventory inventory)
        {
            if (item.SellValue <= 0 || !inventory.Remove(item.Id)) return "Das kann ich nicht annehmen.";
            money.Deposit(item.SellValue);
            return item.Name + " abgegeben (+" + item.SellValue + " Chips).";
        }
    }
}
