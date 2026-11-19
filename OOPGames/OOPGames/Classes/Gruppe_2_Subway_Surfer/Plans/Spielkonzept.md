# Spielkonzept: Subway Surfer

**Status:** Entwurf; dem Dozenten vor Umsetzung/Abnahme vorzustellen
**Gruppe:** Gruppe 2
**Plattform:** bestehendes WPF-Spiel-Framework

## Idee und Spielziel

Eine Läuferin flieht auf drei parallel verlaufenden Bahngleisen vor einem Bahnwärter. Die Spielerin weicht entgegenkommenden Zügen und zufällig angeordneten Hindernissen aus, sammelt Münzen und versucht, einen möglichst hohen Punktestand zu erreichen. Die Geschwindigkeit nimmt mit der Laufzeit und dem Score zu. Die Darstellung verwendet perspektivische Formen auf dem vom Framework bereitgestellten Canvas; eine zusätzliche Grafikbibliothek oder ein eigenes Fenster ist nicht vorgesehen.

## Spielablauf und Steuerung

- **Pfeil links/rechts:** zwischen den drei Gleisen wechseln.
- **Pfeil hoch:** springen; offene Güterklappen ermöglichen den Aufstieg auf ein Zugdach.
- **Pfeil runter:** rollen; erforderlich unter großen Absperrgittern.
- Züge nähern sich schneller als zuvor und verlassen den sichtbaren Spielbereich kurz nach der Läuferin; geöffnete Laderampen werden perspektivisch gezeichnet.
- Die Läuferin hat eine fortlaufende Laufanimation.
- Im Hauptfenster den Subway-Surfer-Spieler in **beiden** Spielerfeldern auswählen; das unveränderte Framework wechselt nach jedem Tastendruck zwischen Spieler 1 und 2.
- Auf dem Zugdach muss die Läuferin vor dem Zugende wieder auf ein Gleis wechseln, sonst stürzt sie ab.
- Ein nicht vermiedenes Hindernis beendet den Lauf. Beinahe-Kollisionen verringern den Abstand des Bahnwärters; ohne Beinahe-Kollision ist er nicht sichtbar.
- Münzen und Upgrade-Boxen gewähren Magnet, Münzverdopplung, Surfboard, Hüpfstange oder Jetpack. Diese Effekte erleichtern das Sammeln oder schützen/heben die Läuferin zeitweise an.
- **R:** nach Spielende einen neuen Lauf starten.

## Welt und Hindernisse

Die drei Gleise werden als perspektivische Bahnebene gezeichnet. Züge, Absperrgitter, Büsche, Bordsteine, Brückenpfeiler und Signale erscheinen in zufälligen Wellen. Jede Hinderniswelle lässt mindestens ein Gleis frei. Züge und Hindernisse nähern sich der Läuferin; Münzen und Upgrades liegen auf den Gleisen. Es gibt drei dargestellte Ebenen: Gleis, Zugdach und Luft.

## Umsetzung im Framework

Die Regeln implementieren `IGameRules2`, der Painter `IPaintGame2`, und die Tastatursteuerung erfolgt als `IHumanGamePlayer` über `IKeySelection`. Das Feld prüft die Painter-Kompatibilität mit `CanBePaintedBy`; der Spieler prüft die Regeln mit `CanBeRuledBy`. Alle Klassen und Dateien liegen in `Classes/Gruppe_2_Subway_Surfer/`; nur die Anmeldung der drei Klassen im vorgesehenen Registrierungsblock von `MainWindow.xaml.cs` ist außerhalb des Gruppenordners nötig. Framework-Interfaces und -Ablauf werden nicht verändert.

## Abnahmekriterien

1. Das Spiel ist über die vorhandene Painter-/Rules-/Player-Auswahl startbar und zeichnet ausschließlich auf dem übergebenen Canvas.
2. Gleiswechsel, Sprung, Rolle, zufällige Hindernisse, sichere Ausweichspur und steigendes Tempo funktionieren.
3. Münzen und Upgrades aktualisieren Score bzw. Effektanzeige.
4. Zugdach, Sturz am Zugende, Spielende und Neustart sind nachvollziehbar.
5. Build des bestehenden Projekts ist erfolgreich.
