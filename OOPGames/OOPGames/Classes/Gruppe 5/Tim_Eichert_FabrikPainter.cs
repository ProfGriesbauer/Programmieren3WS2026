using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    public sealed class G5_FabrikPainter : IG5_FabrikPainter
    {
        private const double StandardKachelGroesse = 32.0;
        private const double HudZeilenHoehe = 28.0;
        private const double HudHoehe = HudZeilenHoehe * 2;
        private const double VollbildSeitenverhaeltnis = 1.3;

        private static readonly Brush HoverPanelBrush = CreateBrush(245, 20, 30, 31);
        private static readonly Brush HoverAccentBrush = CreateBrush(255, 142, 230, 120);

        private readonly G5_WieseHintergrund _wieseHintergrund = new G5_WieseHintergrund();
        private IG5_WeltSpielfeld _hintergrundWelt;
        private Canvas _hintergrundEbene;
        private Canvas _dynamischeEbene;
        private Canvas _hoverEbene;
        private Canvas _zielCanvas;
        private IG5_WeltSpielfeld _hoverWelt;
        private IReadOnlyList<IG5_GebaeudeInfo> _hoverGebaeude;
        private Border _hoverKarte;
        private string _hoverName;
        private Point _letzteMausposition;
        private bool _hatMausposition;
        private bool _inventarGeoeffnet = true;
        private double _hintergrundViewportBreite = -1;
        private double _hintergrundViewportHoehe = -1;

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
                        mentorField);
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

            canvas.Background = Brushes.DimGray;
            canvas.ClipToBounds = true;
            PasseAnsichtAn(canvas, currentField);
            StelleHintergrundSicher(canvas, currentField);
            _dynamischeEbene.Children.Clear();
            _hoverWelt = currentField;
            _hoverGebaeude = fabrikInfo.Gebaeude;
            long animationTick = currentField is SatisfactoryFieldView satisfactoryField
                ? satisfactoryField.SimulationTickNumber
                : 0;

            ZeichneVorkommen(_dynamischeEbene, currentField);

            foreach (IG5_GebaeudeInfo gebaeude in _hoverGebaeude)
            {
                ZeichneGebaeude(_dynamischeEbene, currentField, gebaeude, animationTick);
            }

            ZeichneHud(_dynamischeEbene, canvas.ActualWidth, fabrikInfo);
            if (_hatMausposition)
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

            if (!welt.TryRasterZuBildschirm(0, 0, out Point ursprung) ||
                !TryGetKachelGroesse(welt, 0, 0, out double kachelBreite, out double kachelHoehe))
            {
                return;
            }

            Image wiese = new Image
            {
                Source = _wieseHintergrund.HoleHintergrund(welt),
                Width = welt.Breite * kachelBreite,
                Height = welt.Hoehe * kachelHoehe,
                Stretch = Stretch.Fill,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(wiese, ursprung.X);
            Canvas.SetTop(wiese, ursprung.Y);
            _hintergrundEbene.Children.Add(wiese);
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
                _zielCanvas.MouseDown -= Canvas_MouseDown;
            }

            _zielCanvas = canvas;
            _zielCanvas.MouseMove += Canvas_MouseMove;
            _zielCanvas.MouseLeave += Canvas_MouseLeave;
            _zielCanvas.MouseDown += Canvas_MouseDown;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!ReferenceEquals(sender, _zielCanvas))
            {
                return;
            }

            _letzteMausposition = e.GetPosition(_zielCanvas);
            _hatMausposition = true;
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

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!ReferenceEquals(sender, _zielCanvas))
            {
                return;
            }

            Point position = e.GetPosition(_zielCanvas);
            double panelWidth = _inventarGeoeffnet ? 190 : 34;
            double left = _zielCanvas.ActualWidth - panelWidth - 10;
            if (position.X >= left && position.X <= left + panelWidth &&
                position.Y >= HudHoehe + 10 && position.Y <= HudHoehe + 190)
            {
                _inventarGeoeffnet = !_inventarGeoeffnet;
            }
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
            IG5_GebaeudeInfo gebaeude,
            long animationTick)
        {
            if (!welt.TryRasterZuBildschirm(gebaeude.X, gebaeude.Y, out Point position) ||
                !TryGetKachelGroesse(welt, gebaeude.X, gebaeude.Y, out double kachelBreite, out double kachelHoehe))
            {
                return;
            }

            double breite = kachelBreite * gebaeude.Breite;
            double hoehe = kachelHoehe * gebaeude.Hoehe;
            Canvas abbildung = G5_FabrikGebaeudeZeichner.Create(
                gebaeude.Typ,
                gebaeude.Level,
                breite,
                hoehe);
            bool conveyorVertical = false;
            bool conveyorReversed = false;
            if (gebaeude.Typ == G5_FabrikGebaeudeTyp.Laufband &&
                welt is SatisfactoryFieldView satisfactoryField &&
                satisfactoryField.TryGetConveyorFlow(
                    gebaeude.X,
                    gebaeude.Y,
                    out conveyorVertical,
                    out conveyorReversed) &&
                conveyorVertical)
            {
                abbildung.RenderTransform = new RotateTransform(90, breite / 2, hoehe / 2);
            }

            Canvas.SetLeft(abbildung, position.X);
            Canvas.SetTop(abbildung, position.Y);
            canvas.Children.Add(abbildung);

            if (gebaeude.Typ == G5_FabrikGebaeudeTyp.Laufband &&
                gebaeude.Status == G5_BuildingStatus.Working)
            {
                ZeichneTransportgut(
                    canvas,
                    position,
                    breite,
                    hoehe,
                    animationTick,
                    conveyorVertical,
                    conveyorReversed);
            }

            Brush levelFarbe = gebaeude.Level >= 3
                ? new SolidColorBrush(Color.FromRgb(191, 89, 64))
                : gebaeude.Level == 2
                    ? new SolidColorBrush(Color.FromRgb(74, 165, 197))
                    : new SolidColorBrush(Color.FromRgb(171, 128, 56));
            Rectangle levelHintergrund = new Rectangle
            {
                Width = Math.Min(23, breite * 0.38),
                Height = Math.Min(11, hoehe * 0.18),
                Fill = levelFarbe,
                Stroke = Brushes.Black,
                StrokeThickness = 0.5
            };
            Canvas.SetLeft(levelHintergrund, position.X + breite - levelHintergrund.Width - 2);
            Canvas.SetTop(levelHintergrund, position.Y + 2);
            canvas.Children.Add(levelHintergrund);

            TextBlock levelText = new TextBlock
            {
                Text = "MK" + Math.Max(1, gebaeude.Level),
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                FontSize = 6,
                TextAlignment = TextAlignment.Center,
                Width = levelHintergrund.Width
            };
            Canvas.SetLeft(levelText, Canvas.GetLeft(levelHintergrund));
            Canvas.SetTop(levelText, Canvas.GetTop(levelHintergrund));
            canvas.Children.Add(levelText);
        }

        private static void ZeichneTransportgut(
            Canvas canvas,
            Point position,
            double width,
            double height,
            long animationTick,
            bool isVertical,
            bool isReversed)
        {
            double itemSize = Math.Max(3, Math.Min(8, Math.Min(width, height) * 0.28));
            double fraction = (animationTick % 8) / 7.0;
            if (isReversed)
            {
                fraction = 1 - fraction;
            }

            Rectangle item = new Rectangle
            {
                Width = itemSize,
                Height = itemSize * 0.7,
                Fill = new SolidColorBrush(Color.FromRgb(255, 205, 76)),
                Stroke = Brushes.White,
                StrokeThickness = 0.6,
                RadiusX = 1.5,
                RadiusY = 1.5,
                IsHitTestVisible = false
            };
            double itemX = isVertical
                ? position.X + width * 0.43
                : position.X + 2 + fraction * Math.Max(0, width - itemSize - 4);
            double itemY = isVertical
                ? position.Y + 2 + fraction * Math.Max(0, height - itemSize - 4)
                : position.Y + height * 0.43;
            Canvas.SetLeft(item, itemX);
            Canvas.SetTop(item, itemY);
            canvas.Children.Add(item);
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
                case G5_FabrikGebaeudeTyp.Laufband:
                    return "Förderband";
                case G5_FabrikGebaeudeTyp.Splitter:
                    return "Splitter";
                case G5_FabrikGebaeudeTyp.Merger:
                    return "Merger";
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

        private static string WerkzeugName(G5_Bauwerkzeug werkzeug)
        {
            switch (werkzeug)
            {
                case G5_Bauwerkzeug.Foerderer:
                    return "Förderer";
                case G5_Bauwerkzeug.Schmelze:
                    return "Schmelze";
                case G5_Bauwerkzeug.Foerderband:
                    return "Förderband";
                case G5_Bauwerkzeug.GebaeudeVerschieben:
                    return "Verschieben";
                default:
                    return "Keines";
            }
        }

        private void ZeichneHud(Canvas canvas, double viewportBreite, IG5_FabrikSpielInfo info)
        {
            string werkzeug = WerkzeugName(info.AktuellesWerkzeug);
            Rectangle hintergrund = new Rectangle
            {
                Width = Math.Max(0, viewportBreite),
                Height = HudHoehe,
                Fill = new SolidColorBrush(Color.FromArgb(235, 16, 20, 18))
            };
            Canvas.SetLeft(hintergrund, 0);
            Canvas.SetTop(hintergrund, 0);
            canvas.Children.Add(hintergrund);

            string steuerung = info is SatisfactoryFieldView
                ? "F: Förderer    S: Schmelze    B: Förderband    M: Gebäude verschieben    Esc: Werkzeug abwählen"
                : "F: Förderer platzieren    S: Schmelze platzieren    Esc: Werkzeug abwählen    Mausklick: Gebäude platzieren";
            ZeichneHudZeile(canvas, viewportBreite, steuerung, 0);
            ZeichneHudZeile(
                canvas,
                viewportBreite,
                "Werkzeug: " + werkzeug + "    |    " + info.StatusMeldung,
                HudZeilenHoehe);
            ZeichneInventar(canvas, viewportBreite, info);
        }

        private void ZeichneInventar(Canvas canvas, double viewportBreite, IG5_FabrikSpielInfo info)
        {
            double width = _inventarGeoeffnet ? 190 : 34;
            double left = viewportBreite - width - 10;
            double top = HudHoehe + 10;

            Border panel = new Border
            {
                Width = width,
                Height = _inventarGeoeffnet ? 196 : 34,
                Padding = new Thickness(_inventarGeoeffnet ? 8 : 3),
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

            StackPanel content = new StackPanel();
            Border toggle = new Border
            {
                Padding = new Thickness(0),
                Margin = new Thickness(0, 0, 0, _inventarGeoeffnet ? 5 : 0),
                Child = new TextBlock
                {
                    Text = _inventarGeoeffnet ? "‹ INVENTAR" : "›",
                    Foreground = HoverAccentBrush,
                    FontSize = 11,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = _inventarGeoeffnet ? TextAlignment.Left : TextAlignment.Center
                }
            };
            content.Children.Add(toggle);

            if (_inventarGeoeffnet)
            {
                G5_Item[] items =
                {
                    G5_Item.Eisenerz,
                    G5_Item.Kupfererz,
                    G5_Item.Eisenbarren,
                    G5_Item.Kupferbarren,
                    G5_Item.Eisenplatte,
                    G5_Item.Eisenstange,
                    G5_Item.Kupferdraht,
                    G5_Item.Schraube,
                    G5_Item.Rotor
                };

                foreach (G5_Item item in items)
                {
                    int amount = info.HubMenge(item);
                    DockPanel rowContent = new DockPanel();
                    Border symbol = ErzeugeItemSymbol(item);
                    DockPanel.SetDock(symbol, Dock.Left);
                    rowContent.Children.Add(symbol);
                    TextBlock amountText = new TextBlock
                    {
                        Text = amount.ToString(),
                        Foreground = Brushes.White,
                        FontSize = 11,
                        FontWeight = FontWeights.Bold,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    DockPanel.SetDock(amountText, Dock.Right);
                    rowContent.Children.Add(amountText);
                    Border row = new Border
                    {
                        Margin = new Thickness(0, 1, 0, 1),
                        Padding = new Thickness(5, 2, 5, 2),
                        Background = new SolidColorBrush(Color.FromArgb(125, 42, 58, 52)),
                        CornerRadius = new CornerRadius(6),
                        Child = rowContent
                    };
                    content.Children.Add(row);
                }
            }

            panel.Child = content;
            Canvas.SetLeft(panel, left);
            Canvas.SetTop(panel, top);
            canvas.Children.Add(panel);
        }

        private static Border ErzeugeItemSymbol(G5_Item item)
        {
            Brush fill;
            switch (item)
            {
                case G5_Item.Eisenerz:
                case G5_Item.Eisenbarren:
                case G5_Item.Eisenplatte:
                case G5_Item.Eisenstange:
                case G5_Item.Schraube:
                case G5_Item.Rotor:
                    fill = new SolidColorBrush(Color.FromRgb(196, 205, 199));
                    break;
                case G5_Item.Kupfererz:
                case G5_Item.Kupferbarren:
                case G5_Item.Kupferdraht:
                    fill = new SolidColorBrush(Color.FromRgb(213, 137, 79));
                    break;
                default:
                    fill = HoverAccentBrush;
                    break;
            }

            TextBlock symbol = new TextBlock
            {
                Text = ItemKuerzel(item),
                Foreground = Brushes.Black,
                FontSize = 8,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            Ellipse icon = new Ellipse
            {
                Width = 18,
                Height = 18,
                Fill = fill,
                Margin = new Thickness(0, 0, 7, 0)
            };
            Grid iconGrid = new Grid();
            iconGrid.Children.Add(icon);
            iconGrid.Children.Add(symbol);
            DockPanel.SetDock(iconGrid, Dock.Left);
            Border wrapper = new Border { Child = iconGrid };
            DockPanel.SetDock(wrapper, Dock.Left);
            return wrapper;
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

        private static string ItemKuerzel(G5_Item item)
        {
            switch (item)
            {
                case G5_Item.Eisenerz: return "Fe";
                case G5_Item.Kupfererz: return "Cu";
                case G5_Item.Eisenbarren: return "Fe";
                case G5_Item.Kupferbarren: return "Cu";
                case G5_Item.Eisenplatte: return "P";
                case G5_Item.Eisenstange: return "St";
                case G5_Item.Kupferdraht: return "Dr";
                case G5_Item.Schraube: return "Sc";
                case G5_Item.Rotor: return "Ro";
                default: return "?";
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
