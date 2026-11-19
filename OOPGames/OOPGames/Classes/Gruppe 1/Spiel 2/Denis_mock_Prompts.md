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
## 19-11-2026

### Prompt 7

Werkzeug: Copilot SDK in VS Code
Modell: Im Chat nicht angezeigt

`	ext
# Aufgabe
Implementiere die folgenden Änderungen in unserem bestehenden Casino-Spiel.

**Wichtig:** Arbeite ausschließlich auf Basis der bestehenden Codebasis und erweitere vorhandene Systeme, sofern diese bereits existieren. Erstelle keine parallelen Systeme für Funktionen, die bereits vorhanden sind.

## 0. Gesamten bisherigen Änderungsverlauf prüfen und dokumentieren
Bevor du Änderungen implementierst, gehe **alle bisherigen Anforderungen und Änderungen dieses Projekts** durch.

Insbesondere müssen auch die vorherigen Anforderungen berücksichtigt werden, darunter:

- Parkpavillon / Stadtpavillon und Layering der Stützen
- Krankenhaus und Hinweiszettel
- Angler-NPC
- Angel-Spiel und Fischarten
- Handy
- Speichern über das Handy
- E-/Enter-Interaktion
- Geldhai
- Kreditsystem
- 50 % Tageszinsen
- Schulden
- Respektverlust
- SMS-System
- Audioeinstellungen
- Lautstärkeregelung
- Mute-Funktion
- Vollbilddarstellung der Casino-Spiele
- alle sonstigen bereits im Projekt umgesetzten Anforderungen

### `Denis_mock_Prompt.md`
Suche im Projekt nach der Datei:

`Denis_mock_Prompt.md`

Falls sie vorhanden ist:

1. Lies die komplette Datei.
2. Gehe anschließend **alle bisherigen Anforderungen und Änderungen** durch, die in diesem Projekt bisher an das Spiel gestellt wurden.
3. Vergleiche diese mit dem aktuellen Inhalt von `Denis_mock_Prompt.md`.
4. Identifiziere fehlende Anforderungen.
5. Ergänze alle fehlenden Anforderungen vollständig und strukturiert.
6. Behalte die bereits vorhandenen Inhalte bei.
7. Erstelle keine Duplikate.
8. Ergänze auch die neuen Anforderungen dieses Prompts.
9. Die Datei soll anschließend als vollständige Dokumentation des bisherigen Änderungsverlaufs dienen.
Falls `Denis_mock_Prompt.md` an einer anderen Stelle im Repository liegt, finde die vorhandene Datei und bearbeite diese.

**Wichtig:** Nicht nur die Anforderungen 15–20 dokumentieren. Der komplette bisherige Änderungsverlauf soll berücksichtigt werden.

---

# 21. Angler-NPC – vollständige Fischarten als Scroll-Menü
Der Angler-NPC funktioniert grundsätzlich bereits, aktuell werden bei der Übersicht der Fischarten jedoch nur **4 von insgesamt 20 Fischen gleichzeitig angezeigt**.

Das soll verbessert werden.

## Anforderungen
Erstelle für die Fischarten eine **scrollbare Liste / ein Scroll-Menü**.

Die Anzeige soll:

- alle vorhandenen Fischarten enthalten,
- aktuell also alle **20 Fischarten** anzeigen,
- weiterhin innerhalb des bestehenden Dialog-/NPC-UI-Systems funktionieren,
- nicht mehr auf die momentan sichtbaren 4 Einträge beschränkt sein.
Der Spieler soll durch die Liste scrollen können.

Beispielsweise:

```
Fischarten
────────────────────
> Forelle
  Karpfen
  Hecht
  ...
