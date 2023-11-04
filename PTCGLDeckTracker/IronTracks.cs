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
    }

}
