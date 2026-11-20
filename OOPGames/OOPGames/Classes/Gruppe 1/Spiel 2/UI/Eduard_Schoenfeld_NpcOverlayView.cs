using System;
using System.Collections.Generic;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_NpcOverlayView
    // Entwickler: Eduard Schoenfeld
    // Erstellt: 12.11.2026
    // Beschreibung: Zeichnet über der Casino-Karte die Aufgaben-Gegenstände, die Sprechblasen
    //               der NPCs und eine kleine Aufgabenanzeige. Enthält keine Spiellogik.
    // =====================================================
    public class G1C_NpcOverlayView
    {
        private const double BubbleHeight = 14;
        private const double BubbleFont = 7.5;
        private const double CharWidth = 4.2; // Consolas: ca. 0,55 × Schriftgröße

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Gegenstände liegen auf dem Boden, deshalb vor den Figuren zeichnen.
        public void DrawItems(G1C_Renderer r, G1C_NpcSystem npcs, double mapX, double mapY, double tileSize)
        {
            foreach (G1C_CollectTask task in npcs.Tasks.Tasks)
            {
                foreach (KeyValuePair<int, int> item in task.OpenItems())
                {
                    double size = tileSize * 0.7;
                    double offset = (tileSize - size) / 2;
                    r.Sprite(task.ItemSpriteId, mapX + item.Key * tileSize + offset, mapY + item.Value * tileSize + offset, size, size);
                }
            }
        }

        // Entwickler: Eduard Schoenfeld
        // Aufgabe: Sprechblasen direkt über den Köpfen; am Bildschirmrand nach innen geschoben.
        public void DrawBubbles(G1C_Renderer r, G1C_NpcSystem npcs, double mapX, double mapY, double tileSize)
        {
            foreach (G1C_SpeechBubble bubble in npcs.Bubbles.Active)
            {
                double width = bubble.Text.Length * CharWidth + 10;
                double centerX = mapX + (bubble.Npc.X + 0.5) * tileSize;
                double top = mapY + bubble.Npc.Y * tileSize - BubbleHeight - 6;
                double x = Math.Max(2, Math.Min(G1C_Renderer.ScreenWidth - width - 2, centerX - width / 2));

                r.DrawRect(x, top, width, BubbleHeight, G1C_UiColors.PanelFill,
                           bubble.IsReaction ? G1C_UiColors.Gold : G1C_UiColors.PanelBorder, 1.5);
                r.FillRect(centerX - 2, top + BubbleHeight, 4, 3, G1C_UiColors.PanelBorder); // Sprechblasen-Spitze
                r.Text(bubble.Text, x, top + 2, BubbleFont, G1C_UiColors.Text, width);
            }
        }

        // Entwickler: Eduard Schoenfeld
        public void DrawTaskStatus(G1C_Renderer r, G1C_NpcSystem npcs)
        {
            double y = G1C_GameHUD.Height + 2;
            foreach (G1C_CollectTask task in npcs.Tasks.Tasks)
            {
                if (task.State != G1C_TaskState.Active) continue;
                string text = task.AllCollected
                    ? "Aufgabe: zurück zu " + task.GiverName
                    : "Aufgabe: " + task.Title + " " + task.CollectedCount + "/" + task.Total;
                double width = text.Length * 4.8 + 12;
                r.DrawRect(4, y, width, 14, G1C_UiColors.PanelFill, G1C_UiColors.Gold, 1.5);
                r.Text(text, 4, y + 2, 8.5, G1C_UiColors.Text, width);
                y += 16;
            }
        }
    }
}
