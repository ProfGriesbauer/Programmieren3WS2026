i# Jessica Seise – Prompt-Protokoll

Werkzeug: Copilot SDK in VS Code
Modell: im Chat nicht angezeigt

## 2026-10-29

### Prompt 1

```text
mach einen pull und erkläre wie es funktioniert
```

Ergebnis: `master` wurde per Fast-Forward aktualisiert. Der Ablauf von `git pull` sowie die Spielregel-Interfaces in `IGameRules.cs` wurden erklärt.

### Prompt 2

```text
ok gut speichere mir bitte alle promts damit ich diese am ende meines projekts abgeben kann
```

Ergebnis: Eine lokale Prompt-Sammlung wurde angelegt. Zunächst war sie durch eine lokale Git-Ausschlussregel vom Push ausgeschlossen.

### Prompt 3

```text
Wie stelle ich socher das ich immer im selben chat bin und werden die promts nur für mich sichtbar gespeichert
```

Ergebnis: Es wurde erklärt, wie der Chatverlauf wieder geöffnet werden kann und dass lokale, nicht gepushte Dateien nur auf diesem Rechner liegen.

### Prompt 4

```text
die promts sollen nur für mich gespeichert werden sodas wenn ich etwas pushe die nicht alle haben und nur bei mir bleicht
```

Ergebnis: `PROMPTS.md` wurde in die lokale Git-Ausschlussliste aufgenommen, damit sie nicht versehentlich committet oder gepusht wird.

### Prompt 5

```text
pull
```

Ergebnis: `master` wurde per Fast-Forward auf `8a5dc2b` aktualisiert.

### Prompt 6

```text
pull
```

Ergebnis: `master` wurde per Fast-Forward auf `d49a43f` aktualisiert.

### Prompt 7

```text
pull
```

Ergebnis: `master` wurde per Fast-Forward auf `ccf9aad` aktualisiert.

### Prompt 8

```text
Erstelle mir einen ordner in gruppe 8 bei plans mit meinem namen und in diesem sollen alle promts von mir gespeichert sein
```

Ergebnis: Ein persönliches Prompt-Protokoll wurde erstellt. Zunächst lag es in einem eigenen Unterordner.

### Prompt 9

```text
nein nicht privat es soll später so sein das ich es pushen kann
```

Ergebnis: Die private lokale Sammlung wurde durch eine versionierbare Prompt-Datei im Gruppenbereich ersetzt.

### Prompt 10

```text
orientiere dich an dem ordner von varena also schreibe bei meinem ebenfalls frage und ergebnis rein und speichere keinen extra ordner mit meinem namen
```

Ergebnis: Die Prompts werden in diesem Protokoll im Frage-/Ergebnis-Format wie bei Varena gesammelt. Die Datei wurde zunächst direkt in `Classes/Gruppe 8/Plans` gespeichert.

### Prompt 11

```text
ok super kleine änderung noch es muss doch nicht im ordner plan gespeichert werden sondern bei classes in der gruppe 8
```

Ergebnis: Das Prompt-Protokoll wurde aus `Classes/Gruppe 8/Plans` direkt nach `Classes/Gruppe 8` verschoben.

### Prompt 12

```text
pushe nun den aktuellen stand
```

Ergebnis: Der Remote-Stand wurde vor dem Push per Fast-Forward aktualisiert und das Prompt-Protokoll in Commit `91e369f` gespeichert. Der Push scheiterte, weil keine GitHub-Anmeldung verfügbar war.

### Prompt 13

```text
wie komm ich ins terminal
```

Ergebnis: Der Weg zum integrierten Terminal in VS Code und der dort auszuführende Push-Befehl wurden erklärt.

### Prompt 14

```text
pushe nun nochmal
```

Ergebnis: Neue Remote-Commits wurden integriert, die drei lokalen Commits auf den aktuellen Remote-Stand rebased und anschließend erfolgreich nach `origin/master` gepusht.

### Prompt 15

```text
ändere in unserem spiel gruppe 8 die ki sodass diese schwieriger ist und nicht bei jedem spiel die selbe vorgehensweise nutzt
```

Ergebnis: Die bestehende Gruppe-8-KI wurde durch eine Alpha-Beta-Suche, taktische Stellungsbewertung und zufällige Auswahl zwischen gleichwertigen Zügen verstärkt.

### Prompt 16

```text
speichere das in das varena trabert spiel mit hinein sodass wir nur ein spiel haben
```

