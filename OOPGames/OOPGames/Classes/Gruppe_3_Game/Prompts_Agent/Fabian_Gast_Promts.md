# Prompts von Fabian Gast

## Prompt 1

**Wortlaut:**
> Kannst du mir ein 2d retro drag racing spiel machen, das spiel soll in seiten ansicht sein, die assets von den autos sind in asstes/gruppe_3_game zu finden, bei start soll ein timer abzählen, danach fährt player 1 mit w = beschleunigen, s = bremsen und player 2 mit numpad 8 =beschleunigen und nampad 5 = bremsen, mache jeweils einen computer und human player, die gegeneinader fahren.

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 5.6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Grundlegende Spielinitialisierung, Erstellung der Klassen für Rules, Field, Painter, Input und Move für das Retro Drag Race.

## Prompt 2

**Wortlaut:**
> Ich habe es jetzt im MainWindow ein gebunden, jetzt kommt beim starten der Fehler: Unhandled exception. System.Windows.Markup.XamlParseException: Zeilennummer "6" und Zeilenposition "9" von "Durch den Aufruf des Konstruktors f�r Typ "OOPGames.MainWindow", der den angegebenen Bindungseinschr�nkungen entspricht, wurde eine Ausnahme ausgel�st.".

> ---> System.IO.IOException: Die Ressource "assets/gruppe_3_game/autos_assets.png" kann nicht gefunden werden.

>   at MS.Internal.AppModel.ResourcePart.GetStreamCore(FileMode mode, FileAccess access)

>   at System.IO.Packaging.PackagePart.GetStream(FileMode mode, FileAccess access)

>   at MS.Internal.IO.Packaging.PackagePartExtensions.GetSeekableStream(PackagePart packPart, FileMode mode, FileAccess access)

>   at System.IO.Packaging.PackWebResponse.CachedResponse.GetResponseStream()

>   at System.Windows.Media.Imaging.BitmapDecoder.SetupDecoderFromUriOrStream(Uri uri, Stream stream, BitmapCacheOption cacheOption, Guid& clsId, Boolean& isOriginalWritable, Stream& uriStream, UnmanagedMemoryStream& unmanagedMemoryStream, SafeFileHandle& safeFilehandle)

>   at System.Windows.Media.Imaging.BitmapDecoder.CreateFromUriOrStream(Uri baseUri, Uri uri, Stream stream, BitmapCreateOptions createOptions, BitmapCacheOption cacheOption, RequestCachePolicy uriCachePolicy, Boolean insertInDecoderCache)

>   at System.Windows.Media.Imaging.BitmapImage.FinalizeCreation()

>   at OOPGames.Gruppe3DragRacePainter..ctor() in `C:\Users\Gast Fabian\Documents\Studium\3. Semster\Programmieren 3\Programmieren3WS2026\OOPGames\OOPGames\Classes\Gruppe_3_Game\Gruppe3DragRacePainter.cs:line 16`

>   at OOPGames.MainWindow..ctor() in `C:\Users\Gast Fabian\Documents\Studium\3. Semster\Programmieren 3\Programmieren3WS2026\OOPGames\OOPGames\MainWindow.xaml.cs:line 45`

>   at System.RuntimeType.CreateInstanceDefaultCtor(Boolean publicOnly, Boolean wrapExceptions)

>   --- End of inner exception stack trace ---

>   at System.Windows.Markup.XamlReader.RewrapException(Exception e, IXamlLineInfo lineInfo, Uri baseUri)

>   at System.Windows.Markup.WpfXamlLoader.Load(XamlReader xamlReader, IXamlObjectWriterFactory writerFactory, Boolean skipJournaledProperties, Object rootObject, XamlObjectWriterSettings settings, Uri baseUri)

>   at System.Windows.Markup.WpfXamlLoader.LoadBaml(XamlReader xamlReader, Boolean skipJournaledProperties, Object rootObject, XamlAccessLevel accessLevel, Uri baseUri)

>   at System.Windows.Markup.XamlReader.LoadBaml(Stream stream, ParserContext parserContext, Object parent, Boolean closeStream)

>   at System.Windows.Application.LoadBamlStreamWithSyncInfo(Stream stream, ParserContext pc)

>   at System.Windows.Application.DoStartup()

>   at System.Windows.Application.<.ctor>b__1_0(Object unused)

>   at System.Windows.Threading.ExceptionWrapper.InternalRealCall(Delegate callback, Object args, Int32 numArgs)

