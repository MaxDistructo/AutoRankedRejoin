using AutoRequeue.compat;
using Server.Shared.Info;
using SML;
using System;

namespace AutoRequeue
{
    public class State
    {
        private static bool isInitalized = false;
        private static GameType2 lastGameMode;
        private static bool modWasTriggered = false;
        private static IBTOS2Compat btosCompat = null;
        public static void Init()
        {
            Console.WriteLine("[AutoRequeue] Initializing State...");
            if (!isInitalized)
            {
                isInitalized = true;
                lastGameMode = GameType2.None;
            }
            //Force initialization of this
            GetBTOS2Compat();
            Console.WriteLine("[AutoRequeue] State Initialized!");
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
        public static bool getModState()
        {
            return modWasTriggered;
        }
        public static void toggleModTriggered()
        {
            modWasTriggered = !modWasTriggered;
        }
        // Due to stricter requirements, we fully abstract ALL BTOS methods into a BTOS2Compat object depending on whether BTOS2 is installed.
        public static IBTOS2Compat GetBTOS2Compat()
        {
            if (btosCompat == null)
            {
                if (ModStates.IsInstalled("curtis.tuba.better.tos2"))
                {
                    Console.WriteLine("[AutoRequeue] BTOS2 is installed. Enabling compatability.");
                    btosCompat = new BTOS2Compat();
                }
                else
                {
                    Console.WriteLine("[AutoRequeue] BTOS2 is not installed.");
                    btosCompat = new NonBTOS2Compat();
                }
            }
            return btosCompat;
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
