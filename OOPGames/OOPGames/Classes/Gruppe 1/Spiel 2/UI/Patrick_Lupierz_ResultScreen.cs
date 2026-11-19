namespace OOPGames
{
    /// <summary>Zeigt nach dem Verlassen eines Minispiels Gewinn bzw. Verlust an (Zustand "Result").</summary>
    public class G1C_ResultScreen
    {
        public string GameName { get; private set; } = "";
        public int StartBalance { get; private set; }
        public int EndBalance { get; private set; }
        public int Difference { get { return EndBalance - StartBalance; } }

        public void Show(string gameName, int startBalance, int endBalance)
        {
            GameName = gameName;
            StartBalance = startBalance;
            EndBalance = endBalance;
        }

        public void Draw(G1C_Renderer r)
        {
            double x = (G1C_Renderer.ScreenWidth - 300) / 2;
            double y = (G1C_Renderer.ScreenHeight - 150) / 2;
            r.DrawPanel(x, y, 300, 150);
            r.Text("Ergebnis: " + GameName, x, y + 14, 15, G1C_UiColors.PanelBorder, 300);
            r.Text("Vorher:  " + StartBalance + " Chips", x + 30, y + 46, 12, G1C_UiColors.Text);
            r.Text("Nachher: " + EndBalance + " Chips", x + 30, y + 64, 12, G1C_UiColors.Text);

            string diff = Difference > 0 ? "Gewinn: +" + Difference : (Difference < 0 ? "Verlust: " + Difference : "Ausgeglichen: 0");
            r.Text(diff, x, y + 90, 16, Difference >= 0 ? G1C_UiColors.Win : G1C_UiColors.Loss, 300);
            r.Text("[E] Zurück ins Casino", x, y + 124, 11, G1C_UiColors.PanelBorder, 300);
        }
    }
}
