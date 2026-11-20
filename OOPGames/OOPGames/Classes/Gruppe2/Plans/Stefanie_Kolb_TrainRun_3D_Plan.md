# Subway Surfer: 3D-Darstellung

## Ziel

Die Spielszene soll räumlich in WPF 3D dargestellt werden. Zug- und Wagon-Hindernisse sollen am Horizont sichtbar sein und perspektivisch bis zum Spieler wandern.

## Umsetzung

- Die 3D-Szene wird als `Viewport3D` innerhalb des vom Framework übergebenen Canvas gezeichnet.
- Gleise, Schwellen, Stadt, Spieler, Verfolger, Züge, Wagons, Hindernisse und Münzen werden als 3D-Modelle dargestellt.
- Die vorhandene Spiel-Distanz bestimmt die Z-Position der Objekte. Ein gemeinsamer Projektionsmaßstab lässt sie mit der Annäherung größer erscheinen.
- HUD und Hinweise bleiben als Canvas-Overlay lesbar.
- Es werden keine eigenen Fenster, Render-Schleifen, Framework-Erweiterungen oder zusätzlichen Direct3D-Abhängigkeiten eingeführt.

## Grenzen

Änderungen bleiben auf `Classes/Gruppe2/` beschränkt. Framework-Dateien, Projektdateien und fremde Gruppenordner bleiben unverändert. Native Direct3D-11-Integration ist nicht Teil dieser Lösung.

## Überprüfung

Projekt mit `dotnet build` bauen und anschließend im Hauptfenster prüfen, dass die Szene auf dem übergebenen Canvas gerendert wird und Hindernisse vom Horizont nach vorne wandern.
