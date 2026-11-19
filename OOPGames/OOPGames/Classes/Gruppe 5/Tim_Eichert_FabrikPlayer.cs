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
            if (!(field is IG5_FabrikSpielInfo fabrikInfo))
            {
                return null;
            }

            if (selection is IKeySelection keySelection)
            {
                if (field is SatisfactoryFieldView)
                {
                    if (keySelection.Key == Key.B)
                    {
                        return G5_FabrikAktion.WerkzeugWaehlen(
                            _playerNumber,
                            G5_Bauwerkzeug.Foerderband);
                    }

                    if (keySelection.Key == Key.M)
                    {
                        return G5_FabrikAktion.WerkzeugWaehlen(
                            _playerNumber,
                            G5_Bauwerkzeug.GebaeudeVerschieben);
                    }
                }

                if (keySelection.Key == Key.F)
                {
                    return G5_FabrikAktion.WerkzeugWaehlen(
                        _playerNumber,
                        G5_Bauwerkzeug.Foerderer);
                }

                if (keySelection.Key == Key.S)
                {
                    return G5_FabrikAktion.WerkzeugWaehlen(
                        _playerNumber,
                        G5_Bauwerkzeug.Schmelze);
                }

                if (keySelection.Key == Key.K)
                {
                    return G5_FabrikAktion.WerkzeugWaehlen(
                        _playerNumber,
                        G5_Bauwerkzeug.Kupferschmelze);
                }

                if (keySelection.Key == Key.C)
                {
                    return G5_FabrikAktion.WerkzeugWaehlen(
                        _playerNumber,
                        G5_Bauwerkzeug.Konstruktor);
                }

                if (keySelection.Key == Key.B)
                {
                    return G5_FabrikAktion.WerkzeugWaehlen(
                        _playerNumber,
                        G5_Bauwerkzeug.Foerderband);
                }

                if (keySelection.Key == Key.Escape)
                {
                    return G5_FabrikAktion.WerkzeugWaehlen(
                        _playerNumber,
                        G5_Bauwerkzeug.Keines);
                }

                return null;
            }

            if (selection is IClickSelection clickSelection &&
                ((IG5_WeltSpielfeld)fabrikInfo).TryBildschirmZuRaster(
                    clickSelection.XClickPos,
                    clickSelection.YClickPos,
                    out int x,
                    out int y))
            {
                return G5_FabrikAktion.GebaeudePlatzieren(
                    _playerNumber,
                    x,
                    y,
                    clickSelection.XClickPos,
                    clickSelection.YClickPos);
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
            int bildschirmX,
            int bildschirmY)
        {
            PlayerNumber = playerNumber;
            Aktionstyp = aktionstyp;
            Werkzeug = werkzeug;
            X = x;
            Y = y;
            BildschirmX = bildschirmX;
            BildschirmY = bildschirmY;
        }

        public int PlayerNumber { get; }
        public G5_FabrikAktionstyp Aktionstyp { get; }
        public G5_Bauwerkzeug Werkzeug { get; }
        public int X { get; }
        public int Y { get; }
        public int BildschirmX { get; }
        public int BildschirmY { get; }

        internal static G5_FabrikAktion WerkzeugWaehlen(int playerNumber, G5_Bauwerkzeug werkzeug)
        {
            return new G5_FabrikAktion(
                playerNumber,
                G5_FabrikAktionstyp.WerkzeugWaehlen,
                werkzeug,
                -1,
                -1,
                -1,
                -1);
        }

        internal static G5_FabrikAktion GebaeudePlatzieren(
            int playerNumber,
            int x,
            int y,
            int bildschirmX,
            int bildschirmY)
        {
            return new G5_FabrikAktion(
                playerNumber,
                G5_FabrikAktionstyp.GebaeudePlatzieren,
                G5_Bauwerkzeug.Keines,
                x,
                y,
                bildschirmX,
                bildschirmY);
        }
    }
}
