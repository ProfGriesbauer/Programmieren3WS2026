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
- Beispiel: `player_left_1.png`. NPCs wechseln beim Laufen ebenfalls zwischen Frame `1` und `2`.

**Casino-Kacheln:** `tile_floor`, `tile_floor_alt`, `tile_wall`, `tile_wall_face`, `tile_exit`

**Außenwelt-Kacheln:** `tile_city_grass`, `tile_city_grass_alt`, `tile_city_road`,
`tile_city_sidewalk`, `tile_city_path`, `tile_city_border`, `tile_city_forest`,
`tile_city_water`, `tile_city_water_alt`, `tile_city_bank`, `tile_city_bridge`,
`tile_city_park`, `tile_city_home_entry`, `tile_city_casino_entry`,
`tile_city_hospital_entry`, `tile_city_market_entry`

**Gebäude-Innenraum-Kacheln:** `tile_home_floor`, `tile_home_floor_alt`, `tile_home_wall`,
`tile_home_exit`, `tile_hospital_floor`, `tile_hospital_wall`, `tile_market_floor`,
`tile_market_wall`, `tile_interior_rug`, `tile_interior_exit`

**Möbel:** `obj_roulette_table`, `obj_blackjack_table`, `obj_poker_table` (je 3x2 Kacheln),
`obj_slot_machine`, `obj_bar`, `obj_plant` (je 1 Kachel breit)

**Minispiele:** `roulette_wheel` (Tasche 0 oben, Reihenfolge im Uhrzeigersinn), `card_front`, `card_back`, `chip`,
`slot_cherry`, `slot_lemon`, `slot_bell`, `slot_bar`, `slot_diamond`, `slot_seven`

**Weltobjekte:** `building_home`, `building_hospital`, `building_market`, `building_casino`,
`building_library`, `building_townhouse`, `building_cafe`, `building_flower_shop`,
`building_fire_station`, `building_cinema`, `building_pavilion`, `building_warehouse`,
`building_post`, `obj_city_tree`, `obj_home_bed`, `obj_home_table`, `obj_counter`,
`obj_medical_bed`, `obj_clinic_cabinet`, `obj_store_shelf`, `obj_chair`

**Tiere** – jeweils Frame `0` und `1` für die tickbasierte Umgebungsanimation:
`animal_duck_0`, `animal_duck_1`, `animal_fish_0`, `animal_fish_1`,
`animal_cat_0`, `animal_cat_1`, `animal_bird_0`, `animal_bird_1`

**NPC-Porträts** (Gesprächsfenster) – `portrait_<skin>_<ausdruck>`
- skin: `dealer_red`, `dealer_green`, `dealer_blue`, `bartender`, `guest`
- ausdruck: `neutral`, `happy`, `angry`, `sad`, `surprised`, `thoughtful`
- Beispiel: `portrait_dealer_blue_angry.png` (Pokerprofi Max, wütend). Kopf und Oberkörper,
  Seitenverhältnis 32:26 (z.B. 96x78 oder 128x104).
- Eingebaut ist vorläufig eine vergrößerte Version des Front-Sprites `<skin>_down_0` mit eingezeichnetem
  Gesichtsausdruck. **Noch benötigt:** 30 detaillierte Porträts (5 NPCs × 6 Ausdrücke), die Haare, Haut
  und Kleidung der Figuren übernehmen (Rosa: schwarze Haare, rote Weste; Ben: braune Haare, grüne
  Weste; Max: dunkelgraue Haare, blaue Weste; Toni: braune Haare, schwarze Weste; Lena: blonde Haare,
  grünes Shirt).

**Aufgaben-Gegenstände:** `item_glass`, `item_lost_chip`
