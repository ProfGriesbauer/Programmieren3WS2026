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

## 12-11-2026

### Prompt 5

```text
## NPCs in der gesamten Welt
Verteile die vorhandenen und gegebenenfalls neue NPCs über die ganze Außenwelt. Sie sollen sich auf Wegen, Plätzen und geeigneten Bereichen bewegen, statt nur an einer Stelle zu stehen.

- NPCs laufen eigenständig in ihrem jeweiligen Bereich umher.
- Ihre Bewegung bleibt innerhalb begehbarer Flächen und respektiert Kollisionen mit Gebäuden, Wasser, Bäumen und anderen Hindernissen.
- NPCs dürfen wichtige Wege, Türen, Brücken und Übergänge nicht dauerhaft blockieren.
- Achte auf unterschiedliche, nachvollziehbare Laufwege und Pausen, damit die Welt lebendig wirkt.
- Nutze nur Eingabe-, Aktualisierungs- und Darstellungsmechanismen, die das vorhandene OOPGames-Framework tatsächlich unterstützt. Erstelle dafür keine eigene Game-Loop.
- Trenne NPC-Bewegungslogik und Darstellung. Bestehende Casino-NPCs und ihre Interaktionen sollen erhalten bleiben und – wo passend – auch außerhalb des Casinos erscheinen können.
- Falls viele NPCs die Performance oder die Framework-Architektur belasten würden, setze eine angemessene Anzahl um und erkläre die Entscheidung kurz.
Erweitere die Akzeptanzkriterien: **NPCs sind an verschiedenen Stellen der Welt verteilt, bewegen sich eigenständig innerhalb ihrer erlaubten Bereiche und können weiterhin sinnvoll angesprochen oder anderweitig interagiert werden.**
```

Werkzeug: Copilot SDK in VS Code
Modell: Im Chat nicht angezeigt

Ergebnisvermerk: Vier NPCs wurden in getrennten Stadtbezirken platziert und bewegen sich mit unterschiedlichen Pausen auf begehbaren Kacheln. Hindernisse, weitere NPCs, der Spieler und Türfelder werden bei der Routenwahl berücksichtigt. Casino-Gast Lena erscheint zusätzlich in der Stadt, und die vorhandenen Gespräche bleiben verfügbar. Der Build war erfolgreich (0 Warnungen, 0 Fehler), `git diff --check` ist sauber. Ein interaktiver Spieltest wurde nicht durchgeführt.

## 12-11-2026

### Prompt 6

