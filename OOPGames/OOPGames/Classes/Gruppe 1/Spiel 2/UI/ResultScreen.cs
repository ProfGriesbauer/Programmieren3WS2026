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
            r.DrawPanel(90, 80, 300, 150);
            r.Text("Ergebnis: " + GameName, 90, 94, 15, G1C_UiColors.PanelBorder, 300);
            r.Text("Vorher:  " + StartBalance + " Chips", 120, 126, 12, G1C_UiColors.Text);
            r.Text("Nachher: " + EndBalance + " Chips", 120, 144, 12, G1C_UiColors.Text);

            string diff = Difference > 0 ? "Gewinn: +" + Difference : (Difference < 0 ? "Verlust: " + Difference : "Ausgeglichen: 0");
            r.Text(diff, 90, 170, 16, Difference >= 0 ? G1C_UiColors.Win : G1C_UiColors.Loss, 300);
            r.Text("[E] Zurück ins Casino", 90, 204, 11, G1C_UiColors.PanelBorder, 300);
        }
    }
}
