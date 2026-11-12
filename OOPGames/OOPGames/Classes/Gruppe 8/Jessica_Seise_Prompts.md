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
