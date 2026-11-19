using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_PhoneView
    // Entwickler: Patrick Lupierz
    // Erstellt: 12.11.2026
    // Beschreibung: Zeichnet das Klapphandy im Stil der 2000er (oberer Bildschirm, Scharnier,
    //               Tastenfeld) mit kurzer Aufklapp-Animation. Der Inhalt hängt vom Bildschirm
    //               in G1C_Phone ab. Reine Darstellung auf dem Framework-Canvas.
    // =====================================================
    public class G1C_PhoneView
    {
        private const double X = 168, HingeY = 176, Width = 144, HalfHeight = 132;
        private const double ScreenX = X + 10, ScreenY = HingeY - HalfHeight + 12, ScreenW = Width - 20, ScreenH = 108;
        private const double Font = 8, Line = 11;
        private const int OpenTicks = 6;
        private const int Chars = 25; // Zeichen pro Bildschirmzeile

        private static readonly Color Body = Color.FromRgb(0x48, 0x50, 0x68);
        private static readonly Color BodyLight = Color.FromRgb(0x70, 0x78, 0x98);
        private static readonly Color BodyDark = Color.FromRgb(0x28, 0x2C, 0x40);
        private static readonly Color Lcd = Color.FromRgb(0xC0, 0xDC, 0xA8);
        private static readonly Color LcdDark = Color.FromRgb(0x28, 0x40, 0x30);
        private static readonly Color LcdSelect = Color.FromRgb(0x90, 0xB8, 0x80);

        // Entwickler: Patrick Lupierz
        public void Draw(G1C_Renderer r, G1C_GameManager game)
        {
            G1C_Phone phone = game.Phone;
            double open = Math.Min(1.0, (game.TickCount - phone.OpenedTick + 1) / (double)OpenTicks);

            // Unterteil mit Tastenfeld
            r.DrawRect(X, HingeY, Width, HalfHeight, Body, BodyDark, 2);
            for (int row = 0; row < 4; row++)
                for (int column = 0; column < 3; column++)
                    r.DrawRect(X + 16 + column * 40, HingeY + 34 + row * 22, 32, 16, BodyLight, BodyDark, 1);
            r.DrawRect(X + 52, HingeY + 8, 40, 20, BodyLight, BodyDark, 1);
            r.Text("OK", X + 52, HingeY + 12, 9, G1C_UiColors.TextLight, 40);
            string[] keys = { "1", "2", "3", "4", "5", "6", "7", "8", "9", "*", "0", "#" };
            for (int i = 0; i < keys.Length; i++)
                r.Text(keys[i], X + 16 + i % 3 * 40, HingeY + 36 + i / 3 * 22, 8, G1C_UiColors.TextLight, 32);

            // Oberteil klappt nach oben auf
            double topHeight = HalfHeight * open;
            r.DrawRect(X, HingeY - topHeight, Width, topHeight, Body, BodyDark, 2);
            r.FillRect(X, HingeY - 3, Width, 6, BodyDark); // Scharnier
            if (open < 1.0) return;

            r.DrawRect(ScreenX, ScreenY, ScreenW, ScreenH, Lcd, BodyDark, 2);
            r.Text(game.Clock.TimeText, ScreenX + 4, ScreenY + 2, Font, LcdDark);
            r.Text(TitleOf(phone.Screen), ScreenX, ScreenY + 2, Font, LcdDark, ScreenW);
            if (phone.Unread > 0) r.Text("✉" + phone.Unread, ScreenX + ScreenW - 22, ScreenY + 2, Font, LcdDark);
            r.FillRect(ScreenX + 2, ScreenY + 13, ScreenW - 4, 1, LcdDark);

            double y = ScreenY + 16;
            switch (phone.Screen)
            {
                case G1C_PhoneScreen.Home: DrawRows(r, G1C_Phone.Apps, phone.Selected, y, 1); break;
                case G1C_PhoneScreen.Contacts: DrawContacts(r, game, y); break;
                case G1C_PhoneScreen.Messages: DrawMessages(r, phone, y); break;
                case G1C_PhoneScreen.MessageDetail: DrawMessage(r, phone, y); break;
                case G1C_PhoneScreen.Clock: DrawClock(r, game, y); break;
                case G1C_PhoneScreen.Tasks: DrawTasks(r, game, y); break;
                case G1C_PhoneScreen.Save: DrawRows(r, phone.SlotLines, phone.Selected, y, 3); break;
            }

            if (!string.IsNullOrEmpty(phone.Info))
            {
                List<string> info = Wrap(phone.Info);
                r.FillRect(ScreenX + 2, ScreenY + ScreenH - 2 - info.Count * Line, ScreenW - 4, info.Count * Line, LcdDark);
                for (int i = 0; i < info.Count; i++)
                    r.Text(info[i], ScreenX + 4, ScreenY + ScreenH - 2 - (info.Count - i) * Line + 1, Font, Lcd);
            }
            r.Text("W/S  E: OK  Q: zurück  P: zu", X, HingeY - HalfHeight + 2, 6.5, G1C_UiColors.TextLight, Width);
        }

        private static string TitleOf(G1C_PhoneScreen screen)
        {
            switch (screen)
            {
                case G1C_PhoneScreen.Contacts: return "Kontakte";
                case G1C_PhoneScreen.Messages: case G1C_PhoneScreen.MessageDetail: return "SMS";
                case G1C_PhoneScreen.Clock: return "Uhr";
                case G1C_PhoneScreen.Tasks: return "Aufgaben";
                case G1C_PhoneScreen.Save: return "Speichern";
                default: return "Menü";
            }
        }

        // Zeilen mit Auswahlbalken; linesPerRow > 1 bricht lange Einträge um
        private static void DrawRows(G1C_Renderer r, string[] rows, int selected, double y, int linesPerRow)
        {
            double rowHeight = linesPerRow * Line + 2;
            int visible = (int)((ScreenH - 18) / rowHeight);
            int first = Math.Max(0, Math.Min(selected - visible / 2, rows.Length - visible));
            for (int i = first; i < rows.Length && i < first + visible; i++)
            {
                double rowY = y + (i - first) * rowHeight;
                if (i == selected) r.FillRect(ScreenX + 2, rowY, ScreenW - 4, rowHeight - 1, LcdSelect);
                List<string> lines = Wrap(rows[i]);
                for (int l = 0; l < lines.Count && l < linesPerRow; l++)
                    r.Text(lines[l], ScreenX + 5, rowY + 1 + l * Line, Font, LcdDark);
            }
        }

        // Kontakte: Porträt des NPCs, Name, Nummer und Beziehungsstatus
        private static void DrawContacts(G1C_Renderer r, G1C_GameManager game, double y)
        {
            List<G1C_NpcCharacter> contacts = game.Phone.Contacts;
            if (contacts.Count == 0)
            {
                Paragraph(r, "Noch keine Nummern. Freunde geben dir ihre Nummer.", y);
                return;
            }
            const double rowHeight = 29;
            int first = Math.Max(0, Math.Min(game.Phone.Selected - 1, contacts.Count - 3));
            for (int i = first; i < contacts.Count && i < first + 3; i++)
            {
                G1C_NpcCharacter c = contacts[i];
                double rowY = y + (i - first) * rowHeight;
                if (i == game.Phone.Selected) r.FillRect(ScreenX + 2, rowY, ScreenW - 4, rowHeight - 1, LcdSelect);
                r.Sprite(G1C_NpcSprites.PortraitId(c.Skin, c.MoodExpression), ScreenX + 4, rowY + 2, 30, 24.4);
                r.Text(c.Name, ScreenX + 38, rowY + 1, Font, LcdDark);
                r.Text(game.Phone.NumberOf(c), ScreenX + 38, rowY + 10, 7, LcdDark);
                r.Text(G1C_NpcRelationship.DisplayName(c.Relationship.Status), ScreenX + 38, rowY + 19, 7, LcdDark);
            }
        }

        private static void DrawMessages(G1C_Renderer r, G1C_Phone phone, double y)
        {
            if (phone.Messages.Count == 0) { Paragraph(r, "Keine Nachrichten.", y); return; }
            string[] rows = new string[phone.Messages.Count];
            for (int i = 0; i < rows.Length; i++)
                rows[i] = (phone.Messages[i].Read ? "  " : "● ") + phone.Messages[i].From + " – " + phone.Messages[i].Stamp;
            DrawRows(r, rows, phone.Selected, y, 2);
        }

        private static void DrawMessage(G1C_Renderer r, G1C_Phone phone, double y)
        {
            if (phone.Selected >= phone.Messages.Count) return;
            G1C_Sms sms = phone.Messages[phone.Selected];
            Paragraph(r, sms.From + " (" + sms.Stamp + "): " + sms.Text, y);
        }

        private static void DrawClock(G1C_Renderer r, G1C_GameManager game, double y)
        {
            r.Text(game.Clock.TimeText, ScreenX, y + 6, 24, LcdDark, ScreenW);
            r.Text("Tag " + game.Clock.Day + " – " + game.Clock.PhaseName, ScreenX, y + 40, Font + 1, LcdDark, ScreenW);
            int minutes = game.TickCount / 25 / 60;
            r.Text("Spielzeit " + minutes / 60 + "h " + (minutes % 60).ToString("00") + "m", ScreenX, y + 56, Font, LcdDark, ScreenW);
            r.Text(game.Buffs.Describe(game.Clock.TotalTicks), ScreenX, y + 70, 7, LcdDark, ScreenW);
        }

        private static void DrawTasks(G1C_Renderer r, G1C_GameManager game, double y)
        {
            string text = "";
            foreach (G1C_CollectTask task in game.Npcs.Tasks.Tasks)
            {
                string state = task.State == G1C_TaskState.Active
                    ? (task.AllCollected ? "abgeben bei " + task.GiverName : task.CollectedCount + "/" + task.Total)
                    : (task.State == G1C_TaskState.Completed ? "erledigt" : "bei " + task.GiverName + " erfragen");
                text += "• " + task.Title + ": " + state + "\n";
            }
            Paragraph(r, text, y);
        }

        private static void Paragraph(G1C_Renderer r, string text, double y)
        {
            List<string> lines = Wrap(text);
            for (int i = 0; i < lines.Count && i < 8; i++) r.Text(lines[i], ScreenX + 5, y + i * Line, Font, LcdDark);
        }

        // Einfacher Wortumbruch auf die Breite des Handy-Bildschirms
        private static List<string> Wrap(string text)
        {
            List<string> lines = new List<string>();
            foreach (string paragraph in (text ?? "").Split('\n'))
            {
                string line = "";
                foreach (string word in paragraph.Split(' '))
                {
                    if (line.Length > 0 && line.Length + 1 + word.Length > Chars) { lines.Add(line); line = ""; }
                    line += (line.Length > 0 ? " " : "") + word;
                }
                if (line.Length > 0) lines.Add(line);
            }
            return lines;
        }
    }
}