Ergebnis: Die stärkere KI wurde in `G8_VierGewinntComputerPlayer` in `Varena_Trabert_VierGewinnt.cs` integriert. Die separate KI-Datei wurde entfernt und die bestehende Spielregistrierung wiederhergestellt.

### Prompt 17

```text
pushe den aktuellen stand
```

Ergebnis: Das Projekt wurde gebaut und die Änderungen am Gruppe-8-Spiel sowie dieses Prompt-Protokoll wurden auf `origin/master` gepusht.

### Prompt 18

```text
speicher weiterhin alle meiner promts in die jessica_seise_promts md datei mit dem genauen promt und kurz einem ergebnis dazu
```

Ergebnis: Der Wunsch wurde im Prompt-Protokoll festgehalten. Weitere Prompts in diesem Chat sollen mit dem genauen Wortlaut und einem kurzen Ergebnis ergänzt werden.

### Prompt 19

```text
[[PLAN]] erstelle einen plan für tests unseres programms bzw. unseres hayday spiels  damit wenn wir alleine oder zu mehr programmieren nicht unsere classes und objekte sich in die quere kommen. Dafür brauchen wir ein paar test erstelle hierfür erstmal einen plan und bleibe immer im selben interface
```

Ergebnis: Ein Markdown-Plan für spätere isolierte Hay-Day-Tests wurde erstellt. Der erste Umfang konzentriert sich auf Spiellogik, Regeln, Zustände und Ressourcen; UI-/Painter-Tests sind zunächst ausgeschlossen. Es wurden noch keine Tests implementiert.

### Prompt 20

```text
er soll bei den oopgames unter classes gruppe 8 in hayday gespeichert sein
```

Ergebnis: Der Hay-Day-Testplan wurde unter `OOPGames/OOPGames/Classes/Gruppe 8/Hay Day/HayDay_Testplan.md` gespeichert.

### Prompt 21

```text
ok in diesem plan sollen konzepte beinhaltet sein wie unsere codes und unser spiel getestet werden kann wir wollen später einfach sagen können führe einen test durch. was dieser test genau macht bzw was er prüft soll in dem plan festgeschrieben sein
```

Ergebnis: Der Testplan wurde um eine konkrete Testkonzeption erweitert: benannte Tests mit präzisen Prüfschritten und erwarteten Ergebnissen, Isolation von Spielzustand/Zeit/Speicher sowie einem einheitlichen späteren Testkommando. Tests wurden noch nicht implementiert.

### Prompt 22

```text
pushe den aktuellen stand
```

Ergebnis: Der aktuelle Stand mit dem Hay-Day-Testplan und diesem Prompt-Protokoll wurde für den Push vorbereitet.

### Prompt 23

```text
pull den aktuellen stand
```

Ergebnis: `master` wurde per Fast-Forward von `cff2fd0` auf `a2ab5a5` aktualisiert. Der lokale, nicht versionierte Ordner `HouseExample/JessicaSeise_HouseExample` blieb unberührt.

### Prompt 24

```text
pushe und pulle den aktuellen stand
```

Ergebnis: `master` wurde zuerst per Fast-Forward auf den aktuellen Remote-Stand `8f6b6e6` aktualisiert. Anschließend wurde dieses Prompt-Protokoll für den Push vorgemerkt.

## 2026-11-19

### Prompt 25

```text
Also die prompt und ergebnise weiterhin speichern. in unserem hayday spiel kann man aktuell nur weizen auf den feldern anbauen. bitte ändere dies sodass man auf ein feld klickt und sich dann ein neues menü öffnet vom aussehen her wie bei der futtermühle wo dann ausgefäh
```

Ergebnis: Das Saatgutmenü für Felder wurde ergänzt; der folgende Prompt präzisierte, dass darin die anzubauende Pflanze auswählbar sein soll.

### Prompt 26

```text
wo ausgewählt wird das angebaut werden soll
```

Ergebnis: Das Feldmenü bietet nun die Saatgutauswahl und pflanzt die ausgewählte Pflanze direkt auf dem Feld.

### Prompt 27

```text
ok super. ich habe noch keine anzeige wann das angepflanzte auf dem feld fertig sind da hätte ich gerne noch etwas das man sieht wann man die pflanzen ernten kann. das kann entweder eine anzeige direkt sein oder das die pflanzen sich optisch ändern
```

