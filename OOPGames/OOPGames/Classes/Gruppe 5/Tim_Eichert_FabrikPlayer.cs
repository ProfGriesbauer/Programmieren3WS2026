using OOPGames;
using System.Windows.Input;

namespace OOPGames
{
    public sealed class G5_FabrikHumanPlayer : IHumanGamePlayer
    {
        private int _playerNumber;

        public string Name => "G5 Fabrik-Spieler";
        public int PlayerNumber => _playerNumber;

        public void SetPlayerNumber(int playerNumber)
        {
            _playerNumber = playerNumber;
        }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules is IGameRules2 && rules.CurrentField is IG5_FabrikSpielInfo;
        }

        public IGamePlayer Clone()
        {
            return new G5_FabrikHumanPlayer();
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            if (!(field is IG5_FabrikSpielInfo))
            {
                return null;
            }

            if (selection is IKeySelection keySelection)
            {
                if (field is SatisfactoryFieldView inventarAnsicht)
                {
                    bool inventarGegenstandAusgewaehlt =
                        inventarAnsicht.InventarGeoeffnet &&
                        inventarAnsicht.AusgewaehlterAbwurfGegenstand.HasValue;

                    // H stellt die eingegebene Anzahl des ausgewählten Gegenstands her.
                    if (inventarGegenstandAusgewaehlt && keySelection.Key == Key.H)
                    {
                        return G5_FabrikAktion.ManuellHerstellen(
                            _playerNumber,
                            inventarAnsicht.AusgewaehlterAbwurfGegenstand.Value.ToString());
                    }
                    G5_FabrikGebaeudeTyp? gebaeudeTyp =
                        inventarAnsicht.AusgewaehltesGebaeude?.Typ;
                    bool ausgabeGebaeudeAusgewaehlt =
                        gebaeudeTyp == G5_FabrikGebaeudeTyp.Foerderer ||
                        gebaeudeTyp == G5_FabrikGebaeudeTyp.Schmelze ||
                        gebaeudeTyp == G5_FabrikGebaeudeTyp.Konstruktor ||
                        gebaeudeTyp == G5_FabrikGebaeudeTyp.Assembler;
                    if (inventarGegenstandAusgewaehlt || ausgabeGebaeudeAusgewaehlt)
                    {
                        if (TryGetDigit(keySelection.Key, out char digit))
                        {
                            return G5_FabrikAktion.AbwurfMengeEingeben(_playerNumber, digit.ToString());
                        }

                        if (keySelection.Key == Key.Back)
                        {
                            return G5_FabrikAktion.AbwurfMengeEingeben(_playerNumber, "\b");
                        }

                        if (keySelection.Key == Key.Enter)
                        {
                            return inventarGegenstandAusgewaehlt
                                ? G5_FabrikAktion.InventarGegenstandAblegen(_playerNumber)
                                : G5_FabrikAktion.GegenstandAbholen(
                                    _playerNumber,
                                    inventarAnsicht.MengeEingabe);
                        }
                    }
                }

                switch (keySelection.Key)
                {
                    case Key.W:
                        return G5_FabrikAktion.SpielerBewegen(_playerNumber, 0, -1);
                    case Key.A:
                        return G5_FabrikAktion.SpielerBewegen(_playerNumber, -1, 0);
                    case Key.S:
                        return G5_FabrikAktion.SpielerBewegen(_playerNumber, 0, 1);
                    case Key.D:
                        return G5_FabrikAktion.SpielerBewegen(_playerNumber, 1, 0);
                    case Key.E:
                        return G5_FabrikAktion.RohstoffAbbauen(_playerNumber);
                    case Key.Delete:
                        return G5_FabrikAktion.GebaeudeVerkaufen(_playerNumber);
                    case Key.Tab:
                        return G5_FabrikAktion.BaumenueUmschalten(_playerNumber);
                    case Key.F2:
                        return G5_FabrikAktion.StatistikUmschalten(_playerNumber);
                    case Key.T:
                        return G5_FabrikAktion.TutorialUeberspringen(_playerNumber);
                    case Key.Escape:
                        return G5_FabrikAktion.GebaeudeAnsehen(_playerNumber, -1, -1);
                    default:
                        return null;
                }
            }

            if (!(selection is IClickSelection clickSelection))
            {
                return null;
            }

            if (field is SatisfactoryFieldView startView && !startView.IsGameStarted)
            {
                return G5_FabrikAktion.GebaeudePlatzieren(
                    _playerNumber,
                    clickSelection.XClickPos,
                    clickSelection.YClickPos);
            }

            if (field is SatisfactoryFieldView view &&
                view.TryGetMenuAktion(
                    clickSelection.XClickPos,
                    clickSelection.YClickPos,
                    out G5_FabrikAktionstyp aktionstyp,
                    out G5_Bauwerkzeug werkzeug,
                    out string wert))
            {
                switch (aktionstyp)
                {
                    case G5_FabrikAktionstyp.BaumenueUmschalten:
                        return G5_FabrikAktion.BaumenueUmschalten(_playerNumber);
                    case G5_FabrikAktionstyp.WerkzeugWaehlen:
                        return G5_FabrikAktion.WerkzeugWaehlen(_playerNumber, werkzeug);
                    case G5_FabrikAktionstyp.GebaeudeBauen:
                        return G5_FabrikAktion.GebaeudeBauen(_playerNumber, werkzeug);
                    case G5_FabrikAktionstyp.RezeptWaehlen:
                        return G5_FabrikAktion.RezeptWaehlen(_playerNumber, wert);
                    case G5_FabrikAktionstyp.GebaeudeUpgraden:
                        return G5_FabrikAktion.GebaeudeUpgraden(_playerNumber);
                    case G5_FabrikAktionstyp.GebaeudeVerkaufen:
                        return G5_FabrikAktion.GebaeudeVerkaufen(_playerNumber);
                    case G5_FabrikAktionstyp.RaketeBetanken:
                        return G5_FabrikAktion.RaketeBetanken(_playerNumber);
                    case G5_FabrikAktionstyp.RaketeStarten:
                        return G5_FabrikAktion.RaketeStarten(_playerNumber);
                    case G5_FabrikAktionstyp.StatistikUmschalten:
                        return G5_FabrikAktion.StatistikUmschalten(_playerNumber);
                    case G5_FabrikAktionstyp.GegenstandAbholen:
                        return G5_FabrikAktion.GegenstandAbholen(_playerNumber, wert);
                    case G5_FabrikAktionstyp.ManuellHerstellen:
                        return G5_FabrikAktion.ManuellHerstellen(_playerNumber, wert);
                    case G5_FabrikAktionstyp.InventarGegenstandWaehlen:
                        return G5_FabrikAktion.InventarGegenstandWaehlen(_playerNumber, wert);
                    case G5_FabrikAktionstyp.InventarGegenstandAblegen:
                        return G5_FabrikAktion.InventarGegenstandAblegen(_playerNumber);
                    case G5_FabrikAktionstyp.BodenGegenstaendeAufnehmen:
                        return G5_FabrikAktion.BodenGegenstaendeAufnehmen(_playerNumber);
                    case G5_FabrikAktionstyp.GebaeudeAnsehen:
                        return G5_FabrikAktion.GebaeudeAnsehen(_playerNumber, -1, -1);
                }
            }

            if (field is SatisfactoryFieldView menuView &&
                (menuView.Spielfeld.StatistikGeoeffnet ||
                 menuView.IsMenueBereich(clickSelection.XClickPos, clickSelection.YClickPos) ||
                 menuView.IsInventarBereich(clickSelection.XClickPos, clickSelection.YClickPos)))
            {
                return null;
            }

            if (field is IG5_WeltSpielfeld welt &&
                welt.TryBildschirmZuRaster(
                    clickSelection.XClickPos,
                    clickSelection.YClickPos,
                    out int x,
                    out int y))
            {
                return G5_FabrikAktion.GebaeudeAnsehen(_playerNumber, x, y);
            }

            return null;
        }

