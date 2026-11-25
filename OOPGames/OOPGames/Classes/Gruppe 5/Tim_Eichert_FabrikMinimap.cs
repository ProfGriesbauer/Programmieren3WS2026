using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OOPGames
{
    // Zeichnet die Minimap unten links inklusive Rohstoffsuche.
    // Reine Darstellung: Der Zustand (Minimap sichtbar, gesuchter Rohstoff) liegt im Spielfeld,
    // Klicks werden als Menüaktionen registriert und laufen über Player -> Rules.
    internal sealed class G5_FabrikMinimap
    {
        private const double KartenBreite = 216;
        private const double Rand = 8;
        private const double TitelHoehe = 18;
        private const double ChipBreite = 34;
        private const double ChipHoehe = 16;
        private const double ChipAbstand = 2;
        private const int ChipsProZeile = 6;
        private const double InfoHoehe = 16;

        // Statische Pinsel müssen eingefroren sein, damit sie threadübergreifend nutzbar sind.
        private static readonly Brush AkzentBrush = Eingefroren(new SolidColorBrush(Color.FromRgb(255, 207, 63)));
        private static readonly Color TrefferFarbe = Color.FromRgb(255, 240, 90);
        private static readonly Brush GebaeudeBrush = Eingefroren(new SolidColorBrush(Color.FromRgb(255, 120, 20)));
        private const double GebaeudeMarkerGroesse = 7;
        private const double LegendeHoehe = 14;

        // Alle Rohstoffarten, nach denen gesucht werden kann (ohne "Keiner").
        private static readonly G5_Rohstofftyp[] SuchbareRohstoffe = Enum
            .GetValues(typeof(G5_Rohstofftyp))
            .Cast<G5_Rohstofftyp>()
            .Where(rohstoff => rohstoff != G5_Rohstofftyp.Keiner)
            .ToArray();

        // Zeichnet die Minimap (oder bei ausgeblendeter Minimap nur einen kleinen Knopf)
        // mit linker unterer Ecke bei (left, bottom).
        internal void Zeichne(
            Canvas canvas,
            double left,
            double bottom,
            double viewportBreite,
            double viewportHoehe,
            double hudHoehe,
            SatisfactoryFieldView view)
        {
            G5_FabrikSpielfeld spielfeld = view.Spielfeld;
            IG5_WeltSpielfeld welt = view;
            IG5_FabrikSpielInfo info = view;

            if (!spielfeld.MinimapSichtbar)
            {
                Rect knopf = new Rect(left, bottom - 26, 96, 26);
                Border einblenden = ErzeugeKnopf("▣ KARTE (M)", knopf.Width, knopf.Height, AkzentBrush);
                Canvas.SetLeft(einblenden, knopf.X);
                Canvas.SetTop(einblenden, knopf.Y);
                canvas.Children.Add(einblenden);
                view.SetMinimapBereich(knopf);
                view.RegisterMenuAktion(knopf, G5_FabrikAktionstyp.MinimapUmschalten);
                return;
            }

            double kartenHoehe = KartenBreite * welt.Hoehe / Math.Max(1, welt.Breite);
            double massstab = KartenBreite / Math.Max(1, welt.Breite);
            int chipZeilen = (SuchbareRohstoffe.Length + ChipsProZeile - 1) / ChipsProZeile;
            double panelBreite = KartenBreite + 2 * Rand;
            double panelHoehe = Rand + TitelHoehe + kartenHoehe + LegendeHoehe + 4 +
                chipZeilen * (ChipHoehe + ChipAbstand) + InfoHoehe + Rand;
            double top = bottom - panelHoehe;
            view.SetMinimapBereich(new Rect(left, top, panelBreite, panelHoehe));

            Border panel = new Border
            {
                Width = panelBreite,
                Height = panelHoehe,
                Background = new SolidColorBrush(Color.FromArgb(238, 20, 30, 27)),
                BorderBrush = AkzentBrush,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(10),
                IsHitTestVisible = false
            };
            Canvas.SetLeft(panel, left);
            Canvas.SetTop(panel, top);
            canvas.Children.Add(panel);

            // Titelzeile: Klick blendet die Minimap aus.
            Rect titel = new Rect(left + Rand, top + Rand - 2, KartenBreite, TitelHoehe);
            FuegeTextHinzu(canvas, "‹ MINIMAP   ·   M: aus   ·   R: Rohstoff suchen",
                titel.X, titel.Y, titel.Width, titel.Height, AkzentBrush, 9, FontWeights.Bold);
            view.RegisterMenuAktion(titel, G5_FabrikAktionstyp.MinimapUmschalten);

            double kartenLeft = left + Rand;
            double kartenTop = top + Rand + TitelHoehe;
            G5_Rohstofftyp gesucht = spielfeld.GesuchterRohstoff;

            // 1. Gelände und Rohstoffe als Bitmap (1 Pixel = 1 Kachel), scharf hochskaliert.
            Image karte = new Image
            {
                Source = ErzeugeKartenbild(welt, gesucht),
                Width = KartenBreite,
                Height = kartenHoehe,
                Stretch = Stretch.Fill,
                IsHitTestVisible = false
            };
            RenderOptions.SetBitmapScalingMode(karte, BitmapScalingMode.NearestNeighbor);
            Canvas.SetLeft(karte, kartenLeft);
            Canvas.SetTop(karte, kartenTop);
            canvas.Children.Add(karte);

            // 2. Eigene Gebäude als deutliche Markierung (Förderbänder werden nicht angezeigt).
            foreach (IG5_GebaeudeInfo gebaeude in info.Gebaeude)
            {
                if (gebaeude.Typ == G5_FabrikGebaeudeTyp.Laufband)
                {
                    continue;
                }

                double mitteX = kartenLeft + (gebaeude.X + gebaeude.Breite / 2.0) * massstab;
                double mitteY = kartenTop + (gebaeude.Y + gebaeude.Hoehe / 2.0) * massstab;
                double groesse = Math.Max(GebaeudeMarkerGroesse, Math.Max(gebaeude.Breite, gebaeude.Hoehe) * massstab);
                Rectangle marker = new Rectangle
                {
                    Width = groesse,
                    Height = groesse,
                    Fill = GebaeudeBrush,
                    Stroke = Brushes.White,
                    StrokeThickness = 1.2,
                    RadiusX = 1.5,
                    RadiusY = 1.5,
                    ToolTip = gebaeude.Name,
                    IsHitTestVisible = false
                };
                Canvas.SetLeft(marker, mitteX - groesse / 2);
                Canvas.SetTop(marker, mitteY - groesse / 2);
                canvas.Children.Add(marker);
            }

            // 3. Aktuell sichtbarer Kartenausschnitt als Rahmen.
            ZeichneSichtfenster(canvas, welt, kartenLeft, kartenTop, kartenHoehe, massstab,
                viewportBreite, viewportHoehe, hudHoehe);

            // 4. Spieler.
            G5_GridPosition spieler = info.SpielerPosition;
            Point spielerPunkt = new Point(
                kartenLeft + (spieler.X + 0.5) * massstab,
                kartenTop + (spieler.Y + 0.5) * massstab);

            // 5. Rohstoffsuche: Pfeil vom Spieler zum nächsten Vorkommen.
            string infoText = "Rohstoff anklicken oder R drücken, um zu suchen.";
            if (gesucht != G5_Rohstofftyp.Keiner)
            {
                if (spielfeld.TryFindeNaechstesVorkommen(gesucht, out G5_GridPosition ziel))
                {
                    Point zielPunkt = new Point(
                        kartenLeft + (ziel.X + 0.5) * massstab,
                        kartenTop + (ziel.Y + 0.5) * massstab);
                    ZeichnePfeil(canvas, spielerPunkt, zielPunkt);
                    Ellipse zielKreis = new Ellipse
                    {
                        Width = 12,
                        Height = 12,
                        Stroke = AkzentBrush,
                        StrokeThickness = 2,
                        IsHitTestVisible = false
                    };
                    Canvas.SetLeft(zielKreis, zielPunkt.X - 6);
                    Canvas.SetTop(zielKreis, zielPunkt.Y - 6);
                    canvas.Children.Add(zielKreis);

                    int dx = ziel.X - spieler.X;
                    int dy = ziel.Y - spieler.Y;
                    infoText = RohstoffName(gesucht) + ": " +
                        (int)Math.Round(Math.Sqrt(dx * dx + dy * dy)) + " Felder " + Richtungstext(dx, dy);
                }
                else
                {
                    infoText = RohstoffName(gesucht) + ": kein Vorkommen gefunden.";
                }
            }

            Ellipse spielerPunktForm = new Ellipse
            {
                Width = 7,
                Height = 7,
                Fill = Brushes.White,
                Stroke = Brushes.Black,
                StrokeThickness = 1.5,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(spielerPunktForm, spielerPunkt.X - 3.5);
            Canvas.SetTop(spielerPunktForm, spielerPunkt.Y - 3.5);
            canvas.Children.Add(spielerPunktForm);

            // 6. Legende unter der Karte.
            int gebaeudeAnzahl = info.Gebaeude.Count(gebaeude => gebaeude.Typ != G5_FabrikGebaeudeTyp.Laufband);
            double legendeTop = kartenTop + kartenHoehe + 2;
            Rectangle legendeGebaeude = new Rectangle
            {
                Width = 8,
                Height = 8,
                Fill = GebaeudeBrush,
                Stroke = Brushes.White,
                StrokeThickness = 1,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(legendeGebaeude, kartenLeft);
            Canvas.SetTop(legendeGebaeude, legendeTop + 3);
            canvas.Children.Add(legendeGebaeude);
            FuegeTextHinzu(canvas, "Gebäude (" + gebaeudeAnzahl + ")      ●  Du      ┅  Sicht",
                kartenLeft + 12, legendeTop, KartenBreite - 12, LegendeHoehe,
                Brushes.White, 9, FontWeights.Normal);

            // 7. Rohstoff-Knöpfe zur Suche.
            HashSet<G5_Rohstofftyp> vorhanden = new HashSet<G5_Rohstofftyp>(spielfeld.VorhandeneRohstoffe());
            double chipsTop = legendeTop + LegendeHoehe + 2;
            for (int index = 0; index < SuchbareRohstoffe.Length; index++)
            {
                G5_Rohstofftyp rohstoff = SuchbareRohstoffe[index];
                Rect chip = new Rect(
                    kartenLeft + (index % ChipsProZeile) * (ChipBreite + ChipAbstand),
                    chipsTop + (index / ChipsProZeile) * (ChipHoehe + ChipAbstand),
                    ChipBreite,
                    ChipHoehe);
                Color farbe = RohstoffFarbe(rohstoff);
                bool aktiv = rohstoff == gesucht;
                Border chipForm = new Border
                {
                    Width = chip.Width,
                    Height = chip.Height,
                    Background = new SolidColorBrush(farbe),
                    BorderBrush = aktiv ? AkzentBrush : Brushes.Black,
                    BorderThickness = new Thickness(aktiv ? 2 : 1),
                    CornerRadius = new CornerRadius(3),
                    Opacity = vorhanden.Contains(rohstoff) ? 1.0 : 0.35,
                    ToolTip = RohstoffName(rohstoff),
                    IsHitTestVisible = false,
                    Child = new TextBlock
                    {
                        Text = KurzName(rohstoff),
                        Foreground = IstHell(farbe) ? Brushes.Black : Brushes.White,
                        FontSize = 8,
                        FontWeight = FontWeights.Bold,
                        TextAlignment = TextAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };
                Canvas.SetLeft(chipForm, chip.X);
                Canvas.SetTop(chipForm, chip.Y);
                canvas.Children.Add(chipForm);
                view.RegisterMenuAktion(chip, G5_FabrikAktionstyp.RohstoffSucheWaehlen, wert: rohstoff.ToString());
            }

            FuegeTextHinzu(canvas, infoText,
                kartenLeft, chipsTop + chipZeilen * (ChipHoehe + ChipAbstand) + 1, KartenBreite, InfoHoehe,
                gesucht == G5_Rohstofftyp.Keiner ? new SolidColorBrush(Color.FromRgb(190, 205, 198)) : AkzentBrush,
                9, FontWeights.SemiBold);
        }

        // Pfeil am Spieler auf der großen Karte, der zum nächsten gesuchten Vorkommen zeigt.
        internal static void ZeichneSuchpfeilAmSpieler(
            Canvas canvas,
            Point spielerMitte,
            double kachelGroesse,
            G5_GridPosition spieler,
            G5_GridPosition ziel)
        {
            int dx = ziel.X - spieler.X;
            int dy = ziel.Y - spieler.Y;
            if (Math.Abs(dx) + Math.Abs(dy) <= 1)
            {
                return;
            }

            double laenge = Math.Sqrt(dx * dx + dy * dy);
            double ux = dx / laenge;
            double uy = dy / laenge;
            double abstand = kachelGroesse * 0.95;
            Point spitze = new Point(spielerMitte.X + ux * (abstand + 10), spielerMitte.Y + uy * (abstand + 10));
            Point basis = new Point(spielerMitte.X + ux * abstand, spielerMitte.Y + uy * abstand);
            Polygon pfeil = new Polygon
            {
                Points = new PointCollection
                {
                    spitze,
                    new Point(basis.X - uy * 7, basis.Y + ux * 7),
                    new Point(basis.X + uy * 7, basis.Y - ux * 7)
                },
                Fill = new SolidColorBrush(TrefferFarbe),
                Stroke = Brushes.Black,
                StrokeThickness = 1.2,
                IsHitTestVisible = false
            };
            canvas.Children.Add(pfeil);
        }

        internal static string RohstoffName(G5_Rohstofftyp rohstoff)
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
                default: return "–";
            }
        }

        // Himmelsrichtung für eine Rasterverschiebung (y wächst nach Süden).
        internal static string Richtungstext(int dx, int dy)
        {
            if (dx == 0 && dy == 0)
            {
                return "direkt hier";
            }

            double winkel = Math.Atan2(-dy, dx) * 180.0 / Math.PI;
            if (winkel < 0)
            {
                winkel += 360;
            }

            string[] richtungen =
            {
                "östlich", "nordöstlich", "nördlich", "nordwestlich",
                "westlich", "südwestlich", "südlich", "südöstlich"
            };
            return richtungen[(int)Math.Round(winkel / 45.0) % 8];
        }

        private static ImageSource ErzeugeKartenbild(IG5_WeltSpielfeld welt, G5_Rohstofftyp gesucht)
        {
            int breite = Math.Max(1, welt.Breite);
            int hoehe = Math.Max(1, welt.Hoehe);
            int[] pixel = new int[breite * hoehe];
            bool sucheAktiv = gesucht != G5_Rohstofftyp.Keiner;

            for (int y = 0; y < hoehe; y++)
            {
                for (int x = 0; x < breite; x++)
                {
                    Color farbe;
                    if (!welt.TryGetKachelInfo(x, y, out IG5_KachelInfo kachel) || !kachel.IstErkundet)
                    {
                        farbe = Color.FromRgb(20, 22, 22);
                    }
                    else if (kachel.Rohstoff != G5_Rohstofftyp.Keiner)
                    {
                        farbe = !sucheAktiv
                            ? RohstoffFarbe(kachel.Rohstoff)
                            : kachel.Rohstoff == gesucht
                                ? TrefferFarbe
                                : Abdunkeln(RohstoffFarbe(kachel.Rohstoff));
                    }
                    else
                    {
                        farbe = kachel.Gelaende == G5_Gelaendeart.Hindernis
                            ? Color.FromRgb(105, 100, 95)
                            : Color.FromRgb(60, 95, 55);
                        if (sucheAktiv)
                        {
                            farbe = Abdunkeln(farbe);
                        }
                    }

                    pixel[y * breite + x] = (255 << 24) | (farbe.R << 16) | (farbe.G << 8) | farbe.B;
                }
            }

            WriteableBitmap bild = new WriteableBitmap(breite, hoehe, 96, 96, PixelFormats.Bgra32, null);
            bild.WritePixels(new Int32Rect(0, 0, breite, hoehe), pixel, breite * 4, 0);
            bild.Freeze();
            return bild;
        }

        private static void ZeichneSichtfenster(
            Canvas canvas,
            IG5_WeltSpielfeld welt,
            double kartenLeft,
            double kartenTop,
            double kartenHoehe,
            double massstab,
            double viewportBreite,
            double viewportHoehe,
            double hudHoehe)
        {
            // Kachelgröße und Versatz aus zwei Rasterpunkten bestimmen (gleichmäßiges Raster).
            if (!welt.TryRasterZuBildschirm(0, 0, out Point p0) ||
                !welt.TryRasterZuBildschirm(1, 1, out Point p1) ||
                p1.X - p0.X <= 0 || p1.Y - p0.Y <= 0)
            {
                return;
            }

            double kachelBreite = p1.X - p0.X;
            double kachelHoehe = p1.Y - p0.Y;
            double x1 = Math.Max(0, (0 - p0.X) / kachelBreite);
            double y1 = Math.Max(0, (hudHoehe - p0.Y) / kachelHoehe);
            double x2 = Math.Min(welt.Breite, (viewportBreite - p0.X) / kachelBreite);
            double y2 = Math.Min(welt.Hoehe, (viewportHoehe - p0.Y) / kachelHoehe);
            if (x2 <= x1 || y2 <= y1)
            {
                return;
            }

            Rectangle rahmen = new Rectangle
            {
                Width = Math.Min(KartenBreite, (x2 - x1) * massstab),
                Height = Math.Min(kartenHoehe, (y2 - y1) * massstab),
                Stroke = Brushes.White,
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 3, 2 },
                IsHitTestVisible = false
            };
            Canvas.SetLeft(rahmen, kartenLeft + x1 * massstab);
            Canvas.SetTop(rahmen, kartenTop + y1 * massstab);
            canvas.Children.Add(rahmen);
        }

        private static void ZeichnePfeil(Canvas canvas, Point start, Point ende)
        {
            double dx = ende.X - start.X;
            double dy = ende.Y - start.Y;
            double laenge = Math.Sqrt(dx * dx + dy * dy);
            if (laenge < 4)
            {
                return;
            }

            double ux = dx / laenge;
            double uy = dy / laenge;
            Point spitze = new Point(ende.X - ux * 6, ende.Y - uy * 6);
            canvas.Children.Add(new Line
            {
                X1 = start.X,
                Y1 = start.Y,
                X2 = spitze.X,
                Y2 = spitze.Y,
                Stroke = AkzentBrush,
                StrokeThickness = 2,
                IsHitTestVisible = false
            });
            canvas.Children.Add(new Polygon
            {
                Points = new PointCollection
                {
                    spitze,
                    new Point(spitze.X - ux * 7 - uy * 4, spitze.Y - uy * 7 + ux * 4),
                    new Point(spitze.X - ux * 7 + uy * 4, spitze.Y - uy * 7 - ux * 4)
                },
                Fill = AkzentBrush,
                IsHitTestVisible = false
            });
        }

        private static Color RohstoffFarbe(G5_Rohstofftyp rohstoff)
        {
            switch (rohstoff)
            {
                case G5_Rohstofftyp.Holz: return Color.FromRgb(139, 94, 52);
                case G5_Rohstofftyp.Eisen: return Color.FromRgb(190, 110, 90);
                case G5_Rohstofftyp.Kupfer: return Color.FromRgb(215, 125, 45);
                case G5_Rohstofftyp.Kohle: return Color.FromRgb(45, 45, 50);
                case G5_Rohstofftyp.Kalkstein: return Color.FromRgb(220, 215, 195);
                case G5_Rohstofftyp.Quarz: return Color.FromRgb(230, 170, 230);
                case G5_Rohstofftyp.Schwefel: return Color.FromRgb(225, 215, 60);
                case G5_Rohstofftyp.Uran: return Color.FromRgb(90, 230, 90);
                case G5_Rohstofftyp.Wasser: return Color.FromRgb(60, 130, 220);
                case G5_Rohstofftyp.Gas: return Color.FromRgb(170, 200, 210);
                case G5_Rohstofftyp.Oel: return Color.FromRgb(95, 45, 110);
                default: return Color.FromRgb(60, 95, 55);
            }
        }

        private static string KurzName(G5_Rohstofftyp rohstoff)
        {
            switch (rohstoff)
            {
                case G5_Rohstofftyp.Eisen: return "Eisen";
                case G5_Rohstofftyp.Kupfer: return "Kupfer";
                case G5_Rohstofftyp.Kalkstein: return "Kalk";
                case G5_Rohstofftyp.Schwefel: return "Schwef.";
                default: return RohstoffName(rohstoff);
            }
        }

        private static Brush Eingefroren(SolidColorBrush brush)
        {
            brush.Freeze();
            return brush;
        }

        private static Color Abdunkeln(Color farbe)
        {
            return Color.FromRgb((byte)(farbe.R * 0.35), (byte)(farbe.G * 0.35), (byte)(farbe.B * 0.35));
        }

        private static bool IstHell(Color farbe)
        {
            return 0.299 * farbe.R + 0.587 * farbe.G + 0.114 * farbe.B > 150;
        }

        private static Border ErzeugeKnopf(string text, double breite, double hoehe, Brush akzent)
        {
            return new Border
            {
                Width = breite,
                Height = hoehe,
                Background = new SolidColorBrush(Color.FromArgb(230, 20, 30, 27)),
                BorderBrush = akzent,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(5),
                IsHitTestVisible = false,
                Child = new TextBlock
                {
                    Text = text,
                    Foreground = akzent,
                    FontSize = 10,
                    FontWeight = FontWeights.Bold,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
        }

        private static void FuegeTextHinzu(
            Canvas canvas,
            string text,
            double left,
            double top,
            double breite,
            double hoehe,
            Brush farbe,
            double schriftgroesse,
            FontWeight gewicht)
        {
            TextBlock block = new TextBlock
            {
                Text = text,
                Width = breite,
                Height = hoehe,
                Foreground = farbe,
                FontSize = schriftgroesse,
                FontWeight = gewicht,
                TextTrimming = TextTrimming.CharacterEllipsis,
                IsHitTestVisible = false
            };
            Canvas.SetLeft(block, left);
            Canvas.SetTop(block, top);
            canvas.Children.Add(block);
        }
    }
}
