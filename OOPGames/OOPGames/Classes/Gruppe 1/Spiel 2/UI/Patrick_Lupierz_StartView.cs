using System.Windows.Media;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_StartView
    // Entwickler: Patrick Lupierz
    // Erstellt: 12.11.2026
    // Beschreibung: Zeichnet Hauptmenü, Speicherplatz-Auswahl und Charaktererstellung auf das
    //               Framework-Canvas. Enthält nur Darstellung und die Zeilen-Geometrie für Mausklicks.
    // =====================================================
    public class G1C_StartView
    {
        private static readonly Color Sky = Color.FromRgb(0x28, 0x30, 0x58);
        private static readonly Color SkyLow = Color.FromRgb(0x40, 0x58, 0x90);
        private static readonly Color SelectedRow = Color.FromRgb(0xD8, 0xE8, 0xF8);

        // Zeilenraster je Schritt: x, y der ersten Zeile, Breite, Zeilenabstand
        private static double[] Grid(G1C_StartStage stage)
        {
            double xOffset = (G1C_Renderer.ScreenWidth - G1C_Renderer.BaseScreenWidth) / 2;
            double yOffset = (G1C_Renderer.ScreenHeight - G1C_Renderer.BaseScreenHeight) / 2;
            switch (stage)
            {
                case G1C_StartStage.Title: return new double[] { xOffset + 160, yOffset + 176, 160, 30 };
                case G1C_StartStage.Slots: return new double[] { xOffset + 40, yOffset + 120, 400, 40 };
                case G1C_StartStage.Look: return new double[] { xOffset + 200, yOffset + 112, 250, 30 };
                default: return null;
            }
        }

        /// <summary>Zeile unter dem Mauszeiger oder -1 (bei der Namenseingabe zählt jeder Klick als "Weiter").</summary>
        public static int RowAt(double x, double y, G1C_StartFlow flow)
        {
            double[] g = Grid(flow.Stage);
            if (g == null || x < g[0] || x > g[0] + g[2] || y < g[1]) return -1;
            int row = (int)((y - g[1]) / g[3]);
            return row < flow.RowCount ? row : -1;
        }

        // Entwickler: Patrick Lupierz
        public void Draw(G1C_Renderer r, G1C_StartFlow flow)
        {
            double xOffset = (G1C_Renderer.ScreenWidth - G1C_Renderer.BaseScreenWidth) / 2;
            double yOffset = (G1C_Renderer.ScreenHeight - G1C_Renderer.BaseScreenHeight) / 2;
            r.FillRect(0, 0, G1C_Renderer.ScreenWidth, G1C_Renderer.ScreenHeight, Sky);
            r.FillRect(0, 230 + yOffset, G1C_Renderer.ScreenWidth, G1C_Renderer.ScreenHeight - 230 - yOffset, SkyLow);
            for (int i = 0; i < 7; i++) r.Sprite("chip", xOffset + 30 + i * 66, 292 + yOffset - (i % 2) * 4, 18, 18);

            switch (flow.Stage)
            {
                case G1C_StartStage.Title:
                    r.Sprite("building_home", xOffset + 36, 132 + yOffset, 96, 80);
                    r.Sprite("building_casino", xOffset + 348, 132 + yOffset, 96, 80);
                    r.Text("CASINO ADVENTURE", 0, 60 + yOffset, 34, G1C_UiColors.Gold, G1C_Renderer.ScreenWidth);
                    r.Text("Gruppe 1 – Spiel 2", 0, 106 + yOffset, 12, G1C_UiColors.TextLight, G1C_Renderer.ScreenWidth);
                    r.Text("Erkunde die Stadt, triff NPCs und spiele Minispiele", 0, 142 + yOffset, 9,
                           G1C_UiColors.TextLight, G1C_Renderer.ScreenWidth);
                    DrawRows(r, flow, G1C_StartFlow.TitleOptions, 15);
                    Footer(r, "W/S wählen   E bestätigen   (oder anklicken)");
                    break;

                case G1C_StartStage.Slots:
                    r.Text("SPIEL LADEN", 0, 60 + yOffset, 22, G1C_UiColors.Gold, G1C_Renderer.ScreenWidth);
                    DrawRows(r, flow, flow.SlotLines, 10);
                    Footer(r, "W/S wählen   E laden   Q zurück");
                    break;

                case G1C_StartStage.Name:
                    r.Text("WIE HEISST DU?", 0, 60 + yOffset, 22, G1C_UiColors.Gold, G1C_Renderer.ScreenWidth);
                    r.DrawPanel(xOffset + 120, 130 + yOffset, 240, 44);
                    r.Text(flow.Profile.Name + "_", xOffset + 120, 140 + yOffset, 20, G1C_UiColors.Text, 240);
                    Footer(r, "Buchstaben tippen   Rücktaste löscht   Pfeil ↓ oder Klick: Weiter");
                    break;

                case G1C_StartStage.Look:
                    r.Text("DEIN AUSSEHEN", 0, 50 + yOffset, 22, G1C_UiColors.Gold, G1C_Renderer.ScreenWidth);
                    r.DrawPanel(xOffset + 40, 100 + yOffset, 140, 136);
                    r.Sprite(flow.Profile.SkinId + "_down_0", xOffset + 62, 112 + yOffset, 96, 96);
                    r.Text(flow.Profile.Name, xOffset + 40, 212 + yOffset, 12, G1C_UiColors.Text, 140);
                    string[] rows = new string[G1C_StartFlow.LookRows.Length];
                    for (int i = 0; i < rows.Length; i++) rows[i] = G1C_StartFlow.LookRows[i] + ":  ◄ " + flow.LookValue(i) + " ►";
                    DrawRows(r, flow, rows, 11);
                    Footer(r, "W/S Zeile   A/D ändern   E Spiel starten   Q zurück");
                    break;
            }

            if (!string.IsNullOrEmpty(flow.Message))
                r.Text(flow.Message, 0, 252 + yOffset, 12, G1C_UiColors.Highlight, G1C_Renderer.ScreenWidth);
        }

        private static void DrawRows(G1C_Renderer r, G1C_StartFlow flow, string[] rows, double fontSize)
        {
            double[] g = Grid(flow.Stage);
            for (int i = 0; i < rows.Length; i++)
            {
                bool selected = i == flow.Selected;
                r.DrawRect(g[0], g[1] + i * g[3], g[2], g[3] - 6, selected ? SelectedRow : G1C_UiColors.PanelFill,
                           selected ? G1C_UiColors.Gold : G1C_UiColors.PanelBorder, 2);
                r.Text(rows[i], g[0], g[1] + i * g[3] + (g[3] - 6 - fontSize * 1.3) / 2, fontSize, G1C_UiColors.Text, g[2]);
            }
        }

        private static void Footer(G1C_Renderer r, string text)
        {
            double yOffset = (G1C_Renderer.ScreenHeight - G1C_Renderer.BaseScreenHeight) / 2;
            r.Text(text, 0, 268 + yOffset, 10, G1C_UiColors.TextLight, G1C_Renderer.ScreenWidth);
        }
    }

    // =====================================================
    // Klasse: G1C_ListMenuView
    // Entwickler: Patrick Lupierz
    // Erstellt: 12.11.2026
    // Beschreibung: Zeichnet das allgemeine Listenmenü (Laden, Inventar, Kleiderschrank).
    // =====================================================
    public class G1C_ListMenuView
    {
        private const double Width = 340, Height = 236, RowHeight = 17;
        private static readonly Color SelectedRow = Color.FromRgb(0xD8, 0xE8, 0xF8);

        public static int RowAt(double x, double y, G1C_ListMenu menu)
        {
            double left = (G1C_Renderer.ScreenWidth - Width) / 2;
            double top = (G1C_Renderer.ScreenHeight - Height) / 2;
            double rowsTop = top + 34;
            if (x < left || x > left + Width || y < rowsTop) return -1;
            int row = menu.FirstVisible + (int)((y - rowsTop) / RowHeight);
            return row < menu.Rows.Count && row < menu.FirstVisible + G1C_ListMenu.VisibleRows ? row : -1;
        }

        // Entwickler: Patrick Lupierz
        public void Draw(G1C_Renderer r, G1C_GameManager game)
        {
            G1C_ListMenu menu = game.ListMenu;
            double left = (G1C_Renderer.ScreenWidth - Width) / 2;
            double top = (G1C_Renderer.ScreenHeight - Height) / 2;
            double rowsTop = top + 34;
            r.DrawPanel(left, top, Width, Height);
            r.Text(menu.Title.ToUpperInvariant(), left + 12, top + 8, 14, G1C_UiColors.PanelBorder);
            r.Text("Chips: " + game.Money.Balance, left + 210, top + 10, 11, G1C_UiColors.Text, 118);

            if (menu.Rows.Count == 0) r.Text("(leer)", left, rowsTop + 40, 11, G1C_UiColors.Disabled, Width);
            int first = menu.FirstVisible;
            for (int i = first; i < menu.Rows.Count && i < first + G1C_ListMenu.VisibleRows; i++)
            {
                double y = rowsTop + (i - first) * RowHeight;
                if (i == menu.Selected) r.FillRect(left + 8, y, Width - 16, RowHeight - 1, SelectedRow);
                double textX = left + 12;
                if (!string.IsNullOrEmpty(menu.Rows[i].SpriteId))
                {
                    r.Sprite(menu.Rows[i].SpriteId, left + 12, y + 1, 14, 14);
                    textX = left + 30;
                }
                r.Text((i == menu.Selected ? "► " : "  ") + menu.Rows[i].Text, textX, y + 2, 10, G1C_UiColors.Text);
                r.Text(menu.Rows[i].Right, left + 220, y + 2, 10, G1C_UiColors.Text, 108);
            }
            if (menu.Rows.Count > first + G1C_ListMenu.VisibleRows) r.Text("▼", left + Width - 22, rowsTop + 140, 9, G1C_UiColors.PanelBorder);

            if (menu.Selected < menu.Rows.Count)
                r.Text(menu.Rows[menu.Selected].Detail, left + 8, top + 192, 8.5, G1C_UiColors.Text, Width - 16);
            r.Text(menu.Message, left + 8, top + 205, 9.5, G1C_UiColors.Win, Width - 16);
            r.Text("W/S wählen   E auswählen   Q schließen", left, top + 220, 9, G1C_UiColors.PanelBorder, Width);
        }
    }
}
