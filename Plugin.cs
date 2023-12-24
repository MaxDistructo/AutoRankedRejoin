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

//We need to do the following:
//Via the settings menu, go to home
//Then do the inputs to click on Ranked, Join Queue
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
                //Leave the current game to return to the lobby.
                ApplicationController.ApplicationContext.pendingTransitionType = CutoutTransitionType.NONE;
                Service.Game.Network.Send((GameMessage)new RemovePlayerFromCellMessage(RemovedFromGameReason.EXIT_TO_MAIN_MENU, false));
                //Join the ranked queue how the main menu would cause it
                Service.Home.UserService.RequestGame("", GameType.Ranked, Service.Home.PlatformService.IsOnBetaBranch());

                //Let AutoAcceptRanked take over from here in the RankedQueueController
            }
        }
        return;
    }

    
}
//We patch the HomeScene controller to not allow players to join another gamemode in ranked queue
//This is because we cannot directly patch the button to be disabled automatically so if they attempt to
//play another gamemode, disable the button.
[HarmonyPatch(typeof(HomeSceneController), "HandleClickJoinSelectedGameMode")]
public class HomeSceneControllerPatch
{
    [HarmonyPrefix]
    public static void Prefix(HomeSceneController __instance)
    {

        System.Console.WriteLine("[AutoRejoinRanked] PATCH ENTRY (HomeSceneController#HandleClickJoinSelectedGameMode)");
        if (__instance == null)
        {
            System.Console.WriteLine("Instance is null");
            return;
        }
        UnityEngine.UI.Button joinSelectedGameModeButtonRef = FieldRefAccess<HomeSceneController, UnityEngine.UI.Button>(__instance, "joinSelectedGameModeButton");
        __instance.joinSelectedGameModeButton.interactable = false;
    }
}
