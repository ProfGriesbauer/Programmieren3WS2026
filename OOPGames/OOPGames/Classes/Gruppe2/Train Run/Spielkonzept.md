# Spielkonzept: Train Run

**Gruppe:** Gruppe 2
**Projektordner:** `Classes/Gruppe2/Train Run`
**Plattform:** bestehendes WPF-Spiel-Framework

## Idee und Spielziel

Train Run ist ein endloser 3D-Lauf auf drei Bahngleisen. Die Läuferin bewegt sich automatisch vorwärts und muss Zügen sowie Hindernissen ausweichen, Münzen einsammeln und aktivierte Upgrades nutzen. Der Punktestand steigt mit der Laufzeit und der Laufgeschwindigkeit. Das Tempo nimmt während eines Laufs zu.

Die Bahnwelt wird perspektivisch im vom Framework bereitgestellten Canvas dargestellt. Die Darstellung verwendet WPF-3D-Modelle und Beleuchtung; es wird kein eigenes Fenster, keine zusätzliche Spielschleife und keine externe Grafikbibliothek verwendet.

## Spielablauf

1. Nach dem Start läuft die Läuferin auf der mittleren Spur.
2. Der Spieler wechselt die Spur, springt, rollt oder aktiviert verfügbare Höhenbewegungen.
3. Züge und Hindernisse erscheinen in zufälligen Wellen. Jede Welle lässt mindestens eine sichere Ausweichoption.
4. Münzen und Upgrades werden gesammelt und wirken sich unmittelbar auf den Punktestand oder den Spielerstatus aus.
5. Bei einer harten Kollision endet der Lauf. Die Anzeige zeigt den Spielstand und meldet das Ende an.

Bei Spielende kann ein neuer Lauf über eine neue Instanz beziehungsweise Auswahl des Spiels gestartet werden.

## Steuerung

- **Pfeil links / A:** eine Spur nach links wechseln.
- **Pfeil rechts / D:** eine Spur nach rechts wechseln.
- **Pfeil hoch / W:** springen.
- **Pfeil runter / S:** rollen beziehungsweise ducken.

Im Hauptfenster wird der menschliche Train-Run-Spieler in beiden Spielerfeldern ausgewählt. Das vorhandene Framework verarbeitet gültige Tastendrücke und wechselt danach den aktiven Spieler.

## Bahnwelt und Hindernisse

Die Welt besteht aus drei Spuren und drei Höhenebenen:

- Gleis: normale Laufebene.
- Zugdach: erreichbar über offene Güterrampen beziehungsweise Übergänge auf Züge.
- Flughöhe: erreichbar mit dem Jetpack.

Folgende Hindernisse und Situationen sind umgesetzt oder vorgesehen:

- kleine und große Absperrbalken,
- Büsche und Pflanzentöpfe,
- Bordsteine,
- Züge und Güterwagen,
- offene Güterklappen beziehungsweise Rampen,
- Signale,
- Hinderniswellen mit mindestens einer freien Spur.

Ein Zugdach wird nur betreten, wenn eine passende Güterrampe vorhanden ist. Die Läuferin kann nicht auf beliebiges Zugmaterial wechseln. Wird sie am Ende eines Zuges nicht rechtzeitig zurück auf ein Gleis bewegt, endet der Lauf.

## Münzen und Upgrades

- **Münzen:** erhöhen den Münzstand und den Punktestand.
- **Magnet:** zieht Münzen aus der eigenen und benachbarten Spuren an.
- **Doppelte Münzen:** verdoppelt gesammelte Münzen für die aktive Dauer.
- **Surfboard:** schützt einmalig vor einer Kollision.
- **Hüpfstab:** unterstützt Sprünge über Hindernisse.
- **Jetpack:** hebt die Läuferin in die Flughöhe und ermöglicht das Überfliegen von Hindernissen.

Die HUD-Anzeige zeigt Punktestand, Münzen, Tempo, aktive Upgrades und den Abstand zum Verfolger. Die aktive Wirkung der Upgrades wird über ihre Restdauer verwaltet.

Der Verfolger wird in einer Einführungsphase beziehungsweise bei sinkendem Abstand sichtbar. Eine Beinahe-Kollision verkürzt den Abstand, eine erfolgreiche Ausweichphase lässt ihn zurückfallen.

## Technik und Framework

Die Klassen liegen in `Classes/Gruppe2/Train Run/`:

- `Gruppe2_TrainRunField` verwaltet Zustand und Daten des Laufs.
- `Gruppe2_TrainRunRules` verwaltet Spielablauf, Kollisionen, Hindernisse, Münzen und Upgrades.
- `Gruppe2_TrainRunPainter` zeichnet die perspektivische 3D-Szene auf das übergebene WPF-Canvas.
- `Gruppe2_TrainRunHumanPlayer` übersetzt Tastatureingaben in Spielzüge.
- Zusätzliche Klassen trennen Lane-Wechsel, Layout, Beleuchtung, Szene und 3D-Objektverwaltung.

Die Umsetzung verwendet:

- `IGameRules2` für die Spielregeln,
- `IPaintGame2` für die Darstellung,
- `IHumanGamePlayer` und `IKeySelection` für Eingaben.

Das Spielfeld prüft die Painter-Kompatibilität mit `CanBePaintedBy`. Der Spieler prüft die Rules-Kompatibilität mit `CanBeRuledBy`. Die Anmeldung erfolgt im vorgesehenen Registrierungsblock von `MainWindow.xaml.cs`. Die Interfaces und der Ablauf des Frameworks werden nicht verändert.

## Abnahmekriterien

1. Train Run ist über die vorhandene Painter-, Rules- und Player-Auswahl startbar.
2. Das Spiel zeichnet ausschließlich auf das vom Framework übergebene Canvas.
3. Drei Spuren, Spurwechsel, Sprung, Rolle und automatisches Tempo funktionieren.
4. Zufällige Hinderniswellen lassen mindestens eine sichere Spur.
5. Züge, Güterrampen, Zugdächer und Sturz beziehungsweise Spielende sind erkennbar umgesetzt.
6. Münzen und alle Upgrades werden eingesammelt und in HUD beziehungsweise Spielzustand angezeigt.
7. Kollisionen führen zu Schutz oder Spielende; Beinahe-Kollisionen beeinflussen den Verfolgerabstand.
8. Das bestehende WPF-Projekt kompiliert und startet Train Run fehlerfrei.
