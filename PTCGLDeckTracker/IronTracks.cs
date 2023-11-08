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

namespace PTCGLDeckTracker
{
    public class IronTracks : MelonMod
    {
        bool enableDeckTracker = false;
        bool enableOpponentDeck = false;
        static Deck playerOneDeck = new Deck("playerOne");
        static Deck playerTwoDeck = new Deck("playerTwo");

        public override void OnUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                enableDeckTracker = !enableDeckTracker;
                LoggerInstance.Msg("Toggled deck tracker: " + enableDeckTracker.ToString());
            }
            else if (Input.GetKeyDown(KeyCode.X))
            {
                enableOpponentDeck = !enableOpponentDeck;
                LoggerInstance.Msg("Toggled Opponent Deck Tracker: " + enableOpponentDeck.ToString());
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                LoggerInstance.Msg("Debug: Spawning a Card Basic");

                string currentScene = SceneManager.GetActiveScene().name;
                float rotationOnX = (currentScene == "Match_Landscape") ? -55f : 0f;

                CardBasic cardBasic = ManagerSingleton<RainierManager>.instance.cardSpawner.SpawnCardBasic();
                Vector3 vector = new Vector3(0f, 0f, Card3D.cardDepth * 1);
                // For some reason, these cards are instantiated backwards
                cardBasic.transform.position = vector;
                cardBasic.transform.rotation = Quaternion.Euler(rotationOnX, 180f, 0f);
                cardBasic.transform.localScale = new Vector3(2f, 2f, 2f);
                cardBasic.Init("swsh11_137");
            }
        }

        public override void OnGUI()
        {
            if (enableDeckTracker)
            {
                var width = 250;
                var location = new Rect(Screen.width - width, 0, width, Screen.height);
                var textLocation = new Rect(Screen.width - width + 5, 25, width, Screen.height);
                GUI.Box(location, "Deck " + "(" + playerOneDeck.GetDeckOwner() + ")");

                string deckString = playerOneDeck.DeckStringForRender();

                GUI.Label(textLocation, deckString);
            }

            if (enableOpponentDeck)
            {
                var width = 250;
                var location = new Rect(0, 0, width, Screen.height);
                var textLocation = new Rect(5, 25, width, Screen.height);
                GUI.Box(location, "Deck " + "(" + playerTwoDeck.GetDeckOwner() + ")");

                string deckString = playerTwoDeck.DeckStringForRender();

                GUI.Label(textLocation, deckString);
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(MatchManager), "SendMatchStartTelemetry")]
        class Patch
        {
            static void Prefix(MatchManager __instance, NetworkMatchController.MatchDetails game)
            {
                var playerOneName = game.players[0].playerName;
                var playerTwoName = game.players[1].playerName;

                playerOneDeck.SetDeckOwner(playerOneName);
                playerTwoDeck.SetDeckOwner(playerTwoName);

                Melon<IronTracks>.Logger.Msg(playerOneName + " vs. " + playerTwoName);

                playerOneDeck.PopulateDeck(game.players[0].deckInfo.cards);
                playerTwoDeck.PopulateDeck(game.players[1].deckInfo.cards);
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(DeckController), "ProcessCardGainedResult")]
        class ProcessCardGainedPatch
        {
            static void Postfix(DeckController __instance, OwnerData data, bool gainedFromDrop)
            {
                if (!__instance)
                {
                    return;
                }
                if (__instance.GetType() == typeof(DeckController))
                {
                    var ownedCards = "";
                    if (__instance.playerID != PlayerID.LOCAL)
                    {
                        return;
                    }
                    foreach (var card in __instance.GetOwnedCards())
                    {
                        ownedCards += "Card Name: " + card.name + ", Card ID: " + card.cardSourceID + "\n";
                    }
                    Melon<IronTracks>.Logger.Msg(ownedCards);
                }
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(DeckController), "ProcessCardRemovalResult")]
        class ProcessCardRemovalResultPatch
        {
            static void Postfix(DeckController __instance, OwnerData data, bool droppingCard)
            {
                if (!__instance)
                {
                    return;
                }
                if (__instance.GetType() == typeof(DeckController))
                {
                    if (__instance.playerID != PlayerID.LOCAL)
                    {
                        return;
                    }
                    Melon<IronTracks>.Logger.Msg("Removed Card from DeckController: " + data.card.name);
                }
            }
        }
    }

}
