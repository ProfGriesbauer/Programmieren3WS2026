# Prompt-Protokoll – Eduard Schoenfeld (Casino, Spiel 2)

## 12-11-2026

### Prompt 1

Werkzeug: Claude Code (VS-Code-Erweiterung)
Modell: Claude Sonnet 5.5 / Claude Opus 5.5

```text
# Casino Adventure – Vollständiges dynamisches NPC-, Dialog- und Beziehungssystem

## Projektauftrag

Erweitere mein bestehendes Casino Adventure um ein lebendiges, interaktives NPC-System mit individuellen Charakteren, Monologen, Gesprächen, Erinnerungen, Vertrauens- und Respektwerten, Freundschaften, Rivalitäten, Feindschaften, Aufgaben und einer gemeinsamen Casino-Spielökonomie.


**Das wichtigste Ziel:** Die Charaktere sollen sich wie eigenständige Personen anfühlen, die sich an den Spieler erinnern und abhängig von dessen Verhalten unterschiedlich reagieren.

Das System muss zu den bereits vorhandenen Charakteren, Grafiken, Spielmechaniken und der bestehenden Architektur passen.

# 1. Verbindliche Projektregeln

Lies vor jeder Implementierung die vollständige AGENTS.md.

Untersuche anschließend:

- Sämtliche vorhandenen NPCs und ihre Grafiken.
- Bestehende NPC-Klassen und Interaktionsmechanismen.
- Vorhandene Dialogfenster.
- Die Spielerbewegung und Kollisionslogik.
- Das Geldsystem.
- Den Painter und das vorhandene Rendering.
- Die Spielzustandsverwaltung.
- Die tatsächlich existierenden OOPGames-Interfaces.
- Die bisherigen Minispiele und deren Ereignisse.

Die vorhandenen Implementierungen dürfen nicht unnötig ersetzt werden.

Halte alle Dateipfad-, Entwicklerkennzeichnungs-, Dokumentations- und Framework-Regeln der AGENTS.md ein.

Insbesondere:

- Keine Änderungen an Framework-Dateien.
- Keine eigenen Fenster.
- Keine eigene Game-Loop.
- Keine unabhängigen Eingabe-Handler.
- Keine erfundenen Framework-Methoden.
- Ausschließlich das vorhandene Framework-Canvas verwenden.
- Neue Funktionen nur in den zulässigen Projektverzeichnissen erstellen.
- Verantwortliche Entwickler korrekt kennzeichnen.
- Änderungen müssen mit den existierenden Minispielen kompatibel bleiben.

Falls eine gewünschte Funktion nicht ohne unzulässige Framework-Änderung implementiert werden kann, erläutere die Einschränkung und schlage eine zulässige Alternative vor.

# 2. Bestehende NPCs und visueller Stil

**Die bereits bestehenden Charaktere haben absolute Priorität.**

Erfinde keine vollständig neuen Charakterdesigns, wenn bereits NPC-Grafiken existieren.

Jeder NPC soll seine bisherige visuelle Identität behalten.

Dazu gehören:

- Frisur und Haarfarbe.
- Hautfarbe.
- Kleidung und Kleidungsfarben.
- Körperform und Proportionen.
- Auffällige Accessoires.
- Bestehende Animationen.
- Vorhandener Pixel-Art-Stil.
- Wiedererkennbare Merkmale.

Orientiere dich an der bestehenden Pokémon-Rubin-/Saphir-inspirierten Top-Down-2D-Pixel-Art.

Die NPCs dürfen durch das neue System nicht plötzlich wie moderne Comicfiguren, Anime-Porträts oder realistische 3D-Charaktere aussehen.

Alle visuellen Ergänzungen müssen sich harmonisch in das vorhandene Spiel integrieren.

Vorhandene NPC-Designs dürfen nicht ohne Notwendigkeit geändert werden.

# 3. Detaillierte Charakterporträts während Gesprächen

Wenn der Spieler mit einem NPC spricht, soll eine größere, detailliertere Darstellung des Charakters im Dialogfenster erscheinen.

Das ist eine besonders wichtige Erweiterung.

## 3.1 Charakterporträts

Im normalen Spiel bleibt der NPC als kleiner Top-Down-Pixel-Art-Charakter sichtbar.

Beim Gespräch erscheint zusätzlich ein größeres Pixel-Art-Porträt, das den Charakter detaillierter zeigt.

Dieses Porträt muss eindeutig derselbe Charakter sein.

Beispiele:

Ein NPC mit schwarzem Anzug, dunklen Haaren und goldener Krawatte muss auch im Dialogfenster diese Merkmale besitzen.

Ein älterer NPC soll im Porträt entsprechend älter aussehen.

Ein NPC mit Brille muss diese auch im Porträt tragen.

Ein Charakter mit bestimmten Haarfarben, Accessoires oder Kleidung muss wiedererkennbar bleiben.

Das Porträt zeigt vorzugsweise Kopf und Oberkörper.

Die Darstellung soll wie ein hochwertiger Charakterdialog aus einem klassischen 16-Bit-RPG wirken.

## 3.2 Emotionale Porträts

Jeder NPC soll nach Möglichkeit verschiedene emotionale Darstellungen erhalten:

- Neutral.
- Fröhlich.
- Wütend.
- Traurig.
- Überrascht.
- Nachdenklich.

Die angezeigte Variante hängt von der aktuellen Stimmung und dem Gesprächsverlauf ab.

Beispiel:

Der Spieler beleidigt einen Geschäftsmann.

Sein Porträt wechselt zu einem wütenden Gesichtsausdruck.

Er antwortet:

„Pass auf, wie du mit mir redest!“

Sein Vertrauenswert sinkt.

Bei einem Kompliment kann stattdessen eine zufriedene Variante erscheinen.

## 3.3 Technische Umsetzung

Die Porträts müssen über das vorhandene Grafiksystem verwaltet und austauschbar sein.

Verwende nach Möglichkeit vorhandene passende Grafikressourcen.

Falls noch keine detaillierten Porträts vorhanden sind:

- Definiere eine saubere, austauschbare Porträtzuordnung.
- Verwende vorläufig vorhandene Sprites oder eine geeignete vergrößerte Darstellung.
- Erhalte das Seitenverhältnis und die scharfen Pixelkonturen.
- Erfinde keine unpassenden oder visuell widersprüchlichen Porträts.
- Dokumentiere, welche Porträtressourcen noch benötigt werden.

Es dürfen keine zusätzlichen grafischen Fenster erstellt werden.

Falls für neue Bilddateien nicht erlaubte Asset-Verzeichnisse benötigt würden, kläre dies entsprechend der AGENTS.md, statt unerlaubt Dateien anzulegen.

# 4. Automatische NPC-Monologe

Alle vorhandenen NPCs sollen gelegentlich kurze Monologe führen.

Die Monologe erscheinen als kleine Pixel-Art-Sprechblasen unmittelbar über ihren Köpfen.

Anforderungen:

- Idealerweise 2–8 Wörter pro Monolog.
- Monologe sollen zur Persönlichkeit passen.
- Die Texte erscheinen in zeitlichen Abständen.
- Sie verschwinden automatisch.
- Sie dürfen sich nicht gegenseitig unübersichtlich überdecken.
- Gleichzeitig sollen höchstens zwei bis drei spontane Sprechblasen sichtbar sein.
- NPCs in Spielnähe erhalten Priorität.
- Gespräche haben Vorrang vor automatischen Monologen.

Die Aussagen sollen sich an die aktuelle Situation anpassen.

Beispiele:

Reicher Geschäftsmann:
„Heute verdopple ich mein Vermögen!“

Armer Spieler:
„Das waren meine letzten Chips ...“

Croupier:
„Bitte platzieren Sie Ihre Einsätze.“

Arroganter Rivale:
„Der hat doch nur Glück!“

Verärgerter NPC:
„Mit dem rede ich nicht mehr.“

Freundlicher NPC:
„Schön, ihn wiederzusehen!“

Die Monologe dürfen sich verändern, wenn sich die Beziehung zum Spieler verändert.

# 5. Interaktives Dialogsystem

Der Spieler soll NPCs über die vorgesehenen Framework-Eingaben ansprechen können.

Beim Ansprechen öffnet sich ein Dialogfenster im vorhandenen Pixel-Art-Stil.

Das Fenster zeigt:

- Den Namen des NPCs.
- Sein detailliertes Charakterporträt.
- Den aktuellen Gesichtsausdruck.
- Seine Gesprächsantwort.
- Zwei bis vier passende Antwortmöglichkeiten.
- Eine Vertrauensleiste.
- Eine Respektleiste.
- Den aktuellen Beziehungsstatus.

Während eines Dialogs ist die Spielerbewegung deaktiviert.

Das Gespräch kann jederzeit beendet werden.

Dialoge sollen mehrere Stufen besitzen und unterschiedliche Verläufe ermöglichen.

Beispiel:

NPC:
„Ich habe heute 500 Chips verloren.“

Antwortmöglichkeiten:

1. „Das tut mir leid.“
2. „Vielleicht solltest du aufhören.“
3. „Du kannst wohl nicht spielen.“
4. „Kannst du mir etwas Geld geben?“

Jede Antwort führt zu einer charakterabhängigen Reaktion.

Wiederholte Gespräche müssen sich abhängig von früheren Entscheidungen verändern können.

Keine externen KI-Schnittstellen erforderlich.

Verwende eine nachvollziehbare, datenbasierte Dialoglogik mit vordefinierten Aussagen und dynamischen Bedingungen.

# 6. Individuelle NPC-Persönlichkeiten

Jeder NPC soll ein eigenes Charakterprofil besitzen.

Mindestens folgende Eigenschaften sind vorgesehen:

- Name.
- Rolle im Casino.
- Persönlichkeit.
- Stimmung.
- Vertrauen zum Spieler.
- Respekt gegenüber dem Spieler.
- Beziehungsstatus.
- Persönlicher Geldbeutel.
- Großzügigkeit.
- Risikofreude.
- Individuelle Dialoge.
- Monologe.
- Relevante Erinnerungen.

Die Charakterprofile sollen sich an den bereits vorhandenen NPCs orientieren.

Mögliche Archetypen:

Reicher Geschäftsmann:
Arrogant, vermögend, selbstbewusst, nachtragend.

Freundliche ältere Dame:
Wohlhabend, hilfsbereit, höflich, großzügig.

Armer Glücksspieler:
Verzweifelt, wenig Geld, emotional, grundsätzlich freundlich.

Croupier:
Professionell, zurückhaltend, höflich, regelorientiert.

Überheblicher Spieler:
Selbstbewusst, leicht provozierbar, risikofreudig.

Misstrauischer Besucher:
Vorsichtig, skeptisch, wenig großzügig gegenüber Fremden.

Diese Beispiele sollen nicht dazu führen, dass vorhandene Charaktere grundlos ersetzt werden.

# 7. Vertrauen und Respekt

Implementiere zwei voneinander unabhängige Werte für jeden NPC.

## Vertrauen

Wertebereich: 0–100.

Vertrauen beschreibt, wie stark der NPC dem Spieler vertraut.

Es beeinflusst:

- Persönliche Gespräche.
- Geldgeschenke.
- Hilfsbereitschaft.
- Das Angebot bestimmter Aufgaben.
- Die Bereitschaft, Geheimnisse zu erzählen.
- Freundschaftsentwicklung.

## Respekt

Wertebereich: 0–100.

Respekt beschreibt, wie ernst der NPC den Spieler nimmt.

Er wird beispielsweise durch Erfolge, Herausforderungen, faire Entscheidungen oder besondere Aufgaben beeinflusst.

Hoher Respekt bedeutet nicht automatisch Freundschaft.

Beispiel:

Ein Geschäftsmann kann den Spieler sehr respektieren, weil dieser ihn mehrfach beim Poker besiegt hat.

Trotzdem kann er den Spieler nicht besonders mögen und ihn als Rivalen betrachten.

## Darstellung

Beide Werte erscheinen im Gesprächsfenster als kompakte Pixel-Art-Leisten.

Bei Änderungen wird kurz ein entsprechender Hinweis angezeigt.

Beispiele:

„Vertrauen +5“

„Respekt +10“

„Vertrauen -15“

Alle Werte werden auf 0–100 begrenzt.

Dialoge dürfen nicht beliebig oft wiederholt werden, um diese Werte unbegrenzt zu erhöhen.

# 8. Dynamischer Beziehungsstatus

Implementiere ein Beziehungssystem.

Mögliche Zustände:

- Unbekannt.
- Bekannt.
- Freund.
- Guter Freund.
- Rivale.
- Respektierter Rivale.
- Gegner.
- Erzfeind.

Die Beziehung soll nicht ausschließlich durch den Vertrauenswert bestimmt werden.

Berücksichtige zusätzlich:

- Respekt.
- Frühere Interaktionen.
- Beleidigungen.
- Erhaltene Hilfe.
- Poker- und Casino-Ergebnisse.
- Erfüllte Aufgaben.
- Individuelle Charaktereigenschaften.
- Besonders wichtige Ereignisse.

Beispiel:

Ein NPC kann viel Vertrauen besitzen und trotzdem ein freundschaftlicher Rivale sein.

Ein anderer NPC kann den Spieler respektieren, ihn aber aufgrund einer früheren Beleidigung ablehnen.

Beziehungsübergänge müssen nachvollziehbar erfolgen und dürfen nicht bei jeder kleinen Wertänderung unkontrolliert wechseln.

# 9. NPC-Verhalten abhängig vom Beziehungsstatus

**Das ist eine der wichtigsten Anforderungen.**

Die Beziehung muss sich tatsächlich auf das Verhalten eines NPCs auswirken.

Es reicht nicht, lediglich die Bezeichnung „Freund“ oder „Rivale“ anzuzeigen.

## 9.1 Unbekannte NPCs

- Begrüßen den Spieler zurückhaltend.
- Geben nur allgemeine Informationen.
- Sind bei Geldanfragen skeptisch.
- Erzählen keine persönlichen Geheimnisse.

## 9.2 Freunde

- Begrüßen den Spieler freundlich.
- Erkennen ihn wieder.
- Erzählen persönliche Geschichten.
- Bieten besondere Dialoge an.
- Helfen dem Spieler gelegentlich.
- Können zusätzliche Aufgaben anbieten.
- Freuen sich über Erfolge des Spielers.

Beispiel:

„Hey, schön dich wiederzusehen!“

## 9.3 Gute Freunde

- Sind besonders hilfsbereit.
- Können spezielle Aufgaben freischalten.
- Reagieren auf Probleme des Spielers.
- Bieten bei Geldmangel eher Unterstützung an.
- Erinnern sich an gemeinsame Ereignisse.

Beispiel:

„Du siehst aus, als könntest du Hilfe gebrauchen.“

## 9.4 Rivalen

- Fordern den Spieler heraus.
- Reagieren auf dessen Gewinne.
- Machen provokante Bemerkungen.
- Können spezielle Duelle anbieten.
- Respektieren gute Leistungen.
- Können trotz Rivalität freundlich sein.

Beispiel:

„Na, bereit für die nächste Niederlage?“

## 9.5 Respektierte Rivalen

- Nehmen den Spieler besonders ernst.
- Bieten anspruchsvollere Duelle an.
- Kommentieren seine Erfolge.
- Können nach einer Niederlage Anerkennung zeigen.
- Entwickeln eine dauerhafte Wettbewerbsdynamik.

Beispiel:

„Du bist besser geworden. Das muss ich zugeben.“

## 9.6 Gegner

- Reagieren abweisend.
- Verweigern bestimmte Hilfen.
- Beleidigen oder provozieren den Spieler gelegentlich.
- Können Gespräche schneller beenden.
- Sind bei Geldanfragen besonders ablehnend.
- Können die Beziehung durch spätere positive Handlungen wieder verbessern.

Beispiel:

„Was willst du schon wieder?“

## 9.7 Erzfeinde

- Verweigern normalerweise persönliche Hilfe.
- Reagieren besonders negativ auf den Spieler.
- Können besondere Konfliktdialoge auslösen.
- Können durch bestimmte Ereignisse wieder zu normalen Gegnern werden.

Es darf keine dauerhaft unumkehrbare Feindschaft ohne nachvollziehbaren Grund entstehen.

NPCs sollen keine völlig neuen Mechaniken erhalten, die den bestehenden Casino-Regeln widersprechen.

# 10. NPC-Erinnerungssystem

Jeder NPC soll sich wichtige Ereignisse merken.

Beispiele:

- Erste Begegnung mit dem Spieler.
- Anzahl bisheriger Gespräche.
- Wichtige freundliche oder unfreundliche Entscheidungen.
- Ob eine Beleidigung stattgefunden hat.
- Ob Geld verschenkt wurde.
- Anzahl der Geldanfragen.
- Erledigte Aufgaben.
- Gemeinsame Casino-Duelle.
- Besondere Gewinne oder Verluste.
- Aktuelle Rivalität.

Speichere nur tatsächlich relevante Ereignisse.

Eine umfangreiche Speicherung sämtlicher Gesprächszeilen ist nicht notwendig.

Erinnerungen sollen spätere Dialoge beeinflussen.

Beispiel:

Erste Begegnung:
„Hallo, wir kennen uns noch nicht.“

Nach mehreren Gesprächen:
„Ach, du schon wieder!“

Nach einem gewonnenen Pokerduell:
„Du hast mich letztes Mal überrascht.“

Nach einer Beleidigung:
„Ich habe nicht vergessen, was du gesagt hast.“

Die Zustände müssen während einer Spielsitzung zuverlässig erhalten bleiben.

Eine dauerhafte Speicherung ist nur zu ergänzen, wenn sie mit dem bestehenden Framework und den Projektvorgaben vereinbar ist.

# 11. Individuelle Geldbeutel

Jeder NPC besitzt einen eigenen virtuellen Geldbeutel.

Beispielwerte:

- Reicher Geschäftsmann: 20.000 Chips.
- Wohlhabende Dame: 12.000 Chips.
- Normaler Besucher: 1.500 Chips.
- Armer Spieler: 120 Chips.
- Croupier: 800 Chips.

Die Werte sind an die tatsächliche Spielökonomie anzupassen.

Ein NPC darf niemals mehr Geld verschenken, als er besitzt.

Geld darf nicht aus dem Nichts entstehen.

Beispiel:

NPC verschenkt 100 Chips.

NPC-Geldbeutel: -100 Chips.

Spieler-Geldbeutel: +100 Chips.

Die Geldtransaktion muss vollständig und konsistent ausgeführt werden.

Nutze möglichst die bereits vorhandene Geldverwaltung.

Achte darauf, private NPC-Geldbeutel und eventuelle Casino-Gelder des Hauses sauber voneinander zu unterscheiden.

# 12. NPCs um Geld bitten

Der Spieler soll geeignete NPCs im Gespräch um Chips bitten können.

Dialogoption:

„Kannst du mir etwas Geld geben?“

Die Entscheidung hängt ab von:

- Vertrauen.
- Beziehungsstatus.
- Geldbeutel.
- Großzügigkeit.
- Stimmung.
- Frühere Geldanfragen.
- Individuellen Charaktereigenschaften.

Ein armer NPC darf auch bei sehr hohem Vertrauen ablehnen, weil er selbst nicht genug Geld hat.

Ein reicher NPC kann bei ausreichendem Vertrauen größere Beträge verschenken.

Freunde unterstützen eher als Gegner.

Rivalen können unter Umständen kleine Beträge aus Überheblichkeit vergeben.

Die tatsächlichen Beträge müssen sinnvoll berechnet werden und zum Charakter passen.

Implementiere Begrenzungen gegen wiederholtes Betteln und unendliche Geldgenerierung.

Wiederholte Geldanfragen können Vertrauen und Stimmung verschlechtern.

Jede Antwort soll zur Persönlichkeit des NPCs passen.

# 13. Aufgaben bei Geldmangel

Wenn der Spieler keine Chips mehr besitzt, soll er weiterhin eine Möglichkeit haben, Geld zu verdienen.

Dafür erhalten geeignete NPCs kleine Aufgaben.

Beispiele:

**Geschäftsmann:** Gewinne ein kostenloses Pokerduell gegen ihn.

**Croupier:** Hilf beim Einsammeln verlorener Chips.

**Älterer Besucher:** Finde einen verlorenen Gegenstand im Casino.

**Nervöser Spieler:** Überzeuge ihn durch ein Gespräch, eine Pause einzulegen.

**Casino-Manager:** Erledige einen kleinen Botengang.

Aufgaben sollen:

- Zur Persönlichkeit des Auftraggebers passen.
- Einen klaren Fortschritt besitzen.
- Eine nachvollziehbare Belohnung liefern.
- Mit dem bestehenden Geldsystem verbunden sein.
- Vertrauen oder Respekt beeinflussen können.
- Nach Abschluss korrekt als erledigt markiert werden.
- Nicht beliebig zur unendlichen Geldproduktion missbraucht werden.

Implementiere mindestens eine faire, wiederholbare und risikofreie Verdienstmöglichkeit, die auch bei vollständig fehlenden Chips verfügbar ist.

Der Spieler darf niemals dauerhaft feststecken, weil er kein Geld mehr besitzt und sämtliche NPCs ihn ablehnen.

Vermeide dabei zusätzliche große Minispiele. Bevorzuge Aufgaben, die mit der vorhandenen Casino-Welt umsetzbar sind.

# 14. NPC-Stimmungssystem

Jeder NPC soll eine einfache aktuelle Stimmung besitzen.

Mögliche Stimmungen:

- Glücklich.
- Neutral.
- Traurig.
- Verärgert.
- Nervös.

Die Stimmung beeinflusst:

- Monologe.
- Gesichtsausdrücke im Dialogporträt.
- Gesprächsantworten.
- Geldanfragen.
- Hilfsbereitschaft.
- Verhalten gegenüber dem Spieler.

Beispiele:

Ein NPC gewinnt beim Roulette und wird glücklich.

Ein NPC verliert viel Geld und wird verärgert.

Ein Spieler beleidigt einen NPC und verschlechtert dessen Stimmung.

Ein Freund unterstützt einen NPC und verbessert dessen Stimmung.

Die Stimmungen sollen sich nachvollziehbar ändern.

Vermeide übermäßig häufige Stimmungswechsel.

# 15. Reaktionen auf Ereignisse im Casino

**NPCs sollen auf das reagieren, was tatsächlich im Casino passiert.**

Die Spielfigur soll nicht unabhängig vom Rest der Spielwelt existieren.

Berücksichtige vorhandene Casino-Ereignisse.

Beispiele:

- Der Spieler gewinnt viel Geld.
- Der Spieler verliert viel Geld.
- Ein NPC gewinnt eine Pokerrunde.
- Der Spieler besiegt einen Rivalen.
- Ein NPC verliert seine letzten Chips.
- Der Spieler erledigt eine Aufgabe.
- Eine neue Freundschaft entsteht.
- Ein NPC wird zum Gegner.

Diese Ereignisse sollen passende Reaktionen auslösen können.

Beispiel:

Der Spieler gewinnt 2.000 Chips.

Ein befreundeter NPC sagt:
„Unglaublich! Glückwunsch!“

Ein Rivale sagt:
„Genieß es, solange es anhält.“

Ein Gegner sagt:
„Pures Glück. Mehr nicht.“

Ein misstrauischer NPC sagt:
„Das ging aber schnell ...“

Nicht jeder NPC muss auf jedes Ereignis reagieren.

Bevorzuge NPCs, die das Ereignis glaubwürdig wahrnehmen konnten.

Vermeide, dass alle NPCs automatisch über Ereignisse informiert werden, die außerhalb ihres Wahrnehmungsbereichs stattfinden.

Verknüpfe die Ereignisse möglichst mit vorhandenen Spielzuständen und Minispiel-Ergebnissen.

Führe keine parallelen Casino-Ergebnisberechnungen ein.

# 16. Dynamische Rivalitäten durch Casino-Duelle

Rivalitäten sollen sich auch aus dem tatsächlichen Spielverhalten entwickeln können.

Beispiel:

Der Spieler gewinnt mehrfach gegen denselben NPC beim Poker.

Anfangs:
„Gutes Spiel.“

Später:
„Das war bestimmt nur Glück.“

Nach weiteren Niederlagen:
„Jetzt reicht's! Revanche!“

Abhängig von seiner Persönlichkeit kann der NPC mehr Respekt gewinnen oder zunehmend verärgert sein.

Ein fairer Spieler reagiert anders als ein arroganter Spieler.

Wichtig:

Eine Rivalität ist nicht zwangsläufig eine Feindschaft.

Der Spieler kann einen NPC respektieren und trotzdem mit ihm konkurrieren.

Die Casino-Regeln bleiben unverändert.

# 17. Rufsystem im Casino

Ergänze ein einfaches globales Rufsystem, sofern es mit vertretbarem Aufwand integrierbar ist.

Der Ruf beschreibt, wie andere Casino-Besucher den Spieler grundsätzlich wahrnehmen.

Er wird durch besonders wichtige öffentliche Handlungen beeinflusst.

Beispiele:

- Hilfsbereitschaft kann den Ruf verbessern.
- Öffentliches respektloses Verhalten kann ihn verschlechtern.
- Erfolge in Casino-Duellen können den Bekanntheitsgrad steigern.

Unterscheide persönlichen NPC-Vertrauensstand und globalen Ruf.

Ein NPC darf den Spieler trotz schlechtem Ruf persönlich mögen.

Umgekehrt kann ein NPC einen bekannten Spieler ablehnen.

Rufänderungen sollen nur bei glaubwürdig beobachtbaren oder weitergegebenen Ereignissen stattfinden.

Das System darf nicht zu kompliziert werden.

# 18. NPC-Bewegungen und kleine Tagesabläufe

Falls die bestehende Architektur und die verfügbaren Framework-Mechanismen es erlauben, sollen NPCs einfache Aktivitäten ausführen können.

Beispiele:

- An einem Pokertisch sitzen.
- Zu einem anderen Spielbereich laufen.
- In der Nähe eines Spielautomaten stehen.
- Bei einem Gespräch zum Spieler schauen.
- Nach einem Duell den Aufenthaltsort verändern.

Die vorhandenen Sprite-Richtungen und Animationen sollen verwendet werden.

NPCs dürfen nicht durch Wände oder Möbel laufen.

Die Spielerbewegung und vorhandene Kollisionen müssen unverändert zuverlässig funktionieren.

Vermeide komplexe Wegfindung, wenn sie unverhältnismäßig viel Entwicklungsaufwand erfordert.

Priorität haben funktionierende Dialoge und Beziehungen.

# 19. Benutzeroberfläche

Alle Oberflächenelemente müssen zum bestehenden Casino-Stil passen.

Verwende:

- Pixel-Art-Rahmen.
- Gut lesbare Schrift.
- Bestehende Farbschemata.
- Austauschbare Grafikressourcen.
- Kompakte Sprechblasen.
- Klare Dialogauswahl.
- Detaillierte NPC-Porträts.
- Vertrauens- und Respektleisten.
- Anzeige des Beziehungsstatus.
- Kleine Hinweise bei Wertänderungen.
- Optische Rückmeldung bei Geldtransaktionen.

Die Oberfläche muss innerhalb der vorhandenen Zeichenfläche funktionieren.

Achte darauf, dass wichtige Spielbereiche nicht unnötig verdeckt werden.

Die Dialogansicht soll wie eine hochwertige Erweiterung des bestehenden Spiels wirken und nicht wie eine fremde Benutzeroberfläche.

# 20. Modulare Architektur

Verwende eine objektorientierte, modular aufgebaute Architektur.

Trenne insbesondere:

- NPC-Charakterdaten.
- NPC-Persönlichkeit.
- Dialogverwaltung.
- Gesprächsoptionen.
- Vertrauen und Respekt.
- Beziehungsstatus.
- Erinnerungen.
- Stimmung.
- Geldtransaktionen.
- Aufgaben.
- Casino-Ereignisse.
- Grafische Darstellung.
- NPC-Porträts.
- Sprechblasen.

Nutze vorhandene Klassen und Mechanismen, wo möglich.

Erstelle keine riesige NPC-Klasse, die sämtliche Funktionen enthält.

Ebenso sollen nicht unnötig viele Managerklassen entstehen.

Der Painter darf nicht die vollständige Dialog-, Geld- oder Beziehungslogik enthalten.

Alle Grafiken müssen ohne Änderungen an den eigentlichen Spielregeln ausgetauscht werden können.

Die tatsächlichen Schnittstellen des OOPGames-Frameworks haben Vorrang vor jeder vorgeschlagenen Architektur.

# 21. Technische Qualität und Tests

Überprüfe folgende Fälle:

1. NPCs behalten ihre bestehenden Grafiken und Identitäten.
2. Detaillierte Gesprächsporträts passen optisch zu den jeweiligen NPCs.
3. Dialoge lassen sich starten und beenden.
4. Die Spielerbewegung wird während Dialogen korrekt gesperrt und anschließend freigegeben.
5. Sprechblasen verschwinden zuverlässig.
6. Vertrauen und Respekt bleiben zwischen 0 und 100.
7. NPCs reagieren unterschiedlich auf Freundschaften, Rivalitäten und Feindschaften.
8. Erinnerungen beeinflussen spätere Gespräche.
9. Geldtransaktionen verändern beide Geldbestände korrekt.
10. NPCs können kein Geld verschenken, das sie nicht besitzen.
11. Wiederholte Geldanfragen können nicht beliebig ausgenutzt werden.
12. Der Spieler kann auch ohne Chips wieder Geld verdienen.
13. Aufträge vergeben ihre Belohnungen nicht mehrfach durch denselben Abschluss.
14. Casino-Ereignisse beeinflussen nur passende NPCs.
15. Bestehende Minispiele funktionieren weiterhin.
16. Sämtliche Funktionen verwenden das vorhandene Framework.
17. Keine unerlaubten Projektdateien wurden verändert.
18. Das Projekt lässt sich weiterhin kompilieren.

# 22. Verbindliche Entwicklungsreihenfolge

**Phase 1 – Analyse**

Untersuche AGENTS.md, vorhandene Charaktere, Framework-Schnittstellen, Spielzustände und Geldsystem.

**Phase 2 – Architektur**

Erstelle einen konkreten Implementierungsplan und erläutere die erforderlichen Dateien.

**Phase 3 – NPC-Grundsystem**

Ergänze Persönlichkeit, Geldbeutel, Vertrauen und Respekt.

**Phase 4 – Grafische Dialoge**

Implementiere Sprechblasen, Gesprächsfenster und austauschbare Charakterporträts.

**Phase 5 – Gesprächssystem**

Ergänze Dialogauswahl, individuelle Reaktionen und mehrstufige Gespräche.

**Phase 6 – Beziehungen**

Implementiere Freundschaft, Rivalität, Feindschaft und charakterabhängiges Verhalten.

**Phase 7 – Erinnerungen**

Verbinde vergangene Spielerhandlungen mit zukünftigen Reaktionen.

**Phase 8 – Geld und Aufgaben**

Implementiere Geldanfragen, individuelle Geldbeutel und Möglichkeiten zum Verdienen neuer Chips.

**Phase 9 – Ereignisse**

Verbinde Casino-Gewinne, Verluste, Minispiele und NPC-Reaktionen.

**Phase 10 – Erweiterungen**

Ergänze bei ausreichender Kompatibilität Stimmung, globalen Ruf und einfache NPC-Bewegungen.

**Phase 11 – Qualitätssicherung**

Überprüfe alle Funktionen, die grafische Konsistenz, die Dateiregeln und die Kompilierbarkeit.

Arbeite in nachvollziehbaren, kleineren Entwicklungsschritten.

Nach größeren Änderungen muss das Projekt überprüft werden.

Vorhandene Funktionen dürfen nicht unnötig ersetzt oder beschädigt werden.

# 23. Erwartetes Endergebnis

Das Casino soll sich wie eine lebendige, interaktive Spielwelt anfühlen.

Die Charaktere sollen:

- Eigenständige Persönlichkeiten besitzen.
- Kurze Monologe über ihren Köpfen anzeigen.
- In Gesprächen als detailliertere Pixel-Art-Porträts erscheinen.
- Unterschiedliche Gesichtsausdrücke zeigen.
- Auf Entscheidungen reagieren.
- Vertrauen und Respekt aufbauen oder verlieren.
- Sich an wichtige Ereignisse erinnern.
- Zu Freunden, Rivalen oder Gegnern werden.
- Ihr Verhalten abhängig von diesen Beziehungen verändern.
- Eigene finanzielle Mittel besitzen.
- Geld verschenken oder verweigern können.
- Kleine Aufgaben anbieten können.
- Auf Casino-Gewinne und Verluste reagieren.
- Glaubwürdig in die vorhandene Spielwelt integriert sein.

**Der entscheidende Grundsatz lautet:**

Ein NPC soll den Spieler nach einer Freundschaft anders behandeln als während einer Rivalität oder Feindschaft.

Diese Unterschiede müssen durch Dialoge, Emotionen, Angebote, Aufgaben, Hilfsbereitschaft und Reaktionen tatsächlich spürbar sein.

Das System soll erweiterbar bleiben, damit später zusätzliche Charaktere, Dialoge und Ereignisse eingefügt werden können.

Die bereits vorhandenen Charakterdesigns und der bestehende visuelle Stil sind dabei jederzeit zu respektieren.

**Beginne mit der Analyse des vorhandenen Projekts und erstelle einen konkreten Implementierungsplan. Setze anschließend die Funktionen schrittweise unter vollständiger Einhaltung der AGENTS.md um.** setze bei den änderungen den Namen Eduard_Schoenfeld
```

Ergebnisvermerk: NPC-System für die 5 vorhandenen Casino-NPCs umgesetzt (Ordner `NPC/`, dazu `UI/Eduard_Schoenfeld_*`,
`Graphics/Eduard_Schoenfeld_NpcSprites.cs`); geänderte Stellen in `GameState`, `GameManager`, `Painter` und `SpriteLibrary`
sind mit „Änderung durch: Eduard Schoenfeld“ markiert. Plan: `Plans/Eduard_Schoenfeld_NPC_System_Plan.md`.
`dotnet build`: 0 Fehler, 0 Warnungen. Automatischer Testlauf (separates Testprojekt außerhalb des Repos) mit 118 Prüfungen
zu Abschnitt 21; Bedienung mit echter Tastatur/Maus im Hauptfenster nicht getestet. Nicht umgesetzt: NPC-Bewegungen,
kostenloses Pokerduell, dauerhafte Speicherung (siehe Plan). Kein Commit.