────────────────────
↑ / ↓ Scrollen
```
Die genaue Darstellung soll sich an das vorhandene UI des Spiels anpassen.

### Daten
Die Fischarten dürfen **nicht doppelt im Code gepflegt werden**.

Verwende die bereits vorhandene Fischdatenquelle.

Wenn zukünftig beispielsweise 21 statt 20 Fischarten existieren, soll die Liste automatisch auch den zusätzlichen Fisch anzeigen.

Für jeden Fisch sollen weiterhin die bereits vorgesehenen Informationen angezeigt werden, beispielsweise:

- Name
- Seltenheit
- Wert
- Verwendung
- sonstige vorhandene Eigenschaften

### Wichtig
Keine harte Begrenzung auf 20 Einträge programmieren.

Die Liste soll dynamisch aus den tatsächlich vorhandenen Fischdaten erzeugt werden.

---

# 22. Geldhai – Konsequenz bei Nichtzahlung
Erweitere das bestehende Geldhai-System.

Der Geldhai soll den Spieler **nicht sofort angreifen**, wenn Schulden bestehen.

Stattdessen soll folgende Sequenz entstehen:

## Ablauf
Wenn der Spieler seine Schulden beim Geldhai nicht begleicht und die entsprechenden Bedingungen erfüllt sind:

1. Der Spieler befindet sich zunächst weiterhin normal im Haus.
2. Der Geldhai wartet vor dem Haus.
3. Der Spieler verlässt das Haus.
4. Beim Verlassen des Hauses wird die entsprechende Begegnung ausgelöst.
5. Der Geldhai steht vor der Tür.
6. Es kommt zu einer kurzen Szene/Interaktion.
7. Der Geldhai bricht dem Spieler im Rahmen der Spielwelt die Knochen.
8. Danach wird der Spieler nicht im Haus weitergespielt, sondern ins Krankenhaus gebracht.
9. Der Spieler wacht im Krankenhaus auf.

## Inszenierung
Die Szene soll nicht unnötig grafisch oder detailliert dargestellt werden.

Es reicht beispielsweise:

- Geldhai vor der Tür
- kurzer Dialog
- Bildschirm-/Soundeffekt oder kurze Szene
- Übergang zum Krankenhaus
- Spieler wacht dort auf
Beispielhafte Dialogrichtung:

> „Ich hab dich lange genug warten lassen.“
Danach kann die Szene ausgeblendet werden.

### Krankenhaus
Nach der Begegnung soll der Spieler:

- im Krankenhaus erscheinen,
- dort wieder die Kontrolle erhalten,
- entsprechend der bestehenden Spielmechanik weiter spielen können.
Falls das Spiel bereits eine Krankenhaus-/Teleport-/Respawn-Mechanik besitzt, soll diese wiederverwendet werden.

### Wichtig
Die Begegnung darf **nur beim tatsächlichen Verlassen des Hauses** ausgelöst werden.

Nicht:

- beim Betreten des Hauses,
- beim Öffnen des Handys,
- zufällig irgendwo auf der Karte,
- sofort beim Überschreiten eines Tageswechsels.
Der Geldhai soll also gezielt vor dem Haus auf den Spieler warten.

---

# 23. Neuer NPC „Eduard“
Füge einen neuen NPC namens:

**Eduard**

hinzu.

## Position
Eduard steht im Casino **in der Nähe der Slotmaschinen**.

Er soll dort dauerhaft als NPC anzutreffen sein.

## Charakter
Eduard ist:

- stark spielsüchtig,
- ein typischer Dummschwätzer,
- unbeliebt,
- unzuverlässig,
- ständig am Reden,
- überzeugt davon, dass er beim nächsten Spiel garantiert gewinnt,
- offensichtlich nicht besonders kompetent,
- häufig zu spät.
Er soll dadurch einen deutlich anderen Charakter als die anderen NPCs bekommen.

## Dialogstil
Eduard soll nicht wie ein normaler freundlicher Tutorial-NPC sprechen.

Seine Dialoge sollen bewusst etwas nervig, großspurig und unsinnig wirken.

Beispielsweise in dieser Richtung:

> „Also pass auf, ich hab da ein System. Das funktioniert immer. Na ja, fast immer. Außer gestern. Und vorgestern. Aber heute ganz sicher.“
Oder:

> „Ich wäre ja pünktlich gewesen, aber die Slotmaschine hier hat mich aufgehalten.“
Oder:

> „Die anderen sagen, ich wäre unbeliebt. Das stimmt überhaupt nicht. Die mögen mich nur nicht.“
Die konkreten Dialoge sollen zum bestehenden Schreibstil des Spiels passen.

### Spielsucht
Eduard soll deutlich machen, dass er ständig an den Slotmaschinen spielt und kaum davon loskommt.

Seine Dialoge können sich deshalb regelmäßig auf folgende Themen beziehen:

- nächste Runde
- Gewinnchancen
- verlorene Chips
- angebliche Strategien
- „Glück“
- seine angeblich sichere Gewinnmethode
- verspätetes Erscheinen
- Ausreden
Er soll dabei **nicht als kompetenter Ratgeber** dargestellt werden.

---

# 24. Eduard – Unzuverlässigkeit
Eduard soll zusätzlich als **unzuverlässigster NPC der Spielwelt** charakterisiert werden.

Falls das bestehende NPC-System Zeitpläne, Termine, Tagesabläufe oder NPC-Zeitpunkte unterstützt, soll Eduard dieses System entsprechend nutzen.

Beispiele:

- Er erscheint zu spät.
- Er hält sich nicht zuverlässig an seine geplanten Zeiten.
- Er kann bei bestimmten Ereignissen verspätet auftauchen.
- Andere NPCs können sich über seine Unzuverlässigkeit äußern.
Falls es noch kein geeignetes NPC-Zeitplansystem gibt, soll nicht unnötig ein komplexes neues System gebaut werden. Die Eigenschaft kann zunächst über seinen Dialog und seine vorhandenen NPC-Mechaniken umgesetzt werden.

---

# 25. Stadt-/Parkpavillon – Dach und NPC-Layering korrigieren
Die bisherige Änderung am Pavillon hat teilweise funktioniert:

Die beiden hinteren Beine/Stützen befinden sich inzwischen korrekt **hinter dem NPC**.

Dabei wurde jedoch ein neues Problem erzeugt:

Das **Dach befindet sich ebenfalls hinter dem NPC**.

Wenn der Spieler/NPC durch den Pavillon läuft, wird dadurch die NPC-Grafik vor dem Dach dargestellt.

Das soll korrigiert werden.

## Gewünschte Layer-Reihenfolge
Die Darstellung soll sinngemäß so funktionieren:

```
Hintergrund
↓
hintere Stützen
↓
Spieler/NPC
↓
Dach / vordere Pavillon-Elemente
```
Das Dach soll also weiterhin **vor dem NPC** liegen, während die beiden hinteren Stützen **hinter dem NPC** liegen.

### Wichtig
Nicht einfach den gesamten Pavillon auf einen einzigen Layer verschieben.

Falls notwendig:

- Dach,
- hintere Stützen,
- vordere Stützen,
- sonstige Pavillon-Bestandteile
getrennt sortieren.

Verwende das bereits vorhandene Rendering-/Sorting-System des Spiels.

Die Korrektur darf keine anderen Gebäude oder Objekte der Welt beeinflussen.

---

# 26. Vollbildmodus – Casino-Spiele korrekt zentrieren
Bei den Casino-Spielen gibt es im Vollbildmodus aktuell ein Darstellungsproblem.

Das gesamte Spiel bzw. Spielfeld ist **etwas nach links verschoben**.

Korrigiere die Positionierung so, dass das jeweilige Casino-Spiel im Vollbildmodus **exakt horizontal und vertikal zentriert** dargestellt wird.

## Wichtig
Nicht nur ein einzelnes Spiel korrigieren.

Kontrolliere **alle vorhandenen Casino-Spiele**.

Für jedes Spiel soll geprüft werden:

- horizontale Zentrierung
- vertikale Zentrierung
- Spielfeldgröße
- UI-Position
- Kartenposition
- Buttons
- Texte
- Anzeigen
- Spielbereich
- Hintergrund
- sonstige HUD-Elemente

---

# 27. Kartenposition im Vollbild korrigieren
Bei den Kartenspielen befinden sich die Karten im Vollbildmodus aktuell **zu weit unten**.

Passe die Position der Karten an das tatsächliche Fenster bzw. Spielfeld an.

Die Karten sollen:

- im Vollbild korrekt vertikal positioniert sein,
- nicht unnötig weit unten liegen,
- bei unterschiedlichen Auflösungen korrekt positioniert werden,
- weiterhin im Fenstermodus funktionieren.
Keine feste Pixelposition verwenden, wenn das bestehende UI-System eine relative Positionierung ermöglicht.

Die Position soll sich an der tatsächlichen verfügbaren Fenster-/Spielfläche orientieren.

---

# 28. Alle Casino-Spiele im Vollbild systematisch überprüfen
Führe nach der Korrektur einen vollständigen Check aller Casino-Spiele durch.

Nicht nur die aktuell bekannten Fehler beheben.

Für jedes Spiel prüfen:

### Layout

- Ist das Spielfeld horizontal zentriert?
- Ist es vertikal korrekt positioniert?
- Wird der verfügbare Platz sinnvoll genutzt?
- Gibt es unerwartete Abstände?

### UI

- Sind Buttons korrekt positioniert?
- Sind Texte sichtbar?
- Sind Anzeigen nicht abgeschnitten?
- Sind Elemente nicht außerhalb des Bildschirms?

### Spielobjekte

- Karten
- Chips
- Spielfiguren
- Roulette-Elemente
- Slotmaschinen
- sonstige spielabhängige Elemente
Alle müssen im Vollbild korrekt positioniert werden.

### Unterschiedliche Auflösungen
Das Layout soll nicht ausschließlich für eine einzelne Bildschirmauflösung funktionieren.

Vermeide nach Möglichkeit harte absolute Pixelwerte für das gesamte Spielfeld.

Nutze vorhandene:

- Anchors
- Container
- Canvas-Systeme
- Relative Positionierung
- Skalierung
- bestehende UI-Layout-Systeme

---

# 29. Regressionstest der bisherigen Änderungen
Nach der Implementierung müssen die bisherigen Systeme erneut überprüft werden.

Insbesondere:

### Angler

- Angler existiert weiterhin.
- Dialog funktioniert.
- Alle Fischarten werden angezeigt.
- Fischliste ist scrollbar.
- Liste wird dynamisch aus den Fischdaten erzeugt.

### Geldhai

- Kredite funktionieren.
- höhere Kreditsummen funktionieren.
- 50 % Tageszinsen funktionieren.
- Zinsen werden pro Tag nur einmal berechnet.
- Rückzahlung funktioniert.
- Respektverlust funktioniert.
- SMS funktionieren.
- Geldhai wartet bei entsprechender Konsequenz vor dem Haus.
- Begegnung wird erst beim Verlassen des Hauses ausgelöst.
- Spieler landet anschließend im Krankenhaus.

### Handy

- Speichern funktioniert.
- E und Enter verhalten sich weiterhin wie vorgesehen.
- Spiel wird nicht mehr versehentlich über Enter geschlossen.
- Audioeinstellungen funktionieren.
- Mute funktioniert.
- Einstellungen werden gespeichert.

### Pavillon

- Hintere Stützen liegen hinter dem Spieler.
- Dach liegt vor dem Spieler/NPC.
- Keine falsche Layer-Reihenfolge.
- Keine Nebenwirkungen auf andere Objekte.

### Eduard

- NPC existiert.
- Position bei den Slotmaschinen stimmt.
- Dialog funktioniert.
- Charakter entspricht der beschriebenen Persönlichkeit.
- Eduard wirkt unzuverlässig.
- Spielsucht wird durch seine Dialoge/Verhalten erkennbar.

### Vollbild

- Alle Casino-Spiele geprüft.
- Alle Spiele horizontal zentriert.
- Alle Spiele vertikal korrekt positioniert.
- Karten korrekt positioniert.
- UI vollständig sichtbar.
- Unterschiedliche Fenstergrößen funktionieren.
- Fenstermodus funktioniert weiterhin.

---

# 30. Technische Vorgehensweise
Arbeite in dieser Reihenfolge:

1. Bestehende Projektstruktur analysieren.
2. `Denis_mock_Prompt.md` suchen und vollständig lesen.
3. Alle bisherigen Anforderungen aus der Datei und aus dem bisherigen Projektverlauf erfassen.
4. Fehlende Anforderungen in `Denis_mock_Prompt.md` ergänzen.
5. Bestehende NPC-, Dialog-, Fisch-, Handy-, Save-, Audio-, Geldhai- und UI-Systeme identifizieren.
6. Angler-Scrollmenü implementieren.
7. Geldhai-Begegnung implementieren.
8. Eduard implementieren.
9. Pavillon-Layering korrigieren.
10. Vollbildsystem aller Casino-Spiele analysieren.
11. Zentrierung und Kartenposition korrigieren.
12. Alle Casino-Spiele einzeln testen.
13. Regressionstest der bisherigen Funktionen durchführen.
14. `Denis_mock_Prompt.md` abschließend erneut mit dem tatsächlichen Implementierungsstand abgleichen.
15. Fehlende Änderungen in der Datei ergänzen.

---

# 31. Anforderungen an die Codequalität

- Bestehende Architektur verwenden.
- Keine unnötigen parallelen Systeme erstellen.
- Keine funktionierenden Systeme ohne Grund neu schreiben.
- Bestehende Datenquellen wiederverwenden.
- Neue Funktionen sauber in bestehende Klassen/Strukturen integrieren.
- Keine hart codierten Fischlisten.
- Keine hart codierten Vollbildpositionen, wenn relative UI-Positionierung möglich ist.
- Alte Spielstände müssen weiterhin geladen werden können.
- Keine Regressionen bei bestehenden Casino-Spielen.
- Keine Regressionen bei Handy, NPCs, Dialogen oder Save-System.

---

# 32. Abschlussbericht
Nach Abschluss der Implementierung gib eine kurze Übersicht aus:

### Geänderte Dateien
Liste aller geänderten Dateien.

### Neue Dateien
Liste aller neu erstellten Dateien.

### Implementierte Funktionen
Kurze Beschreibung der umgesetzten Änderungen.

### `Denis_mock_Prompt.md`
Bestätige, dass die Datei geprüft und um fehlende bisherige Anforderungen ergänzt wurde.

### Tests
Liste der durchgeführten Tests.

### Offene Probleme
Falls etwas nicht vollständig umgesetzt werden konnte, genau angeben, was fehlt und warum.

**Wichtig: Nicht nur beschreiben, wie die Änderungen umgesetzt werden könnten. Die Änderungen sollen tatsächlich in der bestehenden Codebasis implementiert werden.**
```

Ergebnisvermerk: Promptpaket 21 bis 32 wurde vollständig dokumentiert und geprüft. Umgesetzt wurden die dynamische Fischliste, Eduard bei den Spielautomaten, die Haustür-Begegnung beim tatsächlichen Verlassen des Hauses mit Krankenhaus-Respawn sowie separate Pavillon-Dach-/Stützen-Layer und eine mittig zentrierte virtuelle Spielfläche. Das Projekt baut mit 0 Warnungen und 0 Fehlern. Der externe Selbsttest meldete 84 von 85 ausführbaren Prüfungen bestanden; der externe Datei-Existenztest für G1CASINO_SOUNDS war nicht ausführbar. Die tatsächliche UI- und Auflösungsprüfung im Hauptfenster bleibt offen.
