using System.Windows.Input;

namespace OOPGames
{
    public sealed class Gruppe2_SubwayMove : IGruppe2_SubwayMove
    {
        public Gruppe2_SubwayMove(Key key, int playerNumber)
        {
            Key = key;
            PlayerNumber = playerNumber;
        }

        public Key Key { get; private set; }
        public int PlayerNumber { get; private set; }
    }

    public sealed class Gruppe2_SubwayHumanPlayer : IHumanGamePlayer
    {
        public string Name { get { return "Gruppe 2 - Subway Surfer Laeufer"; } }
        public int PlayerNumber { get; private set; }

        public void SetPlayerNumber(int playerNumber)
        {
            PlayerNumber = playerNumber;
        }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules is Gruppe2_SubwayRules;
        }

        public IGamePlayer Clone()
        {
            return new Gruppe2_SubwayHumanPlayer();
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            IGruppe2_SubwayField subwayField = field as IGruppe2_SubwayField;
            IKeySelection keySelection = selection as IKeySelection;
            if (subwayField == null || subwayField.GameOver || keySelection == null)
            {
                return null;
            }

            switch (keySelection.Key)
            {
                case Key.Left:
                case Key.Right:
                case Key.Up:
                case Key.Down:
                case Key.A:
                case Key.D:
                case Key.W:
                case Key.S:
                    return new Gruppe2_SubwayMove(keySelection.Key, PlayerNumber);
                default:
                    return null;
            }
        }
    }
}
