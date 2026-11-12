using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace OOPGames
{
    public class G1C_Sms
    {
        public string From { get; set; }
        public string Text { get; set; }
        public string Stamp { get; set; }
        public bool Read { get; set; }
    }

    public enum G1C_PhoneScreen { Home, Contacts, Messages, MessageDetail, Clock, Tasks, Save }

    public enum G1C_PhoneRequestKind { None, Close, Call, Inventory, Save }

    public struct G1C_PhoneRequest
    {
        public G1C_PhoneRequestKind Kind;
        public G1C_NpcCharacter Contact;
        public int Slot;
    }

    // =====================================================
    // Klasse: G1C_Phone
    // Entwickler: Patrick Lupierz
    // Erstellt: 12.11.2026
    // Beschreibung: Zustand des Klapphandys: Apps, Kontakte (freigeschaltete Telefonnummern),
    //               SMS und Erreichbarkeit der NPCs. Reine Logik – gezeichnet wird in G1C_PhoneView,
    //               Eingaben kommen über den GameManager aus dem Framework.
    //               Neue Apps: Namen in Apps ergänzen und in Select/HandleKey behandeln.
    // =====================================================
    public class G1C_Phone
    {
        public static readonly string[] Apps = { "Kontakte", "SMS", "Uhr", "Aufgaben", "Inventar", "Speichern" };
        // Bildschirm je App (gleiche Reihenfolge wie Apps; "Inventar" öffnet das Inventarmenü)
        private static readonly G1C_PhoneScreen[] AppScreens =
        {
            G1C_PhoneScreen.Contacts, G1C_PhoneScreen.Messages, G1C_PhoneScreen.Clock,
            G1C_PhoneScreen.Tasks, G1C_PhoneScreen.Home, G1C_PhoneScreen.Save
        };
        public const int MaxMessages = 20;

        private readonly G1C_NpcSystem _npcs;
        private int _lastSmsCheck = -1; // Tag * 24 + Stunde der letzten Prüfung

        public G1C_Phone(G1C_NpcSystem npcs)
        {
            _npcs = npcs ?? throw new ArgumentNullException(nameof(npcs));
        }

        public G1C_PhoneScreen Screen { get; private set; }
        public int Selected { get; private set; }
        public int OpenedTick { get; private set; }
        public string Info { get; set; } = "";
        public List<G1C_Sms> Messages { get; set; } = new List<G1C_Sms>();
        public string[] SlotLines { get; private set; } = new string[0];
        public int Unread { get { return Messages.Count(m => !m.Read); } }

        public List<G1C_NpcCharacter> Contacts
        {
            get { return _npcs.Characters.Where(c => c.Memory.PhoneNumberGiven).ToList(); }
        }

        /// <summary>Erfundene, eindeutige Nummer je NPC (Reihenfolge im Charakterverzeichnis).</summary>
        public string NumberOf(G1C_NpcCharacter contact)
        {
            int index = Math.Max(0, _npcs.Characters.ToList().IndexOf(contact));
            return "0555-01" + (index + 10);
        }

        /// <summary>Speicherplatz-Zeilen neu von der Platte lesen (nur beim Öffnen der App und nach dem Speichern).</summary>
        public void RefreshSlots()
        {
            SlotLines = Enumerable.Range(1, G1C_SaveSystem.SlotCount).Select(G1C_SaveSystem.Describe).ToArray();
        }

        public void Open(int tick)
        {
            Screen = G1C_PhoneScreen.Home;
            Selected = 0;
            Info = "";
            OpenedTick = tick;
        }

        private int RowCount
        {
            get
            {
                switch (Screen)
                {
                    case G1C_PhoneScreen.Home: return Apps.Length;
                    case G1C_PhoneScreen.Contacts: return Contacts.Count;
                    case G1C_PhoneScreen.Messages: return Messages.Count;
                    case G1C_PhoneScreen.Save: return G1C_SaveSystem.SlotCount;
                    default: return 0;
                }
            }
        }

        // Entwickler: Patrick Lupierz
        // Aufgabe: W/S wählen, E bestätigen, Q zurück, P schließen. Aktionen außerhalb des Handys
        //          (Anruf, Inventar, Speichern) werden als Anfrage an den GameManager zurückgegeben.
        public G1C_PhoneRequest HandleKey(Key key, G1C_GameClock clock)
        {
            G1C_PhoneRequest none = new G1C_PhoneRequest();
            if (key == Key.P) return new G1C_PhoneRequest { Kind = G1C_PhoneRequestKind.Close };

            int rows = RowCount;
            if (rows > 0 && (key == Key.W || key == Key.Up)) Selected = (Selected + rows - 1) % rows;
            else if (rows > 0 && (key == Key.S || key == Key.Down)) Selected = (Selected + 1) % rows;
            else if (G1C_InputManager.IsBack(key))
            {
                if (Screen == G1C_PhoneScreen.Home) return new G1C_PhoneRequest { Kind = G1C_PhoneRequestKind.Close };
                Screen = Screen == G1C_PhoneScreen.MessageDetail ? G1C_PhoneScreen.Messages : G1C_PhoneScreen.Home;
                if (Screen == G1C_PhoneScreen.Home) Selected = 0;
                Info = "";
            }
            else if (G1C_InputManager.IsConfirm(key))
            {
                return Select(clock);
            }
            return none;
        }

        private G1C_PhoneRequest Select(G1C_GameClock clock)
        {
            G1C_PhoneRequest none = new G1C_PhoneRequest();
            switch (Screen)
            {
                case G1C_PhoneScreen.Home:
                    if (Apps[Selected] == "Inventar") return new G1C_PhoneRequest { Kind = G1C_PhoneRequestKind.Inventory };
                    Screen = AppScreens[Selected];
                    if (Screen == G1C_PhoneScreen.Save) RefreshSlots();
                    Selected = 0;
                    Info = "";
                    return none;

                case G1C_PhoneScreen.Contacts:
                    List<G1C_NpcCharacter> contacts = Contacts;
                    if (Selected >= contacts.Count) return none;
                    string blocked = CallBlockReason(contacts[Selected], clock);
                    if (blocked != null) { Info = blocked; return none; }
                    return new G1C_PhoneRequest { Kind = G1C_PhoneRequestKind.Call, Contact = contacts[Selected] };

                case G1C_PhoneScreen.Messages:
                    if (Selected >= Messages.Count) return none;
                    Messages[Selected].Read = true;
                    Screen = G1C_PhoneScreen.MessageDetail;
                    return none;

                case G1C_PhoneScreen.Save:
                    return new G1C_PhoneRequest { Kind = G1C_PhoneRequestKind.Save, Slot = Selected + 1 };

                default: return none;
            }
        }

        // Entwickler: Patrick Lupierz
        // Aufgabe: Erreichbarkeit nach Beziehung, Stimmung und Uhrzeit. null = der NPC nimmt ab.
        public string CallBlockReason(G1C_NpcCharacter contact, G1C_GameClock clock)
        {
            G1C_RelationshipStatus status = contact.Relationship.Status;
            if (G1C_NpcRelationship.IsHostile(status)) return contact.Name + " drückt dich weg.";
            if (status == G1C_RelationshipStatus.GoodFriend) return null;
            if (contact.Mood == G1C_NpcMood.Angry) return contact.Name + " geht gerade nicht ran.";

            bool friend = status == G1C_RelationshipStatus.Friend;
            int from = friend ? 7 : 8;
            int until = friend ? 24 : 21;
            if (clock.Hour >= from && clock.Hour < until) return null;
            return "Niemand geht ran. Es ist " + clock.TimeText + " Uhr.";
        }

        // Entwickler: Patrick Lupierz
        // Aufgabe: Einmal pro Ingame-Stunde prüfen, ob jemand schreibt (höchstens eine SMS pro Stunde).
        public void HourlyUpdate(G1C_GameClock clock, string playerName, int balance, Random random)
        {
            int stamp = clock.Day * 24 + clock.Hour;
            if (stamp == _lastSmsCheck) return;
            bool firstCheck = _lastSmsCheck < 0;
            _lastSmsCheck = stamp;
            if (firstCheck) return;

            List<G1C_NpcCharacter> friends = Contacts.Where(c => G1C_NpcRelationship.IsFriendly(c.Relationship.Status)).ToList();
            G1C_NpcCharacter rival = _npcs.Characters.FirstOrDefault(c => G1C_NpcRelationship.IsRival(c.Relationship.Status));

            if (clock.Hour == 18 && friends.Count > 0)
                Receive(friends[random.Next(friends.Count)].Name, "Hey " + playerName + ", komm heute Abend ins Casino!", clock);
            else if (clock.Hour == 12 && rival != null)
                Receive(rival.Name, "Zeit für eine Revanche! Ich warte am Pokertisch.", clock);
            else if (clock.Hour == 10 && balance < 100)
                Receive("Barkeeper Toni", "Knapp bei Kasse? Komm vorbei, ich habe Arbeit für dich.", clock);
        }

        public void Receive(string from, string text, G1C_GameClock clock)
        {
            Messages.Insert(0, new G1C_Sms { From = from, Text = text, Stamp = "Tag " + clock.Day + ", " + clock.TimeText });
            if (Messages.Count > MaxMessages) Messages.RemoveAt(Messages.Count - 1);
        }
    }
}
