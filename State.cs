using Server.Shared.Info;
using System.Collections.Generic;

namespace AutoRejoinRanked
{
    public class State
    {
        private static bool isInitalized = false;
        private static Dictionary<string, string> dict;
        private static GameType lastGameMode;
        public static void Init()
        {
            if (!isInitalized)
            {
                isInitalized = true;
                dict = new Dictionary<string, string>();
                lastGameMode = GameType.None;
            }
        }
        public static string getObj(string key){return dict[key];}
        public static void setObj(string key, string value) { dict[key] = value;}

        public static GameType getLastGameMode() { return lastGameMode;}
        public static void setLastGameMode(GameType gameMode) { lastGameMode = gameMode; }
    }
}
