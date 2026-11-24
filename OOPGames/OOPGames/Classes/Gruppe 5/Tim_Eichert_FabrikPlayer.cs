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
                    case G5_FabrikAktionstyp.GebaeudeAnsehen:
                        return G5_FabrikAktion.GebaeudeAnsehen(_playerNumber, -1, -1);
                }
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
