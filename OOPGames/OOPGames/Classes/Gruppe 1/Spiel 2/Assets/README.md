# Grafiken austauschen

Jede Grafik hat eine Sprite-ID. Ohne eigene Datei wird die eingebaute Pixel-Art verwendet
(`Graphics/BuiltInSprites.cs`).

**Eigene Grafik verwenden:** PNG-Datei mit dem Namen `<Sprite-ID>.png` in den Ordner
`G1Casino_Assets` neben der `OOPGames.exe` legen (z.B. `bin/Debug/net10.0-windows/G1Casino_Assets/`)
und das Programm neu starten. Die Bildgröße ist frei (z.B. 90x90 Pixel), Transparenz wird unterstützt.
Hohe Bilder (z.B. 90x135) ragen nach oben über ihre Kachel hinaus.

## Sprite-IDs

**Figuren** – `<skin>_<richtung>_<frame>`
- skin: `player`, `dealer_red`, `dealer_green`, `dealer_blue`, `bartender`, `guest`
- richtung: `down`, `up`, `left`, `right` (jede Laufrichtung hat eigene Grafiken)
- frame: `0` = stehen, `1` und `2` = Schritte
- Beispiel: `player_left_1.png`. NPCs benutzen nur Frame `0`.

**Kacheln:** `tile_floor`, `tile_floor_alt`, `tile_wall`, `tile_wall_face`, `tile_exit`

**Möbel:** `obj_roulette_table`, `obj_blackjack_table`, `obj_poker_table` (je 3x2 Kacheln),
`obj_slot_machine`, `obj_bar`, `obj_plant` (je 1 Kachel breit)

**Minispiele:** `roulette_wheel` (Tasche 0 oben, Reihenfolge im Uhrzeigersinn), `card_front`, `card_back`, `chip`,
`slot_cherry`, `slot_lemon`, `slot_bell`, `slot_bar`, `slot_diamond`, `slot_seven`
