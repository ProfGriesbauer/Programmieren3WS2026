using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    // Verantwortlich nur fuer Darstellung: liest Welt und Spielfeld und zeichnet Canvas-Elemente.
    // Platzierungs-, Produktions- oder Lageraenderungen werden hier nie direkt ausgefuehrt.
    public sealed class G5_FabrikPainter : IG5_FabrikPainter
    {
        private sealed class GebaeuderegisterEintrag
        {
            internal GebaeuderegisterEintrag(string name, G5_Bauwerkzeug werkzeug)
            {
                Name = name;
                Werkzeug = werkzeug;
            }

            internal string Name { get; }
            internal G5_Bauwerkzeug Werkzeug { get; }
        }

        private const double StandardKachelGroesse = 32.0;
        private const double HudZeilenHoehe = 28.0;
        private const double HudHoehe = HudZeilenHoehe * 2;
        private const double VollbildSeitenverhaeltnis = 1.3;
        private static readonly GebaeuderegisterEintrag[] GebaeuderegisterEintraege =
        {
            new GebaeuderegisterEintrag("Förderer", G5_Bauwerkzeug.Foerderer),
            new GebaeuderegisterEintrag("Schmelze", G5_Bauwerkzeug.Schmelze),
            new GebaeuderegisterEintrag("Förderband", G5_Bauwerkzeug.Foerderband),
            new GebaeuderegisterEintrag("Konstruktor", G5_Bauwerkzeug.Konstruktor),
            new GebaeuderegisterEintrag("Montagewerk", G5_Bauwerkzeug.Assembler),
            new GebaeuderegisterEintrag("Kohlekraftwerk", G5_Bauwerkzeug.Verbrennungsgenerator),
            new GebaeuderegisterEintrag("Strommast", G5_Bauwerkzeug.Strommast),
            new GebaeuderegisterEintrag("Lager", G5_Bauwerkzeug.Lager),
            new GebaeuderegisterEintrag("Rakete", G5_Bauwerkzeug.Rakete)
        };

        private static readonly Brush HoverPanelBrush = CreateBrush(245, 20, 30, 31);
        private static readonly Brush HoverAccentBrush = CreateBrush(255, 142, 230, 120);
        private static readonly Brush TransportBoxBrush = CreateBrush(255, 177, 123, 61);
        private static readonly Brush TransportBoxEdgeBrush = CreateBrush(255, 76, 49, 28);
        private static readonly Brush TransportBoxStripeBrush = CreateBrush(255, 225, 190, 122);
        private static readonly ImageSource SpielerBild = ErzeugeSpielerBild();
        private static readonly IReadOnlyList<G5_Recipe> Handwerksrezepte =
            G5_StarterRecipes.CreateManualCraftingRecipes();
        private static readonly Dictionary<(string Text, FontWeight Gewicht), double> KostenTextBreiten =
            new Dictionary<(string, FontWeight), double>();

        private readonly G5_WieseHintergrund _wieseHintergrund = new G5_WieseHintergrund();
        private IG5_WeltSpielfeld _hintergrundWelt;
        private Canvas _hintergrundEbene;
        private Image _hintergrundBild;
        private Canvas _dynamischeEbene;
        private Canvas _hoverEbene;
        private Canvas _zielCanvas;
        private IG5_WeltSpielfeld _hoverWelt;
        private IReadOnlyList<IG5_GebaeudeInfo> _hoverGebaeude;
        private Border _hoverKarte;
        private string _hoverName;
        private Point _letzteMausposition;
        private bool _hatMausposition;
        private double _hintergrundViewportBreite = -1;
        private double _hintergrundViewportHoehe = -1;
        private G5_Bauwerkzeug _draggedBuildTool;
        private bool _isDraggingBuild;
        private bool _mouseDownWhileStatisticsOpen;
        private bool _mouseDownOnBuildingMenu;
        private G5_GridPosition? _conveyorDragStart;
        private G5_GridPosition? _conveyorDragEnd;

        public string Name => "G5 Fabrik-Welt";

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            if (!(currentField is IG5_WeltSpielfeld welt))
            {
                throw new ArgumentException("Der Painter benötigt ein Gruppe-5-Weltspielfeld.", nameof(currentField));
            }

            if (currentField is SatisfactoryFieldView satisfactoryField &&
                !satisfactoryField.IsGameStarted)
            {
                _hoverWelt = null;
                _hoverGebaeude = null;
                _hatMausposition = false;
                ClearGebaeudeHover();
                satisfactoryField.StartScreen.Paint(canvas);
                return;
            }

            PaintWeltField(canvas, welt);
            if (currentField is SatisfactoryFieldView mentorField)
            {
                if (mentorField.Mentor.IsActive)
                {
                    mentorField.Mentor.Paint(
                        mentorField.MentorOverlay,
                        canvas.ActualWidth,
                        canvas.ActualHeight,
                        mentorField,
                        mentorField.Spielfeld.StatistikGeoeffnet);
                    if (!canvas.Children.Contains(mentorField.MentorOverlay))
                    {
                        canvas.Children.Add(mentorField.MentorOverlay);
                    }
                }
                else
                {
                    mentorField.MentorOverlay.Children.Clear();
                    canvas.Children.Remove(mentorField.MentorOverlay);
                }
            }
        }

        public void TickPaintGameField(Canvas canvas, IGameField currentField)
        {
            PaintGameField(canvas, currentField);
        }

        public void TickPaintWeltField(Canvas canvas, IG5_WeltSpielfeld currentField)
        {
            PaintWeltField(canvas, currentField);
        }

        public void PaintWeltField(Canvas canvas, IG5_WeltSpielfeld currentField)
        {
            if (canvas == null)
            {
                throw new ArgumentNullException(nameof(canvas));
            }

            if (!(currentField is IG5_FabrikSpielInfo fabrikInfo))
            {
                throw new ArgumentException("Das Spielfeld stellt keine Fabrik-Anzeigedaten bereit.", nameof(currentField));
            }

            if (currentField is SatisfactoryFieldView menuField)
            {
                menuField.BeginMenuAktionen();
            }

            canvas.Background = Brushes.DimGray;
            canvas.ClipToBounds = true;
            PasseAnsichtAn(canvas, currentField);
            StelleHintergrundSicher(canvas, currentField);
            _dynamischeEbene.Children.Clear();
            if (currentField is SatisfactoryFieldView endField &&
                endField.Spielfeld.Spielphase != G5_Spielphase.Fabrik)
            {
                _hoverWelt = null;
                _hoverGebaeude = null;
                ClearGebaeudeHover();
                ZeichneSpielende(_dynamischeEbene, canvas.ActualWidth, canvas.ActualHeight, endField);
                return;
            }

            _hoverWelt = currentField;
            _hoverGebaeude = fabrikInfo.Gebaeude;
            ZeichneVorkommen(_dynamischeEbene, currentField);
            ZeichneAbgelegteGegenstaende(_dynamischeEbene, currentField, fabrikInfo);

            for (int index = 0; index < _hoverGebaeude.Count; index++)
            {
                IG5_GebaeudeInfo gebaeude = _hoverGebaeude[index];
                if (gebaeude.Typ != G5_FabrikGebaeudeTyp.Laufband)
                {
                    ZeichneGebaeude(_dynamischeEbene, currentField, gebaeude);
                }
            }

            for (int index = 0; index < _hoverGebaeude.Count; index++)
            {
                IG5_GebaeudeInfo gebaeude = _hoverGebaeude[index];
                if (gebaeude.Typ == G5_FabrikGebaeudeTyp.Laufband)
                {
                    ZeichneGebaeude(_dynamischeEbene, currentField, gebaeude);
                }
            }

            ZeichneSpielerUndZiel(_dynamischeEbene, currentField, fabrikInfo);
            ZeichneConveyorDragPreview(_dynamischeEbene, currentField);
            ZeichneHud(_dynamischeEbene, canvas.ActualWidth, fabrikInfo);
            if (currentField is SatisfactoryFieldView statistikField)
            {
                if (statistikField.Spielfeld.StatistikGeoeffnet)
                {
                    statistikField.BeginMenuAktionen();
                    ClearGebaeudeHover();
                    ZeichneStatistik(_dynamischeEbene, canvas.ActualWidth, canvas.ActualHeight, statistikField);
                }
                else
                {
                    ZeichneStatistikKnopf(_dynamischeEbene, canvas.ActualWidth - 145, 0, statistikField);
                }
            }
            if (_hatMausposition &&
                (!(currentField is SatisfactoryFieldView view) || !view.Spielfeld.StatistikGeoeffnet))
            {
                AktualisiereGebaeudeHover(_letzteMausposition);
            }
        }

        private void StelleHintergrundSicher(Canvas canvas, IG5_WeltSpielfeld welt)
        {
            bool weltGewechselt = !ReferenceEquals(_hintergrundWelt, welt);
            if (weltGewechselt)
            {
                _wieseHintergrund.Zuruecksetzen();
                _hintergrundWelt = welt;
            }

            bool ansichtGewechselt =
                !ReferenceEquals(_zielCanvas, canvas) ||
                _hintergrundViewportBreite != canvas.ActualWidth ||
                _hintergrundViewportHoehe != canvas.ActualHeight ||
                _hintergrundEbene == null ||
                _dynamischeEbene == null ||
                !canvas.Children.Contains(_hintergrundEbene) ||
                !canvas.Children.Contains(_dynamischeEbene);
            if (!weltGewechselt && !ansichtGewechselt)
            {
                AktualisiereHintergrundBild(welt);
                return;
            }

            canvas.Children.Clear();
            _hintergrundViewportBreite = canvas.ActualWidth;
            _hintergrundViewportHoehe = canvas.ActualHeight;

            _hintergrundEbene = ErzeugeEbene(canvas);
            _dynamischeEbene = ErzeugeEbene(canvas);
            canvas.Children.Add(_hintergrundEbene);
            canvas.Children.Add(_dynamischeEbene);
            _hoverEbene = ErzeugeEbene(canvas);
            Canvas.SetZIndex(_hoverEbene, 20);
            canvas.Children.Add(_hoverEbene);
            VerbindeMausereignisse(canvas);

            _hintergrundBild = new Image
            {
                Source = _wieseHintergrund.HoleHintergrund(welt),
                Stretch = Stretch.Fill,
                IsHitTestVisible = false
            };
            _hintergrundEbene.Children.Add(_hintergrundBild);
            AktualisiereHintergrundBild(welt);
        }

        private void AktualisiereHintergrundBild(IG5_WeltSpielfeld welt)
        {
            if (_hintergrundBild == null ||
                !welt.TryRasterZuBildschirm(0, 0, out Point ursprung) ||
                !TryGetKachelGroesse(welt, 0, 0, out double kachelBreite, out double kachelHoehe))
            {
                return;
            }

            _hintergrundBild.Width = welt.Breite * kachelBreite;
            _hintergrundBild.Height = welt.Hoehe * kachelHoehe;
            Canvas.SetLeft(_hintergrundBild, ursprung.X);
            Canvas.SetTop(_hintergrundBild, ursprung.Y);
        }

        private static Canvas ErzeugeEbene(Canvas parent)
        {
            return new Canvas
            {
                Width = parent.ActualWidth,
                Height = parent.ActualHeight,
                ClipToBounds = true,
                IsHitTestVisible = false
            };
        }

        private void VerbindeMausereignisse(Canvas canvas)
        {
            if (ReferenceEquals(_zielCanvas, canvas))
            {
                return;
            }

            if (_zielCanvas != null)
            {
                _zielCanvas.MouseMove -= Canvas_MouseMove;
                _zielCanvas.MouseLeave -= Canvas_MouseLeave;
                _zielCanvas.PreviewMouseDown -= Canvas_PreviewMouseDown;
                _zielCanvas.MouseDown -= Canvas_MouseDown;
                _zielCanvas.MouseUp -= Canvas_MouseUp;
            }

            _zielCanvas = canvas;
            _zielCanvas.MouseMove += Canvas_MouseMove;
            _zielCanvas.MouseLeave += Canvas_MouseLeave;
            _zielCanvas.PreviewMouseDown += Canvas_PreviewMouseDown;
            _zielCanvas.MouseDown += Canvas_MouseDown;
            _zielCanvas.MouseUp += Canvas_MouseUp;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!ReferenceEquals(sender, _zielCanvas))
            {
                return;
            }

            _letzteMausposition = e.GetPosition(_zielCanvas);
            _hatMausposition = true;
            if (_conveyorDragStart.HasValue &&
                _hoverWelt is SatisfactoryFieldView conveyorField &&
                TryGetMapPosition(conveyorField, _letzteMausposition, out G5_GridPosition conveyorEnd))
            {
                _conveyorDragEnd = conveyorEnd;
            }

            if (_draggedBuildTool != G5_Bauwerkzeug.Keines && e.LeftButton == MouseButtonState.Pressed)
            {
                _isDraggingBuild = true;
            }

            AktualisiereGebaeudeHover(_letzteMausposition);
        }

        private void Canvas_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!ReferenceEquals(sender, _zielCanvas))
            {
                return;
            }

            _hatMausposition = false;
            ClearGebaeudeHover();
        }

        private void Canvas_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            _mouseDownWhileStatisticsOpen = _hoverWelt is SatisfactoryFieldView field &&
                field.Spielfeld.StatistikGeoeffnet;
            Point position = e.GetPosition(_zielCanvas);
            _mouseDownOnBuildingMenu = _hoverWelt is SatisfactoryFieldView menuField &&
                menuField.AusgewaehltesGebaeude != null &&
                menuField.IsGebaeudeMenueBereich((int)position.X, (int)position.Y);
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!ReferenceEquals(sender, _zielCanvas))
            {
                return;
            }

            Point position = e.GetPosition(_zielCanvas);
            if (_conveyorDragStart.HasValue)
            {
                if (_hoverWelt is SatisfactoryFieldView lineField)
                {
                    if (!lineField.Spielfeld.StatistikGeoeffnet &&
                        !lineField.IsMenueBereich((int)position.X, (int)position.Y) &&
                        !lineField.IsInventarBereich((int)position.X, (int)position.Y) &&
                        TryGetMapPosition(lineField, position, out G5_GridPosition lineEnd))
                    {
                        if (lineEnd != _conveyorDragStart.Value)
                        {
                            lineField.QueuePendingConveyorLine(_conveyorDragStart.Value, lineEnd);
                        }
                    }
                    else
                    {
                        lineField.Spielfeld.SetStatusMeldung("Förderbandvorschau abgebrochen.");
                    }
                }

                _conveyorDragStart = null;
                _conveyorDragEnd = null;
                _zielCanvas.ReleaseMouseCapture();
                e.Handled = true;
                return;
            }

            if (_draggedBuildTool == G5_Bauwerkzeug.Keines)
            {
                return;
            }

            if (_isDraggingBuild && _hoverWelt is SatisfactoryFieldView fieldView)
            {
                if (!fieldView.Spielfeld.StatistikGeoeffnet &&
                    !fieldView.IsMenueBereich((int)position.X, (int)position.Y) &&
                    !fieldView.IsInventarBereich((int)position.X, (int)position.Y) &&
                    TryGetMapPosition(fieldView, position, out G5_GridPosition dropPosition))
                {
                    fieldView.QueuePendingBuild(_draggedBuildTool, dropPosition);
                }
                else
                {
                    fieldView.Spielfeld.SetStatusMeldung("Bau abgebrochen: Wähle ein sichtbares Feld auf der Karte.");
                }
            }

            _draggedBuildTool = G5_Bauwerkzeug.Keines;
            _isDraggingBuild = false;
            _zielCanvas.ReleaseMouseCapture();
            e.Handled = true;
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!ReferenceEquals(sender, _zielCanvas))
            {
                return;
            }

            Point position = e.GetPosition(_zielCanvas);
            if (_mouseDownWhileStatisticsOpen || _mouseDownOnBuildingMenu ||
                (_hoverWelt is SatisfactoryFieldView statistikField &&
                 statistikField.Spielfeld.StatistikGeoeffnet))
            {
                return;
            }

            if (_hoverWelt is SatisfactoryFieldView registerField &&
                IstImGebaeuderegister(position, registerField.GebaeuderegisterGeoeffnet))
            {
                double registerTop = HudHoehe + 10;
                if (!registerField.GebaeuderegisterGeoeffnet ||
                    position.Y <= registerTop + 23)
                {
                    registerField.ToggleGebaeuderegister();
                }
                else if (e.ChangedButton == MouseButton.Left &&
                    TryGetBuildRowTool(position, out G5_Bauwerkzeug tool))
                {
                    _draggedBuildTool = tool;
                    _isDraggingBuild = false;
                    _zielCanvas.CaptureMouse();
                }

                e.Handled = true;
                return;
            }

            if (_hoverWelt is SatisfactoryFieldView buildField &&
                e.ChangedButton == MouseButton.Left &&
                !buildField.IsMenueBereich((int)position.X, (int)position.Y) &&
                !buildField.IsInventarBereich((int)position.X, (int)position.Y) &&
                TryGetMapPosition(buildField, position, out G5_GridPosition startPosition))
            {
                G5_Bauwerkzeug activeTool = ((IG5_FabrikSpielInfo)buildField).AktuellesWerkzeug;
                if (activeTool == G5_Bauwerkzeug.Foerderband)
                {
                    _conveyorDragStart = startPosition;
                    _conveyorDragEnd = startPosition;
                    _zielCanvas.CaptureMouse();
                    e.Handled = true;
                    return;
                }

                if (activeTool != G5_Bauwerkzeug.Keines)
                {
                    _draggedBuildTool = activeTool;
                    _isDraggingBuild = false;
                    _zielCanvas.CaptureMouse();
                    e.Handled = true;
                    return;
                }
            }

            if (_hoverWelt is SatisfactoryFieldView inventoryField &&
                inventoryField.AusgewaehltesGebaeude == null)
            {
                double panelWidth = inventoryField.InventarGeoeffnet ? 268 : 34;
                double panelHeight = inventoryField.InventarGeoeffnet ? 560 : 34;
                double left = _zielCanvas.ActualWidth - panelWidth - 10;
                double top = HudHoehe + 10;
                if (position.X >= left && position.X <= left + panelWidth &&
                    position.Y >= top && position.Y <= top + panelHeight)
                {
                    // Klick auf die Kopfzeile klappt das (kombinierte) Inventar ein bzw. aus.
                    if (position.Y <= top + (inventoryField.InventarGeoeffnet ? 27 : 34))
                    {
                        inventoryField.ToggleInventar();
                    }

                    e.Handled = true;
                }
            }
        }

        private bool TryGetMapPosition(
            SatisfactoryFieldView fieldView, Point screenPosition, out G5_GridPosition gridPosition)
        {
            if (screenPosition.X >= 0 && screenPosition.X < _zielCanvas.ActualWidth &&
                screenPosition.Y >= HudHoehe && screenPosition.Y < _zielCanvas.ActualHeight &&
                ((IG5_WeltSpielfeld)fieldView).TryBildschirmZuRaster(
                    (int)screenPosition.X, (int)screenPosition.Y, out int x, out int y))
            {
                gridPosition = new G5_GridPosition(x, y);
                return true;
            }

            gridPosition = default;
            return false;
        }

        private void ZeichneConveyorDragPreview(Canvas canvas, IG5_WeltSpielfeld welt)
        {
            if (!_conveyorDragStart.HasValue || !_conveyorDragEnd.HasValue ||
                !TryGetKachelMitte(welt, _conveyorDragStart.Value, out Point start) ||
                !TryGetKachelMitte(welt, _conveyorDragEnd.Value, out Point ende))
            {
                return;
            }

            canvas.Children.Add(new Line
            {
                X1 = start.X,
                Y1 = start.Y,
                X2 = ende.X,
                Y2 = ende.Y,
                Stroke = new SolidColorBrush(Color.FromArgb(220, 94, 210, 255)),
                StrokeThickness = 4,
                StrokeDashArray = new DoubleCollection { 5, 3 },
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                IsHitTestVisible = false
            });
        }

        private static bool TryGetKachelMitte(IG5_WeltSpielfeld welt, G5_GridPosition position, out Point mitte)
        {
            mitte = default;
            if (!welt.TryRasterZuBildschirm(position.X, position.Y, out Point rasterPosition) ||
                !TryGetKachelGroesse(welt, position.X, position.Y, out double breite, out double hoehe))
            {
                return false;
            }

            mitte = new Point(rasterPosition.X + breite / 2, rasterPosition.Y + hoehe / 2);
            return true;
        }

        private static void ZeichneAbgelegteGegenstaende(
            Canvas canvas, IG5_WeltSpielfeld welt, IG5_FabrikSpielInfo info)
        {
            DateTimeOffset jetzt = DateTimeOffset.UtcNow;
            foreach (G5_AbgelegterGegenstandInfo abgelegt in info.AbgelegteGegenstaende)
            {
                if (abgelegt.Ablaufzeit <= jetzt ||
                    !welt.TryRasterZuBildschirm(abgelegt.X, abgelegt.Y, out Point position) ||
                    !TryGetKachelGroesse(welt, abgelegt.X, abgelegt.Y, out double breite, out double hoehe))
                {
                    continue;
                }

                double taschenBreite = Math.Max(12, breite * 0.42);
                double taschenHoehe = Math.Max(9, hoehe * 0.3);
                Rectangle tasche = new Rectangle
                {
                    Width = taschenBreite,
                    Height = taschenHoehe,
                    Fill = new SolidColorBrush(Color.FromRgb(184, 132, 57)),
                    Stroke = Brushes.White,
                    StrokeThickness = 1,
                    RadiusX = 2,
                    RadiusY = 2,
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(tasche, position.X + (breite - taschenBreite) / 2);
                Canvas.SetTop(tasche, position.Y + hoehe * 0.22);
                canvas.Children.Add(tasche);

                TextBlock beschriftung = new TextBlock
                {
                    Text = abgelegt.Anzahl + " " +
                        G5_FabrikBezeichnungen.Gegenstand(abgelegt.Gegenstand) +
                        " · " + Math.Ceiling((abgelegt.Ablaufzeit - jetzt).TotalSeconds) + " s",
                    Width = Math.Max(60, breite * 2),
                    FontSize = 8,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.White,
                    Background = new SolidColorBrush(Color.FromArgb(220, 34, 39, 31)),
                    Padding = new Thickness(3, 1, 3, 1),
                    TextAlignment = TextAlignment.Center,
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(beschriftung, position.X + breite / 2 - beschriftung.Width / 2);
                Canvas.SetTop(beschriftung, position.Y + hoehe * 0.55);
                canvas.Children.Add(beschriftung);
            }
        }

        private static bool IstImGebaeuderegister(Point position, bool isOpen)
        {
            double width = isOpen ? 220 : 38;
            double height = isOpen ? 420 : 42;
            return position.X >= 10 && position.X <= 10 + width &&
                position.Y >= HudHoehe + 10 && position.Y <= HudHoehe + 10 + height;
        }

        private static bool TryGetBuildRowTool(
            Point position,
            out G5_Bauwerkzeug werkzeug)
        {
            double left = 10;
            double top = HudHoehe + 10;
            int row = (int)Math.Floor((position.Y - top - 24) / 35);
            if (position.X >= left + 7 &&
                position.X <= left + 213 &&
                row >= 0 &&
                row < GebaeuderegisterEintraege.Length &&
                position.Y >= top + 24 + row * 35 &&
                position.Y <= top + 57 + row * 35)
            {
                werkzeug = GebaeuderegisterEintraege[row].Werkzeug;
                return true;
            }

            werkzeug = G5_Bauwerkzeug.Keines;
            return false;
        }

        private void AktualisiereGebaeudeHover(Point mausposition)
        {
            if (_hoverWelt == null || _hoverGebaeude == null || _hoverEbene == null ||
                !_hoverWelt.TryBildschirmZuRaster(
                    (int)mausposition.X,
                    (int)mausposition.Y,
                    out int x,
                    out int y))
            {
                ClearGebaeudeHover();
                return;
            }

            IG5_GebaeudeInfo gebaeude = null;
            foreach (IG5_GebaeudeInfo kandidat in _hoverGebaeude)
            {
                if (x >= kandidat.X && x < kandidat.X + kandidat.Breite &&
                    y >= kandidat.Y && y < kandidat.Y + kandidat.Hoehe)
                {
                    gebaeude = kandidat;
                    break;
                }
            }

            if (gebaeude != null && !string.IsNullOrWhiteSpace(gebaeude.Name))
            {
                string name = GebaeudeName(gebaeude.Typ);
                string detail = StatusText(gebaeude.Status) + " · MK " + Math.Max(1, gebaeude.Level);
                string key = "Gebaeude|" + name + "|" + detail;
                if (_hoverKarte == null || !string.Equals(_hoverName, key, StringComparison.Ordinal))
                {
                    _hoverName = key;
                    _hoverKarte = ErzeugeHoverKarte("GEBÄUDE", name, detail);
                    _hoverEbene.Children.Clear();
                    _hoverEbene.Children.Add(_hoverKarte);
                }

                PlatziereHoverKarte(mausposition);
                return;
            }

            if (_hoverWelt.TryGetKachelInfo(x, y, out IG5_KachelInfo kachel) &&
                kachel.Rohstoff != G5_Rohstofftyp.Keiner)
            {
                string name = RohstoffName(kachel.Rohstoff);
                string detail = kachel.RohstoffMenge + " verfügbar";
                string key = "Rohstoff|" + name + "|" + detail;
                if (_hoverKarte == null || !string.Equals(_hoverName, key, StringComparison.Ordinal))
                {
                    _hoverName = key;
                    _hoverKarte = ErzeugeHoverKarte("ROHSTOFFFELD", name, detail);
                    _hoverEbene.Children.Clear();
                    _hoverEbene.Children.Add(_hoverKarte);
                }

                PlatziereHoverKarte(mausposition);
                return;
            }

            ClearGebaeudeHover();
        }

        private Border ErzeugeHoverKarte(string label, string name, string detail)
        {
            double width = Math.Max(140, Math.Min(280, _zielCanvas.ActualWidth - 16));
            StackPanel content = new StackPanel();
            content.Children.Add(new TextBlock
            {
                Text = label,
                Foreground = HoverAccentBrush,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 3)
            });
            content.Children.Add(new TextBlock
            {
                Text = name,
                Foreground = Brushes.White,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 20,
                FontWeight = FontWeights.SemiBold,
                TextWrapping = TextWrapping.Wrap
            });
            content.Children.Add(new TextBlock
            {
                Text = detail,
                Foreground = new SolidColorBrush(Color.FromRgb(205, 215, 211)),
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 11,
                Margin = new Thickness(0, 5, 0, 0),
                TextWrapping = TextWrapping.Wrap
            });

            Border card = new Border
            {
                Width = width,
                Padding = new Thickness(15, 11, 15, 11),
                Background = HoverPanelBrush,
                BorderBrush = HoverAccentBrush,
                BorderThickness = new Thickness(1.5),
                CornerRadius = new CornerRadius(12),
                Child = content,
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 16,
                    ShadowDepth = 3,
                    Opacity = 0.45
                },
                IsHitTestVisible = false
            };
            card.Measure(new Size(width, double.PositiveInfinity));
            card.Arrange(new Rect(new Point(0, 0), card.DesiredSize));
            return card;
        }

        private void PlatziereHoverKarte(Point mausposition)
        {
            double width = _hoverKarte.DesiredSize.Width;
            double height = _hoverKarte.DesiredSize.Height;
            double left = mausposition.X + 18;
            double top = mausposition.Y + 18;
            if (left + width > _zielCanvas.ActualWidth - 8)
            {
                left = mausposition.X - width - 18;
            }
            if (top + height > _zielCanvas.ActualHeight - 8)
            {
                top = mausposition.Y - height - 14;
            }

            left = Math.Max(8, Math.Min(left, _zielCanvas.ActualWidth - width - 8));
            top = Math.Max(8, Math.Min(top, _zielCanvas.ActualHeight - height - 8));
            Canvas.SetLeft(_hoverKarte, left);
            Canvas.SetTop(_hoverKarte, top);
        }

        private void ClearGebaeudeHover()
        {
            if (_hoverEbene != null)
            {
                _hoverEbene.Children.Clear();
            }

            _hoverKarte = null;
            _hoverName = null;
        }

        private static bool TryGetKachelGroesse(
            IG5_WeltSpielfeld welt,
            int x,
            int y,
            out double breite,
            out double hoehe)
        {
            breite = StandardKachelGroesse;
            hoehe = StandardKachelGroesse;

            if (x + 1 < welt.Breite &&
                welt.TryRasterZuBildschirm(x + 1, y, out Point rechts) &&
                welt.TryRasterZuBildschirm(x, y, out Point links))
            {
                breite = rechts.X - links.X;
            }
            else if (x > 0 &&
                welt.TryRasterZuBildschirm(x - 1, y, out Point vorherigeKachel) &&
                welt.TryRasterZuBildschirm(x, y, out Point aktuelleKachel))
            {
                breite = aktuelleKachel.X - vorherigeKachel.X;
            }

            if (y + 1 < welt.Hoehe &&
                welt.TryRasterZuBildschirm(x, y + 1, out Point unten) &&
                welt.TryRasterZuBildschirm(x, y, out Point oben))
            {
                hoehe = unten.Y - oben.Y;
            }
            else if (y > 0 &&
                welt.TryRasterZuBildschirm(x, y - 1, out Point vorherigeZeile) &&
                welt.TryRasterZuBildschirm(x, y, out Point aktuelleZeile))
            {
                hoehe = aktuelleZeile.Y - vorherigeZeile.Y;
            }

            return breite > 0 && hoehe > 0;
        }

        private static void PasseAnsichtAn(Canvas canvas, IG5_WeltSpielfeld welt)
        {
            if (welt is SatisfactoryFieldView fieldView)
            {
                fieldView.CenterCamera(canvas.ActualWidth, canvas.ActualHeight);
                return;
            }

            if (!(welt is IG5_KartenansichtSteuerung steuerung) ||
                canvas.ActualWidth <= 0 ||
                canvas.ActualHeight <= HudHoehe)
            {
                return;
            }

            if (canvas.ActualWidth / canvas.ActualHeight < VollbildSeitenverhaeltnis)
            {
                steuerung.SetzeAnsicht(1, 0, HudHoehe);
                return;
            }

            double verfuegbareBreite = Math.Max(1, canvas.ActualWidth - 16);
            double verfuegbareHoehe = Math.Max(1, canvas.ActualHeight - HudHoehe - 16);
            double zoom = Math.Min(
                verfuegbareBreite / (welt.Breite * StandardKachelGroesse),
                verfuegbareHoehe / (welt.Hoehe * StandardKachelGroesse));
            zoom = Math.Max(0.25, Math.Min(4, zoom));

            double kachelGroesse = StandardKachelGroesse * zoom;
            double mapBreite = welt.Breite * kachelGroesse;
            double mapHoehe = welt.Hoehe * kachelGroesse;
            double offsetX = (canvas.ActualWidth - mapBreite) / 2;
            double offsetY = HudHoehe + (canvas.ActualHeight - HudHoehe - mapHoehe) / 2;
            steuerung.SetzeAnsicht(zoom, offsetX, offsetY);
        }

        private void ZeichneVorkommen(Canvas canvas, IG5_WeltSpielfeld welt)
        {
            for (int y = 0; y < welt.Hoehe; y++)
            {
                for (int x = 0; x < welt.Breite; x++)
                {
                    if (!welt.TryGetKachelInfo(x, y, out IG5_KachelInfo kachel) ||
                        kachel.Rohstoff == G5_Rohstofftyp.Keiner ||
                        !welt.TryRasterZuBildschirm(x, y, out Point position) ||
                        !TryGetKachelGroesse(welt, x, y, out double breite, out double hoehe))
                    {
                        continue;
                    }

                    if (position.X + breite < 0 ||
                        position.Y + hoehe < HudHoehe ||
                        position.X > canvas.ActualWidth ||
                        position.Y > canvas.ActualHeight)
                    {
                        continue;
                    }

                    UIElement vorkommen = _wieseHintergrund.ErzeugeVorkommen(kachel, x, y, breite, hoehe);
                    Canvas.SetLeft(vorkommen, position.X);
                    Canvas.SetTop(vorkommen, position.Y);
                    canvas.Children.Add(vorkommen);
                }
            }
        }

        private static void ZeichneGebaeude(
            Canvas canvas,
            IG5_WeltSpielfeld welt,
            IG5_GebaeudeInfo gebaeude)
        {
            if (!welt.TryRasterZuBildschirm(gebaeude.X, gebaeude.Y, out Point position) ||
                !TryGetKachelGroesse(welt, gebaeude.X, gebaeude.Y, out double kachelBreite, out double kachelHoehe))
            {
                return;
            }

            double breite = kachelBreite * gebaeude.Breite;
            double hoehe = kachelHoehe * gebaeude.Hoehe;
            G5_ConveyorDirection conveyorInput = G5_ConveyorDirection.West;
            G5_ConveyorDirection conveyorOutput = G5_ConveyorDirection.East;
            if (gebaeude.Typ == G5_FabrikGebaeudeTyp.Laufband &&
                welt is SatisfactoryFieldView conveyorField &&
                conveyorField.TryGetConveyorRoute(
                    gebaeude.X,
                    gebaeude.Y,
                    out G5_ConveyorDirection connectedInput,
                    out G5_ConveyorDirection connectedOutput))
            {
                conveyorInput = connectedInput;
                conveyorOutput = connectedOutput;
            }

            Canvas abbildung = G5_FabrikGebaeudeZeichner.Create(
                gebaeude.Typ,
                gebaeude.Level,
                breite,
                hoehe,
                conveyorInput,
                conveyorOutput,
                kachelFuellen: true);

            Canvas.SetLeft(abbildung, position.X);
            Canvas.SetTop(abbildung, position.Y);
            canvas.Children.Add(abbildung);

            IG5_GebaeudeInfo selectedBuilding =
                (welt as SatisfactoryFieldView)?.AusgewaehltesGebaeude;
            if (selectedBuilding != null &&
                selectedBuilding.X == gebaeude.X &&
                selectedBuilding.Y == gebaeude.Y)
            {
                Rectangle selection = new Rectangle
                {
                    Width = breite,
                    Height = hoehe,
                    Stroke = HoverAccentBrush,
                    StrokeThickness = Math.Max(2, Math.Min(breite, hoehe) * 0.06),
                    Fill = new SolidColorBrush(Color.FromArgb(25, 142, 230, 120)),
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(selection, position.X);
                Canvas.SetTop(selection, position.Y);
                canvas.Children.Add(selection);
            }

            if (gebaeude.Typ == G5_FabrikGebaeudeTyp.Laufband &&
                welt is SatisfactoryFieldView conveyorItems &&
                conveyorItems.TryGetConveyorTransportProgress(
                    gebaeude.X,
                    gebaeude.Y,
                    out double transportProgress))
            {
                ZeichneTransportgut(
                    canvas,
                    position,
                    breite,
                    hoehe,
                    conveyorInput,
                    conveyorOutput,
                    transportProgress);
            }
        }

        private static void ZeichneTransportgut(
            Canvas canvas,
            Point position,
            double width,
            double height,
            G5_ConveyorDirection inputDirection,
            G5_ConveyorDirection outputDirection,
            double progress)
        {
            Point routePoint = G5_FabrikGebaeudeZeichner.ConveyorPoint(
                inputDirection,
                outputDirection,
                progress);
            double boxWidth = Math.Max(8, Math.Min(15, width * 0.46));
            double boxHeight = Math.Max(7, Math.Min(13, height * 0.38));
            Canvas box = new Canvas
            {
                Width = boxWidth,
                Height = boxHeight,
                IsHitTestVisible = false
            };
            Rectangle crate = new Rectangle
            {
                Width = boxWidth,
                Height = boxHeight,
                Fill = TransportBoxBrush,
                Stroke = TransportBoxEdgeBrush,
                StrokeThickness = 1,
                IsHitTestVisible = false
            };
            Rectangle stripe = new Rectangle
            {
                Width = Math.Max(2, boxWidth * 0.16),
                Height = boxHeight - 2,
                Fill = TransportBoxStripeBrush,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(stripe, (boxWidth - stripe.Width) / 2);
            Canvas.SetTop(stripe, 1);
            box.Children.Add(crate);
            box.Children.Add(stripe);

            Point screenPoint = new Point(
                position.X + routePoint.X / 160 * width,
                position.Y + routePoint.Y / 130 * height);
            Canvas.SetLeft(box, screenPoint.X - boxWidth / 2);
            Canvas.SetTop(box, screenPoint.Y - boxHeight / 2);
            canvas.Children.Add(box);
        }

        private static string GebaeudeName(G5_FabrikGebaeudeTyp typ)
        {
            switch (typ)
            {
                case G5_FabrikGebaeudeTyp.Hub:
                    return "Hub";
                case G5_FabrikGebaeudeTyp.Foerderer:
                    return "Förderer";
                case G5_FabrikGebaeudeTyp.Schmelze:
                    return "Schmelze";
                case G5_FabrikGebaeudeTyp.Konstruktor:
                    return "Konstruktor";
                case G5_FabrikGebaeudeTyp.Assembler:
                    return "Assembler";
                case G5_FabrikGebaeudeTyp.Verbrennungsgenerator:
                    return "Verbrennungsgenerator";
                case G5_FabrikGebaeudeTyp.Strommast:
                    return "Strommast";
                case G5_FabrikGebaeudeTyp.Lager:
                    return "Lager";
                case G5_FabrikGebaeudeTyp.Laufband:
                    return "Förderband";
                case G5_FabrikGebaeudeTyp.Splitter:
                    return "Splitter";
                case G5_FabrikGebaeudeTyp.Merger:
                    return "Merger";
                case G5_FabrikGebaeudeTyp.Rakete:
                    return "Rakete";
                default:
                    throw new InvalidOperationException("Unbekannter Gebäudetyp in der Bezeichnung.");
            }
        }

        private static string StatusText(G5_BuildingStatus status)
        {
            switch (status)
            {
                case G5_BuildingStatus.Working:
                    return "Produktion";
                case G5_BuildingStatus.WaitingForInput:
                    return "Wartet auf Erz";
                case G5_BuildingStatus.OutputBlocked:
                    return "Ausgang voll";
                case G5_BuildingStatus.WaitingForPower:
                    return "Kein Strom";
                case G5_BuildingStatus.DepositDepleted:
                    return "Vorkommen leer";
                case G5_BuildingStatus.WaitingForRecipe:
                    return "Kein Rezept";
                case G5_BuildingStatus.WaitingForConnection:
                    return "Wartet auf Verbindung";
                default:
                    return "Bereit";
            }
        }

        private static void ZeichneSpielerUndZiel(
            Canvas canvas,
            IG5_WeltSpielfeld welt,
            IG5_FabrikSpielInfo info)
        {
            if (!(welt is SatisfactoryFieldView fieldView) || !fieldView.IsGameStarted)
            {
                return;
            }

            if (welt.TryRasterZuBildschirm(info.ZielKachel.X, info.ZielKachel.Y, out Point ziel) &&
                TryGetKachelGroesse(welt, info.ZielKachel.X, info.ZielKachel.Y, out double zielBreite, out double zielHoehe))
            {
                Rectangle umriss = new Rectangle
                {
                    Width = zielBreite,
                    Height = zielHoehe,
                    Stroke = new SolidColorBrush(Color.FromArgb(245, 255, 218, 74)),
                    StrokeThickness = Math.Max(2, zielBreite * 0.075),
                    Fill = new SolidColorBrush(Color.FromArgb(30, 255, 218, 74)),
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(umriss, ziel.X);
                Canvas.SetTop(umriss, ziel.Y);
                canvas.Children.Add(umriss);
            }

            G5_GridPosition player = info.SpielerPosition;
            if (!welt.TryRasterZuBildschirm(player.X, player.Y, out Point position) ||
                !TryGetKachelGroesse(welt, player.X, player.Y, out double breite, out double hoehe))
            {
                return;
            }

            Ellipse shadow = new Ellipse
            {
                Width = breite * 0.72,
                Height = hoehe * 0.42,
                Fill = new SolidColorBrush(Color.FromArgb(115, 0, 0, 0)),
                IsHitTestVisible = false
            };
            Canvas.SetLeft(shadow, position.X + (breite - shadow.Width) / 2);
            Canvas.SetTop(shadow, position.Y + hoehe * 0.42);
            canvas.Children.Add(shadow);

            double winkel;
            switch (info.SpielerBlickrichtung)
            {
                case G5_SpielerBlickrichtung.Norden: winkel = 180; break;
                case G5_SpielerBlickrichtung.Osten: winkel = 270; break;
                case G5_SpielerBlickrichtung.Sueden: winkel = 0; break;
                case G5_SpielerBlickrichtung.Westen: winkel = 90; break;
                default: throw new ArgumentOutOfRangeException();
            }

            double spielerGroesse = Math.Min(breite, hoehe) * 0.88;
            Image spieler = new Image
            {
                Source = SpielerBild,
                Width = spielerGroesse,
                Height = spielerGroesse,
                Stretch = Stretch.Fill,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new RotateTransform(winkel),
                IsHitTestVisible = false
            };
            Canvas.SetLeft(spieler, position.X + (breite - spielerGroesse) / 2);
            Canvas.SetTop(spieler, position.Y + (hoehe - spielerGroesse) / 2);
            canvas.Children.Add(spieler);

            double centerX = position.X + breite / 2;
            double centerY = position.Y + hoehe / 2;
            double directionX = 0;
            double directionY = 0;
            switch (info.SpielerBlickrichtung)
            {
                case G5_SpielerBlickrichtung.Norden: directionY = -1; break;
                case G5_SpielerBlickrichtung.Osten: directionX = 1; break;
                case G5_SpielerBlickrichtung.Sueden: directionY = 1; break;
                case G5_SpielerBlickrichtung.Westen: directionX = -1; break;
            }

            Line blick = new Line
            {
                X1 = centerX + directionX * breite * 0.35,
                Y1 = centerY + directionY * hoehe * 0.35,
                X2 = centerX + directionX * breite * 0.47,
                Y2 = centerY + directionY * hoehe * 0.47,
                Stroke = new SolidColorBrush(Color.FromRgb(255, 238, 170)),
                StrokeThickness = 2,
                StrokeEndLineCap = PenLineCap.Triangle,
                IsHitTestVisible = false
            };
            canvas.Children.Add(blick);
        }

        private static ImageSource ErzeugeSpielerBild()
        {
            Brush outline = CreateBrush(255, 21, 33, 47);
            Brush blau = CreateBrush(255, 55, 112, 166);
            Brush blauHell = CreateBrush(255, 100, 166, 211);
            Brush blauDunkel = CreateBrush(255, 33, 66, 104);
            Brush haut = CreateBrush(255, 222, 169, 122);
            Brush haar = CreateBrush(255, 79, 53, 38);
            Brush stiefel = CreateBrush(255, 29, 34, 39);
            Pen kontur = new Pen(outline, 1.1);
            kontur.Freeze();
            Pen mund = new Pen(haar, 0.65);
            mund.Freeze();

            DrawingGroup zeichnung = new DrawingGroup();
            using (DrawingContext dc = zeichnung.Open())
            {
                dc.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, 32, 32));
                dc.DrawRoundedRectangle(stiefel, kontur, new Rect(8, 24, 6, 5), 1.5, 1.5);
                dc.DrawRoundedRectangle(stiefel, kontur, new Rect(18, 24, 6, 5), 1.5, 1.5);
                dc.DrawRoundedRectangle(blauDunkel, kontur, new Rect(8, 18, 6, 8), 2, 2);
                dc.DrawRoundedRectangle(blauDunkel, kontur, new Rect(18, 18, 6, 8), 2, 2);
                dc.DrawRoundedRectangle(blauDunkel, kontur, new Rect(9, 15, 14, 10), 3, 3);

                dc.DrawRoundedRectangle(blau, kontur, new Rect(3, 11, 6, 11), 3, 3);
                dc.DrawRoundedRectangle(blau, kontur, new Rect(23, 11, 6, 11), 3, 3);
                dc.DrawEllipse(haut, kontur, new Point(6, 22), 2.2, 2.5);
                dc.DrawEllipse(haut, kontur, new Point(26, 22), 2.2, 2.5);
                dc.DrawRoundedRectangle(blau, kontur, new Rect(7, 9, 18, 14), 5, 5);
                dc.DrawRoundedRectangle(blauHell, null, new Rect(10, 11, 12, 8), 3, 3);
                dc.DrawRoundedRectangle(blauDunkel, null, new Rect(9, 20, 14, 2), 1, 1);

                dc.DrawEllipse(haut, kontur, new Point(16, 8), 5.2, 5.4);
                dc.DrawEllipse(haar, null, new Point(16, 5.2), 4.5, 2.5);
                dc.DrawEllipse(outline, null, new Point(14.2, 8.6), 0.6, 0.65);
                dc.DrawEllipse(outline, null, new Point(17.8, 8.6), 0.6, 0.65);
                dc.DrawLine(mund, new Point(15, 10.6), new Point(17, 10.6));
                dc.DrawRoundedRectangle(blauDunkel, null, new Rect(11, 12, 10, 2), 1, 1);
            }

            DrawingImage bild = new DrawingImage(zeichnung);
            bild.Freeze();
            return bild;
        }

        private void ZeichneHud(Canvas canvas, double viewportBreite, IG5_FabrikSpielInfo info)
        {
            Rectangle hintergrund = new Rectangle
            {
                Width = Math.Max(0, viewportBreite),
                Height = HudHoehe,
                Fill = new SolidColorBrush(Color.FromArgb(235, 16, 20, 18))
            };
            Canvas.SetLeft(hintergrund, 0);
            Canvas.SetTop(hintergrund, 0);
            canvas.Children.Add(hintergrund);

            ZeichneHudZeile(
                canvas,
                viewportBreite,
                "WASD: laufen    E: abbauen    Baumenü: kaufen & bauen    Gebäude anklicken: Details    Entf: verkaufen    Tab: Baumenü",
                0);
            G5_GridPosition ziel = info.ZielKachel;
            ZeichneHudZeile(
                canvas,
                viewportBreite,
                "Zielkachel: " + ziel.X + ", " + ziel.Y + "    |    " + info.StatusMeldung,
                HudZeilenHoehe);
            if (info is SatisfactoryFieldView fieldView)
            {
                ZeichneGebaeuderegister(canvas, fieldView);
                if (fieldView.AusgewaehltesGebaeude != null)
                {
                    fieldView.SetInventarBereich(Rect.Empty);
                    ZeichneGebaeudeMenue(canvas, viewportBreite, fieldView, fieldView.AusgewaehltesGebaeude);
                }
                else
                {
                    fieldView.SetGebaeudeMenueBereich(Rect.Empty);
                    ZeichneInventar(canvas, viewportBreite, info);
                }
            }
        }

        private static void ZeichneGebaeuderegister(Canvas canvas, SatisfactoryFieldView fieldView)
        {
            double left = 10;
            double top = HudHoehe + 10;
            bool isOpen = fieldView.GebaeuderegisterGeoeffnet;
            double width = isOpen ? 220 : 38;
            double height = isOpen ? 420 : 42;
            fieldView.SetBaumenueBereich(new Rect(left, top, width, height));
            Border panel = new Border
            {
                Width = width,
                Height = height,
                Padding = new Thickness(isOpen ? 7 : 3),
                Background = HoverPanelBrush,
                BorderBrush = HoverAccentBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 9,
                    ShadowDepth = 2,
                    Opacity = 0.35
                },
                IsHitTestVisible = false
            };

            if (!isOpen)
            {
                panel.Child = new TextBlock
                {
                    Text = "BAU",
                    Foreground = HoverAccentBrush,
                    FontFamily = new FontFamily("Segoe UI"),
                    FontSize = 9,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
            }
            else
            {
                StackPanel content = new StackPanel();
                content.Children.Add(new TextBlock
                {
                    Text = "BAUGEBÄUDE  ·  KLICKEN ZUM BAUEN",
                    Foreground = HoverAccentBrush,
                    FontFamily = new FontFamily("Segoe UI"),
                    FontSize = 10,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(3, 0, 0, 4)
                });

                for (int rowIndex = 0; rowIndex < GebaeuderegisterEintraege.Length; rowIndex++)
                {
                    GebaeuderegisterEintrag entry = GebaeuderegisterEintraege[rowIndex];
                    bool selected = entry.Werkzeug == ((IG5_FabrikSpielInfo)fieldView).AktuellesWerkzeug;
                    IReadOnlyDictionary<G5_Item, int> cost = G5_FabrikKosten.Baupreis(entry.Werkzeug);
                    bool affordable = cost.All(item => fieldView.HubMenge(item.Key) >= item.Value);
                    StackPanel rowContent = new StackPanel();
                    rowContent.Children.Add(new TextBlock
                    {
                        Text = entry.Name,
                        Foreground = Brushes.White,
                        FontFamily = new FontFamily("Segoe UI"),
                        FontSize = 11,
                        FontWeight = FontWeights.Bold
                    });
                    rowContent.Children.Add(ErzeugeKostenanzeige(
                        FormatCost(cost),
                        affordable
                            ? new SolidColorBrush(Color.FromRgb(161, 220, 137))
                            : new SolidColorBrush(Color.FromRgb(202, 177, 152)),
                        FontWeights.Normal,
                        width - 26,
                        false));
                    Border row = new Border
                    {
                        Height = 33,
                        Margin = new Thickness(0, 1, 0, 1),
                        Padding = new Thickness(6, 2, 4, 2),
                        Background = selected
                            ? new SolidColorBrush(Color.FromArgb(205, 72, 106, 64))
                            : new SolidColorBrush(Color.FromArgb(150, 42, 58, 52)),
                        CornerRadius = new CornerRadius(6),
                        Child = rowContent
                    };
                    content.Children.Add(row);
                    fieldView.RegisterMenuAktion(
                        new Rect(left + 7, top + 24 + rowIndex * 35, width - 14, 33),
                        G5_FabrikAktionstyp.WerkzeugWaehlen,
                        entry.Werkzeug);
                }

                content.Children.Add(new TextBlock
                {
                    Text = "Ziehe ein Gebäude auf die Karte oder baue auf der Zielkachel.",
                    Foreground = new SolidColorBrush(Color.FromRgb(190, 205, 198)),
                    FontFamily = new FontFamily("Segoe UI"),
                    FontSize = 9,
                    Margin = new Thickness(3, 4, 0, 0)
                });
                G5_Bauwerkzeug selectedTool = ((IG5_FabrikSpielInfo)fieldView).AktuellesWerkzeug;
                string selectedName = GebaeuderegisterEintraege
                    .FirstOrDefault(entry => entry.Werkzeug == selectedTool)?.Name;
                Border buildButton = CreateMenuButton(
                    selectedTool == G5_Bauwerkzeug.Keines ? "GEBÄUDE AUSWÄHLEN" : "BAUEN · " + selectedName,
                    HoverAccentBrush,
                    width - 14);
                buildButton.Margin = new Thickness(0, 6, 0, 0);
                content.Children.Add(buildButton);
                if (selectedTool != G5_Bauwerkzeug.Keines)
                {
                    fieldView.RegisterMenuAktion(
                        new Rect(left + 7, top + height - 56, width - 14, 30),
                        G5_FabrikAktionstyp.GebaeudeBauen,
                        selectedTool);
                }

                panel.Child = content;
            }

            Canvas.SetLeft(panel, left);
            Canvas.SetTop(panel, top);
            canvas.Children.Add(panel);
        }

        private static void ZeichneGebaeudeMenue(
            Canvas canvas,
            double viewportBreite,
            SatisfactoryFieldView fieldView,
            IG5_GebaeudeInfo gebaeude)
        {
            double width = 270;
            double left = Math.Max(10, viewportBreite - width - 10);
            double top = HudHoehe + 10;
            double height = Math.Max(220, Math.Min(365, canvas.ActualHeight - top - 8));
            fieldView.SetGebaeudeMenueBereich(new Rect(left, top, width, height));

            Border panel = new Border
            {
                Width = width,
                Height = height,
                Padding = new Thickness(8),
                Background = HoverPanelBrush,
                BorderBrush = HoverAccentBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                IsHitTestVisible = false
            };
            Canvas content = new Canvas
            {
                Width = width - 16,
                Height = height - 16,
                IsHitTestVisible = false
            };
            AddMenuText(content, gebaeude.Name + "  ·  MK " + gebaeude.Level, 0, 0, width - 46, 22,
                HoverAccentBrush, 13, FontWeights.Bold);
            AddMenuText(content, "×", 232, 0, 20, 20, Brushes.White, 16, FontWeights.Bold, TextAlignment.Center);
            fieldView.RegisterMenuAktion(
                new Rect(left + 238, top + 8, 24, 22),
                G5_FabrikAktionstyp.GebaeudeAnsehen);
            AddMenuText(content, gebaeude.Beschreibung, 0, 26, width - 34, 48, Brushes.White, 10);
            AddMenuText(
                content,
                "Status: " + StatusText(gebaeude.Status) + "   Feld: " + gebaeude.X + ", " + gebaeude.Y,
                0,
                77,
                width - 34,
                18,
                new SolidColorBrush(Color.FromRgb(205, 215, 211)),
                9);

            bool canCollectOutput =
                gebaeude.Typ == G5_FabrikGebaeudeTyp.Foerderer ||
                gebaeude.Typ == G5_FabrikGebaeudeTyp.Schmelze ||
                gebaeude.Typ == G5_FabrikGebaeudeTyp.Konstruktor ||
                gebaeude.Typ == G5_FabrikGebaeudeTyp.Assembler;
            if (canCollectOutput)
            {
                KeyValuePair<G5_Item, int> output = gebaeude.AusgabeInhalt
                    .FirstOrDefault(item => item.Value > 0);
                string outputText = output.Value > 0
                    ? "ABHOLEN · " + fieldView.MengeEingabe + " / " + output.Value + " " +
                        G5_FabrikBezeichnungen.Gegenstand(output.Key)
                    : "NOCH NICHTS ZUM ABHOLEN";
                Border collectButton = CreateMenuButton(outputText, HoverAccentBrush, width - 34);
                Canvas.SetLeft(collectButton, 0);
                Canvas.SetTop(collectButton, 101);
                content.Children.Add(collectButton);
                AddMenuText(content, "Abholmenge: " + fieldView.MengeEingabe +
                    " · Zahlentasten, Rücktaste löscht", 0, 134, width - 34, 16,
                    new SolidColorBrush(Color.FromRgb(190, 205, 198)), 8);
                if (output.Value > 0)
                {
                    fieldView.RegisterMenuAktion(
                        new Rect(left + 8, top + 8 + 101, width - 34, 30),
                        G5_FabrikAktionstyp.GegenstandAbholen,
                        wert: fieldView.MengeEingabe);
                }
            }

            if (gebaeude.VerfuegbareRezepte.Count > 0)
            {
                double recipeTop = canCollectOutput ? 155 : 101;
                AddMenuText(
                    content,
                    gebaeude.Level >= 2 ? "REZEPTE" : "REZEPT (UPGRADE SCHALTET MEHR FREI)",
                    0,
                    recipeTop,
                    width - 34,
                    16,
                    HoverAccentBrush,
                    9,
                    FontWeights.Bold);
                int recipeIndex = 0;
                foreach (string recipe in gebaeude.VerfuegbareRezepte)
                {
                    bool selected = string.Equals(recipe, gebaeude.AusgewaehltesRezept, StringComparison.Ordinal);
                    Border recipeRow = new Border
                    {
                        Width = width - 34,
                        Height = 24,
                        Padding = new Thickness(5, 3, 4, 2),
                        Background = selected
                            ? new SolidColorBrush(Color.FromArgb(190, 83, 117, 67))
                            : new SolidColorBrush(Color.FromArgb(120, 42, 58, 52)),
                        CornerRadius = new CornerRadius(5),
                        Child = new TextBlock
                        {
                            Text = recipe,
                            Foreground = Brushes.White,
                            FontSize = 9
                        }
                    };
                    Canvas.SetLeft(recipeRow, 0);
                    Canvas.SetTop(recipeRow, recipeTop + 18 + recipeIndex * 28);
                    content.Children.Add(recipeRow);
                    fieldView.RegisterMenuAktion(
                        new Rect(
                            left + 8,
                            top + 8 + recipeTop + 18 + recipeIndex * 28,
                            width - 34,
                            24),
                        G5_FabrikAktionstyp.RezeptWaehlen,
                        wert: recipe);
                    recipeIndex++;
                }
            }

            if (gebaeude.KannVerbessertWerden)
            {
                string upgradeLabel = "UPGRADE (MK " + (gebaeude.Level + 1) + ")  ·  " +
                    FormatCost(gebaeude.UpgradeKosten);
                double upgradeTop = height - 78;
                Border upgrade = CreateMenuButton(upgradeLabel, HoverAccentBrush, width - 34);
                upgrade.Child = ErzeugeKostenanzeige(
                    upgradeLabel, Brushes.White, FontWeights.Bold, width - 46, true);
                Canvas.SetLeft(upgrade, 0);
                Canvas.SetTop(upgrade, upgradeTop);
                content.Children.Add(upgrade);
                fieldView.RegisterMenuAktion(
                    new Rect(left + 8, top + 8 + upgradeTop, width - 34, 30),
                    G5_FabrikAktionstyp.GebaeudeUpgraden);
            }

            if (gebaeude.Typ == G5_FabrikGebaeudeTyp.Rakete)
            {
                Border betanken = CreateMenuButton("URAN EINFÜLLEN", HoverAccentBrush, width - 34);
                Canvas.SetLeft(betanken, 0);
                Canvas.SetTop(betanken, 110);
                content.Children.Add(betanken);
                fieldView.RegisterMenuAktion(
                    new Rect(left + 8, top + 8 + 110, width - 34, 30),
                    G5_FabrikAktionstyp.RaketeBetanken);
                Border starten = CreateMenuButton("EINSTEIGEN UND STARTEN", HoverAccentBrush, width - 34);
                Canvas.SetLeft(starten, 0);
                Canvas.SetTop(starten, 150);
                content.Children.Add(starten);
                fieldView.RegisterMenuAktion(
                    new Rect(left + 8, top + 8 + 150, width - 34, 30),
                    G5_FabrikAktionstyp.RaketeStarten);
            }

            double sellTop = height - 41;
            Border sell = CreateMenuButton(
                "VERKAUFEN  ·  50 % RÜCKERSTATTUNG",
                new SolidColorBrush(Color.FromRgb(205, 133, 95)),
                width - 34);
            Canvas.SetLeft(sell, 0);
            Canvas.SetTop(sell, sellTop);
            content.Children.Add(sell);
            fieldView.RegisterMenuAktion(
                new Rect(left + 8, top + 8 + sellTop, width - 34, 30),
                G5_FabrikAktionstyp.GebaeudeVerkaufen);

            panel.Child = content;
            Canvas.SetLeft(panel, left);
            Canvas.SetTop(panel, top);
            canvas.Children.Add(panel);
        }

        private static void ZeichneSpielende(
            Canvas canvas, double breite, double hoehe, SatisfactoryFieldView fieldView)
        {
            G5_FabrikSpielfeld field = fieldView.Spielfeld;
            canvas.Children.Add(new Rectangle
            {
                Width = Math.Max(0, breite),
                Height = Math.Max(0, hoehe),
                Fill = new SolidColorBrush(Color.FromRgb(7, 17, 40))
            });
            for (int i = 0; i < 32; i++)
            {
                Ellipse stern = new Ellipse
                {
                    Width = i % 5 == 0 ? 3 : 2,
                    Height = i % 5 == 0 ? 3 : 2,
                    Fill = Brushes.White
                };
                Canvas.SetLeft(stern, (i * 193 % 997) * breite / 997);
                Canvas.SetTop(stern, (i * 137 % 503) * hoehe / 503);
                canvas.Children.Add(stern);
            }

            Ellipse planet = new Ellipse
            {
                Width = breite * 1.3,
                Height = hoehe * 0.75,
                Fill = new SolidColorBrush(Color.FromRgb(37, 112, 145)),
                Stroke = new SolidColorBrush(Color.FromRgb(107, 184, 176)),
                StrokeThickness = 7
            };
            Canvas.SetLeft(planet, -breite * 0.15);
            Canvas.SetTop(planet, hoehe * 0.83);
            canvas.Children.Add(planet);

            if (field.Spielphase == G5_Spielphase.Raketenstart)
            {
                double fortschritt = (double)field.RaketenstartTicks / G5_FabrikSpielfeld.RaketenstartDauerTicks;
                double rocketLeft = breite / 2 - 85;
                double rocketTop = hoehe * 0.62 - fortschritt * (hoehe + 160);
                Polygon flamme = new Polygon
                {
                    Points = new PointCollection
                    {
                        new Point(rocketLeft + 68, rocketTop + 112),
                        new Point(rocketLeft + 85, rocketTop + 175 + (field.RaketenstartTicks % 4) * 4),
                        new Point(rocketLeft + 102, rocketTop + 112)
                    },
                    Fill = new SolidColorBrush(Color.FromRgb(255, 164, 47))
                };
                canvas.Children.Add(flamme);
                Canvas rakete = G5_FabrikGebaeudeZeichner.Create(G5_FabrikGebaeudeTyp.Rakete, 1, 170, 140);
                Canvas.SetLeft(rakete, rocketLeft);
                Canvas.SetTop(rakete, rocketTop);
                canvas.Children.Add(rakete);
                AddMenuText(canvas, "AUFBRUCH ZU NEUEN WELTEN", 0, 35, breite, 45,
                    Brushes.White, 22, FontWeights.Bold, TextAlignment.Center);
            }
            else
            {
                AddMenuText(canvas, "GESCHAFFT!", 0, hoehe * 0.2, breite, 65,
                    HoverAccentBrush, 36, FontWeights.Bold, TextAlignment.Center);
                AddMenuText(canvas, "Du hast den Planeten verlassen und erkundest neue Welten.",
                    breite * 0.1, hoehe * 0.34, breite * 0.8, 60,
                    Brushes.White, 17, FontWeights.Normal, TextAlignment.Center);
                if (field.StatistikGeoeffnet)
                {
                    ZeichneStatistik(canvas, breite, hoehe, fieldView);
                }
                else
                {
                    ZeichneStatistikKnopf(canvas, breite / 2 - 70, hoehe * 0.52, fieldView);
                }
            }
        }

        private static void ZeichneStatistikKnopf(
            Canvas canvas, double left, double top, SatisfactoryFieldView fieldView)
        {
            left = Math.Max(0, left);
            Border knopf = CreateMenuButton("STATISTIK  ·  F2", HoverAccentBrush, 140);
            Canvas.SetLeft(knopf, left);
            Canvas.SetTop(knopf, top);
            canvas.Children.Add(knopf);
            fieldView.RegisterMenuAktion(
                new Rect(left, top, 140, 30), G5_FabrikAktionstyp.StatistikUmschalten);
        }

        private static void ZeichneStatistik(
            Canvas canvas, double breite, double hoehe, SatisfactoryFieldView fieldView)
        {
            G5_FabrikSpielfeld field = fieldView.Spielfeld;
            double width = Math.Max(0, Math.Min(500, breite - 20));
            double height = Math.Max(0, Math.Min(360, hoehe - 20));
            double left = (breite - width) / 2;
            double top = (hoehe - height) / 2;
            fieldView.SetStatistikBereich(new Rect(left, top, width, height));
            Canvas inhalt = new Canvas { Width = width, Height = height, ClipToBounds = true };
            Border panel = new Border
            {
                Width = width,
                Height = height,
                Background = HoverPanelBrush,
                BorderBrush = HoverAccentBrush,
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(10),
                Child = inhalt
            };
            Canvas.SetLeft(panel, left);
            Canvas.SetTop(panel, top);
            canvas.Children.Add(panel);
            AddMenuText(inhalt, "SPIELSTATISTIK", 15, 12, width - 65, 26,
                HoverAccentBrush, 17, FontWeights.Bold);
            AddMenuText(inhalt, "Gebaute Gebäude: " + field.GebauteGebaeude +
                "   ·   Spielzeit: " + field.Simulation.TickNumber / 25 + " s",
                15, 45, width - 30, 25, Brushes.White, 11);
            double spalte = (width - 30) / 2;
            AddMenuText(inhalt, "ABGEBAUTE ROHSTOFFE", 15, 76, spalte, 20,
                HoverAccentBrush, 10, FontWeights.Bold);
            AddMenuText(inhalt, "HERGESTELLTE PRODUKTE", 15 + spalte, 76, spalte, 20,
                HoverAccentBrush, 10, FontWeights.Bold);
            AddMenuText(inhalt, StatistikText(field.AbgebauteRohstoffe), 15, 98, spalte, Math.Max(0, height - 108),
                Brushes.White, 11);
            AddMenuText(inhalt, StatistikText(field.HergestellteProdukte), 15 + spalte, 98,
                spalte, Math.Max(0, height - 108), Brushes.White, 11);
            AddMenuText(inhalt, "×", width - 40, 8, 25, 30, Brushes.White, 20,
                FontWeights.Bold, TextAlignment.Center);
            fieldView.RegisterMenuAktion(
                new Rect(left + width - 44, top + 8, 32, 30),
                G5_FabrikAktionstyp.StatistikUmschalten);
        }

        private static string StatistikText(IReadOnlyDictionary<G5_Item, long> zaehler)
        {
            return zaehler.Count == 0
                ? "Noch keine"
                : string.Join("\n", zaehler.OrderBy(eintrag => eintrag.Key)
                    .Select(eintrag => G5_FabrikBezeichnungen.Gegenstand(eintrag.Key) + ": " + eintrag.Value));
        }

        private static void AddMenuText(
            Canvas canvas,
            string text,
            double left,
            double top,
            double width,
            double height,
            Brush foreground,
            double fontSize,
            FontWeight fontWeight = default(FontWeight),
            TextAlignment alignment = TextAlignment.Left)
        {
            TextBlock block = new TextBlock
            {
                Text = text,
                Foreground = foreground,
                FontSize = fontSize,
                FontWeight = fontWeight == default(FontWeight) ? FontWeights.Normal : fontWeight,
                TextAlignment = alignment,
                TextWrapping = TextWrapping.Wrap,
                Width = width,
                Height = height,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(block, left);
            Canvas.SetTop(block, top);
            canvas.Children.Add(block);
        }

        private static Border CreateMenuButton(string text, Brush accent, double width)
        {
            return new Border
            {
                Width = width,
                Height = 30,
                Padding = new Thickness(5, 6, 5, 3),
                Background = new SolidColorBrush(Color.FromArgb(190, 42, 58, 52)),
                BorderBrush = accent,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(5),
                Child = new TextBlock
                {
                    Text = text,
                    Foreground = Brushes.White,
                    FontSize = 9,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    TextTrimming = TextTrimming.CharacterEllipsis
                },
                IsHitTestVisible = false
            };
        }

        private static Canvas ErzeugeKostenanzeige(
            string text, Brush farbe, FontWeight gewicht, double breite, bool zentriert)
        {
            TextBlock block = new TextBlock
            {
                Text = text,
                Foreground = farbe,
                FontFamily = new FontFamily("Segoe UI"),
                FontSize = 9,
                FontWeight = gewicht,
                TextWrapping = TextWrapping.NoWrap,
                IsHitTestVisible = false
            };
            var schluessel = (text, gewicht);
            if (!KostenTextBreiten.TryGetValue(schluessel, out double textBreite))
            {
                block.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                textBreite = block.DesiredSize.Width;
                KostenTextBreiten.Add(schluessel, textBreite);
            }

            Canvas ausschnitt = new Canvas
            {
                Width = breite,
                Height = 14,
                ClipToBounds = true,
                IsHitTestVisible = false
            };
            double versatz = zentriert ? Math.Max(0, (breite - textBreite) / 2) : 0;
            if (textBreite > breite)
            {
                const double geschwindigkeit = 30;
                const double pause = 1;
                double strecke = textBreite - breite + 1;
                double laufzeit = strecke / geschwindigkeit;
                double phase = Environment.TickCount64 / 1000.0 % (2 * (pause + laufzeit));
                if (phase < pause)
                {
                    versatz = 0;
                }
                else if (phase < pause + laufzeit)
                {
                    versatz = -(phase - pause) * geschwindigkeit;
                }
                else if (phase < 2 * pause + laufzeit)
                {
                    versatz = -strecke;
                }
                else
                {
                    versatz = -strecke + (phase - 2 * pause - laufzeit) * geschwindigkeit;
                }
            }

            Canvas.SetLeft(block, versatz);
            ausschnitt.Children.Add(block);
            return ausschnitt;
        }

        private static string FormatCost(IReadOnlyDictionary<G5_Item, int> cost)
        {
            return string.Join(" · ", cost.Select(item =>
                item.Value + " " + (item.Key == G5_Item.Eisenerz ? "Eisenerz" :
                    item.Key == G5_Item.Kupfererz ? "Kupfererz" :
                    item.Key == G5_Item.Holz ? "Holz" :
                    item.Key == G5_Item.Kohle ? "Kohle" : item.Key.ToString())));
        }

        private void ZeichneInventar(Canvas canvas, double viewportBreite, IG5_FabrikSpielInfo info)
        {
            SatisfactoryFieldView fieldView = info as SatisfactoryFieldView;
            bool isOpen = fieldView == null || fieldView.InventarGeoeffnet;
            double width = isOpen ? 268 : 34;
            double left = viewportBreite - width - 10;
            double top = HudHoehe + 10;
            double height = isOpen ? 560 : 34;
            if (fieldView != null)
            {
                fieldView.SetInventarBereich(new Rect(left, top, width, height));
            }

            Border panel = new Border
            {
                Width = width,
                Height = height,
                Padding = new Thickness(isOpen ? 8 : 3),
                Background = new SolidColorBrush(Color.FromArgb(238, 20, 30, 27)),
                BorderBrush = HoverAccentBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(12),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    BlurRadius = 10,
                    ShadowDepth = 2,
                    Opacity = 0.38
                },
                IsHitTestVisible = false
            };

            Canvas content = new Canvas
            {
                Width = Math.Max(0, width - 18),
                Height = Math.Max(0, height - 18),
                IsHitTestVisible = false
            };
            Grid header = new Grid { Width = width - 18, Height = 26 };
            header.ColumnDefinitions.Add(new ColumnDefinition());
            header.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(104) });
            Border toggle = new Border
            {
                Padding = new Thickness(0),
                Height = 18,
                Child = new TextBlock
                {
                    Text = !isOpen ? "›" : "‹ INVENTAR & HERSTELLEN",
                    Foreground = HoverAccentBrush,
                    FontSize = 11,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = isOpen ? TextAlignment.Left : TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            header.Children.Add(toggle);

            content.Children.Add(header);

            if (isOpen)
            {
                // Inventar und Herstellen in einer Ansicht: oben die Gegenstandsliste,
                // darunter Details zum ausgewählten Gegenstand mit Rezept, Anzahl und Knöpfen.
                // Rahmen (1 px) + Innenabstand (8 px) = 9 px Versatz für die Klickflächen.
                const double rand = 9;
                double contentWidth = width - 18;

                G5_Item[] items =
                {
                    G5_Item.Eisenerz,
                    G5_Item.Kupfererz,
                    G5_Item.Kohle,
                    G5_Item.Holz,
                    G5_Item.Kalkstein,
                    G5_Item.Quarz,
                    G5_Item.Schwefel,
                    G5_Item.Uran,
                    G5_Item.Wasser,
                    G5_Item.Gas,
                    G5_Item.Oel,
                    G5_Item.Eisenbarren,
                    G5_Item.Kupferbarren,
                    G5_Item.Eisenplatte,
                    G5_Item.Eisenstange,
                    G5_Item.Beton,
                    G5_Item.Kupferdraht,
                    G5_Item.Schraube,
                    G5_Item.Rotor
                };

                AddMenuText(content,
                    "Rucksack: " + info.SpielerInventarBelegt + "/" + info.SpielerInventarKapazitaet,
                    0, 26, contentWidth, 16, Brushes.White, 11, FontWeights.SemiBold);

                Grid headings = new Grid { Width = contentWidth - 6, Height = 16 };
                headings.ColumnDefinitions.Add(new ColumnDefinition());
                headings.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(62) });
                headings.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(44) });
                AddInventoryCell(headings, "Gegenstand", 0, true);
                AddInventoryCell(headings, "Rucksack", 1, true);
                AddInventoryCell(headings, "Lager", 2, true);
                Canvas.SetLeft(headings, 3);
                Canvas.SetTop(headings, 43);
                content.Children.Add(headings);

                for (int itemIndex = 0; itemIndex < items.Length; itemIndex++)
                {
                    G5_Item item = items[itemIndex];
                    int backpackAmount = info.SpielerInventar.TryGetValue(item, out int heldAmount)
                        ? heldAmount
                        : 0;
                    int storageAmount = info.HubMenge(item) - backpackAmount;
                    double rowWidth = contentWidth - 6;
                    Grid rowContent = new Grid { Width = rowWidth, Height = 17 };
                    rowContent.ColumnDefinitions.Add(new ColumnDefinition());
                    rowContent.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(62) });
                    rowContent.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(44) });
                    bool itemHerstellbar = Handwerksrezepte.Any(recipe => recipe.Output == item);
                    AddInventoryCell(
                        rowContent,
                        ItemAnzeigename(item) + (itemHerstellbar ? " ⚒" : ""),
                        0);
                    AddInventoryCell(rowContent, backpackAmount.ToString(), 1);
                    AddInventoryCell(rowContent, storageAmount.ToString(), 2);

                    bool itemSelected = fieldView != null &&
                        fieldView.AusgewaehlterAbwurfGegenstand == item;
                    Border row = new Border
                    {
                        Width = contentWidth,
                        Height = 19,
                        Padding = new Thickness(3, 1, 3, 1),
                        Background = itemSelected
                            ? new SolidColorBrush(Color.FromArgb(220, 77, 112, 72))
                            : new SolidColorBrush(Color.FromArgb(125, 42, 58, 52)),
                        CornerRadius = new CornerRadius(5),
                        Child = rowContent
                    };
                    double rowTop = 61 + itemIndex * 18;
                    Canvas.SetTop(row, rowTop);
                    content.Children.Add(row);
                    if (fieldView != null)
                    {
                        fieldView.RegisterMenuAktion(
                            new Rect(left + rand + 3, top + rand + rowTop + 1, rowWidth, 17),
                            G5_FabrikAktionstyp.InventarGegenstandWaehlen,
                            wert: item.ToString());
                    }
                }

                // ---- Detailbereich zum ausgewählten Gegenstand ----
                const double detailTop = 409;
                G5_Item? selectedItem = fieldView?.AusgewaehlterAbwurfGegenstand;
                G5_Recipe selectedRecipe = selectedItem.HasValue
                    ? Handwerksrezepte.FirstOrDefault(recipe => recipe.Output == selectedItem.Value)
                    : null;

                Border trennlinie = new Border
                {
                    Width = contentWidth,
                    Height = 1,
                    Background = HoverAccentBrush,
                    Opacity = 0.5,
                    IsHitTestVisible = false
                };
                Canvas.SetTop(trennlinie, detailTop - 3);
                content.Children.Add(trennlinie);

                if (selectedItem.HasValue)
                {
                    int imRucksack = info.SpielerInventar.TryGetValue(selectedItem.Value, out int held)
                        ? held
                        : 0;
                    AddMenuText(
                        content,
                        ItemAnzeigename(selectedItem.Value) + "  ·  Rucksack: " + imRucksack,
                        0, detailTop, contentWidth, 16,
                        HoverAccentBrush, 11, FontWeights.Bold);

                    AddMenuText(
                        content,
                        selectedRecipe != null
                            ? "Benötigt pro Stück (" + selectedRecipe.OutputAmount + "×, 2 s):\n" +
                                FormatCraftingRequirements(selectedRecipe, info.SpielerInventar)
                            : "Kann nicht von Hand hergestellt werden.",
                        0, detailTop + 16, contentWidth, 28,
                        Brushes.White, 9);
                }
                else
                {
                    AddMenuText(
                        content,
                        "Gegenstand anklicken, um Details, Rezept\nund Aktionen zu sehen (⚒ = herstellbar).",
                        0, detailTop, contentWidth, 44,
                        new SolidColorBrush(Color.FromRgb(190, 205, 198)), 9);
                }

                // Anzahl-Feld: wird über die Zahlentasten befüllt und gilt für Herstellen und Ablegen.
                AddMenuText(content, "Anzahl:", 0, detailTop + 47, 50, 16, Brushes.White, 10, FontWeights.SemiBold);
                Border anzahlFeld = new Border
                {
                    Width = 52,
                    Height = 20,
                    Background = new SolidColorBrush(Color.FromArgb(230, 10, 16, 14)),
                    BorderBrush = selectedItem.HasValue ? HoverAccentBrush : Brushes.Gray,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(3),
                    IsHitTestVisible = false,
                    Child = new TextBlock
                    {
                        Text = fieldView?.MengeEingabe ?? "1",
                        Foreground = Brushes.White,
                        FontSize = 11,
                        FontWeight = FontWeights.Bold,
                        TextAlignment = TextAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };
                Canvas.SetLeft(anzahlFeld, 50);
                Canvas.SetTop(anzahlFeld, detailTop + 45);
                content.Children.Add(anzahlFeld);
                if (selectedRecipe != null)
                {
                    AddMenuText(
                        content,
                        "max. " + MaxHerstellbar(selectedRecipe, info.SpielerInventar),
                        108, detailTop + 47, 80, 16,
                        new SolidColorBrush(Color.FromRgb(190, 205, 198)), 9);
                }

                // Knöpfe: Herstellen | Ablegen | Aufnehmen
                double buttonTop = detailTop + 69;
                Border craftButton = CreateMenuButton(
                    "HERSTELLEN ×" + (fieldView?.MengeEingabe ?? "1"),
                    selectedRecipe != null ? HoverAccentBrush : Brushes.Gray,
                    100);
                if (selectedRecipe == null)
                {
                    craftButton.Opacity = 0.45;
                }

                Canvas.SetLeft(craftButton, 0);
                Canvas.SetTop(craftButton, buttonTop);
                content.Children.Add(craftButton);
                if (fieldView != null && selectedRecipe != null)
                {
                    fieldView.RegisterMenuAktion(
                        new Rect(left + rand, top + rand + buttonTop, 100, 30),
                        G5_FabrikAktionstyp.ManuellHerstellen,
                        wert: selectedRecipe.Output.ToString());
                }

                Border dropButton = CreateMenuButton("ABLEGEN", HoverAccentBrush, 64);
                Canvas.SetLeft(dropButton, 104);
                Canvas.SetTop(dropButton, buttonTop);
                content.Children.Add(dropButton);

                G5_GridPosition playerPosition = info.SpielerPosition;
                int aufnehmbareMenge = info.AbgelegteGegenstaende
                    .Where(gegenstand =>
                        gegenstand.X == playerPosition.X &&
                        gegenstand.Y == playerPosition.Y &&
                        gegenstand.Ablaufzeit > DateTimeOffset.UtcNow)
                    .Sum(gegenstand => gegenstand.Anzahl);
                Border pickupButton = CreateMenuButton(
                    aufnehmbareMenge > 0
                        ? "AUFNEHMEN (" + aufnehmbareMenge + ")"
                        : "AUFNEHMEN",
                    new SolidColorBrush(Color.FromRgb(111, 190, 226)),
                    78);
                Canvas.SetLeft(pickupButton, 172);
                Canvas.SetTop(pickupButton, buttonTop);
                content.Children.Add(pickupButton);

                if (fieldView != null)
                {
                    fieldView.RegisterMenuAktion(
                        new Rect(left + rand + 104, top + rand + buttonTop, 64, 30),
                        G5_FabrikAktionstyp.InventarGegenstandAblegen);
                    fieldView.RegisterMenuAktion(
                        new Rect(left + rand + 172, top + rand + buttonTop, 78, 30),
                        G5_FabrikAktionstyp.BodenGegenstaendeAufnehmen);
                }

                // Fortschritt einer laufenden Herstellung.
                if (fieldView != null && fieldView.LaufendesHandwerk != null)
                {
                    int prozent = 100 - (int)Math.Round(
                        100.0 * fieldView.VerbleibendeHandwerkTicks /
                        G5_FabrikSpielfeld.ManuellesHandwerkDauerTicks);
                    AddMenuText(
                        content,
                        "Stellt her: " + ItemAnzeigename(fieldView.LaufendesHandwerk.Output) +
                            " · " + prozent + " % · noch " + fieldView.OffeneHandwerkAnzahl + " Stück",
                        0, buttonTop + 33, contentWidth, 14,
                        HoverAccentBrush, 9, FontWeights.SemiBold);
                }

                AddMenuText(
                    content,
                    "Zahlen: Anzahl · ⌫: löschen · H: herstellen · Enter: ablegen",
                    0,
                    buttonTop + 48,
                    contentWidth,
                    14,
                    new SolidColorBrush(Color.FromRgb(190, 205, 198)),
                    8);
            }

            panel.Child = content;
            Canvas.SetLeft(panel, left);
            Canvas.SetTop(panel, top);
            canvas.Children.Add(panel);
        }

        private static string FormatCraftingRequirements(
            G5_Recipe recipe,
            IReadOnlyDictionary<G5_Item, int> inventory)
        {
            return string.Join(", ", recipe.Ingredients.Select(ingredient =>
            {
                inventory.TryGetValue(ingredient.Key, out int available);
                return ingredient.Value + " " +
                    G5_FabrikBezeichnungen.Gegenstand(ingredient.Key) +
                    " (hast " + available + (available >= ingredient.Value ? " ✓" : " ✗") + ")";
            }));
        }

        // Anzeige-Hilfe: wie oft das Rezept mit dem aktuellen Rucksackinhalt möglich ist
        // (die verbindliche Prüfung inkl. Rucksackplatz macht das Spielfeld).
        private static int MaxHerstellbar(G5_Recipe recipe, IReadOnlyDictionary<G5_Item, int> inventory)
        {
            int maximum = G5_FabrikSpielfeld.MaxHandwerkAnzahl;
            foreach (KeyValuePair<G5_Item, int> ingredient in recipe.Ingredients)
            {
                if (ingredient.Value > 0)
                {
                    inventory.TryGetValue(ingredient.Key, out int available);
                    maximum = Math.Min(maximum, available / ingredient.Value);
                }
            }

            return maximum;
        }

        private static void AddInventoryCell(Grid row, string text, int column, bool header = false)
        {
            TextBlock cell = new TextBlock
            {
                Text = text,
                Foreground = header ? HoverAccentBrush : Brushes.White,
                FontSize = header ? 9 : 10,
                FontWeight = header || column > 0 ? FontWeights.Bold : FontWeights.Normal,
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                TextAlignment = column > 0 ? TextAlignment.Right : TextAlignment.Left
            };
            Grid.SetColumn(cell, column);
            row.Children.Add(cell);
        }

        private static string ItemAnzeigename(G5_Item item)
        {
            switch (item)
            {
                case G5_Item.Eisenerz: return "Eisenerz";
                case G5_Item.Kupfererz: return "Kupfererz";
                case G5_Item.Holz: return "Holz";
                case G5_Item.Kohle: return "Kohle";
                case G5_Item.Kalkstein: return "Kalkstein";
                case G5_Item.Quarz: return "Quarz";
                case G5_Item.Schwefel: return "Schwefel";
                case G5_Item.Uran: return "Uran";
                case G5_Item.Wasser: return "Wasser";
                case G5_Item.Gas: return "Gas";
                case G5_Item.Oel: return "Öl";
                case G5_Item.Eisenbarren: return "Eisenbarren";
                case G5_Item.Kupferbarren: return "Kupferbarren";
                case G5_Item.Eisenplatte: return "Eisenplatte";
                case G5_Item.Eisenstange: return "Eisenstange";
                case G5_Item.Beton: return "Beton";
                case G5_Item.Kupferdraht: return "Kupferdraht";
                case G5_Item.Schraube: return "Schraube";
                case G5_Item.Rotor: return "Rotor";
                default: return item.ToString();
            }
        }

        private static void ZeichneHudZeile(Canvas canvas, double viewportBreite, string text, double y)
        {
            TextBlock zeile = new TextBlock
            {
                Text = text,
                Foreground = Brushes.White,
                Background = Brushes.Transparent,
                Padding = new Thickness(8, 4, 8, 2),
                FontSize = 13,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Width = Math.Max(0, viewportBreite),
                Height = HudZeilenHoehe
            };
            Canvas.SetLeft(zeile, 0);
            Canvas.SetTop(zeile, y);
            canvas.Children.Add(zeile);
        }

        private static string RohstoffName(G5_Rohstofftyp rohstoff)
        {
            switch (rohstoff)
            {
                case G5_Rohstofftyp.Holz: return "Holz";
                case G5_Rohstofftyp.Eisen: return "Eisenerz";
                case G5_Rohstofftyp.Kupfer: return "Kupfererz";
                case G5_Rohstofftyp.Kohle: return "Kohle";
                case G5_Rohstofftyp.Kalkstein: return "Kalkstein";
                case G5_Rohstofftyp.Quarz: return "Quarz";
                case G5_Rohstofftyp.Schwefel: return "Schwefel";
                case G5_Rohstofftyp.Uran: return "Uran";
                case G5_Rohstofftyp.Wasser: return "Wasser";
                case G5_Rohstofftyp.Gas: return "Gas";
                case G5_Rohstofftyp.Oel: return "Öl";
                default: return "Rohstoff";
            }
        }

        private static Brush CreateBrush(byte alpha, byte red, byte green, byte blue)
        {
            SolidColorBrush brush = new SolidColorBrush(Color.FromArgb(alpha, red, green, blue));
            brush.Freeze();
            return brush;
        }
    }
}
