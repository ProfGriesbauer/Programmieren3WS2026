namespace OOPGames
{
    public enum G10_ScheduleEvent
    {
        None,
        ActiveModeChanged,
        FrightenedEnded
    }

    // Global timetable of the ghosts: scatter and chase alternate; frightened time pauses it.
    public sealed class G10_ModeScheduler
    {
        // Frightened ghosts flash white during the last part of the frightened time.
        static readonly int FlashTicks = G10_PacManConstants.Seconds(2);

        int[] _PhaseTicks;
        int _Index;
        int _Elapsed;
        int _FrightenedTicks;

        public G10_ModeScheduler(G10_LevelSettings level)
        {
            Reset(level);
        }

        public G10_GhostModeKind ActiveMode
        {
            get { return _Index % 2 == 0 ? G10_GhostModeKind.Scatter : G10_GhostModeKind.Chase; }
        }

        public bool IsFrightened { get { return _FrightenedTicks > 0; } }
        public bool IsFlashing { get { return IsFrightened && _FrightenedTicks <= FlashTicks; } }

        public void Reset(G10_LevelSettings level)
        {
            _PhaseTicks = level.ModeTicks;
            _Index = 0;
            _Elapsed = 0;
            _FrightenedTicks = 0;
        }

        public void StartFrightened(int ticks)
        {
            _FrightenedTicks = ticks;
        }

        public G10_ScheduleEvent Tick()
        {
            if (_FrightenedTicks > 0)
            {
                _FrightenedTicks--;
                return _FrightenedTicks == 0 ? G10_ScheduleEvent.FrightenedEnded : G10_ScheduleEvent.None;
            }
            // The last entry of the timetable lasts forever.
            if (_Index >= _PhaseTicks.Length - 1) return G10_ScheduleEvent.None;
            _Elapsed++;
            if (_Elapsed < _PhaseTicks[_Index]) return G10_ScheduleEvent.None;
            _Index++;
            _Elapsed = 0;
            return G10_ScheduleEvent.ActiveModeChanged;
        }
    }
}
