using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OOPGames
{
    public static class Gruppe2_SubwayTests
    {
        public static void RunAllTests()
        {
            TestInitialState();
            TestMovementAndLaneBounds();
            TestJumpAndRollTransitions();
            TestHumanPlayerInput();
            TestPainterFocusesGameCanvas();
            TestFatalAndNonFatalObstacles();
            TestRewardCollectionAndUpgrades();
            TestWaveSpawningKeepsOneSafeLane();
        }

        private static void TestInitialState()
        {
            Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
            rules.ClearField();
            Gruppe2_SubwayField field = (Gruppe2_SubwayField)rules.CurrentField;

            AssertTrue(rules.MovesPossible,
                "new Subway Surfer game accepts input");
            AssertTrue(field.Lane == 1,
                "starting lane is the middle lane");
            AssertTrue(field.Level == Gruppe2_SubwayLevel.Track,
                "runner starts on the track");
            AssertTrue(field.Speed == 1,
                "starting speed matches the design sheet");
            AssertTrue(field.GuardGap == 100,
                "guard starts at a full distance");
        }

        private static void TestMovementAndLaneBounds()
        {
            Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
            rules.ClearField();
            Gruppe2_SubwayField field = (Gruppe2_SubwayField)rules.CurrentField;

            rules.DoMove(new Gruppe2_SubwayMove(Key.Left, 1));
            AssertTrue(field.Lane == 0,
                "left key moves runner to the left lane");

            rules.DoMove(new Gruppe2_SubwayMove(Key.Right, 1));
            rules.DoMove(new Gruppe2_SubwayMove(Key.Right, 1));
            AssertTrue(field.Lane == 2,
                "right key moves runner to the right lane");

            rules.DoMove(new Gruppe2_SubwayMove(Key.Right, 1));
            AssertTrue(field.Lane == 2,
                "lane movement stays capped at the rightmost lane");
        }

        private static void TestJumpAndRollTransitions()
        {
            Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
            rules.ClearField();
            Gruppe2_SubwayField field = (Gruppe2_SubwayField)rules.CurrentField;

            rules.DoMove(new Gruppe2_SubwayMove(Key.Up, 1));
            AssertTrue(field.Level == Gruppe2_SubwayLevel.Air,
                "up key triggers a jump into the air state");

            rules.DoMove(new Gruppe2_SubwayMove(Key.Down, 1));
            AssertTrue(field.Level == Gruppe2_SubwayLevel.Track,
                "down key returns the runner to the track");
        }

        private static void TestHumanPlayerInput()
        {
            Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
            rules.ClearField();
            Gruppe2_SubwayField field = (Gruppe2_SubwayField)rules.CurrentField;
            Gruppe2_SubwayHumanPlayer player = new Gruppe2_SubwayHumanPlayer();
            player.SetPlayerNumber(1);

            AssertTrue(player.CanBeRuledBy(rules),
                "Subway Surfer human player is valid for the Subway Surfer rules");

            IPlayMove validMove = player.GetMove(new KeySelection(Key.Left), field);
            AssertTrue(validMove is IGruppe2_SubwayMove && ((IGruppe2_SubwayMove)validMove).Key == Key.Left,
                "valid keyboard input produces a Subway Surfer move");

            AssertTrue(player.GetMove(new KeySelection(Key.Space), field) == null,
                "unsupported keys are rejected");
        }

        private static void TestPainterFocusesGameCanvas()
        {
            Window window = new Window
            {
                Width = 320,
                Height = 260,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.ToolWindow
            };
            Canvas canvas = new Canvas();
            window.Content = canvas;

            try
            {
                window.Show();
                Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
                rules.ClearField();

                new Gruppe2_SubwayPainter().PaintGameField(canvas, rules.CurrentField);

                AssertTrue(canvas.Focusable,
                    "game canvas accepts keyboard focus");
                AssertTrue(Keyboard.FocusedElement == canvas,
                    "painting a new game focuses the canvas instead of the start button");
            }
            finally
            {
                window.Close();
            }
        }

        private static void TestFatalAndNonFatalObstacles()
        {
            Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
            Gruppe2_SubwayField field = (Gruppe2_SubwayField)rules.CurrentField;

            rules.ClearField();
            field.MutableObstacles.Add(new Gruppe2_SubwayObstacle(
                Gruppe2_SubwayObstacleKind.Curb, 1, 0.5));
            rules.TickGameCall();
            AssertTrue(field.GuardGap < 100 && field.GuardGap > 0,
                "light collision reduces the guard distance");
            AssertTrue(!field.GameOver,
                "light collision keeps the run alive");

            rules.ClearField();
            field = (Gruppe2_SubwayField)rules.CurrentField;
            field.MutableObstacles.Add(new Gruppe2_SubwayObstacle(
                Gruppe2_SubwayObstacleKind.Train, 1, 0.5));
            rules.TickGameCall();
            AssertTrue(field.GameOver,
                "fatal collision ends the run");
            AssertTrue(rules.CheckIfPLayerWon() == -1,
                "game over is not reported as a win");
            AssertTrue(!rules.MovesPossible,
                "moves are blocked once the run ends");
        }

        private static void TestRewardCollectionAndUpgrades()
        {
            Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
            Gruppe2_SubwayField field = (Gruppe2_SubwayField)rules.CurrentField;

            rules.ClearField();
            field.MutableRewards.Add(new Gruppe2_SubwayReward(
                Gruppe2_SubwayRewardKind.Coin, 1, 0.5));
            rules.TickGameCall();
            AssertTrue(field.Coins == 1,
                "coin in the current lane is collected");

            rules.ClearField();
            field = (Gruppe2_SubwayField)rules.CurrentField;
            field.MutableRewards.Add(new Gruppe2_SubwayReward(
                Gruppe2_SubwayRewardKind.UpgradeBox, 1, 0.5,
                Gruppe2_SubwayUpgradeKind.Magnet));
            rules.TickGameCall();
            AssertTrue(field.ActiveUpgrades.ContainsKey(Gruppe2_SubwayUpgradeKind.Magnet),
                "upgrade box grants the expected upgrade");
        }

        private static void TestWaveSpawningKeepsOneSafeLane()
        {
            Gruppe2_SubwayRules rules = new Gruppe2_SubwayRules();
            Gruppe2_SubwayField field = (Gruppe2_SubwayField)rules.CurrentField;

            rules.ClearField();
            for (int tick = 0; tick < 90; tick++)
            {
                rules.TickGameCall();
                if (field.MutableObstacles.Count > 0)
                {
                    break;
                }
            }

            AssertTrue(field.MutableObstacles.Count > 0,
                "waves spawn after the configured distance threshold");
            AssertTrue(field.MutableObstacles.Select(item => item.Lane).Distinct().Count() < 3,
                "spawn logic always leaves at least one lane open");
        }

        private static void AssertTrue(bool condition, string description)
        {
            if (!condition)
            {
                throw new InvalidOperationException("Assertion failed: " + description);
            }
        }
    }
}
