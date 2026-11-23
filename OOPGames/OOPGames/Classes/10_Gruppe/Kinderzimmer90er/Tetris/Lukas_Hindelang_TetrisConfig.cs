using System;
using System.Collections.Generic;

namespace OOPGames
{
    internal static class G10_TetrisConfig
    {
        public const int Columns = 10;
        public const int VisibleRows = 20;
        public const int HiddenRows = 2;
        public const int Rows = VisibleRows + HiddenRows;
        public const int PreviewCount = 3;
        public const float FixedDeltaSeconds = 0.04f;
        public const float ReadySeconds = 1.4f;
        public const float AutoShiftDelay = 0.16f;
        public const float AutoShiftInterval = 0.08f;
        public const float SoftDropInterval = 0.04f;
        public const float LockDelay = 0.5f;
        public const int MaxLockResets = 15;
        public const float LineClearSeconds = 0.36f;
        public const float GameOverInputDelay = 0.8f;
        public const int LinesPerLevel = 10;

        // Game Boy Tetris: frames per row at 59.73 fps. Level 1 uses the first entry.
        static readonly int[] FramesPerRow =
            { 53, 49, 45, 41, 37, 33, 28, 22, 17, 11, 10, 9, 8, 7, 6, 6, 5, 5, 4, 4, 3 };

        public static int MaxLevel { get { return FramesPerRow.Length; } }

        public static float DropInterval(int level)
        {
            int index = Math.Max(0, Math.Min(FramesPerRow.Length - 1, level - 1));
            return FramesPerRow[index] / 59.73f;
        }
    }
}
