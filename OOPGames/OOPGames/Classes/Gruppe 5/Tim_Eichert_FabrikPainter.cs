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

            Brush farbe;
            string beschriftung;
            switch (gebaeude.Typ)
            {
                case G5_FabrikGebaeudeTyp.Hub:
                    farbe = Brushes.SteelBlue;
                    beschriftung = "HUB";
                    break;
                case G5_FabrikGebaeudeTyp.Foerderer:
                    farbe = Brushes.DarkOrange;
                    beschriftung = "F";
                    break;
                case G5_FabrikGebaeudeTyp.Schmelze:
                    farbe = Brushes.Firebrick;
                    beschriftung = "S";
                    break;
                default:
                    throw new InvalidOperationException("Unbekannter Gebäudetyp in der Darstellung.");
            }

            Rectangle rechteck = new Rectangle
            {
                Width = kachelBreite * gebaeude.Breite,
                Height = kachelHoehe * gebaeude.Hoehe,
                Fill = farbe,
                Stroke = Brushes.White,
                StrokeThickness = 2,
                Opacity = 0.9
            };
            Canvas.SetLeft(rechteck, position.X);
            Canvas.SetTop(rechteck, position.Y);
            canvas.Children.Add(rechteck);

            string status = gebaeude.Typ == G5_FabrikGebaeudeTyp.Hub
                ? "Lager"
                : StatusText(gebaeude.Status);
            TextBlock text = new TextBlock
            {
                Text = beschriftung + Environment.NewLine + status,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                FontSize = Math.Max(9, Math.Min(14, kachelHoehe * 0.4)),
                TextAlignment = TextAlignment.Center,
                Width = rechteck.Width
            };
            Canvas.SetLeft(text, position.X);
            Canvas.SetTop(text, position.Y + rechteck.Height * 0.25);
            canvas.Children.Add(text);
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
