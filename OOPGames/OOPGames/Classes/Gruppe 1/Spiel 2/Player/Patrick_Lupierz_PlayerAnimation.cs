namespace OOPGames
{
    /// <summary>
    /// Bestimmt die passende Sprite-ID für Spielfigur und NPCs.
    /// Jede Laufrichtung hat eigene Grafiken; beim Laufen wechseln sich
    /// Frame 1 und Frame 2 (linker/rechter Schritt) ab.
    /// </summary>
    public class G1C_PlayerAnimation
    {
        public string GetSpriteId(G1C_Player player)
        {
            int frame = 0;
            if (player.IsMoving && player.StepProgress < (player.StepTicks + 1) / 2)
            {
                frame = player.StepCount % 2 == 0 ? 1 : 2;
            }
            return BuildSpriteId(player.Skin, player.Facing, frame);
        }

        public static string GetStandingSpriteId(string skin, G1C_Direction facing)
        {
            return BuildSpriteId(skin, facing, 0);
        }

        private static string BuildSpriteId(string skin, G1C_Direction facing, int frame)
        {
            return skin + "_" + G1C_DirectionHelper.ToSpriteName(facing) + "_" + frame;
        }
    }
}