>   at System.Windows.Threading.ExceptionWrapper.TryCatchWhen(Object source, Delegate callback, Object args, Int32 numArgs, Delegate catchHandler)

>   at System.Windows.Threading.DispatcherOperation.InvokeImpl()

>   at MS.Internal.CulturePreservingExecutionContext.CallbackWrapper(Object obj)

>   at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)

> --- End of stack trace from previous location ---

>   at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state)

>   at MS.Internal.CulturePreservingExecutionContext.Run(CulturePreservingExecutionContext executionContext, ContextCallback callback, Object state)

>   at System.Windows.Threading.DispatcherOperation.Invoke()

>   at System.Windows.Threading.Dispatcher.ProcessQueue()

>   at System.Windows.Threading.Dispatcher.WndProcHook(IntPtr hwnd, Int32 msg, IntPtr wParam, IntPtr lParam, Boolean& handled)

>   at MS.Win32.HwndWrapper.WndProc(IntPtr hwnd, Int32 msg, IntPtr wParam, IntPtr lParam, Boolean& handled)

>   at MS.Win32.HwndSubclass.DispatcherCallbackOperation(Object o)

>   at System.Windows.Threading.ExceptionWrapper.InternalRealCall(Delegate callback, Object args, Int32 numArgs)

>   at System.Windows.Threading.ExceptionWrapper.TryCatchWhen(Object source, Delegate callback, Object args, Int32 numArgs, Delegate catchHandler)

>   at System.Windows.Threading.Dispatcher.LegacyInvokeImpl(DispatcherPriority priority, TimeSpan timeout, Delegate method, Object args, Int32 numArgs)

>   at System.Windows.Threading.Dispatcher.Invoke(DispatcherPriority priority, Delegate method, Object arg)

>   at MS.Win32.HwndSubclass.SubclassWndProc(IntPtr hwnd, Int32 msg, IntPtr wParam, IntPtr lParam)

>   at MS.Win32.UnsafeNativeMethods.DispatchMessage(MSG& msg)

>   at System.Windows.Threading.Dispatcher.PushFrameImpl(DispatcherFrame frame)

>   at System.Windows.Application.RunDispatcher(Object ignore)

>   at System.Windows.Application.RunInternal(Window window)

>   at OOPGames.App.Main()

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 5.6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Korrektur der Asset-Pfade und Build Action (Resource), sodass das Spiel im MainWindow korrekt startet.

## Prompt 3

**Wortlaut:**
> mache, dass die autos wie jetzt an der linken seite vom bilschirm starten, dann bis in die mitte vom bildschirm fahren und sich dann die strecke mit bewegt und damit ein bischen läger ist

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 5.6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Implementierung des Kamera-Scrollings und Startpositionen am linken Bildschirmrand mit Bewegung bis zur Mitte.

## Prompt 4

**Wortlaut:**
> Mach das das Eck so aussieht, ein schöner scharfer 90° WInkel und eine 45° Diagonale, welche beide kanten trennt

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 5.6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Anpassung der UI-Rahmen und Eckenformen im Painter.

## Prompt 5

**Wortlaut:**
> es sieht immer noch anderst aus, mach es genau so wie auf dem Bild

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 5.6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Feintuning der WPF-Geometrie für die UI-Boxen.

## Prompt 6

**Wortlaut:**
> kannst du mir unten die steuerung anzeigen (player 1 w & s, player 2 8 & 5 bzw. ↑ & ↓) bei der Auswahl der autos und kannst ab start die sterung durch einen fortschrittsbalken ersetzen

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 5.6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Hinzufügen von Steuerungshinweisen bei der Fahrerauswahl und Umschalten auf einen Fortschrittsbalken beim Rennstart.

## Prompt 7

**Wortlaut:**
> mach den fortschrittsbalken gemeinsam, also das dieser ohne eine bezeichnung der player und beide fortschrittsbalken direkt übereinader liegen

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 5.6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Zusammenlegen der Fortschrittsbalken.

## Prompt 8

**Wortlaut:**
> Kannst du den fortschrittsbalken so machen, dass nicht beide fortschrittsbalken überlagert werden sondern untereinander liegen

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 5.6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Anordnung der Fortschrittsbalken untereinander.

## Prompt 9

**Wortlaut:**
> kannst du die steuerungshinweise auch skalieren, das bei kleinen fenstergrößen nicht abgeschnitten ist

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 5.6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Responsives Skalieren der Steuerungshinweise für kleine Fenstergrößen.

