using System.Collections.Generic;
using System.Windows.Input;

namespace OOPGames
{
    internal sealed class G10_ShooterInputMapper
    {
        public G10_ShooterInputMapper(G10_InputLatch horizontalLatch,
            G10_InputLatch verticalLatch,
            G10_InputLatch fireLatch)
        {
            HorizontalLatch = horizontalLatch;
            VerticalLatch = verticalLatch;
            FireLatch = fireLatch;
        }

        G10_InputLatch HorizontalLatch { get; set; }
        G10_InputLatch VerticalLatch { get; set; }
        G10_InputLatch FireLatch { get; set; }

        public bool Map(IReadOnlyCollection<Key> keys,
            out G10_SolarShooterInput input)
        {
            bool restart = false;
            int left = 0;
            int right = 0;
            int up = 0;
            int down = 0;

            foreach (Key key in keys)
            {
                if (key == Key.Left || key == Key.A) left++;
                if (key == Key.Right || key == Key.D) right++;
                if (key == Key.Up || key == Key.W) up++;
                if (key == Key.Down || key == Key.S) down++;
                if (key == Key.Z || key == Key.Space) FireLatch.Latch();
                if (key == Key.Enter) restart = true;
            }

            if (left > 0) HorizontalLatch.Latch(-1);
            if (right > 0) HorizontalLatch.Latch(1);
            if (up > 0) VerticalLatch.Latch(-1);
            if (down > 0) VerticalLatch.Latch(1);

            HorizontalLatch.Update(G10_ShooterConfig.FixedDeltaSeconds);
            VerticalLatch.Update(G10_ShooterConfig.FixedDeltaSeconds);
            FireLatch.Update(G10_ShooterConfig.FixedDeltaSeconds);

            if (left > 0 && right > 0) HorizontalLatch.Clear();
            if (up > 0 && down > 0) VerticalLatch.Clear();

            input = new G10_SolarShooterInput(
                HorizontalLatch.Value,
                VerticalLatch.Value,
                FireLatch.IsActive);
            return restart;
        }

        public void Reset()
        {
            HorizontalLatch.Reset();
            VerticalLatch.Reset();
            FireLatch.Reset();
        }
    }
}
