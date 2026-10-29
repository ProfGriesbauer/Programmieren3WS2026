using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace OOPGames
{
    internal static class Gruppe2_4GWKonstanten
    {
        public const int Zeilen = 6;
        public const int Spalten = 7;
        public const int Zellgroesse = 40;
    }

    public class Gruppe2_4GWFeld : I4GWFeld
    {
        private readonly int[,] _felder =
            new int[Gruppe2_4GWKonstanten.Zeilen, Gruppe2_4GWKonstanten.Spalten];

        public int this[int zeile, int spalte]
        {
            get
            {
                if (!IstGueltigesFeld(zeile, spalte))
                {
                    return -1;
                }

                return _felder[zeile, spalte];
            }
            set
            {
                if (IstGueltigesFeld(zeile, spalte))
                {
                    _felder[zeile, spalte] = value;
                }
            }
        }

        public bool CanBePaintedBy(IPaintGame painter)
        {
            return painter != null;
        }

        private static bool IstGueltigesFeld(int zeile, int spalte)
        {
            return zeile >= 0 && zeile < Gruppe2_4GWKonstanten.Zeilen
                && spalte >= 0 && spalte < Gruppe2_4GWKonstanten.Spalten;
        }
    }

    public class Gruppe2_4GWMove : I4GWMove
    {
        public Gruppe2_4GWMove(int zeile, int spalte, int spielerNummer)
        {
            Row = zeile;
            Column = spalte;
            PlayerNumber = spielerNummer;
        }
        //vllt noch ändern unnötiger Schritt in einer Sprache schreiben und bleiben 

        public int Row { get; }

        public int Column { get; }

        public int PlayerNumber { get; }
    }

    public class Gruppe2_4GWRules : IGameRules
    {
        private readonly I4GWFeld _feld = new Gruppe2_4GWFeld();

        public string Name { get { return "Gruppe 2 - Vier gewinnt Regeln"; } }

        public I4GWFeld VierGewinntFeld { get { return _feld; } }

        public IGameField CurrentField { get { return _feld; } }

        public bool MovesPossible
        {
            get
            {
                if (CheckIfPLayerWon() > 0)
                {
                    return false;
                }

                for (int spalte = 0; spalte < Gruppe2_4GWKonstanten.Spalten; spalte++)
                {
                    if (Gruppe2_4GWLogik.FindeEinwurfzeile(_feld, spalte) >= 0)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public void DoMove(IPlayMove move)
        {
            if (!(move is I4GWMove vierGewinntMove)
                || vierGewinntMove.PlayerNumber < 1
                || vierGewinntMove.PlayerNumber > 2
                || CheckIfPLayerWon() > 0)
            {
                return;
            }

            int einwurfzeile = Gruppe2_4GWLogik.FindeEinwurfzeile(_feld, vierGewinntMove.Column);
            if (einwurfzeile < 0 || vierGewinntMove.Row != einwurfzeile)
            {
                return;
            }

            _feld[einwurfzeile, vierGewinntMove.Column] = vierGewinntMove.PlayerNumber;
        }

        public void ClearField()
        {
            for (int zeile = 0; zeile < Gruppe2_4GWKonstanten.Zeilen; zeile++)
            {
                for (int spalte = 0; spalte < Gruppe2_4GWKonstanten.Spalten; spalte++)
                {
                    _feld[zeile, spalte] = 0;
                }
            }
        }

        public int CheckIfPLayerWon()
        {
            return Gruppe2_4GWLogik.FindeGewinner(_feld);
        }

        public bool IsDraw
        {
            get { return !MovesPossible && CheckIfPLayerWon() < 0; }
        }

    }

    internal static class Gruppe2_4GWLogik
    {
        private static readonly int[,] Richtungen =
        {
            { 0, 1 },
            { 1, 0 },
            { 1, 1 },
            { 1, -1 }
        };

        public static bool VerbindetVier(I4GWFeld feld, int zeile, int spalte, int spielerNummer)
        {
            for (int richtung = 0; richtung < Richtungen.GetLength(0); richtung++)
            {
                int zeilenSchritt = Richtungen[richtung, 0];
                int spaltenSchritt = Richtungen[richtung, 1];
                int anzahl = 1
                    + ZaehleRichtung(feld, zeile, spalte, zeilenSchritt, spaltenSchritt, spielerNummer)
                    + ZaehleRichtung(feld, zeile, spalte, -zeilenSchritt, -spaltenSchritt, spielerNummer);

                if (anzahl >= 4)
                {
                    return true;
                }
            }

            return false;
        }

        public static int FindeGewinner(I4GWFeld feld)
        {
            for (int zeile = 0; zeile < Gruppe2_4GWKonstanten.Zeilen; zeile++)
            {
                for (int spalte = 0; spalte < Gruppe2_4GWKonstanten.Spalten; spalte++)
                {
                    int spielerNummer = feld[zeile, spalte];
                    if (spielerNummer > 0
                        && VerbindetVier(feld, zeile, spalte, spielerNummer))
                    {
                        return spielerNummer;
                    }
                }
            }

            return -1;
        }

        public static int FindeEinwurfzeile(I4GWFeld feld, int spalte)
        {
            if (spalte < 0 || spalte >= Gruppe2_4GWKonstanten.Spalten)
            {
                return -1;
            }

            for (int zeile = Gruppe2_4GWKonstanten.Zeilen - 1; zeile >= 0; zeile--)
            {
                if (feld[zeile, spalte] == 0)
                {
                    return zeile;
                }
            }

            return -1;
        }

        private static int ZaehleRichtung(
            I4GWFeld feld,
            int zeile,
            int spalte,
            int zeilenSchritt,
            int spaltenSchritt,
            int spielerNummer)
        {
            int anzahl = 0;
            int aktuelleZeile = zeile + zeilenSchritt;
            int aktuelleSpalte = spalte + spaltenSchritt;

            while (aktuelleZeile >= 0
                && aktuelleZeile < Gruppe2_4GWKonstanten.Zeilen
                && aktuelleSpalte >= 0
                && aktuelleSpalte < Gruppe2_4GWKonstanten.Spalten
                && feld[aktuelleZeile, aktuelleSpalte] == spielerNummer)
            {
                anzahl++;
                aktuelleZeile += zeilenSchritt;
                aktuelleSpalte += spaltenSchritt;
            }

            return anzahl;
        }
    }

    public class Gruppe2_4GWPainter : IPaintGame
    {
        public string Name { get { return "Gruppe 2 - Vier gewinnt Painter"; } }

        public void PaintGameField(Canvas canvas, IGameField currentField)
        {
            if (canvas == null || !(currentField is I4GWFeld feld))
            {
                return;
            }

            canvas.Children.Clear();
            canvas.Background = Brushes.White;

            int breite = Gruppe2_4GWKonstanten.Spalten * Gruppe2_4GWKonstanten.Zellgroesse;
            int hoehe = Gruppe2_4GWKonstanten.Zeilen * Gruppe2_4GWKonstanten.Zellgroesse;
            Rectangle spielbrett = new Rectangle
            {
                Width = breite,
                Height = hoehe,
                Fill = Brushes.RoyalBlue
            };
            canvas.Children.Add(spielbrett);

            for (int zeile = 0; zeile < Gruppe2_4GWKonstanten.Zeilen; zeile++)
            {
                for (int spalte = 0; spalte < Gruppe2_4GWKonstanten.Spalten; spalte++)
                {
                    Ellipse stein = new Ellipse
                    {
                        Width = Gruppe2_4GWKonstanten.Zellgroesse - 8,
                        Height = Gruppe2_4GWKonstanten.Zellgroesse - 8,
                        Fill = FarbeFuerSpieler(feld[zeile, spalte])
                    };
                    Canvas.SetLeft(stein, spalte * Gruppe2_4GWKonstanten.Zellgroesse + 4);
                    Canvas.SetTop(stein, zeile * Gruppe2_4GWKonstanten.Zellgroesse + 4);
                    canvas.Children.Add(stein);
                }
            }

            if (IstUnentschieden(feld))
            {
                TextBlock unentschieden = new TextBlock
                {
                    Text = "Unentschieden",
                    FontSize = 22,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black,
                    Background = Brushes.White,
                    Padding = new Thickness(8)
                };
                Canvas.SetLeft(unentschieden, 75);
                Canvas.SetTop(unentschieden, 100);
                canvas.Children.Add(unentschieden);
            }
        }

        private static Brush FarbeFuerSpieler(int spielerNummer)
        {
            if (spielerNummer == 1)
            {
                return Brushes.Red;
            }

            if (spielerNummer == 2)
            {
                return Brushes.Yellow;
            }

            return Brushes.White;
        }

        private static bool IstUnentschieden(I4GWFeld feld)
        {
            if (Gruppe2_4GWLogik.FindeGewinner(feld) > 0)
            {
                return false;
            }

            for (int spalte = 0; spalte < Gruppe2_4GWKonstanten.Spalten; spalte++)
            {
                if (Gruppe2_4GWLogik.FindeEinwurfzeile(feld, spalte) >= 0)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public class Gruppe2_4GWHumanPlayer : IHumanGamePlayer
    {
        private int _spielerNummer;

        public string Name { get { return "Gruppe 2 - Vier gewinnt Mensch"; } }

        public int PlayerNumber { get { return _spielerNummer; } }

        public void SetPlayerNumber(int playerNumber)
        {
            _spielerNummer = playerNumber;
        }

        public IGamePlayer Clone()
        {
            Gruppe2_4GWHumanPlayer kopie = new Gruppe2_4GWHumanPlayer();
            kopie.SetPlayerNumber(_spielerNummer);
            return kopie;
        }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules != null && rules.CurrentField is I4GWFeld;
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (!(selection is IClickSelection klick)
                || !(field is I4GWFeld vierGewinntFeld)
                || _spielerNummer < 1
                || _spielerNummer > 2
                || klick.XClickPos < 0
                || klick.XClickPos >= Gruppe2_4GWKonstanten.Spalten * Gruppe2_4GWKonstanten.Zellgroesse
                || klick.YClickPos < 0
                || klick.YClickPos >= Gruppe2_4GWKonstanten.Zeilen * Gruppe2_4GWKonstanten.Zellgroesse)
            {
                return null;
            }

            int spalte = klick.XClickPos / Gruppe2_4GWKonstanten.Zellgroesse;
            int zeile = Gruppe2_4GWLogik.FindeEinwurfzeile(vierGewinntFeld, spalte);
            if (zeile < 0)
            {
                return null;
            }

            return new Gruppe2_4GWMove(zeile, spalte, _spielerNummer);
        }
    }

    public class Gruppe2_4GWComputerPlayer : IComputerGamePlayer
    {
        private static readonly int[] BevorzugteSpalten = { 3, 2, 4, 1, 5, 0, 6 };
        private int _spielerNummer;

        public string Name { get { return "Gruppe 2 - Vier gewinnt Computer"; } }

        public int PlayerNumber { get { return _spielerNummer; } }

        public void SetPlayerNumber(int playerNumber)
        {
            _spielerNummer = playerNumber;
        }

        public IGamePlayer Clone()
        {
            Gruppe2_4GWComputerPlayer kopie = new Gruppe2_4GWComputerPlayer();
            kopie.SetPlayerNumber(_spielerNummer);
            return kopie;
        }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules != null && rules.CurrentField is I4GWFeld;
        }

        public IPlayMove GetMove(IGameField field)
        {
            if (!(field is I4GWFeld vierGewinntFeld)
                || _spielerNummer < 1
                || _spielerNummer > 2
                || Gruppe2_4GWLogik.FindeGewinner(vierGewinntFeld) > 0)
            {
                return null;
            }

            int gegnerNummer = _spielerNummer == 1 ? 2 : 1;
            foreach (int spalte in BevorzugteSpalten)
            {
                int zeile = Gruppe2_4GWLogik.FindeEinwurfzeile(vierGewinntFeld, spalte);
                if (zeile >= 0
                    && Gruppe2_4GWLogik.VerbindetVier(
                        vierGewinntFeld, zeile, spalte, _spielerNummer))
                {
                    return new Gruppe2_4GWMove(zeile, spalte, _spielerNummer);
                }
            }

            foreach (int spalte in BevorzugteSpalten)
            {
                int zeile = Gruppe2_4GWLogik.FindeEinwurfzeile(vierGewinntFeld, spalte);
                if (zeile >= 0
                    && Gruppe2_4GWLogik.VerbindetVier(
                        vierGewinntFeld, zeile, spalte, gegnerNummer))
                {
                    return new Gruppe2_4GWMove(zeile, spalte, _spielerNummer);
                }
            }

            foreach (int spalte in BevorzugteSpalten)
            {
                int zeile = Gruppe2_4GWLogik.FindeEinwurfzeile(vierGewinntFeld, spalte);
                if (zeile >= 0)
                {
                    return new Gruppe2_4GWMove(zeile, spalte, _spielerNummer);
                }
            }

            return null;
        }
    }
}