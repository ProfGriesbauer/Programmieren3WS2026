using System.Windows.Input;

namespace OOPGames
{
    public sealed class Gruppe2_TrainRunMove : IGruppe2_TrainRunMove
    {
        public Gruppe2_TrainRunMove(Key key, int playerNumber)
        {
            Key = key;
            PlayerNumber = playerNumber;
        }

        public Key Key { get; private set; }
        public int PlayerNumber { get; private set; }
    }

    public sealed class Gruppe2_TrainRunHumanPlayer : IHumanGamePlayer
    {
        public string Name { get { return "Gruppe 2 - Train Run Laeufer"; } }
        public int PlayerNumber { get; private set; }

        public void SetPlayerNumber(int playerNumber)
        {
            PlayerNumber = playerNumber;
        }

        public bool CanBeRuledBy(IGameRules rules)
        {
            return rules is Gruppe2_TrainRunRules;
        }

        public IGamePlayer Clone()
        {
            return new Gruppe2_TrainRunHumanPlayer();
        }

        public IPlayMove GetMove(IMoveSelection selection, IGameField field)
        {
            IGruppe2_TrainRunField trainRunField = field as IGruppe2_TrainRunField;
            IKeySelection keySelection = selection as IKeySelection;
            if (trainRunField == null || trainRunField.GameOver || keySelection == null)
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
                    return new Gruppe2_TrainRunMove(keySelection.Key, PlayerNumber);
                default:
                    return null;
            }
        }
    }
}
