using System;
using System.Windows.Media;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_GameClock
    // Entwickler: Patrick Lupierz
    // Erstellt: 12.11.2026
    // Beschreibung: Einzige Ingame-Uhr des Spiels. Sie zählt Framework-Ticks (40 ms):
    //               ein Ingame-Tag = 30 Echtzeitminuten = 45.000 Ticks, eine Stunde = 75 Sekunden.
    //               Die Uhr läuft nur, wenn der GameManager sie weiterschaltet (nicht im Hauptmenü,
    //               nicht im Pausenmenü). Liefert außerdem die Lichtfarbe für den Tag-Nacht-Wechsel.
    // =====================================================
    public class G1C_GameClock
    {
        public const int TicksPerDay = 45000;
        public const int TicksPerHour = TicksPerDay / 24;
        public const int StartTicks = 8 * TicksPerHour; // Tag 1, 08:00 Uhr

        // Stützpunkte des Lichts: Stunde, R, G, B, Deckkraft (0–255) der Farbfläche über der Außenwelt
        private static readonly double[][] Light =
        {
            new double[] { 0, 16, 24, 80, 140 },
            new double[] { 5, 16, 24, 80, 140 },
            new double[] { 7, 255, 190, 110, 50 },
            new double[] { 10, 255, 190, 110, 0 },
            new double[] { 17, 230, 120, 110, 0 },
            new double[] { 19.5, 220, 110, 130, 70 },
            new double[] { 22, 16, 24, 80, 140 },
            new double[] { 24, 16, 24, 80, 140 },
        };

        public int TotalTicks { get; set; } = StartTicks;

        public int Day { get { return TotalTicks / TicksPerDay + 1; } }
        public double HourExact { get { return TotalTicks % TicksPerDay / (double)TicksPerHour; } }
        public int Hour { get { return (int)HourExact; } }
        public int Minute { get { return (int)((HourExact - Hour) * 60); } }
        public string TimeText { get { return Hour.ToString("00") + ":" + Minute.ToString("00"); } }
        public bool IsNight { get { return Hour >= 22 || Hour < 6; } }

        public string PhaseName
        {
            get
            {
                if (Hour >= 6 && Hour < 10) return "Morgen";
                if (Hour >= 10 && Hour < 18) return "Tag";
                if (Hour >= 18 && Hour < 22) return "Abend";
                return "Nacht";
            }
        }

        public void Advance()
        {
            TotalTicks++;
        }

        // Entwickler: Patrick Lupierz – Schlafen: vorspulen bis zum nächsten Morgen (06:00)
        public void SkipToMorning()
        {
            int morning = 6 * TicksPerHour;
            int today = TotalTicks % TicksPerDay;
            TotalTicks += today < morning ? morning - today : TicksPerDay - today + morning;
        }

        // Entwickler: Patrick Lupierz
        // Aufgabe: Halbtransparente Lichtfarbe; Innenräume sind beleuchtet und dunkeln nur leicht ab.
        public Color Tint(bool outdoors)
        {
            double hour = HourExact;
            for (int i = 0; i + 1 < Light.Length; i++)
            {
                double[] a = Light[i];
                double[] b = Light[i + 1];
                if (hour < a[0] || hour > b[0]) continue;
                double t = (hour - a[0]) / (b[0] - a[0]);
                double alpha = (a[4] + (b[4] - a[4]) * t) * (outdoors ? 1.0 : 0.25);
                return Color.FromArgb((byte)alpha, (byte)(a[1] + (b[1] - a[1]) * t),
                                      (byte)(a[2] + (b[2] - a[2]) * t), (byte)(a[3] + (b[3] - a[3]) * t));
            }
            return Color.FromArgb(0, 0, 0, 0);
        }
    }
}
