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
    public sealed class G5_FabrikPainter : IG5_FabrikPainter
    {
        private const double StandardKachelGroesse = 32.0;
        private const double HudZeilenHoehe = 28.0;
        private const double HudHoehe = HudZeilenHoehe * 2;
        private const double VollbildSeitenverhaeltnis = 1.3;

        private static readonly Brush AusgangsAnschlussBrush =
            new SolidColorBrush(Color.FromRgb(240, 70, 65));
        private static readonly Brush EingangsAnschlussBrush =
            new SolidColorBrush(Color.FromRgb(70, 165, 255));
        private static readonly Brush FoerderbandRandBrush =
            new SolidColorBrush(Color.FromRgb(25, 32, 29));
        private static readonly Brush FoerderbandKernBrush =
            new SolidColorBrush(Color.FromRgb(68, 105, 78));
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

            ZeichneFoerderbandRouten(_dynamischeEbene, currentField, fabrikInfo);
            ZeichneAnschluesse(_dynamischeEbene, currentField, fabrikInfo);
            ZeichneHud(_dynamischeEbene, canvas.ActualWidth, fabrikInfo);

            IG5_GebaeudeInfo ausgewaehltesGebaeude =
                fabrikInfo.Gebaeude.FirstOrDefault(building => building.IstAusgewaehlt);
            if (ausgewaehltesGebaeude != null)
            {
                ZeichneGebaeudeMenue(_dynamischeEbene, ausgewaehltesGebaeude);
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
            }

            _zielCanvas = canvas;
            _zielCanvas.MouseMove += Canvas_MouseMove;
            _zielCanvas.MouseLeave += Canvas_MouseLeave;
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

            if (gebaeude == null || string.IsNullOrWhiteSpace(gebaeude.Name))
            {
                ClearGebaeudeHover();
                return;
            }

            string name = GebaeudeName(gebaeude.Typ);
            if (_hoverKarte == null || !string.Equals(_hoverName, name, StringComparison.Ordinal))
            {
                _hoverName = name;
                _hoverKarte = ErzeugeHoverKarte(_hoverName);
                _hoverEbene.Children.Clear();
                _hoverEbene.Children.Add(_hoverKarte);
            }

            PlatziereHoverKarte(mausposition);
        }

        private Border ErzeugeHoverKarte(string name)
        {
            double width = Math.Max(140, Math.Min(280, _zielCanvas.ActualWidth - 16));
            StackPanel content = new StackPanel();
            content.Children.Add(new TextBlock
            {
                Text = "GEBÄUDE",
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

            string status = gebaeude.Typ == G5_FabrikGebaeudeTyp.Hub
                ? "Lager"
                : gebaeude.Typ == G5_FabrikGebaeudeTyp.Laufband &&
                    gebaeude.Status == G5_BuildingStatus.Working
                    ? "Transport"
                    : StatusText(gebaeude.Status);
            double statusHoehe = Math.Min(15, hoehe * 0.24);
            Rectangle statusHintergrund = new Rectangle
            {
                Width = breite,
                Height = statusHoehe,
                Fill = new SolidColorBrush(Color.FromArgb(205, 24, 29, 27)),
                Stroke = Brushes.Transparent
            };
            Canvas.SetLeft(statusHintergrund, position.X);
            Canvas.SetTop(statusHintergrund, position.Y + hoehe - statusHoehe);
            canvas.Children.Add(statusHintergrund);

            TextBlock text = new TextBlock
            {
                Text = status,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                FontSize = Math.Max(6, Math.Min(9, statusHoehe * 0.65)),
                TextAlignment = TextAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Width = breite
            };
            Canvas.SetLeft(text, position.X);
            Canvas.SetTop(text, position.Y + hoehe - statusHoehe);
            canvas.Children.Add(text);

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

        private static void ZeichneAnschluesse(
            Canvas canvas,
            IG5_WeltSpielfeld welt,
            IG5_FabrikSpielInfo info)
        {
            if (!info.FoerderbaenderAktiv)
            {
                return;
            }

            foreach (IG5_GebaeudeInfo gebaeude in info.Gebaeude)
            {
                if (gebaeude.HatAusgang)
                {
                    ZeichneAnschluss(
                        canvas,
                        welt,
                        gebaeude.AusgangsPosition,
                        AusgangsAnschlussBrush,
                        gebaeude.AusgangAusgewaehlt,
                        istAusgang: true);
                }

                if (gebaeude.HatEingang)
                {
                    ZeichneAnschluss(
                        canvas,
                        welt,
                        gebaeude.EingangsPosition,
                        EingangsAnschlussBrush,
                        ausgewaehlt: false,
                        istAusgang: false);
                }
            }
        }

        private static void ZeichneFoerderbandRouten(
            Canvas canvas,
            IG5_WeltSpielfeld welt,
            IG5_FabrikSpielInfo info)
        {
            foreach (IReadOnlyList<G5_GridPosition> route in info.FoerderbandRouten)
            {
                for (int i = 1; i < route.Count; i++)
                {
                    if (!TryGetKachelMitte(welt, route[i - 1], out Point start) ||
                        !TryGetKachelMitte(welt, route[i], out Point ende) ||
                        !TryGetKachelGroesse(welt, route[i - 1].X, route[i - 1].Y,
                            out double breite, out double hoehe))
                    {
                        continue;
                    }

                    double linienBreite = Math.Max(4, Math.Min(12, Math.Min(breite, hoehe) * 0.42));
                    ZeichneRoutenLinie(canvas, start, ende, FoerderbandRandBrush, linienBreite);
                    ZeichneRoutenLinie(canvas, start, ende, FoerderbandKernBrush, linienBreite * 0.48);
                }
            }
        }

        private static bool TryGetKachelMitte(
            IG5_WeltSpielfeld welt,
            G5_GridPosition position,
            out Point mitte)
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

        private static void ZeichneRoutenLinie(
            Canvas canvas,
            Point start,
            Point ende,
            Brush farbe,
            double breite)
        {
            canvas.Children.Add(new Line
            {
                X1 = start.X,
                Y1 = start.Y,
                X2 = ende.X,
                Y2 = ende.Y,
                Stroke = farbe,
                StrokeThickness = breite,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                IsHitTestVisible = false
            });
        }

        private static void ZeichneAnschluss(
            Canvas canvas,
            IG5_WeltSpielfeld welt,
            G5_GridPosition position,
            Brush farbe,
            bool ausgewaehlt,
            bool istAusgang)
        {
            if (!welt.TryRasterZuBildschirm(position.X, position.Y, out Point rasterPosition) ||
                !TryGetKachelGroesse(welt, position.X, position.Y, out double breite, out double hoehe))
            {
                return;
            }

            double linienStart = istAusgang ? 0.44 : 0.04;
            double linienEnde = istAusgang ? 0.96 : 0.56;
            Line anschluss = new Line
            {
                X1 = rasterPosition.X + breite * linienStart,
                X2 = rasterPosition.X + breite * linienEnde,
                Y1 = rasterPosition.Y + hoehe / 2,
                Y2 = rasterPosition.Y + hoehe / 2,
                Stroke = farbe,
                StrokeThickness = ausgewaehlt ? 4 : 2.5,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
                IsHitTestVisible = false
            };
            canvas.Children.Add(anschluss);
        }

        private static string GebaeudeKurzname(G5_FabrikGebaeudeTyp typ)
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

        private static void ZeichneHud(Canvas canvas, double viewportBreite, IG5_FabrikSpielInfo info)
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

            string foerderbandHinweis = info.FoerderbaenderAktiv
                ? "B: roten Ausgang, dann blauen Eingang anklicken"
                : "Materialübergabe automatisch";
            ZeichneHudZeile(
                canvas,
                viewportBreite,
                "F: Förderer    S: Eisen-Schmelze*    K: Kupfer-Schmelze*    C: Konstruktor*    B: Band verbinden*    Esc: abwählen    " +
                foerderbandHinweis,
                0);
            ZeichneHudZeile(
                canvas,
                viewportBreite,
                "Werkzeug: " + werkzeug + "    |    HUB: Eisenerz " +
                info.HubMenge(G5_Item.Eisenerz) + "    Eisenbarren " +
                info.HubMenge(G5_Item.Eisenbarren) + "    |    " + info.StatusMeldung,
                HudZeilenHoehe);
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

        private static void ZeichneGebaeudeMenue(Canvas canvas, IG5_GebaeudeInfo gebaeude)
        {
            double left = SatisfactoryBuildingMenuLayout.Left;
            double top = SatisfactoryBuildingMenuLayout.Top;
            double width = SatisfactoryBuildingMenuLayout.Width;

            Rectangle panel = new Rectangle
            {
                Width = width,
                Height = SatisfactoryBuildingMenuLayout.Height,
                Fill = new SolidColorBrush(Color.FromArgb(245, 24, 31, 28)),
                Stroke = new SolidColorBrush(Color.FromRgb(116, 151, 124)),
                StrokeThickness = 1.5
            };
            Canvas.SetLeft(panel, left);
            Canvas.SetTop(panel, top);
            canvas.Children.Add(panel);

            ZeichneMenueText(canvas, gebaeude.Name, left + 12, top + 8, width - 58, 18, true);
            ZeichneMenueText(canvas, "Stufe: MK" + gebaeude.Level, left + 12, top + 34, width - 24, 16, false);

            Rectangle schliessen = new Rectangle
            {
                Width = 22,
                Height = 22,
                Fill = new SolidColorBrush(Color.FromRgb(112, 55, 48)),
                Stroke = Brushes.White,
                StrokeThickness = 0.6
            };
            Canvas.SetLeft(schliessen, left + width - 30);
            Canvas.SetTop(schliessen, top + 6);
            canvas.Children.Add(schliessen);
            ZeichneMenueText(canvas, "X", left + width - 29, top + 8, 20, 16, true, TextAlignment.Center);

            IReadOnlyList<string> rezepte = gebaeude.VerfuegbareRezepte;
            ZeichneMenueText(canvas, rezepte.Count > 0 ? "Rezept auswählen:" : "Kein Rezept verfügbar",
                left + 12, top + 66, width - 24, 16, true);

            for (int i = 0; i < rezepte.Count && i < 5; i++)
            {
                double recipeTop = SatisfactoryBuildingMenuLayout.RecipeTop + i * SatisfactoryBuildingMenuLayout.RecipeRowHeight;
                bool selected = string.Equals(
                    rezepte[i],
                    gebaeude.AktuellesRezept,
                    StringComparison.Ordinal);
                Rectangle recipeButton = new Rectangle
                {
                    Width = width - 24,
                    Height = SatisfactoryBuildingMenuLayout.RecipeRowHeight - 3,
                    Fill = selected
                        ? new SolidColorBrush(Color.FromRgb(66, 112, 77))
                        : new SolidColorBrush(Color.FromRgb(48, 58, 52)),
                    Stroke = new SolidColorBrush(Color.FromRgb(104, 127, 110)),
                    StrokeThickness = 0.5
                };
                Canvas.SetLeft(recipeButton, left + 12);
                Canvas.SetTop(recipeButton, recipeTop);
                canvas.Children.Add(recipeButton);
                ZeichneMenueText(canvas, rezepte[i], left + 18, recipeTop + 4, width - 36, 16, false);
            }

            double inputHeaderTop = SatisfactoryBuildingMenuLayout.RecipeTop +
                Math.Max(1, Math.Min(rezepte.Count, 5)) * SatisfactoryBuildingMenuLayout.RecipeRowHeight + 4;
            ZeichneMenueText(canvas, "Eingänge / aktueller Bestand:", left + 12, inputHeaderTop, width - 24, 16, true);

            List<KeyValuePair<G5_Item, int>> eingang = gebaeude.EingangsInhalt
                .OrderBy(item => item.Key)
                .ToList();
            if (eingang.Count == 0)
            {
                ZeichneMenueText(canvas, "Keine Items vorhanden", left + 12, inputHeaderTop + 21, width - 24, 16, false);
                return;
            }

            for (int i = 0; i < eingang.Count && i < 6; i++)
            {
                ZeichneMenueText(
                    canvas,
                    eingang[i].Key + ": " + eingang[i].Value,
                    left + 12,
                    inputHeaderTop + 21 + i * 18,
                    width - 24,
                    16,
                    false);
            }
        }

        private static void ZeichneMenueText(
            Canvas canvas,
            string text,
            double left,
            double top,
            double width,
            double height,
            bool bold,
            TextAlignment alignment = TextAlignment.Left)
        {
            TextBlock label = new TextBlock
            {
                Text = text,
                Foreground = Brushes.White,
                FontWeight = bold ? FontWeights.Bold : FontWeights.Normal,
                FontSize = 12,
                TextAlignment = alignment,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Width = width,
                Height = height,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(label, left);
            Canvas.SetTop(label, top);
            canvas.Children.Add(label);
        }
    }
}