## Prompt 10

**Wortlaut:**
> kannst du die auto auswahl auch verbessern, so dass bei kleineren fenstergrößen der rechte rand der auswahlfenster wie links ein bisschen abstand zum rand hat

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 5.6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Layout-Verbesserung der Fahrzeugauswahl mit symmetrischen Rändern.

## Prompt 11

**Wortlaut:**
> kannst du mir einen tacho hinzufügen und diesen eine motordrehzahl und die geschwindigkeit anzeigen lassen

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** Mai Code 2 Flash (Copilot Fusion)
- **Ergebnisvermerk:** Grundlegende Tacho-Anzeige für Drehzahl und Geschwindigkeit implementiert.

## Prompt 12

**Wortlaut:**
> füge jetzt die möglichkeit hinzu für player 1 mit q und e hoch bzw runter zuschalten und für player 2 mit 7, 9 oder bildup ober bilddown

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** Mai Code 2 Flash (Copilot Fusion)
- **Ergebnisvermerk:** Gangschaltungstasten für Player 1 (`Q`/`E`) und Player 2 (`7`/`9`, `Bild Auf`/`Bild Ab`) hinzugefügt.

## Prompt 13

**Wortlaut:**
> mach die gangschaltung realistisch, im ersten gang soll das auto schneller beschleunigen, aber die drehzahl auch schnell hochgehen, füge diesen effekt in allen gängen hinzu, immer weniger stark als im vorherigen gang. wenn der player nicht hochschaltet, wenn das auto bei voller drehzahl fährt, soll die drehzahlnadel minimals zittern und das auto nicht weiter beschleunigen

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** Mai Code 2 Flash (Copilot Fusion)
- **Ergebnisvermerk:** Realistische Getriebeabstufung, Drehzahlanstieg, Begrenzer-Zittern bei Maximaldrehzahl ohne Hochschalten.

## Prompt 14

**Wortlaut:**
> im assets ordner sind jetz drei autos, füge diese hinzu und lösche die anderen, in jedem ordner sind ein hintergrund für ein kombiinstrument sowie die tachonadeln dazu, der drehpunkt auf dem kombiinstrument für die nadeln ist ein lilaner punkt. mach es so, dass je nach auto das ausgewählt ist das jeweilige kombiinstrument angezeigt wird.

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** Mai Code 2 Flash (Copilot Fusion)
- **Ergebnisvermerk:** Integration der drei neuen Autos samt spezifischer Kombiinstrument-Hintergründe und lila Referenzpunkten (`#e100ff`).

## Prompt 15

**Wortlaut:**
> lösche die alten tachonadeln, mache zudem , das wenn man runterschaltet, die drehzahl hochschnellt und das auto auf die maximalgeschwindigkeit in diesem gang runter bremst

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** Mai Code 2 Flash (Copilot Fusion)
- **Ergebnisvermerk:** Entfernung alter Standardnadeln und Einbau von Motorbrems- und Drehzahlschnell-Effekten beim Herunterschalten.

## Prompt 16

**Wortlaut:**
> entferne den text unter den tachos (den player die geschwindigkeit drehzahl und den gang) mache die tachos um einiges größer

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** Mai Code 2 Flash (Copilot Fusion)
- **Ergebnisvermerk:** Text unter den Tachos entfernt, Tachos größer skaliert.

## Prompt 17

**Wortlaut:**
> mache den fortschrittsbalken in der vertikalen wesentlick kleiner und lasse die tachos dynamisch an die fenster größe skalieren

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** Mai Code 2 Flash (Copilot Fusion)
- **Ergebnisvermerk:** Fortschrittsbalken verkleinert, Tachos dynamisch größenanpassbar gemacht.

## Prompt 18

**Wortlaut:**
> mach, dass die kombiinstrumente, immer so skaliert werden, dass sie nicht über den fensterrand (aktuell sind sie bei der normalen ansciht zu groß)

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** Mai Code 2 Flash (Copilot Fusion)
- **Ergebnisvermerk:** Begrenzung und dynamische Skalierung der Kombiinstrumente gegen Überstand über den Fensterrand.

## Prompt 19

**Wortlaut:**
> Kannst du mir die ganzen Promts in dieser Session in eine Fabian_Gast_Promts.md schreiben

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 5.6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Erstellung der initialen `Fabian_Gast_Promts.md` mit allen bisherigen Prompts der Session.

