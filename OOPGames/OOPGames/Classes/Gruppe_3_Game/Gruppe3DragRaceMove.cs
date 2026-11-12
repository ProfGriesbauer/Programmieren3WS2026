namespace OOPGames
{
    public sealed class Gruppe3DragRaceMove : IGruppe3DragRaceMove
    {
        public Gruppe3DragRaceMove(
            int playerNumber, bool accelerate, bool brake, int carIndex = -1)
        {
            PlayerNumber = playerNumber;
            Accelerate = accelerate;
            Brake = brake;
            CarIndex = carIndex;
        }

        public int PlayerNumber { get; private set; }
        public bool Accelerate { get; private set; }
        public bool Brake { get; private set; }
        public int CarIndex { get; private set; }
    }
}