Ergebnis: Ein Ernte-Countdown und eine optische Reifeanzeige wurden ergänzt; unreife Pflanzen zeigen zusätzlich die verbleibende Zeit.

### Prompt 28

```text
Ok Mais ist wertvoller wie Weizen deswegen soll die zeit bis man es ernten kann bei Mais 3 minuten betragen
```

Ergebnis: Die Mais-Wachstumszeit wurde zunächst auf drei Minuten gesetzt. Sie wurde später auf Wunsch wieder auf zwei Minuten geändert.

### Prompt 29

```text
pushe das ganze
```

Ergebnis: Das Hay-Day-Update wurde in Commit `a1e8d5a` auf `origin/master` gepusht.

### Prompt 30

```text
pulle den aktuellen stand
```

Ergebnis: Der aktuelle Stand von `origin/master` wurde per Pull übernommen.

### Prompt 31

```text
pulle den aktuellen stand
```

Ergebnis: `master` wurde auf den aktuellen Stand `f9ddfb2` aktualisiert.

### Prompt 32

```text
pushe alles
```

Ergebnis: `master` war bereits mit `origin/master` auf `f9ddfb2` synchronisiert; es gab keine offenen Repository-Änderungen.

### Prompt 33

```text
ok nun funktioniert es nicht mehr das wir auswählen können welche pflanze wir anbauen wollen. außerdem möchte ich das man die tiere nur füttern kann mit der anzahl der tier die man auch hat also zum beispiel 5 hühner können mit 5 futter gefüttert werden und ich möchte eine andere darstellung die anzeigt wenn die tiere fertig mit der produktion ihrer sachen sind das man dann bei den hühnern zum beispiel sieht wenn die eier fertig sind
```

Ergebnis: Die Saatgutauswahl wurde wiederhergestellt, Futter wird passend zur Tierzahl verbraucht und der Tierproduktionsstatus wird angezeigt.

### Prompt 34

```text
es gibt eine mausbutton inkonsistens hayday humanplayer akzeptiert nur 0 und 1 und das framework übergibt 0 1 und 2 und wir wollen das der human player auch 0 1 und 2 akzeptiert. alles was aktuell mittlere maustaste ist soll auf die linke taste verlegt werden. wir wollen nur rechte und linke mastaste nutzen
```

Ergebnis: Der Human Player akzeptiert die Buttonwerte `0`, `1` und `2`; die mittlere Maustaste wird wie Linksklick behandelt.

### Prompt 35

```text
ändere die anzeige bei den tieren. wenn sie gefüttert sind dauert es 5 min um ein produkt zu produzieren dann haben sie wieder hunger und müssen wieder gefüttert werden. erst wenn sie gefüttert wurden produzieren sie auch wieder produkte
```

Ergebnis: Die Tieranzeige und der Fütterungszyklus wurden zunächst auf einen fünfminütigen Zeitraum angepasst.

### Prompt 36

```text
ok das füttern funktioniert noch nicht wenn ich auf füttern klicke passier gar nichts
```

Ergebnis: Der Fütterungspfad über den Stallmenü-Klick wurde korrigiert und getestet.

### Prompt 37

```text
ok der letzte befehl hat nicht funktioniert ich kann unsere tiere nicht füttern es passiert gar nichts wenn ich auf füttern kicke egal mit welcher taste
```

Ergebnis: Die Fütterungsoption im Menü wurde erneut repariert; Statusmeldungen und Futterverbrauch wurden überprüft.

### Prompt 38

```text
pushe alles
```

Ergebnis: Die Änderungen wurden in Commit `b2fc506` auf `origin/master` gepusht.

### Prompt 39

```text
entwickeln wir hier irgendwas auf einem branch?
```

Ergebnis: Es wurde bestätigt, dass direkt auf `master` gearbeitet wurde und kein lokaler Hay-Day-Feature-Branch bestand.

### Prompt 40

```text
ist alles gepushed?
```

Ergebnis: Die versionierten Änderungen waren auf `origin/master`; `master` stand auf `b2fc506`.

### Prompt 41

```text
gibt es datein in meinem spiel die verwendet werden und die nicht gepushed werden?
```

Ergebnis: Die verwendeten Hay-Day-Dateien wurden als versioniert geprüft; der unversionierte Ordner `HouseExample/JessicaSeise_HouseExample` lag außerhalb des Repositories.

### Prompt 42

