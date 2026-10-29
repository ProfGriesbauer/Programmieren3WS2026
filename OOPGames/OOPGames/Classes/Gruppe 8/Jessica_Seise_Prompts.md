# Jessica Seise – Prompt-Protokoll

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
