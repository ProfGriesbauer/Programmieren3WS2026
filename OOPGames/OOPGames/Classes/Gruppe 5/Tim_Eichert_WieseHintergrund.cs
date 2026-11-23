using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

        private static readonly Pen BueschelHell = GefrorenerStift(Color.FromArgb(140, 160, 190, 90), RenderKachelGroesse / 22);
        private static readonly Pen BueschelDunkel = GefrorenerStift(Color.FromArgb(115, 30, 50, 25), RenderKachelGroesse / 22);
        private static readonly Pen BrockenRand = GefrorenerStift(Color.FromArgb(89, 0, 0, 0), RenderKachelGroesse / 40);

        private static readonly Brush[] FelsFarben =
        {
            Gefroren(Color.FromRgb(0x7B, 0x7F, 0x78)),
            Gefroren(Color.FromRgb(0x62, 0x66, 0x5F)),
            Gefroren(Color.FromRgb(0x8F, 0x93, 0x8A))
        };

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
            dc.DrawRectangle(FelsGrund, null, new Rect(x * s, y * s, s, s));
            ZeichneBrocken(dc, x * s, y * s, s, FelsFarben, x * 31 + y, 4);
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
                ZeichneBrocken(dc, 0, 0, s, ErzFarben(typ), x * 7 + y, 5);
                dc.Pop();
            }

            DrawingImage bild = new DrawingImage(zeichnung);
            bild.Freeze();
            _vorkommenBilder[schluessel] = bild;
            return bild;
        }

        /// <summary>
        /// Zeichnet unregelmäßige Sechsecke als Gesteins- bzw. Erzbrocken.
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

                StreamGeometry form = new StreamGeometry();
                using (StreamGeometryContext ctx = form.Open())
                {
                    for (int k = 0; k < 6; k++)
                    {
                        double winkel = k / 6.0 * Math.PI * 2 + Zufall(i, k, seed);
                        double r = radius * (0.7 + 0.5 * Zufall(k, i, seed + 3));
                        Point ecke = new Point(cx + Math.Cos(winkel) * r, cy + Math.Sin(winkel) * r);
                        if (k == 0)
                        {
                            ctx.BeginFigure(ecke, true, true);
                        }
                        else
                        {
                            ctx.LineTo(ecke, true, false);
                        }
                    }
                }

                form.Freeze();
                dc.DrawGeometry(farben[i % farben.Length], BrockenRand, form);
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
