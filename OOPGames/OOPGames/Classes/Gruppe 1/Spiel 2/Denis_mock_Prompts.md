# Prompt-Protokoll – Casino (Spiel 2)

## 12-11-2026

### Prompt 1
zieh mir den branch von patrick

Werkzeug: Copilot (VS-Code)
Modell: Nicht angezeigt

Ergebnisvermerk: Nach Auswahl des Branches `origin/Casino-Revise-Version-1` wurde dieser ausgecheckt. Lokale Änderungen an `HouseExample/obj` wurden zuvor im Stash `before switching to origin/Casino-Revise-Version-1` gesichert und nicht verworfen.

### Prompt 2
speichere meine prompts in Denis_mock_Prompts.md, orientiere dich dabei an die stuktur von patrick lupierz ab jetzt

Werkzeug: Copilot (VS-Code)
Modell: Nicht angezeigt

Ergebnisvermerk: Dieses Prompt-Protokoll wurde nach der Struktur von `Patrick_Lupierz_Prompts.md` angelegt. Künftige in diesem Chat erfasste Prompts werden im selben Format ergänzt.

## 12-11-2026

### Prompt 3

```text
Erweitere unser bestehendes Casino-Spiel um eine begehbare Außenwelt. **Untersuche zuerst das vollständige Projekt in VS Code, insbesondere „Spiel 2“ in „Gruppe 1“, und orientiere dich an dessen tatsächlichem Aufbau und den dort verwendeten Lösungen.** Übernimm passende Muster für Weltaufbau, Bewegung, Kollisionen, Interaktionen und Übergänge, aber prüfe sie gegen unser eigenes Projekt und das vorhandene OOPGames-Framework.

## Gewünschtes Spielerlebnis
Das Spiel startet im eigenen Zuhause. Von dort kann die Spielfigur das Haus verlassen, sich durch eine quadratische Stadt bewegen und zum Casino laufen. Das Casino ist ein Gebäude in dieser Außenwelt und bleibt mit dem vorhandenen Casino-Spiel verbunden.

Die Welt soll im Pokémon-Rubin/Saphir-inspirierten 2D-Pixel-Art-Stil und aus der Top-Down-Perspektive gestaltet sein. Sie enthält Straßen, Wege, Gassen und mehrere erkennbare Gebäude, mindestens:

- das Zuhause des Spielers als Startort,
- das Casino,
- ein Krankenhaus,
- einen Supermarkt,
- weitere passende Gebäude oder Stadtbereiche.
Die Gebäude sollen sinnvoll in die quadratische Karte eingebettet sein. Gassen und Wege müssen begehbar und miteinander verbunden sein. **Mindestens das Zuhause und das Casino müssen betretbar und wieder verlassbar sein.** Für weitere Gebäude soll geprüft werden, ob sich ihre Innenräume mit der vorhandenen Architektur sinnvoll und frameworkkonform umsetzen lassen.

Die Welt erhält eine klar erkennbare, geschlossene Außengrenze nach dem Vorbild einer Pokémon-World-Border. Die Spielfigur darf die Karte nicht verlassen. Grenzen, Gebäude, Wände und nicht begehbare Objekte müssen Kollisionen besitzen; die Bewegung entlang von Hindernissen soll weiterhin möglich sein.

## Verbindliche Analyse vor der Umsetzung

1. Lies die Projektregeln und halte sie vollständig ein.
2. Untersuche „Spiel 2“ in „Gruppe 1“ vollständig in VS Code: relevante Klassen, Kartenaufbau, Bewegung, Kollisionen, Eingaben, Zeichnung und Gebäude- oder Szenenübergänge.
3. Untersuche anschließend die relevanten Klassen unseres Casino-Spiels und die tatsächlich vorhandenen Framework-Interfaces und Beispielimplementierungen.
4. Erfinde keine Framework-Methoden, Properties, Events, Eingaben oder Canvas-Funktionen. Verwende nur nachweislich vorhandene Schnittstellen.
5. Leite aus dem Vergleich ab, welche Muster sich für unser Projekt eignen. Nenne vor der Umsetzung kurz die betroffenen eigenen Dateien und die geplante Aufteilung.
6. Stelle sicher, dass der vollständige Name des verantwortlichen Teammitglieds bekannt ist. Falls er nicht aus dem Projekt hervorgeht, frage danach, bevor du Code erstellst oder änderst.

## Technische und organisatorische Grenzen

- Das Spiel muss vollständig als reguläre Komponente des vorhandenen OOPGames-Frameworks laufen.
- Keine eigene Main-Methode, Game-Loop, Fenster, Zeichenfläche oder Framework-Initialisierung erstellen.
- Ausschließlich auf dem vom Framework übergebenen Canvas zeichnen und Eingaben nur über die tatsächlich vorgesehenen Framework-Schnittstellen verarbeiten.
- Framework-Dateien, Basis-Interfaces, Projektdateien, fremde Gruppenordner und Dateien außerhalb des erlaubten Gruppenordners nicht verändern.
- Änderungen sind nur in `Classes/<eigene Gruppe>/` und `Interfaces/<eigene Gruppe>/` zulässig. Die einzige mögliche Ausnahme ist die vorgesehene Klassenregistrierung innerhalb des ausdrücklich markierten Registrierungsblocks in `MainWindow.xaml.cs`; dort ausschließlich registrieren.
- Bestehende Spiel- und Framework-Funktionen nicht unnötig ersetzen.
- Grafik und Spiellogik getrennt halten. Grafiken müssen austauschbar bleiben. Lege keine Assets in nicht freigegebenen Verzeichnissen ab. Falls benötigte Grafiken dort nicht zulässig sind, verwende eine mit den Projektregeln vereinbare Darstellung oder beschreibe die offene Abhängigkeit.
- Neue und wesentlich geänderte Klassen und Methoden mit dem vollständigen Namen des verantwortlichen Teammitglieds kennzeichnen. KI-Unterstützung muss in dessen persönlicher Prompt-Datei dokumentiert werden.
- Klassen modular halten: Weltkarte, Kollisionen, Gebäude beziehungsweise Übergänge und Interaktionen sollen klare, getrennte Aufgaben besitzen.

## Akzeptanzkriterien
Die Erweiterung ist erst fertig, wenn:

- ein neues Spiel im eigenen Haus beginnt;
- die Spielfigur das Haus verlassen und wieder betreten kann;
- die Stadt quadratisch ist und eine sichtbare, wirksame Außengrenze hat;
- Straßen und Gassen zwischen den Bereichen begehbar verbunden sind;
- Casino, Krankenhaus und Supermarkt in der Außenwelt erkennbar sind;
- die Spielfigur nicht durch Gebäude, Wände oder andere Hindernisse läuft;
- der Weg vom Zuhause zum Casino und der Übergang in das vorhandene Casino funktionieren;
- der Rückweg vom Casino in die Außenwelt funktioniert;
- vorhandene Casino-Funktionen und Framework-Abläufe erhalten bleiben.
Nach der Umsetzung: Fasse knapp zusammen, welche Dateien geändert wurden, wie der Übergang zwischen Außenwelt und Casino umgesetzt ist und welche Punkte wegen vorhandener Framework- oder Projektgrenzen offenbleiben. Prüfe bei größeren Änderungen die Kompilierbarkeit, ohne dafür verbotene Dateien anzupassen.
```

