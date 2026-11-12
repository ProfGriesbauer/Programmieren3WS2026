# Spielkonzept: Gruppe 2 Subway Surfer

## Konzept

Ein weiblicher Charakter laeuft in einer perspektivisch gezeichneten Bahnwelt automatisch nach vorne. Drei Spuren, herannahende Zuege und weitere Hindernisse bilden einen endlosen Lauf. Pfeiltasten wechseln die Spur, springen oder rollen. Gesammelte Muenzen und zufaellige Upgrade-Boxen verbessern den Lauf; mit wachsendem Punktestand nimmt das Tempo zu. Nach einem leichten Zusammenstoss kommt der Bahnbaer naeher, ein schwerer Zusammenstoss beendet das Spiel.

## Regeln und Spielablauf

- Drei Spuren und ein stets freier Weg pro Hinderniswelle.
- Gleis-, Dach- und Luftzustand; offene Gueterklappen erlauben den Sprung aufs Zugdach.
- Schranken, Buesche, Bordsteine, Zuege, Rampen, Signale und Mittelpfeiler.
- Muenzen sowie Magnet, Muenzenverdopplung, Surfboard, Huepfstab, Jetpack und Tempo-Boost.
- Score, Muenzen, Tempo, aktive Upgrades und Verfolgerabstand werden angezeigt.
- Pfeiltasten: links/rechts Spur wechseln, hoch springen, runter ducken/rollen.

## Umsetzung im vorgegebenen Framework

Das Spiel verwendet `IGameRules2` und `IPaintGame2` fuer die vorhandenen 40-ms-Ticks sowie `IHumanGamePlayer` und `KeySelection` fuer Eingaben. Gezeichnet wird ausschliesslich auf dem vom Framework uebergebenen Canvas. Es wird kein eigenes Fenster und keine eigene Spielschleife angelegt. Die Framework-Interfaces bleiben unveraendert; im Hauptfenster werden nur die neuen Gruppe-2-Klassen im vorgesehenen Registrierungsblock eingetragen.

Das Hauptfenster wechselt nach jedem gueltigen Eingabeschritt den aktiven Spieler. Deshalb muss fuer beide Spielerauswahlen der menschliche Subway-Surfer-Spieler ausgewaehlt werden.

## Inkremente

1. Feld, Eingabe, drei Spuren, Perspektive und kontinuierliche Bewegung.
2. Zufallswellen, Kollisionen, Sprung/Rolle und freier Fluchtweg.
3. Muenzen, Upgrades, steigendes Tempo, Punktestand und Verfolger.
4. Build und Spielstart im bestehenden WPF-Hauptfenster pruefen.

**Freigabe:** Vor Abgabe mit der Lehrperson abstimmen, wie in den Projektanforderungen vorgesehen.
