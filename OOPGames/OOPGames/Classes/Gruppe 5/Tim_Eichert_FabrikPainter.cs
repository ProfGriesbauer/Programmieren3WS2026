using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    public sealed class G5_FabrikPainter : IG5_FabrikPainter
    {
        private const double StandardKachelGroesse = 32.0;
        private const double HudHoehe = 36.0;

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

            canvas.Children.Clear();
            canvas.Background = Brushes.DimGray;

            for (int y = 0; y < currentField.Hoehe; y++)
            {
                for (int x = 0; x < currentField.Breite; x++)
                {
                    if (!currentField.TryGetKachelInfo(x, y, out IG5_KachelInfo kachel) ||
                        !currentField.TryRasterZuBildschirm(x, y, out Point position) ||
                        !TryGetKachelGroesse(currentField, x, y, out double breite, out double hoehe))
                    {
                        continue;
                    }

                    ZeichneKachel(canvas, position, breite, hoehe, kachel);
                }
            }

            foreach (IG5_GebaeudeInfo gebaeude in fabrikInfo.Gebaeude)
            {
                ZeichneGebaeude(canvas, currentField, gebaeude);
            }

            ZeichneHud(canvas, fabrikInfo);
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

        private static void ZeichneKachel(
            Canvas canvas,
            Point position,
            double breite,
            double hoehe,
            IG5_KachelInfo kachel)
        {
            Brush hintergrund = kachel.Gelaende == G5_Gelaendeart.Hindernis
                ? Brushes.SlateGray
                : kachel.Rohstoff == G5_Rohstofftyp.Keiner
                    ? Brushes.DarkOliveGreen
                    : Brushes.SaddleBrown;

            Rectangle rechteck = new Rectangle
            {
                Width = breite,
                Height = hoehe,
                Fill = hintergrund,
                Stroke = Brushes.Black,
                StrokeThickness = 1
            };
            Canvas.SetLeft(rechteck, position.X);
            Canvas.SetTop(rechteck, position.Y);
            canvas.Children.Add(rechteck);

            string text = kachel.Gelaende == G5_Gelaendeart.Hindernis
                ? "Fels"
                : RessourcenText(kachel);
            if (!string.IsNullOrEmpty(text))
            {
                TextBlock beschriftung = new TextBlock
                {
                    Text = text,
                    Foreground = Brushes.White,
                    FontSize = Math.Max(8, Math.Min(12, hoehe * 0.3)),
                    TextAlignment = TextAlignment.Center,
                    Width = breite
                };
                Canvas.SetLeft(beschriftung, position.X);
                Canvas.SetTop(beschriftung, position.Y + hoehe * 0.32);
                canvas.Children.Add(beschriftung);
            }
        }

        private static string RessourcenText(IG5_KachelInfo kachel)
        {
            if (kachel.Rohstoff == G5_Rohstofftyp.Keiner)
            {
                return string.Empty;
            }

            string symbol = kachel.Rohstoff == G5_Rohstofftyp.Eisen
                ? "Fe"
                : kachel.Rohstoff.ToString();
            return symbol + " " + kachel.RohstoffMenge;
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
                default:
                    return "Bereit";
            }
        }

        private static void ZeichneHud(Canvas canvas, IG5_FabrikSpielInfo info)
        {
            string werkzeug = info.AktuellesWerkzeug == G5_Bauwerkzeug.Keines
                ? "Keines"
                : info.AktuellesWerkzeug.ToString();
            string text =
                "F: Förderer   S: Schmelze   Esc: Abbrechen   |   " +
                "Bauwerkzeug: " + werkzeug + "   |   HUB: Eisenerz " +
                info.HubMenge(G5_Item.Eisenerz) + "   Eisenbarren " +
                info.HubMenge(G5_Item.Eisenbarren);

            TextBlock hud = new TextBlock
            {
                Text = text + Environment.NewLine + info.StatusMeldung,
                Foreground = Brushes.White,
                Background = Brushes.Black,
                Padding = new Thickness(5),
                FontSize = 13
            };
            Canvas.SetLeft(hud, 4);
            Canvas.SetTop(hud, 2);
            canvas.Children.Add(hud);
        }
    }
}
