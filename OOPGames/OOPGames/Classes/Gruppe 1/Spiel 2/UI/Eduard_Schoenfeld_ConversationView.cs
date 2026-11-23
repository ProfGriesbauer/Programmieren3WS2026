using System;
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
        private const double OptionHeight = 16;

        private static readonly Color TrustColor = Color.FromRgb(0x38, 0xB0, 0x48);
        private static readonly Color RespectColor = Color.FromRgb(0x40, 0x70, 0xD8);
        private static readonly Color PortraitBack = Color.FromRgb(0x38, 0x48, 0x78);
        private static readonly Color SelectedRow = Color.FromRgb(0xD8, 0xE8, 0xF8);

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Welche Option liegt unter einem Klick (virtuelle Koordinaten)? -1 = keine.
        public static int OptionAt(double x, double y, int optionCount)
        {
            double optionY = G1C_Renderer.ScreenHeight - 82;
            double optionWidth = G1C_Renderer.ScreenWidth - OptionX - 8;
            if (x < OptionX || x > OptionX + optionWidth || y < optionY) return -1;
            int index = (int)((y - optionY) / OptionHeight);
            return index < optionCount ? index : -1;
        }

        // Entwickler: Eduard Schoenfeld
        public void Draw(G1C_Renderer r, G1C_NpcSystem npcs)
        {
            G1C_Conversation c = npcs.Conversation;
            if (!c.IsOpen || c.Npc == null) return;
            G1C_NpcCharacter npc = c.Npc;
            double screenWidth = G1C_Renderer.ScreenWidth;
            double panelY = G1C_Renderer.ScreenHeight - 138;
            double optionY = panelY + 56;
            double optionWidth = screenWidth - OptionX - 8;

            DrawStatusStrip(r, npc, npcs.Reputation);
            DrawNotices(r, c.Notices, panelY - 54);

            r.DrawPanel(6, panelY, screenWidth - 12, 134);

            // Porträt mit aktuellem Gesichtsausdruck
            r.DrawRect(14, panelY + 8, 100, 84, PortraitBack, G1C_UiColors.PanelBorder, 2);
            r.Sprite(G1C_NpcSprites.PortraitId(npc.Skin, c.Expression), 16, panelY + 12, 96, 78);
            r.Text(npc.Name, 10, panelY + 96, 10, G1C_UiColors.PanelBorder, 108);
            r.Text(npc.Personality.Role, 10, panelY + 110, 8, G1C_UiColors.Text, 108);

            r.DrawRect(OptionX - 2, panelY + 6, optionWidth + 2, 48, G1C_UiColors.PanelFill,
                       G1C_UiColors.PanelInner, 1);
            r.WrappedText(c.Line, OptionX + 2, panelY + 8, 9, G1C_UiColors.Text, optionWidth - 8);

            if (c.FishScrollMode)
            {
                double listY = optionY;
                int visible = 4;
                int first = System.Math.Max(0, System.Math.Min(c.FishScrollIndex - visible / 2,
                    c.FishScroll.Count - visible));
                for (int i = first; i < c.FishScroll.Count && i < first + visible; i++)
                {
                    G1C_CatchInfo fish = c.FishScroll[i];
                    G1C_Item item = G1C_ItemCatalog.Get(fish.ItemId);
                    if (item == null) continue;
                    double y = listY + (i - first) * OptionHeight;
                    if (i == c.FishScrollIndex)
                        r.DrawRect(OptionX - 2, y, optionWidth, OptionHeight - 1, SelectedRow, G1C_UiColors.PanelInner, 1);
                    // Änderung durch: Patrick Lupierz – Datum: 23.11.2026 – kleine Fischgrafik links neben dem Namen
                    r.Sprite("item_" + item.Id, OptionX + 1, y, OptionHeight - 2, OptionHeight - 2);
                    r.Text(item.Name + "  " + item.SellValue + " Chips", OptionX + OptionHeight + 2, y + 2, 8.5,
                        G1C_UiColors.Text);
                }
                r.Text("[W/S] scrollen  [Q] zurück", OptionX, panelY + 122, 8, G1C_UiColors.Disabled);
                return;
            }

            for (int i = 0; i < c.Options.Count; i++)
            {
                double y = optionY + i * OptionHeight;
                if (i == c.Selected) r.DrawRect(OptionX - 2, y, optionWidth, OptionHeight - 1, SelectedRow, G1C_UiColors.PanelInner, 1);
                r.WrappedText((i + 1) + "  " + c.Options[i].Text, OptionX + 2, y + 2, 9,
                              G1C_UiColors.Text, optionWidth - 6);
            }
            r.Text("[1-4] oder [W]/[S] + [E] wählen    [Q] Gespräch beenden",
                   OptionX, panelY + 122, 8, G1C_UiColors.Disabled);
        }

        private static void DrawStatusStrip(G1C_Renderer r, G1C_NpcCharacter npc, int reputation)
        {
            double statusY = G1C_Renderer.ScreenHeight - 172;
            r.DrawPanel(6, statusY, G1C_Renderer.ScreenWidth - 12, 32);
            DrawBar(r, 14, statusY + 6, "Vertrauen", npc.Relationship.Trust, TrustColor);
            DrawBar(r, 14, statusY + 18, "Respekt", npc.Relationship.Respect, RespectColor);
            r.Text("Beziehung: " + G1C_NpcRelationship.DisplayName(npc.Relationship.Status),
                   214, statusY + 5, 9, G1C_UiColors.PanelBorder);
            r.Text("Stimmung: " + G1C_NpcCharacter.MoodName(npc.Mood) + "   Ruf im Casino: " + reputation,
                   214, statusY + 17, 9, G1C_UiColors.Text);
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

        private static void DrawNotices(G1C_Renderer r, System.Collections.Generic.IReadOnlyList<G1C_Notice> notices,
                                        double baseY)
        {
            for (int i = 0; i < notices.Count; i++)
            {
                double y = baseY - i * 16;
                G1C_Notice n = notices[notices.Count - 1 - i]; // neuester unten
                r.DrawRect(352, y, 120, 15, G1C_UiColors.PanelFill, n.Positive ? G1C_UiColors.Win : G1C_UiColors.Loss, 1.5);
                r.Text(n.Text, 352, y + 2, 9, n.Positive ? G1C_UiColors.Win : G1C_UiColors.Loss, 120);
            }
        }
    }
}
