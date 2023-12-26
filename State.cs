using Server.Shared.Info;

namespace AutoRejoinRanked
{
    public class State
    {
        private static bool isInitalized = false;
        private static GameType lastGameMode;
        public static void Init()
        {
            if (!isInitalized)
            {
                isInitalized = true;
                lastGameMode = GameType.None;
            }
        }
        public static GameType getLastGameMode() { return lastGameMode; }
        public static void setLastGameMode(GameType gameMode) { lastGameMode = gameMode; }
    }
}
