using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_SoundDef
    // Entwickler: Patrick Lupierz
    // Erstellt: 19.11.2026
    // Beschreibung: Beschreibung eines Sounds im Spiel: Id (so wird er aufgerufen), Anzeigename für den Handy-Mixer,
    //               Kategorie, Ordner und Dateimuster im Ordner Assets\Sounds, Standardwerte und Verhalten.
    //               Dateien werden über reguläre Ausdrücke auf den Dateinamen (ohne Endung) gefunden; passen mehrere
    //               Dateien (z. B. StepGrass, StepGrass (2), StepGrass (3)), sind das Varianten desselben Sounds.
    // =====================================================
    public sealed class G1C_SoundDef
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; } = "";
        public string Category { get; set; }
        public string Folder { get; set; }              // relativ zu Assets\Sounds; null = nur eingebaute Version
        public string[] Patterns { get; set; } = new string[0];
        public string Fallback { get; set; }            // eingebaute Chiptune-Version (G1C_ChipTune), wenn keine Datei da ist
        public bool IsMusic { get; set; }
        public bool Loop { get; set; }                  // Effekt läuft in Schleife, bis er gestoppt wird
        public bool Sequential { get; set; }            // Varianten reihum statt zufällig
        public bool Optional { get; set; }              // darf ohne Datei bleiben (nur eingebaute Version)
        public int Priority { get; set; } = 2;          // 1 niedrig .. 5 hoch: verdrängt bei zu vielen gleichzeitigen Tönen
        public int Volume { get; set; } = 100;
        public int Chance { get; set; } = 100;
        public int CooldownMs { get; set; }
    }

    // =====================================================
    // Klasse: G1C_SoundCatalog
    // Entwickler: Patrick Lupierz
    // Erstellt: 19.11.2026
    // Beschreibung: Alle Sounds des Spiels an einer Stelle (Quelle: Sound-Paket "CasinoAdventureSounds"). Wer einen Sound
    //               austauschen will, ersetzt die Datei im Ordner; wer einen neuen Auslöser baut, ruft
    //               G1C_SoundSystem.Play("<id>") auf. Die Standardlautstärken gleichen die unterschiedlich lauten
    //               Originale grob an; den Feinschliff macht der Handy-Mixer.
    // =====================================================
    public static class G1C_SoundCatalog
    {
        public const string Music = "Musik";
        public const string Steps = "Schritte";
        public const string Animals = "Tiere";
        public const string Casino = "Casino";
        public const string General = "Allgemein";

        public static readonly string[] Categories = { Music, Steps, Animals, Casino, General };

        public static readonly G1C_SoundDef[] All = Build();

        private static readonly Dictionary<string, G1C_SoundDef> ById = All.ToDictionary(d => d.Id);

        public static G1C_SoundDef Find(string id)
        {
            G1C_SoundDef def;
            return id != null && ById.TryGetValue(id, out def) ? def : null;
        }

        private static G1C_SoundDef M(string theme, string name, string description, string folder, string pattern,
                                      int volume, bool optional = false)
        {
            return new G1C_SoundDef
            {
                Id = "music_" + theme, Name = name, Description = description, Category = Music, Folder = folder,
                Patterns = new[] { pattern }, Fallback = "music_" + theme, IsMusic = true, Volume = volume, Optional = optional
            };
        }

        private static G1C_SoundDef E(string id, string category, string name, string description, string folder, string pattern,
                                      int volume, int priority = 2, int chance = 100, int cooldownMs = 0, string fallback = null,
                                      bool loop = false, bool sequential = false, bool optional = false)
        {
            return new G1C_SoundDef
            {
                Id = id, Name = name, Description = description, Category = category, Folder = folder,
                Patterns = pattern == null ? new string[0] : new[] { pattern }, Fallback = fallback, Loop = loop,
                Sequential = sequential, Optional = optional, Priority = priority, Volume = volume, Chance = chance,
                CooldownMs = cooldownMs
            };
        }

        private static G1C_SoundDef[] Build()
        {
            const string stepFolder = "Soundeffekte/Schritte", animalFolder = "Soundeffekte/Tiere";
            const string casinoFolder = "Soundeffekte/Casino", generalFolder = "Soundeffekte/Allgemein";
            return new[]
            {
                // ---- Musik und Ambiente (läuft immer nur ein Stück; mehrere Dateien wechseln sich ab)
                M("town", "Stadt (draußen)", "Ambiente in der Stadt, mehrere Aufnahmen im Wechsel", "Musik_Aussen", "^Ambient Ou", 100),
                M("title", "Titelbild und Charakter", "Ambiente im Hauptmenü und bei der Charaktererstellung", "Musik_Gebaeude", "^Ambient Charakter$", 100),
                M("casino", "Casino", "Hintergrundmusik im Casino", "Musik_Gebaeude", "^CasinoBackgroundLoop$", 70),
                M("arcade", "Spielhalle", "Chip-Musik in der Spielhalle, vier Stücke im Wechsel", "Musik_Gebaeude", "^chippy", 70),
                M("home", "Zuhause", "Ambiente 'AmbientSound house.mp3' (oder eigene Datei Home.mp3) in Musik_Gebaeude; sonst eingebaute Melodie", "Musik_Gebaeude", "^(Home|AmbientSound house)$", 100, true),
                M("shop", "Läden und Bank", "Ambiente 'AmbientSound house.mp3' (oder eigene Datei Shop.mp3) in Musik_Gebaeude; sonst eingebaute Melodie", "Musik_Gebaeude", "^(Shop|AmbientSound house)$", 100, true),
                M("fishing", "Angeln", "Eigene Datei 'Fishing.mp3' in Musik_Aussen ablegen; sonst eingebaute Melodie", "Musik_Aussen", "^Fishing$", 100, true),

                // ---- Schritte: Untergrund und Name der Datei bestimmen die Auswahl; Varianten laufen reihum
                E("step_grass", Steps, "Schritt – Gras", "Wiese, Park, Blumen", stepFolder, "^StepGrass", 55, 1, 100, 100, null, false, true),
                E("step_path", Steps, "Schritt – Weg und Straße", "Wege, Straßen, Steinplatten", stepFolder, "^StepsPath", 50, 1, 100, 100, null, false, true),
                E("step_bridge", Steps, "Schritt – Brücke", "Holzbrücken über den Fluss", stepFolder, "^StepBridge", 55, 1, 100, 100, null, false, true),
                E("step_indoor", Steps, "Schritt – Innenräume", "Zuhause, Läden, Casino und alle Gebäude", stepFolder, "^Steps_Indoor", 45, 1, 100, 100, null, false, true),
                E("bicycle", General, "Fahrrad", "Läuft in Schleife, solange du fährst", generalFolder, "^BicycleSoundLoop$", 35, 1, 100, 0, null, true),

                // ---- Tiere: nur beim Streicheln; Vögel nur im Umkreis
                E("meow", Animals, "Katze – Miau", "Nur beim Streicheln", animalFolder, "^catMeow", 70, 2, 100, 1000, "sfx_meow"),
                E("woof", Animals, "Hund – Wuff", "Nur beim Streicheln (drei Aufnahmen im Wechsel)", animalFolder, "^(dogBark|dog_bark|puppy_bark)", 70, 2, 100, 1000, "sfx_woof"),
                E("bird", Animals, "Vogel – Zwitschern", "Nur, wenn du dich im Vogel-Radius befindest", animalFolder, "^songbird", 55, 1, 60, 7000),
                E("frog", Animals, "Frosch – Quaken", "Noch kein Frosch im Spiel; Sound liegt bereit", animalFolder, "^frog", 60, 1, 100, 4000),

                // ---- Casino
                E("slot_lever", Casino, "Spielautomat – Hebel", "Beim Drehen", casinoFolder, "^slot_lever", 70),
                E("slots_spin", Casino, "Spielautomat – Walzen", "Läuft in Schleife, solange sich die Walzen drehen", casinoFolder, "^SlotsSpinSoundLoop$", 55, 2, 100, 0, null, true),
                E("slot_win_small", Casino, "Spielautomat – kleiner Gewinn", "Gewinn unter dem 20-fachen Einsatz", casinoFolder, "^slot_win_small", 75, 3),
                E("slot_win_big", Casino, "Spielautomat – großer Gewinn", "Gewinn ab dem 20-fachen Einsatz", casinoFolder, "^slot_win_big", 85, 3),
                E("jackpot", Casino, "Jackpot", "7-7-7 am Spielautomaten", casinoFolder, "^jackpot$", 90, 4, 100, 0, "sfx_jackpot"),
                E("jackpot_alarm", Casino, "Jackpot – Alarm", "Läuft zusammen mit dem Münzregen", casinoFolder, "^JackpotAlarm$", 60, 3),
                E("coins_small", Casino, "Münzen – wenige", "Kleiner Gewinn", casinoFolder, "^3_Coins$", 65, 3),
                E("coins_mid", Casino, "Münzen – mittel", "Mittlerer Gewinn", casinoFolder, "^4_Coins$", 65, 3),
                E("coins_big", Casino, "Münzen – viele", "Großer Gewinn", casinoFolder, "^5_Coins$", 70, 3),
                E("win", Casino, "Sieg", "Gewinn bei Roulette, Blackjack und Spielhalle", casinoFolder, "^GeneralVictory$", 80, 3, 100, 0, "sfx_win"),
                E("poker_win", Casino, "Poker – Sieg", "Du gewinnst eine Pokerhand", casinoFolder, "^PokerWinSound$", 80, 3),
                E("shuffle", Casino, "Karten mischen", "Blackjack und Poker beim Austeilen", casinoFolder, "^shuffle_riffle", 70),
                E("chip", Casino, "Chips setzen", "Roulette beim Setzen", casinoFolder, "^ChipUp$", 70),
                E("bet", Casino, "Einsatz ändern", "Einsatz erhöhen oder senken", casinoFolder, "^ui_bet_up", 55, 2, 100, 80),
                E("lose", Casino, "Verlust", "Verlorene Runde (eingebaute Melodie)", null, null, 60, 2, 100, 0, "sfx_lose"),

                // ---- Allgemein
                E("coin", General, "Kauf bezahlt", "Einkauf im Laden und am Automaten", generalFolder, "^purchase_coin", 70, 2, 100, 150, "sfx_coin"),
                E("buff", General, "Effekt erhalten", "Essen, Trinken und andere Gegenstände benutzen", generalFolder, "^EffectGainTypeAny$", 60),
                E("furniture_open", General, "Einrichtungsplan", "Beim Öffnen des Einrichtungsplans im Zuhause", generalFolder, "^OpenFurnitureDesk$", 70),
                E("trash_open", General, "Mülleimer", "Beim Durchsuchen eines Mülleimers", generalFolder, "^OpenTrashCan$", 70),
                E("door", General, "Tür", "Gebäude betreten oder verlassen (eingebaute Melodie)", null, null, 60, 2, 100, 0, "sfx_door"),
                E("click", General, "Klick", "Menü-Klick (eingebaute Melodie)", null, null, 50, 2, 100, 80, "sfx_click"),
                E("splash", General, "Platschen", "Angeln: Fisch springt (eingebaute Melodie)", null, null, 55, 2, 100, 250, "sfx_splash"),
                E("catch", General, "Fang", "Angeln: Fisch gefangen (eingebaute Melodie)", null, null, 70, 2, 100, 0, "sfx_catch"),
            };
        }
    }

    // =====================================================
    // Klasse: G1C_SoundFiles
    // Entwickler: Patrick Lupierz
    // Erstellt: 19.11.2026
    // Beschreibung: Sucht die Audiodateien (wav, mp3) zu einem Sound. Suchorte in dieser Reihenfolge:
    //               1. Umgebungsvariable G1CASINO_SOUNDS, 2. G1Casino_Assets\Sounds neben der OOPGames.exe,
    //               3. Classes\Gruppe 1\Spiel 2\Assets\Sounds im Quellordner (von bin\Debug aus nach oben gesucht –
    //               so läuft das Spiel beim Entwickeln ohne Kopieren), 4. Roots (nur für Tests).
    //               OGG wird bewusst nicht gesucht: WPF kann es ohne Zusatzbibliothek nicht abspielen.
    // =====================================================
    public static class G1C_SoundFiles
    {
        public static readonly string[] Extensions = { ".wav", ".mp3" };

        /// <summary>Zusätzliche/ersetzende Suchorte (Tests). Wenn gesetzt, werden nur diese benutzt.</summary>
        public static string[] OverrideRoots { get; set; }

        private static readonly Dictionary<string, string[]> Cache = new Dictionary<string, string[]>();

        public static IList<string> Roots()
        {
            if (OverrideRoots != null) return OverrideRoots;
            List<string> roots = new List<string>();
            try
            {
                string env = Environment.GetEnvironmentVariable("G1CASINO_SOUNDS");
                if (!string.IsNullOrEmpty(env) && Directory.Exists(env)) roots.Add(env);
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string next = Path.Combine(baseDir, "G1Casino_Assets", "Sounds");
                if (Directory.Exists(next)) roots.Add(next);
                DirectoryInfo dir = new DirectoryInfo(baseDir);
                for (int i = 0; i < 8 && dir != null; i++, dir = dir.Parent)
                {
                    string source = Path.Combine(dir.FullName, "Classes", "Gruppe 1", "Spiel 2", "Assets", "Sounds");
                    if (Directory.Exists(source)) { roots.Add(source); break; }
                }
            }
            catch (Exception) { }
            return roots;
        }

        public static void ClearCache()
        {
            lock (Cache) { Cache.Clear(); }
        }

        // Entwickler: Patrick Lupierz – alle Dateien eines Sounds, nach Namen sortiert (Varianten)
        public static string[] For(G1C_SoundDef def)
        {
            if (def == null || def.Folder == null || def.Patterns.Length == 0) return new string[0];
            lock (Cache)
            {
                string[] cached;
                string key = def.Id + "|" + string.Join(";", Roots());
                if (Cache.TryGetValue(key, out cached)) return cached;
                List<string> found = new List<string>();
                try
                {
                    foreach (string root in Roots())
                    {
                        string dir = Path.Combine(root, def.Folder.Replace('/', Path.DirectorySeparatorChar));
                        if (!Directory.Exists(dir)) continue;
                        foreach (string file in Directory.GetFiles(dir).OrderBy(f => f, StringComparer.OrdinalIgnoreCase))
                        {
                            if (!Extensions.Contains(Path.GetExtension(file).ToLowerInvariant())) continue;
                            string name = Path.GetFileNameWithoutExtension(file);
                            if (def.Patterns.Any(p => Regex.IsMatch(name, p, RegexOptions.IgnoreCase))) found.Add(file);
                        }
                        if (found.Count > 0) break;   // erster Suchort mit Treffern gewinnt
                    }
                }
                catch (Exception) { }
                cached = found.ToArray();
                Cache[key] = cached;
                return cached;
            }
        }

        /// <summary>Alle Audiodateien unterhalb eines Suchorts (für die Prüfung, ob jede Datei einem Sound gehört).</summary>
        public static IEnumerable<string> AllFiles(string root)
        {
            return Directory.GetFiles(root, "*.*", SearchOption.AllDirectories)
                            .Where(f => Extensions.Contains(Path.GetExtension(f).ToLowerInvariant()));
        }

        /// <summary>Gehört die Datei (relativ zu root) zu einem Sound des Katalogs?</summary>
        public static G1C_SoundDef Owner(string root, string file)
        {
            string folder = Path.GetDirectoryName(file.Substring(root.TrimEnd('\\', '/').Length).TrimStart('\\', '/')).Replace('\\', '/');
            string name = Path.GetFileNameWithoutExtension(file);
            return G1C_SoundCatalog.All.FirstOrDefault(d => d.Folder == folder && d.Patterns.Any(p => Regex.IsMatch(name, p, RegexOptions.IgnoreCase)));
        }
    }
}