Werkzeug: Copilot SDK in VS Code
Modell: Im Chat nicht angezeigt

Ergebnisvermerk: Die Projektregeln, die tatsächlichen Framework-Interfaces sowie Welt-, Bewegungs-, Kollisions-, Eingabe-, Zeichen- und Übergangsklassen wurden untersucht. Die Regel in `Agents.md` wurde nach ausdrücklicher Freigabe um Zuhause und Außenwelt erweitert. Eine quadratische Stadt, ein begehbares Zuhause, Türübergänge zum bestehenden Casino und pixelbasierte Stadtgrafiken wurden ergänzt. `dotnet build OOPGames\OOPGames\OOPGames.csproj --no-restore` war erfolgreich (0 Warnungen, 0 Fehler); `git diff --check` ist ebenfalls erfolgreich. Die interaktive Prüfung im laufenden Spiel konnte wegen wiederholter Eingabeunterbrechungen im Spielfenster nicht abgeschlossen werden und bleibt offen.

### Prompt 4

```text
ja, haus und außenwelt als bewusste erweiterung umsetzten, und die regel im agents.md anpassen
```

Werkzeug: Copilot SDK in VS Code
Modell: Im Chat nicht angezeigt

Ergebnisvermerk: Freigabe zur bewussten Erweiterung von Abschnitt 8 der `Agents.md` erteilt; die Projektregeln wurden entsprechend aktualisiert.