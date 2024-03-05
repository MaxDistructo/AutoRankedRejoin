using Game;
using HarmonyLib;
using Home.HomeScene;
using Home.Shared;
using Home.Shared.Enums;
using Server.Shared.Info;
using Server.Shared.Messages;
using Server.Shared.State;
using Services;
using SML;
using System.Collections;
using System.Reflection;
using UnityEngine;

using Game.Interface;
using System;

namespace AutoRequeue;

[Mod.SalemMod]
public class Main
{
    public static void Start()
    {
        System.Console.WriteLine("[AutoRequeue] Preparing to take over the WORLD!!!!!");
    }
    public static IEnumerator PostGameWaitCorouine(int delay)
    {
        yield return new WaitForSeconds(delay);
        SendToMainMenu();
    }
    public static void SendToMainMenu()
    {
        ApplicationController.ApplicationContext.pendingTransitionType = CutoutTransitionType.NONE;
        Service.Game.Network.Send((GameMessage)new RemovePlayerFromCellMessage(RemovedFromGameReason.EXIT_TO_MAIN_MENU, false));
        State.toggleModTriggered();
    }
}

public class ModInfo
{
    public const string PLUGIN_GUID = "AutoRequeue";
    public const string PLUGIN_NAME = "Auto Requeue";
    public const string PLUGIN_VERSION = "1.2.3";
}

//GameSceneController is updated every time the game changes phase.
//We detect if it's the post game by asking if the lobby is ShuttingDown (which is ONLY post game in a ranked match)
//This triggers us to do what the settings menu does to send us back to home where the other patch takes over and sets us back into a new match.
[HarmonyPatch(typeof(GameSceneController), "HandleGameInfoChanged")]
public class GameSceneControllerPatch
{
    [HarmonyPrefix]
    public static void Prefix(GameSceneController __instance, GameInfo gameInfo)
    {
        System.Console.WriteLine("[AutoRequeue] PATCH ENTRY (GameSceneController)");
        if (__instance == null)
        {
            System.Console.WriteLine("Instance is null");
            return;
        }
        //If we are in the post game of the lobby (aka it's shutting down in 60 seconds), requeue the player
        if (Service.Game.Sim.info.gameMode.Data.gameType == GameType.Ranked && Service.Game.Sim.info.lobby.Data.isShuttingDown)
        {
            //BTOS2 Compatibility, Checks if the mod is installed and if so, we check if it's the modded ranked game provided.
            if (ModStates.IsInstalled("curtis.tuba.better.tos2") && BetterTOS2.BTOSInfo.IS_MODDED)
            {
                State.setLastGameMode(GameType2.BTOS2Casual);
            }
            else 
            {
                //Vanilla Ranked
                State.setLastGameMode(Service.Game.Sim.info.gameMode.Data.gameType);
            }
            if (ModSettings.GetInt("Lobby Leave Delay", "maxdistructo.AutoRejoinRanked") > 0)
            {
                __instance.StartCoroutine(Main.PostGameWaitCorouine(ModSettings.GetInt("Lobby Leave Delay", "maxdistructo.AutoRejoinRanked")));
            }
            else
            {
                Main.SendToMainMenu();
            }

            //Once loaded into the main screen, if the last game mode was Ranked, set it there.
        }
        //In other game modes, the lobby does not end but instead restarts. We check if the restart timer is running and if so, kick the player out and cause the requeue.
        else if (ModSettings.GetBool("Use for all Game Modes", "maxdistructo.AutoRejoinRanked") && Service.Game.Sim.info.lobby.Data.restartTimer.GetWholeSecondsRemaining() > 0)
        {
            State.setLastGameMode(Service.Game.Sim.info.gameMode.Data.gameType);
            if (ModSettings.GetInt("Lobby Leave Delay", "maxdistructo.AutoRejoinRanked") > 0)
            {
                __instance.StartCoroutine(Main.PostGameWaitCorouine(ModSettings.GetInt("Lobby Leave Delay", "maxdistructo.AutoRejoinRanked")));
            }
            else
            {
                Main.SendToMainMenu();
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
        if ((State.getLastGameMode() == GameType2.Ranked || State.getLastGameMode() == GameType2.BTOS2Casual || (ModSettings.GetBool("Use for all Game Modes", "maxdistructo.AutoRejoinRanked") && State.getLastGameMode() != GameType2.None)) && State.getModState())
        {
            State.toggleModTriggered();
            if (ModStates.IsInstalled("curtis.tuba.better.tos2") && State.getLastGameMode() == GameType2.BTOS2Casual)
            {
                //START BTOS2 LOGIC FOR REJOINING CASUAL MODE
                if (BetterTOS2.AddHomeSceneButtons.joinedQueue.AddSeconds(10.0) > DateTime.Now)
                {
                    Service.Game.HudMessage.ShowMessage("You must wait 10 seconds before rejoining the Casual Mode Queue", interrupt: true, messageType: HudMessageType.Warning);
                }
                else
                {
                    if ((UnityEngine.Object)BetterTOS2.BTOSInfo.CasualModeController == (UnityEngine.Object)null)
                    {
                        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(BetterTOS2.BTOSInfo.assetBundle.LoadAsset<GameObject>("CasualModeUI"), GameObject.Find("HomeUI(Clone)/HomeScreenMainCanvas/SafeArea/").transform);
                        gameObject.transform.SetAsLastSibling();
                        gameObject.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
                        BetterTOS2.BTOSInfo.CasualModeController = gameObject.AddComponent<BetterTOS2.CasualModeMenuController>();
                    }
                    BetterTOS2.BTOSInfo.NetworkService.SendMessage((BetterTOS2.Messages.Message)new BetterTOS2.JoinCasualQueue());
                }
                //END BTOS2 LOGIC
            }
            else if (State.getLastGameMode() == GameType2.Custom || State.getLastGameMode() == GameType2.BTOS2CustomPlus)
            {
                //IGNORE
                //We can't support auto joining custom games.
            }
            else
            {
                //Use the built in methods to set us to be a ranked game then click join for the user.
                MethodInfo methodInfo = typeof(HomeSceneController).GetMethod("SetSelectedGameMode", BindingFlags.NonPublic | BindingFlags.Instance);
                var parameters = new object[] { State.convertToGameType(State.getLastGameMode()) };
                methodInfo.Invoke(__instance, parameters);
                __instance.HandleClickJoinSelectedGameMode();
                State.setLastGameMode(GameType.None);
                //Let AutoAcceptRanked take over from here in the RankedQueueController
            }
        }
    }
}