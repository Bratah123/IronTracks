using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Harmony;
using HarmonyLib;
using MelonLoader;

namespace PTCGLDeckTracker
{
    public class IronTracks : MelonMod
    {
        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                LoggerInstance.Msg("User Clicked F");
            }
        }

        public override void OnGUI()
        {
            var width = 250;
            var height = 300;
            GUI.Box(new Rect(Screen.width - width, 0, width, height), "Deck");
        }

        [HarmonyLib.HarmonyPatch(typeof(MatchManager), "SendMatchStartTelemetry")]
        class Patch
        {
            static void Prefix(MatchManager __instance, NetworkMatchController.MatchDetails game)
            {
                var playerOneName = game.players[0].playerName;
                var playerTwoName = game.players[1].playerName;
                Melon<IronTracks>.Logger.Msg(playerOneName + " vs. " + playerTwoName);
            }
        }
    }

}
