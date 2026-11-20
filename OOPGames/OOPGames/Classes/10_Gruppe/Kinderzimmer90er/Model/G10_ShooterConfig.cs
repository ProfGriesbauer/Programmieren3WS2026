using System;

namespace OOPGames
{
    internal static class G10_ShooterConfig
    {
        public const float WorldWidth = 160;
        public const float WorldHeight = 144;
        public const float FixedDeltaSeconds = 0.04f;
        public const float PlayfieldTop = 18;
        public const float HeaderHeight = 18;
        public const float PlayerStartX = 80;
        public const float PlayerStartY = 124;
        public const float PlayerMinimumX = 7;
        public const float PlayerMaximumX = 153;
        public const float PlayerMinimumY = 28;
        public const float PlayerMaximumY = 135;
        public const float PlayerSpeed = 92;
        public const float BossStartX = 80;
        public const float BossStartY = 35;
        public const int MaximumLives = 3;
        public static float LowerThirdY
        {
            get { return PlayfieldTop + (WorldHeight - PlayfieldTop) * 2 / 3; }
        }
    }
}
