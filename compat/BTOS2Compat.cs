using Game.Interface;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AutoRequeue.compat
{
    class BTOS2Compat : IBTOS2Compat
    {
        public bool CheckEnabled()
        {
            return true;
        }

        public bool CheckModdedGamemode()
        {
            return BetterTOS2.BTOSInfo.IS_MODDED;
        }

        public void JoinCasualMode()
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
                BetterTOS2.BTOSInfo.NetworkService.SendMessage(new BetterTOS2.Messages.JoinCasualQueue());
            }
            //END BTOS2 LOGIC
        }
    }
}
