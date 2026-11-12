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
            switch (stage)
            {
                case G1C_StartStage.Title: return new double[] { 160, 176, 160, 30 };
                case G1C_StartStage.Slots: return new double[] { 40, 120, 400, 40 };
                case G1C_StartStage.Look: return new double[] { 200, 112, 250, 30 };
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
            r.FillRect(0, 0, G1C_Renderer.ScreenWidth, G1C_Renderer.ScreenHeight, Sky);
            r.FillRect(0, 230, G1C_Renderer.ScreenWidth, 90, SkyLow);
            for (int i = 0; i < 7; i++) r.Sprite("chip", 30 + i * 66, 292 - (i % 2) * 4, 18, 18);

            switch (flow.Stage)
            {
                case G1C_StartStage.Title:
                    r.Text("CASINO ADVENTURE", 0, 60, 34, G1C_UiColors.Gold, G1C_Renderer.ScreenWidth);
                    r.Text("Gruppe 1 – Spiel 2", 0, 106, 12, G1C_UiColors.TextLight, G1C_Renderer.ScreenWidth);
                    DrawRows(r, flow, G1C_StartFlow.TitleOptions, 15);
                    Footer(r, "W/S wählen   E bestätigen   (oder anklicken)");
                    break;

                case G1C_StartStage.Slots:
                    r.Text("SPIEL LADEN", 0, 60, 22, G1C_UiColors.Gold, G1C_Renderer.ScreenWidth);
                    DrawRows(r, flow, flow.SlotLines, 10);
                    Footer(r, "W/S wählen   E laden   Q zurück");
                    break;

                case G1C_StartStage.Name:
                    r.Text("WIE HEISST DU?", 0, 60, 22, G1C_UiColors.Gold, G1C_Renderer.ScreenWidth);
                    r.DrawPanel(120, 130, 240, 44);
                    r.Text(flow.Profile.Name + "_", 120, 140, 20, G1C_UiColors.Text, 240);
                    Footer(r, "Buchstaben tippen   Rücktaste löscht   Pfeil ↓ oder Klick: Weiter");
                    break;

                case G1C_StartStage.Look:
                    r.Text("DEIN AUSSEHEN", 0, 50, 22, G1C_UiColors.Gold, G1C_Renderer.ScreenWidth);
                    r.DrawPanel(40, 100, 140, 136);
                    r.Sprite(flow.Profile.SkinId + "_down_0", 62, 112, 96, 96);
                    r.Text(flow.Profile.Name, 40, 212, 12, G1C_UiColors.Text, 140);
                    string[] rows = new string[G1C_StartFlow.LookRows.Length];
                    for (int i = 0; i < rows.Length; i++) rows[i] = G1C_StartFlow.LookRows[i] + ":  ◄ " + flow.LookValue(i) + " ►";
                    DrawRows(r, flow, rows, 11);
                    Footer(r, "W/S Zeile   A/D ändern   E Spiel starten   Q zurück");
                    break;
            }

            if (!string.IsNullOrEmpty(flow.Message))
                r.Text(flow.Message, 0, 252, 12, G1C_UiColors.Highlight, G1C_Renderer.ScreenWidth);
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
            r.Text(text, 0, 268, 10, G1C_UiColors.TextLight, G1C_Renderer.ScreenWidth);
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
        private const double Left = 70, Top = 44, Width = 340, RowsTop = 78, RowHeight = 17;
        private static readonly Color SelectedRow = Color.FromRgb(0xD8, 0xE8, 0xF8);

        public static int RowAt(double x, double y, G1C_ListMenu menu)
        {
            if (x < Left || x > Left + Width || y < RowsTop) return -1;
            int row = menu.FirstVisible + (int)((y - RowsTop) / RowHeight);
            return row < menu.Rows.Count && row < menu.FirstVisible + G1C_ListMenu.VisibleRows ? row : -1;
        }

        // Entwickler: Patrick Lupierz
        public void Draw(G1C_Renderer r, G1C_GameManager game)
        {
            G1C_ListMenu menu = game.ListMenu;
            r.DrawPanel(Left, Top, Width, 236);
            r.Text(menu.Title.ToUpperInvariant(), Left + 12, Top + 8, 14, G1C_UiColors.PanelBorder);
            r.Text("Chips: " + game.Money.Balance, Left + 210, Top + 10, 11, G1C_UiColors.Text, 118);

            if (menu.Rows.Count == 0) r.Text("(leer)", Left, RowsTop + 40, 11, G1C_UiColors.Disabled, Width);
            int first = menu.FirstVisible;
            for (int i = first; i < menu.Rows.Count && i < first + G1C_ListMenu.VisibleRows; i++)
            {
                double y = RowsTop + (i - first) * RowHeight;
                if (i == menu.Selected) r.FillRect(Left + 8, y, Width - 16, RowHeight - 1, SelectedRow);
                r.Text((i == menu.Selected ? "► " : "  ") + menu.Rows[i].Text, Left + 12, y + 2, 10, G1C_UiColors.Text);
                r.Text(menu.Rows[i].Right, Left + 220, y + 2, 10, G1C_UiColors.Text, 108);
            }
            if (menu.Rows.Count > first + G1C_ListMenu.VisibleRows) r.Text("▼", Left + Width - 22, RowsTop + 140, 9, G1C_UiColors.PanelBorder);

            if (menu.Selected < menu.Rows.Count)
                r.Text(menu.Rows[menu.Selected].Detail, Left + 8, Top + 192, 8.5, G1C_UiColors.Text, Width - 16);
            r.Text(menu.Message, Left + 8, Top + 205, 9.5, G1C_UiColors.Win, Width - 16);
            r.Text("W/S wählen   E auswählen   Q schließen", Left, Top + 220, 9, G1C_UiColors.PanelBorder, Width);
        }
    }
}