```text
ok das mit den schafen füttern dann warten dann wolle einsammeln dann wieder füttern können funktioniert. nur bei den hühnern funktioniert dieser ablauf noch nicht ändere dies bitte. stelle auch sicher das die timer bei den tieren und bei den pflanzen kontinuierlich durchlaufen.
```

Ergebnis: Der Hühnerzyklus und die durchlaufenden Tier- und Pflanzentimer wurden korrigiert und mit 14 Hay-Day-Tests geprüft.

### Prompt 43

```text
Ok bei dem Lieferwagen sollen bestellungen sein mit denen wir münzen bekommen können. sobald eine bestellung abgegeben wurde soll schon eine neue verfügbar sein ändere dies bitte noch
```

Ergebnis: Nach erfolgreicher Abgabe erscheint sofort eine neue gespeicherte Bestellung mit Münzenbelohnung.

### Prompt 44

```text
ok es funktioniert noch nicht das wenn die tiere gefüttert wurden und der timer durchgelaufen ist das erst die produkte wie eier und wolle eingesammelt werden müssen und erst dann kann man sie wieder füttern
```

Ergebnis: Erneutes Füttern wurde gesperrt, solange noch fertige Tierprodukte im Stall eingesammelt werden müssen.

### Prompt 45

```text
bei den schafen hat es funktioniert bei den hühner aber nicht wenn ich sie gefüttert hab läuft der timer wenn der durch ist sind die wieder hungrig und ich kann keine eier einsammeln
```

Ergebnis: Eier können nun auch bei begrenztem Scheunenplatz teilweise eingesammelt werden; der verbleibende Stapel bleibt im Stall bereit.

### Prompt 46

```text
Es ist immernoch falsch. jetzt funktioniert der vorgang bei unserem schafstall nicht mehr. Bei unserem hühnerstall funktioniert es auch nich nicht wie es soll. Ähnlich wie bei den pflanzen soll es ablaufen. ich erkläre nochmal was genau ich will. man soll die tiere füttern dann läuft ein timer nach ablauf von diesem sind die tierspezifischen produkte bereit. diese müssen erst vom spieler eingesammelt werden. nach einsammeln der produkte sind die tiere wieder hungrig und können gefütterrt werden und so soll das bei den tieren ablaufen
```

Ergebnis: Der Ablauf wurde vereinheitlicht: füttern, fünf Minuten warten, Produkte bereitstellen, einsammeln und danach erneut füttern. 20 Tests waren erfolgreich.

### Prompt 47

```text
ok nun  muss noch die scheune und unser marktstand überarbeitet werden. in der scheune soll immer der aktuelle stand der produkte von den tieren undvon dem futter zu sehen sein. beim marktstand funktioniert das verkaufen noch nicht richtig. hier soll ebenfalls angezeigt werden wie viel von den verschiedenen produkten vorhanden ist
```

Ergebnis: Die Scheune zeigt Tierprodukte, Futtersorten und Lagerbelegung; der Markt zeigt Bestand und Stückpreis und aktualisiert Münzen und Bestand beim Verkauf.

### Prompt 48

```text
ändere noch bei den bestellungen das man entweder die bestellung abgeben kann oder das man falls man die produkte nicht hat das man die bestellung überspringen kann. und es soll nicht jede bestellung die selbe sein ein bisschen abwechslung wär gut
```

Ergebnis: Bestellungen können am Marktstand oder Lieferwagen abgegeben oder kostenfrei übersprungen werden. Produkt, Menge und Belohnung variieren.

### Prompt 49

```text
Mais soll bitte wie die anderen pflanzen nach 2 minuten ernte bereit sein
```

Ergebnis: Mais wächst nun wie die anderen Feldfrüchte zwei Minuten; der Test und die Spielregeln wurden aktualisiert.

### Prompt 50

```text
passe bitte noch unser silo an. hier sollen auch alle pflanzen mit ihrer anzahl angezeigt werden
```

Ergebnis: Das Silo zeigt die Mengen von Weizen, Mais, Karotten und Soja sowie die belegte Lagerkapazität.

### Prompt 51

```text
speichere alle promts in dem dokument von jessica seise promts mit dem genauen prompt und einer kurzen erklärung was dann gemacht wurde
```

Ergebnis: Die Hay-Day-Prompts aus diesem Verlauf wurden mit ihrem Wortlaut und einer kurzen Ergebnisbeschreibung in dieses Protokoll aufgenommen.
