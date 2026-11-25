using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OOPGames
{
    // Visualisierung des Wiesenhintergrundes und zufaellig erscheinender Kartendetails.
    /// <summary>
    /// Hintergrund-Design "Lebendige Wiese" für das Fabrik-Spielfeld.
    ///
    /// Alle Muster (Grasfarbe, Büschel, Blüten, Erzbrocken) werden nur aus der
    /// Kachelposition berechnet. Dadurch sieht die Karte bei jedem Neuzeichnen
    /// gleich aus und es wird kein Zufallsgenerator mit Zustand benötigt.
    ///
    /// Performance: Der Gras-Hintergrund wird nur einmal als Bitmap erzeugt
    /// (32 px pro Kachel) und danach aus dem Cache geliefert. Der Painter skaliert
    /// das Bild auf die aktuelle Zoomstufe. Rohstoffvorkommen ändern sich während
    /// des Spiels (Menge sinkt, Vorkommen verschwindet) und werden deshalb pro
    /// Tick als eigenes Element über den Hintergrund gelegt.
    /// </summary>
    internal sealed class G5_WieseHintergrund
    {
        private const double RenderKachelGroesse = 32.0;
        private const double Dpi = 96.0;

        private static readonly Color GrasDunkel = Color.FromRgb(0x4B, 0x6A, 0x37);
        private static readonly Color GrasHell = Color.FromRgb(0x6D, 0x8C, 0x44);

        private static readonly Brush GitterBrush = Gefroren(Color.FromArgb(26, 0, 0, 0));
        private static readonly Brush BlueteGelb = Gefroren(Color.FromRgb(0xF2, 0xE0, 0x8A));
        private static readonly Brush BlueteWeiss = Gefroren(Color.FromRgb(0xF4, 0xF0, 0xE6));
        private static readonly Brush ErdeBrush = Gefroren(Color.FromRgb(0x6B, 0x51, 0x36));
        private static readonly Brush FelsGrund = Gefroren(Color.FromRgb(0x4A, 0x4D, 0x4A));
        private static readonly Brush LabelHintergrund = Gefroren(Color.FromArgb(179, 20, 25, 20));
        private static readonly Brush FelsFuellung = Gefroren(new LinearGradientBrush(
            Color.FromRgb(0xB8, 0xB4, 0xA4),
            Color.FromRgb(0x4B, 0x4D, 0x47),
            70));
        private static readonly Brush FelsSchatten = Gefroren(Color.FromArgb(145, 10, 13, 11));
        private static readonly Brush FelsHighlight = Gefroren(Color.FromArgb(80, 245, 239, 218));
        private static readonly Brush BrockenHighlight = Gefroren(Color.FromArgb(85, 255, 255, 255));
        private static readonly Brush QuarzHell = Gefroren(Color.FromRgb(0xF5, 0xF2, 0xE9));
        private static readonly Brush QuarzMittel = Gefroren(Color.FromRgb(0xD5, 0xCE, 0xBF));
        private static readonly Brush QuarzSchatten = Gefroren(Color.FromRgb(0xA7, 0x9B, 0x8B));
        private static readonly Brush UranLeuchten = Gefroren(new RadialGradientBrush(
            Color.FromArgb(145, 0xC6, 0xFF, 0x2C),
            Color.FromArgb(0, 0xA7, 0xF5, 0x20)));

        private static readonly Pen BueschelHell = GefrorenerStift(Color.FromArgb(140, 160, 190, 90), RenderKachelGroesse / 22);
        private static readonly Pen BueschelDunkel = GefrorenerStift(Color.FromArgb(115, 30, 50, 25), RenderKachelGroesse / 22);
        private static readonly Pen BrockenRand = GefrorenerStift(Color.FromArgb(89, 0, 0, 0), RenderKachelGroesse / 40);
        private static readonly Pen FelsKontur = GefrorenerStift(Color.FromArgb(135, 32, 35, 32), RenderKachelGroesse / 42);
        private static readonly Pen QuarzKontur = GefrorenerStift(Color.FromRgb(0x8C, 0x80, 0x73), RenderKachelGroesse / 45);

        private static readonly Brush[] EisenFarben =
        {
            Gefroren(Color.FromRgb(0x9A, 0x6A, 0x5A)),
            Gefroren(Color.FromRgb(0x7D, 0x87, 0x90)),
            Gefroren(Color.FromRgb(0xB5, 0x82, 0x6C))
        };

        private static readonly Brush[] KupferFarben =
        {
            Gefroren(Color.FromRgb(0xD4, 0x84, 0x3E)),
            Gefroren(Color.FromRgb(0x4F, 0xA3, 0x8F)),
            Gefroren(Color.FromRgb(0xB8, 0x69, 0x2F))
        };

        private static readonly Brush[] KohleFarben =
        {
            Gefroren(Color.FromRgb(0x2B, 0x2B, 0x2E)),
            Gefroren(Color.FromRgb(0x44, 0x44, 0x48)),
            Gefroren(Color.FromRgb(0x1C, 0x1C, 0x1E))
        };

        private static readonly Brush[] SonstigeFarben =
        {
            Gefroren(Color.FromRgb(0xC9, 0xC2, 0xB0)),
            Gefroren(Color.FromRgb(0xA8, 0xA1, 0x90)),
            Gefroren(Color.FromRgb(0xDD, 0xD6, 0xC4))
        };

        private static readonly Brush[] SchwefelFarben =
        {
            Gefroren(Color.FromRgb(0x9C, 0x75, 0x16)),
            Gefroren(Color.FromRgb(0xBB, 0x8D, 0x1D)),
            Gefroren(Color.FromRgb(0xD0, 0xA3, 0x2B))
        };

        private static readonly Brush[] UranFarben =
        {
            Gefroren(Color.FromRgb(0xA8, 0xF2, 0x28)),
            Gefroren(Color.FromRgb(0xD8, 0xFF, 0x39)),
            Gefroren(Color.FromRgb(0x70, 0xC9, 0x1C))
        };

        private readonly Dictionary<(G5_Rohstofftyp, int, int), ImageSource> _vorkommenBilder =
            new Dictionary<(G5_Rohstofftyp, int, int), ImageSource>();

        private ImageSource _hintergrund;
        private int _gecachteBreite = -1;
        private int _gecachteHoehe = -1;

        /// <summary>
        /// Liefert den Gras-Hintergrund (inklusive Fels-Hindernisse und Gitter) für
        /// die ganze Karte. Das Bild ist Breite*32 x Hoehe*32 Pixel groß und wird
        /// nur neu erzeugt, wenn sich die Kartengröße ändert oder
        /// <see cref="Zuruecksetzen"/> aufgerufen wurde.
        /// </summary>
        public ImageSource HoleHintergrund(IG5_WeltSpielfeld welt)
        {
            if (welt == null)
            {
                throw new ArgumentNullException(nameof(welt));
            }

            if (_hintergrund != null && _gecachteBreite == welt.Breite && _gecachteHoehe == welt.Hoehe)
            {
                return _hintergrund;
            }

            _hintergrund = ErzeugeHintergrund(welt);
            _gecachteBreite = welt.Breite;
            _gecachteHoehe = welt.Hoehe;
            return _hintergrund;
        }

        /// <summary>
        /// Verwirft den gecachten Hintergrund, z. B. nach einem neuen Spiel
        /// (ClearField), wenn sich Hindernisse geändert haben könnten.
        /// </summary>
        public void Zuruecksetzen()
        {
            _hintergrund = null;
            _gecachteBreite = -1;
            _gecachteHoehe = -1;
        }

        /// <summary>
        /// Erzeugt ein Rohstoffvorkommen (freigelegte Erde mit Erzbrocken und
        /// Mengen-Beschriftung) in der angegebenen Größe. Der Aufrufer setzt die
        /// Position mit Canvas.SetLeft/SetTop.
        /// </summary>
        public UIElement ErzeugeVorkommen(IG5_KachelInfo kachel, int x, int y, double breite, double hoehe)
        {
            if (kachel == null)
            {
                throw new ArgumentNullException(nameof(kachel));
            }

            if (kachel.Rohstoff == G5_Rohstofftyp.Holz)
            {
                return ErzeugeBaum(breite, hoehe);
            }

            Canvas vorkommen = new Canvas
            {
                Width = breite,
                Height = hoehe,
                IsHitTestVisible = false
            };

            Image bild = new Image
            {
                Source = HoleVorkommenBild(kachel.Rohstoff, x, y),
                Width = breite,
                Height = hoehe,
                Stretch = Stretch.Fill
            };
            vorkommen.Children.Add(bild);

            return vorkommen;
        }

        private static UIElement ErzeugeBaum(double breite, double hoehe)
        {
            Canvas baum = new Canvas
            {
                Width = breite,
                Height = hoehe,
                IsHitTestVisible = false
            };
            double baumBreite = breite * 0.72;
            double baumHoehe = hoehe * 0.74;
            baum.Children.Add(new Ellipse
            {
                Width = baumBreite * 0.5,
                Height = hoehe * 0.13,
                Fill = new SolidColorBrush(Color.FromArgb(110, 22, 30, 18))
            });
            Canvas.SetLeft(baum.Children[0], (breite - baumBreite * 0.5) / 2);
            Canvas.SetTop(baum.Children[0], hoehe * 0.78);

            Rectangle stamm = new Rectangle
            {
                Width = Math.Max(3, breite * 0.14),
                Height = hoehe * 0.32,
                Fill = ErdeBrush
            };
            Canvas.SetLeft(stamm, (breite - stamm.Width) / 2);
            Canvas.SetTop(stamm, hoehe * 0.55);
            baum.Children.Add(stamm);

            Brush[] kronenFarben =
            {
                Gefroren(Color.FromRgb(51, 101, 48)),
                Gefroren(Color.FromRgb(68, 128, 58)),
                Gefroren(Color.FromRgb(84, 146, 65))
            };
            for (int index = 0; index < kronenFarben.Length; index++)
            {
                double versatz = index * baumBreite * 0.19;
                Ellipse kronenTeil = new Ellipse
                {
                    Width = baumBreite * 0.56,
                    Height = baumHoehe * 0.48,
                    Fill = kronenFarben[index],
                    Stroke = new SolidColorBrush(Color.FromArgb(120, 25, 55, 24)),
                    StrokeThickness = Math.Max(0.5, breite * 0.025)
                };
                Canvas.SetLeft(kronenTeil, (breite - baumBreite * 0.56) / 2 + versatz - baumBreite * 0.19);
                Canvas.SetTop(kronenTeil, hoehe * (0.08 + index * 0.05));
                baum.Children.Add(kronenTeil);
            }

            return baum;
        }

        private static ImageSource ErzeugeHintergrund(IG5_WeltSpielfeld welt)
        {
            double s = RenderKachelGroesse;
            DrawingVisual visual = new DrawingVisual();

            using (DrawingContext dc = visual.RenderOpen())
            {
                for (int y = 0; y < welt.Hoehe; y++)
                {
                    for (int x = 0; x < welt.Breite; x++)
                    {
                        ZeichneGras(dc, x, y, s);
                    }
                }

                ZeichneGitter(dc, welt.Breite, welt.Hoehe, s);

                for (int y = 0; y < welt.Hoehe; y++)
                {
                    for (int x = 0; x < welt.Breite; x++)
                    {
                        if (welt.TryGetKachelInfo(x, y, out IG5_KachelInfo kachel) &&
                            kachel.Gelaende == G5_Gelaendeart.Hindernis)
                        {
                            ZeichneFels(dc, x, y, s);
                        }
                    }
                }
            }

            int pixelBreite = (int)Math.Ceiling(welt.Breite * s);
            int pixelHoehe = (int)Math.Ceiling(welt.Hoehe * s);
            RenderTargetBitmap bild = new RenderTargetBitmap(pixelBreite, pixelHoehe, Dpi, Dpi, PixelFormats.Pbgra32);
            bild.Render(visual);
            bild.Freeze();
            return bild;
        }

        private static void ZeichneGras(DrawingContext dc, int x, int y, double s)
        {
            double px = x * s;
            double py = y * s;

            // Großflächige Helligkeitsverläufe plus etwas Unruhe pro Kachel.
            double anteil = Rauschen(x, y, 7, 1) * 0.75 + Zufall(x, y, 2) * 0.25;
            dc.DrawRectangle(new SolidColorBrush(Mische(GrasDunkel, GrasHell, anteil)), null, new Rect(px, py, s, s));

            double r = Zufall(x, y, 3);
            if (r > 0.55)
            {
                Pen stift = r > 0.8 ? BueschelHell : BueschelDunkel;
                for (int i = 0; i < 3; i++)
                {
                    double bx = px + s * (0.25 + 0.5 * Zufall(x, y, 20 + i));
                    double by = py + s * (0.35 + 0.5 * Zufall(y, x, 30 + i));
                    double neigung = s * (Zufall(i, x, 4) - 0.5) * 0.2;
                    dc.DrawLine(stift, new Point(bx, by), new Point(bx + neigung, by - s * 0.18));
                }
            }

            if (r > 0.975)
            {
                Brush bluete = Zufall(x, y, 5) > 0.5 ? BlueteGelb : BlueteWeiss;
                dc.DrawEllipse(bluete, null, new Point(px + s * 0.6, py + s * 0.4), s * 0.06, s * 0.06);
            }
        }

        private static void ZeichneGitter(DrawingContext dc, int breite, int hoehe, double s)
        {
            double kartenBreite = breite * s;
            double kartenHoehe = hoehe * s;

            for (int x = 0; x <= breite; x++)
            {
                double linieX = Math.Min(x * s, kartenBreite - 1);
                dc.DrawRectangle(GitterBrush, null, new Rect(linieX, 0, 1, kartenHoehe));
            }

            for (int y = 0; y <= hoehe; y++)
            {
                double linieY = Math.Min(y * s, kartenHoehe - 1);
                dc.DrawRectangle(GitterBrush, null, new Rect(0, linieY, kartenBreite, 1));
            }
        }

        private static void ZeichneFels(DrawingContext dc, int x, int y, double s)
        {
            double px = x * s;
            double py = y * s;
            Point center = new Point(px + s * 0.5, py + s * 0.51);
            dc.DrawRectangle(FelsGrund, null, new Rect(px, py, s, s));
            dc.DrawEllipse(FelsSchatten, null,
                new Point(center.X + s * 0.02, py + s * 0.79), s * 0.39, s * 0.09);
            dc.DrawEllipse(FelsFuellung, FelsKontur, center, s * 0.43, s * 0.34);
            dc.DrawEllipse(FelsHighlight, null,
                new Point(center.X - s * 0.12, center.Y - s * 0.17), s * 0.17, s * 0.055);
        }

        private ImageSource HoleVorkommenBild(G5_Rohstofftyp typ, int x, int y)
        {
            (G5_Rohstofftyp, int, int) schluessel = (typ, x, y);
            if (_vorkommenBilder.TryGetValue(schluessel, out ImageSource vorhanden))
            {
                return vorhanden;
            }

            double s = RenderKachelGroesse;
            DrawingGroup zeichnung = new DrawingGroup();
            using (DrawingContext dc = zeichnung.Open())
            {
                // Clip auf genau eine Kachel, damit das Bild beim Strecken nicht verrutscht.
                dc.PushClip(new RectangleGeometry(new Rect(0, 0, s, s)));
                dc.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, s, s));
                dc.DrawEllipse(ErdeBrush, null, new Point(s / 2, s / 2), s * 0.48, s * 0.44);
                if (typ == G5_Rohstofftyp.Quarz)
                {
                    ZeichneQuarz(dc, s, x, y);
                }
                else
                {
                    if (typ == G5_Rohstofftyp.Uran)
                    {
                        dc.DrawEllipse(UranLeuchten, null, new Point(s / 2, s / 2), s * 0.46, s * 0.42);
                    }

                    ZeichneBrocken(dc, 0, 0, s, ErzFarben(typ), x * 7 + y, 5);
                }
                dc.Pop();
            }

            DrawingImage bild = new DrawingImage(zeichnung);
            bild.Freeze();
            _vorkommenBilder[schluessel] = bild;
            return bild;
        }

        private static void ZeichneQuarz(DrawingContext dc, double s, int x, int y)
        {
            double versatz = (Zufall(x, y, 41) - 0.5) * s * 0.06;
            ZeichneKristall(dc, s * 0.20 + versatz, s * 0.38, s * 0.24, s * 0.42);
            ZeichneKristall(dc, s * 0.58 + versatz, s * 0.34, s * 0.24, s * 0.45);
            ZeichneKristall(dc, s * 0.36 + versatz, s * 0.12, s * 0.29, s * 0.70);
            ZeichneKristall(dc, s * 0.11 + versatz, s * 0.57, s * 0.21, s * 0.25);
            ZeichneKristall(dc, s * 0.69 + versatz, s * 0.55, s * 0.19, s * 0.26);
        }

        private static void ZeichneKristall(DrawingContext dc, double x, double y, double breite, double hoehe)
        {
            Point spitze = new Point(x + breite * 0.55, y);
            Point linksOben = new Point(x + breite * 0.10, y + hoehe * 0.24);
            Point rechtsOben = new Point(x + breite * 0.94, y + hoehe * 0.24);
            Point linksUnten = new Point(x, y + hoehe * 0.82);
            Point rechtsUnten = new Point(x + breite, y + hoehe * 0.82);
            Point mitteUnten = new Point(x + breite * 0.52, y + hoehe);
            Point mitteOben = new Point(x + breite * 0.48, y + hoehe * 0.30);

            dc.DrawGeometry(QuarzMittel, QuarzKontur,
                Polygon(spitze, rechtsOben, rechtsUnten, mitteUnten, linksUnten, linksOben));
            dc.DrawGeometry(QuarzHell, null, Polygon(spitze, linksOben, linksUnten, mitteUnten, mitteOben));
            dc.DrawGeometry(QuarzSchatten, null, Polygon(mitteOben, rechtsOben, rechtsUnten, mitteUnten));
        }

        private static Geometry Polygon(params Point[] punkte)
        {
            StreamGeometry geometrie = new StreamGeometry();
            using (StreamGeometryContext kontext = geometrie.Open())
            {
                kontext.BeginFigure(punkte[0], true, true);
                for (int i = 1; i < punkte.Length; i++)
                {
                    kontext.LineTo(punkte[i], true, false);
                }
            }

            geometrie.Freeze();
            return geometrie;
        }

        /// <summary>
        /// Zeichnet weiche, abgerundete Gesteins- bzw. Erzbrocken.
        /// </summary>
        private static void ZeichneBrocken(
            DrawingContext dc,
            double px,
            double py,
            double s,
            Brush[] farben,
            int seed,
            int anzahl)
        {
            for (int i = 0; i < anzahl; i++)
            {
                double cx = px + s * (0.22 + 0.56 * Zufall(i, seed, 11));
                double cy = py + s * (0.22 + 0.56 * Zufall(seed, i, 12));
                double radius = s * (0.09 + 0.08 * Zufall(i, i + seed, 13));
                double breite = radius * (0.85 + 0.45 * Zufall(i, seed, 14));
                double hoehe = radius * (0.55 + 0.35 * Zufall(seed, i, 15));

                dc.PushTransform(new RotateTransform((Zufall(i, seed, 16) - 0.5) * 40, cx, cy));
                dc.DrawEllipse(farben[i % farben.Length], BrockenRand, new Point(cx, cy), breite, hoehe);
                dc.DrawEllipse(BrockenHighlight, null,
                    new Point(cx - breite * 0.25, cy - hoehe * 0.25),
                    breite * 0.36, hoehe * 0.2);
                dc.Pop();
            }
        }

        private static Brush[] ErzFarben(G5_Rohstofftyp typ)
        {
            switch (typ)
            {
                case G5_Rohstofftyp.Eisen:
                    return EisenFarben;
                case G5_Rohstofftyp.Kupfer:
                    return KupferFarben;
                case G5_Rohstofftyp.Kohle:
                    return KohleFarben;
                case G5_Rohstofftyp.Schwefel:
                    return SchwefelFarben;
                case G5_Rohstofftyp.Uran:
                    return UranFarben;
                default:
                    return SonstigeFarben;
            }
        }

        private static string Kurzzeichen(G5_Rohstofftyp typ)
        {
            switch (typ)
            {
                case G5_Rohstofftyp.Eisen:
                    return "Fe";
                case G5_Rohstofftyp.Kupfer:
                    return "Cu";
                case G5_Rohstofftyp.Kohle:
                    return "C";
                case G5_Rohstofftyp.Quarz:
                    return "Si";
                case G5_Rohstofftyp.Schwefel:
                    return "S";
                case G5_Rohstofftyp.Uran:
                    return "U";
                default:
                    return typ.ToString();
            }
        }

        /// <summary>
        /// Deterministischer Pseudozufall im Bereich [0, 1) aus Position und Seed.
        /// Gleiche Eingaben liefern immer denselben Wert.
        /// </summary>
        private static double Zufall(int x, int y, int seed)
        {
            unchecked
            {
                uint n = (uint)(x * 374761393 + y * 668265263 + seed * 982451653);
                n = (n ^ (n >> 13)) * 1274126177u;
                n ^= n >> 16;
                return n / 4294967296.0;
            }
        }

        /// <summary>
        /// Weiches Rauschen (Value Noise): Zufallswerte auf einem groben Gitter
        /// mit Abstand <paramref name="skala"/> Kacheln, dazwischen geglättet
        /// interpoliert. Ergibt großflächige, fließende Farbverläufe.
        /// </summary>
        private static double Rauschen(int x, int y, double skala, int seed)
        {
            double fx = x / skala;
            double fy = y / skala;
            int x0 = (int)Math.Floor(fx);
            int y0 = (int)Math.Floor(fy);
            double tx = Glaette(fx - x0);
            double ty = Glaette(fy - y0);

            double a = Zufall(x0, y0, seed);
            double b = Zufall(x0 + 1, y0, seed);
            double c = Zufall(x0, y0 + 1, seed);
            double d = Zufall(x0 + 1, y0 + 1, seed);

            double oben = a + (b - a) * tx;
            double unten = c + (d - c) * tx;
            return oben + (unten - oben) * ty;
        }

        private static double Glaette(double t)
        {
            return t * t * (3 - 2 * t);
        }

        private static Color Mische(Color von, Color nach, double anteil)
        {
            anteil = Math.Max(0, Math.Min(1, anteil));
            return Color.FromRgb(
                (byte)Math.Round(von.R + (nach.R - von.R) * anteil),
                (byte)Math.Round(von.G + (nach.G - von.G) * anteil),
                (byte)Math.Round(von.B + (nach.B - von.B) * anteil));
        }

        private static Brush Gefroren(Color farbe)
        {
            SolidColorBrush pinsel = new SolidColorBrush(farbe);
            pinsel.Freeze();
            return pinsel;
        }

        private static Brush Gefroren(Brush pinsel)
        {
            pinsel.Freeze();
            return pinsel;
        }

        private static Pen GefrorenerStift(Color farbe, double dicke)
        {
            Pen stift = new Pen(Gefroren(farbe), Math.Max(0.5, dicke))
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round
            };
            stift.Freeze();
            return stift;
        }
    }
}
