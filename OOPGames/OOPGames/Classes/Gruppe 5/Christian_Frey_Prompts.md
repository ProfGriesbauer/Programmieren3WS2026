# Christian Frey – Prompts

## 2026-11-12

- **Werkzeug/Modell:** GitHub Copilot SDK in VS Code; der Modellname wird in dieser Sitzung nicht angezeigt.
- **Prompt im Wortlaut:**

> Wo ist der LS gespeichert? Erstelle für die zukünftigen Prompts von uns eine Datei in Gruppe 5 in denen du diese speicherst. nach dem Beispiel von Tim_Eichert_Prompts und nenne sie Christian_Frey_Prompts

- **Ergebnisvermerk:** Diese Prompt-Datei wurde nach dem Beispiel von `Tim_Eichert_Prompts.md` angelegt. Der Landwirtschaftssimulator liegt derzeit in der lokalen Git-Sicherung `stash@{2}` unter `OOPGames/OOPGames/Classes/Gruppe 5/Kreative Spielidee/Vorname_Nachname_Landwirtschaftsspiel.cs`. Weitere Prompts und Ergebnisvermerke werden künftig hier ergänzt.

## 2026-11-12

- **Werkzeug/Modell:** GitHub Copilot SDK in VS Code; der Modellname wird in dieser Sitzung nicht angezeigt.
- **Prompt im Wortlaut:**

> Ich erstelle mit zwei meiner Komilitonen ein Computerspiel nach dem Beipiel von Factorio und Satisfactory. Dieses soll eine 2D Version aus der Vogelperspektive sein. Unsere bisher festgelegten Anforderungen und Wünsche findest du immer in der aktuelle Anfprderungen.md der Gruppe 5.
> Mein erster Auftrag ist es jetzt erstmal die Gebäude für das Spiel zu gestalten und ihre Funktionen festzulegen. Hier mal meine erste Idee wie ich das ganze umsetzen will. Orientier dich zusätzlich an den bereits erstellten Dokumenten, falls bereits vorhanden.
> Für unser Spiel wollen wir uns an den Grundideen von Satisfactory und Factorio orientieren. Der Fokus liegt auf dem Aufbau einer automatisierten Fabrik, die Rohstoffe abbaut, verarbeitet und daraus immer komplexere Produkte herstellt. Die Gebäude sollen dabei klar unterscheidbare Aufgaben haben und für den Spieler sofort erkennbar sein.
> Der Start erfolgt zunächst über den Handabbau. Der Spieler kann Rohstoffe wie Eisen, Kupfer, Kohle oder Holz direkt sammeln und einfache Gegenstände im Inventar herstellen. Dies dient hauptsächlich dazu, die ersten Gebäude zu bauen und die Automatisierung anzustoßen.
> Das wichtigste frühe Gebäude ist der Miner. Dieser wird direkt auf einem Rohstoffvorkommen platziert und fördert kontinuierlich Ressourcen. Je nach Vorkommen können holz, eisen, kupfer, kohle, kalkstein, quarz, schwefel, uran, wasser, gas, öl etc abgebaut werden. Optisch könnte der Miner durch einen Bohrkopf, bewegliche Arme oder Fördermechanismen dargestellt werden, damit sofort erkennbar ist, dass hier Rohstoffe gewonnen werden.
> Die nächste Verarbeitungsstufe bildet die Schmelze. Dort werden Erze in Barren umgewandelt. Eisenerz wird zu Eisenbarren, Kupfererz zu Kupferbarren usw. Die Schmelze sollte optisch wie ein Industrieofen wirken, beispielsweise mit glühendem Inneren, Rauch und Schornsteinen. Durch diese visuelle Gestaltung erkennt der Spieler auf einen Blick, dass hier Rohstoffe verarbeitet werden.
> Anschließend kommt der Konstruktor zum Einsatz. Dieser verarbeitet Barren oder andere Rohstoffe zu Bauteilen weiter. Eisenbarren können beispielsweise zu Eisenplatten oder Eisenstangen werden, Kupferbarren zu Draht. Optisch könnte der Konstruktor eine kleine Werkhalle oder Maschine mit Zahnrädern darstellen. Über ein Menü kann der Spieler das gewünschte Rezept auswählen.
> Für spätere Spielphasen bietet sich ein Assembler an. Dieser unterscheidet sich vom Konstruktor dadurch, dass mehrere Eingangsmaterialien gleichzeitig benötigt werden. So könnten beispielsweise Eisenstangen und Schrauben zu Rotoren verarbeitet werden. Der Assembler stellt den Übergang zu komplexeren Produktionsketten dar und sollte optisch größer und technisch anspruchsvoller wirken als die vorherigen Gebäude.
> Neben der Produktion wird ein System zur Lagerung benötigt. Lagergebäude dienen dazu, Ressourcen zwischenzuspeichern und Produktionsketten zu entkoppeln. Je höher das Level eines Lagers, desto mehr Gegenstände können gespeichert werden. Optisch könnte sich ein Upgrade durch größere Lagerhallen, zusätzliche Container oder Regalanlagen bemerkbar machen.
> Ein weiterer wichtiger Bereich ist die Stromversorgung. Für das frühe Spiel eignet sich ein Biomassegenerator, der mit Holz oder anderen Brennstoffen betrieben wird. Später könnten Kohlekraftwerke, Gaskraftwerke oder sogar Kernkraftwerke freigeschaltet werden. Jedes Kraftwerk sollte dabei eine höhere Leistung bieten, aber auch komplexere Anforderungen an die Versorgung stellen.
> Zur Verbindung der Gebäude werden Förderbänder benötigt. Diese transportieren Gegenstände automatisch zwischen den verschiedenen Produktionsstufen. Die Förderbänder bewegen Gegenstände tickbasiert über die Karte und bilden das Rückgrat der gesamten Fabrik. Später könnten zusätzliche Gebäude wie Splitter zum Aufteilen von Ressourcenströmen oder Merger zum Zusammenführen mehrerer Ströme ergänzt werden.
> Da sämtliche Gebäude Strom benötigen, ist außerdem ein Stromnetz erforderlich. Strommasten verbinden Gebäude mit den Kraftwerken und ermöglichen die Energieversorgung größerer Fabrikbereiche. Die Verbindung könnte durch sichtbare Leitungen zwischen den Gebäuden dargestellt werden.
> Für das Upgradesystem sollte nicht nur die Produktionsgeschwindigkeit erhöht werden. Interessanter ist es, wenn mehrere Eigenschaften verbessert werden. Ein Miner könnte mit jedem Level mehr Ressourcen fördern, größere interne Speicher erhalten oder einen geringeren Energieverbrauch besitzen. Eine Schmelze könnte schneller arbeiten und größere Eingangs- und Ausgangslager erhalten. Konstruktoren könnten kürzere Produktionszeiten besitzen und Förderbänder höhere Transportgeschwindigkeiten erreichen.
> Wichtig ist dabei, dass Upgrades auch optisch sichtbar werden. Ein höherstufiger Miner könnte zusätzliche Bohrarme erhalten, eine größere Schmelze mehr Schornsteine und stärkere Lichteffekte. Förderbänder könnten unterschiedliche Farben besitzen, zum Beispiel Gelb für MK1, Blau für MK2 und Rot für MK3. Dadurch erkennt der Spieler den Entwicklungsstand seiner Fabrik sofort, ohne Menüs öffnen zu müssen.

