# Prompt-Dokumentation – Jonas Frey

Diese Datei dokumentiert die in diesem Projektverlauf verfügbaren Prompts und ihre Ergebnisse.

**KI-Werkzeug:** Copilot SDK in VS Code
**Modell der älteren Sitzungsdaten:** nicht ausgewiesen
**Projekt:** OOPGames, Gruppe 9
**Arbeitsverzeichnis:** `OOPGames/OOPGames`

Ältere Prompts aus früheren Projektsitzungen sind bereits vollständig in
`Promts/Prompts_Till_Markhof.md` dokumentiert. Diese Datei ergänzt die aus der
lokalen Sitzungs-Historie nachweisbaren späteren Anfragen.

## Prompts aus den lokalen Projekt-Sitzungen

| Datum und Uhrzeit | Mein Prompt (Benutzer) | Ergebnis |
|---|---|---|
| 15.11.2026 14:49:37 | „für den Gruppe 9 code. Wenn man mit dem Character Kurt im stehen nach hinten schläget sollte er eigendlich eine animation mit "IdleAttackBackwards" ausführen. Diese animation exsistiert aber nicht. stattdesen spielt er einen teil von "WalkAttackBackwards". Wieso?“ | Ursache der Animation gesucht und die Abweichung zwischen Animator-Aufruf und vorhandenen Sprite-Sheet-Animationen analysiert. |
| 15.11.2026 14:56:17 | „Sind die namen der Animationen die im Painter aufgerufen werden abhänig davon welcher character ausgewählt ist?“ | Die Abhängigkeit der Animator-Namen vom ausgewählten Character geprüft und die Trennung von Character-Pfad und einheitlichen Animationsnamen erläutert. |
| 15.11.2026 15:10:50 | „In Zukunft sollen die namen der animationen in den Sprite sheets gleich sein. Du sollst den code so ändern das der ausgewählte character nur noch für den pfad der spritesheet datein relevant ist. Animationen sollen über die selben namen aufgerufen werden. Benutzte die namen aus Kurt. Den Sprite Sheets wurde eine animation namens "NotFound" hinzugefügt. Wird wenn das programm versucht eine animation abzuspielen die nicht im sheet vorhanden ist soll es stattdesen "NotFound" abspielen“ | Die Character-Auswahl wurde als Pfadangabe vorgesehen; die Animationen sollten über einheitliche Namen aufgerufen und fehlende Animationen durch `NotFound` ersetzt werden. |
| 15.11.2026 15:16:07 | „beim rückwertsschlaggen gibt es keine unterscheidung für wenn der player sich bewegt oder stillsteht. Füge ein IdleAttackBackwards hinzu“ | Eine eigene rückwärtige Idle-Angriffanimation wurde angefordert und in die Animationsbehandlung aufgenommen. |
| 18.11.2026 09:43:21 | „Die aufgeruffenen animationen für den backwards angriffe sowohl im laufen als auch im stehen werden bei Kurt nicht gefunden nicht gefunden. Weicht der im animator aufgeruffene animationsname von den namen in der json ab?“ | Die aufgerufenen Namen und die Einträge in der Kurt-Sprite-Sheet-JSON wurden abgeglichen und Abweichungen geprüft. |
| 18.11.2026 09:45:12 | „Liste alle namen von animationen auf die aufgeruffen werden können“ | Eine Liste aller aktuell aufrufbaren Animationen wurde angefordert. |
| 18.11.2026 10:01:50 | „ich habe die animationen für kurt überarbeitet. Lieste alle auf“ | Die aktualisierten Kurt-Animationen sollten erneut ausgelesen und vollständig aufgelistet werden. |
| 18.11.2026 10:04:50 | „Passe den code dem JSON an (konflikte mit boxingKnight können ignoriert wetrden)“ | Der Code sollte an die aktualisierte Kurt-JSON angepasst werden; Konflikte mit Boxing Knight durften ignoriert werden. |
| 18.11.2026 10:07:36 | „kannst du die namen der animationen in der JASON von boxing knight anpassen?“ | Die Animationsnamen in der Boxing-Knight-JSON sollten vereinheitlicht angepasst werden. |
| 18.11.2026 10:09:26 | „du solltest sie nicht entfernen sondern umbennenen“ | Die vorhandenen Boxing-Knight-Einträge sollten nicht gelöscht, sondern umbenannt werden. |
| 19.11.2026 07:56:57 | „löser den pull konflikt, überschreibe die datein und mache ein backup unter C:\Users\jfrey\MyFiles\Hochschule\Programmieren\Programieren3\savethat Das backup soll keine verbindung mit git haben“ | Einen Pull-Konflikt lösen und die lokalen Dateien überschreiben, wobei ein von Git unabhängiges vollständiges Backup unter `C:\Users\jfrey\MyFiles\Hochschule\Programmieren\Programieren3\savethat` angefordert wurde. |
| 19.11.2026 08:20:08 | „Ich habe den Gruppe9/Assets ordner um ein Background unterordner erweitert. Darin liegt das png Background1 nutzte es als hintergrund für die arena“ | Das Bild `Background1.png` sollte als Arena-Hintergrund eingebunden werden. |
| 19.11.2026 08:22:58 | „Momentan zeichnet der painter auf dem hintergrund ein gitter. Entferne das gezeichnete gitter“ | Das bisher vom Painter auf den Arena-Hintergrund gezeichnete Gitter sollte entfernt werden. |
| 19.11.2026 08:26:40 | „Kannst du die charactermodels upscalen? Also der character von jedem spielermodel größer machen?“ | Alle Character-Models sollten vergrößert dargestellt werden. |
| 19.11.2026 08:29:03 | „sieht gut aus upscale ihn noch etwas mehr (2, statt 1,5 fache der ausgangsgröße)“ | Die Character-Skalierung wurde auf das Zweifache der Ausgangsgröße statt 1,5-fach angefordert. |
| 19.11.2026 09:29:55 | „In Gruppe 9/Assets habe ich den Overlay Ordner Hinzugefügt. Er enthält Overlay 1 das das Mein ovelay sein wird, sowie weiter komponenten, wie healthybar und indicatoren für die abilities und ihren cooldown. Implementiere das neue Overlay. In Overlay 1 ist in der Linken unteren ecke ein kreis. In ihm soll ein bild des spielercharacters gezeigt werden“ | Das neue Arena-Overlay mit Lebensanzeige, Ability-Indikatoren und Cooldowns sollte eingebunden werden; unten links sollte der Spielercharacter im Kreis erscheinen. |
| 19.11.2026 09:43:24 | „Ein paar änderungen brauch ich: Der Maus schlag soll nicht als abillity mit cooldown angezeigt werden, beim cooldown soll der ability kreis sich von unten nach oben auffüllen, Bei der Character anzeige unten links muss der character einen layer höher als das overlay angezeigt werden“ | Der Mausschlag sollte kein Ability-Cooldown erhalten; Cooldowns sollten den Ability-Kreis von unten nach oben auffüllen; die Character-Anzeige sollte über dem Overlay liegen. |
| 19.11.2026 09:46:30 | „mach den health indicator rot statt grün“ | Der Gesundheitsindikator sollte rot statt grün dargestellt werden. |
| 19.11.2026 09:55:58 | „ich kann meine changes nicht pushen, ich soll git pull benutzten, das hab ich aber schon gemacht. Was wie ist das problem“ | Das Push-/Pull-/Merge-Problem wurde analysiert und eine Erklärung sowie Lösung des Git-Zustands angefordert. |
| 19.11.2026 09:58:07 | „can you resolve the conflicts for me“ | Die vorhandenen Git-Konflikte sollten automatisch geprüft und aufgelöst werden. |
| 22.11.2026 15:27:59 | „Dokumentiere alle vorherigen prompts aus diesem projekt in Gruppe9/Prompts nach dem Forbild von Promts_Till_Markhof.md (neue datei)“ | Diese Prompt-Dokumentation `Prompts_Jonas_Frey.md` wurde im vorhandenen Gruppe-9-Promptordner angelegt. |
| 22.11.2026 15:33:39 | „Dokumentiere alle weiteren prompts die im verlauf dieses Projekts entstehen in dieser datei“ | Festgehalten: Künftige Projektprompts des Benutzers und ihre Ergebnisse werden fortlaufend in dieser Datei ergänzt. |
