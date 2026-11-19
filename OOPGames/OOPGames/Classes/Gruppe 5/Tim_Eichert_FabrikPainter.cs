using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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
        private Canvas _zielCanvas;
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

            ZeichneVorkommen(_dynamischeEbene, currentField);

            foreach (IG5_GebaeudeInfo gebaeude in fabrikInfo.Gebaeude)
            {
                ZeichneGebaeude(_dynamischeEbene, currentField, gebaeude);
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
            _zielCanvas = canvas;
            _hintergrundViewportBreite = canvas.ActualWidth;
            _hintergrundViewportHoehe = canvas.ActualHeight;

            _hintergrundEbene = ErzeugeEbene(canvas);
            _dynamischeEbene = ErzeugeEbene(canvas);
            canvas.Children.Add(_hintergrundEbene);
            canvas.Children.Add(_dynamischeEbene);

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
            IG5_GebaeudeInfo gebaeude)
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
            Canvas.SetLeft(abbildung, position.X);
            Canvas.SetTop(abbildung, position.Y);
            canvas.Children.Add(abbildung);

            string status = gebaeude.Typ == G5_FabrikGebaeudeTyp.Hub
                ? "Lager"
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
                Text = GebaeudeKurzname(gebaeude.Typ) + " · " + status,
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
                    return "HUB";
                case G5_FabrikGebaeudeTyp.Foerderer:
                    return "MINER";
                case G5_FabrikGebaeudeTyp.Schmelze:
                    return "SCHMELZE";
                case G5_FabrikGebaeudeTyp.Konstruktor:
                    return "KONSTR.";
                case G5_FabrikGebaeudeTyp.Assembler:
                    return "ASSEMB.";
                case G5_FabrikGebaeudeTyp.Verbrennungsgenerator:
                    return "GENERATOR";
                case G5_FabrikGebaeudeTyp.Strommast:
                    return "STROMMAST";
                case G5_FabrikGebaeudeTyp.Laufband:
                    return "LAUFBAND";
                case G5_FabrikGebaeudeTyp.Splitter:
                    return "SPLITTER";
                case G5_FabrikGebaeudeTyp.Merger:
                    return "MERGER";
                default:
                    throw new InvalidOperationException("Unbekannter Gebäudetyp in der Beschriftung.");
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

        private static void ZeichneHud(Canvas canvas, double viewportBreite, IG5_FabrikSpielInfo info)
        {
            string werkzeug = info.AktuellesWerkzeug == G5_Bauwerkzeug.Keines
                ? "Keines"
                : info.AktuellesWerkzeug.ToString();
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