        private static bool TryGetDigit(Key key, out char digit)
        {
            switch (key)
            {
                case Key.D0:
                case Key.NumPad0: digit = '0'; return true;
                case Key.D1:
                case Key.NumPad1: digit = '1'; return true;
                case Key.D2:
                case Key.NumPad2: digit = '2'; return true;
                case Key.D3:
                case Key.NumPad3: digit = '3'; return true;
                case Key.D4:
                case Key.NumPad4: digit = '4'; return true;
                case Key.D5:
                case Key.NumPad5: digit = '5'; return true;
                case Key.D6:
                case Key.NumPad6: digit = '6'; return true;
                case Key.D7:
                case Key.NumPad7: digit = '7'; return true;
                case Key.D8:
                case Key.NumPad8: digit = '8'; return true;
                case Key.D9:
                case Key.NumPad9: digit = '9'; return true;
                default:
                    digit = default;
                    return false;
            }
        }
    }

    internal sealed class G5_FabrikAktion : IG5_FabrikAktion
    {
        private G5_FabrikAktion(
            int playerNumber,
            G5_FabrikAktionstyp aktionstyp,
            G5_Bauwerkzeug werkzeug,
            int x,
            int y,
            string wert = null)
        {
            PlayerNumber = playerNumber;
            Aktionstyp = aktionstyp;
            Werkzeug = werkzeug;
            X = x;
            Y = y;
            Wert = wert ?? string.Empty;
        }

        public int PlayerNumber { get; }
        public G5_FabrikAktionstyp Aktionstyp { get; }
        public G5_Bauwerkzeug Werkzeug { get; }
        public int X { get; }
        public int Y { get; }
        internal string Wert { get; }

        internal static G5_FabrikAktion SpielerBewegen(int playerNumber, int dx, int dy)
        {
            return new G5_FabrikAktion(playerNumber, G5_FabrikAktionstyp.SpielerBewegen, G5_Bauwerkzeug.Keines, dx, dy);
        }

        internal static G5_FabrikAktion RohstoffAbbauen(int playerNumber)
        {
            return new G5_FabrikAktion(playerNumber, G5_FabrikAktionstyp.RohstoffAbbauen, G5_Bauwerkzeug.Keines, -1, -1);
        }

        internal static G5_FabrikAktion GebaeudeAnsehen(int playerNumber, int x, int y)
        {
            return new G5_FabrikAktion(playerNumber, G5_FabrikAktionstyp.GebaeudeAnsehen, G5_Bauwerkzeug.Keines, x, y);
        }

        internal static G5_FabrikAktion GebaeudeBauen(int playerNumber, G5_Bauwerkzeug werkzeug)
        {
            return new G5_FabrikAktion(playerNumber, G5_FabrikAktionstyp.GebaeudeBauen, werkzeug, -1, -1);
        }

        internal static G5_FabrikAktion RezeptWaehlen(int playerNumber, string rezept)
        {
            return new G5_FabrikAktion(playerNumber, G5_FabrikAktionstyp.RezeptWaehlen, G5_Bauwerkzeug.Keines, -1, -1, rezept);
        }

        internal static G5_FabrikAktion GebaeudeUpgraden(int playerNumber)
        {
            return new G5_FabrikAktion(playerNumber, G5_FabrikAktionstyp.GebaeudeUpgraden, G5_Bauwerkzeug.Keines, -1, -1);
        }

        internal static G5_FabrikAktion GebaeudeVerkaufen(int playerNumber)
        {
            return new G5_FabrikAktion(playerNumber, G5_FabrikAktionstyp.GebaeudeVerkaufen, G5_Bauwerkzeug.Keines, -1, -1);
        }

        internal static G5_FabrikAktion RaketeBetanken(int playerNumber)
        {
            return new G5_FabrikAktion(playerNumber, G5_FabrikAktionstyp.RaketeBetanken, G5_Bauwerkzeug.Keines, -1, -1);
        }

        internal static G5_FabrikAktion RaketeStarten(int playerNumber)
        {
            return new G5_FabrikAktion(playerNumber, G5_FabrikAktionstyp.RaketeStarten, G5_Bauwerkzeug.Keines, -1, -1);
        }

        internal static G5_FabrikAktion StatistikUmschalten(int playerNumber)
        {
            return new G5_FabrikAktion(playerNumber, G5_FabrikAktionstyp.StatistikUmschalten, G5_Bauwerkzeug.Keines, -1, -1);
        }

        internal static G5_FabrikAktion GegenstandAbholen(int playerNumber, string anzahl)
        {
            return new G5_FabrikAktion(
                playerNumber,
                G5_FabrikAktionstyp.GegenstandAbholen,
                G5_Bauwerkzeug.Keines,
                -1,
                -1,
                anzahl);
        }

        internal static G5_FabrikAktion ManuellHerstellen(int playerNumber, string itemName)
        {
            return new G5_FabrikAktion(playerNumber, G5_FabrikAktionstyp.ManuellHerstellen, G5_Bauwerkzeug.Keines, -1, -1, itemName);
        }

        internal static G5_FabrikAktion InventarGegenstandWaehlen(int playerNumber, string itemName)
        {
            return new G5_FabrikAktion(
                playerNumber,
                G5_FabrikAktionstyp.InventarGegenstandWaehlen,
                G5_Bauwerkzeug.Keines,
                -1,
                -1,
                itemName);
        }

        internal static G5_FabrikAktion AbwurfMengeEingeben(int playerNumber, string eingabe)
        {
            return new G5_FabrikAktion(
                playerNumber,
                G5_FabrikAktionstyp.AbwurfMengeEingeben,
                G5_Bauwerkzeug.Keines,
                -1,
                -1,
                eingabe);
        }

        internal static G5_FabrikAktion InventarGegenstandAblegen(int playerNumber)
        {
            return new G5_FabrikAktion(
                playerNumber,
                G5_FabrikAktionstyp.InventarGegenstandAblegen,
                G5_Bauwerkzeug.Keines,
                -1,
                -1);
        }

        internal static G5_FabrikAktion BodenGegenstaendeAufnehmen(int playerNumber)
        {
            return new G5_FabrikAktion(
                playerNumber,
                G5_FabrikAktionstyp.BodenGegenstaendeAufnehmen,
                G5_Bauwerkzeug.Keines,
                -1,
                -1);
        }

        internal static G5_FabrikAktion BaumenueUmschalten(int playerNumber)
        {
            return new G5_FabrikAktion(playerNumber, G5_FabrikAktionstyp.BaumenueUmschalten, G5_Bauwerkzeug.Keines, -1, -1);
        }

        internal static G5_FabrikAktion WerkzeugWaehlen(int playerNumber, G5_Bauwerkzeug werkzeug)
        {
            return new G5_FabrikAktion(playerNumber, G5_FabrikAktionstyp.WerkzeugWaehlen, werkzeug, -1, -1);
        }

        internal static G5_FabrikAktion GebaeudePlatzieren(int playerNumber, int x, int y)
        {
            return new G5_FabrikAktion(playerNumber, G5_FabrikAktionstyp.GebaeudePlatzieren, G5_Bauwerkzeug.Keines, x, y);
        }

        internal static G5_FabrikAktion TutorialUeberspringen(int playerNumber)
        {
            return new G5_FabrikAktion(playerNumber, G5_FabrikAktionstyp.TutorialUeberspringen, G5_Bauwerkzeug.Keines, -1, -1);
        }

        internal static G5_FabrikAktion GebaeuderegisterUmschalten(int playerNumber)
        {
            return new G5_FabrikAktion(playerNumber, G5_FabrikAktionstyp.GebaeuderegisterUmschalten, G5_Bauwerkzeug.Keines, -1, -1);
        }

        internal static G5_FabrikAktion GebaeuderegisterScrollen(int playerNumber, int schritte)
        {
            return new G5_FabrikAktion(playerNumber, G5_FabrikAktionstyp.GebaeuderegisterScrollen, G5_Bauwerkzeug.Keines, schritte, -1);
        }
    }
}
