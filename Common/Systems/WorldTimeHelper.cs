using Terraria;

namespace VerminLordMod.Common.Systems
{
    public static class WorldTimeHelper
    {
        public const int TICKS_PER_DAY = 36000;

        public static int CurrentDay => (int)(Main.GameUpdateCount / TICKS_PER_DAY);

        public static bool IsNewDay(ref int lastDay)
        {
            int current = CurrentDay;
            if (current > lastDay)
            {
                lastDay = current;
                return true;
            }
            return false;
        }

        public static float DayProgress => (Main.GameUpdateCount % TICKS_PER_DAY) / (float)TICKS_PER_DAY;

        public static int DaysSince(int startDay) => CurrentDay - startDay;
    }
}