- **Ergebnisvermerk:** Die vorhandenen Anforderungen wurden in eine Gebaeudespezifikation mit empfohlenem Umfang fuer die erste spielbare Version, spaeteren Erweiterungen, Aufgaben und Produktionsablaeufen, visuellen Unterscheidungsmerkmalen sowie noch gemeinsam festzulegenden Spielwerten ueberfuehrt. Die Spezifikation liegt unter `Plans/Gebaeude_und_Funktionen.md`.

## 2026-11-12

- **Werkzeug/Modell:** GitHub Copilot SDK in VS Code; der Modellname wird in dieser Sitzung nicht angezeigt.
- **Prompt im Wortlaut:**

> Der Plan sieht gut aus. setze das ganze jetzt in Code um

- **Ergebnisvermerk:** Eine kompilierbare Gruppe-5-Simulationsschicht mit Gebaeuden, konfigurierbaren Rezepten und Upgrades, Itempuffern, tickbasierten Laufbaendern und Stromnetz wurde in `Christian_Frey_FabrikGebaeude.cs` umgesetzt. Der Build der OOPGames-App war erfolgreich. Karten-/WPF-Darstellung und Handabbau/Spielerinventar bleiben ausserhalb dieser Gebaeudeschicht.

## 2026-11-12

- **Werkzeug/Modell:** GitHub Copilot SDK in VS Code; der Modellname wird in dieser Sitzung nicht angezeigt.
- **Prompt im Wortlaut:**

> änder nichts auserhalb der gruppe 5. Wenn du noch Vorgaben brauchst, erstelle einen Abschnitt in der Gebaude datei in Plans, was du brauchst. viele sachen werden noch ergänzt durch die anderen Entwickler welche parallel beispielsweise die Spielumgebung und die Bedienung entwickeln

- **Ergebnisvermerk:** Der Gebaeude-Plan wurde um die fuer die spaetere Integration benoetigten Absprachen mit Spielumgebung und Bedienung erweitert. Die laufende Umsetzung ist auf Gruppe 5 begrenzt; der temporäre Testordner ausserhalb von Gruppe 5 wurde wieder entfernt.
