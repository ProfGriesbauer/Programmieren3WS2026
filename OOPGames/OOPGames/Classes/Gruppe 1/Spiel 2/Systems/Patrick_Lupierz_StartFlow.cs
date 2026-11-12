using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace OOPGames
{
    public enum G1C_StartStage { Title, Slots, Name, Look, Done }

    // =====================================================
    // Klasse: G1C_StartFlow
    // Entwickler: Patrick Lupierz
    // Erstellt: 12.11.2026
    // Beschreibung: Ablauf vor dem eigentlichen Spiel: Hauptmenü, Speicherplatz-Auswahl und
    //               Charaktererstellung (Name, Haare, Haut, Anfangskleidung). Reine Logik; Eingaben
    //               kommen über den GameManager aus dem Framework, gezeichnet wird in G1C_StartView.
    //               Ergebnis: entweder ein neues Profil (Profile) oder ein geladener Spielstand (Loaded).
    // =====================================================
    public class G1C_StartFlow
    {
        public static readonly string[] TitleOptions = { "Neues Spiel", "Spiel laden" };
        public static readonly string[] LookRows = { "Haarfarbe", "Hautfarbe", "Oberteil", "Hose" };

        private readonly List<G1C_Item> _tops = G1C_ItemCatalog.Starters(G1C_ClothingSlot.Top).ToList();
        private readonly List<G1C_Item> _pants = G1C_ItemCatalog.Starters(G1C_ClothingSlot.Pants).ToList();

        public G1C_StartStage Stage { get; private set; } = G1C_StartStage.Title;
        public int Selected { get; private set; }
        public string Message { get; private set; } = "";
        public G1C_PlayerProfile Profile { get; } = new G1C_PlayerProfile();
        public G1C_SaveData Loaded { get; private set; }
        public string[] SlotLines { get; private set; } = new string[0];

        public int RowCount
        {
            get
            {
                switch (Stage)
                {
                    case G1C_StartStage.Title: return TitleOptions.Length;
                    case G1C_StartStage.Slots: return G1C_SaveSystem.SlotCount;
                    case G1C_StartStage.Look: return LookRows.Length;
                    default: return 0;
                }
            }
        }

        public string LookValue(int row)
        {
            switch (row)
            {
                case 0: return G1C_PlayerProfile.HairNames[Profile.Hair];
                case 1: return G1C_PlayerProfile.SkinNames[Profile.Skin];
                case 2: return G1C_ItemCatalog.Get(Profile.Top).Name;
                default: return G1C_ItemCatalog.Get(Profile.Pants).Name;
            }
        }

        // Entwickler: Patrick Lupierz
        public void HandleKey(Key key)
        {
            Message = "";
            if (Stage == G1C_StartStage.Name) { HandleNameKey(key); return; }

            int rows = RowCount;
            if (key == Key.W || key == Key.Up) Selected = (Selected + rows - 1) % rows;
            else if (key == Key.S || key == Key.Down) Selected = (Selected + 1) % rows;
            else if (G1C_InputManager.IsConfirm(key)) Confirm();
            else if (G1C_InputManager.IsBack(key)) Back();
            else if (Stage == G1C_StartStage.Look && G1C_InputManager.IsPrevious(key)) ChangeLook(-1);
            else if (Stage == G1C_StartStage.Look && G1C_InputManager.IsNext(key)) ChangeLook(1);
        }

        /// <summary>Klick auf eine Zeile (Index aus G1C_StartView.RowAt) bzw. auf "Weiter" bei der Namenseingabe.</summary>
        public void Click(int row)
        {
            Message = "";
            if (Stage == G1C_StartStage.Name) { ConfirmName(); return; }
            if (row < 0 || row >= RowCount) return;
            Selected = row;
            if (Stage == G1C_StartStage.Look) ChangeLook(1);
            else Confirm();
        }

        // Entwickler: Patrick Lupierz
        // Aufgabe: Namenseingabe – Buchstaben/Ziffern tippen, Rücktaste löscht, Pfeil runter bestätigt.
        //          (Enter/Leertaste lösen im Framework den Start-Knopf aus und werden deshalb nicht benutzt.)
        private void HandleNameKey(Key key)
        {
            string name = Profile.Name;
            if (key == Key.Back) Profile.Name = name.Length > 0 ? name.Substring(0, name.Length - 1) : "";
            else if (key == Key.Down) ConfirmName();
            else if (key == Key.Escape) { Stage = G1C_StartStage.Title; Selected = 0; }
            else if (name.Length < G1C_PlayerProfile.MaxNameLength)
            {
                if (key >= Key.A && key <= Key.Z)
                {
                    char letter = (char)('A' + (key - Key.A));
                    Profile.Name = name + (name.Length == 0 ? letter : char.ToLowerInvariant(letter));
                }
                else if (G1C_InputManager.ToNumber(key) >= 0 && name.Length > 0)
                {
                    Profile.Name = name + G1C_InputManager.ToNumber(key);
                }
            }
            else Message = "Höchstens " + G1C_PlayerProfile.MaxNameLength + " Zeichen.";
        }

        private void ConfirmName()
        {
            if (!G1C_PlayerProfile.IsValidName(Profile.Name)) { Message = "Bitte gib einen Namen ein."; return; }
            Stage = G1C_StartStage.Look;
            Selected = 0;
        }

        private void Confirm()
        {
            switch (Stage)
            {
                case G1C_StartStage.Title:
                    if (Selected == 0) Stage = G1C_StartStage.Name;
                    else
                    {
                        SlotLines = Enumerable.Range(1, G1C_SaveSystem.SlotCount).Select(G1C_SaveSystem.Describe).ToArray();
                        Stage = G1C_StartStage.Slots;
                    }
                    Selected = 0;
                    break;
                case G1C_StartStage.Slots:
                    Loaded = G1C_SaveSystem.Load(Selected + 1);
                    if (Loaded != null) Stage = G1C_StartStage.Done;
                    else Message = G1C_SaveSystem.Exists(Selected + 1) ? "Dieser Spielstand ist beschädigt." : "Dieser Platz ist leer.";
                    break;
                case G1C_StartStage.Look:
                    Stage = G1C_StartStage.Done;
                    break;
            }
        }

        private void Back()
        {
            if (Stage == G1C_StartStage.Look) Stage = G1C_StartStage.Name;
            else Stage = G1C_StartStage.Title;
            Selected = 0;
        }

        private void ChangeLook(int step)
        {
            switch (Selected)
            {
                case 0: Profile.Hair = Wrap(Profile.Hair + step, G1C_PlayerProfile.HairColors.Length); break;
                case 1: Profile.Skin = Wrap(Profile.Skin + step, G1C_PlayerProfile.SkinColors.Length); break;
                case 2: Profile.Top = Next(_tops, Profile.Top, step); break;
                case 3: Profile.Pants = Next(_pants, Profile.Pants, step); break;
            }
        }

        private static string Next(List<G1C_Item> items, string current, int step)
        {
            return items[Wrap(items.FindIndex(i => i.Id == current) + step, items.Count)].Id;
        }

        private static int Wrap(int value, int length)
        {
            return ((value % length) + length) % length;
        }
    }

    public class G1C_MenuRow
    {
        public string Text { get; set; }
        public string Right { get; set; } = "";
        public string Detail { get; set; } = "";
        public Action Run { get; set; }
    }

    // =====================================================
    // Klasse: G1C_ListMenu
    // Entwickler: Patrick Lupierz
    // Erstellt: 12.11.2026
    // Beschreibung: Allgemeines Listenmenü für Laden, Inventar und Kleiderschrank. Die Zeilen werden
    //               nach jeder Aktion neu aufgebaut, damit Preise, Mengen und Kontostand stimmen.
    // =====================================================
    public class G1C_ListMenu
    {
        public const int VisibleRows = 9;
        private Func<List<G1C_MenuRow>> _build;

        public string Title { get; private set; } = "";
        public string Message { get; set; } = "";
        public List<G1C_MenuRow> Rows { get; private set; } = new List<G1C_MenuRow>();
        public int Selected { get; private set; }
        public int FirstVisible { get { return Math.Max(0, Math.Min(Selected - VisibleRows / 2, Rows.Count - VisibleRows)); } }

        public void Open(string title, Func<List<G1C_MenuRow>> build)
        {
            Title = title;
            _build = build;
            Message = "";
            Selected = 0;
            Rows = build();
        }

        /// <summary>Gibt true zurück, wenn das Menü geschlossen werden soll.</summary>
        public bool HandleKey(Key key)
        {
            if (G1C_InputManager.IsBack(key) || key == Key.I) return true;
            if (Rows.Count == 0) return false;
            if (key == Key.W || key == Key.Up) Selected = (Selected + Rows.Count - 1) % Rows.Count;
            else if (key == Key.S || key == Key.Down) Selected = (Selected + 1) % Rows.Count;
            else if (G1C_InputManager.IsConfirm(key)) Choose(Selected);
            return false;
        }

        public void Choose(int index)
        {
            if (index < 0 || index >= Rows.Count) return;
            Selected = index;
            if (Rows[index].Run != null) Rows[index].Run();
            Rows = _build();
            Selected = Math.Max(0, Math.Min(Selected, Rows.Count - 1));
        }
    }
}
