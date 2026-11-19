# Prompt-Dokumentation – Elena Stempfle

Datum der dokumentierten Sitzung: 29.10.2026
Werkzeug: Copilot SDK in VS Code
Modell: In der Sitzung nicht ausgewiesen

## 1. Projektanforderungen und Rahmenbedingungen

**Datum:** 29.10.2026
**Werkzeug/Modell:** Copilot SDK in VS Code / Modell nicht ausgewiesen
**Prompt im Wortlaut:**

> wir müssen ein Spiel "Vier gewinnt" programmieren, es ist wichtig das du das vorgegebenes Framework nicht verläst ich gebe dir nochdie restlichen Anforderungen
>
> Projektanforderungen OOPGames – Kurzfassung
> Programmieren 3 · Wintersemester 2026/27 · Josef Griesbauer
> ausführliche Fassung: Projektanforderungen_OOPGames.pdf
>
> Alle Gruppen arbeiten am WPF-Framework OOPGames (C#, .NET 6). Jedes Spiel besteht aus den austauschbaren Rollen Painter (IPaintGame), Rules (IGameRules, IGameField) und Player (IHumanGamePlayer, IComputerGamePlayer). Die Kompatibilität wird über CanBePaintedBy / CanBeRuledBy immer gegen Interfaces geprüft – Muster: Beispiel X_Griesbauer.
>
> **Generelle Anforderungen**
>
> Eigene Verzeichnisse: Jede Gruppe arbeitet ausschließlich in Classes/<Gruppe>/ und Interfaces/<Gruppe>/ (analog X_Griesbauer). Dateien: Vorname_Nachname_<Inhalt>.cs; Klassen/Interfaces mit Gruppenpräfix; eindeutige Name-Properties.
>
> Außerhalb davon ist nur das Registrieren der eigenen Klassen im Block `//REGISTER YOUR CLASSES HERE` in MainWindow.xaml.cs erlaubt.
>
> Jedes Teammitglied committet und pusht selbst (eigener GitHub-Account) – die Historie zeigt die Aufteilung im Team. Kleine Commits mit aussagekräftigen Messages; vor dem Push git pull und dotnet build; kein --force, keine fremden Dateien, kein bin/obj/.
>
> Prompts dokumentieren: Für jeden Studierenden eine eigene Datei Vorname_Nachname_Prompts.md im Hauptverzeichnis der Gruppe (Classes/<Gruppe>/) mit jedem Prompt im Wortlaut, Datum, Werkzeug/Modell und kurzem Ergebnisvermerk.
>
> Pläne dokumentieren: Alle Pläne (vom Agenten erzeugt oder eigene) als Markdown in Classes/<Gruppe>/Plans/. Prompts und Pläne werden zusammen mit dem zugehörigen Code committet.
>
> Framework-Dateien sind tabu – auch für Agenten. Die Basis-Interfaces in Interfaces/, Classes/OOPGames.cs, MainWindow.* (außer Registrierung), App.*, .csproj/.sln, .vscode/, X_Griesbauer und fremde Gruppenordner werden nicht verändert. Ist eine Framework-Erweiterung nötig, MUSS sie vorab mit dem Dozenten abgeklärt werden. Der Agent ist entsprechend anzuweisen („Ändere nur Dateien in Classes/<Gruppe>/ und Interfaces/<Gruppe>/; ist eine Framework-Änderung nötig, brich ab und beschreibe sie.“); vor jedem Commit mit git status/git diff prüfen.
>
> **Anforderung 1: Vier gewinnt – kompatibel über alle Gruppen**
>
> Jede Gruppe implementiert Painter, Rules (mit Spielfeld), Human-Player und Computer-Player. Jede Kombination aus Komponenten aller Gruppen muss im Hauptfenster funktionieren.
>
> Grundlage ist ein gemeinsamer Satz Vier-gewinnt-Interfaces (z. B. Interfaces/C4_Gemeinsam/: IC4_Field, IC4_Move : IColumnMove, IC4_Rules, IC4_Painter, IC4_HumanPlayer, IC4_ComputerPlayer), von allen Gruppen gemeinsam erarbeitet (je eine Vertretung), vom Dozenten freigegeben und danach eingefroren. Teil der Vereinbarung: Feldgröße, Indexkonventionen, Zeichengeometrie (für die Klick-Umrechnung). Programmiert wird nur gegen die gemeinsamen Interfaces – nie gegen konkrete Klassen (auch nicht die eigenen).
>
> Rules weisen ungültige Züge ab und erkennen Sieg (waagerecht, senkrecht, diagonal) und Unentschieden; Human-Player liefert bei ungültiger Eingabe null; Computer-Player spielt immer gültig und mindestens „gewinnen, wenn möglich – sonst blockieren“.
>
> Nachweis: Test mit mindestens drei anderen Gruppen, dokumentiert als Kompatibilitätsmatrix; Fehler in fremden Komponenten als Issue melden, nicht selbst ändern.
>
> **Anforderung 2: Eigenes komplexes Spiel**
>
> Jede Gruppe entwickelt mit Hilfe von Agenten ein eigenes, komplexes bzw. innovatives Spiel; Kompatibilität mit anderen Gruppen ist nicht nötig. Erwartet wird deutlich mehr als ein einfacher Brettspielklon (z. B. Echtzeit über IGameRules2/IPaintGame2, starker KI-Gegner, Levels/Phasen).
>
> Spielkonzept (eine Seite in Plans/) vor Beginn dem Dozenten vorstellen.
>
> Harte Vorgabe: Das Spiel läuft vollständig im vorgegebenen Framework – Auswahl und Start über das Hauptfenster, Eingaben nur über Klick-/Tasten-Selections, Zeichnen nur auf das übergebene Canvas, kein eigenes Fenster oder eigene Hauptschleife. Framework-Erweiterungen nur nach Absprache (siehe oben). Sauberer OO-Entwurf; das Team muss den gesamten Code erklären können, auch den agentisch erzeugten.
>
> Teamarbeit nach SCRUM, dokumentiert in Classes/<Gruppe>/Scrum/ (oder GitHub Issues/Projects): Product Owner und Scrum Master benannt, Product Backlog mit User Stories, Sprints (1–2 Wochen) mit Sprintziel und Verantwortlichen je Aufgabe, Kurzprotokolle der Abstimmungen, Review und Retrospektive je Sprint. Backlog, Prompt-Dateien, Pläne und Commits müssen zusammenpassen; jedes Mitglied hat einen substanziellen, sichtbaren Anteil.
>
> Termine für Konzeptvorstellung, Zwischenstände und Abnahme werden in der Vorlesung bekanntgegeben. Bei Unklarheiten gilt: vorher fragen.

**Ergebnisvermerk:** Die Framework-Beschränkung und die Projektvorgaben wurden als Leitplanken festgehalten; zunächst wurde die vorhandene Gruppen- und Interface-Struktur geprüft.

## 2. Die wichtigsten Anforderungen

**Datum:** 29.10.2026
**Werkzeug/Modell:** Copilot SDK in VS Code / Modell nicht ausgewiesen
**Prompt im Wortlaut:**

> okay hier sind die wichtigsten anforderungen:

**Ergebnisvermerk:** Es folgten die konkretisierten Anforderungen in Prompt 3.

## 3. Framework-Nutzung, OOP und Lesbarkeit

**Datum:** 29.10.2026
**Werkzeug/Modell:** Copilot SDK in VS Code / Modell nicht ausgewiesen
**Prompt im Wortlaut:**

> 1. Du darfst das vorgegebene Framework OOP Games nicht verlassen, falls du es verlassen möchtest hör auf und beschreibe warum du aus dem Framework raus willst                                                     2. Es ist wichtig das du Objektorientiert Programmierst und den Code so strukturierst das er leserlich bleibt                3. Es ist wichtig das keine Wiederholungen vorkommen die keinen Mehrwert haben

**Ergebnisvermerk:** Die Framework-Grenze, objektorientierte und gut lesbare Struktur sowie das Vermeiden unnötiger Wiederholungen wurden bestätigt.

## 4. Fehlende Interfaces

**Datum:** 29.10.2026
**Werkzeug/Modell:** Copilot SDK in VS Code / Modell nicht ausgewiesen
**Prompt im Wortlaut:**

> welche Interfaces fehlen deiner meinung noch

**Ergebnisvermerk:** Die vorhandenen Framework-Interfaces wurden mit den noch benötigten Vier-gewinnt-Verträgen abgeglichen.

## 5. Gemeinsames kompatibles Interface

**Datum:** 29.10.2026
**Werkzeug/Modell:** Copilot SDK in VS Code / Modell nicht ausgewiesen
**Prompt im Wortlaut:**

> also muss ich dir die angaben für das interface geben, damit du ein kompatibles Inteface auf die alle zugreifen können erstellen kannst

**Ergebnisvermerk:** Es wurde erklärt, dass die gemeinsamen Signaturen und Konventionen abgestimmt und freigegeben sein müssen, bevor ein gemeinsames Interface angelegt wird.

## 6. Framework verwenden, aber nicht verändern

**Datum:** 29.10.2026
**Werkzeug/Modell:** Copilot SDK in VS Code / Modell nicht ausgewiesen
**Prompt im Wortlaut:**

> So wie ich das jezt verstanden habe darfst du mit dem framework arbeiten du darfst es nur nicht verändern oder verlassen

**Ergebnisvermerk:** Die Unterscheidung zwischen Framework verwenden und Framework-Dateien verändern/verlassen wurde bestätigt.

## 7. Neues 4GWInterface

**Datum:** 29.10.2026
**Werkzeug/Modell:** Copilot SDK in VS Code / Modell nicht ausgewiesen
**Prompt im Wortlaut:**

> das 4GWInterface ist neu dazugekommen in dem wird das Feld und das Move interface erstellt auch das darfst du nicht ausserhalb vom Framework arbeiten

**Ergebnisvermerk:** `I4GWFeld` und `I4GWMove` aus der bereitgestellten Framework-Datei wurden eingesehen und als vorgegebene Verträge berücksichtigt.

## 8. Noch benötigte Interfaces erstellen

**Datum:** 29.10.2026
**Werkzeug/Modell:** Copilot SDK in VS Code / Modell nicht ausgewiesen
**Prompt im Wortlaut:**

> kannst du mit den vorgegebenen Interface 4GWInterface die noch zu benötigen Interfaces erstellen und die dann in dem Ordner Gruppe 2 speichern

**Ergebnisvermerk:** Die Umsetzung wurde zunächst auf Basis der freigegebenen gemeinsamen Verträge eingeordnet.

## 9. Vier gewinnt implementieren

**Datum:** 29.10.2026
**Werkzeug/Modell:** Copilot SDK in VS Code / Modell nicht ausgewiesen
**Prompt im Wortlaut:**

> kannst du mit den vorgegebenen Interfaces das SPiel 4 Gewinnt implementieren, das Spilefeld hat 7 Spalten und 6 Zeilen, du darfs nicht außerhalb vom Framework arbeiten und es muss kompatibel mit den anderen Spielen der Gruppen sein oder leicht veränderbar sein

**Ergebnisvermerk:** Spielfeld, Regeln, Painter, Human- und Computer-Spieler wurden in `Classes/Gruppe2/4Gewinnt.cs` implementiert und im erlaubten Registrierungsblock registriert. Der Build war erfolgreich.

## 10. Englisch und Zeilen 55–60

**Datum:** 29.10.2026
**Werkzeug/Modell:** Copilot SDK in VS Code / Modell nicht ausgewiesen
**Prompt im Wortlaut:**

> kannst du die Sprache auf englisch stellen und dann die Zeilen 55 bis 60 löschen da diese ja nur wiederholungen sind

**Ergebnisvermerk:** Bezeichner und sichtbare Texte wurden auf Englisch umgestellt. Die betreffenden Zeilen enthielten den Move-Konstruktor und notwendige Interface-Zuweisungen und wurden deshalb beibehalten; ein überflüssiger Kommentar wurde entfernt. Der Build war erfolgreich.

## 11. Pull und Push

**Datum:** 29.10.2026
**Werkzeug/Modell:** Copilot SDK in VS Code / Modell nicht ausgewiesen
**Prompt im Wortlaut:**

> kannst du pullen und dann pushen

**Ergebnisvermerk:** Es wurde festgestellt, dass für den Push zunächst ein Commit nötig war; die vorhandenen, sachfremden Änderungen wurden nicht einbezogen.

## 12. Commit, Pull und Push

**Datum:** 29.10.2026
**Werkzeug/Modell:** Copilot SDK in VS Code / Modell nicht ausgewiesen
**Prompt im Wortlaut:**

> kannst du comitten dann pullen und dann pushen

**Ergebnisvermerk:** Die Vier-gewinnt-Änderungen wurden committet und per Rebase integriert. Der Push scheiterte zunächst an fehlenden Zugangsdaten.

## 13. Push erneut versuchen

**Datum:** 29.10.2026
**Werkzeug/Modell:** Copilot SDK in VS Code / Modell nicht ausgewiesen
**Prompt im Wortlaut:**

> kannst du nochmal pushen

**Ergebnisvermerk:** Nach zwischenzeitlichen Remote-Änderungen wurde erfolgreich rebased und anschließend gepusht; der Branch war synchron.

## 14. Prompt-Dokumentation speichern

**Datum:** 29.10.2026
**Werkzeug/Modell:** Copilot SDK in VS Code / Modell nicht ausgewiesen
**Prompt im Wortlaut:**

> kannst du die prompts in einer md datei speichern

**Ergebnisvermerk:** Diese Prompt-Dokumentation wurde unter `Classes/Gruppe2/Elena_Stempfle_Prompts.md` erstellt. Der Dateiname folgt dem vorgegebenen Schema `Vorname_Nachname_Prompts.md`.

---

# Sitzung vom 19.11.2026

Werkzeug: Claude Code (VS Code-Erweiterung)
Modell: Claude Opus 5.5 (claude-opus-5-5)

## 15. Framework-Grenze und Prompt-Dokumentation

**Datum:** 19.11.2026
**Werkzeug/Modell:** Claude Code in VS Code / Claude Opus 5.5
**Prompt im Wortlaut:**

> Das Vorgegebene Framework darfst du nicht verlassen, wenn du das Fframework verlassen willst, hört auf zu coden und frag nach, ob du die eerweiterung durchführen darfst. Zudem müssen alle prompts in der .md Elena_Stempfle_Prompts.md gespeichert werden, mit dem Prompt und den Antworten von dir

**Antwort:**

> Verstanden. Für diese Sitzung gilt:
>
> 1. Ich arbeite nur innerhalb des Frameworks OOPGames, also in `Classes/Gruppe2/` und `Interfaces/Gruppe2/` (dazu kommt nur die Registrierung im Block `//REGISTER YOUR CLASSES HERE`). Wenn eine Framework-Erweiterung nötig wäre, höre ich auf zu coden, beschreibe die Änderung und frage, ob ich sie durchführen darf.
> 2. Jeder Prompt kommt im Wortlaut zusammen mit meiner Antwort in diese Datei.
>
> Hinweis: Die bisherigen Einträge (1–14) haben kaputte Umlaute (z. B. „Ã¼“ statt „ü“). Auf Wunsch kann ich das korrigieren.

**Ergebnisvermerk:** Regeln bestätigt und dauerhaft gespeichert, Dokumentation fortgeführt. Code wurde nicht geändert.

## 16. Umlaute korrigieren und Datei verschieben

**Datum:** 19.11.2026
**Werkzeug/Modell:** Claude Code in VS Code / Claude Opus 5.5
**Prompt im Wortlaut:**

> ja ändere bitte diese zwei Bausteine

**Antwort:**

> Beides ist erledigt:
>
> 1. Die kaputten Umlaute in den Einträgen 1–14 sind korrigiert („Ã¼“ → „ü“, „â€““ → „–“ usw.). Ursache war eine doppelte Kodierung (UTF-8 wurde als Windows-1252 gelesen und erneut als UTF-8 gespeichert). Der Text selbst wurde nicht verändert.
> 2. Die Datei wurde mit `git mv` von `Classes/Gruppe2/4Gewinn/` nach `Classes/Gruppe2/Elena_Stempfle_Prompts.md` verschoben, wie es die Projektanforderungen vorgeben. Die Git-Historie bleibt so erhalten.

**Ergebnisvermerk:** Datei repariert und in das Hauptverzeichnis der Gruppe verschoben. Kein Code geändert, kein Framework-Eingriff.

## 17. Commit, Pull und Push

**Datum:** 19.11.2026
**Werkzeug/Modell:** Claude Code in VS Code / Claude Opus 5.5
**Prompt im Wortlaut:**

> kannst du die änderungen comitten und dann pullen und pushen

**Antwort:**

> Ich committe nur die Prompt-Datei (Umbenennung und Korrektur). Die fremden, geänderten bin/obj-Dateien aus anderen Gruppen kommen nicht in den Commit. Danach folgen `git pull --rebase` und `git push`.

**Ergebnisvermerk:** Commit mit der Prompt-Datei erstellt, danach Pull mit Rebase und Push.

## 18. Subway Surfer: Züge gerade auf die Schienen, nach jeder Rampe ein Zug

**Datum:** 19.11.2026
**Werkzeug/Modell:** Claude Code in VS Code / Claude Opus 5.5
**Prompt im Wortlaut:**

> Das sind die Projektanforderungen, diese Angaben muss der Code erfüllen. Wir wollen Subway Surfer implementieren, bis jetzt funktioniert die 3D Grafik noch nicht, kannst du die Grafik so ändern, dass die Züge gerade auf den Schienen sind und das nach einer Güterwagonrampe immer mindestens ein Zuzg kommt auf dem man laufen kann
>
> (Dazu die Projektanforderungen OOPGames – Kurzfassung als HTML eingefügt, siehe Prompt 1.)

**Antwort:**

> Ich habe nur Dateien in `Classes/Gruppe2/Subway Surfer/` geändert. Eine Framework-Erweiterung war nicht nötig.
>
> **Ursachen:**
> - Jeder Zugteil war ein eigenes kleines 3D-Bild (Viewport3D) mit schräger Kamera. Dieses Bild wurde zusätzlich um den „Gleiswinkel“ gedreht, sodass die Züge seitlich gekippt waren.
> - Züge, Hindernisse und Münzen hatten unterschiedliche Vorschau-Distanzen (280, 180, 100). Gleiche Entfernungen landeten dadurch an verschiedenen Bildschirmpositionen.
> - Fahrende Züge wackelten seitlich (`Math.Sin`) und verließen dabei die Schienen.
> - In der Rules-Klasse war die Reihenfolge vertauscht: Zuerst kam der Zug, die Rampe kam ganz zuletzt. Nach der Rampe folgte also nichts mehr. Außerdem gab es einzelne Rampen ohne Zug dahinter.
>
> **Änderungen:**
> 1. `Elena_Stempfle_SubwayTrainLayout.cs` (neu): Diese gemeinsame Zug-Geometrie (Wagenlänge, Rampenlänge, „Wagen unter dem Läufer“) nutzen Rules und Painter, damit Zeichnung und Kollision übereinstimmen.
> 2. Painter: Es gibt jetzt eine einzige Perspektiv-Projektion (`ScreenProjection`) für Gleise, Schwellen, Züge, Hindernisse und Läufer. Züge werden als perspektivische Quader gezeichnet: Unterboden, die zur Mitte zeigende Seitenwand, das Dach und die Front. Die Rampe ist ein Keil vom Gleis bis zur Dachhöhe. Die Lok-Front verwendet weiterhin die vorhandene Detailzeichnung. Drehung und Wackeln wurden entfernt. Der Läufer steht auf dem Zugdach jetzt genau auf Dachhöhe.
> 3. Rules: Ein Konvoi besteht immer aus Rampe → 1 bis 3 Güterwagen → Lok, und der erste Wagen beginnt genau an der Rampenoberkante. Ein einzelner Rampen-Spawn erzeugt ebenfalls einen Konvoi. Man bleibt auf dem Dach, solange ein Wagen unter einem ist. Danach geht es zurück aufs Gleis. Wer auf einem Zug landet, steht auf dem Dach. Auf Spuren mit Zug entstehen keine weiteren Hindernisse, und die sichere Spur ist nie eine Zugspur. Ein Spurwechsel seitlich in einen Zug wird blockiert; der Bahnwärter kommt dann näher.
> 4. Nicht mehr verwendeter Code wurde entfernt: alte Zug-Viewports, `RoofTicks`, Kameradrehung.
> 5. Drei neue Tests: Nach einer Rampe folgen zusammenhängende Wagen. Man bleibt bis zum letzten Wagen auf dem Dach. Ein Spurwechsel in einen Zug ist blockiert.
>
> Build: 0 Warnungen, 0 Fehler. `RunSubwayTests.ps1`: Alle Tests bestanden. Zusätzlich habe ich Spielszenen als Bild gerendert und kontrolliert: Die Züge stehen auf allen drei Spuren gerade zwischen den Schienen.

**Ergebnisvermerk:** Züge stehen perspektivisch korrekt auf den Schienen. Nach jeder Rampe folgt mindestens ein begehbarer Wagen. Die Tests sind grün. Noch nicht committet.

## 19. Commit und Pull – Konflikt mit Stefanies 3D-Szene

**Datum:** 19.11.2026
**Werkzeug/Modell:** Claude Code in VS Code / Claude Opus 5.5
**Prompt im Wortlaut:**

> kannst du die änderung comitten und dann pullen

**Antwort:**

> Beim Pull gab es einen Konflikt. Stefanie hatte zwischenzeitlich den Painter als echte 3D-Szene (Viewport3D) neu geschrieben (Commit „Add 3D Subway Surfer scene“, Plan `Plans/Stefanie_Kolb_Subway_3D_Plan.md`). Ich habe den Rebase abgebrochen, ihre Arbeit nicht überschrieben und nachgefragt, wie zusammengeführt werden soll.

**Rückfrage und Entscheidung:** „Ihre 3D-Szene + meine Rules (Empfohlen)“

**Umsetzung:**

> - Mein lokaler Commit ist als Branch `backup/elena-subway-projection` gesichert (nur lokal). `master` steht auf dem Server-Stand mit Stefanies 3D-Szene.
> - Übernommen: Zuglogik in den Rules (Rampe → 1–3 Güterwagen → Lok, Dach bis zum letzten Wagen, blockierter Spurwechsel in einen Zug), `Elena_Stempfle_SubwayTrainLayout.cs` und die neuen Tests.
> - Stefanies Painter angepasst statt ersetzt: Jedes Zugmodell wird mit `AddTrainPart` genau in den Platz eingepasst, den die Rules reservieren (zwischen den Schienen, vom vorderen bis zum hinteren Ende des Wagens). Alle Wagen haben eine gemeinsame Dachhöhe. Die Rampe ist gespiegelt und steigt jetzt vom Läufer weg an. Eine gemeinsame Funktion `WorldZ` rechnet die Entfernung in die Z-Position der 3D-Welt um, für alle Objekte gleich. Auf dem Zugdach stehen Läufer und Kamera auf Dachhöhe.
> - Stefanies Idee, Züge schon am Horizont erscheinen zu lassen, bleibt erhalten: Konvois starten bei Entfernung 420, der längste endet vor 520. Die Rampe ist für eine flachere Steigung länger (30).
> - Build: 0 Warnungen, 0 Fehler. Alle Subway-Surfer-Tests sind bestanden, die gerenderten Szenen wurden kontrolliert.
> - Hinweis: Im Painter stehen noch die alten 2D-Zeichenmethoden von Stefanie (z. B. `DrawTrack`, `DrawWorldObjects`), die nicht mehr aufgerufen werden. Ich habe sie nicht gelöscht, weil es ihr Code ist.

**Ergebnisvermerk:** Zusammengeführte Version committet und gepullt, ohne Push.
