namespace OOPGames
{
    internal sealed class G10_RisingEdge
    {
        bool _WasDown;

        public bool Update(bool isDown)
        {
            bool pressed = isDown && !_WasDown;
            _WasDown = isDown;
            return pressed;
        }

        public void Reset(bool isDown)
        {
            _WasDown = isDown;
        }
    }
}