## Prompt 20

**Wortlaut:**
> Mache eine Gruppe3DragRaceCars.cs datei und füge dort alles, was mit den autos zu tun hat (offsets der tachonadeln zu den tachos (also die position der lila punkte), gänge, maximaldrehzahl auflösung des tachos (drehzahl/geschwindigkeit bereich und winkel bereich) und sonst noch was alles so sinnvoll wäre

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 5.6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Erstellung der `Gruppe3DragRaceCars.cs` zur Kapselung von Fahrzeugdaten und Tacho-Metriken.

## Prompt 21

**Wortlaut:**
> Mache, dass die Autos alle Objekte der Klasse Cars sind und die Werte also (offsets der tachonadeln zu den tachos (also die position der lila punkte), gänge, maximaldrehzahl auflösung des tachos (drehzahl/geschwindigkeit bereich und winkel bereich) usw. bei jeden auto anderst sein können

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 5.6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Objektorientierte Modellierung der Fahrzeuge als Instanzen der Klasse `Cars` mit individuellen Parametern.

## Prompt 22

**Wortlaut:**
> Mach, dass die ganzen werte in konstruktor des autos gesetzt werden könnnen, auserdem will ich wissen, was welcher wert ist, nicht wie jetzt eine aufreihung der werte untereinader ohne bezeichnung irgenwas

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 5.6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Refactoring des Auto-Konstruktors auf benannte Parameter bzw. lesbare Zuweisungen.

## Prompt 23

**Wortlaut:**
> Die Bildmitte der tachonadeln ist der drehpunkt, dieser sollte auf dem lila punkt an tacho liegen. Zudem ist die skalierung der tachonadeln genau auf den tacho abgestimmt (die tachonadeln müssen mit dem tacho 1 zu 1 skaliert werden)

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 5.6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Korrektur des Rotationsmittelpunkts und 1:1 Skalierung der Tachonadeln auf die Cockpit-Anzeigen.

## Prompt 24

**Wortlaut:**
> die tachonadeln sind ja jetzt noch weiter vom lila punkt verschoden wie davor, die lila punkte haben alle den farbwert von #e100ff und sind 2x2 pixel groß

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 5.6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Präzise Bestimmung und Offset-Korrektur anhand des lila Referenzpunkts (`#e100ff`, 2x2 Pixel).

## Prompt 25

**Wortlaut:**
> beim golf ist der drehzahlmesser und der tacho invertiert

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 5.6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Unterstützung für invertierte Tachometer- und Drehzahlmesseranordnung beim Golf-Modell.

## Prompt 26

**Wortlaut:**
> Die Autos haben alle die gleiche Max Geschwindigkeit. Alle Nadeln sind bei 0 Grad in der Statrt position (geschwindigkeit null, drehzahl standgas) füge daher hinzu, dass im Konstruktor der Maximal winkel der beiden nadeln implementiert wird, zudem die geschwindigkeit und drehzahl in diesem winkel

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Ergänzung von Maximalwinkeln sowie Skalierungs-Zonen für Nadeln im Auto-Konstruktor.

## Prompt 27

**Wortlaut:**
> Ich habe jetzt die Werte angepasst und die für die Turbo nadel eingefügt, kannst du dazu die Turbonadel noch einfügen. Die Nadeln drehen alle gegen den uhrzeigersinn. die autos sollen 300 km/h fahren, wenn die maximalgeschwindigkeit von tacho kleiner ist soll dieser bei der maximal geschwindigkeit stehen bleiben

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Integration der Turbo-Nadel und Kapselung der 300 km/h Endgeschwindigkeits-Begrenzung bei Tachos mit niedrigerem Skalenendwert.

## Prompt 28

**Wortlaut:**
> Was macht maxSpeed: 17.0,?

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Erklärung des Parameters `maxSpeed` in den Fahrzeugkonfigurationen.

## Prompt 29

**Wortlaut:**
> entferne das, ich habe doch gesagt, dass alle autos genau gleich schnell sind. Ausserdem kann ich es nicht starten fixe das

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Entfernung überflüssiger Parameter und Fehlerbehebung beim Startvorgang.

## Prompt 30

**Wortlaut:**
> Nein, du sollt nichts am Factrorio spiel ändern!!!

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Kurskorrektur: Code-Änderungen streng auf Gruppe 3 Drag Race begrenzt.

## Prompt 31

**Wortlaut:**
> Ich habe jetzt das Factorio Spiel im MainWindow auskommentiert, warum geht es immer noch nicht

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Analyse von MainWindow-Einbindungen und Behebung von Startproblemen.

## Prompt 32

**Wortlaut:**
> Gut, dann ändere dass im Factorio Spiel

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Anpassung im MainWindow zur korrekten Spiel-Initialisierung.

## Prompt 33

**Wortlaut:**
> Hab mich vertan, die nadeln drehen alle im Uhrzeigersinn

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Korrektur der Drehrichtung aller Tachonadeln auf Uhrzeigersinn.

## Prompt 34

**Wortlaut:**
> Mach, dass die autos erst die höchst geschwindigkeit kurz vor der ziellinie erreichen

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Anpassung der Geschwindigkeits- und Beschleunigungskurve, sodass Vmax erst gegen Ende des Drag Strips anliegt.

## Prompt 35

**Wortlaut:**
> Mach, das ich bereits, wenn die zum start heruntergezählt wird, gas geben kann, also das auto im neutral gang beschleunigt und wenn ich vor der timer abgelaufen ist verloren habe und oben die meldung mit Fehlstart kommt

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Implementierung der Countdown-Gasannahme und Fehlstart-Erkennung (Spieler verliert bei vorzeitiger Aktion).

## Prompt 36

**Wortlaut:**
> Mache, das bei einem Fehlstart und am ende vom rennen die autos langsam ausrollen, ändere zudem die abstufung der getriebe so, dass der drehzahl unterschied der gänge zwischen 1500 und 500 rpm liegt und je höher der gang wird im kleiner wird (aber immer noch in dem Bereich von 1500-500)

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Ausroll-Physik bei Fehlstart/Ziel und realistische Getriebeabstufung implementiert.

## Prompt 37

**Wortlaut:**
> Mach, dass es eine art leistungskurve gibt, die von standgas bis kurz vor maximal drehzahl linear ansteigt und kurz vor der maximaldrehzahl wieder abflacht. Mach zudem, dass der computer zufallig rund um diese optimal drehzahl schaltet, also manchmal davor, manchmal danach. Mache das gleiche beim losfahren, also das er manchmal einen fehlstart macht (aber eher sehr selten 10% ca) und eher bei start ein bischen zuspät schaltet also ca mit einer durchschnitts reaktionszeit von 250 ms

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Motoren-Leistungskurve (linear mit Abflachung vor Vmax) sowie intelligentes KI-Schaltverhalten mit Reaktionszeiten und seltenen Fehlstarts hinzugefügt.

## Prompt 38

**Wortlaut:**
> Mach das der Computer auch am start gas gibt mach, dass das rennen nach ca 7,5 sekunden durchschnittlich endet (mit endgeschwindigkeit ca 300 kmh) skaliere so die geschwindigkeit, das die 7,5 sekunden bei der akutellen strecken länge ist. Mach zudem dass das Auto beim fahren etwas überdrehen kann dann aber kaum leistung hat und dadurch auch etwas schneller als die 300 km/h am ende sein kann

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** KI-Startgas, Zielzeit ca. 7.5s bei ~300 km/h, Überdrehschutz und Leistungsabfall bei rotem Drehzahlbereich integriert.

## Prompt 39

**Wortlaut:**
> Ich habe jetzt eine Font in Asstets/Gruppe_3_Game/Font eingefügt, kannst du diese für das Spiel nutzen. Mach zudem, das nach dem ende für jeden player die zeit und die endgeschwindigkeit, sowie die reaktionszeit in ms nebeneinader angezeigt werden.

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Integration der Custom Font aus `Assets/Gruppe_3_Game/Font` und Anzeige von Rennzeit, Endgeschwindigkeit und Reaktionszeit im Zielbildschirm.

## Prompt 40

**Wortlaut:**
> Was musst du an OOPGames.csproj ändern?

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Erklärung der Notwendigkeit des Einbindens von neuen C#-Quelldateien und Asset-Dateien in die `.csproj`.

## Prompt 41

**Wortlaut:**
> Geht das auch ohne dort was zu ändern?

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Erläuterung, dass C#-Dateien im Projekt registriert sein müssen, während Asset-Dateien über Build Actions (Resource) eingebunden werden.

## Prompt 42

**Wortlaut:**
> Invertiere die schriftart (also weiße schrift schwarzer schaten) und mache den rand des fensters am ende eckig und etwas dicker mache zudem den auto auswählen schriftzug etwas höher und ersetze das ä zu ae. auserdem haben die autos nach der halben strecke bereits die höchstgeschwindigkeit erreicht, mache das sich die autos wesenlich schneller auf der strecke bewegen, so das die weißen striche kaum unterscheidbar sind bei fullspee, skaliere die strecke dementsprechend

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Schriftfarben angepasst, Umlaute korrigiert ("AE"), Streckenlänge und Physik-Beschleunigungsprofil angepasst, damit Höchstgeschwindigkeit erst gegen Ende erreicht wird und Motion-Blur-Effekt bei den Streckenstrichen entsteht.

## Prompt 43

**Wortlaut:**
> der computer kommt auf eine zeit von ca 10s, die strecke ist viel zu lang, ausserdem , wenn ich sie manuell kürzer mache komme ich auf 6,5 s und die autos erreichen immer noch weit vor der zielinie die maximalgeschwindigkeit. Und der Text ist immer noch nicht invertiert

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Feintuning von Streckenlänge, Beschleunigungskurve und Rundenzeiten für realistische ca. 7.5 Sekunden Drag Races.

## Prompt 44

**Wortlaut:**
> Vergiss das mit der Text invertierung, lösche alles in der richtung und ändere die text farbe in schwarz. Kannst du zudem die Fabian_Gast_Promts.md in Gruppe 3 Game für diese Session aktualisieren

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Textfarbe auf Schwarz umgestellt, Invertierungslogik entfernt und Dokumentation der Prompts initial angelegt.

## Prompt 45

**Wortlaut:**
> Geht es den durchsichtigen teil des textes weiß zu machen

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Prüfung von WPF Text-Render-Optionen für transparente Effekte.

## Prompt 46

**Wortlaut:**
> Kannst du mir das Spiel besser struktorieren, so dass nicht alles in den Rules drin steht und sinnvoll aufgeteilt ist

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** Google Gemini 4 Hydrogen
- **Ergebnisvermerk:** Entwurf zur Modularisierung der `Gruppe3DragRaceRules.cs` in separate Klassen (Physics, RacerState, AI, etc.).

## Prompt 47

**Wortlaut:**
> Der Agent ist gerade mitten in der Anfrage abgestürtzt, kannst du weiter machen? Der Agent hat die Rules Datei schon aufgeteil und war gerade dabei die Rules Datei auszumissten, also alle ausgelagerten sachen zu löschen

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** GPT 6 Luna (Copilot Fusion)
- **Ergebnisvermerk:** Fortsetzung der Refaktorisierung nach Unterbrechung, Bereinigung von redundantem Code in `Gruppe3DragRaceRules.cs` und vollständige Aufteilung in spezialisierte Klassen (Physics, RacerState, Input, Painter).

## Prompt 48

**Wortlaut:**
> Kannst du mir in Gruppe_3_Game/Prompts_Agent eine Gruppe_3_Plans.md Datei erstellen, mit dem Spielkonzept

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** Google Gemini 3.5 Flash-Lite
- **Ergebnisvermerk:** Erstellung der `Gruppe_3_Plans.md` im Ordner `OOPGames/Classes/Gruppe_3_Game/Prompts_Agent/` mit dem vollständigen Spielkonzept, Architektur, Regeln, Phasen, Steuerung und Ausblick.

## Prompt 49

**Wortlaut:**
> Kannst du das in der Fabian_Gast_Prompts.md umsetzen?: Wortlaut, Datum, Werkzeug/Modell und kurzem Ergebnisvermerk.

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** Google Gemini 3.5 Flash-Lite
- **Ergebnisvermerk:** Umfassende Aktualisierung und Formatierung der `Fabian_Gast_Promts.md` mit strukturierten Angaben (Wortlaut, Datum, Werkzeug/Modell, Ergebnisvermerk) für alle Prompts 1 bis 48.

## Prompt 50

**Wortlaut:**
> Kannst du diese Session auch mit einbinden

- **Datum:** 2026-11-25
- **Werkzeug/Modell:** Google Gemini 3.5 Flash-Lite
- **Ergebnisvermerk:** Vollständige Integration der aktuellen Session-Prompts (Erstellung des Projektplans `Gruppe_3_Plans.md` und finale Dokumentationspflege in `Fabian_Gast_Promts.md`).
