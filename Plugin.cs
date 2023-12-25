using SML;
using Game;
using Server.Shared.Info;
using Services;
using HarmonyLib;
using Home.Shared.Enums;
using Home.Shared;
using Server.Shared.Messages;
using Server.Shared.State;
using Home.HomeScene;
using System.Reflection;
using Mono.Cecil;
using static HarmonyLib.AccessTools;
using System.Collections.Generic;

namespace AutoRejoinRanked;

[Mod.SalemMod]
public class Main
{

    
    public static void Start() 
    {
        System.Console.WriteLine("[AutoRejoinRanked] Preparing to take over the WORLD!!!!!");
    }
}

public class ModInfo 
{
    public const string PLUGIN_GUID = "AutoRejoinRanked";
    public const string PLUGIN_NAME = "Auto Rejoin Ranked";
    public const string PLUGIN_VERSION = "1.0.0";
}

//GameSceneController is updated every time the game changes phase.
//We detect if it's the post game by asking if the lobby is ShuttingDown (which is ONLY post game in a ranked match)
//This triggers us to do what the settings menu does to send us back to home where the other patch takes over and sets us back into a new match.
[HarmonyPatch(typeof (GameSceneController), "HandleGameInfoChanged")]
public class GameSceneControllerPatch 
{
    [HarmonyPrefix]
    public static void Prefix(GameSceneController __instance, GameInfo gameInfo)
    {
        System.Console.WriteLine("[AutoRejoinRanked] PATCH ENTRY (GameSceneController)");
        if (__instance == null)
        {
            System.Console.WriteLine("Instance is null");
            return;
        }
        if (Service.Game.Sim.info.gameMode.Data.gameType == GameType.Ranked)
        {
            //If we are in the post game of the lobby (aka it's shutting down in 60 seconds), requeue the player
            if (Service.Game.Sim.info.lobby.Data.isShuttingDown)
            {
                State.setLastGameMode(Service.Game.Sim.info.gameMode.Data.gameType);
                //Leave the current game to return to the lobby.
                ApplicationController.ApplicationContext.pendingTransitionType = CutoutTransitionType.NONE;
                Service.Game.Network.Send((GameMessage)new RemovePlayerFromCellMessage(RemovedFromGameReason.EXIT_TO_MAIN_MENU, false));
                //Once loaded into the main screen, if the last game mode was Ranked, set it there.
            }
        }
        return;
    }

    
}
//We patch the HomeScene controller to setup our data storage state and joining a gamemode once we are sent back here by the GameScene controller patch
[HarmonyPatch(typeof(HomeSceneController), "Start")]
public class HomeSceneControllerStartPatch
{
    [HarmonyPrefix]
    public static void Postfix(HomeSceneController __instance) 
    {
        State.Init();
        if(State.getLastGameMode() == GameType.Ranked)
        {
            //Use the built in methods to set us to be a ranked game then click join for the user.
            MethodInfo methodInfo = typeof(HomeSceneController).GetMethod("SetSelectedGameMode", BindingFlags.NonPublic | BindingFlags.Instance);
            var parameters = new object[] { GameType.Ranked };
            methodInfo.Invoke(__instance, parameters);
            __instance.HandleClickJoinSelectedGameMode();
            State.setLastGameMode(GameType.None);
            //Let AutoAcceptRanked take over from here in the RankedQueueController
        }
    }
}