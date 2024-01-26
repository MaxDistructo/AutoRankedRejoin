using Server.Shared.Info;

namespace AutoRejoinRanked
{
    public class State
    {
        private static bool isInitalized = false;
        private static GameType2 lastGameMode;
        public static void Init()
        {
            if (!isInitalized)
            {
                isInitalized = true;
                lastGameMode = GameType2.None;
            }
        }
        public static GameType2 getLastGameMode() { return lastGameMode; }
        public static void setLastGameMode(GameType2 gameMode) { lastGameMode = gameMode; }
        public static void setLastGameMode(GameType gameMode)
        {
            lastGameMode = convertToGameType2(gameMode);
        }
        public static GameType2 convertToGameType2(GameType type)
        {
            if (type <= GameType.NewPlayerClassic)
            {
                return (GameType2)type;
            }
            return GameType2.None;
        }
        public static GameType convertToGameType(GameType2 type)
        {
            if (type <= GameType2.NewPlayerClassic)
            {
                return (GameType)type;
            }
            return GameType.None;
        }
    }
    //We define our own enum to support BTOS2 Game Modes
    public enum GameType2
    {
        None,
        Classic,
        Custom,
        Tutorial,
        FourHorsemen,
        AllAny,
        DraculasPalace,
        CursedSouls,
        Party,
        RankedPractice,
        Ranked,
        TownTraitor,
        VIP,
        NoTownHanged,
        AnonymousVotes,
        NoRoleReveal,
        HiddenKillers,
        GhostTown,
        OneTrialPerDay,
        NewPlayerClassic,
        BTOS2Casual,
        BTOS2CustomPlus
    }
}
