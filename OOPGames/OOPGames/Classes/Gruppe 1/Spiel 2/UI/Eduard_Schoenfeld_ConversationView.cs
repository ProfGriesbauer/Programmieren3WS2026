using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_ConversationView
    // Entwickler: Eduard Schoenfeld
    // Erstellt: 12.11.2026
    // Beschreibung: Zeichnet das Gesprächsfenster im vorhandenen Pixel-Art-Stil: Porträt mit
    //               Gesichtsausdruck, Name, Antwort, 2–4 Optionen, Vertrauens-/Respektleiste,
    //               Beziehungsstatus und kurze Hinweise. Enthält keine Gesprächslogik.
    // =====================================================
    public class G1C_ConversationView
    {
        private const double OptionX = 122;
        private const double OptionY = 238;
        private const double OptionHeight = 16;
        private const double OptionWidth = 348;
        private const int CharsPerLine = 60;

        private static readonly Color TrustColor = Color.FromRgb(0x38, 0xB0, 0x48);
        private static readonly Color RespectColor = Color.FromRgb(0x40, 0x70, 0xD8);
        private static readonly Color PortraitBack = Color.FromRgb(0x38, 0x48, 0x78);
        private static readonly Color SelectedRow = Color.FromRgb(0xD8, 0xE8, 0xF8);

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Welche Option liegt unter einem Klick (virtuelle Koordinaten)? -1 = keine.
        public static int OptionAt(double x, double y, int optionCount)
        {
            if (x < OptionX || x > OptionX + OptionWidth || y < OptionY) return -1;
            int index = (int)((y - OptionY) / OptionHeight);
            return index < optionCount ? index : -1;
        }

        // Entwickler: Eduard Schoenfeld
        public void Draw(G1C_Renderer r, G1C_NpcSystem npcs)
        {
            G1C_Conversation c = npcs.Conversation;
            if (!c.IsOpen || c.Npc == null) return;
            G1C_NpcCharacter npc = c.Npc;

            DrawStatusStrip(r, npc, npcs.Reputation);
            DrawNotices(r, c.Notices);

            r.DrawPanel(6, 182, 468, 134);

            // Porträt mit aktuellem Gesichtsausdruck
            r.DrawRect(14, 190, 100, 84, PortraitBack, G1C_UiColors.PanelBorder, 2);
            r.Sprite(G1C_NpcSprites.PortraitId(npc.Skin, c.Expression), 16, 194, 96, 78);
            r.Text(npc.Name, 10, 278, 10, G1C_UiColors.PanelBorder, 108);
            r.Text(npc.Personality.Role, 10, 292, 8, G1C_UiColors.Text, 108);

            List<string> lines = Wrap(c.Line, CharsPerLine);
            for (int i = 0; i < lines.Count && i < 3; i++)
            {
                r.Text(lines[i], OptionX, 190 + i * 14, 10, G1C_UiColors.Text);
            }

            for (int i = 0; i < c.Options.Count; i++)
            {
                double y = OptionY + i * OptionHeight;
                if (i == c.Selected) r.DrawRect(OptionX - 2, y, OptionWidth, OptionHeight - 1, SelectedRow, G1C_UiColors.PanelInner, 1);
                r.Text((i + 1) + "  " + c.Options[i].Text, OptionX + 2, y + 2, 10, G1C_UiColors.Text);
            }
            r.Text("[1-4] oder [W]/[S] + [E] wählen    [Q] Gespräch beenden", OptionX, 304, 8, G1C_UiColors.Disabled);
        }

        private static void DrawStatusStrip(G1C_Renderer r, G1C_NpcCharacter npc, int reputation)
        {
            r.DrawPanel(6, 148, 468, 32);
            DrawBar(r, 14, 154, "Vertrauen", npc.Relationship.Trust, TrustColor);
            DrawBar(r, 14, 166, "Respekt", npc.Relationship.Respect, RespectColor);
            r.Text("Beziehung: " + G1C_NpcRelationship.DisplayName(npc.Relationship.Status), 214, 153, 9, G1C_UiColors.PanelBorder);
            r.Text("Stimmung: " + G1C_NpcCharacter.MoodName(npc.Mood) + "   Ruf im Casino: " + reputation, 214, 165, 9, G1C_UiColors.Text);
        }

        // Pixel-Leiste 0–100 in 20 Segmenten
        private static void DrawBar(G1C_Renderer r, double x, double y, string label, int value, Color color)
        {
            r.Text(label, x, y - 2, 8, G1C_UiColors.Text);
            double barX = x + 58;
            r.DrawRect(barX, y, 102, 9, Color.FromRgb(0x50, 0x50, 0x58), G1C_UiColors.PanelBorder, 1);
            int segments = (int)Math.Round(value / 5.0);
            for (int i = 0; i < segments; i++) r.FillRect(barX + 1 + i * 5, y + 1, 4, 7, color);
            r.Text(value.ToString(), barX + 106, y - 2, 8, G1C_UiColors.Text);
        }

        private static void DrawNotices(G1C_Renderer r, IReadOnlyList<G1C_Notice> notices)
        {
            for (int i = 0; i < notices.Count; i++)
            {
                double y = 128 - i * 16;
                G1C_Notice n = notices[notices.Count - 1 - i]; // neuester unten
                r.DrawRect(352, y, 120, 15, G1C_UiColors.PanelFill, n.Positive ? G1C_UiColors.Win : G1C_UiColors.Loss, 1.5);
                r.Text(n.Text, 352, y + 2, 9, n.Positive ? G1C_UiColors.Win : G1C_UiColors.Loss, 120);
            }
        }

        // Einfacher Zeilenumbruch an Wortgrenzen; "\n" im Text wird übernommen
        private static List<string> Wrap(string text, int maxChars)
        {
            List<string> result = new List<string>();
            foreach (string paragraph in (text ?? "").Split('\n'))
            {
                string line = "";
                foreach (string word in paragraph.Split(' '))
                {
                    if (line.Length > 0 && line.Length + 1 + word.Length > maxChars)
                    {
                        result.Add(line);
                        line = word;
                    }
                    else
                    {
                        line = line.Length == 0 ? word : line + " " + word;
                    }
                }
                result.Add(line);
            }
            return result;
        }
    }
}
