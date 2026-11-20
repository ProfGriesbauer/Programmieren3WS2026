using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_SoundEntrySettings
    // Entwickler: Patrick Lupierz
    // Erstellt: 19.11.2026
    // Beschreibung: Einstellungen eines einzelnen Sounds: Lautstärke, Häufigkeit (Wahrscheinlichkeit pro Auslösung und
    //               Mindestabstand zwischen zwei Wiedergaben) und ein/aus. Wird im Handy-Mixer verändert.
    // =====================================================
    public sealed class G1C_SoundEntrySettings
    {
        public int Volume { get; set; } = 100;       // 0..100 %
        public int Chance { get; set; } = 100;       // 0..100 % – so oft wird ein Auslöser tatsächlich hörbar
        public int CooldownMs { get; set; }          // 0..60000 – Mindestabstand zwischen zwei Wiedergaben
        public bool Enabled { get; set; } = true;
    }

    // =====================================================
    // Klasse: G1C_SoundSettings
    // Entwickler: Patrick Lupierz
    // Erstellt: 19.11.2026
    // Beschreibung: Alle Klangeinstellungen des Spiels: Gesamt-, Musik- und Effektlautstärke, wie viele Effekte höchstens
    //               gleichzeitig hörbar sind, Vogel-Radius (Kacheln) und je Sound ein G1C_SoundEntrySettings.
    //               Änderungen kommen vom Handy-Mixer (anderer Thread) – deshalb alle Zugriffe über ein Schloss.
    //               Gespeichert wird als JSON (System.Text.Json, Teil von .NET) in %LocalAppData%\G1Casino\.
    // =====================================================
    public sealed class G1C_SoundSettings
    {
        public const string GlobalId = "_global";

        private readonly object _lock = new object();

        public int Master { get; set; } = 80;
        public int Music { get; set; } = 50;
        public int Effects { get; set; } = 80;
        public int MaxSimultaneous { get; set; } = 3;      // 1..8 Effekte gleichzeitig (Musik zählt nicht)
        public int BirdRadius { get; set; } = 4;           // Kacheln, in denen Vögel zu hören sind
        public Dictionary<string, G1C_SoundEntrySettings> Sounds { get; set; } = new Dictionary<string, G1C_SoundEntrySettings>();

        /// <summary>Zählt jede Änderung hoch (zum Speichern und zum Aktualisieren laufender Töne).</summary>
        public int Revision { get; private set; }

        // Entwickler: Patrick Lupierz – Einstellung eines Sounds (mit den Standardwerten des Katalogs angelegt)
        public G1C_SoundEntrySettings Get(string id)
        {
            lock (_lock)
            {
                G1C_SoundEntrySettings entry;
                if (id != null && Sounds.TryGetValue(id, out entry)) return entry;
                G1C_SoundDef def = G1C_SoundCatalog.Find(id);
                entry = def == null ? new G1C_SoundEntrySettings() : DefaultFor(def);
                if (id != null) Sounds[id] = entry;
                return entry;
            }
        }

        public static G1C_SoundEntrySettings DefaultFor(G1C_SoundDef def)
        {
            return new G1C_SoundEntrySettings { Volume = def.Volume, Chance = def.Chance, CooldownMs = def.CooldownMs, Enabled = true };
        }

        // Entwickler: Patrick Lupierz
        // Aufgabe: Ein Feld ändern. id = Sound-Id oder "_global"; field = volume | chance | cooldown | enabled bzw.
        //          master | music | effects | max | birdradius. Werte werden auf den erlaubten Bereich begrenzt.
        //          Gibt false zurück, wenn id oder Feld unbekannt sind.
        public bool Set(string id, string field, int value)
        {
            if (id == null || field == null) return false;
            lock (_lock)
            {
                if (id == GlobalId)
                {
                    switch (field.ToLowerInvariant())
                    {
                        case "master": Master = Clamp(value, 0, 100); break;
                        case "music": Music = Clamp(value, 0, 100); break;
                        case "effects": Effects = Clamp(value, 0, 100); break;
                        case "max": MaxSimultaneous = Clamp(value, 1, 8); break;
                        case "birdradius": BirdRadius = Clamp(value, 1, 12); break;
                        default: return false;
                    }
                }
                else
                {
                    if (G1C_SoundCatalog.Find(id) == null) return false;
                    G1C_SoundEntrySettings entry = Get(id);
                    switch (field.ToLowerInvariant())
                    {
                        case "volume": entry.Volume = Clamp(value, 0, 100); break;
                        case "chance": entry.Chance = Clamp(value, 0, 100); break;
                        case "cooldown": entry.CooldownMs = Clamp(value, 0, 60000); break;
                        case "enabled": entry.Enabled = value != 0; break;
                        default: return false;
                    }
                }
                Revision++;
                return true;
            }
        }

        public void ResetDefaults()
        {
            lock (_lock)
            {
                Master = 80; Music = 50; Effects = 80; MaxSimultaneous = 3; BirdRadius = 4;
                Sounds = new Dictionary<string, G1C_SoundEntrySettings>();
                Revision++;
            }
        }

        public static int Clamp(int value, int min, int max)
        {
            return value < min ? min : value > max ? max : value;
        }

        // Entwickler: Patrick Lupierz – Werte nach dem Laden prüfen: Bereiche, unbekannte Sounds entfernen, fehlende ergänzen
        public void Sanitize()
        {
            lock (_lock)
            {
                Master = Clamp(Master, 0, 100); Music = Clamp(Music, 0, 100); Effects = Clamp(Effects, 0, 100);
                MaxSimultaneous = Clamp(MaxSimultaneous, 1, 8); BirdRadius = Clamp(BirdRadius, 1, 12);
                Dictionary<string, G1C_SoundEntrySettings> clean = new Dictionary<string, G1C_SoundEntrySettings>();
                foreach (G1C_SoundDef def in G1C_SoundCatalog.All)
                {
                    G1C_SoundEntrySettings entry;
                    if (Sounds == null || !Sounds.TryGetValue(def.Id, out entry) || entry == null) entry = DefaultFor(def);
                    entry.Volume = Clamp(entry.Volume, 0, 100);
                    entry.Chance = Clamp(entry.Chance, 0, 100);
                    entry.CooldownMs = Clamp(entry.CooldownMs, 0, 60000);
                    clean[def.Id] = entry;
                }
                Sounds = clean;
            }
        }

        // ------------------------------------------------------------------ JSON

        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions { WriteIndented = true };

        public string ToJson()
        {
            lock (_lock) { return JsonSerializer.Serialize(this, Options); }
        }

        /// <summary>Lädt aus JSON; bei fehlerhaftem Text kommen die Standardwerte zurück.</summary>
        public static G1C_SoundSettings FromJson(string json)
        {
            G1C_SoundSettings settings = null;
            try { settings = JsonSerializer.Deserialize<G1C_SoundSettings>(json ?? ""); }
            catch (Exception) { }
            if (settings == null) settings = new G1C_SoundSettings();
            settings.Sanitize();
            return settings;
        }

        public static string DefaultPath
        {
            get
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "G1Casino", "sound_settings.json");
            }
        }

        // Entwickler: Patrick Lupierz – Speichern/Laden schlucken Dateifehler (Spiel läuft mit Standardwerten weiter)
        public bool Save(string path)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, ToJson());
                return true;
            }
            catch (Exception) { return false; }
        }

        /// <summary>Übernimmt die gespeicherten Werte in diese Instanz (false, wenn keine Datei vorhanden ist).</summary>
        public bool LoadFrom(string path)
        {
            try
            {
                if (!File.Exists(path)) return false;
                G1C_SoundSettings loaded = FromJson(File.ReadAllText(path));
                lock (_lock)
                {
                    Master = loaded.Master; Music = loaded.Music; Effects = loaded.Effects;
                    MaxSimultaneous = loaded.MaxSimultaneous; BirdRadius = loaded.BirdRadius; Sounds = loaded.Sounds;
                    Revision++;
                }
                return true;
            }
            catch (Exception) { return false; }
        }
    }
}
