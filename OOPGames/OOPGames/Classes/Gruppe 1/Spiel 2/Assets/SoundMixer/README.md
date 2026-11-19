# Sound-Mixer – die Handy-App

Stellt alle Sounds des Spiels vom Handy aus ein: Lautstärke (gesamt, Musik, Effekte, je Sound), Häufigkeit (Wahrscheinlichkeit
und Mindestabstand), wie viele Effekte höchstens gleichzeitig hörbar sind, Vogel-Radius, Stumm und Probehören (▶ spielt den Ton
auf dem PC).

## Benutzen

1. Spiel starten, Menü **B** öffnen, **F11** drücken. Es erscheinen Adresse und vierstellige PIN.
2. Handy und PC im **selben WLAN**. Im Handy-Browser die Adresse öffnen (z. B. `http://192.168.1.23:8765/`) und die PIN
   eingeben. Beim ersten Start fragt die Windows-Firewall nach Freigabe für das private Netzwerk.
3. Regler bewegen: Änderungen gelten sofort und werden gespeichert (`%LocalAppData%\G1Casino\sound_settings.json`).
4. Beenden: wieder Menü **B**, **F11**. Der Server läuft nur, solange du ihn eingeschaltet hast.

Als App ablegen: im Handy-Browser „Zum Startbildschirm hinzufügen“. Es ist eine Web-App (kein APK/IPA); sie braucht nichts
zu installieren.

## Was einstellbar ist

| Einstellung | Wirkung |
|-------------|---------|
| Gesamt / Musik / Effekte | Endlautstärke = Gesamt × (Musik oder Effekte) × Einzelwert |
| Vogel-Radius | Vögel sind nur innerhalb dieser Kachelzahl zu hören (Standard 4) |
| Gleichzeitig | Höchstzahl gleichzeitiger Effekte (1–8). Wichtigere Töne verdrängen unwichtigere; derselbe Ton überlagert sich nie |
| Je Sound: Lautstärke | 0–100 % |
| Je Sound: Häufigkeit | Wahrscheinlichkeit, dass ein Auslöser hörbar wird (0–100 %) |
| Je Sound: Mindestabstand | Pause zwischen zwei Wiedergaben (0–30 s) |
| Je Sound: Ein/Aus | Sound ganz abschalten |

## Dateien

| Datei | Inhalt |
|-------|--------|
| `Patrick_Lupierz_SoundCatalog.cs` | alle Sounds: Id, Name, Kategorie, Ordner, Dateimuster, Standardwerte, Priorität |
| `Patrick_Lupierz_SoundSettings.cs` | Einstellungen, Grenzen, JSON speichern/laden |
| `Patrick_Lupierz_SoundMixerServer.cs` | kleiner HTTP-Server (System.Net.Sockets), PIN, Anfragen |
| `Patrick_Lupierz_SoundMixerPage.cs` | die Handy-Seite (HTML/CSS/JS in einem Textblock) |

Die Klangsteuerung selbst (was wann wie laut, Grenzen, Schleifen) steht in `Systems/Patrick_Lupierz_SoundSystem.cs`.

## Sicherheit

Der Server startet nur auf Tastendruck, ist nur im lokalen Netz erreichbar, verlangt für alles unter `/api/` die PIN und sperrt
nach 8 Fehlversuchen für eine Minute. Er liest oder schreibt keine Dateien über die Adresse. Nicht im Uni-/Gast-WLAN mit
fremden Geräten einschalten.

## Neuen Sound einbauen

1. Datei in `Assets/Sounds/<Ordner>` legen.
2. In `Patrick_Lupierz_SoundCatalog.cs` einen Eintrag ergänzen (Muster für den Dateinamen, Standardlautstärke, Priorität).
3. Im Code `Sound.Play("<id>")` aufrufen (Minispiele: `Sfx("<id>")`; Schleifen mit `"<id>:stop"` beenden).
Der Mixer zeigt ihn automatisch an.
