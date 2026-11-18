# Sounds – Musik und Soundeffekte

Drei Ordner, je nach Art des Tons:

| Ordner | Wann es läuft | Dateinamen (ohne Endung) |
|--------|---------------|--------------------------|
| `Musik_Aussen/` | Musik in der Stadt und beim Angeln | `town`, `fishing` |
| `Musik_Gebaeude/` | Musik in Zuhause, Läden, Bank, Casino, Spielhalle, Titel | `home`, `shop`, `casino`, `arcade`, `title` |
| `Soundeffekte/` | Geräusche bei Aktionen | `win`, `jackpot`, `lose`, `coin`, `catch`, `door`, `meow`, `woof`, `splash`, `click` |

Erlaubte Formate: `.wav` und `.mp3`. Liegt eine Datei mit passendem Namen im Ordner, wird sie benutzt. Fehlt sie,
erzeugt das Spiel den Ton selbst (eingebaute Chiptune-Version).

**Wichtig zum Start:** Das Spiel liest die Sounds aus `G1Casino_Assets\Sounds\` neben der `OOPGames.exe` (wie die
Grafiken). Diesen Ordner `Sounds` also nach dem Bauen dorthin kopieren. Eine automatische Kopie über die csproj ist
nicht vorgesehen, weil die Projektdatei nicht geändert werden darf.

Lizenz: Nur frei nutzbare Sounds ablegen und die Quelle hier notieren.

Quellen:
- (noch leer)
