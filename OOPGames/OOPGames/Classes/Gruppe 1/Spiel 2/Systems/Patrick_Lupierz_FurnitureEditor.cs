using System.Collections.Generic;
using System.Linq;

namespace OOPGames
{
    // =====================================================
    // Klasse: G1C_FurnitureEditor
    // Entwickler: Patrick Lupierz
    // Erstellt: 16.11.2026
    // Beschreibung: Einrichtungsmodus (SprintPlanning "Möbel ... klickbare View"): Das echte Zuhause bleibt
    //               sichtbar und aktualisiert sich sofort. Ein Cursor (Tasten oder Mausklick auf eine Kachel) zeigt,
    //               wo das gehaltene Möbel landet – grün = frei, rot = blockiert. Aufgestellte eigene Möbel kann man
    //               anklicken, aufheben, woanders hinstellen oder ins Inventar zurücklegen ([R]).
    //               Reine Logik; gezeichnet wird im G1C_CasinoPainter, Eingaben verteilt der G1C_GameManager.
    // =====================================================
    public sealed class G1C_FurnitureEditor
    {
        public int CursorX { get; private set; } = 1;
        public int CursorY { get; private set; } = 1;
        public G1C_Item Held { get; private set; }
        public G1C_PlacedFurniture PickedUp { get; private set; }
        public string Message { get; private set; } = "";
        public bool IsMoving { get { return PickedUp != null; } }

        // Entwickler: Patrick Lupierz – neues Möbel aus dem Inventar halten (null = Verschiebe-Modus)
        public void Begin(G1C_Item item, G1C_HomeWorld home, G1C_Player player)
        {
            Held = item;
            PickedUp = null;
            CursorX = player != null ? player.TileX : 1;
            CursorY = player != null ? player.TileY : 1;
            if (item != null)
            {
                G1C_FurnitureLocation first = home.AvailableFurnitureLocations(item, player).FirstOrDefault();
                if (first != null) { CursorX = first.X; CursorY = first.Y; }
            }
            Message = item != null ? item.Name + ": Platz wählen und [E] oder Klick zum Aufstellen."
                                   : "Eigenes Möbel anklicken oder mit [E] aufheben, um es zu verschieben.";
        }

        public void MoveCursor(int dx, int dy, G1C_HomeWorld home)
        {
            CursorX = System.Math.Max(1, System.Math.Min(home.Map.Width - 2, CursorX + dx));
            CursorY = System.Math.Max(1, System.Math.Min(home.Map.Height - 2, CursorY + dy));
        }

        public void SetCursor(int x, int y, G1C_HomeWorld home)
        {
            MoveCursor(x - CursorX, y - CursorY, home);
        }

        public bool CursorValid(G1C_HomeWorld home, G1C_Player player)
        {
            return Held != null && home.CanPlaceFurniture(Held, CursorX, CursorY, player);
        }

        // Entwickler: Patrick Lupierz
        // Aufgabe: [E]/Klick. Ohne gehaltenes Möbel: eigenes Möbel unter dem Cursor aufheben. Mit Möbel: aufstellen
        //          (aus dem Inventar nur, wenn noch eins da ist; verschobene Möbel kosten nichts).
        public void Confirm(G1C_HomeWorld home, G1C_Inventory inventory, G1C_Player player)
        {
            if (Held == null)
            {
                G1C_PlacedFurniture placed = home.PlacedAt(CursorX, CursorY);
                G1C_Item item = placed != null ? G1C_ItemCatalog.Get(placed.ItemId) : null;
                if (item == null) { Message = "Hier steht kein eigenes Möbel. (Bett, Schrank und Tisch gehören zum Haus.)"; return; }
                home.RemovePlacedFurniture(placed);
                PickedUp = placed;
                Held = item;
                CursorX = placed.X;
                CursorY = placed.Y;
                Message = item.Name + " aufgehoben. Neuen Platz wählen, [R] ins Inventar, [Q] zurückstellen.";
                return;
            }
            if (!home.CanPlaceFurniture(Held, CursorX, CursorY, player)) { Message = "Hier ist kein Platz für " + Held.Name + "."; return; }
            if (PickedUp != null)
            {
                home.PlaceFurniture(Held, CursorX, CursorY, player);
                Message = Held.Name + " verschoben.";
                PickedUp = null;
                Held = null;
                return;
            }
            if (!inventory.Has(Held.Id)) { Message = "Du hast kein " + Held.Name + " mehr."; Held = null; return; }
            home.PlaceFurniture(Held, CursorX, CursorY, player);
            inventory.Remove(Held.Id);
            Message = Held.Name + " aufgestellt." + (inventory.Has(Held.Id) ? " Noch " + inventory.Count(Held.Id) + " im Inventar." : "");
            if (!inventory.Has(Held.Id)) Held = null;
        }

        // Entwickler: Patrick Lupierz – aufgehobenes Möbel ins Inventar zurücklegen
        public void Store(G1C_Inventory inventory)
        {
            if (PickedUp == null) { Message = "Erst ein aufgestelltes Möbel aufheben."; return; }
            inventory.Add(PickedUp.ItemId);
            Message = Held.Name + " liegt wieder im Inventar.";
            PickedUp = null;
            Held = null;
        }

        // Entwickler: Patrick Lupierz – nächstes Möbel aus dem Inventar wählen (Tab)
        public void NextFromInventory(G1C_Inventory inventory, G1C_HomeWorld home, G1C_Player player)
        {
            if (PickedUp != null) return;
            List<G1C_Item> items = inventory.Items.Where(i => i.Category == G1C_ItemCategory.Moebel).ToList();
            if (items.Count == 0) { Held = null; Message = "Keine Möbel im Inventar – im Möbelhaus gibt es welche."; return; }
            int index = Held != null ? items.FindIndex(i => i.Id == Held.Id) : -1;
            int x = CursorX, y = CursorY;
            Begin(items[(index + 1) % items.Count], home, player);
            CursorX = x;
            CursorY = y;
        }

        // Entwickler: Patrick Lupierz – Abbrechen: ein aufgehobenes Möbel kehrt an seinen alten Platz zurück
        public void Cancel(G1C_HomeWorld home)
        {
            if (PickedUp != null)
            {
                G1C_Item item = G1C_ItemCatalog.Get(PickedUp.ItemId);
                if (item != null) home.PlaceFurniture(item, PickedUp.X, PickedUp.Y, null);
            }
            PickedUp = null;
            Held = null;
        }

        // Entwickler: Patrick Lupierz
        // Aufgabe: Kachel unter einem Mausklick (virtuelle Koordinaten). Gleiche Rechnung wie der Painter für
        //          nicht scrollende Szenen: Karte mittig unter dem HUD.
        public static bool TileAt(double vx, double vy, IG1C_WorldScene world, out int tileX, out int tileY)
        {
            double size = world.TileSize;
            double mapX = (G1C_Renderer.ScreenWidth - world.Grid.Width * size) / 2;
            double mapY = G1C_GameHUD.Height + (G1C_Renderer.ScreenHeight - G1C_GameHUD.Height - world.Grid.Height * size) / 2;
            tileX = (int)System.Math.Floor((vx - mapX) / size);
            tileY = (int)System.Math.Floor((vy - mapY) / size);
            return tileX >= 0 && tileY >= 0 && tileX < world.Grid.Width && tileY < world.Grid.Height;
        }
    }
}
