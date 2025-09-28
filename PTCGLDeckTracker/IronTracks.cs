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
using TPCI.Rainier.Match.Cards.Ownership;

namespace PTCGLDeckTracker
{
    public class IronTracks : MelonMod
    {
        bool enableDeckTracker = false;
        bool enablePrizeCards = false;
        static Player player = new Player();
        const String GAME_SCENE_NAME = "Match_Landscape";
        const float HighlightDuration = 2.0f;

        private Rect _deckTrackerWindowRect = new Rect(Screen.width - 250, 0, 250, 100);
        private Rect _prizeCardsWindowRect = new Rect(0, 200, 250, 100);

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
                _deckTrackerWindowRect = GUI.Window(0, _deckTrackerWindowRect, DrawDeckTrackerWindow, "Deck " + "(" + player.deck.GetDeckOwner() + ")");
            }

            if (enablePrizeCards)
            {
                _prizeCardsWindowRect = GUI.Window(1, _prizeCardsWindowRect, DrawPrizeCardsWindow, "Prize Cards");
            }
        }

        void DrawDeckTrackerWindow(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

            var cards = player.deck.GetCardsForRender();
            // Add 2 for the counters, plus a little extra for spacing
            var boxHeight = (cards.Count + 3) * 20;
            if (boxHeight == 0)
            {
                boxHeight = 100;
            }
            _deckTrackerWindowRect.height = boxHeight + 60; // Adjust height dynamically + padding

            var totalAssumedCards = player.deck.GetAssumedTotalQuantityOfCards();
            var totalActualCards = player.deck.GetTotalQuantityOfCards();
            var isUncertain = totalAssumedCards != totalActualCards;

            var yOffset = 25;
            foreach (var card in cards)
            {
                var deckGUIStyle = new GUIStyle();
                deckGUIStyle.fontSize = 15;
                deckGUIStyle.padding = new RectOffset(5, 5, 5, 5);

                if (card.highlightState != HighlightState.None)
                {
                    if (Time.time >= card.highlightEndTime)
                    {
                        card.highlightState = HighlightState.None;
                        deckGUIStyle.normal.textColor = Color.white;
                    }
                    else
                    {
                        Color highlightColor = GetHighlightColor(card.highlightState);
                        float elapsedTime = Time.time - (card.highlightEndTime - HighlightDuration);
                        float t = elapsedTime / HighlightDuration;
                        deckGUIStyle.normal.textColor = Color.Lerp(highlightColor, Color.white, t);
                    }
                }
                else
                {
                    deckGUIStyle.normal.textColor = Color.white;
                }

                string cardText = card.card.quantity + " " + card.card.englishName;
                if (isUncertain)
                {
                    cardText += " (?)";
                }
                GUI.Label(new Rect(5, yOffset, _deckTrackerWindowRect.width - 10, 20), cardText, deckGUIStyle);
                yOffset += 20;
            }

            // Add the counters back
            var counterGUIStyle = new GUIStyle();
            counterGUIStyle.fontSize = 15;
            counterGUIStyle.padding = new RectOffset(5, 5, 5, 5);
            counterGUIStyle.normal.textColor = Color.white;

            yOffset += 20; // Add some space
            GUI.Label(new Rect(5, yOffset, _deckTrackerWindowRect.width - 10, 20), "Total Cards in Deck: " + totalActualCards, counterGUIStyle);
            yOffset += 20;
            GUI.Label(new Rect(5, yOffset, _deckTrackerWindowRect.width - 10, 20), "Total ASSUMED Cards in Deck: " + totalAssumedCards, counterGUIStyle);
        }

        void DrawPrizeCardsWindow(int windowID)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

            var cards = player.GetPrizeCards().GetCardsForRender();
            // Add 1 for the counter, plus a little extra for spacing
            var boxHeight = (cards.Count + 2) * 20;
            if (boxHeight == 0)
            {
                boxHeight = 100;
            }
            _prizeCardsWindowRect.height = boxHeight + 60; // Adjust height dynamically + padding

            var yOffset = 25;
            foreach (var card in cards)
            {
                var deckGUIStyle = new GUIStyle();
                deckGUIStyle.fontSize = 15;
                deckGUIStyle.padding = new RectOffset(5, 5, 5, 5);

                if (card.highlightState != HighlightState.None)
                {
                    if (Time.time >= card.highlightEndTime)
                    {
                        card.highlightState = HighlightState.None;
                        deckGUIStyle.normal.textColor = Color.white;
                    }
                    else
                    {
                        Color highlightColor = GetHighlightColor(card.highlightState);
                        float elapsedTime = Time.time - (card.highlightEndTime - HighlightDuration);
                        float t = elapsedTime / HighlightDuration;
                        deckGUIStyle.normal.textColor = Color.Lerp(highlightColor, Color.white, t);
                    }
                }
                else
                {
                    deckGUIStyle.normal.textColor = Color.white;
                }

                GUI.Label(new Rect(5, yOffset, _prizeCardsWindowRect.width - 10, 20), card.card.quantity + " " + card.card.englishName, deckGUIStyle);
                yOffset += 20;
            }

            // Add the counter back
            var counterGUIStyle = new GUIStyle();
            counterGUIStyle.fontSize = 15;
            counterGUIStyle.padding = new RectOffset(5, 5, 5, 5);
            counterGUIStyle.normal.textColor = Color.white;

            yOffset += 20; // Add some space
            GUI.Label(new Rect(5, yOffset, _prizeCardsWindowRect.width - 10, 20), "Total Prize Cards: " + player.GetPrizeCards().GetPrizeCount(), counterGUIStyle);
        }

        private Color GetHighlightColor(HighlightState state)
        {
            switch (state)
            {
                case HighlightState.Added:
                    return Color.green;
                case HighlightState.Removed:
                    return Color.red;
                default:
                    return Color.white;
            }
        }

        [HarmonyLib.HarmonyPatch(typeof(MatchManager), "SendMatchStartTelemetry")]
        class Patch
        {
            static void Prefix(MatchManager __instance, NetworkMatchController.MatchDetails game)
            {
                var assumedLocalPlayer = game.players[0];
                var playerName = assumedLocalPlayer.playerName;

                if (playerName != NetworkMatchController.GetPlayerName(PlayerID.LOCAL))
                {
                    // Swap to the other player if this isn't our own player name.
                    assumedLocalPlayer = game.players[1];
                    Melon<IronTracks>.Logger.Msg("Player mismatch detected");
                }

                var playerOneName = assumedLocalPlayer.playerName;

                player.username = playerOneName;

                // For other developers to note, it seems that pokemon exposes the 2nd player's information too :/
                // This could prove to be problematic for many reasons I won't get into.
                var playerTwoName = game.players[1].playerName;

                player.deck.SetDeckOwner(playerOneName);

                Melon<IronTracks>.Logger.Msg(playerOneName + " vs. " + playerTwoName);

                player.deck.PopulateDeck(assumedLocalPlayer.deckInfo.cards);
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