```text
Erweitere unser bestehendes Casino-Spiel um eine deutlich größere, frei erkundbare Außenwelt. **Untersuche zuerst das vollständige Projekt in VS Code, besonders „Spiel 2“ in „Gruppe 1“, und orientiere dich an dessen tatsächlichem Aufbau und den dort verwendeten Lösungen.** Nutze die beigefügten Bilder als visuelle Referenz für Pixel-Art, Gebäude, Straßen, Bäume und Wasser. Kopiere keine fremden Grafiken; orientiere dich an ihrer Gestaltung und Bildsprache.

## Gewünschtes Spielerlebnis
Die Spielfigur startet im eigenen Zuhause. Sie kann das Haus verlassen, durch die Stadt laufen und das Casino zu Fuß erreichen. Die Außenwelt soll ungefähr **100-mal so groß wie die bisherige Karte** sein und zum Erkunden einladen, ähnlich im Spielgefühl klassischer Pokémon-Spiele für den Game Boy Advance.

Die Welt ist quadratisch und enthält ein zusammenhängendes Netz aus Straßen, Wegen und Gassen. Sie umfasst verschiedene Stadtbereiche und erkennbare Gebäude, mindestens:

- das Zuhause des Spielers,
- das Casino,
- ein Krankenhaus,
- einen Supermarkt,
- weitere passende Gebäude und Orte.
Mindestens das Zuhause und das Casino müssen betretbar und wieder verlassbar sein. Prüfe, ob zusätzliche Gebäude mit der bestehenden Architektur sinnvoll als betretbare Innenräume umgesetzt werden können.

## Welt, Karte und Kamera

- Baue eine große, zusammenhängende Außenkarte mit abwechslungsreichen Stadtbereichen, Wegen, Gebäuden, Bäumen und Wasserflächen.
- Die Kartengrenzen sollen natürlich durch dichte Baumreihen, Wasser oder eine Kombination aus beidem sichtbar sein. Die Spielfigur darf die Weltgrenze nicht überschreiten.
- Ein Fluss soll durch die Stadt verlaufen und die Karte sichtbar prägen. Ergänze Ufer, passende Übergänge und Brücken, wo sie für die Erkundung nötig sind.
- Setze eine mit der Spielfigur scrollende Kamera um. Sie folgt der Figur beim Laufen, sodass immer nur ein begrenzter Ausschnitt der Welt zu sehen ist.
- Zeige nicht die gesamte Karte gleichzeitig. Halte die Figur im sichtbaren Bereich und begrenze das Scrollen an den Kartenrändern, damit keine leeren Bereiche außerhalb der Welt sichtbar werden.
- Die Karte soll sich beim Erkunden aktualisieren, wenn die Spielfigur neue Bereiche erreicht. Nutze dafür die im Projekt und Framework vorhandene Karten- und Darstellungsarchitektur. Erfinde keine zusätzliche Game-Loop.

## Tiere und lebendige Umgebung
Verteile kleine, stilistisch passende Tiere über die Karte:

- Enten und sichtbare Fische in geeigneten Gewässern,
- Katzen an unterschiedlichen Stellen der Stadt,
- Vögel in passenden Bereichen, zum Beispiel bei Bäumen, auf Dächern oder entlang von Wegen.
Die Tiere sollen die Welt beleben und dürfen weder Wege blockieren noch Kollisionen oder Interaktionen unbeabsichtigt stören. Prüfe anhand der vorhandenen Framework-Möglichkeiten, ob und wie einfache Animationen zuverlässig dargestellt werden können.

## Gebäude und Innenräume
Die Gebäude sollen von außen unterscheidbar und passend in die Stadt integriert sein. Innenräume sollen wohnlich und gestaltet wirken, nicht leer oder traurig. Ergänze – soweit mit der vorhandenen Architektur umsetzbar – passende Möbel und Details wie Betten, Tische, Stühle, Schränke, Regale, Pflanzen, Lampen, Theken oder medizinische Einrichtung.

Möbel und Wände müssen sinnvoll angeordnet sein und verlässliche Kollisionen besitzen. Die Einrichtung darf wichtige Wege und Türen nicht versperren. Innen- und Außenbereiche müssen über nachvollziehbare Übergänge verbunden sein.

## Stil
Verwende eine einheitliche 2D-Pixel-Art-Gestaltung aus der Top-Down-Perspektive. Orientiere dich bei Farben, Formen und Proportionen an den beigefügten Pokémon-Rubin-Bildern: klare Pixelkonturen, lesbare Wege, charakteristische Baumkronen, markante Gebäude und deutlich erkennbare Wasserflächen. Die Bilder sind Stilreferenzen, keine Anweisung, originale Spielgrafiken zu kopieren.

## Verbindliche Analyse vor der Umsetzung

1. Lies die Projektregeln und halte sie vollständig ein.
2. Untersuche „Spiel 2“ in „Gruppe 1“ vollständig in VS Code: relevante Klassen, Welt- und Kartenaufbau, Kameraverhalten, Bewegung, Kollisionen, Gebäudeübergänge, Tiere oder Animationen und Darstellung.
3. Untersuche die entsprechenden Klassen unseres Casino-Spiels sowie die tatsächlich vorhandenen Framework-Interfaces und Beispielimplementierungen.
4. Vergleiche beide Ansätze und nutze nur Muster, die mit unserem Projekt und Framework kompatibel sind.
5. Erfinde keine Framework-Methoden, Properties, Events, Eingaben oder Canvas-Funktionen.
6. Nenne vor Änderungen kurz die betroffenen eigenen Dateien und die geplante Aufgabenverteilung der Klassen.
7. Stelle sicher, dass der vollständige Name des verantwortlichen Teammitglieds bekannt ist. Wenn er nicht aus dem Projekt hervorgeht, frage danach, bevor du Code erstellst oder änderst.

## Technische und organisatorische Grenzen

- Das Spiel muss vollständig als reguläre Komponente des vorhandenen OOPGames-Frameworks laufen.
- Erstelle keine eigene Main-Methode, Game-Loop, Fenster, Zeichenfläche oder Framework-Initialisierung.
- Zeichne ausschließlich auf dem vom Framework übergebenen Canvas und verarbeite Eingaben nur über nachweislich vorhandene Framework-Schnittstellen.
- Verändere keine Framework-Dateien, Basis-Interfaces, Projektdateien oder fremden Gruppenordner.
- Änderungen sind nur in `Classes/<eigene Gruppe>/` und `Interfaces/<eigene Gruppe>/` zulässig. Die einzige mögliche Ausnahme ist die vorgesehene Klassenregistrierung innerhalb des ausdrücklich markierten Registrierungsblocks in `MainWindow.xaml.cs`; dort darf ausschließlich registriert werden.
- Erhalte bestehende Casino-Funktionen und ersetze vorhandene Lösungen nicht unnötig.
- Trenne Darstellung, Spiellogik, Kartendaten, Kollisionen, Kamera, Übergänge und Interaktionen nachvollziehbar voneinander.
- Grafiken müssen austauschbar bleiben und dürfen nicht fest in die Spiellogik eingebaut werden.
- Lege keine Grafiken oder sonstigen Dateien in nicht freigegebenen Verzeichnissen ab. Falls Assets dort nicht zulässig sind, verwende eine projektkonforme Darstellung oder beschreibe die offene Abhängigkeit.
- Kennzeichne neue und wesentlich geänderte Klassen und Methoden mit dem vollständigen Namen des verantwortlichen Teammitglieds. Dokumentiere KI-Unterstützung in dessen persönlicher Prompt-Datei.

## Akzeptanzkriterien
Die Erweiterung ist fertig, wenn:

- ein neues Spiel im eigenen Zuhause startet;
- die Spielfigur das Haus verlassen und wieder betreten kann;
- die quadratische Außenwelt gegenüber der bisherigen Karte deutlich vergrößert und erkundbar ist;
- die Kamera der Spielfigur folgt und an den Kartenrändern korrekt stoppt;
- immer nur der Bereich um die Spielfigur sichtbar ist, nicht die gesamte Welt;
- Wege und Gassen wichtige Bereiche miteinander verbinden;
- der Fluss durch die Stadt verläuft und mit passenden Ufern sowie nötigen Brücken gestaltet ist;
- Baum- und Wassergrenzen die Karte sichtbar abschließen und nicht überschritten werden können;
- Casino, Krankenhaus und Supermarkt in der Stadt erkennbar sind;
- Enten und Fische im oder am Wasser sowie Katzen und Vögel an passenden Stellen vorkommen;
- Gebäudeinnenräume, soweit umgesetzt, mit passenden Möbeln gestaltet sind;
- Kollisionen und Übergänge zuverlässig funktionieren;
- der Weg vom Zuhause zum Casino und zurück in die Außenwelt funktioniert;
- vorhandene Casino-Funktionen und Framework-Abläufe erhalten bleiben.
Fasse nach der Umsetzung knapp zusammen, welche Dateien geändert wurden, wie Karte, Kamera, Fluss und Gebäudeübergänge umgesetzt sind und welche Punkte wegen der vorhandenen Framework- oder Projektgrenzen offenbleiben. Prüfe bei größeren Änderungen die Kompilierbarkeit, ohne dafür verbotene Dateien anzupassen.
```

Werkzeug: Copilot SDK in VS Code
Modell: Im Chat nicht angezeigt

Ergebnisvermerk: Die Außenwelt wurde auf 180x180 Kacheln (100-fache Kartenfläche) erweitert; sie enthält einen Fluss, Ufer, Brücken, Straßen, Baumgrenze und eine an den Kartenrändern begrenzte Scrollkamera. Krankenhaus und Supermarkt erhielten möblierte Innenräume mit Tür-Rückwegen. Enten, Fische, Katzen und Vögel werden ohne Kollisionen dargestellt und über Framework-Ticks animiert. Stadtbewohner patrouillieren weiterhin in getrennten Bezirken. Die Framework- und Projektdateien blieben unverändert. `dotnet build OOPGames\OOPGames\OOPGames.csproj --no-restore` war erfolgreich (0 Warnungen, 0 Fehler); `git diff --check` war sauber. Ein visueller Laufzeittest wurde nicht durchgeführt.