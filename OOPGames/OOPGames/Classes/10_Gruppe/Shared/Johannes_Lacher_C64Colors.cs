namespace OOPGames
{
    // The 16 colours of the Commodore 64 (as shown by today's emulators), plus a floor shadow.
    internal static class G10_C64Colors
    {
        public const uint Black = 0xFF000000;
        public const uint White = 0xFFFFFFFF;
        public const uint Red = 0xFF9F4E44;
        public const uint Cyan = 0xFF6ABFC6;
        public const uint Purple = 0xFFA057A3;
        public const uint Green = 0xFF5CAB5E;
        public const uint Blue = 0xFF50459B;
        public const uint Yellow = 0xFFC9D487;
        public const uint Orange = 0xFFA1683C;
        public const uint Brown = 0xFF6D5412;
        public const uint LightRed = 0xFFCB7E75;
        public const uint DarkGrey = 0xFF626262;
        public const uint Grey = 0xFF898989;
        public const uint LightGreen = 0xFF9AE29B;
        public const uint LightBlue = 0xFF887ECB;
        public const uint LightGrey = 0xFFADADAD;
        // Not a C64 colour: the shadow that blocks throw onto the floor in Bomberman.
        public const uint Shadow = 0xFF3F7A41;
    }
}
