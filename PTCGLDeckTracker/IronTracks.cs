using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Harmony;
using HarmonyLib;
using MelonLoader;
using RainierClientSDK;
using UnityEngine.SceneManagement;
using PTCGLDeckTracker.CardCollection;

namespace PTCGLDeckTracker
{
    public class IronTracks : MelonMod
    {
        bool enableDeckTracker = false;
        bool enablePrizeCards = false;
        static Player player = new Player();
        const String GAME_SCENE_NAME = "Match_Landscape";

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                enableDeckTracker = !enableDeckTracker;
                LoggerInstance.Msg("Toggled Deck Tracker: " + enableDeckTracker.ToString());
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                enablePrizeCards = !enablePrizeCards;
                LoggerInstance.Msg("Toggled Prize Tracker: " + enablePrizeCards.ToString());
            }
            else if (Input.GetKeyDown(KeyCode.C) && SceneManager.GetActiveScene().name == GAME_SCENE_NAME)
            {
                /*LoggerInstance.Msg("Debug: Spawning a Card Basic");
                
                // While in the game scene, the basic card spawn in with a rotation of 0 while the entire game is
                // rotated on the X axis ever so slightly, this -55f offset is to make the card appear flat on the
                // player's POV
                float rotationOnX = -55f;

                CardBasic cardBasic = ManagerSingleton<RainierManager>.instance.cardSpawner.SpawnCardBasic();
                Vector3 vector = new Vector3(0f, 0f, Card3D.cardDepth * 1);
                // For some reason, these cards are instantiated backwards, which causes the players to see the backside of the card
                cardBasic.transform.position = vector;
                cardBasic.transform.rotation = Quaternion.Euler(rotationOnX, 180f, 0f);
                cardBasic.transform.localScale = new Vector3(2f, 2f, 2f);
                cardBasic.Init("swsh11_137");*/
            }
        }

        public override void OnGUI()
        {
            if (enableDeckTracker)
            {
                string deckString = player.deck.DeckStringForRender();

                var width = 250;
                var boxHeight = deckString.Count(s => s == '\n') * 20;
                if (boxHeight == 0)
                {
                    boxHeight = 100;
                }
                var location = new Rect(Screen.width - width, 0, width, boxHeight);
                var textLocation = new Rect(Screen.width - width + 5, 25, width, Screen.height);
                
                var deckGUIStyle = new GUIStyle();
                deckGUIStyle.normal.textColor = Color.white;
                deckGUIStyle.fontSize = 15;

                GUI.Box(location, "Deck " + "(" + player.deck.GetDeckOwner() + ")");
                GUI.Label(textLocation, deckString, deckGUIStyle);
            }

            if (enablePrizeCards)
            {
                var width = 250;
                var height = 200;
                var location = new Rect(0, height, width, 250);

                var deckGUIStyle = new GUIStyle();
                deckGUIStyle.normal.textColor = Color.white;
                deckGUIStyle.fontSize = 15;

                var textLocation = new Rect(5, height + 25, width, 500);
                GUI.Box(location, "Prize Cards");
                GUI.Label(textLocation, player.GetPrizeCards().PrizeCardStringForRender(), deckGUIStyle);
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(MatchManager), "SendMatchStartTelemetry")]
        class Patch
        {
            static void Prefix(MatchManager __instance, NetworkMatchController.MatchDetails game)
            {
                var playerOneName = game.players[0].playerName;

                player.username = playerOneName;

                // For other developers to note, it seems that pokemon exposes the 2nd player's information too :/
                // This could prove to be problematic for many reasons I won't get into.
                var playerTwoName = game.players[1].playerName;

                player.deck.SetDeckOwner(playerOneName);

                Melon<IronTracks>.Logger.Msg(playerOneName + " vs. " + playerTwoName);

                player.deck.PopulateDeck(game.players[0].deckInfo.cards);
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(PlayerCardOwner), "ProcessCardGainedResult")]
        class ProcessCardGainedPatch
        {
            static void Postfix(PlayerCardOwner __instance, OwnerData data, bool gainedFromDrop)
            {
                if (!__instance)
                {
                    return;
                }
                // Make sure that we are targetting the local player (ourself)
                if (__instance.playerID != PlayerID.LOCAL)
                {
                    return;
                }
                player.OnGainCardIntoCollection(data.card, __instance);
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(PlayerCardOwner), "ProcessCardRemovalResult")]
        class ProcessCardRemovalResultPatch
        {
            static void Postfix(PlayerCardOwner __instance, OwnerData data, bool droppingCard)
            {
                if (!__instance)
                {
                    return;
                }
                // Make sure that we are targetting the local player (ourself)
                if (__instance.playerID != PlayerID.LOCAL)
                {
                    return;
                }
                player.OnRemovedCardFromCollection(data.card, __instance);
            }
        }
    }

}
