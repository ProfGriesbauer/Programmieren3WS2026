# Prompt-Dokumentation â€“ Elena Stempfle

Datum der dokumentierten Sitzung: 29.10.2026
Werkzeug: Copilot SDK in VS Code
Modell: In der Sitzung nicht ausgewiesen

## 1. Projektanforderungen und Rahmenbedingungen

**Datum:** 29.10.2026
**Werkzeug/Modell:** Copilot SDK in VS Code / Modell nicht ausgewiesen
**Prompt im Wortlaut:**

> wir mÃ¼ssen ein Spiel "Vier gewinnt" programmieren, es ist wichtig das du das vorgegebenes Framework nicht verlÃ¤st ich gebe dir nochdie restlichen Anforderungen
>
> Projektanforderungen OOPGames â€“ Kurzfassung
> Programmieren 3 Â· Wintersemester 2026/27 Â· Josef Griesbauer
> ausfÃ¼hrliche Fassung: Projektanforderungen_OOPGames.pdf
>
> Alle Gruppen arbeiten am WPF-Framework OOPGames (C#, .NET 6). Jedes Spiel besteht aus den austauschbaren Rollen Painter (IPaintGame), Rules (IGameRules, IGameField) und Player (IHumanGamePlayer, IComputerGamePlayer). Die KompatibilitÃ¤t wird Ã¼ber CanBePaintedBy / CanBeRuledBy immer gegen Interfaces geprÃ¼ft â€“ Muster: Beispiel X_Griesbauer.
>
> **Generelle Anforderungen**
>
> Eigene Verzeichnisse: Jede Gruppe arbeitet ausschlieÃŸlich in Classes/<Gruppe>/ und Interfaces/<Gruppe>/ (analog X_Griesbauer). Dateien: Vorname_Nachname_<Inhalt>.cs; Klassen/Interfaces mit GruppenprÃ¤fix; eindeutige Name-Properties.
>
> AuÃŸerhalb davon ist nur das Registrieren der eigenen Klassen im Block `//REGISTER YOUR CLASSES HERE` in MainWindow.xaml.cs erlaubt.
>
> Jedes Teammitglied committet und pusht selbst (eigener GitHub-Account) â€“ die Historie zeigt die Aufteilung im Team. Kleine Commits mit aussagekrÃ¤ftigen Messages; vor dem Push git pull und dotnet build; kein --force, keine fremden Dateien, kein bin/obj/.
>
> Prompts dokumentieren: FÃ¼r jeden Studierenden eine eigene Datei Vorname_Nachname_Prompts.md im Hauptverzeichnis der Gruppe (Classes/<Gruppe>/) mit jedem Prompt im Wortlaut, Datum, Werkzeug/Modell und kurzem Ergebnisvermerk.
>
> PlÃ¤ne dokumentieren: Alle PlÃ¤ne (vom Agenten erzeugt oder eigene) als Markdown in Classes/<Gruppe>/Plans/. Prompts und PlÃ¤ne werden zusammen mit dem zugehÃ¶rigen Code committet.
>
> Framework-Dateien sind tabu â€“ auch fÃ¼r Agenten. Die Basis-Interfaces in Interfaces/, Classes/OOPGames.cs, MainWindow.* (auÃŸer Registrierung), App.*, .csproj/.sln, .vscode/, X_Griesbauer und fremde Gruppenordner werden nicht verÃ¤ndert. Ist eine Framework-Erweiterung nÃ¶tig, MUSS sie vorab mit dem Dozenten abgeklÃ¤rt werden. Der Agent ist entsprechend anzuweisen (â€žÃ„ndere nur Dateien in Classes/<Gruppe>/ und Interfaces/<Gruppe>/; ist eine Framework-Ã„nderung nÃ¶tig, brich ab und beschreibe sie.â€œ); vor jedem Commit mit git status/git diff prÃ¼fen.
>
> **Anforderung 1: Vier gewinnt â€“ kompatibel Ã¼ber alle Gruppen**
>
> Jede Gruppe implementiert Painter, Rules (mit Spielfeld), Human-Player und Computer-Player. Jede Kombination aus Komponenten aller Gruppen muss im Hauptfenster funktionieren.
>
> Grundlage ist ein gemeinsamer Satz Vier-gewinnt-Interfaces (z. B. Interfaces/C4_Gemeinsam/: IC4_Field, IC4_Move : IColumnMove, IC4_Rules, IC4_Painter, IC4_HumanPlayer, IC4_ComputerPlayer), von allen Gruppen gemeinsam erarbeitet (je eine Vertretung), vom Dozenten freigegeben und danach eingefroren. Teil der Vereinbarung: FeldgrÃ¶ÃŸe, Indexkonventionen, Zeichengeometrie (fÃ¼r die Klick-Umrechnung). Programmiert wird nur gegen die gemeinsamen Interfaces â€“ nie gegen konkrete Klassen (auch nicht die eigenen).
>
> Rules weisen ungÃ¼ltige ZÃ¼ge ab und erkennen Sieg (waagerecht, senkrecht, diagonal) und Unentschieden; Human-Player liefert bei ungÃ¼ltiger Eingabe null; Computer-Player spielt immer gÃ¼ltig und mindestens â€žgewinnen, wenn mÃ¶glich â€“ sonst blockierenâ€œ.
>
> Nachweis: Test mit mindestens drei anderen Gruppen, dokumentiert als KompatibilitÃ¤tsmatrix; Fehler in fremden Komponenten als Issue melden, nicht selbst Ã¤ndern.
>
> **Anforderung 2: Eigenes komplexes Spiel**
>
> Jede Gruppe entwickelt mit Hilfe von Agenten ein eigenes, komplexes bzw. innovatives Spiel; KompatibilitÃ¤t mit anderen Gruppen ist nicht nÃ¶tig. Erwartet wird deutlich mehr als ein einfacher Brettspielklon (z. B. Echtzeit Ã¼ber IGameRules2/IPaintGame2, starker KI-Gegner, Levels/Phasen).
>
> Spielkonzept (eine Seite in Plans/) vor Beginn dem Dozenten vorstellen.
>
> Harte Vorgabe: Das Spiel lÃ¤uft vollstÃ¤ndig im vorgegebenen Framework â€“ Auswahl und Start Ã¼ber das Hauptfenster, Eingaben nur Ã¼ber Klick-/Tasten-Selections, Zeichnen nur auf das Ã¼bergebene Canvas, kein eigenes Fenster oder eigene Hauptschleife. Framework-Erweiterungen nur nach Absprache (siehe oben). Sauberer OO-Entwurf; das Team muss den gesamten Code erklÃ¤ren kÃ¶nnen, auch den agentisch erzeugten.
>
> Teamarbeit nach SCRUM, dokumentiert in Classes/<Gruppe>/Scrum/ (oder GitHub Issues/Projects): Product Owner und Scrum Master benannt, Product Backlog mit User Stories, Sprints (1â€“2 Wochen) mit Sprintziel und Verantwortlichen je Aufgabe, Kurzprotokolle der Abstimmungen, Review und Retrospektive je Sprint. Backlog, Prompt-Dateien, PlÃ¤ne und Commits mÃ¼ssen zusammenpassen; jedes Mitglied hat einen substanziellen, sichtbaren Anteil.
>
> Termine fÃ¼r Konzeptvorstellung, ZwischenstÃ¤nde und Abnahme werden in der Vorlesung bekanntgegeben. Bei Unklarheiten gilt: vorher fragen.

**Ergebnisvermerk:** Die Framework-BeschrÃ¤nkung und die Projektvorgaben wurden als Leitplanken festgehalten; zunÃ¤chst wurde die vorhandene Gruppen- und Interface-Struktur geprÃ¼ft.

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

> 1. Du darfst das vorgegebene Framework OOP Games nicht verlassen, falls du es verlassen mÃ¶chtest hÃ¶r auf und beschreibe warum du aus dem Framework raus willst                                                     2. Es ist wichtig das du Objektorientiert Programmierst und den Code so strukturierst das er leserlich bleibt                3. Es ist wichtig das keine Wiederholungen vorkommen die keinen Mehrwert haben

**Ergebnisvermerk:** Die Framework-Grenze, objektorientierte und gut lesbare Struktur sowie das Vermeiden unnÃ¶tiger Wiederholungen wurden bestÃ¤tigt.

## 4. Fehlende Interfaces

**Datum:** 29.10.2026
**Werkzeug/Modell:** Copilot SDK in VS Code / Modell nicht ausgewiesen
**Prompt im Wortlaut:**

> welche Interfaces fehlen deiner meinung noch

**Ergebnisvermerk:** Die vorhandenen Framework-Interfaces wurden mit den noch benÃ¶tigten Vier-gewinnt-VertrÃ¤gen abgeglichen.

## 5. Gemeinsames kompatibles Interface

**Datum:** 29.10.2026
**Werkzeug/Modell:** Copilot SDK in VS Code / Modell nicht ausgewiesen
**Prompt im Wortlaut:**

> also muss ich dir die angaben fÃ¼r das interface geben, damit du ein kompatibles Inteface auf die alle zugreifen kÃ¶nnen erstellen kannst

**Ergebnisvermerk:** Es wurde erklÃ¤rt, dass die gemeinsamen Signaturen und Konventionen abgestimmt und freigegeben sein mÃ¼ssen, bevor ein gemeinsames Interface angelegt wird.

## 6. Framework verwenden, aber nicht verÃ¤ndern

**Datum:** 29.10.2026
**Werkzeug/Modell:** Copilot SDK in VS Code / Modell nicht ausgewiesen
**Prompt im Wortlaut:**

> So wie ich das jezt verstanden habe darfst du mit dem framework arbeiten du darfst es nur nicht verÃ¤ndern oder verlassen

**Ergebnisvermerk:** Die Unterscheidung zwischen Framework verwenden und Framework-Dateien verÃ¤ndern/verlassen wurde bestÃ¤tigt.

## 7. Neues 4GWInterface

**Datum:** 29.10.2026
**Werkzeug/Modell:** Copilot SDK in VS Code / Modell nicht ausgewiesen
**Prompt im Wortlaut:**

> das 4GWInterface ist neu dazugekommen in dem wird das Feld und das Move interface erstellt auch das darfst du nicht ausserhalb vom Framework arbeiten

**Ergebnisvermerk:** `I4GWFeld` und `I4GWMove` aus der bereitgestellten Framework-Datei wurden eingesehen und als vorgegebene VertrÃ¤ge berÃ¼cksichtigt.

## 8. Noch benÃ¶tigte Interfaces erstellen

**Datum:** 29.10.2026
**Werkzeug/Modell:** Copilot SDK in VS Code / Modell nicht ausgewiesen
**Prompt im Wortlaut:**

> kannst du mit den vorgegebenen Interface 4GWInterface die noch zu benÃ¶tigen Interfaces erstellen und die dann in dem Ordner Gruppe 2 speichern

**Ergebnisvermerk:** Die Umsetzung wurde zunÃ¤chst auf Basis der freigegebenen gemeinsamen VertrÃ¤ge eingeordnet.

## 9. Vier gewinnt implementieren

**Datum:** 29.10.2026
**Werkzeug/Modell:** Copilot SDK in VS Code / Modell nicht ausgewiesen
**Prompt im Wortlaut:**

> kannst du mit den vorgegebenen Interfaces das SPiel 4 Gewinnt implementieren, das Spilefeld hat 7 Spalten und 6 Zeilen, du darfs nicht auÃŸerhalb vom Framework arbeiten und es muss kompatibel mit den anderen Spielen der Gruppen sein oder leicht verÃ¤nderbar sein

**Ergebnisvermerk:** Spielfeld, Regeln, Painter, Human- und Computer-Spieler wurden in `Classes/Gruppe2/4Gewinnt.cs` implementiert und im erlaubten Registrierungsblock registriert. Der Build war erfolgreich.

## 10. Englisch und Zeilen 55â€“60

**Datum:** 29.10.2026
**Werkzeug/Modell:** Copilot SDK in VS Code / Modell nicht ausgewiesen
**Prompt im Wortlaut:**

> kannst du die Sprache auf englisch stellen und dann die Zeilen 55 bis 60 lÃ¶schen da diese ja nur wiederholungen sind

**Ergebnisvermerk:** Bezeichner und sichtbare Texte wurden auf Englisch umgestellt. Die betreffenden Zeilen enthielten den Move-Konstruktor und notwendige Interface-Zuweisungen und wurden deshalb beibehalten; ein Ã¼berflÃ¼ssiger Kommentar wurde entfernt. Der Build war erfolgreich.

## 11. Pull und Push

**Datum:** 29.10.2026
**Werkzeug/Modell:** Copilot SDK in VS Code / Modell nicht ausgewiesen
**Prompt im Wortlaut:**

> kannst du pullen und dann pushen

**Ergebnisvermerk:** Es wurde festgestellt, dass fÃ¼r den Push zunÃ¤chst ein Commit nÃ¶tig war; die vorhandenen, sachfremden Ã„nderungen wurden nicht einbezogen.

## 12. Commit, Pull und Push

**Datum:** 29.10.2026
**Werkzeug/Modell:** Copilot SDK in VS Code / Modell nicht ausgewiesen
**Prompt im Wortlaut:**

> kannst du comitten dann pullen und dann pushen

**Ergebnisvermerk:** Die Vier-gewinnt-Ã„nderungen wurden committet und per Rebase integriert. Der Push scheiterte zunÃ¤chst an fehlenden Zugangsdaten.

## 13. Push erneut versuchen

**Datum:** 29.10.2026
**Werkzeug/Modell:** Copilot SDK in VS Code / Modell nicht ausgewiesen
**Prompt im Wortlaut:**

> kannst du nochmal pushen

**Ergebnisvermerk:** Nach zwischenzeitlichen Remote-Ã„nderungen wurde erfolgreich rebased und anschlieÃŸend gepusht; der Branch war synchron.

## 14. Prompt-Dokumentation speichern

**Datum:** 29.10.2026
**Werkzeug/Modell:** Copilot SDK in VS Code / Modell nicht ausgewiesen
**Prompt im Wortlaut:**

> kannst du die prompts in einer md datei speichern

**Ergebnisvermerk:** Diese Prompt-Dokumentation wurde unter `Classes/Gruppe2/Elena_Stempfle_Prompts.md` erstellt. Der Dateiname folgt dem vorgegebenen Schema `Vorname_Nachname_Prompts.md`.
